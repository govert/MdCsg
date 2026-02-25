using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdge — traversal, twin/next/prev consistency, origin/target</summary>
public class HalfEdgeTraversalPropertyTests
{
    [Fact]
    public void Cube_AllHalfEdges_HaveTarget()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
            Assert.NotNull(he.Target);
    }

    [Fact]
    public void Cube_AllHalfEdges_HaveNext()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
            Assert.NotNull(he.Next);
    }

    [Fact]
    public void Cube_AllHalfEdges_HavePrev()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
            Assert.NotNull(he.Prev);
    }

    [Fact]
    public void Cube_AllHalfEdges_HaveFace()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
            Assert.NotNull(he.Face);
    }

    [Fact]
    public void Cube_AllHalfEdges_HaveTwin()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void Cube_TwinTwin_IsSelf()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
            Assert.Same(he, he.Twin!.Twin);
    }

    [Fact]
    public void Cube_NextPrev_IsSelf()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Cube_TwinOriginTarget_Reversed()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.Equal(he.Origin.Id, he.Twin!.Target.Id);
            Assert.Equal(he.Target.Id, he.Twin.Origin.Id);
        }
    }

    [Fact]
    public void Cube_FaceCycle_Length3()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var start = face.Edge;
            int count = 0;
            var current = start;
            do { count++; current = current.Next; } while (current != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Sphere_AllHalfEdges_HaveTwin()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var he in sphere.Mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void Sphere_TwinTwin_IsSelf()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var he in sphere.Mesh.HalfEdges)
            Assert.Same(he, he.Twin!.Twin);
    }

    [Fact]
    public void Sphere_NextPrev_IsSelf()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var he in sphere.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Tetrahedron_AllHalfEdges_HaveTwin()
    {
        var tet = MeshFactory.CreateTetrahedron();
        foreach (var he in tet.Mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void Tetrahedron_FaceCycle_Length3()
    {
        var tet = MeshFactory.CreateTetrahedron();
        foreach (var face in tet.Mesh.Faces)
        {
            var start = face.Edge;
            int count = 0;
            var current = start;
            do { count++; current = current.Next; } while (current != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void HalfEdge_Id_IsAssigned()
    {
        var he = new HalfEdge(42);
        Assert.Equal(42, he.Id);
    }

    [Fact]
    public void Cube_TwinFace_DifferentFromFace()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            if (he.Twin != null)
                Assert.NotSame(he.Face, he.Twin.Face);
        }
    }

    [Fact]
    public void Cube_AllFaceEdges_PointToFace()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
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
}
