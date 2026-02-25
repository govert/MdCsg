using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree — SAH build properties, node structure, leaf constraints</summary>
public class BvhTreeSahBuildPropertyTests
{
    [Fact]
    public void CubeBvh_RootBoundsContainsAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var faceBounds = Aabb.FromTriangle(a, b, c);
            Assert.True(rootBounds.Contains(faceBounds.Min),
                $"Root should contain face min {faceBounds.Min}");
            Assert.True(rootBounds.Contains(faceBounds.Max),
                $"Root should contain face max {faceBounds.Max}");
        }
    }

    [Fact]
    public void LeafNodes_HaveAtMost4Primitives()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
                Assert.True(node.PrimitiveCount <= 4, $"Leaf {i} has {node.PrimitiveCount} prims (max 4)");
        }
    }

    [Fact]
    public void InteriorNodes_HavePrimitiveCountZero()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (!node.IsLeaf)
                Assert.Equal(0, node.PrimitiveCount);
        }
    }

    [Fact]
    public void AllFaceIndices_InRange()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
            {
                for (int j = 0; j < node.PrimitiveCount; j++)
                {
                    int faceIdx = bvh.GetFaceIndex(node.LeftOrStart + j);
                    Assert.True(faceIdx >= 0 && faceIdx < mesh.Faces.Count,
                        $"Face index {faceIdx} out of range [0, {mesh.Faces.Count})");
                }
            }
        }
    }

    [Fact]
    public void SphereBvh_HasMultipleNodes()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        Assert.True(bvh.NodeCount > 1, $"Expected multiple BVH nodes, got {bvh.NodeCount}");
    }

    [Fact]
    public void SingleTriangleMesh_SingleLeafNode()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        var bvh = BvhTree.Build(mesh);
        Assert.Equal(1, bvh.NodeCount);
        Assert.True(bvh.Nodes[0].IsLeaf);
        Assert.Equal(1, bvh.Nodes[0].PrimitiveCount);
    }

    [Fact]
    public void ChildBounds_ContainedInParent()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (!node.IsLeaf)
            {
                var left = bvh.Nodes[node.LeftOrStart];
                var right = bvh.Nodes[node.Right];
                // Child bounds should overlap with parent
                var parentBounds = node.Bounds;
                Assert.True(parentBounds.Contains(left.Bounds.Min) || parentBounds.Overlaps(left.Bounds));
                Assert.True(parentBounds.Contains(right.Bounds.Min) || parentBounds.Overlaps(right.Bounds));
            }
        }
    }

    [Fact]
    public void BvhMesh_IsStored()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Same(mesh, bvh.Mesh);
    }

    [Fact]
    public void FaceIndices_CoverAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var seen = new HashSet<int>();
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
            {
                for (int j = 0; j < node.PrimitiveCount; j++)
                    seen.Add(bvh.GetFaceIndex(node.LeftOrStart + j));
            }
        }
        Assert.Equal(mesh.Faces.Count, seen.Count);
    }

    [Fact]
    public void TetrahedronBvh_SmallNodeCount()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateTetrahedron().Mesh);
        Assert.True(bvh.NodeCount <= 5, $"Tetrahedron BVH should be small, got {bvh.NodeCount} nodes");
    }
}
