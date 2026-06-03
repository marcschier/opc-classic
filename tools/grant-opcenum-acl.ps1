#requires -Version 5.1
<#
.SYNOPSIS
    Grant the calling identity (or another account) DCOM Launch / Activation /
    Access permissions on the OPCEnum AppID so opcclassic.discovery.enumerate_servers
    and ProgID-based connect flows succeed without per-call -username / -password.

.DESCRIPTION
    OPCEnum (`OPC.ServerList.1`, CLSID `{13486D51-4821-11D2-A494-3CB306C10000}`,
    AppID `{13486D44-4821-11D2-A494-3CB306C10000}` — note that CLSID and AppID
    differ in one hex digit; both come from the same OPC Foundation Core
    Components install)
    is used by every discovery and ProgID-based connect tool. On hardened
    Windows hosts (KB5004442+) DCOM activation requires both:

        1. At least RPC_C_AUTHN_LEVEL_PKT_INTEGRITY at the call layer (already
           handled by the managed stack — see docs/interop/opcenum-auth.md).
        2. An AppID-scope ACE granting the calling identity Local/Remote
           Launch + Activation + Access permissions.

    This script automates (2) by reading the OPCEnum AppID's existing
    `AccessPermission` and `LaunchPermission` REG_BINARY security descriptors,
    appending `(A;;CCDCLCSWRP;;;<SID>)` ACEs for the supplied account, and
    writing the merged descriptors back. Idempotent — re-running with the
    same `-Account` is a no-op once the ACE is present.

    Requires admin elevation from 64-bit PowerShell because the OPCEnum AppID
    lives under HKLM and the security descriptors are SACL-relevant.

.PARAMETER Account
    Account to grant permissions to. Defaults to the calling user
    (DOMAIN\username or COMPUTER\username). Pass an explicit
    `DOMAIN\groupname` to grant to a group (e.g. `BUILTIN\Distributed COM Users`).

.PARAMETER Unregister
    Remove the ACE for `-Account` (or current user) from both AccessPermission
    and LaunchPermission. Other ACEs are preserved.

.EXAMPLE
    .\tools\grant-opcenum-acl.ps1
    # Grants the current user OPCEnum Launch + Activation + Access.

.EXAMPLE
    .\tools\grant-opcenum-acl.ps1 -Account "CORP\opcprobe"
    # Grants a service account.

.EXAMPLE
    .\tools\grant-opcenum-acl.ps1 -Account "BUILTIN\Distributed COM Users"
    # Grants every member of the standard DCOM users group.

.EXAMPLE
    .\tools\grant-opcenum-acl.ps1 -Unregister
    # Removes the calling user's ACE from both descriptors.
#>

[CmdletBinding()]
param(
    [string]$Account,
    [switch]$Unregister,
    [string]$AppIdOverride
)

$ErrorActionPreference = 'Stop'

$script:OpcEnumAppId = if ($AppIdOverride) { $AppIdOverride } else { '{13486D44-4821-11D2-A494-3CB306C10000}' }
$script:OpcEnumAppIdRegPath = "HKLM:\SOFTWARE\Classes\AppID\$script:OpcEnumAppId"

function Test-IsAdministrator {
    $id = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object System.Security.Principal.WindowsPrincipal($id)
    return $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Resolve-AccountSid {
    param([Parameter(Mandatory)][string]$Account)
    try {
        $ntAccount = New-Object System.Security.Principal.NTAccount($Account)
        return $ntAccount.Translate([System.Security.Principal.SecurityIdentifier]).Value
    }
    catch {
        throw "Cannot resolve '$Account' to a SID. Pass a valid DOMAIN\name or BUILTIN\group. (Inner: $($_.Exception.Message))"
    }
}

function Get-AppIdSdBytes {
    param([Parameter(Mandatory)][string]$ValueName)
    $regKey = Get-Item -LiteralPath $script:OpcEnumAppIdRegPath -ErrorAction Stop
    $existing = $regKey.GetValue($ValueName, $null)
    if ($null -eq $existing) {
        return $null
    }
    if ($existing -isnot [byte[]]) {
        throw "OPCEnum AppID '$ValueName' is not REG_BINARY (got $($existing.GetType().Name)). Refusing to overwrite."
    }
    return $existing
}

function Set-AppIdSdBytes {
    param(
        [Parameter(Mandatory)][string]$ValueName,
        [Parameter(Mandatory)][byte[]]$Bytes
    )
    Set-ItemProperty -LiteralPath $script:OpcEnumAppIdRegPath -Name $ValueName -Value $Bytes -Type Binary -Force
}

# The 'CCDCLCSWRP' mask grants the DCOM call-execution + activation rights we need:
#   CC = COM_RIGHTS_EXECUTE (call execution)
#   DC = COM_RIGHTS_EXECUTE_LOCAL (local execution)
#   LC = COM_RIGHTS_EXECUTE_REMOTE (remote execution)
#   SW = COM_RIGHTS_ACTIVATE_LOCAL (local activation)
#   RP = COM_RIGHTS_ACTIVATE_REMOTE (remote activation)
# This matches what dcomcnfg.exe writes when you tick all six boxes
# (Local/Remote Access + Local/Remote Launch + Local/Remote Activation).
$script:AceTemplate = '(A;;CCDCLCSWRP;;;{0})'

function Get-DefaultSecurityDescriptor {
    # Local SYSTEM + Administrators + INTERACTIVE → preserves the Windows
    # default ACL so we never silently revoke what dcomcnfg starts with.
    # O:BAG:BAD: = Owner=BUILTIN\Administrators, Group=BUILTIN\Administrators, DACL...
    return 'O:BAG:BAD:(A;;CCDCLCSWRP;;;SY)(A;;CCDCLCSWRP;;;BA)(A;;CCDCLCSWRP;;;IU)'
}

function Add-AceForSid {
    param(
        [Parameter(Mandatory)][string]$ValueName,
        [Parameter(Mandatory)][string]$Sid
    )

    $existingBytes = Get-AppIdSdBytes -ValueName $ValueName
    $sd = New-Object System.Security.AccessControl.CommonSecurityDescriptor(
        $false, $false,
        $(if ($existingBytes) { (New-Object System.Security.AccessControl.RawSecurityDescriptor($existingBytes, 0)).GetSddlForm('All') } else { Get-DefaultSecurityDescriptor }))

    $sddl = $sd.GetSddlForm([System.Security.AccessControl.AccessControlSections]::All)
    $needle = $script:AceTemplate -f $Sid

    if ($sddl -notlike "*$needle*") {
        $sddl = $sddl.Replace('D:', "D:$needle")
        Write-Host "  Adding ACE for $Sid to $ValueName"
    }
    else {
        Write-Host "  ACE for $Sid already present in $ValueName"
        return
    }

    $sdNew = New-Object System.Security.AccessControl.RawSecurityDescriptor($sddl)
    $bytesNew = New-Object byte[] $sdNew.BinaryLength
    $sdNew.GetBinaryForm($bytesNew, 0)
    Set-AppIdSdBytes -ValueName $ValueName -Bytes $bytesNew
}

function Remove-AceForSid {
    param(
        [Parameter(Mandatory)][string]$ValueName,
        [Parameter(Mandatory)][string]$Sid
    )

    $existingBytes = Get-AppIdSdBytes -ValueName $ValueName
    if (-not $existingBytes) {
        Write-Host "  $ValueName not present; nothing to remove"
        return
    }

    $sd = New-Object System.Security.AccessControl.RawSecurityDescriptor($existingBytes, 0)
    $sddl = $sd.GetSddlForm([System.Security.AccessControl.AccessControlSections]::All)
    $needle = $script:AceTemplate -f $Sid

    if ($sddl -notlike "*$needle*") {
        Write-Host "  ACE for $Sid not present in $ValueName"
        return
    }

    $sddlNew = $sddl.Replace($needle, '')
    Write-Host "  Removing ACE for $Sid from $ValueName"
    $sdNew = New-Object System.Security.AccessControl.RawSecurityDescriptor($sddlNew)
    $bytesNew = New-Object byte[] $sdNew.BinaryLength
    $sdNew.GetBinaryForm($bytesNew, 0)
    Set-AppIdSdBytes -ValueName $ValueName -Bytes $bytesNew
}

if (-not (Test-IsAdministrator)) {
    Write-Error 'This script must be run from an elevated PowerShell window.'
    exit 1
}

if (-not [Environment]::Is64BitProcess) {
    Write-Error 'OPCEnum AppID lives under HKLM and must be edited from 64-bit PowerShell. Use %SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe.'
    exit 1
}

if (-not (Test-Path -LiteralPath $script:OpcEnumAppIdRegPath)) {
    Write-Error @"
OPCEnum AppID key not found at $script:OpcEnumAppIdRegPath.

Common causes:
  1. OPCEnum / OPC Core Components is not installed on this host.
     Install from https://opcfoundation.org/ (or vendor your OPC server
     setup, which usually bundles Core Components).
     Verify with: Get-Service OpcEnum

  2. Partial install: OPCEnum.exe + the OPC.ServerList.1 ProgID + the
     CLSID {13486D51-...} are registered, but the AppID key was skipped.
     Confirm CLSID->AppID linkage with:
         reg query "HKLM\SOFTWARE\Classes\CLSID\{13486D51-4821-11D2-A494-3CB306C10000}" /reg:32

     The 'AppID' named value under the CLSID points at the AppID GUID
     that THIS script expects ($script:OpcEnumAppId). If it differs,
     pass the right GUID via -AppIdOverride.

Note: the OPCEnum CLSID ({13486D51-...}) and AppID ({13486D44-...}) differ
in one hex digit. Both come from the same install but live under different
registry keys (CLSID under HKLM\SOFTWARE\Classes\CLSID, AppID under
HKLM\SOFTWARE\Classes\AppID).
"@
    exit 1
}

if (-not $Account) {
    $Account = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
}

$sid = Resolve-AccountSid -Account $Account
Write-Host "OPCEnum AppID: $script:OpcEnumAppId"
Write-Host "Account:       $Account"
Write-Host "SID:           $sid"

if ($Unregister) {
    Write-Host 'Removing OPCEnum ACE...'
    Remove-AceForSid -ValueName 'AccessPermission' -Sid $sid
    Remove-AceForSid -ValueName 'LaunchPermission' -Sid $sid
    Write-Host 'Done.'
    exit 0
}

Write-Host 'Granting OPCEnum Launch + Activation + Access permissions...'
Add-AceForSid -ValueName 'AccessPermission' -Sid $sid
Add-AceForSid -ValueName 'LaunchPermission' -Sid $sid
Write-Host 'Done. Verify with:'
Write-Host '  python tools\probe_servers.py --da-progid Matrikon.OPC.Simulation.1 --auth-level pkt_integrity --request-timeout 30'
