using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 09: Extrude/Revolve profiles + CSG.</summary>
public class ExtrudeRevolveCsgTests
{
    private static Vec2[] SquareProfile => new[]
    {
        new Vec2(-1, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(-1, 1)
    };

    private static Vec2[] TriangleProfile => new[]
    {
        new Vec2(0, 1), new Vec2(-1, -1), new Vec2(1, -1)
    };

    private static Vec2[] LProfile => new[]
    {
        new Vec2(0, 0), new Vec2(2, 0), new Vec2(2, 0.5),
        new Vec2(0.5, 0.5), new Vec2(0.5, 2), new Vec2(0, 2)
    };

    private static Vec2[] RevolveProfile => new[]
    {
        new Vec2(0, -1), new Vec2(1, -1), new Vec2(1, 1), new Vec2(0, 1)
    };

    [Fact]
    public void Extrude_SquareProfile_HasVolume()
    {
        var solid = Primitives.Extrude(SquareProfile, Vec3.UnitZ, 2.0);
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void Extrude_TriangleProfile_HasVolume()
    {
        var solid = Primitives.Extrude(TriangleProfile, Vec3.UnitZ, 2.0);
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void Extrude_LProfile_HasVolume()
    {
        var solid = Primitives.Extrude(LProfile, Vec3.UnitZ, 1.0);
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void Revolve_Profile_HasVolume()
    {
        var solid = Primitives.Revolve(RevolveProfile, Vec3.UnitZ, 16);
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void ExtrudedSquare_UnionWithCube_ProducesValidMesh()
    {
        var extruded = Primitives.Extrude(SquareProfile, Vec3.UnitZ, 2.0);
        var cube = Primitives.Cube(new Vec3(0, 0, 1.5), 2.0);
        var r = Csg.Union(extruded, cube);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ExtrudedSquare_DifferenceWithSphere_ProducesValidMesh()
    {
        var extruded = Primitives.Extrude(SquareProfile, Vec3.UnitZ, 2.0);
        var sphere = Primitives.Sphere(new Vec3(0, 0, 1), 0.5, 2);
        var r = Csg.Difference(extruded, sphere);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void ExtrudedTriangle_IntersectWithCube_ProducesValidMesh()
    {
        var extruded = Primitives.Extrude(TriangleProfile, Vec3.UnitZ, 2.0);
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var r = Csg.Intersect(extruded, cube);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Revolved_UnionWithCube_ProducesValidMesh()
    {
        var revolved = Primitives.Revolve(RevolveProfile, Vec3.UnitZ, 16);
        var cube = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var r = Csg.Union(revolved, cube);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Revolved_DifferenceWithCylinder_ProducesValidMesh()
    {
        var revolved = Primitives.Revolve(RevolveProfile, Vec3.UnitZ, 16);
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.3, 4.0, 12);
        var r = Csg.Difference(revolved, cyl);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void Revolved_IntersectWithSphere_ProducesValidMesh()
    {
        var revolved = Primitives.Revolve(RevolveProfile, Vec3.UnitZ, 16);
        var sphere = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = Csg.Intersect(revolved, sphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ExtrudedL_UnionWithExtrudedSquare_ProducesValidMesh()
    {
        var lSolid = Primitives.Extrude(LProfile, Vec3.UnitZ, 1.0);
        var sqSolid = Primitives.Extrude(SquareProfile, Vec3.UnitZ, 1.0);
        var r = Csg.Union(lSolid, sqSolid);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ExtrudeAlongDiagonal_HasVolume()
    {
        var solid = Primitives.Extrude(SquareProfile, new Vec3(1, 1, 1).Normalized, 2.0);
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void Extrude_ThenHalfSpaceSlice_ProducesValidMesh()
    {
        var extruded = Primitives.Extrude(SquareProfile, Vec3.UnitZ, 2.0);
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 1), Vec3.UnitZ);
        var r = Csg.Intersect(extruded, hs);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Revolve_ThenHalfSpaceSlice_ProducesValidMesh()
    {
        var revolved = Primitives.Revolve(RevolveProfile, Vec3.UnitZ, 16);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var r = Csg.Intersect(revolved, hs);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Extrude_ThenImplicitSphere_ProducesValidMesh()
    {
        var extruded = Primitives.Extrude(SquareProfile, Vec3.UnitZ, 2.0);
        var iSphere = new ImplicitSphere(new Vec3(0, 0, 1), 1.0);
        var r = Csg.Intersect(extruded, iSphere);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void ExtrudeAndRevolve_DifferenceProducesValidMesh()
    {
        var extruded = Primitives.Extrude(SquareProfile, Vec3.UnitZ, 3.0);
        var revolved = Primitives.Revolve(RevolveProfile, Vec3.UnitZ, 16);
        var r = Csg.Difference(extruded, revolved);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Revolve_HighSegments_ProducesValidMesh()
    {
        var solid = Primitives.Revolve(RevolveProfile, Vec3.UnitZ, 32);
        Assert.True(solid.Volume() > 0);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Extrude_TallProfile_ProducesValidMesh()
    {
        var solid = Primitives.Extrude(SquareProfile, Vec3.UnitZ, 10.0);
        Assert.True(solid.Volume() > 0);
    }
}
