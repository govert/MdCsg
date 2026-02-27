using System.Globalization;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Diagnostics.Replay;

public static class ArrangementReplayCodec
{
    private const string Header = "MDCSG_ARR_REPLAY_V1";
    private const uint MeshPayloadMagic = 0x4D445241; // "MDRA"
    private const ushort MeshPayloadVersion = 1;

    public static ArrangementReplayCase Capture(
        HalfEdgeMesh meshA,
        HalfEdgeMesh meshB,
        double gridSize = MathUtil.DefaultGridSize)
    {
        return new ArrangementReplayCase(
            gridSize,
            SerializeMesh(meshA),
            SerializeMesh(meshB));
    }

    public static string Serialize(ArrangementReplayCase replayCase)
    {
        if (replayCase is null)
            throw new ArgumentNullException(nameof(replayCase));

        var lines = new[]
        {
            Header,
            $"gridSize={replayCase.GridSize.ToString("R", CultureInfo.InvariantCulture)}",
            $"meshA={Convert.ToBase64String(replayCase.MeshAData)}",
            $"meshB={Convert.ToBase64String(replayCase.MeshBData)}"
        };
        return string.Join(Environment.NewLine, lines);
    }

    public static ArrangementReplayCase Deserialize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Replay text cannot be null or empty.", nameof(text));

        var lines = text
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(static l => l.Trim())
            .Where(static l => l.Length > 0)
            .ToArray();

        if (lines.Length < 4 || lines[0] != Header)
            throw new FormatException($"Replay text must start with '{Header}'.");

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int i = 1; i < lines.Length; i++)
        {
            int equals = lines[i].IndexOf('=');
            if (equals <= 0 || equals == lines[i].Length - 1)
                continue;

            string key = lines[i][..equals];
            string value = lines[i][(equals + 1)..];
            values[key] = value;
        }

        if (!values.TryGetValue("gridSize", out string? gridText)
            || !double.TryParse(gridText, NumberStyles.Float, CultureInfo.InvariantCulture, out double gridSize))
        {
            throw new FormatException("Replay text is missing a valid gridSize.");
        }

        if (!values.TryGetValue("meshA", out string? meshAText))
            throw new FormatException("Replay text is missing meshA payload.");
        if (!values.TryGetValue("meshB", out string? meshBText))
            throw new FormatException("Replay text is missing meshB payload.");

        try
        {
            return new ArrangementReplayCase(
                gridSize,
                Convert.FromBase64String(meshAText),
                Convert.FromBase64String(meshBText));
        }
        catch (FormatException ex)
        {
            throw new FormatException("Replay text contains invalid Base64 mesh payloads.", ex);
        }
    }

    public static void Save(string path, ArrangementReplayCase replayCase)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
        File.WriteAllText(path, Serialize(replayCase));
    }

    public static ArrangementReplayCase Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
        return Deserialize(File.ReadAllText(path));
    }

    public static (HalfEdgeMesh MeshA, HalfEdgeMesh MeshB) DecodeMeshes(ArrangementReplayCase replayCase)
    {
        if (replayCase is null)
            throw new ArgumentNullException(nameof(replayCase));
        return (DeserializeMesh(replayCase.MeshAData), DeserializeMesh(replayCase.MeshBData));
    }

    private static byte[] SerializeMesh(HalfEdgeMesh mesh)
    {
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(MeshPayloadMagic);
            writer.Write(MeshPayloadVersion);

            writer.Write(mesh.IsComplemented);

            writer.Write(mesh.Vertices.Count);
            foreach (var v in mesh.Vertices)
            {
                writer.Write(v.Position.X);
                writer.Write(v.Position.Y);
                writer.Write(v.Position.Z);
            }

            writer.Write(mesh.Faces.Count);
            foreach (var face in mesh.Faces)
            {
                var e0 = face.Edge;
                writer.Write(e0.Origin.Id);
                writer.Write(e0.Target.Id);
                writer.Write(e0.Next.Target.Id);
            }
        }
        return ms.ToArray();
    }

    private static HalfEdgeMesh DeserializeMesh(byte[] data)
    {
        using var ms = new MemoryStream(data, writable: false);
        using var reader = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        uint magic = reader.ReadUInt32();
        if (magic != MeshPayloadMagic)
            throw new FormatException($"Invalid replay mesh payload magic: 0x{magic:X8}");

        ushort version = reader.ReadUInt16();
        if (version != MeshPayloadVersion)
            throw new FormatException($"Unsupported replay mesh payload version: {version}");

        bool isComplemented = reader.ReadBoolean();

        int vertexCount = reader.ReadInt32();
        var positions = new List<Vec3>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
        {
            double x = reader.ReadDouble();
            double y = reader.ReadDouble();
            double z = reader.ReadDouble();
            positions.Add(new Vec3(x, y, z));
        }

        int faceCount = reader.ReadInt32();
        var triangles = new List<(int I0, int I1, int I2)>(faceCount);
        for (int i = 0; i < faceCount; i++)
        {
            int i0 = reader.ReadInt32();
            int i1 = reader.ReadInt32();
            int i2 = reader.ReadInt32();
            triangles.Add((i0, i1, i2));
        }

        var mesh = new MeshBuilder(0.0).Build(positions, triangles);
        mesh.IsComplemented = isComplemented;
        return mesh;
    }
}
