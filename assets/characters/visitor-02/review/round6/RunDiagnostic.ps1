$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -Path (Join-Path $PSScriptRoot 'AlignmentDiagnostic.cs') -ReferencedAssemblies System.Drawing
$repoRoot=(Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$result=[R6Diagnostic]::Run($repoRoot)
$result | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'measurements.json')
$result | ConvertTo-Json -Depth 8
