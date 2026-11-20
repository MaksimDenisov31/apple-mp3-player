#define AppVersion "1.1.1"
#ifdef InstallerTest
  #define RegisteredName "Apple MP3 Player.InstallerTest"
  #define AudioProgId "Apple.MP3.Player.Test.Audio"
  #define PreviousProduct "Previous Music Player Test"
#else
  #define RegisteredName "Apple MP3 Player"
  #define AudioProgId "Apple.MP3.Player.Audio"
  #define PreviousProduct "Apple" + "Player"
#endif
#define PreviousProgId PreviousProduct + ".MP3"
#define PreviousCompactExe "Apple" + "MP3" + "Player.exe"
#define CapabilitiesPath "Software\" + RegisteredName + "\Capabilities"
[Setup]
#ifdef InstallerTest
AppId=Apple MP3 Player.InstallerTest
#else
AppId={#PreviousProduct}.MaksimDenisovRussia
#endif
AppName=Apple MP3 Player
AppVersion={#AppVersion}
AppPublisher=MaksimDenisov31
AppPublisherURL=https://github.com/MaksimDenisov31/apple-mp3-player
AppSupportURL=https://github.com/MaksimDenisov31/apple-mp3-player/issues
AppUpdatesURL=https://github.com/MaksimDenisov31/apple-mp3-player/releases
DefaultDirName={localappdata}\Programs\Apple MP3 Player
UsePreviousAppDir=yes
DefaultGroupName=Apple MP3 Player
PrivilegesRequired=lowest
MinVersion=10.0
DisableProgramGroupPage=yes
DisableDirPage=no
UninstallDisplayIcon={app}\Apple MP3 Player.exe
SetupIconFile=..\assets\app.ico
OutputDir=..\installer-output
#ifdef InstallerTest
OutputBaseFilename=Apple MP3 Player-Setup-Test
#else
OutputBaseFilename=Apple MP3 Player-Setup-{#AppVersion}
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
russian.SetDefault=Сделать Apple MP3 Player плеером по умолчанию (открыть настройки Windows)
english.SetDefault=Make Apple MP3 Player the default music player (open Windows Settings)
russian.DefaultHelp=Чтобы музыка открывалась в Apple MP3 Player, оставьте отмеченным пункт ниже. В настройках Windows выберите Apple MP3 Player для MP3, WAV, M4A, AAC, WMA и FLAC и подтвердите выбор. Музыка и медиатека сохранены.
english.DefaultHelp=To open music in Apple MP3 Player, leave the option below selected. In Windows Settings, choose Apple MP3 Player for MP3, WAV, M4A, AAC, WMA and FLAC and confirm your choice. Music and library data are preserved.
[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Ярлыки:"; Flags: unchecked
[Files]
Source: "..\bin\Apple MP3 Player.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
[Icons]
#ifndef InstallerTest
Name: "{userprograms}\Apple MP3 Player"; Filename: "{app}\Apple MP3 Player.exe"
Name: "{userdesktop}\Apple MP3 Player"; Filename: "{app}\Apple MP3 Player.exe"; Tasks: desktopicon
#endif
[Registry]
Root: HKCU; Subkey: "Software\Classes\{#PreviousProgId}"; ValueType: string; ValueName: ""; ValueData: "Apple MP3 Player Audio"; Flags: uninsdeletekey; Check: HasPreviousAssociation
Root: HKCU; Subkey: "Software\Classes\{#PreviousProgId}\Application"; ValueType: string; ValueName: "ApplicationName"; ValueData: "Apple MP3 Player"; Check: HasPreviousAssociation
Root: HKCU; Subkey: "Software\Classes\{#PreviousProgId}\Application"; ValueType: string; ValueName: "ApplicationIcon"; ValueData: """{app}\Apple MP3 Player.exe"",0"; Check: HasPreviousAssociation
Root: HKCU; Subkey: "Software\Classes\{#PreviousProgId}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: """{app}\Apple MP3 Player.exe"",0"; Check: HasPreviousAssociation
Root: HKCU; Subkey: "Software\Classes\{#PreviousProgId}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\Apple MP3 Player.exe"" ""%1"""; Check: HasPreviousAssociation
Root: HKCU; Subkey: "Software\RegisteredApplications"; ValueName: "{#PreviousProduct}"; Flags: deletevalue
Root: HKCU; Subkey: "Software\{#PreviousProduct}\Capabilities"; Flags: deletekey
Root: HKCU; Subkey: "Software\Classes\{#AudioProgId}"; ValueType: string; ValueName: ""; ValueData: "Apple MP3 Player Audio"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\{#AudioProgId}\Application"; ValueType: string; ValueName: "ApplicationName"; ValueData: "Apple MP3 Player"
Root: HKCU; Subkey: "Software\Classes\{#AudioProgId}\Application"; ValueType: string; ValueName: "ApplicationIcon"; ValueData: """{app}\Apple MP3 Player.exe"",0"
Root: HKCU; Subkey: "Software\Classes\{#AudioProgId}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: """{app}\Apple MP3 Player.exe"",0"
Root: HKCU; Subkey: "Software\Classes\{#AudioProgId}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\Apple MP3 Player.exe"" ""%1"""
Root: HKCU; Subkey: "{#CapabilitiesPath}"; ValueType: string; ValueName: "ApplicationName"; ValueData: "Apple MP3 Player"; Flags: uninsdeletekey
Root: HKCU; Subkey: "{#CapabilitiesPath}"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "Локальный музыкальный плеер"
Root: HKCU; Subkey: "{#CapabilitiesPath}"; ValueType: string; ValueName: "ApplicationIcon"; ValueData: """{app}\Apple MP3 Player.exe"",0"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".mp3"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".wav"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".m4a"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".aac"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".wma"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "{#CapabilitiesPath}\FileAssociations"; ValueType: string; ValueName: ".flac"; ValueData: "{#AudioProgId}"
Root: HKCU; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "{#RegisteredName}"; ValueData: "{#CapabilitiesPath}"; Flags: uninsdeletevalue
#ifndef InstallerTest
Root: HKCU; Subkey: "Software\Classes\.mp3\OpenWithProgids"; ValueType: string; ValueName: "{#AudioProgId}"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.wav\OpenWithProgids"; ValueType: string; ValueName: "{#AudioProgId}"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.m4a\OpenWithProgids"; ValueType: string; ValueName: "{#AudioProgId}"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.aac\OpenWithProgids"; ValueType: string; ValueName: "{#AudioProgId}"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.wma\OpenWithProgids"; ValueType: string; ValueName: "{#AudioProgId}"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Classes\.flac\OpenWithProgids"; ValueType: string; ValueName: "{#AudioProgId}"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
#endif
[Run]
#ifndef InstallerTest
Filename: "{app}\Apple MP3 Player.exe"; Description: "Запустить Apple MP3 Player"; Flags: nowait postinstall skipifsilent
Filename: "ms-settings:defaultapps?registeredAppUser=Apple%20MP3%20Player"; Description: "{cm:SetDefault}"; Flags: shellexec nowait postinstall skipifsilent
#endif
[InstallDelete]
#ifndef InstallerTest
Type: files; Name: "{userprograms}\Apple Player.lnk"
Type: files; Name: "{userdesktop}\Apple Player.lnk"
Type: files; Name: "{app}\{#PreviousProduct}.exe"
Type: files; Name: "{app}\{#PreviousCompactExe}"
#endif
[Code]
function HasPreviousAssociation(): Boolean;
begin
  Result := RegKeyExists(HKCU, 'Software\Classes\{#PreviousProgId}');
end;

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
    MsgBox('Для Apple MP3 Player требуется .NET Framework 4.8. Установите его с сайта Microsoft.', mbError, MB_OK);
end;
