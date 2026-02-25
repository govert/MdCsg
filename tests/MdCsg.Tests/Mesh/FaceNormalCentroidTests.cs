using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face property tests — Normal, UnitNormal, Centroid, GetVertices, GetTrianglePositions</summary>
public class FaceNormalCentroidTests
{
    private static HalfEdgeMesh BuildSingleTriangleMesh(Vec3 a, Vec3 b, Vec3 c)
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(a);
        var v1 = mesh.AddVertex(b);
        var v2 = mesh.AddVertex(c);
        mesh.AddFace(v0, v1, v2);
        return mesh;
    }

    [Fact]
    public void Normal_XYPlane_PointsAlongZ()
    {
        var mesh = BuildSingleTriangleMesh(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var face = mesh.Faces[0];
        var n = face.Normal;
        Assert.True(n.Z > 0);
        Assert.True(System.Math.Abs(n.X) < 1e-10);
        Assert.True(System.Math.Abs(n.Y) < 1e-10);
    }

    [Fact]
    public void Normal_XZPlane_PointsAlongNegY()
    {
        var mesh = BuildSingleTriangleMesh(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        var face = mesh.Faces[0];
        var n = face.Normal;
        // Cross(B-A, C-A) = Cross((1,0,0),(0,0,1)) = (0,-1,0)
        Assert.True(n.Y < 0);
    }

    [Fact]
    public void UnitNormal_LengthOne()
    {
        var mesh = BuildSingleTriangleMesh(
            new Vec3(0, 0, 0), new Vec3(5, 0, 0), new Vec3(0, 5, 0));
        var face = mesh.Faces[0];
        var un = face.UnitNormal;
        Assert.Equal(1.0, un.Length, 1e-10);
    }

    [Fact]
    public void UnitNormal_SameDirection_AsFaceNormal()
    {
        var mesh = BuildSingleTriangleMesh(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var face = mesh.Faces[0];
        var n = face.Normal;
        var un = face.UnitNormal;
        double dot = Vec3.Dot(n, un);
        Assert.True(dot > 0);
    }

    [Fact]
    public void Centroid_AverageOfVertices()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var mesh = BuildSingleTriangleMesh(a, b, c);
        var face = mesh.Faces[0];
        var centroid = face.Centroid;
        Assert.Equal(1.0, centroid.X, 1e-10);
        Assert.Equal(1.0, centroid.Y, 1e-10);
        Assert.Equal(0.0, centroid.Z, 1e-10);
    }

    [Fact]
    public void Centroid_Offset_Triangle()
    {
        var a = new Vec3(10, 20, 30);
        var b = new Vec3(13, 20, 30);
        var c = new Vec3(10, 23, 30);
        var mesh = BuildSingleTriangleMesh(a, b, c);
        var centroid = mesh.Faces[0].Centroid;
        Assert.Equal(11.0, centroid.X, 1e-10);
        Assert.Equal(21.0, centroid.Y, 1e-10);
        Assert.Equal(30.0, centroid.Z, 1e-10);
    }

    [Fact]
    public void GetVertices_Returns3()
    {
        var mesh = BuildSingleTriangleMesh(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var verts = mesh.Faces[0].GetVertices();
        Assert.Equal(3, verts.Count);
    }

    [Fact]
    public void GetTrianglePositions_MatchesVertices()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var mesh = BuildSingleTriangleMesh(a, b, c);
        mesh.Faces[0].GetTrianglePositions(out var pa, out var pb, out var pc);
        // Vertices are stored as targets of half-edges: he0→v1, he1→v2, he2→v0
        // So GetTrianglePositions gets he0.Target=v1=b, he1.Target=v2=c, he2.Target=v0=a
        // Let's just verify all 3 original positions appear
        var positions = new[] { pa, pb, pc };
        Assert.Contains(a, positions);
        Assert.Contains(b, positions);
        Assert.Contains(c, positions);
    }

    [Fact]
    public void Face_Id_Sequential()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(1, 1, 0));
        var f0 = mesh.AddFace(v0, v1, v2);
        var f1 = mesh.AddFace(v1, v3, v2);
        Assert.Equal(0, f0.Id);
        Assert.Equal(1, f1.Id);
    }

    [Fact]
    public void Face_OriginalFaceId_DefaultsSameAsId()
    {
        var mesh = BuildSingleTriangleMesh(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var face = mesh.Faces[0];
        Assert.Equal(face.Id, face.OriginalFaceId);
    }

    [Fact]
    public void Face_PatchId_DefaultNegativeOne()
    {
        var mesh = BuildSingleTriangleMesh(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(-1, mesh.Faces[0].PatchId);
    }

    [Fact]
    public void Face_PatchId_Settable()
    {
        var mesh = BuildSingleTriangleMesh(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        mesh.Faces[0].PatchId = 42;
        Assert.Equal(42, mesh.Faces[0].PatchId);
    }

    [Fact]
    public void FacesAroundVertex_SharedVertex()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(-1, 0, 0));
        mesh.AddFace(v0, v1, v2);
        mesh.AddFace(v0, v2, v3);
        var faces = mesh.FacesAroundVertex(v0).ToList();
        Assert.Equal(2, faces.Count);
    }

    [Fact]
    public void FacesAroundVertex_UnsharedVertex_OneFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(5, 5, 5));
        var v4 = mesh.AddVertex(new Vec3(6, 5, 5));
        var v5 = mesh.AddVertex(new Vec3(5, 6, 5));
        mesh.AddFace(v0, v1, v2);
        mesh.AddFace(v3, v4, v5);
        var facesV0 = mesh.FacesAroundVertex(v0).ToList();
        var facesV3 = mesh.FacesAroundVertex(v3).ToList();
        Assert.Single(facesV0);
        Assert.Single(facesV3);
    }

    [Fact]
    public void Normal_ConsistentWithCrossProduct()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 1, 2);
        var c = new Vec3(2, 5, 1);
        var mesh = BuildSingleTriangleMesh(a, b, c);
        var face = mesh.Faces[0];
        face.GetTrianglePositions(out var pa, out var pb, out var pc);
        var expected = Vec3.Cross(pb - pa, pc - pa);
        var actual = face.Normal;
        Assert.Equal(expected.X, actual.X, 1e-10);
        Assert.Equal(expected.Y, actual.Y, 1e-10);
        Assert.Equal(expected.Z, actual.Z, 1e-10);
    }
}
