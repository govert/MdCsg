using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG chained operations — multi-step unions, nested differences, sphere-cube chains</summary>
public class CsgChainedOperationStressTests
{
    [Fact]
    public void Union_ThreeCubes_HasVertices()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var c = MeshFactory.CreateCube(new Vec3(1.0, 0, 0));
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void DifferenceChain_TwoHoles()
    {
        var block = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var hole1 = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), 0.5);
        var hole2 = MeshFactory.CreateCube(new Vec3(1.0, 0.3, 0.3), 0.5);
        var r1 = Csg.Difference(block, hole1);
        var r2 = Csg.Difference(new Solid(r1.Mesh), hole2);
        Assert.True(r2.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void UnionThenDifference_HasFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var c = MeshFactory.CreateCube(new Vec3(0.25, 0.25, -0.25), 0.5);
        var ab = Csg.Union(a, b);
        var result = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void CubeSphere_AllThreeOps_NonEmpty()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        Assert.True(Csg.Union(cube, sphere).Mesh.Faces.Count > 0);
        Assert.True(Csg.Intersect(cube, sphere).Mesh.Faces.Count > 0);
        Assert.True(Csg.Difference(cube, sphere).Mesh.Faces.Count > 0);
    }

    [Fact]
    public void SphereSphere_AllThreeOps_NonEmpty()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        Assert.True(Csg.Union(a, b).Mesh.Faces.Count > 0);
        Assert.True(Csg.Intersect(a, b).Mesh.Faces.Count > 0);
        Assert.True(Csg.Difference(a, b).Mesh.Faces.Count > 0);
    }

    [Fact]
    public void CubeTetrahedron_Union_HasVertices()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var tet = MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.5);
        var result = Csg.Union(cube, tet);
        Assert.True(result.Mesh.Vertices.Count > 0);
    }

    [Fact]
    public void DisjointCubes_UnionFromTriangles_Works()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        // Verify the result can be used as a Solid
        var solid = new Solid(result.Mesh);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }
}
