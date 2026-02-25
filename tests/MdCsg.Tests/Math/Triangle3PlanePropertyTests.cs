using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Triangle3 — Normal, UnitNormal, Plane, Centroid, Bounds, Area, indexer; Plane interaction</summary>
public class Triangle3PlanePropertyTests
{
    [Fact]
    public void Triangle3_Normal_CrossProduct()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var expected = Vec3.Cross(new Vec3(1,0,0), new Vec3(0,1,0));
        Assert.Equal(expected, tri.Normal);
    }

    [Fact]
    public void Triangle3_UnitNormal_IsUnit()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(3,0,0), new Vec3(0,4,0));
        Assert.True(System.Math.Abs(tri.UnitNormal.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Triangle3_Centroid_IsAverage()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(3,0,0), new Vec3(0,6,0));
        Assert.Equal(1.0, tri.Centroid.X, 10);
        Assert.Equal(2.0, tri.Centroid.Y, 10);
        Assert.Equal(0.0, tri.Centroid.Z, 10);
    }

    [Fact]
    public void Triangle3_Area_RightTriangle()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(4,0,0), new Vec3(0,3,0));
        Assert.Equal(6.0, tri.Area, 10);
    }

    [Fact]
    public void Triangle3_DoubleArea_IsNormalLength()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        Assert.Equal(tri.Normal.Length, tri.DoubleArea, 10);
    }

    [Fact]
    public void Triangle3_Area_IsHalfDoubleArea()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(2,0,0), new Vec3(0,3,0));
        Assert.Equal(tri.DoubleArea * 0.5, tri.Area, 10);
    }

    [Fact]
    public void Triangle3_Bounds_EnclosesVertices()
    {
        var tri = new Triangle3(new Vec3(1,5,3), new Vec3(4,2,7), new Vec3(6,8,1));
        Assert.True(tri.Bounds.Contains(tri.A));
        Assert.True(tri.Bounds.Contains(tri.B));
        Assert.True(tri.Bounds.Contains(tri.C));
    }

    [Fact]
    public void Triangle3_Indexer_Returns_ABC()
    {
        var a = new Vec3(1,2,3);
        var b = new Vec3(4,5,6);
        var c = new Vec3(7,8,9);
        var tri = new Triangle3(a, b, c);
        Assert.Equal(a, tri[0]);
        Assert.Equal(b, tri[1]);
        Assert.Equal(c, tri[2]);
    }

    [Fact]
    public void Triangle3_Plane_ContainsAllVertices()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var plane = tri.Plane;
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.A)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.B)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.C)) < 1e-10);
    }

    [Fact]
    public void Triangle3_Plane_NormalPointsCorrectDirection()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        // CCW in XY plane → normal should point +Z
        Assert.True(tri.Plane.Normal.Z > 0);
    }

    [Fact]
    public void Triangle3_DegenerateTriangle_ZeroArea()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(2,0,0));
        Assert.True(tri.Area < 1e-20);
    }

    [Fact]
    public void Triangle3_LargeTriangle_CorrectArea()
    {
        var tri = new Triangle3(new Vec3(0,0,0), new Vec3(1000,0,0), new Vec3(0,1000,0));
        Assert.Equal(500000.0, tri.Area, 6);
    }

    [Fact]
    public void Triangle3_3DTriangle_CorrectArea()
    {
        // Equilateral triangle with side 2 in 3D
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(-1, 0, 0);
        var c = new Vec3(0, System.Math.Sqrt(3), 0);
        var tri = new Triangle3(a, b, c);
        // Area of equilateral triangle with side 2: sqrt(3)
        Assert.True(System.Math.Abs(tri.Area - System.Math.Sqrt(3)) < 1e-10);
    }

    [Fact]
    public void Plane_FromPoints_VerticesOnPlane()
    {
        var plane = Plane.FromPoints(new Vec3(1,0,0), new Vec3(0,1,0), new Vec3(0,0,1));
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(new Vec3(1,0,0))) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(new Vec3(0,1,0))) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(new Vec3(0,0,1))) < 1e-10);
    }

    [Fact]
    public void Plane_ToString_ContainsInfo()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        string s = plane.ToString();
        Assert.Contains("Plane", s);
    }
}
