using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree SAH construction and leaf properties — leaf size, node bounds, face reachability</summary>
public class BvhSahLeafTests
{
    [Fact]
    public void Cube_AllLeavesHavePrimitives()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
                Assert.True(nodes[i].PrimitiveCount > 0, $"Leaf node {i} has 0 primitives");
        }
    }

    [Fact]
    public void Cube_LeafPrimitiveCount_AtMost4()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
                Assert.True(nodes[i].PrimitiveCount <= 4, $"Leaf {i} has {nodes[i].PrimitiveCount} primitives (max 4)");
        }
    }

    [Fact]
    public void Sphere_MoreNodes_ThanCube()
    {
        var cube = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var sphere = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh);
        Assert.True(sphere.NodeCount > cube.NodeCount,
            $"Sphere ({sphere.NodeCount}) should have more BVH nodes than cube ({cube.NodeCount})");
    }

    [Fact]
    public void InternalNodes_NotLeaf()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                Assert.Equal(0, nodes[i].PrimitiveCount);
                Assert.True(nodes[i].LeftOrStart >= 0, $"Internal node {i} has invalid left child");
                Assert.True(nodes[i].Right >= 0, $"Internal node {i} has invalid right child");
            }
        }
    }

    [Fact]
    public void Root_BoundsContainsAllFaces()
    {
        var mesh = MeshFactory.CreateCube(Vec3.Zero, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var rootBounds = bvh.Nodes[0].Bounds;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            Assert.True(rootBounds.Contains(a), $"Vertex {a} outside root bounds");
            Assert.True(rootBounds.Contains(b), $"Vertex {b} outside root bounds");
            Assert.True(rootBounds.Contains(c), $"Vertex {c} outside root bounds");
        }
    }

    [Fact]
    public void ChildBounds_InsideParent()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (!nodes[i].IsLeaf)
            {
                var parentBounds = nodes[i].Bounds;
                int left = nodes[i].LeftOrStart;
                int right = nodes[i].Right;
                // Children should overlap with parent (child bounds within parent)
                Assert.True(parentBounds.Overlaps(nodes[left].Bounds),
                    $"Left child {left} doesn't overlap parent {i}");
                Assert.True(parentBounds.Overlaps(nodes[right].Bounds),
                    $"Right child {right} doesn't overlap parent {i}");
            }
        }
    }

    [Fact]
    public void AllFaces_ReachableFromLeaves()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var nodes = bvh.Nodes;
        var reachable = new HashSet<int>();
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            if (nodes[i].IsLeaf)
            {
                for (int j = 0; j < nodes[i].PrimitiveCount; j++)
                    reachable.Add(bvh.GetFaceIndex(nodes[i].LeftOrStart + j));
            }
        }
        Assert.Equal(mesh.Faces.Count, reachable.Count);
    }

    [Fact]
    public void Tetrahedron_SmallBvh()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        // 4 faces → small BVH
        Assert.True(bvh.NodeCount >= 1);
        Assert.True(bvh.NodeCount <= 7); // At most 3 levels for 4 faces
    }

    [Fact]
    public void FaceIndices_ValidRange()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var faceIndices = bvh.FaceIndices;
        for (int i = 0; i < faceIndices.Length; i++)
        {
            Assert.True(faceIndices[i] >= 0 && faceIndices[i] < mesh.Faces.Count,
                $"FaceIndex[{i}] = {faceIndices[i]} out of range [0, {mesh.Faces.Count})");
        }
    }

    [Fact]
    public void FaceIndices_NoDuplicates()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var faceIndices = bvh.FaceIndices;
        var set = new HashSet<int>();
        for (int i = 0; i < faceIndices.Length; i++)
            Assert.True(set.Add(faceIndices[i]), $"Duplicate face index {faceIndices[i]}");
    }

    [Fact]
    public void FaceIndices_Count_MatchesFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(mesh.Faces.Count, bvh.FaceIndices.Length);
    }

    [Fact]
    public void EmptyMesh_EmptyBvh()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(0, bvh.NodeCount);
    }

    [Fact]
    public void Query_InsideBounds_FindsFaces()
    {
        var mesh = MeshFactory.CreateCube(Vec3.Zero, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(0.5, 0.5, 0.5), new Vec3(1.5, 1.5, 1.5)), results);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void Query_OutsideBounds_NoFaces()
    {
        var mesh = MeshFactory.CreateCube(Vec3.Zero, 1).Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(100, 100, 100), new Vec3(101, 101, 101)), results);
        Assert.Empty(results);
    }
}
