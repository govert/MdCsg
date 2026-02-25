using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 — cross product properties, dot product, normalize edge cases</summary>
public class Vec3CrossDotNormalizeTests
{
    [Fact]
    public void Cross_Anticommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.Equal(-ab.X, ba.X, 10);
        Assert.Equal(-ab.Y, ba.Y, 10);
        Assert.Equal(-ab.Z, ba.Z, 10);
    }

    [Fact]
    public void Cross_SelfIsZero()
    {
        var a = new Vec3(3, 4, 5);
        var result = Vec3.Cross(a, a);
        Assert.Equal(0.0, result.Length, 10);
    }

    [Fact]
    public void Cross_PerpendicularToInputs()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = Vec3.Cross(a, b);
        Assert.Equal(0, Vec3.Dot(c, a), 10);
        Assert.Equal(0, Vec3.Dot(c, b), 10);
    }

    [Fact]
    public void Cross_UnitXY_IsUnitZ()
    {
        var result = Vec3.Cross(new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(new Vec3(0, 0, 1), result);
    }

    [Fact]
    public void Cross_UnitYZ_IsUnitX()
    {
        var result = Vec3.Cross(new Vec3(0, 1, 0), new Vec3(0, 0, 1));
        Assert.Equal(new Vec3(1, 0, 0), result);
    }

    [Fact]
    public void Cross_UnitZX_IsUnitY()
    {
        var result = Vec3.Cross(new Vec3(0, 0, 1), new Vec3(1, 0, 0));
        Assert.Equal(new Vec3(0, 1, 0), result);
    }

    [Fact]
    public void Dot_Commutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Dot(a, b), Vec3.Dot(b, a));
    }

    [Fact]
    public void Dot_Orthogonal_IsZero()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        Assert.Equal(0.0, Vec3.Dot(a, b));
    }

    [Fact]
    public void Dot_Parallel_IsProductOfLengths()
    {
        var a = new Vec3(3, 0, 0);
        var b = new Vec3(4, 0, 0);
        Assert.Equal(12.0, Vec3.Dot(a, b));
    }

    [Fact]
    public void Dot_AntiParallel_IsNegative()
    {
        var a = new Vec3(3, 0, 0);
        var b = new Vec3(-4, 0, 0);
        Assert.Equal(-12.0, Vec3.Dot(a, b));
    }

    [Fact]
    public void Normalized_UnitLength()
    {
        var v = new Vec3(3, 4, 0);
        var n = v.Normalized;
        Assert.Equal(1.0, n.Length, 10);
    }

    [Fact]
    public void Normalized_Direction_Preserved()
    {
        var v = new Vec3(3, 4, 0);
        var n = v.Normalized;
        // Same direction: dot with original should be positive
        Assert.True(Vec3.Dot(v, n) > 0);
    }

    [Fact]
    public void Length_345()
    {
        var v = new Vec3(3, 4, 0);
        Assert.Equal(5.0, v.Length, 10);
    }

    [Fact]
    public void LengthSquared_345()
    {
        var v = new Vec3(3, 4, 0);
        Assert.Equal(25.0, v.LengthSquared, 10);
    }

    [Fact]
    public void DistanceSquared_Symmetric()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.DistanceSquared(a, b), Vec3.DistanceSquared(b, a));
    }

    [Fact]
    public void DistanceSquared_ToSelf_IsZero()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(0.0, Vec3.DistanceSquared(a, a));
    }

    [Fact]
    public void ScalarTripleProduct_IsDeterminant()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        // det = a · (b × c) = 1
        Assert.Equal(1.0, Vec3.Dot(a, Vec3.Cross(b, c)), 10);
    }

    [Fact]
    public void ScalarTripleProduct_CyclicPermutation()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 10);
        double abc = Vec3.Dot(a, Vec3.Cross(b, c));
        double bca = Vec3.Dot(b, Vec3.Cross(c, a));
        double cab = Vec3.Dot(c, Vec3.Cross(a, b));
        Assert.Equal(abc, bca, 10);
        Assert.Equal(bca, cab, 10);
    }

    [Fact]
    public void Zero_HasZeroLength()
    {
        Assert.Equal(0.0, Vec3.Zero.Length);
    }
}
