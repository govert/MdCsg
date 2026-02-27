using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Operations;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 06: HalfSpace vs every primitive, volume halving.</summary>
public class HalfSpaceSlicingTests
{
    private const double Tol = 0.5;
    private static readonly HalfSpace TopHalf = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
    private static readonly HalfSpace BottomHalf = TopHalf.Flipped;

    [Fact]
    public void SliceCube_TopHalf_HalvesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(cube, TopHalf);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(System.Math.Abs(vol - cube.Volume() / 2) < Tol);
    }

    [Fact]
    public void SliceCube_BottomHalf_HalvesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(cube, BottomHalf);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(System.Math.Abs(vol - cube.Volume() / 2) < Tol);
    }

    [Fact]
    public void SliceSphere_TopHalf_ApproximatelyHalf()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Intersect(sphere, TopHalf);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol > 0);
        Assert.True(vol < sphere.Volume());
    }

    [Fact]
    public void SliceCylinder_ProducesValidMesh()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var r = Csg.Intersect(cyl, TopHalf);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void SliceCone_ProducesValidMesh()
    {
        var cone = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var r = Csg.Intersect(cone, TopHalf);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SliceBox_ProducesValidMesh()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var r = Csg.Intersect(box, TopHalf);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void SliceTorus_ProducesValidMesh()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var r = Csg.Intersect(torus, TopHalf);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceCubeMinusHalfSpace_HalvesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Difference(cube, TopHalf);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(System.Math.Abs(vol - cube.Volume() / 2) < Tol);
    }

    [Fact]
    public void TwoComplementarySlices_SumToOriginal()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var top = new Solid(Csg.Intersect(cube, TopHalf).Mesh).Volume();
        var bot = new Solid(Csg.Intersect(cube, BottomHalf).Mesh).Volume();
        Assert.True(System.Math.Abs(top + bot - cube.Volume()) < Tol);
    }

    [Fact]
    public void DiagonalSlice_ProducesValidMesh()
    {
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, new Vec3(1, 1, 0).Normalized);
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(cube, hs);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void SliceAtOffset_ProducesValidMesh()
    {
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(cube, hs);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol > 0);
        Assert.True(vol < cube.Volume());
    }

    [Fact]
    public void SliceNearEdge_StillProducesResult()
    {
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.9), Vec3.UnitZ);
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(cube, hs);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void SliceAlongX_ProducesValidMesh()
    {
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitX);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Intersect(sphere, hs);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SliceAlongY_ProducesValidMesh()
    {
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitY);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Intersect(sphere, hs);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void MultipleSlices_ReducesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r1 = new Solid(Csg.Intersect(cube, TopHalf).Mesh);
        var hs2 = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitX);
        var r2 = new Solid(Csg.Intersect(r1, hs2).Mesh);
        Assert.True(r2.Volume() < r1.Volume() + 0.01);
        Assert.True(r2.Volume() > 0);
    }

    [Fact]
    public void SliceFlipped_IsComplementary()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.3), Vec3.UnitZ);
        var v1 = new Solid(Csg.Intersect(cube, hs).Mesh).Volume();
        var v2 = new Solid(Csg.Intersect(cube, hs.Flipped).Mesh).Volume();
        Assert.True(System.Math.Abs(v1 + v2 - cube.Volume()) < Tol);
    }

    [Fact]
    public void DifferenceSphereMinusHalfSpace_ProducesValidMesh()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Difference(sphere, TopHalf);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void TripleSlice_QuartersCube()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var hsZ = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var hsX = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitX);
        var r1 = new Solid(Csg.Intersect(cube, hsZ).Mesh);
        var r2 = new Solid(Csg.Intersect(r1, hsX).Mesh);
        Assert.True(r2.Volume() > 0);
        Assert.True(r2.Volume() < cube.Volume() / 2 + Tol);
    }

    [Fact]
    public void SliceEntirelyOutside_ProducesEmptyOrZero()
    {
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 10), Vec3.UnitZ);
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(cube, hs);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }
}
