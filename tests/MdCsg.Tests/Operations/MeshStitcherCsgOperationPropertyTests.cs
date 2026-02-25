using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: MeshStitcher — stitch triangles into mesh; CsgOperation — enum values</summary>
public class MeshStitcherCsgOperationPropertyTests
{
    [Fact]
    public void Stitch_SingleTriangle_ProducesOneFace()
    {
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Stitch_TwoAdjacentTriangles_WeldsVertices()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0))
        };
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(4, mesh.Vertices.Count); // shared edge welded
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_TwoAdjacentTriangles_LinksTwins()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0))
        };
        var mesh = MeshStitcher.Stitch(tris);
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        Assert.True(twinCount > 0, "Shared edge should have twins");
    }

    [Fact]
    public void Stitch_EmptyInput_EmptyMesh()
    {
        var mesh = MeshStitcher.Stitch(Array.Empty<Triangle3>());
        Assert.Equal(0, mesh.Faces.Count);
        Assert.Equal(0, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_CustomWeldTolerance_LargeMergesMore()
    {
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1.001,0,0), new Vec3(1,1,0), new Vec3(0.001,1,0))
        };
        var tightMesh = MeshStitcher.Stitch(tris, weldTolerance: 1e-10);
        var looseMesh = MeshStitcher.Stitch(tris, weldTolerance: 0.01);
        Assert.True(looseMesh.Vertices.Count <= tightMesh.Vertices.Count,
            $"Loose ({looseMesh.Vertices.Count}) should have <= vertices than tight ({tightMesh.Vertices.Count})");
    }

    [Fact]
    public void Stitch_PreservesFaceCount()
    {
        var tris = new Triangle3[10];
        for (int i = 0; i < 10; i++)
        {
            tris[i] = new Triangle3(
                new Vec3(i * 10, 0, 0), new Vec3(i * 10 + 1, 0, 0), new Vec3(i * 10, 1, 0));
        }
        var mesh = MeshStitcher.Stitch(tris);
        Assert.Equal(10, mesh.Faces.Count);
    }

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
    public void CsgOperation_HasThreeValues()
    {
        var values = Enum.GetValues(typeof(CsgOperation));
        Assert.Equal(3, values.Length);
    }
}
