using System.Globalization;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class RobustFuzzEscalationTests
{
    private static readonly RobustOperationOptions StrictRobustOptions = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        TreatOpenArrangementAsError = true,
        UseRobustTriangulationKernel = true
    };

    private static readonly RobustOperationOptions ReplayStrictOptions = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true
    };

    [Fact]
    public void StageFocusedMutationFamilies_HaveNoUnknownFailureClasses()
    {
        var knownSignatures = LoadManifest(GetCorpusDirectory())
            .Select(static r => r.Signature)
            .ToHashSet(StringComparer.Ordinal);
        var unknown = new List<string>();

        foreach (var mutationCase in BuildMutationCases())
        {
            var eval = EvaluateMutationCase(mutationCase, mutationCase.InitialDelta);
            if (eval.IsHealthy)
                continue;

            if (knownSignatures.Contains(eval.Signature))
                continue;

            var minimized = MinimizeDelta(mutationCase, eval.Signature);
            string path = WriteMinimizedArtifact(mutationCase, minimized);
            unknown.Add(
                $"{mutationCase.CaseId};family={mutationCase.Family};"
                + $"signature={eval.Signature};delta={minimized.Delta.ToString("G17", CultureInfo.InvariantCulture)};"
                + $"artifact={path}");

            if (unknown.Count >= 8)
                break;
        }

        Assert.True(
            unknown.Count == 0,
            "Unknown strict fuzz failure classes:\n" + string.Join('\n', unknown));
    }

    [Fact]
    public void MinimizedReplayManifest_ReplaysToPinnedSignatures()
    {
        string corpusDir = GetCorpusDirectory();
        var rows = LoadManifest(corpusDir);
        Assert.NotEmpty(rows);
        Assert.All(rows, r => Assert.StartsWith("FUZZ-BLOCKER-", r.BlockerId, StringComparison.Ordinal));

        foreach (var row in rows)
        {
            string casePath = Path.Combine(corpusDir, row.CaseFile);
            Assert.True(File.Exists(casePath), $"Missing fuzz replay case: {casePath}");

            var result = RunReplayCase(casePath);
            Assert.False(result.Succeeded);
            RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(result.Diagnostics);
            string actualSignature = BuildFailureSignature(result);
            Assert.True(
                string.Equals(row.Signature, actualSignature, StringComparison.Ordinal),
                $"Signature mismatch for {row.CaseFile}.\nExpected: {row.Signature}\nActual:   {actualSignature}");
        }
    }

    private static IEnumerable<MutationCase> BuildMutationCases()
    {
        var rng = new Random(20260307);
        int caseId = 0;

        for (int i = 0; i < 12; i++)
        {
            double delta = NextRange(rng, 1e-5, 2e-2);
            double yJitter = NextRange(rng, -5e-3, 5e-3);
            var op = i % 2 == 0 ? RobustCsgOperation.Union : RobustCsgOperation.Intersection;
            yield return new MutationCase(
                CaseId: $"coplanar-tangent-{caseId++:D3}",
                Family: "coplanar-tangency",
                Operation: op,
                InitialDelta: delta,
                Build: d => (
                    Primitives.Cube(Vec3.Zero, 2.0),
                    Primitives.Cube(new Vec3(2.0 + d, yJitter, 0), 2.0)));
        }

        for (int i = 0; i < 12; i++)
        {
            double delta = NextRange(rng, 1e-4, 5e-2);
            var op = (i % 3) switch
            {
                0 => RobustCsgOperation.Union,
                1 => RobustCsgOperation.Intersection,
                _ => RobustCsgOperation.Difference
            };

            yield return new MutationCase(
                CaseId: $"near-collinear-{caseId++:D3}",
                Family: "near-collinear-axes",
                Operation: op,
                InitialDelta: delta,
                Build: d => (
                    Primitives.Cylinder(new Vec3(-1.2, 0, 0), new Vec3(1.0, d, 0), 0.5, 2.4, 3),
                    Primitives.Cylinder(new Vec3(-1.2, 0.06, 0), new Vec3(1.0, -d, 0), 0.5, 2.4, 3)));
        }

        for (int i = 0; i < 12; i++)
        {
            double delta = NextRange(rng, 2e-3, 4e-2);
            var op = (i % 3) switch
            {
                0 => RobustCsgOperation.Union,
                1 => RobustCsgOperation.Intersection,
                _ => RobustCsgOperation.Difference
            };

            yield return new MutationCase(
                CaseId: $"thin-shell-{caseId++:D3}",
                Family: "thin-shell",
                Operation: op,
                InitialDelta: delta,
                Build: d =>
                {
                    var cube = Primitives.Cube(Vec3.Zero, 2.0);
                    var top = HalfSpace.FromPointAndNormal(new Vec3(0, 0, d), Vec3.UnitZ);
                    var bottom = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -d), -Vec3.UnitZ);
                    var slabA = new Solid(Csg.Intersect(cube, top).Mesh);
                    var slab = new Solid(Csg.Intersect(slabA, bottom).Mesh);
                    var b = Primitives.Cube(new Vec3(0.25, 0.15, 0), 1.7);
                    return (slab, b);
                });
        }
    }

    private static MutationEval EvaluateMutationCase(MutationCase mutationCase, double delta)
    {
        var (a, b) = mutationCase.Build(delta);
        var result = Execute(mutationCase.Operation, a, b);
        bool healthy = TryValidateResult(result, out string? _);
        string signature = healthy ? "ok" : BuildFailureSignature(result);
        return new MutationEval(healthy, signature);
    }

    private static MinimizedFailure MinimizeDelta(MutationCase mutationCase, string signature)
    {
        double low = 0.0;
        double high = mutationCase.InitialDelta;
        const int rounds = 18;

        for (int i = 0; i < rounds; i++)
        {
            double mid = (low + high) * 0.5;
            if (mid <= 1e-8)
                break;

            var eval = EvaluateMutationCase(mutationCase, mid);
            if (!eval.IsHealthy && string.Equals(eval.Signature, signature, StringComparison.Ordinal))
                high = mid;
            else
                low = mid;
        }

        return new MinimizedFailure(high, signature);
    }

    private static string WriteMinimizedArtifact(MutationCase mutationCase, MinimizedFailure minimized)
    {
        string dir = Path.Combine(
            Path.GetTempPath(),
            "mdcsg-fuzz-minimized",
            DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(dir);

        string path = Path.Combine(dir, mutationCase.CaseId + ".case");
        var lines = new[]
        {
            $"family={mutationCase.Family}",
            $"operation={mutationCase.Operation}",
            $"delta={minimized.Delta.ToString("G17", CultureInfo.InvariantCulture)}",
            $"signature={minimized.Signature}"
        };
        File.WriteAllLines(path, lines);
        return path;
    }

    private static RobustCsgResult Execute(RobustCsgOperation operation, Solid a, Solid b)
        => operation switch
        {
            RobustCsgOperation.Union => RobustCsg.Union(a, b, StrictRobustOptions),
            RobustCsgOperation.Intersection => RobustCsg.Intersect(a, b, StrictRobustOptions),
            RobustCsgOperation.Difference => RobustCsg.Difference(a, b, StrictRobustOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    private static bool TryValidateResult(RobustCsgResult result, out string? failure)
    {
        if (!result.Succeeded || result.Result is null)
        {
            bool hasOutputTopologyErrors = result.Issues.Any(static i =>
                i.Code == RobustIssueCode.OutputMeshNotClosed
                || i.Code == RobustIssueCode.OutputMeshNotEdgeManifold
                || i.Code == RobustIssueCode.OutputMeshHasDegenerateFaces);
            if (!hasOutputTopologyErrors)
            {
                bool hasFailClosedCause = result.Issues.Any(static i =>
                    i.Code == RobustIssueCode.InputArrangementHasOpenEndpoints
                    || i.Code == RobustIssueCode.InputIntersectionContainsCoplanarPairs
                    || i.Code == RobustIssueCode.InputIntersectionContainsOpposingCoplanarPairs
                    || i.Code == RobustIssueCode.TriangulationNativeFailure
                    || i.Code == RobustIssueCode.TriangulationInvalidOrCrossingConstraints
                    || i.Code == RobustIssueCode.TriangulationPartitioningFailed
                    || i.Code == RobustIssueCode.TriangulationConstrainedEarFailed
                    || i.Code == RobustIssueCode.TriangulationWorkBudgetExceeded
                    || i.Code == RobustIssueCode.ReconstructionInvariantViolation
                    || i.Code == RobustIssueCode.ReconstructionPatchSelectionFailed
                    || i.Code == RobustIssueCode.ReconstructionStitchingFailed
                    || i.Code == RobustIssueCode.StageInvariantViolation);
                if (hasFailClosedCause)
                {
                    failure = null;
                    return true;
                }
            }

            failure = "operation failed";
            return false;
        }

        if (result.Issues.Any(static i => i.Severity == RobustIssueSeverity.Error))
        {
            failure = "error issues reported";
            return false;
        }

        if (result.Diagnostics.TriangulationLegacyFallbackCount > 0
            || result.Diagnostics.TriangulationNativeFailureCount > 0)
        {
            failure = "triangulation degraded";
            return false;
        }

        int boundary = MeshValidator.CountBoundaryEdges(result.Result.Mesh);
        if (boundary > 0 || !MeshValidator.IsEdgeManifold(result.Result.Mesh))
        {
            failure = "output topology invalid";
            return false;
        }

        if (!result.Diagnostics.StageInvariantCertificates.Any(static c => c.StartsWith("triangulation:pass;", StringComparison.Ordinal))
            || !result.Diagnostics.StageInvariantCertificates.Any(static c => c.StartsWith("output:pass;", StringComparison.Ordinal)))
        {
            failure = "stage certificates missing pass markers";
            return false;
        }

        failure = null;
        return true;
    }

    private static string BuildFailureSignature(RobustCsgResult result)
    {
        string issues = string.Join(
            "|",
            result.Issues
                .Where(static i => i.Severity == RobustIssueSeverity.Error)
                .Select(static i => i.Code.ToString())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static x => x, StringComparer.Ordinal));

        string reconstruction = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(static c => c.StartsWith("reconstruction:", StringComparison.Ordinal))
            ?? string.Empty;
        string output = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(static c => c.StartsWith("output:", StringComparison.Ordinal))
            ?? string.Empty;
        string residual = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(static c => c.StartsWith("deg-residual:", StringComparison.Ordinal))
            ?? string.Empty;
        int boundary = ParseIntTagOrDefault(reconstruction, "boundary", -1);
        int openLoops = ParseIntTagOrDefault(reconstruction, "openLoops", -1);
        int unmatched = ParseIntTagOrDefault(reconstruction, "unmatched", -1);
        int deg = ParseIntTagOrDefault(output, "deg", -1);
        int taxDupVid = ParseIntTagOrDefault(residual, "taxDupVid", 0);
        int taxZeroEdge = ParseIntTagOrDefault(residual, "taxZeroEdge", 0);
        int taxDupPos = ParseIntTagOrDefault(residual, "taxDupPos", 0);
        int taxCollinear = ParseIntTagOrDefault(residual, "taxCollinear", 0);
        string taxonomy = DominantTaxonomy(taxDupVid, taxZeroEdge, taxDupPos, taxCollinear);
        return $"issues={issues};boundary={boundary};openLoops={openLoops};unmatched={unmatched};deg={deg};tax={taxonomy}";
    }

    private static string DominantTaxonomy(int dupVid, int zeroEdge, int dupPos, int collinear)
    {
        var buckets = new[]
        {
            ("dupVid", dupVid),
            ("zeroEdge", zeroEdge),
            ("dupPos", dupPos),
            ("collinear", collinear)
        };

        var best = buckets
            .OrderByDescending(static b => b.Item2)
            .ThenBy(static b => b.Item1, StringComparer.Ordinal)
            .First();
        return best.Item2 <= 0 ? "none" : best.Item1;
    }

    private static int ParseIntTagOrDefault(string cert, string tag, int fallback)
    {
        if (string.IsNullOrWhiteSpace(cert))
            return fallback;

        string prefix = tag + "=";
        foreach (string part in cert.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!part.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            string text = part[prefix.Length..];
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                return value;
            return fallback;
        }

        return fallback;
    }

    private static RobustCsgResult RunReplayCase(string casePath)
    {
        string recipe = LoadRecipe(casePath);
        return recipe switch
        {
            "showcase-chained-step3" => RunShowcaseChainedStep3(),
            _ => throw new InvalidOperationException($"Unsupported fuzz replay recipe '{recipe}'.")
        };
    }

    private static RobustCsgResult RunShowcaseChainedStep3()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = RobustCsg.Intersect(sphere, box, ReplayStrictOptions);
        Assert.True(step1.Succeeded);
        var step2 = RobustCsg.Difference(new Solid(step1.Result!.Mesh), cylX, ReplayStrictOptions);
        Assert.True(step2.Succeeded);
        return RobustCsg.Difference(new Solid(step2.Result!.Mesh), cylY, ReplayStrictOptions);
    }

    private static string GetCorpusDirectory()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "MdCsg.slnx")))
                return Path.Combine(dir, "tests", "MdCsg.Robust.Conformance", "ReplayCorpus", "fuzz");
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root from test base directory.");
    }

    private static IReadOnlyList<FuzzManifestRow> LoadManifest(string corpusDir)
    {
        string path = Path.Combine(corpusDir, "manifest.csv");
        Assert.True(File.Exists(path), $"Missing fuzz replay manifest: {path}");
        var lines = File.ReadAllLines(path)
            .Where(static l => !string.IsNullOrWhiteSpace(l))
            .ToArray();
        Assert.True(lines.Length >= 2, $"Fuzz replay manifest has no rows: {path}");

        var rows = new List<FuzzManifestRow>(lines.Length - 1);
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',', StringSplitOptions.None);
            Assert.True(parts.Length == 3, $"Invalid fuzz replay manifest row {i + 1}: {lines[i]}");
            rows.Add(new FuzzManifestRow(parts[0], parts[1], parts[2]));
        }

        return rows;
    }

    private static string LoadRecipe(string casePath)
    {
        string line = File.ReadAllLines(casePath)
            .FirstOrDefault(static l => !string.IsNullOrWhiteSpace(l))
            ?? string.Empty;
        const string Prefix = "recipe=";
        Assert.StartsWith(Prefix, line, StringComparison.Ordinal);
        return line[Prefix.Length..];
    }

    private static double NextRange(Random rng, double min, double max)
        => min + (max - min) * rng.NextDouble();

    private sealed record MutationCase(
        string CaseId,
        string Family,
        RobustCsgOperation Operation,
        double InitialDelta,
        Func<double, (Solid A, Solid B)> Build);

    private readonly record struct MutationEval(bool IsHealthy, string Signature);

    private readonly record struct MinimizedFailure(double Delta, string Signature);

    private readonly record struct FuzzManifestRow(string CaseFile, string BlockerId, string Signature);
}
