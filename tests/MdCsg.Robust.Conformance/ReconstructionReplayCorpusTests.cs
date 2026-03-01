using System.Globalization;
using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Robust.Conformance;

public class ReconstructionReplayCorpusTests
{
    private static readonly RobustOperationOptions StrictRobustOpts = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true
    };

    [Fact]
    public void CorpusManifest_AllCases_ReplayToExpectedReconstructionContracts()
    {
        string corpusDir = GetCorpusDirectory();
        string manifestPath = Path.Combine(corpusDir, "manifest.csv");
        Assert.True(File.Exists(manifestPath), $"Missing reconstruction replay manifest: {manifestPath}");

        var rows = LoadManifest(manifestPath);
        Assert.NotEmpty(rows);

        foreach (var row in rows)
        {
            string casePath = Path.Combine(corpusDir, row.CaseFile);
            Assert.True(File.Exists(casePath), $"Missing reconstruction replay case file: {casePath}");

            var first = RunReplayCase(casePath);
            var second = RunReplayCase(casePath);

            Assert.Equal(row.ExpectSuccess, first.Succeeded);
            Assert.Equal(first.Succeeded, second.Succeeded);
            RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(first.Diagnostics);
            RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(second.Diagnostics);

            string pre1 = GetStageCertificate(first, "reconstruction-pre:");
            string pre2 = GetStageCertificate(second, "reconstruction-pre:");
            string recon1 = GetStageCertificate(first, "reconstruction:");
            string recon2 = GetStageCertificate(second, "reconstruction:");

            Assert.Equal(pre1, pre2);
            Assert.Equal(recon1, recon2);
            Assert.StartsWith(row.ExpectReconstructionPrefix, recon1, StringComparison.Ordinal);
            Assert.Equal(row.ExpectBoundary, ParseIntTag(recon1, "boundary"));
            Assert.Equal(row.ExpectOpenLoops, ParseIntTag(recon1, "openLoops"));
            Assert.Equal(row.ExpectUnmatched, ParseIntTag(recon1, "unmatched"));

            foreach (var expectedCode in row.ExpectIssueCodes)
                Assert.Contains(first.Issues, i => i.Code == expectedCode);
        }
    }

    [Fact]
    public void CorpusManifest_CoversPinnedStep3BlockerClass()
    {
        string manifestPath = Path.Combine(GetCorpusDirectory(), "manifest.csv");
        var rows = LoadManifest(manifestPath);

        Assert.Contains(rows, r =>
            r.CaseFile == "showcase-chained-step3.case"
            && !r.ExpectSuccess
            && r.ExpectBoundary == 3
            && r.ExpectUnmatched == 3);
    }

    private static RobustCsgResult RunReplayCase(string casePath)
    {
        string recipe = LoadRecipe(casePath);
        return recipe switch
        {
            "showcase-chained-step3" => RunShowcaseChainedStep3(),
            _ => throw new InvalidOperationException($"Unsupported reconstruction replay recipe '{recipe}'.")
        };
    }

    private static RobustCsgResult RunShowcaseChainedStep3()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = RobustCsg.Intersect(sphere, box, StrictRobustOpts);
        Assert.True(step1.Succeeded);
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(step1.Diagnostics);
        var step1Solid = new Solid(step1.Result!.Mesh);

        var step2 = RobustCsg.Difference(step1Solid, cylX, StrictRobustOpts);
        Assert.True(step2.Succeeded);
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(step2.Diagnostics);
        var step2Solid = new Solid(step2.Result!.Mesh);

        return RobustCsg.Difference(step2Solid, cylY, StrictRobustOpts);
    }

    private static string GetCorpusDirectory()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "MdCsg.slnx")))
                return Path.Combine(dir, "tests", "MdCsg.Robust.Conformance", "ReplayCorpus", "reconstruction");
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root from test base directory.");
    }

    private static IReadOnlyList<ManifestRow> LoadManifest(string manifestPath)
    {
        var lines = File.ReadAllLines(manifestPath)
            .Where(static l => !string.IsNullOrWhiteSpace(l))
            .ToArray();
        Assert.True(lines.Length >= 2, $"Manifest has no rows: {manifestPath}");

        var rows = new List<ManifestRow>(lines.Length - 1);
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',', StringSplitOptions.None);
            Assert.True(parts.Length == 7, $"Invalid manifest row at line {i + 1}: {lines[i]}");

            rows.Add(new ManifestRow(
                CaseFile: parts[0],
                ExpectSuccess: ParseBool(parts[1], i, "expect_success"),
                ExpectReconstructionPrefix: parts[2],
                ExpectBoundary: ParseInt(parts[3], i, "expect_boundary"),
                ExpectOpenLoops: ParseInt(parts[4], i, "expect_open_loops"),
                ExpectUnmatched: ParseInt(parts[5], i, "expect_unmatched"),
                ExpectIssueCodes: ParseIssueCodes(parts[6], i)));
        }

        return rows;
    }

    private static bool ParseBool(string text, int row, string column)
    {
        Assert.True(
            bool.TryParse(text, out bool value),
            $"Invalid boolean in manifest row {row + 1}, column {column}: '{text}'.");
        return value;
    }

    private static int ParseInt(string text, int row, string column)
    {
        Assert.True(
            int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value),
            $"Invalid integer in manifest row {row + 1}, column {column}: '{text}'.");
        return value;
    }

    private static IReadOnlyList<RobustIssueCode> ParseIssueCodes(string text, int row)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<RobustIssueCode>();

        var tokens = text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var codes = new List<RobustIssueCode>(tokens.Length);
        foreach (string token in tokens)
        {
            Assert.True(
                Enum.TryParse<RobustIssueCode>(token, ignoreCase: false, out var code),
                $"Invalid issue code in manifest row {row + 1}: '{token}'.");
            codes.Add(code);
        }

        return codes;
    }

    private static string GetStageCertificate(RobustCsgResult result, string prefix)
    {
        string? cert = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert), $"Missing stage certificate '{prefix}'.");
        return cert!;
    }

    private static int ParseIntTag(string cert, string tag)
    {
        string prefix = tag + "=";
        var parts = cert.Split(';', StringSplitOptions.RemoveEmptyEntries);
        string value = parts.FirstOrDefault(p => p.StartsWith(prefix, StringComparison.Ordinal)) ?? string.Empty;
        Assert.False(string.IsNullOrWhiteSpace(value), $"Missing tag '{tag}' in certificate '{cert}'.");
        string text = value[prefix.Length..];
        Assert.True(int.TryParse(text, out int parsed), $"Invalid integer tag '{tag}' in certificate '{cert}'.");
        return parsed;
    }

    private static string LoadRecipe(string casePath)
    {
        string line = File.ReadAllLines(casePath)
            .FirstOrDefault(static l => !string.IsNullOrWhiteSpace(l))
            ?? string.Empty;
        Assert.False(string.IsNullOrWhiteSpace(line), $"Case file is empty: {casePath}");
        const string Prefix = "recipe=";
        Assert.StartsWith(Prefix, line, StringComparison.Ordinal);
        return line[Prefix.Length..];
    }

    private sealed record ManifestRow(
        string CaseFile,
        bool ExpectSuccess,
        string ExpectReconstructionPrefix,
        int ExpectBoundary,
        int ExpectOpenLoops,
        int ExpectUnmatched,
        IReadOnlyList<RobustIssueCode> ExpectIssueCodes);
}
