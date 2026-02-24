using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 40: Sphere CSG integration tests (20 tests)</summary>
public class SphereIntegrationTests
{
    [Fact]
    public void Sphere_Sub1_VolumePositive()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 1);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Sphere_Sub2_VolumeCloserToAnalytic()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1, 1);
        var s2 = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(s1.Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(s2.Mesh);
        double expected = (4.0 / 3.0) * System.Math.PI;
        // Higher subdivisions should be closer to the analytic volume
        Assert.True(System.Math.Abs(v2 - expected) < System.Math.Abs(v1 - expected),
            $"Sub2 vol {v2} should be closer to {expected} than sub1 vol {v1}");
    }

    [Fact]
    public void Sphere_Sub3_VolumeWithin10Percent()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 3);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        double expected = (4.0 / 3.0) * System.Math.PI;
        Assert.True(System.Math.Abs(vol - expected) / expected < 0.1,
            $"Sphere vol {vol}, expected ~{expected}");
    }

    [Fact]
    public void Sphere_Radius2_VolumeScalesAsCubed()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var s2 = MeshFactory.CreateSphere(Vec3.Zero, 2, 2);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(s1.Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(s2.Mesh);
        // V scales as r^3, so v2/v1 should be ~8
        double ratio = v2 / v1;
        Assert.True(ratio > 6 && ratio < 10, $"Volume ratio {ratio}, expected ~8");
    }

    [Fact]
    public void Sphere_OffsetCenter_ValidManifold()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(5, 10, 15), 1, 2);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Sphere_EulerCharacteristic2()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void TwoSpheres_Union_VolumeGreaterThanSingle()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        double vUnion = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        Assert.True(vUnion > vA, $"Union volume {vUnion} should be > sphere volume {vA}");
    }

    [Fact]
    public void TwoSpheres_Intersect_VolumeLessThanSingle()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var result = Csg.Intersect(a, b);
        double vInt = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        Assert.True(vInt < vA, $"Intersection volume {vInt} should be < sphere volume {vA}");
    }

    [Fact]
    public void TwoSpheres_Difference_VolumeLessThanA()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var result = Csg.Difference(a, b);
        double vDiff = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        Assert.True(vDiff < vA, $"Diff volume {vDiff} should be < sphere volume {vA}");
    }

    [Fact]
    public void SphereCube_Union_IncreasesFaceCount()
    {
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(1.5, 0.5, 0.5), 0.8, 2).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > cube.Mesh.Faces.Count);
    }

    [Fact]
    public void SphereCube_Intersect_FaceCountPositive()
    {
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereCube_Difference_CarvesHole()
    {
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.5, 2).Mesh);
        var result = Csg.Difference(cube, sphere);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double cubeVol = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        Assert.True(vol < cubeVol, $"After carving: {vol} should be < cube vol {cubeVol}");
    }

    [Fact]
    public void ContainedSphere_Union_EqualsOuter()
    {
        var outer = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 2, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh);
        var result = Csg.Union(outer, inner);
        double vResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double vOuter = VolumeCalculator.ComputeAbsoluteVolume(outer.Mesh);
        Assert.True(System.Math.Abs(vResult - vOuter) / vOuter < 0.15,
            $"Union with contained: {vResult}, expected {vOuter}");
    }

    [Fact]
    public void ContainedSphere_Intersect_EqualsInner()
    {
        var outer = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 2, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh);
        var result = Csg.Intersect(outer, inner);
        double vResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        double vInner = VolumeCalculator.ComputeAbsoluteVolume(inner.Mesh);
        Assert.True(System.Math.Abs(vResult - vInner) / vInner < 0.25,
            $"Intersect with contained: {vResult}, expected {vInner}");
    }

    [Fact]
    public void DisjointSpheres_Union_TwoDisconnectedComponents()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(5, 0, 0), 0.5, 2).Mesh);
        var result = Csg.Union(a, b);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vUnion = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vUnion - (vA + vB)) / (vA + vB) < 0.1,
            $"Disjoint union vol {vUnion}, expected sum {vA + vB}");
    }

    [Fact]
    public void DisjointSpheres_Intersect_Empty()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(5, 0, 0), 0.5, 2).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void ThreeSpheres_ChainedUnion()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var c = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 1.5, 0.2), 1, 2).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void SphereTetra_Union()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 1.5).Mesh);
        var result = Csg.Union(sphere, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Sphere_WithWindingNumber_SameAsRayCast()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var rc = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var wn = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rc.FaceCount, wn.FaceCount);
    }
}
