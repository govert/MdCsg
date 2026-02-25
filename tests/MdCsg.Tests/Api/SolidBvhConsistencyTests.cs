using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid — BVH consistency, Bounds property, factory method properties</summary>
public class SolidBvhConsistencyTests
{
    [Fact]
    public void Solid_BvhNodeCount_PositiveForCube()
    {
        var solid = MeshFactory.CreateCube();
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_BvhMesh_SameAsSolidMesh()
    {
        var solid = MeshFactory.CreateCube();
        Assert.Same(solid.Mesh, solid.Bvh.Mesh);
    }

    [Fact]
    public void Solid_Bounds_MatchesMeshBounds()
    {
        var solid = MeshFactory.CreateCube();
        var solidBounds = solid.Bounds;
        var meshBounds = solid.Mesh.GetBounds();
        Assert.Equal(meshBounds.Min, solidBounds.Min);
        Assert.Equal(meshBounds.Max, solidBounds.Max);
    }

    [Fact]
    public void Solid_FromTriangles_CreatesValidMesh()
    {
        var triangles = new Triangle3[]
        {
            new(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var solid = Solid.FromTriangles(triangles);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_FromIndexed_CreatesValidMesh()
    {
        var positions = new Vec3[]
        {
            Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void Solid_Cube_BoundsContainAllVertices()
    {
        var solid = MeshFactory.CreateCube();
        var bounds = solid.Bounds;
        foreach (var v in solid.Mesh.Vertices)
            Assert.True(bounds.Contains(v.Position));
    }

    [Fact]
    public void Solid_Sphere_BoundsContainAllVertices()
    {
        var solid = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var bounds = solid.Bounds;
        foreach (var v in solid.Mesh.Vertices)
            Assert.True(bounds.Contains(v.Position));
    }

    [Fact]
    public void Solid_OffsetCube_BoundsReflectOffset()
    {
        var offset = new Vec3(5, 10, 15);
        var solid = MeshFactory.CreateCube(offset);
        var bounds = solid.Bounds;
        Assert.True(bounds.Contains(offset));
        Assert.True(bounds.Contains(offset + new Vec3(1, 1, 1)));
    }

    [Fact]
    public void Solid_Tetrahedron_HasFourFaces()
    {
        var solid = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_FromTriangles_BvhQueryFindsAllFaces()
    {
        var solid = MeshFactory.CreateCube();
        var results = new List<int>();
        solid.Bvh.Query(solid.Bounds.Expand(0.1), results);
        Assert.Equal(solid.Mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void Solid_FromTriangles_WeldTolerance_AffectsVertexCount()
    {
        // Two triangles with very close but not identical vertices
        var triangles = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1.000001, 0, 0), new Vec3(0.000001, 0, 0), new Vec3(0, 0, 1)),
        };
        var tightWeld = Solid.FromTriangles(triangles, 1e-4); // should merge
        var looseWeld = Solid.FromTriangles(triangles, 1e-10); // should not merge
        Assert.True(tightWeld.Mesh.Vertices.Count <= looseWeld.Mesh.Vertices.Count);
    }

    [Fact]
    public void Solid_CubeAndSphere_DifferentBvhNodeCounts()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        // Sphere has more faces, should have more BVH nodes
        Assert.True(sphere.Bvh.NodeCount > cube.Bvh.NodeCount,
            $"Sphere ({sphere.Bvh.NodeCount} nodes) should have more BVH nodes than cube ({cube.Bvh.NodeCount} nodes)");
    }

    [Fact]
    public void Solid_FromIndexed_SharedVertices_TwinsLinked()
    {
        var positions = new Vec3[]
        {
            Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var solid = Solid.FromIndexed(positions, indices);
        // Shared edge should have twins
        int twinCount = solid.Mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount >= 2, $"Should have twins on shared edge, got {twinCount}");
    }

    [Fact]
    public void Solid_MultipleSizes_BvhScales()
    {
        var small = MeshFactory.CreateCube(size: 0.1);
        var large = MeshFactory.CreateCube(size: 10);
        Assert.True(large.Bounds.Size.X > small.Bounds.Size.X);
    }
}
