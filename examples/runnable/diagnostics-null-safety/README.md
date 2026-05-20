# Diagnostics Null Safety

Tooling workflow example for a strict nullability diagnostic.

```powershell
typesharp check --diagnostic-format json
```

Expected result:

```text
exit code: 1
diagnostic: TS2202
```

