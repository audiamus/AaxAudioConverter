#define MyAppSetupName 'AAX Audio Converter'
#define MyAppVersion '1.17'
#define MyProgramExe = 'AaxAudioConverter.exe'
#define MyCompany = 'audiamus'
#define MyAppName = 'AaxAudioConverter'
#define FFmpeg = 'ffmpeg'

[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright=Copyright © 2021 {#MyCompany}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyCompany}
AppPublisher={#MyCompany}
AppPublisherURL=https://github.com/audiamus/AaxAudioConverter

OutputBaseFilename=AaxAudioConverter-{#MyAppVersion}-Setup
DefaultGroupName={#MyCompany}
DefaultDirName={autopf}\{#MyCompany}\{#MyAppSetupName}
UninstallDisplayIcon={app}\{#MyProgramExe}
OutputDir=.\Setup
SourceDir=..\AaxAudioConverter\bin\Release
SolidCompression=yes

DisableWelcomePage=no
WizardStyle=modern
AllowNoIcons=yes

PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
UsePreviousPrivileges=yes

ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: de; MessagesFile: "compiler:languages\German.isl"

[CustomMessages]
en.MyDocName=Manual
en.MyDocFile={#MyAppName}.pdf

de.MyDocName=Anleitung
de.MyDocFile={#MyAppName}.de.pdf


[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyAppName}.exe"; DestDir: "{app}"
Source: "{#FFmpeg}64.exe"; DestDir: "{app}"; DestName: "{#FFmpeg}64.exe"; Check: Is64BitInstallMode
Source: "{#FFmpeg}.exe"; DestDir: "{app}"; DestName: "{#FFmpeg}.exe"; 
Source: "{#MyAppName}.exe.config"; DestDir: "{app}"
Source: "{#MyAppName}*.pdf"; DestDir: "{app}"
Source: "de\{#MyAppName}.resources.dll"; DestDir: "{app}\de"

[InstallDelete]
Type: files; Name: "{app}\*.html"

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\{#MyProgramExe}"
Name: "{group}\{#MyAppSetupName} {cm:MyDocName}"; Filename: "{app}\{cm:MyDocFile}"
Name: "{group}\{cm:UninstallProgram,{#MyAppSetupName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppSetupName}"; Filename: "{app}\{#MyProgramExe}"; Tasks: desktopicon


[Run]
Filename: "{app}\{#MyProgramExe}"; Description: "{cm:LaunchProgram,{#MyAppSetupName}}"; Flags: nowait postinstall skipifsilent



