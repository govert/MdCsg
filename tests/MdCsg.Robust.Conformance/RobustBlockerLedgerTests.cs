using System.Text.Json;
using System.Text.Json.Serialization;

namespace MdCsg.Robust.Conformance;

public class RobustBlockerLedgerTests
{
    [Fact]
    public void LedgerSchema_IsValid_AndDeterministic()
    {
        var ledger = LoadLedger();
        Assert.Equal(1, ledger.SchemaVersion);
        Assert.NotNull(ledger.QualityBands);
        Assert.False(string.IsNullOrWhiteSpace(ledger.QualityBands.HardFail));
        Assert.False(string.IsNullOrWhiteSpace(ledger.QualityBands.KnownBlocked));
        Assert.False(string.IsNullOrWhiteSpace(ledger.QualityBands.Observability));
        Assert.NotNull(ledger.Blockers);

        var ids = new HashSet<string>(StringComparer.Ordinal);
        string? previousId = null;
        foreach (var blocker in ledger.Blockers)
        {
            Assert.False(string.IsNullOrWhiteSpace(blocker.Id));
            Assert.True(ids.Add(blocker.Id), $"Duplicate blocker id: {blocker.Id}");
            Assert.False(string.IsNullOrWhiteSpace(blocker.Source));
            Assert.False(string.IsNullOrWhiteSpace(blocker.Signature));
            Assert.True(blocker.OwningStage >= 1, $"Invalid owning stage for {blocker.Id}");
            Assert.NotEmpty(blocker.ExpectedCertificates);
            Assert.All(
                blocker.ExpectedCertificates,
                static c => Assert.False(string.IsNullOrWhiteSpace(c)));

            if (previousId is not null)
            {
                Assert.True(
                    string.CompareOrdinal(previousId, blocker.Id) < 0,
                    $"Ledger blockers must be sorted by id: {previousId} before {blocker.Id}");
            }

            previousId = blocker.Id;
        }
    }

    [Fact]
    public void LedgerHasNoActiveBlockers()
    {
        var ledger = LoadLedger();
        Assert.Empty(ledger.Blockers);
    }

    [Fact]
    public void FuzzManifestSignatures_ArePresentInLedger()
    {
        var ledger = LoadLedger();
        var known = ledger.Blockers
            .Select(static b => b.Signature)
            .ToHashSet(StringComparer.Ordinal);

        var manifestSignatures = LoadFuzzManifestSignatures();
        if (manifestSignatures.Count == 0) return; // No blockers, no signatures to check
        foreach (string signature in manifestSignatures)
            Assert.Contains(signature, known);
    }

    [Fact]
    public void UnknownBlockerSignature_IsHardFail_ByPolicy()
    {
        var ledger = LoadLedger();
        var known = ledger.Blockers
            .Select(static b => b.Signature)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Throws<InvalidOperationException>(() =>
            EnforceKnownBlockerSignature(
                "issues=UnknownCase;boundary=-1;openLoops=-1;unmatched=-1",
                known));
    }

    private static void EnforceKnownBlockerSignature(string signature, IReadOnlySet<string> knownSignatures)
    {
        if (!knownSignatures.Contains(signature))
            throw new InvalidOperationException($"hard-fail:unknown-blocker-signature:{signature}");
    }

    private static IReadOnlyList<string> LoadFuzzManifestSignatures()
    {
        string path = Path.Combine(GetRepoRoot(), "tests", "MdCsg.Robust.Conformance", "ReplayCorpus", "fuzz", "manifest.csv");
        Assert.True(File.Exists(path), $"Missing fuzz manifest: {path}");
        var lines = File.ReadAllLines(path)
            .Where(static l => !string.IsNullOrWhiteSpace(l))
            .ToArray();
        if (lines.Length < 2) return new List<string>(); // Header only, no data rows

        var signatures = new List<string>(lines.Length - 1);
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',', StringSplitOptions.None);
            Assert.True(parts.Length == 3, $"Invalid fuzz manifest row {i + 1}: {lines[i]}");
            signatures.Add(parts[2]);
        }

        return signatures;
    }

    private static RobustBlockerLedger LoadLedger()
    {
        string path = Path.Combine(GetRepoRoot(), "tools", "ci", "robust-blocker-ledger.json");
        Assert.True(File.Exists(path), $"Missing blocker ledger: {path}");
        var ledger = JsonSerializer.Deserialize<RobustBlockerLedger>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false
            });

        Assert.NotNull(ledger);
        return ledger!;
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

    private sealed class RobustBlockerLedger
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; init; }

        [JsonPropertyName("qualityBands")]
        public QualityBandPolicy QualityBands { get; init; } = new();

        [JsonPropertyName("blockers")]
        public IReadOnlyList<BlockerEntry> Blockers { get; init; } = Array.Empty<BlockerEntry>();
    }

    private sealed class QualityBandPolicy
    {
        [JsonPropertyName("hard-fail")]
        public string HardFail { get; init; } = string.Empty;

        [JsonPropertyName("known-blocked")]
        public string KnownBlocked { get; init; } = string.Empty;

        [JsonPropertyName("observability")]
        public string Observability { get; init; } = string.Empty;
    }

    private sealed class BlockerEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; init; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; init; } = string.Empty;

        [JsonPropertyName("expectedCertificates")]
        public IReadOnlyList<string> ExpectedCertificates { get; init; } = Array.Empty<string>();

        [JsonPropertyName("owningStage")]
        public int OwningStage { get; init; }
    }
}
