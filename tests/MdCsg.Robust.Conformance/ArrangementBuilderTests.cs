using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust.Kernel.Arrangement;
using System.Text;

namespace MdCsg.Robust.Conformance;

public class ArrangementBuilderTests
{
    [Fact]
    public void OverlappingCubes_ProducesNonEmptyArrangement()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);

        var arrangement = ArrangementBuilder.Build(a.Mesh, b.Mesh);

        Assert.True(arrangement.Edges.Count > 0);
        Assert.True(arrangement.Vertices.Count > 0);
    }

    [Fact]
    public void DisjointCubes_ProducesEmptyArrangement()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(10, 0, 0), 2.0);

        var arrangement = ArrangementBuilder.Build(a.Mesh, b.Mesh);

        Assert.Empty(arrangement.Edges);
        Assert.Empty(arrangement.Vertices);
        Assert.Empty(arrangement.IncidentEdgesByVertex);
    }

    [Fact]
    public void IncidentMap_ReferencesValidEdgeIds()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 1.2, 2);
        var arrangement = ArrangementBuilder.Build(a.Mesh, b.Mesh);

        foreach (var kvp in arrangement.IncidentEdgesByVertex)
        {
            foreach (int edgeId in kvp.Value)
            {
                Assert.InRange(edgeId, 0, arrangement.Edges.Count - 1);
            }
        }
    }

    [Fact]
    public void CoplanarSharedFace_RecordsCoplanarFaceCounts()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(2, 0, 0), 2.0);

        var arrangement = ArrangementBuilder.Build(a.Mesh, b.Mesh);

        Assert.True(arrangement.CoplanarFaceCountA > 0);
        Assert.True(arrangement.CoplanarFaceCountB > 0);
        Assert.True(arrangement.CoplanarPairNormalsOpposeCount > 0);
    }

    [Fact]
    public void IdenticalCubes_RecordAgreeingCoplanarPairs()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);

        var arrangement = ArrangementBuilder.Build(a.Mesh, b.Mesh);

        Assert.True(arrangement.CoplanarFaceCountA > 0);
        Assert.True(arrangement.CoplanarPairNormalsAgreeCount > 0);
        Assert.True(arrangement.CoplanarPairNormalsAgreeCount + arrangement.CoplanarPairNormalsOpposeCount > 0);
    }

    [Fact]
    public void RepeatedBuilds_AreDeterministic()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.35, 0.2, -0.1), 1.15, 3);

        var baseline = ArrangementBuilder.Build(a.Mesh, b.Mesh);
        var baselineFingerprint = Fingerprint(baseline);

        for (int i = 0; i < 5; i++)
        {
            var next = ArrangementBuilder.Build(a.Mesh, b.Mesh);
            Assert.Equal(baselineFingerprint, Fingerprint(next));
        }
    }

    [Fact]
    public void ParallelFlag_DoesNotChangeArrangement()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.35, 0.2, -0.1), 1.15, 3);

        var sequential = ArrangementBuilder.Build(a.Mesh, b.Mesh, parallel: false);
        var parallel = ArrangementBuilder.Build(a.Mesh, b.Mesh, parallel: true);

        Assert.Equal(Fingerprint(sequential), Fingerprint(parallel));
    }

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
