using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

public class CsgOperatorTests
{
    // ── Operator + (Union) ──────────────────────────────────────────

    [Fact]
    public void PlusOperator_SameAsStaticUnion()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2);
        var opResult = a + b;
        var staticResult = Csg.Union(a, b);
        Assert.Equal(staticResult.FaceCount, opResult.FaceCount);
    }

    // ── Operator - (Difference) ─────────────────────────────────────

    [Fact]
    public void MinusOperator_SameAsStaticDifference()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2);
        var opResult = a - b;
        var staticResult = Csg.Difference(a, b);
        Assert.Equal(staticResult.FaceCount, opResult.FaceCount);
    }

    // ── Operator ^ (Intersect) ──────────────────────────────────────

    [Fact]
    public void CaretOperator_SameAsStaticIntersect()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2);
        var opResult = a ^ b;
        var staticResult = Csg.Intersect(a, b);
        Assert.Equal(staticResult.FaceCount, opResult.FaceCount);
    }

    // ── Operators produce valid meshes ──────────────────────────────

    [Fact]
    public void PlusOperator_ReturnsNonEmptyMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2);
        var result = a + b;
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void MinusOperator_ReturnsNonEmptyMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 1);
        var result = a - b;
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CaretOperator_OverlappingCubes_HasFaces()
    {
        var a = Primitives.Cube(Vec3.Zero, 2);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2);
        var result = a ^ b;
        Assert.True(result.FaceCount > 0);
    }
}
