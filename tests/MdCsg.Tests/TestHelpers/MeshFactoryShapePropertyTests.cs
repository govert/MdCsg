using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.TestHelpers;

/// <summary>Phase 6: MeshFactory — shape creation invariants: vertex/face/edge counts, bounds, Euler characteristic</summary>
public class MeshFactoryShapePropertyTests
{
    [Fact]
    public void Cube_Has8Vertices()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void Cube_Has12Faces()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(12, cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Cube_Has36HalfEdges()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(36, cube.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Cube_Offset_MovesCenter()
    {
        var offset = new Vec3(5, 10, 15);
        var cube = MeshFactory.CreateCube(offset, 2.0);
        var bounds = cube.Bounds;
        Assert.True(System.Math.Abs(bounds.Center.X - 6.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Center.Y - 11.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Center.Z - 16.0) < 0.01);
    }

    [Fact]
    public void Cube_Size_ScalesBounds()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var size = cube.Bounds.Size;
        Assert.True(System.Math.Abs(size.X - 3.0) < 0.01);
        Assert.True(System.Math.Abs(size.Y - 3.0) < 0.01);
        Assert.True(System.Math.Abs(size.Z - 3.0) < 0.01);
    }

    [Fact]
    public void Tetrahedron_Has4Vertices()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Vertices.Count);
    }

    [Fact]
    public void Tetrahedron_Has4Faces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Faces.Count);
    }

    [Fact]
    public void Tetrahedron_Has12HalfEdges()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(12, tet.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic_Two()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(tet.Mesh));
    }

    [Fact]
    public void Sphere_Sub0_Has12Vertices()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 0);
        Assert.Equal(12, sphere.Mesh.Vertices.Count);
    }

    [Fact]
    public void Sphere_Sub0_Has20Faces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 0);
        Assert.Equal(20, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_Sub1_Has80Faces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        Assert.Equal(80, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_Sub2_Has320Faces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.Equal(320, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_EulerCharacteristic_Two()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void Sphere_VerticesNearSurface()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        foreach (var v in sphere.Mesh.Vertices)
        {
            double dist = v.Position.Length;
            Assert.True(System.Math.Abs(dist - 2.0) < 0.01,
                $"Vertex distance from center is {dist}, expected 2.0");
        }
    }

    [Fact]
    public void Sphere_Offset_MovesCenter()
    {
        var center = new Vec3(5, 10, 15);
        var sphere = MeshFactory.CreateSphere(center, 1.0, 1);
        var bounds = sphere.Bounds;
        Assert.True(System.Math.Abs(bounds.Center.X - 5.0) < 0.1);
        Assert.True(System.Math.Abs(bounds.Center.Y - 10.0) < 0.1);
        Assert.True(System.Math.Abs(bounds.Center.Z - 15.0) < 0.1);
    }

    [Fact]
    public void Sphere_Radius_ScalesBounds()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 3.0, 1);
        var bounds = sphere.Bounds;
        Assert.True(bounds.Max.X > 2.9 && bounds.Max.X < 3.1);
        Assert.True(bounds.Min.X > -3.1 && bounds.Min.X < -2.9);
    }

    [Fact]
    public void Tetrahedron_Offset_MovesCenter()
    {
        var offset = new Vec3(10, 20, 30);
        var tet = MeshFactory.CreateTetrahedron(offset);
        var bounds = tet.Bounds;
        Assert.True(bounds.Center.X > 9 && bounds.Center.X < 11);
        Assert.True(bounds.Center.Y > 19 && bounds.Center.Y < 21);
    }
}
