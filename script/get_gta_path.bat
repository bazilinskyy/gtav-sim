@echo off

setlocal enableextensions

for /F "usebackq tokens=3*" %%A IN (`reg query "HKLM\SOFTWARE\Rockstar Games\Grand Theft Auto V" /v InstallFolder 2^>nul`) DO (
    set APP_DIR=%%A %%B
    )

if defined APP_DIR (
    echo %APP_DIR%
    exit /B 0
)


for /F "usebackq tokens=3*" %%A IN (`reg query "HKLM\SOFTWARE\WOW6432Node\Rockstar Games\GTAV" /v InstallFolderSteam 2^>nul`) DO (
    set APP_DIR=%%A %%B
    )

if defined APP_DIR (
    echo %APP_DIR%\..
    exit /B 0
)

echo "GTAV not found"
exit /B 1
