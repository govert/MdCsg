using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG determinism — same inputs always produce same outputs, repeatable volumes</summary>
public class CsgDeterminismTests
{
    [Fact]
    public void Union_Deterministic_FaceCount()
    {
        int count1 = RunUnion();
        int count2 = RunUnion();
        Assert.Equal(count1, count2);

        static int RunUnion()
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
            return Csg.Union(a, b).FaceCount;
        }
    }

    [Fact]
    public void Intersection_Deterministic_FaceCount()
    {
        int count1 = RunIntersect();
        int count2 = RunIntersect();
        Assert.Equal(count1, count2);

        static int RunIntersect()
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
            return Csg.Intersect(a, b).FaceCount;
        }
    }

    [Fact]
    public void Difference_Deterministic_FaceCount()
    {
        int count1 = RunDiff();
        int count2 = RunDiff();
        Assert.Equal(count1, count2);

        static int RunDiff()
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
            return Csg.Difference(a, b).FaceCount;
        }
    }

    [Fact]
    public void Union_Deterministic_Volume()
    {
        double vol1 = RunUnionVolume();
        double vol2 = RunUnionVolume();
        Assert.Equal(vol1, vol2, 10);

        static double RunUnionVolume()
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
            return VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        }
    }

    [Fact]
    public void Union_Deterministic_VertexCount()
    {
        int count1 = RunUnionVerts();
        int count2 = RunUnionVerts();
        Assert.Equal(count1, count2);

        static int RunUnionVerts()
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
            return Csg.Union(a, b).VertexCount;
        }
    }

    [Fact]
    public void DiagonalOffset_Deterministic()
    {
        int count1 = RunDiag();
        int count2 = RunDiag();
        Assert.Equal(count1, count2);

        static int RunDiag()
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
            return Csg.Union(a, b).FaceCount;
        }
    }

    [Fact]
    public void SphereCube_Deterministic()
    {
        int count1 = RunSphereCube();
        int count2 = RunSphereCube();
        Assert.Equal(count1, count2);

        static int RunSphereCube()
        {
            var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
            var cube = new Solid(MeshFactory.CreateCube().Mesh);
            return Csg.Union(sphere, cube).FaceCount;
        }
    }

    [Fact]
    public void ThreeRuns_AllSame()
    {
        var results = new int[3];
        for (int i = 0; i < 3; i++)
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.7, 0.1)).Mesh);
            results[i] = Csg.Union(a, b).FaceCount;
        }
        Assert.Equal(results[0], results[1]);
        Assert.Equal(results[1], results[2]);
    }

    [Fact]
    public void IntersectionSegmentCount_Deterministic()
    {
        int count1 = RunSegCount();
        int count2 = RunSegCount();
        Assert.Equal(count1, count2);

        static int RunSegCount()
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
            return Csg.Union(a, b).IntersectionSegmentCount;
        }
    }

    [Fact]
    public void PatchCounts_Deterministic()
    {
        var (pA1, pB1) = RunPatchCounts();
        var (pA2, pB2) = RunPatchCounts();
        Assert.Equal(pA1, pA2);
        Assert.Equal(pB1, pB2);

        static (int, int) RunPatchCounts()
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
            var r = Csg.Intersect(a, b);
            return (r.PatchCountA, r.PatchCountB);
        }
    }
}
