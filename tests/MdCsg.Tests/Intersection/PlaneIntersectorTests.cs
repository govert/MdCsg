using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

public class PlaneIntersectorTests
{
    [Fact]
    public void PlaneThroughCubeCenter_CutsFaces()
    {
        var cube = MeshFactory.CreateCube();
        // Plane at z=0.5 with normal +Z splits the cube horizontally
        var plane = new Plane(Vec3.UnitZ, 0.5);

        var faceSegments = PlaneIntersector.Compute(cube.Mesh, plane);

        // The plane cuts the 4 side faces (front, back, left, right)
        // Top and bottom faces are parallel to the plane
        Assert.True(faceSegments.Count > 0, "Plane should cut at least some faces.");
    }

    [Fact]
    public void PlaneMissingCube_NoSegments()
    {
        var cube = MeshFactory.CreateCube();
        // Plane at z=5, far above the cube [0,1]^3
        var plane = new Plane(Vec3.UnitZ, 5.0);

        var faceSegments = PlaneIntersector.Compute(cube.Mesh, plane);

        Assert.Empty(faceSegments);
    }

    [Fact]
    public void PlaneBelowCube_NoSegments()
    {
        var cube = MeshFactory.CreateCube();
        var plane = new Plane(Vec3.UnitZ, -5.0);

        var faceSegments = PlaneIntersector.Compute(cube.Mesh, plane);

        Assert.Empty(faceSegments);
    }

    [Fact]
    public void PlaneAtZHalf_SegmentsAreNonDegenerate()
    {
        var cube = MeshFactory.CreateCube();
        var plane = new Plane(Vec3.UnitZ, 0.5);

        var faceSegments = PlaneIntersector.Compute(cube.Mesh, plane);

        int totalSegments = 0;
        foreach (var kvp in faceSegments)
        {
            foreach (var seg in kvp.Value)
            {
                Assert.False(seg.IsDegenerate, "Segments should not be degenerate.");
                Assert.True(seg.Length > 0);
                totalSegments++;
            }
        }

        Assert.True(totalSegments > 0);
    }

    [Fact]
    public void SegmentEndpoints_AreSnapRounded()
    {
        var cube = MeshFactory.CreateCube();
        var plane = new Plane(Vec3.UnitZ, 0.5);
        double gridSize = 1e-8;

        var faceSegments = PlaneIntersector.Compute(cube.Mesh, plane, gridSize);

        foreach (var kvp in faceSegments)
        {
            foreach (var seg in kvp.Value)
            {
                // Start and End z should be approximately 0.5
                Assert.Equal(0.5, seg.Start.Z, 1e-6);
                Assert.Equal(0.5, seg.End.Z, 1e-6);
            }
        }
    }

    [Fact]
    public void DiagonalPlane_CutsMultipleFaces()
    {
        var cube = MeshFactory.CreateCube();
        // A diagonal plane through the cube
        var normal = new Vec3(1, 1, 0).Normalized;
        var plane = Plane.FromPointAndNormal(new Vec3(0.5, 0.5, 0.5), normal);

        var faceSegments = PlaneIntersector.Compute(cube.Mesh, plane);

        Assert.True(faceSegments.Count > 0);
    }

    [Fact]
    public void Sphere_PlaneIntersection_ProducesSegments()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var plane = new Plane(Vec3.UnitZ, 0);

        var faceSegments = PlaneIntersector.Compute(sphere.Mesh, plane);

        // A plane through the center of a sphere should cut many faces
        Assert.True(faceSegments.Count > 0);
    }

    [Fact]
    public void FaceIndexB_IsMinusOne()
    {
        var cube = MeshFactory.CreateCube();
        var plane = new Plane(Vec3.UnitZ, 0.5);

        var faceSegments = PlaneIntersector.Compute(cube.Mesh, plane);

        foreach (var kvp in faceSegments)
        {
            foreach (var seg in kvp.Value)
            {
                // PlaneIntersector sets FaceIndexB = -1 (no second mesh)
                Assert.Equal(-1, seg.FaceIndexB);
                // FaceIndexA should match the dictionary key
                Assert.Equal(kvp.Key, seg.FaceIndexA);
            }
        }
    }
}
