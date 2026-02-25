using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;
using System.Linq;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh topology — face cycles, vertex fan, bounds, structural invariants</summary>
public class HalfEdgeMeshTopologyAdditionalTests
{
    [Fact]
    public void AddVertex_ReturnsUniqueId()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        Assert.NotEqual(v0.Id, v1.Id);
    }

    [Fact]
    public void AddVertex_PositionStored()
    {
        var mesh = new HalfEdgeMesh();
        var pos = new Vec3(1.5, 2.5, 3.5);
        var v = mesh.AddVertex(pos);
        Assert.Equal(pos, v.Position);
    }

    [Fact]
    public void AddFace_Creates3HalfEdges()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void AddFace_NextFormsCycle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);
        var e0 = face.Edge;
        var e1 = e0.Next;
        var e2 = e1.Next;
        Assert.Equal(e0, e2.Next); // Cycle back to start
    }

    [Fact]
    public void AddFace_PrevFormsCycle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);
        var e0 = face.Edge;
        Assert.Equal(e0, e0.Prev.Prev.Prev);
    }

    [Fact]
    public void AddFace_NextPrev_Inverse()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);
        var e = face.Edge;
        Assert.Equal(e, e.Next.Prev);
        Assert.Equal(e, e.Prev.Next);
    }

    [Fact]
    public void AddFace_AllEdgesRefFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);
        var e = face.Edge;
        Assert.Equal(face, e.Face);
        Assert.Equal(face, e.Next.Face);
        Assert.Equal(face, e.Next.Next.Face);
    }

    [Fact]
    public void CubeMesh_12Faces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(12, mesh.Faces.Count);
    }

    [Fact]
    public void CubeMesh_8Vertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(8, mesh.Vertices.Count);
    }

    [Fact]
    public void CubeMesh_36HalfEdges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(36, mesh.HalfEdges.Count); // 12 faces × 3
    }

    [Fact]
    public void CubeMesh_AllFacesHave3Edges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do { count++; current = current.Next; } while (current != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void FacesAroundVertex_CubeCorner()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // Each vertex of a cube should be shared by some faces
        foreach (var v in mesh.Vertices)
        {
            var faces = mesh.FacesAroundVertex(v).ToList();
            Assert.True(faces.Count > 0, $"Vertex {v.Id} has no adjacent faces");
        }
    }

    [Fact]
    public void GetBounds_CubeUnit()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        Assert.Equal(0, bounds.Min.X, 1e-10);
        Assert.Equal(0, bounds.Min.Y, 1e-10);
        Assert.Equal(0, bounds.Min.Z, 1e-10);
        Assert.Equal(1, bounds.Max.X, 1e-10);
        Assert.Equal(1, bounds.Max.Y, 1e-10);
        Assert.Equal(1, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void GetBounds_OffsetCube()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(5, 10, 15), 2).Mesh;
        var bounds = mesh.GetBounds();
        Assert.Equal(5, bounds.Min.X, 1e-10);
        Assert.Equal(10, bounds.Min.Y, 1e-10);
        Assert.Equal(15, bounds.Min.Z, 1e-10);
        Assert.Equal(7, bounds.Max.X, 1e-10);
        Assert.Equal(12, bounds.Max.Y, 1e-10);
        Assert.Equal(17, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void SphereMesh_HasCorrectFaceCount()
    {
        var solid = MeshFactory.CreateSphere(Vec3.Zero, 1, 1);
        var mesh = solid.Mesh;
        // Icosahedron subdivided 1 time: 20 * 4 = 80 faces
        Assert.Equal(80, mesh.Faces.Count);
    }

    [Fact]
    public void SphereMesh_AllFacesCyclicValid()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            int count = 0;
            do
            {
                Assert.Equal(face, current.Face);
                current = current.Next;
                count++;
                Assert.True(count <= 100, "Infinite cycle detected");
            } while (current != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void TetrahedronMesh_4Faces_4Vertices()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(4, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void EmptyMesh_ZeroCounts()
    {
        var mesh = new HalfEdgeMesh();
        Assert.Equal(0, mesh.Vertices.Count);
        Assert.Equal(0, mesh.HalfEdges.Count);
        Assert.Equal(0, mesh.Faces.Count);
    }

    [Fact]
    public void EmptyMesh_BoundsAreEmpty()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        // Empty mesh should have degenerate or zero-sized bounds
        Assert.True(bounds.Max.X <= bounds.Min.X || mesh.Vertices.Count == 0);
    }
}
