using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdge and Vertex class tests — construction, Origin derivation, edge cases</summary>
public class HalfEdgeVertexTests
{
    // ── Vertex ──

    [Fact]
    public void Vertex_IdPreserved()
    {
        var v = new Vertex(42, new Vec3(1, 2, 3));
        Assert.Equal(42, v.Id);
    }

    [Fact]
    public void Vertex_PositionPreserved()
    {
        var pos = new Vec3(1.5, 2.5, 3.5);
        var v = new Vertex(0, pos);
        Assert.Equal(pos, v.Position);
    }

    [Fact]
    public void Vertex_PositionMutable()
    {
        var v = new Vertex(0, Vec3.Zero);
        v.Position = new Vec3(10, 20, 30);
        Assert.Equal(new Vec3(10, 20, 30), v.Position);
    }

    [Fact]
    public void Vertex_OutgoingEdge_InitiallyNull()
    {
        var v = new Vertex(0, Vec3.Zero);
        Assert.Null(v.OutgoingEdge);
    }

    [Fact]
    public void Vertex_OutgoingEdge_Settable()
    {
        var v = new Vertex(0, Vec3.Zero);
        var he = new HalfEdge(0);
        v.OutgoingEdge = he;
        Assert.Same(he, v.OutgoingEdge);
    }

    // ── HalfEdge ──

    [Fact]
    public void HalfEdge_IdPreserved()
    {
        var he = new HalfEdge(99);
        Assert.Equal(99, he.Id);
    }

    [Fact]
    public void HalfEdge_Twin_InitiallyNull()
    {
        var he = new HalfEdge(0);
        Assert.Null(he.Twin);
    }

    [Fact]
    public void HalfEdge_Twin_Settable()
    {
        var he1 = new HalfEdge(0);
        var he2 = new HalfEdge(1);
        he1.Twin = he2;
        he2.Twin = he1;
        Assert.Same(he2, he1.Twin);
        Assert.Same(he1, he2.Twin);
    }

    [Fact]
    public void HalfEdge_Face_InitiallyNull()
    {
        var he = new HalfEdge(0);
        Assert.Null(he.Face);
    }

    [Fact]
    public void HalfEdge_IsIntersectionEdge_DefaultFalse()
    {
        var he = new HalfEdge(0);
        Assert.False(he.IsIntersectionEdge);
    }

    [Fact]
    public void HalfEdge_IsIntersectionEdge_Settable()
    {
        var he = new HalfEdge(0);
        he.IsIntersectionEdge = true;
        Assert.True(he.IsIntersectionEdge);
    }

    [Fact]
    public void HalfEdge_Origin_FromPrev()
    {
        var va = new Vertex(0, new Vec3(1, 0, 0));
        var vb = new Vertex(1, new Vec3(2, 0, 0));
        var prev = new HalfEdge(0) { Target = va };
        var he = new HalfEdge(1) { Target = vb, Prev = prev };
        // Origin comes from Prev.Target
        Assert.Same(va, he.Origin);
    }

    [Fact]
    public void HalfEdge_Origin_FromTwin_WhenNoPrev()
    {
        var va = new Vertex(0, new Vec3(1, 0, 0));
        var vb = new Vertex(1, new Vec3(2, 0, 0));
        var twin = new HalfEdge(0) { Target = va };
        var he = new HalfEdge(1) { Target = vb, Twin = twin };
        // Prev is null!, but since this is a null! not null, accessing it would fail
        // So twin is used as fallback
        // Note: Prev is null! (default) — the getter checks Prev?.Target first
        Assert.Same(va, he.Origin);
    }

    [Fact]
    public void HalfEdge_Origin_ThrowsWhenNeitherSet()
    {
        var he = new HalfEdge(0);
        // Neither Prev nor Twin set → InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => he.Origin);
    }

    [Fact]
    public void HalfEdge_NextPrev_Circular()
    {
        // Build a triangle loop
        var he0 = new HalfEdge(0);
        var he1 = new HalfEdge(1);
        var he2 = new HalfEdge(2);
        he0.Next = he1; he1.Next = he2; he2.Next = he0;
        he0.Prev = he2; he1.Prev = he0; he2.Prev = he1;

        Assert.Same(he0, he0.Next.Next.Next);
        Assert.Same(he0, he0.Prev.Prev.Prev);
    }

    // ── Integration with MeshBuilder ──

    [Fact]
    public void MeshBuilder_Cube_AllHalfEdgesHavePrev()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Prev);
    }

    [Fact]
    public void MeshBuilder_Cube_AllHalfEdgesHaveNext()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Next);
    }

    [Fact]
    public void MeshBuilder_Cube_AllFaceHalfEdgesFormTriangle()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            int count = 0;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void MeshBuilder_Cube_AllVerticesHaveOutgoingEdge()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
            Assert.NotNull(v.OutgoingEdge);
    }

    [Fact]
    public void MeshBuilder_Cube_OriginMatchesPrevTarget()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            var origin = he.Origin;
            Assert.Same(he.Prev.Target, origin);
        }
    }

    [Fact]
    public void MeshBuilder_Cube_TwinsAreSymmetric()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Same(he, he.Twin.Twin);
                twinCount++;
            }
        }
        Assert.True(twinCount > 0);
    }
}
