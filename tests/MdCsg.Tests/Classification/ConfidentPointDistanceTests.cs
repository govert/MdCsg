using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint.PointTriangleDistanceSq — Voronoi region point-triangle distance</summary>
public class ConfidentPointDistanceTests
{
    [Fact]
    public void PointOnVertex_A_DistanceZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(a, a, b, c);
        Assert.Equal(0, d, 1e-14);
    }

    [Fact]
    public void PointOnVertex_B_DistanceZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(b, a, b, c);
        Assert.Equal(0, d, 1e-14);
    }

    [Fact]
    public void PointOnVertex_C_DistanceZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(c, a, b, c);
        Assert.Equal(0, d, 1e-14);
    }

    [Fact]
    public void PointOnEdge_AB_DistanceZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.5, 0, 0); // midpoint of AB
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(0, d, 1e-14);
    }

    [Fact]
    public void PointOnEdge_BC_DistanceZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.5, 0.5, 0); // midpoint of BC
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(0, d, 1e-14);
    }

    [Fact]
    public void PointOnEdge_CA_DistanceZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0, 0.5, 0); // midpoint of CA
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(0, d, 1e-14);
    }

    [Fact]
    public void PointInsideTriangle_DistanceZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var centroid = (a + b + c) / 3.0;
        double d = ConfidentPoint.PointTriangleDistanceSq(centroid, a, b, c);
        Assert.Equal(0, d, 1e-14);
    }

    [Fact]
    public void PointAboveTriangle_DistanceIsHeight()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var centroid = (a + b + c) / 3.0;
        var p = centroid + new Vec3(0, 0, 5);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(25, d, 1e-10);
    }

    [Fact]
    public void PointBelowTriangle_DistanceIsHeight()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var centroid = (a + b + c) / 3.0;
        var p = centroid + new Vec3(0, 0, -3);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(9, d, 1e-10);
    }

    [Fact]
    public void PointNearestToEdge_AB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, -1, 0); // projects onto AB edge
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(1, d, 1e-10); // distance 1 to AB
    }

    [Fact]
    public void PointNearestToVertex_A_BeyondCorner()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, -1, 0); // nearest to vertex A
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(2, d, 1e-10); // distance^2 = 1+1=2
    }

    [Fact]
    public void PointNearestToVertex_B_BeyondCorner()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(2, -1, 0); // nearest to vertex B
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(2, d, 1e-10);
    }

    [Fact]
    public void PointNearestToVertex_C_BeyondCorner()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, 2, 0); // nearest to vertex C
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(2, d, 1e-10);
    }

    [Fact]
    public void Distance_IsAlwaysNonNegative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var rng = new System.Random(42);
        for (int i = 0; i < 50; i++)
        {
            var p = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
            double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(d >= 0, $"Distance squared {d} should be >= 0");
        }
    }

    [Fact]
    public void PointOnFace_DistanceZero_ForRandomInteriorPoints()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var rng = new System.Random(123);
        for (int i = 0; i < 20; i++)
        {
            double u = rng.NextDouble();
            double v = rng.NextDouble();
            if (u + v > 1) { u = 1 - u; v = 1 - v; }
            var p = a + (b - a) * u + (c - a) * v;
            double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(d < 1e-20, $"Point on face should have zero distance, got {d}");
        }
    }

    [Fact]
    public void Symmetry_PermutingTriangleVertices()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.3, 0.3, 1);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p, b, c, a);
        double d3 = ConfidentPoint.PointTriangleDistanceSq(p, c, a, b);
        Assert.Equal(d1, d2, 1e-10);
        Assert.Equal(d1, d3, 1e-10);
    }

    [Fact]
    public void LargeTriangle_PointFarAway()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(100, 0, 0);
        var c = new Vec3(0, 100, 0);
        var p = new Vec3(50, 50, 1000);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Point is above hypotenuse midpoint, nearest point is on edge BC
        // The edge BC goes from (100,0,0) to (0,100,0), midpoint (50,50,0)
        // Point projects to (50,50,0), distance = 1000
        Assert.Equal(1e6, d, 1e-4);
    }
}
