using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree ray-triangle intersection — Moller-Trumbore hit/miss, AABB intersection, edge cases</summary>
public class BvhRayIntersectsTriangleMollerTests
{
    [Fact]
    public void RayCastCount_EmptyMesh_ReturnsZero()
    {
        var mesh = new HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        int count = bvh.RayCastCount(new Ray(Vec3.Zero, new Vec3(1, 0, 0)));
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_RayHitsSingleTriangle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(-1, -1, 5));
        var v1 = mesh.AddVertex(new Vec3(1, -1, 5));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 5));
        mesh.AddFace(v0, v1, v2);
        var bvh = BvhTree.Build(mesh);
        // Ray along +Z from origin should hit the triangle at z=5
        int count = bvh.RayCastCount(new Ray(Vec3.Zero, new Vec3(0, 0, 1)));
        Assert.Equal(1, count);
    }

    [Fact]
    public void RayCastCount_RayMissesSingleTriangle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(-1, -1, 5));
        var v1 = mesh.AddVertex(new Vec3(1, -1, 5));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 5));
        mesh.AddFace(v0, v1, v2);
        var bvh = BvhTree.Build(mesh);
        // Ray along +X from (10,10,10) should miss
        int count = bvh.RayCastCount(new Ray(new Vec3(10, 10, 10), new Vec3(1, 0, 0)));
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_RayBehindTriangle_NoHit()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(-1, -1, -5));
        var v1 = mesh.AddVertex(new Vec3(1, -1, -5));
        var v2 = mesh.AddVertex(new Vec3(0, 1, -5));
        mesh.AddFace(v0, v1, v2);
        var bvh = BvhTree.Build(mesh);
        // Ray along +Z from origin; triangle is behind at z=-5
        int count = bvh.RayCastCount(new Ray(Vec3.Zero, new Vec3(0, 0, 1)));
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_PerturbedRay_InsideCube_HitsOnce()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        // Perturbed direction avoids edge hits
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(1, count);
    }

    [Fact]
    public void RayCastCount_PerturbedRay_OutsideCube_HitsEven()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        // From outside, perturbed ray through cube should hit even number of faces
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count % 2);
    }

    [Fact]
    public void RayCastCount_MultipleDirections_InsideSphere()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        var directions = new Vec3[]
        {
            new Vec3(1, 0.00013, 0.00017).Normalized,
            new Vec3(0.00019, 1, 0.00023).Normalized,
            new Vec3(0.00029, 0.00031, 1).Normalized,
        };
        foreach (var dir in directions)
        {
            var ray = new Ray(Vec3.Zero, dir);
            int count = bvh.RayCastCount(ray);
            Assert.Equal(1, count % 2); // Inside → odd
        }
    }

    [Fact]
    public void RayCastCount_MultipleDirections_OutsideSphere()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var bvh = BvhTree.Build(sphere.Mesh);
        var directions = new Vec3[]
        {
            new Vec3(1, 0.00013, 0.00017).Normalized,
            new Vec3(0.00019, 1, 0.00023).Normalized,
            new Vec3(0.00029, 0.00031, 1).Normalized,
        };
        foreach (var dir in directions)
        {
            var ray = new Ray(new Vec3(5, 5, 5), dir);
            int count = bvh.RayCastCount(ray);
            Assert.Equal(0, count % 2); // Outside → even
        }
    }

    [Fact]
    public void RayCastCount_ParallelToTriangle_NoHit()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var bvh = BvhTree.Build(mesh);
        // Ray parallel to XY plane at z=0 should not hit
        var ray = new Ray(new Vec3(-5, 0.25, 0), new Vec3(1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_TwoTriangles_HitsBoth()
    {
        var mesh = new HalfEdgeMesh();
        // Two parallel triangles at z=2 and z=5
        var v0 = mesh.AddVertex(new Vec3(-1, -1, 2));
        var v1 = mesh.AddVertex(new Vec3(1, -1, 2));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 2));
        mesh.AddFace(v0, v1, v2);
        var v3 = mesh.AddVertex(new Vec3(-1, -1, 5));
        var v4 = mesh.AddVertex(new Vec3(1, -1, 5));
        var v5 = mesh.AddVertex(new Vec3(0, 1, 5));
        mesh.AddFace(v3, v4, v5);
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(Vec3.Zero, new Vec3(0, 0, 1));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(2, count);
    }

    [Fact]
    public void RayCastCount_Tetrahedron_InsideIsOdd()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var bvh = BvhTree.Build(tet.Mesh);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(1, count % 2);
    }

    [Fact]
    public void RayCastCount_Tetrahedron_OutsideIsEven()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var bvh = BvhTree.Build(tet.Mesh);
        var ray = new Ray(new Vec3(100, 100, 100), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count % 2);
    }

    [Fact]
    public void RayCastCount_NonNegative()
    {
        var cube = MeshFactory.CreateCube();
        var bvh = BvhTree.Build(cube.Mesh);
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.3, 0.7).Normalized);
        Assert.True(bvh.RayCastCount(ray) >= 0);
    }
}
