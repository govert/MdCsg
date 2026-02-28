$ErrorActionPreference = "Stop"

Write-Host "Running robust performance budget checks..."

$project = "tests/MdCsg.Robust.Conformance/MdCsg.Robust.Conformance.csproj"
$filter = "FullyQualifiedName~RobustPerformanceBudgetTests"
$args = @($project, "-c", "Release", "--nologo", "--blame-hang-timeout", "10m", "--filter", $filter)

& dotnet test @args
if ($LASTEXITCODE -ne 0)
{
    throw "Robust performance budget checks failed."
}

Write-Host "PERF_BUDGET_STATUS=PASS"
