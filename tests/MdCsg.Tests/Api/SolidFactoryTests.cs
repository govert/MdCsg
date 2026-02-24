using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Batch 48: Solid factory method tests (20 tests)</summary>
public class SolidFactoryTests
{
    [Fact]
    public void FromTriangles_SingleTriangle_HasOneFace()
    {
        var tris = new List<Triangle3> { new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var solid = Solid.FromTriangles(tris);
        Assert.Single(solid.Mesh.Faces);
    }

    [Fact]
    public void FromTriangles_SingleTriangle_HasBvh()
    {
        var tris = new List<Triangle3> { new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var solid = Solid.FromTriangles(tris);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void FromTriangles_CubeTriangles_12Faces()
    {
        var cube = MeshFactory.CreateCube();
        var tris = new List<Triangle3>();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(12, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromTriangles_CustomWeldTolerance()
    {
        var tris = new List<Triangle3>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY)
        };
        var solid = Solid.FromTriangles(tris, weldTolerance: 0.01);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromIndexed_TetrahedronShape()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0),
            new(1, 0, 0),
            new(0.5, 0.866, 0),
            new(0.5, 0.289, 0.816)
        };
        var indices = new List<(int, int, int)>
        {
            (0, 2, 1),
            (0, 1, 3),
            (1, 2, 3),
            (0, 3, 2)
        };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(4, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromIndexed_HasBvh()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var indices = new List<(int, int, int)>
        {
            (0, 2, 1), (0, 1, 3), (1, 2, 3), (0, 3, 2)
        };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.NotNull(solid.Bvh);
    }

    [Fact]
    public void Solid_Constructor_BuildsBvh()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_Bounds_CorrectForCube()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        var bounds = solid.Bounds;
        Assert.True(bounds.Min.X >= -0.01);
        Assert.True(bounds.Max.X <= 1.01);
    }

    [Fact]
    public void Solid_Bounds_CorrectForOffsetCube()
    {
        var cube = MeshFactory.CreateCube(new Vec3(5, 10, 15));
        var solid = new Solid(cube.Mesh);
        var bounds = solid.Bounds;
        Assert.True(System.Math.Abs(bounds.Min.X - 5) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Y - 10) < 0.01);
    }

    [Fact]
    public void FromTriangles_CanBeUsedInCsg()
    {
        var cubeA = MeshFactory.CreateCube();
        var tris = new List<Triangle3>();
        foreach (var face in cubeA.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var solidA = Solid.FromTriangles(tris);
        var solidB = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(solidA, solidB);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FromIndexed_CanBeUsedInCsg()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var indices = new List<(int, int, int)>
        {
            (0, 2, 1), (0, 1, 3), (1, 2, 3), (0, 3, 2)
        };
        var solidA = Solid.FromIndexed(positions, indices);
        var solidB = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1)).Mesh);
        var result = Csg.Union(solidA, solidB);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Solid_Mesh_Property_ReturnsSameMesh()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        Assert.Same(cube.Mesh, solid.Mesh);
    }

    [Fact]
    public void FromTriangles_TwoDisjointTriangles()
    {
        var tris = new List<Triangle3>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0))
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.Equal(6, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromTriangles_Sphere_RoundTrip()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        var tris = new List<Triangle3>();
        foreach (var face in sphere.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(sphere.Mesh.Faces.Count, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_Bvh_MeshProperty()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        Assert.Same(cube.Mesh, solid.Bvh.Mesh);
    }

    [Fact]
    public void Solid_LargeCube_Bounds()
    {
        var cube = MeshFactory.CreateCube(size: 100);
        var solid = new Solid(cube.Mesh);
        var bounds = solid.Bounds;
        Assert.True(bounds.Size.X > 99);
        Assert.True(bounds.Size.Y > 99);
    }

    [Fact]
    public void Solid_SmallCube_Bounds()
    {
        var cube = MeshFactory.CreateCube(size: 0.01);
        var solid = new Solid(cube.Mesh);
        var bounds = solid.Bounds;
        Assert.True(bounds.Size.X < 0.02);
    }

    [Fact]
    public void FromTriangles_EmptyList_ProducesEmptyMesh()
    {
        var solid = Solid.FromTriangles(new List<Triangle3>());
        Assert.Empty(solid.Mesh.Faces);
    }

    [Fact]
    public void Solid_Tetrahedron_HasCorrectBounds()
    {
        var tet = MeshFactory.CreateTetrahedron(Vec3.Zero, 1);
        var solid = new Solid(tet.Mesh);
        var bounds = solid.Bounds;
        Assert.True(bounds.Size.X > 0);
        Assert.True(bounds.Size.Y > 0);
        Assert.True(bounds.Size.Z > 0);
    }
}
