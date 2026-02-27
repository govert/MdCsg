using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 15: Very thin slabs, near-zero volumes.</summary>
public class ThinSliceRobustnessTests
{
    [Fact]
    public void ThinBox_HasPositiveVolume()
    {
        var thin = Primitives.Box(Vec3.Zero, new Vec3(2, 2, 0.01));
        Assert.True(thin.Volume() > 0);
    }

    [Fact]
    public void ThinBox_UnionWithCube_ProducesValidMesh()
    {
        var thin = Primitives.Box(Vec3.Zero, new Vec3(2, 2, 0.01));
        var cube = Primitives.Cube(Vec3.Zero, 1.0);
        var r = Csg.Union(thin, cube);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ThinBox_IntersectWithCube_ProducesValidMesh()
    {
        var thin = Primitives.Box(Vec3.Zero, new Vec3(3, 3, 0.05));
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(thin, cube);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ThinSlice_ViaHalfSpace_ProducesValidMesh()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        // Keep -0.01 < z < 0.01 using two consistent half-space clips.
        var top = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.01), -Vec3.UnitZ);
        var bot = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -0.01), Vec3.UnitZ);
        var r1 = new Solid(Csg.Intersect(cube, top).Mesh);
        var r2 = Csg.Intersect(r1, bot);
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void VeryThinBox_DifferenceFromCube()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var thin = Primitives.Box(Vec3.Zero, new Vec3(3, 3, 0.02));
        var r = Csg.Difference(cube, thin);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ThinCylinder_HasPositiveVolume()
    {
        var thin = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 0.01, 12);
        Assert.True(thin.Volume() > 0);
    }

    [Fact]
    public void ThinCylinder_DiffFromCube()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var thin = Primitives.Cylinder(new Vec3(0, 0, -0.005), Vec3.UnitZ, 0.5, 0.01, 12);
        var r = Csg.Difference(cube, thin);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void NarrowBox_HasPositiveVolume()
    {
        var narrow = Primitives.Box(Vec3.Zero, new Vec3(0.01, 0.01, 2));
        Assert.True(narrow.Volume() > 0);
    }

    [Fact]
    public void SmallSphere_HasPositiveVolume()
    {
        var small = Primitives.Sphere(Vec3.Zero, 0.01, 1);
        Assert.True(small.Volume() > 0);
    }

    [Fact]
    public void SmallSphere_DiffFromCube()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var small = Primitives.Sphere(Vec3.Zero, 0.05, 1);
        var r = Csg.Difference(cube, small);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void ThinHalfSpaceSlice_ProducesResult()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var hs1 = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -0.95), Vec3.UnitZ);
        var r = Csg.Intersect(cube, hs1);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void FlatBox_IntersectWithSphere()
    {
        var flat = Primitives.Box(Vec3.Zero, new Vec3(4, 4, 0.1));
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Intersect(flat, sphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ThinExtrudedProfile_HasVolume()
    {
        var profile = new[] { new Vec2(-1, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(-1, 1) };
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 0.01);
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void ThinBoxLargeArea_StillHasVolume()
    {
        var thin = Primitives.Box(Vec3.Zero, new Vec3(10, 10, 0.01));
        Assert.True(thin.Volume() > 0);
        Assert.True(thin.SurfaceArea() > 0);
    }

    [Fact]
    public void SmallCube_IntersectWithLargeCube_EqualsSmallCube()
    {
        var small = Primitives.Cube(Vec3.Zero, 0.1);
        var large = Primitives.Cube(Vec3.Zero, 10.0);
        var r = Csg.Intersect(small, large);
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - small.Volume()) < 0.01);
    }

    [Fact]
    public void VerySmallCube_HasPositiveVolume()
    {
        var tiny = Primitives.Cube(Vec3.Zero, 0.001);
        Assert.True(tiny.Volume() > 0);
    }

    [Fact]
    public void ThinBox_SurfaceAreaDominated()
    {
        var thin = Primitives.Box(Vec3.Zero, new Vec3(2, 2, 0.01));
        // Surface area should be dominated by the two large faces
        Assert.True(thin.SurfaceArea() > 2 * 2 * 2 - 0.1);
    }

    [Fact]
    public void SmallBox_SubtractedFromLarge_VolumeNearlyUnchanged()
    {
        var large = Primitives.Cube(Vec3.Zero, 10.0);
        var tiny = Primitives.Cube(Vec3.Zero, 0.1);
        var r = Csg.Difference(large, tiny);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(System.Math.Abs(vol - large.Volume()) < 0.1);
    }

    [Fact]
    public void ThinBox_DifferenceFromSphere()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 2.0, 2);
        var thin = Primitives.Box(Vec3.Zero, new Vec3(4, 4, 0.05));
        var r = Csg.Difference(sphere, thin);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SmallCone_HasPositiveVolume()
    {
        var small = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 0.01, 0.01, 8);
        Assert.True(small.Volume() > 0);
    }
}
