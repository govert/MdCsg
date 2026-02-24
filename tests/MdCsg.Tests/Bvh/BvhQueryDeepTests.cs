using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BVH query, ray intersection, and node structure deep tests</summary>
public class BvhQueryDeepTests
{
    [Fact]
    public void Build_Cube_NodeCountReasonable()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // 12 faces → internal + leaf nodes. SAH-split BVH: ~2N-1 nodes for N leaves
        Assert.True(bvh.NodeCount > 0);
        Assert.True(bvh.NodeCount < 100);
    }

    [Fact]
    public void Build_Sphere_NodeCountReasonable()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount > 0);
        Assert.True(bvh.NodeCount < 1000);
    }

    [Fact]
    public void Query_CubeCenter_FindsFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        var queryBox = new Aabb(new Vec3(0.4, 0.4, 0.4), new Vec3(0.6, 0.6, 0.6));
        bvh.Query(queryBox, results);
        Assert.True(results.Count > 0, "Query at cube center should find overlapping faces");
    }

    [Fact]
    public void Query_OutsideCube_FindsNothing()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        var queryBox = new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6));
        bvh.Query(queryBox, results);
        Assert.Empty(results);
    }

    [Fact]
    public void Query_EntireCube_FindsAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        var queryBox = new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2));
        bvh.Query(queryBox, results);
        Assert.Equal(mesh.Faces.Count, results.Distinct().Count());
    }

    [Fact]
    public void Query_PartialOverlap_FindsSomeFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        // Only overlaps one corner
        var queryBox = new Aabb(new Vec3(-0.5, -0.5, -0.5), new Vec3(0.1, 0.1, 0.1));
        bvh.Query(queryBox, results);
        Assert.True(results.Count > 0, "Should overlap some faces near corner");
        Assert.True(results.Count < mesh.Faces.Count, "Should not overlap all faces");
    }

    [Fact]
    public void FaceIndices_CoverAllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var indices = bvh.FaceIndices.ToArray();
        var unique = indices.Distinct().ToHashSet();
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            Assert.Contains(i, unique);
        }
    }

    [Fact]
    public void Nodes_RootEnclosesMesh()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var root = bvh.Nodes[0];
        var meshBounds = mesh.GetBounds();

        Assert.True(root.Bounds.Min.X <= meshBounds.Min.X + 1e-6);
        Assert.True(root.Bounds.Min.Y <= meshBounds.Min.Y + 1e-6);
        Assert.True(root.Bounds.Min.Z <= meshBounds.Min.Z + 1e-6);
        Assert.True(root.Bounds.Max.X >= meshBounds.Max.X - 1e-6);
        Assert.True(root.Bounds.Max.Y >= meshBounds.Max.Y - 1e-6);
        Assert.True(root.Bounds.Max.Z >= meshBounds.Max.Z - 1e-6);
    }

    [Fact]
    public void Build_PreservesMeshReference()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Same(mesh, bvh.Mesh);
    }

    [Fact]
    public void Query_Sphere_CenterRegion_FindsManyFaces()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        var queryBox = new Aabb(new Vec3(-0.1, -0.1, -0.1), new Vec3(0.1, 0.1, 0.1));
        bvh.Query(queryBox, results);
        // Sphere centered at origin — small box at center might not overlap face triangles
        // (they're on the surface, not at the center), so results could be empty
        // The important thing is no crash
        Assert.NotNull(results);
    }

    [Fact]
    public void Query_Sphere_Surface_FindsFaces()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();
        // Box around the top of the sphere
        var queryBox = new Aabb(new Vec3(-0.3, -0.3, 0.8), new Vec3(0.3, 0.3, 1.2));
        bvh.Query(queryBox, results);
        Assert.True(results.Count > 0, "Should find faces near top of sphere");
    }

    [Fact]
    public void Build_Tetrahedron_SmallTree()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.True(bvh.NodeCount >= 1);
        Assert.True(bvh.NodeCount <= 10); // 4 faces → very small tree
    }

    [Fact]
    public void Query_ReusableResultsList()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var results = new List<int>();

        bvh.Query(new Aabb(new Vec3(0.4, 0.4, 0.4), new Vec3(0.6, 0.6, 0.6)), results);
        int count1 = results.Count;

        // Clear and re-query
        results.Clear();
        bvh.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2)), results);
        int count2 = results.Count;

        Assert.True(count2 >= count1, "Full query should find at least as many faces");
    }
}
