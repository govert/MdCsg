using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Triangle3 — indexer, Plane, Bounds, Area, Centroid, degenerate triangles</summary>
public class Triangle3AdvancedTests
{
    [Fact]
    public void Indexer_ReturnsCorrectVertices()
    {
        var t = new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 2, 0), new Vec3(0, 0, 3));
        Assert.Equal(new Vec3(1, 0, 0), t[0]);
        Assert.Equal(new Vec3(0, 2, 0), t[1]);
        Assert.Equal(new Vec3(0, 0, 3), t[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var t = new Triangle3(Vec3.Zero, Vec3.Zero, Vec3.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() => t[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => t[-1]);
    }

    [Fact]
    public void Normal_RightHandRule()
    {
        // Triangle in XY plane, CCW → normal along +Z
        var t = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(t.Normal.Z > 0);
        Assert.Equal(0, t.Normal.X, 10);
        Assert.Equal(0, t.Normal.Y, 10);
    }

    [Fact]
    public void UnitNormal_HasLengthOne()
    {
        var t = new Triangle3(Vec3.Zero, new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.Equal(1.0, t.UnitNormal.Length, 10);
    }

    [Fact]
    public void Area_RightTriangle()
    {
        // 3-4-5 triangle has area = 6
        var t = new Triangle3(Vec3.Zero, new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.Equal(6.0, t.Area, 10);
    }

    [Fact]
    public void DoubleArea_IsTwiceArea()
    {
        var t = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(2 * t.Area, t.DoubleArea, 10);
    }

    [Fact]
    public void Centroid_IsAverage()
    {
        var t = new Triangle3(new Vec3(3, 0, 0), new Vec3(0, 6, 0), new Vec3(0, 0, 9));
        Assert.Equal(new Vec3(1, 2, 3), t.Centroid);
    }

    [Fact]
    public void Plane_ContainsAllVertices()
    {
        var t = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var plane = t.Plane;

        Assert.True(System.Math.Abs(plane.SignedDistanceTo(t.A)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(t.B)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(t.C)) < 1e-10);
    }

    [Fact]
    public void Bounds_ContainsAllVertices()
    {
        var t = new Triangle3(new Vec3(-1, 2, -3), new Vec3(4, -5, 6), new Vec3(7, -8, 9));
        var b = t.Bounds;
        Assert.True(b.Contains(t.A));
        Assert.True(b.Contains(t.B));
        Assert.True(b.Contains(t.C));
    }

    [Fact]
    public void Bounds_MinMax_Correct()
    {
        var t = new Triangle3(new Vec3(1, 5, 3), new Vec3(4, 2, 6), new Vec3(7, 8, 0));
        Assert.Equal(new Vec3(1, 2, 0), t.Bounds.Min);
        Assert.Equal(new Vec3(7, 8, 6), t.Bounds.Max);
    }

    [Fact]
    public void DegenerateTriangle_ZeroArea()
    {
        // Collinear points
        var t = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(2, 0, 0));
        Assert.Equal(0, t.Area, 10);
    }

    [Fact]
    public void DegenerateTriangle_CoincidentPoints()
    {
        var t = new Triangle3(Vec3.Zero, Vec3.Zero, Vec3.Zero);
        Assert.Equal(0, t.Area, 10);
    }

    [Fact]
    public void RecordEquality()
    {
        var t1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(t1, t2);
    }

    [Fact]
    public void RecordInequality_DifferentWinding()
    {
        var t1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(1, 0, 0));
        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void Normal_FlipsWithWinding()
    {
        var t1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(1, 0, 0));
        // Reversed winding → opposite normal direction
        Assert.True(Vec3.Dot(t1.Normal, t2.Normal) < 0);
    }

    [Fact]
    public void Centroid_InsideBounds()
    {
        var t = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        Assert.True(t.Bounds.Contains(t.Centroid));
    }
}
