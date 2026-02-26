using System.IO;
using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// Binary serialization and deserialization of <see cref="HalfEdgeMesh"/> objects.
/// </summary>
public static class MeshSerializer
{
    private const uint Magic = 0x4D444353; // "MDCS"
    private const ushort Version = 1;

    /// <summary>
    /// Serializes a mesh to a binary writer.
    /// Format: [magic u32][version u16][V i32][positions f64*3*V][F i32][indices i32*3*F]
    /// </summary>
    /// <param name="mesh">The mesh to serialize.</param>
    /// <param name="writer">The binary writer.</param>
    public static void Serialize(HalfEdgeMesh mesh, BinaryWriter writer)
    {
        writer.Write(Magic);
        writer.Write(Version);

        int vertCount = mesh.Vertices.Count;
        writer.Write(vertCount);
        foreach (var v in mesh.Vertices)
        {
            writer.Write(v.Position.X);
            writer.Write(v.Position.Y);
            writer.Write(v.Position.Z);
        }

        int faceCount = mesh.Faces.Count;
        writer.Write(faceCount);
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            writer.Write(verts[0].Id);
            writer.Write(verts[1].Id);
            writer.Write(verts[2].Id);
        }
    }

    /// <summary>
    /// Deserializes a mesh from a binary reader.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The deserialized mesh.</returns>
    public static HalfEdgeMesh Deserialize(BinaryReader reader)
    {
        uint magic = reader.ReadUInt32();
        if (magic != Magic)
            throw new InvalidDataException($"Invalid mesh file magic: 0x{magic:X8}");

        ushort version = reader.ReadUInt16();
        if (version != Version)
            throw new InvalidDataException($"Unsupported mesh version: {version}");

        int vertCount = reader.ReadInt32();
        var positions = new List<Vec3>(vertCount);
        for (int i = 0; i < vertCount; i++)
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            double z = reader.ReadDouble();
            positions.Add(new Vec3(x, y, z));
        }

        int faceCount = reader.ReadInt32();
        var triangles = new List<(int, int, int)>(faceCount);
        for (int i = 0; i < faceCount; i++)
        {
            int i0 = reader.ReadInt32();
            int i1 = reader.ReadInt32();
            int i2 = reader.ReadInt32();
            triangles.Add((i0, i1, i2));
        }

        var builder = new MeshBuilder(1e-12);
        return builder.Build(positions, triangles);
    }
}
