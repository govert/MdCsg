using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh — AddVertex, AddFace, FacesAroundVertex, GetBounds, topology</summary>
public class HalfEdgeMeshOperationsPropertyTests
{
    [Fact]
    public void Empty_NoVertices()
    {
        var mesh = new HalfEdgeMesh();
        Assert.Equal(0, mesh.Vertices.Count);
    }

    [Fact]
    public void Empty_NoFaces()
    {
        var mesh = new HalfEdgeMesh();
        Assert.Equal(0, mesh.Faces.Count);
    }

    [Fact]
    public void Empty_NoHalfEdges()
    {
        var mesh = new HalfEdgeMesh();
        Assert.Equal(0, mesh.HalfEdges.Count);
    }

    [Fact]
    public void AddVertex_IncreasesCount()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(Vec3.Zero);
        Assert.Equal(1, mesh.Vertices.Count);
    }

    [Fact]
    public void AddVertex_PreservesPosition()
    {
        var mesh = new HalfEdgeMesh();
        var pos = new Vec3(1, 2, 3);
        var v = mesh.AddVertex(pos);
        Assert.Equal(pos, v.Position);
    }

    [Fact]
    public void AddVertex_AssignsSequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
    }

    [Fact]
    public void AddFace_CreatesThreeHalfEdges()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void AddFace_IncrementsFaceCount()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void AddFace_FaceHasEdge()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);
        Assert.NotNull(face.Edge);
    }

    [Fact]
    public void AddFace_HalfEdgeCycle_Length3()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);
        var start = face.Edge;
        Assert.Same(start, start.Next.Next.Next);
    }

    [Fact]
    public void AddFace_HalfEdgePrev_Consistent()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);
        var he = face.Edge;
        Assert.Same(he, he.Next.Prev);
        Assert.Same(he.Next, he.Next.Next.Prev);
    }

    [Fact]
    public void FacesAroundVertex_CornerVertex_FindsAdjacentFaces()
    {
        var builder = new MeshBuilder();
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        var tris = new[]
        {
            new Triangle3(v0, v1, v2),
            new Triangle3(v1, v3, v2),
        };
        var mesh = builder.Build(tris);
        // v1 (1,0,0) is shared by both faces
        var sharedVertex = mesh.Vertices.First(v => Vec3.DistanceSquared(v.Position, v1) < 1e-10);
        var faces = mesh.FacesAroundVertex(sharedVertex).ToList();
        Assert.Equal(2, faces.Count);
    }

    [Fact]
    public void FacesAroundVertex_IsolatedVertex_NoFaces()
    {
        var mesh = new HalfEdgeMesh();
        var v = mesh.AddVertex(new Vec3(99, 99, 99));
        var faces = mesh.FacesAroundVertex(v).ToList();
        Assert.Empty(faces);
    }

    [Fact]
    public void GetBounds_Empty_ReturnsEmptyAabb()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        Assert.Equal(Aabb.Empty, bounds);
    }

    [Fact]
    public void GetBounds_SingleVertex_PointBounds()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(new Vec3(3, 4, 5));
        var bounds = mesh.GetBounds();
        Assert.Equal(new Vec3(3, 4, 5), bounds.Min);
        Assert.Equal(new Vec3(3, 4, 5), bounds.Max);
    }

    [Fact]
    public void GetBounds_Cube_CorrectRange()
    {
        var cube = MeshFactory.CreateCube();
        var bounds = cube.Mesh.GetBounds();
        Assert.True(bounds.Min.X >= -0.01 && bounds.Min.X <= 0.01);
        Assert.True(bounds.Max.X >= 0.99 && bounds.Max.X <= 1.01);
    }

    [Fact]
    public void GetBounds_OffsetCube_Shifted()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var bounds = cube.Mesh.GetBounds();
        Assert.True(bounds.Min.X >= 9.99);
        Assert.True(bounds.Max.X <= 11.01);
    }

    [Fact]
    public void Face_Centroid_InsideBounds()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var centroid = face.Centroid;
            var meshBounds = cube.Mesh.GetBounds();
            Assert.True(meshBounds.Contains(centroid),
                $"Face centroid {centroid} should be within mesh bounds");
        }
    }

    [Fact]
    public void Face_Normal_NonZero()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var normal = face.Normal;
            Assert.True(normal.Length > 1e-10, "Face normal should be non-zero");
        }
    }

    [Fact]
    public void Face_UnitNormal_Normalized()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var unitNormal = face.UnitNormal;
            Assert.True(System.Math.Abs(unitNormal.Length - 1.0) < 1e-10,
                $"Unit normal length should be 1.0, got {unitNormal.Length}");
        }
    }

    [Fact]
    public void Face_GetVertices_ReturnsThree()
    {
        var cube = MeshFactory.CreateCube();
        var face = cube.Mesh.Faces[0];
        var verts = face.GetVertices();
        Assert.Equal(3, verts.Count);
    }
}
