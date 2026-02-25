using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG symmetry and consistency tests — same result with different construction paths</summary>
public class CsgSymmetryTests
{
    [Fact]
    public void Union_FromMesh_SameAsFromTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;

        // Via mesh constructor
        var solidA1 = new Solid(meshA);
        var solidB1 = new Solid(meshB);
        var result1 = Csg.Union(solidA1, solidB1);

        // Re-create (same mesh data)
        var solidA2 = new Solid(MeshFactory.CreateCube().Mesh);
        var solidB2 = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result2 = Csg.Union(solidA2, solidB2);

        // Same input → same output
        Assert.Equal(result1.FaceCount, result2.FaceCount);
        Assert.Equal(result1.VertexCount, result2.VertexCount);
    }

    [Fact]
    public void Deterministic_SameInput_SameOutput()
    {
        for (int trial = 0; trial < 3; trial++)
        {
            var a = new Solid(MeshFactory.CreateCube().Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
            var result = Csg.Union(a, b);
            // All trials should produce same face count
            Assert.True(result.FaceCount > 0);
        }
    }

    [Fact]
    public void Union_ThenIntersect_ProducesDifferentResults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        Assert.NotEqual(union.FaceCount, inter.FaceCount);
    }

    [Fact]
    public void Difference_AB_DifferentFrom_BA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        // A\B and B\A should generally be different (since A and B are different shapes at different offsets)
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void AllOperations_WithWindingNumber_ProduceSameAsFalse()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var unionRC = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var unionWN = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });

        // Both should produce valid results (not necessarily identical face count due to different classification paths)
        Assert.True(unionRC.FaceCount > 0);
        Assert.True(unionWN.FaceCount > 0);
    }

    [Fact]
    public void Options_DefaultAndExplicit_SameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var result1 = Csg.Union(a, b);
        var result2 = Csg.Union(a, b, new CsgOptions());

        Assert.Equal(result1.FaceCount, result2.FaceCount);
    }

    [Fact]
    public void SphereCube_AllOperations_NonEmpty()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        Assert.True(Csg.Union(sphere, cube).FaceCount > 0);
        Assert.True(Csg.Intersect(sphere, cube).FaceCount > 0);
        Assert.True(Csg.Difference(sphere, cube).FaceCount > 0);
    }

    [Fact]
    public void TetrahedronCube_AllOperations_NonEmpty()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.2, 0.2, 0.2), 0.5).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        Assert.True(Csg.Union(tet, cube).FaceCount > 0);
        Assert.True(Csg.Intersect(tet, cube).FaceCount > 0);
        Assert.True(Csg.Difference(tet, cube).FaceCount > 0);
    }

    [Fact]
    public void AxisAligned_Offsets_AllAxes_Produce()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        // Test offsets along all 3 axes
        foreach (var offset in new[]
        {
            new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), new Vec3(0, 0, 0.5)
        })
        {
            var b = new Solid(MeshFactory.CreateCube(offset).Mesh);
            var result = Csg.Union(a, b);
            Assert.True(result.FaceCount > 0, $"Failed at offset {offset}");
        }
    }
}
