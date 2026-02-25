using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG random fuzz — randomized offsets, multiple shapes, no crashes, positive volume</summary>
public class CsgRandomFuzzTests
{
    [Fact]
    public void RandomCubeCube_Union_NoCrash()
    {
        var rng = new System.Random(42);
        for (int i = 0; i < 5; i++)
        {
            var offsetB = new Vec3(rng.NextDouble() * 1.5 - 0.25, rng.NextDouble() * 1.5 - 0.25, rng.NextDouble() * 1.5 - 0.25);
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(offsetB).Mesh);
            var result = Csg.Union(a, b);
            Assert.True(result.FaceCount > 0, $"Union face count should be > 0 at offset {offsetB}");
        }
    }

    [Fact]
    public void RandomCubeCube_Intersection_NoCrash()
    {
        var rng = new System.Random(123);
        for (int i = 0; i < 5; i++)
        {
            var offsetB = new Vec3(rng.NextDouble() * 0.9, rng.NextDouble() * 0.9, rng.NextDouble() * 0.9);
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(offsetB).Mesh);
            var result = Csg.Intersect(a, b);
            if (result.FaceCount > 0)
            {
                double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
                Assert.True(vol > 0, $"Intersection volume should be > 0 at offset {offsetB}");
            }
        }
    }

    [Fact]
    public void RandomCubeCube_Difference_NoCrash()
    {
        var rng = new System.Random(456);
        for (int i = 0; i < 5; i++)
        {
            var offsetB = new Vec3(rng.NextDouble() * 1.5 - 0.25, rng.NextDouble() * 1.5 - 0.25, rng.NextDouble() * 1.5 - 0.25);
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(offsetB).Mesh);
            var result = Csg.Difference(a, b);
            Assert.True(result.FaceCount > 0, $"Difference face count should be > 0 at offset {offsetB}");
        }
    }

    [Fact]
    public void RandomCubeSphere_Union_NoCrash()
    {
        var rng = new System.Random(789);
        for (int i = 0; i < 3; i++)
        {
            var center = new Vec3(rng.NextDouble() * 1.5, rng.NextDouble() * 1.5, rng.NextDouble() * 1.5);
            var cube = new Solid(MeshFactory.CreateCube().Mesh);
            var sphere = new Solid(MeshFactory.CreateSphere(center, 0.5, 2).Mesh);
            var result = Csg.Union(cube, sphere);
            Assert.True(result.FaceCount > 0);
        }
    }

    [Fact]
    public void RandomSpheresSphere_Intersection_NoCrash()
    {
        var rng = new System.Random(321);
        for (int i = 0; i < 3; i++)
        {
            var center = new Vec3(rng.NextDouble() * 0.5, rng.NextDouble() * 0.5, rng.NextDouble() * 0.5);
            var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
            var b = new Solid(MeshFactory.CreateSphere(center, 1.0, 2).Mesh);
            var result = Csg.Intersect(a, b);
            if (result.FaceCount > 0)
            {
                double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
                Assert.True(vol > 0);
            }
        }
    }

    [Fact]
    public void DisjointCubes_AllOps_NoCrash()
    {
        var rng = new System.Random(654);
        for (int i = 0; i < 3; i++)
        {
            var offsetB = new Vec3(5 + rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(offsetB).Mesh);
            Assert.True(Csg.Union(a, b).FaceCount > 0);
            var intersect = Csg.Intersect(a, b);
            Assert.True(intersect.FaceCount == 0 || VolumeCalculator.ComputeAbsoluteVolume(intersect.Mesh) < 0.001);
            Assert.True(Csg.Difference(a, b).FaceCount > 0);
        }
    }

    [Fact]
    public void FullyContained_AllOps_NoCrash()
    {
        var rng = new System.Random(987);
        for (int i = 0; i < 3; i++)
        {
            var innerOffset = new Vec3(0.3 + rng.NextDouble() * 0.4, 0.3 + rng.NextDouble() * 0.4, 0.3 + rng.NextDouble() * 0.4);
            var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
            var inner = new Solid(MeshFactory.CreateCube(innerOffset, 0.3).Mesh);
            Assert.True(Csg.Union(outer, inner).FaceCount > 0);
            Assert.True(Csg.Intersect(outer, inner).FaceCount > 0);
            Assert.True(Csg.Difference(outer, inner).FaceCount > 0);
        }
    }

    [Fact]
    public void VaryingSizes_Union_PositiveVolume()
    {
        double[] sizes = { 0.5, 1.0, 1.5, 2.0 };
        foreach (double size in sizes)
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), size).Mesh);
            var result = Csg.Union(a, b);
            double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            Assert.True(vol > 0, $"Union volume should be > 0 for size {size}");
        }
    }
}
