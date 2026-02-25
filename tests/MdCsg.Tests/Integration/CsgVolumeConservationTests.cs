using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG volume conservation — mathematical properties that volumes must satisfy</summary>
public class CsgVolumeConservationTests
{
    private static double Volume(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void DifferenceVolume_LessThanOrEqualA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = Volume(Csg.Difference(a, b));
        Assert.True(volDiff <= volA + 0.01, $"|A-B|={volDiff:F4} > |A|={volA:F4}");
    }

    [Fact]
    public void IntersectionVolume_LessThanOrEqualMinAB()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.7, 0.1)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volInt = Volume(Csg.Intersect(a, b));
        Assert.True(volInt <= System.Math.Min(volA, volB) + 0.01);
    }

    [Fact]
    public void UnionVolume_GreaterThanOrEqualMaxAB()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.7, 0.1)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = Volume(Csg.Union(a, b));
        Assert.True(volUnion >= System.Math.Max(volA, volB) - 0.01);
    }

    [Fact]
    public void InclusionExclusion_ThreeBody()
    {
        // |A∪B∪C| ≤ |A| + |B| + |C|
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volC = VolumeCalculator.ComputeAbsoluteVolume(c.Mesh);

        var abResult = Csg.Union(a, b);
        var abSolid = new Solid(abResult.Mesh);
        double volABC = Volume(Csg.Union(abSolid, c));

        Assert.True(volABC <= volA + volB + volC + 0.2);
        Assert.True(volABC > 0);
    }

    [Fact]
    public void Difference_FullyContained_Empty()
    {
        // Small cube fully inside big cube: small - big = empty
        var big = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);

        var result = Csg.Difference(small, big);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Intersection_FullyContained_EqualsSmaller()
    {
        var big = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 4).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);

        double volSmall = VolumeCalculator.ComputeAbsoluteVolume(small.Mesh);
        double volInt = Volume(Csg.Intersect(big, small));

        Assert.True(System.Math.Abs(volInt - volSmall) < 0.01,
            $"|A∩B|={volInt:F4} vs |B|={volSmall:F4}");
    }

    [Fact]
    public void Union_FullyContained_EqualsBigger()
    {
        var big = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), 4).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);

        double volBig = VolumeCalculator.ComputeAbsoluteVolume(big.Mesh);
        double volUnion = Volume(Csg.Union(big, small));

        Assert.True(System.Math.Abs(volUnion - volBig) < 0.2,
            $"|A∪B|={volUnion:F4} vs |A|={volBig:F4}");
    }

    [Fact]
    public void Difference_Complement()
    {
        // |A - B| + |A ∩ B| ≈ |A|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = Volume(Csg.Difference(a, b));
        double volInt = Volume(Csg.Intersect(a, b));

        Assert.True(System.Math.Abs((volDiff + volInt) - volA) < 0.1,
            $"|A-B|+|A∩B|={volDiff + volInt:F4} vs |A|={volA:F4}");
    }

    [Fact]
    public void SphereCube_Union_VolumeBounds()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);

        double volS = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        double volC = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double volUnion = Volume(Csg.Union(sphere, cube));

        Assert.True(volUnion >= System.Math.Max(volS, volC) - 0.1);
        Assert.True(volUnion <= volS + volC + 0.1);
    }

    [Fact]
    public void SphereCube_Intersection_VolumeBounds()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);

        double volS = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        double volC = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double volInt = Volume(Csg.Intersect(sphere, cube));

        Assert.True(volInt >= 0);
        Assert.True(volInt <= System.Math.Min(volS, volC) + 0.1);
    }

    [Fact]
    public void SphereCube_Difference_VolumeBounds()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);

        double volC = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double volS = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        double volDiff = Volume(Csg.Difference(cube, sphere));

        Assert.True(volDiff >= 0);
        Assert.True(volDiff <= volC + 0.01);
        Assert.True(volDiff >= volC - volS - 0.1,
            $"|C-S|={volDiff:F4} < |C|-|S|={volC - volS:F4}");
    }

    [Fact]
    public void Volume_NonNegative_AllOperations()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.1)).Mesh);

        Assert.True(Volume(Csg.Union(a, b)) >= 0);
        Assert.True(Volume(Csg.Intersect(a, b)) >= 0);
        Assert.True(Volume(Csg.Difference(a, b)) >= 0);
        Assert.True(Volume(Csg.Difference(b, a)) >= 0);
    }
}
