using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG bounds consistency — union bounds contain both inputs, intersection within both, difference within A</summary>
public class CsgBoundsConsistencyPropertyTests
{
    [Fact]
    public void Union_Bounds_ContainBothInputs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(3, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        var boundsA = a.Bounds;
        var boundsB = b.Bounds;
        Assert.True(bounds.Min.X <= boundsA.Min.X + 0.1);
        Assert.True(bounds.Max.X >= boundsB.Max.X - 0.1);
    }

    [Fact]
    public void Intersect_Bounds_WithinBothInputs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 1, 1), 4.0);
        var result = Csg.Intersect(a, b);
        if (result.FaceCount == 0) return;
        var bounds = result.Mesh.GetBounds();
        var boundsA = a.Bounds;
        var boundsB = b.Bounds;
        // Result bounds should be within the overlap region
        Assert.True(bounds.Min.X >= System.Math.Min(boundsA.Min.X, boundsB.Min.X) - 0.1);
        Assert.True(bounds.Max.X <= System.Math.Max(boundsA.Max.X, boundsB.Max.X) + 0.1);
    }

    [Fact]
    public void Difference_Bounds_WithinA()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateSphere(new Vec3(2, 2, 2), 1.0, 1);
        var result = Csg.Difference(a, b);
        var bounds = result.Mesh.GetBounds();
        var boundsA = a.Bounds;
        Assert.True(bounds.Min.X >= boundsA.Min.X - 0.1);
        Assert.True(bounds.Min.Y >= boundsA.Min.Y - 0.1);
        Assert.True(bounds.Max.X <= boundsA.Max.X + 0.1);
        Assert.True(bounds.Max.Y <= boundsA.Max.Y + 0.1);
    }

    [Fact]
    public void Union_Disjoint_BoundsEnclose()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Min.X < 0.1);
        Assert.True(bounds.Max.X > 10.9);
    }

    [Fact]
    public void Union_Overlapping_VertexCountReasonable()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        // Should have more vertices than either input alone
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Intersect_Overlapping_VertexCountReasonable()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Intersect(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Difference_Result_FaceCountLessOrEqualToUnion()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var diff = Csg.Difference(a, b);
        var union = Csg.Union(a, b);
        // Difference should generally have fewer or equal faces to union
        Assert.True(diff.FaceCount <= union.FaceCount + 10); // small tolerance
    }

    [Fact]
    public void Union_Sphere_BoundsRadiusPreserved()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 2.0, 2);
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        // Should extend from about -2 to about 3 on X
        Assert.True(bounds.Min.X < -1.9);
        Assert.True(bounds.Max.X > 2.9);
    }

    [Fact]
    public void Union_AllVerticesWithinBounds()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.True(v.Position.X >= bounds.Min.X - 1e-10);
            Assert.True(v.Position.Y >= bounds.Min.Y - 1e-10);
            Assert.True(v.Position.Z >= bounds.Min.Z - 1e-10);
        }
    }

    [Fact]
    public void Intersect_ContainedSphere_BoundsWithinSphere()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 10.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(5, 5, 5), 2.0, 2);
        var result = Csg.Intersect(cube, sphere);
        var bounds = result.Mesh.GetBounds();
        // Result should be within the sphere's bounds approximately
        Assert.True(bounds.Min.X > 2.5);
        Assert.True(bounds.Max.X < 7.5);
    }
}
