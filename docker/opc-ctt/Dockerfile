# escape=`

# opc-classic/ctt — OPC Compliance Test Tool in a Windows container
#
# Bakes the six vendored CTT MSIs (~13 MB total, all under ext/private/ctt/)
# into a windows/servercore:ltsc2022 image. The container's ENTRYPOINT is
# a PowerShell shim (run-ctt.ps1) that locates OpcCtt.exe and runs it against
# a target ProgID + remote host, emitting an XML conformance report.
#
# Companion files in this folder:
#   - dcom-test-acls.reg : relaxed DCOM ACLs (anonymous access; test rig only)
#   - run-ctt.ps1        : the ENTRYPOINT shim
#   - README.md          : usage cookbook
#
# Build:
#   docker build --file docker/opc-ctt/Dockerfile --tag opc-classic/ctt .
# (run from the repository root so ext/private/ctt/ is on the build context)

FROM mcr.microsoft.com/windows/servercore:ltsc2022

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/

# ----------------------------------------------------------------------------
# Install the OPC CTT shell + plugins from the vendored MSIs.
#
# Install order matters: the Common Modules MSI provides OpcCtt.exe + the
# OPCEnum service that the plugin MSIs depend on. Plugins follow in spec
# order (DA 2.05a -> DA 3.0 -> AE -> HDA -> XML-DA).
#
# Exit codes 0 (success) and 3010 (reboot pending; harmless in CI) are
# acceptable; anything else is fatal.
# ----------------------------------------------------------------------------
COPY ["ext/private/ctt", "C:/ctt-msi/"]

RUN $msiOrder = @( `
        'Test Tool v2.0.15\OPC CTT Common Modules 2.0.15.msi', `
        'Data Access 2.05a Plugin v2.0.22\OPC CTT DataAccess2 Modules 2.0.22.msi', `
        'Data Access 3.0 Plugin v1.0.18\OPC CTT DataAccess3 Modules 1.0.18.msi', `
        'Alarm and Events Plugin v1.0.14\OPC CTT AlarmEvents Modules 1.0.14.msi', `
        'Historical Data Plugin v1.0.8\OPC CTT Historical DA Modules 1.0.8.msi', `
        'XML-DA Plugin v1.0.8\OPC CTT XMLDA Modules 1.0.8.msi' `
    ); `
    foreach ($rel in $msiOrder) { `
        $msi = 'C:/ctt-msi/' + $rel; `
        Write-Host ('Installing {0}' -f $msi); `
        $rc = (Start-Process msiexec -ArgumentList '/i', $msi, '/quiet', '/norestart' -Wait -PassThru).ExitCode; `
        if ($rc -ne 0 -and $rc -ne 3010) { throw ('msiexec failed: {0}' -f $rc) } `
    }; `
    Remove-Item -Path C:/ctt-msi -Recurse -Force

# ----------------------------------------------------------------------------
# Apply the DCOM ACL relaxations + pin the dynamic port range to 49152-49200
# so we can EXPOSE a narrow range below.
# ----------------------------------------------------------------------------
COPY ["docker/opc-ctt/dcom-test-acls.reg", "C:/dcom-test-acls.reg"]
RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg

# ----------------------------------------------------------------------------
# Ports: 135 = RPC endpoint mapper; 49152-49200 = pinned DCOM dynamic range.
# ----------------------------------------------------------------------------
EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp

# ----------------------------------------------------------------------------
# ENTRYPOINT shim. Parameters are forwarded directly:
#   docker run --rm --network opc-test-net opc-classic/ctt `
#     -ProgId Opc.Classic.DaSample.1 -TargetHost opc-classic-managed
# ----------------------------------------------------------------------------
COPY ["docker/opc-ctt/run-ctt.ps1", "C:/run-ctt.ps1"]
VOLUME ["C:/results"]
ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/run-ctt.ps1"]
