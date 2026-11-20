$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
$root = (Get-Location).Path
$target = Join-Path $root ('.local\install-' + [Guid]::NewGuid().ToString('N'))
& 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe' /DInstallerTest "installer\Apple MP3 Player.iss" | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Test installer build failed' }
$setup = Join-Path $root 'installer-output\Apple MP3 Player-Setup-Test.exe'
$extensions = @('.mp3','.wav','.m4a','.aac','.wma','.flac')
$previousDefaults = @{}
foreach ($extension in $extensions) {
 $key = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\' + $extension + '\UserChoice'
 $previousDefaults[$extension] = (Get-ItemProperty $key -ErrorAction SilentlyContinue).ProgId
}
function Install-Test {
 $process = Start-Process -FilePath $setup -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/DIR="' + $target + '"')) -Wait -PassThru
 if ($process.ExitCode -ne 0) { throw ('Installer exit: ' + $process.ExitCode) }
 if (-not (Test-Path (Join-Path $target 'Apple MP3 Player.exe'))) { throw 'Installed EXE missing' }
 if ((Get-FileHash (Join-Path $target 'Apple MP3 Player.exe')).Hash -ne (Get-FileHash '.\bin\Apple MP3 Player.exe').Hash) { throw 'Installed EXE hash mismatch' }
}
Install-Test
Write-Output 'PASS install and binary hash'
$registered = (Get-ItemProperty 'HKCU:\Software\RegisteredApplications' -ErrorAction Stop).'Apple MP3 Player.InstallerTest'
if ($registered -ne 'Software\Apple MP3 Player.InstallerTest\Capabilities') { throw 'Default apps candidate not registered' }
if ((Get-ItemProperty ('HKCU:\' + $registered)).ApplicationName -ne 'Apple MP3 Player') { throw 'Default apps display name was not renamed' }
if ((Get-ItemProperty 'HKCU:\Software\Classes\Apple.MP3.Player.Test.Audio\Application').ApplicationName -ne 'Apple MP3 Player') { throw 'Open With display name was not renamed' }
Write-Output 'PASS renamed default app and Open With labels; compatibility identifiers retained'
foreach ($extension in $extensions) {
 $progId = (Get-ItemProperty ('HKCU:\' + $registered + '\FileAssociations') -ErrorAction Stop).$extension
 if ($progId -ne 'Apple.MP3.Player.Test.Audio') { throw ('Missing format registration: ' + $extension) }
 $key = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\' + $extension + '\UserChoice'
 if ((Get-ItemProperty $key -ErrorAction SilentlyContinue).ProgId -ne $previousDefaults[$extension]) { throw 'Installer changed protected default choice' }
}
$command = (Get-Item 'HKCU:\Software\Classes\Apple.MP3.Player.Test.Audio\shell\open\command').GetValue('')
if ($command -ne ('"' + (Join-Path $target 'Apple MP3 Player.exe') + '" "%1"')) { throw 'Audio open command is not correctly quoted' }
Write-Output 'PASS registration for six music formats and quoted open command; existing defaults preserved'
$process = Start-Process -FilePath (Join-Path $target 'Apple MP3 Player.exe') -ArgumentList ('--self-test "' + (Join-Path $root '.local\tests') + '"') -PassThru
if (-not $process.WaitForExit(45000)) { throw 'Installed application test timed out' }
if ($process.ExitCode -ne 0) { throw 'Installed application functional checks failed' }
Write-Output 'PASS installed application functional checks'
$sentinel = Join-Path $target 'user-document.txt'
Set-Content $sentinel 'preserve user data'
New-Item -Path 'HKCU:\Software\Classes\Previous Music Player Test.MP3\shell\open\command' -Force | Out-Null
Set-Item -Path 'HKCU:\Software\Classes\Previous Music Player Test.MP3\shell\open\command' -Value 'previous command'
New-ItemProperty -Path 'HKCU:\Software\RegisteredApplications' -Name 'Previous Music Player Test' -Value 'Software\Previous Music Player Test\Capabilities' -PropertyType String -Force | Out-Null
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Apple MP3 Player.InstallerTest_is1'
Set-ItemProperty $uninstallKey -Name DisplayName -Value 'Apple Player'
Set-ItemProperty ('HKCU:\' + $registered) -Name ApplicationName -Value 'Apple Player'
Install-Test
if ((Get-Content $sentinel) -ne 'preserve user data') { throw 'Update changed user document' }
if ((Get-ItemProperty $uninstallKey).DisplayName -notlike 'Apple MP3 Player*') { throw 'Installed app name was not updated' }
if ((Get-ItemProperty ('HKCU:\' + $registered)).ApplicationName -ne 'Apple MP3 Player') { throw 'Upgrade did not replace legacy display name' }
Write-Output 'PASS upgrade replaces legacy display names without moving installation or user document'
$legacyCommand=(Get-Item 'HKCU:\Software\Classes\Previous Music Player Test.MP3\shell\open\command').GetValue('')
if ($legacyCommand -ne ('"' + (Join-Path $target 'Apple MP3 Player.exe') + '" "%1"')) { throw 'Existing default association was not redirected to renamed EXE' }
if ((Get-ItemProperty 'HKCU:\Software\RegisteredApplications').'Previous Music Player Test') { throw 'Legacy default apps label remains' }
Write-Output 'PASS legacy association redirected and obsolete default apps label removed'
Write-Output 'PASS update preserves user document'
$process = Start-Process -FilePath (Join-Path $target 'unins000.exe') -ArgumentList '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART' -Wait -PassThru
if ($process.ExitCode -ne 0) { throw 'Uninstaller failed' }
if (Test-Path (Join-Path $target 'Apple MP3 Player.exe')) { throw 'Uninstaller left application binary' }
if (-not (Test-Path $sentinel)) { throw 'Uninstaller removed user document' }
Write-Output 'PASS uninstall removes binary and preserves user document'
if (Test-Path 'HKCU:\Software\Apple MP3 Player.InstallerTest\Capabilities') { throw 'Uninstaller left capability registration' }
if (Test-Path 'HKCU:\Software\Classes\Apple.MP3.Player.Test.Audio') { throw 'Uninstaller left audio ProgID' }
if ((Get-ItemProperty 'HKCU:\Software\RegisteredApplications' -ErrorAction SilentlyContinue).'Apple MP3 Player.InstallerTest') { throw 'Uninstaller left default app candidate' }
Write-Output 'PASS uninstall cleans isolated default app registrations'

if (Test-Path 'HKCU:\Software\Classes\Previous Music Player Test.MP3') { throw 'Uninstaller left compatibility association' }
