using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhNode, BvhTraversal — node properties, dual-tree traversal, overlapping pairs</summary>
public class BvhNodeTraversalPropertyTests
{
    [Fact]
    public void BvhNode_DefaultIsLeaf_IsFalse()
    {
        var node = new BvhNode();
        Assert.False(node.IsLeaf); // PrimitiveCount = 0
    }

    [Fact]
    public void BvhNode_WithPrimitives_IsLeaf()
    {
        var node = new BvhNode { PrimitiveCount = 3 };
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_Interior_HasChildren()
    {
        var node = new BvhNode { PrimitiveCount = 0, LeftOrStart = 1, Right = 2 };
        Assert.False(node.IsLeaf);
        Assert.Equal(1, node.LeftOrStart);
        Assert.Equal(2, node.Right);
    }

    [Fact]
    public void BvhTree_CubeNodes_AllHaveBounds()
    {
        var cube = MeshFactory.CreateCube();
        var nodes = cube.Bvh.Nodes;
        for (int i = 0; i < cube.Bvh.NodeCount; i++)
        {
            var bounds = nodes[i].Bounds;
            Assert.True(bounds.Max.X >= bounds.Min.X);
            Assert.True(bounds.Max.Y >= bounds.Min.Y);
            Assert.True(bounds.Max.Z >= bounds.Min.Z);
        }
    }

    [Fact]
    public void BvhTree_CubeRoot_ContainsAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var rootBounds = cube.Bvh.Nodes[0].Bounds;
        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
        {
            var face = cube.Mesh.Faces[i];
            face.GetTrianglePositions(out var a, out var b, out var c);
            Assert.True(rootBounds.Contains(a) || rootBounds.Contains(b) || rootBounds.Contains(c) ||
                rootBounds.Overlaps(Aabb.FromTriangle(a, b, c)),
                $"Face {i} should be within root bounds");
        }
    }

    [Fact]
    public void BvhTree_LeafNodes_HavePositivePrimitiveCount()
    {
        var cube = MeshFactory.CreateCube();
        var nodes = cube.Bvh.Nodes;
        for (int i = 0; i < cube.Bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
                Assert.True(nodes[i].PrimitiveCount > 0);
        }
    }

    [Fact]
    public void BvhTree_InteriorNodes_HaveZeroPrimitiveCount()
    {
        var cube = MeshFactory.CreateCube();
        var nodes = cube.Bvh.Nodes;
        for (int i = 0; i < cube.Bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
                Assert.Equal(0, nodes[i].PrimitiveCount);
        }
    }

    [Fact]
    public void BvhTraversal_SameShape_FindsAllSelfPairs()
    {
        var cube = MeshFactory.CreateCube();
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, cube.Bvh);
        Assert.True(pairs.Count > 0, "Same BVH should have self-overlapping face pairs");
    }

    [Fact]
    public void BvhTraversal_DisjointShapes_NoPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0));
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void BvhTraversal_OverlappingCubes_FindsPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0, "Overlapping cubes should have face pairs");
    }

    [Fact]
    public void BvhTraversal_CubeSphere_FindsPairs()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, sphere.Bvh);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void BvhTraversal_EmptyTree_NoPairs()
    {
        var emptyMesh = new MdCsg.Mesh.HalfEdgeMesh();
        var emptyBvh = BvhTree.Build(emptyMesh);
        var cube = MeshFactory.CreateCube();
        var pairs = BvhTraversal.FindOverlappingPairs(emptyBvh, cube.Bvh);
        Assert.Empty(pairs);
    }

    [Fact]
    public void BvhTraversal_PairsHaveValidIndices()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        foreach (var (faceA, faceB) in pairs)
        {
            Assert.True(faceA >= 0 && faceA < a.Mesh.Faces.Count,
                $"FaceA index {faceA} out of range");
            Assert.True(faceB >= 0 && faceB < b.Mesh.Faces.Count,
                $"FaceB index {faceB} out of range");
        }
    }

    [Fact]
    public void BvhTraversal_PartiallyOverlappingCubes_FindsPairs()
    {
        // Cubes that partially overlap — faces should cross
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0, "Partially overlapping cubes should have face pairs");
    }
}
