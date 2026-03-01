using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class InputPolicyConformanceTests
{
    [Fact]
    public void RejectPolicy_FailCloses_OnDirtyInput_AndEmitsPolicyCertificate()
    {
        var dirty = BuildCubeWithDanglingTriangle();
        var other = Primitives.Cube(new Vec3(0.6, 0, 0), 2.0);

        var result = RobustCsg.Union(dirty, other, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = true,
            NonManifoldInputPolicy = NonManifoldInputPolicy.Reject
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.InputMeshNotClosed);

        string policy = GetCert(result, "input-policy:");
        Assert.Contains("policy=Reject", policy, StringComparison.Ordinal);
        Assert.Contains("A=original", policy, StringComparison.Ordinal);
        Assert.Contains("Atotal=13", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void SanitizePolicy_CanDropDanglingComponent_AndProceed()
    {
        var dirty = BuildCubeWithDanglingTriangle();
        var other = Primitives.Cube(new Vec3(0.6, 0, 0), 2.0);

        var result = RobustCsg.Union(dirty, other, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = true,
            NonManifoldInputPolicy = NonManifoldInputPolicy.SanitizeAndContinue
        });

        string policy = GetCert(result, "input-policy:");
        Assert.Contains("policy=SanitizeAndContinue", policy, StringComparison.Ordinal);
        Assert.Contains("A=sanitized", policy, StringComparison.Ordinal);
        Assert.Contains("Araw=2", policy, StringComparison.Ordinal);
        Assert.Contains("Avalid=1", policy, StringComparison.Ordinal);
        Assert.Contains("Akept=12", policy, StringComparison.Ordinal);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(result.Result!.Mesh));
    }

    [Fact]
    public void SanitizePolicy_FailCloses_WhenNoClosedComponentExists()
    {
        var open = BuildOpenSingleTriangle();
        var other = Primitives.Cube(new Vec3(0.2, 0, 0), 1.5);

        var result = RobustCsg.Union(open, other, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = true,
            NonManifoldInputPolicy = NonManifoldInputPolicy.SanitizeAndContinue
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.InputMeshNotClosed);

        string policy = GetCert(result, "input-policy:");
        Assert.Contains("policy=SanitizeAndContinue", policy, StringComparison.Ordinal);
        Assert.Contains("A=original", policy, StringComparison.Ordinal);
        Assert.Contains("Araw=1", policy, StringComparison.Ordinal);
        Assert.Contains("Avalid=0", policy, StringComparison.Ordinal);
        Assert.Contains("Akept=1", policy, StringComparison.Ordinal);
    }

    private static Solid BuildCubeWithDanglingTriangle()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var positions = cube.Mesh.Vertices.Select(v => v.Position).ToList();
        var triangles = cube.Mesh.Faces
            .Select(static f =>
            {
                var verts = f.GetVertices();
                return (verts[0].Id, verts[1].Id, verts[2].Id);
            })
            .ToList();

        int baseIndex = positions.Count;
        positions.Add(new Vec3(10.0, 10.0, 10.0));
        positions.Add(new Vec3(10.4, 10.0, 10.0));
        positions.Add(new Vec3(10.0, 10.4, 10.0));
        triangles.Add((baseIndex, baseIndex + 1, baseIndex + 2));

        var mesh = new MeshBuilder(0.0).Build(positions, triangles);
        return new Solid(mesh);
    }

    private static Solid BuildOpenSingleTriangle()
    {
        var positions = new[]
        {
            new Vec3(0.0, 0.0, 0.0),
            new Vec3(1.0, 0.0, 0.0),
            new Vec3(0.0, 1.0, 0.0)
        };
        var triangles = new[] { (0, 1, 2) };
        return new Solid(new MeshBuilder(0.0).Build(positions, triangles));
    }

    private static string GetCert(RobustCsgResult result, string prefix)
    {
        string? cert = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert));
        return cert!;
    }
}
