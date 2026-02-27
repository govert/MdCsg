using MdCsg.Api;

namespace MdCsg.Robust;

public sealed class RobustCsgResult
{
    public RobustCsgResult(
        CsgResult? result,
        IReadOnlyList<RobustIssue> issues,
        RobustDiagnostics diagnostics)
    {
        Result = result;
        Issues = issues;
        Diagnostics = diagnostics;
    }

    public CsgResult? Result { get; }

    public IReadOnlyList<RobustIssue> Issues { get; }

    public RobustDiagnostics Diagnostics { get; }

    public bool HasErrors => Issues.Any(i => i.Severity == RobustIssueSeverity.Error);

    public bool Succeeded => Result != null && !HasErrors;
}
