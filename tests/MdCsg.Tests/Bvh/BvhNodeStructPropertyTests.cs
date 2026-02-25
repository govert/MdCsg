using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhNode struct — IsLeaf, field values, node array structure from BvhTree.Build</summary>
public class BvhNodeStructPropertyTests
{
    [Fact]
    public void BvhNode_IsLeaf_WhenPrimitiveCountPositive()
    {
        var node = new BvhNode { PrimitiveCount = 3, LeftOrStart = 0 };
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_NotLeaf_WhenPrimitiveCountZero()
    {
        var node = new BvhNode { PrimitiveCount = 0, LeftOrStart = 1, Right = 2 };
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_DefaultPrimitiveCount_IsZero()
    {
        var node = new BvhNode();
        Assert.Equal(0, node.PrimitiveCount);
    }

    [Fact]
    public void BvhNode_DefaultIsLeaf_False()
    {
        var node = new BvhNode();
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void BvhNode_Fields_SetCorrectly()
    {
        var bounds = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var node = new BvhNode
        {
            Bounds = bounds,
            LeftOrStart = 5,
            Right = 10,
            PrimitiveCount = 0
        };
        Assert.Equal(5, node.LeftOrStart);
        Assert.Equal(10, node.Right);
        Assert.Equal(0, node.PrimitiveCount);
    }

    [Fact]
    public void BvhTree_Build_Cube_HasNodes()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.True(cube.Bvh.NodeCount > 0);
    }

    [Fact]
    public void BvhTree_Build_RootBounds_EnclosesMesh()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var nodes = cube.Bvh.Nodes;
        var rootBounds = nodes[0].Bounds;
        var meshBounds = cube.Mesh.GetBounds();
        // Root should enclose the mesh
        Assert.True(rootBounds.Min.X <= meshBounds.Min.X + 1e-10);
        Assert.True(rootBounds.Min.Y <= meshBounds.Min.Y + 1e-10);
        Assert.True(rootBounds.Min.Z <= meshBounds.Min.Z + 1e-10);
        Assert.True(rootBounds.Max.X >= meshBounds.Max.X - 1e-10);
        Assert.True(rootBounds.Max.Y >= meshBounds.Max.Y - 1e-10);
        Assert.True(rootBounds.Max.Z >= meshBounds.Max.Z - 1e-10);
    }

    [Fact]
    public void BvhTree_Build_Sphere_HasNodes()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(sphere.Bvh.NodeCount > 0);
    }

    [Fact]
    public void BvhTree_Build_AllLeavesHavePositivePrimitiveCount()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var nodes = cube.Bvh.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].IsLeaf)
                Assert.True(nodes[i].PrimitiveCount > 0);
        }
    }

    [Fact]
    public void BvhTree_Build_AllInteriorNodesHaveZeroPrimCount()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var nodes = sphere.Bvh.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].IsLeaf)
                Assert.Equal(0, nodes[i].PrimitiveCount);
        }
    }

    [Fact]
    public void BvhTree_Build_LeafPrimitivesInRange()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var nodes = sphere.Bvh.Nodes;
        int faceCount = sphere.Mesh.Faces.Count;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].IsLeaf)
            {
                Assert.True(nodes[i].LeftOrStart >= 0);
                Assert.True(nodes[i].LeftOrStart + nodes[i].PrimitiveCount <= faceCount);
            }
        }
    }

    [Fact]
    public void BvhTree_Build_InteriorChildren_InRange()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var nodes = sphere.Bvh.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                Assert.True(nodes[i].LeftOrStart >= 0 && nodes[i].LeftOrStart < nodes.Length);
                Assert.True(nodes[i].Right >= 0 && nodes[i].Right < nodes.Length);
            }
        }
    }

    [Fact]
    public void BvhTree_Build_FaceIndices_ArePermutation()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        int faceCount = cube.Mesh.Faces.Count;
        var faceIndices = cube.Bvh.FaceIndices;
        Assert.Equal(faceCount, faceIndices.Length);
        var seen = new HashSet<int>();
        for (int i = 0; i < faceIndices.Length; i++)
        {
            Assert.True(faceIndices[i] >= 0 && faceIndices[i] < faceCount);
            Assert.True(seen.Add(faceIndices[i]), $"Duplicate face index {faceIndices[i]}");
        }
    }

    [Fact]
    public void BvhTree_Build_EmptyMesh_NoNodes()
    {
        var empty = Solid.FromTriangles(Array.Empty<Triangle3>());
        Assert.Equal(0, empty.Bvh.NodeCount);
    }
}
