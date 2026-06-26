# Exports logo assets from SVG sources using Inkscape (canonical geometry for UI + HTML).
$ErrorActionPreference = 'Stop'

$inkscape = 'C:\Program Files\Inkscape\bin\inkscape.exe'
if (-not (Test-Path $inkscape)) {
	throw "Inkscape not found at $inkscape"
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$assetsDir = Join-Path $repoRoot 'HDLG winforms\Assets'
$wordmarkSvg = Join-Path $assetsDir 'hdlg-logo.svg'
$appIconSvg = Join-Path $assetsDir 'hdlg-app-icon.svg'

function Export-Png {
	param(
		[string]$InputSvg,
		[string]$OutputPng,
		[int]$Width,
		[int]$Height
	)

	& $inkscape $InputSvg `
		--export-type=png `
		--export-filename=$OutputPng `
		-w $Width `
		-h $Height `
		--export-background-opacity=0 | Out-Null
}

Export-Png $wordmarkSvg (Join-Path $assetsDir 'hdlg-logo.png') 640 320
Export-Png $appIconSvg (Join-Path $assetsDir 'hdlg-icon-256.png') 256 256
Export-Png $appIconSvg (Join-Path $assetsDir 'hdlg-icon-48.png') 48 48
Export-Png $appIconSvg (Join-Path $assetsDir 'hdlg-icon-32.png') 32 32
Export-Png $appIconSvg (Join-Path $assetsDir 'hdlg-icon-16.png') 16 16

Add-Type -AssemblyName System.Drawing
$iconPath = Join-Path $assetsDir 'hdlg-icon.ico'
$sizeFiles = @(
	@{ Size = 16; Path = Join-Path $assetsDir 'hdlg-icon-16.png' },
	@{ Size = 32; Path = Join-Path $assetsDir 'hdlg-icon-32.png' },
	@{ Size = 48; Path = Join-Path $assetsDir 'hdlg-icon-48.png' },
	@{ Size = 256; Path = Join-Path $assetsDir 'hdlg-icon-256.png' }
)

$memoryStream = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter $memoryStream
$writer.Write([uint16]0)
$writer.Write([uint16]1)
$writer.Write([uint16]$sizeFiles.Count)

$pngEntries = New-Object System.Collections.Generic.List[byte[]]
foreach ($entry in $sizeFiles) {
	$pngEntries.Add([System.IO.File]::ReadAllBytes($entry.Path))
}

$offset = 6 + ($sizeFiles.Count * 16)
foreach ($index in 0..($sizeFiles.Count - 1)) {
	$size = $sizeFiles[$index].Size
	$pngBytes = $pngEntries[$index]
	$iconDimension = if ($size -ge 256) { 0 } else { $size }
	$writer.Write([byte]$iconDimension)
	$writer.Write([byte]$iconDimension)
	$writer.Write([byte]0)
	$writer.Write([byte]0)
	$writer.Write([uint16]1)
	$writer.Write([uint16]32)
	$writer.Write([uint32]$pngBytes.Length)
	$writer.Write([uint32]$offset)
	$offset += $pngBytes.Length
}

foreach ($pngBytes in $pngEntries) {
	$writer.Write($pngBytes)
}

$fileStream = [System.IO.File]::Open($iconPath, [System.IO.FileMode]::Create)
try {
	$memoryStream.Position = 0
	$memoryStream.CopyTo($fileStream)
}
finally {
	$fileStream.Close()
	$writer.Close()
	$memoryStream.Close()
}

Write-Host "Generated logo assets in $assetsDir"