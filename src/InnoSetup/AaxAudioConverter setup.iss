#define MyAppSetupName 'AAX Audio Converter'
#define MyAppVersion '1.7'
#define MyProgramExe = 'AaxAudioConverter.exe'
#define MyCompany = 'audiamus'

[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright=Copyright © 2020 {#MyCompany}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyCompany}
AppPublisher={#MyCompany}
AppPublisherURL=https://github.com/audiamus/AaxAudioConverter
;AppSupportURL=http://...
;AppUpdatesURL=http://...
OutputBaseFilename=AaxAudioConverter-{#MyAppVersion}-Setup
DefaultGroupName={#MyCompany}
DefaultDirName={pf}\{#MyCompany}\{#MyAppSetupName}
UninstallDisplayIcon={app}\{#MyProgramExe}
OutputDir=.\Setup
SourceDir=..\AaxAudioConverter\bin\Release
AllowNoIcons=yes
;SetupIconFile=MyProgramIcon
SolidCompression=yes
DisableWelcomePage=no

PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: de; MessagesFile: "compiler:languages\German.isl"

[CustomMessages]
en.MyDocName=Manual
en.MyDocFile=AaxAudioConverter.pdf

de.MyDocName=Anleitung
de.MyDocFile=AaxAudioConverter.de.pdf


[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "*.exe"; DestDir: "{app}"
Source: "*.exe.config"; DestDir: "{app}"
Source: "*.pdf"; DestDir: "{app}"
Source: "de\*.resources.dll"; DestDir: "{app}\de"

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\{#MyProgramExe}"
Name: "{group}\{#MyAppSetupName} {cm:MyDocName}"; Filename: "{app}\{cm:MyDocFile}"
Name: "{group}\{cm:UninstallProgram,{#MyAppSetupName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppSetupName}"; Filename: "{app}\{#MyProgramExe}"; Tasks: desktopicon


[Run]
Filename: "{app}\{#MyProgramExe}"; Description: "{cm:LaunchProgram,{#MyAppSetupName}}"; Flags: nowait postinstall skipifsilent



