# C# Interop Billing

TypeSharp project that consumes an explicit local `net48` C# DLL. The legacy DLL exposes a customer repository, invoice rules with optional/named arguments, an `out` parameter API, and a `params` formatter.

## Code Walkthrough

This C# block is compiled first into `lib/Legacy.Tools.dll`. It represents the kind of existing .NET Framework library TypeSharp must consume without rewriting the legacy code:

```csharp
public static class LegacyInvoiceRules
{
    public static int CalculateRenewalTotal(int baseLicenseFee, int supportFee = 250)
    {
        return baseLicenseFee + supportFee;
    }

    public static bool TryGetCreditLimit(string accountId, out int creditLimit)
    {
        creditLimit = accountId == "CUST-0042" ? 5000 : 1000;
        return true;
    }
}
```

This TypeSharp block imports public C# metadata from the local DLL, uses named optional arguments, and calls a `params` formatter. The generated C# keeps those interop calls explicit:

```tysh
export fun renewalSummary(): string {
  let customer = LegacyCustomerRepository.Load(42)
  let total = LegacyInvoiceRules.CalculateRenewalTotal(baseLicenseFee: customer.BaseLicenseFee, supportFee: 300)
  LegacyBatchFormatter.Join(" | ", customer.AccountId, customer.DisplayName, total.ToString())
}
```

This TypeSharp block shows the `out` parameter case. The local mutable variable is addressable, so the checker can validate the byref call before generated C# compilation:

```tysh
export fun creditLimit(): int {
  let mut limit: int = 0
  LegacyInvoiceRules.TryGetCreditLimit("CUST-0042", out limit)
  limit
}
```

## Commands

This command block builds the legacy C# DLL, checks TypeSharp against that DLL metadata, and then builds the generated TypeSharp `net48` library:

```powershell
dotnet build legacy-src/Legacy.Tools.csproj --nologo --verbosity quiet --ignore-failed-sources
typesharp check
typesharp build
```

The legacy build writes `lib/Legacy.Tools.dll`, and `TypeSharp.toml` references that DLL through `references.paths`. The generated TypeSharp library exposes `renewalSummary`, `creditLimit`, and `batchLine`.

