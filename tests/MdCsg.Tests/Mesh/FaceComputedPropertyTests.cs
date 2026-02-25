using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face — Centroid, Normal, UnitNormal, GetVertices, GetTrianglePositions</summary>
public class FaceComputedPropertyTests
{
    [Fact]
    public void Centroid_IsAverageOfVertices()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(3, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 6, 0));
        mesh.AddFace(v0, v1, v2);
        var face = mesh.Faces[0];
        var centroid = face.Centroid;
        Assert.Equal(1.0, centroid.X, 10);
        Assert.Equal(2.0, centroid.Y, 10);
        Assert.Equal(0.0, centroid.Z, 10);
    }

    [Fact]
    public void Normal_XYPlane_PointsZ()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var normal = mesh.Faces[0].Normal;
        Assert.Equal(0.0, normal.X, 10);
        Assert.Equal(0.0, normal.Y, 10);
        Assert.True(normal.Z != 0, "Normal should have non-zero Z component in XY plane");
    }

    [Fact]
    public void UnitNormal_HasLengthOne()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(5, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 3, 0));
        mesh.AddFace(v0, v1, v2);
        var unitNormal = mesh.Faces[0].UnitNormal;
        Assert.Equal(1.0, unitNormal.Length, 10);
    }

    [Fact]
    public void UnitNormal_NormalizedFromNormal()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(2, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 2, 0));
        mesh.AddFace(v0, v1, v2);
        var face = mesh.Faces[0];
        var normal = face.Normal;
        var unitNormal = face.UnitNormal;
        var expected = normal / normal.Length;
        Assert.Equal(expected.X, unitNormal.X, 10);
        Assert.Equal(expected.Y, unitNormal.Y, 10);
        Assert.Equal(expected.Z, unitNormal.Z, 10);
    }

    [Fact]
    public void GetVertices_ReturnsThreeVertices()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var vertices = mesh.Faces[0].GetVertices();
        Assert.Equal(3, vertices.Count);
    }

    [Fact]
    public void GetVertices_ContainsAllOriginalPositions()
    {
        var p0 = new Vec3(1, 2, 3);
        var p1 = new Vec3(4, 5, 6);
        var p2 = new Vec3(7, 8, 9);
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(p0);
        var v1 = mesh.AddVertex(p1);
        var v2 = mesh.AddVertex(p2);
        mesh.AddFace(v0, v1, v2);
        var vertices = mesh.Faces[0].GetVertices();
        var positions = vertices.Select(v => v.Position).ToList();
        Assert.Contains(p0, positions);
        Assert.Contains(p1, positions);
        Assert.Contains(p2, positions);
    }

    [Fact]
    public void GetTrianglePositions_MatchesGetVertices()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 0, 1));
        mesh.AddFace(v0, v1, v2);
        var face = mesh.Faces[0];
        face.GetTrianglePositions(out var a, out var b, out var c);
        var vertices = face.GetVertices();
        var positions = new HashSet<Vec3> { a, b, c };
        foreach (var vert in vertices)
            Assert.Contains(vert.Position, positions);
    }

    [Fact]
    public void CubeFaces_AllHavePositiveAreaNormals()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var normal = face.Normal;
            Assert.True(normal.Length > 0, $"Face {face.Id} should have positive area normal");
        }
    }

    [Fact]
    public void CubeFaces_UnitNormalsAlignedToAxes()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var n = face.UnitNormal;
            // Each cube face normal should be axis-aligned
            double maxComponent = System.Math.Max(System.Math.Abs(n.X),
                System.Math.Max(System.Math.Abs(n.Y), System.Math.Abs(n.Z)));
            Assert.True(maxComponent > 0.99, $"Cube face normal should be axis-aligned, got ({n.X}, {n.Y}, {n.Z})");
        }
    }

    [Fact]
    public void CubeFaces_CentroidsInsideBounds()
    {
        var cube = MeshFactory.CreateCube();
        var bounds = cube.Mesh.GetBounds();
        foreach (var face in cube.Mesh.Faces)
        {
            Assert.True(bounds.Contains(face.Centroid),
                $"Centroid of face {face.Id} should be inside mesh bounds");
        }
    }

    [Fact]
    public void Face_OriginalFaceId_EqualsIdByDefault()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var face = mesh.Faces[0];
        Assert.Equal(face.Id, face.OriginalFaceId);
    }

    [Fact]
    public void Face_PatchId_DefaultIsMinusOne()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        Assert.Equal(-1, mesh.Faces[0].PatchId);
    }

    [Fact]
    public void SphereFaces_AllHaveUnitNormalLength()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        foreach (var face in sphere.Mesh.Faces)
        {
            double len = face.UnitNormal.Length;
            Assert.True(System.Math.Abs(len - 1.0) < 1e-10,
                $"Face {face.Id} UnitNormal length should be 1, got {len}");
        }
    }

    [Fact]
    public void MultipleFaces_CentroidsDistinct()
    {
        var cube = MeshFactory.CreateCube();
        var centroids = cube.Mesh.Faces.Select(f => f.Centroid).ToList();
        // Not all centroids should be identical (cube has faces on 6 different planes)
        var distinct = centroids.Distinct().ToList();
        Assert.True(distinct.Count > 1, "Cube should have distinct face centroids");
    }

    [Fact]
    public void Normal_CrossProductOrder_ConsistentWithWinding()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var face = mesh.Faces[0];
        face.GetTrianglePositions(out var a, out var b, out var c);
        var expected = Vec3.Cross(b - a, c - a);
        Assert.Equal(expected, face.Normal);
    }
}
