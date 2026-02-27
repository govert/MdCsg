using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 27: Classify/Distances/Filter on CSG results.</summary>
public class PointCloudQueryTests
{
    [Fact]
    public void ClassifyCube_InsidePointsAreInside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cloud = new PointCloud(new[] { Vec3.Zero, new Vec3(0.5, 0.5, 0.5) });
        var query = new PointCloudQuery(cube);
        var cls = query.Classify(cloud);
        Assert.Equal(SolidClassification.Inside, cls[0]);
        Assert.Equal(SolidClassification.Inside, cls[1]);
    }

    [Fact]
    public void ClassifyCube_OutsidePointsAreOutside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cloud = new PointCloud(new[] { new Vec3(5, 0, 0), new Vec3(0, 5, 0) });
        var query = new PointCloudQuery(cube);
        var cls = query.Classify(cloud);
        Assert.Equal(SolidClassification.Outside, cls[0]);
        Assert.Equal(SolidClassification.Outside, cls[1]);
    }

    [Fact]
    public void Distances_AllNonNegative()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var pts = Enumerable.Range(0, 10)
            .Select(i => new Vec3(i * 0.5 - 2.5, 0, 0)).ToArray();
        var cloud = new PointCloud(pts);
        var query = new PointCloudQuery(cube);
        var dists = query.Distances(cloud);
        foreach (var d in dists)
            Assert.True(d >= 0);
    }

    [Fact]
    public void SignedDistances_InsideNegative_OutsidePositive()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cloud = new PointCloud(new[] { Vec3.Zero, new Vec3(5, 0, 0) });
        var query = new PointCloudQuery(cube);
        var sds = query.SignedDistances(cloud);
        Assert.True(sds[0] < 0);
        Assert.True(sds[1] > 0);
    }

    [Fact]
    public void FilterInside_OnlyReturnsInsidePoints()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var pts = new[]
        {
            Vec3.Zero, new Vec3(0.5, 0, 0), new Vec3(5, 0, 0), new Vec3(0, 5, 0)
        };
        var cloud = new PointCloud(pts);
        var query = new PointCloudQuery(cube);
        var inside = query.FilterInside(cloud);
        Assert.Equal(2, inside.Count);
    }

    [Fact]
    public void FilterOutside_OnlyReturnsOutsidePoints()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var pts = new[]
        {
            Vec3.Zero, new Vec3(0.5, 0, 0), new Vec3(5, 0, 0), new Vec3(0, 5, 0)
        };
        var cloud = new PointCloud(pts);
        var query = new PointCloudQuery(cube);
        var outside = query.FilterOutside(cloud);
        Assert.Equal(2, outside.Count);
    }

    [Fact]
    public void FilterNearSurface_ReturnsNearPoints()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var pts = new[]
        {
            new Vec3(1.0, 0, 0), // on surface
            new Vec3(0.95, 0, 0), // near surface inside
            new Vec3(1.05, 0, 0), // near surface outside
            new Vec3(5, 0, 0), // far away
        };
        var cloud = new PointCloud(pts);
        var query = new PointCloudQuery(cube);
        var near = query.FilterNearSurface(cloud, 0.2);
        Assert.True(near.Count >= 2);
    }

    [Fact]
    public void ClassifyWithDistances_BothOutputsValid()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cloud = new PointCloud(new[] { Vec3.Zero, new Vec3(5, 0, 0) });
        var query = new PointCloudQuery(cube);
        var cls = new SolidClassification[2];
        var dists = new double[2];
        query.ClassifyWithDistances(cloud, cls, dists);
        Assert.Equal(SolidClassification.Inside, cls[0]);
        Assert.Equal(SolidClassification.Outside, cls[1]);
        Assert.True(dists[0] >= 0);
        Assert.True(dists[1] >= 0);
    }

    [Fact]
    public void ClassifyOnCsgResult_WorksCorrectly()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var cloud = new PointCloud(new[] { Vec3.Zero, new Vec3(0.8, 0, 0) });
        var query = new PointCloudQuery(r);
        var cls = query.Classify(cloud);
        Assert.Equal(SolidClassification.Outside, cls[0]); // hollowed center
        Assert.Equal(SolidClassification.Inside, cls[1]); // still inside cube shell
    }

    [Fact]
    public void EmptyPointCloud_NoError()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cloud = new PointCloud(Array.Empty<Vec3>());
        var query = new PointCloudQuery(cube);
        var cls = query.Classify(cloud);
        Assert.Empty(cls);
    }

    [Fact]
    public void LargePointCloud_AllClassified()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 1000)
            .Select(_ => new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2))
            .ToArray();
        var cloud = new PointCloud(pts);
        var query = new PointCloudQuery(cube);
        var cls = query.Classify(cloud);
        Assert.Equal(1000, cls.Length);
    }

    [Fact]
    public void ClassifyWithWindingNumber_AgreesWithRayCast()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cloud = new PointCloud(new[] { Vec3.Zero, new Vec3(5, 0, 0) });
        var qRay = new PointCloudQuery(cube, useWindingNumber: false);
        var qWind = new PointCloudQuery(cube, useWindingNumber: true);
        var clsRay = qRay.Classify(cloud);
        var clsWind = qWind.Classify(cloud);
        Assert.Equal(clsRay[0], clsWind[0]);
        Assert.Equal(clsRay[1], clsWind[1]);
    }

    [Fact]
    public void Distances_FarPointHasLargeDistance()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cloud = new PointCloud(new[] { new Vec3(100, 0, 0) });
        var query = new PointCloudQuery(cube);
        var dists = query.Distances(cloud);
        Assert.True(dists[0] > 90);
    }

    [Fact]
    public void Distances_CenterHasSomeDistance()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cloud = new PointCloud(new[] { Vec3.Zero });
        var query = new PointCloudQuery(cube);
        var dists = query.Distances(cloud);
        Assert.True(dists[0] > 0);
    }

    [Fact]
    public void PointCloudWithNormals_ClassifiesCorrectly()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var pts = new[] { Vec3.Zero, new Vec3(5, 0, 0) };
        var normals = new[] { Vec3.UnitZ, Vec3.UnitX };
        var cloud = new PointCloud(pts, normals);
        var query = new PointCloudQuery(cube);
        var cls = query.Classify(cloud);
        Assert.Equal(SolidClassification.Inside, cls[0]);
    }

    [Fact]
    public void FilterInside_OnSphere_ReturnsInterior()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 100)
            .Select(_ => new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2))
            .ToArray();
        var cloud = new PointCloud(pts);
        var query = new PointCloudQuery(sphere);
        var inside = query.FilterInside(cloud);
        // All returned points should be inside the sphere
        foreach (var p in inside.Points)
            Assert.True(p.Length < 1.2); // generous tolerance for mesh approximation
    }

    [Fact]
    public void SignedDistances_ConsistentWithClassify()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var pts = new[] { Vec3.Zero, new Vec3(0.5, 0, 0), new Vec3(5, 0, 0) };
        var cloud = new PointCloud(pts);
        var query = new PointCloudQuery(cube);
        var cls = query.Classify(cloud);
        var sds = query.SignedDistances(cloud);
        for (int i = 0; i < 3; i++)
        {
            if (cls[i] == SolidClassification.Inside)
                Assert.True(sds[i] < 0);
            else
                Assert.True(sds[i] > 0);
        }
    }

    [Fact]
    public void NonParallel_ClassifyAgreesWithParallel()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var pts = new[] { Vec3.Zero, new Vec3(0.5, 0, 0), new Vec3(5, 0, 0) };
        var cloud = new PointCloud(pts);
        var query = new PointCloudQuery(cube);
        var par = query.Classify(cloud, parallel: true);
        var seq = query.Classify(cloud, parallel: false);
        for (int i = 0; i < 3; i++)
            Assert.Equal(par[i], seq[i]);
    }
}
