using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 19: ConnectedComponents, BoundaryLoops on CSG results.</summary>
public class ConnectedComponentsTests
{
    [Fact]
    public void SingleCube_OneComponent()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var comps = MeshQueries.ConnectedComponents(cube.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void SingleSphere_OneComponent()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var comps = MeshQueries.ConnectedComponents(sphere.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void DisjointUnion_TwoComponents()
    {
        var a = Primitives.Cube(new Vec3(-5, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(5, 0, 0), 2.0);
        var r = new Solid(Csg.Union(a, b).Mesh);
        var comps = MeshQueries.ConnectedComponents(r.Mesh);
        Assert.True(comps.Count >= 2);
    }

    [Fact]
    public void OverlappingUnion_OneComponent()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var r = new Solid(Csg.Union(a, b).Mesh);
        var comps = MeshQueries.ConnectedComponents(r.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void CsgDifference_OneComponent()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var comps = MeshQueries.ConnectedComponents(r.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void CsgIntersection_OneComponent()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var r = new Solid(Csg.Intersect(a, b).Mesh);
        var comps = MeshQueries.ConnectedComponents(r.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void ClosedMesh_NoBoundaryLoops()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var loops = MeshQueries.BoundaryLoops(cube.Mesh);
        Assert.Empty(loops);
    }

    [Fact]
    public void Sphere_NoBoundaryLoops()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var loops = MeshQueries.BoundaryLoops(sphere.Mesh);
        Assert.Empty(loops);
    }

    [Fact]
    public void Cylinder_NoBoundaryLoops()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16);
        var loops = MeshQueries.BoundaryLoops(cyl.Mesh);
        Assert.Empty(loops);
    }

    [Fact]
    public void Torus_NoBoundaryLoops()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var loops = MeshQueries.BoundaryLoops(torus.Mesh);
        Assert.Empty(loops);
    }

    [Fact]
    public void CsgResult_NoBoundaryLoops()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var loops = MeshQueries.BoundaryLoops(r.Mesh);
        Assert.Empty(loops);
    }

    [Fact]
    public void ComponentsAllHaveFaces()
    {
        var a = Primitives.Cube(new Vec3(-5, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(5, 0, 0), 2.0);
        var r = new Solid(Csg.Union(a, b).Mesh);
        var comps = MeshQueries.ConnectedComponents(r.Mesh);
        foreach (var comp in comps)
            Assert.True(comp.Count > 0);
    }

    [Fact]
    public void AllFacesInSomeComponent()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var comps = MeshQueries.ConnectedComponents(cube.Mesh);
        int totalFaces = comps.Sum(c => c.Count);
        Assert.Equal(cube.Mesh.Faces.Count, totalFaces);
    }

    [Fact]
    public void ThreeDisjointSolids_ThreeComponents()
    {
        var a = Primitives.Cube(new Vec3(-10, 0, 0), 2.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        var c = Primitives.Cube(new Vec3(10, 0, 0), 2.0);
        var r1 = new Solid(Csg.Union(a, b).Mesh);
        var r2 = new Solid(Csg.Union(r1, c).Mesh);
        var comps = MeshQueries.ConnectedComponents(r2.Mesh);
        Assert.True(comps.Count >= 3);
    }

    [Fact]
    public void Cone_OneComponent()
    {
        var cone = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16);
        var comps = MeshQueries.ConnectedComponents(cone.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void Box_OneComponent()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var comps = MeshQueries.ConnectedComponents(box.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void Extrude_OneComponent()
    {
        var profile = new[] { new Vec2(-1, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(-1, 1) };
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 2.0);
        var comps = MeshQueries.ConnectedComponents(solid.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void Revolve_OneComponent()
    {
        var profile = new[] { new Vec2(0, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(0, 1) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 16);
        var comps = MeshQueries.ConnectedComponents(solid.Mesh);
        Assert.Single(comps);
    }

    [Fact]
    public void HalfSpaceSlice_OneComponent()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var r = new Solid(Csg.Intersect(cube, hs).Mesh);
        var comps = MeshQueries.ConnectedComponents(r.Mesh);
        Assert.Single(comps);
    }
}
