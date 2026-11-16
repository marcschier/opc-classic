# Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

import argparse
import hashlib
import os
from pathlib import Path
import subprocess
import tempfile
import xml.etree.ElementTree as ET
import zipfile


PACKAGE_IDS = (
    "Opc.Classic",
    "Opc.Classic.Windows",
    "Opc.Classic.Generators",
    "Opc.Classic.MigrationAnalyzer",
    "Opc.Classic.Mcp",
    "Opc.Classic.Mcp.Capture",
    "Opc.Classic.Mcp.win-x64",
    "Opc.Classic.Mcp.win-arm64",
    "Opc.Classic.Mcp.osx-arm64",
    "Opc.Classic.Mcp.linux-x64",
    "Opc.Classic.Mcp.linux-arm64",
    "Opc.Classic.Mcp.linux-musl-x64",
    "Opc.Classic.Core",
    "Opc.Classic.Ae",
    "Opc.Classic.Batch",
    "Opc.Classic.Commands",
    "Opc.Classic.Cpx",
    "Opc.Classic.Da",
    "Opc.Classic.Dcom",
    "Opc.Classic.Dcom.Kerberos",
    "Opc.Classic.Dcom.Smb",
    "Opc.Classic.Discovery",
    "Opc.Classic.Dx",
    "Opc.Classic.Hda",
    "Opc.Classic.Hosting",
    "Opc.Classic.Hosting.Windows",
    "Opc.Classic.Security",
    "Opc.Classic.Xml",
)


def package_inventory(directory, version):
    packages = {}
    for path in directory.glob("*.nupkg"):
        with zipfile.ZipFile(path) as archive:
            nuspecs = [name for name in archive.namelist() if name.endswith(".nuspec") and "/" not in name]
            if len(nuspecs) != 1:
                raise ValueError(f"{path.name}: expected exactly one root nuspec")
            metadata = ET.fromstring(archive.read(nuspecs[0])).find("{*}metadata")
            if metadata is None:
                raise ValueError(f"{path.name}: missing nuspec metadata")
            package_id = metadata.findtext("{*}id")
            package_version = metadata.findtext("{*}version")
            if package_id not in PACKAGE_IDS or package_version != version:
                raise ValueError(f"{path.name}: unexpected package identity {package_id} {package_version}")
            if path.name != f"{package_id}.{version}.nupkg":
                raise ValueError(f"{path.name}: filename does not match package identity")
            if package_id in packages:
                raise ValueError(f"Duplicate package ID: {package_id}")
            packages[package_id] = path
    missing = set(PACKAGE_IDS) - packages.keys()
    if missing:
        raise ValueError(f"Missing packages: {', '.join(sorted(missing))}")
    return packages


def verify_downloads(packages, version, owner, token):
    base = f"https://nuget.pkg.github.com/{owner}/download"
    with tempfile.TemporaryDirectory() as temporary_directory:
        downloaded = Path(temporary_directory) / "package.nupkg"
        for package_id, original in packages.items():
            lower_id = package_id.lower()
            lower_version = version.lower()
            url = f"{base}/{lower_id}/{lower_version}/{lower_id}.{lower_version}.nupkg"
            print(f"Verifying {package_id} {version}", flush=True)
            subprocess.run(
                [
                    "curl", "--fail", "--silent", "--show-error", "--location",
                    "--retry", "5", "--retry-delay", "5", "--retry-all-errors",
                    "--retry-max-time", "90", "--config", "-", "--output", str(downloaded), url,
                ],
                input=f'header = "Authorization: Bearer {token}"\n',
                text=True,
                check=True,
            )
            with original.open("rb") as local_file, downloaded.open("rb") as downloaded_file:
                local_hash = hashlib.file_digest(local_file, "sha256").digest()
                downloaded_hash = hashlib.file_digest(downloaded_file, "sha256").digest()
            if local_hash != downloaded_hash:
                raise ValueError(f"{package_id} {version}: GitHub feed bytes do not match the release package")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("mode", choices=("inventory", "verify"))
    parser.add_argument("version")
    parser.add_argument("directory", type=Path)
    args = parser.parse_args()
    packages = package_inventory(args.directory, args.version)
    print(f"Validated {len(packages)} package identities at {args.version}", flush=True)
    if args.mode == "verify":
        verify_downloads(packages, args.version, os.environ["GITHUB_REPOSITORY_OWNER"], os.environ["GITHUB_TOKEN"])


if __name__ == "__main__":
    main()
