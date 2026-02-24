using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.ScalingTests;

static class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"Process: {(Environment.Is64BitProcess ? "x64" : "x86")}");
        Console.WriteLine();

        RunEdgeCaseTests();
        Console.WriteLine();
        RunScalingTests();
    }

    // ========================================================================
    // Edge case tests — correctness under adversarial geometry
    // ========================================================================
    static void RunEdgeCaseTests()
    {
        Console.WriteLine("=== EDGE CASE TESTS ===");
        Console.WriteLine();

        int passed = 0, failed = 0;

        void Check(string name, Func<bool> test)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                bool ok = test();
                sw.Stop();
                if (ok)
                {
                    Console.WriteLine($"  PASS  {name} ({sw.ElapsedMilliseconds} ms)");
                    passed++;
                }
                else
                {
                    Console.WriteLine($"  FAIL  {name} ({sw.ElapsedMilliseconds} ms)");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR {name}: {ex.GetType().Name}: {ex.Message}");
                failed++;
            }
        }

        var opts = new CsgOptions { UseWindingNumber = true };

        // --- Disjoint objects (should be trivial) ---
        Check("Disjoint cubes: union volume = sum", () =>
        {
            var a = CreateCube(Vec3.Zero, 1);
            var b = CreateCube(new Vec3(10, 0, 0), 1);
            var result = Csg.Union(a, b, opts);
            double vol = Volume(result.Mesh);
            return System.Math.Abs(vol - 2.0) < 0.05;
        });

        Check("Disjoint cubes: intersection is empty", () =>
        {
            var a = CreateCube(Vec3.Zero, 1);
            var b = CreateCube(new Vec3(10, 0, 0), 1);
            var result = Csg.Intersect(a, b, opts);
            double vol = Volume(result.Mesh);
            return vol < 0.01;
        });

        // --- Fully contained ---
        Check("Contained cube: union = outer", () =>
        {
            var outer = CreateCube(new Vec3(-0.1, -0.1, -0.1), 1.2);
            var inner = CreateCube(new Vec3(0.1, 0.1, 0.1), 0.8);
            var result = Csg.Union(outer, inner, opts);
            double outerVol = Volume(outer.Mesh);
            double resultVol = Volume(result.Mesh);
            return System.Math.Abs(resultVol - outerVol) < 0.2;
        });

        Check("Contained cube: intersection = inner", () =>
        {
            var outer = CreateCube(new Vec3(-0.1, -0.1, -0.1), 1.2);
            var inner = CreateCube(new Vec3(0.1, 0.1, 0.1), 0.8);
            var result = Csg.Intersect(outer, inner, opts);
            double innerVol = Volume(inner.Mesh);
            double resultVol = Volume(result.Mesh);
            return System.Math.Abs(resultVol - innerVol) < 0.2;
        });

        // --- Nearly touching (gap < epsilon) ---
        Check("Nearly touching cubes: union volume ~ 2", () =>
        {
            var a = CreateCube(Vec3.Zero, 1);
            var b = CreateCube(new Vec3(1.0001, 0, 0), 1);
            var result = Csg.Union(a, b, opts);
            double vol = Volume(result.Mesh);
            return System.Math.Abs(vol - 2.0) < 0.05;
        });

        // --- Tiny overlap ---
        Check("Tiny overlap cubes: union volume reasonable", () =>
        {
            var a = CreateCube(Vec3.Zero, 1);
            var b = CreateCube(new Vec3(0.999, 0, 0), 1);
            var result = Csg.Union(a, b, opts);
            double vol = Volume(result.Mesh);
            // Expected ~ 2.0 - 0.001*1*1 = 1.999
            return vol > 1.9 && vol < 2.05;
        });

        // --- Large size ratio ---
        Check("Large cube minus tiny cube", () =>
        {
            var big = CreateCube(Vec3.Zero, 10);
            var tiny = CreateCube(new Vec3(4.5, 4.5, 4.5), 0.1);
            var result = Csg.Difference(big, tiny, opts);
            double bigVol = Volume(big.Mesh);
            double resultVol = Volume(result.Mesh);
            double expected = bigVol - 0.001; // 10^3 - 0.1^3
            return System.Math.Abs(resultVol - expected) < 1.0;
        });

        // --- Sphere-sphere intersection ---
        Check("Overlapping spheres: volume conservation", () =>
        {
            var a = CreateSphere(Vec3.Zero, 1.0, 2);
            var b = CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
            var union = Csg.Union(a, b, opts);
            var inter = Csg.Intersect(a, b, opts);
            double lhs = Volume(union.Mesh) + Volume(inter.Mesh);
            double rhs = Volume(a.Mesh) + Volume(b.Mesh);
            return System.Math.Abs(lhs - rhs) < 0.5;
        });

        // --- Cube-sphere intersection ---
        Check("Cube-sphere intersection bounded by sphere volume", () =>
        {
            var cube = CreateCube(Vec3.Zero, 2);
            var sphere = CreateSphere(new Vec3(1, 1, 1), 0.8, 2);
            var result = Csg.Intersect(cube, sphere, opts);
            double sphereVol = Volume(sphere.Mesh);
            double resultVol = Volume(result.Mesh);
            // Sphere is fully inside cube, so intersection = sphere
            return System.Math.Abs(resultVol - sphereVol) < 0.5;
        });

        // --- Sequential operations ---
        Check("Sequential: (A union B) difference C", () =>
        {
            var a = CreateCube(Vec3.Zero, 1);
            var b = CreateCube(new Vec3(0.5, 0, 0), 1);
            var c = CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5);
            var ab = Csg.Union(a, b, opts);
            var abSolid = new Solid(ab.Mesh);
            var result = Csg.Difference(abSolid, c, opts);
            double resultVol = Volume(result.Mesh);
            // AB volume ~ 1.5, minus ~0.125 cube
            return resultVol > 1.2 && resultVol < 1.6;
        });

        // --- Tetrahedron operations ---
        Check("Tetrahedron-cube intersection", () =>
        {
            var tet = CreateTetrahedron(Vec3.Zero, 0.5);
            var cube = CreateCube(Vec3.Zero, 1);
            var result = Csg.Intersect(tet, cube, opts);
            double tetVol = Volume(tet.Mesh);
            double resultVol = Volume(result.Mesh);
            // Tet is inside cube, so intersection = tet
            return System.Math.Abs(resultVol - tetVol) < 0.3;
        });

        // --- Commutativity of union on spheres ---
        Check("Sphere union commutativity", () =>
        {
            var a = CreateSphere(Vec3.Zero, 1.0, 2);
            var b = CreateSphere(new Vec3(0.7, 0.3, 0.2), 0.8, 2);
            var ab = Csg.Union(a, b, opts);
            var ba = Csg.Union(b, a, opts);
            double volAB = Volume(ab.Mesh);
            double volBA = Volume(ba.Mesh);
            return System.Math.Abs(volAB - volBA) < 0.3;
        });

        // --- De Morgan's law: A \ B = A intersect complement(B) ---
        // We can't directly compute complement, but we test that
        // Vol(A \ B) = Vol(A) - Vol(A intersect B)
        Check("De Morgan volume: Vol(A\\B) = Vol(A) - Vol(A&B)", () =>
        {
            var a = CreateCube(Vec3.Zero, 1);
            var b = CreateCube(new Vec3(0.5, 0.5, 0.5), 1);
            var diff = Csg.Difference(a, b, opts);
            var inter = Csg.Intersect(a, b, opts);
            double volA = Volume(a.Mesh);
            double volDiff = Volume(diff.Mesh);
            double volInter = Volume(inter.Mesh);
            return System.Math.Abs(volDiff - (volA - volInter)) < 0.2;
        });

        // --- High triangle count edge case ---
        Check("High-res sphere union (subdiv 3, 1280 tris each)", () =>
        {
            var a = CreateSphere(Vec3.Zero, 1.0, 3);
            var b = CreateSphere(new Vec3(0.8, 0, 0), 1.0, 3);
            var result = Csg.Union(a, b, opts);
            return result.FaceCount > 500;
        });

        Console.WriteLine();
        Console.WriteLine($"Results: {passed} passed, {failed} failed out of {passed + failed} tests");
    }

    // ========================================================================
    // Scaling tests — algorithmic complexity validation
    // ========================================================================
    static void RunScalingTests()
    {
        Console.WriteLine("=== SCALING TESTS ===");
        Console.WriteLine();
        Console.WriteLine("Testing CSG operations on progressively larger meshes to validate");
        Console.WriteLine("O(n log n + k) scaling behavior.");
        Console.WriteLine();

        var opts = new CsgOptions { UseWindingNumber = true };

        // --- Sphere-sphere scaling (geometric intersection growth) ---
        Console.WriteLine("--- Sphere-sphere union (varying subdivision) ---");
        Console.WriteLine($"{"Subdiv",6} {"Tris/mesh",10} {"TotalTris",10} {"IntSegs",8} {"Faces",8} {"Time ms",10} {"MB",6}");
        Console.WriteLine(new string('-', 66));

        for (int subdiv = 0; subdiv <= 7; subdiv++)
        {
            int expectedTris = 20 * (1 << (2 * subdiv));
            Console.Write($"{subdiv,6} {expectedTris,10}");
            Console.Out.Flush();

            var memBefore = GC.GetTotalMemory(true);
            var sw = Stopwatch.StartNew();

            var a = CreateSphere(Vec3.Zero, 1.0, subdiv);
            var b = CreateSphere(new Vec3(0.8, 0, 0), 1.0, subdiv);

            var buildMs = sw.ElapsedMilliseconds;

            var result = Csg.Union(a, b, opts);
            sw.Stop();

            var memAfter = GC.GetTotalMemory(false);
            double memMB = (memAfter - memBefore) / (1024.0 * 1024.0);

            Console.WriteLine($" {result.IntersectionSegmentCount + result.FaceCount,10} {result.IntersectionSegmentCount,8} {result.FaceCount,8} {sw.ElapsedMilliseconds,10} {memMB,6:F1}");

            if (sw.ElapsedMilliseconds > 120_000)
            {
                Console.WriteLine("  (exceeded 2 minutes, stopping)");
                break;
            }
        }

        Console.WriteLine();

        // --- Cube-sphere scaling (asymmetric intersection) ---
        Console.WriteLine("--- Cube minus sphere (varying sphere subdivision) ---");
        Console.WriteLine($"{"Subdiv",6} {"SphTris",10} {"IntSegs",8} {"Faces",8} {"Time ms",10}");
        Console.WriteLine(new string('-', 50));

        for (int subdiv = 0; subdiv <= 7; subdiv++)
        {
            int expectedTris = 20 * (1 << (2 * subdiv));
            Console.Write($"{subdiv,6} {expectedTris,10}");
            Console.Out.Flush();

            var sw = Stopwatch.StartNew();
            var cube = CreateCube(Vec3.Zero, 2.0);
            var sphere = CreateSphere(new Vec3(1, 1, 1), 0.8, subdiv);
            var result = Csg.Difference(cube, sphere, opts);
            sw.Stop();

            Console.WriteLine($" {result.IntersectionSegmentCount,8} {result.FaceCount,8} {sw.ElapsedMilliseconds,10}");

            if (sw.ElapsedMilliseconds > 120_000)
            {
                Console.WriteLine("  (exceeded 2 minutes, stopping)");
                break;
            }
        }

        Console.WriteLine();

        // --- Disjoint scaling (should be O(n log n), near-zero k) ---
        Console.WriteLine("--- Disjoint sphere pair (no intersections, BVH-only cost) ---");
        Console.WriteLine($"{"Subdiv",6} {"Tris/mesh",10} {"IntSegs",8} {"Time ms",10}");
        Console.WriteLine(new string('-', 42));

        for (int subdiv = 0; subdiv <= 7; subdiv++)
        {
            int expectedTris = 20 * (1 << (2 * subdiv));
            Console.Write($"{subdiv,6} {expectedTris,10}");
            Console.Out.Flush();

            var sw = Stopwatch.StartNew();
            var a = CreateSphere(Vec3.Zero, 1.0, subdiv);
            var b = CreateSphere(new Vec3(10, 0, 0), 1.0, subdiv);
            var result = Csg.Union(a, b, opts);
            sw.Stop();

            Console.WriteLine($" {result.IntersectionSegmentCount,8} {sw.ElapsedMilliseconds,10}");

            if (sw.ElapsedMilliseconds > 120_000)
            {
                Console.WriteLine("  (exceeded 2 minutes, stopping)");
                break;
            }
        }

        Console.WriteLine();

        // --- Multi-object chain: union of N cubes along a line ---
        Console.WriteLine("--- Chain of overlapping cubes (sequential union) ---");
        Console.WriteLine($"{"NCubes",6} {"TotalTris",10} {"Faces",8} {"Time ms",10}");
        Console.WriteLine(new string('-', 42));

        for (int nCubes = 2; nCubes <= 64; nCubes *= 2)
        {
            Console.Write($"{nCubes,6}");
            Console.Out.Flush();

            var sw = Stopwatch.StartNew();
            Solid current = CreateCube(Vec3.Zero, 1.0);
            for (int i = 1; i < nCubes; i++)
            {
                var next = CreateCube(new Vec3(i * 0.5, 0, 0), 1.0);
                var csgResult = Csg.Union(current, next, opts);
                current = new Solid(csgResult.Mesh);
            }
            sw.Stop();

            Console.WriteLine($" {current.Mesh.Faces.Count * 2,10} {current.Mesh.Faces.Count,8} {sw.ElapsedMilliseconds,10}");

            if (sw.ElapsedMilliseconds > 120_000)
            {
                Console.WriteLine("  (exceeded 2 minutes, stopping)");
                break;
            }
        }

        Console.WriteLine();

        // --- Sphere grid: dense intersection pattern ---
        Console.WriteLine("--- Grid of spheres union (NxNxN, subdiv 1) ---");
        Console.WriteLine($"{"N",6} {"Spheres",8} {"Faces",8} {"Time ms",10}");
        Console.WriteLine(new string('-', 40));

        for (int n = 1; n <= 6; n++)
        {
            int total = n * n * n;
            Console.Write($"{n,6} {total,8}");
            Console.Out.Flush();

            var sw = Stopwatch.StartNew();
            Solid? current = null;
            for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
            for (int z = 0; z < n; z++)
            {
                var sphere = CreateSphere(new Vec3(x * 1.5, y * 1.5, z * 1.5), 1.0, 1);
                if (current == null)
                {
                    current = sphere;
                }
                else
                {
                    var csgResult = Csg.Union(current, sphere, opts);
                    current = new Solid(csgResult.Mesh);
                }
            }
            sw.Stop();

            Console.WriteLine($" {current!.Mesh.Faces.Count,8} {sw.ElapsedMilliseconds,10}");

            if (sw.ElapsedMilliseconds > 120_000)
            {
                Console.WriteLine("  (exceeded 2 minutes, stopping)");
                break;
            }
        }
    }

    // ========================================================================
    // Mesh construction helpers
    // ========================================================================

    static Solid CreateCube(Vec3 origin, double size)
    {
        var o = origin;
        var s = size;
        Vec3[] v =
        {
            new(o.X, o.Y, o.Z),           new(o.X+s, o.Y, o.Z),
            new(o.X+s, o.Y+s, o.Z),       new(o.X, o.Y+s, o.Z),
            new(o.X, o.Y, o.Z+s),         new(o.X+s, o.Y, o.Z+s),
            new(o.X+s, o.Y+s, o.Z+s),     new(o.X, o.Y+s, o.Z+s),
        };
        var tris = new Triangle3[]
        {
            new(v[0], v[2], v[1]), new(v[0], v[3], v[2]),
            new(v[4], v[5], v[6]), new(v[4], v[6], v[7]),
            new(v[0], v[1], v[5]), new(v[0], v[5], v[4]),
            new(v[3], v[6], v[2]), new(v[3], v[7], v[6]),
            new(v[0], v[4], v[7]), new(v[0], v[7], v[3]),
            new(v[1], v[2], v[6]), new(v[1], v[6], v[5]),
        };
        return Solid.FromTriangles(tris);
    }

    static Solid CreateTetrahedron(Vec3 offset, double size)
    {
        var positions = new Vec3[]
        {
            new Vec3(1, 1, 1) * size + offset,
            new Vec3(1, -1, -1) * size + offset,
            new Vec3(-1, 1, -1) * size + offset,
            new Vec3(-1, -1, 1) * size + offset,
        };
        var triangles = new (int, int, int)[] { (0,1,2), (0,3,1), (0,2,3), (1,3,2) };
        return Solid.FromIndexed(positions, triangles);
    }

    static Solid CreateSphere(Vec3 center, double radius, int subdivisions)
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
            (0,11,5),(0,5,1),(0,1,7),(0,7,10),(0,10,11),
            (1,5,9),(5,11,4),(11,10,2),(10,7,6),(7,1,8),
            (3,9,4),(3,4,2),(3,2,6),(3,6,8),(3,8,9),
            (4,9,5),(2,4,11),(6,2,10),(8,6,7),(9,8,1),
        };
        for (int s = 0; s < subdivisions; s++)
        {
            var newFaces = new List<(int, int, int)>();
            var midCache = new Dictionary<(int, int), int>();
            int GetMid(int a, int b)
            {
                var key = a < b ? (a, b) : (b, a);
                if (midCache.TryGetValue(key, out int idx)) return idx;
                var mid = ((verts[a] + verts[b]) * 0.5 - center).Normalized * radius + center;
                idx = verts.Count;
                verts.Add(mid);
                midCache[key] = idx;
                return idx;
            }
            foreach (var (a, b, c) in faces)
            {
                int ab = GetMid(a, b), bc = GetMid(b, c), ca = GetMid(c, a);
                newFaces.Add((a, ab, ca));
                newFaces.Add((b, bc, ab));
                newFaces.Add((c, ca, bc));
                newFaces.Add((ab, bc, ca));
            }
            faces = newFaces;
        }
        return Solid.FromIndexed(verts, faces);
    }

    static double Volume(HalfEdgeMesh mesh)
    {
        double vol = 0;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            if (verts.Count < 3) continue;
            var a = verts[0].Position;
            var b = verts[1].Position;
            var c = verts[2].Position;
            vol += Vec3.Dot(a, Vec3.Cross(b, c)) / 6.0;
        }
        return System.Math.Abs(vol);
    }
}
