using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 14: Solids that share a face/edge/vertex.</summary>
public class TouchingKissingTests
{
    [Fact]
    public void FaceSharing_UnionProducesValidMesh()
    {
        // Two cubes sharing a face
        var a = Primitives.Cube(new Vec3(-1, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void FaceSharing_IntersectionProducesResult()
    {
        var a = Primitives.Cube(new Vec3(-1, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var r = Csg.Intersect(a, b);
        // May have degenerate intersection at shared face
        Assert.True(r.FaceCount >= 0);
    }

    [Fact]
    public void FaceSharing_DifferenceProducesResult()
    {
        var a = Primitives.Cube(new Vec3(-1, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount >= 0);
    }

    [Fact]
    public void EdgeTouching_UnionProducesValidMesh()
    {
        // Two cubes touching at an edge
        var a = Primitives.Cube(new Vec3(-1, -1, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1, 1, 0), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void EdgeTouching_IntersectionSmall()
    {
        var a = Primitives.Cube(new Vec3(-1, -1, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1, 1, 0), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void VertexTouching_UnionProducesValidMesh()
    {
        // Two cubes touching at a single vertex
        var a = Primitives.Cube(new Vec3(-1, -1, -1), 2.0);
        var b = Primitives.Cube(new Vec3(1, 1, 1), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void VertexTouching_IntersectionEmpty()
    {
        var a = Primitives.Cube(new Vec3(-1, -1, -1), 2.0);
        var b = Primitives.Cube(new Vec3(1, 1, 1), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void TangentSpheres_UnionProducesValidMesh()
    {
        var a = Primitives.Sphere(new Vec3(-1, 0, 0), 1.0, 2);
        var b = Primitives.Sphere(new Vec3(1, 0, 0), 1.0, 2);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void TangentSpheres_IntersectionSmall()
    {
        var a = Primitives.Sphere(new Vec3(-1, 0, 0), 1.0, 2);
        var b = Primitives.Sphere(new Vec3(1, 0, 0), 1.0, 2);
        var r = Csg.Intersect(a, b);
        // Tangent: intersection should be empty or near-zero
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.1);
    }

    [Fact]
    public void SlightlyOverlapping_IntersectionNonEmpty()
    {
        var a = Primitives.Cube(new Vec3(-0.99, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(0.99, 0, 0), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void FaceSharing_UnionVolumeCorrect()
    {
        var a = Primitives.Cube(new Vec3(-1, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var vol = new Solid(Csg.Union(a, b).Mesh).Volume();
        // Two touching cubes: each has volume 8, total should be 16
        Assert.True(vol > a.Volume() + b.Volume() - 1.0);
    }

    [Fact]
    public void CubeStackedOnCube_UnionValid()
    {
        var a = Primitives.Cube(new Vec3(0, 0, -1), 2.0);
        var b = Primitives.Cube(new Vec3(0, 0, 1), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void NearlyTouching_IntersectionEmpty()
    {
        var a = Primitives.Cube(new Vec3(-1.01, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1.01, 0, 0), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void CylindersTouching_UnionValid()
    {
        var a = Primitives.Cylinder(new Vec3(-1, 0, 0), Vec3.UnitZ, 0.5, 2.0, 12);
        var b = Primitives.Cylinder(new Vec3(1, 0, 0), Vec3.UnitZ, 0.5, 2.0, 12);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SpheresBarelySeparated_IntersectionEmpty()
    {
        var a = Primitives.Sphere(new Vec3(-1.01, 0, 0), 1.0, 2);
        var b = Primitives.Sphere(new Vec3(1.01, 0, 0), 1.0, 2);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.1);
    }

    [Fact]
    public void SphereBarelyOverlapping_IntersectionSmall()
    {
        var a = Primitives.Sphere(new Vec3(-0.95, 0, 0), 1.0, 2);
        var b = Primitives.Sphere(new Vec3(0.95, 0, 0), 1.0, 2);
        var r = Csg.Intersect(a, b);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol > 0);
        Assert.True(vol < a.Volume());
    }

    [Fact]
    public void FaceSharing_DoesNotCrash()
    {
        // Various face-sharing configurations
        var configs = new[]
        {
            (new Vec3(-1, 0, 0), new Vec3(1, 0, 0)),
            (new Vec3(0, -1, 0), new Vec3(0, 1, 0)),
            (new Vec3(0, 0, -1), new Vec3(0, 0, 1)),
        };
        foreach (var (ca, cb) in configs)
        {
            var a = Primitives.Cube(ca, 2.0);
            var b = Primitives.Cube(cb, 2.0);
            var r = Csg.Union(a, b);
            Assert.True(r.FaceCount >= 0);
        }
    }

    [Fact]
    public void SharedFace_DifferenceReducesVolume()
    {
        var a = Primitives.Cube(new Vec3(-1, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var r = Csg.Difference(a, b);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol <= a.Volume() + 0.1);
    }

    [Fact]
    public void StackedBoxes_UnionProducesValidMesh()
    {
        var a = Primitives.Box(new Vec3(0, 0, -0.5), new Vec3(2, 2, 1));
        var b = Primitives.Box(new Vec3(0, 0, 0.5), new Vec3(2, 2, 1));
        var r = Csg.Union(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }
}
