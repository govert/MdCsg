using System;

namespace MdCsg.Showcase;

internal sealed class ShowcaseCliOptions
{
    private ShowcaseCliOptions(
        bool screenshotMode,
        string? screenshotDir,
        bool useLegacyCsg,
        bool allowLegacyFailover,
        bool strictRobustOnly)
    {
        ScreenshotMode = screenshotMode;
        ScreenshotDir = screenshotDir;
        UseLegacyCsg = useLegacyCsg;
        AllowLegacyFailover = allowLegacyFailover;
        StrictRobustOnly = strictRobustOnly;
    }

    public bool ScreenshotMode { get; }

    public string? ScreenshotDir { get; }

    public bool UseLegacyCsg { get; }

    public bool AllowLegacyFailover { get; }

    public bool StrictRobustOnly { get; }

    public bool UseClosureAttemptRobust => !StrictRobustOnly;

    public string ModeSummary
    {
        get
        {
            if (UseLegacyCsg)
                return "[showcase] CSG mode: legacy (explicit opt-out)";

            if (AllowLegacyFailover)
            {
                return $"[showcase] CSG mode: robust-strict (closureAttempt={(UseClosureAttemptRobust ? 1 : 0)}) with explicit legacy failover";
            }

            return $"[showcase] CSG mode: robust-strict (closureAttempt={(UseClosureAttemptRobust ? 1 : 0)}, no automatic fallback)";
        }
    }

    public static ShowcaseCliOptions Parse(string[] args)
    {
        bool screenshotMode = false;
        string? screenshotDir = null;
        bool useLegacyCsg = false;
        bool allowLegacyFailover = false;
        bool strictRobustOnly = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (string.Equals(arg, "--screenshot", StringComparison.OrdinalIgnoreCase))
            {
                screenshotMode = true;
                if (i + 1 < args.Length && !IsKnownFlag(args[i + 1]))
                    screenshotDir = args[++i];
                continue;
            }

            if (string.Equals(arg, "--legacy-csg", StringComparison.OrdinalIgnoreCase))
            {
                useLegacyCsg = true;
                continue;
            }

            if (string.Equals(arg, "--allow-legacy-failover", StringComparison.OrdinalIgnoreCase))
            {
                allowLegacyFailover = true;
                continue;
            }

            if (string.Equals(arg, "--strict-robust-only", StringComparison.OrdinalIgnoreCase))
            {
                strictRobustOnly = true;
                continue;
            }

            throw new ArgumentException($"Unknown showcase argument '{arg}'.", nameof(args));
        }

        if (useLegacyCsg && allowLegacyFailover)
        {
            throw new ArgumentException(
                "Cannot combine --legacy-csg with --allow-legacy-failover.",
                nameof(args));
        }

        if (useLegacyCsg && strictRobustOnly)
        {
            throw new ArgumentException(
                "Cannot combine --legacy-csg with --strict-robust-only.",
                nameof(args));
        }

        return new ShowcaseCliOptions(
            screenshotMode,
            screenshotDir,
            useLegacyCsg,
            allowLegacyFailover,
            strictRobustOnly);
    }

    private static bool IsKnownFlag(string value)
        => string.Equals(value, "--screenshot", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "--legacy-csg", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "--allow-legacy-failover", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "--strict-robust-only", StringComparison.OrdinalIgnoreCase);
}
