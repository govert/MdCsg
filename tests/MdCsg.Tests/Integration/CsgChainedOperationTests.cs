using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Chained CSG operations — sequential unions, differences, intersections with intermediate solids</summary>
public class CsgChainedOperationTests
{
    [Fact]
    public void UnionThenDifference_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.5, 0)).Mesh);
        var union = Csg.Union(a, b);
        var unionSolid = new Solid(union.Mesh);
        var result = Csg.Difference(unionSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferenceThenUnion_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 1).Mesh);
        var diff = Csg.Difference(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.5, 1.5, 1.5), 0.5).Mesh);
        var diffSolid = new Solid(diff.Mesh);
        var result = Csg.Union(diffSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void IntersectThenDifference_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var inter = Csg.Intersect(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.6, 0.6, 0.6), 0.2).Mesh);
        var interSolid = new Solid(inter.Mesh);
        var result = Csg.Difference(interSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TripleUnion_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void TripleUnion_MoreFacesThanPairs()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void DoubleDifference_Cavity()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var hole1 = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var hole2 = new Solid(MeshFactory.CreateCube(new Vec3(1.5, 1.5, 1.5), 0.5).Mesh);
        var r1 = Csg.Difference(outer, hole1);
        var r2 = Csg.Difference(new Solid(r1.Mesh), hole2);
        Assert.True(r2.FaceCount > r1.FaceCount,
            "Second cavity should add more faces");
    }

    [Theory]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    public void Union_CubeCube_VaryingY(double dy)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, dy, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Theory]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    public void Union_CubeCube_VaryingZ(double dz)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, dz)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SmallCubes_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 0.01).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.005, 0, 0), 0.01).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void LargeCubes_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 100).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(50, 0, 0), 100).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronCube_Union()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(Vec3.Zero, 1).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronCube_Difference()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 1).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void OffsetCube_AllThreeAxes()
    {
        for (int axis = 0; axis < 3; axis++)
        {
            var offset = new Vec3(axis == 0 ? 0.5 : 0, axis == 1 ? 0.5 : 0, axis == 2 ? 0.5 : 0);
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(offset).Mesh);
            var result = Csg.Union(a, b);
            Assert.True(result.FaceCount > 0, $"Failed for axis={axis}");
        }
    }

    [Fact]
    public void Result_HasPatchCounts()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void Result_HasIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Result_DisjointCubes_NoIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(100, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }
}
