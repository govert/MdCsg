using System.Globalization;
using System.Text;
using MdCsg.Robust.Diagnostics.Replay;
using MdCsg.Robust.Kernel.Arrangement;

namespace MdCsg.Robust.Conformance;

public class ArrangementReplayCorpusTests
{
    [Fact]
    public void CorpusManifest_AllCases_ReplayToExpectedArrangementSignatures()
    {
        string corpusDir = GetCorpusDirectory();
        string manifestPath = Path.Combine(corpusDir, "manifest.csv");
        Assert.True(File.Exists(manifestPath), $"Missing replay corpus manifest: {manifestPath}");

        var rows = LoadManifest(manifestPath);
        Assert.NotEmpty(rows);

        foreach (var row in rows)
        {
            string casePath = Path.Combine(corpusDir, row.CaseFile);
            Assert.True(File.Exists(casePath), $"Missing replay case file: {casePath}");

            string text = File.ReadAllText(casePath);
            var parsed = ArrangementReplayCodec.Deserialize(text);
            var graph1 = ArrangementReplayRunner.BuildArrangement(parsed);
            var graph2 = ArrangementReplayRunner.BuildArrangement(parsed);
            var analysis = ArrangementReplayRunner.AnalyzeArrangement(parsed);

            Assert.Equal(Fingerprint(graph1), Fingerprint(graph2));
            Assert.Equal(row.ArrangementVertices, graph1.Vertices.Count);
            Assert.Equal(row.ArrangementEdges, graph1.Edges.Count);
            Assert.Equal(row.EndpointVertices, analysis.EndpointVertexCount);
            Assert.Equal(row.ConnectedComponents, analysis.ConnectedComponentCount);
            Assert.Equal(row.CoplanarFaceA, graph1.CoplanarFaceCountA);
            Assert.Equal(row.CoplanarFaceB, graph1.CoplanarFaceCountB);
            Assert.Equal(row.CoplanarOppose, graph1.CoplanarPairNormalsOpposeCount);
        }
    }

    [Fact]
    public void CorpusManifest_CoversKnownRobustnessClasses()
    {
        string manifestPath = Path.Combine(GetCorpusDirectory(), "manifest.csv");
        var rows = LoadManifest(manifestPath);

        Assert.Contains(rows, r => r.CaseFile == "showcase-chained-step3-input.txt");
        Assert.Contains(rows, r => r.CoplanarOppose > 0);
        Assert.Contains(rows, r => r.EndpointVertices > 0);
        Assert.Contains(rows, r => r.ArrangementVertices > 10_000);
    }

    private static string GetCorpusDirectory()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            string candidateRoot = Path.Combine(dir, "MdCsg.slnx");
            if (File.Exists(candidateRoot))
                return Path.Combine(dir, "tests", "MdCsg.Robust.Conformance", "ReplayCorpus", "arrangement");
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root from test base directory.");
    }

    private static IReadOnlyList<ReplayManifestRow> LoadManifest(string manifestPath)
    {
        var lines = File.ReadAllLines(manifestPath)
            .Where(static l => !string.IsNullOrWhiteSpace(l))
            .ToArray();
        Assert.True(lines.Length >= 2, $"Manifest has no rows: {manifestPath}");

        var rows = new List<ReplayManifestRow>(lines.Length - 1);
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',', StringSplitOptions.None);
            Assert.True(parts.Length == 8, $"Invalid manifest row at line {i + 1}: {lines[i]}");

            rows.Add(new ReplayManifestRow(
                CaseFile: parts[0],
                ArrangementVertices: ParseInt(parts[1], i, "arrangement_vertices"),
                ArrangementEdges: ParseInt(parts[2], i, "arrangement_edges"),
                EndpointVertices: ParseInt(parts[3], i, "endpoint_vertices"),
                ConnectedComponents: ParseInt(parts[4], i, "connected_components"),
                CoplanarFaceA: ParseInt(parts[5], i, "coplanar_face_a"),
                CoplanarFaceB: ParseInt(parts[6], i, "coplanar_face_b"),
                CoplanarOppose: ParseInt(parts[7], i, "coplanar_oppose")));
        }

        return rows;
    }

    private static int ParseInt(string text, int row, string column)
    {
        Assert.True(
            int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value),
            $"Invalid integer in manifest row {row + 1}, column {column}: '{text}'.");
        return value;
    }

    private static string Fingerprint(ArrangementGraph graph)
    {
        var sb = new StringBuilder();
        sb.Append("V[");
        foreach (var v in graph.Vertices)
        {
            sb.Append(v.Id);
            sb.Append(':');
            sb.Append(v.Position.X.ToString("R", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(v.Position.Y.ToString("R", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(v.Position.Z.ToString("R", CultureInfo.InvariantCulture));
            sb.Append(';');
        }

        sb.Append("]E[");
        foreach (var e in graph.Edges)
        {
            sb.Append(e.Id);
            sb.Append(':');
            sb.Append(e.StartVertexId);
            sb.Append('>');
            sb.Append(e.EndVertexId);
            sb.Append(':');
            sb.Append(e.FaceIndexA);
            sb.Append('/');
            sb.Append(e.FaceIndexB);
            sb.Append(':');
            sb.Append(e.IsDegenerate ? '1' : '0');
            sb.Append(';');
        }

        sb.Append("]I[");
        foreach (var kvp in graph.IncidentEdgesByVertex.OrderBy(k => k.Key))
        {
            sb.Append(kvp.Key);
            sb.Append(':');
            foreach (int edgeId in kvp.Value.OrderBy(x => x))
            {
                sb.Append(edgeId);
                sb.Append(',');
            }
            sb.Append(';');
        }

        sb.Append(']');
        return sb.ToString();
    }

    private sealed record ReplayManifestRow(
        string CaseFile,
        int ArrangementVertices,
        int ArrangementEdges,
        int EndpointVertices,
        int ConnectedComponents,
        int CoplanarFaceA,
        int CoplanarFaceB,
        int CoplanarOppose);
}
