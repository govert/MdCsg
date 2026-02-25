using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG result mesh properties — face cycles, positive volume, face/vertex counts</summary>
public class CsgResultMeshPropertiesTests
{
    private static double Vol(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void Union_HasValidFaceCycles()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Intersection_HasValidFaceCycles()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Intersect(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Difference_HasValidFaceCycles()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Difference(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Union_PositiveVolume()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        Assert.True(Vol(result) > 0, $"Union should have positive volume, got {Vol(result)}");
    }

    [Fact]
    public void Intersection_PositiveVolume()
    {
        // Use fresh objects for each CSG call to avoid any mutable state issues
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Intersect(a, b);
        Assert.True(Vol(result) > 0, $"Intersection should have positive volume, got {Vol(result)}");
    }

    [Fact]
    public void Difference_PositiveVolume()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Difference(a, b);
        Assert.True(Vol(result) > 0, $"Difference should have positive volume, got {Vol(result)}");
    }

    [Fact]
    public void InputMesh_Cube_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void InputMesh_Sphere_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void InputMesh_Tetrahedron_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void DisjointCubes_Union_HasMoreFacesThanEitherInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount >= a.Mesh.Faces.Count + b.Mesh.Faces.Count,
            $"Disjoint union should have >= sum of faces: {result.FaceCount} vs {a.Mesh.Faces.Count + b.Mesh.Faces.Count}");
    }

    [Fact]
    public void Result_FaceCount_MatchesMeshFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Result_VertexCount_MatchesMeshVertices()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void Union_MoreFacesThanSingleInput()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > a.Mesh.Faces.Count,
            $"Union should have more faces than single cube: {result.FaceCount} vs {a.Mesh.Faces.Count}");
    }

    [Fact]
    public void Intersection_FewerFacesThanUnion()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        Assert.True(inter.FaceCount <= union.FaceCount,
            $"Intersection should have fewer or equal faces vs union: {inter.FaceCount} vs {union.FaceCount}");
    }

    [Fact]
    public void Union_HasPositiveFaceCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        Assert.True(Csg.Union(a, b).FaceCount > 0);
    }

    [Fact]
    public void Intersection_HasPositiveFaceCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
    }

    [Fact]
    public void Difference_HasPositiveFaceCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void Union_HasPositiveVertexCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        Assert.True(Csg.Union(a, b).VertexCount > 0);
    }

    [Fact]
    public void Intersection_HasPositiveVertexCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        Assert.True(Csg.Intersect(a, b).VertexCount > 0);
    }

    [Fact]
    public void Difference_HasPositiveVertexCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        Assert.True(Csg.Difference(a, b).VertexCount > 0);
    }

    [Fact]
    public void SphereCube_AllOperations_HaveFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }
}
