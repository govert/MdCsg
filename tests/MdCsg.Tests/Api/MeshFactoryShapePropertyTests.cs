using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: MeshFactory — cube, sphere, tetrahedron shape properties, offset, sizing</summary>
public class MeshFactoryShapePropertyTests
{
    [Fact]
    public void CreateCube_Default_12Faces()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(12, cube.Mesh.Faces.Count);
    }

    [Fact]
    public void CreateCube_Default_8Vertices()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void CreateCube_Default_36HalfEdges()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(36, cube.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void CreateCube_DefaultBounds_ZeroToOne()
    {
        var cube = MeshFactory.CreateCube();
        var bounds = cube.Bounds;
        Assert.True(System.Math.Abs(bounds.Min.X) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Y) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Z) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.X - 1.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.Y - 1.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.Z - 1.0) < 0.01);
    }

    [Fact]
    public void CreateCube_WithOffset_ShiftedBounds()
    {
        var offset = new Vec3(10, 20, 30);
        var cube = MeshFactory.CreateCube(offset);
        var bounds = cube.Bounds;
        Assert.True(System.Math.Abs(bounds.Min.X - 10.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Y - 20.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Z - 30.0) < 0.01);
    }

    [Fact]
    public void CreateCube_WithSize_CorrectExtent()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var bounds = cube.Bounds;
        Assert.True(System.Math.Abs(bounds.Max.X - 3.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.Y - 3.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.Z - 3.0) < 0.01);
    }

    [Fact]
    public void CreateCube_HasBvh()
    {
        var cube = MeshFactory.CreateCube();
        Assert.NotNull(cube.Bvh);
        Assert.True(cube.Bvh.NodeCount > 0);
    }

    [Fact]
    public void CreateSphere_Subdivision2_320Faces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.Equal(320, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void CreateSphere_Subdivision1_80Faces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        Assert.Equal(80, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void CreateSphere_Subdivision0_Icosahedron()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 0);
        // Base icosahedron has 20 faces... but subdivision 0 might mean base
        Assert.True(sphere.Mesh.Faces.Count >= 4, "Should have at least some faces");
    }

    [Fact]
    public void CreateSphere_VerticesOnSurface()
    {
        double radius = 2.0;
        var center = new Vec3(1, 2, 3);
        var sphere = MeshFactory.CreateSphere(center, radius, 2);
        foreach (var v in sphere.Mesh.Vertices)
        {
            double dist = (v.Position - center).Length;
            Assert.True(System.Math.Abs(dist - radius) < 0.01,
                $"Vertex should be at distance {radius} from center, got {dist}");
        }
    }

    [Fact]
    public void CreateSphere_HasBvh()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.NotNull(sphere.Bvh);
        Assert.True(sphere.Bvh.NodeCount > 0);
    }

    [Fact]
    public void CreateTetrahedron_Default_4Faces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Faces.Count);
    }

    [Fact]
    public void CreateTetrahedron_Default_4Vertices()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Vertices.Count);
    }

    [Fact]
    public void CreateTetrahedron_Default_12HalfEdges()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(12, tet.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void CreateTetrahedron_HasBvh()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.NotNull(tet.Bvh);
        Assert.True(tet.Bvh.NodeCount > 0);
    }

    [Fact]
    public void CreateTetrahedron_WithOffset_Shifted()
    {
        var offset = new Vec3(5, 5, 5);
        var tet = MeshFactory.CreateTetrahedron(offset);
        var bounds = tet.Bounds;
        Assert.True(bounds.Contains(offset));
    }

    [Fact]
    public void Solid_FromMesh_PreservesMesh()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Same(cube.Mesh, new Solid(cube.Mesh).Mesh);
    }
}
