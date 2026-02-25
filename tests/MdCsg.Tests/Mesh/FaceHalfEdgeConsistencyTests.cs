using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face/HalfEdge/Vertex consistency — structural invariants on built meshes</summary>
public class FaceHalfEdgeConsistencyTests
{
    [Fact]
    public void CubeFaces_AllHaveThreeVertices()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var verts = face.GetVertices();
            Assert.Equal(3, verts.Count);
        }
    }

    [Fact]
    public void CubeFaces_CentroidInsideBounds()
    {
        var cube = MeshFactory.CreateCube();
        var bounds = cube.Mesh.GetBounds();
        foreach (var face in cube.Mesh.Faces)
        {
            var c = face.Centroid;
            Assert.True(bounds.Contains(c), $"Centroid {c} outside bounds");
        }
    }

    [Fact]
    public void CubeFaces_NormalNonZero()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            Assert.True(face.Normal.Length > 0.001);
        }
    }

    [Fact]
    public void CubeFaces_UnitNormal_IsUnit()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var un = face.UnitNormal;
            Assert.True(System.Math.Abs(un.Length - 1.0) < 1e-10);
        }
    }

    [Fact]
    public void CubeFaces_OriginalFaceIdMatchesId()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            Assert.Equal(face.Id, face.OriginalFaceId);
        }
    }

    [Fact]
    public void CubeFaces_PatchIdDefault()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            Assert.Equal(-1, face.PatchId);
        }
    }

    [Fact]
    public void CubeHalfEdges_NextPrevInverse()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void CubeHalfEdges_TwinSymmetry()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
            Assert.Same(he, he.Twin!.Twin);
        }
    }

    [Fact]
    public void CubeHalfEdges_TwinOriginEqualsTarget()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Same(he.Target, he.Twin.Origin);
                Assert.Same(he.Origin, he.Twin.Target);
            }
        }
    }

    [Fact]
    public void CubeHalfEdges_AllHaveFaces()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Face);
        }
    }

    [Fact]
    public void CubeVertices_AllHaveOutgoingEdge()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.NotNull(v.OutgoingEdge);
        }
    }

    [Fact]
    public void CubeVertices_OutgoingOriginIsSelf()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.Same(v, v.OutgoingEdge!.Origin);
        }
    }

    [Fact]
    public void CubeHalfEdges_IsIntersectionEdge_FalseByDefault()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.False(he.IsIntersectionEdge);
        }
    }

    [Fact]
    public void SphereFaces_NormalsPointOutward()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var face in sphere.Mesh.Faces)
        {
            var centroid = face.Centroid;
            var normal = face.UnitNormal;
            // For sphere centered at origin, normal should point roughly in same direction as centroid
            double dot = Vec3.Dot(normal, centroid);
            Assert.True(dot > 0, $"Face {face.Id}: normal doesn't point outward (dot={dot:F4})");
        }
    }

    [Fact]
    public void SphereFaces_AllTriangular()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var face in sphere.Mesh.Faces)
        {
            var he = face.Edge;
            Assert.Same(he, he.Next.Next.Next); // cycle of length 3
        }
    }

    [Fact]
    public void GetTrianglePositions_MatchesGetVertices()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var verts = face.GetVertices();
            Assert.Equal(verts[0].Position, a);
            Assert.Equal(verts[1].Position, b);
            Assert.Equal(verts[2].Position, c);
        }
    }
}
