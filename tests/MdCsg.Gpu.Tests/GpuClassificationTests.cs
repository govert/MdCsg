using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using Xunit;

namespace MdCsg.Gpu.Tests;

/// <summary>
/// Verifies that GPU classification produces the same results as CPU.
/// Tests are skipped if no Vulkan device is available.
/// </summary>
public class GpuClassificationTests : IDisposable
{
    private readonly GpuPatchClassifier? _gpuClassifier;

    public GpuClassificationTests()
    {
        _gpuClassifier = GpuAccelerator.TryCreate();
    }

    public void Dispose()
    {
        _gpuClassifier?.Dispose();
    }

    [Fact]
    public void CubeCubeUnion_GpuMatchesCpu()
    {
        if (_gpuClassifier == null) return;

        var a = CreateCube(Vec3.Zero);
        var b = CreateCube(new Vec3(0.5, 0.5, 0.5));

        var cpuResult = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });

        var gpuResult = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = _gpuClassifier
        });

        Assert.Equal(cpuResult.PatchCountA, gpuResult.PatchCountA);
        Assert.Equal(cpuResult.PatchCountB, gpuResult.PatchCountB);
        // Both should produce a valid mesh
        Assert.True(cpuResult.Mesh.Faces.Count > 0);
        Assert.True(gpuResult.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void CubeCubeIntersection_GpuMatchesCpu()
    {
        if (_gpuClassifier == null) return;

        var a = CreateCube(Vec3.Zero);
        var b = CreateCube(new Vec3(0.5, 0.5, 0.5));

        var cpuResult = Csg.Intersect(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });

        var gpuResult = Csg.Intersect(a, b, new CsgOptions
        {
            ClassificationStrategy = _gpuClassifier
        });

        Assert.Equal(cpuResult.PatchCountA, gpuResult.PatchCountA);
        Assert.Equal(cpuResult.PatchCountB, gpuResult.PatchCountB);
        Assert.Equal(cpuResult.Mesh.Faces.Count, gpuResult.Mesh.Faces.Count);
    }

    [Fact]
    public void CubeCubeDifference_GpuMatchesCpu()
    {
        if (_gpuClassifier == null) return;

        var a = CreateCube(Vec3.Zero);
        var b = CreateCube(new Vec3(0.5, 0.5, 0.5));

        var cpuResult = Csg.Difference(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });

        var gpuResult = Csg.Difference(a, b, new CsgOptions
        {
            ClassificationStrategy = _gpuClassifier
        });

        Assert.Equal(cpuResult.PatchCountA, gpuResult.PatchCountA);
        Assert.Equal(cpuResult.PatchCountB, gpuResult.PatchCountB);
        Assert.Equal(cpuResult.Mesh.Faces.Count, gpuResult.Mesh.Faces.Count);
    }

    [Fact]
    public void CubeSphereUnion_GpuMatchesCpu()
    {
        if (_gpuClassifier == null) return;

        var a = CreateCube(Vec3.Zero);
        var b = CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2);

        var cpuResult = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });

        var gpuResult = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = _gpuClassifier
        });

        Assert.Equal(cpuResult.PatchCountA, gpuResult.PatchCountA);
        Assert.Equal(cpuResult.PatchCountB, gpuResult.PatchCountB);
        Assert.True(gpuResult.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void WindingNumber_GpuMatchesCpu()
    {
        if (_gpuClassifier == null) return;

        var a = CreateCube(Vec3.Zero);
        var b = CreateCube(new Vec3(0.5, 0.5, 0.5));

        var cpuResult = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy(),
            UseWindingNumber = true
        });

        var gpuResult = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = _gpuClassifier,
            UseWindingNumber = true
        });

        Assert.Equal(cpuResult.PatchCountA, gpuResult.PatchCountA);
        Assert.Equal(cpuResult.PatchCountB, gpuResult.PatchCountB);
        Assert.True(gpuResult.Mesh.Faces.Count > 0);
    }

    private static Solid CreateCube(Vec3 offset, double size = 1.0)
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0) * size + offset,
            new Vec3(1, 0, 0) * size + offset,
            new Vec3(1, 1, 0) * size + offset,
            new Vec3(0, 1, 0) * size + offset,
            new Vec3(0, 0, 1) * size + offset,
            new Vec3(1, 0, 1) * size + offset,
            new Vec3(1, 1, 1) * size + offset,
            new Vec3(0, 1, 1) * size + offset,
        };

        var triangles = new (int, int, int)[]
        {
            (0, 2, 1), (0, 3, 2),
            (4, 5, 6), (4, 6, 7),
            (0, 1, 5), (0, 5, 4),
            (2, 3, 7), (2, 7, 6),
            (0, 4, 7), (0, 7, 3),
            (1, 2, 6), (1, 6, 5),
        };

        return Solid.FromIndexed(positions, triangles);
    }

    private static Solid CreateSphere(Vec3 center, double radius, int subdivisions)
    {
        double phi = (1.0 + System.Math.Sqrt(5.0)) / 2.0;
        var verts = new List<Vec3>
        {
            new Vec3(-1, phi, 0).Normalized * radius + center,
            new Vec3(1, phi, 0).Normalized * radius + center,
            new Vec3(-1, -phi, 0).Normalized * radius + center,
            new Vec3(1, -phi, 0).Normalized * radius + center,
            new Vec3(0, -1, phi).Normalized * radius + center,
            new Vec3(0, 1, phi).Normalized * radius + center,
            new Vec3(0, -1, -phi).Normalized * radius + center,
            new Vec3(0, 1, -phi).Normalized * radius + center,
            new Vec3(phi, 0, -1).Normalized * radius + center,
            new Vec3(phi, 0, 1).Normalized * radius + center,
            new Vec3(-phi, 0, -1).Normalized * radius + center,
            new Vec3(-phi, 0, 1).Normalized * radius + center,
        };

        var faces = new List<(int, int, int)>
        {
            (0, 11, 5), (0, 5, 1), (0, 1, 7), (0, 7, 10), (0, 10, 11),
            (1, 5, 9), (5, 11, 4), (11, 10, 2), (10, 7, 6), (7, 1, 8),
            (3, 9, 4), (3, 4, 2), (3, 2, 6), (3, 6, 8), (3, 8, 9),
            (4, 9, 5), (2, 4, 11), (6, 2, 10), (8, 6, 7), (9, 8, 1),
        };

        for (int s = 0; s < subdivisions; s++)
        {
            var newFaces = new List<(int, int, int)>();
            var midpointCache = new Dictionary<(int, int), int>();

            int GetMidpoint(int a, int b)
            {
                var key = a < b ? (a, b) : (b, a);
                if (midpointCache.TryGetValue(key, out int idx))
                    return idx;
                var mid = ((verts[a] + verts[b]) * 0.5 - center).Normalized * radius + center;
                idx = verts.Count;
                verts.Add(mid);
                midpointCache[key] = idx;
                return idx;
            }

            foreach (var (a, b, c) in faces)
            {
                int ab = GetMidpoint(a, b);
                int bc = GetMidpoint(b, c);
                int ca = GetMidpoint(c, a);

                newFaces.Add((a, ab, ca));
                newFaces.Add((b, bc, ab));
                newFaces.Add((c, ca, bc));
                newFaces.Add((ab, bc, ca));
            }

            faces = newFaces;
        }

        return Solid.FromIndexed(verts, faces);
    }
}
