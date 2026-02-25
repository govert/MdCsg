using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: MathUtil — Epsilon, DefaultGridSize, SnapToGrid, Fma correctness and edge cases</summary>
public class MathUtilFmaSnapPropertyTests
{
    [Fact]
    public void Epsilon_IsSmallPositive()
    {
        Assert.True(MathUtil.Epsilon > 0);
        Assert.True(MathUtil.Epsilon < 1e-5);
    }

    [Fact]
    public void DefaultGridSize_IsSmallPositive()
    {
        Assert.True(MathUtil.DefaultGridSize > 0);
        Assert.True(MathUtil.DefaultGridSize < 1e-4);
    }

    [Fact]
    public void SnapToGrid_ExactMultiple_Unchanged()
    {
        double val = 0.5;
        Assert.Equal(val, MathUtil.SnapToGrid(val, 0.5));
    }

    [Fact]
    public void SnapToGrid_RoundsToNearest()
    {
        double val = 0.3;
        double snapped = MathUtil.SnapToGrid(val, 0.5);
        Assert.Equal(0.5, snapped);
    }

    [Fact]
    public void SnapToGrid_ZeroValue_StaysZero()
    {
        Assert.Equal(0.0, MathUtil.SnapToGrid(0.0, 0.5));
    }

    [Fact]
    public void SnapToGrid_NegativeValue_SnapsCorrectly()
    {
        double val = -0.3;
        double snapped = MathUtil.SnapToGrid(val, 0.5);
        Assert.Equal(-0.5, snapped);
    }

    [Fact]
    public void SnapToGrid_DefaultGridSize_SmallChange()
    {
        double val = 1.0 + 1e-10;
        double snapped = MathUtil.SnapToGrid(val);
        Assert.True(System.Math.Abs(snapped - 1.0) < 1e-6);
    }

    [Fact]
    public void SnapToGrid_Idempotent()
    {
        double val = 0.123456;
        double grid = 0.01;
        double snap1 = MathUtil.SnapToGrid(val, grid);
        double snap2 = MathUtil.SnapToGrid(snap1, grid);
        Assert.Equal(snap1, snap2);
    }

    [Fact]
    public void Fma_SimpleCase_Correct()
    {
        double result = MathUtil.Fma(2.0, 3.0, 4.0);
        Assert.Equal(10.0, result);
    }

    [Fact]
    public void Fma_ZeroAdd_IsProduct()
    {
        Assert.Equal(6.0, MathUtil.Fma(2.0, 3.0, 0.0));
    }

    [Fact]
    public void Fma_ZeroProduct_IsAdd()
    {
        Assert.Equal(5.0, MathUtil.Fma(0.0, 3.0, 5.0));
    }

    [Fact]
    public void Fma_NegativeValues()
    {
        Assert.Equal(-2.0, MathUtil.Fma(-1.0, 3.0, 1.0)); // -3 + 1 = -2
    }

    [Fact]
    public void Vec3_SnapToGrid_AllComponents()
    {
        var v = new Vec3(0.123, 0.456, 0.789);
        var snapped = v.SnapToGrid(0.25);
        Assert.True(System.Math.Abs(snapped.X - 0.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 0.5) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 0.75) < 1e-10);
    }

    [Fact]
    public void Segment_Properties_Correct()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(3, 4, 0));
        Assert.Equal(5.0, seg.Length);
        Assert.Equal(new Vec3(1.5, 2, 0), seg.Midpoint);
        Assert.Equal(new Vec3(3, 4, 0), seg.Direction);
    }

    [Fact]
    public void Segment_PointAt_Zero_IsStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(seg.Start, seg.PointAt(0));
    }

    [Fact]
    public void Segment_PointAt_One_IsEnd()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(seg.End, seg.PointAt(1));
    }

    [Fact]
    public void Segment_PointAt_Half_IsMidpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(4, 6, 8));
        var mid = seg.PointAt(0.5);
        Assert.True(Vec3.DistanceSquared(mid, seg.Midpoint) < 1e-20);
    }

    [Fact]
    public void Ray_PointAt_Zero_IsOrigin()
    {
        var ray = new Ray(new Vec3(1, 2, 3), Vec3.UnitX);
        Assert.Equal(ray.Origin, ray.PointAt(0));
    }

    [Fact]
    public void Ray_PointAt_AdvancesAlongDirection()
    {
        var ray = new Ray(Vec3.Zero, Vec3.UnitX);
        var point = ray.PointAt(5.0);
        Assert.Equal(new Vec3(5, 0, 0), point);
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_ZeroLength()
    {
        var seg = new MdCsg.Intersection.IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 0);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_NonDegenerate()
    {
        var seg = new MdCsg.Intersection.IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 1);
        Assert.False(seg.IsDegenerate);
    }
}
