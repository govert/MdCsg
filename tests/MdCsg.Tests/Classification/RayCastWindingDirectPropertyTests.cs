using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: RayCastClassifier + WindingNumberClassifier — Direct point classification, inside/outside consistency</summary>
public class RayCastWindingDirectPropertyTests
{
    [Fact]
    public void RayCast_PointInsideCube_Inside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_PointOutsideCube_Outside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = RayCastClassifier.Classify(new Vec3(10, 10, 10), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Winding_PointInsideCube_Inside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Winding_PointOutsideCube_Outside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = WindingNumberClassifier.Classify(new Vec3(10, 10, 10), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCast_Winding_Agree_InsideCube()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(1.0, 1.0, 1.0);
        var rayCast = RayCastClassifier.Classify(point, cube.Bvh);
        var winding = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(rayCast, winding);
    }

    [Fact]
    public void RayCast_Winding_Agree_OutsideCube()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(10, 10, 10);
        var rayCast = RayCastClassifier.Classify(point, cube.Bvh);
        var winding = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(rayCast, winding);
    }

    [Fact]
    public void RayCast_MultiplePointsInsideSphere_AllInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var points = new[] {
            new Vec3(0, 0, 0),
            new Vec3(0.5, 0, 0),
            new Vec3(0, 0.5, 0),
            new Vec3(0, 0, 0.5),
            new Vec3(0.3, 0.3, 0.3)
        };
        foreach (var p in points)
        {
            var result = RayCastClassifier.Classify(p, sphere.Bvh);
            Assert.Equal(SolidClassification.Inside, result);
        }
    }

    [Fact]
    public void RayCast_MultiplePointsOutsideSphere_AllOutside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var points = new[] {
            new Vec3(5, 0, 0),
            new Vec3(0, 5, 0),
            new Vec3(0, 0, 5),
            new Vec3(-5, 0, 0),
            new Vec3(3, 3, 3)
        };
        foreach (var p in points)
        {
            var result = RayCastClassifier.Classify(p, sphere.Bvh);
            Assert.Equal(SolidClassification.Outside, result);
        }
    }

    [Fact]
    public void Winding_ComputeWindingNumber_InsideCube_NearOne()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(1.0, 1.0, 1.0), cube.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1,
            $"Winding number inside closed cube should be near 1.0, got {wn}");
    }

    [Fact]
    public void Winding_ComputeWindingNumber_OutsideCube_NearZero()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(10, 10, 10), cube.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.1,
            $"Winding number outside closed cube should be near 0, got {wn}");
    }

    [Fact]
    public void Winding_InsideSphere_NearOne()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.1, 0.1, 0.1), sphere.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.15,
            $"Winding number inside sphere should be near 1.0, got {wn}");
    }

    [Fact]
    public void Winding_EmptyMesh_Zero()
    {
        var empty = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, empty.Bvh);
        Assert.Equal(0.0, wn);
    }

    [Fact]
    public void RayCast_Tetrahedron_CentroidInside()
    {
        var tet = MeshFactory.CreateTetrahedron();
        // Centroid of regular tetrahedron with vertices at known positions should be inside
        // Need to find the actual centroid from the mesh
        var verts = tet.Mesh.Vertices;
        var centroid = Vec3.Zero;
        foreach (var v in verts)
            centroid = centroid + v.Position;
        centroid = centroid / verts.Count;
        var result = RayCastClassifier.Classify(centroid, tet.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_Deterministic()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(0.7, 0.8, 0.9);
        var r1 = RayCastClassifier.Classify(point, cube.Bvh);
        var r2 = RayCastClassifier.Classify(point, cube.Bvh);
        Assert.Equal(r1, r2);
    }

    [Fact]
    public void Winding_Deterministic()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(0.7, 0.8, 0.9);
        var r1 = WindingNumberClassifier.Classify(point, cube.Bvh);
        var r2 = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(r1, r2);
    }

    [Fact]
    public void SolidClassification_Enum_TwoValues()
    {
        var values = Enum.GetValues(typeof(SolidClassification));
        Assert.Equal(2, values.Length);
    }

    [Fact]
    public void PatchClassifier_DegenerateMarginThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-5);
        Assert.True(PatchClassifier.DegenerateMarginThreshold > 0);
    }
}
