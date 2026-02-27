using System;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using Xunit;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Diagnostic for CSG mesh health on concentric primitives.</summary>
public class CsgMeshHealthDiagnostic
{
    private readonly ITestOutputHelper _out;

    public CsgMeshHealthDiagnostic(ITestOutputHelper output) => _out = output;

    [Fact]
    public void Scene2_CsgOperations_OffsetPrimitives()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var b = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);

        _out.WriteLine("=== Scene 2: CSG Operations (offset primitives) ===");
        Validate("A (Sphere)", a.Mesh);
        Validate("B (Cube)", b.Mesh);

        var union = Csg.Union(a, b);
        Validate("A ∪ B", union.Mesh);

        var inter = Csg.Intersect(a, b);
        Validate("A ∩ B", inter.Mesh);

        var diff = Csg.Difference(a, b);
        Validate("A - B", diff.Mesh);
    }

    [Fact]
    public void Scene5_ChainedCsg_ConcentricPrimitives()
    {
        var y = new Vec3(0, 1, 0);

        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);
        var cylZ = Primitives.Cylinder(new Vec3(0, 0, -1.5), new Vec3(0, 0, 1), 0.5, 3.0);

        _out.WriteLine("=== Scene 5: Chained CSG (concentric primitives) ===");
        Validate("Sphere", sphere.Mesh);
        Validate("Box", box.Mesh);

        var step1 = Csg.Intersect(sphere, box);
        Validate("Step1: Sphere ∩ Box", step1.Mesh);

        var s1 = new Solid(step1.Mesh);
        var step2 = Csg.Difference(s1, cylX);
        Validate("Step2: Step1 - CylX", step2.Mesh);

        var s2 = new Solid(step2.Mesh);
        var step3 = Csg.Difference(s2, cylY);
        Validate("Step3: Step2 - CylY", step3.Mesh);

        var s3 = new Solid(step3.Mesh);
        var step4 = Csg.Difference(s3, cylZ);
        Validate("Step4: Step3 - CylZ", step4.Mesh);
    }

    [Fact]
    public void Scene7_Smoothing_ConcentricBoxSphere()
    {
        var box = Primitives.Cube(Vec3.Zero, 2.0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.4, 3);

        _out.WriteLine("=== Scene 7: Smoothing (concentric box ∩ sphere) ===");
        Validate("Box", box.Mesh);
        Validate("Sphere", sphere.Mesh);

        var inter = Csg.Intersect(box, sphere);
        Validate("Box ∩ Sphere", inter.Mesh);

        var smoothed = MeshSmoothing.LaplacianSmooth(inter.Mesh, iterations: 5, lambda: 0.5);
        Validate("Smoothed", smoothed);
    }

    [Fact]
    public void ConcentricVsOffset_SphereBox()
    {
        _out.WriteLine("=== Concentric vs Offset comparison ===");

        // Offset: sphere at origin, box at (0.6, 0, 0) — same as scene 2
        var sOffset = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var bOffset = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);
        var interOffset = Csg.Intersect(sOffset, bOffset);
        Validate("OFFSET: Sphere ∩ Box", interOffset.Mesh);

        // Concentric: both at origin — same as scene 5
        var sConcentric = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var bConcentric = Primitives.Cube(Vec3.Zero, 1.8);
        var interConcentric = Csg.Intersect(sConcentric, bConcentric);
        Validate("CONCENTRIC: Sphere ∩ Box", interConcentric.Mesh);

        // Slight offset
        var sSlight = Primitives.Sphere(new Vec3(0.01, 0.01, 0.01), 1.3, 3);
        var bSlight = Primitives.Cube(Vec3.Zero, 1.8);
        var interSlight = Csg.Intersect(sSlight, bSlight);
        Validate("SLIGHT OFFSET (0.01): Sphere ∩ Box", interSlight.Mesh);
    }

    private void Validate(string label, HalfEdgeMesh mesh)
    {
        var v = MeshValidator.Validate(mesh);
        var boundaries = MeshQueries.BoundaryLoops(mesh);
        var components = MeshQueries.ConnectedComponents(mesh);

        double volume = 0;
        try { volume = MeshQueries.Volume(mesh); } catch { volume = double.NaN; }

        double area = 0;
        try { area = MeshQueries.SurfaceArea(mesh); } catch { area = double.NaN; }

        _out.WriteLine($"\n--- {label} ---");
        _out.WriteLine($"  Verts={v.VertexCount} Edges={v.EdgeCount} Faces={v.FaceCount}");
        _out.WriteLine($"  Euler={v.EulerCharacteristic} (expect 2 for closed genus-0)");
        _out.WriteLine($"  AllTwins={v.AllEdgesHaveTwins} Manifold={v.IsEdgeManifold} Oriented={v.IsConsistentlyOriented} ValidCycles={v.HasValidFaceCycles}");
        _out.WriteLine($"  IsClosedManifold={v.IsClosedManifold}");
        _out.WriteLine($"  BoundaryLoops={boundaries.Count} ConnectedComponents={components.Count}");
        _out.WriteLine($"  Volume={volume:F6} Area={area:F6}");

        // Count degenerate faces
        int degen = 0;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var cross = Vec3.Cross(b - a, c - a);
            if (cross.LengthSquared < 1e-20)
                degen++;
        }
        if (degen > 0)
            _out.WriteLine($"  DEGENERATE FACES: {degen}");
    }
}
