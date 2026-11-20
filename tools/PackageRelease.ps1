$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
& cmd /c BuildInstaller.bat
if ($LASTEXITCODE -ne 0) { throw 'Release build failed' }
Compress-Archive -LiteralPath @('bin\Apple MP3 Player.exe','README.md','docs') -DestinationPath 'installer-output\Apple MP3 Player-Portable-1.1.1.zip' -Force
$lines = foreach ($name in @('Apple MP3 Player-Setup-1.1.1.exe','Apple MP3 Player-Portable-1.1.1.zip')) {
 $hash = (Get-FileHash (Join-Path 'installer-output' $name) -Algorithm SHA256).Hash.ToLowerInvariant()
 "$hash  $name"
}
[IO.File]::WriteAllLines((Join-Path (Get-Location) 'installer-output\SHA256SUMS.txt'),[string[]]$lines,[Text.Encoding]::ASCII)
$lines
