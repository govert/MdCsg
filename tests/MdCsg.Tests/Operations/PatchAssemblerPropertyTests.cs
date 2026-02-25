using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler - Selection logic for Union/Intersect/Difference, coplanar handling, normal flipping</summary>
public class PatchAssemblerPropertyTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int faceIdx = 0)
        => new(a, b, c, faceIdx, false, 0);

    private static Patch MakePatch(int id, int[] triIndices, bool isInside, bool hasConfident = true)
    {
        var p = new Patch(id);
        foreach (var i in triIndices) p.SubTriangleIndices.Add(i);
        p.IsInside = isInside;
        if (hasConfident) p.ConfidentPoint = Vec3.UnitX;
        return p;
    }

    private static Patch MakeCoplanarPatch(int id, int[] triIndices, bool normalsAgree)
    {
        var p = new Patch(id);
        foreach (var i in triIndices) p.SubTriangleIndices.Add(i);
        p.CoplanarNormalsAgree = normalsAgree;
        return p;
    }

    private static readonly FaceCutter.SubTriangle[] _subs =
    [
        MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
        MakeSub(Vec3.UnitX, new Vec3(2,0,0), Vec3.UnitY),
        MakeSub(Vec3.Zero, Vec3.UnitY, Vec3.UnitZ),
    ];

    [Fact]
    public void Union_KeepsOutsidePatchesFromA()
    {
        var patchesA = new[] { MakePatch(0, [0, 1], isInside: false) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Union_DiscardsInsidePatchesFromA()
    {
        var patchesA = new[] { MakePatch(0, [0], isInside: true) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Union_KeepsOutsidePatchesFromB()
    {
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakePatch(0, [0], isInside: false) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Union_DiscardsInsidePatchesFromB()
    {
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakePatch(0, [0], isInside: true) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Union_NoFlipFromB()
    {
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakePatch(0, [0], isInside: false) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void Intersect_KeepsInsidePatchesFromA()
    {
        var patchesA = new[] { MakePatch(0, [0], isInside: true) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Intersection);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Intersect_DiscardsOutsidePatchesFromA()
    {
        var patchesA = new[] { MakePatch(0, [0], isInside: false) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Intersection);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Intersect_KeepsInsidePatchesFromB()
    {
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakePatch(0, [0], isInside: true) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Intersection);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Intersect_NoFlipFromB()
    {
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakePatch(0, [0], isInside: true) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Intersection);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void Difference_KeepsOutsidePatchesFromA()
    {
        var patchesA = new[] { MakePatch(0, [0], isInside: false) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Difference);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Difference_DiscardsInsidePatchesFromA()
    {
        var patchesA = new[] { MakePatch(0, [0], isInside: true) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Difference_KeepsInsidePatchesFromB_WithFlip()
    {
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakePatch(0, [0], isInside: true) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Difference);
        Assert.Single(result.Triangles);
        Assert.True(result.FlipNormals[0]);
    }

    [Fact]
    public void Difference_DiscardsOutsidePatchesFromB()
    {
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakePatch(0, [0], isInside: false) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Difference_FlippedB_ReverseWinding()
    {
        var subB = new[] { MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)) };
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakePatch(0, [0], isInside: true) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, subB, CsgOperation.Difference);
        var tri = result.Triangles[0];
        Assert.Equal(new Vec3(0, 0, 0), tri.A);
        Assert.Equal(new Vec3(0, 1, 0), tri.B);
        Assert.Equal(new Vec3(1, 0, 0), tri.C);
    }

    [Fact]
    public void Union_CoplanarSameNormal_Kept()
    {
        var patchesA = new[] { MakeCoplanarPatch(0, [0], normalsAgree: true) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Union_CoplanarOppositeNormal_Discarded()
    {
        var patchesA = new[] { MakeCoplanarPatch(0, [0], normalsAgree: false) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Intersect_CoplanarSameNormal_Kept()
    {
        var patchesA = new[] { MakeCoplanarPatch(0, [0], normalsAgree: true) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Intersection);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Difference_CoplanarOppositeNormal_Kept()
    {
        var patchesA = new[] { MakeCoplanarPatch(0, [0], normalsAgree: false) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Difference);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Difference_CoplanarSameNormal_Discarded()
    {
        var patchesA = new[] { MakeCoplanarPatch(0, [0], normalsAgree: true) };
        var patchesB = Array.Empty<Patch>();
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarFromB_AlwaysDiscarded()
    {
        var patchesA = Array.Empty<Patch>();
        var patchesB = new[] { MakeCoplanarPatch(0, [0], normalsAgree: true) };
        var resultU = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        var resultI = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Intersection);
        var resultD = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Difference);
        Assert.Empty(resultU.Triangles);
        Assert.Empty(resultI.Triangles);
        Assert.Empty(resultD.Triangles);
    }

    [Fact]
    public void Assemble_MultiplePatches_SelectsCorrectly()
    {
        var patchesA = new[]
        {
            MakePatch(0, [0], isInside: false),
            MakePatch(1, [1], isInside: true),
        };
        var patchesB = new[]
        {
            MakePatch(0, [2], isInside: false),
            MakePatch(1, [0], isInside: true),
        };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Union);
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Assemble_EmptyPatches_EmptyResult()
    {
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), Array.Empty<Patch>(),
            _subs, _subs, CsgOperation.Union);
        Assert.Empty(result.Triangles);
        Assert.Empty(result.FlipNormals);
    }

    [Fact]
    public void Assemble_FlipNormals_MatchesTriangles()
    {
        var patchesA = new[] { MakePatch(0, [0, 1], isInside: false) };
        var patchesB = new[] { MakePatch(0, [0], isInside: true) };
        var result = PatchAssembler.Assemble(patchesA, patchesB, _subs, _subs, CsgOperation.Difference);
        Assert.Equal(result.Triangles.Count, result.FlipNormals.Count);
    }
}
