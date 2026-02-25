using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhNode struct — IsLeaf, field semantics, tree structure invariants for built BVHs</summary>
public class BvhNodeStructTests
{
    [Fact]
    public void LeafNode_IsLeafTrue()
    {
        var node = new BvhNode { PrimitiveCount = 3 };
        Assert.True(node.IsLeaf);
    }

    [Fact]
    public void InteriorNode_IsLeafFalse()
    {
        var node = new BvhNode { PrimitiveCount = 0 };
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void DefaultNode_IsLeafFalse()
    {
        var node = new BvhNode();
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public void CubeBvh_HasMultipleNodes()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        Assert.True(bvh.NodeCount > 1, $"Cube BVH should have > 1 node, got {bvh.NodeCount}");
    }

    [Fact]
    public void CubeBvh_RootNode_BoundsContainAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var rootBounds = bvh.Nodes[0].Bounds;

        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var faceBounds = Aabb.FromTriangle(a, b, c);
            Assert.True(rootBounds.Contains(faceBounds.Min),
                $"Face {face.Id} min {faceBounds.Min} outside root bounds");
            Assert.True(rootBounds.Contains(faceBounds.Max),
                $"Face {face.Id} max {faceBounds.Max} outside root bounds");
        }
    }

    [Fact]
    public void SphereBvh_RootBounds_ApproximateSphere()
    {
        double r = 1.0;
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, r, 2);
        var bvh = sphere.Bvh;
        var root = bvh.Nodes[0];

        // Root bounds should be approximately [-1, 1]^3
        Assert.True(root.Bounds.Max.X <= r + 0.01);
        Assert.True(root.Bounds.Min.X >= -r - 0.01);
    }

    [Fact]
    public void AllLeafNodes_HavePositivePrimitiveCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
                Assert.True(node.PrimitiveCount > 0);
        }
    }

    [Fact]
    public void LeafNodes_PrimitiveCount_AtMost4()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var bvh = sphere.Bvh;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
                Assert.True(node.PrimitiveCount <= 4,
                    $"Leaf node {i} has {node.PrimitiveCount} primitives, max is 4");
        }
    }

    [Fact]
    public void InteriorNodes_HaveValidChildIndices()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var bvh = sphere.Bvh;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (!node.IsLeaf)
            {
                Assert.True(node.LeftOrStart >= 0 && node.LeftOrStart < bvh.NodeCount,
                    $"Node {i} left child {node.LeftOrStart} out of range");
                Assert.True(node.Right >= 0 && node.Right < bvh.NodeCount,
                    $"Node {i} right child {node.Right} out of range");
                Assert.NotEqual(node.LeftOrStart, node.Right);
            }
        }
    }

    [Fact]
    public void TotalLeafPrimitives_EqualsFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        int totalPrimitives = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            var node = bvh.Nodes[i];
            if (node.IsLeaf)
                totalPrimitives += node.PrimitiveCount;
        }
        Assert.Equal(cube.Mesh.Faces.Count, totalPrimitives);
    }

    [Fact]
    public void FaceIndices_AllValid()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var indices = bvh.FaceIndices;
        for (int i = 0; i < indices.Length; i++)
        {
            Assert.True(indices[i] >= 0 && indices[i] < cube.Mesh.Faces.Count,
                $"Face index [{i}]={indices[i]} out of range");
        }
    }

    [Fact]
    public void FaceIndices_Length_EqualsFaceCount()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var bvh = sphere.Bvh;
        Assert.Equal(sphere.Mesh.Faces.Count, bvh.FaceIndices.Length);
    }

    [Fact]
    public void FaceIndices_IsPermutation()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = cube.Bvh;
        var seen = new HashSet<int>();
        var indices = bvh.FaceIndices;
        for (int i = 0; i < indices.Length; i++)
            Assert.True(seen.Add(indices[i]), $"Duplicate face index {indices[i]}");
        Assert.Equal(cube.Mesh.Faces.Count, seen.Count);
    }

    [Fact]
    public void EmptyMesh_BvhHasNoNodes()
    {
        var builder = new MdCsg.Mesh.MeshBuilder();
        var mesh = builder.Build(Array.Empty<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(0, bvh.NodeCount);
    }

    [Fact]
    public void SingleTriangle_BvhHasOneLeaf()
    {
        var builder = new MdCsg.Mesh.MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount >= 1);
        Assert.True(bvh.Nodes[0].IsLeaf);
    }

    [Fact]
    public void TetrahedronBvh_CorrectPrimitiveTotal()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var bvh = tet.Bvh;
        int total = 0;
        for (int i = 0; i < bvh.NodeCount; i++)
            if (bvh.Nodes[i].IsLeaf)
                total += bvh.Nodes[i].PrimitiveCount;
        Assert.Equal(4, total); // Tetrahedron has 4 faces
    }
}
