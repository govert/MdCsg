using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG edge cases — touching, contained, scaled, rotated</summary>
public class CsgEdgeCaseTests
{
    [Fact]
    public void Union_ContainedCube_VolumeIsOuter()
    {
        // Small cube fully inside large cube
        var outer = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        var r = Csg.Union(outer, inner);
        double vo = VolumeCalculator.ComputeAbsoluteVolume(outer.Mesh);
        double vu = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(System.Math.Abs(vu - vo) < 0.5,
            $"Contained union: vol {vu} should be ~{vo}");
    }

    [Fact]
    public void Intersection_ContainedCube_VolumeIsInner()
    {
        var outer = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        var r = Csg.Intersect(outer, inner);
        double vi = VolumeCalculator.ComputeAbsoluteVolume(inner.Mesh);
        double vr = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(System.Math.Abs(vr - vi) < 0.5,
            $"Contained intersection: vol {vr} should be ~{vi}");
    }

    [Fact]
    public void Difference_ContainedCube_MakesHole()
    {
        var outer = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        double vo = VolumeCalculator.ComputeAbsoluteVolume(outer.Mesh);
        double vi = VolumeCalculator.ComputeAbsoluteVolume(inner.Mesh);
        var r = Csg.Difference(outer, inner);
        double vd = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vd < vo, $"Difference vol {vd} should be < outer vol {vo}");
        Assert.True(System.Math.Abs(vd - (vo - vi)) < 0.5,
            $"Difference vol {vd} should be ~{vo - vi}");
    }

    [Fact]
    public void Union_FaceSharing_DoesNotCrash()
    {
        // Two cubes touching on one face (shared Z=1 / Z=0 plane)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 1)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Intersection_FaceSharing_DoesNotCrash()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 1)).Mesh);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount >= 0);
    }

    [Fact]
    public void Difference_FaceSharing_DoesNotCrash()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 1)).Mesh);
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount >= 0);
    }

    [Fact]
    public void Union_TwoSpheres_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Intersection_TwoSpheres_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Difference_TwoSpheres_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeTetrahedron_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.3).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Difference_CubeTetrahedron_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.3).Mesh);
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Union_SmallOffset_LargeOverlap()
    {
        // Almost completely overlapping cubes
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.01, 0.01, 0.01)).Mesh);
        var r = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // Volume should be slightly more than 1
        Assert.True(vol > 0.9 && vol < 1.5, $"Near-overlapping union vol: {vol}");
    }

    [Fact]
    public void Intersection_SmallOffset_LargeIntersection()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.01, 0.01, 0.01)).Mesh);
        var r = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 0.5, $"Near-overlapping intersection vol: {vol}");
    }

    [Fact]
    public void Union_LargeAndSmallCube_ContainsSmall()
    {
        var large = new Solid(MeshFactory.CreateCube(size: 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), size: 0.5).Mesh);
        var r = Csg.Union(large, small);
        // All vertices of result should be within large cube bounds
        foreach (var v in r.Mesh.Vertices)
        {
            Assert.True(v.Position.X >= -0.1 && v.Position.X <= 3.1);
            Assert.True(v.Position.Y >= -0.1 && v.Position.Y <= 3.1);
            Assert.True(v.Position.Z >= -0.1 && v.Position.Z <= 3.1);
        }
    }

    [Fact]
    public void CsgResult_Metadata_IntersectionSegmentCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.IntersectionSegmentCount > 0,
            "Overlapping cubes should have intersection segments");
    }

    [Fact]
    public void CsgResult_Metadata_DisjointCubes_NoIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh);
        var r = Csg.Union(a, b);
        Assert.Equal(0, r.IntersectionSegmentCount);
    }

    [Fact]
    public void CsgResult_PatchCounts_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.PatchCountA > 0);
        Assert.True(r.PatchCountB > 0);
    }

    [Fact]
    public void CsgResult_DisjointCubes_SinglePatchPerMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh);
        var r = Csg.Union(a, b);
        Assert.Equal(1, r.PatchCountA);
        Assert.Equal(1, r.PatchCountB);
    }

    [Fact]
    public void Solid_Bounds_ContainsAllVertices()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        foreach (var v in solid.Mesh.Vertices)
        {
            Assert.True(solid.Bounds.Contains(v.Position));
        }
    }

    [Fact]
    public void Solid_FromTriangles()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }
}
