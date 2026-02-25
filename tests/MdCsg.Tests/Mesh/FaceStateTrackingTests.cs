using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face class — OriginalFaceId, PatchId, state tracking, GetVertices, GetTrianglePositions</summary>
public class FaceStateTrackingTests
{
    [Fact]
    public void Constructor_OriginalFaceId_EqualsId()
    {
        var face = new Face(7);
        Assert.Equal(7, face.Id);
        Assert.Equal(7, face.OriginalFaceId);
    }

    [Fact]
    public void PatchId_DefaultMinusOne()
    {
        var face = new Face(0);
        Assert.Equal(-1, face.PatchId);
    }

    [Fact]
    public void PatchId_Settable()
    {
        var face = new Face(0);
        face.PatchId = 42;
        Assert.Equal(42, face.PatchId);
    }

    [Fact]
    public void OriginalFaceId_Settable()
    {
        var face = new Face(0);
        face.OriginalFaceId = 99;
        Assert.Equal(99, face.OriginalFaceId);
    }

    [Fact]
    public void Cube_AllFacesHave_OriginalFaceId()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            Assert.True(mesh.Faces[i].OriginalFaceId >= 0);
        }
    }

    [Fact]
    public void Cube_AllFacesHave_ValidEdge()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            Assert.NotNull(face.Edge);
        }
    }

    [Fact]
    public void GetVertices_Triangle_ReturnsThree()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            Assert.Equal(3, verts.Count);
        }
    }

    [Fact]
    public void GetTrianglePositions_MatchesGetVertices()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            face.GetTrianglePositions(out var a, out var b, out var c);
            Assert.Equal(verts[0].Position, a);
            Assert.Equal(verts[1].Position, b);
            Assert.Equal(verts[2].Position, c);
        }
    }

    [Fact]
    public void Centroid_IsAverageOfVertices()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var expected = (a + b + c) / 3.0;
            var centroid = face.Centroid;
            Assert.Equal(expected.X, centroid.X, 1e-14);
            Assert.Equal(expected.Y, centroid.Y, 1e-14);
            Assert.Equal(expected.Z, centroid.Z, 1e-14);
        }
    }

    [Fact]
    public void Normal_IsNonZero_ForNonDegenerateFace()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var normal = face.Normal;
            Assert.True(normal.Length > 0);
        }
    }

    [Fact]
    public void UnitNormal_HasLength1()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var un = face.UnitNormal;
            Assert.Equal(1.0, un.Length, 1e-14);
        }
    }

    [Fact]
    public void Cube_Faces_AreAxisAligned()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var un = face.UnitNormal;
            // Each face normal should be axis-aligned (one component ±1, others 0)
            double maxComponent = System.Math.Max(System.Math.Abs(un.X),
                System.Math.Max(System.Math.Abs(un.Y), System.Math.Abs(un.Z)));
            Assert.Equal(1.0, maxComponent, 1e-14);
        }
    }

    [Fact]
    public void Tetrahedron_Faces_NormalsNonZero()
    {
        var mesh = TestHelpers.MeshFactory.CreateTetrahedron().Mesh;
        foreach (var face in mesh.Faces)
        {
            Assert.True(face.Normal.Length > 0);
        }
    }

    [Fact]
    public void Face_Id_IsImmutable()
    {
        var face = new Face(5);
        Assert.Equal(5, face.Id);
        // Id has no setter, verified by compilation
    }
}
