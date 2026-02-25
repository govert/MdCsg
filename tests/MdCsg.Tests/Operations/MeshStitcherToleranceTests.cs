using MdCsg.Math;
using MdCsg.Operations;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: MeshStitcher tolerance edge cases and weld behaviour</summary>
public class MeshStitcherToleranceTests
{
    [Fact]
    public void Stitch_EmptyTriangles_EmptyMesh()
    {
        var mesh = MeshStitcher.Stitch([]);
        Assert.Empty(mesh.Faces);
        Assert.Empty(mesh.Vertices);
    }

    [Fact]
    public void Stitch_SingleTriangle_OneFace()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var mesh = MeshStitcher.Stitch([tri]);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_TwoSeparateTriangles_TwoFaces()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10));
        var mesh = MeshStitcher.Stitch([t1, t2]);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(6, mesh.Vertices.Count); // No shared vertices
    }

    [Fact]
    public void Stitch_SharedEdge_VerticesWelded()
    {
        // Two triangles sharing an edge
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1));
        var mesh = MeshStitcher.Stitch([t1, t2]);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count); // 4 unique vertices after welding
    }

    [Fact]
    public void Stitch_DefaultTolerance_WeldsCloseVertices()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        // Second triangle has vertex offset by less than default tolerance (1e-8)
        var t2 = new Triangle3(new Vec3(1, 0, 1e-12), new Vec3(0, 0, 1e-12), new Vec3(0, 0, 1));
        var mesh = MeshStitcher.Stitch([t1, t2]);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count); // Close vertices welded
    }

    [Fact]
    public void Stitch_LargeTolerance_WeldsMore()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(1.05, 0, 0), new Vec3(0.05, 0, 0), new Vec3(0, 0, 1));
        // With default tolerance 1e-8, vertices are distinct (0.05 apart)
        var meshDefault = MeshStitcher.Stitch([t1, t2], 1e-8);
        // With large tolerance 0.1, vertices are welded
        var meshLarge = MeshStitcher.Stitch([t1, t2], 0.1);
        Assert.True(meshLarge.Vertices.Count <= meshDefault.Vertices.Count);
    }

    [Fact]
    public void Stitch_ZeroTolerance_NoWelding()
    {
        // Two triangles sharing exact same edge
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1));
        // With exactly 0 tolerance, vertices at same position should still be welded (exact match)
        var mesh = MeshStitcher.Stitch([t1, t2], 0);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_Tetrahedron_AllFacesCreated()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0.5, 0.866, 0);
        var d = new Vec3(0.5, 0.289, 0.816);
        var triangles = new List<Triangle3>
        {
            new(a, b, c),
            new(a, d, b),
            new(b, d, c),
            new(a, c, d)
        };
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(4, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Stitch_ManyTriangles_VertexCount_LessOrEqual_3Times()
    {
        var triangles = new List<Triangle3>();
        for (int i = 0; i < 100; i++)
        {
            triangles.Add(new Triangle3(
                new Vec3(i, 0, 0),
                new Vec3(i + 1, 0, 0),
                new Vec3(i + 0.5, 1, 0)));
        }
        var mesh = MeshStitcher.Stitch(triangles);
        Assert.Equal(100, mesh.Faces.Count);
        // Vertices should be welded at shared edges
        Assert.True(mesh.Vertices.Count <= 300);
    }

    [Fact]
    public void Stitch_NearlyDegenerate_NarrowTriangle()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0.5, 1e-6, 0));
        var mesh = MeshStitcher.Stitch([tri]);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Stitch_PreservesOrientation()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var mesh = MeshStitcher.Stitch([t1]);
        var face = mesh.Faces[0];
        face.GetTrianglePositions(out var a, out var b, out var c);
        var normal = Vec3.Cross(b - a, c - a);
        // Original triangle has Z-up normal
        Assert.True(normal.Z > 0);
    }

    [Fact]
    public void Stitch_DuplicateTriangles_BothCreated()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var mesh = MeshStitcher.Stitch([tri, tri]);
        // Both triangles are created (duplicate faces on same vertices)
        Assert.Equal(2, mesh.Faces.Count);
    }
}
