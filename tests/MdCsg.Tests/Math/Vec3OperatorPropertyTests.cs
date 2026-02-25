using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 operator properties — algebraic identities, distance, normalize, special values</summary>
public class Vec3OperatorPropertyTests
{
    [Fact]
    public void Addition_Commutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Addition_Associative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var left = (a + b) + c;
        var right = a + (b + c);
        Assert.Equal(left.X, right.X, 1e-10);
        Assert.Equal(left.Y, right.Y, 1e-10);
        Assert.Equal(left.Z, right.Z, 1e-10);
    }

    [Fact]
    public void Addition_ZeroIdentity()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a, a + Vec3.Zero);
    }

    [Fact]
    public void Subtraction_Self_Zero()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(Vec3.Zero, a - a);
    }

    [Fact]
    public void ScalarMultiplication_One_Identity()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a, a * 1);
    }

    [Fact]
    public void ScalarMultiplication_Zero_GivesZero()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(Vec3.Zero, a * 0);
    }

    [Fact]
    public void ScalarMultiplication_Distributive()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var left = (a + b) * 3;
        var right = a * 3 + b * 3;
        Assert.Equal(left.X, right.X, 1e-10);
        Assert.Equal(left.Y, right.Y, 1e-10);
        Assert.Equal(left.Z, right.Z, 1e-10);
    }

    [Fact]
    public void Negate_DoubleNegate_Identity()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a, -(-a));
    }

    [Fact]
    public void Length_UnitVectors()
    {
        Assert.Equal(1, new Vec3(1, 0, 0).Length, 1e-10);
        Assert.Equal(1, new Vec3(0, 1, 0).Length, 1e-10);
        Assert.Equal(1, new Vec3(0, 0, 1).Length, 1e-10);
    }

    [Fact]
    public void Length_Zero_IsZero()
    {
        Assert.Equal(0, Vec3.Zero.Length);
    }

    [Fact]
    public void Length_3_4_5()
    {
        Assert.Equal(5, new Vec3(3, 4, 0).Length, 1e-10);
    }

    [Fact]
    public void Normalized_HasUnitLength()
    {
        var v = new Vec3(3, 4, 5);
        var n = v.Normalized;
        Assert.Equal(1, n.Length, 1e-10);
    }

    [Fact]
    public void Normalized_SameDirection()
    {
        var v = new Vec3(3, 4, 5);
        var n = v.Normalized;
        // Normalized should be parallel (same direction)
        Assert.True(Vec3.Dot(v, n) > 0);
    }

    [Fact]
    public void Distance_SamePoint_Zero()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(0, Vec3.Distance(a, a));
    }

    [Fact]
    public void Distance_Symmetric()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Distance(a, b), Vec3.Distance(b, a));
    }

    [Fact]
    public void DistanceSquared_ConsistentWithDistance()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var d = Vec3.Distance(a, b);
        var dsq = Vec3.DistanceSquared(a, b);
        Assert.Equal(d * d, dsq, 1e-10);
    }

    [Fact]
    public void Dot_OrthogonalVectors_Zero()
    {
        Assert.Equal(0, Vec3.Dot(new Vec3(1, 0, 0), new Vec3(0, 1, 0)));
        Assert.Equal(0, Vec3.Dot(new Vec3(1, 0, 0), new Vec3(0, 0, 1)));
        Assert.Equal(0, Vec3.Dot(new Vec3(0, 1, 0), new Vec3(0, 0, 1)));
    }

    [Fact]
    public void Dot_ParallelVectors_Positive()
    {
        var v = new Vec3(1, 2, 3);
        Assert.True(Vec3.Dot(v, v) > 0);
    }

    [Fact]
    public void Dot_SelfEquals_LengthSquared()
    {
        var v = new Vec3(3, 4, 5);
        Assert.Equal(v.LengthSquared, Vec3.Dot(v, v), 1e-10);
    }

    [Fact]
    public void Cross_ParallelVectors_Zero()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(2, 0, 0);
        var cross = Vec3.Cross(a, b);
        Assert.Equal(0, cross.Length, 1e-10);
    }

    [Fact]
    public void Cross_UnitX_UnitY_UnitZ()
    {
        var x = new Vec3(1, 0, 0);
        var y = new Vec3(0, 1, 0);
        var z = Vec3.Cross(x, y);
        Assert.Equal(0, z.X, 1e-10);
        Assert.Equal(0, z.Y, 1e-10);
        Assert.Equal(1, z.Z, 1e-10);
    }

    [Fact]
    public void SnapToGrid_Idempotent()
    {
        var v = new Vec3(1.23456789, 2.34567891, 3.45678912);
        var snapped = v.SnapToGrid();
        var snappedAgain = snapped.SnapToGrid();
        Assert.Equal(snapped, snappedAgain);
    }

    [Fact]
    public void Vec3Zero_AllZero()
    {
        Assert.Equal(0, Vec3.Zero.X);
        Assert.Equal(0, Vec3.Zero.Y);
        Assert.Equal(0, Vec3.Zero.Z);
    }

    [Fact]
    public void Division_ByScalar()
    {
        var v = new Vec3(6, 8, 10);
        var result = v / 2;
        Assert.Equal(new Vec3(3, 4, 5), result);
    }
}
