using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree RayCastCount — accuracy with various ray origins and directions</summary>
public class BvhRayCastAccuracyPropertyTests
{
    private static readonly Vec3 PerturbedX = new Vec3(1, 0.00013, 0.00017).Normalized;
    private static readonly Vec3 PerturbedY = new Vec3(0.00019, 1, 0.00023).Normalized;
    private static readonly Vec3 PerturbedZ = new Vec3(0.00029, 0.00031, 1).Normalized;

    [Fact]
    public void RayCast_InsideCube_AllDirections_Odd()
    {
        var cube = MeshFactory.CreateCube();
        var origin = new Vec3(0.5, 0.5, 0.5);
        foreach (var dir in new[] { PerturbedX, PerturbedY, PerturbedZ })
        {
            int hits = cube.Bvh.RayCastCount(new Ray(origin, dir));
            Assert.True(hits % 2 == 1, $"Inside cube, direction {dir}: expected odd hits, got {hits}");
        }
    }

    [Fact]
    public void RayCast_OutsideCube_AllDirections_Even()
    {
        var cube = MeshFactory.CreateCube();
        var origin = new Vec3(5, 5, 5);
        foreach (var dir in new[] { PerturbedX, PerturbedY, PerturbedZ })
        {
            int hits = cube.Bvh.RayCastCount(new Ray(origin, dir));
            Assert.True(hits % 2 == 0, $"Outside cube, direction {dir}: expected even hits, got {hits}");
        }
    }

    [Fact]
    public void RayCast_InsideSphere_AllDirections_Odd()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var origin = Vec3.Zero;
        foreach (var dir in new[] { PerturbedX, PerturbedY, PerturbedZ })
        {
            int hits = sphere.Bvh.RayCastCount(new Ray(origin, dir));
            Assert.True(hits % 2 == 1, $"Inside sphere, direction {dir}: expected odd hits, got {hits}");
        }
    }

    [Fact]
    public void RayCast_OutsideSphere_AllDirections_Even()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var origin = new Vec3(10, 10, 10);
        foreach (var dir in new[] { PerturbedX, PerturbedY, PerturbedZ })
        {
            int hits = sphere.Bvh.RayCastCount(new Ray(origin, dir));
            Assert.True(hits % 2 == 0, $"Outside sphere, direction {dir}: expected even hits, got {hits}");
        }
    }

    [Fact]
    public void RayCast_MissEntirelyAway_ZeroHits()
    {
        var cube = MeshFactory.CreateCube();
        // Ray from far away, pointing further away
        var ray = new Ray(new Vec3(10, 10, 10), new Vec3(1, 1, 1).Normalized);
        Assert.Equal(0, cube.Bvh.RayCastCount(ray));
    }

    [Fact]
    public void RayCast_ThroughCube_TwoOrMoreHits()
    {
        var cube = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), PerturbedX);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0,
            $"Through cube should be even >= 2, got {hits}");
    }

    [Fact]
    public void RayCast_InsideTetrahedron_Odd()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var center = tet.Bounds.Min + (tet.Bounds.Max - tet.Bounds.Min) * 0.5;
        int hits = tet.Bvh.RayCastCount(new Ray(center, PerturbedX));
        Assert.True(hits % 2 == 1, $"Inside tetrahedron: expected odd, got {hits}");
    }

    [Fact]
    public void RayCast_OutsideTetrahedron_Even()
    {
        var tet = MeshFactory.CreateTetrahedron();
        int hits = tet.Bvh.RayCastCount(new Ray(new Vec3(100, 100, 100), PerturbedX));
        Assert.True(hits % 2 == 0, $"Outside tetrahedron: expected even, got {hits}");
    }

    [Fact]
    public void RayCast_InsideLargeCube_Odd()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 10.0);
        var ray = new Ray(new Vec3(5, 5, 5), PerturbedX);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 1);
    }

    [Fact]
    public void RayCast_InsideOffsetCube_Odd()
    {
        var cube = MeshFactory.CreateCube(new Vec3(100, 200, 300));
        var ray = new Ray(new Vec3(100.5, 200.5, 300.5), PerturbedX);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits % 2 == 1);
    }

    [Fact]
    public void RayCast_EmptyBvh_ZeroHits()
    {
        var emptyMesh = new MdCsg.Mesh.HalfEdgeMesh();
        var bvh = BvhTree.Build(emptyMesh);
        int hits = bvh.RayCastCount(new Ray(Vec3.Zero, PerturbedX));
        Assert.Equal(0, hits);
    }

    [Fact]
    public void RayCast_NegativeDirectionThroughCube_TwoOrMoreHits()
    {
        var cube = MeshFactory.CreateCube();
        // Ray from positive side, pointing negative
        var dir = new Vec3(-1, -0.00013, -0.00017).Normalized;
        var ray = new Ray(new Vec3(5, 0.5, 0.5), dir);
        int hits = cube.Bvh.RayCastCount(ray);
        Assert.True(hits >= 2 && hits % 2 == 0,
            $"Through cube negative: expected even >= 2, got {hits}");
    }
}
