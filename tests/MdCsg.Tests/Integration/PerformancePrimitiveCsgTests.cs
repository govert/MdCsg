using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Math;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 42: Timed: primitive CSG &lt; thresholds.</summary>
public class PerformancePrimitiveCsgTests
{
    private readonly ITestOutputHelper _output;

    public PerformancePrimitiveCsgTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void CubeCubeUnion_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        sw.Stop();
        _output.WriteLine($"CubeCubeUnion: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void CubeSphereIntersect_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.2);
        var result = Csg.Intersect(a, b);
        sw.Stop();
        _output.WriteLine($"CubeSphereIntersect: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void CubeCylinderDifference_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.5, 4.0);
        var result = Csg.Difference(a, b);
        sw.Stop();
        _output.WriteLine($"CubeCylinderDifference: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void SphereSphereUnion_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Sphere(Vec3.Zero, 1.0);
        var b = Primitives.Sphere(new Vec3(1, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        sw.Stop();
        _output.WriteLine($"SphereSphereUnion: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void CylinderCylinderIntersect_Under3Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 1.0, 4.0);
        var b = Primitives.Cylinder(new Vec3(-2, 0, 0), Vec3.UnitX, 1.0, 4.0);
        var result = Csg.Intersect(a, b);
        sw.Stop();
        _output.WriteLine($"CylinderCylinderIntersect: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 3000);
    }

    [Fact]
    public void CubeConeDifference_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 0.8, 2.0);
        var result = Csg.Difference(a, b);
        sw.Stop();
        _output.WriteLine($"CubeConeDifference: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void TorusCubeUnion_Under3Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        var result = Csg.Union(a, b);
        sw.Stop();
        _output.WriteLine($"TorusCubeUnion: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 3000);
    }

    [Fact]
    public void ThreeOperations_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.5);
        var c = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.5, 4.0);
        var r1 = new Solid(Csg.Difference(a, b).Mesh);
        var result = Csg.Union(r1, c);
        sw.Stop();
        _output.WriteLine($"ThreeOps: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void PrimitiveCreation_Fast()
    {
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            _ = Primitives.Cube(Vec3.Zero, 1.0);
            _ = Primitives.Sphere(Vec3.Zero, 1.0);
            _ = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0);
        }
        sw.Stop();
        _output.WriteLine($"300 primitives: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void VolumeCalculation_Fast()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
            _ = cube.Volume();
        sw.Stop();
        _output.WriteLine($"1000 volume calcs: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void IsInsideCheck_Fast()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
            _ = cube.IsInside(new Vec3(0.5, 0.5, 0.5));
        sw.Stop();
        _output.WriteLine($"100 IsInside: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void BoxBoxUnion_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Box(Vec3.Zero, new Vec3(2, 3, 4));
        var b = Primitives.Box(new Vec3(1, 1, 1), new Vec3(2, 3, 4));
        var result = Csg.Union(a, b);
        sw.Stop();
        _output.WriteLine($"BoxBoxUnion: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void CubeHalfSpaceIntersect_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var result = Csg.Intersect(cube, hs);
        sw.Stop();
        _output.WriteLine($"CubeHalfSpace: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void CubeImplicitSphereIntersect_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var implSphere = new ImplicitSphere(Vec3.Zero, 1.2);
        var result = Csg.Intersect(cube, implSphere);
        sw.Stop();
        _output.WriteLine($"CubeImplicitSphere: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void CubeImplicitCylinderDiff_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var implCyl = new ImplicitCylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.5, 4.0);
        var result = Csg.Difference(cube, implCyl);
        sw.Stop();
        _output.WriteLine($"CubeImplicitCylinder: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void BulkUnion5Cubes_Under5Seconds()
    {
        var cubes = Enumerable.Range(0, 5).Select(i =>
            Primitives.Cube(new Vec3(i * 1.5, 0, 0), 2.0)).ToList();
        var sw = Stopwatch.StartNew();
        var result = Csg.Union(cubes);
        sw.Stop();
        _output.WriteLine($"BulkUnion5: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SolidTransform_Fast()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var t = Transform3.FromRotationX(0.5) * Transform3.FromTranslation(new Vec3(1, 2, 3));
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
            _ = cube.Transform(t);
        sw.Stop();
        _output.WriteLine($"1000 transforms: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 3000);
    }

    [Fact]
    public void CubeComplement_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var comp = cube.Complement();
        sw.Stop();
        _output.WriteLine($"Complement: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
        Assert.True(MdCsg.Mesh.MeshQueries.Volume(comp.Mesh) < 0);
    }

    [Fact]
    public void OperatorChain_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.5);
        var c = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.5, 4.0);
        var ab = new Solid((a - b).Mesh);
        var result = Csg.Union(ab, c);
        sw.Stop();
        _output.WriteLine($"OperatorChain: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }
}
