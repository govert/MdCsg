using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 48: Timed: simplification of 10k+ face meshes.</summary>
public class PerformanceSimplificationTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceSimplificationTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void SimplifyCube_Under2Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(cube.Mesh, 6);
        sw.Stop();
        _output.WriteLine($"Simplify cube to 6: {sw.ElapsedMilliseconds}ms, {simplified.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void SimplifySphere_HalfFaces_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        int target = sphere.Mesh.Faces.Count / 2;
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, target);
        sw.Stop();
        _output.WriteLine($"Simplify sphere(3) to {target}: {sw.ElapsedMilliseconds}ms, got {simplified.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SimplifyHighPolySphere_Under10Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        int target = sphere.Mesh.Faces.Count / 4;
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, target);
        sw.Stop();
        _output.WriteLine($"Simplify sphere(4) {sphere.Mesh.Faces.Count}→{simplified.Faces.Count}: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void SimplifyTorus_Under5Seconds()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 3.0, 1.0, 48, 24);
        int target = torus.Mesh.Faces.Count / 2;
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(torus.Mesh, target);
        sw.Stop();
        _output.WriteLine($"Simplify torus {torus.Mesh.Faces.Count}→{simplified.Faces.Count}: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SimplifyCsgResult_Under10Seconds()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.2);
        var result = Csg.Intersect(a, b);
        int target = result.FaceCount / 2;
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(result.Mesh, target);
        sw.Stop();
        _output.WriteLine($"Simplify CSG {result.FaceCount}→{simplified.Faces.Count}: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void SimplifyCylinder_Under5Seconds()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 48);
        int target = cyl.Mesh.Faces.Count / 2;
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(cyl.Mesh, target);
        sw.Stop();
        _output.WriteLine($"Simplify cylinder(48) {cyl.Mesh.Faces.Count}→{simplified.Faces.Count}: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SimplifyByErrorThreshold_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, 0.01);
        sw.Stop();
        _output.WriteLine($"Simplify by threshold {sphere.Mesh.Faces.Count}→{simplified.Faces.Count}: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SimplifiedMesh_HasFewerFaces()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        int target = sphere.Mesh.Faces.Count / 3;
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, target);
        Assert.True(simplified.Faces.Count <= target + 10); // Allow some tolerance
    }

    [Fact]
    public void SimplifiedMesh_VolumeApproximatelyPreserved()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var originalVol = new Solid(sphere.Mesh).Volume();
        int target = sphere.Mesh.Faces.Count / 2;
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, target);
        var simplifiedVol = new Solid(simplified).Volume();
        _output.WriteLine($"Vol original={originalVol:F4}, simplified={simplifiedVol:F4}");
        Assert.True(System.Math.Abs(originalVol - simplifiedVol) / originalVol < 0.2);
    }

    [Fact]
    public void SmoothThenSimplify_Under10Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sw = Stopwatch.StartNew();
        var smoothed = MeshSmoothing.LaplacianSmooth(sphere.Mesh, 3);
        var simplified = MeshSimplifier.Simplify(smoothed, sphere.Mesh.Faces.Count / 2);
        sw.Stop();
        _output.WriteLine($"Smooth+Simplify: {sw.ElapsedMilliseconds}ms, {simplified.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void SimplifyMinimal_StillValid()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var simplified = MeshSimplifier.Simplify(cube.Mesh, 4);
        Assert.True(simplified.Faces.Count >= 4);
    }

    [Fact]
    public void SimplifyNoChange_SameAsFaceCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var simplified = MeshSimplifier.Simplify(cube.Mesh, cube.Mesh.Faces.Count);
        Assert.Equal(cube.Mesh.Faces.Count, simplified.Faces.Count);
    }

    [Fact]
    public void SimplifiedCsgResult_CanComputeVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var result = Csg.Difference(a, b);
        var simplified = MeshSimplifier.Simplify(result.Mesh, result.FaceCount / 2);
        var vol = new Solid(simplified).Volume();
        _output.WriteLine($"Simplified CSG volume: {vol:F4}");
        Assert.True(vol > 0);
    }

    [Fact]
    public void SimplifyCone_Under5Seconds()
    {
        var cone = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 48);
        int target = cone.Mesh.Faces.Count / 2;
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(cone.Mesh, target);
        sw.Stop();
        _output.WriteLine($"Simplify cone(48) {cone.Mesh.Faces.Count}→{simplified.Faces.Count}: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SimplifySphereAggressive_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        int target = 20;
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, target);
        sw.Stop();
        _output.WriteLine($"Aggressive simplify {sphere.Mesh.Faces.Count}→{simplified.Faces.Count}: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void MultipleSimplifications_Under10Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 5; i++)
        {
            int target = sphere.Mesh.Faces.Count / (i + 2);
            MeshSimplifier.Simplify(sphere.Mesh, target);
        }
        sw.Stop();
        _output.WriteLine($"5 simplifications: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void LaplacianSmooth_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sw = Stopwatch.StartNew();
        var smoothed = MeshSmoothing.LaplacianSmooth(sphere.Mesh, 10);
        sw.Stop();
        _output.WriteLine($"Laplacian smooth 10 iters: {sw.ElapsedMilliseconds}ms, {smoothed.Faces.Count} faces");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void LaplacianSmooth_HighPoly_Under10Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var sw = Stopwatch.StartNew();
        var smoothed = MeshSmoothing.LaplacianSmooth(sphere.Mesh, 5);
        sw.Stop();
        _output.WriteLine($"Smooth highPoly 5 iters: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void SimplifyThenCsg_Under15Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, sphere.Mesh.Faces.Count / 2);
        var simplifiedSolid = new Solid(simplified);
        var cube = Primitives.Cube(Vec3.Zero, 1.0);
        var sw = Stopwatch.StartNew();
        var result = Csg.Intersect(simplifiedSolid, cube);
        sw.Stop();
        _output.WriteLine($"Simplified CSG: {sw.ElapsedMilliseconds}ms, {result.FaceCount} faces");
        Assert.True(sw.ElapsedMilliseconds < 15000);
    }

    [Fact]
    public void SimplifyByThreshold_Large_Under10Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var sw = Stopwatch.StartNew();
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, 0.1);
        sw.Stop();
        _output.WriteLine($"Threshold simplify highPoly: {sw.ElapsedMilliseconds}ms, {sphere.Mesh.Faces.Count}→{simplified.Faces.Count}");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }
}
