using System.Globalization;
using System.Text.Json;
using MdCsg.Math;
using MdCsg.Robust.Kernel.Triangulation;

namespace MdCsg.Robust.Conformance;

public class TriangulationReplayCorpusTests
{
    [Fact]
    public void CorpusManifest_AllCases_ReplayToExpectedTriangulationOutcomes()
    {
        string corpusDir = GetCorpusDirectory();
        string manifestPath = Path.Combine(corpusDir, "manifest.csv");
        Assert.True(File.Exists(manifestPath), $"Missing triangulation replay manifest: {manifestPath}");

        var rows = LoadManifest(manifestPath);
        Assert.NotEmpty(rows);

        var triangulator = new RobustConstrainedTriangulator();
        foreach (var row in rows)
        {
            string casePath = Path.Combine(corpusDir, row.CaseFile);
            Assert.True(File.Exists(casePath), $"Missing triangulation replay case file: {casePath}");

            var replay = LoadCase(casePath);
            var options = new RobustTriangulationOptions
            {
                ConstraintWorkBudgetOverride = row.ConstraintWorkBudgetOverride
            };

            var result1 = triangulator.Triangulate(
                replay.Vertices,
                replay.Constraints,
                replay.FaceNormal,
                options);
            var result2 = triangulator.Triangulate(
                replay.Vertices,
                replay.Constraints,
                replay.FaceNormal,
                options);

            Assert.Equal(row.ExpectSuccess, result1.Succeeded);
            Assert.Equal(row.ExpectUsedLegacy, result1.UsedLegacyKernel);
            Assert.Equal(row.ExpectedTriangleCount, result1.Triangles.Count);
            Assert.Equal(Fingerprint(result1), Fingerprint(result2));

            if (row.ExpectSuccess)
            {
                Assert.Equal(RobustTriangulationFallbackReason.None, result1.FailureReason);
                Assert.Equal(RobustTriangulationFailureStage.None, result1.FailureStage);
                Assert.Null(result1.FailureCode);
                Assert.Null(result1.FailureSignature);
            }
            else
            {
                Assert.Equal(row.ExpectFailureReason, result1.FailureReason);
                Assert.Equal(row.ExpectFailureStage, result1.FailureStage);
                Assert.Equal(row.ExpectFailureCode, result1.FailureCode);
                Assert.NotNull(result1.FailureSignature);
                Assert.StartsWith(row.ExpectFailureCode + ":", result1.FailureSignature, StringComparison.Ordinal);
            }

            foreach (var edge in row.RequiredEdges)
                Assert.Contains(result1.Triangles, t => HasEdge(t, edge.A, edge.B));
        }
    }

    [Fact]
    public void CorpusManifest_CoversKnownNativeFailureClasses()
    {
        string manifestPath = Path.Combine(GetCorpusDirectory(), "manifest.csv");
        var rows = LoadManifest(manifestPath);

        Assert.Contains(rows, r => r.ExpectFailureCode == "constraint-validation/invalid-or-crossing");
        Assert.Contains(rows, r => r.ExpectFailureCode == "face-point-set/work-budget-exceeded");
    }

    private static string GetCorpusDirectory()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "MdCsg.slnx")))
                return Path.Combine(dir, "tests", "MdCsg.Robust.Conformance", "ReplayCorpus", "triangulation");
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
            Assert.True(parts.Length == 9, $"Invalid manifest row at line {i + 1}: {lines[i]}");

            bool expectSuccess = ParseBool(parts[1], i, "expect_success");
            bool expectUsedLegacy = ParseBool(parts[2], i, "expect_used_legacy");
            var expectFailureReason = ParseFailureReason(parts[3], i);
            var expectFailureStage = ParseFailureStage(parts[4], i);
            string? expectFailureCode = string.IsNullOrWhiteSpace(parts[5]) ? null : parts[5];
            int? budgetOverride = ParseOptionalInt(parts[6], i, "constraint_budget_override");
            int expectedTriangles = ParseInt(parts[7], i, "expected_triangles");
            var requiredEdges = ParseEdges(parts[8]);

            if (expectSuccess)
            {
                Assert.Equal(RobustTriangulationFallbackReason.None, expectFailureReason);
                Assert.Equal(RobustTriangulationFailureStage.None, expectFailureStage);
                Assert.Null(expectFailureCode);
            }
            else
            {
                Assert.NotEqual(RobustTriangulationFallbackReason.None, expectFailureReason);
                Assert.NotEqual(RobustTriangulationFailureStage.None, expectFailureStage);
                Assert.False(string.IsNullOrWhiteSpace(expectFailureCode));
            }

            rows.Add(new ManifestRow(
                CaseFile: parts[0],
                ExpectSuccess: expectSuccess,
                ExpectUsedLegacy: expectUsedLegacy,
                ExpectFailureReason: expectFailureReason,
                ExpectFailureStage: expectFailureStage,
                ExpectFailureCode: expectFailureCode,
                ConstraintWorkBudgetOverride: budgetOverride,
                ExpectedTriangleCount: expectedTriangles,
                RequiredEdges: requiredEdges));
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

    private static int? ParseOptionalInt(string text, int row, string column)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        return ParseInt(text, row, column);
    }

    private static RobustTriangulationFallbackReason ParseFailureReason(string text, int row)
    {
        if (string.IsNullOrWhiteSpace(text))
            return RobustTriangulationFallbackReason.None;

        Assert.True(
            Enum.TryParse<RobustTriangulationFallbackReason>(text, ignoreCase: false, out var reason),
            $"Invalid fallback reason in manifest row {row + 1}: '{text}'.");
        return reason;
    }

    private static RobustTriangulationFailureStage ParseFailureStage(string text, int row)
    {
        if (string.IsNullOrWhiteSpace(text))
            return RobustTriangulationFailureStage.None;

        Assert.True(
            Enum.TryParse<RobustTriangulationFailureStage>(text, ignoreCase: false, out var stage),
            $"Invalid failure stage in manifest row {row + 1}: '{text}'.");
        return stage;
    }

    private static IReadOnlyList<(int A, int B)> ParseEdges(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<(int A, int B)>();

        var edges = new List<(int A, int B)>();
        var parts = text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            var pair = part.Split('-', StringSplitOptions.TrimEntries);
            Assert.True(pair.Length == 2, $"Invalid edge specifier '{part}'.");
            Assert.True(int.TryParse(pair[0], out int a), $"Invalid edge start '{pair[0]}'.");
            Assert.True(int.TryParse(pair[1], out int b), $"Invalid edge end '{pair[1]}'.");
            edges.Add((a, b));
        }

        return edges;
    }

    private static string Fingerprint(RobustTriangulationResult result)
    {
        string tris = string.Join("|", result.Triangles.Select(t => $"{t.A},{t.B},{t.C}"));
        string code = result.FailureCode ?? "<none>";
        string stage = result.FailureStage.ToString();
        string reason = result.FailureReason.ToString();
        return $"{result.Succeeded}:{result.UsedLegacyKernel}:{result.DroppedDegenerateTriangleCount}:{reason}:{stage}:{code}:{tris}";
    }

    private static bool HasEdge((int A, int B, int C) tri, int start, int end)
        => (tri.A == start && tri.B == end) || (tri.B == start && tri.A == end)
            || (tri.B == start && tri.C == end) || (tri.C == start && tri.B == end)
            || (tri.C == start && tri.A == end) || (tri.A == start && tri.C == end);

    private sealed record ManifestRow(
        string CaseFile,
        bool ExpectSuccess,
        bool ExpectUsedLegacy,
        RobustTriangulationFallbackReason ExpectFailureReason,
        RobustTriangulationFailureStage ExpectFailureStage,
        string? ExpectFailureCode,
        int? ConstraintWorkBudgetOverride,
        int ExpectedTriangleCount,
        IReadOnlyList<(int A, int B)> RequiredEdges);

    private sealed record TriangulationReplayCase(
        Vec3 FaceNormal,
        Vec3[] Vertices,
        (int Start, int End)[] Constraints);

    private sealed class TriangulationReplayCaseJson
    {
        public double[] FaceNormal { get; init; } = Array.Empty<double>();
        public double[][] Vertices { get; init; } = Array.Empty<double[]>();
        public int[][] Constraints { get; init; } = Array.Empty<int[]>();
    }

    private static TriangulationReplayCase? DeserializeCase(string json)
    {
        var data = JsonSerializer.Deserialize<TriangulationReplayCaseJson>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (data is null || data.FaceNormal.Length != 3 || data.Vertices.Length < 3)
            return null;

        var faceNormal = new Vec3(data.FaceNormal[0], data.FaceNormal[1], data.FaceNormal[2]);
        var vertices = new Vec3[data.Vertices.Length];
        for (int i = 0; i < data.Vertices.Length; i++)
        {
            var v = data.Vertices[i];
            if (v.Length != 3)
                return null;
            vertices[i] = new Vec3(v[0], v[1], v[2]);
        }

        var constraints = new (int Start, int End)[data.Constraints.Length];
        for (int i = 0; i < data.Constraints.Length; i++)
        {
            var e = data.Constraints[i];
            if (e.Length != 2)
                return null;
            constraints[i] = (e[0], e[1]);
        }

        return new TriangulationReplayCase(faceNormal, vertices, constraints);
    }

    private static TriangulationReplayCase LoadCase(string casePath)
    {
        var parsed = DeserializeCase(File.ReadAllText(casePath));
        Assert.NotNull(parsed);
        return parsed!;
    }
}
