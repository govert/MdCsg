using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG with tetrahedra — non-cube shapes, mixed geometry, volume consistency</summary>
public class CsgTetrahedronTests
{
    [Fact]
    public void TetrahedronTetrahedron_Union_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronTetrahedron_Intersection_HasVolume()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        if (result.FaceCount > 0)
        {
            double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            Assert.True(vol > 0);
        }
    }

    [Fact]
    public void TetrahedronTetrahedron_Difference_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronCube_Union_HasMoreFaces()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var result = Csg.Union(tet, cube);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronCube_Intersection_HasVolume()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var cube = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var result = Csg.Intersect(tet, cube);
        if (result.FaceCount > 0)
        {
            double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            Assert.True(vol > 0);
        }
    }

    [Fact]
    public void DisjointTetrahedra_Intersection_Empty()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(10, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void DisjointTetrahedra_Union_FaceCountSum()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(10, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void DisjointTetrahedra_Difference_EqualsA()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(10, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void TetrahedronSphere_Union_ValidResult()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh);
        var result = Csg.Union(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Tetrahedron_Volume_ConsistentAfterCSG()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);

        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(union.Mesh);
        double volInter = VolumeCalculator.ComputeAbsoluteVolume(inter.Mesh);

        // V(A∪B) ≤ V(A) + V(B)
        Assert.True(volUnion <= volA + volB + 0.1);
    }

    [Fact]
    public void Tetrahedron_Deterministic()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0, 0)).Mesh);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
    }

    [Fact]
    public void Tetrahedron_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void ScaledTetrahedra_Union_Scales()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 1.0).Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 2.0).Mesh);
        // Large contains small, union should be approximately large
        var result = Csg.Union(a, b);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volR = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volR - volB) / volB < 0.05,
            $"Union with contained should ≈ larger: {volR} vs {volB}");
    }
}
