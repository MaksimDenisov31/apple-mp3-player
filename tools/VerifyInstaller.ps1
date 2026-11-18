$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
$root = (Get-Location).Path
$target = Join-Path $root ('.local\install-' + [Guid]::NewGuid().ToString('N'))
& 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe' /DInstallerTest installer\ApplePlayer.iss | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Test installer build failed' }
$setup = Join-Path $root 'installer-output\ApplePlayer-Setup-Test.exe'
function Install-Test {
 $process = Start-Process -FilePath $setup -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/DIR="' + $target + '"')) -Wait -PassThru
 if ($process.ExitCode -ne 0) { throw ('Installer exit: ' + $process.ExitCode) }
 if (-not (Test-Path (Join-Path $target 'ApplePlayer.exe'))) { throw 'Installed EXE missing' }
 if ((Get-FileHash (Join-Path $target 'ApplePlayer.exe')).Hash -ne (Get-FileHash '.\ApplePlayer.exe').Hash) { throw 'Installed EXE hash mismatch' }
}
Install-Test
Write-Output 'PASS install and binary hash'
$process = Start-Process -FilePath (Join-Path $target 'ApplePlayer.exe') -ArgumentList ('--self-test "' + (Join-Path $root '.local\tests') + '"') -PassThru
if (-not $process.WaitForExit(45000)) { throw 'Installed application test timed out' }
if ($process.ExitCode -ne 0) { throw 'Installed application functional checks failed' }
Write-Output 'PASS installed application functional checks'
$sentinel = Join-Path $target 'user-document.txt'
Set-Content $sentinel 'preserve user data'
Install-Test
if ((Get-Content $sentinel) -ne 'preserve user data') { throw 'Update changed user document' }
Write-Output 'PASS update preserves user document'
$process = Start-Process -FilePath (Join-Path $target 'unins000.exe') -ArgumentList '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART' -Wait -PassThru
if ($process.ExitCode -ne 0) { throw 'Uninstaller failed' }
if (Test-Path (Join-Path $target 'ApplePlayer.exe')) { throw 'Uninstaller left application binary' }
if (-not (Test-Path $sentinel)) { throw 'Uninstaller removed user document' }
Write-Output 'PASS uninstall removes binary and preserves user document'
