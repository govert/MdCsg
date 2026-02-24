using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Volume and geometry invariants for CSG operations</summary>
public class VolumeGeometryInvariantTests
{
    [Fact]
    public void Cube_Volume_IsOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.Equal(1.0, vol, 2);
    }

    [Fact]
    public void Cube_ScaledVolume_CubesSize()
    {
        var mesh = MeshFactory.CreateCube(size: 2).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.Equal(8.0, vol, 2);
    }

    [Fact]
    public void Cube_OffsetDoesNotChangeVolume()
    {
        var v1 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube().Mesh);
        var v2 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube(new Vec3(10, 20, 30)).Mesh);
        Assert.Equal(v1, v2, 2);
    }

    [Fact]
    public void Sphere_VolumeApproximation()
    {
        // Icosphere with 3 subdivisions should approximate 4/3 * pi * r^3
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        double expected = 4.0 / 3.0 * System.Math.PI;
        // With subdivision 3, should be within ~2% of exact
        Assert.True(System.Math.Abs(vol - expected) / expected < 0.02,
            $"Sphere vol {vol}, expected ~{expected}");
    }

    [Fact]
    public void Tetrahedron_ValidVolume()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.True(vol > 0, "Tetrahedron should have positive volume");
    }

    [Fact]
    public void Union_DisjointCubes_VolumeIsSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vb = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vu = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        Assert.True(System.Math.Abs(vu - (va + vb)) < 0.3,
            $"Disjoint union vol {vu} != {va} + {vb}");
    }

    [Fact]
    public void Intersection_DisjointCubes_VolumeIsZero()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh);
        var r = Csg.Intersect(a, b);
        double vi = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vi < 0.1, $"Disjoint intersection vol {vi} should be ~0");
    }

    [Fact]
    public void Difference_DisjointCubes_VolumeIsA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vd = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        Assert.True(System.Math.Abs(vd - va) < 0.1,
            $"Disjoint diff vol {vd} != vol(A) {va}");
    }

    [Fact]
    public void Union_OverlappingCubes_LessThanSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vb = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vu = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        Assert.True(vu < va + vb + 0.1, $"Union vol {vu} >= sum {va + vb}");
    }

    [Fact]
    public void Intersection_OverlappingCubes_GreaterThanZero()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double vi = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(vi > 0.01, $"Overlapping intersection vol {vi} should be > 0");
    }

    [Fact]
    public void Difference_OverlappingCubes_LessThanA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vd = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        Assert.True(vd < va + 0.1, $"Diff vol {vd} >= vol(A) {va}");
        Assert.True(vd > 0, "Diff vol should be positive");
    }

    [Fact]
    public void InclusionExclusion_Cubes_Offset03()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vb = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vu = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double vi = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        // va + vb - vi ≈ vu
        double expected = va + vb - vi;
        Assert.True(System.Math.Abs(vu - expected) < 0.5,
            $"Inclusion-exclusion: |{vu} - {expected}| >= 0.5");
    }

    [Fact]
    public void InclusionExclusion_Cubes_Offset05()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vb = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vu = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double vi = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double expected = va + vb - vi;
        Assert.True(System.Math.Abs(vu - expected) < 0.5,
            $"Inclusion-exclusion: |{vu} - {expected}| >= 0.5");
    }

    [Fact]
    public void Difference_PlusIntersection_EqualsA()
    {
        // Vol(A\B) + Vol(A∩B) ≈ Vol(A)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vd = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        double vi = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(System.Math.Abs((vd + vi) - va) < 0.5,
            $"Vol(A\\B)+Vol(A∩B) = {vd + vi}, Vol(A) = {va}");
    }

    [Fact]
    public void Union_Result_AllFacesNonDegenerate()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b);
        foreach (var face in r.Mesh.Faces)
        {
            face.GetTrianglePositions(out var v0, out var v1, out var v2);
            var tri = new Triangle3(v0, v1, v2);
            Assert.True(tri.Area > 1e-15, "Degenerate triangle in union result");
        }
    }

    [Fact]
    public void Difference_Result_AllFacesNonDegenerate()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Difference(a, b);
        foreach (var face in r.Mesh.Faces)
        {
            face.GetTrianglePositions(out var v0, out var v1, out var v2);
            var tri = new Triangle3(v0, v1, v2);
            Assert.True(tri.Area > 1e-15, "Degenerate triangle in difference result");
        }
    }

    [Fact]
    public void Union_CubeSphere_ValidMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.7, 2).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Intersection_CubeSphere_ValidMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.7, 2).Mesh);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Difference_CubeSphere_ValidMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount > 0);
    }
}
