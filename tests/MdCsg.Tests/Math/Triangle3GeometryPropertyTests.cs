using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Triangle3 — area, normal, centroid, bounds, plane, indexer</summary>
public class Triangle3GeometryPropertyTests
{
    [Fact]
    public void Area_UnitRightTriangle_IsHalf()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(tri.Area - 0.5) < 1e-10);
    }

    [Fact]
    public void Area_ScaledTriangle_ScalesQuadratically()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        Assert.True(System.Math.Abs(tri.Area - 2.0) < 1e-10);
    }

    [Fact]
    public void Area_DegenerateTriangle_Zero()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, new Vec3(2, 0, 0));
        Assert.True(tri.Area < 1e-10);
    }

    [Fact]
    public void Normal_XYPlane_PointsZ()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(tri.UnitNormal.Z - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(tri.UnitNormal.X) < 1e-10);
        Assert.True(System.Math.Abs(tri.UnitNormal.Y) < 1e-10);
    }

    [Fact]
    public void Normal_ReversedWinding_FlipsNormal()
    {
        var tri1 = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var tri2 = new Triangle3(Vec3.Zero, Vec3.UnitY, Vec3.UnitX);
        Assert.True(System.Math.Abs(tri1.UnitNormal.Z + tri2.UnitNormal.Z) < 1e-10);
    }

    [Fact]
    public void Centroid_IsAverage()
    {
        var a = new Vec3(3, 6, 9);
        var b = new Vec3(0, 3, 6);
        var c = new Vec3(6, 0, 3);
        var tri = new Triangle3(a, b, c);
        Assert.True(System.Math.Abs(tri.Centroid.X - 3.0) < 1e-10);
        Assert.True(System.Math.Abs(tri.Centroid.Y - 3.0) < 1e-10);
        Assert.True(System.Math.Abs(tri.Centroid.Z - 6.0) < 1e-10);
    }

    [Fact]
    public void Bounds_ContainsAllVertices()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(7, 2, 8);
        var c = new Vec3(4, 9, 1);
        var tri = new Triangle3(a, b, c);
        Assert.True(tri.Bounds.Contains(a));
        Assert.True(tri.Bounds.Contains(b));
        Assert.True(tri.Bounds.Contains(c));
    }

    [Fact]
    public void Bounds_MinMaxCorrect()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 0, 1));
        Assert.Equal(1.0, tri.Bounds.Min.X);
        Assert.Equal(0.0, tri.Bounds.Min.Y);
        Assert.Equal(1.0, tri.Bounds.Min.Z);
        Assert.Equal(7.0, tri.Bounds.Max.X);
        Assert.Equal(5.0, tri.Bounds.Max.Y);
        Assert.Equal(6.0, tri.Bounds.Max.Z);
    }

    [Fact]
    public void Plane_VerticesOnPlane()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var plane = tri.Plane;
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.A)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.B)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.C)) < 1e-10);
    }

    [Fact]
    public void DoubleArea_IsTwiceArea()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.True(System.Math.Abs(tri.DoubleArea - 2 * tri.Area) < 1e-10);
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
    public void Area_EquilateralTriangle_Correct()
    {
        // Equilateral triangle with side length 2
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(1, System.Math.Sqrt(3), 0);
        var tri = new Triangle3(a, b, c);
        double expected = System.Math.Sqrt(3); // side^2 * sqrt(3)/4 = 4*sqrt(3)/4
        Assert.True(System.Math.Abs(tri.Area - expected) < 1e-10);
    }

    [Fact]
    public void UnitNormal_IsUnitLength()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 0, 1));
        Assert.True(System.Math.Abs(tri.UnitNormal.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Normal_PerpendicularToEdges()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var n = tri.Normal;
        Assert.True(System.Math.Abs(Vec3.Dot(n, tri.B - tri.A)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Dot(n, tri.C - tri.A)) < 1e-10);
    }
}
