using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree.Query — box queries, empty mesh, containment, partial overlap</summary>
public class BvhQueryBoxTests
{
    [Fact]
    public void QueryEntireBounds_ReturnsAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(bvh.Nodes[0].Bounds.Expand(0.1), results);
        Assert.Equal(mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void QueryOutside_ReturnsEmpty()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        var farBox = new Aabb(new Vec3(100, 100, 100), new Vec3(101, 101, 101));
        bvh.Query(farBox, results);
        Assert.Empty(results);
    }

    [Fact]
    public void QueryPartialOverlap_ReturnsSomeFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        // Small box at one corner
        var cornerBox = new Aabb(new Vec3(-0.1, -0.1, -0.1), new Vec3(0.1, 0.1, 0.1));
        bvh.Query(cornerBox, results);
        Assert.True(results.Count > 0, "Should find some faces near corner");
        Assert.True(results.Count < mesh.Faces.Count, "Should not find all faces");
    }

    [Fact]
    public void QueryEmptyMesh_ReturnsEmpty()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(Vec3.Zero, new Vec3(1, 1, 1)), results);
        Assert.Empty(results);
    }

    [Fact]
    public void QuerySingleTriangleMesh()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-0.1, -0.1, -0.1), new Vec3(1.1, 1.1, 0.1)), results);
        Assert.Single(results);
        Assert.Equal(0, results[0]);
    }

    [Fact]
    public void Query_FaceIndicesInRange()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(bvh.Nodes[0].Bounds, results);
        foreach (int idx in results)
            Assert.True(idx >= 0 && idx < mesh.Faces.Count, $"Face {idx} out of range");
    }

    [Fact]
    public void Query_NoDuplicateResults()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        bvh.Query(bvh.Nodes[0].Bounds.Expand(0.1), results);
        Assert.Equal(results.Count, new HashSet<int>(results).Count);
    }

    [Fact]
    public void Query_PointBox_ReturnsFacesNearPoint()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        // Point box near center top
        var pointBox = Aabb.FromPoint(new Vec3(0.5, 0.5, 1.0)).Expand(0.01);
        bvh.Query(pointBox, results);
        Assert.True(results.Count > 0, "Should find faces near top surface");
    }

    [Fact]
    public void Query_Sphere_ReturnsAllFaces_WhenEnclosed()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1).Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        // Large box enclosing entire sphere
        bvh.Query(new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2)), results);
        Assert.Equal(mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void Query_MultipleCalls_Independent()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);

        var results1 = new List<int>();
        bvh.Query(new Aabb(new Vec3(-0.1, -0.1, -0.1), new Vec3(0.1, 0.1, 0.1)), results1);

        var results2 = new List<int>();
        bvh.Query(new Aabb(new Vec3(0.9, 0.9, 0.9), new Vec3(1.1, 1.1, 1.1)), results2);

        // Different corners, may return different face sets
        Assert.True(results1.Count > 0);
        Assert.True(results2.Count > 0);
    }
}
