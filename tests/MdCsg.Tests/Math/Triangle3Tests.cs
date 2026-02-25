using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Triangle3 tests — Normal, UnitNormal, Centroid, Area, Bounds, indexer, Plane</summary>
public class Triangle3Tests
{
    [Fact]
    public void Normal_XYPlane()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        // Cross((1,0,0), (0,1,0)) = (0,0,1)
        Assert.True(System.Math.Abs(tri.Normal.Z - 1) < 1e-12);
        Assert.True(System.Math.Abs(tri.Normal.X) < 1e-12);
        Assert.True(System.Math.Abs(tri.Normal.Y) < 1e-12);
    }

    [Fact]
    public void UnitNormal_IsUnit()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        var n = tri.UnitNormal;
        Assert.True(System.Math.Abs(n.Length - 1.0) < 1e-12);
    }

    [Fact]
    public void Centroid_IsAverage()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 3, 0));
        Assert.Equal(new Vec3(1, 1, 0), tri.Centroid);
    }

    [Fact]
    public void Area_RightTriangle()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 3, 0));
        Assert.Equal(3.0, tri.Area, 12); // 0.5 * 2 * 3 = 3
    }

    [Fact]
    public void DoubleArea_IsTwiceArea()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 3, 0));
        Assert.Equal(tri.Area * 2, tri.DoubleArea, 12);
    }

    [Fact]
    public void Area_Equilateral()
    {
        double h = System.Math.Sqrt(3) / 2;
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0.5, h, 0));
        Assert.True(System.Math.Abs(tri.Area - h / 2) < 1e-12);
    }

    [Fact]
    public void Bounds_ContainsAllVertices()
    {
        var a = new Vec3(1, -2, 3);
        var b = new Vec3(-4, 5, 0);
        var c = new Vec3(2, 1, -1);
        var tri = new Triangle3(a, b, c);
        Assert.True(tri.Bounds.Contains(a));
        Assert.True(tri.Bounds.Contains(b));
        Assert.True(tri.Bounds.Contains(c));
    }

    [Fact]
    public void Indexer_ReturnsCorrectVertex()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 2, 0);
        var c = new Vec3(0, 0, 3);
        var tri = new Triangle3(a, b, c);
        Assert.Equal(a, tri[0]);
        Assert.Equal(b, tri[1]);
        Assert.Equal(c, tri[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.Zero, Vec3.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[-1]);
    }

    [Fact]
    public void Plane_PointsOnTriangleAreOnPlane()
    {
        var tri = new Triangle3(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        var plane = tri.Plane;
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.A)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.B)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.C)) < 1e-10);
    }

    [Fact]
    public void Normal_ReversedWinding_FlipsNormal()
    {
        var tri1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var tri2 = new Triangle3(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(1, 0, 0));
        Assert.True(Vec3.Distance(tri1.Normal, -tri2.Normal) < 1e-12);
    }

    [Fact]
    public void DegenerateTriangle_ZeroArea()
    {
        // Collinear points
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(2, 0, 0));
        Assert.True(tri.Area < 1e-15);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var b = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        Assert.Equal(a, b);
    }

    [Fact]
    public void LargeTriangle_AreaCorrect()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1000, 0, 0), new Vec3(0, 1000, 0));
        Assert.Equal(500000.0, tri.Area, 6);
    }

    [Fact]
    public void SmallTriangle_AreaCorrect()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1e-6, 0, 0), new Vec3(0, 1e-6, 0));
        Assert.True(System.Math.Abs(tri.Area - 0.5e-12) < 1e-18);
    }
}
