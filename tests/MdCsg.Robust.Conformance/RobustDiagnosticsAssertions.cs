namespace MdCsg.Robust.Conformance;

internal static class RobustDiagnosticsAssertions
{
    public static void AssertNoTriangulationDegradation(RobustDiagnostics diagnostics)
    {
        Assert.True(
            diagnostics.TriangulationLegacyFallbackCount == 0
            && diagnostics.TriangulationNativeFailureCount == 0,
            BuildTriangulationDegradationMessage(diagnostics));

        Assert.Equal(0, diagnostics.TriangulationFallbackInvalidOrCrossingConstraintCount);
        Assert.Equal(0, diagnostics.TriangulationFallbackPartitionFailureCount);
        Assert.Equal(0, diagnostics.TriangulationFallbackConstrainedEarFailureCount);
        Assert.Equal(0, diagnostics.TriangulationFallbackWorkBudgetExceededCount);
        Assert.Empty(diagnostics.TriangulationFallbackSignatures);

        Assert.Equal(0, diagnostics.TriangulationNativeFailureInvalidOrCrossingConstraintCount);
        Assert.Equal(0, diagnostics.TriangulationNativeFailurePartitionFailureCount);
        Assert.Equal(0, diagnostics.TriangulationNativeFailureConstrainedEarFailureCount);
        Assert.Equal(0, diagnostics.TriangulationNativeFailureWorkBudgetExceededCount);
        Assert.Empty(diagnostics.TriangulationNativeFailureSignatures);
        Assert.Empty(diagnostics.TriangulationNativeFailureCodes);
    }

    public static string BuildTriangulationDegradationMessage(RobustDiagnostics diagnostics)
    {
        var legacySigs = diagnostics.TriangulationFallbackSignatures.Count == 0
            ? "<none>"
            : string.Join(" | ", diagnostics.TriangulationFallbackSignatures);
        var nativeSigs = diagnostics.TriangulationNativeFailureSignatures.Count == 0
            ? "<none>"
            : string.Join(" | ", diagnostics.TriangulationNativeFailureSignatures);
        var nativeCodes = diagnostics.TriangulationNativeFailureCodes.Count == 0
            ? "<none>"
            : string.Join(" | ", diagnostics.TriangulationNativeFailureCodes);

        return $"LegacyFallback={diagnostics.TriangulationLegacyFallbackCount}, "
            + $"LegacyInvalidOrCrossing={diagnostics.TriangulationFallbackInvalidOrCrossingConstraintCount}, "
            + $"LegacyPartition={diagnostics.TriangulationFallbackPartitionFailureCount}, "
            + $"LegacyConstrainedEar={diagnostics.TriangulationFallbackConstrainedEarFailureCount}, "
            + $"LegacyWorkBudgetExceeded={diagnostics.TriangulationFallbackWorkBudgetExceededCount}, "
            + $"LegacySignatures={legacySigs}; "
            + $"NativeFailure={diagnostics.TriangulationNativeFailureCount}, "
            + $"NativeInvalidOrCrossing={diagnostics.TriangulationNativeFailureInvalidOrCrossingConstraintCount}, "
            + $"NativePartition={diagnostics.TriangulationNativeFailurePartitionFailureCount}, "
            + $"NativeConstrainedEar={diagnostics.TriangulationNativeFailureConstrainedEarFailureCount}, "
            + $"NativeWorkBudgetExceeded={diagnostics.TriangulationNativeFailureWorkBudgetExceededCount}, "
            + $"NativeSignatures={nativeSigs}, NativeCodes={nativeCodes}";
    }
}
