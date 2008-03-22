@echo off
REM Installs a reference to the Gallio test runner for ReSharper for local debugging.

setlocal
set LOCALDIR=%~dp0

set RESHARPERRUNNER_DLL=%LOCALDIR%Gallio.ReSharperRunner\bin\Gallio.ReSharperRunner.dll
set RESHARPER_DIR=%PROGRAMFILES%\JetBrains\ReSharper

echo Installing the locally compiled Gallio test runner for ReSharper.
echo.

:V30_VS80_RETRY_PROMPT
set /P Answer=Support ReSharper v3.1 for VS 2005?  (Y/N)
if /I "%Answer%"=="Y" call :INSTALL "v3.1" "vs8.0" & goto :V30_VS80_DONE_PROMPT
if /I not "%Answer%"=="N" goto :V30_VS80_RETRY_PROMPT
:V30_VS80_DONE_PROMPT

:V30_VS90_RETRY_PROMPT
set /P Answer=Support ReSharper v3.1 for VS 2008?  (Y/N)
if /I "%Answer%"=="Y" call :INSTALL "v3.1" "vs9.0" & goto :V30_VS90_DONE_PROMPT
if /I not "%Answer%"=="N" goto :V30_VS90_RETRY_PROMPT
:V30_VS90_DONE_PROMPT

exit /b 0

:INSTALL
set RESHARPER_VERSION=%~1
set VS_VERSION=%~2

set RESHARPER_BIN_DIR=%RESHARPER_DIR%\%RESHARPER_VERSION%\%VS_VERSION%\Bin
if not exist "%RESHARPER_BIN_DIR%" (
    echo ReSharper %RESHARPER_VERSION% for %VS_VERSION% was not found.  Skipped.
    goto :EOF
)

set RESHARPER_PLUGINS_DIR=%RESHARPER_BIN_DIR%\Plugins
set GALLIO_PLUGIN_DIR=%RESHARPER_PLUGINS_DIR%\Gallio

if not exist "%RESHARPER_PLUGINS_DIR%" mkdir "%RESHARPER_PLUGINS_DIR%"
if not exist "%GALLIO_PLUGIN_DIR%" mkdir "%GALLIO_PLUGIN_DIR%"

copy "%RESHARPERRUNNER_DLL%" "%GALLIO_PLUGIN_DIR%" /Y >nul
copy "%RESHARPERRUNNER_DLL%.config" "%GALLIO_PLUGIN_DIR%" /Y >nul
goto :EOF

