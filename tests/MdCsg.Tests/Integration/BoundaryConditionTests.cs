using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Boundary condition integration tests</summary>
public class BoundaryConditionTests
{
    [Fact]
    public void Union_TwoCubes_VolumeValid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var r = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // Two overlapping cubes: 1 + 1 - 0.5 = 1.5
        Assert.True(vol > 1.2 && vol < 1.8, $"Union volume: {vol}");
    }

    [Fact]
    public void Intersect_TwoCubes_VolumeValid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var r = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // Overlap: 0.5 * 1 * 1 = 0.5
        Assert.True(vol > 0.3 && vol < 0.7, $"Intersect volume: {vol}");
    }

    [Fact]
    public void Difference_TwoCubes_VolumeValid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var r = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // A minus overlap: 1 - 0.5 = 0.5
        Assert.True(vol > 0.3 && vol < 0.7, $"Difference volume: {vol}");
    }

    [Fact]
    public void InclusionExclusion_TwoCubes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double volU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double volI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        // |A ∪ B| = |A| + |B| - |A ∩ B|
        double expected = volA + volB - volI;
        Assert.True(System.Math.Abs(volU - expected) < 0.2,
            $"Inclusion-exclusion: U={volU}, A={volA}, B={volB}, I={volI}, A+B-I={expected}");
    }

    [Fact]
    public void Union_Volume_GreaterThan_Intersection_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double volU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double volI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(volU > volI, $"Union volume {volU} should exceed intersection volume {volI}");
    }

    [Fact]
    public void RayCast_CubeCenter_Inside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_CubeCorner_Outside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Slightly outside the cube corner
        var result = RayCastClassifier.Classify(new Vec3(-0.1, -0.1, -0.1), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_CubeCenter_Inside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumber_CubeCorner_Outside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(-0.1, -0.1, -0.1), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Union_Sphere_Cube_ProducesValidMesh()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var r = Csg.Union(sphere, cube);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 0.5);
    }

    [Fact]
    public void Intersect_Sphere_Cube_ProducesValidMesh()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var r = Csg.Intersect(sphere, cube);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 0.1);
    }

    [Fact]
    public void Difference_Cube_Sphere_ProducesValidMesh()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var r = Csg.Difference(cube, sphere);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // Cube=1, sphere≈0.113 → diff≈0.887
        Assert.True(vol > 0.5);
    }

    [Fact]
    public void Union_ThreeSpheres()
    {
        var s1 = new Solid(MeshFactory.CreateSphere(new Vec3(0, 0, 0), 1, 2).Mesh);
        var s2 = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var s3 = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.86, 0), 1, 2).Mesh);
        var r12 = Csg.Union(s1, s2);
        var r123 = Csg.Union(new Solid(r12.Mesh), s3);
        Assert.True(r123.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r123.Mesh);
        Assert.True(vol > 3.0, $"Three overlapping spheres union volume: {vol}");
    }

    [Fact]
    public void ClassifyMany_CubePoints()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Grid of points, verify inside/outside consistency
        for (double x = 0.2; x < 0.8; x += 0.3)
        for (double y = 0.2; y < 0.8; y += 0.3)
        for (double z = 0.2; z < 0.8; z += 0.3)
        {
            var p = new Vec3(x, y, z);
            var rc = RayCastClassifier.Classify(p, bvh);
            var wn = WindingNumberClassifier.Classify(p, bvh);
            Assert.Equal(SolidClassification.Inside, rc);
            Assert.Equal(rc, wn);
        }
    }

    [Fact]
    public void ClassifyMany_OutsideCubePoints()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var outsidePoints = new[]
        {
            new Vec3(-1, 0.5, 0.5),
            new Vec3(2, 0.5, 0.5),
            new Vec3(0.5, -1, 0.5),
            new Vec3(0.5, 2, 0.5),
            new Vec3(0.5, 0.5, -1),
            new Vec3(0.5, 0.5, 2),
        };
        foreach (var p in outsidePoints)
        {
            var rc = RayCastClassifier.Classify(p, bvh);
            Assert.Equal(SolidClassification.Outside, rc);
        }
    }

    [Fact]
    public void Csg_Union_Disjoint_VolumeIsSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var r = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(System.Math.Abs(vol - 2.0) < 0.1, $"Disjoint union volume: {vol}");
    }

    [Fact]
    public void Csg_Difference_Disjoint_VolumeIsA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var r = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(System.Math.Abs(vol - 1.0) < 0.1, $"Disjoint difference volume: {vol}");
    }
}
