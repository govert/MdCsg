using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhNode struct property tests and BvhTree structural invariants</summary>
public class BvhNodePropertyTests
{
    [Fact]
    public void IsLeaf_WhenPrimitiveCountPositive_True()
    {
        var node = new BvhNode { PrimitiveCount = 1 };
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void IsLeaf_WhenPrimitiveCountZero_False()
    {
        var node = new BvhNode { PrimitiveCount = 0 };
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void IsLeaf_MultiPrimitive_True()
    {
        var node = new BvhNode { PrimitiveCount = 4 };
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_DefaultValues()
    {
        var node = new BvhNode();
        Assert.Equal(0, node.LeftOrStart);
        Assert.Equal(0, node.Right);
        Assert.Equal(0, node.PrimitiveCount);
    }

    [Fact]
    public void CubeTree_RootBounds_EnclosesAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        // Cube from (0,0,0) to (1,1,1)
        Assert.True(rootBounds.Min.X <= 0.01);
        Assert.True(rootBounds.Max.X >= 0.99);
        Assert.True(rootBounds.Min.Y <= 0.01);
        Assert.True(rootBounds.Max.Y >= 0.99);
        Assert.True(rootBounds.Min.Z <= 0.01);
        Assert.True(rootBounds.Max.Z >= 0.99);
    }

    [Fact]
    public void AllLeaves_HavePrimitiveCountLe4()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
                Assert.True(nodes[i].PrimitiveCount <= 4,
                    $"Leaf node {i} has {nodes[i].PrimitiveCount} primitives");
        }
    }

    [Fact]
    public void AllFaceIndices_Valid()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
            {
                for (int j = 0; j < nodes[i].PrimitiveCount; j++)
                {
                    int faceIdx = bvh.GetFaceIndex(nodes[i].LeftOrStart + j);
                    Assert.True(faceIdx >= 0 && faceIdx < mesh.Faces.Count,
                        $"Face index {faceIdx} out of range");
                }
            }
        }
    }

    [Fact]
    public void InteriorNodes_HaveValidChildren()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                int left = nodes[i].LeftOrStart;
                int right = nodes[i].Right;
                Assert.True(left >= 0 && left < bvh.NodeCount,
                    $"Left child {left} out of range");
                Assert.True(right >= 0 && right < bvh.NodeCount,
                    $"Right child {right} out of range");
                Assert.NotEqual(left, right);
            }
        }
    }

    [Fact]
    public void ChildBounds_FitInsideParent()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                var parent = nodes[i].Bounds;
                var left = nodes[nodes[i].LeftOrStart].Bounds;
                var right = nodes[nodes[i].Right].Bounds;
                // Children bounds should be contained within parent
                Assert.True(left.Min.X >= parent.Min.X - 1e-10);
                Assert.True(left.Max.X <= parent.Max.X + 1e-10);
                Assert.True(right.Min.X >= parent.Min.X - 1e-10);
                Assert.True(right.Max.X <= parent.Max.X + 1e-10);
            }
        }
    }

    [Fact]
    public void Query_FullBounds_ReturnsAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2)), results);
        Assert.Equal(mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void Query_EmptyRegion_ReturnsEmpty()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(10, 10, 10), new Vec3(11, 11, 11)), results);
        Assert.Empty(results);
    }

    [Fact]
    public void RayCastCount_ThroughCube_EvenHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from outside through center — even count = outside (enter+exit through triangulated faces)
        var ray = new Ray(new Vec3(0.5, 0.5, -2), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits > 0 && hits % 2 == 0, $"Expected even hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_MissCube_ZeroHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(5, 5, -2), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.Equal(0, hits);
    }

    [Fact]
    public void RayCastCount_ThroughSphere_EvenHits()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(0, 0, -5), new Vec3(0, 0, 1));
        int hits = bvh.RayCastCount(ray);
        Assert.True(hits > 0 && hits % 2 == 0, $"Expected even hits, got {hits}");
    }

    [Fact]
    public void GetFaceIndex_AllLeavesReachable()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var reachedFaces = new HashSet<int>();
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
            {
                for (int j = 0; j < nodes[i].PrimitiveCount; j++)
                    reachedFaces.Add(bvh.GetFaceIndex(nodes[i].LeftOrStart + j));
            }
        }
        Assert.Equal(mesh.Faces.Count, reachedFaces.Count);
    }

    [Fact]
    public void FaceIndices_Span_HasCorrectLength()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(mesh.Faces.Count, bvh.FaceIndices.Length);
    }

    [Fact]
    public void EmptyMesh_ZeroNodes()
    {
        var mesh = new MeshBuilder().Build(new List<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(0, bvh.NodeCount);
    }
}
