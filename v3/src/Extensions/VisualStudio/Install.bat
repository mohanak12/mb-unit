@echo off & if not "%ECHO%"=="" echo %ECHO%
REM Installs a reference to the Gallio TIP provider for local debugging.

setlocal
set LOCALDIR=%~dp0
set OPTION=%~1
set SRCDIR=%LOCALDIR%..\..\
set ROOTDIR=%SRCDIR%..\
set BINDIR=%BINDIR%bin\

set REG=%BINDIR%reg.exe
set SED=%BINDIR%minised.exe
set GACUTIL=%BINDIR%gacutil.exe

set SHELL_BIN_DIR=%LOCALDIR%Gallio.VisualStudio.Shell\bin
set SAIL_BIN_DIR=%LOCALDIR%Gallio.VisualStudio.Sail\bin
set TIP_BIN_DIR=%LOCALDIR%Gallio.VisualStudio.Tip\bin
set TIPPROXY_BIN_DIR=%LOCALDIR%Gallio.VisualStudio.Tip.Proxy\bin

echo Installs Visual Studio packages for Gallio.
echo.

call :SETVARS "9.0"
if not defined VS_INSTALL_DIR goto :VS90_NOT_INSTALLED
echo Uninstalling support for Visual Studio 2008...
call :UNINSTALL
:VS90_RETRY_PROMPT
set ANSWER=%OPTION%
if not defined ANSWER set /P ANSWER=Support VS 2008?  (Y/N)
if /I "%ANSWER%"=="N" goto :VS90_DONE_PROMPT
if /I not "%ANSWER%"=="Y" goto :VS90_RETRY_PROMPT
echo Installing support for Visual Studio 2008...
call :INSTALL
:VS90_DONE_PROMPT
"%VS_INSTALL_DIR%\devenv.exe" /setup /nosetupvstemplates
:VS90_NOT_INSTALLED

exit /b 0


:INSTALL
gacutil /i /f "%SHELL_BIN_DIR%\Gallio.Loader.dll"
gacutil /i /f "%TIPPROXY_BIN_DIR%\Gallio.VisualStudio.Tip.Proxy.dll"

REM Register Shell
"%REG%" ADD "%VS_PRODUCT_KEY%" /V Package /D "{9e600ffc-344d-4e6f-89c0-ded6afb42459}" /F >nul
"%REG%" ADD "%VS_PRODUCT_KEY%" /V UseInterface /T REG_DWORD /D "1" /F >nul

"%REG%" ADD %VS_PACKAGE_KEY% /VE /D "Gallio.VisualStudio.Shell.ShellPackage, Gallio.VisualStudio.Shell" /F >nul
"%REG%" ADD %VS_PACKAGE_KEY% /V InprocServer32 /D "%SystemRoot%\system32\mscoree.dll" /F >nul
"%REG%" ADD %VS_PACKAGE_KEY% /V Class /D "Gallio.VisualStudio.Shell.ShellPackage" /F >nul
"%REG%" ADD %VS_PACKAGE_KEY% /V Assembly /D "Gallio.VisualStudio.Shell" /F >nul
"%REG%" ADD %VS_PACKAGE_KEY% /V ID /T REG_DWORD /D 1 /F >nul
"%REG%" ADD %VS_PACKAGE_KEY% /V MinEdition /D "Standard" /F >nul
"%REG%" ADD %VS_PACKAGE_KEY% /V ProductVersion /D "3.0" /F >nul
"%REG%" ADD %VS_PACKAGE_KEY% /V ProductName /D "Gallio" /F >nul
"%REG%" ADD %VS_PACKAGE_KEY% /V CompanyName /D "Gallio Project" /F >nul

"%REG%" ADD "%VS_ROOT_KEY%\AutoLoadPackages\{f1536ef8-92ec-443c-9ed7-fdadf150da82}" /V "{9e600ffc-344d-4e6f-89c0-ded6afb42459}" /T REG_DWORD /D "0" /F >nul

REM Register TIP
"%REG%" ADD %VS_TEST_TYPE_KEY% /V NameId /D "#100" /F >nul
"%REG%" ADD %VS_TEST_TYPE_KEY% /V TipProvider /D "Gallio.VisualStudio.Tip.GallioTipProxy, Gallio.VisualStudio.Tip.Proxy" /F >nul
"%REG%" ADD %VS_TEST_TYPE_KEY% /V ServiceType /D "Gallio.VisualStudio.Tip.SGallioTestService, Gallio.VisualStudio.Tip.Proxy" /F >nul

"%REG%" ADD %VS_TEST_TYPE_KEY%\Extensions /V .dll /T REG_DWORD /D "101" /F >nul
"%REG%" ADD %VS_TEST_TYPE_KEY%\Extensions /V .exe /T REG_DWORD /D "101" /F >nul

exit /b 0


:UNINSTALL
gacutil /u Gallio.Loader
gacutil /u Gallio.VisualStudio.Tip.Proxy

"%REG%" DELETE "%VS_TEST_TYPE_KEY%" /F 2>nul >nul
"%REG%" DELETE "%VS_PRODUCT_KEY%" /F 2>nul >nul
"%REG%" DELETE "%VS_PACKAGE_KEY%" /F 2>nul >nul

"%REG%" DELETE "%VS_ROOT_KEY%\AutoLoadPackages\{f1536ef8-92ec-443c-9ed7-fdadf150da82}" /V "{9e600ffc-344d-4e6f-89c0-ded6afb42459}" /F 2>nul >nul
exit /b 0


:SETVARS
set VS_VERSION=%~1

set VS_ROOT_KEY=HKLM\Software\Microsoft\VisualStudio\%VS_VERSION%
set VS_INSTALL_DIR=
for /F "tokens=1,2*" %%V in ('"%REG%" query %VS_ROOT_KEY% /V InstallDir') do (
    if "%%V"=="InstallDir" set VS_INSTALL_DIR=%%X
)

if not exist "%VS_INSTALL_DIR%" (
    echo Visual Studio version %VS_VERSION% was not found.  Skipped.
    set VS_INSTALL_DIR=
    exit /b 1
)

set VS_PRIVATE_ASSEMBLIES_DIR=%VS_INSTALL_DIR%\PrivateAssemblies
set VS_TEST_TYPE_KEY=%VS_ROOT_KEY%\EnterpriseTools\QualityTools\TestTypes\{F3589083-259C-4054-87F7-75CDAD4B08E5}
set VS_PRODUCT_KEY=%VS_ROOT_KEY%\InstalledProducts\Gallio
set VS_PACKAGE_KEY=%VS_ROOT_KEY%\Packages\{9e600ffc-344d-4e6f-89c0-ded6afb42459}
exit /b 0

