using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid and CsgResult API edge cases</summary>
public class SolidCsgResultTests
{
    [Fact]
    public void Solid_Cube_HasBvh()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_Cube_Bounds()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        var b = solid.Bounds;
        Assert.True(b.Min.X >= -0.01);
        Assert.True(b.Max.X <= 1.01);
        Assert.True(b.Min.Y >= -0.01);
        Assert.True(b.Max.Y <= 1.01);
    }

    [Fact]
    public void Solid_OffsetCube_Bounds()
    {
        var solid = new Solid(MeshFactory.CreateCube(new Vec3(5, 10, 15)).Mesh);
        var b = solid.Bounds;
        Assert.True(b.Min.X >= 4.9);
        Assert.True(b.Max.X <= 6.1);
        Assert.True(b.Min.Y >= 9.9);
        Assert.True(b.Max.Y <= 11.1);
    }

    [Fact]
    public void Solid_FromTriangles_SingleTriangle()
    {
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(1, solid.Mesh.Faces.Count);
        Assert.Equal(3, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void Solid_FromTriangles_Cube()
    {
        var factory = MeshFactory.CreateCube();
        var tris = new List<Triangle3>();
        foreach (var face in factory.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(12, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_FromIndexed_Tetrahedron()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var triangles = new List<(int, int, int)>
        {
            (0, 2, 1), (0, 1, 3), (0, 3, 2), (1, 2, 3)
        };
        var solid = Solid.FromIndexed(positions, triangles);
        Assert.Equal(4, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void Solid_FromTriangles_WeldsSharedVertices()
    {
        // Two triangles sharing edge AB should weld shared vertices
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1))
        };
        var solid = Solid.FromTriangles(tris);
        // 4 unique positions, welded from 6 raw vertices
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void CsgResult_Union_Metadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        Assert.True(r.VertexCount > 0);
        Assert.True(r.IntersectionSegmentCount > 0);
        Assert.True(r.PatchCountA > 0);
        Assert.True(r.PatchCountB > 0);
    }

    [Fact]
    public void CsgResult_Disjoint_ZeroSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh);
        var r = Csg.Union(a, b);
        Assert.Equal(0, r.IntersectionSegmentCount);
    }

    [Fact]
    public void CsgResult_FaceCount_MatchesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b);
        Assert.Equal(r.Mesh.Faces.Count, r.FaceCount);
    }

    [Fact]
    public void CsgResult_VertexCount_MatchesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Intersect(a, b);
        Assert.Equal(r.Mesh.Vertices.Count, r.VertexCount);
    }

    [Fact]
    public void CsgResult_Difference_HasPatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Difference(a, b);
        Assert.True(r.PatchCountA >= 1);
        Assert.True(r.PatchCountB >= 1);
    }

    [Fact]
    public void CsgOptions_Default_Values()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.GridSize);
        Assert.Equal(1e-8, opts.WeldTolerance);
        Assert.False(opts.UseWindingNumber);
        Assert.Null(opts.ClassificationStrategy);
    }

    [Fact]
    public void CsgOptions_CustomValues_Applied()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions
        {
            GridSize = 1e-6,
            WeldTolerance = 1e-6,
            UseWindingNumber = true
        };
        var r = Csg.Union(a, b, opts);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Solid_Sphere_HasCorrectFaceCount()
    {
        var solid = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        Assert.Equal(320, solid.Mesh.Faces.Count); // icosahedron sub 2 = 20*16 = 320
    }

    [Fact]
    public void Solid_Tetrahedron_HasCorrectCounts()
    {
        var solid = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        Assert.Equal(4, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void CsgResult_Intersect_ContainedCube_ProducesResult()
    {
        var big = new Solid(MeshFactory.CreateCube(size: 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), size: 0.5).Mesh);
        var r = Csg.Intersect(big, small);
        // Contained cube has no intersection segments - result is the small cube itself
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Csg_Union_Commutative_FaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        int fc1 = Csg.Union(a, b).FaceCount;
        int fc2 = Csg.Union(b, a).FaceCount;
        Assert.Equal(fc1, fc2);
    }

    [Fact]
    public void Csg_Intersect_Commutative_FaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        int fc1 = Csg.Intersect(a, b).FaceCount;
        int fc2 = Csg.Intersect(b, a).FaceCount;
        Assert.Equal(fc1, fc2);
    }
}
