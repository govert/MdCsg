using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 arithmetic and property tests — operators, dot, cross, normalize, snap, indexer</summary>
public class Vec3ArithmeticTests
{
    [Fact]
    public void Add_Components()
    {
        Assert.Equal(new Vec3(5, 7, 9), new Vec3(1, 2, 3) + new Vec3(4, 5, 6));
    }

    [Fact]
    public void Subtract_Components()
    {
        Assert.Equal(new Vec3(1, 2, 3), new Vec3(5, 7, 9) - new Vec3(4, 5, 6));
    }

    [Fact]
    public void Negate()
    {
        Assert.Equal(new Vec3(-1, 2, -3), -new Vec3(1, -2, 3));
    }

    [Fact]
    public void ScalarMultiply_Right()
    {
        Assert.Equal(new Vec3(2, 4, 6), new Vec3(1, 2, 3) * 2);
    }

    [Fact]
    public void ScalarMultiply_Left()
    {
        Assert.Equal(new Vec3(3, 6, 9), 3 * new Vec3(1, 2, 3));
    }

    [Fact]
    public void ScalarDivide()
    {
        Assert.Equal(new Vec3(2, 3, 4), new Vec3(4, 6, 8) / 2);
    }

    [Fact]
    public void Add_Identity()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(a, a + Vec3.Zero);
    }

    [Fact]
    public void DoubleNegate_Identity()
    {
        var a = new Vec3(1, -2, 3);
        Assert.Equal(a, -(-a));
    }

    [Fact]
    public void Dot_Orthogonal_Zero()
    {
        Assert.Equal(0, Vec3.Dot(Vec3.UnitX, Vec3.UnitY), 12);
        Assert.Equal(0, Vec3.Dot(Vec3.UnitY, Vec3.UnitZ), 12);
    }

    [Fact]
    public void Dot_Parallel_Product()
    {
        Assert.Equal(6, Vec3.Dot(new Vec3(2, 0, 0), new Vec3(3, 0, 0)), 12);
    }

    [Fact]
    public void Dot_SelfDot_IsLengthSq()
    {
        var a = new Vec3(3, 4, 5);
        Assert.Equal(a.LengthSquared, Vec3.Dot(a, a), 12);
    }

    [Fact]
    public void Cross_UnitAxes()
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
        Assert.Equal(Vec3.Cross(a, b), -Vec3.Cross(b, a));
    }

    [Fact]
    public void Cross_Parallel_Zero()
    {
        var cross = Vec3.Cross(new Vec3(1, 2, 3), new Vec3(2, 4, 6));
        Assert.True(cross.LengthSquared < 1e-24);
    }

    [Fact]
    public void Length_UnitVectors_One()
    {
        Assert.Equal(1.0, Vec3.UnitX.Length, 12);
        Assert.Equal(1.0, Vec3.UnitY.Length, 12);
        Assert.Equal(1.0, Vec3.UnitZ.Length, 12);
    }

    [Fact]
    public void Length_345()
    {
        Assert.Equal(5.0, new Vec3(3, 4, 0).Length, 12);
    }

    [Fact]
    public void Normalized_IsUnit()
    {
        Assert.True(System.Math.Abs(new Vec3(3, 4, 5).Normalized.Length - 1.0) < 1e-12);
    }

    [Fact]
    public void Distance_Same_Zero()
    {
        Assert.Equal(0.0, Vec3.Distance(new Vec3(1, 2, 3), new Vec3(1, 2, 3)), 12);
    }

    [Fact]
    public void DistanceSquared_Consistent()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 6, 8);
        double d = Vec3.Distance(a, b);
        Assert.Equal(d * d, Vec3.DistanceSquared(a, b), 10);
    }

    [Fact]
    public void Min_ComponentWise()
    {
        Assert.Equal(new Vec3(1, 2, 3), Vec3.Min(new Vec3(1, 5, 3), new Vec3(4, 2, 6)));
    }

    [Fact]
    public void Max_ComponentWise()
    {
        Assert.Equal(new Vec3(4, 5, 6), Vec3.Max(new Vec3(1, 5, 3), new Vec3(4, 2, 6)));
    }

    [Fact]
    public void SnapToGrid_RoundsCorrectly()
    {
        var v = new Vec3(0.100000001, 0.199999999, 0.300000002);
        var s = v.SnapToGrid(1e-8);
        Assert.True(System.Math.Abs(s.X - 0.1) < 1e-7);
        Assert.True(System.Math.Abs(s.Y - 0.2) < 1e-7);
    }

    [Fact]
    public void Indexer_ReturnsComponents()
    {
        var v = new Vec3(10, 20, 30);
        Assert.Equal(10, v[0]);
        Assert.Equal(20, v[1]);
        Assert.Equal(30, v[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Vec3.Zero[3]);
    }

    [Fact]
    public void ToString_ContainsComponents()
    {
        Assert.Contains("1", new Vec3(1, 2, 3).ToString());
    }

    [Fact]
    public void RecordEquality()
    {
        Assert.Equal(new Vec3(1, 2, 3), new Vec3(1, 2, 3));
    }

    [Fact]
    public void RecordInequality()
    {
        Assert.NotEqual(new Vec3(1, 2, 3), new Vec3(1, 2, 4));
    }
}
