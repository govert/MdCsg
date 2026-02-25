using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: SubTriangle record tests — edge flags, dedup, CutFace area conservation</summary>
public class SubTrianglePropertyTests
{
    [Fact]
    public void SubTriangle_Properties_Preserved()
    {
        var st = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 42, true, 0b101);
        Assert.Equal(new Vec3(0, 0, 0), st.A);
        Assert.Equal(new Vec3(1, 0, 0), st.B);
        Assert.Equal(new Vec3(0, 1, 0), st.C);
        Assert.Equal(42, st.OriginalFaceIndex);
        Assert.True(st.HasIntersectionEdge);
        Assert.Equal(0b101, st.IntersectionEdgeFlags);
    }

    [Fact]
    public void IsEdgeIntersection_Bit0_AB()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, true, 0b001);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void IsEdgeIntersection_Bit1_BC()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, true, 0b010);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void IsEdgeIntersection_Bit2_CA()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, true, 0b100);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void IsEdgeIntersection_AllBits()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void IsEdgeIntersection_NoBits()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, false, 0);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_RecordEquality()
    {
        var a = new FaceCutter.SubTriangle(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9), 0, false);
        var b = new FaceCutter.SubTriangle(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9), 0, false);
        Assert.Equal(a, b);
    }

    [Fact]
    public void CutFace_NoSegments_PreservesTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 5, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(tri.A, result[0].A);
        Assert.Equal(tri.B, result[0].B);
        Assert.Equal(tri.C, result[0].C);
        Assert.Equal(5, result[0].OriginalFaceIndex);
        Assert.False(result[0].HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_SingleSegment_ProducesMultiple()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void CutFace_OriginalFaceIndex_AllPreserved()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 42, new[] { seg });
        Assert.All(result, st => Assert.Equal(42, st.OriginalFaceIndex));
    }

    [Fact]
    public void CutFace_HasIntersectionEdgeFlagged()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.Contains(result, st => st.HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_AreaConserved()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });

        double originalArea = tri.Area;
        double totalArea = result.Sum(st => new Triangle3(st.A, st.B, st.C).Area);
        Assert.True(System.Math.Abs(originalArea - totalArea) < 1e-6,
            $"Area mismatch: original={originalArea}, sum={totalArea}");
    }

    [Fact]
    public void CutFace_DegenerateSegment_StillProduces()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(1, 0.5, 0), new Vec3(1, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 1);
    }
}
