using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Triangle3 — computed properties: Area, Centroid, Plane, Bounds, indexer, Normal</summary>
public class Triangle3ComputedPropertyTests
{
    [Fact]
    public void Area_UnitTriangleInXY()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.5, tri.Area, 10);
    }

    [Fact]
    public void Area_ScaledTriangle()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        Assert.Equal(2.0, tri.Area, 10);
    }

    [Fact]
    public void Area_NonOriginTriangle()
    {
        var tri = new Triangle3(new Vec3(1, 1, 1), new Vec3(2, 1, 1), new Vec3(1, 2, 1));
        Assert.Equal(0.5, tri.Area, 10);
    }

    [Fact]
    public void DoubleArea_IsTwiceArea()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.Equal(2.0 * tri.Area, tri.DoubleArea, 10);
    }

    [Fact]
    public void Normal_XYPlaneTriangle_PointsInZ()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var n = tri.Normal;
        Assert.Equal(0.0, n.X, 10);
        Assert.Equal(0.0, n.Y, 10);
        Assert.True(n.Z > 0, $"Normal should point +Z, got {n}");
    }

    [Fact]
    public void UnitNormal_IsUnitLength()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.Equal(1.0, tri.UnitNormal.Length, 10);
    }

    [Fact]
    public void UnitNormal_SameDirectionAsNormal()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(Vec3.Dot(tri.Normal, tri.UnitNormal) > 0);
    }

    [Fact]
    public void Centroid_IsAverageOfVertices()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 6, 0);
        var tri = new Triangle3(a, b, c);
        Assert.Equal(1.0, tri.Centroid.X, 10);
        Assert.Equal(2.0, tri.Centroid.Y, 10);
        Assert.Equal(0.0, tri.Centroid.Z, 10);
    }

    [Fact]
    public void Centroid_InsideBounds()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 0, 1), new Vec3(2, 5, 0));
        Assert.True(tri.Bounds.Contains(tri.Centroid),
            $"Centroid {tri.Centroid} should be inside bounds");
    }

    [Fact]
    public void Plane_VerticesOnPlane()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 0, 1);
        var c = new Vec3(2, 5, 0);
        var tri = new Triangle3(a, b, c);
        var plane = tri.Plane;
        Assert.Equal(0.0, plane.SignedDistanceTo(a), 8);
        Assert.Equal(0.0, plane.SignedDistanceTo(b), 8);
        Assert.Equal(0.0, plane.SignedDistanceTo(c), 8);
    }

    [Fact]
    public void Plane_NormalDirection()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var plane = tri.Plane;
        Assert.True(plane.Normal.Z > 0, $"Plane normal should point +Z for CCW XY triangle");
    }

    [Fact]
    public void Bounds_MinMaxCorrect()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var c = new Vec3(2, 3, 1);
        var tri = new Triangle3(a, b, c);
        var bounds = tri.Bounds;
        Assert.Equal(1.0, bounds.Min.X, 15);
        Assert.Equal(2.0, bounds.Min.Y, 15);
        Assert.Equal(1.0, bounds.Min.Z, 15);
        Assert.Equal(4.0, bounds.Max.X, 15);
        Assert.Equal(5.0, bounds.Max.Y, 15);
        Assert.Equal(6.0, bounds.Max.Z, 15);
    }

    [Fact]
    public void Indexer_ReturnsCorrectVertex()
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
    public void Indexer_OutOfRange_Throws()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[-1]);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var b = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordInequality()
    {
        var a = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var b = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitZ);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void DegenerateTriangle_ZeroArea()
    {
        // Collinear points
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(2, 0, 0));
        Assert.Equal(0.0, tri.Area, 10);
    }
}
