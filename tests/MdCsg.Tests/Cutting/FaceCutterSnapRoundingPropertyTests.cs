using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter — SubTriangle edge flags, CutFace with segments, SnapRounding — snap/merge</summary>
public class FaceCutterSnapRoundingPropertyTests
{
    [Fact]
    public void CutFace_NoSegments_ReturnsSingleTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 42, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(42, result[0].OriginalFaceIndex);
        Assert.False(result[0].HasIntersectionEdge);
        Assert.Equal(0, result[0].IntersectionEdgeFlags);
    }

    [Fact]
    public void CutFace_NoSegments_PreservesVertices()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var result = FaceCutter.CutFace(tri, 0, Array.Empty<IntersectionSegment>());
        Assert.Equal(tri.A, result[0].A);
        Assert.Equal(tri.B, result[0].B);
        Assert.Equal(tri.C, result[0].C);
    }

    [Fact]
    public void CutFace_OneSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        // Segment crossing from edge AB to edge AC
        var seg = new IntersectionSegment(new Vec3(2, 0, 0), new Vec3(0, 2, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2, $"Expected at least 2 sub-triangles, got {result.Count}");
    }

    [Fact]
    public void CutFace_OneSegment_AllOriginalFaceIndexPreserved()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(new Vec3(2, 0, 0), new Vec3(0, 2, 0), 5, 10);
        var result = FaceCutter.CutFace(tri, 5, new[] { seg });
        foreach (var st in result)
        {
            Assert.Equal(5, st.OriginalFaceIndex);
        }
    }

    [Fact]
    public void CutFace_OneSegment_SomeHaveIntersectionEdge()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(new Vec3(2, 0, 0), new Vec3(0, 2, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        bool anyHasIntersectionEdge = false;
        foreach (var st in result)
        {
            if (st.HasIntersectionEdge) anyHasIntersectionEdge = true;
        }
        Assert.True(anyHasIntersectionEdge, "At least one sub-triangle should have intersection edge");
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit0()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b001);
        Assert.True(st.IsEdgeIntersection(0));  // A-B
        Assert.False(st.IsEdgeIntersection(1)); // B-C
        Assert.False(st.IsEdgeIntersection(2)); // C-A
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit1()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b010);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit2()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b100);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_AllBits()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_NoBits_NoIntersectionEdges()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SnapRounding_Snap_RoundsToGrid()
    {
        var point = new Vec3(1.000000003, 2.000000007, 3.000000001);
        var snapped = SnapRounding.Snap(point);
        // Default grid size is 1e-8
        Assert.True(System.Math.Abs(snapped.X - 1.0) < 1e-7);
        Assert.True(System.Math.Abs(snapped.Y - 2.0) < 1e-7);
        Assert.True(System.Math.Abs(snapped.Z - 3.0) < 1e-7);
    }

    [Fact]
    public void SnapRounding_Snap_ExactValues_Unchanged()
    {
        var point = new Vec3(1.0, 2.0, 3.0);
        var snapped = SnapRounding.Snap(point);
        Assert.Equal(1.0, snapped.X);
        Assert.Equal(2.0, snapped.Y);
        Assert.Equal(3.0, snapped.Z);
    }

    [Fact]
    public void SnapRounding_SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(
            new Vec3(1.0000000001, 0, 0),
            new Vec3(0, 1.0000000001, 0),
            7, 13);
        var snapped = SnapRounding.SnapSegment(seg);
        Assert.Equal(7, snapped.FaceIndexA);
        Assert.Equal(13, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapRounding_MergePoints_IdenticalPoints_MergedToOne()
    {
        var points = new List<Vec3>
        {
            new Vec3(1, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Single(unique);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(0, mapping[1]);
        Assert.Equal(0, mapping[2]);
    }

    [Fact]
    public void SnapRounding_MergePoints_DistinctPoints_AllKept()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(10, 0, 0),
            new Vec3(0, 10, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(3, unique.Count);
        // Each maps to different index
        Assert.NotEqual(mapping[0], mapping[1]);
        Assert.NotEqual(mapping[1], mapping[2]);
    }

    [Fact]
    public void SnapRounding_MergePoints_NearPoints_MergedWithTolerance()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1e-9, 0, 0), // within default tolerance
            new Vec3(10, 0, 0)     // far away
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[1]); // merged
        Assert.NotEqual(mapping[0], mapping[2]); // distinct
    }

    [Fact]
    public void SnapRounding_MergePoints_CustomTolerance()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0.5, 0, 0) // within 1.0 tolerance
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance: 1.0);
        Assert.Single(unique);
    }

    [Fact]
    public void SnapRounding_MergePoints_EmptyInput_EmptyOutput()
    {
        var (unique, mapping) = SnapRounding.MergePoints(Array.Empty<Vec3>());
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void IntersectionSegment_Length_Correct()
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
    public void IntersectionSegment_IsDegenerate_LargeSegment_False()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 0, 0), 0, 1);
        Assert.False(seg.IsDegenerate);
    }
}
