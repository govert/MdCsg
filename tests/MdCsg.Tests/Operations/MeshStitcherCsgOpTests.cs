using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;

namespace MdCsg.Tests.Operations;

/// <summary>Batch 25: MeshStitcher and CsgOperation tests (20 tests)</summary>
public class MeshStitcherCsgOpTests
{
    [Fact]
    public void Stitch_SingleTriangle_ProducesMesh()
    {
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_TwoTriangles_SharedEdge()
    {
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY),
        };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_CubeTriangles_ProducesCube()
    {
        // Build cube from 12 triangles
        var tris = new List<Triangle3>();
        // Front face (z=0)
        tris.Add(new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(1,1,0)));
        tris.Add(new Triangle3(new Vec3(0,0,0), new Vec3(1,1,0), new Vec3(0,1,0)));
        // Back face (z=1)
        tris.Add(new Triangle3(new Vec3(0,0,1), new Vec3(1,1,1), new Vec3(1,0,1)));
        tris.Add(new Triangle3(new Vec3(0,0,1), new Vec3(0,1,1), new Vec3(1,1,1)));
        // Bottom face (y=0)
        tris.Add(new Triangle3(new Vec3(0,0,0), new Vec3(1,0,1), new Vec3(1,0,0)));
        tris.Add(new Triangle3(new Vec3(0,0,0), new Vec3(0,0,1), new Vec3(1,0,1)));
        // Top face (y=1)
        tris.Add(new Triangle3(new Vec3(0,1,0), new Vec3(1,1,0), new Vec3(1,1,1)));
        tris.Add(new Triangle3(new Vec3(0,1,0), new Vec3(1,1,1), new Vec3(0,1,1)));
        // Left face (x=0)
        tris.Add(new Triangle3(new Vec3(0,0,0), new Vec3(0,1,0), new Vec3(0,1,1)));
        tris.Add(new Triangle3(new Vec3(0,0,0), new Vec3(0,1,1), new Vec3(0,0,1)));
        // Right face (x=1)
        tris.Add(new Triangle3(new Vec3(1,0,0), new Vec3(1,0,1), new Vec3(1,1,1)));
        tris.Add(new Triangle3(new Vec3(1,0,0), new Vec3(1,1,1), new Vec3(1,1,0)));

        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(12, mesh.Faces.Count);
        Assert.Equal(8, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_EmptyList_EmptyMesh()
    {
        var mesh = MeshStitcher.Stitch(Array.Empty<Triangle3>());
        Assert.Equal(0, mesh.Faces.Count);
        Assert.Equal(0, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_DisjointTriangles_NoSharedVertices()
    {
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0)),
        };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_CustomWeldTolerance()
    {
        // Two triangles with slightly offset shared vertices
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(new Vec3(1 + 1e-10, 0, 0), new Vec3(1, 1, 0), new Vec3(0 + 1e-10, 1, 0)),
        };
        var mesh = MeshStitcher.Stitch(tris, 1e-8);
        // With welding, the shared vertices should merge
        Assert.True(mesh.Vertices.Count <= 5);
    }

    [Fact]
    public void Stitch_TightTolerance_NoWelding()
    {
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(new Vec3(1 + 1e-5, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1 + 1e-5, 0)),
        };
        var mesh = MeshStitcher.Stitch(tris, 1e-10);
        // With tight tolerance, vertices won't merge
        Assert.True(mesh.Vertices.Count >= 5);
    }

    [Fact]
    public void Stitch_PreservesTriangleCount()
    {
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY),
            new Triangle3(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0)),
        };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(3, mesh.Faces.Count);
    }

    [Fact]
    public void CsgOperation_Union_Value()
    {
        Assert.Equal(CsgOperation.Union, (CsgOperation)0);
    }

    [Fact]
    public void CsgOperation_Intersection_Value()
    {
        Assert.Equal(CsgOperation.Intersection, (CsgOperation)1);
    }

    [Fact]
    public void CsgOperation_Difference_Value()
    {
        Assert.Equal(CsgOperation.Difference, (CsgOperation)2);
    }

    [Fact]
    public void Stitch_ManyTriangles_ProducesCorrectCount()
    {
        var tris = new List<Triangle3>();
        for (int i = 0; i < 100; i++)
        {
            double x = i * 3;
            tris.Add(new Triangle3(
                new Vec3(x, 0, 0),
                new Vec3(x + 1, 0, 0),
                new Vec3(x, 1, 0)));
        }
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(100, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_DuplicateTriangles_StillProcesses()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var mesh = MeshStitcher.Stitch(new[] { tri, tri });
        // MeshBuilder may handle duplicates in various ways
        Assert.True(mesh.Faces.Count >= 1);
    }

    [Fact]
    public void Stitch_LargeTriangle_ProducesMesh()
    {
        var tri = new Triangle3(new Vec3(-1000, -1000, 0), new Vec3(1000, -1000, 0), new Vec3(0, 1000, 0));
        var mesh = MeshStitcher.Stitch(new[] { tri });
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_TinyTriangle_ProducesMesh()
    {
        double eps = 1e-6;
        var tri = new Triangle3(Vec3.Zero, new Vec3(eps, 0, 0), new Vec3(0, eps, 0));
        var mesh = MeshStitcher.Stitch(new[] { tri }, 1e-10);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_ThreeTriangleFan_SharedVertex()
    {
        var center = new Vec3(0.5, 0.5, 0);
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, center),
            new Triangle3(Vec3.UnitX, new Vec3(1, 1, 0), center),
            new Triangle3(new Vec3(1, 1, 0), Vec3.UnitY, center),
        };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(3, mesh.Faces.Count);
        Assert.True(mesh.Vertices.Count <= 5); // center is shared
    }

    [Fact]
    public void Stitch_DefaultTolerance()
    {
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        // Default tolerance should be 1e-8
        var mesh = MeshStitcher.Stitch(tris);
        Assert.NotNull(mesh);
    }

    [Fact]
    public void Stitch_InXZPlane()
    {
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitZ) };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_InYZPlane()
    {
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitY, Vec3.UnitZ) };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_3DTriangle()
    {
        var tris = new[] { new Triangle3(Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ) };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
    }
}
