using MdCsg.Math;
using MdCsg.Robust.Kernel.Triangulation;

namespace MdCsg.Robust.Conformance;

public class RobustTriangulationBridgeTests
{
    [Fact]
    public void SingleTriangle_Unconstrained_UsesNativePath()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var verts = new[]
        {
            Vec3.Zero,
            Vec3.UnitX,
            Vec3.UnitY
        };

        var result = triangulator.Triangulate(verts, Array.Empty<(int Start, int End)>(), Vec3.UnitZ);

        Assert.False(result.UsedLegacyKernel);
        Assert.Equal(0, result.DroppedDegenerateTriangleCount);
        Assert.Single(result.Triangles);
        Assert.Equal((0, 1, 2), result.Triangles[0]);
    }

    [Fact]
    public void UnconstrainedConcavePolygon_UsesNativePath_AndIsDeterministic()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var verts = new[]
        {
            new Vec3(0, 0, 0),
            new Vec3(3, 0, 0),
            new Vec3(3, 2, 0),
            new Vec3(1.5, 1, 0),
            new Vec3(0, 2, 0)
        };

        var baseline = triangulator.Triangulate(verts, Array.Empty<(int Start, int End)>(), Vec3.UnitZ);
        Assert.False(baseline.UsedLegacyKernel);
        Assert.Equal(3, baseline.Triangles.Count);
        var baselineFingerprint = Fingerprint(baseline.Triangles);

        for (int i = 0; i < 5; i++)
        {
            var next = triangulator.Triangulate(verts, Array.Empty<(int Start, int End)>(), Vec3.UnitZ);
            Assert.False(next.UsedLegacyKernel);
            Assert.Equal(baselineFingerprint, Fingerprint(next.Triangles));
        }
    }

    [Fact]
    public void ConstrainedSimpleDiagonal_UsesNativePath_AndPreservesConstraint()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var verts = new[]
        {
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(2, 2, 0),
            new Vec3(0, 2, 0)
        };
        var constraints = new (int Start, int End)[]
        {
            (0, 2)
        };

        var result = triangulator.Triangulate(verts, constraints, Vec3.UnitZ);

        Assert.False(result.UsedLegacyKernel);
        Assert.Equal(2, result.Triangles.Count);
        Assert.Contains(result.Triangles, t => HasEdge(t, 0, 2));
    }

    [Fact]
    public void CrossingConstraints_FailClosedByDefault()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var verts = new[]
        {
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(2, 2, 0),
            new Vec3(0, 2, 0)
        };
        var constraints = new (int Start, int End)[]
        {
            (0, 2),
            (1, 3)
        };

        var result = triangulator.Triangulate(verts, constraints, Vec3.UnitZ);

        Assert.False(result.Succeeded);
        Assert.False(result.UsedLegacyKernel);
        Assert.Equal(RobustTriangulationFallbackReason.InvalidOrCrossingConstraints, result.FailureReason);
        Assert.Equal(RobustTriangulationFailureStage.ConstraintValidation, result.FailureStage);
        Assert.Equal("constraint-validation/invalid-or-crossing", result.FailureCode);
        Assert.NotNull(result.FailureSignature);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CrossingConstraints_CanUseLegacyFallback_WhenExplicitlyEnabled()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var verts = new[]
        {
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(2, 2, 0),
            new Vec3(0, 2, 0)
        };
        var constraints = new (int Start, int End)[]
        {
            (0, 2),
            (1, 3)
        };

        var result = triangulator.Triangulate(
            verts,
            constraints,
            Vec3.UnitZ,
            new RobustTriangulationOptions { AllowLegacyFallback = true });

        Assert.True(result.Succeeded);
        Assert.True(result.UsedLegacyKernel);
        Assert.Equal(RobustTriangulationFallbackReason.InvalidOrCrossingConstraints, result.LegacyFallbackReason);
        Assert.NotNull(result.LegacyFallbackSignature);
        Assert.NotEmpty(result.Triangles);
    }

    [Fact]
    public void ConstrainedInput_RepeatedRuns_AreDeterministic()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var verts = new[]
        {
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(2, 2, 0),
            new Vec3(1, 3, 0),
            new Vec3(0, 2, 0)
        };
        var constraints = new (int Start, int End)[]
        {
            (0, 2),
            (2, 4)
        };

        var baseline = triangulator.Triangulate(verts, constraints, Vec3.UnitZ);
        Assert.False(baseline.UsedLegacyKernel);
        var baselineFingerprint = Fingerprint(baseline.Triangles);

        for (int i = 0; i < 5; i++)
        {
            var next = triangulator.Triangulate(verts, constraints, Vec3.UnitZ);
            Assert.False(next.UsedLegacyKernel);
            Assert.Equal(baselineFingerprint, Fingerprint(next.Triangles));
        }
    }

    [Fact]
    public void SeededNonCrossingConstraintCorpus_UsesNativePath()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var rng = new Random(20260227);

        for (int caseIdx = 0; caseIdx < 64; caseIdx++)
        {
            int vertexCount = rng.Next(5, 11);
            var verts = CreateConvexPolygon(vertexCount, radiusBase: 3.0, rng);
            var constraints = CreateFanConstraintSubset(vertexCount, rng);

            var result = triangulator.Triangulate(verts, constraints, Vec3.UnitZ);

            Assert.False(result.UsedLegacyKernel);
            foreach (var (start, end) in constraints)
                Assert.Contains(result.Triangles, t => HasEdge(t, start, end));
        }
    }

    [Fact]
    public void DegenerateTolerance_CanDropAllTriangles()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var verts = new[]
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0)
        };

        var result = triangulator.Triangulate(
            verts,
            Array.Empty<(int Start, int End)>(),
            Vec3.UnitZ,
            new RobustTriangulationOptions
            {
                DegenerateAreaTolerance = 10.0,
                DropDegenerateTriangles = true
            });

        Assert.Empty(result.Triangles);
        Assert.True(result.DroppedDegenerateTriangleCount > 0);
    }

    private static string Fingerprint(IReadOnlyList<(int A, int B, int C)> triangles)
        => string.Join("|", triangles.Select(t => $"{t.A},{t.B},{t.C}"));

    private static bool HasEdge((int A, int B, int C) tri, int start, int end)
        => (tri.A == start && tri.B == end) || (tri.B == start && tri.A == end)
            || (tri.B == start && tri.C == end) || (tri.C == start && tri.B == end)
            || (tri.C == start && tri.A == end) || (tri.A == start && tri.C == end);

    private static Vec3[] CreateConvexPolygon(int count, double radiusBase, Random rng)
    {
        var verts = new Vec3[count];
        for (int i = 0; i < count; i++)
        {
            double angle = i * (2.0 * System.Math.PI / count);
            double jitter = 0.75 + 0.35 * rng.NextDouble();
            double r = radiusBase * jitter;
            verts[i] = new Vec3(r * System.Math.Cos(angle), r * System.Math.Sin(angle), 0);
        }

        return verts;
    }

    private static (int Start, int End)[] CreateFanConstraintSubset(int vertexCount, Random rng)
    {
        var constraints = new List<(int Start, int End)>();
        for (int i = 2; i <= vertexCount - 2; i++)
        {
            if (rng.NextDouble() < 0.65)
                constraints.Add((0, i));
        }

        if (constraints.Count == 0)
            constraints.Add((0, vertexCount / 2));

        return constraints.ToArray();
    }
}
