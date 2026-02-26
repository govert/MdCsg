using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Api;

public class ExtrudeTests
{
    // ── Square profile ───────────────────────────────────────────────

    [Fact]
    public void Extrude_Square_CreatesClosedMesh()
    {
        var profile = UnitSquare();
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 2.0);
        Assert.True(solid.Mesh.Faces.Count > 0);
        Assert.True(IsClosedManifold(solid.Mesh));
    }

    [Fact]
    public void Extrude_Square_VolumeMatchesExpected()
    {
        // 1x1 square extruded 2 units along Z => volume = 1 * 2 = 2
        var profile = UnitSquare();
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 2.0);
        double volume = solid.Volume();
        Assert.True(System.Math.Abs(volume - 2.0) < 0.1, $"Expected volume ~2, got {volume}");
    }

    [Fact]
    public void Extrude_Square_FaceCount()
    {
        // 4-sided profile: 4 side quads (8 tris) + 2 caps (2 tris each) = 12 triangles
        var profile = UnitSquare();
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 1.0);
        Assert.Equal(12, solid.Mesh.Faces.Count);
    }

    // ── Triangle profile ─────────────────────────────────────────────

    [Fact]
    public void Extrude_Triangle_VolumeMatchesExpected()
    {
        // Right triangle with legs 2 and 3, area = 3; extruded 4 units => volume = 12
        var profile = new Vec2[] { new(0, 0), new(2, 0), new(0, 3) };
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 4.0);
        double volume = solid.Volume();
        Assert.True(System.Math.Abs(volume - 12.0) < 0.5, $"Expected volume ~12, got {volume}");
    }

    [Fact]
    public void Extrude_Triangle_FaceCount()
    {
        // 3-sided: 3 side quads (6 tris) + 2 caps (1 tri each) = 8
        var profile = new Vec2[] { new(0, 0), new(1, 0), new(0, 1) };
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 1.0);
        Assert.Equal(8, solid.Mesh.Faces.Count);
    }

    // ── L-shape profile ──────────────────────────────────────────────

    [Fact]
    public void Extrude_LShape_CreatesClosedMesh()
    {
        var profile = LShapeProfile();
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 1.0);
        Assert.True(solid.Mesh.Faces.Count > 0);
        Assert.True(IsClosedManifold(solid.Mesh));
    }

    [Fact]
    public void Extrude_LShape_VolumeMatchesExpected()
    {
        // L-shape: total area = 3*3 - 2*2 = 5, extruded 1 unit => volume = 5
        var profile = LShapeProfile();
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 1.0);
        double volume = solid.Volume();
        Assert.True(System.Math.Abs(volume - 5.0) < 0.5, $"Expected volume ~5, got {volume}");
    }

    // ── Direction variations ─────────────────────────────────────────

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    [InlineData(1, 1, 1)]
    public void Extrude_VariousDirections_ClosedMesh(double dx, double dy, double dz)
    {
        var profile = UnitSquare();
        var dir = new Vec3(dx, dy, dz);
        var solid = Primitives.Extrude(profile, dir, 2.0);
        Assert.True(IsClosedManifold(solid.Mesh));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(5.0)]
    public void Extrude_VariousHeights_VolumeScalesLinearly(double height)
    {
        var profile = UnitSquare();
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, height);
        double volume = solid.Volume();
        // Unit square area = 1, so volume = height
        Assert.True(System.Math.Abs(volume - height) < 0.1, $"Expected volume ~{height}, got {volume}");
    }

    // ── Edge cases ───────────────────────────────────────────────────

    [Fact]
    public void Extrude_TooFewPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Primitives.Extrude(new Vec2[] { new(0, 0), new(1, 0) }, Vec3.UnitZ, 1.0));
    }

    [Fact]
    public void Extrude_Pentagon_ClosedMesh()
    {
        var profile = RegularPolygon(5, 1.0);
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 2.0);
        Assert.True(IsClosedManifold(solid.Mesh));
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void Extrude_Hexagon_ClosedMesh()
    {
        var profile = RegularPolygon(6, 1.0);
        var solid = Primitives.Extrude(profile, Vec3.UnitZ, 1.0);
        Assert.True(IsClosedManifold(solid.Mesh));
    }

    // ── Ear clipping tests ───────────────────────────────────────────

    [Fact]
    public void EarClip_Triangle_SingleTriangle()
    {
        var poly = new Vec2[] { new(0, 0), new(1, 0), new(0, 1) };
        var tris = Primitives.EarClipTriangulate(poly);
        Assert.Single(tris);
    }

    [Fact]
    public void EarClip_Square_TwoTriangles()
    {
        var tris = Primitives.EarClipTriangulate(UnitSquare());
        Assert.Equal(2, tris.Count);
    }

    [Fact]
    public void EarClip_Pentagon_ThreeTriangles()
    {
        var poly = RegularPolygon(5, 1.0);
        var tris = Primitives.EarClipTriangulate(poly);
        Assert.Equal(3, tris.Count);
    }

    [Fact]
    public void EarClip_LShape_CorrectCount()
    {
        // L-shape has 6 vertices → 4 triangles
        var tris = Primitives.EarClipTriangulate(LShapeProfile());
        Assert.Equal(4, tris.Count);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static Vec2[] UnitSquare() =>
        new Vec2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };

    private static Vec2[] LShapeProfile() =>
        new Vec2[] { new(0, 0), new(3, 0), new(3, 1), new(1, 1), new(1, 3), new(0, 3) };

    private static Vec2[] RegularPolygon(int sides, double radius)
    {
        var verts = new Vec2[sides];
        for (int i = 0; i < sides; i++)
        {
            double angle = 2.0 * System.Math.PI * i / sides;
            verts[i] = new Vec2(System.Math.Cos(angle) * radius, System.Math.Sin(angle) * radius);
        }
        return verts;
    }

    private static bool IsClosedManifold(HalfEdgeMesh mesh)
    {
        // Every half-edge should have a twin (closed manifold)
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null) return false;
        }
        return true;
    }
}
