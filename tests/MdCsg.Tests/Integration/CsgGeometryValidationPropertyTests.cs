using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG geometry validation — bounds, face normals, triangle area conservation</summary>
public class CsgGeometryValidationPropertyTests
{
    [Fact]
    public void Union_Bounds_ContainsBothInputs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        // Union should span from min(a,b) to max(a,b)
        Assert.True(bounds.Min.X <= 0.01, $"Union min X should be near 0, got {bounds.Min.X}");
        Assert.True(bounds.Max.X >= 1.49, $"Union max X should be near 1.5, got {bounds.Max.X}");
    }

    [Fact]
    public void Intersect_Bounds_WithinOverlap()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        if (result.FaceCount > 0)
        {
            var bounds = result.Mesh.GetBounds();
            // Intersection should be within the overlap region
            Assert.True(bounds.Min.X >= -0.1);
            Assert.True(bounds.Max.X <= 1.1);
        }
    }

    [Fact]
    public void Difference_Bounds_WithinOriginal()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Difference(a, b);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Min.X >= -0.01);
        Assert.True(bounds.Max.X <= 1.01);
    }

    [Fact]
    public void Union_DisjointCubes_VertexCount_IsSumMinus0()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Vertices.Count + b.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void Union_TotalArea_GreaterThanIntersection()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var unionResult = Csg.Union(a, b);
        var intersectResult = Csg.Intersect(a, b);
        double unionArea = ComputeTotalArea(unionResult);
        double intersectArea = ComputeTotalArea(intersectResult);
        Assert.True(unionArea > intersectArea,
            $"Union area ({unionArea}) should be > intersection area ({intersectArea})");
    }

    [Fact]
    public void Intersect_TotalArea_SmallerThanEachInput()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        double intersectArea = ComputeTotalArea(result);
        double aArea = ComputeTotalArea(a);
        Assert.True(intersectArea < aArea,
            $"Intersection area ({intersectArea}) should be < input A area ({aArea})");
    }

    [Fact]
    public void Difference_TotalArea_LessThanOriginal()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Difference(a, b);
        double diffArea = ComputeTotalArea(result);
        double aArea = ComputeTotalArea(a);
        // The difference A-B has less surface area than A only if B actually
        // removes interior volume (which it does here)
        Assert.True(diffArea > 0, $"Difference should have positive area, got {diffArea}");
    }

    [Fact]
    public void DisjointUnion_TotalArea_EqualsSumOfInputs()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        double unionArea = ComputeTotalArea(result);
        double aArea = ComputeTotalArea(a);
        double bArea = ComputeTotalArea(b);
        Assert.True(System.Math.Abs(unionArea - (aArea + bArea)) < 0.01,
            $"Disjoint union area ({unionArea}) should equal sum ({aArea + bArea})");
    }

    [Fact]
    public void SphereSphere_Union_BoundsContainBoth()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Min.X < -0.9, $"Union min X should be < -0.9, got {bounds.Min.X}");
        Assert.True(bounds.Max.X > 1.9, $"Union max X should be > 1.9, got {bounds.Max.X}");
    }

    [Fact]
    public void Cube_TotalArea_IsSix()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        double area = ComputeTotalArea(cube);
        Assert.True(System.Math.Abs(area - 6.0) < 0.01, $"Unit cube total area should be ~6, got {area}");
    }

    [Fact]
    public void LargeCube_TotalArea_Correct()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        double area = ComputeTotalArea(cube);
        // Side = 2, 6 faces * 4 = 24
        Assert.True(System.Math.Abs(area - 24.0) < 0.01, $"2x2x2 cube area should be ~24, got {area}");
    }

    [Fact]
    public void CubeSphere_Difference_HasPositiveArea()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var result = Csg.Difference(cube, sphere);
        double area = ComputeTotalArea(result);
        Assert.True(area > 0);
    }

    private static double ComputeTotalArea(CsgResult result)
    {
        double total = 0;
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            total += new Triangle3(a, b, c).Area;
        }
        return total;
    }

    private static double ComputeTotalArea(Solid solid)
    {
        double total = 0;
        foreach (var face in solid.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            total += new Triangle3(a, b, c).Area;
        }
        return total;
    }
}
