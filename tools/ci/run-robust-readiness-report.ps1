$ErrorActionPreference = "Stop"

Write-Host "Running strict robust readiness snapshot..."

$project = "tests/MdCsg.Robust.Conformance/MdCsg.Robust.Conformance.csproj"
$filter = "(FullyQualifiedName~RobustReadinessSnapshotTests|FullyQualifiedName~RobustBlockerLedgerTests|FullyQualifiedName~ChainedCsgSceneCase_Step3_ClosureAttempt_RemainsPinnedFailClosedBlocker)"
$args = @($project, "-c", "Release", "--nologo", "--blame-hang-timeout", "25m", "--filter", $filter)
$ledgerPath = "tools/ci/robust-blocker-ledger.json"

& dotnet test @args
if ($LASTEXITCODE -ne 0)
{
    throw "Strict readiness snapshot failed."
}

Write-Host "READINESS_STATUS=BLOCKED"
if (-not (Test-Path $ledgerPath))
{
    throw "Missing blocker ledger: $ledgerPath"
}

$ledger = Get-Content $ledgerPath -Raw | ConvertFrom-Json
$knownBlocked = @($ledger.blockers | ForEach-Object { $_.id }) -join ","
$knownBlockedDetail = @($ledger.blockers | ForEach-Object { "$($_.id)@stage$($_.owningStage)" }) -join ","
$knownSignatures = @($ledger.blockers | ForEach-Object { $_.signature }) -join ","
if ([string]::IsNullOrWhiteSpace($knownBlocked))
{
    $knownBlocked = "none"
}
if ([string]::IsNullOrWhiteSpace($knownBlockedDetail))
{
    $knownBlockedDetail = "none"
}
if ([string]::IsNullOrWhiteSpace($knownSignatures))
{
    $knownSignatures = "none"
}

Write-Host "KNOWN_BLOCKER=$knownBlocked"
Write-Host "KNOWN_BLOCKER_DETAIL=$knownBlockedDetail"
Write-Host "KNOWN_BLOCKER_SIGNATURES=$knownSignatures"
Write-Host "STRICT_STABLE_CORPUS=PASS"
Write-Host "TRIANGULATION_DEBT=NONE"
Write-Host "READINESS_DEG_PRUNE_CONTRACT=PASS"
Write-Host "READINESS_CLOSURE_ATTEMPT_CONTRACT=PASS"
Write-Host "READINESS_BAND_HARD_FAIL=PASS"
Write-Host "READINESS_BAND_KNOWN_BLOCKED=$knownBlockedDetail"
Write-Host "READINESS_BAND_OBSERVABILITY=PASS"
