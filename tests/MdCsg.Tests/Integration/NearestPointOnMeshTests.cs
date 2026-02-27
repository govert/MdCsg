using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 17: NearestPointOnMesh on CSG results.</summary>
public class NearestPointOnMeshTests
{
    [Fact]
    public void CubeCenter_NearestPointOnSurface()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var (pt, _, dist) = MeshQueries.NearestPointOnMesh(Vec3.Zero, cube.Mesh, cube.Bvh);
        Assert.True(dist > 0);
        Assert.True(dist < 1.1); // half-extent is 1
    }

    [Fact]
    public void SphereCenter_NearestPointApproximatelyRadius()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(Vec3.Zero, sphere.Mesh, sphere.Bvh);
        Assert.True(System.Math.Abs(dist - 1.0) < 0.1);
    }

    [Fact]
    public void PointOutsideCube_NearestPointOnFace()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var query = new Vec3(0, 0, 5);
        var (pt, _, dist) = MeshQueries.NearestPointOnMesh(query, cube.Mesh, cube.Bvh);
        Assert.True(System.Math.Abs(dist - 4.0) < 0.1);
    }

    [Fact]
    public void PointOnSurface_DistanceNearZero()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var query = new Vec3(0, 0, 1); // exactly on surface
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(query, cube.Mesh, cube.Bvh);
        Assert.True(dist < 0.1);
    }

    [Fact]
    public void NearestOnCsgResult_ProducesValidResult()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var (pt, faceIdx, dist) = MeshQueries.NearestPointOnMesh(Vec3.Zero, r.Mesh, r.Bvh);
        Assert.True(dist > 0);
        Assert.True(faceIdx >= 0);
    }

    [Fact]
    public void NearestOnUnion_ProducesValidResult()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(1, 0, 0), 0.8, 2);
        var r = new Solid(Csg.Union(a, b).Mesh);
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(new Vec3(5, 0, 0), r.Mesh, r.Bvh);
        Assert.True(dist > 0);
    }

    [Fact]
    public void NearestOnIntersection_ProducesValidResult()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var r = new Solid(Csg.Intersect(a, b).Mesh);
        var (_, faceIdx, dist) = MeshQueries.NearestPointOnMesh(new Vec3(3, 0, 0), r.Mesh, r.Bvh);
        Assert.True(dist > 0);
        Assert.True(faceIdx >= 0);
    }

    [Fact]
    public void NearestOnCylinder_ProducesValidResult()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(new Vec3(3, 0, 0), cyl.Mesh, cyl.Bvh);
        Assert.True(dist > 1.5);
    }

    [Fact]
    public void NearestOnTorus_ProducesValidResult()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(Vec3.Zero, torus.Mesh, torus.Bvh);
        Assert.True(dist > 0.5); // center is outside the tube
    }

    [Fact]
    public void NearestDistanceIsNonNegative()
    {
        var solids = new[]
        {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(Vec3.Zero, 1.0, 2),
            Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1)),
        };
        foreach (var s in solids)
        {
            var (_, _, dist) = MeshQueries.NearestPointOnMesh(new Vec3(5, 0, 0), s.Mesh, s.Bvh);
            Assert.True(dist >= 0);
        }
    }

    [Fact]
    public void NearestPointOnFace_IsOnMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var (pt, faceIdx, _) = MeshQueries.NearestPointOnMesh(new Vec3(5, 0, 0), cube.Mesh, cube.Bvh);
        Assert.True(faceIdx >= 0 && faceIdx < cube.Mesh.Faces.Count);
    }

    [Fact]
    public void MultipleQueries_AllValid()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var queries = new[]
        {
            new Vec3(2, 0, 0), new Vec3(0, 2, 0), new Vec3(0, 0, 2),
            new Vec3(-2, 0, 0), new Vec3(0, -2, 0),
        };
        foreach (var q in queries)
        {
            var (_, faceIdx, dist) = MeshQueries.NearestPointOnMesh(q, sphere.Mesh, sphere.Bvh);
            Assert.True(dist > 0);
            Assert.True(faceIdx >= 0);
        }
    }

    [Fact]
    public void NearestOnBox_CornerQuery()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(2, 2, 2));
        var corner = new Vec3(2, 2, 2);
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(corner, box.Mesh, box.Bvh);
        Assert.True(dist > 0);
    }

    [Fact]
    public void NearestOnSlicedSolid_ProducesValidResult()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var sliced = new Solid(Csg.Intersect(cube, hs).Mesh);
        var (_, faceIdx, dist) = MeshQueries.NearestPointOnMesh(new Vec3(0, 0, 3), sliced.Mesh, sliced.Bvh);
        Assert.True(dist > 0);
        Assert.True(faceIdx >= 0);
    }

    [Fact]
    public void NearestSymmetric_OppositePoints()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var (_, _, dist1) = MeshQueries.NearestPointOnMesh(new Vec3(3, 0, 0), sphere.Mesh, sphere.Bvh);
        var (_, _, dist2) = MeshQueries.NearestPointOnMesh(new Vec3(-3, 0, 0), sphere.Mesh, sphere.Bvh);
        Assert.True(System.Math.Abs(dist1 - dist2) < 0.1);
    }

    [Fact]
    public void NearestOnCone_ProducesValidResult()
    {
        var cone = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(new Vec3(0, 0, 5), cone.Mesh, cone.Bvh);
        Assert.True(dist > 0);
    }

    [Fact]
    public void NearestOnFarPoint_LargeDistance()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(new Vec3(100, 0, 0), cube.Mesh, cube.Bvh);
        Assert.True(dist > 90);
    }

    [Fact]
    public void NearestOnExtrudedSolid_ProducesValidResult()
    {
        var profile = new[] { new Vec2(-1, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(-1, 1) };
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 2.0);
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(new Vec3(5, 0, 1), solid.Mesh, solid.Bvh);
        Assert.True(dist > 0);
    }
}
