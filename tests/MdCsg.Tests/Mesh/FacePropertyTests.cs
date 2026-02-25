using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face property tests — centroid, normal, unit normal, vertex enumeration</summary>
public class FacePropertyTests
{
    [Fact]
    public void Face_GetVertices_ReturnsThreeVertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            Assert.Equal(3, verts.Count);
        }
    }

    [Fact]
    public void Face_GetTrianglePositions_MatchesGetVertices()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var verts = face.GetVertices();
            Assert.Equal(verts[0].Position, a);
            Assert.Equal(verts[1].Position, b);
            Assert.Equal(verts[2].Position, c);
        }
    }

    [Fact]
    public void Face_Centroid_IsAverage()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var expected = (a + b + c) / 3.0;
            var centroid = face.Centroid;
            Assert.True(Vec3.DistanceSquared(centroid, expected) < 1e-20);
        }
    }

    [Fact]
    public void Face_Normal_PerpToEdges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var n = face.Normal;
            // Normal should be perpendicular to both edges
            Assert.True(System.Math.Abs(Vec3.Dot(n, b - a)) < 1e-10);
            Assert.True(System.Math.Abs(Vec3.Dot(n, c - a)) < 1e-10);
        }
    }

    [Fact]
    public void Face_UnitNormal_HasLengthOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var un = face.UnitNormal;
            Assert.True(System.Math.Abs(un.Length - 1.0) < 1e-10, $"Face {face.Id}: length={un.Length}");
        }
    }

    [Fact]
    public void Face_Normal_CrossProduct()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var expected = Vec3.Cross(b - a, c - a);
            var actual = face.Normal;
            Assert.True(Vec3.DistanceSquared(expected, actual) < 1e-20);
        }
    }

    [Fact]
    public void Face_Id_UniqueAcrossMesh()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var ids = mesh.Faces.Select(f => f.Id).ToHashSet();
        Assert.Equal(mesh.Faces.Count, ids.Count);
    }

    [Fact]
    public void Face_OriginalFaceId_DefaultEqualsId()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            Assert.Equal(face.Id, face.OriginalFaceId);
        }
    }

    [Fact]
    public void Face_PatchId_DefaultIsNegativeOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            Assert.Equal(-1, face.PatchId);
        }
    }

    [Fact]
    public void Face_Edge_NotNull()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            Assert.NotNull(face.Edge);
        }
    }

    [Fact]
    public void Face_Centroid_InsideCubeBounds()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var c = face.Centroid;
            Assert.True(c.X >= -0.01 && c.X <= 1.01);
            Assert.True(c.Y >= -0.01 && c.Y <= 1.01);
            Assert.True(c.Z >= -0.01 && c.Z <= 1.01);
        }
    }

    [Fact]
    public void Face_UnitNormal_CubeAxialAligned()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var n = face.UnitNormal;
            // Each face of a cube should have a normal along one axis
            int axialCount = 0;
            if (System.Math.Abs(System.Math.Abs(n.X) - 1) < 0.01) axialCount++;
            if (System.Math.Abs(System.Math.Abs(n.Y) - 1) < 0.01) axialCount++;
            if (System.Math.Abs(System.Math.Abs(n.Z) - 1) < 0.01) axialCount++;
            Assert.True(axialCount == 1, $"Face {face.Id}: normal=({n.X},{n.Y},{n.Z})");
        }
    }

    [Fact]
    public void Face_Sphere_AllNormalsPointOutward()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        foreach (var face in mesh.Faces)
        {
            var centroid = face.Centroid;
            var normal = face.UnitNormal;
            // Normal should point away from center (0,0,0)
            double dot = Vec3.Dot(normal, centroid);
            Assert.True(dot > 0, $"Face {face.Id}: dot={dot}, should be positive");
        }
    }
}
