using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Vertex, Face — properties, topology, centroid, normal, original face tracking</summary>
public class VertexFaceTopologyPropertyTests
{
    [Fact]
    public void Vertex_Id_AssignedCorrectly()
    {
        var v = new Vertex(42, new Vec3(1, 2, 3));
        Assert.Equal(42, v.Id);
    }

    [Fact]
    public void Vertex_Position_AssignedCorrectly()
    {
        var pos = new Vec3(1, 2, 3);
        var v = new Vertex(0, pos);
        Assert.Equal(pos, v.Position);
    }

    [Fact]
    public void Vertex_Position_CanBeModified()
    {
        var v = new Vertex(0, Vec3.Zero);
        v.Position = new Vec3(5, 6, 7);
        Assert.Equal(new Vec3(5, 6, 7), v.Position);
    }

    [Fact]
    public void Vertex_OutgoingEdge_InitiallyNull()
    {
        var v = new Vertex(0, Vec3.Zero);
        Assert.Null(v.OutgoingEdge);
    }

    [Fact]
    public void Vertex_AfterAddFace_HasOutgoingEdge()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);
        Assert.NotNull(v0.OutgoingEdge);
        Assert.NotNull(v1.OutgoingEdge);
        Assert.NotNull(v2.OutgoingEdge);
    }

    [Fact]
    public void Face_Id_AssignedSequentially()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var v3 = mesh.AddVertex(new Vec3(1, 1, 0));
        var f0 = mesh.AddFace(v0, v1, v2);
        var f1 = mesh.AddFace(v1, v3, v2);
        Assert.Equal(0, f0.Id);
        Assert.Equal(1, f1.Id);
    }

    [Fact]
    public void Face_OriginalFaceId_DefaultsToId()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var f = mesh.AddFace(v0, v1, v2);
        Assert.Equal(f.Id, f.OriginalFaceId);
    }

    [Fact]
    public void Face_OriginalFaceId_CanBeSet()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var f = mesh.AddFace(v0, v1, v2);
        f.OriginalFaceId = 99;
        Assert.Equal(99, f.OriginalFaceId);
    }

    [Fact]
    public void Face_PatchId_DefaultsToNegativeOne()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var f = mesh.AddFace(v0, v1, v2);
        Assert.Equal(-1, f.PatchId);
    }

    [Fact]
    public void Face_Centroid_IsAverageOfVertices()
    {
        var builder = new MeshBuilder();
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var mesh = builder.Build(new[] { new Triangle3(a, b, c) });
        var centroid = mesh.Faces[0].Centroid;
        Assert.True(System.Math.Abs(centroid.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(centroid.Y - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(centroid.Z) < 1e-10);
    }

    [Fact]
    public void Face_Normal_PerpendicularToFace()
    {
        var builder = new MeshBuilder();
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var mesh = builder.Build(new[] { new Triangle3(a, b, c) });
        var normal = mesh.Faces[0].UnitNormal;
        // For this CCW triangle in XY plane, normal should point in +Z
        Assert.True(System.Math.Abs(normal.Z - 1.0) < 0.01 || System.Math.Abs(normal.Z + 1.0) < 0.01,
            $"Normal Z should be ±1, got {normal.Z}");
    }

    [Fact]
    public void Cube_AllFaces_HaveNonZeroArea()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var tri = new Triangle3(a, b, c);
            Assert.True(tri.Area > 1e-10, "Cube face should have non-zero area");
        }
    }

    [Fact]
    public void Sphere_AllFaces_HaveNonZeroArea()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var face in sphere.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var tri = new Triangle3(a, b, c);
            Assert.True(tri.Area > 1e-10, "Sphere face should have non-zero area");
        }
    }

    [Fact]
    public void HalfEdge_Origin_IsCorrect()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);
        // Edge from v0→v1: origin is v0, target is v1
        var he = face.Edge;
        Assert.Same(he.Prev.Target, he.Origin);
    }
}
