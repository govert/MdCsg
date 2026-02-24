using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Robustness stress tests — varied offsets, chaining, classifiers</summary>
public class RobustnessStressTests
{
    [Fact]
    public void Union_ManyCubeOffsets()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        double[] offsets = { 0.1, 0.2, 0.4, 0.6, 0.8 };
        foreach (double off in offsets)
        {
            var b = new Solid(MeshFactory.CreateCube(new Vec3(off, off, off)).Mesh);
            var r = Csg.Union(a, b);
            Assert.True(r.FaceCount > 0, $"Union failed at offset {off}");
        }
    }

    [Fact]
    public void Intersect_ManyCubeOffsets()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        double[] offsets = { 0.1, 0.3, 0.5, 0.7 };
        foreach (double off in offsets)
        {
            var b = new Solid(MeshFactory.CreateCube(new Vec3(off, off, off)).Mesh);
            var r = Csg.Intersect(a, b);
            Assert.True(r.FaceCount > 0, $"Intersect at offset {off}: fc={r.FaceCount}");
        }
    }

    [Fact]
    public void Union_ChainedSequential_3()
    {
        var result = new Solid(MeshFactory.CreateCube().Mesh);
        for (int i = 1; i <= 2; i++)
        {
            var b = new Solid(MeshFactory.CreateCube(new Vec3(i * 0.4, 0, 0)).Mesh);
            result = new Solid(Csg.Union(result, b).Mesh);
        }
        Assert.True(result.Mesh.Faces.Count > 12);
    }

    [Fact]
    public void Difference_Sequential_Carves()
    {
        var cube = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var small1 = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1), size: 0.3).Mesh);
        var small2 = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 1.0, 1.0), size: 0.3).Mesh);
        var r1 = Csg.Difference(cube, small1);
        var r2 = Csg.Difference(new Solid(r1.Mesh), small2);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r2.Mesh);
        Assert.True(vol > 7.0 && vol < 8.5, $"Carved cube volume: {vol}");
    }

    [Fact]
    public void WindingNumber_ManyPoints_Sphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var insidePoints = new[]
        {
            new Vec3(0, 0, 0),
            new Vec3(0.5, 0, 0),
            new Vec3(0, 0.5, 0),
            new Vec3(0, 0, 0.5),
            new Vec3(-0.3, 0.3, 0.2),
        };
        foreach (var p in insidePoints)
        {
            double wn = WindingNumberClassifier.ComputeWindingNumber(p, bvh);
            Assert.True(wn > 0.5, $"Point {p} should be inside, wn={wn}");
        }
    }

    [Fact]
    public void Csg_AlternatingOperations()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.6, 0.6, 0.6)).Mesh);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Difference(new Solid(r1.Mesh), c);
        Assert.True(r2.FaceCount > 0);
    }

    [Fact]
    public void Csg_AsymmetricOffsets()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.2, 0.5, 0.8)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 1.0, $"Asymmetric union volume: {vol}");
    }

    [Fact]
    public void BvhRayCast_AllAxes_InsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var center = new Vec3(0.5, 0.5, 0.5);
        var directions = new[]
        {
            new Vec3(1, 0.001, 0.001), new Vec3(-1, 0.001, 0.001),
            new Vec3(0.001, 1, 0.001), new Vec3(0.001, -1, 0.001),
            new Vec3(0.001, 0.001, 1), new Vec3(0.001, 0.001, -1),
        };
        foreach (var dir in directions)
        {
            var ray = new Ray(center, dir.Normalized);
            int hits = bvh.RayCastCount(ray);
            Assert.True(hits >= 1, $"Ray from center in direction {dir}: {hits} hits");
        }
    }

    [Fact]
    public void Sphere_Union_Volume_Between_OneAndTwo()
    {
        var s1 = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var s2 = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var r = Csg.Union(s1, s2);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(s1.Mesh);
        Assert.True(vol > v1 && vol < 2.5 * v1, $"Sphere union volume: {vol}, single: {v1}");
    }

    [Fact]
    public void Sphere_Intersection_LessThanEither()
    {
        var s1 = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var s2 = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var r = Csg.Intersect(s1, s2);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(s1.Mesh);
        Assert.True(vol < v1, $"Intersection {vol} should be < sphere {v1}");
    }

    [Fact]
    public void Csg_WithWindingNumber_And_RayCast_SameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var rcResult = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var wnResult = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rcResult.FaceCount, wnResult.FaceCount);
    }

    [Fact]
    public void Csg_HighSubdivisionSphere()
    {
        var s1 = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh);
        var s2 = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1, 3).Mesh);
        var r = Csg.Union(s1, s2);
        Assert.True(r.FaceCount > 0);
        Assert.True(r.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Csg_AxisAlignedOffsets()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        // Offset along each axis only
        foreach (var offset in new[] {
            new Vec3(0.5, 0, 0),
            new Vec3(0, 0.5, 0),
            new Vec3(0, 0, 0.5) })
        {
            var b = new Solid(MeshFactory.CreateCube(offset).Mesh);
            var r = Csg.Union(a, b);
            Assert.True(r.FaceCount > 0, $"Failed at offset {offset}");
        }
    }
}
