# Diagnostics Null Safety

Tooling workflow example for a strict nullability diagnostic. The source models a nullable customer profile lookup that is incorrectly returned through a non-null public boundary.

```powershell
typesharp check --diagnostic-format json
```

Expected result:

```text
exit code: 1
diagnostic: TS2202
```

