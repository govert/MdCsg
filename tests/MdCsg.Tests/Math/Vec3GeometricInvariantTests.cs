using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 geometric invariants — cross product properties, normalization, distance, indexer, edge cases</summary>
public class Vec3GeometricInvariantTests
{
    [Fact]
    public void Cross_AntiCommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.Equal(-ab.X, ba.X, 1e-14);
        Assert.Equal(-ab.Y, ba.Y, 1e-14);
        Assert.Equal(-ab.Z, ba.Z, 1e-14);
    }

    [Fact]
    public void Cross_SelfIsZero()
    {
        var a = new Vec3(3, 7, -2);
        var cross = Vec3.Cross(a, a);
        Assert.Equal(0, cross.X, 1e-14);
        Assert.Equal(0, cross.Y, 1e-14);
        Assert.Equal(0, cross.Z, 1e-14);
    }

    [Fact]
    public void Cross_PerpendicularToInputs()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var cross = Vec3.Cross(a, b);
        Assert.Equal(0, Vec3.Dot(cross, a), 1e-10);
        Assert.Equal(0, Vec3.Dot(cross, b), 1e-10);
    }

    [Fact]
    public void Cross_UnitAxes_RightHandRule()
    {
        Assert.Equal(Vec3.UnitZ, Vec3.Cross(Vec3.UnitX, Vec3.UnitY));
        Assert.Equal(Vec3.UnitX, Vec3.Cross(Vec3.UnitY, Vec3.UnitZ));
        Assert.Equal(Vec3.UnitY, Vec3.Cross(Vec3.UnitZ, Vec3.UnitX));
    }

    [Fact]
    public void Cross_Magnitude_IsSinOfAngle()
    {
        // |a × b| = |a||b|sin(θ)
        // For perpendicular unit vectors: |a × b| = 1
        var a = Vec3.UnitX;
        var b = Vec3.UnitY;
        Assert.Equal(1.0, Vec3.Cross(a, b).Length, 1e-14);
    }

    [Fact]
    public void Dot_Commutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Dot(a, b), Vec3.Dot(b, a), 1e-14);
    }

    [Fact]
    public void Dot_SelfIs_LengthSquared()
    {
        var a = new Vec3(3, 4, 5);
        Assert.Equal(a.LengthSquared, Vec3.Dot(a, a), 1e-14);
    }

    [Fact]
    public void Dot_Perpendicular_IsZero()
    {
        Assert.Equal(0, Vec3.Dot(Vec3.UnitX, Vec3.UnitY), 1e-14);
        Assert.Equal(0, Vec3.Dot(Vec3.UnitY, Vec3.UnitZ), 1e-14);
        Assert.Equal(0, Vec3.Dot(Vec3.UnitX, Vec3.UnitZ), 1e-14);
    }

    [Fact]
    public void Normalized_HasUnitLength()
    {
        var v = new Vec3(3, 4, 5);
        Assert.Equal(1.0, v.Normalized.Length, 1e-14);
    }

    [Fact]
    public void Normalized_PreservesDirection()
    {
        var v = new Vec3(3, 4, 0);
        var n = v.Normalized;
        // Angle between v and n should be zero
        Assert.True(Vec3.Dot(v, n) > 0);
        // Cross product should be zero
        Assert.Equal(0, Vec3.Cross(v, n).Length, 1e-10);
    }

    [Fact]
    public void Normalized_UnitVector_IsSame()
    {
        var n = Vec3.UnitX.Normalized;
        Assert.Equal(1, n.X, 1e-14);
        Assert.Equal(0, n.Y, 1e-14);
        Assert.Equal(0, n.Z, 1e-14);
    }

    [Fact]
    public void Distance_Symmetric()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Distance(a, b), Vec3.Distance(b, a), 1e-14);
    }

    [Fact]
    public void Distance_SelfIsZero()
    {
        var a = new Vec3(7, 8, 9);
        Assert.Equal(0, Vec3.Distance(a, a), 1e-14);
    }

    [Fact]
    public void DistanceSquared_IsSquareOfDistance()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var d = Vec3.Distance(a, b);
        Assert.Equal(d * d, Vec3.DistanceSquared(a, b), 1e-10);
    }

    [Fact]
    public void Distance_UnitAxes()
    {
        Assert.Equal(System.Math.Sqrt(2), Vec3.Distance(Vec3.UnitX, Vec3.UnitY), 1e-14);
    }

    [Fact]
    public void Indexer_ReturnsCorrectComponents()
    {
        var v = new Vec3(10, 20, 30);
        Assert.Equal(10, v[0]);
        Assert.Equal(20, v[1]);
        Assert.Equal(30, v[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var v = Vec3.Zero;
        Assert.Throws<ArgumentOutOfRangeException>(() => v[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[3]);
    }

    [Fact]
    public void Min_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var m = Vec3.Min(a, b);
        Assert.Equal(1, m.X);
        Assert.Equal(2, m.Y);
        Assert.Equal(3, m.Z);
    }

    [Fact]
    public void Max_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var m = Vec3.Max(a, b);
        Assert.Equal(4, m.X);
        Assert.Equal(5, m.Y);
        Assert.Equal(6, m.Z);
    }

    [Fact]
    public void Negation_DoubleNegation_Identity()
    {
        var v = new Vec3(3, -4, 5);
        Assert.Equal(v, -(-v));
    }

    [Fact]
    public void Addition_WithZero_Identity()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v, v + Vec3.Zero);
    }

    [Fact]
    public void Subtraction_Self_IsZero()
    {
        var v = new Vec3(7, 8, 9);
        Assert.Equal(Vec3.Zero, v - v);
    }

    [Fact]
    public void ScalarMultiply_Zero_IsZero()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(Vec3.Zero, v * 0);
    }

    [Fact]
    public void ScalarMultiply_One_Identity()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v, v * 1);
    }

    [Fact]
    public void ScalarMultiply_LeftRight_Same()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v * 5.0, 5.0 * v);
    }

    [Fact]
    public void Division_ByOne_Identity()
    {
        var v = new Vec3(4, 5, 6);
        Assert.Equal(v, v / 1.0);
    }

    [Fact]
    public void SnapToGrid_RoundsToGrid()
    {
        var v = new Vec3(1.0000000049, 2.0000000051, 3.0);
        var snapped = v.SnapToGrid(1e-8);
        // Values should snap to nearest 1e-8 grid point
        Assert.Equal(1.0, snapped.X, 1e-9);
        Assert.Equal(2.00000001, snapped.Y, 1e-9);
        Assert.Equal(3.0, snapped.Z, 1e-14);
    }

    [Fact]
    public void LengthSquared_NonNegative()
    {
        var v = new Vec3(-3, -4, -5);
        Assert.True(v.LengthSquared >= 0);
    }

    [Fact]
    public void Length_345_Is_5Sqrt2()
    {
        var v = new Vec3(3, 4, 5);
        Assert.Equal(System.Math.Sqrt(50), v.Length, 1e-14);
    }

    [Fact]
    public void ToString_ContainsComponents()
    {
        var v = new Vec3(1.5, 2.5, 3.5);
        var s = v.ToString();
        Assert.Contains("1.5", s);
        Assert.Contains("2.5", s);
        Assert.Contains("3.5", s);
    }

    [Fact]
    public void RecordEquality_SameValues()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(1, 2, 3);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void RecordEquality_DifferentValues()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(1, 2, 4);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Cross_Distributive_OverAddition()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var lhs = Vec3.Cross(a, b + c);
        var rhs = Vec3.Cross(a, b) + Vec3.Cross(a, c);
        Assert.Equal(lhs.X, rhs.X, 1e-10);
        Assert.Equal(lhs.Y, rhs.Y, 1e-10);
        Assert.Equal(lhs.Z, rhs.Z, 1e-10);
    }

    [Fact]
    public void Cross_ScalarFactor()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var lhs = Vec3.Cross(a * 3.0, b);
        var rhs = Vec3.Cross(a, b) * 3.0;
        Assert.Equal(lhs.X, rhs.X, 1e-10);
        Assert.Equal(lhs.Y, rhs.Y, 1e-10);
        Assert.Equal(lhs.Z, rhs.Z, 1e-10);
    }
}
