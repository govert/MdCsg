using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler logic tests — selection rules for each CSG operation</summary>
public class PatchAssemblerLogicTests
{
    private static int _nextPatchId;

    private static Patch MakePatch(bool isInside, params int[] subTriIndices)
    {
        var patch = new Patch(_nextPatchId++);
        patch.SubTriangleIndices.AddRange(subTriIndices);
        patch.IsInside = isInside;
        patch.HasConfidentPoint = true;
        return patch;
    }

    private static Patch MakeCoplanarPatch(bool normalsAgree, params int[] subTriIndices)
    {
        var patch = new Patch(_nextPatchId++);
        patch.SubTriangleIndices.AddRange(subTriIndices);
        patch.CoplanarNormalsAgree = normalsAgree;
        patch.HasConfidentPoint = false;
        return patch;
    }

    private static FaceCutter.SubTriangle MakeSubTri(int originalFace = 0)
    {
        return new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            originalFace, false);
    }

    // --- Union: keep A-outside, keep B-outside ---

    [Fact]
    public void Union_KeepsA_Outside()
    {
        var patchesA = new[] { MakePatch(false, 0) }; // outside B
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Union);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Union_DiscardsA_Inside()
    {
        var patchesA = new[] { MakePatch(true, 0) }; // inside B
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Union_KeepsB_Outside()
    {
        var patchesB = new[] { MakePatch(false, 0) }; // outside A
        var subTrisB = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble([], patchesB, [], subTrisB, CsgOperation.Union);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Union_DiscardsB_Inside()
    {
        var patchesB = new[] { MakePatch(true, 0) }; // inside A
        var subTrisB = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble([], patchesB, [], subTrisB, CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    // --- Intersection: keep A-inside, keep B-inside ---

    [Fact]
    public void Intersection_KeepsA_Inside()
    {
        var patchesA = new[] { MakePatch(true, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Intersection);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Intersection_DiscardsA_Outside()
    {
        var patchesA = new[] { MakePatch(false, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Intersection);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Intersection_KeepsB_Inside()
    {
        var patchesB = new[] { MakePatch(true, 0) };
        var subTrisB = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble([], patchesB, [], subTrisB, CsgOperation.Intersection);
        Assert.Single(result.Triangles);
    }

    // --- Difference: keep A-outside, keep B-inside (flipped) ---

    [Fact]
    public void Difference_KeepsA_Outside()
    {
        var patchesA = new[] { MakePatch(false, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Difference);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Difference_DiscardsA_Inside()
    {
        var patchesA = new[] { MakePatch(true, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Difference_KeepsB_Inside_FlipsNormals()
    {
        var patchesB = new[] { MakePatch(true, 0) };
        var subTrisB = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble([], patchesB, [], subTrisB, CsgOperation.Difference);
        Assert.Single(result.Triangles);
        Assert.True(result.FlipNormals[0], "B inside patches should flip normals for Difference");
    }

    [Fact]
    public void Difference_DiscardsB_Outside()
    {
        var patchesB = new[] { MakePatch(false, 0) };
        var subTrisB = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble([], patchesB, [], subTrisB, CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    // --- Coplanar patches ---

    [Fact]
    public void Coplanar_A_SameNormal_Union_Kept()
    {
        var patchesA = new[] { MakeCoplanarPatch(true, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Union);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Coplanar_A_OppositeNormal_Union_Discarded()
    {
        var patchesA = new[] { MakeCoplanarPatch(false, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Coplanar_A_SameNormal_Intersection_Kept()
    {
        var patchesA = new[] { MakeCoplanarPatch(true, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Intersection);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Coplanar_A_SameNormal_Difference_Discarded()
    {
        var patchesA = new[] { MakeCoplanarPatch(true, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Coplanar_A_OppositeNormal_Difference_Kept()
    {
        var patchesA = new[] { MakeCoplanarPatch(false, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Difference);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void Coplanar_B_AlwaysDiscarded()
    {
        var patchesB = new[] { MakeCoplanarPatch(true, 0) };
        var subTrisB = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble([], patchesB, [], subTrisB, CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    // --- Multiple patches ---

    [Fact]
    public void MultiplePatches_MixedClassification()
    {
        var patchesA = new[]
        {
            MakePatch(false, 0), // outside → kept for Union
            MakePatch(true, 1)   // inside → discarded for Union
        };
        var subTrisA = new[] { MakeSubTri(0), MakeSubTri(1) };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Union);
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void MultipleSubTrianglesPerPatch()
    {
        var patchesA = new[] { MakePatch(false, 0, 1, 2) };
        var subTrisA = new[] { MakeSubTri(0), MakeSubTri(0), MakeSubTri(0) };
        var result = PatchAssembler.Assemble(patchesA, [], subTrisA, [], CsgOperation.Union);
        Assert.Equal(3, result.Triangles.Count);
    }

    [Fact]
    public void AssemblyResult_FlipNormals_AllFalse_ForUnion()
    {
        var patchesA = new[] { MakePatch(false, 0) };
        var patchesB = new[] { MakePatch(false, 0) };
        var subTrisA = new[] { MakeSubTri() };
        var subTrisB = new[] { MakeSubTri() };
        var result = PatchAssembler.Assemble(patchesA, patchesB, subTrisA, subTrisB, CsgOperation.Union);
        Assert.Equal(2, result.Triangles.Count);
        Assert.All(result.FlipNormals, flip => Assert.False(flip));
    }
}
