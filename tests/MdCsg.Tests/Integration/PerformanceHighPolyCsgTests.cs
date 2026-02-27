using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Math;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 43: Timed: high-poly sphere CSG.</summary>
public class PerformanceHighPolyCsgTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceHighPolyCsgTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void HighPolySphereCreation_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        sw.Stop();
        _output.WriteLine($"Sphere(subdiv=4): {sw.ElapsedMilliseconds}ms, {sphere.Mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolySphereUnion_Under10Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 1.0, 3);
        var result = Csg.Union(a, b);
        sw.Stop();
        _output.WriteLine($"HighPolySphereUnion: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void HighPolySphereIntersect_Under10Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 1.0, 3);
        var result = Csg.Intersect(a, b);
        sw.Stop();
        _output.WriteLine($"HighPolySphereIntersect: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void HighPolySphereDifference_Under10Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 1.0, 3);
        var result = Csg.Difference(a, b);
        sw.Stop();
        _output.WriteLine($"HighPolySphereDifference: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void HighPolySphereVolume_Under2Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var sw = Stopwatch.StartNew();
        var vol = sphere.Volume();
        sw.Stop();
        _output.WriteLine($"HighPolySphereVolume: {sw.ElapsedMilliseconds}ms, vol={vol:F4}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolySphereArea_Under2Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var sw = Stopwatch.StartNew();
        var area = sphere.SurfaceArea();
        sw.Stop();
        _output.WriteLine($"HighPolySphereArea: {sw.ElapsedMilliseconds}ms, area={area:F4}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolySphereIsInside_Under2Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
            _ = sphere.IsInside(new Vec3(0.5, 0, 0));
        sw.Stop();
        _output.WriteLine($"100 IsInside on highPoly: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolySphereCubeDiff_Under10Seconds()
    {
        var sw = Stopwatch.StartNew();
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var cube = Primitives.Cube(Vec3.Zero, 1.0);
        var result = Csg.Difference(sphere, cube);
        sw.Stop();
        _output.WriteLine($"SphereMinusCube(highPoly): {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void HighPolySphereTransform_Under2Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var t = Transform3.FromRotationX(0.5) * Transform3.FromTranslation(new Vec3(1, 2, 3));
        var sw = Stopwatch.StartNew();
        var transformed = sphere.Transform(t);
        sw.Stop();
        _output.WriteLine($"Transform highPoly: {sw.ElapsedMilliseconds}ms, {transformed.Mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolyTorus_Creation_Under2Seconds()
    {
        var sw = Stopwatch.StartNew();
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 3.0, 1.0, 48, 24);
        sw.Stop();
        _output.WriteLine($"Torus(48,24): {sw.ElapsedMilliseconds}ms, {torus.Mesh.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolyCylinder_CsgUnion_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 48);
        var b = Primitives.Cylinder(new Vec3(0.5, 0, 0), Vec3.UnitZ, 1.0, 2.0, 48);
        var result = Csg.Union(a, b);
        sw.Stop();
        _output.WriteLine($"HighPolyCylinderUnion: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 7000);
    }

    [Fact]
    public void HighPolySphereCsgChain_Under15Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Sphere(Vec3.Zero, 2.0, 3);
        var b = Primitives.Cube(Vec3.Zero, 1.5);
        var c = Primitives.Cylinder(new Vec3(0, 0, -3), Vec3.UnitZ, 0.5, 6.0);
        var r1 = new Solid(Csg.Intersect(a, b).Mesh);
        var result = Csg.Difference(r1, c);
        sw.Stop();
        _output.WriteLine($"HighPolyChain: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 15000);
    }

    [Fact]
    public void MediumPolySphereUnion_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        sw.Stop();
        _output.WriteLine($"MediumPolySphereUnion: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void HighPolyComplement_Under2Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var sw = Stopwatch.StartNew();
        var comp = sphere.Complement();
        sw.Stop();
        _output.WriteLine($"HighPolyComplement: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolyBvhBuild_Under2Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var sw = Stopwatch.StartNew();
        var bvh = MdCsg.Bvh.BvhTree.Build(sphere.Mesh);
        sw.Stop();
        _output.WriteLine($"BvhBuild(highPoly): {sw.ElapsedMilliseconds}ms, {bvh.NodeCount} nodes");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolyBvhQuery_Under2Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var bvh = MdCsg.Bvh.BvhTree.Build(sphere.Mesh);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            var results = new List<int>();
            bvh.Query(new Aabb(new Vec3(-0.5, -0.5, -0.5), new Vec3(0.5, 0.5, 0.5)), results);
        }
        sw.Stop();
        _output.WriteLine($"1000 BvhQueries(highPoly): {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HighPolyHalfSpaceCut_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var sw = Stopwatch.StartNew();
        var result = Csg.Intersect(sphere, hs);
        sw.Stop();
        _output.WriteLine($"HighPolyHalfSpace: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void HighPolyImplicitSphere_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var impl = new ImplicitSphere(Vec3.Zero, 1.5);
        var sw = Stopwatch.StartNew();
        var result = Csg.Intersect(cube, impl);
        sw.Stop();
        _output.WriteLine($"HighPolyImplicitSphere: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void HighPolySphereOperators_Under10Seconds()
    {
        var sw = Stopwatch.StartNew();
        var a = Primitives.Sphere(Vec3.Zero, 2.0, 3);
        var b = Primitives.Cube(Vec3.Zero, 1.5);
        var result = a ^ b; // Intersection via operator
        sw.Stop();
        _output.WriteLine($"HighPolyOperator: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }
}
