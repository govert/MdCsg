using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Code coverage: Vec3, Vec2, Triangle3 operators and properties</summary>
public class VecTriangleMathCoverageTests
{
    // --- Vec3 coverage ---

    [Fact]
    public void Vec3_Indexer_XYZ()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(1, v[0]);
        Assert.Equal(2, v[1]);
        Assert.Equal(3, v[2]);
    }

    [Fact]
    public void Vec3_Indexer_OutOfRange_Throws()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[-1]);
    }

    [Fact]
    public void Vec3_Negation()
    {
        var v = new Vec3(1, -2, 3);
        var neg = -v;
        Assert.Equal(-1, neg.X);
        Assert.Equal(2, neg.Y);
        Assert.Equal(-3, neg.Z);
    }

    [Fact]
    public void Vec3_ScalarLeftMultiply()
    {
        var v = new Vec3(1, 2, 3);
        var r = 2.0 * v;
        Assert.Equal(2, r.X);
        Assert.Equal(4, r.Y);
        Assert.Equal(6, r.Z);
    }

    [Fact]
    public void Vec3_Division()
    {
        var v = new Vec3(4, 6, 8);
        var r = v / 2.0;
        Assert.Equal(2, r.X);
        Assert.Equal(3, r.Y);
        Assert.Equal(4, r.Z);
    }

    [Fact]
    public void Vec3_Normalized()
    {
        var v = new Vec3(3, 0, 0);
        var n = v.Normalized;
        Assert.Equal(1.0, n.X, 10);
        Assert.Equal(0.0, n.Y, 10);
        Assert.Equal(0.0, n.Z, 10);
    }

    [Fact]
    public void Vec3_Normalized_Diagonal()
    {
        var v = new Vec3(1, 1, 1);
        var n = v.Normalized;
        double expected = 1.0 / System.Math.Sqrt(3);
        Assert.True(System.Math.Abs(n.X - expected) < 1e-12);
        Assert.True(System.Math.Abs(n.Length - 1.0) < 1e-12);
    }

    [Fact]
    public void Vec3_LengthSquared()
    {
        var v = new Vec3(3, 4, 0);
        Assert.Equal(25, v.LengthSquared);
    }

    [Fact]
    public void Vec3_Length()
    {
        var v = new Vec3(3, 4, 0);
        Assert.Equal(5.0, v.Length, 10);
    }

    [Fact]
    public void Vec3_SnapToGrid()
    {
        var v = new Vec3(1.123456789, 2.987654321, 0.5);
        var snapped = v.SnapToGrid(0.01);
        Assert.Equal(1.12, snapped.X, 10);
        Assert.Equal(2.99, snapped.Y, 10);
        Assert.Equal(0.5, snapped.Z, 10);
    }

    [Fact]
    public void Vec3_ToString_Format()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal("(1, 2, 3)", v.ToString());
    }

    [Fact]
    public void Vec3_StaticConstants()
    {
        Assert.Equal(0, Vec3.Zero.X);
        Assert.Equal(0, Vec3.Zero.Y);
        Assert.Equal(0, Vec3.Zero.Z);
        Assert.Equal(1, Vec3.UnitX.X);
        Assert.Equal(1, Vec3.UnitY.Y);
        Assert.Equal(1, Vec3.UnitZ.Z);
    }

    [Fact]
    public void Vec3_Distance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 4, 0);
        Assert.Equal(5.0, Vec3.Distance(a, b), 10);
    }

    [Fact]
    public void Vec3_DistanceSquared()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 6, 3);
        Assert.Equal(25, Vec3.DistanceSquared(a, b));
    }

    [Fact]
    public void Vec3_MinMax()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var min = Vec3.Min(a, b);
        var max = Vec3.Max(a, b);
        Assert.Equal(new Vec3(1, 2, 3), min);
        Assert.Equal(new Vec3(4, 5, 6), max);
    }

    // --- Vec2 coverage ---

    [Fact]
    public void Vec2_Operators()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(new Vec2(4, 6), a + b);
        Assert.Equal(new Vec2(-2, -2), a - b);
        Assert.Equal(new Vec2(-1, -2), -a);
    }

    [Fact]
    public void Vec2_ScalarMultiply()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(new Vec2(6, 8), v * 2);
        Assert.Equal(new Vec2(6, 8), 2 * v);
    }

    [Fact]
    public void Vec2_Division()
    {
        var v = new Vec2(6, 8);
        Assert.Equal(new Vec2(3, 4), v / 2);
    }

    [Fact]
    public void Vec2_Normalized()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        Assert.True(System.Math.Abs(n.Length - 1.0) < 1e-12);
    }

    [Fact]
    public void Vec2_LengthSquared_Length()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(25, v.LengthSquared);
        Assert.Equal(5, v.Length, 10);
    }

    [Fact]
    public void Vec2_Cross()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(1, Vec2.Cross(a, b));
        Assert.Equal(-1, Vec2.Cross(b, a));
    }

    [Fact]
    public void Vec2_Dot()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(11, Vec2.Dot(a, b));
    }

    [Fact]
    public void Vec2_ToString()
    {
        var v = new Vec2(1, 2);
        Assert.Equal("(1, 2)", v.ToString());
    }

    [Fact]
    public void Vec2_Zero()
    {
        Assert.Equal(0, Vec2.Zero.X);
        Assert.Equal(0, Vec2.Zero.Y);
    }

    // --- Triangle3 coverage ---

    [Fact]
    public void Triangle3_Normal()
    {
        var t = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(new Vec3(0, 0, 1), t.Normal);
    }

    [Fact]
    public void Triangle3_UnitNormal()
    {
        var t = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var un = t.UnitNormal;
        Assert.True(System.Math.Abs(un.Length - 1.0) < 1e-12);
        Assert.True(un.Z > 0.99);
    }

    [Fact]
    public void Triangle3_Plane()
    {
        var t = new Triangle3(
            new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        var plane = t.Plane;
        Assert.True(System.Math.Abs(plane.Normal.Z) > 0);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(new Vec3(0, 0, 1))) < 1e-10);
    }

    [Fact]
    public void Triangle3_Centroid()
    {
        var t = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 3, 0));
        var c = t.Centroid;
        Assert.Equal(1, c.X, 10);
        Assert.Equal(1, c.Y, 10);
        Assert.Equal(0, c.Z, 10);
    }

    [Fact]
    public void Triangle3_Bounds()
    {
        var t = new Triangle3(
            new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var b = t.Bounds;
        Assert.Equal(1, b.Min.X);
        Assert.Equal(2, b.Min.Y);
        Assert.Equal(3, b.Min.Z);
        Assert.Equal(7, b.Max.X);
        Assert.Equal(8, b.Max.Y);
        Assert.Equal(9, b.Max.Z);
    }

    [Fact]
    public void Triangle3_Area()
    {
        var t = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        Assert.Equal(2.0, t.Area, 10);
    }

    [Fact]
    public void Triangle3_DoubleArea()
    {
        var t = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        Assert.Equal(4.0, t.DoubleArea, 10);
    }

    [Fact]
    public void Triangle3_Indexer()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var t = new Triangle3(a, b, c);
        Assert.Equal(a, t[0]);
        Assert.Equal(b, t[1]);
        Assert.Equal(c, t[2]);
    }

    [Fact]
    public void Triangle3_Indexer_OutOfRange_Throws()
    {
        var t = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Throws<ArgumentOutOfRangeException>(() => t[3]);
    }

    // --- MathUtil coverage ---

    [Fact]
    public void MathUtil_SnapToGrid_DefaultGridSize()
    {
        double val = 1.00000001;
        double snapped = MathUtil.SnapToGrid(val);
        Assert.True(System.Math.Abs(snapped - 1.0) < 1e-7);
    }

    [Fact]
    public void MathUtil_SnapToGrid_CustomGridSize()
    {
        double snapped = MathUtil.SnapToGrid(1.37, 0.1);
        Assert.Equal(1.4, snapped, 10);
    }

    [Fact]
    public void MathUtil_Fma()
    {
        double result = MathUtil.Fma(3.0, 4.0, 5.0);
        Assert.Equal(17.0, result, 10);
    }

    [Fact]
    public void MathUtil_Constants()
    {
        Assert.Equal(1e-10, MathUtil.Epsilon);
        Assert.Equal(1e-8, MathUtil.DefaultGridSize);
    }
}
