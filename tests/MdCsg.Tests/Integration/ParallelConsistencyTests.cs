using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 41: Parallel=true vs false give same results.</summary>
public class ParallelConsistencyTests
{
    [Fact]
    public void Union_ParallelVsSerial_SameVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(1, 0, 0), 1.0);
        var serial = Csg.Union(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Union(a, b, new CsgOptions { Parallel = true });
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.1);
    }

    [Fact]
    public void Intersect_ParallelVsSerial_SameVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.2);
        var serial = Csg.Intersect(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Intersect(a, b, new CsgOptions { Parallel = true });
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.1);
    }

    [Fact]
    public void Difference_ParallelVsSerial_SameVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var serial = Csg.Difference(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Difference(a, b, new CsgOptions { Parallel = true });
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.1);
    }

    [Fact]
    public void Union_ParallelVsSerial_SameFaceCount()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var serial = Csg.Union(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Union(a, b, new CsgOptions { Parallel = true });
        Assert.Equal(serial.FaceCount, parallel.FaceCount);
    }

    [Fact]
    public void Intersect_ParallelVsSerial_SameFaceCount()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 2.0);
        var serial = Csg.Intersect(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Intersect(a, b, new CsgOptions { Parallel = true });
        Assert.Equal(serial.FaceCount, parallel.FaceCount);
    }

    [Fact]
    public void Difference_ParallelVsSerial_SameFaceCount()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 2.0);
        var serial = Csg.Difference(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Difference(a, b, new CsgOptions { Parallel = true });
        Assert.Equal(serial.FaceCount, parallel.FaceCount);
    }

    [Fact]
    public void Union_SphereCylinder_ParallelVsSerial()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.5);
        var b = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 0.5, 3.0);
        var serial = Csg.Union(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Union(a, b, new CsgOptions { Parallel = true });
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.1);
    }

    [Fact]
    public void Intersect_SphereSphere_ParallelVsSerial()
    {
        var a = Primitives.Sphere(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(1, 0, 0), 2.0);
        var serial = Csg.Intersect(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Intersect(a, b, new CsgOptions { Parallel = true });
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.1);
    }

    [Fact]
    public void ParallelFalse_StillProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var result = Csg.Difference(a, b, new CsgOptions { Parallel = false });
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void ParallelTrue_StillProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var result = Csg.Difference(a, b, new CsgOptions { Parallel = true });
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Union_ConeCube_ParallelVsSerial()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 0.8, 2.0);
        var serial = Csg.Union(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Union(a, b, new CsgOptions { Parallel = true });
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.2);
    }

    [Fact]
    public void Difference_CubeCylinder_ParallelVsSerial()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.5, 4.0);
        var serial = Csg.Difference(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Difference(a, b, new CsgOptions { Parallel = true });
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.1);
    }

    [Fact]
    public void WindingNumber_ParallelVsSerial()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var optSerial = new CsgOptions { Parallel = false, UseWindingNumber = true };
        var optParallel = new CsgOptions { Parallel = true, UseWindingNumber = true };
        var serial = Csg.Union(a, b, optSerial);
        var parallel = Csg.Union(a, b, optParallel);
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.1);
    }

    [Fact]
    public void IsInside_ConsistentAcrossParallelModes()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8);
        var serial = new Solid(Csg.Difference(a, b, new CsgOptions { Parallel = false }).Mesh);
        var parallel = new Solid(Csg.Difference(a, b, new CsgOptions { Parallel = true }).Mesh);
        // Test a point that should be inside both results
        var p = new Vec3(0.95, 0, 0);
        Assert.Equal(serial.IsInside(p), parallel.IsInside(p));
    }

    [Fact]
    public void SurfaceArea_ConsistentAcrossParallelModes()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var serial = new Solid(Csg.Union(a, b, new CsgOptions { Parallel = false }).Mesh);
        var parallel = new Solid(Csg.Union(a, b, new CsgOptions { Parallel = true }).Mesh);
        Assert.True(System.Math.Abs(serial.SurfaceArea() - parallel.SurfaceArea()) < 0.1);
    }

    [Fact]
    public void IntersectionSegmentCount_ConsistentAcrossParallelModes()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.2);
        var serial = Csg.Intersect(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Intersect(a, b, new CsgOptions { Parallel = true });
        Assert.Equal(serial.IntersectionSegmentCount, parallel.IntersectionSegmentCount);
    }

    [Fact]
    public void MultiplePrimitives_ParallelVsSerial()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Sphere(new Vec3(1, 0, 0), 1.0);
        var c = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.5, 4.0);
        var optS = new CsgOptions { Parallel = false };
        var optP = new CsgOptions { Parallel = true };
        var r1 = Csg.Difference(a, b, optS);
        var s1 = new Solid(r1.Mesh);
        var r2 = Csg.Difference(s1, c, optS);
        var volSerial = new Solid(r2.Mesh).Volume();

        var r3 = Csg.Difference(a, b, optP);
        var s3 = new Solid(r3.Mesh);
        var r4 = Csg.Difference(s3, c, optP);
        var volParallel = new Solid(r4.Mesh).Volume();

        Assert.True(System.Math.Abs(volSerial - volParallel) < 0.2);
    }

    [Fact]
    public void HighPolyInput_ParallelVsSerial()
    {
        var a = Primitives.Sphere(Vec3.Zero, 2.0, 3);
        var b = Primitives.Sphere(new Vec3(1, 0, 0), 2.0, 3);
        var serial = Csg.Union(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Union(a, b, new CsgOptions { Parallel = true });
        Assert.Equal(serial.FaceCount, parallel.FaceCount);
    }

    [Fact]
    public void TorusUnion_ParallelVsSerial()
    {
        var a = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 3.0, 1.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        var serial = Csg.Union(a, b, new CsgOptions { Parallel = false });
        var parallel = Csg.Union(a, b, new CsgOptions { Parallel = true });
        var volS = new Solid(serial.Mesh).Volume();
        var volP = new Solid(parallel.Mesh).Volume();
        Assert.True(System.Math.Abs(volS - volP) < 0.2);
    }

    [Fact]
    public void DefaultOptions_AreParallel()
    {
        var opts = new CsgOptions();
        Assert.True(opts.Parallel);
    }
}
