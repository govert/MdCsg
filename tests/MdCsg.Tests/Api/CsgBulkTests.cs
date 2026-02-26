using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

public class CsgBulkTests
{
    // ── Union bulk ──────────────────────────────────────────────────

    [Fact]
    public void Union_SingleSolid_ReturnsSameMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var result = Csg.Union(new[] { cube });
        Assert.Equal(cube.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_TwoSolids_SameAsDirectCall()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2);
        var bulk = Csg.Union(new[] { a, b });
        var direct = Csg.Union(a, b);
        Assert.Equal(direct.FaceCount, bulk.FaceCount);
    }

    [Fact]
    public void Union_ThreeSolids_ReturnsValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2);
        var c = Primitives.Cube(new Vec3(0, 1, 0), 2);
        var result = Csg.Union(new[] { a, b, c });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_EmptyCollection_Throws()
    {
        Assert.Throws<ArgumentException>(() => Csg.Union(Array.Empty<Solid>()));
    }

    // ── Intersect bulk ──────────────────────────────────────────────

    [Fact]
    public void Intersect_SingleSolid_ReturnsSameMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var result = Csg.Intersect(new[] { cube });
        Assert.Equal(cube.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_TwoOverlapping_SameAsDirectCall()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2);
        var bulk = Csg.Intersect(new[] { a, b });
        var direct = Csg.Intersect(a, b);
        Assert.Equal(direct.FaceCount, bulk.FaceCount);
    }

    [Fact]
    public void Intersect_EmptyCollection_Throws()
    {
        Assert.Throws<ArgumentException>(() => Csg.Intersect(Array.Empty<Solid>()));
    }

    // ── Complement bulk ─────────────────────────────────────────────

    [Fact]
    public void Complement_PreservesFaceCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var comp = Csg.Complement(cube);
        Assert.Equal(cube.Mesh.Faces.Count, comp.Mesh.Faces.Count);
    }

    [Fact]
    public void Complement_PreservesVertexCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var comp = Csg.Complement(cube);
        Assert.Equal(cube.Mesh.Vertices.Count, comp.Mesh.Vertices.Count);
    }
}
