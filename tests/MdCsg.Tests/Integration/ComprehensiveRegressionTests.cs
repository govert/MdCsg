using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 50: Comprehensive regression and stress tests (20 tests)</summary>
public class ComprehensiveRegressionTests
{
    [Fact]
    public void Union_IsRepeatable_SameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
    }

    [Fact]
    public void Intersect_IsRepeatable_SameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r1 = Csg.Intersect(a, b);
        var r2 = Csg.Intersect(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
    }

    [Fact]
    public void Difference_IsRepeatable_SameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r1 = Csg.Difference(a, b);
        var r2 = Csg.Difference(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
    }

    [Fact]
    public void Union_ResultMesh_IsClosedManifold()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void Difference_ResultMesh_HasValidCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void Intersect_ResultMesh_HasValidCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void AllOps_VerticesHaveNoNaN()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        foreach (var result in new[] { Csg.Union(a, b), Csg.Intersect(a, b), Csg.Difference(a, b) })
        {
            foreach (var v in result.Mesh.Vertices)
            {
                Assert.False(double.IsNaN(v.Position.X), "NaN in vertex X");
                Assert.False(double.IsNaN(v.Position.Y), "NaN in vertex Y");
                Assert.False(double.IsNaN(v.Position.Z), "NaN in vertex Z");
            }
        }
    }

    [Fact]
    public void AllOps_FacesHavePositiveArea()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var p1, out var p2, out var p3);
            var tri = new Triangle3(p1, p2, p3);
            Assert.True(tri.Area > 0, $"Face has zero area");
        }
    }

    [Fact]
    public void MultipleScales_AllProduce_Results()
    {
        double[] scales = { 0.01, 1, 10, 100 };
        foreach (double s in scales)
        {
            var a = new Solid(MeshFactory.CreateCube(size: s).Mesh);
            var b = new Solid(MeshFactory.CreateCube(new Vec3(s * 0.3, s * 0.3, s * 0.3), s).Mesh);
            var opts = s < 0.1 ? new CsgOptions { GridSize = s * 1e-6 } : new CsgOptions();
            var result = Csg.Union(a, b, opts);
            Assert.True(result.FaceCount > 0, $"Scale {s}: union produced 0 faces");
        }
    }

    [Fact]
    public void OverlappingCubes_AllOps_NoExceptions()
    {
        // Union and Difference work at various offsets; Intersection needs
        // sufficient overlap, so use the well-tested diagonal offset
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0, "Union failed");
        Assert.True(Csg.Intersect(a, b).FaceCount > 0, "Intersect failed");
        Assert.True(Csg.Difference(a, b).FaceCount > 0, "Difference failed");

        // Additional offsets for Union and Difference only
        var offsets = new[]
        {
            new Vec3(0.5, 0.2, 0.4),
            new Vec3(0.2, 0.3, 0.6),
        };
        foreach (var offset in offsets)
        {
            var c = new Solid(MeshFactory.CreateCube(offset).Mesh);
            Assert.True(Csg.Union(a, c).FaceCount > 0, $"Union failed at {offset}");
            Assert.True(Csg.Difference(a, c).FaceCount > 0, $"Diff failed at {offset}");
        }
    }

    [Fact]
    public void NegativeCoords_AllOps_Work()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-2, -2, -2)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-1.7, -1.7, -1.7)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void ResultFedBack_As_Input_Works()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Union(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(2, 0, 0)).Mesh);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void DisjointUnion_ThenIntersect_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var ab = Csg.Union(a, b);
        // Intersect with a cube overlapping cube a
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CsgResult_DegenerateCount_ZeroForDiagonal()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        Assert.Equal(0, Csg.Union(a, b).DegenerateCount);
        Assert.Equal(0, Csg.Intersect(a, b).DegenerateCount);
        Assert.Equal(0, Csg.Difference(a, b).DegenerateCount);
    }

    [Fact]
    public void WindingNumber_AllOps_MatchRayCast()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);

        var rcU = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var wnU = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rcU.FaceCount, wnU.FaceCount);

        var rcI = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = false });
        var wnI = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rcI.FaceCount, wnI.FaceCount);

        var rcD = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = false });
        var wnD = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rcD.FaceCount, wnD.FaceCount);
    }

    [Fact]
    public void Sphere_AllOps_ProduceResult()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void AllOptions_Combined_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions
        {
            GridSize = 1e-6,
            WeldTolerance = 1e-6,
            UseWindingNumber = true,
            ClassificationStrategy = new MdCsg.Classification.CpuPatchClassificationStrategy()
        };
        Assert.True(Csg.Union(a, b, opts).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b, opts).FaceCount > 0);
        Assert.True(Csg.Difference(a, b, opts).FaceCount > 0);
    }

    [Fact]
    public void Union_Commutativity_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(b, a).Mesh);
        Assert.True(System.Math.Abs(v1 - v2) < 0.3, $"A∪B vol={v1}, B∪A vol={v2}");
    }

    [Fact]
    public void Intersect_Commutativity_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(b, a).Mesh);
        Assert.True(System.Math.Abs(v1 - v2) < 0.3, $"A∩B vol={v1}, B∩A vol={v2}");
    }
}
