using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG — comparing winding number vs ray cast classification consistency</summary>
public class CsgWindingVsRayCastConsistencyTests
{
    private static readonly CsgOptions WindingOptions = new() { UseWindingNumber = true };

    [Fact]
    public void DisjointUnion_BothModes_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var ray = Csg.Union(a, b);
        var winding = Csg.Union(a, b, WindingOptions);
        Assert.Equal(ray.Mesh.Faces.Count, winding.Mesh.Faces.Count);
    }

    [Fact]
    public void DisjointIntersection_BothModes_Empty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var ray = Csg.Intersect(a, b);
        var winding = Csg.Intersect(a, b, WindingOptions);
        Assert.Equal(0, ray.Mesh.Faces.Count);
        Assert.Equal(0, winding.Mesh.Faces.Count);
    }

    [Fact]
    public void DisjointDifference_BothModes_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var ray = Csg.Difference(a, b);
        var winding = Csg.Difference(a, b, WindingOptions);
        Assert.Equal(ray.Mesh.Faces.Count, winding.Mesh.Faces.Count);
    }

    [Fact]
    public void OverlappingUnion_BothModes_NonEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ray = Csg.Union(a, b);
        var winding = Csg.Union(a, b, WindingOptions);
        Assert.True(ray.Mesh.Faces.Count > 0);
        Assert.True(winding.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void OverlappingIntersection_BothModes_NonEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ray = Csg.Intersect(a, b);
        var winding = Csg.Intersect(a, b, WindingOptions);
        Assert.True(ray.Mesh.Faces.Count > 0);
        Assert.True(winding.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void OverlappingDifference_BothModes_NonEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ray = Csg.Difference(a, b);
        var winding = Csg.Difference(a, b, WindingOptions);
        Assert.True(ray.Mesh.Faces.Count > 0);
        Assert.True(winding.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void CubeSphere_BothModes_SamePatchCounts()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var ray = Csg.Union(cube, sphere);
        var winding = Csg.Union(cube, sphere, WindingOptions);
        // Patch counts should be the same (patches are formed before classification)
        Assert.Equal(ray.PatchCountA, winding.PatchCountA);
        Assert.Equal(ray.PatchCountB, winding.PatchCountB);
    }

    [Fact]
    public void CubeSphere_BothModes_SameIntersectionSegmentCount()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var ray = Csg.Union(cube, sphere);
        var winding = Csg.Union(cube, sphere, WindingOptions);
        Assert.Equal(ray.IntersectionSegmentCount, winding.IntersectionSegmentCount);
    }

    [Fact]
    public void SphereSphere_BothModes_AllOps_NonEmpty()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        Assert.True(Csg.Union(a, b, WindingOptions).Mesh.Faces.Count > 0);
        Assert.True(Csg.Intersect(a, b, WindingOptions).Mesh.Faces.Count > 0);
        Assert.True(Csg.Difference(a, b, WindingOptions).Mesh.Faces.Count > 0);
    }
}
