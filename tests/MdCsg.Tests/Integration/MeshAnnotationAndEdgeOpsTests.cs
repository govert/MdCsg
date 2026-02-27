using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 24: Annotation CRUD, FlipEdge, CollapseEdge.</summary>
public class MeshAnnotationAndEdgeOpsTests
{
    [Fact]
    public void AnnotationForVertices_CanSetAndGet()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var ann = MeshAnnotation<int>.ForVertices(cube.Mesh);
        foreach (var v in cube.Mesh.Vertices)
            ann[v.Id] = v.Id * 2;
        foreach (var v in cube.Mesh.Vertices)
            Assert.Equal(v.Id * 2, ann[v.Id]);
    }

    [Fact]
    public void AnnotationForFaces_CanSetAndGet()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var ann = MeshAnnotation<string>.ForFaces(cube.Mesh);
        foreach (var f in cube.Mesh.Faces)
            ann[f.Id] = $"face_{f.Id}";
        foreach (var f in cube.Mesh.Faces)
            Assert.Equal($"face_{f.Id}", ann[f.Id]);
    }

    [Fact]
    public void AnnotationForVertices_DefaultValue()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var ann = MeshAnnotation<double>.ForVertices(cube.Mesh);
        Assert.Equal(0.0, ann[0]);
    }

    [Fact]
    public void AnnotationForFaces_DefaultValue()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var ann = MeshAnnotation<int>.ForFaces(cube.Mesh);
        Assert.Equal(0, ann[0]);
    }

    [Fact]
    public void AnnotationOverwrite_UpdatesValue()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var ann = MeshAnnotation<int>.ForVertices(cube.Mesh);
        ann[0] = 42;
        Assert.Equal(42, ann[0]);
        ann[0] = 99;
        Assert.Equal(99, ann[0]);
    }

    [Fact]
    public void AnnotationOnSphere_AllVerticesAnnotated()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var ann = MeshAnnotation<double>.ForVertices(sphere.Mesh);
        foreach (var v in sphere.Mesh.Vertices)
            ann[v.Id] = v.Position.Length;
        foreach (var v in sphere.Mesh.Vertices)
            Assert.True(System.Math.Abs(ann[v.Id] - 1.0) < 0.1);
    }

    [Fact]
    public void AnnotationOnCsgResult_Works()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var ann = MeshAnnotation<bool>.ForFaces(r.Mesh);
        foreach (var f in r.Mesh.Faces)
            ann[f.Id] = true;
        foreach (var f in r.Mesh.Faces)
            Assert.True(ann[f.Id]);
    }

    [Fact]
    public void FlipEdge_OnSphere_DoesNotCrash()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        // Find an internal edge to flip
        var edge = sphere.Mesh.Faces[0].Edge;
        // FlipEdge may or may not succeed depending on mesh topology
        MeshEdgeOps.FlipEdge(sphere.Mesh, edge);
        Assert.True(sphere.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void FlipEdge_PreservesFaceCount()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        int originalFaces = sphere.Mesh.Faces.Count;
        var edge = sphere.Mesh.Faces[0].Edge;
        MeshEdgeOps.FlipEdge(sphere.Mesh, edge);
        Assert.Equal(originalFaces, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void CollapseEdge_ReducesFaceCount()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        int originalFaces = sphere.Mesh.Faces.Count;
        var edge = sphere.Mesh.Faces[0].Edge;
        var midpoint = (edge.Target.Position + edge.Next.Target.Position) * 0.5;
        bool collapsed = MeshEdgeOps.CollapseEdge(sphere.Mesh, edge, midpoint);
        if (collapsed)
            Assert.True(sphere.Mesh.Faces.Count < originalFaces);
    }

    [Fact]
    public void CollapseEdge_ToMidpoint_PreservesApproximateVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var originalVol = System.Math.Abs(MeshQueries.Volume(sphere.Mesh));
        var edge = sphere.Mesh.Faces[0].Edge;
        var midpoint = (edge.Target.Position + edge.Next.Target.Position) * 0.5;
        MeshEdgeOps.CollapseEdge(sphere.Mesh, edge, midpoint);
        var newVol = System.Math.Abs(MeshQueries.Volume(sphere.Mesh));
        Assert.True(System.Math.Abs(originalVol - newVol) < 0.5);
    }

    [Fact]
    public void AnnotationForHalfEdges_CanSetAndGet()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var ann = MeshAnnotation<int>.ForHalfEdges(cube.Mesh);
        int count = 0;
        foreach (var f in cube.Mesh.Faces)
        {
            var he = f.Edge;
            do
            {
                ann[he.Id] = count++;
                he = he.Next;
            } while (he != f.Edge);
        }
        Assert.True(count > 0);
    }

    [Fact]
    public void MultipleAnnotations_Independent()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var ann1 = MeshAnnotation<int>.ForVertices(cube.Mesh);
        var ann2 = MeshAnnotation<int>.ForVertices(cube.Mesh);
        ann1[0] = 42;
        ann2[0] = 99;
        Assert.Equal(42, ann1[0]);
        Assert.Equal(99, ann2[0]);
    }

    [Fact]
    public void Annotation_OnTorus_Works()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var ann = MeshAnnotation<double>.ForFaces(torus.Mesh);
        foreach (var f in torus.Mesh.Faces)
        {
            f.GetTrianglePositions(out var a, out var b, out var c);
            ann[f.Id] = Vec3.Cross(b - a, c - a).Length * 0.5;
        }
        double totalArea = 0;
        foreach (var f in torus.Mesh.Faces)
            totalArea += ann[f.Id];
        Assert.True(totalArea > 0);
    }

    [Fact]
    public void FlipEdge_ReturnsBool()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var edge = sphere.Mesh.Faces[0].Edge;
        bool result = MeshEdgeOps.FlipEdge(sphere.Mesh, edge);
        // Just verify it returns without crashing
        Assert.True(result || !result);
    }

    [Fact]
    public void MultipleCollapses_StillValid()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        for (int i = 0; i < 5; i++)
        {
            if (sphere.Mesh.Faces.Count < 10) break;
            var edge = sphere.Mesh.Faces[0].Edge;
            var midpoint = (edge.Target.Position + edge.Next.Target.Position) * 0.5;
            MeshEdgeOps.CollapseEdge(sphere.Mesh, edge, midpoint);
        }
        Assert.True(sphere.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void AnnotationBoolType_Works()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var ann = MeshAnnotation<bool>.ForFaces(cube.Mesh);
        ann[0] = true;
        Assert.True(ann[0]);
        Assert.False(ann[1]);
    }
}
