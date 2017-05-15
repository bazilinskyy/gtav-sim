@echo off

setlocal enableextensions

if "%1"=="" goto usage

for /f "tokens=*" %%i in ('%0\..\get_gta_path') do set APP_DIR=%%i

if defined %APP_DIR% (
    set SCRIPTS_DIR="%APP_DIR%"\scripts

    echo copy %1 to %SCRIPTS_DIR%

    if not exist %SCRIPTS_DIR% mkdir %SCRIPTS_DIR%
    copy %1 %SCRIPTS_DIR%
)

goto :eof

:usage
@echo Usage: %0 ^<File^>
exit /B 1
