using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

public class CsgHalfSpaceTests
{
    // ── Intersect(Solid, HalfSpace) ─────────────────────────

    [Fact]
    public void Intersect_CubeWithHalfSpace_ProducesHalfCube()
    {
        var cube = MeshFactory.CreateCube();
        // Half-space: z > 0.5 (keep upper half)
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Intersect(cube, hs);

        Assert.True(result.FaceCount > 0, "Result should have faces.");
        Assert.True(result.VertexCount > 0, "Result should have vertices.");
    }

    [Fact]
    public void Intersect_CubeWithHalfSpace_ResultHasCorrectBounds()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Intersect(cube, hs);

        // All vertices should have z >= 0.5 (approximately)
        foreach (var vert in result.Mesh.Vertices)
        {
            Assert.True(vert.Position.Z >= 0.5 - 1e-6,
                $"Vertex at z={vert.Position.Z} should be >= 0.5");
        }
    }

    [Fact]
    public void Intersect_CubeWithHalfSpace_ApproximateVolume()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Intersect(cube, hs);

        double volume = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.Equal(0.5, volume, 0.1); // Half of unit cube
    }

    [Fact]
    public void Intersect_HalfSpaceAndCube_Commutative()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result1 = Csg.Intersect(cube, hs);
        var result2 = Csg.Intersect(hs, cube);

        // Both should produce the same face count
        Assert.Equal(result1.FaceCount, result2.FaceCount);
    }

    // ── Difference(Solid, HalfSpace) ────────────────────────

    [Fact]
    public void Difference_CubeMinusHalfSpace_ProducesOtherHalf()
    {
        var cube = MeshFactory.CreateCube();
        // Half-space: z > 0.5
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Difference(cube, hs);

        Assert.True(result.FaceCount > 0, "Result should have faces.");

        // All vertices should have z <= 0.5 (approximately)
        foreach (var vert in result.Mesh.Vertices)
        {
            Assert.True(vert.Position.Z <= 0.5 + 1e-6,
                $"Vertex at z={vert.Position.Z} should be <= 0.5");
        }
    }

    [Fact]
    public void Difference_CubeMinusHalfSpace_ApproximateVolume()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Difference(cube, hs);

        double volume = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.Equal(0.5, volume, 0.1);
    }

    // ── Operations that should throw ────────────────────────

    [Fact]
    public void Union_MeshAndHalfSpace_Throws()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);

        Assert.Throws<NotSupportedException>(() => Csg.Union(cube, hs));
    }

    [Fact]
    public void Union_HalfSpaceAndMesh_Throws()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);

        Assert.Throws<NotSupportedException>(() => Csg.Union(hs, cube));
    }

    [Fact]
    public void Difference_HalfSpaceMinusMesh_Throws()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);

        Assert.Throws<NotSupportedException>(() => Csg.Difference(hs, cube));
    }

    [Fact]
    public void Union_HalfSpaceHalfSpace_Throws()
    {
        var hs1 = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var hs2 = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitX);

        Assert.Throws<NotSupportedException>(() => Csg.Union(hs1, hs2));
    }

    [Fact]
    public void Intersect_HalfSpaceHalfSpace_Throws()
    {
        var hs1 = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var hs2 = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitX);

        Assert.Throws<NotSupportedException>(() => Csg.Intersect(hs1, hs2));
    }

    [Fact]
    public void Difference_HalfSpaceHalfSpace_Throws()
    {
        var hs1 = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var hs2 = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitX);

        Assert.Throws<NotSupportedException>(() => Csg.Difference(hs1, hs2));
    }

    // ── Mesh quality ────────────────────────────────────────

    [Fact]
    public void Intersect_Result_PassesMeshValidation()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Intersect(cube, hs);

        MeshAssertions.AssertClosedManifold(result.Mesh);
    }

    [Fact]
    public void Difference_Result_PassesMeshValidation()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Difference(cube, hs);

        MeshAssertions.AssertClosedManifold(result.Mesh);
    }

    // ── Different shapes ────────────────────────────────────

    [Fact]
    public void Intersect_SphereWithHalfSpace_ProducesValidMesh()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);

        var result = Csg.Intersect(sphere, hs);

        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_TetrahedronWithHalfSpace_ProducesValidMesh()
    {
        var tet = MeshFactory.CreateTetrahedron(Vec3.Zero, 1.0);
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);

        var result = Csg.Intersect(tet, hs);

        Assert.True(result.FaceCount > 0);
    }

    // ── Chained operations ──────────────────────────────────

    [Fact]
    public void ChainedIntersectThenDifference()
    {
        var cube = MeshFactory.CreateCube();
        var hs1 = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);
        var hs2 = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.75), Vec3.UnitZ);

        // First clip: keep z > 0.5
        var result1 = Csg.Intersect(cube, hs1);
        // Second clip: remove z > 0.75
        var solid2 = new Solid(result1.Mesh);
        var result2 = Csg.Difference(solid2, hs2);

        Assert.True(result2.FaceCount > 0);
        // Volume should be approximately 1.0 * 1.0 * 0.25 = 0.25
        double volume = VolumeCalculator.ComputeAbsoluteVolume(result2.Mesh);
        Assert.Equal(0.25, volume, 0.1);
    }

    [Fact]
    public void HalfSpaceResultFedIntoMeshMeshCsg()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var halfCubeResult = Csg.Intersect(cube, hs);
        var halfCubeSolid = new Solid(halfCubeResult.Mesh);

        // Now do mesh-mesh CSG with the half-cube result
        var otherCube = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Intersect(halfCubeSolid, otherCube);

        Assert.True(result.FaceCount > 0);
    }

    // ── Edge cases ──────────────────────────────────────────

    [Fact]
    public void Intersect_PlaneMissesCubeEntirely_EmptyResult()
    {
        var cube = MeshFactory.CreateCube();
        // Plane at z=5, well above cube [0,1]^3, normal pointing up
        // The cube is entirely outside the half-space z > 5
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 5), Vec3.UnitZ);

        var result = Csg.Intersect(cube, hs);

        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Intersect_PlaneFullyBelowCube_KeepsAll()
    {
        var cube = MeshFactory.CreateCube();
        // Half-space z > -5 contains the entire cube
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -5), Vec3.UnitZ);

        var result = Csg.Intersect(cube, hs);

        // The entire cube is inside the half-space → full cube preserved
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_CubeWithXHalfSpace()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0.5, 0, 0), Vec3.UnitX);

        var result = Csg.Intersect(cube, hs);

        Assert.True(result.FaceCount > 0);
        // All vertices should have x >= 0.5 (approximately)
        foreach (var vert in result.Mesh.Vertices)
        {
            Assert.True(vert.Position.X >= 0.5 - 1e-6,
                $"Vertex at x={vert.Position.X} should be >= 0.5");
        }
    }

    [Fact]
    public void Result_HasPatchMetadata()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Intersect(cube, hs);

        Assert.True(result.PatchCountA > 0);
        Assert.Equal(0, result.PatchCountB); // HalfSpace contributes no patches
    }

    [Fact]
    public void Result_HasIntersectionSegments()
    {
        var cube = MeshFactory.CreateCube();
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.5), Vec3.UnitZ);

        var result = Csg.Intersect(cube, hs);

        Assert.True(result.IntersectionSegmentCount > 0);
    }
}
