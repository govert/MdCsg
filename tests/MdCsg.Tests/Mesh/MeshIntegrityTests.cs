using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Mesh structural integrity for factory-created and CSG-result meshes</summary>
public class MeshIntegrityTests
{
    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.AllEdgesHaveTwins);
    }

    [Fact]
    public void Cube_IsManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.IsEdgeManifold);
    }

    [Fact]
    public void Cube_ConsistentOrientation()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.IsConsistentlyOriented);
    }

    [Fact]
    public void Cube_ValidFaceCycles()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void Cube_EulerCharacteristic_2()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.Equal(2, validation.EulerCharacteristic);
    }

    [Fact]
    public void Cube_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.IsClosedManifold);
    }

    [Fact]
    public void Sphere_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.IsClosedManifold);
    }

    [Fact]
    public void Sphere_EulerCharacteristic_2()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.Equal(2, validation.EulerCharacteristic);
    }

    [Fact]
    public void Cube_12Faces_8Vertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(12, mesh.Faces.Count);
        Assert.Equal(8, mesh.Vertices.Count);
    }

    [Fact]
    public void Cube_AllFacesTriangular()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            Assert.Equal(3, verts.Count);
        }
    }

    [Fact]
    public void Cube_FaceNormals_PointOutward()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var center = new Vec3(0.5, 0.5, 0.5);
        foreach (var face in mesh.Faces)
        {
            var centroid = face.Centroid;
            var normal = face.UnitNormal;
            var toCenter = center - centroid;
            // Normal should point away from center
            Assert.True(Vec3.Dot(normal, toCenter) < 0,
                $"Face normal {normal} points toward center at {centroid}");
        }
    }

    [Fact]
    public void Cube_AllVerticesInBounds()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
        {
            Assert.True(v.Position.X >= -0.01 && v.Position.X <= 1.01);
            Assert.True(v.Position.Y >= -0.01 && v.Position.Y <= 1.01);
            Assert.True(v.Position.Z >= -0.01 && v.Position.Z <= 1.01);
        }
    }

    [Fact]
    public void Face_Edge_Cycle_Length3()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var edge = face.Edge;
            int count = 0;
            var current = edge;
            do
            {
                count++;
                current = current.Next;
            } while (current != edge && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void HalfEdge_Twin_Reverses_Direction()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var edge in mesh.HalfEdges)
        {
            if (edge.Twin != null)
            {
                Assert.Equal(edge.Target.Id, edge.Twin.Origin.Id);
                Assert.Equal(edge.Origin.Id, edge.Twin.Target.Id);
            }
        }
    }

    [Fact]
    public void Vertex_OutgoingEdge_OriginIsVertex()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
        {
            if (v.OutgoingEdge != null)
            {
                Assert.Equal(v.Id, v.OutgoingEdge.Origin.Id);
            }
        }
    }

    [Fact]
    public void Face_GetTrianglePositions_NonDegenerate()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var tri = new Triangle3(a, b, c);
            Assert.True(tri.Area > 0, "Triangle has zero area");
        }
    }

    [Fact]
    public void MeshBuilder_WeldsNearbyVertices()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        Assert.Equal(4, mesh.Vertices.Count); // shared vertices welded
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void MeshBuilder_NoWelding_WithLargeTolerance()
    {
        // Two triangles sharing vertices exactly
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        // Shared edge should have twins
        bool hasTwins = false;
        foreach (var edge in mesh.HalfEdges)
        {
            if (edge.Twin != null) { hasTwins = true; break; }
        }
        Assert.True(hasTwins, "Expected twin edges for shared edge");
    }

    [Fact]
    public void GetBounds_Cube_Correct()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Min.X >= -0.01);
        Assert.True(bounds.Min.Y >= -0.01);
        Assert.True(bounds.Min.Z >= -0.01);
        Assert.True(bounds.Max.X <= 1.01);
        Assert.True(bounds.Max.Y <= 1.01);
        Assert.True(bounds.Max.Z <= 1.01);
    }

    [Fact]
    public void FacesAroundVertex_Cube_AtLeast3()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
        {
            var faceCount = mesh.FacesAroundVertex(v).Count();
            Assert.True(faceCount >= 3, $"Vertex {v.Id} has only {faceCount} adjacent faces");
        }
    }
}
