using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face normal coherence — Normal, UnitNormal, Centroid, edge traversal, vertex enumeration</summary>
public class FaceNormalCoherenceTests
{
    [Fact]
    public void CubeFaces_AllHaveNonZeroNormal()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var n = face.Normal;
            Assert.True(n.Length > 0, $"Face {face.Id} has zero normal");
        }
    }

    [Fact]
    public void CubeFaces_UnitNormal_HasUnitLength()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var n = face.UnitNormal;
            Assert.Equal(1, n.Length, 1e-10);
        }
    }

    [Fact]
    public void CubeFaces_CentroidInsideBounds()
    {
        var mesh = MeshFactory.CreateCube(Vec3.Zero, 2).Mesh;
        var bounds = mesh.GetBounds();
        foreach (var face in mesh.Faces)
        {
            var c = face.Centroid;
            Assert.True(bounds.Contains(c), $"Centroid {c} outside bounds");
        }
    }

    [Fact]
    public void CubeFaces_GetVertices_Returns3()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
            Assert.Equal(3, face.GetVertices().Count);
    }

    [Fact]
    public void CubeFaces_GetTrianglePositions_NotNaN()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            Assert.False(double.IsNaN(a.X) || double.IsNaN(a.Y) || double.IsNaN(a.Z));
            Assert.False(double.IsNaN(b.X) || double.IsNaN(b.Y) || double.IsNaN(b.Z));
            Assert.False(double.IsNaN(c.X) || double.IsNaN(c.Y) || double.IsNaN(c.Z));
        }
    }

    [Fact]
    public void SphereFaces_NormalsPointOutward()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var center = new Vec3(0, 0, 0);
        int outward = 0;
        int total = 0;
        foreach (var face in mesh.Faces)
        {
            var centroid = face.Centroid;
            var normal = face.UnitNormal;
            var toCenter = center - centroid;
            // Normal should point away from center (dot with to-center should be negative)
            if (Vec3.Dot(normal, toCenter) < 0)
                outward++;
            total++;
        }
        // Most normals should point outward for a well-oriented sphere
        Assert.True(outward > total * 0.9, $"Only {outward}/{total} normals point outward");
    }

    [Fact]
    public void TetrahedronFaces_AllHave3Vertices()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(4, mesh.Faces.Count);
        foreach (var face in mesh.Faces)
            Assert.Equal(3, face.GetVertices().Count);
    }

    [Fact]
    public void Face_OriginalFaceId_DefaultValue()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // After initial construction, OriginalFaceId is not set (default 0)
        foreach (var face in mesh.Faces)
            Assert.True(face.OriginalFaceId >= 0);
    }

    [Fact]
    public void Face_PatchId_DefaultNegative()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
            Assert.Equal(-1, face.PatchId);
    }

    [Fact]
    public void Face_Id_Sequential()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        for (int i = 0; i < mesh.Faces.Count; i++)
            Assert.Equal(i, mesh.Faces[i].Id);
    }

    [Fact]
    public void Face_Edge_NotNull()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
            Assert.NotNull(face.Edge);
    }

    [Fact]
    public void NormalConsistency_AcrossTrianglePair()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // Cube has 6 faces × 2 triangles = 12 faces
        // Pairs of triangles forming the same quad should have similar normals
        // Group by approximate normal direction
        var normalGroups = new Dictionary<string, int>();
        foreach (var face in mesh.Faces)
        {
            var n = face.UnitNormal;
            // Round to nearest axis, normalize -0 to 0
            double rx = System.Math.Round(n.X) + 0.0;
            double ry = System.Math.Round(n.Y) + 0.0;
            double rz = System.Math.Round(n.Z) + 0.0;
            string key = $"{rx},{ry},{rz}";
            if (!normalGroups.ContainsKey(key))
                normalGroups[key] = 0;
            normalGroups[key]++;
        }
        // Should have 6 distinct axis-aligned normal directions, each with 2 triangles
        Assert.Equal(6, normalGroups.Count);
        foreach (var count in normalGroups.Values)
            Assert.Equal(2, count);
    }

    [Fact]
    public void GetBounds_NonEmpty()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Max.X > bounds.Min.X);
        Assert.True(bounds.Max.Y > bounds.Min.Y);
        Assert.True(bounds.Max.Z > bounds.Min.Z);
    }
}
