using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler — selection rules for Union, Intersection, Difference, coplanar handling, normal flipping</summary>
public class PatchAssemblerSelectionRulePropertyTests
{
    private static FaceCutter.SubTriangle MakeSubTri(int origFace = 0) =>
        new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, origFace, false);

    private static int _nextId;

    private static Patch MakePatch(int subTriIdx, bool isInside)
    {
        var patch = new Patch(_nextId++);
        patch.SubTriangleIndices.Add(subTriIdx);
        patch.IsInside = isInside;
        patch.HasConfidentPoint = true;
        return patch;
    }

    private static Patch MakeCoplanarPatch(int subTriIdx, bool normalsAgree)
    {
        var patch = new Patch(_nextId++);
        patch.SubTriangleIndices.Add(subTriIdx);
        patch.CoplanarNormalsAgree = normalsAgree;
        patch.HasConfidentPoint = false;
        return patch;
    }

    [Fact]
    public void Union_OutsideA_IsKept()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakePatch(0, isInside: false);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void Union_InsideA_IsDiscarded()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakePatch(0, isInside: true);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Union_OutsideB_IsKept()
    {
        var subTris = new[] { MakeSubTri() };
        var patchB = MakePatch(0, isInside: false);
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB },
            Array.Empty<FaceCutter.SubTriangle>(), subTris,
            CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void Union_InsideB_IsDiscarded()
    {
        var subTris = new[] { MakeSubTri() };
        var patchB = MakePatch(0, isInside: true);
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB },
            Array.Empty<FaceCutter.SubTriangle>(), subTris,
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Intersection_InsideA_IsKept()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakePatch(0, isInside: true);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Intersection);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void Intersection_OutsideA_IsDiscarded()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakePatch(0, isInside: false);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Intersection);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Intersection_InsideB_IsKept()
    {
        var subTris = new[] { MakeSubTri() };
        var patchB = MakePatch(0, isInside: true);
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB },
            Array.Empty<FaceCutter.SubTriangle>(), subTris,
            CsgOperation.Intersection);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void Difference_OutsideA_IsKept()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakePatch(0, isInside: false);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void Difference_InsideA_IsDiscarded()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakePatch(0, isInside: true);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Difference_InsideB_IsKeptWithFlippedNormals()
    {
        var subTris = new[] { new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false) };
        var patchB = MakePatch(0, isInside: true);
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB },
            Array.Empty<FaceCutter.SubTriangle>(), subTris,
            CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count);
        Assert.True(result.FlipNormals[0], "B inside patches should have flipped normals in difference");
        // Winding should be reversed: A,C,B instead of A,B,C
        Assert.Equal(new Vec3(0, 0, 0), result.Triangles[0].A);
        Assert.Equal(new Vec3(0, 1, 0), result.Triangles[0].B);
        Assert.Equal(new Vec3(1, 0, 0), result.Triangles[0].C);
    }

    [Fact]
    public void Difference_OutsideB_IsDiscarded()
    {
        var subTris = new[] { MakeSubTri() };
        var patchB = MakePatch(0, isInside: false);
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB },
            Array.Empty<FaceCutter.SubTriangle>(), subTris,
            CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Union_NormalsFromB_NotFlipped()
    {
        var subTris = new[] { MakeSubTri() };
        var patchB = MakePatch(0, isInside: false);
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB },
            Array.Empty<FaceCutter.SubTriangle>(), subTris,
            CsgOperation.Union);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void Intersection_NormalsFromB_NotFlipped()
    {
        var subTris = new[] { MakeSubTri() };
        var patchB = MakePatch(0, isInside: true);
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB },
            Array.Empty<FaceCutter.SubTriangle>(), subTris,
            CsgOperation.Intersection);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void Coplanar_SameNormals_Union_Kept()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakeCoplanarPatch(0, normalsAgree: true);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void Coplanar_OppositeNormals_Union_Discarded()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakeCoplanarPatch(0, normalsAgree: false);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Coplanar_SameNormals_Intersection_Kept()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakeCoplanarPatch(0, normalsAgree: true);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Intersection);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void Coplanar_SameNormals_Difference_Discarded()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakeCoplanarPatch(0, normalsAgree: true);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Difference);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void Coplanar_OppositeNormals_Difference_Kept()
    {
        var subTris = new[] { MakeSubTri() };
        var patchA = MakeCoplanarPatch(0, normalsAgree: false);
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(),
            subTris, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void CoplanarB_AlwaysDiscarded()
    {
        var subTris = new[] { MakeSubTri() };
        var coplanarB = new Patch(_nextId++);
        coplanarB.SubTriangleIndices.Add(0);
        coplanarB.CoplanarNormalsAgree = true;
        coplanarB.HasConfidentPoint = false;

        foreach (var op in new[] { CsgOperation.Union, CsgOperation.Intersection, CsgOperation.Difference })
        {
            var result = PatchAssembler.Assemble(
                Array.Empty<Patch>(), new[] { coplanarB },
                Array.Empty<FaceCutter.SubTriangle>(), subTris,
                op);
            Assert.Empty(result.Triangles);
        }
    }

    [Fact]
    public void MultiplePatches_MixedSelection()
    {
        var subTrisA = new[] { MakeSubTri(0), MakeSubTri(1) };
        var patchInside = MakePatch(0, isInside: true);
        var patchOutside = MakePatch(1, isInside: false);
        var result = PatchAssembler.Assemble(
            new[] { patchInside, patchOutside }, Array.Empty<Patch>(),
            subTrisA, Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        // Union keeps outside: only patch at index 1 should be kept
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void EmptyPatches_EmptyResult()
    {
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), Array.Empty<Patch>(),
            Array.Empty<FaceCutter.SubTriangle>(), Array.Empty<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
        Assert.Empty(result.FlipNormals);
    }
}
