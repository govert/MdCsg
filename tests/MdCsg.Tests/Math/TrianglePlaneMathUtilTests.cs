using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Batch 3: Triangle3, Plane, MathUtil extended tests (20 tests)</summary>
public class TrianglePlaneMathUtilTests
{
    // --- Triangle3 ---

    [Fact]
    public void Triangle_Normal_XYPlane()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var n = tri.Normal;
        Assert.Equal(0, n.X, 1e-15);
        Assert.Equal(0, n.Y, 1e-15);
        Assert.True(n.Z > 0); // CCW in XY -> positive Z
    }

    [Fact]
    public void Triangle_UnitNormal_HasUnitLength()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.Equal(1.0, tri.UnitNormal.Length, 1e-15);
    }

    [Fact]
    public void Triangle_Area_UnitRightTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.5, tri.Area, 1e-15);
    }

    [Fact]
    public void Triangle_Area_ScaledTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 3, 0));
        Assert.Equal(3.0, tri.Area, 1e-15);
    }

    [Fact]
    public void Triangle_Centroid()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 3, 0));
        Assert.Equal(new Vec3(1, 1, 0), tri.Centroid);
    }

    [Fact]
    public void Triangle_Bounds()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 1, 5), new Vec3(2, 6, 1));
        var bounds = tri.Bounds;
        Assert.Equal(new Vec3(1, 1, 1), bounds.Min);
        Assert.Equal(new Vec3(4, 6, 5), bounds.Max);
    }

    [Fact]
    public void Triangle_Indexer()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var tri = new Triangle3(a, b, c);
        Assert.Equal(a, tri[0]);
        Assert.Equal(b, tri[1]);
        Assert.Equal(c, tri[2]);
    }

    [Fact]
    public void Triangle_Indexer_ThrowsOutOfRange()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[3]);
    }

    [Fact]
    public void Triangle_DoubleArea_IsTwiceArea()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(2 * tri.Area, tri.DoubleArea, 1e-15);
    }

    // --- Plane ---

    [Fact]
    public void Plane_FromPoints_XYPlane()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0, p.Normal.X, 1e-15);
        Assert.Equal(0, p.Normal.Y, 1e-15);
        Assert.Equal(1, p.Normal.Z, 1e-15);
        Assert.Equal(0, p.Distance, 1e-15);
    }

    [Fact]
    public void Plane_FromPoints_OffsetPlane()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.Equal(5, p.Distance, 1e-15);
    }

    [Fact]
    public void Plane_SignedDistance_Above()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(p.SignedDistanceTo(new Vec3(0, 0, 1)) > 0);
    }

    [Fact]
    public void Plane_SignedDistance_Below()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(p.SignedDistanceTo(new Vec3(0, 0, -1)) < 0);
    }

    [Fact]
    public void Plane_SignedDistance_OnPlane()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0, p.SignedDistanceTo(new Vec3(0.5, 0.5, 0)), 1e-15);
    }

    [Fact]
    public void Plane_Flipped_ReversesNormal()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var f = p.Flipped;
        Assert.Equal(-p.Normal.X, f.Normal.X, 1e-15);
        Assert.Equal(-p.Normal.Y, f.Normal.Y, 1e-15);
        Assert.Equal(-p.Normal.Z, f.Normal.Z, 1e-15);
        Assert.Equal(-p.Distance, f.Distance, 1e-15);
    }

    [Fact]
    public void Plane_Flipped_NegatesSignedDistance()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var pt = new Vec3(0, 0, 1);
        Assert.Equal(-p.SignedDistanceTo(pt), p.Flipped.SignedDistanceTo(pt), 1e-15);
    }

    // --- MathUtil ---

    [Fact]
    public void MathUtil_SnapToGrid_RoundsCorrectly()
    {
        Assert.Equal(1.0, MathUtil.SnapToGrid(1.0000000049, 1e-8), 1e-15);
        Assert.Equal(1.00000001, MathUtil.SnapToGrid(1.0000000051, 1e-8), 1e-12);
    }

    [Fact]
    public void MathUtil_SnapToGrid_NegativeValues()
    {
        Assert.Equal(-1.0, MathUtil.SnapToGrid(-1.0000000049, 1e-8), 1e-15);
    }

    [Fact]
    public void MathUtil_Fma_BasicOperation()
    {
        double result = MathUtil.Fma(2.0, 3.0, 4.0);
        Assert.Equal(10.0, result, 1e-15);
    }

    [Fact]
    public void MathUtil_Epsilon_IsSmallPositive()
    {
        Assert.True(MathUtil.Epsilon > 0);
        Assert.True(MathUtil.Epsilon < 1e-5);
    }
}
