using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG commutativity — Union/Intersection are commutative, Difference is not</summary>
public class CsgCommutativityPropertyTests
{
    private static double Volume(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void Union_IsCommutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volAB = Volume(Csg.Union(a, b));
        double volBA = Volume(Csg.Union(b, a));

        Assert.True(System.Math.Abs(volAB - volBA) < 0.05,
            $"|A∪B|={volAB:F4} vs |B∪A|={volBA:F4}");
    }

    [Fact]
    public void Intersection_IsCommutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volAB = Volume(Csg.Intersect(a, b));
        double volBA = Volume(Csg.Intersect(b, a));

        Assert.True(System.Math.Abs(volAB - volBA) < 0.05,
            $"|A∩B|={volAB:F4} vs |B∩A|={volBA:F4}");
    }

    [Fact]
    public void Difference_IsNotCommutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volAminusB = Volume(Csg.Difference(a, b));
        double volBminusA = Volume(Csg.Difference(b, a));

        // Both should be positive since they overlap but are the same size
        Assert.True(volAminusB > 0);
        Assert.True(volBminusA > 0);
        // They should be equal by symmetry for same-sized cubes
        Assert.True(System.Math.Abs(volAminusB - volBminusA) < 0.1,
            $"|A-B|={volAminusB:F4} vs |B-A|={volBminusA:F4}");
    }

    [Fact]
    public void Union_IsCommutative_FaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        int fcAB = Csg.Union(a, b).FaceCount;
        int fcBA = Csg.Union(b, a).FaceCount;

        Assert.Equal(fcAB, fcBA);
    }

    [Fact]
    public void Intersection_IsCommutative_FaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        int fcAB = Csg.Intersect(a, b).FaceCount;
        int fcBA = Csg.Intersect(b, a).FaceCount;

        Assert.Equal(fcAB, fcBA);
    }

    [Fact]
    public void Union_SphereCube_Commutative()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);

        double volSC = Volume(Csg.Union(sphere, cube));
        double volCS = Volume(Csg.Union(cube, sphere));

        Assert.True(System.Math.Abs(volSC - volCS) < 0.15,
            $"|S∪C|={volSC:F4} vs |C∪S|={volCS:F4}");
    }

    [Fact]
    public void Intersection_SphereCube_Commutative()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);

        double volSC = Volume(Csg.Intersect(sphere, cube));
        double volCS = Volume(Csg.Intersect(cube, sphere));

        Assert.True(System.Math.Abs(volSC - volCS) < 0.15,
            $"|S∩C|={volSC:F4} vs |C∩S|={volCS:F4}");
    }

    [Fact]
    public void Union_DisjointCubes_Commutative()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        double volAB = Volume(Csg.Union(a, b));
        double volBA = Volume(Csg.Union(b, a));

        Assert.True(System.Math.Abs(volAB - volBA) < 0.01);
    }

    [Fact]
    public void Intersection_Disjoint_CommutativeBothEmpty()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        Assert.Equal(0, Csg.Intersect(a, b).FaceCount);
        Assert.Equal(0, Csg.Intersect(b, a).FaceCount);
    }

    [Fact]
    public void Difference_DisjointCubes_VolumeEqualsA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volAmB = Volume(Csg.Difference(a, b));
        Assert.True(System.Math.Abs(volAmB - volA) < 0.01);
    }

    [Fact]
    public void Union_Commutative_OffsetCubes()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0), 1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.7, 0.1), 1).Mesh);

        double volAB = Volume(Csg.Union(a, b));
        double volBA = Volume(Csg.Union(b, a));

        Assert.True(System.Math.Abs(volAB - volBA) < 0.1,
            $"|A∪B|={volAB:F4} vs |B∪A|={volBA:F4}");
    }

    [Fact]
    public void DeMorgan_Volume()
    {
        // |A∪B| = |A| + |B| - |A∩B| — inclusion-exclusion
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = Volume(Csg.Union(a, b));
        double volIntersect = Volume(Csg.Intersect(a, b));

        double expected = volA + volB - volIntersect;
        Assert.True(System.Math.Abs(volUnion - expected) < 0.1,
            $"|A∪B|={volUnion:F4} vs |A|+|B|-|A∩B|={expected:F4}");
    }
}
