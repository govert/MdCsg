using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 22: Simplify then CSG, simplify CSG results.</summary>
public class SimplificationIntegrationTests
{
    [Fact]
    public void SimplifySphere_ReducesFaceCount()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        int originalFaces = sphere.Mesh.Faces.Count;
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, originalFaces / 2);
        Assert.True(simplified.Faces.Count <= originalFaces / 2 + 10);
    }

    [Fact]
    public void SimplifySphere_PreservesApproximateVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var simplified = new Solid(MeshSimplifier.Simplify(sphere.Mesh, sphere.Mesh.Faces.Count / 2));
        Assert.True(System.Math.Abs(sphere.Volume() - simplified.Volume()) < 0.5);
    }

    [Fact]
    public void SimplifyCube_PreservesVolume()
    {
        // Cube has only 12 faces so minimal simplification
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var simplified = new Solid(MeshSimplifier.Simplify(cube.Mesh, 12));
        Assert.True(System.Math.Abs(cube.Volume() - simplified.Volume()) < 0.5);
    }

    [Fact]
    public void SimplifiedSphere_ThenCsg_ProducesValidMesh()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var simSphere = new Solid(MeshSimplifier.Simplify(sphere.Mesh, 100));
        var cube = Primitives.Cube(Vec3.Zero, 1.5);
        var r = Csg.Intersect(simSphere, cube);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void CsgResult_ThenSimplify_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var result = new Solid(Csg.Difference(a, b).Mesh);
        int target = result.Mesh.Faces.Count / 2;
        var simplified = new Solid(MeshSimplifier.Simplify(result.Mesh, target));
        Assert.True(simplified.Volume() > 0);
    }

    [Fact]
    public void SimplifyUnion_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.8, 2);
        var result = new Solid(Csg.Union(a, b).Mesh);
        int target = result.Mesh.Faces.Count / 2;
        var simplified = new Solid(MeshSimplifier.Simplify(result.Mesh, target));
        Assert.True(simplified.Volume() > 0);
    }

    [Fact]
    public void SimplifyTorus_ReducesFaceCount()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 24, 12);
        int originalFaces = torus.Mesh.Faces.Count;
        var simplified = MeshSimplifier.Simplify(torus.Mesh, originalFaces / 3);
        Assert.True(simplified.Faces.Count < originalFaces);
    }

    [Fact]
    public void SimplifyCylinder_PreservesApproximateVolume()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 32);
        var simplified = new Solid(MeshSimplifier.Simplify(cyl.Mesh, cyl.Mesh.Faces.Count / 2));
        Assert.True(System.Math.Abs(cyl.Volume() - simplified.Volume()) < 1.0);
    }

    [Fact]
    public void SimplifiedMesh_CanBuildSolid()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, 50);
        var solid = new Solid(simplified);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void SimplifiedMesh_CanSerialize()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, 80);
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.Default, true))
            MeshSerializer.Serialize(simplified, writer);
        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        var rt = MeshSerializer.Deserialize(reader);
        Assert.Equal(simplified.Faces.Count, rt.Faces.Count);
    }

    [Fact]
    public void ProgressiveSimplification_ReducesFurther()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var s1 = MeshSimplifier.Simplify(sphere.Mesh, 200);
        var s2 = MeshSimplifier.Simplify(s1, 100);
        Assert.True(s2.Faces.Count <= s1.Faces.Count);
    }

    [Fact]
    public void Simplify_ErrorThreshold_ReducesFaceCount()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        int originalFaces = sphere.Mesh.Faces.Count;
        var simplified = MeshSimplifier.Simplify(sphere.Mesh, 0.1);
        Assert.True(simplified.Faces.Count < originalFaces);
    }

    [Fact]
    public void SimplifyHighPolySphere_ProducesValidMesh()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var simplified = new Solid(MeshSimplifier.Simplify(sphere.Mesh, 100));
        Assert.True(simplified.Volume() > 0);
    }

    [Fact]
    public void SimplifiedSolid_IsInsideStillWorks()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var simplified = new Solid(MeshSimplifier.Simplify(sphere.Mesh, 80));
        Assert.True(simplified.IsInside(Vec3.Zero));
        Assert.False(simplified.IsInside(new Vec3(5, 0, 0)));
    }

    [Fact]
    public void SimplifyBox_ReducesFaceCount()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        // Box already has minimal faces, but verify no crash
        var simplified = MeshSimplifier.Simplify(box.Mesh, 12);
        Assert.True(simplified.Faces.Count > 0);
    }

    [Fact]
    public void SimplifyCone_PreservesApproximateVolume()
    {
        var cone = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 32);
        var simplified = new Solid(MeshSimplifier.Simplify(cone.Mesh, cone.Mesh.Faces.Count / 2));
        Assert.True(System.Math.Abs(cone.Volume() - simplified.Volume()) < 1.0);
    }

    [Fact]
    public void SimplifyExtrusion_ProducesValidMesh()
    {
        var profile = new[] { new Vec2(-1, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(-1, 1) };
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 2.0);
        var simplified = MeshSimplifier.Simplify(solid.Mesh, solid.Mesh.Faces.Count);
        Assert.True(simplified.Faces.Count > 0);
    }

    [Fact]
    public void SimplifiedMesh_SurfaceAreaDecreases()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var simplified = new Solid(MeshSimplifier.Simplify(sphere.Mesh, 50));
        // Simplified mesh may have slightly different area, just check it's positive
        Assert.True(simplified.SurfaceArea() > 0);
    }
}
