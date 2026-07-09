# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

Initial development toward the first Opc.Classic release. See
[docs/ROADMAP.md](docs/ROADMAP.md) for the planned scope and open gates.

### Changed

- **Build / versioning:** adopt [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
  for build-time version derivation. Versions now come from the repo-root
  [`version.json`](version.json) + git height instead of a hard-coded
  `<Version>` in `src/Directory.Build.props`. Release versions remain
  tag-driven: the release workflow stamps the tag version into
  `version.json` and builds with `-p:PublicRelease=true` so the published
  package version is exactly the tag. Adds the `nbgv` CLI as a local
  dotnet tool and `fetch-depth: 0` to CI checkouts so nbgv can read the
  full git history.
