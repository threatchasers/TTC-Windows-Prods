Add-Type -AssemblyName System.Drawing
$dir = $PSScriptRoot

$S = 256
$bmp = New-Object System.Drawing.Bitmap $S, $S
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.Clear([System.Drawing.Color]::Transparent)
$bg = New-Object System.Drawing.Drawing2D.GraphicsPath
$r = 48
$bg.AddArc(8, 8, $r, $r, 180, 90); $bg.AddArc($S-8-$r, 8, $r, $r, 270, 90)
$bg.AddArc($S-8-$r, $S-8-$r, $r, $r, 0, 90); $bg.AddArc(8, $S-8-$r, $r, $r, 90, 90); $bg.CloseFigure()
$g.FillPath((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 24, 52, 92))), $bg)
$cup = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 255, 196, 61))
$g.FillRectangle($cup, 58, 112, 106, 86)
$g.FillPie($cup, 58, 150, 106, 70, 0, 180)
$pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 255, 196, 61)), 14
$g.DrawArc($pen, 140, 126, 62, 58, -90, 180)
$g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 220, 226, 235))), 48, 214, 126, 10)
$sp = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 235, 240, 250)), 10
$sp.StartCap = 'Round'; $sp.EndCap = 'Round'
foreach ($x in 84, 112, 140) { $g.DrawBezier($sp, $x, 100, ($x-18), 80, ($x+18), 62, $x, 38) }
$g.Dispose()

# multi-size ICO (PNG entries): 16, 32, 48, 256
$sizes = 16, 32, 48, 256
$pngs = @()
foreach ($sz in $sizes) {
  $b = New-Object System.Drawing.Bitmap $sz, $sz
  $gg = [System.Drawing.Graphics]::FromImage($b)
  $gg.InterpolationMode = 'HighQualityBicubic'; $gg.SmoothingMode = 'AntiAlias'
  $gg.DrawImage($bmp, 0, 0, $sz, $sz); $gg.Dispose()
  $ms = New-Object System.IO.MemoryStream
  $b.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
  $pngs += ,($ms.ToArray()); $b.Dispose()
}
$ico = Join-Path $dir 'keepawake.ico'
$fs = [System.IO.File]::Open($ico, 'Create'); $bw = New-Object System.IO.BinaryWriter $fs
$bw.Write([UInt16]0); $bw.Write([UInt16]1); $bw.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
  $sz = $sizes[$i]; $wb = if ($sz -ge 256) { 0 } else { $sz }
  $bw.Write([byte]$wb); $bw.Write([byte]$wb); $bw.Write([byte]0); $bw.Write([byte]0)
  $bw.Write([UInt16]1); $bw.Write([UInt16]32)
  $bw.Write([UInt32]$pngs[$i].Length); $bw.Write([UInt32]$offset)
  $offset += $pngs[$i].Length
}
foreach ($p in $pngs) { $bw.Write($p) }
$bw.Close(); $fs.Close()
"icon: " + (Get-Item $ico).Length + " bytes"

$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$out = Join-Path $PSScriptRoot 'KeepAwake.exe'
& $csc /nologo /target:winexe "/win32icon:$ico" "/out:$out" /r:System.Windows.Forms.dll /r:System.Drawing.dll (Join-Path $PSScriptRoot 'KeepAwake.cs')
if (-not (Test-Path $out)) { throw 'build failed' }
Copy-Item $out ([Environment]::GetFolderPath('Desktop')) -Force
"built: $out (copied to the Desktop)"
