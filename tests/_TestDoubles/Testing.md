# Mocking and test doubles

The project uses hand-written test doubles by default. This keeps tests aligned with the NativeAOT requirements for production code: no Castle.Core, no Reflection.Emit, no DispatchProxy, and no runtime proxy generation.

## Naming

- `FakeXxx` for general-purpose doubles with configurable behavior.
- `StubXxx` for narrow one-shot scaffolds local to a scenario.
- `CapturingXxx` for doubles whose main job is recording calls or state for assertions.

`InMemoryCallChannel` in `src\OpcClassic.Core\Testing\` is the canonical example: small, delegate-configured, and explicit about what it records.

## When to add one

Add a shared double when multiple tests need the same interface seam. Keep it per-interface, around 50 LOC, and free of production logic. Prefer delegates for behavior and simple collections for captured calls.

For `ILogger` assertions, use the Microsoft fake logger APIs from `Microsoft.Extensions.Diagnostics.Testing` (`Microsoft.Extensions.Logging.Testing` namespace). If a logger scenario outgrows that package, add a small hand-written logger double here.

See `CONTRIBUTING.md` for contributor policy and `docs\ARCHITECTURE.md` for the broader NativeAOT architecture constraints.
