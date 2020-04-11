@echo off

REM Settings
set PROJECT=..\SharpBuster\SharpBuster.csproj
set BUILD_DIR=..\Build
set CONFIGURATION=Release
set SINGLE_FILE=true
set RID=%1

dotnet publish %PROJECT% -c %CONFIGURATION% -r %RID% -p:PublishDir=%BUILD_DIR%\%RID% -p:PublishSingleFile=%SINGLE_FILE%