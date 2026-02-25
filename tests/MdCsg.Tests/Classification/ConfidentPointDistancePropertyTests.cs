using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint.PointTriangleDistanceSq — Voronoi region tests, distance properties</summary>
public class ConfidentPointDistancePropertyTests
{
    [Fact]
    public void PointOnVertex_DistanceZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(0.0, ConfidentPoint.PointTriangleDistanceSq(a, a, b, c), 15);
    }

    [Fact]
    public void PointOnVertexB_DistanceZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(0.0, ConfidentPoint.PointTriangleDistanceSq(b, a, b, c), 15);
    }

    [Fact]
    public void PointOnVertexC_DistanceZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(0.0, ConfidentPoint.PointTriangleDistanceSq(c, a, b, c), 15);
    }

    [Fact]
    public void PointOnEdgeAB_DistanceZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, 0, 0); // midpoint of AB
        Assert.True(ConfidentPoint.PointTriangleDistanceSq(p, a, b, c) < 1e-20);
    }

    [Fact]
    public void PointOnEdgeAC_DistanceZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(0, 1, 0); // midpoint of AC
        Assert.True(ConfidentPoint.PointTriangleDistanceSq(p, a, b, c) < 1e-20);
    }

    [Fact]
    public void PointOnEdgeBC_DistanceZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, 1, 0); // midpoint of BC
        Assert.True(ConfidentPoint.PointTriangleDistanceSq(p, a, b, c) < 1e-20);
    }

    [Fact]
    public void PointInsideTriangle_DistanceZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var p = new Vec3(1, 1, 0); // centroid area
        Assert.True(ConfidentPoint.PointTriangleDistanceSq(p, a, b, c) < 1e-20);
    }

    [Fact]
    public void PointAboveTriangle_DistanceIsHeight()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(0.5, 0.5, 3); // above interior of triangle
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(9.0, distSq, 10); // height = 3, distSq = 9
    }

    [Fact]
    public void PointBelowTriangle_DistanceIsHeight()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(0.5, 0.5, -5);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(25.0, distSq, 10);
    }

    [Fact]
    public void PointNearVertexA_DistanceToA()
    {
        var a = Vec3.Zero;
        var b = new Vec3(10, 0, 0);
        var c = new Vec3(0, 10, 0);
        var p = new Vec3(-1, -1, 0); // nearest vertex is A
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(2.0, distSq, 10); // sqrt(2)^2 = 2
    }

    [Fact]
    public void PointNearEdgeAB_ProjectsOntoEdge()
    {
        var a = Vec3.Zero;
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(0, 4, 0);
        var p = new Vec3(2, -3, 0); // closest point on AB is (2,0,0)
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(9.0, distSq, 10); // distance = 3
    }

    [Fact]
    public void Distance_AlwaysNonNegative()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var testPoints = new[]
        {
            new Vec3(0, 0, 0), new Vec3(5, 5, 5), new Vec3(-3, -3, -3),
            new Vec3(0.3, 0.3, 0), new Vec3(100, 0, 0),
        };
        foreach (var p in testPoints)
        {
            double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(distSq >= 0, $"Distance squared must be >= 0, got {distSq} for point {p}");
        }
    }

    [Fact]
    public void SymmetricTriangle_CentroidDistance_Symmetric()
    {
        // An equilateral triangle in XY plane
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(1, System.Math.Sqrt(3), 0);
        // Point directly above centroid
        var centroid = (a + b + c) / 3.0;
        var p = new Vec3(centroid.X, centroid.Y, 5);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(25.0, distSq, 10);
    }

    [Fact]
    public void DegenerateTriangle_CollinearPoints_ReturnsFiniteDistance()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0); // collinear
        var p = new Vec3(1, 1, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point should be on the line segment, distance = 1
        Assert.Equal(1.0, distSq, 10);
    }

    [Fact]
    public void FarPoint_DistanceSq_Consistent()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(1000, 1000, 1000);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Distance must be less than distance to origin (which would be 3e6)
        Assert.True(distSq < 3e6);
        // But significantly positive
        Assert.True(distSq > 1e5);
    }

    [Fact]
    public void Translation_InvariantDistance()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(2, 2, 2);
        double dist1 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);

        var offset = new Vec3(100, 200, 300);
        double dist2 = ConfidentPoint.PointTriangleDistanceSq(
            p + offset, a + offset, b + offset, c + offset);
        Assert.Equal(dist1, dist2, 6);
    }

    [Fact]
    public void ScaledTriangle_ScaledDistance()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.5, 0.5, 1);
        double dist1 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);

        double s = 3.0;
        double dist2 = ConfidentPoint.PointTriangleDistanceSq(
            p * s, a * s, b * s, c * s);
        Assert.Equal(dist1 * s * s, dist2, 6);
    }

    [Fact]
    public void PointOnTrianglePlaneOutside_ProjectsToEdge()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        // Point outside the hypotenuse
        var p = new Vec3(2, 2, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point on BC is midpoint (1,1,0), distance = sqrt(2)
        Assert.Equal(2.0, distSq, 10);
    }

    [Fact]
    public void VertexRegion_B_Correct()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(0, 4, 0);
        var p = new Vec3(5, -1, 0); // nearest is vertex B
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expectedSq = Vec3.DistanceSquared(p, b);
        Assert.Equal(expectedSq, distSq, 10);
    }

    [Fact]
    public void VertexRegion_C_Correct()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(0, 4, 0);
        var p = new Vec3(-1, 5, 0); // nearest is vertex C
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expectedSq = Vec3.DistanceSquared(p, c);
        Assert.Equal(expectedSq, distSq, 10);
    }
}
