namespace MdCsg.Robust;

public sealed record RobustIssue(
    RobustIssueSeverity Severity,
    RobustIssueCode Code,
    string Message);
