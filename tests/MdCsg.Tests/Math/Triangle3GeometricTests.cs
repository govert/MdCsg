using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Triangle3 geometric properties — area, normal, centroid, bounds, indexer, degenerate cases</summary>
public class Triangle3GeometricTests
{
    [Fact]
    public void Area_UnitRightTriangle()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.5, tri.Area, 1e-14);
    }

    [Fact]
    public void Area_EquilateralTriangle()
    {
        // Equilateral with side = 2 in XY plane
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, System.Math.Sqrt(3), 0));
        Assert.Equal(System.Math.Sqrt(3), tri.Area, 1e-10);
    }

    [Fact]
    public void DoubleArea_IsDouble_OfArea()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.Equal(tri.Area * 2.0, tri.DoubleArea, 1e-14);
    }

    [Fact]
    public void Area_DegenerateCollinear_IsZero()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(2, 0, 0));
        Assert.Equal(0.0, tri.Area, 1e-14);
    }

    [Fact]
    public void Area_DegeneratePoint_IsZero()
    {
        var p = new Vec3(5, 5, 5);
        var tri = new Triangle3(p, p, p);
        Assert.Equal(0.0, tri.Area, 1e-14);
    }

    [Fact]
    public void Normal_XYPlane_PointsAlongZ()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var n = tri.Normal;
        Assert.Equal(0, n.X, 1e-14);
        Assert.Equal(0, n.Y, 1e-14);
        Assert.True(n.Z > 0);
    }

    [Fact]
    public void Normal_XZPlane_PointsAlongNegY()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        var n = tri.Normal;
        Assert.True(n.Y < 0); // CCW in XZ => -Y
    }

    [Fact]
    public void UnitNormal_IsNormalized()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        var un = tri.UnitNormal;
        Assert.Equal(1.0, un.Length, 1e-14);
    }

    [Fact]
    public void UnitNormal_PerpendicularToEdges()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        var un = tri.UnitNormal;
        Assert.Equal(0, Vec3.Dot(un, tri.B - tri.A), 1e-14);
        Assert.Equal(0, Vec3.Dot(un, tri.C - tri.A), 1e-14);
    }

    [Fact]
    public void Centroid_IsAverageOfVertices()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var tri = new Triangle3(a, b, c);
        var centroid = tri.Centroid;
        Assert.Equal(4, centroid.X, 1e-14);
        Assert.Equal(5, centroid.Y, 1e-14);
        Assert.Equal(6, centroid.Z, 1e-14);
    }

    [Fact]
    public void Centroid_Origin()
    {
        var tri = new Triangle3(new Vec3(-1, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 0));
        Assert.Equal(0, tri.Centroid.X, 1e-14);
        Assert.Equal(0, tri.Centroid.Y, 1e-14);
        Assert.Equal(0, tri.Centroid.Z, 1e-14);
    }

    [Fact]
    public void Bounds_ContainsAllVertices()
    {
        var a = new Vec3(-1, 5, 2);
        var b = new Vec3(3, -2, 0);
        var c = new Vec3(0, 4, 10);
        var tri = new Triangle3(a, b, c);
        Assert.True(tri.Bounds.Contains(a));
        Assert.True(tri.Bounds.Contains(b));
        Assert.True(tri.Bounds.Contains(c));
    }

    [Fact]
    public void Bounds_MinMaxCorrect()
    {
        var tri = new Triangle3(new Vec3(1, -3, 5), new Vec3(7, 2, -1), new Vec3(4, 8, 3));
        Assert.Equal(1, tri.Bounds.Min.X, 1e-14);
        Assert.Equal(-3, tri.Bounds.Min.Y, 1e-14);
        Assert.Equal(-1, tri.Bounds.Min.Z, 1e-14);
        Assert.Equal(7, tri.Bounds.Max.X, 1e-14);
        Assert.Equal(8, tri.Bounds.Max.Y, 1e-14);
        Assert.Equal(5, tri.Bounds.Max.Z, 1e-14);
    }

    [Fact]
    public void Bounds_ContainsCentroid()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        Assert.True(tri.Bounds.Contains(tri.Centroid));
    }

    [Fact]
    public void Indexer_Returns_CorrectVertices()
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
        var tri = new Triangle3(Vec3.Zero, Vec3.Zero, Vec3.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[3]);
    }

    [Fact]
    public void Plane_ContainsAllVertices()
    {
        var tri = new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1));
        var plane = tri.Plane;
        Assert.Equal(0, plane.SignedDistanceTo(tri.A), 1e-14);
        Assert.Equal(0, plane.SignedDistanceTo(tri.B), 1e-14);
        Assert.Equal(0, plane.SignedDistanceTo(tri.C), 1e-14);
    }

    [Fact]
    public void Plane_NormalAligned()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var triNorm = tri.UnitNormal;
        var planeNorm = tri.Plane.Normal;
        // Should be parallel (dot product ~1 or ~-1)
        Assert.True(System.Math.Abs(Vec3.Dot(triNorm, planeNorm)) > 0.99);
    }

    [Fact]
    public void RecordEquality_SameVertices()
    {
        var a = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var b = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void RecordEquality_DifferentVertices()
    {
        var a = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var b = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 10));
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Area_3DTriangle_NotZero()
    {
        // Triangle not in any axis plane
        var tri = new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1));
        // Area = 0.5 * |cross| = 0.5 * sqrt(3) ≈ 0.866
        Assert.Equal(System.Math.Sqrt(3) / 2.0, tri.Area, 1e-10);
    }

    [Fact]
    public void Normal_ReverseWinding_FlipsDirection()
    {
        var tri1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var tri2 = new Triangle3(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(1, 0, 0));
        var n1 = tri1.Normal;
        var n2 = tri2.Normal;
        // Opposite directions
        Assert.Equal(0, n1.X + n2.X, 1e-14);
        Assert.Equal(0, n1.Y + n2.Y, 1e-14);
        Assert.Equal(0, n1.Z + n2.Z, 1e-14);
    }

    [Fact]
    public void LargeCoordinates_AreaNotNaN()
    {
        var tri = new Triangle3(new Vec3(1e6, 0, 0), new Vec3(0, 1e6, 0), new Vec3(0, 0, 1e6));
        Assert.False(double.IsNaN(tri.Area));
        Assert.True(tri.Area > 0);
    }

    [Fact]
    public void Area_ScalesQuadratically()
    {
        var tri1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var tri2 = new Triangle3(Vec3.Zero, new Vec3(3, 0, 0), new Vec3(0, 3, 0));
        Assert.Equal(tri1.Area * 9.0, tri2.Area, 1e-14);
    }
}
