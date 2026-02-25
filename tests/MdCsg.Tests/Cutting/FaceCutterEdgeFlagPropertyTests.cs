using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter SubTriangle edge flag — bit operations, IsEdgeIntersection, record properties</summary>
public class FaceCutterEdgeFlagPropertyTests
{
    [Fact]
    public void NoFlags_AllEdgesNonIntersection()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        Assert.False(sub.IsEdgeIntersection(0));
        Assert.False(sub.IsEdgeIntersection(1));
        Assert.False(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void EdgeAB_Only()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001);
        Assert.True(sub.IsEdgeIntersection(0));
        Assert.False(sub.IsEdgeIntersection(1));
        Assert.False(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void EdgeBC_Only()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b010);
        Assert.False(sub.IsEdgeIntersection(0));
        Assert.True(sub.IsEdgeIntersection(1));
        Assert.False(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void EdgeCA_Only()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b100);
        Assert.False(sub.IsEdgeIntersection(0));
        Assert.False(sub.IsEdgeIntersection(1));
        Assert.True(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void AllEdges_AllIntersection()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b111);
        Assert.True(sub.IsEdgeIntersection(0));
        Assert.True(sub.IsEdgeIntersection(1));
        Assert.True(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void TwoEdges_ABandCA()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b101);
        Assert.True(sub.IsEdgeIntersection(0));
        Assert.False(sub.IsEdgeIntersection(1));
        Assert.True(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void OriginalFaceIndex_Preserved()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 42, false, 0);
        Assert.Equal(42, sub.OriginalFaceIndex);
    }

    [Fact]
    public void HasIntersectionEdge_True_WhenFlags()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001);
        Assert.True(sub.HasIntersectionEdge);
    }

    [Fact]
    public void HasIntersectionEdge_False_WhenNoFlags()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        Assert.False(sub.HasIntersectionEdge);
    }

    [Fact]
    public void VertexPositions_Preserved()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var sub = new FaceCutter.SubTriangle(a, b, c, 0, false, 0);
        Assert.Equal(a, sub.A);
        Assert.Equal(b, sub.B);
        Assert.Equal(c, sub.C);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        var b = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordInequality_DifferentFlags()
    {
        var a = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001);
        var b = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b010);
        Assert.NotEqual(a, b);
    }
}
