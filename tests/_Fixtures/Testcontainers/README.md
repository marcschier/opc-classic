# Testcontainers fixtures

Cross-process integration tests use Testcontainers to spin up dependencies in disposable containers.

## Available fixtures

- **KerberosKdcFixture** — MIT Kerberos KDC for Phase 3D/3E/3F integration tests.

## Requirements

Docker must be running locally. CI runs on a Linux runner with Docker preinstalled.

## Adding a fixture

1. Place under `tests/_Fixtures/Testcontainers/<Name>Fixture.cs`
2. Implement `IAsyncDisposable`
3. Use `DotNet.Testcontainers.Builders.ContainerBuilder` to define the image + ports + env
4. Return strongly-typed connection info via fixture properties
