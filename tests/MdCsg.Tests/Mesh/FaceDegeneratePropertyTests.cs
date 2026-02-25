using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face properties with degenerate/near-degenerate triangles and numerical edge cases</summary>
public class FaceDegeneratePropertyTests
{
    private static HalfEdgeMesh BuildSingleFace(Vec3 a, Vec3 b, Vec3 c)
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(a);
        var v1 = mesh.AddVertex(b);
        var v2 = mesh.AddVertex(c);
        mesh.AddFace(v0, v1, v2);
        return mesh;
    }

    [Fact]
    public void NearDegenerate_ThinTriangle_NormalNotNaN()
    {
        // Very thin triangle (nearly collinear)
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1e-8, 0));
        var face = mesh.Faces[0];
        var n = face.Normal;
        Assert.False(double.IsNaN(n.X) || double.IsNaN(n.Y) || double.IsNaN(n.Z));
    }

    [Fact]
    public void NearDegenerate_UnitNormal_Finite()
    {
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1e-8, 0));
        var face = mesh.Faces[0];
        var n = face.UnitNormal;
        Assert.False(double.IsNaN(n.X) || double.IsNaN(n.Y) || double.IsNaN(n.Z));
        Assert.False(double.IsInfinity(n.X) || double.IsInfinity(n.Y) || double.IsInfinity(n.Z));
    }

    [Fact]
    public void Centroid_IsAverage_XYPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var mesh = BuildSingleFace(a, b, c);
        var centroid = mesh.Faces[0].Centroid;
        Assert.Equal(1.0, centroid.X, 1e-10);
        Assert.Equal(1.0, centroid.Y, 1e-10);
        Assert.Equal(0.0, centroid.Z, 1e-10);
    }

    [Fact]
    public void Centroid_LargeCoordinates()
    {
        var a = new Vec3(1e6, 1e6, 1e6);
        var b = new Vec3(1e6 + 1, 1e6, 1e6);
        var c = new Vec3(1e6, 1e6 + 1, 1e6);
        var mesh = BuildSingleFace(a, b, c);
        var centroid = mesh.Faces[0].Centroid;
        var expected = (a + b + c) / 3.0;
        Assert.Equal(expected.X, centroid.X, 1e-5);
        Assert.Equal(expected.Y, centroid.Y, 1e-5);
        Assert.Equal(expected.Z, centroid.Z, 1e-5);
    }

    [Fact]
    public void Normal_RightHandRule_XYPlane()
    {
        // CCW vertices on XY plane → normal points in +Z
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var n = mesh.Faces[0].Normal;
        Assert.True(n.Z > 0, $"Expected +Z normal, got {n}");
    }

    [Fact]
    public void Normal_Reversed_XYPlane()
    {
        // CW vertices on XY plane → normal points in -Z
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 0, 0));
        var n = mesh.Faces[0].Normal;
        Assert.True(n.Z < 0, $"Expected -Z normal, got {n}");
    }

    [Fact]
    public void UnitNormal_LengthOne()
    {
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var n = mesh.Faces[0].UnitNormal;
        Assert.Equal(1.0, n.Length, 1e-10);
    }

    [Fact]
    public void UnitNormal_SameDirection_AsNormal()
    {
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var face = mesh.Faces[0];
        Assert.True(Vec3.Dot(face.Normal, face.UnitNormal) > 0);
    }

    [Fact]
    public void GetVertices_ReturnsExactly3()
    {
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(3, mesh.Faces[0].GetVertices().Count);
    }

    [Fact]
    public void GetTrianglePositions_MatchesVertexPositions()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var mesh = BuildSingleFace(a, b, c);
        mesh.Faces[0].GetTrianglePositions(out var pa, out var pb, out var pc);
        // The positions should match (in some order based on half-edge traversal)
        var positions = new[] { pa, pb, pc };
        Assert.Contains(a, positions);
        Assert.Contains(b, positions);
        Assert.Contains(c, positions);
    }

    [Fact]
    public void OriginalFaceId_DefaultsToId()
    {
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var face = mesh.Faces[0];
        Assert.Equal(face.Id, face.OriginalFaceId);
    }

    [Fact]
    public void OriginalFaceId_Settable()
    {
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var face = mesh.Faces[0];
        face.OriginalFaceId = 42;
        Assert.Equal(42, face.OriginalFaceId);
    }

    [Fact]
    public void PatchId_DefaultsToMinusOne()
    {
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(-1, mesh.Faces[0].PatchId);
    }

    [Fact]
    public void PatchId_Settable()
    {
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var face = mesh.Faces[0];
        face.PatchId = 7;
        Assert.Equal(7, face.PatchId);
    }

    [Fact]
    public void Normal_TiltedTriangle_Perpendicular()
    {
        // 45-degree tilted triangle
        var mesh = BuildSingleFace(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 1));
        var face = mesh.Faces[0];
        face.GetTrianglePositions(out var a, out var b, out var c);
        var edge1 = b - a;
        var edge2 = c - a;
        var n = face.Normal;
        // Normal should be perpendicular to both edges
        Assert.Equal(0, Vec3.Dot(n, edge1), 1e-10);
        Assert.Equal(0, Vec3.Dot(n, edge2), 1e-10);
    }

    [Fact]
    public void CubeFaces_EachNormal_IsAxisAligned()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var n = face.UnitNormal;
            // For a unit cube, normals should be axis-aligned
            // Exactly one component should be ±1, others 0
            int nonZero = 0;
            if (System.Math.Abs(n.X) > 0.5) nonZero++;
            if (System.Math.Abs(n.Y) > 0.5) nonZero++;
            if (System.Math.Abs(n.Z) > 0.5) nonZero++;
            Assert.Equal(1, nonZero);
        }
    }

    [Fact]
    public void MultiFace_Ids_Sequential()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        for (int i = 0; i < mesh.Faces.Count; i++)
            Assert.Equal(i, mesh.Faces[i].Id);
    }

    [Fact]
    public void MultiFace_Edges_AllNonNull()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
            Assert.NotNull(face.Edge);
    }
}
