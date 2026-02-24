using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

public class PatchExtractorTests
{
    [Fact]
    public void SingleTriangle_OnePatach()
    {
        var subTris = new FaceCutter.SubTriangle[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
        };

        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Single(patches);
        Assert.Single(patches[0].SubTriangleIndices);
    }

    [Fact]
    public void TwoAdjacentTriangles_NoIntersection_OnePatch()
    {
        var subTris = new FaceCutter.SubTriangle[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, false),
        };

        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void TwoAdjacentTriangles_WithIntersectionEdge_TwoPatches()
    {
        // These share an edge, but it's an intersection edge
        var subTris = new FaceCutter.SubTriangle[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, true),
        };

        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void DisjointTriangles_SeparatePatches()
    {
        var subTris = new FaceCutter.SubTriangle[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5), 1, false),
        };

        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void ThreeTriangles_IntersectionSplitsInTwo()
    {
        // Three triangles in a row: T0-T1-T2
        // T0-T1 share a non-intersection edge
        // T1-T2 share an intersection edge
        // Result: patch {T0, T1} and patch {T2}
        var subTris = new FaceCutter.SubTriangle[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 1, 0), 0, true),
        };

        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);

        // T0 and T1 should be in one patch, T2 in another
        Assert.Equal(2, patches.Count);
        var bigPatch = patches.First(p => p.SubTriangleIndices.Count == 2);
        var smallPatch = patches.First(p => p.SubTriangleIndices.Count == 1);
        Assert.Contains(0, bigPatch.SubTriangleIndices);
        Assert.Contains(1, bigPatch.SubTriangleIndices);
        Assert.Contains(2, smallPatch.SubTriangleIndices);
    }
}
