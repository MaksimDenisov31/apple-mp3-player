param([string]$FFmpeg)
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
if (-not $FFmpeg) {
    $command = Get-Command ffmpeg -ErrorAction SilentlyContinue
    if ($command) { $FFmpeg = $command.Source }
    else { $FFmpeg = (Get-ChildItem '.local\packages\imageio_ffmpeg\binaries\ffmpeg*.exe' -ErrorAction SilentlyContinue | Select-Object -First 1).FullName }
}
if (-not $FFmpeg) { throw 'Укажите -FFmpeg с путём к ffmpeg.exe' }
& cmd /c Build.bat
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
New-Item -ItemType Directory -Force .local\tests | Out-Null
& $FFmpeg -hide_banner -loglevel error -y -f lavfi -i 'color=c=0xf95380:s=600x600' -frames:v 1 .local\tests\cover.jpg
if ($LASTEXITCODE -ne 0) { throw 'Cover generation failed' }
& $FFmpeg -hide_banner -loglevel error -y -f lavfi -i 'sine=frequency=440:duration=8' -i .local\tests\cover.jpg -map 0:a -map 1:v -c:a libmp3lame -b:a 128k -c:v copy -id3v2_version 3 -metadata title='Тестовый трек' -metadata artist='Apple Player Test' -metadata album='Проверка' -metadata:s:v title='Album cover' -metadata:s:v comment='Cover (front)' .local\tests\sample.mp3
if ($LASTEXITCODE -ne 0) { throw 'MP3 generation failed' }
foreach ($extension in @('wav','m4a','aac','wma','flac')) {
 & $FFmpeg -hide_banner -loglevel error -y -f lavfi -i 'sine=frequency=440:duration=3' ('.local\tests\sample.' + $extension)
 if ($LASTEXITCODE -ne 0) { throw ('Audio generation failed: ' + $extension) }
}
$process = Start-Process -FilePath .\bin\AppleMP3Player.exe -ArgumentList '--self-test .local/tests' -PassThru
if (-not $process.WaitForExit(45000)) { throw 'Functional checks timed out' }
Get-Content .local\tests\results.txt -Encoding UTF8
if ($process.ExitCode -ne 0) { throw 'Functional checks failed' }
