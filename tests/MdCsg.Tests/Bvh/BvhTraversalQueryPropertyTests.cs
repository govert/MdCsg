using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTraversal.FindOverlappingPairs, BvhTree.Query — overlap/disjoint/self scenarios</summary>
public class BvhTraversalQueryPropertyTests
{
    [Fact]
    public void FindOverlappingPairs_OverlappingCubes_HasPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_DisjointCubes_NoPairs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Equal(0, pairs.Count);
    }

    [Fact]
    public void FindOverlappingPairs_AllFaceIndicesValid()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        foreach (var (fA, fB) in pairs)
        {
            Assert.True(fA >= 0 && fA < a.Mesh.Faces.Count);
            Assert.True(fB >= 0 && fB < b.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void FindOverlappingPairs_SphereSphere_HasPairs()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var pairs = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.True(pairs.Count > 0);
    }

    [Fact]
    public void FindOverlappingPairs_Deterministic()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var p1 = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        var p2 = BvhTraversal.FindOverlappingPairs(a.Bvh, b.Bvh);
        Assert.Equal(p1.Count, p2.Count);
    }

    [Fact]
    public void Query_FullBounds_ReturnsAllFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var allBounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(20, 20, 20));
        var results = new List<int>();
        cube.Bvh.Query(allBounds, results);
        Assert.Equal(cube.Mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void Query_EmptyBox_ReturnsNothing()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var farBox = new Aabb(new Vec3(100, 100, 100), new Vec3(101, 101, 101));
        var results = new List<int>();
        cube.Bvh.Query(farBox, results);
        Assert.Equal(0, results.Count);
    }

    [Fact]
    public void Query_PartialOverlap_ReturnsSomeFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        // Box overlapping only the "right" half of the cube
        var partialBox = new Aabb(new Vec3(1.5, 0, 0), new Vec3(3, 3, 3));
        var results = new List<int>();
        cube.Bvh.Query(partialBox, results);
        Assert.True(results.Count > 0);
        Assert.True(results.Count <= cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Query_AllIndicesValid()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var box = new Aabb(new Vec3(-0.5, -0.5, -0.5), new Vec3(0.5, 0.5, 0.5));
        var results = new List<int>();
        sphere.Bvh.Query(box, results);
        foreach (int idx in results)
        {
            Assert.True(idx >= 0 && idx < sphere.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void BvhTree_NodeCount_Positive()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.True(cube.Bvh.NodeCount > 0);
    }

    [Fact]
    public void BvhTree_Mesh_IsNotNull()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.NotNull(cube.Bvh.Mesh);
    }

    [Fact]
    public void BvhTree_GetFaceIndex_ValidRange()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        int faceCount = cube.Mesh.Faces.Count;
        for (int i = 0; i < faceCount; i++)
        {
            int faceIdx = cube.Bvh.GetFaceIndex(i);
            Assert.True(faceIdx >= 0 && faceIdx < faceCount);
        }
    }

    [Fact]
    public void FindOverlappingPairs_CubeTetrahedron_HasPairs()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var tet = MeshFactory.CreateTetrahedron(new Vec3(1, 1, 1));
        var pairs = BvhTraversal.FindOverlappingPairs(cube.Bvh, tet.Bvh);
        Assert.True(pairs.Count > 0);
    }
}
