using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: MeshStitcher public facade tests — Stitch method, edge cases</summary>
public class MeshStitcherFacadeTests
{
    [Fact]
    public void Stitch_SingleTriangle_ProducesMesh()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_TwoSharedEdgeTriangles()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_Tetrahedron_ValidMesh()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        // Re-stitch from triangles
        var triangles = new List<Triangle3>();
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            triangles.Add(new Triangle3(a, b, c));
        }
        var stitched = MeshStitcher.Stitch(triangles);
        Assert.Equal(mesh.Faces.Count, stitched.Faces.Count);
        Assert.Equal(mesh.Vertices.Count, stitched.Vertices.Count);
    }

    [Fact]
    public void Stitch_Cube_ValidMesh()
    {
        var original = MeshFactory.CreateCube().Mesh;
        var triangles = new List<Triangle3>();
        foreach (var face in original.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            triangles.Add(new Triangle3(a, b, c));
        }
        var stitched = MeshStitcher.Stitch(triangles);
        Assert.Equal(original.Faces.Count, stitched.Faces.Count);
    }

    [Fact]
    public void Stitch_EmptyList_EmptyMesh()
    {
        var mesh = MeshStitcher.Stitch(new List<Triangle3>());
        Assert.Equal(0, mesh.Faces.Count);
        Assert.Equal(0, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_CustomWeldTolerance()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var mesh = MeshStitcher.Stitch(triangles, weldTolerance: 1e-6);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_NearbyVertices_WeldedWithinTolerance()
    {
        // Two triangles with vertices that are very close but not identical
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1 + 1e-10, 1, 0), new Vec3(0, 1 + 1e-10, 0))
        };
        var mesh = MeshStitcher.Stitch(triangles, weldTolerance: 1e-8);
        // With welding, shared vertices should be merged
        Assert.True(mesh.Vertices.Count <= 4);
    }

    [Fact]
    public void Stitch_FarVertices_NotWelded()
    {
        // Two completely separate triangles
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(100, 0, 0), new Vec3(101, 0, 0), new Vec3(100, 1, 0))
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(6, mesh.Vertices.Count); // no welding between distant triangles
    }

    [Fact]
    public void Stitch_LargeOffset_Works()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(1000, 2000, 3000), new Vec3(1001, 2000, 3000), new Vec3(1000, 2001, 3000))
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_Sphere_PreservesFaceCount()
    {
        var original = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var triangles = new List<Triangle3>();
        foreach (var face in original.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            triangles.Add(new Triangle3(a, b, c));
        }
        var stitched = MeshStitcher.Stitch(triangles);
        Assert.Equal(original.Faces.Count, stitched.Faces.Count);
    }
}
