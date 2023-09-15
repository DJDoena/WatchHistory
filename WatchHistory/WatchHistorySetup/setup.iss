[Setup]
AppName=WatchHistory
AppId=WatchHistory
AppVerName=WatchHistory 1.4.6.11
AppCopyright=Copyright © Doena Soft. 2016 - 2023
AppPublisher=Doena Soft.
; AppPublisherURL=http://doena-journal.net/en/dvd-profiler-tools/
DefaultDirName={commonpf32}\Doena Soft.\WatchHistory
DefaultGroupName=WatchHistory
DirExistsWarning=No
SourceDir=..\WatchHistory\bin\x64\Release\net472
Compression=zip/9
AppMutex=WatchHistory
OutputBaseFilename=WatchHistorySetup
OutputDir=..\..\..\..\..\WatchHistorySetup\Setup\WatchHistory
MinVersion=0,6.1sp1
PrivilegesRequired=admin
WizardStyle=modern
DisableReadyPage=yes
ShowLanguageDialog=no
VersionInfoCompany=Doena Soft.
VersionInfoCopyright=2016 - 2023
VersionInfoDescription=WatchHistory Setup
VersionInfoVersion=1.4.6.11
UninstallDisplayIcon={app}\djdsoft.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WinVersionTooLowError=This program requires Windows XP or above to be installed.%n%nWindows 9x, NT and 2000 are not supported.

[Types]
Name: "full"; Description: "Full installation"

[Files]
Source: "djdsoft.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.AbstractionLayer.IO.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.AbstractionLayer.UI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.AbstractionLayer.Web.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.AbstractionLayer.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.AbstractionLayer.WPF.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.DVDProfiler.Helper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.DVDProfiler.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.MediaInfoHelper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.ToolBox.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "DoenaSoft.WatchHistory.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "WatchHistory.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "WatchHistory.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "de\DoenaSoft.DVDProfiler.Helper.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion

Source: "Microsoft.Win32.Registry.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NAudio.Asio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NAudio.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NAudio.Midi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NAudio.Wasapi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NAudio.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NAudio.WinMM.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "NReco.VideoInfo.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "policy.2.0.taglib-sharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "System.Resources.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "System.Security.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "System.Security.Principal.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "taglib-sharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Xceed.Wpf.AvalonDock.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Xceed.Wpf.AvalonDock.Themes.Aero.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Xceed.Wpf.AvalonDock.Themes.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Xceed.Wpf.AvalonDock.Themes.VS2010.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Xceed.Wpf.Toolkit.dll"; DestDir: "{app}"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\WatchHistory"; Filename: "{app}\WatchHistory.exe"; WorkingDir: "{app}"; IconFilename: "{app}\djdsoft.ico"
Name: "{commondesktop}\WatchHistory"; Filename: "{app}\WatchHistory.exe"; WorkingDir: "{app}"; IconFilename: "{app}\djdsoft.ico"

[Run]

;[UninstallDelete]

[UninstallRun]

[Registry]

[Code]
function IsDotNET4Detected(): boolean;
// Function to detect dotNet framework version 4
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

if not IsDotNET4Detected then
begin
MsgBox( 'The Microsoft .NET Framework version 4 is not installed. Please install it and try again.', mbInformation, MB_OK );
Result := false;
end
else
Result := true;
end;