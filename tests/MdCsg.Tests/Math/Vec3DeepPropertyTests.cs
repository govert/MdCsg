using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 — operators, Dot, Cross, Min, Max, Distance, SnapToGrid, indexer, Normalized</summary>
public class Vec3DeepPropertyTests
{
    [Fact]
    public void Addition()
    {
        var r = new Vec3(1, 2, 3) + new Vec3(4, 5, 6);
        Assert.Equal(5, r.X);
        Assert.Equal(7, r.Y);
        Assert.Equal(9, r.Z);
    }

    [Fact]
    public void Subtraction()
    {
        var r = new Vec3(10, 20, 30) - new Vec3(1, 2, 3);
        Assert.Equal(9, r.X);
        Assert.Equal(18, r.Y);
        Assert.Equal(27, r.Z);
    }

    [Fact]
    public void Negation()
    {
        var r = -new Vec3(1, -2, 3);
        Assert.Equal(-1, r.X);
        Assert.Equal(2, r.Y);
        Assert.Equal(-3, r.Z);
    }

    [Fact]
    public void ScalarMultiply_Right()
    {
        var r = new Vec3(1, 2, 3) * 2.0;
        Assert.Equal(2, r.X);
        Assert.Equal(4, r.Y);
        Assert.Equal(6, r.Z);
    }

    [Fact]
    public void ScalarMultiply_Left()
    {
        var r = 3.0 * new Vec3(1, 2, 3);
        Assert.Equal(3, r.X);
        Assert.Equal(6, r.Y);
        Assert.Equal(9, r.Z);
    }

    [Fact]
    public void ScalarDivide()
    {
        var r = new Vec3(10, 20, 30) / 10.0;
        Assert.Equal(1, r.X);
        Assert.Equal(2, r.Y);
        Assert.Equal(3, r.Z);
    }

    [Fact]
    public void Dot_Perpendicular_Zero()
    {
        Assert.True(System.Math.Abs(Vec3.Dot(Vec3.UnitX, Vec3.UnitY)) < 1e-15);
    }

    [Fact]
    public void Dot_Parallel_Product()
    {
        Assert.True(System.Math.Abs(Vec3.Dot(new Vec3(3, 0, 0), new Vec3(5, 0, 0)) - 15.0) < 1e-15);
    }

    [Fact]
    public void Cross_UnitXY_UnitZ()
    {
        var r = Vec3.Cross(Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(r.Z - 1.0) < 1e-15);
    }

    [Fact]
    public void Cross_AntiCommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.True(System.Math.Abs(ab.X + ba.X) < 1e-10);
        Assert.True(System.Math.Abs(ab.Y + ba.Y) < 1e-10);
        Assert.True(System.Math.Abs(ab.Z + ba.Z) < 1e-10);
    }

    [Fact]
    public void Min_ComponentWise()
    {
        var r = Vec3.Min(new Vec3(1, 5, 3), new Vec3(4, 2, 6));
        Assert.Equal(1, r.X);
        Assert.Equal(2, r.Y);
        Assert.Equal(3, r.Z);
    }

    [Fact]
    public void Max_ComponentWise()
    {
        var r = Vec3.Max(new Vec3(1, 5, 3), new Vec3(4, 2, 6));
        Assert.Equal(4, r.X);
        Assert.Equal(5, r.Y);
        Assert.Equal(6, r.Z);
    }

    [Fact]
    public void Distance_SamePoint_Zero()
    {
        Assert.True(Vec3.Distance(new Vec3(1, 2, 3), new Vec3(1, 2, 3)) < 1e-15);
    }

    [Fact]
    public void Distance_UnitApart()
    {
        Assert.True(System.Math.Abs(Vec3.Distance(Vec3.Zero, Vec3.UnitX) - 1.0) < 1e-15);
    }

    [Fact]
    public void DistanceSquared_Correct()
    {
        Assert.True(System.Math.Abs(Vec3.DistanceSquared(Vec3.Zero, new Vec3(1, 2, 2)) - 9.0) < 1e-15);
    }

    [Fact]
    public void SnapToGrid_SnapsCorrectly()
    {
        var v = new Vec3(0.123456, 0.654321, -0.987654);
        var snapped = v.SnapToGrid(0.1);
        Assert.True(System.Math.Abs(snapped.X - 0.1) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 0.7) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - (-1.0)) < 1e-10);
    }

    [Fact]
    public void Indexer_Works()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(1, v[0]);
        Assert.Equal(2, v[1]);
        Assert.Equal(3, v[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[3]);
    }

    [Fact]
    public void Normalized_HasUnitLength()
    {
        var v = new Vec3(3, 4, 5);
        Assert.True(System.Math.Abs(v.Normalized.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void LengthSquared_345()
    {
        var v = new Vec3(3, 4, 0);
        Assert.True(System.Math.Abs(v.LengthSquared - 25.0) < 1e-15);
    }

    [Fact]
    public void ToString_Format()
    {
        var v = new Vec3(1.5, 2.5, 3.5);
        var s = v.ToString();
        Assert.Contains("1.5", s);
        Assert.Contains("2.5", s);
        Assert.Contains("3.5", s);
    }

    [Fact]
    public void RecordEquality()
    {
        Assert.Equal(new Vec3(1, 2, 3), new Vec3(1, 2, 3));
        Assert.NotEqual(new Vec3(1, 2, 3), new Vec3(1, 2, 4));
    }

    [Fact]
    public void UnitVectors_AreOrthonormal()
    {
        Assert.True(System.Math.Abs(Vec3.Dot(Vec3.UnitX, Vec3.UnitY)) < 1e-15);
        Assert.True(System.Math.Abs(Vec3.Dot(Vec3.UnitY, Vec3.UnitZ)) < 1e-15);
        Assert.True(System.Math.Abs(Vec3.Dot(Vec3.UnitX, Vec3.UnitZ)) < 1e-15);
        Assert.True(System.Math.Abs(Vec3.UnitX.Length - 1.0) < 1e-15);
        Assert.True(System.Math.Abs(Vec3.UnitY.Length - 1.0) < 1e-15);
        Assert.True(System.Math.Abs(Vec3.UnitZ.Length - 1.0) < 1e-15);
    }
}
