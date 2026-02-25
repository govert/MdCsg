using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree.RayCastCount — parity checks, axis-aligned rays, origin inside, diagonal rays</summary>
public class BvhRayCastCountDeepPropertyTests
{
    [Fact]
    public void RayCastCount_ThroughCube_EvenParity()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(-10, 1, 1), new Vec3(1, 0, 0));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0, $"Through-ray should have even hit count, got {count}");
    }

    [Fact]
    public void RayCastCount_FromInsideCube_OddParity()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        // Origin inside the cube at center (1,1,1)
        var ray = new Ray(new Vec3(1, 1, 1), new Vec3(1, 0.00013, 0.00017));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1, $"Inside-ray should have odd hit count, got {count}");
    }

    [Fact]
    public void RayCastCount_MissCompletely_Zero()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(-10, -10, -10), new Vec3(0, 1, 0)); // shoots upward, misses cube
        int count = cube.Bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_AlongXAxis_ThroughSphere_Even()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(new Vec3(-5, 0, 0), new Vec3(1, 0.00013, 0.00017));
        int count = sphere.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0, $"Through sphere should be even, got {count}");
    }

    [Fact]
    public void RayCastCount_InsideSphere_OddParity()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0.00013, 0.00017));
        int count = sphere.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1, $"Inside sphere should be odd, got {count}");
    }

    [Fact]
    public void RayCastCount_Deterministic_SameRaySameResult()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(-5, 1, 1), new Vec3(1, 0.00013, 0.00017));
        int c1 = cube.Bvh.RayCastCount(ray);
        int c2 = cube.Bvh.RayCastCount(ray);
        Assert.Equal(c1, c2);
    }

    [Fact]
    public void RayCastCount_AlongYAxis_ThroughCube_Even()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(1, -10, 1), new Vec3(0.00013, 1, 0.00017));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0, $"Y-axis through-ray should be even, got {count}");
    }

    [Fact]
    public void RayCastCount_AlongZAxis_ThroughCube_Even()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(1, 1, -10), new Vec3(0.00013, 0.00017, 1));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0, $"Z-axis through-ray should be even, got {count}");
    }

    [Fact]
    public void RayCastCount_DiagonalRay_ThroughCube()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(-5, -5, -5), new Vec3(1, 1, 1).Normalized);
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count >= 0);
    }

    [Fact]
    public void RayCastCount_Tetrahedron_InsideOdd()
    {
        var tet = MeshFactory.CreateTetrahedron();
        // Tetrahedron centroid: roughly (0.5, y, z)
        var centroid = Vec3.Zero;
        foreach (var v in tet.Mesh.Vertices)
            centroid = centroid + v.Position;
        centroid = centroid / tet.Mesh.Vertices.Count;
        var ray = new Ray(centroid, new Vec3(1, 0.00013, 0.00017));
        int count = tet.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1, $"Inside tetrahedron should be odd, got {count}");
    }

    [Fact]
    public void RayCastCount_FarOutsideSphere_Zero()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var ray = new Ray(new Vec3(100, 100, 100), new Vec3(1, 0, 0));
        int count = sphere.Bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_NonNegative()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var rays = new[]
        {
            new Ray(new Vec3(-5, 1, 1), Vec3.UnitX),
            new Ray(new Vec3(100, 100, 100), Vec3.UnitY),
            new Ray(Vec3.Zero, Vec3.UnitZ)
        };
        foreach (var ray in rays)
        {
            Assert.True(cube.Bvh.RayCastCount(ray) >= 0);
        }
    }
}
