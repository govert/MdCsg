using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG mesh quality — all faces are triangles, normals non-degenerate, vertex positions finite</summary>
public class CsgMeshQualityPropertyTests
{
    [Fact]
    public void Union_CubeCube_AllFacesTriangles()
    {
        var result = OverlappingCubeUnion();
        AssertAllFacesAreTriangles(result.Mesh);
    }

    [Fact]
    public void Union_CubeCube_AllNormalsNonZero()
    {
        var result = OverlappingCubeUnion();
        foreach (var face in result.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-30,
                "Face normal should be non-zero");
        }
    }

    [Fact]
    public void Union_CubeCube_AllVerticesFinite()
    {
        var result = OverlappingCubeUnion();
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X));
            Assert.False(double.IsNaN(v.Position.Y));
            Assert.False(double.IsNaN(v.Position.Z));
            Assert.False(double.IsInfinity(v.Position.X));
            Assert.False(double.IsInfinity(v.Position.Y));
            Assert.False(double.IsInfinity(v.Position.Z));
        }
    }

    [Fact]
    public void Intersect_SphereSphere_AllFacesTriangles()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Intersect(a, b);
        AssertAllFacesAreTriangles(result.Mesh);
    }

    [Fact]
    public void Intersect_SphereSphere_AllVerticesFinite()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Intersect(a, b);
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X));
            Assert.False(double.IsInfinity(v.Position.X));
        }
    }

    [Fact]
    public void Difference_CubeSphere_AllFacesTriangles()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1.5, 1.5, 1.5), 0.5, 1);
        var result = Csg.Difference(cube, sphere);
        AssertAllFacesAreTriangles(result.Mesh);
    }

    [Fact]
    public void Union_CubeCube_AllHalfEdgesHaveTarget()
    {
        var result = OverlappingCubeUnion();
        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Target);
        }
    }

    [Fact]
    public void Union_CubeCube_AllHalfEdgesHaveNextAndPrev()
    {
        var result = OverlappingCubeUnion();
        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Next);
            Assert.NotNull(he.Prev);
        }
    }

    [Fact]
    public void Union_CubeCube_NextPrevAreInverse()
    {
        var result = OverlappingCubeUnion();
        foreach (var he in result.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Union_CubeCube_FaceEdgeCyclesCorrect()
    {
        var result = OverlappingCubeUnion();
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do { count++; current = current.Next; } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Difference_SphereSphere_AllNormalsNonZero()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Difference(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-30);
        }
    }

    [Fact]
    public void Union_CubeSphere_BoundsEncloseResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.True(v.Position.X >= bounds.Min.X - 1e-10);
            Assert.True(v.Position.Y >= bounds.Min.Y - 1e-10);
            Assert.True(v.Position.Z >= bounds.Min.Z - 1e-10);
            Assert.True(v.Position.X <= bounds.Max.X + 1e-10);
            Assert.True(v.Position.Y <= bounds.Max.Y + 1e-10);
            Assert.True(v.Position.Z <= bounds.Max.Z + 1e-10);
        }
    }

    private static CsgResult OverlappingCubeUnion()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        return Csg.Union(a, b);
    }

    private static void AssertAllFacesAreTriangles(HalfEdgeMesh mesh)
    {
        foreach (var face in mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do { count++; current = current.Next; } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }
}
