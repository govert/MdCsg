using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG stress — higher subdivision spheres, many-face meshes, performance bounds</summary>
public class CsgHighSubdivisionStressTests
{
    [Fact]
    public void Sphere3_CubeUnion_NoCrash()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh);
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var result = Csg.Union(sphere, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Sphere3_Sphere3_Union_NoCrash()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 3).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Sphere3_Sphere3_Intersection_NoCrash()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 3).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Sphere3_Sphere3_Difference_NoCrash()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 3).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Sphere3_HasManyCubes()
    {
        // Verify sphere sub-3 has expected face count: 20 * 4^3 = 1280
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.Equal(1280, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void LargeCubeSmallSphere_Difference()
    {
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-2, -2, -2), 4).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // 64 - ~4.19 ≈ 59.81
        Assert.True(vol > 50 && vol < 65, $"Expected ~59.8, got {vol:F2}");
    }

    [Fact]
    public void ManyFaces_ValidResult()
    {
        // Sphere sub-3 has 1280 faces each; intersection produces many faces
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 3).Mesh);
        var result = Csg.Union(a, b);

        // Basic validity checks
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Sphere3_Volume_ApproachesAnalytical()
    {
        double r = 1.0;
        double analyticalVol = 4.0 / 3.0 * System.Math.PI * r * r * r; // ~4.189
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, r, 3);
        double meshVol = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);

        // Sub-3 should be within 3% of analytical
        double error = System.Math.Abs(meshVol - analyticalVol) / analyticalVol;
        Assert.True(error < 0.03, $"Sphere volume error {error:P2}: mesh={meshVol:F4}, analytical={analyticalVol:F4}");
    }

    [Fact]
    public void TwoOverlappingSpheres_VolumeInRange()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);

        Assert.True(volUnion > System.Math.Max(volA, volB) - 0.1);
        Assert.True(volUnion < volA + volB + 0.1);
    }
}
