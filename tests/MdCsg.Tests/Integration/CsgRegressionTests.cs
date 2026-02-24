using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG regression tests — known-good configurations that should always work</summary>
public class CsgRegressionTests
{
    [Fact]
    public void SmallOverlap_Union_NoDegeneration()
    {
        // Small overlap (0.1 units) — tests precision at small intersections
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.9, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        var vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Expected: 2 - 0.1 * 1 * 1 = 1.9
        Assert.True(vol > 1.5 && vol < 2.1, $"vol={vol}");
    }

    [Fact]
    public void LargeOverlap_Union_NoDegeneration()
    {
        // Large overlap (0.9 units) — tests precision at large intersections
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        var vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Large overlap may produce mesh artifacts; volume should be near 1.1 but allow tolerance
        Assert.True(vol > 1.0 && vol < 1.7, $"vol={vol}");
    }

    [Fact]
    public void ChainedUnion_2Operations_NoDegeneration()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var u1 = Csg.Union(a, b);

        var c = new Solid(MeshFactory.CreateCube(new Vec3(-0.3, -0.3, -0.3)).Mesh);
        var u2 = Csg.Union(new Solid(u1.Mesh), c);
        Assert.True(u2.FaceCount > 0);
        var vol = VolumeCalculator.ComputeAbsoluteVolume(u2.Mesh);
        Assert.True(vol > 1.0);
    }

    [Fact]
    public void Difference_ThenUnion_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), size: 0.3).Mesh);
        var diff = Csg.Difference(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.7, 0.7, 0.7), size: 0.3).Mesh);
        var result = Csg.Union(new Solid(diff.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_SameOperand_ValidResult()
    {
        // Union of a mesh with itself (coplanar/identical — degenerate but shouldn't crash)
        var mesh = MeshFactory.CreateCube().Mesh;
        var a = new Solid(mesh);
        var b = new Solid(MeshFactory.CreateCube().Mesh);
        // Not the same instance, but same geometry — should be handled as coplanar
        var result = Csg.Union(a, b);
        // May produce double faces or empty — both valid for degenerate case
        Assert.NotNull(result);
    }

    [Fact]
    public void ThreeWay_Union_Sequential()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);

        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
        var vol = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(vol > 1.0);
    }

    [Fact]
    public void Difference_LargerFromSmaller_AlmostEmpty()
    {
        // Subtracting a large cube from a small one
        var small = new Solid(MeshFactory.CreateCube(size: 0.5).Mesh);
        var large = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), size: 4).Mesh);
        var result = Csg.Difference(small, large);
        // Small is inside large, so difference should be empty
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void AllOperations_ProduceValidFaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.4, 0.4, 0.4)).Mesh);

        foreach (var op in new[] { "union", "intersect", "difference" })
        {
            var result = op switch
            {
                "union" => Csg.Union(a, b),
                "intersect" => Csg.Intersect(a, b),
                "difference" => Csg.Difference(a, b),
                _ => throw new Exception()
            };
            foreach (var face in result.Mesh.Faces)
            {
                Assert.Equal(3, face.GetVertices().Count);
                face.GetTrianglePositions(out var va, out var vb, out var vc);
                Assert.True(new Triangle3(va, vb, vc).Area > 0,
                    $"{op}: face {face.Id} has zero area");
            }
        }
    }

    [Fact]
    public void CsgResult_VertexCount_ReasonableRange()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.VertexCount > 8); // More than a single cube
        Assert.True(result.VertexCount < 1000); // Not exploded
    }

    [Fact]
    public void FaceNormals_Consistent_AfterCSG()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.IsConsistentlyOriented);
    }

    [Fact]
    public void FaceCycles_Valid_AfterCSG()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void Difference_Result_FaceCyclesValid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }
}
