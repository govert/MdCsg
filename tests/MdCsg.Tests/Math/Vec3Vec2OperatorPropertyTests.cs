using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3/Vec2 — operator coverage, identity laws, Dot/Cross, Normalized, indexer, Min/Max</summary>
public class Vec3Vec2OperatorPropertyTests
{
    // --- Vec3 operator tests ---

    [Fact]
    public void Vec3_AddZero_Identity()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v, v + Vec3.Zero);
    }

    [Fact]
    public void Vec3_SubtractSelf_Zero()
    {
        var v = new Vec3(5, 6, 7);
        Assert.Equal(Vec3.Zero, v - v);
    }

    [Fact]
    public void Vec3_Negate_DoubleNegate_Identity()
    {
        var v = new Vec3(1, -2, 3);
        Assert.Equal(v, -(-v));
    }

    [Fact]
    public void Vec3_ScalarMultiply_One_Identity()
    {
        var v = new Vec3(4, 5, 6);
        Assert.Equal(v, v * 1.0);
    }

    [Fact]
    public void Vec3_ScalarMultiply_Zero_GivesZero()
    {
        var v = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Zero, v * 0.0);
    }

    [Fact]
    public void Vec3_LeftMultiply_EqualsRightMultiply()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v * 5.0, 5.0 * v);
    }

    [Fact]
    public void Vec3_Division_ByOne_Identity()
    {
        var v = new Vec3(10, 20, 30);
        Assert.Equal(v, v / 1.0);
    }

    [Fact]
    public void Vec3_Division_ByTwo_HalvesComponents()
    {
        var v = new Vec3(10, 20, 30);
        var half = v / 2.0;
        Assert.Equal(5.0, half.X);
        Assert.Equal(10.0, half.Y);
        Assert.Equal(15.0, half.Z);
    }

    [Fact]
    public void Vec3_Dot_Orthogonal_Zero()
    {
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitX, Vec3.UnitY));
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitY, Vec3.UnitZ));
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitZ, Vec3.UnitX));
    }

    [Fact]
    public void Vec3_Dot_SameDirection_LengthSquared()
    {
        var v = new Vec3(3, 4, 0);
        Assert.Equal(25.0, Vec3.Dot(v, v));
    }

    [Fact]
    public void Vec3_Cross_UnitVectors_Standard()
    {
        Assert.Equal(Vec3.UnitZ, Vec3.Cross(Vec3.UnitX, Vec3.UnitY));
        Assert.Equal(Vec3.UnitX, Vec3.Cross(Vec3.UnitY, Vec3.UnitZ));
        Assert.Equal(Vec3.UnitY, Vec3.Cross(Vec3.UnitZ, Vec3.UnitX));
    }

    [Fact]
    public void Vec3_Cross_Anticommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Cross(a, b), -Vec3.Cross(b, a));
    }

    [Fact]
    public void Vec3_Cross_SelfIsZero()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(Vec3.Zero, Vec3.Cross(v, v));
    }

    [Fact]
    public void Vec3_Normalized_UnitLength()
    {
        var v = new Vec3(3, 4, 0);
        var n = v.Normalized;
        Assert.True(System.Math.Abs(n.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Vec3_Indexer_ReturnsCorrectComponents()
    {
        var v = new Vec3(10, 20, 30);
        Assert.Equal(10.0, v[0]);
        Assert.Equal(20.0, v[1]);
        Assert.Equal(30.0, v[2]);
    }

    [Fact]
    public void Vec3_Min_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var min = Vec3.Min(a, b);
        Assert.Equal(1.0, min.X);
        Assert.Equal(2.0, min.Y);
        Assert.Equal(3.0, min.Z);
    }

    [Fact]
    public void Vec3_Max_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var max = Vec3.Max(a, b);
        Assert.Equal(4.0, max.X);
        Assert.Equal(5.0, max.Y);
        Assert.Equal(6.0, max.Z);
    }

    [Fact]
    public void Vec3_Distance_TriangleInequality()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.True(Vec3.Distance(a, c) <= Vec3.Distance(a, b) + Vec3.Distance(b, c) + 1e-10);
    }

    [Fact]
    public void Vec3_SnapToGrid_RoundsCorrectly()
    {
        var v = new Vec3(1.000000003, 2.0, 3.0);
        var snapped = v.SnapToGrid();
        Assert.True(System.Math.Abs(snapped.X - 1.0) < 1e-7);
    }

    // --- Vec2 operator tests ---

    [Fact]
    public void Vec2_AddZero_Identity()
    {
        var v = new Vec2(1, 2);
        Assert.Equal(v, v + Vec2.Zero);
    }

    [Fact]
    public void Vec2_SubtractSelf_Zero()
    {
        var v = new Vec2(5, 6);
        Assert.Equal(Vec2.Zero, v - v);
    }

    [Fact]
    public void Vec2_Negate_DoubleNegate_Identity()
    {
        var v = new Vec2(1, -2);
        Assert.Equal(v, -(-v));
    }

    [Fact]
    public void Vec2_Dot_Orthogonal_Zero()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(0.0, Vec2.Dot(a, b));
    }

    [Fact]
    public void Vec2_Cross_Perpendicular_Positive()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(1.0, Vec2.Cross(a, b));
    }

    [Fact]
    public void Vec2_Cross_Anticommutative()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(-Vec2.Cross(b, a), Vec2.Cross(a, b));
    }

    [Fact]
    public void Vec2_Normalized_UnitLength()
    {
        var v = new Vec2(3, 4);
        Assert.True(System.Math.Abs(v.Normalized.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Vec2_LeftMultiply_EqualsRightMultiply()
    {
        var v = new Vec2(1, 2);
        Assert.Equal(v * 3.0, 3.0 * v);
    }

    [Fact]
    public void Vec2_Division_ByTwo()
    {
        var v = new Vec2(10, 20);
        var half = v / 2.0;
        Assert.Equal(5.0, half.X);
        Assert.Equal(10.0, half.Y);
    }
}
