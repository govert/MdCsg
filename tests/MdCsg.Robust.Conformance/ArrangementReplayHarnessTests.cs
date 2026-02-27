using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust.Diagnostics.Replay;
using MdCsg.Robust.Kernel.Arrangement;
using System.Text;

namespace MdCsg.Robust.Conformance;

public class ArrangementReplayHarnessTests
{
    [Fact]
    public void CaptureSerializeDeserialize_ReplaysSameArrangement()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.45, -0.2, 0.1), 1.25, 3);

        var direct = ArrangementBuilder.Build(a.Mesh, b.Mesh);
        var replayCase = ArrangementReplayCodec.Capture(a.Mesh, b.Mesh);
        var serialized = ArrangementReplayCodec.Serialize(replayCase);
        var parsed = ArrangementReplayCodec.Deserialize(serialized);
        var replayed = ArrangementReplayRunner.BuildArrangement(parsed);

        Assert.Equal(Fingerprint(direct), Fingerprint(replayed));
    }

    [Fact]
    public void ReplayCase_CanBeSavedAndLoaded()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.4, 0.4, 0), 2.0);
        var replayCase = ArrangementReplayCodec.Capture(a.Mesh, b.Mesh);

        string path = Path.Combine(Path.GetTempPath(), $"mdcsg-arr-replay-{Guid.NewGuid():N}.txt");
        try
        {
            ArrangementReplayCodec.Save(path, replayCase);
            var loaded = ArrangementReplayCodec.Load(path);
            var graph = ArrangementReplayRunner.BuildArrangement(loaded);
            var analysis = ArrangementReplayRunner.AnalyzeArrangement(loaded);

            Assert.True(graph.Vertices.Count > 0);
            Assert.True(graph.Edges.Count > 0);
            Assert.True(analysis.ConnectedComponentCount > 0);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void SeededReplayBatch_IsDeterministic()
    {
        const int seed = 424242;
        var rng = new Random(seed);

        for (int i = 0; i < 8; i++)
        {
            var a = Primitives.Cube(Vec3.Zero, 2.0);
            var b = Primitives.Sphere(
                new Vec3(
                    NextRange(rng, -0.8, 0.8),
                    NextRange(rng, -0.8, 0.8),
                    NextRange(rng, -0.8, 0.8)),
                NextRange(rng, 0.8, 1.4),
                2);

            var replayCase = ArrangementReplayCodec.Capture(a.Mesh, b.Mesh);
            var serialized = ArrangementReplayCodec.Serialize(replayCase);
            var parsed = ArrangementReplayCodec.Deserialize(serialized);

            var first = ArrangementReplayRunner.BuildArrangement(parsed);
            var second = ArrangementReplayRunner.BuildArrangement(parsed);

            Assert.Equal(Fingerprint(first), Fingerprint(second));
        }
    }

    private static double NextRange(Random rng, double min, double max)
        => min + (max - min) * rng.NextDouble();

    private static string Fingerprint(ArrangementGraph graph)
    {
        var sb = new StringBuilder();
        sb.Append("V[");
        foreach (var v in graph.Vertices)
        {
            sb.Append(v.Id);
            sb.Append(':');
            sb.Append(v.Position.X.ToString("R"));
            sb.Append(',');
            sb.Append(v.Position.Y.ToString("R"));
            sb.Append(',');
            sb.Append(v.Position.Z.ToString("R"));
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
}
