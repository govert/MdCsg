using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Api;

public class RevolveTests
{
    // ── Cylinder from rectangle ──────────────────────────────────────

    [Fact]
    public void Revolve_Rectangle_CreatesCylinderShape()
    {
        // Rectangle profile: vertical line at radius 1, from y=0 to y=2
        // Revolving creates a cylinder-like surface (open top/bottom if no axis points)
        var profile = new Vec2[] { new(0, 0), new(1, 0), new(1, 2), new(0, 2) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 24);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Revolve_Rectangle_VolumeNearCylinder()
    {
        // Revolve rectangle [0,0]-[1,0]-[1,2]-[0,2] around Z
        // This creates a cylinder with r=1, h=2, V = π*r²*h = 2π ≈ 6.28
        var profile = new Vec2[] { new(0, 0), new(1, 0), new(1, 2), new(0, 2) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 48);
        double volume = solid.Volume();
        double expected = 2 * System.Math.PI; // ~6.28
        Assert.True(System.Math.Abs(volume - expected) < 0.5,
            $"Expected volume ~{expected:F2}, got {volume:F2}");
    }

    // ── Sphere from semicircle ───────────────────────────────────────

    [Fact]
    public void Revolve_Semicircle_VolumeNearSphere()
    {
        // Semicircle profile: axis points at (0,r) and (0,-r), with arc at radius r
        double r = 1.0;
        int profilePoints = 20;
        var profile = new Vec2[profilePoints];
        // From south pole to north pole
        for (int i = 0; i < profilePoints; i++)
        {
            double t = System.Math.PI * i / (profilePoints - 1); // 0 to π
            profile[i] = new Vec2(r * System.Math.Sin(t), -r * System.Math.Cos(t));
        }

        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 48);
        double volume = solid.Volume();
        double expected = 4.0 / 3.0 * System.Math.PI * r * r * r; // ~4.19
        Assert.True(System.Math.Abs(volume - expected) < 0.5,
            $"Expected volume ~{expected:F2}, got {volume:F2}");
    }

    // ── Various segment counts ───────────────────────────────────────

    [Theory]
    [InlineData(6)]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(48)]
    public void Revolve_VariousSegments_HasFaces(int segments)
    {
        var profile = new Vec2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, segments);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(48)]
    public void Revolve_MoreSegments_CloserToAnalytical(int segments)
    {
        var profile = new Vec2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, segments);
        double volume = solid.Volume();
        double expected = System.Math.PI; // π*1²*1
        // Higher segments should be closer to analytical
        double tolerance = 10.0 / segments; // roughly
        Assert.True(System.Math.Abs(volume - expected) < tolerance,
            $"segments={segments}: Expected volume ~{expected:F2}, got {volume:F2}");
    }

    // ── Cone from triangle profile ───────────────────────────────────

    [Fact]
    public void Revolve_Triangle_VolumeNearCone()
    {
        // Triangle from origin to (1,0) to (0,2): revolve creates a cone
        // Cone volume = π*r²*h/3 = π*1*2/3 ≈ 2.094
        var profile = new Vec2[] { new(0, 0), new(1, 0), new(0, 2) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 48);
        double volume = solid.Volume();
        double expected = System.Math.PI * 1 * 1 * 2 / 3.0; // ~2.094
        Assert.True(System.Math.Abs(volume - expected) < 0.3,
            $"Expected volume ~{expected:F2}, got {volume:F2}");
    }

    // ── Torus-like from offset circle ────────────────────────────────

    [Fact]
    public void Revolve_OffsetCircle_VolumeNearTorus()
    {
        // Circle of radius 0.5 centered at (2, 0): torus with R=2, r=0.5
        // Torus volume = 2*π²*R*r² = 2*π²*2*0.25 ≈ 9.87
        double R = 2.0, r = 0.5;
        int n = 20;
        var profile = new Vec2[n + 1]; // closed loop — first == last
        for (int i = 0; i <= n; i++)
        {
            double t = 2.0 * System.Math.PI * i / n;
            profile[i] = new Vec2(R + r * System.Math.Cos(t), r * System.Math.Sin(t));
        }

        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 48);
        double volume = solid.Volume();
        double expected = 2 * System.Math.PI * System.Math.PI * R * r * r; // ~9.87
        Assert.True(System.Math.Abs(volume - expected) < 1.0,
            $"Expected volume ~{expected:F2}, got {volume:F2}");
    }

    // ── Different axes ───────────────────────────────────────────────

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    public void Revolve_VariousAxes_HasFaces(double ax, double ay, double az)
    {
        var profile = new Vec2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
        var solid = Primitives.Revolve(profile, new Vec3(ax, ay, az), 12);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    // ── Edge cases ───────────────────────────────────────────────────

    [Fact]
    public void Revolve_TooFewPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Primitives.Revolve(new Vec2[] { new(1, 0) }, Vec3.UnitZ));
    }

    [Fact]
    public void Revolve_TooFewSegments_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Primitives.Revolve(new Vec2[] { new(0, 0), new(1, 0) }, Vec3.UnitZ, 2));
    }

    [Fact]
    public void Revolve_TwoPointLine_CreatesSurface()
    {
        // Simple line segment: (1, 0) to (1, 2) — cylinder wall without caps
        var profile = new Vec2[] { new(1, 0), new(1, 2) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 12);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Revolve_AllOnAxis_NoFaces()
    {
        // All points on axis (X=0): degenerate, should produce no faces
        var profile = new Vec2[] { new(0, 0), new(0, 1), new(0, 2) };
        var solid = Primitives.Revolve(profile, Vec3.UnitZ, 12);
        Assert.Equal(0, solid.Mesh.Faces.Count);
    }
}
