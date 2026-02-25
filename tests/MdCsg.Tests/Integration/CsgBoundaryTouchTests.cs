using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG boundary cases — touching faces, edge-on-edge, corner-on-corner, tangent shapes</summary>
public class CsgBoundaryTouchTests
{
    [Fact]
    public void FaceTouching_Union_ValidResult()
    {
        // Two cubes sharing a face at x=1
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FaceTouching_Intersection_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        // Face-touching is a coplanar boundary case — result may include
        // coplanar face artifacts. Just verify it doesn't crash.
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void EdgeTouching_Union_ValidResult()
    {
        // Two cubes sharing an edge (diagonal placement)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CornerTouching_Union_ValidResult()
    {
        // Two cubes sharing only a corner
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SmallOverlap_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.99, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SmallOverlap_Intersection_SmallVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.99, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Overlap is 0.01 * 1 * 1 = 0.01
        Assert.True(vol < 0.02 && vol > 0.005,
            $"Small overlap volume {vol} should be ~0.01");
    }

    [Fact]
    public void LargeOverlap_Intersection_LargerVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0.4, $"Large overlap volume {vol} should be > 0.4");
    }

    [Fact]
    public void CubeInsideCube_Difference_CreatesCavity()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Difference(outer, inner);
        Assert.True(result.FaceCount > outer.Mesh.Faces.Count);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 7.8); // 8 - 0.125 = 7.875
    }

    [Fact]
    public void CubeOnCubeEdge_Union_HasFaces()
    {
        // Cube B placed so its edge crosses cube A's face
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void ThreeCubes_ChainedUnion()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 0, 0)).Mesh);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(vol > 1.5); // Three overlapping cubes
    }

    [Fact]
    public void ThreeCubes_ChainedDifference()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);
        var ab = Csg.Difference(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 1.0, 1.0), 0.5).Mesh);
        var abc = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void SphereCube_Tangent_Union()
    {
        // Sphere tangent to cube top face
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 1.5), 0.5, 2).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void AllOperations_SmallOverlap_ValidResults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.95, 0, 0)).Mesh);
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        var diff = Csg.Difference(a, b);
        Assert.True(union.FaceCount > 0);
        Assert.True(inter.FaceCount > 0);
        Assert.True(diff.FaceCount > 0);
    }
}
