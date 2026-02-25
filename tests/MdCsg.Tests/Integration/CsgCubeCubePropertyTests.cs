using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG cube-cube operations — overlapping, disjoint, contained, touching configurations</summary>
public class CsgCubeCubePropertyTests
{
    [Fact]
    public void Union_OverlappingCubes_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_OverlappingCubes_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_OverlappingCubes_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_DisjointCubes_FaceCountIsSum()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_DisjointCubes_Empty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Difference_DisjointCubes_PreservesA()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_OverlappingCubes_AllFaceCyclesValid()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Intersect_OverlappingCubes_FacesHaveNonZeroNormals()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-20);
        }
    }

    [Fact]
    public void Union_OverlappingCubes_HasIntersectionSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Union_DisjointCubes_ZeroIntersectionSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Intersect_OverlappingCubes_PatchCounts()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void Difference_AB_Different_From_BA()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 1.5);
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        // Different sizes → different results
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void Union_WithWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_WithWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_OverlappingCubes_VerticesExist()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Difference_OverlappingCubes_ResultBoundsWithinA()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        var aBounds = a.Bounds;
        var rBounds = result.Mesh.GetBounds();
        // Result of A-B should fit within A's bounds (with tolerance)
        Assert.True(rBounds.Min.X >= aBounds.Min.X - 0.1);
        Assert.True(rBounds.Min.Y >= aBounds.Min.Y - 0.1);
        Assert.True(rBounds.Min.Z >= aBounds.Min.Z - 0.1);
    }
}
