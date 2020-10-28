[Setup]
AppName=WatchHistory
AppId=WatchHistory
AppVerName=WatchHistory 1.4.5.1
AppCopyright=Copyright © Doena Soft. 2016 - 2020
AppPublisher=Doena Soft.
; AppPublisherURL=http://doena-journal.net/en/dvd-profiler-tools/
DefaultDirName={commonpf32}\Doena Soft.\WatchHistory
DefaultGroupName=WatchHistory
DirExistsWarning=No
SourceDir=..\WatchHistory\bin\x64\Release\WatchHistory
Compression=zip/9
AppMutex=WatchHistory
OutputBaseFilename=WatchHistorySetup
OutputDir=..\..\..\..\..\WatchHistorySetup\Setup\WatchHistory
MinVersion=0,6.0
PrivilegesRequired=admin
WizardImageFile=compiler:wizmodernimage-is.bmp
WizardSmallImageFile=compiler:wizmodernsmallimage-is.bmp
DisableReadyPage=yes
ShowLanguageDialog=no
VersionInfoCompany=Doena Soft.
VersionInfoCopyright=2016 - 2020
VersionInfoDescription=WatchHistory Setup
VersionInfoVersion=1.4.5.1
UninstallDisplayIcon={app}\djdsoft.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WinVersionTooLowError=This program requires Windows XP or above to be installed.%n%nWindows 9x, NT and 2000 are not supported.

[Types]
Name: "full"; Description: "Full installation"

[Files]
Source: "djdsoft.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "AbstractionLayer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "AbstractionLayer.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "DVDProfilerHelper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DVDProfilerHelper.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "DVDProfilerXML.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DVDProfilerXML.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "WatchHistory.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "WatchHistory.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "Xceed.Wpf.Toolkit.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NReco.VideoInfo.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source: "Google.Apis.Auth.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source: "Google.Apis.Auth.PlatformServices.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source: "Google.Apis.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source: "Google.Apis.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source: "Google.Apis.PlatformServices.dll"; DestDir: "{app}"; Flags: ignoreversion
; Source: "Google.Apis.YouTube.v3.dll"; DestDir: "{app}"; Flags: ignoreversion

Source: "de\DVDProfilerHelper.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\WatchHistory"; Filename: "{app}\WatchHistory.exe"; WorkingDir: "{app}"; IconFilename: "{app}\djdsoft.ico"
Name: "{commondesktop}\WatchHistory"; Filename: "{app}\WatchHistory.exe"; WorkingDir: "{app}"; IconFilename: "{app}\djdsoft.ico"

[Run]

;[UninstallDelete]

[UninstallRun]

[Registry]

[Code]
function IsDotNET40Detected(): boolean;
// Function to detect dotNet framework version 2.0
// Returns true if it is available, false it's not.
var
dotNetStatus: boolean;
begin
dotNetStatus := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4');
Result := dotNetStatus;
end;

function InitializeSetup(): Boolean;
// Called at the beginning of the setup package.
begin

if not IsDotNET40Detected then
begin
MsgBox( 'The Microsoft .NET Framework version 4.0 is not installed. Please install it and try again.', mbInformation, MB_OK );
Result := false;
end
else
Result := true;
end;