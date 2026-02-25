using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdge origin resolution — Prev.Target, Twin.Target, face assignment, cycle consistency</summary>
public class HalfEdgeOriginResolutionTests
{
    [Fact]
    public void Origin_ViasPrev_CorrectVertex()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            // Origin should be the Target of Prev
            Assert.Equal(he.Prev.Target, he.Origin);
        }
    }

    [Fact]
    public void Origin_MatchesPrevTarget_ForAllCubeFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(he.Prev.Target, he.Origin);
        }
    }

    [Fact]
    public void AllHalfEdges_HaveNonNullNext()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Next);
        }
    }

    [Fact]
    public void AllHalfEdges_HaveNonNullPrev()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Prev);
        }
    }

    [Fact]
    public void AllHalfEdges_HaveNonNullTarget()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Target);
        }
    }

    [Fact]
    public void AllHalfEdges_HaveNonNullFace()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Face);
        }
    }

    [Fact]
    public void NextPrev_IsIdentity()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(he, he.Next.Prev);
            Assert.Equal(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Twin_OriginIsTarget()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                // Twin's origin should be this edge's target
                Assert.Equal(he.Target, he.Twin.Origin);
                // This edge's origin should be twin's target
                Assert.Equal(he.Origin, he.Twin.Target);
            }
        }
    }

    [Fact]
    public void Twin_IsSymmetric()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Equal(he, he.Twin.Twin);
            }
        }
    }

    [Fact]
    public void FaceCycle_AllSameFace()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            var he = start;
            do
            {
                Assert.Equal(face, he.Face);
                he = he.Next;
            } while (he != start);
        }
    }

    [Fact]
    public void Vertex_OutgoingEdge_NotNull_ForNonIsolatedVertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
        {
            Assert.NotNull(v.OutgoingEdge);
        }
    }

    [Fact]
    public void Tetra_EachFace_HasExactly3HalfEdges()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        foreach (var face in mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var he = start;
            do { count++; he = he.Next; } while (he != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Sphere_Origin_ViasPrev_Consistent()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(he.Prev.Target, he.Origin);
        }
    }

    [Fact]
    public void Sphere_Twin_Symmetric()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                Assert.Equal(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void CsgOutput_HalfEdge_OriginConsistent()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.Equal(he.Prev.Target, he.Origin);
        }
    }

    [Fact]
    public void CsgOutput_Twin_Symmetric()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        foreach (var he in result.Mesh.HalfEdges)
        {
            if (he.Twin != null)
                Assert.Equal(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void CsgOutput_NextPrev_IsIdentity()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Difference(a, b);
        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.Equal(he, he.Next.Prev);
            Assert.Equal(he, he.Prev.Next);
        }
    }

    [Fact]
    public void HalfEdge_Id_Unique()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var ids = new HashSet<int>();
        foreach (var he in mesh.HalfEdges)
        {
            Assert.True(ids.Add(he.Id), $"Duplicate HalfEdge Id: {he.Id}");
        }
    }

    [Fact]
    public void Vertex_Id_Unique()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var ids = new HashSet<int>();
        foreach (var v in mesh.Vertices)
        {
            Assert.True(ids.Add(v.Id), $"Duplicate Vertex Id: {v.Id}");
        }
    }

    [Fact]
    public void Face_Id_Unique()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var ids = new HashSet<int>();
        foreach (var f in mesh.Faces)
        {
            Assert.True(ids.Add(f.Id), $"Duplicate Face Id: {f.Id}");
        }
    }
}
