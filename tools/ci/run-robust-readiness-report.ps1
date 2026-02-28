$ErrorActionPreference = "Stop"

Write-Host "Running strict robust readiness snapshot..."

$project = "tests/MdCsg.Robust.Conformance/MdCsg.Robust.Conformance.csproj"
$filter = "FullyQualifiedName~RobustReadinessSnapshotTests"
$args = @($project, "-c", "Release", "--nologo", "--blame-hang-timeout", "10m", "--filter", $filter)

& dotnet test @args
if ($LASTEXITCODE -ne 0)
{
    throw "Strict readiness snapshot failed."
}

Write-Host "READINESS_STATUS=BLOCKED"
Write-Host "KNOWN_BLOCKER=chained-step3-reconstruction-fail-closed"
Write-Host "STRICT_STABLE_CORPUS=PASS"
Write-Host "TRIANGULATION_DEBT=NONE"
