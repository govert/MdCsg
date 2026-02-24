using MdCsg.Bvh;
using MdCsg.Cutting;
using MdCsg.Gpu.Interop;
using MdCsg.Mesh;
using MdCsg.Patches;

namespace MdCsg.Gpu;

/// <summary>
/// Converts MdCsg data structures (BVH, mesh, patches) into flat GPU-compatible arrays.
/// </summary>
public static class BvhFlattener
{
    /// <summary>
    /// Holds all flattened data needed by GPU compute shaders.
    /// </summary>
    public sealed class FlattenedData
    {
        /// <summary>BVH nodes in flat array.</summary>
        public required GpuBvhNode[] BvhNodes { get; init; }

        /// <summary>Face indices (maps leaf primitive slots to mesh face indices).</summary>
        public required int[] FaceIndices { get; init; }

        /// <summary>Triangle vertices: 3 GpuVec3 per face (v0, v1, v2).</summary>
        public required GpuVec3[] TriangleVertices { get; init; }

        /// <summary>Sub-triangle vertices: 3 GpuVec3 per sub-triangle (A, B, C).</summary>
        public required GpuVec3[] SubTriangleVertices { get; init; }

        /// <summary>Patch offsets: prefix-sum of sub-triangle counts. Length = patchCount + 1.</summary>
        public required int[] PatchOffsets { get; init; }

        /// <summary>Concatenated sub-triangle indices for all patches.</summary>
        public required int[] PatchSubTriangleIndices { get; init; }
    }

    /// <summary>
    /// Flattens BVH, mesh, sub-triangles, and patches into GPU-compatible arrays.
    /// </summary>
    public static FlattenedData Flatten(
        BvhTree bvh,
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles)
    {
        // Flatten BVH nodes
        var nodes = bvh.Nodes;
        var gpuNodes = new GpuBvhNode[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            ref readonly var node = ref nodes[i];
            gpuNodes[i] = new GpuBvhNode
            {
                BoundsMinX = (float)node.Bounds.Min.X,
                BoundsMinY = (float)node.Bounds.Min.Y,
                BoundsMinZ = (float)node.Bounds.Min.Z,
                BoundsMaxX = (float)node.Bounds.Max.X,
                BoundsMaxY = (float)node.Bounds.Max.Y,
                BoundsMaxZ = (float)node.Bounds.Max.Z,
                LeftOrStart = node.LeftOrStart,
                Right = node.Right,
                PrimitiveCount = node.PrimitiveCount
            };
        }

        // Copy face indices
        var faceIndicesSpan = bvh.FaceIndices;
        var faceIndices = new int[faceIndicesSpan.Length];
        faceIndicesSpan.CopyTo(faceIndices);

        // Flatten triangle vertices from mesh (3 vertices per face)
        var mesh = bvh.Mesh;
        var triVerts = new GpuVec3[mesh.Faces.Count * 3];
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            var face = mesh.Faces[i];
            var verts = face.GetVertices();
            triVerts[i * 3 + 0] = new GpuVec3(verts[0].Position.X, verts[0].Position.Y, verts[0].Position.Z);
            triVerts[i * 3 + 1] = new GpuVec3(verts[1].Position.X, verts[1].Position.Y, verts[1].Position.Z);
            triVerts[i * 3 + 2] = new GpuVec3(verts[2].Position.X, verts[2].Position.Y, verts[2].Position.Z);
        }

        // Flatten sub-triangle vertices (3 per sub-triangle)
        var subTriVerts = new GpuVec3[subTriangles.Count * 3];
        for (int i = 0; i < subTriangles.Count; i++)
        {
            var st = subTriangles[i];
            subTriVerts[i * 3 + 0] = new GpuVec3(st.A.X, st.A.Y, st.A.Z);
            subTriVerts[i * 3 + 1] = new GpuVec3(st.B.X, st.B.Y, st.B.Z);
            subTriVerts[i * 3 + 2] = new GpuVec3(st.C.X, st.C.Y, st.C.Z);
        }

        // Build patch offsets (prefix-sum) and concatenated sub-triangle indices
        var patchOffsets = new int[patches.Count + 1];
        int totalSubTris = 0;
        for (int i = 0; i < patches.Count; i++)
        {
            patchOffsets[i] = totalSubTris;
            totalSubTris += patches[i].SubTriangleIndices.Count;
        }
        patchOffsets[patches.Count] = totalSubTris;

        var patchSubTriIndices = new int[totalSubTris];
        int offset = 0;
        for (int i = 0; i < patches.Count; i++)
        {
            foreach (var idx in patches[i].SubTriangleIndices)
                patchSubTriIndices[offset++] = idx;
        }

        return new FlattenedData
        {
            BvhNodes = gpuNodes,
            FaceIndices = faceIndices,
            TriangleVertices = triVerts,
            SubTriangleVertices = subTriVerts,
            PatchOffsets = patchOffsets,
            PatchSubTriangleIndices = patchSubTriIndices
        };
    }
}
