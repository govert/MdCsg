$ErrorActionPreference = "Stop"

Write-Host "Running robust conformance rescue bar..."

$robustProject = "tests/MdCsg.Robust.Conformance/MdCsg.Robust.Conformance.csproj"
$showcaseContractProject = "tests/MdCsg.Showcase.ContractTests/MdCsg.Showcase.ContractTests.csproj"
$ledgerPath = "tools/ci/robust-blocker-ledger.json"
$commonArgs = @("-c", "Release", "--nologo", "--blame-hang-timeout", "25m")

function TestHostCrashDetected([string]$output)
{
    return $output.IndexOf("Test Run Aborted", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 `
        -or $output.IndexOf("Fatal error", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 `
        -or $output.IndexOf("Internal CLR error", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

function Invoke-GateSlice([string]$label, [string]$projectPath, [string]$filter)
{
    Write-Host $label

    $args = @($projectPath) + $commonArgs + @("--filter", $filter)
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
    "1/4: Showcase/backlog/replay + deg-prune + closure-attempt contract robustness gates..." `
    $robustProject `
    "(FullyQualifiedName~RobustShowcaseParityTests|FullyQualifiedName~RobustConformanceBacklogTests|FullyQualifiedName~ArrangementReplayCorpusTests|FullyQualifiedName~TriangulationReplayCorpusTests|FullyQualifiedName~ReconstructionReplayCorpusTests|FullyQualifiedName~KnownBlockerCorpus_IsExplicitlyFailClosed|FullyQualifiedName~RobustBlockerLedgerTests)"

Invoke-GateSlice `
    "2/4: Seeded strict fuzz smoke gate..." `
    $robustProject `
    "(FullyQualifiedName~RobustFuzzSmokeTests|FullyQualifiedName~RobustFuzzEscalationTests)"

Invoke-GateSlice `
    "3/4: Triangulation bridge + smoke + reconstruction/algebraic/differential/dependency/shadow/readiness/budget guardrails..." `
    $robustProject `
    "(FullyQualifiedName~RobustTriangulationBridgeTests|FullyQualifiedName~RobustCsgSmokeTests|FullyQualifiedName~ReconstructionIncidenceTests|FullyQualifiedName~RobustAlgebraicConformanceTests|FullyQualifiedName~RobustDifferentialParityTests|FullyQualifiedName~RobustKernelDependencyGuardTests|FullyQualifiedName~RobustShadowRolloutTests|FullyQualifiedName~RobustReadinessSnapshotTests|FullyQualifiedName~RobustPerformanceBudgetTests|FullyQualifiedName~RobustApiContractSnapshotTests|FullyQualifiedName~RobustMigrationDocTests)"

Invoke-GateSlice `
    "4/4: Showcase runtime strict/failover contract gate..." `
    $showcaseContractProject `
    "FullyQualifiedName~ShowcaseCsgContractTests"

if (-not (Test-Path $ledgerPath))
{
    throw "Missing blocker ledger: $ledgerPath"
}

$ledger = Get-Content $ledgerPath -Raw | ConvertFrom-Json
$knownBlocked = @($ledger.blockers | ForEach-Object { $_.id }) -join ","
$knownSignatures = @($ledger.blockers | ForEach-Object { $_.signature }) -join ","
if ([string]::IsNullOrWhiteSpace($knownBlocked))
{
    $knownBlocked = "none"
}
if ([string]::IsNullOrWhiteSpace($knownSignatures))
{
    $knownSignatures = "none"
}

Write-Host "ROBUST_GATE_BAND_HARD_FAIL=PASS"
Write-Host "ROBUST_GATE_BAND_KNOWN_BLOCKED=$knownBlocked"
Write-Host "ROBUST_GATE_BAND_OBSERVABILITY=PASS"
Write-Host "ROBUST_GATE_DEG_PRUNE_CONTRACT=PASS"
Write-Host "ROBUST_GATE_COLLINEAR_GUARD_CONTRACT=PASS"
Write-Host "ROBUST_GATE_LOCAL_RETRIANGULATION_CONTRACT=PASS"
Write-Host "ROBUST_GATE_SHOWCASE_CLOSURE_MODE_CONTRACT=PASS"
Write-Host "ROBUST_GATE_SHOWCASE_ISSUE_SUMMARY_CONTRACT=PASS"
Write-Host "ROBUST_GATE_KNOWN_BLOCKER_SIGNATURES=$knownSignatures"
