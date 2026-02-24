using BenchmarkDotNet.Attributes;
using MdCsg.Api;
using MdCsg.Arithmetic;
using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Benchmarks;

[MemoryDiagnoser]
public class CsgBenchmarks
{
    private Solid _cubeA = null!;
    private Solid _cubeB = null!;
    private Solid _sphere = null!;
    private readonly CsgOptions _options = new() { UseWindingNumber = true };

    [GlobalSetup]
    public void Setup()
    {
        _cubeA = CreateCubeSolid(Vec3.Zero, 1.0);
        _cubeB = CreateCubeSolid(new Vec3(0.5, 0.5, 0.5), 1.0);
        _sphere = CreateSphereSolid(new Vec3(0.5, 0.5, 0.5), 0.6, 2);
    }

    [Benchmark]
    public CsgResult CubeUnion() => Csg.Union(_cubeA, _cubeB, _options);

    [Benchmark]
    public CsgResult CubeIntersection() => Csg.Intersect(_cubeA, _cubeB, _options);

    [Benchmark]
    public CsgResult CubeDifference() => Csg.Difference(_cubeA, _cubeB, _options);

    [Benchmark]
    public CsgResult CubeSphereIntersection() => Csg.Intersect(_cubeA, _sphere, _options);

    [Benchmark]
    public BvhTree BvhBuild() => BvhTree.Build(_cubeA.Mesh);

    [Benchmark]
    public HalfEdgeMesh MeshBuild()
    {
        var triangles = CreateCubeTriangles(Vec3.Zero, 1.0);
        var builder = new MeshBuilder();
        return builder.Build(triangles);
    }

    [Benchmark]
    public int ExpansionArithmetic_TwoProduct()
    {
        int sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            var (p, e) = ExpansionArithmetic.TwoProduct(1.0 + i * 1e-10, 2.0 + i * 1e-10);
            sum += p > 0 ? 1 : 0;
        }
        return sum;
    }

    private static Solid CreateCubeSolid(Vec3 origin, double size)
    {
        var triangles = CreateCubeTriangles(origin, size);
        return Solid.FromTriangles(triangles);
    }

    private static Triangle3[] CreateCubeTriangles(Vec3 origin, double size)
    {
        var o = origin;
        var s = size;
        Vec3[] v =
        {
            new(o.X, o.Y, o.Z),         new(o.X+s, o.Y, o.Z),
            new(o.X+s, o.Y+s, o.Z),     new(o.X, o.Y+s, o.Z),
            new(o.X, o.Y, o.Z+s),       new(o.X+s, o.Y, o.Z+s),
            new(o.X+s, o.Y+s, o.Z+s),   new(o.X, o.Y+s, o.Z+s),
        };
        return new[]
        {
            // -Z
            new Triangle3(v[0], v[2], v[1]), new Triangle3(v[0], v[3], v[2]),
            // +Z
            new Triangle3(v[4], v[5], v[6]), new Triangle3(v[4], v[6], v[7]),
            // -Y
            new Triangle3(v[0], v[1], v[5]), new Triangle3(v[0], v[5], v[4]),
            // +Y
            new Triangle3(v[3], v[6], v[2]), new Triangle3(v[3], v[7], v[6]),
            // -X
            new Triangle3(v[0], v[4], v[7]), new Triangle3(v[0], v[7], v[3]),
            // +X
            new Triangle3(v[1], v[2], v[6]), new Triangle3(v[1], v[6], v[5]),
        };
    }

    private static Solid CreateSphereSolid(Vec3 center, double radius, int subdivisions)
    {
        // Icosphere
        double t = (1.0 + System.Math.Sqrt(5.0)) / 2.0;
        var baseVerts = new Vec3[]
        {
            new Vec3(-1,  t,  0).Normalized * radius + center,
            new Vec3( 1,  t,  0).Normalized * radius + center,
            new Vec3(-1, -t,  0).Normalized * radius + center,
            new Vec3( 1, -t,  0).Normalized * radius + center,
            new Vec3( 0, -1,  t).Normalized * radius + center,
            new Vec3( 0,  1,  t).Normalized * radius + center,
            new Vec3( 0, -1, -t).Normalized * radius + center,
            new Vec3( 0,  1, -t).Normalized * radius + center,
            new Vec3( t,  0, -1).Normalized * radius + center,
            new Vec3( t,  0,  1).Normalized * radius + center,
            new Vec3(-t,  0, -1).Normalized * radius + center,
            new Vec3(-t,  0,  1).Normalized * radius + center,
        };

        var triangles = new List<Triangle3>
        {
            new(baseVerts[0], baseVerts[11], baseVerts[5]),
            new(baseVerts[0], baseVerts[5], baseVerts[1]),
            new(baseVerts[0], baseVerts[1], baseVerts[7]),
            new(baseVerts[0], baseVerts[7], baseVerts[10]),
            new(baseVerts[0], baseVerts[10], baseVerts[11]),
            new(baseVerts[1], baseVerts[5], baseVerts[9]),
            new(baseVerts[5], baseVerts[11], baseVerts[4]),
            new(baseVerts[11], baseVerts[10], baseVerts[2]),
            new(baseVerts[10], baseVerts[7], baseVerts[6]),
            new(baseVerts[7], baseVerts[1], baseVerts[8]),
            new(baseVerts[3], baseVerts[9], baseVerts[4]),
            new(baseVerts[3], baseVerts[4], baseVerts[2]),
            new(baseVerts[3], baseVerts[2], baseVerts[6]),
            new(baseVerts[3], baseVerts[6], baseVerts[8]),
            new(baseVerts[3], baseVerts[8], baseVerts[9]),
            new(baseVerts[4], baseVerts[9], baseVerts[5]),
            new(baseVerts[2], baseVerts[4], baseVerts[11]),
            new(baseVerts[6], baseVerts[2], baseVerts[10]),
            new(baseVerts[8], baseVerts[6], baseVerts[7]),
            new(baseVerts[9], baseVerts[8], baseVerts[1]),
        };

        for (int s = 0; s < subdivisions; s++)
        {
            var refined = new List<Triangle3>();
            foreach (var tri in triangles)
            {
                var m01 = ((tri.A + tri.B) / 2.0 - center).Normalized * radius + center;
                var m12 = ((tri.B + tri.C) / 2.0 - center).Normalized * radius + center;
                var m20 = ((tri.C + tri.A) / 2.0 - center).Normalized * radius + center;

                refined.Add(new Triangle3(tri.A, m01, m20));
                refined.Add(new Triangle3(tri.B, m12, m01));
                refined.Add(new Triangle3(tri.C, m20, m12));
                refined.Add(new Triangle3(m01, m12, m20));
            }
            triangles = refined;
        }

        return Solid.FromTriangles(triangles);
    }
}
