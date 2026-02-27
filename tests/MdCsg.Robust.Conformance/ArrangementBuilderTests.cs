using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust.Kernel.Arrangement;

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
}
