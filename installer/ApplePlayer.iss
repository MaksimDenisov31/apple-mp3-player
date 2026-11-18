#define AppVersion "1.0.3"
#ifdef InstallerTest
  #define RegisteredName "ApplePlayer.InstallerTest"
#else
  #define RegisteredName "ApplePlayer"
#endif
#define AudioProgId RegisteredName + ".MP3"
#define CapabilitiesPath "Software\" + RegisteredName + "\Capabilities"
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
[CustomMessages]
russian.SetDefault=Сделать Apple Player плеером по умолчанию (открыть настройки Windows)
english.SetDefault=Make Apple Player the default music player (open Windows Settings)
russian.DefaultHelp=Чтобы музыка открывалась в Apple Player, оставьте отмеченным пункт ниже. В настройках Windows выберите Apple Player для MP3, WAV, M4A, AAC, WMA и FLAC и подтвердите выбор. Музыка и медиатека сохранены.
english.DefaultHelp=To open music in Apple Player, leave the option below selected. In Windows Settings, choose Apple Player for MP3, WAV, M4A, AAC, WMA and FLAC and confirm your choice. Music and library data are preserved.
[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Ярлыки:"; Flags: unchecked
[Files]
Source: "..\bin\ApplePlayer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
[Icons]
#ifndef InstallerTest
Name: "{userprograms}\Apple Player"; Filename: "{app}\ApplePlayer.exe"
Name: "{userdesktop}\Apple Player"; Filename: "{app}\ApplePlayer.exe"; Tasks: desktopicon
#endif
[Registry]
Root: HKCU; Subkey: "Software\Classes\{#AudioProgId}"; ValueType: string; ValueName: ""; ValueData: "Apple Player Audio"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\{#AudioProgId}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: """{app}\ApplePlayer.exe"",0"
Root: HKCU; Subkey: "Software\Classes\{#AudioProgId}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\ApplePlayer.exe"" ""%1"""
Root: HKCU; Subkey: "{#CapabilitiesPath}"; ValueType: string; ValueName: "ApplicationName"; ValueData: "Apple Player"; Flags: uninsdeletekey
Root: HKCU; Subkey: "{#CapabilitiesPath}"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "Локальный музыкальный плеер"
Root: HKCU; Subkey: "{#CapabilitiesPath}"; ValueType: string; ValueName: "ApplicationIcon"; ValueData: """{app}\ApplePlayer.exe"",0"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".mp3"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".wav"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".m4a"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".aac"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".wma"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".flac"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "{#RegisteredName}"; ValueData: "{#CapabilitiesPath}"; Flags: uninsdeletevalue
#ifndef InstallerTest
Root: HKCU; Subkey: "Software\Classes\.mp3\OpenWithProgids"; ValueType: string; ValueName: "ApplePlayer.MP3"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.wav\OpenWithProgids"; ValueType: string; ValueName: "ApplePlayer.MP3"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.m4a\OpenWithProgids"; ValueType: string; ValueName: "ApplePlayer.MP3"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.aac\OpenWithProgids"; ValueType: string; ValueName: "ApplePlayer.MP3"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.wma\OpenWithProgids"; ValueType: string; ValueName: "ApplePlayer.MP3"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.flac\OpenWithProgids"; ValueType: string; ValueName: "ApplePlayer.MP3"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
#endif
[Run]
#ifndef InstallerTest
Filename: "{app}\ApplePlayer.exe"; Description: "Запустить Apple Player"; Flags: nowait postinstall skipifsilent
Filename: "ms-settings:defaultapps?registeredAppUser=ApplePlayer"; Description: "{cm:SetDefault}"; Flags: shellexec nowait postinstall skipifsilent
#endif
[Code]
procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpFinished then
    WizardForm.FinishedLabel.Caption := WizardForm.FinishedLabel.Caption + #13#10#13#10 + ExpandConstant('{cm:DefaultHelp}');
end;

function InitializeSetup(): Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 528040);
  if not Result and not WizardSilent then
    MsgBox('Для Apple Player требуется .NET Framework 4.8. Установите его с сайта Microsoft.', mbError, MB_OK);
end;
