Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = Split-Path $PSScriptRoot -Parent
$source = Join-Path $root 'source'
$zip = [IO.Compression.ZipFile]::OpenRead('C:\Users\strid\Downloads\benchzen_character_art_handoff.zip')
$sha = [Security.Cryptography.SHA256]::Create()
$records = foreach ($file in Get-ChildItem -LiteralPath $source -Filter *.png | Sort-Object Name) {
    $entry = $zip.GetEntry('benchzen_art_handoff/' + $file.Name)
    $stream = $entry.Open()
    $archiveHash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','')
    $stream.Dispose()
    $fileHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    if ($archiveHash -ne $fileHash) { throw ('Source mismatch: ' + $file.Name) }
    $bmp = [Drawing.Bitmap]::new($file.FullName)
    $zero = 0; $partial = 0; $opaque = 0
    for ($y = 0; $y -lt $bmp.Height; $y++) {
        for ($x = 0; $x -lt $bmp.Width; $x++) {
            $a = $bmp.GetPixel($x,$y).A
            if ($a -eq 0) { $zero++ } elseif ($a -eq 255) { $opaque++ } else { $partial++ }
        }
    }
    [pscustomobject]@{file=$file.Name; width=$bmp.Width; height=$bmp.Height; pixelFormat=$bmp.PixelFormat.ToString(); transparentPixels=$zero; partialAlphaPixels=$partial; opaquePixels=$opaque; sha256=$fileHash; matchesArchive=$true}
    $bmp.Dispose()
}
$records | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'source-integrity.json') -Encoding UTF8
$zip.Dispose(); $sha.Dispose()
$sheet = [Drawing.Bitmap]::new(1200,1830)
$g = [Drawing.Graphics]::FromImage($sheet)
$g.Clear([Drawing.Color]::FromArgb(210,213,218))
$g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$font = [Drawing.Font]::new('Arial',16)
$small = [Drawing.Font]::new('Arial',12)
$g.DrawString('VISITOR-02 / RAW SOURCE AUDIT', $font,[Drawing.Brushes]::Black,16,10)
$g.DrawString('Scaled over grey; opaque backgrounds remain visible. Originals unchanged. Pose order: left to right.', $small,[Drawing.Brushes]::Black,16,40)
$items = @(
    @('approved-master-character.png',16,80,560,374),
    @('original-reference.png',610,80,574,287),
    @('walk-cycle-candidate.png',16,510,1168,389),
    @('rear-sit-motion-reference.png',16,950,1168,389),
    @('side-sit-motion-reference.png',16,1390,1168,389)
)
foreach ($item in $items) {
    $g.DrawString($item[0],$small,[Drawing.Brushes]::Black,[single]$item[1],[single]($item[2]-24))
    $bmp = [Drawing.Bitmap]::new((Join-Path $source $item[0]))
    $g.DrawImage($bmp,[int]$item[1],[int]$item[2],[int]$item[3],[int]$item[4])
    $bmp.Dispose()
}
$sheet.Save((Join-Path $PSScriptRoot 'contact-sheet.png'),[Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $sheet.Dispose(); $font.Dispose(); $small.Dispose()
$records | Format-Table file,width,height,transparentPixels,partialAlphaPixels,opaquePixels,matchesArchive -AutoSize
