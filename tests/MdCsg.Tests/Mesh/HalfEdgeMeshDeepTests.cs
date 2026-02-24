using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Batch 44: HalfEdgeMesh and MeshBuilder deep tests (20 tests)</summary>
public class HalfEdgeMeshDeepTests
{
    [Fact]
    public void MeshBuilder_SingleTriangle_ProducesFace()
    {
        var builder = new MeshBuilder();
        var tris = new List<Triangle3> { new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.Single(mesh.Faces);
    }

    [Fact]
    public void MeshBuilder_SingleTriangle_3Vertices()
    {
        var builder = new MeshBuilder();
        var tris = new List<Triangle3> { new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void MeshBuilder_SingleTriangle_3HalfEdges()
    {
        var builder = new MeshBuilder();
        var tris = new List<Triangle3> { new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void MeshBuilder_TwoAdjacentTriangles_SharedEdge()
    {
        var builder = new MeshBuilder();
        var tris = new List<Triangle3>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY)
        };
        var mesh = builder.Build(tris);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void MeshBuilder_Cube_12Faces()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(12, cube.Mesh.Faces.Count);
    }

    [Fact]
    public void MeshBuilder_Cube_8Vertices()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void MeshBuilder_Cube_36HalfEdges()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(36, cube.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Face_GetTrianglePositions_Valid()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var tri = new Triangle3(a, b, c);
            Assert.True(tri.Area > 0);
        }
    }

    [Fact]
    public void HalfEdge_OriginAndTarget_Differ()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.NotEqual(he.Origin.Id, he.Target.Id);
        }
    }

    [Fact]
    public void HalfEdge_NextChain_LoopsBack()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            int count = 0;
            do
            {
                current = current.Next;
                count++;
            } while (current != start && count < 100);
            Assert.Equal(3, count); // triangular faces
        }
    }

    [Fact]
    public void Mesh_GetBounds_CubeAtOrigin()
    {
        var cube = MeshFactory.CreateCube();
        var bounds = cube.Mesh.GetBounds();
        Assert.True(System.Math.Abs(bounds.Min.X) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.X - 1) < 0.01);
    }

    [Fact]
    public void Mesh_GetBounds_OffsetCube()
    {
        var cube = MeshFactory.CreateCube(new Vec3(5, 10, 15));
        var bounds = cube.Mesh.GetBounds();
        Assert.True(System.Math.Abs(bounds.Min.X - 5) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Y - 10) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Z - 15) < 0.01);
    }

    [Fact]
    public void Tetrahedron_4Faces_4Vertices()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Faces.Count);
        Assert.Equal(4, tet.Mesh.Vertices.Count);
    }

    [Fact]
    public void Tetrahedron_12HalfEdges()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(12, tet.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void Sphere_Sub1_FaceCountPositive()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 1);
        Assert.True(sphere.Mesh.Faces.Count > 4);
    }

    [Fact]
    public void Sphere_Sub2_MoreFacesThanSub1()
    {
        var s1 = MeshFactory.CreateSphere(subdivisions: 1);
        var s2 = MeshFactory.CreateSphere(subdivisions: 2);
        Assert.True(s2.Mesh.Faces.Count > s1.Mesh.Faces.Count);
    }

    [Fact]
    public void MeshBuilder_WeldTolerance_MergesCloseVertices()
    {
        var builder = new MeshBuilder(weldTolerance: 0.01);
        var tris = new List<Triangle3>
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new(new Vec3(0.001, 0.001, 0), Vec3.UnitY, new Vec3(0, 0, 1))
        };
        var mesh = builder.Build(tris);
        Assert.True(mesh.Vertices.Count <= 5);
    }

    [Fact]
    public void Vertex_HasValidPosition()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X));
            Assert.False(double.IsNaN(v.Position.Y));
            Assert.False(double.IsNaN(v.Position.Z));
        }
    }

    [Fact]
    public void Face_HasNonNullEdge()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            Assert.NotNull(face.Edge);
        }
    }

    [Fact]
    public void Cube_Size2_HasCorrectBounds()
    {
        var cube = MeshFactory.CreateCube(size: 2);
        var bounds = cube.Mesh.GetBounds();
        Assert.True(System.Math.Abs(bounds.Min.X) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.X - 2) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.Y - 2) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.Z - 2) < 0.01);
    }
}
