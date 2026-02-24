using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding edge cases</summary>
public class SnapRoundingEdgeCaseTests
{
    [Fact]
    public void Snap_Origin_NoChange()
    {
        var p = SnapRounding.Snap(Vec3.Zero);
        Assert.Equal(0, p.X);
        Assert.Equal(0, p.Y);
        Assert.Equal(0, p.Z);
    }

    [Fact]
    public void Snap_IntegerCoords_NoChange()
    {
        var p = SnapRounding.Snap(new Vec3(1, 2, 3));
        Assert.Equal(1, p.X, 10);
        Assert.Equal(2, p.Y, 10);
        Assert.Equal(3, p.Z, 10);
    }

    [Fact]
    public void Snap_SmallPerturbation_SnapsToGrid()
    {
        double gridSize = 0.1;
        var p = SnapRounding.Snap(new Vec3(1.03, 2.07, 3.01), gridSize);
        Assert.Equal(1.0, p.X, 5);
        Assert.Equal(2.1, p.Y, 5);
        Assert.Equal(3.0, p.Z, 5);
    }

    [Fact]
    public void Snap_NegativeCoords()
    {
        var p = SnapRounding.Snap(new Vec3(-1.5, -2.5, -3.5), 1.0);
        Assert.Equal(-2.0, p.X, 5);
        Assert.Equal(-2.0, p.Y, 5); // round to nearest: -2.5 → -2 or -3 depends on rounding
    }

    [Fact]
    public void Snap_CustomGridSize()
    {
        var p = SnapRounding.Snap(new Vec3(0.123, 0.456, 0.789), 0.5);
        Assert.Equal(0.0, p.X, 5);
        Assert.Equal(0.5, p.Y, 5);
        Assert.Equal(1.0, p.Z, 5);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.1001, 0.2001, 0.3001),
            new Vec3(0.4001, 0.5001, 0.6001),
            42, 99);
        var snapped = SnapRounding.SnapSegment(seg);
        Assert.Equal(42, snapped.FaceIndexA);
        Assert.Equal(99, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapSegment_SnapsEndpoints()
    {
        double grid = 0.1;
        var seg = new IntersectionSegment(
            new Vec3(0.04, 0.04, 0.04),
            new Vec3(1.06, 1.06, 1.06),
            0, 1);
        var snapped = SnapRounding.SnapSegment(seg, grid);
        Assert.Equal(0.0, snapped.Start.X, 5);
        Assert.Equal(1.1, snapped.End.X, 5);
    }

    [Fact]
    public void MergePoints_AllDistinct()
    {
        var pts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(pts);
        Assert.Equal(3, unique.Count);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(1, mapping[1]);
        Assert.Equal(2, mapping[2]);
    }

    [Fact]
    public void MergePoints_AllIdentical()
    {
        var pts = new List<Vec3>
        {
            new(1, 1, 1), new(1, 1, 1), new(1, 1, 1)
        };
        var (unique, mapping) = SnapRounding.MergePoints(pts);
        Assert.Single(unique);
        Assert.Equal(mapping[0], mapping[1]);
        Assert.Equal(mapping[0], mapping[2]);
    }

    [Fact]
    public void MergePoints_WithinTolerance_Merged()
    {
        var pts = new List<Vec3>
        {
            new(0, 0, 0),
            new(1e-12, 1e-12, 1e-12), // within default tolerance
            new(1, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(pts);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
        Assert.NotEqual(mapping[0], mapping[2]);
    }

    [Fact]
    public void MergePoints_JustOutsideTolerance_Separate()
    {
        double tol = 0.01;
        var pts = new List<Vec3>
        {
            new(0, 0, 0),
            new(0.02, 0, 0) // outside tolerance
        };
        var (unique, mapping) = SnapRounding.MergePoints(pts, tol);
        Assert.Equal(2, unique.Count);
    }

    [Fact]
    public void MergePoints_Empty()
    {
        var pts = new List<Vec3>();
        var (unique, mapping) = SnapRounding.MergePoints(pts);
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void MergePoints_SinglePoint()
    {
        var pts = new List<Vec3> { new(5, 5, 5) };
        var (unique, mapping) = SnapRounding.MergePoints(pts);
        Assert.Single(unique);
        Assert.Equal(0, mapping[0]);
    }

    [Fact]
    public void MergePoints_ChainMerge()
    {
        // A is near B, B is near C, but A is not near C
        double tol = 0.1;
        var pts = new List<Vec3>
        {
            new(0, 0, 0),       // A
            new(0.05, 0, 0),    // B near A
            new(0.12, 0, 0)     // C near B but NOT near A
        };
        var (unique, mapping) = SnapRounding.MergePoints(pts, tol);
        // A and B merge (B is within 0.05 < 0.1 of A)
        // C: checks against A (dist 0.12 > 0.1) → separate
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
    }

    [Fact]
    public void MergePoints_MappingCoversAllIndices()
    {
        var pts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
            new(0, 0, 0.001), new(1, 0, 0.001)
        };
        var (unique, mapping) = SnapRounding.MergePoints(pts, 0.01);
        for (int i = 0; i < pts.Count; i++)
        {
            Assert.True(mapping.ContainsKey(i), $"Missing mapping for index {i}");
            Assert.True(mapping[i] >= 0 && mapping[i] < unique.Count);
        }
    }

    [Fact]
    public void IntersectionSegment_Length()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(3, 4, 0), 0, 1);
        Assert.Equal(5.0, seg.Length, 10);
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_ZeroLength()
    {
        var seg = new IntersectionSegment(new Vec3(1, 2, 3), new Vec3(1, 2, 3), 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_IsNotDegenerate()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 0, 0), 0, 1);
        Assert.False(seg.IsDegenerate);
    }
}
