using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

public class PointCloudQueryTests
{
    private static Solid UnitCube() => Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);

    // =========================================================================
    // Classify against cube
    // =========================================================================

    [Fact]
    public void Classify_InsidePoint_ReturnsInside()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(0.5, 0.5, 0.5) });
        var results = query.Classify(cloud);

        Assert.Equal(SolidClassification.Inside, results[0]);
    }

    [Fact]
    public void Classify_OutsidePoint_ReturnsOutside()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(2.0, 2.0, 2.0) });
        var results = query.Classify(cloud);

        Assert.Equal(SolidClassification.Outside, results[0]);
    }

    [Fact]
    public void Classify_MultiplePoints_CorrectClassifications()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[]
        {
            new Vec3(0.5, 0.5, 0.5),   // inside
            new Vec3(2.0, 0.5, 0.5),   // outside
            new Vec3(0.2, 0.8, 0.3),   // inside
            new Vec3(-1.0, -1.0, -1.0) // outside
        });
        var results = query.Classify(cloud);

        Assert.Equal(SolidClassification.Inside, results[0]);
        Assert.Equal(SolidClassification.Outside, results[1]);
        Assert.Equal(SolidClassification.Inside, results[2]);
        Assert.Equal(SolidClassification.Outside, results[3]);
    }

    [Fact]
    public void Classify_EmptyCloud_ReturnsEmpty()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(Array.Empty<Vec3>());
        var results = query.Classify(cloud);

        Assert.Empty(results);
    }

    // =========================================================================
    // Classify against sphere
    // =========================================================================

    [Fact]
    public void Classify_Sphere_CenterIsInside()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 2.0, 2);
        var query = new PointCloudQuery(sphere);
        var cloud = new PointCloud(new[] { Vec3.Zero });
        var results = query.Classify(cloud);

        Assert.Equal(SolidClassification.Inside, results[0]);
    }

    [Fact]
    public void Classify_Sphere_FarPointIsOutside()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 2.0, 2);
        var query = new PointCloudQuery(sphere);
        var cloud = new PointCloud(new[] { new Vec3(10, 0, 0) });
        var results = query.Classify(cloud);

        Assert.Equal(SolidClassification.Outside, results[0]);
    }

    // =========================================================================
    // Classify against cylinder
    // =========================================================================

    [Fact]
    public void Classify_Cylinder_CenterIsInside()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, 0), Vec3.UnitZ, 1.0, 5.0);
        var query = new PointCloudQuery(cyl);
        var cloud = new PointCloud(new[] { new Vec3(0, 0, 2.5) });
        var results = query.Classify(cloud);

        Assert.Equal(SolidClassification.Inside, results[0]);
    }

    [Fact]
    public void Classify_Cylinder_FarPointIsOutside()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, 0), Vec3.UnitZ, 1.0, 5.0);
        var query = new PointCloudQuery(cyl);
        var cloud = new PointCloud(new[] { new Vec3(10, 0, 0) });
        var results = query.Classify(cloud);

        Assert.Equal(SolidClassification.Outside, results[0]);
    }

    // =========================================================================
    // Distances
    // =========================================================================

    [Fact]
    public void Distances_PointOneUnitAway_ReturnsApproximatelyOne()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(2.0, 0.5, 0.5) });
        var results = query.Distances(cloud);

        Assert.True(System.Math.Abs(results[0] - 1.0) < 0.01,
            $"Distance should be ~1.0, got {results[0]}");
    }

    [Fact]
    public void Distances_CenterOfCube_DistanceIsHalf()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(0.5, 0.5, 0.5) });
        var results = query.Distances(cloud);

        Assert.True(System.Math.Abs(results[0] - 0.5) < 0.01,
            $"Distance from center to nearest face should be ~0.5, got {results[0]}");
    }

    [Fact]
    public void Distances_MultipleDistances()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[]
        {
            new Vec3(0.5, 0.5, 0.5),  // center
            new Vec3(2.0, 0.5, 0.5),  // 1 unit away from +X face
            new Vec3(0.5, 0.5, -1.0), // 1 unit from -Z face
        });
        var results = query.Distances(cloud);

        Assert.True(results[0] > 0);
        Assert.True(results[1] > 0);
        Assert.True(results[2] > 0);
    }

    // =========================================================================
    // SignedDistances
    // =========================================================================

    [Fact]
    public void SignedDistances_InsidePoint_Negative()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(0.5, 0.5, 0.5) });
        var results = query.SignedDistances(cloud);

        Assert.True(results[0] < 0, $"Inside point should have negative SDF, got {results[0]}");
    }

    [Fact]
    public void SignedDistances_OutsidePoint_Positive()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(2.0, 0.5, 0.5) });
        var results = query.SignedDistances(cloud);

        Assert.True(results[0] > 0, $"Outside point should have positive SDF, got {results[0]}");
    }

    [Fact]
    public void SignedDistances_MatchesSignedDistanceField()
    {
        var cube = UnitCube();
        var query = new PointCloudQuery(cube);
        var sdf = new SignedDistanceField(cube);

        var pts = new[]
        {
            new Vec3(0.5, 0.5, 0.5),
            new Vec3(2.0, 0.5, 0.5),
            new Vec3(0.2, 0.8, 0.3),
        };
        var cloud = new PointCloud(pts);
        var results = query.SignedDistances(cloud, parallel: false);

        for (int i = 0; i < pts.Length; i++)
        {
            double expected = sdf.SignedDistance(pts[i]);
            Assert.True(System.Math.Abs(results[i] - expected) < 1e-10,
                $"Point {i}: expected SDF {expected}, got {results[i]}");
        }
    }

    [Theory]
    [InlineData(0.5, 0.5, 0.5)]   // inside
    [InlineData(2.0, 0.5, 0.5)]   // outside +X
    [InlineData(0.5, -1.0, 0.5)]  // outside -Y
    [InlineData(0.5, 0.5, 3.0)]   // outside +Z
    public void SignedDistances_SignCorrectness(double px, double py, double pz)
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(px, py, pz) });
        var cls = query.Classify(cloud);
        var sd = query.SignedDistances(cloud);

        if (cls[0] == SolidClassification.Inside)
            Assert.True(sd[0] < 0, $"Inside at ({px},{py},{pz}): SDF should be negative, got {sd[0]}");
        else
            Assert.True(sd[0] > 0, $"Outside at ({px},{py},{pz}): SDF should be positive, got {sd[0]}");
    }

    // =========================================================================
    // ClassifyWithDistances
    // =========================================================================

    [Fact]
    public void ClassifyWithDistances_MatchesSeparateCalls()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[]
        {
            new Vec3(0.5, 0.5, 0.5),
            new Vec3(2.0, 0.5, 0.5),
        });

        var cls = new SolidClassification[2];
        var dists = new double[2];
        query.ClassifyWithDistances(cloud, cls, dists, parallel: false);

        var expectedCls = query.Classify(cloud, parallel: false);
        var expectedDist = query.Distances(cloud, parallel: false);

        for (int i = 0; i < 2; i++)
        {
            Assert.Equal(expectedCls[i], cls[i]);
            Assert.True(System.Math.Abs(expectedDist[i] - dists[i]) < 1e-10);
        }
    }

    // =========================================================================
    // FilterInside / FilterOutside complementarity
    // =========================================================================

    [Fact]
    public void FilterInside_ReturnsOnlyInsidePoints()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[]
        {
            new Vec3(0.5, 0.5, 0.5),   // inside
            new Vec3(2.0, 2.0, 2.0),   // outside
            new Vec3(0.3, 0.3, 0.3),   // inside
        });

        var inside = query.FilterInside(cloud);
        Assert.Equal(2, inside.Count);
    }

    [Fact]
    public void FilterOutside_ReturnsOnlyOutsidePoints()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[]
        {
            new Vec3(0.5, 0.5, 0.5),   // inside
            new Vec3(2.0, 2.0, 2.0),   // outside
            new Vec3(0.3, 0.3, 0.3),   // inside
        });

        var outside = query.FilterOutside(cloud);
        Assert.Equal(1, outside.Count);
    }

    [Fact]
    public void FilterInside_Plus_FilterOutside_AreComplementary()
    {
        var query = new PointCloudQuery(UnitCube());
        var pts = new Vec3[50];
        var rng = new Random(123);
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1);

        var cloud = new PointCloud(pts);
        var inside = query.FilterInside(cloud);
        var outside = query.FilterOutside(cloud);

        Assert.Equal(cloud.Count, inside.Count + outside.Count);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(99)]
    [InlineData(123)]
    public void Complementarity_VariousSeeds(int seed)
    {
        var query = new PointCloudQuery(UnitCube());
        var pts = new Vec3[100];
        var rng = new Random(seed);
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1);

        var cloud = new PointCloud(pts);
        var inside = query.FilterInside(cloud);
        var outside = query.FilterOutside(cloud);

        Assert.Equal(cloud.Count, inside.Count + outside.Count);
    }

    [Fact]
    public void Complementarity_AgainstSphere()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 2.0, 2);
        var query = new PointCloudQuery(sphere);
        var pts = new Vec3[100];
        var rng = new Random(77);
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);

        var cloud = new PointCloud(pts);
        var inside = query.FilterInside(cloud);
        var outside = query.FilterOutside(cloud);

        Assert.Equal(cloud.Count, inside.Count + outside.Count);
    }

    // =========================================================================
    // FilterNearSurface
    // =========================================================================

    [Fact]
    public void FilterNearSurface_ReturnsNearbyPoints()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[]
        {
            new Vec3(0.5, 0.5, 0.5),     // inside, dist=0.5
            new Vec3(1.05, 0.5, 0.5),     // outside, dist ~0.05
            new Vec3(5.0, 5.0, 5.0),      // far away
        });

        var near = query.FilterNearSurface(cloud, 0.1);
        Assert.Equal(1, near.Count);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void FilterNearSurface_VariousThresholds(double threshold)
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[]
        {
            new Vec3(1.0 + threshold * 0.5, 0.5, 0.5),  // within threshold
            new Vec3(1.0 + threshold * 2.0, 0.5, 0.5),  // beyond threshold
        });

        var near = query.FilterNearSurface(cloud, threshold);
        Assert.True(near.Count >= 1, $"threshold={threshold}: expected at least 1 near point");
    }

    // =========================================================================
    // Parallel vs Sequential consistency
    // =========================================================================

    [Fact]
    public void Classify_ParallelMatchesSequential()
    {
        var query = new PointCloudQuery(UnitCube());
        var pts = new Vec3[100];
        var rng = new Random(42);
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1);

        var cloud = new PointCloud(pts);
        var sequential = query.Classify(cloud, parallel: false);
        var parallel = query.Classify(cloud, parallel: true);

        for (int i = 0; i < pts.Length; i++)
            Assert.Equal(sequential[i], parallel[i]);
    }

    [Fact]
    public void Distances_ParallelMatchesSequential()
    {
        var query = new PointCloudQuery(UnitCube());
        var pts = new Vec3[100];
        var rng = new Random(42);
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1);

        var cloud = new PointCloud(pts);
        var sequential = query.Distances(cloud, parallel: false);
        var parallel = query.Distances(cloud, parallel: true);

        for (int i = 0; i < pts.Length; i++)
            Assert.Equal(sequential[i], parallel[i]);
    }

    [Fact]
    public void SignedDistances_ParallelMatchesSequential()
    {
        var query = new PointCloudQuery(UnitCube());
        var pts = new Vec3[100];
        var rng = new Random(42);
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1);

        var cloud = new PointCloud(pts);
        var sequential = query.SignedDistances(cloud, parallel: false);
        var parallel = query.SignedDistances(cloud, parallel: true);

        for (int i = 0; i < pts.Length; i++)
            Assert.Equal(sequential[i], parallel[i]);
    }

    // =========================================================================
    // Single-point queries
    // =========================================================================

    [Fact]
    public void SinglePoint_Classify_Works()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(0.5, 0.5, 0.5) });
        var results = query.Classify(cloud, parallel: false);
        Assert.Equal(SolidClassification.Inside, results[0]);
    }

    [Fact]
    public void SinglePoint_Distance_Works()
    {
        var query = new PointCloudQuery(UnitCube());
        var cloud = new PointCloud(new[] { new Vec3(0.5, 0.5, 0.5) });
        var results = query.Distances(cloud, parallel: false);
        Assert.True(results[0] > 0);
    }

    // =========================================================================
    // Large cloud spot-check
    // =========================================================================

    [Fact]
    public void LargeCloud_10KPoints_SpotCheck()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var query = new PointCloudQuery(sphere);

        var pts = new Vec3[10_000];
        var rng = new Random(99);
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);

        var cloud = new PointCloud(pts);
        var cls = query.Classify(cloud);
        var dists = query.Distances(cloud);

        for (int i = 0; i < cls.Length; i++)
            Assert.True(dists[i] >= 0, $"Point {i} has negative distance {dists[i]}");

        var originCloud = new PointCloud(new[] { Vec3.Zero });
        var originCls = query.Classify(originCloud);
        Assert.Equal(SolidClassification.Inside, originCls[0]);
    }

    [Fact]
    public void LargeCloud_AllInsideSphere_AllClassifiedInside()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 5.0, 2);
        var query = new PointCloudQuery(sphere);

        var pts = new Vec3[100];
        var rng = new Random(42);
        for (int i = 0; i < pts.Length; i++)
        {
            // Points within radius 2 (well inside radius 5 sphere)
            double r = rng.NextDouble() * 2.0;
            double theta = rng.NextDouble() * System.Math.PI;
            double phi = rng.NextDouble() * 2 * System.Math.PI;
            pts[i] = new Vec3(
                r * System.Math.Sin(theta) * System.Math.Cos(phi),
                r * System.Math.Sin(theta) * System.Math.Sin(phi),
                r * System.Math.Cos(theta));
        }

        var cloud = new PointCloud(pts);
        var cls = query.Classify(cloud);

        for (int i = 0; i < cls.Length; i++)
            Assert.Equal(SolidClassification.Inside, cls[i]);
    }

    // =========================================================================
    // Winding number mode
    // =========================================================================

    [Fact]
    public void Classify_WindingNumber_MatchesRayCast()
    {
        var cube = UnitCube();
        var rayQuery = new PointCloudQuery(cube, useWindingNumber: false);
        var windQuery = new PointCloudQuery(cube, useWindingNumber: true);

        var cloud = new PointCloud(new[]
        {
            new Vec3(0.5, 0.5, 0.5),   // well inside
            new Vec3(2.0, 2.0, 2.0),   // well outside
        });

        var rayCls = rayQuery.Classify(cloud, parallel: false);
        var windCls = windQuery.Classify(cloud, parallel: false);

        Assert.Equal(rayCls[0], windCls[0]);
        Assert.Equal(rayCls[1], windCls[1]);
    }

    [Fact]
    public void WindingNumber_MultiplePoints_MatchesRayCast()
    {
        var cube = UnitCube();
        var rayQuery = new PointCloudQuery(cube, useWindingNumber: false);
        var windQuery = new PointCloudQuery(cube, useWindingNumber: true);

        var pts = new Vec3[50];
        var rng = new Random(42);
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1, rng.NextDouble() * 3 - 1);

        var cloud = new PointCloud(pts);
        var rayCls = rayQuery.Classify(cloud, parallel: false);
        var windCls = windQuery.Classify(cloud, parallel: false);

        for (int i = 0; i < pts.Length; i++)
            Assert.Equal(rayCls[i], windCls[i]);
    }
}
