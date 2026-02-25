using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree.RayCastCount — accuracy for known geometries, edge cases</summary>
public class BvhRayCastCountAccuracyTests
{
    [Fact]
    public void RayThroughCubeCenter_Hits2Faces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from well inside along +X should hit exactly 1 face (exit face)
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017));
        int count = bvh.RayCastCount(ray);
        Assert.True(count >= 1, $"Ray through cube should hit >= 1 face, got {count}");
    }

    [Fact]
    public void RayFromOutside_ThroughCube_Hits2()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from -X through center
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(2, count);
    }

    [Fact]
    public void RayMissingCube_HitsZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray far above the cube
        var ray = new Ray(new Vec3(0.5, 0.5, 100), new Vec3(1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayFromInside_HitsOddCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from inside cube with perturbed direction to avoid edge hits
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017));
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1, $"Inside ray should hit odd count, got {count}");
    }

    [Fact]
    public void RayFromOutside_HitsEvenCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from outside: should hit even number (enter+exit = 2)
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0, $"Outside ray should hit even count, got {count}");
    }

    [Fact]
    public void RayBackward_HitsZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray pointing away from cube
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(-1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void EmptyMesh_HitsZero()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void SingleTriangle_RayThrough_Hits1()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(2, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(1, 2, 0));
        mesh.AddFace(v0, v1, v2);
        var bvh = BvhTree.Build(mesh);

        // Ray along -Z toward the triangle in XY plane
        var ray = new Ray(new Vec3(1, 0.5, 5), new Vec3(0, 0, -1));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(1, count);
    }

    [Fact]
    public void SingleTriangle_RayMiss_Hits0()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(2, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(1, 2, 0));
        mesh.AddFace(v0, v1, v2);
        var bvh = BvhTree.Build(mesh);

        // Ray far from triangle
        var ray = new Ray(new Vec3(100, 100, 5), new Vec3(0, 0, -1));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void Sphere_RayFromOutside_Hits2()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from far away through center
        var ray = new Ray(new Vec3(-5, 0, 0), new Vec3(1, 0.00013, 0.00017));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(2, count);
    }

    [Fact]
    public void Tetrahedron_RayFromOutside_HitsEven()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from far outside the tetrahedron
        var ray = new Ray(new Vec3(-10, 0.1, 0.1), new Vec3(1, 0.00013, 0.00017));
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0, $"Outside ray should hit even count, got {count}");
    }

    [Fact]
    public void MultipleDirections_ConsistentInsideOutside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var insidePoint = new Vec3(0.5, 0.5, 0.5);

        var directions = new[]
        {
            new Vec3(1, 0.00013, 0.00017),
            new Vec3(0.00013, 1, 0.00017),
            new Vec3(0.00013, 0.00017, 1),
        };

        foreach (var dir in directions)
        {
            var ray = new Ray(insidePoint, dir);
            int count = bvh.RayCastCount(ray);
            Assert.True(count % 2 == 1,
                $"Inside point with dir {dir} should give odd count, got {count}");
        }
    }
}
