using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 20: VertexNormal quality on various meshes.</summary>
public class VertexNormalIntegrationTests
{
    [Fact]
    public void CubeVertexNormals_AreNonZero()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        foreach (var v in cube.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, cube.Mesh);
            Assert.True(n.Length > 0.1);
        }
    }

    [Fact]
    public void SphereVertexNormals_PointOutward()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        foreach (var v in sphere.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, sphere.Mesh);
            // For a sphere centered at origin, normals should point away from center
            Assert.True(Vec3.Dot(n, v.Position.Normalized) > 0);
        }
    }

    [Fact]
    public void SphereVertexNormals_AreUnitLength()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        foreach (var v in sphere.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, sphere.Mesh);
            Assert.True(System.Math.Abs(n.Length - 1.0) < 0.01);
        }
    }

    [Fact]
    public void CylinderVertexNormals_AreNonZero()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16);
        foreach (var v in cyl.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, cyl.Mesh);
            Assert.True(n.Length > 0.1);
        }
    }

    [Fact]
    public void ConeVertexNormals_AreNonZero()
    {
        var cone = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16);
        foreach (var v in cone.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, cone.Mesh);
            Assert.True(n.Length > 0.01);
        }
    }

    [Fact]
    public void TorusVertexNormals_AreNonZero()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        foreach (var v in torus.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, torus.Mesh);
            Assert.True(n.Length > 0.1);
        }
    }

    [Fact]
    public void TorusVertexNormals_AreUnitLength()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        foreach (var v in torus.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, torus.Mesh);
            Assert.True(System.Math.Abs(n.Length - 1.0) < 0.01);
        }
    }

    [Fact]
    public void CsgResultNormals_AreNonZero()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        foreach (var v in r.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, r.Mesh);
            Assert.True(n.Length > 0.01);
        }
    }

    [Fact]
    public void CsgUnionNormals_AreNonZero()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.8, 2);
        var r = new Solid(Csg.Union(a, b).Mesh);
        foreach (var v in r.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, r.Mesh);
            Assert.True(n.Length > 0.01);
        }
    }

    [Fact]
    public void BoxNormals_AreNonZero()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        foreach (var v in box.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, box.Mesh);
            Assert.True(n.Length > 0.1);
        }
    }

    [Fact]
    public void SphereNormals_ApproximatelyEqualToPositionNormalized()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        foreach (var v in sphere.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, sphere.Mesh);
            var expected = v.Position.Normalized;
            Assert.True(Vec3.Distance(n, expected) < 0.15);
        }
    }

    [Fact]
    public void ExtrudedSolid_NormalsAreNonZero()
    {
        var profile = new[] { new Vec2(-1, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(-1, 1) };
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 2.0);
        foreach (var v in solid.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, solid.Mesh);
            Assert.True(n.Length > 0.01);
        }
    }

    [Fact]
    public void RevolvedSolid_NormalsAreNonZero()
    {
        var profile = new[] { new Vec2(0, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(0, 1) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 16);
        foreach (var v in solid.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, solid.Mesh);
            Assert.True(n.Length > 0.01);
        }
    }

    [Fact]
    public void NormalsAreUnitLength_Cube()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        foreach (var v in cube.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, cube.Mesh);
            Assert.True(System.Math.Abs(n.Length - 1.0) < 0.01);
        }
    }

    [Fact]
    public void SlicedCube_NormalsAreNonZero()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var r = new Solid(Csg.Intersect(cube, hs).Mesh);
        foreach (var v in r.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, r.Mesh);
            Assert.True(n.Length > 0.01);
        }
    }

    [Fact]
    public void HighSubdivisionSphere_NormalsAccurate()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        double maxError = 0;
        foreach (var v in sphere.Mesh.Vertices)
        {
            var n = MeshQueries.VertexNormal(v, sphere.Mesh);
            var expected = v.Position.Normalized;
            double err = Vec3.Distance(n, expected);
            if (err > maxError) maxError = err;
        }
        Assert.True(maxError < 0.1);
    }

    [Fact]
    public void AllNormalsHaveFiniteComponents()
    {
        var shapes = new[]
        {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(Vec3.Zero, 1.0, 2),
            Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16),
        };
        foreach (var s in shapes)
            foreach (var v in s.Mesh.Vertices)
            {
                var n = MeshQueries.VertexNormal(v, s.Mesh);
                Assert.False(double.IsNaN(n.X) || double.IsNaN(n.Y) || double.IsNaN(n.Z));
                Assert.False(double.IsInfinity(n.X) || double.IsInfinity(n.Y) || double.IsInfinity(n.Z));
            }
    }

    [Fact]
    public void CylinderSideNormals_RadialDirection()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 32);
        // Check a side vertex (not cap center)
        var sideVerts = cyl.Mesh.Vertices.Where(v =>
            v.Position.Z > 0.01 && v.Position.Z < 1.99 &&
            System.Math.Sqrt(v.Position.X * v.Position.X + v.Position.Y * v.Position.Y) > 0.5).ToList();
        if (sideVerts.Count > 0)
        {
            var v = sideVerts[0];
            var n = MeshQueries.VertexNormal(v, cyl.Mesh);
            // Should be roughly radial (Z component small)
            var radial = new Vec3(v.Position.X, v.Position.Y, 0).Normalized;
            Assert.True(Vec3.Dot(n, radial) > 0);
        }
    }
}
