using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh topology tests — FacesAroundVertex, bounds, AddFace cycle, open mesh detection</summary>
public class HalfEdgeMeshTopologyTests
{
    [Fact]
    public void AddVertex_AssignsSequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
    }

    [Fact]
    public void AddFace_HalfEdge_FaceLink()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);

        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(face, he.Face);
        }
    }

    [Fact]
    public void AddFace_SetsOutgoingEdgeOnVertices()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        Assert.NotNull(v0.OutgoingEdge);
        Assert.NotNull(v1.OutgoingEdge);
        Assert.NotNull(v2.OutgoingEdge);
    }

    [Fact]
    public void FacesAroundVertex_Cube_VertexTouchesMultipleFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
        {
            var faces = mesh.FacesAroundVertex(v).ToList();
            Assert.True(faces.Count >= 3, $"Vertex {v.Id} touches only {faces.Count} faces");
        }
    }

    [Fact]
    public void FacesAroundVertex_SingleTriangle_OnePerVertex()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        Assert.Single(mesh.FacesAroundVertex(v0));
        Assert.Single(mesh.FacesAroundVertex(v1));
        Assert.Single(mesh.FacesAroundVertex(v2));
    }

    [Fact]
    public void Face_Centroid_AverageOfVertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            double cx = verts.Average(v => v.Position.X);
            double cy = verts.Average(v => v.Position.Y);
            double cz = verts.Average(v => v.Position.Z);
            var c = face.Centroid;
            Assert.True(System.Math.Abs(c.X - cx) < 1e-10);
            Assert.True(System.Math.Abs(c.Y - cy) < 1e-10);
            Assert.True(System.Math.Abs(c.Z - cz) < 1e-10);
        }
    }

    [Fact]
    public void Sphere_AllFaceNormals_PointOutward()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var face in mesh.Faces)
        {
            var centroid = face.Centroid;
            var normal = face.UnitNormal;
            double dot = Vec3.Dot(centroid, normal);
            Assert.True(dot > 0, $"Normal should point outward from origin, dot={dot}");
        }
    }

    [Fact]
    public void Cube_AllFaceNormals_UnitLength()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            double len = face.UnitNormal.Length;
            Assert.True(System.Math.Abs(len - 1) < 1e-10);
        }
    }

    [Fact]
    public void OpenMesh_TwoTriangles_BoundaryEdgesHaveNoTwins()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, -1)),
        });

        // The shared edge should have twins, boundary edges should not
        int withTwin = mesh.HalfEdges.Count(he => he.Twin != null);
        int withoutTwin = mesh.HalfEdges.Count(he => he.Twin == null);
        Assert.Equal(2, withTwin); // shared edge
        Assert.Equal(4, withoutTwin); // 4 boundary half-edges
    }

    [Fact]
    public void MeshValidator_EulerCharacteristic_Cube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void MeshValidator_EulerCharacteristic_Sphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void MeshValidator_AllEdgesHaveTwins_Cube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void MeshValidator_IsEdgeManifold_Cube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
    }

    [Fact]
    public void MeshValidator_IsConsistentlyOriented_Cube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void MeshValidator_HasValidFaceCycles_Cube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void MeshValidationResult_IsClosedManifold_Cube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(18, result.EdgeCount);
        Assert.Equal(12, result.FaceCount);
    }
}
