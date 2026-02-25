using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 — comprehensive operator, geometric, and algebraic property tests</summary>
public class Vec3ComprehensivePropertyTests
{
    [Fact]
    public void Zero_AllComponentsZero()
    {
        Assert.Equal(0.0, Vec3.Zero.X);
        Assert.Equal(0.0, Vec3.Zero.Y);
        Assert.Equal(0.0, Vec3.Zero.Z);
    }

    [Fact]
    public void UnitVectors_AreUnit()
    {
        Assert.Equal(1.0, Vec3.UnitX.Length, 15);
        Assert.Equal(1.0, Vec3.UnitY.Length, 15);
        Assert.Equal(1.0, Vec3.UnitZ.Length, 15);
    }

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
        var lhs = (a + b) + c;
        var rhs = a + (b + c);
        Assert.Equal(lhs.X, rhs.X, 15);
        Assert.Equal(lhs.Y, rhs.Y, 15);
        Assert.Equal(lhs.Z, rhs.Z, 15);
    }

    [Fact]
    public void Addition_ZeroIdentity()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a, a + Vec3.Zero);
    }

    [Fact]
    public void Subtraction_SameVector_Zero()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(Vec3.Zero, a - a);
    }

    [Fact]
    public void Negation_DoubleNegation_Identity()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a, -(-a));
    }

    [Fact]
    public void ScalarMul_OneIdentity()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a, a * 1.0);
    }

    [Fact]
    public void ScalarMul_ZeroGivesZero()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(Vec3.Zero, a * 0.0);
    }

    [Fact]
    public void ScalarMul_Commutative()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a * 3.0, 3.0 * a);
    }

    [Fact]
    public void ScalarDiv_ByOne_Identity()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a, a / 1.0);
    }

    [Fact]
    public void ScalarDiv_ByTwo_HalvesComponents()
    {
        var a = new Vec3(2, 4, 6);
        Assert.Equal(new Vec3(1, 2, 3), a / 2.0);
    }

    [Fact]
    public void Dot_Orthogonal_Zero()
    {
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitX, Vec3.UnitY), 15);
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitY, Vec3.UnitZ), 15);
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitZ, Vec3.UnitX), 15);
    }

    [Fact]
    public void Dot_Parallel_LengthProduct()
    {
        var a = new Vec3(3, 0, 0);
        var b = new Vec3(5, 0, 0);
        Assert.Equal(15.0, Vec3.Dot(a, b), 15);
    }

    [Fact]
    public void Dot_Commutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Dot(a, b), Vec3.Dot(b, a), 15);
    }

    [Fact]
    public void Cross_UnitVectors_RightHandRule()
    {
        Assert.Equal(Vec3.UnitZ, Vec3.Cross(Vec3.UnitX, Vec3.UnitY));
        Assert.Equal(Vec3.UnitX, Vec3.Cross(Vec3.UnitY, Vec3.UnitZ));
        Assert.Equal(Vec3.UnitY, Vec3.Cross(Vec3.UnitZ, Vec3.UnitX));
    }

    [Fact]
    public void Cross_AntiCommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.Equal(-ab.X, ba.X, 15);
        Assert.Equal(-ab.Y, ba.Y, 15);
        Assert.Equal(-ab.Z, ba.Z, 15);
    }

    [Fact]
    public void Cross_WithSelf_Zero()
    {
        var a = new Vec3(1, 2, 3);
        var cross = Vec3.Cross(a, a);
        Assert.Equal(0.0, cross.X, 15);
        Assert.Equal(0.0, cross.Y, 15);
        Assert.Equal(0.0, cross.Z, 15);
    }

    [Fact]
    public void Cross_OrthogonalToInputs()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = Vec3.Cross(a, b);
        Assert.True(System.Math.Abs(Vec3.Dot(c, a)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Dot(c, b)) < 1e-10);
    }

    [Fact]
    public void Length_UnitVector_IsOne()
    {
        Assert.Equal(1.0, new Vec3(1, 0, 0).Length, 15);
    }

    [Fact]
    public void Length_345Triangle()
    {
        Assert.Equal(5.0, new Vec3(3, 4, 0).Length, 15);
    }

    [Fact]
    public void LengthSquared_Consistent()
    {
        var a = new Vec3(3, 4, 5);
        Assert.Equal(a.Length * a.Length, a.LengthSquared, 10);
    }

    [Fact]
    public void Normalized_IsUnitLength()
    {
        var a = new Vec3(3, 4, 5);
        Assert.Equal(1.0, a.Normalized.Length, 10);
    }

    [Fact]
    public void Normalized_SameDirection()
    {
        var a = new Vec3(3, 4, 0);
        var n = a.Normalized;
        Assert.Equal(0.6, n.X, 10);
        Assert.Equal(0.8, n.Y, 10);
    }

    [Fact]
    public void Distance_ToSelf_Zero()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(0.0, Vec3.Distance(a, a), 15);
    }

    [Fact]
    public void Distance_Symmetric()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Distance(a, b), Vec3.Distance(b, a), 15);
    }

    [Fact]
    public void DistanceSquared_EqualsDistanceTimesDistance()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        double d = Vec3.Distance(a, b);
        Assert.Equal(d * d, Vec3.DistanceSquared(a, b), 10);
    }

    [Fact]
    public void Min_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var m = Vec3.Min(a, b);
        Assert.Equal(1.0, m.X);
        Assert.Equal(2.0, m.Y);
        Assert.Equal(3.0, m.Z);
    }

    [Fact]
    public void Max_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var m = Vec3.Max(a, b);
        Assert.Equal(4.0, m.X);
        Assert.Equal(5.0, m.Y);
        Assert.Equal(6.0, m.Z);
    }

    [Fact]
    public void Indexer_ReturnsCorrectComponents()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(1.0, v[0]);
        Assert.Equal(2.0, v[1]);
        Assert.Equal(3.0, v[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[-1]);
    }

    [Fact]
    public void ToString_Format()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Contains("1", v.ToString());
        Assert.Contains("2", v.ToString());
        Assert.Contains("3", v.ToString());
    }

    [Fact]
    public void SnapToGrid_RoundsComponents()
    {
        var v = new Vec3(1.0 + 3e-9, 2.0 + 3e-9, 3.0 + 3e-9);
        var snapped = v.SnapToGrid(1e-8);
        Assert.True(System.Math.Abs(snapped.X - 1.0) < 1e-7);
        Assert.True(System.Math.Abs(snapped.Y - 2.0) < 1e-7);
        Assert.True(System.Math.Abs(snapped.Z - 3.0) < 1e-7);
    }
}
