using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree — Query, RayCastCount, Build, structural properties for various shapes</summary>
public class BvhTreeQueryPropertyTests
{
    [Fact]
    public void Build_EmptyMesh_ZeroNodes()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(0, bvh.NodeCount);
    }

    [Fact]
    public void Build_Cube_NonZeroNodes()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(cube.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Build_Sphere_NonZeroNodes()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(sphere.Bvh.NodeCount > 0);
    }

    [Fact]
    public void RayCastCount_InsideCube_OddHits()
    {
        var cube = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 1, $"Ray from inside cube should have odd hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_OutsideCube_EvenHits()
    {
        var cube = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(5, 5, 5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0, $"Ray from outside cube should have even hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_InsideSphere_OddHits()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0.00013, 0.00017).Normalized);
        int hits = sphere.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 1, $"Ray from inside sphere should have odd hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_OutsideSphere_EvenHits()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(new Vec3(10, 0, 0), new Vec3(1, 0.00013, 0.00017).Normalized);
        int hits = sphere.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0, $"Ray from outside sphere should have even hits, got {hits}");
    }

    [Fact]
    public void RayCastCount_MissesCompletely_ZeroHits()
    {
        var cube = MeshFactory.CreateCube();
        // Ray going away from cube
        var ray = new Ray(new Vec3(5, 5, 5), new Vec3(1, 1, 1).Normalized);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.Equal(0, hits);
    }

    [Fact]
    public void RayCastCount_ThroughCube_EvenHits()
    {
        var cube = MeshFactory.CreateCube();
        // Ray through cube with slightly perturbed direction
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0 && hits >= 2, $"Ray through cube should have even >=2 hits, got {hits}");
    }

    [Fact]
    public void Query_BoxContainingAll_ReturnsAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var bigBox = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var result = new List<int>();
        cube.Bvh.Query(bigBox, result);
        Assert.Equal(cube.Mesh.Faces.Count, result.Count);
    }

    [Fact]
    public void Query_DisjointBox_ReturnsEmpty()
    {
        var cube = MeshFactory.CreateCube();
        var disjointBox = new Aabb(new Vec3(10, 10, 10), new Vec3(20, 20, 20));
        var result = new List<int>();
        cube.Bvh.Query(disjointBox, result);
        Assert.Empty(result);
    }

    [Fact]
    public void Query_PartialOverlap_ReturnsSome()
    {
        var cube = MeshFactory.CreateCube();
        // Box that only covers part of the cube
        var partialBox = new Aabb(new Vec3(0.5, -1, -1), new Vec3(2, 2, 2));
        var result = new List<int>();
        cube.Bvh.Query(partialBox, result);
        Assert.True(result.Count > 0, "Partial overlap should return some faces");
        Assert.True(result.Count < cube.Mesh.Faces.Count, "Partial overlap should not return all faces");
    }

    [Fact]
    public void Build_Tetrahedron_ValidNodeCount()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.True(tet.Bvh.NodeCount > 0);
        Assert.True(tet.Bvh.NodeCount < 100, "4-face BVH should not have excessive nodes");
    }

    [Fact]
    public void FaceIndices_Count_MatchesFaces()
    {
        var cube = MeshFactory.CreateCube();
        var indices = cube.Bvh.FaceIndices;
        Assert.Equal(cube.Mesh.Faces.Count, indices.Length);
    }

    [Fact]
    public void Mesh_Property_ReturnsMesh()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Same(cube.Mesh, cube.Bvh.Mesh);
    }
}
