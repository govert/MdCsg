using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 10: Csg.Union/Intersect(IEnumerable), 5-20 solids.</summary>
public class BulkCsgTests
{
    [Fact]
    public void BulkUnion_FiveCubes_ProducesValidMesh()
    {
        var solids = Enumerable.Range(0, 5)
            .Select(i => Primitives.Cube(new Vec3(i * 0.8, 0, 0), 1.0))
            .ToList();
        var r = Csg.Union(solids);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void BulkUnion_TenOverlappingSpheres_ProducesValidMesh()
    {
        var solids = Enumerable.Range(0, 10)
            .Select(i => Primitives.Sphere(new Vec3(i * 0.3, 0, 0), 0.5, 1))
            .ToList();
        var r = Csg.Union(solids);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void BulkIntersect_FiveOverlappingCubes_ProducesValidMesh()
    {
        var solids = Enumerable.Range(0, 5)
            .Select(i => Primitives.Cube(new Vec3(i * 0.1, 0, 0), 2.0))
            .ToList();
        var r = Csg.Intersect(solids);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void BulkUnion_VolumeGrowsMonotonically()
    {
        var solids = new List<Solid>();
        double prevVol = 0;
        for (int i = 0; i < 5; i++)
        {
            solids.Add(Primitives.Cube(new Vec3(i * 0.5, 0, 0), 1.0));
            var vol = new Solid(Csg.Union(solids).Mesh).Volume();
            Assert.True(vol >= prevVol - 0.01);
            prevVol = vol;
        }
    }

    [Fact]
    public void BulkIntersect_VolumeShrinks()
    {
        var solids = new List<Solid>();
        double prevVol = double.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            solids.Add(Primitives.Cube(new Vec3(i * 0.3, 0, 0), 2.0));
            var vol = new Solid(Csg.Intersect(solids).Mesh).Volume();
            Assert.True(vol <= prevVol + 0.1);
            prevVol = vol;
        }
    }

    [Fact]
    public void BulkUnion_SingleSolid_ReturnsSame()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Union(new[] { cube });
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - cube.Volume()) < 0.01);
    }

    [Fact]
    public void BulkIntersect_SingleSolid_ReturnsSame()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(new[] { cube });
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - cube.Volume()) < 0.01);
    }

    [Fact]
    public void BulkUnion_DisjointCubes_VolumeEqualsSum()
    {
        var solids = Enumerable.Range(0, 5)
            .Select(i => Primitives.Cube(new Vec3(i * 5, 0, 0), 1.0))
            .ToList();
        var r = Csg.Union(solids);
        var vol = new Solid(r.Mesh).Volume();
        var sum = solids.Sum(s => s.Volume());
        Assert.True(System.Math.Abs(vol - sum) < 1.0);
    }

    [Fact]
    public void BulkUnion_TwentyCubes_ProducesValidMesh()
    {
        var solids = Enumerable.Range(0, 20)
            .Select(i => Primitives.Cube(new Vec3(i * 0.4, 0, 0), 0.8))
            .ToList();
        var r = Csg.Union(solids);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void BulkUnion_MixedPrimitives_ProducesValidMesh()
    {
        var solids = new List<Solid>
        {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(new Vec3(1, 0, 0), 0.5, 1),
            Primitives.Cylinder(new Vec3(-1, 0, -0.5), Vec3.UnitZ, 0.3, 1.0, 8),
        };
        var r = Csg.Union(solids);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void BulkIntersect_ThreeCubes_InclusionExclusion()
    {
        var a = Primitives.Cube(new Vec3(0.2, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(-0.2, 0, 0), 2.0);
        var c = Primitives.Cube(new Vec3(0, 0.2, 0), 2.0);
        var r = Csg.Intersect(new[] { a, b, c });
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol > 0);
        Assert.True(vol < a.Volume());
    }

    [Fact]
    public void BulkUnion_IsInsideForAllOriginals()
    {
        var centers = new[] { new Vec3(-3, 0, 0), Vec3.Zero, new Vec3(3, 0, 0) };
        var solids = centers.Select(c => Primitives.Cube(c, 1.0)).ToList();
        var result = new Solid(Csg.Union(solids).Mesh);
        foreach (var c in centers)
            Assert.True(result.IsInside(c));
    }

    [Fact]
    public void BulkUnion_FiveSpheres_HasPositiveVolume()
    {
        var solids = Enumerable.Range(0, 5)
            .Select(i => Primitives.Sphere(new Vec3(i * 0.5, 0, 0), 0.4, 1))
            .ToList();
        var r = Csg.Union(solids);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void BulkIntersect_TwoSpheres_SmallerThanEither()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 1.0, 2);
        var r = Csg.Intersect(new[] { a, b });
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol < a.Volume() + 0.1);
    }

    [Fact]
    public void BulkUnion_GridOfCubes_ProducesValidMesh()
    {
        var solids = new List<Solid>();
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                solids.Add(Primitives.Cube(new Vec3(x * 0.8, y * 0.8, 0), 1.0));
        var r = Csg.Union(solids);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void BulkIntersect_ConcentricCubes_EqualsSmallest()
    {
        var solids = new List<Solid>
        {
            Primitives.Cube(Vec3.Zero, 4.0),
            Primitives.Cube(Vec3.Zero, 3.0),
            Primitives.Cube(Vec3.Zero, 2.0),
        };
        var r = Csg.Intersect(solids);
        var vol = new Solid(r.Mesh).Volume();
        var smallest = solids.Last().Volume();
        Assert.True(System.Math.Abs(vol - smallest) < 0.5);
    }

    [Fact]
    public void BulkUnion_EightCubes_ProducesValidMesh()
    {
        var solids = Enumerable.Range(0, 8)
            .Select(i => Primitives.Cube(new Vec3(i * 0.6, 0, 0), 1.0))
            .ToList();
        var r = Csg.Union(solids);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void BulkIntersect_FiveCubes_SmallResult()
    {
        var solids = Enumerable.Range(0, 5)
            .Select(i => Primitives.Cube(new Vec3(i * 0.2, 0, 0), 2.0))
            .ToList();
        var r = Csg.Intersect(solids);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol > 0);
        Assert.True(vol < solids[0].Volume());
    }

    [Fact]
    public void BulkUnion_SpheresInLine_Continuous()
    {
        var solids = Enumerable.Range(0, 7)
            .Select(i => Primitives.Sphere(new Vec3(i * 0.3, 0, 0), 0.3, 1))
            .ToList();
        var r = Csg.Union(solids);
        var result = new Solid(r.Mesh);
        // Middle should be inside
        Assert.True(result.IsInside(new Vec3(0.9, 0, 0)));
    }
}
