using System.IO;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

public class MeshSerializerTests
{
    // ── Roundtrip ───────────────────────────────────────────────────

    [Fact]
    public void Roundtrip_Cube_PreservesVertexCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var deserialized = RoundTrip(cube.Mesh);
        Assert.Equal(cube.Mesh.Vertices.Count, deserialized.Vertices.Count);
    }

    [Fact]
    public void Roundtrip_Cube_PreservesFaceCount()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var deserialized = RoundTrip(cube.Mesh);
        Assert.Equal(cube.Mesh.Faces.Count, deserialized.Faces.Count);
    }

    [Fact]
    public void Roundtrip_Cube_PreservesVertexPositions()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var deserialized = RoundTrip(cube.Mesh);

        for (int i = 0; i < cube.Mesh.Vertices.Count; i++)
        {
            Assert.Equal(cube.Mesh.Vertices[i].Position.X, deserialized.Vertices[i].Position.X);
            Assert.Equal(cube.Mesh.Vertices[i].Position.Y, deserialized.Vertices[i].Position.Y);
            Assert.Equal(cube.Mesh.Vertices[i].Position.Z, deserialized.Vertices[i].Position.Z);
        }
    }

    [Fact]
    public void Roundtrip_Sphere_PreservesVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 2);
        var deserialized = RoundTrip(sphere.Mesh);

        double origVol = System.Math.Abs(MeshQueries.Volume(sphere.Mesh));
        double newVol = System.Math.Abs(MeshQueries.Volume(deserialized));
        Assert.True(System.Math.Abs(origVol - newVol) < 1e-8,
            $"Volume changed: {origVol} -> {newVol}");
    }

    public static IEnumerable<object[]> PrimitiveTypes()
    {
        yield return new object[] { "Cube" };
        yield return new object[] { "Sphere" };
        yield return new object[] { "Cylinder" };
        yield return new object[] { "Cone" };
    }

    [Theory]
    [MemberData(nameof(PrimitiveTypes))]
    public void Roundtrip_AllPrimitives(string type)
    {
        var solid = CreatePrimitive(type);
        var deserialized = RoundTrip(solid.Mesh);
        Assert.Equal(solid.Mesh.Vertices.Count, deserialized.Vertices.Count);
        Assert.Equal(solid.Mesh.Faces.Count, deserialized.Faces.Count);
    }

    // ── Binary format validation ────────────────────────────────────

    [Fact]
    public void Serialize_StartsWithMagicAndVersion()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        MeshSerializer.Serialize(cube.Mesh, writer);

        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        uint magic = reader.ReadUInt32();
        ushort version = reader.ReadUInt16();

        Assert.Equal(0x4D444353u, magic); // "MDCS"
        Assert.Equal((ushort)1, version);
    }

    [Fact]
    public void Deserialize_InvalidMagic_Throws()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write((uint)0xDEADBEEF);
        writer.Write((ushort)1);

        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        Assert.Throws<InvalidDataException>(() => MeshSerializer.Deserialize(reader));
    }

    [Fact]
    public void Deserialize_InvalidVersion_Throws()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write(0x4D444353u);
        writer.Write((ushort)99);

        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        Assert.Throws<InvalidDataException>(() => MeshSerializer.Deserialize(reader));
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static HalfEdgeMesh RoundTrip(HalfEdgeMesh mesh)
    {
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.Default, true))
            MeshSerializer.Serialize(mesh, writer);

        ms.Position = 0;
        using var reader = new BinaryReader(ms);
        return MeshSerializer.Deserialize(reader);
    }

    private static Solid CreatePrimitive(string type) => type switch
    {
        "Cube" => Primitives.Cube(Vec3.Zero, 2),
        "Sphere" => Primitives.Sphere(Vec3.Zero, 1, 2),
        "Cylinder" => Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1, 2, 24),
        "Cone" => Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1, 2, 24),
        _ => throw new ArgumentException()
    };
}
