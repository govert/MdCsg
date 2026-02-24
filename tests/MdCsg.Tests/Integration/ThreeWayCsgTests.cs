using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Three-way and chained CSG operations</summary>
public class ThreeWayCsgTests
{
    [Fact]
    public void ThreeWayUnion_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void ThreeWayUnion_Volume_LessThan3()
    {
        // Three overlapping unit cubes → volume should be between 1 and 3
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(vol > 0.9 && vol < 3.1, $"Three-way union vol: {vol}");
    }

    [Fact]
    public void ChainedDifference_ShrinksCube()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1), size: 0.5).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 1.0, 1.0), size: 0.5).Mesh);
        var ab = Csg.Difference(a, b);
        var abc = Csg.Difference(new Solid(ab.Mesh), c);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vabc = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(vabc < va, "Chained differences should reduce volume");
    }

    [Fact]
    public void Union_ThenIntersection_ConsistentResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var union_ab = Csg.Union(a, b);
        var result = Csg.Intersect(new Solid(union_ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersection_ThenUnion_ConsistentResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var intersect_ab = Csg.Intersect(a, b);
        var result = Csg.Union(new Solid(intersect_ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeSphere_Union_ThenDifference()
    {
        var cube = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(1, 1, 1), 0.8, 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.3).Mesh);

        var union = Csg.Union(cube, sphere);
        var result = Csg.Difference(new Solid(union.Mesh), small);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void AllOps_NoNaN_ThreeWay()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.6, 0.6, 0.6)).Mesh);

        var r1 = Csg.Union(a, b);
        var r2 = Csg.Difference(new Solid(r1.Mesh), c);

        foreach (var v in r2.Mesh.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X));
            Assert.False(double.IsNaN(v.Position.Y));
            Assert.False(double.IsNaN(v.Position.Z));
            Assert.False(double.IsInfinity(v.Position.X));
            Assert.False(double.IsInfinity(v.Position.Y));
            Assert.False(double.IsInfinity(v.Position.Z));
        }
    }

    [Fact]
    public void Difference_CubeMinus2Spheres()
    {
        var cube = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var s1 = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var s2 = new Solid(MeshFactory.CreateSphere(new Vec3(1.5, 1.5, 1.5), 0.3, 2).Mesh);

        var r1 = Csg.Difference(cube, s1);
        var r2 = Csg.Difference(new Solid(r1.Mesh), s2);
        double vCube = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double vResult = VolumeCalculator.ComputeAbsoluteVolume(r2.Mesh);
        Assert.True(vResult < vCube, "Cube minus spheres should be smaller");
    }

    [Fact]
    public void DeMorgan_Union_Complement()
    {
        // A ∪ B = complement(complement(A) ∩ complement(B))
        // We can't compute complements, but we can check:
        // A ∪ B should have the same volume as B ∪ A (commutativity)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vab = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double vba = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(b, a).Mesh);
        Assert.True(System.Math.Abs(vab - vba) < 0.3, $"Union commutativity: {vab} vs {vba}");
    }

    [Fact]
    public void Intersection_Commutativity_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vab = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double vba = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(b, a).Mesh);
        Assert.True(System.Math.Abs(vab - vba) < 0.3, $"Intersection commutativity: {vab} vs {vba}");
    }

    [Fact]
    public void Difference_Associativity_Volume()
    {
        // (A \ B) \ C vs A \ (B ∪ C) — these should give similar volumes
        var a = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1), size: 0.4).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.1, 1.1, 1.1), size: 0.4).Mesh);

        var ab = Csg.Difference(a, b);
        var abc1 = Csg.Difference(new Solid(ab.Mesh), c);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(abc1.Mesh);

        var bc = Csg.Union(b, c);
        var abc2 = Csg.Difference(a, new Solid(bc.Mesh));
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(abc2.Mesh);

        Assert.True(System.Math.Abs(v1 - v2) < 1.0,
            $"Diff associativity: {v1} vs {v2}");
    }

    [Fact]
    public void AllOps_AllFacesTriangular_ThreeWay()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var r = Csg.Union(a, b);
        var result = Csg.Difference(new Solid(r.Mesh), c);

        foreach (var face in result.Mesh.Faces)
        {
            var verts = face.GetVertices();
            Assert.Equal(3, verts.Count);
        }
    }

    [Fact]
    public void SphereUnion_ValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(r.Mesh));
    }
}
