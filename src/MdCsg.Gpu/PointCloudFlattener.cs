using MdCsg.Bvh;
using MdCsg.Gpu.Interop;
using MdCsg.Math;

namespace MdCsg.Gpu;

/// <summary>
/// Converts BVH and query points into flat GPU-compatible arrays for point cloud shaders.
/// </summary>
public static class PointCloudFlattener
{
    /// <summary>
    /// Holds flattened data for point cloud GPU queries.
    /// </summary>
    public sealed class FlattenedPointCloudData
    {
        /// <summary>BVH nodes in flat array.</summary>
        public required GpuBvhNode[] BvhNodes { get; init; }

        /// <summary>Face indices (maps leaf primitive slots to mesh face indices).</summary>
        public required int[] FaceIndices { get; init; }

        /// <summary>Triangle vertices: 3 GpuVec3 per face (v0, v1, v2).</summary>
        public required GpuVec3[] TriangleVertices { get; init; }

        /// <summary>Query points as GpuVec3.</summary>
        public required GpuVec3[] QueryPoints { get; init; }
    }

    /// <summary>
    /// Flattens BVH and query points into GPU-compatible arrays.
    /// </summary>
    /// <param name="bvh">The BVH of the solid mesh.</param>
    /// <param name="points">The query points.</param>
    /// <returns>Flattened data ready for GPU upload.</returns>
    public static FlattenedPointCloudData Flatten(BvhTree bvh, Vec3[] points)
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

        // Convert query points
        var queryPts = new GpuVec3[points.Length];
        for (int i = 0; i < points.Length; i++)
            queryPts[i] = new GpuVec3(points[i].X, points[i].Y, points[i].Z);

        return new FlattenedPointCloudData
        {
            BvhNodes = gpuNodes,
            FaceIndices = faceIndices,
            TriangleVertices = triVerts,
            QueryPoints = queryPts
        };
    }
}
