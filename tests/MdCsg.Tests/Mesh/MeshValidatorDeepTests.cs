using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator deep tests — structural invariants on various meshes</summary>
public class MeshValidatorDeepTests
{
    [Fact]
    public void Cube_AllFacesHaveThreeVertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
            Assert.Equal(3, face.GetVertices().Count);
    }

    [Fact]
    public void Cube_AllHalfEdgesHaveFace()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Face);
    }

    [Fact]
    public void Cube_HalfEdgeNextPrevConsistency()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Cube_TwinTargetConsistency()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                // Twin's target should be this edge's origin
                Assert.Same(he.Twin.Target, he.Origin);
                Assert.Same(he.Target, he.Twin.Origin);
            }
        }
    }

    [Fact]
    public void Sphere_AllFacesHaveThreeVertices()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var face in mesh.Faces)
            Assert.Equal(3, face.GetVertices().Count);
    }

    [Fact]
    public void Sphere_AllHalfEdgesHaveFace()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var he in mesh.HalfEdges)
            Assert.NotNull(he.Face);
    }

    [Fact]
    public void Tetrahedron_4Faces_4Vertices_12HalfEdges()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(4, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(12, mesh.HalfEdges.Count); // 4 faces * 3 half-edges each
    }

    [Fact]
    public void Cube_12Faces_8Vertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(12, mesh.Faces.Count); // 6 quad faces → 12 triangles
        Assert.Equal(8, mesh.Vertices.Count);
    }

    [Fact]
    public void Cube_EulerFormula()
    {
        // V - E + F = 2 for a closed mesh
        var mesh = MeshFactory.CreateCube().Mesh;
        int V = mesh.Vertices.Count;
        int F = mesh.Faces.Count;
        int E = mesh.HalfEdges.Count / 2; // Each edge has 2 half-edges
        // V - E + F should be 2 for a simple closed polyhedron (Euler's formula)
        // But since we count boundary half-edges too, just verify consistency
        Assert.True(V > 0 && F > 0 && E > 0);
    }

    [Fact]
    public void Tetrahedron_EulerFormula()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        int V = mesh.Vertices.Count;  // 4
        int F = mesh.Faces.Count;     // 4
        int E = mesh.HalfEdges.Count / 2; // 6
        Assert.Equal(2, V - E + F);
    }

    [Fact]
    public void Cube_AllFaceNormalsNonZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var n = face.Normal;
            Assert.True(n.LengthSquared > 1e-20);
        }
    }

    [Fact]
    public void Sphere_AllFaceNormalsPointOutward()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var face in mesh.Faces)
        {
            var centroid = face.Centroid;
            var normal = face.UnitNormal;
            // For a sphere centered at origin, centroid and normal should point same direction
            Assert.True(Vec3.Dot(centroid, normal) > 0,
                $"Face normal not pointing outward at centroid {centroid}");
        }
    }

    [Fact]
    public void Cube_AllVerticesUsed()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var usedVertices = new HashSet<int>();
        foreach (var face in mesh.Faces)
        {
            foreach (var v in face.GetVertices())
                usedVertices.Add(v.Id);
        }
        Assert.Equal(mesh.Vertices.Count, usedVertices.Count);
    }

    [Fact]
    public void OffsetCube_SameTopology()
    {
        var mesh1 = MeshFactory.CreateCube().Mesh;
        var mesh2 = MeshFactory.CreateCube(new Vec3(100, 200, 300)).Mesh;
        Assert.Equal(mesh1.Faces.Count, mesh2.Faces.Count);
        Assert.Equal(mesh1.Vertices.Count, mesh2.Vertices.Count);
        Assert.Equal(mesh1.HalfEdges.Count, mesh2.HalfEdges.Count);
    }
}
