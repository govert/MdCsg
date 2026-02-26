using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>
/// Comprehensive Plane operations tests: construction, classification, geometry.
/// </summary>
public class PlaneOperationsTests
{
    // =========================================================================
    // FromPoints: many triangle orientations
    // =========================================================================

    public static IEnumerable<object[]> TriangleOrientations()
    {
        // Triangles in various planes
        var triangles = new (Vec3 a, Vec3 b, Vec3 c)[]
        {
            (new(0,0,0), new(1,0,0), new(0,1,0)),  // XY
            (new(0,0,0), new(1,0,0), new(0,0,1)),  // XZ
            (new(0,0,0), new(0,1,0), new(0,0,1)),  // YZ
            (new(1,1,0), new(2,1,0), new(1,2,0)),  // XY offset
            (new(0,0,5), new(1,0,5), new(0,1,5)),  // XY at z=5
            (new(1,0,0), new(1,1,0), new(1,0,1)),  // YZ at x=1
            (new(0,2,0), new(1,2,0), new(0,2,1)),  // XZ at y=2
        };
        foreach (var (a, b, c) in triangles)
            yield return new object[] { a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z };
    }

    [Theory]
    [MemberData(nameof(TriangleOrientations))]
    public void FromPoints_AllVerticesOnPlane(double ax, double ay, double az,
                                              double bx, double by, double bz,
                                              double cx, double cy, double cz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var c = new Vec3(cx, cy, cz);
        var plane = Plane.FromPoints(a, b, c);

        Assert.True(System.Math.Abs(plane.SignedDistanceTo(a)) < 1e-10, $"A not on plane");
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(b)) < 1e-10, $"B not on plane");
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(c)) < 1e-10, $"C not on plane");
    }

    // =========================================================================
    // FromPointAndNormal: various combinations
    // =========================================================================

    public static IEnumerable<object[]> PointNormalPairs()
    {
        var points = new Vec3[]
        {
            Vec3.Zero, new(1,0,0), new(0,1,0), new(0,0,1),
            new(5,5,5), new(-3,2,-1), new(100,0,0),
        };
        var normals = new Vec3[]
        {
            Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ,
            new(1,1,0), new(1,0,1), new(0,1,1), new(1,1,1),
            new(2,-1,3), new(-3,2,-1),
        };
        foreach (var p in points)
        foreach (var n in normals)
            yield return new object[] { p.X, p.Y, p.Z, n.X, n.Y, n.Z };
    }

    [Theory]
    [MemberData(nameof(PointNormalPairs))]
    public void FromPointAndNormal_PointHasZeroDistance(double px, double py, double pz,
                                                        double nx, double ny, double nz)
    {
        var point = new Vec3(px, py, pz);
        var normal = new Vec3(nx, ny, nz);
        var plane = Plane.FromPointAndNormal(point, normal);
        double sd = plane.SignedDistanceTo(point);
        Assert.True(System.Math.Abs(sd) < 1e-10, $"sd={sd}");
    }

    // =========================================================================
    // SignedDistance: positive on normal side
    // =========================================================================

    [Theory]
    [MemberData(nameof(PointNormalPairs))]
    public void SignedDistance_PointAlongNormal_Positive(double px, double py, double pz,
                                                         double nx, double ny, double nz)
    {
        var point = new Vec3(px, py, pz);
        var normal = new Vec3(nx, ny, nz).Normalized;
        var plane = Plane.FromPointAndNormal(point, normal);
        var testPoint = point + normal * 5.0;
        double sd = plane.SignedDistanceTo(testPoint);
        Assert.True(sd > 0, $"Point along normal should have positive sd, got {sd}");
    }

    // =========================================================================
    // SignedDistance: negative on opposite side
    // =========================================================================

    [Theory]
    [MemberData(nameof(PointNormalPairs))]
    public void SignedDistance_PointOppositeNormal_Negative(double px, double py, double pz,
                                                            double nx, double ny, double nz)
    {
        var point = new Vec3(px, py, pz);
        var normal = new Vec3(nx, ny, nz).Normalized;
        var plane = Plane.FromPointAndNormal(point, normal);
        var testPoint = point - normal * 5.0;
        double sd = plane.SignedDistanceTo(testPoint);
        Assert.True(sd < 0, $"Point opposite normal should have negative sd, got {sd}");
    }

    // =========================================================================
    // Flipped plane: reversed signed distance
    // =========================================================================

    [Theory]
    [MemberData(nameof(PointNormalPairs))]
    public void Flipped_ReversesSignedDistance(double px, double py, double pz,
                                               double nx, double ny, double nz)
    {
        var normal = new Vec3(nx, ny, nz).Normalized;
        var plane = Plane.FromPointAndNormal(new Vec3(px, py, pz), normal);
        var flipped = plane.Flipped;
        var testPoint = new Vec3(px + 1, py + 2, pz + 3);
        double sd = plane.SignedDistanceTo(testPoint);
        double fsd = flipped.SignedDistanceTo(testPoint);
        Assert.True(System.Math.Abs(sd + fsd) < 1e-10, $"sd={sd}, fsd={fsd}");
    }

    // =========================================================================
    // FromPoints: normal has unit length
    // =========================================================================

    [Theory]
    [MemberData(nameof(TriangleOrientations))]
    public void FromPoints_NormalIsUnitLength(double ax, double ay, double az,
                                              double bx, double by, double bz,
                                              double cx, double cy, double cz)
    {
        var plane = Plane.FromPoints(new Vec3(ax, ay, az), new Vec3(bx, by, bz), new Vec3(cx, cy, cz));
        Assert.True(System.Math.Abs(plane.Normal.Length - 1.0) < 1e-10);
    }

    // =========================================================================
    // Signed distance: linear in displacement
    // =========================================================================

    [Theory]
    [InlineData(1, 2, 3, 4, 5, 6)]
    [InlineData(0, 0, 1, 0, 0, 0)]
    [InlineData(1, 0, 0, 5, 0, 0)]
    public void SignedDistance_LinearInDisplacement(double nx, double ny, double nz,
                                                    double px, double py, double pz)
    {
        var normal = new Vec3(nx, ny, nz).Normalized;
        var plane = Plane.FromPointAndNormal(Vec3.Zero, normal);
        var point = new Vec3(px, py, pz);
        double sd1 = plane.SignedDistanceTo(point);
        double sd2 = plane.SignedDistanceTo(point * 2);
        Assert.True(System.Math.Abs(sd2 - 2 * sd1) < 1e-8, $"sd1={sd1}, sd2={sd2}");
    }
}
