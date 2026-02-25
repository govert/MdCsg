using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG normal consistency — outward normals, volume sign, face winding</summary>
public class CsgNormalConsistencyTests
{
    [Fact]
    public void Union_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0, $"Union signed volume {vol} should be positive (outward normals)");
    }

    [Fact]
    public void Intersection_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0, $"Intersection signed volume {vol} should be positive");
    }

    [Fact]
    public void Difference_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0, $"Difference signed volume {vol} should be positive");
    }

    [Fact]
    public void InputCube_PositiveVolume()
    {
        var cube = MeshFactory.CreateCube().Mesh;
        double vol = VolumeCalculator.ComputeVolume(cube);
        Assert.True(vol > 0, $"Cube signed volume {vol} should be positive");
    }

    [Fact]
    public void InputSphere_PositiveVolume()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        double vol = VolumeCalculator.ComputeVolume(sphere);
        Assert.True(vol > 0, $"Sphere signed volume {vol} should be positive");
    }

    [Fact]
    public void InputTetrahedron_PositiveVolume()
    {
        var tet = MeshFactory.CreateTetrahedron().Mesh;
        double vol = VolumeCalculator.ComputeVolume(tet);
        Assert.True(vol > 0, $"Tetrahedron signed volume {vol} should be positive");
    }

    [Fact]
    public void DisjointUnion_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void SphereSphere_Union_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void SphereSphere_Intersection_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void FullyContained_Difference_PositiveVolume()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Difference(outer, inner);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void CubeSphere_Difference_PositiveVolume()
    {
        var cube = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(1, 1, 2), 0.5, 2).Mesh);
        var result = Csg.Difference(cube, sphere);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void TetrahedronCube_Union_PositiveVolume()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var result = Csg.Union(tet, cube);
        double vol = VolumeCalculator.ComputeVolume(result.Mesh);
        Assert.True(vol > 0);
    }
}
