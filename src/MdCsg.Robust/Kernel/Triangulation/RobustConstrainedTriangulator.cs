using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Robust.Kernel.Triangulation;

/// <summary>
/// Transitional robust triangulator: delegates to the current constrained
/// triangulator and normalizes output for deterministic/validated consumption.
/// </summary>
public sealed class RobustConstrainedTriangulator : IRobustConstrainedTriangulator
{
    public RobustTriangulationResult Triangulate(
        IReadOnlyList<Vec3> vertices3D,
        IReadOnlyList<(int Start, int End)> constraints,
        Vec3 faceNormal,
        RobustTriangulationOptions? options = null)
    {
        var opts = options ?? RobustTriangulationOptions.Default;
        if (vertices3D.Count < 3)
            return new RobustTriangulationResult([], 0, UsedLegacyKernel: true);

        var rawTriangles = ConstrainedTriangulator.Triangulate(vertices3D, constraints, faceNormal);
        var normalizedTriangles = new List<(int A, int B, int C)>(rawTriangles.Count);
        int droppedDegenerate = 0;
        double tol = System.Math.Max(0, opts.DegenerateAreaTolerance);
        double tolSq = tol * tol;
        bool hasNormal = faceNormal.LengthSquared > 0;

        foreach (var tri in rawTriangles)
        {
            if (!HasValidIndices(tri, vertices3D.Count) || HasRepeatedVertex(tri))
            {
                droppedDegenerate++;
                continue;
            }

            var a = vertices3D[tri.A];
            var b = vertices3D[tri.B];
            var c = vertices3D[tri.C];
            var cross = Vec3.Cross(b - a, c - a);
            var triAreaSq = cross.LengthSquared;

            if (triAreaSq <= tolSq)
            {
                if (opts.DropDegenerateTriangles)
                {
                    droppedDegenerate++;
                    continue;
                }

                normalizedTriangles.Add(tri);
                continue;
            }

            var oriented = tri;
            if (hasNormal && Vec3.Dot(cross, faceNormal) < 0)
                oriented = (tri.A, tri.C, tri.B);

            normalizedTriangles.Add(oriented);
        }

        if (opts.DeterministicOrdering)
        {
            normalizedTriangles.Sort(static (x, y) =>
                CompareTriangle(CanonicalizeForSort(x), CanonicalizeForSort(y)));
        }

        return new RobustTriangulationResult(
            normalizedTriangles,
            droppedDegenerate,
            UsedLegacyKernel: true);
    }

    private static bool HasValidIndices((int A, int B, int C) tri, int vertexCount)
        => tri.A >= 0 && tri.B >= 0 && tri.C >= 0
            && tri.A < vertexCount && tri.B < vertexCount && tri.C < vertexCount;

    private static bool HasRepeatedVertex((int A, int B, int C) tri)
        => tri.A == tri.B || tri.B == tri.C || tri.C == tri.A;

    private static (int A, int B, int C) CanonicalizeForSort((int A, int B, int C) tri)
    {
        if (tri.A <= tri.B && tri.A <= tri.C)
            return tri;
        if (tri.B <= tri.A && tri.B <= tri.C)
            return (tri.B, tri.C, tri.A);
        return (tri.C, tri.A, tri.B);
    }

    private static int CompareTriangle((int A, int B, int C) a, (int A, int B, int C) b)
    {
        int cmpA = a.A.CompareTo(b.A);
        if (cmpA != 0) return cmpA;
        int cmpB = a.B.CompareTo(b.B);
        if (cmpB != 0) return cmpB;
        return a.C.CompareTo(b.C);
    }
}
