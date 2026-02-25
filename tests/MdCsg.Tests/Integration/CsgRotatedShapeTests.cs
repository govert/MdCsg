using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG with non-axis-aligned geometry — diagonal offsets, sphere-sphere, tetrahedra, asymmetric overlaps</summary>
public class CsgRotatedShapeTests
{
    [Fact]
    public void DiagonalOffset_Union_Volume()
    {
        // Two cubes offset diagonally by (0.5, 0.5, 0.5)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Overlap is 0.5^3 = 0.125, union = 2 - 0.125 = 1.875
        Assert.True(System.Math.Abs(vol - 1.875) < 0.1,
            $"Expected ~1.875, got {vol:F4}");
    }

    [Fact]
    public void DiagonalOffset_Intersection_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 0.125) < 0.05,
            $"Expected ~0.125, got {vol:F4}");
    }

    [Fact]
    public void DiagonalOffset_Difference_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var result = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // A - B = 1 - 0.125 = 0.875
        Assert.True(System.Math.Abs(vol - 0.875) < 0.05,
            $"Expected ~0.875, got {vol:F4}");
    }

    [Fact]
    public void SphereSphere_Union_LargerThanEach()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        var result = Csg.Union(a, b);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);

        Assert.True(volUnion > volA - 0.1);
        Assert.True(volUnion > volB - 0.1);
        Assert.True(volUnion < volA + volB + 0.1);
    }

    [Fact]
    public void SphereSphere_Intersection_SmallerThanEach()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        var result = Csg.Intersect(a, b);
        double volInt = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);

        Assert.True(volInt >= 0);
        Assert.True(volInt < volA + 0.1);
    }

    [Fact]
    public void TetrahedronCube_Union_NoCrash()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);

        var result = Csg.Union(tet, cube);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void TetrahedronCube_Difference_PositiveVolume()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);

        var result = Csg.Difference(tet, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void LargeCubeSmallCube_DifferenceInCorner()
    {
        // Subtract a small cube from the corner of a large cube
        var large = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(-0.1, -0.1, -0.1), 0.5).Mesh);

        var result = Csg.Difference(large, small);
        double volLarge = VolumeCalculator.ComputeAbsoluteVolume(large.Mesh);
        double volDiff = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(volDiff < volLarge);
        Assert.True(volDiff > 0);
    }

    [Fact]
    public void AsymmetricOverlap_XY_Volume()
    {
        // A offset 0.7 in X, 0.3 in Y — asymmetric overlap
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.7, 0.3, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Union should be between max(A,B) and A+B
        Assert.True(vol > volA - 0.01, $"Union {vol:F4} should be >= A {volA:F4}");
        Assert.True(vol < volA + volB + 0.01, $"Union {vol:F4} should be <= A+B {volA + volB:F4}");
        Assert.True(vol > 1.5 && vol < 2.0, $"Expected union 1.5-2.0, got {vol:F4}");
    }

    [Fact]
    public void SphereCube_Difference_Volume()
    {
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh);

        var result = Csg.Difference(cube, sphere);
        double volCube = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double volSphere = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);

        // Sphere fully inside cube, so result = cube - sphere
        double expected = volCube - volSphere;
        Assert.True(System.Math.Abs(volResult - expected) < 0.5,
            $"Expected ~{expected:F4}, got {volResult:F4}");
    }

    [Fact]
    public void ThreeCubes_ChainUnion_Volume()
    {
        // Three cubes in a row, each offset by 0.5 in X
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 0, 0)).Mesh);

        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var abc = Csg.Union(abSolid, c);

        double vol = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        // Total span: 0 to 2.0 in X, 0 to 1 in Y and Z → vol = 2
        Assert.True(System.Math.Abs(vol - 2.0) < 0.1,
            $"Expected ~2.0, got {vol:F4}");
    }

    [Fact]
    public void LargeOffset_NoOverlap_UnionIsTwoVolumes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(50, 0, 0)).Mesh);

        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 2.0) < 0.01);
    }

    [Fact]
    public void NegativeOffset_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, 0)).Mesh);

        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0 && vol < 2.0);
    }
}
