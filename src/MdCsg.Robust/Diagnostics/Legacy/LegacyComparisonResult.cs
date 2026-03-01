using MdCsg.Api;

namespace MdCsg.Robust.Diagnostics.Legacy;

public sealed class LegacyComparisonResult
{
    public required RobustCsgOperation Operation { get; init; }

    public required RobustCsgResult RobustResult { get; init; }

    public required CsgResult LegacyResult { get; init; }
}
