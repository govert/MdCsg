using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

public class PatchExtractionProvenanceTests
{
    [Fact]
    public void IntraFaceExtractor_AssignsIntersectionAuthorityAndStableIdentity()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);

        var subTriangles = new List<FaceCutter.SubTriangle>
        {
            new(a, b, c, 3, false, 0),
            new(b, d, c, 3, false, 0)
        };

        var patches = IntraFacePatchExtractor.Extract(subTriangles);
        Assert.Single(patches);

        var patch = patches[0];
        Assert.Equal(PatchBoundaryAuthority.IntersectionFlags, patch.BoundaryAuthority);
        Assert.Equal([3], patch.SourceFaceIndices);
        Assert.NotEqual(0UL, patch.StableId);
    }

    [Fact]
    public void GlobalExtractor_AssignsIntersectionAuthorityAndDeterministicStableIds()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(2, 0, 0);
        var e = new Vec3(3, 0, 0);
        var f = new Vec3(2, 1, 0);

        var subTriangles = new List<FaceCutter.SubTriangle>
        {
            new(a, b, c, 1, false, 0),
            new(d, e, f, 7, false, 0)
        };

        var adjacency = SubTriangleAdjacency.Build(subTriangles);
        var first = PatchExtractor.Extract(subTriangles, adjacency);
        var second = PatchExtractor.Extract(subTriangles, adjacency);

        Assert.Equal(first.Count, second.Count);
        for (int i = 0; i < first.Count; i++)
        {
            Assert.Equal(PatchBoundaryAuthority.IntersectionFlags, first[i].BoundaryAuthority);
            Assert.Equal(first[i].BoundaryAuthority, second[i].BoundaryAuthority);
            Assert.Equal(first[i].SourceFaceIndices, second[i].SourceFaceIndices);
            Assert.Equal(first[i].StableId, second[i].StableId);
            Assert.NotEqual(0UL, first[i].StableId);
        }
    }
}
