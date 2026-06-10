#requires -Version 5.1
<#
.SYNOPSIS
    Grant the calling identity (or another account) DCOM Launch / Activation /
    Access permissions on the OPC Foundation TestServer AppID so non-admin
    callers can activate it for probe / interop testing.

.DESCRIPTION
    OPC Foundation TestServer (`OpcTestServer_x64.exe`, CLSID
    `{F8582CF9-88FB-11DA-A5ED-0060B0692061}`) is registered via
    `interop/tools/register-testserver.ps1`. The EXE's self-registration writes its
    AppID alias but does NOT install custom LaunchPermission / AccessPermission
    ACLs — so DCOM SCM falls back to the per-host
    `DefaultLaunchPermission` (typically Administrators + INTERACTIVE only).
    Non-admin probe callers (for example `REDMOND\<user>`) then trigger
    `CO_E_SERVER_EXEC_FAILURE` because SCM denies the activation.

    This script delegates to `interop/tools/grant-opcenum-acl.ps1` (the same
    underlying SD-merge logic; see that script for the full description) with
    the TestServer's AppID. Each invocation appends
    `(A;;CCDCLCSWRP;;;<SID>)` ACEs for the supplied account; the operation
    is idempotent.

    Requires admin elevation from 64-bit PowerShell.

.PARAMETER Account
    Account to grant permissions to. Defaults to the calling user
    (DOMAIN\username or COMPUTER\username). Pass an explicit
    `DOMAIN\groupname` to grant to a group (e.g. `BUILTIN\Distributed COM Users`).

.PARAMETER Unregister
    Remove the ACE for `-Account` (or current user) from both AccessPermission
    and LaunchPermission. Other ACEs are preserved.

.EXAMPLE
    .\interop\tools\grant-testserver-acl.ps1
    # Grants the current user TestServer Launch + Activation + Access.

.EXAMPLE
    .\interop\tools\grant-testserver-acl.ps1 -Account "BUILTIN\Distributed COM Users"
    # Grants every member of the standard DCOM users group.

.EXAMPLE
    .\interop\tools\grant-testserver-acl.ps1 -Unregister
    # Removes the calling user's ACE from both descriptors.
#>

[CmdletBinding()]
param(
    [string]$Account,
    [switch]$Unregister
)

$ErrorActionPreference = 'Stop'

$script:TestServerAppId = '{F8582CF9-88FB-11DA-A5ED-0060B0692061}'
$delegateScript = Join-Path $PSScriptRoot 'grant-opcenum-acl.ps1'

if (-not (Test-Path -LiteralPath $delegateScript)) {
    Write-Error "Cannot find grant-opcenum-acl.ps1 at $delegateScript."
    exit 1
}

$delegateArgs = @{
    AppIdOverride = $script:TestServerAppId
}

if ($PSBoundParameters.ContainsKey('Account') -and -not [string]::IsNullOrWhiteSpace($Account)) {
    $delegateArgs['Account'] = $Account
}

if ($Unregister) {
    $delegateArgs['Unregister'] = $true
}

Write-Host "Delegating to grant-opcenum-acl.ps1 with AppID $script:TestServerAppId ..."
& $delegateScript @delegateArgs
exit $LASTEXITCODE
