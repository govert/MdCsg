using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG with spheres at different subdivisions — volume accuracy improves with resolution</summary>
public class CsgSphereSubdivisionTests
{
    [Fact]
    public void Sphere_Subdiv1_PositiveVolume()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Sphere_Subdiv2_PositiveVolume()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Sphere_VolumeIncreases_WithSubdivision()
    {
        // As subdivision increases, the icosphere approaches the true sphere volume (4π/3)
        double vol1 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1).Mesh);
        double vol2 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        double vol3 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh);
        Assert.True(vol2 > vol1);
        Assert.True(vol3 > vol2);
    }

    [Fact]
    public void Sphere_Subdiv3_ApproachesExactVolume()
    {
        double exactVol = 4.0 / 3.0 * System.Math.PI; // ~4.189
        double vol = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh);
        Assert.True(System.Math.Abs(vol - exactVol) / exactVol < 0.02,
            $"Sphere vol {vol} should be within 2% of {exactVol}");
    }

    [Fact]
    public void SphereSphere_Union_Subdiv2_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void SphereSphere_Intersection_Subdiv2_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void SphereSphere_Difference_Subdiv2_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        var result = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void SphereSphere_Union_LargerThanEach()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        Assert.True(volUnion >= volA * 0.95);
        Assert.True(volUnion >= volB * 0.95);
    }

    [Fact]
    public void SphereSphere_Intersection_SmallerThanEach()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volIntersect = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(volIntersect <= volA * 1.05);
        Assert.True(volIntersect <= volB * 1.05);
    }

    [Fact]
    public void CubeSphere_Difference_SmallerThanCube()
    {
        var cube = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(1, 1, 1), 0.5, 2).Mesh);
        double volCube = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(cube, sphere).Mesh);
        Assert.True(volResult < volCube);
        Assert.True(volResult > volCube * 0.9);
    }

    [Fact]
    public void Sphere_FaceCount_Grows_WithSubdivision()
    {
        int f1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1).Mesh.Faces.Count;
        int f2 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh.Faces.Count;
        int f3 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh.Faces.Count;
        // Icosphere: faces quadruple per subdivision (20 * 4^n)
        Assert.Equal(80, f1);
        Assert.Equal(320, f2);
        Assert.Equal(1280, f3);
    }
}
