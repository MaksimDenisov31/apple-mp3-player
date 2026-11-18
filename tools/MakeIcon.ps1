Add-Type -AssemblyName System.Drawing
$bitmap = New-Object Drawing.Bitmap 256,256
$g = [Drawing.Graphics]::FromImage($bitmap)
$g.SmoothingMode = 'AntiAlias'
$g.Clear([Drawing.Color]::FromArgb(250,53,89))
$font = New-Object Drawing.Font 'Segoe UI',148,([Drawing.FontStyle]::Bold),([Drawing.GraphicsUnit]::Pixel)
$g.DrawString([string][char]0x266B,$font,[Drawing.Brushes]::White,20,25)
$stream = New-Object IO.MemoryStream
$bitmap.Save($stream,[Drawing.Imaging.ImageFormat]::Png)
$png = $stream.ToArray()
$output = [IO.File]::Create((Join-Path $PSScriptRoot '..\assets\app.ico'))
$writer = New-Object IO.BinaryWriter $output
$writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]1)
$writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([byte]0)
$writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$png.Length); $writer.Write([uint32]22); $writer.Write($png)
$writer.Dispose(); $stream.Dispose(); $font.Dispose(); $g.Dispose(); $bitmap.Dispose()
