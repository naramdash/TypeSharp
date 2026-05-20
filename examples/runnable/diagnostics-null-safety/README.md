# Diagnostics Null Safety

Tooling workflow example for a strict nullability diagnostic. The source models a nullable customer profile lookup that is incorrectly returned through a non-null public boundary.

## Code Walkthrough

This TypeSharp block is intentionally invalid. `loadProfile()` can return `null`, but `renderWelcomeEmail()` promises a non-null `CustomerProfile`, so strict nullability must report `TS2202`:

```tysh
public record CustomerProfile(DisplayName: string, Email: string)

fun loadProfile(): CustomerProfile? = null

export fun renderWelcomeEmail(): CustomerProfile = loadProfile()
```

## Commands

This command block runs only the checker and asks for JSON diagnostics so tooling can parse the `TS2202` result:

```powershell
typesharp check --diagnostic-format json
```

Expected result:

This output block is the expected failing shape: nonzero exit and a deterministic null-safety diagnostic code:

```text
exit code: 1
diagnostic: TS2202
```

