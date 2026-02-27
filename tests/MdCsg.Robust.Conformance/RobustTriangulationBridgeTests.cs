using MdCsg.Math;
using MdCsg.Robust.Kernel.Triangulation;

namespace MdCsg.Robust.Conformance;

public class RobustTriangulationBridgeTests
{
    [Fact]
    public void SingleTriangle_PassesThrough()
    {
        var triangulator = new RobustConstrainedTriangulator();
        var verts = new[]
        {
            Vec3.Zero,
            Vec3.UnitX,
            Vec3.UnitY
        };

        var result = triangulator.Triangulate(verts, Array.Empty<(int Start, int End)>(), Vec3.UnitZ);

        Assert.True(result.UsedLegacyKernel);
        Assert.Equal(0, result.DroppedDegenerateTriangleCount);
        Assert.Single(result.Triangles);
        Assert.Equal((0, 1, 2), result.Triangles[0]);
    }

    [Fact]
    public void RepeatedRuns_AreDeterministic()
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
        var baselineFingerprint = Fingerprint(baseline.Triangles);

        for (int i = 0; i < 5; i++)
        {
            var next = triangulator.Triangulate(verts, constraints, Vec3.UnitZ);
            Assert.Equal(baselineFingerprint, Fingerprint(next.Triangles));
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
}
