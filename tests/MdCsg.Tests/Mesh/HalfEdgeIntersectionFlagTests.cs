using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdge.IsIntersectionEdge flag — initial state, toggling, independence from twin</summary>
public class HalfEdgeIntersectionFlagTests
{
    [Fact]
    public void IsIntersectionEdge_InitiallyFalse()
    {
        var he = new HalfEdge(0);
        Assert.False(he.IsIntersectionEdge);
    }

    [Fact]
    public void IsIntersectionEdge_Settable()
    {
        var he = new HalfEdge(0);
        he.IsIntersectionEdge = true;
        Assert.True(he.IsIntersectionEdge);
    }

    [Fact]
    public void IsIntersectionEdge_Resettable()
    {
        var he = new HalfEdge(0);
        he.IsIntersectionEdge = true;
        he.IsIntersectionEdge = false;
        Assert.False(he.IsIntersectionEdge);
    }

    [Fact]
    public void Twin_FlagIndependent()
    {
        var he1 = new HalfEdge(0);
        var he2 = new HalfEdge(1);
        he1.Twin = he2;
        he2.Twin = he1;
        he1.IsIntersectionEdge = true;
        Assert.False(he2.IsIntersectionEdge);
    }

    [Fact]
    public void HalfEdge_Id_Preserved()
    {
        var he = new HalfEdge(42);
        Assert.Equal(42, he.Id);
    }

    [Fact]
    public void HalfEdge_Twin_InitiallyNull()
    {
        var he = new HalfEdge(0);
        Assert.Null(he.Twin);
    }

    [Fact]
    public void HalfEdge_Face_InitiallyNull()
    {
        var he = new HalfEdge(0);
        Assert.Null(he.Face);
    }

    [Fact]
    public void HalfEdge_Target_Settable()
    {
        var he = new HalfEdge(0);
        var v = new Vertex(0, new Vec3(1, 2, 3));
        he.Target = v;
        Assert.Same(v, he.Target);
    }

    [Fact]
    public void HalfEdge_Next_Prev_Settable()
    {
        var he1 = new HalfEdge(0);
        var he2 = new HalfEdge(1);
        he1.Next = he2;
        he2.Prev = he1;
        Assert.Same(he2, he1.Next);
        Assert.Same(he1, he2.Prev);
    }

    [Fact]
    public void Origin_FromPrev()
    {
        var v1 = new Vertex(0, new Vec3(0, 0, 0));
        var v2 = new Vertex(1, new Vec3(1, 0, 0));
        var he1 = new HalfEdge(0) { Target = v2 };
        var he2 = new HalfEdge(1) { Target = v1 };
        he1.Prev = he2;
        Assert.Same(v1, he1.Origin);
    }

    [Fact]
    public void Origin_FromTwin_WhenNoPrev()
    {
        var v1 = new Vertex(0, new Vec3(0, 0, 0));
        var v2 = new Vertex(1, new Vec3(1, 0, 0));
        var he1 = new HalfEdge(0) { Target = v2 };
        var he2 = new HalfEdge(1) { Target = v1 };
        he1.Twin = he2;
        Assert.Same(v1, he1.Origin);
    }

    [Fact]
    public void Origin_ThrowsWhenNoPrevNorTwin()
    {
        var he = new HalfEdge(0);
        Assert.Throws<InvalidOperationException>(() => he.Origin);
    }

    [Fact]
    public void Cube_AllEdges_NotIntersectionEdge()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.False(he.IsIntersectionEdge);
        }
    }

    [Fact]
    public void Cube_AllEdges_HaveFaces()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Face);
        }
    }

    [Fact]
    public void Cube_FaceCycle_Length3()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
                Assert.True(count <= 3, "Face cycle exceeded 3 half-edges");
            } while (current != start);
            Assert.Equal(3, count);
        }
    }
}
