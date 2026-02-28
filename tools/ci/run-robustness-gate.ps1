$ErrorActionPreference = "Stop"

Write-Host "Running robust conformance rescue bar..."

$project = "tests/MdCsg.Robust.Conformance/MdCsg.Robust.Conformance.csproj"
$commonArgs = @("-c", "Release", "--nologo", "--blame-hang-timeout", "10m")

function TestHostCrashDetected([string]$output)
{
    return $output.Contains("Test Run Aborted", [System.StringComparison]::OrdinalIgnoreCase) `
        -or $output.Contains("Fatal error", [System.StringComparison]::OrdinalIgnoreCase) `
        -or $output.Contains("Internal CLR error", [System.StringComparison]::OrdinalIgnoreCase)
}

function Invoke-GateSlice([string]$label, [string]$filter)
{
    Write-Host $label

    $args = @($project) + $commonArgs + @("--filter", $filter)
    $output = (& dotnet test @args 2>&1 | Tee-Object -Variable captured)
    $exitCode = $LASTEXITCODE
    $joined = ($captured | ForEach-Object { $_.ToString() }) -join "`n"
    $crashed = TestHostCrashDetected $joined

    if ($exitCode -eq 0 -and -not $crashed)
    {
        return
    }

    if (-not $crashed)
    {
        throw "Gate slice failed: $label (exit $exitCode)."
    }

    Write-Warning "Detected test-host crash/abort in '$label'. Re-running once to confirm stability..."

    & dotnet test @args
    $retryExit = $LASTEXITCODE
    if ($retryExit -ne 0)
    {
        throw "Gate slice failed after crash retry: $label (exit $retryExit)."
    }

    Write-Warning "Crash was transient for '$label'; retry passed."
}

Invoke-GateSlice `
    "1/3: Showcase/backlog/replay robustness gates..." `
    "(FullyQualifiedName~RobustShowcaseParityTests|FullyQualifiedName~RobustConformanceBacklogTests|FullyQualifiedName~ArrangementReplayCorpusTests|FullyQualifiedName~TriangulationReplayCorpusTests)"

Invoke-GateSlice `
    "2/3: Seeded strict fuzz smoke gate..." `
    "FullyQualifiedName~RobustFuzzSmokeTests"

Invoke-GateSlice `
    "3/3: Triangulation bridge + smoke guardrails..." `
    "(FullyQualifiedName~RobustTriangulationBridgeTests|FullyQualifiedName~RobustCsgSmokeTests|FullyQualifiedName~ReconstructionIncidenceTests|FullyQualifiedName~RobustAlgebraicConformanceTests)"
