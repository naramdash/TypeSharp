namespace Samples.Runnable.PublicApi.Host
{
    public sealed class InvoiceConsumer
    {
        public string Render()
        {
            var account = new BillingAccount("CUST-1001", "Contoso Billing");
            var quote = new InvoiceQuote(account, 900, 300);
            var calculator = new InvoiceCalculator();
            var decision = calculator.Approve(quote);
            return calculator.Render(decision)
                + ":"
                + TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework
                + ":"
                + TypeSharp.Core.Unit.Value.ToString();
        }
    }
}
