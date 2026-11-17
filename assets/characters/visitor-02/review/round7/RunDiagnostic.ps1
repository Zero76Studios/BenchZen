$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -Path (Join-Path $PSScriptRoot 'PerspectiveDiagnostic.cs') -ReferencedAssemblies System.Drawing
$repoRoot=(Resolve-Path (Join-Path $PSScriptRoot '../../../../..')).Path
[R7Perspective]::Run($repoRoot)
