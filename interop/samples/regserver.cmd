@echo off
rem Register OPC Foundation Sample Servers (Phase 14A/14B prerequisite)

set "SERVER_BIN=%~dp0BuildOutput\bin\servers\Win32\Release"

if exist "%SERVER_BIN%\OpcDaServer.exe" (
    "%SERVER_BIN%\OpcDaServer.exe" /RegServer
) else (
    echo Skipping missing "%SERVER_BIN%\OpcDaServer.exe"
)

if exist "%SERVER_BIN%\OpcAeServer.exe" (
    "%SERVER_BIN%\OpcAeServer.exe" /RegServer
) else (
    echo Skipping missing "%SERVER_BIN%\OpcAeServer.exe"
)

if exist "%SERVER_BIN%\OpcHdaServer.exe" (
    "%SERVER_BIN%\OpcHdaServer.exe" /RegServer
) else (
    echo Skipping missing "%SERVER_BIN%\OpcHdaServer.exe"
)

echo Native sample server registration attempted.
