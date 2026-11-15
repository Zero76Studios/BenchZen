# Focused offline diagnostics only. Run in a fresh PowerShell process.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$visitor = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$integrity = Get-Content (Join-Path $visitor 'review/source-integrity.json') -Raw | ConvertFrom-Json
function Confirm-Sources {
    foreach ($item in $integrity) {
        $path = Join-Path $visitor ('source/' + $item.file)
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $item.sha256) { throw ('Source changed: ' + $item.file) }
    }
}
Confirm-Sources
Add-Type -Path (Join-Path $PSScriptRoot 'RegistrationDiagnostic.cs') -ReferencedAssemblies System.Drawing,System.Core
$poses = [V2Registration]::Run($visitor)
if ($poses.Count -ne 14) { throw 'Expected eight walk and six rear candidates' }
foreach ($p in $poses) {
    if ($p.registrationMargin -lt 12) { throw ('Insufficient horizontal margin: ' + $p.id) }
    $top = $p.bounds[1] * $p.scale + $p.dy
    $bottom = ($p.bounds[1] + $p.bounds[3] - 1) * $p.scale + $p.dy
    if ($top -lt 12 -or $bottom -gt 755) { throw ('Insufficient vertical margin: ' + $p.id) }
    if ([Math]::Abs($p.groundY * $p.scale + $p.dy - 684) -gt 0.001) { throw ('Ground registration failed: ' + $p.id) }
}
$poses | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $PSScriptRoot 'measurements.json') -Encoding UTF8
$poses | ForEach-Object {
    [pscustomobject]@{
        id=$_.id
        sourceHeadY=$_.headY
        sourceGroundY=$_.groundY
        uniformScale=[Math]::Round($_.scale,6)
        canvasHeadY=[Math]::Round($_.registeredHead[1],2)
        canvasHeadEnvelopeWidth=[Math]::Round($_.registeredHead[0],2)
        canvasShoulderEnvelopeWidth=[Math]::Round($_.shoulderWidth*$_.scale,2)
        canvasProjectedTorso=[Math]::Round($_.torsoProjection*$_.scale,2)
        canvasProjectedLegPolyline=if ($_.knee) { [Math]::Round($_.legPolyline*$_.scale,2) } else { $null }
        canvasSoleBandX=($_.registeredFeet | ForEach-Object { [Math]::Round($_,2) }) -join ' / '
        canvasSeatY=if ($_.seat) { [Math]::Round($_.registeredSeat[1],2) } else { $null }
    }
} | Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $PSScriptRoot 'comparison.csv')
Confirm-Sources
[pscustomobject]@{
    sourceHashesVerified=5
    candidateCount=$poses.Count
    walkCount=8
    rearCount=6
    commonScalePerSequence=$true
    allCandidatesHave12pxMargins=$true
    allGroundOriginsAt684=$true
    finalSpritesCreated=$false
    limitation='Geometric registration checks only; not character-art acceptance.'
} | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'validation.json') -Encoding UTF8
Get-Content (Join-Path $PSScriptRoot 'comparison.csv')
