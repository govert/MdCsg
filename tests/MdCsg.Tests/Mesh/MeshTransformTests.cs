using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

public class MeshTransformTests
{
    private const double Tol = 1e-8;

    // ── Translation ─────────────────────────────────────────────────

    [Fact]
    public void Apply_Translation_ShiftsVertices()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var offset = new Vec3(10, 20, 30);
        var t = Transform3.FromTranslation(offset);

        var result = MeshTransform.Apply(cube.Mesh, t);

        foreach (var v in result.Vertices)
        {
            bool foundMatch = false;
            foreach (var orig in cube.Mesh.Vertices)
            {
                if (Vec3.DistanceSquared(v.Position, orig.Position + offset) < Tol * Tol)
                {
                    foundMatch = true;
                    break;
                }
            }
            Assert.True(foundMatch, $"No matching original vertex for {v.Position}");
        }
    }

    [Fact]
    public void Apply_Translation_PreservesFacesAndVertices()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var t = Transform3.FromTranslation(new Vec3(5, 0, 0));
        var result = MeshTransform.Apply(cube.Mesh, t);

        Assert.Equal(cube.Mesh.Vertices.Count, result.Vertices.Count);
        Assert.Equal(cube.Mesh.Faces.Count, result.Faces.Count);
    }

    // ── Rotation ────────────────────────────────────────────────────

    [Fact]
    public void Apply_Rotation90Z_CorrectVertexPositions()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var rot = Transform3.FromRotationZ(System.Math.PI / 2);
        var result = MeshTransform.Apply(cube.Mesh, rot);

        // Each vertex should be at the rotated position
        foreach (var v in result.Vertices)
        {
            bool foundMatch = false;
            foreach (var orig in cube.Mesh.Vertices)
            {
                var expected = rot.TransformPoint(orig.Position);
                if (Vec3.DistanceSquared(v.Position, expected) < Tol * Tol)
                {
                    foundMatch = true;
                    break;
                }
            }
            Assert.True(foundMatch, $"No matching rotated vertex for {v.Position}");
        }
    }

    [Fact]
    public void Apply_Rotation_PreservesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var rot = Transform3.FromAxisAngle(new Vec3(1, 1, 1), 1.23);
        var result = MeshTransform.Apply(cube.Mesh, rot);

        double origVol = System.Math.Abs(MeshQueries.Volume(cube.Mesh));
        double newVol = System.Math.Abs(MeshQueries.Volume(result));
        Assert.True(System.Math.Abs(origVol - newVol) < 1e-6,
            $"Volume changed: {origVol} -> {newVol}");
    }

    // ── Identity ────────────────────────────────────────────────────

    [Fact]
    public void Apply_Identity_SameVertexPositions()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var result = MeshTransform.Apply(cube.Mesh, Transform3.Identity);

        for (int i = 0; i < cube.Mesh.Vertices.Count; i++)
        {
            Assert.True(
                Vec3.DistanceSquared(cube.Mesh.Vertices[i].Position, result.Vertices[i].Position) < Tol * Tol,
                $"Vertex {i} position differs");
        }
    }

    // ── Composed transform ──────────────────────────────────────────

    [Fact]
    public void Apply_ComposedTransform_SameAsSequentialApply()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var rot = Transform3.FromRotationX(0.5);
        var translate = Transform3.FromTranslation(new Vec3(3, 0, 0));
        var composed = translate * rot;

        var resultComposed = MeshTransform.Apply(cube.Mesh, composed);

        var intermediate = MeshTransform.Apply(cube.Mesh, rot);
        var resultSequential = MeshTransform.Apply(intermediate, translate);

        for (int i = 0; i < resultComposed.Vertices.Count; i++)
        {
            Assert.True(
                Vec3.DistanceSquared(resultComposed.Vertices[i].Position, resultSequential.Vertices[i].Position) < Tol * Tol,
                $"Vertex {i} differs between composed and sequential");
        }
    }

    // ── Parameterized across primitive types ────────────────────────

    public static IEnumerable<object[]> PrimitiveTypes()
    {
        yield return new object[] { "Cube" };
        yield return new object[] { "Sphere" };
        yield return new object[] { "Cylinder" };
    }

    [Theory]
    [MemberData(nameof(PrimitiveTypes))]
    public void Apply_Translation_PreservesVolumeForPrimitive(string primitiveType)
    {
        var solid = CreatePrimitive(primitiveType);
        var t = Transform3.FromTranslation(new Vec3(5, 7, 9));
        var result = MeshTransform.Apply(solid.Mesh, t);

        double origVol = System.Math.Abs(MeshQueries.Volume(solid.Mesh));
        double newVol = System.Math.Abs(MeshQueries.Volume(result));
        Assert.True(System.Math.Abs(origVol - newVol) < 1e-6,
            $"Volume changed for {primitiveType}: {origVol} -> {newVol}");
    }

    [Theory]
    [MemberData(nameof(PrimitiveTypes))]
    public void Apply_Rotation_PreservesSurfaceArea(string primitiveType)
    {
        var solid = CreatePrimitive(primitiveType);
        var rot = Transform3.FromRotationY(1.0);
        var result = MeshTransform.Apply(solid.Mesh, rot);

        double origArea = MeshQueries.SurfaceArea(solid.Mesh);
        double newArea = MeshQueries.SurfaceArea(result);
        Assert.True(System.Math.Abs(origArea - newArea) < 1e-6,
            $"Surface area changed for {primitiveType}: {origArea} -> {newArea}");
    }

    private static Solid CreatePrimitive(string type) => type switch
    {
        "Cube" => Primitives.Cube(Vec3.Zero, 2),
        "Sphere" => Primitives.Sphere(Vec3.Zero, 1, 2),
        "Cylinder" => Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1, 2, 24),
        _ => throw new ArgumentException($"Unknown primitive type: {type}")
    };
}
