using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG with winding number classification — verify consistency with raycast for all shapes and ops</summary>
public class CsgWindingNumberTests
{
    private static CsgOptions RayCast => new() { UseWindingNumber = false };
    private static CsgOptions Winding => new() { UseWindingNumber = true };

    [Fact]
    public void CubeCube_Union_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        Assert.Equal(Csg.Union(a, b, RayCast).FaceCount, Csg.Union(a, b, Winding).FaceCount);
    }

    [Fact]
    public void CubeCube_Intersect_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        Assert.Equal(Csg.Intersect(a, b, RayCast).FaceCount, Csg.Intersect(a, b, Winding).FaceCount);
    }

    [Fact]
    public void CubeCube_Difference_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        Assert.Equal(Csg.Difference(a, b, RayCast).FaceCount, Csg.Difference(a, b, Winding).FaceCount);
    }

    [Fact]
    public void SphereSphere_Union_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1, 2).Mesh);
        Assert.Equal(Csg.Union(a, b, RayCast).FaceCount, Csg.Union(a, b, Winding).FaceCount);
    }

    [Fact]
    public void SphereSphere_Intersect_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1, 2).Mesh);
        Assert.Equal(Csg.Intersect(a, b, RayCast).FaceCount, Csg.Intersect(a, b, Winding).FaceCount);
    }

    [Fact]
    public void CubeTetra_Union_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 1).Mesh);
        Assert.Equal(Csg.Union(a, b, RayCast).FaceCount, Csg.Union(a, b, Winding).FaceCount);
    }

    [Fact]
    public void DisjointCubes_Union_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(100, 0, 0)).Mesh);
        Assert.Equal(Csg.Union(a, b, RayCast).FaceCount, Csg.Union(a, b, Winding).FaceCount);
    }

    [Fact]
    public void NestedCubes_Difference_SameFaceCount()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), 0.5).Mesh);
        Assert.Equal(Csg.Difference(outer, inner, RayCast).FaceCount, Csg.Difference(outer, inner, Winding).FaceCount);
    }

    [Fact]
    public void WindingNumber_DisjointCubes_NoDegenerates()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(100, 0, 0)).Mesh);
        var result = Csg.Union(a, b, Winding);
        Assert.Equal(0, result.DegenerateCount);
    }

    [Fact]
    public void WindingNumber_OverlappingCubes_NoDegenerates()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b, Winding);
        Assert.Equal(0, result.DegenerateCount);
    }
}
