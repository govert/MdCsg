$ErrorActionPreference = "Stop"

Write-Host "Running robust conformance rescue bar..."

$project = "tests/MdCsg.Robust.Conformance/MdCsg.Robust.Conformance.csproj"
$commonArgs = @("-c", "Release", "--nologo", "--blame-hang-timeout", "10m")

Write-Host "1/3: Showcase/backlog/replay robustness gates..."
dotnet test $project @commonArgs --filter `
    "(FullyQualifiedName~RobustShowcaseParityTests|FullyQualifiedName~RobustConformanceBacklogTests|FullyQualifiedName~ArrangementReplayCorpusTests|FullyQualifiedName~TriangulationReplayCorpusTests)"

Write-Host "2/3: Seeded strict fuzz smoke gate..."
dotnet test $project @commonArgs --filter "FullyQualifiedName~RobustFuzzSmokeTests"

Write-Host "3/3: Triangulation bridge + smoke guardrails..."
dotnet test $project @commonArgs --filter "(FullyQualifiedName~RobustTriangulationBridgeTests|FullyQualifiedName~RobustCsgSmokeTests)"
