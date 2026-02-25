using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid creation (FromTriangles, FromIndexed), Plane (FromPoints, SignedDistance, Flipped)</summary>
public class SolidPlaneCreationPropertyTests
{
    [Fact]
    public void Solid_FromTriangles_CreatesMesh()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0))
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.True(solid.Mesh.Vertices.Count >= 3);
    }

    [Fact]
    public void Solid_FromTriangles_BuildsBvh()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0))
        };
        var solid = Solid.FromTriangles(tris);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_FromIndexed_CreatesMesh()
    {
        var positions = new Vec3[]
        {
            new(0,0,0), new(1,0,0), new(1,1,0), new(0,1,0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (0, 2, 3) };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_FromIndexed_BuildsBvh()
    {
        var positions = new Vec3[] { new(0,0,0), new(1,0,0), new(0,1,0) };
        var indices = new (int, int, int)[] { (0, 1, 2) };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_Bounds_ContainsAllVertices()
    {
        var cube = MeshFactory.CreateCube(new Vec3(1, 2, 3), 2.0);
        var bounds = cube.Bounds;
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position) ||
                Vec3.DistanceSquared(v.Position, Vec3.Max(bounds.Min, Vec3.Min(bounds.Max, v.Position))) < 0.01);
        }
    }

    [Fact]
    public void Solid_FromTriangles_EmptyInput_EmptyMesh()
    {
        var solid = Solid.FromTriangles(Array.Empty<Triangle3>());
        Assert.Equal(0, solid.Mesh.Faces.Count);
        Assert.Equal(0, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void Solid_FromTriangles_WeldTolerance_MergesVertices()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0))
        };
        var solid = Solid.FromTriangles(tris);
        // Shared edge vertices should be welded
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    // --- Plane tests ---

    [Fact]
    public void Plane_FromPoints_NormalIsUnitLength()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(plane.Normal.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Plane_FromPoints_XYPlane_NormalIsZ()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.Y) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.Z - 1.0) < 1e-10);
    }

    [Fact]
    public void Plane_FromPoints_DistanceCorrect()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.True(System.Math.Abs(plane.Distance - 5.0) < 1e-10);
    }

    [Fact]
    public void Plane_SignedDistanceTo_PointOnPlane_Zero()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(new Vec3(0.5, 0.5, 0))) < 1e-10);
    }

    [Fact]
    public void Plane_SignedDistanceTo_AbovePlane_Positive()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(plane.SignedDistanceTo(new Vec3(0, 0, 3)) > 0);
    }

    [Fact]
    public void Plane_SignedDistanceTo_BelowPlane_Negative()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(plane.SignedDistanceTo(new Vec3(0, 0, -3)) < 0);
    }

    [Fact]
    public void Plane_Flipped_ReversesNormal()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        Assert.True(Vec3.DistanceSquared(plane.Normal, -flipped.Normal) < 1e-20);
    }

    [Fact]
    public void Plane_Flipped_NegatesDistance()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        var flipped = plane.Flipped;
        Assert.True(System.Math.Abs(plane.Distance + flipped.Distance) < 1e-10);
    }

    [Fact]
    public void Plane_Flipped_SignedDistance_Negated()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        var testPoint = new Vec3(1, 2, 3);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(testPoint) + flipped.SignedDistanceTo(testPoint)) < 1e-10);
    }
}
