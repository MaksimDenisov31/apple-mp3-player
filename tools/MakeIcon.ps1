$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName WindowsBase,PresentationCore,PresentationFramework
$assets = Join-Path $PSScriptRoot '..\assets'
function New-IconFrame([int]$size) {
 $visual = New-Object Windows.Media.DrawingVisual
 $drawing = $visual.RenderOpen()
 $drawing.PushTransform((New-Object Windows.Media.ScaleTransform ($size / 256.0),($size / 256.0)))
 $drawing.DrawRoundedRectangle([Windows.Media.SolidColorBrush]::new([Windows.Media.ColorConverter]::ConvertFromString('#180B1024')), $null, [Windows.Rect]::new(15,19,226,226), 52,52)
 $tile = New-Object Windows.Media.LinearGradientBrush
 $tile.StartPoint = [Windows.Point]::new(0,0); $tile.EndPoint = [Windows.Point]::new(1,1)
 $tile.GradientStops.Add([Windows.Media.GradientStop]::new([Windows.Media.ColorConverter]::ConvertFromString('#FFFFFF'),0))
 $tile.GradientStops.Add([Windows.Media.GradientStop]::new([Windows.Media.ColorConverter]::ConvertFromString('#EAECF5'),1))
 $outline = [Windows.Media.Pen]::new([Windows.Media.SolidColorBrush]::new([Windows.Media.ColorConverter]::ConvertFromString('#DADDE9')),1.5)
 $drawing.DrawRoundedRectangle($tile,$outline,[Windows.Rect]::new(14,12,228,228),52,52)
 $note = New-Object Windows.Media.LinearGradientBrush
 $note.StartPoint = [Windows.Point]::new(0,0); $note.EndPoint = [Windows.Point]::new(1,1)
 $note.GradientStops.Add([Windows.Media.GradientStop]::new([Windows.Media.ColorConverter]::ConvertFromString('#7B5CE1'),0))
 $note.GradientStops.Add([Windows.Media.GradientStop]::new([Windows.Media.ColorConverter]::ConvertFromString('#CA66B0'),1))
 $geometry = New-Object Windows.Media.GeometryGroup
 $geometry.FillRule = 'Nonzero'
 $geometry.Children.Add([Windows.Media.Geometry]::Parse('M 106,87 L 183,65 L 183,92 L 106,114 Z'))
 $geometry.Children.Add([Windows.Media.RectangleGeometry]::new([Windows.Rect]::new(106,94,11,83),3,3))
 $geometry.Children.Add([Windows.Media.RectangleGeometry]::new([Windows.Rect]::new(172,80,11,76),3,3))
 $geometry.Children.Add([Windows.Media.EllipseGeometry]::new([Windows.Point]::new(98,178),19,14))
 $geometry.Children.Add([Windows.Media.EllipseGeometry]::new([Windows.Point]::new(164,157),19,14))
 $drawing.DrawGeometry($note,$null,$geometry)
 $drawing.Pop(); $drawing.Close()
 $bitmap = [Windows.Media.Imaging.RenderTargetBitmap]::new($size,$size,96,96,[Windows.Media.PixelFormats]::Pbgra32)
 $bitmap.Render($visual)
 $encoder = New-Object Windows.Media.Imaging.PngBitmapEncoder
 $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
 $stream = New-Object IO.MemoryStream
 $encoder.Save($stream)
 $bytes = $stream.ToArray(); $stream.Dispose()
 return ,$bytes
}
$sizes = @(16,20,24,32,40,48,64,128,256)
$frames = @($sizes | ForEach-Object { ,(New-IconFrame $_) })
$stream = [IO.File]::Create((Join-Path $assets 'app.ico'))
$writer = [IO.BinaryWriter]::new($stream)
try {
 $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$sizes.Count)
 $offset = 6 + 16 * $sizes.Count
 for ($i=0; $i -lt $sizes.Count; $i++) {
  $dimension = if ($sizes[$i] -eq 256) { 0 } else { $sizes[$i] }
  $writer.Write([byte]$dimension); $writer.Write([byte]$dimension); $writer.Write([byte]0); $writer.Write([byte]0)
  $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$frames[$i].Length); $writer.Write([uint32]$offset)
  $offset += $frames[$i].Length
 }
 foreach ($frame in $frames) { $writer.Write([byte[]]$frame) }
} finally { $writer.Dispose() }
[IO.File]::WriteAllBytes((Join-Path $assets 'app.png'),[byte[]]$frames[-1])
Write-Output ('Created ICO: ' + ($sizes -join ', ') + ' px')
