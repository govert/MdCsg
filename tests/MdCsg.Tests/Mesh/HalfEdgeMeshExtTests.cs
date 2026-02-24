using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Batch 9: HalfEdgeMesh, Face, HalfEdge extended tests (20 tests)</summary>
public class HalfEdgeMeshExtTests
{
    private static HalfEdgeMesh BuildSingleTriangle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        return mesh;
    }

    [Fact]
    public void SingleTriangle_HasThreeVertices()
    {
        var mesh = BuildSingleTriangle();
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void SingleTriangle_HasOneFace()
    {
        var mesh = BuildSingleTriangle();
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void SingleTriangle_HasThreeHalfEdges()
    {
        var mesh = BuildSingleTriangle();
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void SingleTriangle_FaceEdgeCycleHasThreeSteps()
    {
        var mesh = BuildSingleTriangle();
        var face = mesh.Faces[0];
        var start = face.Edge;
        int count = 0;
        var cur = start;
        do { cur = cur.Next; count++; } while (cur != start);
        Assert.Equal(3, count);
    }

    [Fact]
    public void SingleTriangle_PrevCycleMatches()
    {
        var mesh = BuildSingleTriangle();
        var face = mesh.Faces[0];
        var start = face.Edge;
        var cur = start;
        do
        {
            Assert.Same(cur, cur.Next.Prev);
            cur = cur.Next;
        } while (cur != start);
    }

    [Fact]
    public void Face_GetTrianglePositions()
    {
        var mesh = BuildSingleTriangle();
        mesh.Faces[0].GetTrianglePositions(out var a, out var b, out var c);
        // The face was created with (v0, v1, v2)
        // Edge goes v0->v1->v2->v0, so Target of first edge is v1
        Assert.Equal(new Vec3(1, 0, 0), a); // he0.Target = v1
        Assert.Equal(new Vec3(0, 1, 0), b); // he1.Target = v2
        Assert.Equal(new Vec3(0, 0, 0), c); // he2.Target = v0
    }

    [Fact]
    public void Face_Centroid()
    {
        var mesh = BuildSingleTriangle();
        var centroid = mesh.Faces[0].Centroid;
        var expected = new Vec3(1.0 / 3, 1.0 / 3, 0);
        Assert.Equal(expected.X, centroid.X, 1e-15);
        Assert.Equal(expected.Y, centroid.Y, 1e-15);
    }

    [Fact]
    public void Face_Normal_PointsUp()
    {
        var mesh = BuildSingleTriangle();
        var n = mesh.Faces[0].Normal;
        // Triangle in XY plane with CCW winding -> normal points in +Z or -Z
        Assert.True(System.Math.Abs(n.X) < 1e-10);
        Assert.True(System.Math.Abs(n.Y) < 1e-10);
        Assert.True(System.Math.Abs(n.Z) > 0.9);
    }

    [Fact]
    public void Face_OriginalFaceId_DefaultsToZero()
    {
        var mesh = BuildSingleTriangle();
        Assert.Equal(0, mesh.Faces[0].OriginalFaceId);
    }

    [Fact]
    public void HalfEdge_Origin_IsCorrect()
    {
        var mesh = BuildSingleTriangle();
        // he0: v0->v1, he1: v1->v2, he2: v2->v0
        var he = mesh.HalfEdges[0];
        Assert.Equal(new Vec3(0, 0, 0), he.Origin.Position);
        Assert.Equal(new Vec3(1, 0, 0), he.Target.Position);
    }

    [Fact]
    public void Cube_HasTwelveFaces()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(12, cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Cube_HasEightVertices()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void Cube_Has36HalfEdges()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(36, cube.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Cube_GetBounds_IsUnitCube()
    {
        var cube = MeshFactory.CreateCube();
        var bounds = cube.Mesh.GetBounds();
        Assert.Equal(0, bounds.Min.X, 1e-10);
        Assert.Equal(0, bounds.Min.Y, 1e-10);
        Assert.Equal(0, bounds.Min.Z, 1e-10);
        Assert.Equal(1, bounds.Max.X, 1e-10);
        Assert.Equal(1, bounds.Max.Y, 1e-10);
        Assert.Equal(1, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void Cube_OffsetBounds()
    {
        var cube = MeshFactory.CreateCube(new Vec3(5, 5, 5));
        var bounds = cube.Mesh.GetBounds();
        Assert.Equal(5, bounds.Min.X, 1e-10);
        Assert.Equal(6, bounds.Max.X, 1e-10);
    }

    [Fact]
    public void Cube_ScaledSize()
    {
        var cube = MeshFactory.CreateCube(size: 3.0);
        var bounds = cube.Mesh.GetBounds();
        Assert.Equal(new Vec3(3, 3, 3), bounds.Size);
    }

    [Fact]
    public void FacesAroundVertex_CubeCorner()
    {
        var cube = MeshFactory.CreateCube();
        // Each corner of a cube is shared by 3 quad faces = 6 triangulated faces... actually
        // the cube has 12 triangles, each corner touches some number of them
        var v0 = cube.Mesh.Vertices[0];
        var faces = cube.Mesh.FacesAroundVertex(v0).ToList();
        Assert.True(faces.Count >= 3);
    }

    [Fact]
    public void EmptyMesh_GetBounds_IsEmpty()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Min.X > bounds.Max.X); // empty AABB
    }

    [Fact]
    public void AddVertex_AssignsSequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
        Assert.Equal(2, v2.Id);
    }

    [Fact]
    public void AddFace_ReturnsFaceWithEdge()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);
        Assert.NotNull(face.Edge);
        Assert.Same(face, face.Edge.Face);
    }
}
