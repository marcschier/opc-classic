@echo off
set current-path=%~dp0
rem // remove trailing slash
set current-path=%current-path:~0,-1%
set build_root=%current-path%\..

set release=Release
if /I '%1' == '--debug' set release=Debug
set servers=%build_root%\BuildOutput\bin\servers\Win32\%release%
echo Registering servers...
if not exist %servers% echo No servers built && exit /b 1
pushd %servers%
for /f "delims=;" %%i in ('dir /b *.exe') do "%%i" /unregserver
for /f "delims=;" %%i in ('dir /b *.exe') do echo %%i && "%%i" /regserver
echo All servers registered.
popd
goto :eof
