# Console Invoice

Small `net48` executable project for the CLI run workflow. It builds a realistic invoice draft from nominal records, computes a total, renders text with a framework `StringBuilder`, then returns the rendered line from `main`.

```powershell
typesharp check
typesharp build
typesharp run
```

Expected run output:

```text
Invoice Contoso Billing: MIG-48=900, SUP-12=300, total=1200
```

