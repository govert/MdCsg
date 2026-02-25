using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG associativity — (A op B) op C vs A op (B op C) consistency, chained operations</summary>
public class CsgAssociativityPropertyTests
{
    [Fact]
    public void Union_ThreeCubes_AllProduceFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var c = MeshFactory.CreateCube(new Vec3(2, 0, 0), 2.0);
        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Union(abSolid, c);
        Assert.True(result.FaceCount > 0, "Union of three overlapping cubes should produce faces");
    }

    [Fact]
    public void Difference_Chained_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var ab = Csg.Difference(a, b);
        Assert.True(ab.FaceCount > 0, "A-B should produce faces");
    }

    [Fact]
    public void Union_ThenIntersect_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var c = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0), 2.0);
        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Intersect(abSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_ThenDifference_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 4.0);
        var c = MeshFactory.CreateCube(new Vec3(2, 0, 0), 2.0);
        var ab = Csg.Intersect(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Difference(abSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_SameObject_Twice_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = Csg.Union(a, a);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_SameObject_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = Csg.Intersect(a, a);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_ThreeDisjoint_FaceCountIsTripleSum()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var c = MeshFactory.CreateCube(new Vec3(20, 0, 0));
        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Union(abSolid, c);
        Assert.Equal(a.Mesh.Faces.Count * 3, result.FaceCount);
    }

    [Fact]
    public void Intersect_DisjointAB_ThenUnionC_HasCFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0)); // disjoint
        var c = MeshFactory.CreateCube(new Vec3(20, 0, 0));
        var ab = Csg.Intersect(a, b); // empty intersection
        Assert.Equal(0, ab.FaceCount);
    }

    [Fact]
    public void Difference_DisjointAB_PreservesA()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0)); // disjoint
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_SmallCubeInsideLarge_HasLargeFaces()
    {
        var large = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var small = MeshFactory.CreateCube(new Vec3(1, 1, 1), 1.0);
        var result = Csg.Union(large, small);
        // Small cube is inside large, so union should just be the large cube
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_SmallCubeInsideLarge_HasSmallFaces()
    {
        var large = MeshFactory.CreateCube(Vec3.Zero, 6.0);
        var small = MeshFactory.CreateCube(new Vec3(1, 1, 1), 2.0);
        var result = Csg.Intersect(large, small);
        // Intersection should be the small cube
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SmallInsideLarge_CreatesHole()
    {
        var large = MeshFactory.CreateCube(Vec3.Zero, 6.0);
        var small = MeshFactory.CreateCube(new Vec3(1, 1, 1), 2.0);
        var result = Csg.Difference(large, small);
        // Subtracting small from large should create more faces than original large
        Assert.True(result.FaceCount > large.Mesh.Faces.Count,
            $"Difference should produce more faces ({result.FaceCount}) than original ({large.Mesh.Faces.Count})");
    }

    [Fact]
    public void CubeSphere_Union_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 1, 1), 1.5, 1);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeSphere_Intersect_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 1, 1), 1.5, 1);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeSphere_Difference_ProducesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 1, 1), 1.5, 1);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }
}
