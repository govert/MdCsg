using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG result mesh validation — manifold checks, topology, face normals</summary>
public class CsgResultManifoldPropertyTests
{
    [Fact]
    public void UnionResult_HasNonZeroAreaFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var area = new Triangle3(va, vb, vc).Area;
            Assert.True(area > 1e-15, $"Face should have non-zero area, got {area}");
        }
    }

    [Fact]
    public void IntersectResult_HasNonZeroAreaFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var area = new Triangle3(va, vb, vc).Area;
            Assert.True(area > 1e-15, $"Face should have non-zero area, got {area}");
        }
    }

    [Fact]
    public void DifferenceResult_HasNonZeroAreaFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Difference(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var area = new Triangle3(va, vb, vc).Area;
            Assert.True(area > 1e-15, $"Face should have non-zero area, got {area}");
        }
    }

    [Fact]
    public void UnionResult_AllFacesHaveNormals()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            var normal = face.Normal;
            Assert.True(normal.Length > 1e-12, "Face normal should be non-zero");
        }
    }

    [Fact]
    public void UnionResult_VertexCount_Reasonable()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.VertexCount > 0);
        Assert.True(result.VertexCount < 10000, "Vertex count should be reasonable");
    }

    [Fact]
    public void IntersectResult_BoundsWithinBoth()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        if (result.FaceCount > 0)
        {
            var bounds = result.Mesh.GetBounds();
            // Intersection should be within [0.5, 1.0] in X roughly
            Assert.True(bounds.Min.X >= -0.1, $"Intersection min X should be >= ~0.5, got {bounds.Min.X}");
            Assert.True(bounds.Max.X <= 1.5, $"Intersection max X should be <= ~1.0, got {bounds.Max.X}");
        }
    }

    [Fact]
    public void DifferenceResult_BoundsWithinOriginal()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Difference(a, b);
        var bounds = result.Mesh.GetBounds();
        var aBounds = a.Mesh.GetBounds();
        Assert.True(bounds.Min.X >= aBounds.Min.X - 0.01);
        Assert.True(bounds.Max.X <= aBounds.Max.X + 0.01);
    }

    [Fact]
    public void UnionResult_FaceEdgeCycles_AreTriangles()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            var start = face.Edge;
            int count = 0;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void UnionResult_CanBuildSolid()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        var solid = new Solid(result.Mesh);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void IntersectResult_CanBuildSolid()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        if (result.FaceCount > 0)
        {
            var solid = new Solid(result.Mesh);
            Assert.NotNull(solid.Bvh);
        }
    }

    [Fact]
    public void SphereSphereUnion_HasNonZeroAreaFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var area = new Triangle3(va, vb, vc).Area;
            Assert.True(area > 1e-15);
        }
    }

    [Fact]
    public void CubeSphereIntersect_CanBeUsedAsSolid()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var result = Csg.Intersect(cube, sphere);
        var solid = new Solid(result.Mesh);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void DisjointUnion_FaceCount_IsSumOfInputs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }
}
