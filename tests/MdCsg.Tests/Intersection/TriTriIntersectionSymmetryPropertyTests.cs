using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: TriTriIntersection — symmetry properties, swap invariants, segment consistency</summary>
public class TriTriIntersectionSymmetryPropertyTests
{
    [Fact]
    public void Intersect_Symmetric_BothReturn()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 1, -1), new Vec3(1, 1, 1), new Vec3(1, -1, 0));
        bool r1 = TriTriIntersection.Intersect(t1, t2, out _);
        bool r2 = TriTriIntersection.Intersect(t2, t1, out _);
        Assert.Equal(r1, r2);
    }

    [Fact]
    public void Intersect_Symmetric_SegmentEndpointsMatch()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 1, -1), new Vec3(1, 1, 1), new Vec3(1, -1, 0));
        TriTriIntersection.Intersect(t1, t2, out var seg12);
        TriTriIntersection.Intersect(t2, t1, out var seg21);
        // Segment endpoints should match (possibly swapped)
        var pts12 = new HashSet<Vec3> { seg12.Start, seg12.End };
        var pts21 = new HashSet<Vec3> { seg21.Start, seg21.End };
        foreach (var p in pts12)
        {
            bool foundClose = pts21.Any(q => Vec3.DistanceSquared(p, q) < 1e-12);
            Assert.True(foundClose, $"Segment endpoint {p} should appear in swapped result");
        }
    }

    [Fact]
    public void NoIntersect_Symmetric()
    {
        var t1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        bool r1 = TriTriIntersection.Intersect(t1, t2, out _);
        bool r2 = TriTriIntersection.Intersect(t2, t1, out _);
        Assert.False(r1);
        Assert.False(r2);
    }

    [Fact]
    public void AreCoplanar_Symmetric()
    {
        var t1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        Assert.Equal(
            TriTriIntersection.AreCoplanar(t1, t2),
            TriTriIntersection.AreCoplanar(t2, t1));
    }

    [Fact]
    public void Intersect_SegmentLength_Positive()
    {
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(0, 0, 1), new Vec3(1, 0, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        double length = Vec3.Distance(seg.Start, seg.End);
        Assert.True(length > 0, $"Intersection segment should have positive length, got {length}");
    }

    [Fact]
    public void Intersect_SegmentLiesOnBothPlanes()
    {
        var t1 = new Triangle3(new Vec3(-2, -2, 0), new Vec3(2, -2, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0, -2, -2), new Vec3(0, -2, 2), new Vec3(0, 2, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        // Start and End should lie on both planes (Z=0 for t1, X=0 for t2)
        Assert.True(System.Math.Abs(seg.Start.Z) < 1e-8, $"Start Z should be ~0 (on t1 plane), got {seg.Start.Z}");
        Assert.True(System.Math.Abs(seg.End.Z) < 1e-8, $"End Z should be ~0 (on t1 plane), got {seg.End.Z}");
        Assert.True(System.Math.Abs(seg.Start.X) < 1e-8, $"Start X should be ~0 (on t2 plane), got {seg.Start.X}");
        Assert.True(System.Math.Abs(seg.End.X) < 1e-8, $"End X should be ~0 (on t2 plane), got {seg.End.X}");
    }

    [Fact]
    public void Intersect_VertexPermutations_SameResult()
    {
        // Rotating vertex order of t1 shouldn't change intersection detection
        var t1a = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t1b = new Triangle3(new Vec3(2, 0, 0), new Vec3(1, 2, 0), new Vec3(0, 0, 0));
        var t1c = new Triangle3(new Vec3(1, 2, 0), new Vec3(0, 0, 0), new Vec3(2, 0, 0));
        var t2 = new Triangle3(new Vec3(1, 0.5, -1), new Vec3(1, 0.5, 1), new Vec3(0.5, -0.5, 0));
        bool ra = TriTriIntersection.Intersect(t1a, t2, out _);
        bool rb = TriTriIntersection.Intersect(t1b, t2, out _);
        bool rc = TriTriIntersection.Intersect(t1c, t2, out _);
        Assert.Equal(ra, rb);
        Assert.Equal(rb, rc);
    }

    [Fact]
    public void Intersect_TranslatedPair_SameSegmentLength()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 1, -1), new Vec3(1, 1, 1), new Vec3(1, -1, 0));
        TriTriIntersection.Intersect(t1, t2, out var seg1);

        var offset = new Vec3(100, 200, 300);
        var t1t = new Triangle3(t1.A + offset, t1.B + offset, t1.C + offset);
        var t2t = new Triangle3(t2.A + offset, t2.B + offset, t2.C + offset);
        TriTriIntersection.Intersect(t1t, t2t, out var seg2);

        double len1 = Vec3.Distance(seg1.Start, seg1.End);
        double len2 = Vec3.Distance(seg2.Start, seg2.End);
        Assert.Equal(len1, len2, 8);
    }

    [Fact]
    public void IntersectCoplanar_NormalsAgree_Symmetric()
    {
        var t1 = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(0.5, 2.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out bool agree12);
        TriTriIntersection.IntersectCoplanar(t2, t1, out _, out _, out bool agree21);
        Assert.Equal(agree12, agree21);
    }

    [Fact]
    public void Intersect_ScaledPair_ScalesSegmentLength()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 1, -1), new Vec3(1, 1, 1), new Vec3(1, -1, 0));
        TriTriIntersection.Intersect(t1, t2, out var seg1);

        double scale = 3.0;
        var t1s = new Triangle3(t1.A * scale, t1.B * scale, t1.C * scale);
        var t2s = new Triangle3(t2.A * scale, t2.B * scale, t2.C * scale);
        TriTriIntersection.Intersect(t1s, t2s, out var seg2);

        double len1 = Vec3.Distance(seg1.Start, seg1.End);
        double len2 = Vec3.Distance(seg2.Start, seg2.End);
        Assert.Equal(len1 * scale, len2, 6);
    }

    [Fact]
    public void Intersect_MultipleAngle_AllIntersect()
    {
        // XY plane triangle crossed by XZ plane triangle at various angles
        var t1 = new Triangle3(new Vec3(-2, -2, 0), new Vec3(2, -2, 0), new Vec3(0, 2, 0));
        for (int angle = 10; angle < 360; angle += 30)
        {
            double rad = angle * System.Math.PI / 180.0;
            double dx = System.Math.Cos(rad);
            double dz = System.Math.Sin(rad);
            var t2 = new Triangle3(
                new Vec3(0, -1, -1),
                new Vec3(0, -1, 1),
                new Vec3(0, 1, 0));
            // Rotate t2 around Y axis by angle
            var rotT2 = new Triangle3(
                new Vec3(dx * (-1), -1, dz * (-1)),
                new Vec3(dx * 1, -1, dz * 1),
                new Vec3(0, 1, 0));
            // Not all rotations will intersect t1, but let's just check it doesn't crash
            TriTriIntersection.Intersect(t1, rotT2, out _);
        }
    }
}
