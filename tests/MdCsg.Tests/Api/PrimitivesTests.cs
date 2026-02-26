using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

public class PrimitivesTests
{
    // =========================================================================
    // Box
    // =========================================================================

    [Fact]
    public void Box_UnitAtOrigin_Has8Vertices12Faces()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(8, box.Mesh.Vertices.Count);
        Assert.Equal(12, box.Mesh.Faces.Count);
    }

    [Fact]
    public void Box_UnitAtOrigin_IsClosedManifold()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(1, 1, 1));
        MeshAssertions.AssertClosedManifold(box.Mesh);
    }

    [Fact]
    public void Box_UnitAtOrigin_HasUnitVolume()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(1, 1, 1));
        MeshAssertions.AssertVolume(box.Mesh, 1.0, 0.01);
    }

    [Fact]
    public void Box_Rectangular_HasCorrectVolume()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(2, 3, 4));
        MeshAssertions.AssertVolume(box.Mesh, 24.0, 0.01);
    }

    [Fact]
    public void Box_OffCenter_HasCorrectVolume()
    {
        var box = Primitives.Box(new Vec3(5, 5, 5), new Vec3(2, 2, 2));
        MeshAssertions.AssertVolume(box.Mesh, 8.0, 0.01);
    }

    // =========================================================================
    // Cube
    // =========================================================================

    [Fact]
    public void Cube_UnitAtOrigin_IsClosedManifold()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1.0);
        MeshAssertions.AssertClosedManifold(cube.Mesh);
    }

    [Fact]
    public void Cube_UnitAtOrigin_HasUnitVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1.0);
        MeshAssertions.AssertVolume(cube.Mesh, 1.0, 0.01);
    }

    [Fact]
    public void Cube_Extent2_HasVolume8()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        MeshAssertions.AssertVolume(cube.Mesh, 8.0, 0.01);
    }

    // =========================================================================
    // Sphere
    // =========================================================================

    [Fact]
    public void Sphere_Sub0_IsIcosahedron_20Faces()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 0);
        Assert.Equal(20, sphere.Mesh.Faces.Count);
        Assert.Equal(12, sphere.Mesh.Vertices.Count);
    }

    [Fact]
    public void Sphere_Sub2_IsClosedManifold()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        MeshAssertions.AssertClosedManifold(sphere.Mesh);
    }

    [Fact]
    public void Sphere_Sub2_HasApproximateVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        double expected = 4.0 / 3.0 * System.Math.PI; // ~4.189
        double actual = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        Assert.True(System.Math.Abs(actual - expected) < 0.2,
            $"Sphere volume: expected ~{expected:F3}, got {actual:F3}");
    }

    [Fact]
    public void Sphere_Sub3_ApproachesAnalyticVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        double expected = 4.0 / 3.0 * System.Math.PI;
        double actual = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        Assert.True(System.Math.Abs(actual - expected) < 0.05,
            $"Sphere volume sub3: expected ~{expected:F3}, got {actual:F3}");
    }

    [Fact]
    public void Sphere_Sub1_Has80Faces()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 1);
        Assert.Equal(80, sphere.Mesh.Faces.Count);
    }

    // =========================================================================
    // Cylinder
    // =========================================================================

    [Fact]
    public void Cylinder_ZAxis_IsClosedManifold()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, new Vec3(0, 0, 1), 1.0, 2.0, 24);
        MeshAssertions.AssertClosedManifold(cyl.Mesh);
    }

    [Fact]
    public void Cylinder_ZAxis_HasCorrectVertexCount()
    {
        int seg = 24;
        var cyl = Primitives.Cylinder(Vec3.Zero, new Vec3(0, 0, 1), 1.0, 2.0, seg);
        // 2 cap centers + 2 rings of seg vertices
        Assert.Equal(2 + 2 * seg, cyl.Mesh.Vertices.Count);
    }

    [Fact]
    public void Cylinder_ZAxis_HasCorrectFaceCount()
    {
        int seg = 24;
        var cyl = Primitives.Cylinder(Vec3.Zero, new Vec3(0, 0, 1), 1.0, 2.0, seg);
        // seg bottom + seg top + 2*seg side
        Assert.Equal(4 * seg, cyl.Mesh.Faces.Count);
    }

    [Fact]
    public void Cylinder_UnitRadius_HasApproximateVolume()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, new Vec3(0, 0, 1), 1.0, 2.0, 64);
        double expected = System.Math.PI * 1.0 * 1.0 * 2.0; // pi*r^2*h
        double actual = VolumeCalculator.ComputeAbsoluteVolume(cyl.Mesh);
        Assert.True(System.Math.Abs(actual - expected) < 0.1,
            $"Cylinder volume: expected ~{expected:F3}, got {actual:F3}");
    }

    [Fact]
    public void Cylinder_ArbitraryAxis_IsClosedManifold()
    {
        var cyl = Primitives.Cylinder(new Vec3(1, 2, 3), new Vec3(1, 1, 1), 0.5, 3.0, 16);
        MeshAssertions.AssertClosedManifold(cyl.Mesh);
    }

    // =========================================================================
    // Cone
    // =========================================================================

    [Fact]
    public void Cone_ZAxis_IsClosedManifold()
    {
        var cone = Primitives.Cone(Vec3.Zero, new Vec3(0, 0, 1), 1.0, 2.0, 24);
        MeshAssertions.AssertClosedManifold(cone.Mesh);
    }

    [Fact]
    public void Cone_ZAxis_HasCorrectVertexCount()
    {
        int seg = 24;
        var cone = Primitives.Cone(Vec3.Zero, new Vec3(0, 0, 1), 1.0, 2.0, seg);
        // 1 base center + 1 apex + seg ring
        Assert.Equal(2 + seg, cone.Mesh.Vertices.Count);
    }

    [Fact]
    public void Cone_ZAxis_HasCorrectFaceCount()
    {
        int seg = 24;
        var cone = Primitives.Cone(Vec3.Zero, new Vec3(0, 0, 1), 1.0, 2.0, seg);
        // seg base + seg side
        Assert.Equal(2 * seg, cone.Mesh.Faces.Count);
    }

    [Fact]
    public void Cone_UnitRadius_HasApproximateVolume()
    {
        var cone = Primitives.Cone(Vec3.Zero, new Vec3(0, 0, 1), 1.0, 3.0, 64);
        double expected = System.Math.PI * 1.0 * 1.0 * 3.0 / 3.0; // pi*r^2*h/3
        double actual = VolumeCalculator.ComputeAbsoluteVolume(cone.Mesh);
        Assert.True(System.Math.Abs(actual - expected) < 0.1,
            $"Cone volume: expected ~{expected:F3}, got {actual:F3}");
    }

    // =========================================================================
    // Torus
    // =========================================================================

    [Fact]
    public void Torus_ZAxis_IsClosedManifold()
    {
        var torus = Primitives.Torus(Vec3.Zero, new Vec3(0, 0, 1), 2.0, 0.5, 24, 12);
        MeshAssertions.AssertClosedManifold(torus.Mesh, expectedEulerCharacteristic: 0);
    }

    [Fact]
    public void Torus_ZAxis_HasCorrectVertexCount()
    {
        int maj = 24, min = 12;
        var torus = Primitives.Torus(Vec3.Zero, new Vec3(0, 0, 1), 2.0, 0.5, maj, min);
        Assert.Equal(maj * min, torus.Mesh.Vertices.Count);
    }

    [Fact]
    public void Torus_ZAxis_HasCorrectFaceCount()
    {
        int maj = 24, min = 12;
        var torus = Primitives.Torus(Vec3.Zero, new Vec3(0, 0, 1), 2.0, 0.5, maj, min);
        Assert.Equal(2 * maj * min, torus.Mesh.Faces.Count);
    }

    [Fact]
    public void Torus_HasApproximateVolume()
    {
        var torus = Primitives.Torus(Vec3.Zero, new Vec3(0, 0, 1), 2.0, 0.5, 48, 24);
        // V = 2 * pi^2 * R * r^2
        double expected = 2.0 * System.Math.PI * System.Math.PI * 2.0 * 0.5 * 0.5;
        double actual = VolumeCalculator.ComputeAbsoluteVolume(torus.Mesh);
        Assert.True(System.Math.Abs(actual - expected) < 0.2,
            $"Torus volume: expected ~{expected:F3}, got {actual:F3}");
    }

    // =========================================================================
    // CSG Integration between primitives
    // =========================================================================

    [Fact]
    public void CsgIntersect_SphereCylinder_ProducesFaces()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -2), new Vec3(0, 0, 1), 0.5, 4.0, 16);
        var result = Csg.Intersect(sphere, cyl);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CsgDifference_BoxCylinder_ProducesFaces()
    {
        var box = Primitives.Cube(Vec3.Zero, 2.0);
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -2), new Vec3(0, 0, 1), 0.5, 4.0, 16);
        var result = Csg.Difference(box, cyl);
        Assert.True(result.FaceCount > 0);
    }
}
