$ErrorActionPreference = "Stop"

Write-Host "Running robust conformance rescue bar..."
dotnet test tests/MdCsg.Robust.Conformance/MdCsg.Robust.Conformance.csproj -c Release --nologo
