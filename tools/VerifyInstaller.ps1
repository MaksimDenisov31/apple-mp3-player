$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
$root = (Get-Location).Path
$target = Join-Path $root ('.local\install-' + [Guid]::NewGuid().ToString('N'))
& 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe' /DInstallerTest installer\ApplePlayer.iss | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Test installer build failed' }
$setup = Join-Path $root 'installer-output\ApplePlayer-Setup-Test.exe'
$extensions = @('.mp3','.wav','.m4a','.aac','.wma','.flac')
$previousDefaults = @{}
foreach ($extension in $extensions) {
 $key = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\' + $extension + '\UserChoice'
 $previousDefaults[$extension] = (Get-ItemProperty $key -ErrorAction SilentlyContinue).ProgId
}
function Install-Test {
 $process = Start-Process -FilePath $setup -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/DIR="' + $target + '"')) -Wait -PassThru
 if ($process.ExitCode -ne 0) { throw ('Installer exit: ' + $process.ExitCode) }
 if (-not (Test-Path (Join-Path $target 'ApplePlayer.exe'))) { throw 'Installed EXE missing' }
 if ((Get-FileHash (Join-Path $target 'ApplePlayer.exe')).Hash -ne (Get-FileHash '.\bin\ApplePlayer.exe').Hash) { throw 'Installed EXE hash mismatch' }
}
Install-Test
Write-Output 'PASS install and binary hash'
$registered = (Get-ItemProperty 'HKCU:\Software\RegisteredApplications' -ErrorAction Stop).'ApplePlayer.InstallerTest'
if ($registered -ne 'Software\ApplePlayer.InstallerTest\Capabilities') { throw 'Default apps candidate not registered' }
foreach ($extension in $extensions) {
 $progId = (Get-ItemProperty ('HKCU:\' + $registered + '\FileAssociations') -ErrorAction Stop).$extension
 if ($progId -ne 'ApplePlayer.InstallerTest.MP3') { throw ('Missing format registration: ' + $extension) }
 $key = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\' + $extension + '\UserChoice'
 if ((Get-ItemProperty $key -ErrorAction SilentlyContinue).ProgId -ne $previousDefaults[$extension]) { throw 'Installer changed protected default choice' }
}
$command = (Get-Item 'HKCU:\Software\Classes\ApplePlayer.InstallerTest.MP3\shell\open\command').GetValue('')
if ($command -ne ('"' + (Join-Path $target 'ApplePlayer.exe') + '" "%1"')) { throw 'Audio open command is not correctly quoted' }
Write-Output 'PASS registration for six music formats and quoted open command; existing defaults preserved'
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
if (Test-Path 'HKCU:\Software\ApplePlayer.InstallerTest\Capabilities') { throw 'Uninstaller left capability registration' }
if (Test-Path 'HKCU:\Software\Classes\ApplePlayer.InstallerTest.MP3') { throw 'Uninstaller left audio ProgID' }
if ((Get-ItemProperty 'HKCU:\Software\RegisteredApplications' -ErrorAction SilentlyContinue).'ApplePlayer.InstallerTest') { throw 'Uninstaller left default app candidate' }
Write-Output 'PASS uninstall cleans isolated default app registrations'
