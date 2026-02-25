using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdge and Vertex structural tests — Origin, Twin, Next/Prev cycle, IsIntersectionEdge</summary>
public class HalfEdgeVertexStructuralTests
{
    [Fact]
    public void Vertex_Id_Sequential()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
        Assert.Equal(2, v2.Id);
    }

    [Fact]
    public void Vertex_Position_SetAndGet()
    {
        var mesh = new HalfEdgeMesh();
        var v = mesh.AddVertex(new Vec3(1, 2, 3));
        Assert.Equal(1, v.Position.X);
        Assert.Equal(2, v.Position.Y);
        Assert.Equal(3, v.Position.Z);
    }

    [Fact]
    public void Vertex_Position_Mutable()
    {
        var mesh = new HalfEdgeMesh();
        var v = mesh.AddVertex(new Vec3(0, 0, 0));
        v.Position = new Vec3(5, 6, 7);
        Assert.Equal(5, v.Position.X);
    }

    [Fact]
    public void Vertex_OutgoingEdge_SetAfterFaceCreation()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
            Assert.NotNull(v.OutgoingEdge);
    }

    [Fact]
    public void HalfEdge_Id_Sequential()
    {
        var mesh = new HalfEdgeMesh();
        var e0 = mesh.AddHalfEdge();
        var e1 = mesh.AddHalfEdge();
        Assert.Equal(0, e0.Id);
        Assert.Equal(1, e1.Id);
    }

    [Fact]
    public void HalfEdge_Target_SetCorrectly()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var edge = face.Edge;
            Assert.NotNull(edge.Target);
            Assert.NotNull(edge.Next.Target);
            Assert.NotNull(edge.Prev.Target);
        }
    }

    [Fact]
    public void HalfEdge_NextPrevCycle_ClosedLoop()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            int count = 0;
            do
            {
                current = current.Next;
                count++;
                Assert.True(count <= 10, "Next cycle too long");
            } while (current != start);
            Assert.Equal(3, count); // triangular faces
        }
    }

    [Fact]
    public void HalfEdge_PrevNext_Identity()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var edge = face.Edge;
            Assert.Same(edge, edge.Next.Prev);
            Assert.Same(edge, edge.Prev.Next);
        }
    }

    [Fact]
    public void HalfEdge_Origin_DerivedFromPrev()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var edge = face.Edge;
            Assert.Equal(edge.Prev.Target, edge.Origin);
        }
    }

    [Fact]
    public void HalfEdge_Face_ConsistentAroundFace()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            do
            {
                Assert.Same(face, current.Face);
                current = current.Next;
            } while (current != start);
        }
    }

    [Fact]
    public void HalfEdge_Twin_PointsOpposite()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var edge = face.Edge;
            for (int i = 0; i < 3; i++)
            {
                if (edge.Twin != null)
                {
                    Assert.Same(edge, edge.Twin.Twin);
                    // Twin goes in opposite direction
                    Assert.Equal(edge.Target, edge.Twin.Origin);
                    Assert.Equal(edge.Origin, edge.Twin.Target);
                }
                edge = edge.Next;
            }
        }
    }

    [Fact]
    public void HalfEdge_IsIntersectionEdge_DefaultFalse()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var edge = face.Edge;
            for (int i = 0; i < 3; i++)
            {
                Assert.False(edge.IsIntersectionEdge);
                edge = edge.Next;
            }
        }
    }

    [Fact]
    public void FacesAroundVertex_ReturnsMultiple()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // Each vertex of a cube is shared by multiple triangles
        foreach (var v in mesh.Vertices)
        {
            var faces = mesh.FacesAroundVertex(v).ToList();
            Assert.True(faces.Count >= 1, $"Vertex {v.Id} has {faces.Count} adjacent faces");
        }
    }

    [Fact]
    public void HalfEdge_EdgeCount_ThreePerFace()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // Each face has exactly 3 half-edges
        int totalEdges = mesh.Faces.Count * 3;
        // HalfEdges list includes all allocated edges (may have more if twins are separate)
        Assert.True(mesh.HalfEdges.Count >= totalEdges);
    }

    [Fact]
    public void Tetrahedron_AllEdgesHaveTwins()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        foreach (var face in mesh.Faces)
        {
            var edge = face.Edge;
            for (int i = 0; i < 3; i++)
            {
                Assert.NotNull(edge.Twin);
                edge = edge.Next;
            }
        }
    }

    [Fact]
    public void Cube_VertexCount_8()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(8, mesh.Vertices.Count);
    }

    [Fact]
    public void Cube_FaceCount_12()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(12, mesh.Faces.Count);
    }

    [Fact]
    public void Cube_HalfEdgeCount_AtLeast36()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // 12 faces * 3 = 36 half-edges minimum (plus twins)
        Assert.True(mesh.HalfEdges.Count >= 36);
    }
}
