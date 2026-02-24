using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Batch 11: BvhTree build and query extended tests (20 tests)</summary>
public class BvhBuildQueryExtTests
{
    [Fact]
    public void Build_EmptyMesh_HasZeroNodes()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var tree = BvhTree.Build(mesh);
        Assert.Equal(0, tree.NodeCount);
    }

    [Fact]
    public void Build_SingleTriangle_HasOneLeaf()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);
        var tree = BvhTree.Build(mesh);
        Assert.True(tree.NodeCount >= 1);
        Assert.True(tree.Nodes[0].IsLeaf);
    }

    [Fact]
    public void Build_Cube_HasNonZeroNodes()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(cube.Bvh.NodeCount > 1);
    }

    [Fact]
    public void Build_Sphere_HasReasonableDepth()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        Assert.True(sphere.Bvh.NodeCount > 10);
    }

    [Fact]
    public void Query_InsideBox_FindsAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var query = new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2));
        var results = new List<int>();
        cube.Bvh.Query(query, results);
        Assert.Equal(12, results.Count);
    }

    [Fact]
    public void Query_OutsideBox_FindsNothing()
    {
        var cube = MeshFactory.CreateCube();
        var query = new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6));
        var results = new List<int>();
        cube.Bvh.Query(query, results);
        Assert.Empty(results);
    }

    [Fact]
    public void Query_PartialOverlap_FindsSomeFaces()
    {
        var cube = MeshFactory.CreateCube();
        // Query box that overlaps only top face AABB
        var query = new Aabb(new Vec3(0.2, 0.2, 0.99), new Vec3(0.8, 0.8, 1.01));
        var results = new List<int>();
        cube.Bvh.Query(query, results);
        Assert.True(results.Count > 0);
        Assert.True(results.Count <= 12);
    }

    [Fact]
    public void Query_TinyBox_AtCenter()
    {
        var cube = MeshFactory.CreateCube();
        var query = new Aabb(new Vec3(0.49, 0.49, 0.49), new Vec3(0.51, 0.51, 0.51));
        var results = new List<int>();
        cube.Bvh.Query(query, results);
        Assert.True(results.Count >= 0); // Just verifying no crash
    }

    [Fact]
    public void Query_EmptyMesh_ReturnsEmpty()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var tree = BvhTree.Build(mesh);
        var results = new List<int>();
        tree.Query(new Aabb(Vec3.Zero, Vec3.UnitX), results);
        Assert.Empty(results);
    }

    [Fact]
    public void RayCast_ThroughCenter_HitsEvenNumber()
    {
        var cube = MeshFactory.CreateCube();
        // Ray through center — hits entry and exit faces. Each face = 2 triangles,
        // but the ray may hit 1 tri per face. For a closed mesh, hits should be even.
        var ray = new Ray(new Vec3(0.5, 0.5, -1), Vec3.UnitZ);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0, $"Expected even hits, got {hits}");
        Assert.True(hits >= 2);
    }

    [Fact]
    public void RayCast_Miss_HitsZero()
    {
        var cube = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(5, 5, -1), Vec3.UnitZ);
        Assert.Equal(0, cube.Bvh.RayCastCount(ray));
    }

    [Fact]
    public void RayCast_AlongXAxis_EvenHits()
    {
        var cube = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(-1, 0.5, 0.5), Vec3.UnitX);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0 && hits >= 2);
    }

    [Fact]
    public void RayCast_AlongYAxis_EvenHits()
    {
        var cube = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(0.5, -1, 0.5), Vec3.UnitY);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0 && hits >= 2);
    }

    [Fact]
    public void RayCast_Diagonal_EvenHits()
    {
        var cube = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(-1, -1, -1), new Vec3(1, 1, 1).Normalized);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0 && hits >= 2);
    }

    [Fact]
    public void RayCast_EmptyMesh()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var tree = BvhTree.Build(mesh);
        Assert.Equal(0, tree.RayCastCount(new Ray(Vec3.Zero, Vec3.UnitX)));
    }

    [Fact]
    public void RayCast_Sphere_EvenHits()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        var ray = new Ray(new Vec3(0, 0, -5), Vec3.UnitZ);
        int hits = sphere.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0 && hits >= 2);
    }

    [Fact]
    public void RayCast_OffsetCube_EvenHits()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var ray = new Ray(new Vec3(10.5, 10.5, -1), Vec3.UnitZ);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 0 && hits >= 2);
    }

    [Fact]
    public void Build_Tetrahedron_HasNodes()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.True(tet.Bvh.NodeCount >= 1);
    }

    [Fact]
    public void FaceIndices_Length_MatchesFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(12, cube.Bvh.FaceIndices.Length);
    }

    [Fact]
    public void GetFaceIndex_ReturnsValidIndices()
    {
        var cube = MeshFactory.CreateCube();
        var seen = new HashSet<int>();
        for (int i = 0; i < cube.Bvh.FaceIndices.Length; i++)
        {
            int idx = cube.Bvh.GetFaceIndex(i);
            Assert.True(idx >= 0 && idx < 12);
            seen.Add(idx);
        }
        Assert.Equal(12, seen.Count);
    }
}
