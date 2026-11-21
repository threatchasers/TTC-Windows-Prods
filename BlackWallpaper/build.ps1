# build.ps1 -- builds BlackWallpaper.exe (icon + compile) and copies it to the Desktop.
# Needs nothing installed: uses the C# compiler that ships with Windows (.NET Framework 4).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$here = $PSScriptRoot
$S = 256
$bmp = New-Object System.Drawing.Bitmap $S, $S
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.Clear([System.Drawing.Color]::Transparent)
# monitor: dark frame, left half of the screen white (wallpaper), right half black
$frame = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 60, 66, 78))
$g.FillRectangle($frame, 24, 40, 208, 142)
$g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 235, 240, 250))), 36, 52, 92, 118)
$g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::Black)), 128, 52, 92, 118)
$g.FillRectangle($frame, 110, 182, 36, 26)
$g.FillRectangle($frame, 78, 208, 100, 14)
$g.Dispose()
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
$ico = Join-Path $here 'blackwallpaper.ico'
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
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$out = Join-Path $here 'BlackWallpaper.exe'
& $csc /nologo /target:winexe "/win32icon:$ico" "/out:$out" /r:System.Windows.Forms.dll /r:System.Drawing.dll (Join-Path $here 'BlackWallpaper.cs')
if (-not (Test-Path $out)) { throw 'build failed' }
Copy-Item $out ([Environment]::GetFolderPath('Desktop')) -Force
"built: $out (copied to the Desktop)"
