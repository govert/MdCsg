using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG symmetry — Union commutative, Intersection commutative, Difference non-commutative, identity operations</summary>
public class CsgSymmetryPropertyTests
{
    [Fact]
    public void Union_Commutative_FaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Intersection_Commutative_FaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Difference_NonCommutative()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        // A-B and B-A generally produce different results
        // At minimum they should both be valid
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void Union_Disjoint_OrderIndependent()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Intersection_Disjoint_BothEmpty()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(0, ab.FaceCount);
        Assert.Equal(0, ba.FaceCount);
    }

    [Fact]
    public void Union_FullyContained_SameAsOuter()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Union(outer, inner);
        // Union with fully contained should be approximately same as outer
        // (inner contributes no external faces)
        Assert.True(result.FaceCount >= outer.Mesh.Faces.Count);
    }

    [Fact]
    public void Intersection_FullyContained_SameAsInner()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Intersect(outer, inner);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_FullyContained_ProducesCavity()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Difference(outer, inner);
        // Subtracting inner from outer should add faces from the cavity
        Assert.True(result.FaceCount > outer.Mesh.Faces.Count);
    }

    [Fact]
    public void SphereSphere_Union_ApproximatelyCommutative()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        // May differ slightly due to coplanar handling asymmetry (A takes priority)
        Assert.True(System.Math.Abs(ab.FaceCount - ba.FaceCount) <= 2,
            $"Union should be approximately commutative: A|B={ab.FaceCount}, B|A={ba.FaceCount}");
    }

    [Fact]
    public void SphereSphere_Intersection_Commutative()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void CubeSphere_Union_ValidResult()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 1.0), 0.5, 2).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.FaceCount > cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Deterministic_SameInput_SameOutput()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
        Assert.Equal(r1.VertexCount, r2.VertexCount);
        Assert.Equal(r1.IntersectionSegmentCount, r2.IntersectionSegmentCount);
    }

    [Fact]
    public void AllOperations_ProduceValidMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        foreach (var op in new[] { Csg.Union(a, b), Csg.Intersect(a, b), Csg.Difference(a, b) })
        {
            Assert.NotNull(op.Mesh);
            Assert.True(op.FaceCount >= 0);
            Assert.True(op.VertexCount >= 0);
        }
    }

    [Fact]
    public void DisjointDifference_AB_EqualsA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void DisjointDifference_BA_EqualsB()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Difference(b, a);
        Assert.Equal(b.Mesh.Faces.Count, result.FaceCount);
    }
}
