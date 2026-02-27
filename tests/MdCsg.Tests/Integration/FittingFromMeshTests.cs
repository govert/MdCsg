using MdCsg.Api;
using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 29: Extract verts from primitive → fit → compare.</summary>
public class FittingFromMeshTests
{
    private static Vec3[] GetVertices(Solid solid) =>
        solid.Mesh.Vertices.Select(v => v.Position).ToArray();

    [Fact]
    public void FitSphere_FromSphereVertices_ApproximatesRadius()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var pts = GetVertices(sphere);
        var fit = SphereFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Radius - 1.0) < 0.1);
    }

    [Fact]
    public void FitSphere_FromSphereVertices_ApproximatesCenter()
    {
        var sphere = Primitives.Sphere(new Vec3(1, 2, 3), 1.0, 3);
        var pts = GetVertices(sphere);
        var fit = SphereFitter.Fit(pts);
        Assert.True(Vec3.Distance(fit.Center, new Vec3(1, 2, 3)) < 0.1);
    }

    [Fact]
    public void FitSphere_LowRms()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var pts = GetVertices(sphere);
        var fit = SphereFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.1);
    }

    [Fact]
    public void FitPlane_FromCubeFace_LowRms()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        // Top face vertices (Z = 1)
        var topVerts = cube.Mesh.Vertices.Where(v => System.Math.Abs(v.Position.Z - 1.0) < 0.01)
            .Select(v => v.Position).ToArray();
        if (topVerts.Length >= 3)
        {
            var fit = PlaneFitter.Fit(topVerts);
            Assert.True(fit.RmsResidual < 0.01);
        }
    }

    [Fact]
    public void FitPlane_FromCubeFace_NormalApproximatelyZ()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var topVerts = cube.Mesh.Vertices.Where(v => System.Math.Abs(v.Position.Z - 1.0) < 0.01)
            .Select(v => v.Position).ToArray();
        if (topVerts.Length >= 3)
        {
            var fit = PlaneFitter.Fit(topVerts);
            Assert.True(System.Math.Abs(System.Math.Abs(fit.Normal.Z) - 1.0) < 0.1);
        }
    }

    [Fact]
    public void FitCylinder_FromCylinderVertices_ApproximatesRadius()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 3.0, 32);
        // Get side vertices (exclude caps)
        var sideVerts = cyl.Mesh.Vertices
            .Where(v => v.Position.Z > 0.01 && v.Position.Z < 2.99)
            .Select(v => v.Position).ToArray();
        if (sideVerts.Length >= 6)
        {
            var fit = CylinderFitter.Fit(sideVerts, axisHint: Vec3.UnitZ);
            Assert.True(System.Math.Abs(fit.Radius - 1.0) < 0.3);
        }
    }

    [Fact]
    public void MeshFitting_FitSphere_ProducesSolid()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var pts = GetVertices(sphere);
        var solid = MeshFitting.FitSphere(pts);
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void MeshFitting_FitSphere_VolumeApproximatesOriginal()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var pts = GetVertices(sphere);
        var fitted = MeshFitting.FitSphere(pts);
        Assert.True(System.Math.Abs(fitted.Volume() - sphere.Volume()) < 0.5);
    }

    [Fact]
    public void MeshFitting_FitPlane_ProducesPlane()
    {
        var pts = new[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(1, 1, 0), new Vec3(0.5, 0.5, 0),
        };
        var plane = MeshFitting.FitPlane(pts);
        Assert.True(System.Math.Abs(plane.Normal.Z) > 0.9);
    }

    [Fact]
    public void FitSphere_OffCenter_FindsCorrectCenter()
    {
        var center = new Vec3(5, -3, 2);
        var sphere = Primitives.Sphere(center, 2.0, 3);
        var pts = GetVertices(sphere);
        var fit = SphereFitter.Fit(pts);
        Assert.True(Vec3.Distance(fit.Center, center) < 0.2);
        Assert.True(System.Math.Abs(fit.Radius - 2.0) < 0.1);
    }

    [Fact]
    public void FitSphere_LargeRadius_Works()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 10.0, 3);
        var pts = GetVertices(sphere);
        var fit = SphereFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Radius - 10.0) < 0.5);
    }

    [Fact]
    public void FitSphere_SmallRadius_Works()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 0.1, 3);
        var pts = GetVertices(sphere);
        var fit = SphereFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Radius - 0.1) < 0.02);
    }

    [Fact]
    public void FitPlane_FromManyPoints_LowRms()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 100)
            .Select(_ => new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, 0))
            .ToArray();
        var fit = PlaneFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.001);
    }

    [Fact]
    public void MeshFitting_ToSolid_SphereFit_Works()
    {
        var fit = SphereFitter.Fit(GetVertices(Primitives.Sphere(Vec3.Zero, 1.0, 3)));
        var solid = MeshFitting.ToSolid(fit);
        Assert.True(solid.Volume() > 0);
    }

    [Fact]
    public void MeshFitting_ToSolid_CylinderFit_Works()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 3.0, 32);
        var pts = cyl.Mesh.Vertices
            .Where(v => v.Position.Z > 0.01 && v.Position.Z < 2.99)
            .Select(v => v.Position).ToArray();
        if (pts.Length >= 6)
        {
            var fit = CylinderFitter.Fit(pts, axisHint: Vec3.UnitZ);
            var solid = MeshFitting.ToSolid(fit);
            Assert.True(solid.Volume() > 0);
        }
    }

    [Fact]
    public void FittedSphere_CsgWithOriginal_ProducesValidMesh()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var pts = GetVertices(sphere);
        var fitted = MeshFitting.FitSphere(pts);
        var r = Csg.Intersect(sphere, fitted);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void FitPlane_TiltedPlane_FindsCorrectNormal()
    {
        var rng = new Random(42);
        var normal = new Vec3(1, 1, 1).Normalized;
        var pts = Enumerable.Range(0, 50)
            .Select(_ =>
            {
                var u = Vec3.Cross(normal, Vec3.UnitX).Normalized;
                var v = Vec3.Cross(normal, u);
                return u * (rng.NextDouble() * 10 - 5) + v * (rng.NextDouble() * 10 - 5);
            }).ToArray();
        var fit = PlaneFitter.Fit(pts);
        double dotAbs = System.Math.Abs(Vec3.Dot(fit.Normal, normal));
        Assert.True(dotAbs > 0.9);
    }

    [Fact]
    public void FitSphere_CsgResult_Works()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = new Solid(Csg.Intersect(a, b).Mesh);
        var pts = GetVertices(r);
        var fit = SphereFitter.Fit(pts);
        Assert.True(fit.Radius > 0);
    }
}
