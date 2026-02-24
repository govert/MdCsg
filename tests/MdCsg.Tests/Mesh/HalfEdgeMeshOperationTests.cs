using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh operations and structural properties</summary>
public class HalfEdgeMeshOperationTests
{
    [Fact]
    public void Cube_GetBounds_Correct()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Min.X >= -0.01 && bounds.Min.X <= 0.01);
        Assert.True(bounds.Max.X >= 0.99 && bounds.Max.X <= 1.01);
    }

    [Fact]
    public void Cube_FacesAroundVertex_AtLeast3()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var v in mesh.Vertices)
        {
            int count = mesh.FacesAroundVertex(v).Count();
            Assert.True(count >= 3, $"Vertex {v.Id} has {count} faces");
        }
    }

    [Fact]
    public void Cube_FaceGetVertices_Returns3()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            Assert.Equal(3, face.GetVertices().Count);
        }
    }

    [Fact]
    public void Cube_FaceUnitNormal_IsUnit()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            double len = face.UnitNormal.Length;
            Assert.True(System.Math.Abs(len - 1.0) < 0.01, $"Normal length {len}");
        }
    }

    [Fact]
    public void Cube_FaceCentroid_InsideBounds()
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
    public void Cube_HalfEdge_OriginTarget_Distinct()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotEqual(he.Origin.Id, he.Target.Id);
        }
    }

    [Fact]
    public void Cube_HalfEdge_NextPrevCycle()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(he, he.Next.Prev);
            Assert.Equal(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Sphere_GetBounds_ContainsSphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Min.X <= -0.9);
        Assert.True(bounds.Max.X >= 0.9);
    }

    [Fact]
    public void Tetrahedron_4Faces()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(4, mesh.Faces.Count);
    }

    [Fact]
    public void Tetrahedron_4Vertices()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Tetrahedron_AllFacesTriangular()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        foreach (var face in mesh.Faces)
        {
            Assert.Equal(3, face.GetVertices().Count);
        }
    }

    [Fact]
    public void Face_GetTrianglePositions_Valid()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var tri = new Triangle3(a, b, c);
            Assert.True(tri.Area > 0);
        }
    }

    [Fact]
    public void CsgResult_Mesh_AllFacesNonDegenerate()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = MdCsg.Api.Csg.Union(a, b);
        foreach (var face in r.Mesh.Faces)
        {
            face.GetTrianglePositions(out var v0, out var v1, out var v2);
            var tri = new Triangle3(v0, v1, v2);
            Assert.True(tri.Area > 0);
        }
    }

    [Fact]
    public void Sphere_HighSubdivision_ValidCounts()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        // Icosahedron: 20 faces, each subdivision multiplies by 4
        // Sub 1: 80, Sub 2: 320, Sub 3: 1280
        Assert.Equal(1280, mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_LowSubdivision_ValidCounts()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 1).Mesh;
        Assert.Equal(80, mesh.Faces.Count);
    }

    [Fact]
    public void Cube_Scaled_BoundsMatch()
    {
        var mesh = MeshFactory.CreateCube(size: 5).Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Min.X >= -0.1);
        Assert.True(bounds.Max.X <= 5.1);
        Assert.True(bounds.Max.Y <= 5.1);
        Assert.True(bounds.Max.Z <= 5.1);
    }

    [Fact]
    public void Cube_Offset_BoundsMatch()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(10, 20, 30)).Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Min.X >= 9.9);
        Assert.True(bounds.Max.X <= 11.1);
        Assert.True(bounds.Min.Y >= 19.9);
        Assert.True(bounds.Max.Y <= 21.1);
    }
}
