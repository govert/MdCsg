using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh — structural invariants on built meshes, GetBounds, FacesAroundVertex detailed</summary>
public class HalfEdgeMeshStructuralPropertyTests
{
    [Fact]
    public void Cube_AllFacesHaveThreeEdges()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        foreach (var face in cube.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do { count++; current = current.Next; } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Cube_AllTwinsAreSymmetric()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        foreach (var he in cube.Mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Same(he, he.Twin.Twin);
            }
        }
    }

    [Fact]
    public void Cube_AllEdgesLinked()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        int twinCount = 0;
        foreach (var he in cube.Mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        // A closed cube should have all edges linked
        Assert.Equal(cube.Mesh.HalfEdges.Count, twinCount);
    }

    [Fact]
    public void Sphere_AllEdgesLinked()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        int twinCount = 0;
        foreach (var he in sphere.Mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        Assert.Equal(sphere.Mesh.HalfEdges.Count, twinCount);
    }

    [Fact]
    public void GetBounds_Cube_CorrectSize()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var bounds = cube.Mesh.GetBounds();
        var size = bounds.Size;
        Assert.True(System.Math.Abs(size.X - 3.0) < 0.01);
        Assert.True(System.Math.Abs(size.Y - 3.0) < 0.01);
        Assert.True(System.Math.Abs(size.Z - 3.0) < 0.01);
    }

    [Fact]
    public void GetBounds_Sphere_EnclosesRadius()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var bounds = sphere.Mesh.GetBounds();
        Assert.True(bounds.Max.X > 1.9);
        Assert.True(bounds.Min.X < -1.9);
    }

    [Fact]
    public void FacesAroundVertex_CubeCorner_HasMultipleFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        foreach (var v in cube.Mesh.Vertices)
        {
            if (v.OutgoingEdge == null) continue;
            var faces = cube.Mesh.FacesAroundVertex(v).ToList();
            Assert.True(faces.Count > 0);
        }
    }

    [Fact]
    public void FacesAroundVertex_AllReturnedFacesContainVertex()
    {
        var tet = MeshFactory.CreateTetrahedron();
        foreach (var v in tet.Mesh.Vertices)
        {
            if (v.OutgoingEdge == null) continue;
            var faces = tet.Mesh.FacesAroundVertex(v);
            foreach (var face in faces)
            {
                var verts = face.GetVertices();
                bool containsV = false;
                foreach (var fv in verts)
                    if (fv == v) containsV = true;
                Assert.True(containsV, $"Face {face.Id} returned for vertex {v.Id} but doesn't contain it");
            }
        }
    }

    [Fact]
    public void HalfEdgeMesh_AddVertex_IncrementsCount()
    {
        var mesh = new HalfEdgeMesh();
        var v1 = mesh.AddVertex(new Vec3(0, 0, 0));
        Assert.Equal(1, mesh.Vertices.Count);
        var v2 = mesh.AddVertex(new Vec3(1, 0, 0));
        Assert.Equal(2, mesh.Vertices.Count);
    }

    [Fact]
    public void HalfEdgeMesh_AddVertex_AssignsSequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
    }

    [Fact]
    public void HalfEdgeMesh_AddFace_CreatesHalfEdges()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        Assert.Equal(3, mesh.HalfEdges.Count);
        Assert.Single(mesh.Faces);
    }

    [Fact]
    public void HalfEdgeMesh_Empty_HasNoBoundsIssues()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        // Empty mesh bounds: min should be > max
        Assert.True(bounds.Min.X > bounds.Max.X);
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic_Two()
    {
        var tet = MeshFactory.CreateTetrahedron();
        int v = tet.Mesh.Vertices.Count;
        int e = tet.Mesh.HalfEdges.Count / 2; // each edge has 2 half-edges
        int f = tet.Mesh.Faces.Count;
        Assert.Equal(2, v - e + f);
    }

    [Fact]
    public void Cube_EulerCharacteristic_Two()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        int v = cube.Mesh.Vertices.Count;
        int e = cube.Mesh.HalfEdges.Count / 2;
        int f = cube.Mesh.Faces.Count;
        Assert.Equal(2, v - e + f);
    }
}
