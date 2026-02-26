using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

/// <summary>
/// Parameterized SDF tests across geometries, positions, and query modes.
/// </summary>
public class SignedDistanceFieldParameterizedTests
{
    // =========================================================================
    // SDF sign: inside points negative, outside positive
    // =========================================================================

    public static IEnumerable<object[]> InsidePointsOfUnitCube => new[]
    {
        new object[] { 0.5, 0.5, 0.5 },
        new object[] { 0.1, 0.1, 0.1 },
        new object[] { 0.9, 0.9, 0.9 },
        new object[] { 0.5, 0.1, 0.9 },
        new object[] { 0.25, 0.75, 0.5 },
    };

    public static IEnumerable<object[]> OutsidePointsOfUnitCube => new[]
    {
        new object[] { 2.0, 0.5, 0.5 },
        new object[] { -1.0, 0.5, 0.5 },
        new object[] { 0.5, -1.0, 0.5 },
        new object[] { 0.5, 0.5, 2.0 },
        new object[] { 2.0, 2.0, 2.0 },
        new object[] { -1.0, -1.0, -1.0 },
        new object[] { 5.0, 0.5, 0.5 },
        new object[] { 0.5, 5.0, 0.5 },
    };

    [Theory]
    [MemberData(nameof(InsidePointsOfUnitCube))]
    public void UnitCube_InsidePoints_NegativeSdf(double x, double y, double z)
    {
        var sdf = new SignedDistanceField(Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0));
        double d = sdf.SignedDistance(new Vec3(x, y, z));
        Assert.True(d < 0, $"Inside point ({x},{y},{z}): SDF should be negative, got {d}");
    }

    [Theory]
    [MemberData(nameof(OutsidePointsOfUnitCube))]
    public void UnitCube_OutsidePoints_PositiveSdf(double x, double y, double z)
    {
        var sdf = new SignedDistanceField(Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0));
        double d = sdf.SignedDistance(new Vec3(x, y, z));
        Assert.True(d > 0, $"Outside point ({x},{y},{z}): SDF should be positive, got {d}");
    }

    // =========================================================================
    // Sphere SDF: compare with analytical r - R
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1.0)]   // center
    [InlineData(0.5, 0, 0, 1.0)]
    [InlineData(0, 0.5, 0, 1.0)]
    [InlineData(2.0, 0, 0, 1.0)]
    [InlineData(0, 0, 3.0, 1.0)]
    [InlineData(0, 0, 0, 0.5)]
    [InlineData(0, 0, 0, 5.0)]
    [InlineData(3, 0, 0, 5.0)]   // inside large sphere
    [InlineData(10, 0, 0, 5.0)]  // outside large sphere
    public void Sphere_SdfSignMatchesAnalytical(double px, double py, double pz, double radius)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, radius, 2);
        var sdf = new SignedDistanceField(sphere);
        var point = new Vec3(px, py, pz);

        double d = sdf.SignedDistance(point);
        double analytical = point.Length - radius;

        // Sign should match
        if (analytical < -0.1)
            Assert.True(d < 0, $"Point at r={point.Length}, R={radius}: expected negative, got {d}");
        else if (analytical > 0.1)
            Assert.True(d > 0, $"Point at r={point.Length}, R={radius}: expected positive, got {d}");
    }

    // =========================================================================
    // Batch queries match individual queries
    // =========================================================================

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Batch_MatchesIndividual_VariousCounts(int count)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);

        var rng = new Random(42);
        var points = new Vec3[count];
        for (int i = 0; i < count; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);

        var batch = new double[count];
        sdf.SignedDistances(points, batch);

        for (int i = 0; i < count; i++)
        {
            double individual = sdf.SignedDistance(points[i]);
            Assert.True(System.Math.Abs(batch[i] - individual) < 1e-10,
                $"Point {i}: batch={batch[i]}, individual={individual}");
        }
    }

    // =========================================================================
    // SDF magnitude: distance from surface
    // =========================================================================

    [Theory]
    [InlineData(1.5, 0.5)]  // 0.5 away from +X face
    [InlineData(2.0, 1.0)]  // 1.0 away
    [InlineData(3.0, 2.0)]  // 2.0 away
    [InlineData(-0.5, 0.5)] // 0.5 away from -X face
    public void UnitCube_OutsideOnXAxis_DistanceApproximatelyCorrect(double px, double expectedDist)
    {
        var sdf = new SignedDistanceField(Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0));
        double d = sdf.SignedDistance(new Vec3(px, 0.5, 0.5));
        Assert.True(System.Math.Abs(d - expectedDist) < 0.05,
            $"At x={px}: expected dist ~{expectedDist}, got {d}");
    }

    // =========================================================================
    // Grid sampling: inside count approximates volume
    // =========================================================================

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.0)]
    public void GridSample_Sphere_InsideCountApproximatesVolume(double radius)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, radius, 2);
        var sdf = new SignedDistanceField(sphere);

        int res = 20;
        double step = radius * 3 / res;
        int insideCount = 0;
        int totalCount = 0;

        for (int ix = 0; ix < res; ix++)
        for (int iy = 0; iy < res; iy++)
        for (int iz = 0; iz < res; iz++)
        {
            var p = new Vec3(
                (ix + 0.5) * step - radius * 1.5,
                (iy + 0.5) * step - radius * 1.5,
                (iz + 0.5) * step - radius * 1.5);
            totalCount++;
            if (sdf.SignedDistance(p) < 0) insideCount++;
        }

        double cellVol = step * step * step;
        double approxVol = insideCount * cellVol;
        double exactVol = (4.0 / 3.0) * System.Math.PI * radius * radius * radius;

        Assert.True(System.Math.Abs(approxVol - exactVol) / exactVol < 0.15,
            $"R={radius}: approx volume {approxVol}, exact {exactVol}");
    }
}
