using MdCsg.Api;
using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Fitting;

public class MeshFittingTests
{
    // =========================================================================
    // ToSolid(FittedSphere)
    // =========================================================================

    [Fact]
    public void ToSolid_Sphere_CreatesValidMesh()
    {
        var fitted = new FittedSphere(Vec3.Zero, 1.0, 0.0);
        var solid = MeshFitting.ToSolid(fitted);
        Assert.True(solid.Mesh.Faces.Count > 0);
        Assert.True(solid.Mesh.Vertices.Count > 0);
    }

    [Theory]
    [InlineData(0, 12, 20)]   // icosahedron: 12 verts, 20 faces
    [InlineData(1, 42, 80)]   // 1 subdiv
    [InlineData(2, 162, 320)] // 2 subdiv
    public void ToSolid_Sphere_CorrectVertexAndFaceCount(int subdivisions, int expectedVerts, int expectedFaces)
    {
        var fitted = new FittedSphere(Vec3.Zero, 1.0, 0.0);
        var solid = MeshFitting.ToSolid(fitted, subdivisions);
        Assert.Equal(expectedVerts, solid.Mesh.Vertices.Count);
        Assert.Equal(expectedFaces, solid.Mesh.Faces.Count);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(5.0)]
    [InlineData(100.0)]
    public void ToSolid_Sphere_VerticesAtExpectedRadius(double radius)
    {
        var center = new Vec3(1, 2, 3);
        var fitted = new FittedSphere(center, radius, 0.0);
        var solid = MeshFitting.ToSolid(fitted, 2);

        for (int i = 0; i < solid.Mesh.Vertices.Count; i++)
        {
            var pos = solid.Mesh.Vertices[i].Position;
            double dist = Vec3.Distance(pos, center);
            Assert.True(System.Math.Abs(dist - radius) < radius * 0.01,
                $"Vertex {i} at distance {dist} from center, expected ~{radius}");
        }
    }

    [Fact]
    public void ToSolid_Sphere_OffCenter_CorrectPosition()
    {
        var center = new Vec3(10, -5, 3);
        var fitted = new FittedSphere(center, 2.0, 0.0);
        var solid = MeshFitting.ToSolid(fitted);

        var sum = Vec3.Zero;
        for (int i = 0; i < solid.Mesh.Vertices.Count; i++)
            sum += solid.Mesh.Vertices[i].Position;
        var centroid = sum * (1.0 / solid.Mesh.Vertices.Count);

        Assert.True(Vec3.Distance(centroid, center) < 0.1,
            $"Centroid {centroid} should be near {center}");
    }

    // =========================================================================
    // ToSolid(FittedCylinder)
    // =========================================================================

    [Fact]
    public void ToSolid_Cylinder_CreatesValidMesh()
    {
        var fitted = new FittedCylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 5.0, 0.0);
        var solid = MeshFitting.ToSolid(fitted);
        Assert.True(solid.Mesh.Faces.Count > 0);
        Assert.True(solid.Mesh.Vertices.Count > 0);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void ToSolid_Cylinder_FaceCountScalesWithSegments(int segments)
    {
        var fitted = new FittedCylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 5.0, 0.0);
        var solid = MeshFitting.ToSolid(fitted, segments);
        // Each segment produces: 1 bottom cap tri, 1 top cap tri, 2 side tris = 4 tris
        Assert.Equal(segments * 4, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void ToSolid_Cylinder_ArbitraryAxis_CreatesValidMesh()
    {
        var axis = new Vec3(1, 1, 1).Normalized;
        var fitted = new FittedCylinder(new Vec3(1, 2, 3), axis, 0.5, 3.0, 0.0);
        var solid = MeshFitting.ToSolid(fitted);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // ToPlane(FittedPlane)
    // =========================================================================

    [Fact]
    public void ToPlane_CorrectNormal()
    {
        var fitted = new FittedPlane(new Vec3(0, 0, 5), Vec3.UnitZ, 0.0);
        var plane = MeshFitting.ToPlane(fitted);

        Assert.True(System.Math.Abs(plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.Y) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.Z - 1.0) < 1e-10);
    }

    [Fact]
    public void ToPlane_PointOnPlane_ZeroDistance()
    {
        var centroid = new Vec3(1, 2, 3);
        var fitted = new FittedPlane(centroid, Vec3.UnitZ, 0.0);
        var plane = MeshFitting.ToPlane(fitted);

        double signedDist = plane.SignedDistanceTo(centroid);
        Assert.True(System.Math.Abs(signedDist) < 1e-10,
            $"Centroid should be on the plane, signed distance = {signedDist}");
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    public void ToPlane_AxisAligned_CorrectDistance(double nx, double ny, double nz)
    {
        var normal = new Vec3(nx, ny, nz);
        var centroid = normal * 5.0;
        var fitted = new FittedPlane(centroid, normal, 0.0);
        var plane = MeshFitting.ToPlane(fitted);

        Assert.True(System.Math.Abs(plane.Distance - 5.0) < 1e-10,
            $"Plane distance should be 5.0, got {plane.Distance}");
    }

    // =========================================================================
    // Full pipeline: FitSphere
    // =========================================================================

    [Fact]
    public void FitSphere_FromPointCloud_CreatesCorrectSolid()
    {
        var center = new Vec3(1, 2, 3);
        double radius = 2.0;
        var points = GenerateSpherePoints(center, radius, 100, seed: 42);

        var solid = MeshFitting.FitSphere(points);
        Assert.True(solid.Mesh.Faces.Count > 0);
        Assert.True(solid.Mesh.Vertices.Count > 0);
    }

    [Fact]
    public void FitSphere_VerticesNearExpectedRadius()
    {
        var center = Vec3.Zero;
        double radius = 3.0;
        var points = GenerateSpherePoints(center, radius, 200, seed: 7);

        var solid = MeshFitting.FitSphere(points);

        var centroid = Vec3.Zero;
        for (int i = 0; i < solid.Mesh.Vertices.Count; i++)
            centroid += solid.Mesh.Vertices[i].Position;
        centroid *= 1.0 / solid.Mesh.Vertices.Count;

        for (int i = 0; i < solid.Mesh.Vertices.Count; i++)
        {
            double dist = Vec3.Distance(solid.Mesh.Vertices[i].Position, centroid);
            Assert.True(System.Math.Abs(dist - radius) < 0.5,
                $"Vertex {i} at distance {dist} from centroid, expected ~{radius}");
        }
    }

    // =========================================================================
    // Full pipeline: FitCylinder
    // =========================================================================

    [Fact]
    public void FitCylinder_FromPointCloud_CreatesValidSolid()
    {
        var points = GenerateCylinderPoints(Vec3.Zero, Vec3.UnitZ, 1.0, 10.0, 500, seed: 42);
        var solid = MeshFitting.FitCylinder(points);
        Assert.True(solid.Mesh.Faces.Count > 0);
        Assert.True(solid.Mesh.Vertices.Count > 0);
    }

    [Fact]
    public void FitCylinder_WithNormals_CreatesValidSolid()
    {
        var rng = new Random(55);
        int count = 200;
        var points = new Vec3[count];
        var normals = new Vec3[count];
        for (int i = 0; i < count; i++)
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double h = rng.NextDouble() * 10.0;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            points[i] = new Vec3(cos * 2.0, sin * 2.0, h);
            normals[i] = new Vec3(cos, sin, 0);
        }

        var solid = MeshFitting.FitCylinder(points, normals);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Full pipeline: FitPlane
    // =========================================================================

    [Fact]
    public void FitPlane_FromPointCloud_ReturnsCorrectPlane()
    {
        var points = new Vec3[]
        {
            new(0, 0, 5), new(1, 0, 5), new(0, 1, 5),
            new(1, 1, 5), new(2, 3, 5), new(-1, 2, 5),
        };

        var plane = MeshFitting.FitPlane(points);
        Assert.True(System.Math.Abs(System.Math.Abs(plane.Normal.Z) - 1.0) < 1e-8);
        Assert.True(System.Math.Abs(plane.Distance - 5.0) < 0.1 ||
                    System.Math.Abs(plane.Distance + 5.0) < 0.1);
    }

    // =========================================================================
    // CSG integration
    // =========================================================================

    [Fact]
    public void FittedSphere_IntersectWithCube_ProducesValidResult()
    {
        var spherePoints = GenerateSpherePoints(Vec3.Zero, 2.0, 100, seed: 10);
        var sphereSolid = MeshFitting.FitSphere(spherePoints);
        var cube = Primitives.Cube(Vec3.Zero, 3.0);

        var result = Csg.Intersect(sphereSolid, cube);
        Assert.True(result.Mesh.Faces.Count > 0, "Intersection should produce faces");
    }

    [Fact]
    public void FittedCylinder_DifferenceFromCube_ProducesValidResult()
    {
        var cylPoints = GenerateCylinderPoints(Vec3.Zero, Vec3.UnitZ, 0.5, 8.0, 500, seed: 20);
        var cylSolid = MeshFitting.FitCylinder(cylPoints);
        var cube = Primitives.Cube(new Vec3(0, 0, 4), 6.0);

        var result = Csg.Difference(cube, cylSolid);
        Assert.True(result.Mesh.Faces.Count > 0, "Difference should produce faces");
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static Vec3[] GenerateSpherePoints(Vec3 center, double radius, int count, int seed)
    {
        var rng = new Random(seed);
        var points = new Vec3[count];
        for (int i = 0; i < count; i++)
            points[i] = center + RandomDirection(rng) * radius;
        return points;
    }

    private static Vec3 RandomDirection(Random rng)
    {
        double u, v, s;
        do
        {
            u = rng.NextDouble() * 2 - 1;
            v = rng.NextDouble() * 2 - 1;
            s = u * u + v * v;
        } while (s >= 1.0 || s < 1e-10);

        double factor = 2.0 * System.Math.Sqrt(1 - s);
        return new Vec3(u * factor, v * factor, 1 - 2 * s);
    }

    private static Vec3[] GenerateCylinderPoints(Vec3 baseCenter, Vec3 axis, double radius,
        double height, int count, int seed)
    {
        var rng = new Random(seed);
        axis = axis.Normalized;
        var helper = System.Math.Abs(axis.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY;
        var u = Vec3.Cross(axis, helper).Normalized;
        var v = Vec3.Cross(axis, u);

        var points = new Vec3[count];
        for (int i = 0; i < count; i++)
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double h = rng.NextDouble() * height;
            points[i] = baseCenter + axis * h + u * (System.Math.Cos(angle) * radius) +
                         v * (System.Math.Sin(angle) * radius);
        }
        return points;
    }
}
