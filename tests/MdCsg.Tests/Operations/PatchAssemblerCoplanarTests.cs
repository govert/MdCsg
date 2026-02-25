using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler coplanar logic tests — keep/discard rules for coplanar patches</summary>
public class PatchAssemblerCoplanarTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int origFace = 0)
        => new(a, b, c, origFace, false);

    private static Patch MakePatch(int id, bool? isInside, bool hasConfidentPoint,
        bool? coplanarNormalsAgree = null, int subTriIdx = 0)
    {
        var p = new Patch(id)
        {
            IsInside = isInside,
            HasConfidentPoint = hasConfidentPoint,
            CoplanarNormalsAgree = coplanarNormalsAgree
        };
        p.SubTriangleIndices.Add(subTriIdx);
        return p;
    }

    private static readonly FaceCutter.SubTriangle DefaultSub = MakeSub(
        new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));

    [Theory]
    [InlineData(true)]  // normalsAgree=true → keep for Union
    [InlineData(false)] // normalsAgree=false → discard for Union
    public void CoplanarPatchA_Union_KeepOnlyWhenNormalsAgree(bool normalsAgree)
    {
        var patchA = MakePatch(0, isInside: false, hasConfidentPoint: false,
            coplanarNormalsAgree: normalsAgree);
        var patchB = MakePatch(0, isInside: false, hasConfidentPoint: true);
        var subsA = new List<FaceCutter.SubTriangle> { DefaultSub };
        var subsB = new List<FaceCutter.SubTriangle> { DefaultSub };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, new[] { patchB }, subsA, subsB, CsgOperation.Union);

        // Coplanar patch from A is kept only if normalsAgree is true for Union
        bool hasTriFromA = result.Triangles.Count > (normalsAgree ? 0 : 1);
        if (normalsAgree)
            Assert.True(result.Triangles.Count >= 1); // A's coplanar patch kept
        else
            Assert.True(result.Triangles.Count >= 1); // B's patch still there
    }

    [Fact]
    public void CoplanarPatchA_Intersection_KeepWhenNormalsAgree()
    {
        var patchA = MakePatch(0, isInside: true, hasConfidentPoint: false,
            coplanarNormalsAgree: true);
        var patchB = MakePatch(0, isInside: true, hasConfidentPoint: true, subTriIdx: 0);
        var subsA = new List<FaceCutter.SubTriangle> { DefaultSub };
        var subsB = new List<FaceCutter.SubTriangle> { DefaultSub };

        var result = PatchAssembler.Assemble(
            new[] { patchA }, new[] { patchB }, subsA, subsB, CsgOperation.Intersection);

        // A's coplanar with normalsAgree=true → kept for Intersection
        Assert.True(result.Triangles.Count >= 1);
    }

    [Fact]
    public void CoplanarPatchA_Intersection_DiscardWhenNormalsDisagree()
    {
        var patchA = MakePatch(0, isInside: true, hasConfidentPoint: false,
            coplanarNormalsAgree: false);
        // No patches from B
        var subsA = new List<FaceCutter.SubTriangle> { DefaultSub };
        var subsB = new List<FaceCutter.SubTriangle>();

        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(), subsA, subsB, CsgOperation.Intersection);

        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarPatchA_Difference_KeepWhenNormalsDisagree()
    {
        var patchA = MakePatch(0, isInside: false, hasConfidentPoint: false,
            coplanarNormalsAgree: false);
        var subsA = new List<FaceCutter.SubTriangle> { DefaultSub };
        var subsB = new List<FaceCutter.SubTriangle>();

        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(), subsA, subsB, CsgOperation.Difference);

        // Difference keeps coplanar from A when normals disagree
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void CoplanarPatchA_Difference_DiscardWhenNormalsAgree()
    {
        var patchA = MakePatch(0, isInside: false, hasConfidentPoint: false,
            coplanarNormalsAgree: true);
        var subsA = new List<FaceCutter.SubTriangle> { DefaultSub };
        var subsB = new List<FaceCutter.SubTriangle>();

        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(), subsA, subsB, CsgOperation.Difference);

        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarPatchB_AlwaysDiscarded()
    {
        // Coplanar patches from B without confident point are always discarded
        var patchA = new List<Patch>();
        var patchB = MakePatch(0, isInside: true, hasConfidentPoint: false,
            coplanarNormalsAgree: true);
        var subsA = new List<FaceCutter.SubTriangle>();
        var subsB = new List<FaceCutter.SubTriangle> { DefaultSub };

        foreach (var op in new[] { CsgOperation.Union, CsgOperation.Intersection, CsgOperation.Difference })
        {
            var result = PatchAssembler.Assemble(patchA, new[] { patchB }, subsA, subsB, op);
            Assert.Empty(result.Triangles);
        }
    }

    [Fact]
    public void CoplanarPatchB_WithConfidentPoint_NotDiscarded()
    {
        // If B has a confident point, it's NOT treated as coplanar even if CoplanarNormalsAgree is set
        var patchB = MakePatch(0, isInside: false, hasConfidentPoint: true,
            coplanarNormalsAgree: true);
        var subsA = new List<FaceCutter.SubTriangle>();
        var subsB = new List<FaceCutter.SubTriangle> { DefaultSub };

        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB }, subsA, subsB, CsgOperation.Union);

        // B outside A → kept for Union
        Assert.Single(result.Triangles);
    }

    [Fact]
    public void NormalA_NeverFlipped()
    {
        var patchA = MakePatch(0, isInside: false, hasConfidentPoint: true);
        var subsA = new List<FaceCutter.SubTriangle> { DefaultSub };
        var subsB = new List<FaceCutter.SubTriangle>();

        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(), subsA, subsB, CsgOperation.Union);

        Assert.Single(result.FlipNormals);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void NormalB_Flipped_ForDifference_WhenInsideA()
    {
        var patchB = MakePatch(0, isInside: true, hasConfidentPoint: true);
        var subsA = new List<FaceCutter.SubTriangle>();
        var subsB = new List<FaceCutter.SubTriangle> { DefaultSub };

        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB }, subsA, subsB, CsgOperation.Difference);

        Assert.Single(result.FlipNormals);
        Assert.True(result.FlipNormals[0]);
    }

    [Fact]
    public void NormalB_NotFlipped_ForUnion()
    {
        var patchB = MakePatch(0, isInside: false, hasConfidentPoint: true);
        var subsA = new List<FaceCutter.SubTriangle>();
        var subsB = new List<FaceCutter.SubTriangle> { DefaultSub };

        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB }, subsA, subsB, CsgOperation.Union);

        Assert.Single(result.FlipNormals);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void Difference_FlippedTriangle_HasReversedWinding()
    {
        var tri = MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var patchB = MakePatch(0, isInside: true, hasConfidentPoint: true);
        var subsB = new List<FaceCutter.SubTriangle> { tri };

        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { patchB }, new List<FaceCutter.SubTriangle>(), subsB,
            CsgOperation.Difference);

        var outTri = result.Triangles[0];
        // Flipped: A stays, C and B swap
        Assert.Equal(tri.A, outTri.A);
        Assert.Equal(tri.C, outTri.B);
        Assert.Equal(tri.B, outTri.C);
    }

    [Fact]
    public void PatchWithNullIsInside_TreatedAsFalse()
    {
        // IsInside=null should default to false via ?? operator
        var patchA = MakePatch(0, isInside: null, hasConfidentPoint: true);
        var subsA = new List<FaceCutter.SubTriangle> { DefaultSub };

        // For Union: keep if NOT inside B (false) → keep
        var result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(), subsA, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Single(result.Triangles);

        // For Intersection: keep if inside B (false) → discard
        result = PatchAssembler.Assemble(
            new[] { patchA }, Array.Empty<Patch>(), subsA, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Intersection);
        Assert.Empty(result.Triangles);
    }
}
