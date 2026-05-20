# Library Public API

Generated `net48` library with a C#-friendly billing account, quote, decision, and calculator API. The TypeSharp build emits the library, then the C# host smoke references the generated assembly plus TypeSharp Core/Runtime dependencies.

## Code Walkthrough

This TypeSharp block defines the public CLR-facing data model. These records are safe public API shapes because each one has a stable generated C# class name and constructor:

```tysh
public record BillingAccount(AccountId: string, DisplayName: string)

public record InvoiceQuote(Account: BillingAccount, LicenseFee: int, SupportFee: int)

public record InvoiceDecision(Quote: InvoiceQuote, Approved: bool, Total: int)
```

This TypeSharp block defines a public class intended for C# consumers. The methods operate on nominal records instead of structural shapes, so the generated metadata stays predictable:

```tysh
public class InvoiceCalculator {
  public fun Total(quote: InvoiceQuote): int =
    quote.LicenseFee + quote.SupportFee

  public fun Approve(quote: InvoiceQuote): InvoiceDecision =
    { Quote: quote, Approved: true, Total: quote.LicenseFee + quote.SupportFee }

  public fun Render(decision: InvoiceDecision): string =
    decision.Quote.Account.DisplayName + ":" + decision.Total.ToString()
}
```

This TypeSharp block gives the library a smokeable exported surface. The test builds the library and verifies a C# host can consume the same public record/class API:

```tysh
export fun sampleDecision(): InvoiceDecision {
  let account: BillingAccount = { AccountId: "CUST-1001", DisplayName: "Contoso Billing" }
  let quote: InvoiceQuote = { Account: account, LicenseFee: 900, SupportFee: 300 }
  InvoiceDecision(quote, true, quoteTotal(quote))
}
```

This C# block is the host-side proof: a normal `net48` project references the generated TypeSharp assembly, constructs the public records, calls `InvoiceCalculator`, and also references TypeSharp Core/Runtime libraries:

```csharp
var account = new BillingAccount("CUST-1001", "Contoso Billing");
var quote = new InvoiceQuote(account, 900, 300);
var calculator = new InvoiceCalculator();
var decision = calculator.Approve(quote);
return calculator.Render(decision)
    + ":"
    + TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework
    + ":"
    + TypeSharp.Core.Unit.Value.ToString();
```

## Commands

This command block checks TypeSharp source, builds the generated `net48` library, and compiles the C# consumer project against that generated assembly:

```powershell
typesharp check
typesharp build
dotnet build host/LibraryConsumerSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources
```

The build emits `generated/bin/Debug/net48/LibraryPublicApi.dll`.
