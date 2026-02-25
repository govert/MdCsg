using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Triangle3 — Area, Centroid, Normal, degenerate, large/small</summary>
public class Triangle3PropertyTests
{
    [Fact]
    public void UnitRightTriangle_Area_0_5()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.5, t.Area, 1e-10);
    }

    [Fact]
    public void ScaledTriangle_Area_Scales()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        Assert.Equal(2.0, t.Area, 1e-10);
    }

    [Fact]
    public void DegenerateTriangle_ZeroArea()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(2, 0, 0));
        Assert.Equal(0, t.Area, 1e-10);
    }

    [Fact]
    public void Triangle_Area_NonNegative()
    {
        var t = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 10));
        Assert.True(t.Area >= 0);
    }

    [Fact]
    public void Centroid_IsAverageOfVertices()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var t = new Triangle3(a, b, c);
        Assert.Equal(1.0, t.Centroid.X, 1e-10);
        Assert.Equal(1.0, t.Centroid.Y, 1e-10);
        Assert.Equal(0, t.Centroid.Z, 1e-10);
    }

    [Fact]
    public void Centroid_Arbitrary()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var t = new Triangle3(a, b, c);
        Assert.Equal(4.0, t.Centroid.X, 1e-10);
        Assert.Equal(5.0, t.Centroid.Y, 1e-10);
        Assert.Equal(6.0, t.Centroid.Z, 1e-10);
    }

    [Fact]
    public void Normal_XYPlane_PointsZ()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var n = t.Normal;
        Assert.Equal(0, n.X, 1e-10);
        Assert.Equal(0, n.Y, 1e-10);
        Assert.True(n.Z > 0); // CCW winding → positive Z
    }

    [Fact]
    public void Normal_ReversedWinding_OppositeDirection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 0, 0));
        var n1 = t1.Normal;
        var n2 = t2.Normal;
        Assert.Equal(-n1.X, n2.X, 1e-10);
        Assert.Equal(-n1.Y, n2.Y, 1e-10);
        Assert.Equal(-n1.Z, n2.Z, 1e-10);
    }

    [Fact]
    public void Normal_PerpendicularToEdges()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var t = new Triangle3(a, b, c);
        var n = t.Normal;
        Assert.Equal(0, Vec3.Dot(n, b - a), 1e-10);
        Assert.Equal(0, Vec3.Dot(n, c - a), 1e-10);
    }

    [Fact]
    public void Vertices_Stored_Correctly()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var t = new Triangle3(a, b, c);
        Assert.Equal(a, t.A);
        Assert.Equal(b, t.B);
        Assert.Equal(c, t.C);
    }

    [Fact]
    public void RecordEquality()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(t1, t2);
    }

    [Fact]
    public void RecordInequality()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 2, 0));
        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void SmallTriangle_HasArea()
    {
        var t = new Triangle3(Vec3.Zero, new Vec3(1e-6, 0, 0), new Vec3(0, 1e-6, 0));
        Assert.True(t.Area > 0);
        Assert.Equal(0.5e-12, t.Area, 1e-18);
    }

    [Fact]
    public void LargeTriangle_HasArea()
    {
        var t = new Triangle3(Vec3.Zero, new Vec3(1000, 0, 0), new Vec3(0, 1000, 0));
        Assert.Equal(500000, t.Area, 1e-4);
    }

    [Fact]
    public void TiltedTriangle_AreaCorrect()
    {
        // Equilateral triangle in 3D
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 0, System.Math.Sqrt(3) / 2));
        Assert.Equal(System.Math.Sqrt(3) / 4, t.Area, 1e-10);
    }
}
