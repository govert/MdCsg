using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

public class CubeSphereTests
{
    [Fact]
    public void CubeSphere_Intersection_ProducesResult()
    {
        var cube = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 0.8, 1);

        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0, "Intersection should produce faces.");
    }

    [Fact]
    public void CubeSphere_Difference_ProducesResult()
    {
        var cube = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 0.4, 1);

        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0, "Difference should produce faces.");
    }
}
