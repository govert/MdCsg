using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 21: Serialize→Deserialize every primitive & CSG result.</summary>
public class SerializationRoundtripTests
{
    private static HalfEdgeMesh RoundTrip(HalfEdgeMesh mesh)
    {
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.Default, true))
            MeshSerializer.Serialize(mesh, writer);
        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        return MeshSerializer.Deserialize(reader);
    }

    [Fact]
    public void Cube_RoundTrip_PreservesFaceCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rt = RoundTrip(cube.Mesh);
        Assert.Equal(cube.Mesh.Faces.Count, rt.Faces.Count);
    }

    [Fact]
    public void Cube_RoundTrip_PreservesVertexCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rt = RoundTrip(cube.Mesh);
        Assert.Equal(cube.Mesh.Vertices.Count, rt.Vertices.Count);
    }

    [Fact]
    public void Cube_RoundTrip_PreservesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rt = new Solid(RoundTrip(cube.Mesh));
        Assert.True(System.Math.Abs(cube.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void Sphere_RoundTrip_PreservesVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var rt = new Solid(RoundTrip(sphere.Mesh));
        Assert.True(System.Math.Abs(sphere.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void Cylinder_RoundTrip_PreservesVolume()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16);
        var rt = new Solid(RoundTrip(cyl.Mesh));
        Assert.True(System.Math.Abs(cyl.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void Cone_RoundTrip_PreservesVolume()
    {
        var cone = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16);
        var rt = new Solid(RoundTrip(cone.Mesh));
        Assert.True(System.Math.Abs(cone.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void Torus_RoundTrip_PreservesFaceCount()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var rt = RoundTrip(torus.Mesh);
        Assert.Equal(torus.Mesh.Faces.Count, rt.Faces.Count);
    }

    [Fact]
    public void Box_RoundTrip_PreservesVolume()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var rt = new Solid(RoundTrip(box.Mesh));
        Assert.True(System.Math.Abs(box.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void CsgUnion_RoundTrip_PreservesVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.8, 2);
        var result = new Solid(Csg.Union(a, b).Mesh);
        var rt = new Solid(RoundTrip(result.Mesh));
        Assert.True(System.Math.Abs(result.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void CsgDifference_RoundTrip_PreservesVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var result = new Solid(Csg.Difference(a, b).Mesh);
        var rt = new Solid(RoundTrip(result.Mesh));
        Assert.True(System.Math.Abs(result.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void CsgIntersect_RoundTrip_PreservesVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var result = new Solid(Csg.Intersect(a, b).Mesh);
        var rt = new Solid(RoundTrip(result.Mesh));
        Assert.True(System.Math.Abs(result.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void Extrude_RoundTrip_PreservesVolume()
    {
        var profile = new[] { new Vec2(-1, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(-1, 1) };
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 2.0);
        var rt = new Solid(RoundTrip(solid.Mesh));
        Assert.True(System.Math.Abs(solid.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void Revolve_RoundTrip_PreservesVolume()
    {
        var profile = new[] { new Vec2(0, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(0, 1) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 16);
        var rt = new Solid(RoundTrip(solid.Mesh));
        Assert.True(System.Math.Abs(solid.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void RoundTrip_PreservesIsInside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rt = new Solid(RoundTrip(cube.Mesh));
        Assert.True(rt.IsInside(Vec3.Zero));
        Assert.False(rt.IsInside(new Vec3(5, 5, 5)));
    }

    [Fact]
    public void RoundTrip_PreservesSurfaceArea()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rt = new Solid(RoundTrip(cube.Mesh));
        Assert.True(System.Math.Abs(cube.SurfaceArea() - rt.SurfaceArea()) < 0.01);
    }

    [Fact]
    public void MultipleRoundTrips_Stable()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var mesh = cube.Mesh;
        for (int i = 0; i < 3; i++)
            mesh = RoundTrip(mesh);
        Assert.Equal(cube.Mesh.Faces.Count, mesh.Faces.Count);
    }

    [Fact]
    public void HalfSpaceSlice_RoundTrip_PreservesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var result = new Solid(Csg.Intersect(cube, hs).Mesh);
        var rt = new Solid(RoundTrip(result.Mesh));
        Assert.True(System.Math.Abs(result.Volume() - rt.Volume()) < 0.01);
    }

    [Fact]
    public void LargeMesh_RoundTrip_PreservesCounts()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var rt = RoundTrip(sphere.Mesh);
        Assert.Equal(sphere.Mesh.Faces.Count, rt.Faces.Count);
        Assert.Equal(sphere.Mesh.Vertices.Count, rt.Vertices.Count);
    }

    [Fact]
    public void RoundTrip_CanBuildSolidFromResult()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rt = RoundTrip(cube.Mesh);
        var solid = new Solid(rt);
        Assert.True(solid.Volume() > 0);
        Assert.NotNull(solid.Bvh);
    }
}
