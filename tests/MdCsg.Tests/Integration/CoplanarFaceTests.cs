using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

public class CoplanarFaceTests
{
    // =========================================================================
    // Identical cubes
    // =========================================================================

    [Fact]
    public void Union_IdenticalCubes_ProducesValidMesh()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube();
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0, "Union of identical cubes should produce faces");
    }

    [Fact]
    public void Intersection_IdenticalCubes_ProducesValidMesh()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube();
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0, "Intersection of identical cubes should produce faces");
    }

    [Fact]
    public void Difference_IdenticalCubes_ProducesEmptyOrSmall()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube();
        var result = Csg.Difference(a, b);
        // A \ A = empty (or near-empty due to degenerate handling)
        double vol = result.FaceCount > 0
            ? VolumeCalculator.ComputeAbsoluteVolume(result.Mesh)
            : 0;
        Assert.True(vol < 0.01, $"Difference of identical cubes should have near-zero volume, got {vol}");
    }

    // =========================================================================
    // Cubes sharing a face
    // =========================================================================

    [Fact]
    public void Union_CubesSharingFace_ProducesValidMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0)); // shares X=1 face
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0, "Union of face-sharing cubes should produce faces");
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.5, $"Union volume should be ~2.0, got {vol}");
    }

    [Fact]
    public void Intersection_CubesSharingFace_ProducesEmptyOrSmall()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0)); // shares X=1 face, no volume overlap
        var result = Csg.Intersect(a, b);
        // No volume overlap — intersection has zero or near-zero volume
        double vol = result.FaceCount > 0
            ? VolumeCalculator.ComputeAbsoluteVolume(result.Mesh)
            : 0;
        Assert.True(vol < 0.01, $"Intersection of face-sharing cubes should have near-zero volume, got {vol}");
    }

    // =========================================================================
    // Cubes sharing an edge (partial overlap)
    // =========================================================================

    [Fact]
    public void Union_CubesSharingEdge_ProducesValidMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(1, 1, 0)); // shares edge along Z at (1,1,z)
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0, "Union of edge-sharing cubes should produce faces");
    }

    // =========================================================================
    // Cube with face-aligned HalfSpace
    // =========================================================================

    [Fact]
    public void Intersect_CubeWithAlignedHalfSpace_ProducesHalfVolume()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2); // [0,2]^3
        var halfSpace = HalfSpace.FromPointAndNormal(new Vec3(1, 0, 0), new Vec3(1, 0, 0)); // X > 1
        var result = Csg.Intersect(cube, halfSpace);
        Assert.True(result.FaceCount > 0, "Clipping cube by aligned halfspace should produce faces");
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 3.0 && vol < 5.0,
            $"Half of 2x2x2 cube should have volume ~4.0, got {vol}");
    }

    [Fact]
    public void Difference_CubeWithAlignedHalfSpace_ProducesHalfVolume()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2);
        var halfSpace = HalfSpace.FromPointAndNormal(new Vec3(1, 0, 0), new Vec3(1, 0, 0));
        var result = Csg.Difference(cube, halfSpace);
        Assert.True(result.FaceCount > 0, "Subtracting aligned halfspace should produce faces");
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 3.0 && vol < 5.0,
            $"Half of 2x2x2 cube should have volume ~4.0, got {vol}");
    }

    // =========================================================================
    // Tetrahedra sharing a face
    // =========================================================================

    [Fact]
    public void Union_TetrahedraSharingFace_ProducesValidMesh()
    {
        var a = MeshFactory.CreateTetrahedron();
        var b = MeshFactory.CreateTetrahedron(); // identical
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0, "Union of identical tetrahedra should produce faces");
    }

    [Fact]
    public void Intersection_TetrahedraSharingFace_ProducesValidMesh()
    {
        var a = MeshFactory.CreateTetrahedron();
        var b = MeshFactory.CreateTetrahedron(); // identical
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0, "Intersection of identical tetrahedra should produce faces");
    }

    // =========================================================================
    // Overlapping cubes with one coplanar face
    // =========================================================================

    [Fact]
    public void Union_OverlappingCubesWithCoplanarFace_CorrectVolume()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);          // [0,1]^3
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)); // [0.5,1.5] x [0,1]^2
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.3 && vol < 1.7, $"Union volume should be ~1.5, got {vol}");
    }

    [Fact]
    public void Intersection_OverlappingCubesWithCoplanarFace_CorrectVolume()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0.3 && vol < 0.7, $"Intersection volume should be ~0.5, got {vol}");
    }
}
