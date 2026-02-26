using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class GeometryUtilTests
{
    private const double Tol = 1e-10;

    // ── NearestPointOnTriangle ──────────────────────────────────────

    [Fact]
    public void NearestPoint_AboveInterior_ReturnsProjection()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.2, 0.2, 5);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(System.Math.Abs(nearest.X - 0.2) < Tol);
        Assert.True(System.Math.Abs(nearest.Y - 0.2) < Tol);
        Assert.True(System.Math.Abs(nearest.Z) < Tol);
    }

    [Fact]
    public void NearestPoint_NearVertexA_ReturnsA()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, -1, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(Vec3.DistanceSquared(nearest, a) < Tol * Tol);
    }

    [Fact]
    public void NearestPoint_NearVertexB_ReturnsB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(2, -1, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(Vec3.DistanceSquared(nearest, b) < Tol * Tol);
    }

    [Fact]
    public void NearestPoint_NearVertexC_ReturnsC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, 2, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(Vec3.DistanceSquared(nearest, c) < Tol * Tol);
    }

    [Fact]
    public void NearestPoint_NearEdgeAB_ReturnsProjectionOnEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.5, -1, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(System.Math.Abs(nearest.Y) < Tol);
        Assert.True(nearest.X > -Tol && nearest.X < 1 + Tol);
    }

    [Fact]
    public void NearestPoint_NearEdgeAC_ReturnsProjectionOnEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, 0.5, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(System.Math.Abs(nearest.X) < Tol);
        Assert.True(nearest.Y > -Tol && nearest.Y < 1 + Tol);
    }

    [Fact]
    public void NearestPoint_NearEdgeBC_ReturnsProjectionOnEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(1, 1, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        // Should be on the edge b->c: x + y = 1
        Assert.True(System.Math.Abs(nearest.X + nearest.Y - 1) < Tol);
    }

    [Fact]
    public void NearestPoint_PointOnTriangle_ReturnsSamePoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.25, 0.25, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(Vec3.DistanceSquared(nearest, p) < Tol * Tol);
    }

    [Fact]
    public void NearestPoint_ConsistentWithDistanceSq()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var points = new[]
        {
            new Vec3(0.3, 0.2, 1), new Vec3(-1, -1, 2), new Vec3(2, 0, 0),
            new Vec3(0.5, 0.5, -3), new Vec3(1, 1, 1)
        };

        foreach (var p in points)
        {
            var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
            double distSq = Vec3.DistanceSquared(p, nearest);
            double refDistSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(System.Math.Abs(distSq - refDistSq) < 1e-8,
                $"Distance mismatch for {p}: {distSq} vs {refDistSq}");
        }
    }

    // ── Circumcircle ────────────────────────────────────────────────

    [Fact]
    public void Circumcircle_EquilateralTriangle_CenterAtCentroid()
    {
        double h = System.Math.Sqrt(3) / 2;
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0.5, h, 0);
        var (center, radius) = GeometryUtil.Circumcircle(a, b, c);

        // For equilateral triangle, circumradius = side / sqrt(3)
        double expectedRadius = 1.0 / System.Math.Sqrt(3);
        Assert.True(System.Math.Abs(radius - expectedRadius) < 1e-10,
            $"Expected radius {expectedRadius}, got {radius}");

        // Center equidistant from all vertices
        Assert.True(System.Math.Abs(Vec3.Distance(center, a) - radius) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Distance(center, b) - radius) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Distance(center, c) - radius) < 1e-10);
    }

    [Fact]
    public void Circumcircle_RightTriangle_CenterAtHypotenuseMidpoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(0, 3, 0);
        var (center, radius) = GeometryUtil.Circumcircle(a, b, c);

        var hypMid = (b + c) * 0.5;
        Assert.True(Vec3.Distance(center, hypMid) < 1e-10,
            $"Center {center} should be at {hypMid}");
        Assert.True(System.Math.Abs(radius - 2.5) < 1e-10,
            $"Radius should be 2.5, got {radius}");
    }

    [Fact]
    public void Circumcircle_3DTriangle_EquidistantFromVertices()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        var (center, radius) = GeometryUtil.Circumcircle(a, b, c);

        Assert.True(System.Math.Abs(Vec3.Distance(center, a) - radius) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Distance(center, b) - radius) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Distance(center, c) - radius) < 1e-10);
    }

    [Fact]
    public void Circumcircle_IsoscelesTriangle_Symmetric()
    {
        var a = new Vec3(-1, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 2, 0);
        var (center, _) = GeometryUtil.Circumcircle(a, b, c);
        // Symmetry: center should be on x=0
        Assert.True(System.Math.Abs(center.X) < 1e-10);
    }

    // ── Circumsphere ────────────────────────────────────────────────

    [Fact]
    public void Circumsphere_RegularTetrahedron_KnownRadius()
    {
        // Regular tetrahedron with edge length 2
        var a = new Vec3(1, 1, 1);
        var b = new Vec3(1, -1, -1);
        var c = new Vec3(-1, 1, -1);
        var d = new Vec3(-1, -1, 1);
        var (center, radius) = GeometryUtil.Circumsphere(a, b, c, d);

        // Center should be at origin
        Assert.True(Vec3.Distance(center, Vec3.Zero) < 1e-10,
            $"Center should be at origin, got {center}");
        // Radius should be sqrt(3) ≈ 1.732
        Assert.True(System.Math.Abs(radius - System.Math.Sqrt(3)) < 1e-10,
            $"Expected radius sqrt(3), got {radius}");
    }

    [Fact]
    public void Circumsphere_AllVerticesEquidistant()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 4, 0);
        var d = new Vec3(0, 0, 5);
        var (center, radius) = GeometryUtil.Circumsphere(a, b, c, d);

        Assert.True(System.Math.Abs(Vec3.Distance(center, a) - radius) < 1e-8);
        Assert.True(System.Math.Abs(Vec3.Distance(center, b) - radius) < 1e-8);
        Assert.True(System.Math.Abs(Vec3.Distance(center, c) - radius) < 1e-8);
        Assert.True(System.Math.Abs(Vec3.Distance(center, d) - radius) < 1e-8);
    }

    [Fact]
    public void Circumsphere_UnitCubeCorners_KnownResult()
    {
        // Use 4 corners of a unit cube that form a tetrahedron
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 1, 0);
        var c = new Vec3(1, 0, 1);
        var d = new Vec3(0, 1, 1);
        var (center, radius) = GeometryUtil.Circumsphere(a, b, c, d);

        // This regular tetrahedron inscribed in the unit cube has circumcenter at (0.5, 0.5, 0.5)
        Assert.True(Vec3.Distance(center, new Vec3(0.5, 0.5, 0.5)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Distance(center, a) - radius) < 1e-10);
    }
}
