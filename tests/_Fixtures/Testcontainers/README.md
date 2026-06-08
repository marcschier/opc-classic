# Testcontainers fixtures

Cross-process integration tests use Testcontainers to spin up dependencies in disposable containers.

## Available fixtures

- **KerberosKdcFixture** — MIT Kerberos KDC for Kerberos integration tests. It runs `gcavalcante8808/krb5-server:latest`, exposes the KDC port, and returns `Realm`, `Host`, `Port`, and `Kdc` connection details.

## Requirements

Docker must be running locally. CI runs on a Linux runner with Docker preinstalled. Kerberos tests are gated by `OPC_CLASSIC_RUN_KDC_TESTS=1` so ordinary local test runs do not require Docker. The Samba WINREG smoke uses `external\docker\samba\docker-compose.yml` directly rather than a Testcontainers fixture.

## Adding a fixture

1. Place under `tests\_Fixtures\Testcontainers\<Name>Fixture.cs`
2. Implement `IAsyncDisposable`
3. Use `DotNet.Testcontainers.Builders.ContainerBuilder` to define the image + ports + env
4. Return strongly-typed connection info via fixture properties
