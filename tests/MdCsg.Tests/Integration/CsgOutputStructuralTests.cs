using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG output structural invariants — face counts, vertex validity, mesh topology, BVH integrity</summary>
public class CsgOutputStructuralTests
{
    [Fact]
    public void Union_FaceCount_GreaterOrEqualMax()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        int maxFaces = System.Math.Max(a.Mesh.Faces.Count, b.Mesh.Faces.Count);
        Assert.True(result.Mesh.Faces.Count >= maxFaces,
            $"Union should have at least {maxFaces} faces, got {result.Mesh.Faces.Count}");
    }

    [Fact]
    public void Intersection_FaceCount_LessOrEqualSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        int sumFaces = a.Mesh.Faces.Count + b.Mesh.Faces.Count;
        Assert.True(result.Mesh.Faces.Count <= sumFaces);
    }

    [Fact]
    public void Difference_FaceCount_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_Bounds_EncloseBothInputBounds()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(2, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Min.X <= a.Bounds.Min.X + 1e-10);
        Assert.True(bounds.Min.Y <= a.Bounds.Min.Y + 1e-10);
        Assert.True(bounds.Max.X >= b.Bounds.Max.X - 1e-10);
    }

    [Fact]
    public void Intersection_Bounds_WithinInputHull()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Min.X >= System.Math.Min(a.Bounds.Min.X, b.Bounds.Min.X) - 1e-10);
        Assert.True(bounds.Max.X <= System.Math.Max(a.Bounds.Max.X, b.Bounds.Max.X) + 1e-10);
    }

    [Fact]
    public void Union_ConsistentOrientation()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.Validate(result.Mesh).IsConsistentlyOriented);
    }

    [Fact]
    public void Intersection_ConsistentOrientation()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(MeshValidator.Validate(result.Mesh).IsConsistentlyOriented);
    }

    [Fact]
    public void Difference_ConsistentOrientation()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(MeshValidator.Validate(result.Mesh).IsConsistentlyOriented);
    }

    [Fact]
    public void Union_ValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.Validate(result.Mesh).HasValidFaceCycles);
    }

    [Fact]
    public void AllOutputFaces_HaveThreeEdges()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var he = start;
            do { count++; he = he.Next; } while (he != start);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void AllOutputVertices_FiniteCoordinates()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X));
            Assert.False(double.IsInfinity(v.Position.X));
            Assert.False(double.IsNaN(v.Position.Y));
            Assert.False(double.IsInfinity(v.Position.Y));
        }
    }

    [Fact]
    public void HalfEdges_Count_IsTripleFaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count * 3, result.Mesh.HalfEdges.Count);
    }

    [Fact]
    public void CsgWithSphere_Difference_ProducesCavity()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Bvh_OnOutputSolid_HasPositiveNodeCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        var solid = new Solid(result.Mesh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Bvh_OnOutputSolid_MeshReference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        var solid = new Solid(result.Mesh);
        Assert.Equal(result.Mesh, solid.Bvh.Mesh);
    }

    [Fact]
    public void Union_CubeSphere_ValidFaceCycles()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(MeshValidator.Validate(result.Mesh).HasValidFaceCycles);
    }

    [Fact]
    public void Intersection_CubeSphere_FiniteVertices()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var result = Csg.Intersect(cube, sphere);
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X));
            Assert.False(double.IsInfinity(v.Position.X));
        }
    }
}
