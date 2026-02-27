using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust.Kernel.Arrangement;

namespace MdCsg.Robust.Conformance;

public class ArrangementAnalyzerTests
{
    [Fact]
    public void DisjointCubes_AnalysisIsEmpty()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(10, 0, 0), 2.0);
        var arrangement = ArrangementBuilder.Build(a.Mesh, b.Mesh);

        var analysis = ArrangementAnalyzer.Analyze(arrangement);

        Assert.Equal(0, analysis.EndpointVertexCount);
        Assert.Equal(0, analysis.ConnectedComponentCount);
    }

    [Fact]
    public void OverlappingShapes_HasAtLeastOneComponent()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 1.2, 2);
        var arrangement = ArrangementBuilder.Build(a.Mesh, b.Mesh);

        var analysis = ArrangementAnalyzer.Analyze(arrangement);

        Assert.True(analysis.ConnectedComponentCount > 0);
    }
}
