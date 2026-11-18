#define AppVersion "1.0.1"
[Setup]
#ifdef InstallerTest
AppId=ApplePlayer.InstallerTest
#else
AppId=ApplePlayer.MaksimDenisovRussia
#endif
AppName=Apple Player
AppVersion={#AppVersion}
AppPublisher=MaksimDenisovRussia
AppPublisherURL=https://github.com/MaksimDenisovRussia/apple-player
AppSupportURL=https://github.com/MaksimDenisovRussia/apple-player/issues
AppUpdatesURL=https://github.com/MaksimDenisovRussia/apple-player/releases
DefaultDirName={localappdata}\Programs\Apple Player
DefaultGroupName=Apple Player
PrivilegesRequired=lowest
MinVersion=10.0
DisableProgramGroupPage=yes
DisableDirPage=no
UninstallDisplayIcon={app}\ApplePlayer.exe
SetupIconFile=..\assets\app.ico
OutputDir=..\installer-output
#ifdef InstallerTest
OutputBaseFilename=ApplePlayer-Setup-Test
#else
OutputBaseFilename=ApplePlayer-Setup-{#AppVersion}
#endif
VersionInfoVersion={#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=no
RestartApplications=no
ChangesAssociations=yes
[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Ярлыки:"; Flags: unchecked
[Files]
Source: "..\ApplePlayer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
[Icons]
#ifndef InstallerTest
Name: "{userprograms}\Apple Player"; Filename: "{app}\ApplePlayer.exe"
Name: "{userdesktop}\Apple Player"; Filename: "{app}\ApplePlayer.exe"; Tasks: desktopicon
#endif
[Registry]
#ifndef InstallerTest
Root: HKCU; Subkey: "Software\Classes\ApplePlayer.MP3"; ValueType: string; ValueName: ""; ValueData: "Apple Player MP3"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\ApplePlayer.MP3\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: """{app}\ApplePlayer.exe"",0"
Root: HKCU; Subkey: "Software\Classes\ApplePlayer.MP3\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\ApplePlayer.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.mp3\OpenWithProgids"; ValueType: string; ValueName: "ApplePlayer.MP3"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\ApplePlayer\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "Apple Player"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\ApplePlayer\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "Локальный MP3-плеер"
Root: HKCU; Subkey: "Software\ApplePlayer\Capabilities\FileAssociations"; ValueType: string; ValueName: ".mp3"; ValueData: "ApplePlayer.MP3"
Root: HKCU; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "ApplePlayer"; ValueData: "Software\ApplePlayer\Capabilities"; Flags: uninsdeletevalue
#endif
[Run]
#ifndef InstallerTest
Filename: "{app}\ApplePlayer.exe"; Description: "Запустить Apple Player"; Flags: nowait postinstall skipifsilent
#endif
[Code]
function InitializeSetup(): Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 528040);
  if not Result and not WizardSilent then
    MsgBox('Для Apple Player требуется .NET Framework 4.8. Установите его с сайта Microsoft.', mbError, MB_OK);
end;
