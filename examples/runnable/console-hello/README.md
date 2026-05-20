# Console Invoice

Small `net48` executable project for the CLI run workflow. It builds a realistic invoice draft from nominal records, computes a total, renders text with a framework `StringBuilder`, then returns the rendered line from `main`.

## Code Walkthrough

This TypeSharp block defines the public data shapes that become C#-visible nominal record types in the generated `net48` executable:

```tysh
public record InvoiceLine(Sku: string, Description: string, Amount: int)

public record InvoiceDraft(Customer: string, Primary: InvoiceLine, Support: InvoiceLine)
```

This TypeSharp block keeps the calculation and rendering logic behind the public record boundary. It shows ordinary function calls, property access, `int` addition, and a framework `StringBuilder` call chain that lowers to C#:

```tysh
fun invoiceTotal(draft: InvoiceDraft): int =
  draft.Primary.Amount + draft.Support.Amount

fun renderInvoice(draft: InvoiceDraft): string {
  let total: int = invoiceTotal(draft)
  let builder = StringBuilder()
  builder.Append("Invoice ")
  builder.Append(draft.Customer)
  builder.Append(": ")
  builder.Append(lineText(draft.Primary))
  builder.Append(", ")
  builder.Append(lineText(draft.Support))
  builder.Append(", total=")
  builder.Append(total.ToString())
  builder.ToString()
}
```

This TypeSharp block is the executable entry point named by `TypeSharp.toml`; `typesharp run` invokes it after building the generated `net48` executable:

```tysh
export fun main(): string {
  let migration: InvoiceLine = { Sku: "MIG-48", Description: "Legacy migration review", Amount: 900 }
  let support: InvoiceLine = { Sku: "SUP-12", Description: "Twelve month support retainer", Amount: 300 }
  let draft: InvoiceDraft = { Customer: "Contoso Billing", Primary: migration, Support: support }
  renderInvoice(draft)
}
```

## Commands

This command block checks the TypeSharp source, builds the generated C# project, and launches the generated executable through the CLI run path:

```powershell
typesharp check
typesharp build
typesharp run
```

Expected run output:

This output block is the stable smoke result when the generated executable can launch on the local machine:

```text
Invoice Contoso Billing: MIG-48=900, SUP-12=300, total=1200
```

