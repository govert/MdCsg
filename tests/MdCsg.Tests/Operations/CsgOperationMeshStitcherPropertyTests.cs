using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: CsgOperation enum, MeshStitcher — enum values, Stitch properties, tolerance</summary>
public class CsgOperationMeshStitcherPropertyTests
{
    // --- CsgOperation enum ---

    [Fact]
    public void CsgOperation_Union_Value()
    {
        Assert.Equal(0, (int)CsgOperation.Union);
    }

    [Fact]
    public void CsgOperation_Intersection_Value()
    {
        Assert.Equal(1, (int)CsgOperation.Intersection);
    }

    [Fact]
    public void CsgOperation_Difference_Value()
    {
        Assert.Equal(2, (int)CsgOperation.Difference);
    }

    [Fact]
    public void CsgOperation_AllValues_Distinct()
    {
        var values = Enum.GetValues(typeof(CsgOperation));
        Assert.Equal(3, values.Length);
    }

    // --- MeshStitcher ---

    [Fact]
    public void Stitch_SingleTriangle_OneeFace()
    {
        var triangles = new Triangle3[]
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY)
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_SingleTriangle_ThreeVertices()
    {
        var triangles = new Triangle3[]
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY)
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_TwoTriangles_SharedEdge_Welded()
    {
        var triangles = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_EmptyTriangles_EmptyMesh()
    {
        var mesh = MeshStitcher.Stitch(Array.Empty<Triangle3>());
        Assert.Equal(0, mesh.Faces.Count);
        Assert.Equal(0, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_WithDefaultTolerance_Works()
    {
        var triangles = new Triangle3[]
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY)
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_WithCustomTolerance_Works()
    {
        var triangles = new Triangle3[]
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY)
        };
        var mesh = MeshStitcher.Stitch(triangles, 1e-6);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_NearbyVertices_Welded()
    {
        var triangles = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1.0000000001, 0, 0), new Vec3(1, 1, 0), new Vec3(0.0000000001, 1, 0)),
        };
        var mesh = MeshStitcher.Stitch(triangles, 1e-8);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_FarApartVertices_NotWelded()
    {
        var triangles = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0)),
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(6, mesh.Vertices.Count); // No welding
    }

    [Fact]
    public void Stitch_ManyTriangles_CorrectFaceCount()
    {
        var triangles = new Triangle3[10];
        for (int i = 0; i < 10; i++)
        {
            double x = i * 5.0;
            triangles[i] = new Triangle3(
                new Vec3(x, 0, 0),
                new Vec3(x + 1, 0, 0),
                new Vec3(x, 1, 0));
        }
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(10, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_ResultHasHalfEdges()
    {
        var triangles = new Triangle3[]
        {
            new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY)
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Stitch_ClosedBox_AllTwinsLinked()
    {
        // Build a tetrahedron (closed mesh)
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0.5, 0.866, 0);
        var v3 = new Vec3(0.5, 0.289, 0.816);
        var triangles = new Triangle3[]
        {
            new(v0, v2, v1),
            new(v0, v1, v3),
            new(v1, v2, v3),
            new(v2, v0, v3),
        };
        var mesh = MeshStitcher.Stitch(triangles);
        int unlinked = mesh.HalfEdges.Count(he => he.Twin == null);
        Assert.Equal(0, unlinked);
    }
}
