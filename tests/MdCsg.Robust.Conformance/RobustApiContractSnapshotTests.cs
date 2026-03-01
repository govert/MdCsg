using System.Reflection;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust.Diagnostics.Legacy;

namespace MdCsg.Robust.Conformance;

public class RobustApiContractSnapshotTests
{
    [Fact]
    public void RobustIssueCode_Snapshot_IsStable()
    {
        var expected = new[]
        {
            "InputMeshContainsNonFiniteCoordinate",
            "InputMeshNotClosed",
            "InputMeshNotEdgeManifold",
            "InputMeshHasDegenerateFaces",
            "InputIntersectionContainsCoplanarPairs",
            "InputIntersectionContainsOpposingCoplanarPairs",
            "InputArrangementHasOpenEndpoints",
            "StageInvariantViolation",
            "TriangulationNativeFailure",
            "TriangulationInvalidOrCrossingConstraints",
            "TriangulationPartitioningFailed",
            "TriangulationConstrainedEarFailed",
            "TriangulationWorkBudgetExceeded",
            "ReconstructionInvariantViolation",
            "ReconstructionPatchSelectionFailed",
            "ReconstructionStitchingFailed",
            "OutputMeshNotClosed",
            "OutputMeshNotEdgeManifold",
            "OutputMeshHasDegenerateFaces"
        };

        Assert.Equal(expected, Enum.GetNames<RobustIssueCode>());
    }

    [Fact]
    public void RobustResultShape_Snapshot_IsStable()
    {
        var robustResultProps = typeof(RobustCsgResult)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(static p => p.Name)
            .OrderBy(static n => n, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            new[] { "Diagnostics", "HasErrors", "Issues", "Result", "Succeeded" },
            robustResultProps);

        var diagnosticsProps = typeof(RobustDiagnostics)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(static p => p.Name)
            .OrderBy(static n => n, StringComparer.Ordinal)
            .ToArray();
        Assert.Contains("StageInvariantCertificates", diagnosticsProps);
        Assert.Contains("ReconstructionInvariantCertificates", diagnosticsProps);
        Assert.Contains("TriangulationNativeFailureCodes", diagnosticsProps);
    }

    [Fact]
    public void StrictCertificates_Snapshot_ContainsRequiredPrefixes()
    {
        var result = RobustCsg.Union(
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Cube(new Vec3(0.75, 0, 0), 2.0),
            new RobustOperationOptions
            {
                Mode = RobustMode.Strict,
                Deterministic = true,
                UseRobustTriangulationKernel = true
            });

        Assert.True(result.Succeeded);
        var certs = result.Diagnostics.StageInvariantCertificates;

        var requiredPrefixes = new[]
        {
            "input-policy:",
            "input:",
            "arrangement:",
            "patch-extraction:",
            "triangulation:",
            "reconstruction-policy:",
            "reconstruction-authority:",
            "reconstruction-pre:",
            "reconstruction:",
            "classification:",
            "coplanar-matrix:",
            "output:"
        };

        foreach (string prefix in requiredPrefixes)
        {
            Assert.Contains(
                certs,
                c => c.StartsWith(prefix, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void LegacyDiagnostics_IsExplicitOptInOnly()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.6, 0, 0), 2.0);

        Assert.Throws<InvalidOperationException>(() => LegacyComparison.Union(a, b));

        var compared = LegacyComparison.Union(a, b, new LegacyDiagnosticsOptions
        {
            AllowLegacyExecution = true,
            RobustOptions = new RobustOperationOptions
            {
                Mode = RobustMode.Strict,
                Deterministic = true,
                UseRobustTriangulationKernel = true
            }
        });

        Assert.Equal(RobustCsgOperation.Union, compared.Operation);
        Assert.NotNull(compared.RobustResult);
        Assert.NotNull(compared.LegacyResult);
    }
}

public class RobustMigrationDocTests
{
    [Fact]
    public void MigrationDoc_ContainsRequiredSections_AndContracts()
    {
        string path = Path.Combine(GetRepoRoot(), "docs", "strict-robust-migration.md");
        Assert.True(File.Exists(path), $"Missing migration doc: {path}");

        string text = File.ReadAllText(path);
        var requiredSections = new[]
        {
            "# Strict Robust Migration Guide",
            "## Behavior Mapping",
            "## Strict Contract Checklist",
            "## Legacy Isolation Policy",
            "## Validation and Gates"
        };

        foreach (string section in requiredSections)
            Assert.Contains(section, text, StringComparison.Ordinal);

        var requiredTokens = new[]
        {
            "RobustCsgResult",
            "RobustDiagnostics",
            "NonManifoldInputPolicy",
            "input-policy:",
            "reconstruction:",
            "output:",
            "LegacyComparison",
            "AllowLegacyExecution",
            "RobustApiContractSnapshotTests",
            "RobustMigrationDocTests"
        };

        foreach (string token in requiredTokens)
            Assert.Contains(token, text, StringComparison.Ordinal);
    }

    private static string GetRepoRoot()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "MdCsg.slnx")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root from test base directory.");
    }
}
