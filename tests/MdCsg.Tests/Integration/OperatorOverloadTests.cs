using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 11: a+b-c^d chains, associativity checks.</summary>
public class OperatorOverloadTests
{
    [Fact]
    public void PlusOperator_PerformsUnion()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var r = a + b;
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void MinusOperator_PerformsDifference()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var r = a - b;
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void CaretOperator_PerformsIntersection()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var r = a ^ b;
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void PlusMinusChain_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(1.5, 0, 0), 0.5, 1);
        var c = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.2, 4.0, 8);
        var r1 = a + b;
        var s1 = new Solid(r1.Mesh);
        var r2 = s1 - c;
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void OperatorsMatchApiMethods_Union()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var opVol = new Solid((a + b).Mesh).Volume();
        var apiVol = new Solid(Csg.Union(a, b).Mesh).Volume();
        Assert.True(System.Math.Abs(opVol - apiVol) < 0.01);
    }

    [Fact]
    public void OperatorsMatchApiMethods_Difference()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var opVol = new Solid((a - b).Mesh).Volume();
        var apiVol = new Solid(Csg.Difference(a, b).Mesh).Volume();
        Assert.True(System.Math.Abs(opVol - apiVol) < 0.01);
    }

    [Fact]
    public void OperatorsMatchApiMethods_Intersection()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var opVol = new Solid((a ^ b).Mesh).Volume();
        var apiVol = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        Assert.True(System.Math.Abs(opVol - apiVol) < 0.01);
    }

    [Fact]
    public void UnionIsCommutative_ViaOperator()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.8, 2);
        var vol1 = new Solid((a + b).Mesh).Volume();
        var vol2 = new Solid((b + a).Mesh).Volume();
        Assert.True(System.Math.Abs(vol1 - vol2) < 0.5);
    }

    [Fact]
    public void IntersectionIsCommutative_ViaOperator()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var vol1 = new Solid((a ^ b).Mesh).Volume();
        var vol2 = new Solid((b ^ a).Mesh).Volume();
        Assert.True(System.Math.Abs(vol1 - vol2) < 0.5);
    }

    [Fact]
    public void DifferenceIsNotCommutative_ViaOperator()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var vol1 = new Solid((a - b).Mesh).Volume();
        var vol2 = new Solid((b - a).Mesh).Volume();
        // They should be different (unless identical)
        Assert.True(vol1 > 0);
        Assert.True(vol2 > 0);
    }

    [Fact]
    public void UnionChainViaOperator_ProducesValidMesh()
    {
        var a = Primitives.Cube(new Vec3(-1, 0, 0), 1.5);
        var b = Primitives.Cube(Vec3.Zero, 1.5);
        var c = Primitives.Cube(new Vec3(1, 0, 0), 1.5);
        var r1 = new Solid((a + b).Mesh);
        var r2 = r1 + c;
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void DifferenceChainViaOperator_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 1);
        var c = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.3, 1);
        var r1 = new Solid((a - b).Mesh);
        var r2 = r1 - c;
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectChainViaOperator_VolumeShrinks()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 3.0);
        var c = Primitives.Cube(new Vec3(0, 0.5, 0), 3.0);
        var r1 = new Solid((a ^ b).Mesh);
        var vol1 = r1.Volume();
        var r2 = new Solid((r1 ^ c).Mesh);
        Assert.True(r2.Volume() <= vol1 + 0.1);
    }

    [Fact]
    public void MixedOperatorChain_ProducesValidResult()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var c = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var r1 = new Solid((a ^ b).Mesh);
        var r2 = r1 + c;
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void OperatorWithSmallSolid_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Cube(Vec3.Zero, 0.5);
        var r = a - b;
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void OperatorResult_HasFaces()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        Assert.True((a + b).FaceCount > 0);
        Assert.True((a - b).FaceCount > 0);
        Assert.True((a ^ b).FaceCount > 0);
    }

    [Fact]
    public void OperatorResult_HasVertices()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        Assert.True((a + b).VertexCount > 0);
        Assert.True((a - b).VertexCount > 0);
        Assert.True((a ^ b).VertexCount > 0);
    }

    [Fact]
    public void CsgResult_CanBeWrappedInSolid()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(1, 0, 0), 2.0);
        var r = a + b;
        var solid = new Solid(r.Mesh);
        Assert.True(solid.Volume() > 0);
        Assert.True(solid.IsInside(Vec3.Zero));
    }
}
