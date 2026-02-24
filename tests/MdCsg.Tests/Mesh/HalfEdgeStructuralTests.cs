using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdge structural integrity deep tests</summary>
public class HalfEdgeStructuralTests
{
    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
        }
    }

    [Fact]
    public void Cube_TwinSymmetric()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                Assert.Equal(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void Cube_TwinOriginTarget()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Equal(he.Origin.Id, he.Twin.Target.Id);
                Assert.Equal(he.Target.Id, he.Twin.Origin.Id);
            }
        }
    }

    [Fact]
    public void Cube_FaceCycle_Length3()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var he = face.Edge;
            int count = 0;
            var current = he;
            do
            {
                count++;
                current = current.Next;
            } while (current != he && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Cube_VertexOutgoingEdges_Consistent()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
        {
            var start = v.OutgoingEdge;
            Assert.NotNull(start);
            Assert.Equal(v.Id, start.Origin.Id);
        }
    }

    [Fact]
    public void Sphere_AllFacesTriangular()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var face in mesh.Faces)
        {
            Assert.Equal(3, face.GetVertices().Count);
        }
    }

    [Fact]
    public void Sphere_AllNormals_PointOutward()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var face in mesh.Faces)
        {
            var centroid = face.Centroid;
            var normal = face.UnitNormal;
            // Normal should point away from origin
            double dot = Vec3.Dot(centroid, normal);
            Assert.True(dot > 0, $"Face normal should point outward, dot={dot}");
        }
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        int V = mesh.Vertices.Count;
        int E = mesh.HalfEdges.Count / 2;
        int F = mesh.Faces.Count;
        Assert.Equal(2, V - E + F); // Closed manifold → Euler = 2
    }

    [Fact]
    public void Cube_EulerCharacteristic()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        int V = mesh.Vertices.Count;
        int E = mesh.HalfEdges.Count / 2;
        int F = mesh.Faces.Count;
        Assert.Equal(2, V - E + F);
    }

    [Fact]
    public void CsgResult_Mesh_ValidStructure()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = MdCsg.Api.Csg.Union(a, b);
        foreach (var he in r.Mesh.HalfEdges)
        {
            Assert.Equal(he, he.Next.Prev);
            Assert.NotEqual(he.Origin.Id, he.Target.Id);
        }
    }

    [Fact]
    public void CsgResult_Mesh_AllFaces_Have3Vertices()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = MdCsg.Api.Csg.Union(a, b);
        foreach (var face in r.Mesh.Faces)
        {
            Assert.Equal(3, face.GetVertices().Count);
        }
    }

    [Fact]
    public void CsgResult_Mesh_NonZeroArea()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = MdCsg.Api.Csg.Union(a, b);
        foreach (var face in r.Mesh.Faces)
        {
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var tri = new Triangle3(va, vb, vc);
            Assert.True(tri.Area > 0);
        }
    }

    [Fact]
    public void SingleTriangle_Mesh_Structure()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder();
        var mesh = builder.Build(tris);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(3, mesh.HalfEdges.Count); // Open mesh: no twins linked
    }
}
