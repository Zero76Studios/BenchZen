$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -Path (Join-Path $PSScriptRoot 'BuildPreview.cs') -ReferencedAssemblies System.Drawing
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
$measurements = [R5Preview]::Run($repoRoot)
$measurements | ConvertTo-Json -Depth 5 | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'measurements.json')
$measurements | ConvertTo-Json -Depth 5
