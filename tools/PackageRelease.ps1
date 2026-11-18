$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
& cmd /c BuildInstaller.bat
if ($LASTEXITCODE -ne 0) { throw 'Release build failed' }
Compress-Archive -LiteralPath @('ApplePlayer.exe','README.md','docs') -DestinationPath 'installer-output\ApplePlayer-Portable-1.0.2.zip' -Force
$lines = foreach ($name in @('ApplePlayer-Setup-1.0.2.exe','ApplePlayer-Portable-1.0.2.zip')) {
 $hash = (Get-FileHash (Join-Path 'installer-output' $name) -Algorithm SHA256).Hash.ToLowerInvariant()
 "$hash  $name"
}
[IO.File]::WriteAllLines((Join-Path (Get-Location) 'installer-output\SHA256SUMS.txt'),[string[]]$lines,[Text.Encoding]::ASCII)
$lines
