using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 49: Mixed shape combination tests (20 tests)</summary>
public class MixedShapeTests
{
    [Fact]
    public void CubeSphere_Union()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 1.2), 0.5, 2).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void CubeSphere_Intersect()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeSphere_Difference_SphereFromCube()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 12);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0.5, $"Cube-sphere diff volume {vol} should be > 0.5");
    }

    [Fact]
    public void CubeTetra_Union()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 1.2), 0.8).Mesh);
        var result = Csg.Union(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeTetra_Intersect()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1).Mesh);
        var result = Csg.Intersect(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeTetra_Difference_TetraFromCube()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.8).Mesh);
        var result = Csg.Difference(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetraSphere_Union()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var result = Csg.Union(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetraSphere_Intersect()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.5, 2).Mesh);
        var result = Csg.Intersect(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetraSphere_Difference()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.5, 2).Mesh);
        var result = Csg.Difference(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeThenSphere_ChainedUnion()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(1.5, 0.5, 0.5), 0.8, 2).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0, $"Volume {vol}");
    }

    [Fact]
    public void SphereMinusCube()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 1, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Difference(sphere, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetraMinusCube()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1.5).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Difference(tet, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TwoCubes_Union_ThenSphere_Difference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Union(a, b);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var result = Csg.Difference(new Solid(ab.Mesh), sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeWithTwoSphereHoles()
    {
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var s1 = new Solid(MeshFactory.CreateSphere(new Vec3(0.3, 0.5, 0.5), 0.2, 1).Mesh);
        var diff1 = Csg.Difference(cube, s1);
        var s2 = new Solid(MeshFactory.CreateSphere(new Vec3(0.8, 0.5, 0.5), 0.2, 1).Mesh);
        var diff2 = Csg.Difference(new Solid(diff1.Mesh), s2);
        Assert.True(diff2.FaceCount > 12);
    }

    [Fact]
    public void DisjointSphereAndCube_Union()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(5, 0, 0), 0.5, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(sphere, cube);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double sphereVol = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        Assert.True(System.Math.Abs(vol - (1 + sphereVol)) / (1 + sphereVol) < 0.15,
            $"Disjoint union vol {vol}, expected ~{1 + sphereVol}");
    }

    [Fact]
    public void CubeSphere_Volume_InclusionExclusion()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        double vU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(cube, sphere).Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(cube, sphere).Mesh);
        double vCube = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double vSphere = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        // V(A∪B) + V(A∩B) ≈ V(A) + V(B)
        double lhs = vU + vI;
        double rhs = vCube + vSphere;
        Assert.True(System.Math.Abs(lhs - rhs) < 0.3,
            $"Inclusion-exclusion: {lhs} != {rhs}");
    }

    [Fact]
    public void BigTetra_SmallCube_ContainedIntersection()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 5).Mesh);
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(1, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Intersect(tet, cube);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0.05, $"Contained intersection should have volume, got {vol}");
    }

    [Fact]
    public void LargeSphere_SmallCube_Difference()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 2, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.25, -0.25, -0.25), 0.5).Mesh);
        var result = Csg.Difference(sphere, cube);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double sphereVol = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        Assert.True(vol < sphereVol, $"Carved sphere vol {vol} should be < original {sphereVol}");
    }

    [Fact]
    public void ThreeShapes_ChainedOperations()
    {
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.5, 2).Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.2, 0.2, 0.2), 0.6).Mesh);
        var diff = Csg.Difference(cube, sphere);
        var result = Csg.Difference(new Solid(diff.Mesh), tet);
        Assert.True(result.FaceCount > 0);
    }
}
