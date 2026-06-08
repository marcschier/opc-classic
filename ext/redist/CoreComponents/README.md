# OPC Classic Core Components

Build system for the OPC Classic COM proxy/stub DLLs, server enumerator,
and native OPC Foundation test applications. These are the core runtime
components that OPC Classic (COM/DCOM) clients and servers need to communicate.

## What's in this repo

- **8 proxy/stub DLLs** — COM marshalling for OPC DA, A&E, HDA, Batch,
  Commands, Data Exchange, Security, and Common interfaces
- **OpcEnum.exe** — OPC Server Enumerator service (x86)
- **OpcCategoryManager.exe** — OPC component category registration (x64)
- **Test Applications** — OPC DA 2.05a test server and test client (x86 and x64),
  with sources relocated to the repository `samples/` tree.

The test applications allow you to quickly verify that x86 COM servers can be
discovered by an x64 client and vice versa. On a 64-bit system with the test
components registered, running either the x86 or x64 version of the test client
should show two servers (one for each platform).

## Prerequisites

### Visual Studio 2022

Install **Visual Studio 2022 17.14+** (Community, Professional, or Enterprise)
with the **Desktop development with C++** workload.

The build requires MSVC CRT >= 14.44.35211 for Windows 7 SP1 compatibility.
Both x86 and x64 builds use static CRT (`/MT`) and target Windows 7 SP1
(subsystem version 6.01).

### CMake

CMake 3.20 or later. Included with Visual Studio (Desktop C++ workload) or
install separately from https://cmake.org.

### AzureSignTool (for code signing)

Required only when code signing is enabled:

```
dotnet tool install --global AzureSignTool
```

## Build

Run from a **Developer Command Prompt for VS** (or any shell where
`vcvarsall.bat` has been called):

```powershell
.\build.ps1                            # both platforms (default)
.\build.ps1 -Platform x86             # x86 only
.\build.ps1 -Platform x64             # x64 only
.\build.ps1 -Clean                    # clean rebuild
. .\signing-key.ps1; .\build.ps1      # with code signing
```

### Parameters / environment variables

| Parameter | Env Variable | Default | Description |
|---|---|---|---|
| `-Platform` | | `both` | Target platform: `x86`, `x64`, or `both` |
| `-BuildRoot` | `BUILD_ROOT` | `.\build` | CMake build directory |
| `-OutDir` | `OUT_DIR` | `.\out` | Output directory for binaries and SDK headers |

### Code signing

Code signing uses Azure Key Vault HSM via AzureSignTool. To enable signing,
dot-source `signing-key.ps1` before running the build:

```powershell
. .\signing-key.ps1
.\build.ps1
```

If `SigningClientSecret` is empty or not set, signing is skipped automatically.
If credentials are set but AzureSignTool is not installed, the build fails
immediately with an error message.

Signing applies to built DLLs and EXEs after install.

### Spectre mitigation (optional)

To enable `/Qspectre`, install the Spectre-mitigated libs from VS Installer
(Individual Components tab), then add `-DOPC_ENABLE_SPECTRE=ON` to the
`cmake -S ...` line in `build.ps1`.

## Output

All output goes to the `out\` directory:

```
out\
  x86\bin\          proxy/stub DLLs, OpcEnum.exe, test server/client (32-bit)
  x64\bin\          proxy/stub DLLs, OpcCategoryManager.exe, test server/client (64-bit)
  x86\include\      SDK headers, IDLs, reference DLL copies (32-bit)
  x64\include\      SDK headers, IDLs, reference DLL copies (64-bit)
```

## Security hardening

All DLLs and EXEs are built with static CRT (`/MT`) and these security flags:

| Flag | Description |
|---|---|
| `/GS` | Buffer security check (stack canaries) |
| `/sdl` | SDL checks (pointer overwrite, format strings) |
| `/guard:cf` | Control Flow Guard (enforced on Win 8.1+) |
| `/NXCOMPAT` | DEP (Data Execution Prevention) |
| `/DYNAMICBASE` | ASLR (Address Space Layout Randomization) |
| `/SAFESEH` | Safe SEH (x86 only) |
| `/HIGHENTROPYVA` | High-Entropy ASLR (x64 only) |
| `/Qspectre` | Spectre v1 mitigation (opt-in, see above) |
