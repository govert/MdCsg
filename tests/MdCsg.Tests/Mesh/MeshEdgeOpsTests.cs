using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

public class MeshEdgeOpsTests
{
    // ── FlipEdge ────────────────────────────────────────────────────

    [Fact]
    public void FlipEdge_InteriorEdge_Succeeds()
    {
        var mesh = CreateTwoTriangleMesh();
        // Find an interior edge (one with a twin)
        HalfEdge? interior = null;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                interior = he;
                break;
            }
        }
        Assert.NotNull(interior);
        bool result = MeshEdgeOps.FlipEdge(mesh, interior);
        Assert.True(result);
    }

    [Fact]
    public void FlipEdge_BoundaryEdge_ReturnsFalse()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        // All edges are boundary (no twin)
        foreach (var he in mesh.HalfEdges)
        {
            Assert.False(MeshEdgeOps.FlipEdge(mesh, he));
        }
    }

    [Fact]
    public void FlipEdge_PreservesFaceCount()
    {
        var mesh = CreateTwoTriangleMesh();
        int faceCountBefore = mesh.Faces.Count;
        var interior = FindInteriorEdge(mesh);
        Assert.NotNull(interior);
        MeshEdgeOps.FlipEdge(mesh, interior);
        Assert.Equal(faceCountBefore, mesh.Faces.Count);
    }

    [Fact]
    public void FlipEdge_DoubleFlip_RestoresTopology()
    {
        var mesh = CreateTwoTriangleMesh();
        var interior = FindInteriorEdge(mesh);
        Assert.NotNull(interior);

        // Save original vertex positions per face
        var origFace0 = GetFaceVertexIds(mesh.Faces[0]);
        var origFace1 = GetFaceVertexIds(mesh.Faces[1]);

        MeshEdgeOps.FlipEdge(mesh, interior);
        MeshEdgeOps.FlipEdge(mesh, interior);

        // After double flip, should have same connectivity
        var newFace0 = GetFaceVertexIds(mesh.Faces[0]);
        var newFace1 = GetFaceVertexIds(mesh.Faces[1]);

        // The vertex sets should match (order may differ)
        Assert.True(SameVertexSet(origFace0, newFace0) || SameVertexSet(origFace0, newFace1));
    }

    // ── CollapseEdge ────────────────────────────────────────────────

    [Fact]
    public void CollapseEdge_BoundaryEdge_ReturnsFalse()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        foreach (var he in mesh.HalfEdges)
        {
            Assert.False(MeshEdgeOps.CollapseEdge(mesh, he, Vec3.Zero));
        }
    }

    [Fact]
    public void CollapseEdge_InteriorEdge_MarksFacesDeleted()
    {
        var mesh = CreateTwoTriangleMesh();
        var interior = FindInteriorEdge(mesh);
        Assert.NotNull(interior);

        var midpoint = (interior.Origin.Position + interior.Target.Position) * 0.5;
        bool result = MeshEdgeOps.CollapseEdge(mesh, interior, midpoint);
        Assert.True(result);

        // Two faces should be marked as deleted (PatchId == -2)
        int deletedCount = 0;
        foreach (var face in mesh.Faces)
        {
            if (face.PatchId == -2) deletedCount++;
        }
        Assert.Equal(2, deletedCount);
    }

    [Fact]
    public void CollapseEdge_MovesVertexToNewPos()
    {
        var mesh = CreateTwoTriangleMesh();
        var interior = FindInteriorEdge(mesh);
        Assert.NotNull(interior);

        var newPos = new Vec3(0.5, 0.5, 0.5);
        var keepVert = interior.Origin;
        MeshEdgeOps.CollapseEdge(mesh, interior, newPos);

        Assert.Equal(newPos, keepVert.Position);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static HalfEdgeMesh CreateTwoTriangleMesh()
    {
        // Two triangles sharing an edge: (0,1,2) and (1,3,2)
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0.5, 1, 0), new(1.5, 1, 0)
        };
        var tris = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var builder = new MeshBuilder();
        return builder.Build(positions, tris);
    }

    private static HalfEdge? FindInteriorEdge(HalfEdgeMesh mesh)
    {
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null) return he;
        }
        return null;
    }

    private static List<int> GetFaceVertexIds(Face face)
    {
        var ids = new List<int>();
        var start = face.Edge;
        var current = start;
        do
        {
            ids.Add(current.Target.Id);
            current = current.Next;
        } while (current != start);
        return ids;
    }

    private static bool SameVertexSet(List<int> a, List<int> b)
    {
        if (a.Count != b.Count) return false;
        var setA = new HashSet<int>(a);
        var setB = new HashSet<int>(b);
        return setA.SetEquals(setB);
    }
}
