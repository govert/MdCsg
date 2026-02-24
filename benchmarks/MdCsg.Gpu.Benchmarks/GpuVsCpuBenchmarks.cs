using BenchmarkDotNet.Attributes;
using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Gpu;
using MdCsg.Math;

namespace MdCsg.Gpu.Benchmarks;

/// <summary>
/// Benchmarks comparing GPU vs CPU patch classification for CSG operations
/// at increasing mesh sizes (sphere subdivision levels 2-5).
/// </summary>
[MemoryDiagnoser]
public class GpuVsCpuBenchmarks : IDisposable
{
    private Solid _sphereA = null!;
    private Solid _sphereB = null!;
    private GpuPatchClassifier? _gpuClassifier;

    [Params(2, 3, 4, 5)]
    public int Subdivisions { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _sphereA = CreateSphere(Vec3.Zero, 1.0, Subdivisions);
        _sphereB = CreateSphere(new Vec3(0.5, 0, 0), 1.0, Subdivisions);
        _gpuClassifier = GpuAccelerator.TryCreate();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _gpuClassifier?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public CsgResult Cpu()
    {
        return Csg.Union(_sphereA, _sphereB, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });
    }

    [Benchmark]
    public CsgResult Gpu()
    {
        if (_gpuClassifier == null)
            throw new InvalidOperationException("No Vulkan device available for GPU benchmark.");

        return Csg.Union(_sphereA, _sphereB, new CsgOptions
        {
            ClassificationStrategy = _gpuClassifier
        });
    }

    public void Dispose()
    {
        _gpuClassifier?.Dispose();
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
