using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler — keep/discard logic per operation, normal flipping; MeshValidator — Euler characteristic, manifold checks</summary>
public class PatchAssemblerMeshValidatorPropertyTests
{
    // --- PatchAssembler tests ---

    private static (List<Patch> patchesA, List<Patch> patchesB,
                     List<FaceCutter.SubTriangle> subTrisA, List<FaceCutter.SubTriangle> subTrisB)
        MakeSimplePatches(bool patchAIsInside, bool patchBIsInside)
    {
        var subTriA = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var subTriB = new FaceCutter.SubTriangle(
            new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0), 0, false);

        var patchA = new Patch(0) { IsInside = patchAIsInside, HasConfidentPoint = true };
        patchA.SubTriangleIndices.Add(0);
        var patchB = new Patch(1) { IsInside = patchBIsInside, HasConfidentPoint = true };
        patchB.SubTriangleIndices.Add(0);

        return (new List<Patch> { patchA }, new List<Patch> { patchB },
                new List<FaceCutter.SubTriangle> { subTriA }, new List<FaceCutter.SubTriangle> { subTriB });
    }

    [Fact]
    public void Union_KeepsOutsideA_KeepsOutsideB()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(false, false);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        Assert.Equal(2, result.Triangles.Count); // both outside → both kept
    }

    [Fact]
    public void Union_DiscardsInsideA_DiscardsInsideB()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(true, true);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        Assert.Equal(0, result.Triangles.Count); // both inside → both discarded
    }

    [Fact]
    public void Intersection_KeepsInsideA_KeepsInsideB()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(true, true);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        Assert.Equal(2, result.Triangles.Count);
    }

    [Fact]
    public void Intersection_DiscardsOutsideA_DiscardsOutsideB()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(false, false);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void Difference_KeepsOutsideA_KeepsInsideB_WithFlip()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(false, true);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        Assert.Equal(2, result.Triangles.Count);
        // A's normal stays, B's is flipped
        Assert.False(result.FlipNormals[0]); // A
        Assert.True(result.FlipNormals[1]);  // B flipped
    }

    [Fact]
    public void Difference_DiscardsInsideA()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(true, false);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        // A is inside B → discarded; B is outside A → discarded for difference
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void Difference_BFlipped_ReversesWinding()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(false, true);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        // B's triangle should have reversed winding (A,C,B instead of A,B,C)
        var origB = stB[0];
        var resultB = result.Triangles[1]; // second triangle is from B
        Assert.Equal(origB.A, resultB.A);
        Assert.Equal(origB.C, resultB.B); // C and B swapped
        Assert.Equal(origB.B, resultB.C);
    }

    [Fact]
    public void Union_NoFlipNormals()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(false, false);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        foreach (bool flip in result.FlipNormals)
        {
            Assert.False(flip);
        }
    }

    [Fact]
    public void Intersection_NoFlipNormals()
    {
        var (pA, pB, stA, stB) = MakeSimplePatches(true, true);
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        foreach (bool flip in result.FlipNormals)
        {
            Assert.False(flip);
        }
    }

    [Fact]
    public void CoplanarPatchA_SameNormals_KeptInUnion()
    {
        var subTriA = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var patchA = new Patch(0) { CoplanarNormalsAgree = true, HasConfidentPoint = false };
        patchA.SubTriangleIndices.Add(0);
        var result = PatchAssembler.Assemble(
            new List<Patch> { patchA }, new List<Patch>(),
            new List<FaceCutter.SubTriangle> { subTriA }, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void CoplanarPatchA_OppositeNormals_DiscardedInUnion()
    {
        var subTriA = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var patchA = new Patch(0) { CoplanarNormalsAgree = false, HasConfidentPoint = false };
        patchA.SubTriangleIndices.Add(0);
        var result = PatchAssembler.Assemble(
            new List<Patch> { patchA }, new List<Patch>(),
            new List<FaceCutter.SubTriangle> { subTriA }, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Equal(0, result.Triangles.Count);
    }

    [Fact]
    public void CoplanarPatchA_OppositeNormals_KeptInDifference()
    {
        var subTriA = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var patchA = new Patch(0) { CoplanarNormalsAgree = false, HasConfidentPoint = false };
        patchA.SubTriangleIndices.Add(0);
        var result = PatchAssembler.Assemble(
            new List<Patch> { patchA }, new List<Patch>(),
            new List<FaceCutter.SubTriangle> { subTriA }, new List<FaceCutter.SubTriangle>(),
            CsgOperation.Difference);
        Assert.Equal(1, result.Triangles.Count);
    }

    [Fact]
    public void CoplanarPatchB_AlwaysDiscarded()
    {
        var subTriB = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var patchB = new Patch(0) { CoplanarNormalsAgree = true, HasConfidentPoint = false };
        patchB.SubTriangleIndices.Add(0);
        foreach (var op in new[] { CsgOperation.Union, CsgOperation.Intersection, CsgOperation.Difference })
        {
            var result = PatchAssembler.Assemble(
                new List<Patch>(), new List<Patch> { patchB },
                new List<FaceCutter.SubTriangle>(), new List<FaceCutter.SubTriangle> { subTriB },
                op);
            Assert.Equal(0, result.Triangles.Count);
        }
    }

    [Fact]
    public void EmptyPatches_EmptyResult()
    {
        var result = PatchAssembler.Assemble(
            new List<Patch>(), new List<Patch>(),
            new List<FaceCutter.SubTriangle>(), new List<FaceCutter.SubTriangle>(),
            CsgOperation.Union);
        Assert.Empty(result.Triangles);
        Assert.Empty(result.FlipNormals);
    }

    // --- MeshValidator tests ---

    [Fact]
    public void MeshValidator_Cube_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void MeshValidator_Cube_EulerCharacteristic_Two()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(cube.Mesh));
    }

    [Fact]
    public void MeshValidator_Sphere_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void MeshValidator_Sphere_EulerCharacteristic_Two()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void MeshValidator_Tetrahedron_IsClosedManifold()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void MeshValidator_Tetrahedron_EulerCharacteristic_Two()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(tet.Mesh));
    }

    [Fact]
    public void MeshValidator_Cube_HasValidFaceCycles()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.HasValidFaceCycles(cube.Mesh));
    }

    [Fact]
    public void MeshValidator_Cube_IsConsistentlyOriented()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.IsConsistentlyOriented(cube.Mesh));
    }

    [Fact]
    public void MeshValidator_Cube_IsEdgeManifold()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.IsEdgeManifold(cube.Mesh));
    }

    [Fact]
    public void MeshValidator_Validate_CorrectCounts()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(18, result.EdgeCount); // 36 half-edges / 2
        Assert.Equal(12, result.FaceCount);
    }

    [Fact]
    public void MeshValidator_SingleTriangle_NotClosed()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) });
        Assert.False(MeshValidator.AllEdgesHaveTwins(mesh));
        Assert.False(MeshValidator.Validate(mesh).IsClosedManifold);
    }
}
