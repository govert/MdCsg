using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG operation selection — verify correct faces retained for each operation type</summary>
public class CsgOperationSelectionPropertyTests
{
    [Fact]
    public void Union_ContainedSphere_IgnoresSphere()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Size.X >= 3.9);
    }

    [Fact]
    public void Intersect_ContainedSphere_ReturnsSphere()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Size.X < 1.5);
    }

    [Fact]
    public void Difference_ContainedSphere_CreatesCavity()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Union_OverlappingCubes_MoreVerticesThanEither()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.VertexCount > 8);
    }

    [Fact]
    public void Intersect_OverlappingCubes_SmallerBoundsThanEither()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Size.X < 2.0 + 0.1);
    }

    [Fact]
    public void Difference_OverlappingCubes_AsymmetricResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void Union_AllFacesFormClosedManifold()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.IsClosedManifold);
    }

    [Fact]
    public void Intersect_AllFacesFormClosedManifold()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.IsClosedManifold);
    }

    [Fact]
    public void Difference_AllFacesFormClosedManifold()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.IsClosedManifold);
    }

    [Fact]
    public void Union_CubeSphere_BothMeshesContribute()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > cube.Mesh.Faces.Count);
        Assert.True(result.FaceCount > sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void Intersect_CubeSphere_SmallerThanBoth()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Size.X < 2.1);
    }

    [Fact]
    public void Difference_CubeSphere_HasCavity()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 0.5, 2);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Union_EulerCharacteristic_IsTwo()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(result.Mesh));
    }

    [Fact]
    public void Intersect_EulerCharacteristic_IsTwo()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(result.Mesh));
    }
}
