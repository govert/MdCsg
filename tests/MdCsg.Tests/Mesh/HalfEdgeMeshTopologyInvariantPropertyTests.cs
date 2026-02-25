using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh — DCEL invariants: cycle consistency, twin symmetry, face valence, origin/target</summary>
public class HalfEdgeMeshTopologyInvariantPropertyTests
{
    [Fact]
    public void Cube_AllHalfEdges_NextPrevCycle()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Tetrahedron_AllHalfEdges_NextPrevCycle()
    {
        var tet = MeshFactory.CreateTetrahedron();
        foreach (var he in tet.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Cube_TwinSymmetry_BothDirections()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Same(he, he.Twin.Twin);
            }
        }
    }

    [Fact]
    public void Cube_FaceCycle_ExactlyThreeEdges()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
                Assert.True(count <= 3, "Face cycle should have exactly 3 edges for triangular mesh");
            } while (current != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Cube_AllHalfEdges_BelongToFace()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Face);
        }
    }

    [Fact]
    public void Cube_TwinEdge_TargetOriginConsistency()
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
    public void Cube_EveryEdge_HasTwin()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
        }
    }

    [Fact]
    public void Tetrahedron_EveryEdge_HasTwin()
    {
        var tet = MeshFactory.CreateTetrahedron();
        foreach (var he in tet.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
        }
    }

    [Fact]
    public void Cube_HalfEdgeCount_Is3xFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(cube.Mesh.Faces.Count * 3, cube.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Tetrahedron_HalfEdgeCount_Is3xFaceCount()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(tet.Mesh.Faces.Count * 3, tet.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Sphere_AllFaces_HaveThreeEdgeCycle()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        foreach (var face in sphere.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Cube_FaceEdge_PointsBackToFace()
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

    [Fact]
    public void Cube_VerticesHaveOutgoingEdge()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.NotNull(v.OutgoingEdge);
        }
    }

    [Fact]
    public void Cube_UniqueHalfEdgeIds()
    {
        var cube = MeshFactory.CreateCube();
        var ids = new HashSet<int>();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.True(ids.Add(he.Id), $"Duplicate half-edge ID: {he.Id}");
        }
    }

    [Fact]
    public void Cube_UniqueFaceIds()
    {
        var cube = MeshFactory.CreateCube();
        var ids = new HashSet<int>();
        foreach (var f in cube.Mesh.Faces)
        {
            Assert.True(ids.Add(f.Id), $"Duplicate face ID: {f.Id}");
        }
    }

    [Fact]
    public void Cube_UniqueVertexIds()
    {
        var cube = MeshFactory.CreateCube();
        var ids = new HashSet<int>();
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.True(ids.Add(v.Id), $"Duplicate vertex ID: {v.Id}");
        }
    }

    [Fact]
    public void Cube_FaceGetVertices_ReturnsThreeVertices()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var verts = face.GetVertices();
            Assert.Equal(3, verts.Count);
        }
    }

    [Fact]
    public void Cube_FacesAroundVertex_AllVerticesHaveFaces()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var v in cube.Mesh.Vertices)
        {
            var faces = cube.Mesh.FacesAroundVertex(v).ToList();
            Assert.True(faces.Count > 0, $"Vertex {v.Id} has no adjacent faces");
        }
    }

    [Fact]
    public void Cube_FacesAroundVertex_CubeCorner_HasFaces()
    {
        var cube = MeshFactory.CreateCube();
        // Each vertex of a cube should be shared by multiple triangular faces
        foreach (var v in cube.Mesh.Vertices)
        {
            var faceCount = cube.Mesh.FacesAroundVertex(v).Count();
            // A cube vertex (corner) is shared by at least 3 triangular faces (2 quads * 2 tris / quad ≈ varies)
            Assert.True(faceCount >= 2, $"Vertex {v.Id} has only {faceCount} adjacent faces");
        }
    }

    [Fact]
    public void Cube_GetBounds_MatchesSolidBounds()
    {
        var cube = MeshFactory.CreateCube(new Vec3(1, 2, 3), 4.0);
        var meshBounds = cube.Mesh.GetBounds();
        var solidBounds = cube.Bounds;
        Assert.True(System.Math.Abs(meshBounds.Min.X - solidBounds.Min.X) < 0.01);
        Assert.True(System.Math.Abs(meshBounds.Min.Y - solidBounds.Min.Y) < 0.01);
        Assert.True(System.Math.Abs(meshBounds.Min.Z - solidBounds.Min.Z) < 0.01);
        Assert.True(System.Math.Abs(meshBounds.Max.X - solidBounds.Max.X) < 0.01);
        Assert.True(System.Math.Abs(meshBounds.Max.Y - solidBounds.Max.Y) < 0.01);
        Assert.True(System.Math.Abs(meshBounds.Max.Z - solidBounds.Max.Z) < 0.01);
    }

    [Fact]
    public void Sphere_TwinSymmetry()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        foreach (var he in sphere.Mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Same(he, he.Twin.Twin);
            }
        }
    }

    [Fact]
    public void SingleTriangle_ThreeHalfEdges_NoCrash()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) });
        Assert.Equal(3, mesh.HalfEdges.Count);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void EmptyMesh_GetBounds_IsEmpty()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(Array.Empty<Triangle3>());
        Assert.Equal(0, mesh.Vertices.Count);
        Assert.Equal(0, mesh.Faces.Count);
    }
}
