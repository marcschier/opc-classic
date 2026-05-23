# Test categories

The default repository test filter excludes environment-dependent suites:

```powershell
dotnet test Opc.Classic.slnx --filter "Category!=NativeConformance&Category!=MatrikonConformance&Category!=CompatMatrix&Category!=Kerberos"
```

- `Kerberos` — Docker/Testcontainers-backed MIT krb5 KDC integration tests. They are skipped unless Docker is running and `OPC_CLASSIC_RUN_KDC_TESTS=1` is set.
- `NativeConformance`, `MatrikonConformance`, `CompatMatrix` — external OPC server or compatibility suites.
