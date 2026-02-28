namespace MdCsg.Robust.Conformance;

public class RobustKernelDependencyGuardTests
{
    private static readonly string[] ForbiddenUsings =
    [
        "using MdCsg.Api;",
        "using MdCsg.Operations;",
        "using MdCsg.Intersection;",
        "using MdCsg.Cutting;"
    ];

    private static readonly Dictionary<string, HashSet<string>> AllowedLegacyUsingsByFile =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["ArrangementBuilder.cs"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "using MdCsg.Intersection;"
            },
            ["RobustConstrainedTriangulator.cs"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "using MdCsg.Cutting;"
            }
        };

    [Fact]
    public void KernelNamespace_DoesNotAccreteNewLegacyDependencies()
    {
        string root = GetRepositoryRoot();
        string kernelDir = Path.Combine(root, "src", "MdCsg.Robust", "Kernel");
        Assert.True(Directory.Exists(kernelDir), $"Kernel directory not found: {kernelDir}");

        var violations = new List<string>();
        foreach (var path in Directory.EnumerateFiles(kernelDir, "*.cs", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileName(path);
            var allowed = AllowedLegacyUsingsByFile.TryGetValue(fileName, out var specific)
                ? specific
                : new HashSet<string>(StringComparer.Ordinal);

            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                foreach (string forbidden in ForbiddenUsings)
                {
                    if (!string.Equals(line, forbidden, StringComparison.Ordinal))
                        continue;
                    if (allowed.Contains(forbidden))
                        continue;
                    violations.Add($"{fileName}:{i + 1}:{forbidden}");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Kernel legacy dependency guard failed:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static string GetRepositoryRoot()
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
