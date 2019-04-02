@echo off
set current-path=%~dp0
rem // remove trailing slash
set current-path=%current-path:~0,-1%
set build_root=%current-path%

if /I not '%1' == 'regasadmin' goto :elevate
set servers=%2
goto :register

:elevate
set release=Release
if /I '%1' == '--debug' set release=Debug
set servers=%build_root%\BuildOutput\bin\servers\Win32\%release%
if not exist %servers% echo No servers built && exit /b 1
echo Unregistering servers...
powershell -Command "Start-Process cmd -ArgumentList '/c %~f0 regasadmin %servers%' -Verb RunAs"
echo All servers unregistered.
goto :eof

:register
pushd %servers%
for /f "delims=;" %%i in ('dir /b *.exe') do "%%i" /unregserver
popd
goto :eof
