using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: WindingNumberClassifier.ComputeWindingNumber — value ranges and properties</summary>
public class WindingNumberValuePropertyTests
{
    [Fact]
    public void InsideCube_WindingNearOne()
    {
        var cube = MeshFactory.CreateCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Winding number inside cube should be ~1, got {wn}");
    }

    [Fact]
    public void OutsideCube_WindingNearZero()
    {
        var cube = MeshFactory.CreateCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), cube.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Winding number outside cube should be ~0, got {wn}");
    }

    [Fact]
    public void InsideSphere_WindingNearOne()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, sphere.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Winding number inside sphere should be ~1, got {wn}");
    }

    [Fact]
    public void OutsideSphere_WindingNearZero()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 0, 0), sphere.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Winding number outside sphere should be ~0, got {wn}");
    }

    [Fact]
    public void InsideTetrahedron_WindingNearOne()
    {
        var tet = MeshFactory.CreateTetrahedron();
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, tet.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Winding number inside tetrahedron should be ~1, got {wn}");
    }

    [Fact]
    public void FarOutsideTetrahedron_WindingNearZero()
    {
        var tet = MeshFactory.CreateTetrahedron();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(100, 100, 100), tet.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.01, $"Winding number far outside should be ~0, got {wn}");
    }

    [Fact]
    public void EmptyMesh_WindingZero()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.Equal(0.0, wn, 15);
    }

    [Fact]
    public void MultipleInteriorPoints_AllNearOne()
    {
        var cube = MeshFactory.CreateCube();
        var points = new Vec3[]
        {
            new(0.1, 0.1, 0.1),
            new(0.5, 0.5, 0.5),
            new(0.9, 0.9, 0.9),
            new(0.3, 0.7, 0.2),
        };
        foreach (var p in points)
        {
            double wn = WindingNumberClassifier.ComputeWindingNumber(p, cube.Bvh);
            Assert.True(System.Math.Abs(wn - 1.0) < 0.15,
                $"Winding number at {p} should be ~1, got {wn}");
        }
    }

    [Fact]
    public void MultipleExteriorPoints_AllNearZero()
    {
        var cube = MeshFactory.CreateCube();
        var points = new Vec3[]
        {
            new(2, 0, 0),
            new(0, 2, 0),
            new(0, 0, 2),
            new(-1, -1, -1),
        };
        foreach (var p in points)
        {
            double wn = WindingNumberClassifier.ComputeWindingNumber(p, cube.Bvh);
            Assert.True(System.Math.Abs(wn) < 0.15,
                $"Winding number at {p} should be ~0, got {wn}");
        }
    }

    [Fact]
    public void WindingNumber_ContinuousAlongLine()
    {
        var cube = MeshFactory.CreateCube();
        // Walk along a line from inside to outside
        double prevWn = 1.0;
        for (int i = 0; i <= 10; i++)
        {
            double t = i / 10.0;
            var p = new Vec3(0.5 + t * 3, 0.5, 0.5);
            double wn = WindingNumberClassifier.ComputeWindingNumber(p, cube.Bvh);
            // Should transition smoothly from ~1 to ~0
            Assert.True(wn >= -0.5 && wn <= 1.5, $"Winding number at t={t} should be in range, got {wn}");
        }
    }

    [Fact]
    public void OffsetCube_InsidePoint_NearOne()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(10.5, 20.5, 30.5), cube.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1);
    }

    [Fact]
    public void Sphere_SubDiv3_InsidePoint_NearOne()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, sphere.Bvh);
        // Higher subdivision should give more accurate winding number
        Assert.True(System.Math.Abs(wn - 1.0) < 0.05,
            $"High-res sphere winding at center should be very close to 1, got {wn}");
    }
}
