!include "StrRep.nsh"
!include "Path.nsh"
!include "SafeUninstall.nsh"
!include "PatchConfigFile.nsh"
!include "VisualStudio.nsh"

; Check arguments.
!ifndef VERSION
	!error "The /DVersion=x.y.z.w argument must be specified."
!endif
!ifndef ROOTDIR
	!error "The /DRootDir=... argument must be specified."
!endif

; Common directories
!define BUILDDIR "${ROOTDIR}\build"
!define TARGETDIR "${BUILDDIR}\target"
!define RELEASEDIR "${BUILDDIR}\release"

; Define your application name
!define APPNAME "Gallio"
!define APPNAMEANDVERSION "Gallio v${VERSION}"

; Main Install settings
Name "${APPNAMEANDVERSION}"
InstallDir "$PROGRAMFILES\Gallio"
OutFile "${RELEASEDIR}\GallioBundle-${VERSION}-Setup.exe"

BrandingText "gallio.org"

ShowInstDetails show
ShowUnInstDetails show

InstType "Full"
InstType "MbUnit v3 Only"

; Constants
!define SF_SELECTED_MASK 254
!define SF_RO_MASK 239

; Modern interface settings
!define MUI_COMPONENTSPAGE_SMALLDESC ; Put description on bottom.
!define MUI_ABORTWARNING
!define MUI_WELCOMEFINISHPAGE_BITMAP "banner-left.bmp"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "banner-top.bmp"
!define MUI_ICON "installer.ico"
!define MUI_UNICON "uninstaller.ico"
!include "MUI.nsh"

; Installer pages
!insertmacro MUI_PAGE_WELCOME
Page custom AddRemovePageEnter AddRemovePageLeave
!insertmacro MUI_PAGE_LICENSE "${TARGETDIR}\Gallio License.txt"
Page custom UserSelectionPageEnter UserSelectionPageLeave
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

; Set languages (first is default language)
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_RESERVEFILE_LANGDLL

; Store installation options in the Reserve data block for
; startup efficiency.
ReserveFile "AddRemovePage.ini"
ReserveFile "UserSelectionPage.ini"
!insertmacro MUI_RESERVEFILE_INSTALLOPTIONS
Var INI_VALUE

; Stores "all" if installing for all users, else "current"
Var UserContext

; Detect whether any components are missing
!tempfile DETECT_TEMP
!system 'if not exist "${TARGETDIR}\docs\Gallio.chm" echo !define MISSING_CHM_HELP >> "${DETECT_TEMP}"'
!system 'if not exist "${TARGETDIR}\docs\vs\Gallio.HxS" echo !define MISSING_VS_HELP >> "${DETECT_TEMP}"'
!system 'if not exist "${TARGETDIR}\bin\MbUnit.Pex.dll" echo !define MISSING_MBUNIT_PEX_PACKAGE >> "${DETECT_TEMP}"'
!system 'if not exist "${TARGETDIR}\bin\ReSharper\v3.1\Gallio.ReSharperRunner.dll" echo !define MISSING_RESHARPER_RUNNER_31 >> "${DETECT_TEMP}"'
!system 'if not exist "${TARGETDIR}\bin\ReSharper\v4.0\Gallio.ReSharperRunner.dll" echo !define MISSING_RESHARPER_RUNNER_40 >> "${DETECT_TEMP}"'
!system 'if not exist "${TARGETDIR}\bin\MSTest\Gallio.MSTestRunner.dll" echo !define MISSING_MSTEST_RUNNER >> "${DETECT_TEMP}"'
!system 'if not exist "${TARGETDIR}\bin\MSTest\Gallio.MSTestAdapter.dll" echo !define MISSING_MSTEST_ADAPTER >> "${DETECT_TEMP}"'
!include "${DETECT_TEMP}"
!delfile "${DETECT_TEMP}"

; Emit warnings if any components are missing
!ifdef MISSING_CHM_HELP
	!warning "Missing CHM file."
!endif

!ifdef MISSING_VS_HELP
	!warning "Missing Visual Studio help collection."
!endif

!ifdef MISSING_MBUNIT_PEX_PACKAGE
	!warning "Missing MbUnit Pex package."
!endif

!ifdef MISSING_RESHARPER_RUNNER_31
	!warning "Missing ReSharper v3.1 runner."
!endif

!ifdef MISSING_RESHARPER_RUNNER_40
	!warning "Missing ReSharper v4.0 runner."
!endif

!ifdef MISSING_MSTEST_RUNNER
	!warning "Missing MSTest runner."
!endif

!ifdef MISSING_MSTEST_ADAPTER
	!warning "Missing MSTest adapter."
!endif


; Define sections
Section "!Gallio" GallioSection
	; Set Section properties
	SectionIn RO
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR"
	File "${TARGETDIR}\ASL - Apache Software Foundation License.txt"
	File "${TARGETDIR}\Gallio License.txt"
	File "${TARGETDIR}\Release Notes.txt"
	File "${TARGETDIR}\Gallio Website.url"

	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\Gallio.dll"
	File "${TARGETDIR}\bin\Gallio.pdb"
	File "${TARGETDIR}\bin\Gallio.xml"
	File "${TARGETDIR}\bin\Gallio.XmlSerializers.dll"
	File "${TARGETDIR}\bin\Gallio.plugin"
	File "${TARGETDIR}\bin\Gallio.Host.exe"
	File "${TARGETDIR}\bin\Gallio.Host.exe.config"
	File "${TARGETDIR}\bin\Gallio.Loader.dll"

	SetOutPath "$INSTDIR\bin\Reports"
	File /r "${TARGETDIR}\bin\Reports\*"

	; Add the Gallio bin folder to the path
	${AddPath} "$UserContext" "$INSTDIR\bin"

	; Register the folder so that Visual Studio Add References can find it
	WriteRegStr SHCTX "SOFTWARE\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\Gallio" "" "$INSTDIR\bin"

	; Create Shortcuts
	SetOutPath "$SMPROGRAMS\${APPNAME}"
	CreateDirectory "$SMPROGRAMS\${APPNAME}"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\Browse Install Folder.lnk" "$INSTDIR\"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\Uninstall.lnk" "$INSTDIR\uninstall.exe"	
	File "${TARGETDIR}\Gallio Website.url"
SectionEnd

SectionGroup "!MbUnit v3"
Section "!MbUnit v3 Framework" MbUnit3Section
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR"
	File "${TARGETDIR}\MbUnit Website.url"

	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\MbUnit.dll"
	File "${TARGETDIR}\bin\MbUnit.pdb"
	File "${TARGETDIR}\bin\MbUnit.plugin"
	File "${TARGETDIR}\bin\MbUnit.xml"

	; Create Shortcuts
	SetOutPath "$SMPROGRAMS\${APPNAME}"
	File "${TARGETDIR}\MbUnit Website.url"
SectionEnd

!ifndef MISSING_MBUNIT_PEX_PACKAGE
Section "MbUnit v3 Pex Package" MbUnit3PexSection
	; Set Section properties
	SetOverwrite on
	
	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\MbUnit.Pex.dll"
SectionEnd
!endif

Section "MbUnit v3 Visual Studio 2005 Templates" MbUnit3VS2005TemplatesSection
	; Set Section properties
	SetOverwrite on

	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\8.0" "InstallDir"
	IfErrors SkipVS2005Templates

        ; C# Item Templates
	SetOutPath "$0\ItemTemplates\CSharp\Test"
	File "${TARGETDIR}\extras\Templates\VS2005\ItemTemplates\CSharp\Test\MbUnit3.TestFixtureTemplate.CSharp.zip"

	; C# Project Templates
	SetOutPath "$0\ProjectTemplates\CSharp\Test"
	File "${TARGETDIR}\extras\Templates\VS2005\ProjectTemplates\CSharp\Test\MbUnit3.TestProjectTemplate.CSharp.zip"

        ; VB Item Templates
	SetOutPath "$0\ItemTemplates\VisualBasic\Test"
	File "${TARGETDIR}\extras\Templates\VS2005\ItemTemplates\VisualBasic\Test\MbUnit3.TestFixtureTemplate.VisualBasic.zip"

	; VB Project Templates
	SetOutPath "$0\ProjectTemplates\VisualBasic\Test"
	File "${TARGETDIR}\extras\Templates\VS2005\ProjectTemplates\VisualBasic\Test\MbUnit3.TestProjectTemplate.VisualBasic.zip"

	StrCpy $VS2005UpdateRequired "1"

	SkipVS2005Templates:
SectionEnd

Section "MbUnit v3 Visual Studio 2008 Templates" MbUnit3VS2008TemplatesSection
	; Set Section properties
	SetOverwrite on

	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\9.0" "InstallDir"
	IfErrors SkipVS2008Templates

        ; C# Item Templates
	SetOutPath "$0\ItemTemplates\CSharp\Test"
	File "${TARGETDIR}\extras\Templates\VS2008\ItemTemplates\CSharp\Test\MbUnit3.TestFixtureTemplate.CSharp.zip"

	; C# Project Templates
	SetOutPath "$0\ProjectTemplates\CSharp\Test"
	File "${TARGETDIR}\extras\Templates\VS2008\ProjectTemplates\CSharp\Test\MbUnit3.TestProjectTemplate.CSharp.zip"
	File "${TARGETDIR}\extras\Templates\VS2008\ProjectTemplates\CSharp\Test\MbUnit3.MvcWebApplicationTestProjectTemplate.CSharp.zip"

	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\C#" "Path" "CSharp\Test"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\C#" "Template" "MbUnit3.MvcWebApplicationTestProjectTemplate.CSharp.zip"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\C#" "TestFrameworkName" "MbUnit v3"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\C#" "AdditionalInfo" "http://www.mbunit.com/"

        ; VB Item Templates
	SetOutPath "$0\ItemTemplates\VisualBasic\Test"
	File "${TARGETDIR}\extras\Templates\VS2008\ItemTemplates\VisualBasic\Test\MbUnit3.TestFixtureTemplate.VisualBasic.zip"

	; VB Project Templates
	SetOutPath "$0\ProjectTemplates\VisualBasic\Test"
	File "${TARGETDIR}\extras\Templates\VS2008\ProjectTemplates\VisualBasic\Test\MbUnit3.TestProjectTemplate.VisualBasic.zip"
	File "${TARGETDIR}\extras\Templates\VS2008\ProjectTemplates\VisualBasic\Test\MbUnit3.MvcWebApplicationTestProjectTemplate.VisualBasic.zip"

	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\VB" "Path" "VisualBasic\Test"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\VB" "Template" "MbUnit3.MvcWebApplicationTestProjectTemplate.VisualBasic.zip"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\VB" "TestFrameworkName" "MbUnit v3"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\VB" "AdditionalInfo" "http://www.mbunit.com/"

	StrCpy $VS2008UpdateRequired "1"

	SkipVS2008Templates:
SectionEnd
SectionGroupEnd

SectionGroup "Plugins"
Section "MbUnit v2 Plugin" MbUnit2PluginSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin\MbUnit2"
	File /r "${TARGETDIR}\bin\MbUnit2\*"
SectionEnd

!ifndef MISSING_MSTEST_ADAPTER
Section "MSTest Plugin" MSTestPluginSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin\MSTest"
	File /r "${TARGETDIR}\bin\MSTest\*"
SectionEnd
!endif

Section "NUnit Plugin" NUnitPluginSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin\NUnit"
	File /r "${TARGETDIR}\bin\NUnit\*"
SectionEnd

Section "xUnit.Net Plugin" XunitPluginSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin\Xunit"
	File /r "${TARGETDIR}\bin\Xunit\*"
SectionEnd
SectionGroupEnd

SectionGroup "Runners"
Section "Echo (Console Test Runner)" EchoSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\Gallio.Echo.exe"
	File "${TARGETDIR}\bin\Gallio.Echo.exe.config"
SectionEnd

Section "Icarus (GUI Test Runner)" IcarusSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\Aga.Controls.dll"
	File "${TARGETDIR}\bin\ICSharpCode.TextEditor.dll"
	File "${TARGETDIR}\bin\WeifenLuo.WinFormsUI.Docking.dll"
	File "${TARGETDIR}\bin\Gallio.Icarus.exe"
	File "${TARGETDIR}\bin\Gallio.Icarus.exe.config"

	CreateDirectory "$SMPROGRAMS\${APPNAME}"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\Icarus GUI Test Runner.lnk" "$INSTDIR\bin\Gallio.Icarus.exe"
SectionEnd

Section "MSBuild Tasks" MSBuildTasksSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\Gallio.MSBuildTasks.dll"
	File "${TARGETDIR}\bin\Gallio.MSBuildTasks.xml"
SectionEnd

Section "NAnt Tasks" NAntTasksSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\Gallio.NAntTasks.dll"
	File "${TARGETDIR}\bin\Gallio.NAntTasks.xml"
SectionEnd

Section "PowerShell Commands" PowerShellCommandsSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\Gallio.PowerShellCommands.dll"
	File "${TARGETDIR}\bin\Gallio.PowerShellCommands.xml"
	File "${TARGETDIR}\bin\Gallio.PowerShellCommands.dll-Help.xml"

	; Registry keys for the snap-in
	WriteRegStr SHCTX "SOFTWARE\Microsoft\PowerShell\1\PowerShellSnapIns\Gallio" "ApplicationBase" "$INSTDIR\bin"
	WriteRegStr SHCTX "SOFTWARE\Microsoft\PowerShell\1\PowerShellSnapIns\Gallio" "AssemblyName" "Gallio.PowerShellCommands, Version=${VERSION}, Culture=neutral, PublicKeyToken=eb9cfa67ee6ab36e"
	WriteRegStr SHCTX "SOFTWARE\Microsoft\PowerShell\1\PowerShellSnapIns\Gallio" "Description" "Gallio Commands."
	WriteRegStr SHCTX "SOFTWARE\Microsoft\PowerShell\1\PowerShellSnapIns\Gallio" "ModuleName" "$INSTDIR\bin\Gallio.PowerShellCommands.dll"
	WriteRegStr SHCTX "SOFTWARE\Microsoft\PowerShell\1\PowerShellSnapIns\Gallio" "PowerShellVersion" "1.0"
	WriteRegStr SHCTX "SOFTWARE\Microsoft\PowerShell\1\PowerShellSnapIns\Gallio" "Vendor" "Gallio"
	WriteRegStr SHCTX "SOFTWARE\Microsoft\PowerShell\1\PowerShellSnapIns\Gallio" "Version" "${VERSION}"
SectionEnd

Var ReSharperInstallDir
Var ReSharperPluginDir
!macro GetReSharperPluginDir RSVersion VSVersion
	ClearErrors
	ReadRegStr $ReSharperInstallDir HKCU "Software\JetBrains\ReSharper\${RSVersion}\${VSVersion}" "InstallDir"
	IfErrors +1 +7

	ClearErrors
	ReadRegStr $ReSharperInstallDir HKLM "Software\JetBrains\ReSharper\${RSVersion}\${VSVersion}" "InstallDir"
	IfErrors +1 +4

	StrCpy $ReSharperInstallDir ""
	StrCpy $ReSharperPluginDir ""
	Goto +5

	StrCmp "current" $UserContext +3
		StrCpy $ReSharperPluginDir "$ReSharperInstallDir\Plugins"
		Goto +2

	StrCpy $ReSharperPluginDir "$APPDATA\JetBrains\ReSharper\${RSVersion}\${VSVersion}\Plugins"
!macroend

Function un.UninstallReSharperRunner
	Exch $0 ; VSVersion
	Exch 1
	Exch $1 ; RSVersion
	Exch 2
	Exch $2 ; RSRunnerSuffix

	!insertmacro GetReSharperPluginDir "$1" "$0"

	StrCmp "" "$ReSharperPluginDir" Done
		${un.SafeDelete} "$ReSharperPluginDir\Gallio\Gallio.Loader.dll"
		${un.SafeDelete} "$ReSharperPluginDir\Gallio\Gallio.ReSharperRunner$2.dll"
		${un.SafeDelete} "$ReSharperPluginDir\Gallio\Gallio.ReSharperRunner$2.dll.config"
		${un.SafeRMDir} "$ReSharperPluginDir\Gallio"

	Done:

	Pop $2
	Pop $1
	Pop $0
FunctionEnd

!macro InstallReSharperRunner RSRunnerSuffix RSVersion VSVersion SourcePath
	!insertmacro GetReSharperPluginDir "${RSVersion}" "${VSVersion}"

	StrCmp "" "$ReSharperPluginDir" +8
		SetOutPath "$ReSharperPluginDir\Gallio"
		File "${SourcePath}\Gallio.Loader.dll"
		File "${SourcePath}\ReSharper\${RSVersion}\Gallio.ReSharperRunner${RSRunnerSuffix}.dll"
		File "/oname=Gallio.ReSharperRunner${RSRunnerSuffix}.dll.config.orig" "${SourcePath}\ReSharper\Gallio.ReSharperRunner${RSRunnerSuffix}.dll.config"
		${PatchConfigFile} "Gallio.ReSharperRunner${RSRunnerSuffix}.dll.config.orig" "Gallio.ReSharperRunner${RSRunnerSuffix}.dll.config"
!macroend

!macro UninstallReSharperRunner RSRunnerSuffix RSVersion VSVersion
	Push "${RSRunnerSuffix}"
	Push "${RSVersion}"
	Push "${VSVersion}"
	Call un.UninstallReSharperRunner
!macroend

Section "ReSharper v3.1 Runner" ReSharperRunner31Section
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	!insertmacro InstallReSharperRunner "31" "v3.1" "vs8.0" "${TARGETDIR}\bin"
	!insertmacro InstallReSharperRunner "31" "v3.1" "vs9.0" "${TARGETDIR}\bin"
SectionEnd

Section "ReSharper v4.0 Runner" ReSharperRunner40Section
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	!insertmacro InstallReSharperRunner "40" "v4.0" "vs8.0" "${TARGETDIR}\bin"
	!insertmacro InstallReSharperRunner "40" "v4.0" "vs9.0" "${TARGETDIR}\bin"
SectionEnd
!endif

!macro InstallTDNetApplication
	WriteRegStr SHCTX "SOFTWARE\MutantDesign\TestDriven.NET\TestRunners\Gallio_Icarus" "Application" "$INSTDIR\bin\Gallio.Icarus.exe"
!macroend

!macro UninstallTDNetApplication
	DeleteRegKey SHCTX "SOFTWARE\MutantDesign\TestDriven.NET\TestRunners\Gallio_Icarus"
!macroend

!macro InstallTDNetRunner Key Framework Priority
	WriteRegStr SHCTX "SOFTWARE\MutantDesign\TestDriven.NET\TestRunners\${Key}" "" "${Priority}"
	WriteRegStr SHCTX "SOFTWARE\MutantDesign\TestDriven.NET\TestRunners\${Key}" "AssemblyPath" "$INSTDIR\bin\Gallio.TDNetRunner.dll"
	WriteRegStr SHCTX "SOFTWARE\MutantDesign\TestDriven.NET\TestRunners\${Key}" "TypeName" "Gallio.TDNetRunner.GallioTestRunner"
	WriteRegStr SHCTX "SOFTWARE\MutantDesign\TestDriven.NET\TestRunners\${Key}" "TargetFrameworkAssemblyName" "${Framework}"
!macroend

!macro UninstallTDNetRunner Key
	DeleteRegKey SHCTX "SOFTWARE\MutantDesign\TestDriven.NET\TestRunners\${Key}"
!macroend

Section "TestDriven.Net Runner" TDNetAddInSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin"
	File "${TARGETDIR}\bin\Gallio.TDNetRunner.dll"

	; Registry Keys
	!insertmacro InstallTDNetApplication

	SectionGetFlags ${MbUnit3Section} $0
	IntOp $0 $0 & ${SF_SELECTED}
	IntCmp $0 0 NoMbUnit
		!insertmacro InstallTDNetRunner "Gallio_MbUnit" "MbUnit" "10"
	NoMbUnit:

	; TODO: We should probably allow users to select whether Gallio should override
	;       their existing TD.Net addins.
	SectionGetFlags ${MbUnit2PluginSection} $0
	IntOp $0 $0 & ${SF_SELECTED}
	IntCmp $0 0 NoMbUnit2
		!insertmacro InstallTDNetRunner "Gallio_MbUnit2" "MbUnit.Framework" "5"
	NoMbUnit2:

!ifndef MISSING_MSTEST_ADAPTER
	SectionGetFlags ${MSTestPluginSection} $0
	IntOp $0 $0 & ${SF_SELECTED}
	IntCmp $0 0 NoMSTest
		!insertmacro InstallTDNetRunner "Gallio_MSTest" "Microsoft.VisualStudio.QualityTools.UnitTestFramework" "5"
	NoMSTest:
!endif

	SectionGetFlags ${NUnitPluginSection} $0
	IntOp $0 $0 & ${SF_SELECTED}
	IntCmp $0 0 NoNUnit
		!insertmacro InstallTDNetRunner "Gallio_NUnit" "nunit.framework" "5"
	NoNUnit:

	SectionGetFlags ${XunitPluginSection} $0
	IntOp $0 $0 & ${SF_SELECTED}
	IntCmp $0 0 NoXunit
		!insertmacro InstallTDNetRunner "Gallio_Xunit" "xunit" "5"
	NoXunit:
SectionEnd

SectionGroupEnd

SectionGroup "Tools Integration"

Section "NCover Integration" NCoverSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin\NCover"
	File /r "${TARGETDIR}\bin\NCover\*"
SectionEnd

Section "TypeMock.Net Integration" TypeMockSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\bin\TypeMock"
	File /r "${TARGETDIR}\bin\TypeMock\*"
SectionEnd

SectionGroupEnd

!ifndef MISSING_CHM_HELP | MISSING_VS_HELP
SectionGroup "Documentation"
!ifndef MISSING_CHM_HELP
Section "Standalone Help Docs" CHMHelpSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR"

	SetOutPath "$INSTDIR\docs"
	File "${TARGETDIR}\docs\Gallio.chm"

	; Create Shortcuts
	CreateShortCut "$INSTDIR\Offline Documentation.lnk" "$INSTDIR\docs\Gallio.chm"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\Offline Documentation.lnk" "$INSTDIR\docs\Gallio.chm"
SectionEnd
!endif

!ifndef MISSING_VS_HELP
Section "Visual Studio Help Docs" VSHelpSection
	; Set Section properties
	SetOverwrite on

	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\docs\vs"
	File "${TARGETDIR}\docs\vs\Gallio.Hx?"
	File "${TARGETDIR}\docs\vs\GallioCollection.*"

	SetOutPath "$INSTDIR\extras\H2Reg"
	File "${TARGETDIR}\extras\H2Reg\*"

	; Merge the collection
	ExecWait '"$INSTDIR\extras\H2Reg\H2Reg.exe" -r CmdFile="$INSTDIR\docs\vs\GallioCollection.h2reg.ini"'
SectionEnd
!endif
SectionGroupEnd
!endif

SectionGroup "Extras"

Section "CruiseControl.Net extensions" CCNetSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\extras"
	File /r "${TARGETDIR}\extras\CCNet"
SectionEnd

Section "Samples" SamplesSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\extras"
	File /r "${TARGETDIR}\extras\Samples"
SectionEnd

SectionGroupEnd

!ifndef MISSING_MSTEST_RUNNER
SectionGroup "Experimental"

Section "Visual Studio Team System Extension (Experimental!)" MSTestRunnerSection
	; Set Section properties
	SetOverwrite on
	
	; Set Section Files and Shortcuts
	DetailPrint "Installing Visual Studio Team System extension."
	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\9.0" "InstallDir"
	IfErrors SkipVS2008Setup

	SetOutPath "$0\PrivateAssemblies"
	File "${TARGETDIR}\bin\Gallio.Loader.dll"
	File "${TARGETDIR}\bin\MSTest\Gallio.MSTestRunner.dll"
	File "/oname=Gallio.MSTestRunner.dll.config.orig" "${TARGETDIR}\bin\MSTest\Gallio.MSTestRunner.dll.config"
	${PatchConfigFile} "Gallio.MSTestRunner.dll.config.orig" "Gallio.MSTestRunner.dll.config"

	; Register the product
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\InstalledProducts\Gallio.MSTestRunner" "Package" "{9e600ffc-344d-4e6f-89c0-ded6afb42459}"
	WriteRegDWORD HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\InstalledProducts\Gallio.MSTestRunner" "UseInterface" 1

	; Register the package
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "" "Gallio.MSTestRunner.GallioPackage, Gallio.MSTestRunner"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "InprocServer32" "$WINDIR\system32\mscoree.dll"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "Class" "Gallio.MSTestRunner.GallioPackage"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "Assembly" "Gallio.MSTestRunner"
	WriteRegDWORD HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "ID" 1
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "MinEdition" "Standard"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "ProductVersion" "3.0"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "ProductName" "Gallio.MSTestRunner"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}" "CompanyName" "Gallio Project"

	WriteRegDWORD HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\AutoLoadPackages\{f1536ef8-92ec-443c-9ed7-fdadf150da82}" "{9e600ffc-344d-4e6f-89c0-ded6afb42459}" 0

	; Register the test types
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\EnterpriseTools\QualityTools\TestTypes\{F3589083-259C-4054-87F7-75CDAD4B08E5}" "NameId" "#100"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\EnterpriseTools\QualityTools\TestTypes\{F3589083-259C-4054-87F7-75CDAD4B08E5}" "TipProvider" "Gallio.MSTestRunner.GallioTip, Gallio.MSTestRunner"
	WriteRegStr HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\EnterpriseTools\QualityTools\TestTypes\{F3589083-259C-4054-87F7-75CDAD4B08E5}" "ServiceType" "Gallio.MSTestRunner.SGallioTestService, Gallio.MSTestRunner"
	WriteRegDWORD HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\EnterpriseTools\QualityTools\TestTypes\{F3589083-259C-4054-87F7-75CDAD4B08E5}\Extensions" ".dll" 101
	WriteRegDWORD HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\EnterpriseTools\QualityTools\TestTypes\{F3589083-259C-4054-87F7-75CDAD4B08E5}\Extensions" ".exe" 101

	StrCpy $VS2008UpdateRequired "1"

	SkipVS2008Setup:
SectionEnd

!macro UninstallMSTestRunner
	DetailPrint "Uninstalling Visual Studio Team System extension."
	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\9.0" "InstallDir"
	IfErrors SkipVS2008Setup

	${un.SafeDelete} "$0\PrivateAssemblies\Gallio.Loader.dll"
	${un.SafeDelete} "$0\PrivateAssemblies\Gallio.MSTestRunner.dll"
	${un.SafeDelete} "$0\PrivateAssemblies\Gallio.MSTestRunner.dll.config"
	${un.SafeDelete} "$0\PrivateAssemblies\Gallio.MSTestRunner.dll.config.orig"

	DeleteRegKey HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\InstalledProducts\Gallio.MSTestRunner"
	DeleteRegKey HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}"
	DeleteRegKey HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\EnterpriseTools\QualityTools\TestTypes\{F3589083-259C-4054-87F7-75CDAD4B08E5}"
	DeleteRegValue HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\AutoLoadPackages\{f1536ef8-92ec-443c-9ed7-fdadf150da82}" "{9e600ffc-344d-4e6f-89c0-ded6afb42459}"

	StrCpy $VS2008UpdateRequired "1"

	SkipVS2008Setup:
!macroend

SectionGroupEnd
!endif

Section -FinishSection
	!insertmacro UpdateVS2005IfNeeded
	!insertmacro UpdateVS2008IfNeeded

	WriteRegStr SHCTX "Software\${APPNAME}" "" "$INSTDIR"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\uninstall.exe"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$INSTDIR"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "HelpLink" "http://www.gallio.org/"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLUpdateInfo" "http://www.gallio.org/"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLInfoAbout" "http://www.gallio.org/"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLInfoAbout" "http://www.gallio.org/"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" "1"
	WriteRegStr SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" "1"
	WriteUninstaller "$INSTDIR\uninstall.exe"
SectionEnd

;Uninstall section
Section Uninstall
	StrCpy $VS2005UpdateRequired "1"
	StrCpy $VS2008UpdateRequired "1"

	; Remove the Gallio bin folder to the path
	DetailPrint "Removing Gallio from path."
	${un.RemovePath} "$UserContext" "$INSTDIR\bin"

	; Uninstall from assembly folders
	DetailPrint "Removing Gallio from assembly folders."
	DeleteRegKey SHCTX "SOFTWARE\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\Gallio"

	; Uninstall from TD.Net
	DetailPrint "Uninstalling TestDriven.Net runner."
	!insertmacro UninstallTDNetApplication
	!insertmacro UninstallTDNetRunner "Gallio_MbUnit"
	!insertmacro UninstallTDNetRunner "Gallio_MbUnit2"
	!insertmacro UninstallTDNetRunner "Gallio_MSTest"
	!insertmacro UninstallTDNetRunner "Gallio_NUnit"
	!insertmacro UninstallTDNetRunner "Gallio_Xunit"

	; Uninstall from ReSharper
	!ifndef MISSING_RESHARPER_RUNNER_31
		DetailPrint "Uninstalling ReSharper v3.1 runner."
		!insertmacro UninstallReSharperRunner "31" "v3.1" "vs8.0"
		!insertmacro UninstallReSharperRunner "31" "v3.1" "vs9.0"
	!endif
	!ifndef MISSING_RESHARPER_RUNNER_40
		DetailPrint "Uninstalling ReSharper v4.0 runner."
		!insertmacro UninstallReSharperRunner "40" "v4.0" "vs8.0"
		!insertmacro UninstallReSharperRunner "40" "v4.0" "vs9.0"
	!endif

	; Uninstall from Visual Studio
	!ifndef MISSING_MSTEST_RUNNER
		!insertmacro UninstallMSTestRunner
	!endif

	; Uninstall from PowerShell
	DetailPrint "Uninstalling PowerShell runner."
	DeleteRegKey SHCTX "SOFTWARE\Microsoft\PowerShell\1\PowerShellSnapIns\Gallio"

	; Uninstall the help collection
	IfFileExists "$INSTDIR\docs\vs\GallioCollection.h2reg.ini" 0 +3
		DetailPrint "Uninstalling Visual Studio help collection."
		ExecWait '"$INSTDIR\extras\H2Reg\H2Reg.exe" -u CmdFile="$INSTDIR\docs\vs\GallioCollection.h2reg.ini"'

	; Uninstall the Visual Studio 2005 templates
	DetailPrint "Uninstalling Visual Studio 2005 templates."
	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\8.0" "InstallDir"
	IfErrors SkipVS2005Templates

	${un.SafeDelete} "$0\ItemTemplates\CSharp\Test\MbUnit3.TestFixtureTemplate.CSharp.zip"
	${un.SafeDelete} "$0\ProjectTemplates\CSharp\Test\MbUnit3.TestProjectTemplate.CSharp.zip"

	${un.SafeDelete} "$0\ItemTemplates\VisualBasic\Test\MbUnit3.TestFixtureTemplate.VisualBasic.zip"
	${un.SafeDelete} "$0\ProjectTemplates\VisualBasic\Test\MbUnit3.TestProjectTemplate.VisualBasic.zip"

	SkipVS2005Templates:

	; Uninstall the Visual Studio 2008 templates
	DetailPrint "Uninstalling Visual Studio 2008 templates."
	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\9.0" "InstallDir"
	IfErrors SkipVS2008Templates

	${un.SafeDelete} "$0\ItemTemplates\CSharp\Test\MbUnit3.TestFixtureTemplate.CSharp.zip"
	${un.SafeDelete} "$0\ProjectTemplates\CSharp\Test\MbUnit3.TestProjectTemplate.CSharp.zip"
	${un.SafeDelete} "$0\ProjectTemplates\CSharp\Test\MbUnit3.MvcWebApplicationTestProjectTemplate.CSharp.zip"
	DeleteRegKey HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\C#"

	${un.SafeDelete} "$0\ItemTemplates\VisualBasic\Test\MbUnit3.TestFixtureTemplate.VisualBasic.zip"
	${un.SafeDelete} "$0\ProjectTemplates\VisualBasic\Test\MbUnit3.TestProjectTemplate.VisualBasic.zip"
	${un.SafeDelete} "$0\ProjectTemplates\VisualBasic\Test\MbUnit3.MvcWebApplicationTestProjectTemplate.VisualBasic.zip"
	DeleteRegKey HKLM "SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates\MbUnit3\VB"

	SkipVS2008Templates:

	; Update Visual Studio
	!insertmacro UpdateVS2005IfNeeded
	!insertmacro UpdateVS2008IfNeeded

	; Delete Shortcuts
	DetailPrint "Uninstalling shortcuts."
	${un.SafeRMDir} "$SMPROGRAMS\${APPNAME}"

	; Remove from registry...
	DetailPrint "Removing registry keys."
	DeleteRegKey SHCTX "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
	DeleteRegKey SHCTX "SOFTWARE\${APPNAME}"

	; Delete self
	DetailPrint "Deleting files."
	${un.SafeDelete} "$INSTDIR\uninstall.exe"

	; Remove all remaining contents
	${un.SafeRMDir} "$INSTDIR"
SectionEnd

; Component descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${GallioSection} "Installs the Gallio test automation platform."

	!insertmacro MUI_DESCRIPTION_TEXT ${MbUnit3Section} "Installs the MbUnit v3 framework components."
	!ifndef MISSING_MBUNIT_PEX_PACKAGE
		!insertmacro MUI_DESCRIPTION_TEXT ${MbUnit3PexSection} "Installs the MbUnit v3 Pex package."
	!endif
	!insertmacro MUI_DESCRIPTION_TEXT ${MbUnit3VS2005TemplatesSection} "Installs the MbUnit v3 Visual Studio 2005 templates."
	!insertmacro MUI_DESCRIPTION_TEXT ${MbUnit3VS2008TemplatesSection} "Installs the MbUnit v3 Visual Studio 2008 templates."

	!insertmacro MUI_DESCRIPTION_TEXT ${MbUnit2PluginSection} "Installs the MbUnit v2 plugin.  Enables Gallio to run MbUnit v2 tests."
	!ifndef MISSING_MSTEST_ADAPTER
		!insertmacro MUI_DESCRIPTION_TEXT ${MSTestPluginSection} "Installs the MSTest plugin.  Enables Gallio to run MSTest tests."
	!endif
	!insertmacro MUI_DESCRIPTION_TEXT ${NUnitPluginSection} "Installs the NUnit plugin.  Enables Gallio to run NUnit tests."
	!insertmacro MUI_DESCRIPTION_TEXT ${XunitPluginSection} "Installs the Xunit plugin.  Enables Gallio to run xUnit.Net tests."

	!insertmacro MUI_DESCRIPTION_TEXT ${EchoSection} "Installs the command-line test runner."
	!insertmacro MUI_DESCRIPTION_TEXT ${IcarusSection} "Installs the GUI-based test runner."
	!insertmacro MUI_DESCRIPTION_TEXT ${MSBuildTasksSection} "Installs the MSBuild tasks."
	!insertmacro MUI_DESCRIPTION_TEXT ${NAntTasksSection} "Installs the NAnt tasks."
	!insertmacro MUI_DESCRIPTION_TEXT ${PowerShellCommandsSection} "Installs the PowerShell commands."
	!ifndef MISSING_RESHARPER_RUNNER_31
		!insertmacro MUI_DESCRIPTION_TEXT ${ReSharperRunner31Section} "Installs the ReSharper v3.1 plug-in."
	!endif
	!ifndef MISSING_RESHARPER_RUNNER_40
		!insertmacro MUI_DESCRIPTION_TEXT ${ReSharperRunner40Section} "Installs the ReSharper v4.0 plug-in."
	!endif
	!ifndef MISSING_MSTEST_RUNNER
		!insertmacro MUI_DESCRIPTION_TEXT ${MSTestRunnerSection} "Installs the Gallio extension for Visual Studio Team System in Visual Studio 2008."
	!endif
	!insertmacro MUI_DESCRIPTION_TEXT ${TDNetAddInSection} "Installs the TestDriven.Net add-in."

	!insertmacro MUI_DESCRIPTION_TEXT ${NCoverSection} "Provides integration with the NCover code coverage tool."
	!insertmacro MUI_DESCRIPTION_TEXT ${TypeMockSection} "Provides integration with the TypeMock.Net mock object framework."

	!insertmacro MUI_DESCRIPTION_TEXT ${CCNetSection} "Installs additional resources to assist with CruiseControl.Net integration."
	!insertmacro MUI_DESCRIPTION_TEXT ${SamplesSection} "Installs code samples."

	!ifndef MISSING_CHM_HELP
		!insertmacro MUI_DESCRIPTION_TEXT ${CHMHelpSection} "Installs the standalone help documentation CHM file."
	!endif

	!ifndef MISSING_VS_HELP
		!insertmacro MUI_DESCRIPTION_TEXT ${VSHelpSection} "Installs the integrated documentation for Visual Studio 2005 or Visual Studio 2008 access with F1 Help."
	!endif
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; Initialization code
Function .onInit
	StrCpy $VS2005UpdateRequired "0"
	StrCpy $VS2008UpdateRequired "0"

	; Extract install option pages.
	!insertmacro MUI_INSTALLOPTIONS_EXTRACT "AddRemovePage.ini"
	!insertmacro MUI_INSTALLOPTIONS_EXTRACT "UserSelectionPage.ini"

	; Set installation types.
	; Bits:
	;   1 - Full
	;   2 - MbUnit v3 Only
	SectionSetInstTypes ${GallioSection} 3

	SectionSetInstTypes ${MbUnit3Section} 3
	!ifndef MISSING_MBUNIT_PEX_PACKAGE
		SectionSetInstTypes ${MbUnit3PexSection} 3
	!endif
	SectionSetInstTypes ${MbUnit3VS2005TemplatesSection} 3
	SectionSetInstTypes ${MbUnit3VS2008TemplatesSection} 3

	SectionSetInstTypes ${MbUnit2PluginSection} 1
	!ifndef MISSING_MSTEST_ADAPTER
		SectionSetInstTypes ${MSTestPluginSection} 1
	!endif
	SectionSetInstTypes ${NUnitPluginSection} 1
	SectionSetInstTypes ${XunitPluginSection} 1

	SectionSetInstTypes ${EchoSection} 3
	SectionSetInstTypes ${IcarusSection} 3
	SectionSetInstTypes ${MSBuildTasksSection} 3
	SectionSetInstTypes ${NAntTasksSection} 3
	SectionSetInstTypes ${PowerShellCommandsSection} 3
	!ifndef MISSING_RESHARPER_RUNNER_31
		SectionSetInstTypes ${ReSharperRunner31Section} 3
	!endif
	!ifndef MISSING_RESHARPER_RUNNER_40
		SectionSetInstTypes ${ReSharperRunner40Section} 3
	!endif
	!ifndef MISSING_MSTEST_RUNNER
		SectionSetInstTypes ${MSTestRunnerSection} 0
	!endif
	SectionSetInstTypes ${TDNetAddInSection} 3

	SectionSetInstTypes ${NCoverSection} 3
	SectionSetInstTypes ${TypeMockSection} 3

	SectionSetInstTypes ${CCNetSection} 3
	SectionSetInstTypes ${SamplesSection} 3
	!ifndef MISSING_CHM_HELP
		SectionSetInstTypes ${CHMHelpSection} 3
	!endif
	!ifndef MISSING_VS_HELP
		SectionSetInstTypes ${VSHelpSection} 3
	!endif

	SetCurInstType 0

	; Disable Visual Studio help section if not installed.
	!ifndef MISSING_VS_HELP
	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\8.0" "InstallDir"
	IfErrors 0 IncludeVSHelp
	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\9.0" "InstallDir"
	IfErrors 0 IncludeVSHelp
		SectionSetFlags ${VSHelpSection} ${SF_RO}
		SectionSetText ${VSHelpSection} "The integrated documentation for Visual Studio requires Visual Studio 2005 or Visual Studio 2008 to be installed."
	IncludeVSHelp:
	!endif

	; Disable VS2005 templates if not installed.
	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\8.0" "InstallDir"
	IfErrors 0 +3
		SectionSetFlags ${MbUnit3VS2005TemplatesSection} ${SF_RO}
		SectionSetText ${MbUnit3VS2005TemplatesSection} "The MbUnit v3 Visual Studio 2005 templates require Visual Studio 2005 to be installed."

	; Disable VS2008 templates if not installed.
	ClearErrors
	ReadRegStr $0 HKLM "SOFTWARE\Microsoft\VisualStudio\9.0" "InstallDir"
	IfErrors 0 +3
		SectionSetFlags ${MbUnit3VS2008TemplatesSection} ${SF_RO}
		SectionSetText ${MbUnit3VS2008TemplatesSection} "The MbUnit v3 Visual Studio 2008 templates require Visual Studio 2008 to be installed."
FunctionEnd

; Uninstaller initialization code.
Function un.onInit
	ClearErrors
	ReadRegStr $0 HKCU "Software\${APPNAME}" ""
	IfErrors NotInstalledForUser
		SetShellVarContext current
		StrCpy $UserContext "current"
		StrCpy $INSTDIR $0
		Goto Installed
	NotInstalledForUser:

	ClearErrors
	ReadRegStr $0 HKLM "Software\${APPNAME}" ""
	IfErrors NotInstalledForSystem
		SetShellVarContext all
		StrCpy $UserContext "all"
		StrCpy $INSTDIR $0
		Goto Installed
	NotInstalledForSystem:

	MessageBox MB_OK "Gallio does not appear to be installed!  Abandoning uninstallation."
	Abort

	Installed:	
FunctionEnd

; Add-remove page.
Var OLD_INSTALL_DIR
Function AddRemovePageEnter
	ClearErrors
	ReadRegStr $OLD_INSTALL_DIR HKCU "Software\${APPNAME}" ""
	IfErrors 0 AlreadyInstalled
	ReadRegStr $OLD_INSTALL_DIR HKLM "Software\${APPNAME}" ""
	IfErrors 0 AlreadyInstalled
	Return

	AlreadyInstalled:
	!insertmacro MUI_HEADER_TEXT "Installation Type" "Please select whether to upgrade or remove the currently installed version."
	!insertmacro MUI_INSTALLOPTIONS_DISPLAY "AddRemovePage.ini"
FunctionEnd

Function AddRemovePageLeave
	!insertmacro MUI_INSTALLOPTIONS_READ $INI_VALUE "AddRemovePage.ini" "Field 2" "State"

	; Note: We don't uninstall silently anymore because it takes too
	;       long and it sucks not to get any feedback during the process.
	CopyFiles /SILENT "$OLD_INSTALL_DIR\uninstall.exe" "$TEMP\Gallio-uninstall.exe"
	ExecWait '"$TEMP\Gallio-uninstall.exe" _?=$OLD_INSTALL_DIR' $0
	Delete "$TEMP\Gallio-uninstall.exe"
	IntCmp $0 0 Ok
	MessageBox MB_OK "Cannot proceed because the old version was not successfully uninstalled."
	Abort

	Ok:
	IntCmp $INI_VALUE 1 Upgrade
	Quit

	Upgrade:
	BringToFront
FunctionEnd

; User-selection page.
Function UserSelectionPageEnter
	!insertmacro MUI_HEADER_TEXT "Installation Options" "Please select for which users to install Gallio."
	!insertmacro MUI_INSTALLOPTIONS_DISPLAY "UserSelectionPage.ini"
FunctionEnd

Function UserSelectionPageLeave
	!insertmacro MUI_INSTALLOPTIONS_READ $INI_VALUE "UserSelectionPage.ini" "Field 2" "State"
	IntCmp $INI_VALUE 0 CurrentUserOnly
		SetShellVarContext all
		StrCpy $UserContext "all"
		Goto Done
	CurrentUserOnly:
		SetShellVarContext current
		StrCpy $UserContext "current"
	Done:
FunctionEnd


; Enforces a dependency from a source section on a target section.
!macro EnforceSelectionDependency SourceSection TargetSection
	SectionGetFlags ${SourceSection} $1

	SectionGetFlags ${TargetSection} $0
	IntOp $0 $0 & ${SF_SELECTED}
	IntCmp $0 0 +3
		IntOp $1 $1 & ${SF_RO_MASK}
		Goto +3
		IntOp $1 $1 & ${SF_SELECTED_MASK}
		IntOp $1 $1 | ${SF_RO}
	
	SectionSetFlags ${SourceSection} $1
!macroend

Function .onSelChange
	!ifndef MISSING_MBUNIT_PEX_PACKAGE
		!insertmacro EnforceSelectionDependency ${MbUnit3PexSection} ${MbUnit3Section}
	!endif
	!insertmacro EnforceSelectionDependency ${MbUnit3VS2005TemplatesSection} ${MbUnit3Section}
	!insertmacro EnforceSelectionDependency ${MbUnit3VS2008TemplatesSection} ${MbUnit3Section}
FunctionEnd

