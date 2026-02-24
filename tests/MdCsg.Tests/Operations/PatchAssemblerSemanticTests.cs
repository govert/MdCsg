using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Tests.Operations;

/// <summary>Semantic paths: PatchAssembler operation selection logic</summary>
public class PatchAssemblerSemanticTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c) =>
        new(a, b, c, 0, false);

    private static Patch MakePatch(int id, bool isInside, List<int> triIndices)
    {
        var p = new Patch(id) { IsInside = isInside, HasConfidentPoint = true };
        p.SubTriangleIndices.AddRange(triIndices);
        return p;
    }

    [Fact]
    public void Union_KeepsA_Outside_B()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var subB = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitZ) };

        var pA = MakePatch(0, isInside: false, new List<int> { 0 }); // outside B → keep for union
        var pB = MakePatch(0, isInside: true, new List<int> { 0 });  // inside A → skip for union

        var result = PatchAssembler.Assemble(
            new[] { pA }, new[] { pB }, subA, subB, CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void Union_KeepsB_Outside_A()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var subB = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitZ) };

        var pA = MakePatch(0, isInside: true, new List<int> { 0 });   // inside B → skip for union
        var pB = MakePatch(0, isInside: false, new List<int> { 0 });  // outside A → keep for union

        var result = PatchAssembler.Assemble(
            new[] { pA }, new[] { pB }, subA, subB, CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
        Assert.False(result.FlipNormals[0]);
    }

    [Fact]
    public void Intersection_KeepsA_Inside_B()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var subB = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitZ) };

        var pA = MakePatch(0, isInside: true, new List<int> { 0 });   // inside B → keep for intersection
        var pB = MakePatch(0, isInside: true, new List<int> { 0 });   // inside A → keep for intersection

        var result = PatchAssembler.Assemble(
            new[] { pA }, new[] { pB }, subA, subB, CsgOperation.Intersection);
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Difference_KeepsA_Outside_B_And_B_Inside_A_Flipped()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var subB = new List<FaceCutter.SubTriangle> { MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1)) };

        var pA = MakePatch(0, isInside: false, new List<int> { 0 });  // outside B → keep for diff
        var pB = MakePatch(0, isInside: true, new List<int> { 0 });   // inside A → keep flipped for diff

        var result = PatchAssembler.Assemble(
            new[] { pA }, new[] { pB }, subA, subB, CsgOperation.Difference);
        Assert.Equal(2, result.Triangles.Count);
        Assert.False(result.FlipNormals[0]); // A not flipped
        Assert.True(result.FlipNormals[1]);  // B flipped
    }

    [Fact]
    public void Difference_B_Outside_A_NotKept()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var subB = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitZ) };

        var pA = MakePatch(0, isInside: false, new List<int> { 0 });
        var pB = MakePatch(0, isInside: false, new List<int> { 0 }); // outside A → skip for diff

        var result = PatchAssembler.Assemble(
            new[] { pA }, new[] { pB }, subA, subB, CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count); // only A kept
    }

    [Fact]
    public void CoplanarPatch_A_SameNormal_Union_Kept()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var subB = new List<FaceCutter.SubTriangle>();

        var pA = new Patch(0) { HasConfidentPoint = false, CoplanarNormalsAgree = true };
        pA.SubTriangleIndices.Add(0);

        var result = PatchAssembler.Assemble(
            new[] { pA }, Array.Empty<Patch>(), subA, subB, CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void CoplanarPatch_A_OppositeNormal_Union_Discarded()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var subB = new List<FaceCutter.SubTriangle>();

        var pA = new Patch(0) { HasConfidentPoint = false, CoplanarNormalsAgree = false };
        pA.SubTriangleIndices.Add(0);

        var result = PatchAssembler.Assemble(
            new[] { pA }, Array.Empty<Patch>(), subA, subB, CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void CoplanarPatch_A_Difference_OppositeNormal_Kept()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };

        var pA = new Patch(0) { HasConfidentPoint = false, CoplanarNormalsAgree = false };
        pA.SubTriangleIndices.Add(0);

        var result = PatchAssembler.Assemble(
            new[] { pA }, Array.Empty<Patch>(), subA, new List<FaceCutter.SubTriangle>(), CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void CoplanarPatch_B_AlwaysDiscarded()
    {
        var subA = new List<FaceCutter.SubTriangle>();
        var subB = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };

        var pB = new Patch(0) { HasConfidentPoint = false, CoplanarNormalsAgree = true };
        pB.SubTriangleIndices.Add(0);

        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), new[] { pB }, subA, subB, CsgOperation.Union);
        Assert.Empty(result.Triangles);
    }

    [Fact]
    public void EmptyPatches_ProducesEmptyResult()
    {
        var result = PatchAssembler.Assemble(
            Array.Empty<Patch>(), Array.Empty<Patch>(),
            new List<FaceCutter.SubTriangle>(), new List<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
        Assert.Empty(result.FlipNormals);
    }

    [Fact]
    public void Patch_NullIsInside_TreatedAsFalse()
    {
        var subA = new List<FaceCutter.SubTriangle> { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var pA = new Patch(0) { IsInside = null, HasConfidentPoint = true };
        pA.SubTriangleIndices.Add(0);

        // Union: IsInside=false → outside B → keep
        var result = PatchAssembler.Assemble(
            new[] { pA }, Array.Empty<Patch>(), subA, new List<FaceCutter.SubTriangle>(), CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }
}
