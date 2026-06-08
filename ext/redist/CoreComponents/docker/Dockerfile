# escape=`
# OPC Core Components - Build Environment
#
# Contains all tools needed to build the proxy/stub DLLs and native test apps.
# Repository root is mounted at runtime (not baked into the image).
#
# Build:  docker build -t opc-core-builder docker
# Run:    docker run --rm -v <repo>:C:\repo -v <output>:C:\out opc-core-builder [x86|x64|both]

FROM mcr.microsoft.com/windows/servercore:ltsc2022

SHELL ["cmd", "/S", "/C"]

# ----------------------------------------------------------------------------
# 1. Visual Studio 2022 Build Tools
#    - C++ build tools workload (includes CMake, MSBuild, Windows SDK)
#    - ATL needed by OpcEnum.exe
# ----------------------------------------------------------------------------
RUN powershell -Command " `
    $ProgressPreference = 'SilentlyContinue'; `
    Invoke-WebRequest -Uri https://aka.ms/vs/17/release/vs_buildtools.exe -OutFile C:\vs_buildtools.exe" `
 && C:\vs_buildtools.exe --quiet --wait --norestart --nocache `
        --installPath C:\BuildTools `
        --add Microsoft.VisualStudio.Workload.VCTools `
        --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
        --add Microsoft.VisualStudio.Component.VC.ATL `
        --add Microsoft.VisualStudio.Component.VC.CMake.Project `
        --add Microsoft.VisualStudio.Component.Windows11SDK.22621 `
 && del /q C:\vs_buildtools.exe

# ----------------------------------------------------------------------------
# Entrypoint
# ----------------------------------------------------------------------------
COPY entrypoint.cmd C:\entrypoint.cmd

ENTRYPOINT ["cmd", "/S", "/C", "C:\\entrypoint.cmd"]
CMD ["both"]
