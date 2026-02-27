using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Predicates;
using MdCsg.Robust.Kernel.Predicates;

namespace MdCsg.Robust.Validation;

internal static class DegenerateFaceInspector
{
    public static int CountDegenerateFaces(HalfEdgeMesh mesh, PredicateTelemetryCounter telemetry)
    {
        int degenerateCount = 0;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var result = EvaluateProjectedAreaSign(a, b, c);
            telemetry.Add(result.Tier);
            if (result.Sign == PredicateSign.Zero)
                degenerateCount++;
        }

        return degenerateCount;
    }

    private static CertifiedPredicateResult EvaluateProjectedAreaSign(Vec3 a, Vec3 b, Vec3 c)
    {
        Vec3 n = Vec3.Cross(b - a, c - a);
        double ax = System.Math.Abs(n.X);
        double ay = System.Math.Abs(n.Y);
        double az = System.Math.Abs(n.Z);

        Vec2 pa;
        Vec2 pb;
        Vec2 pc;

        // Project onto the most stable axis-aligned 2D plane.
        if (ax >= ay && ax >= az)
        {
            pa = new Vec2(a.Y, a.Z);
            pb = new Vec2(b.Y, b.Z);
            pc = new Vec2(c.Y, c.Z);
        }
        else if (ay >= az)
        {
            pa = new Vec2(a.X, a.Z);
            pb = new Vec2(b.X, b.Z);
            pc = new Vec2(c.X, c.Z);
        }
        else
        {
            pa = new Vec2(a.X, a.Y);
            pb = new Vec2(b.X, b.Y);
            pc = new Vec2(c.X, c.Y);
        }

        return CertifiedPredicates.Orient2D(pa, pb, pc);
    }
}
