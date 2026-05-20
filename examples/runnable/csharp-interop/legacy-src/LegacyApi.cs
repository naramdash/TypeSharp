namespace Legacy.Tools
{
    public sealed class LegacyCustomerSnapshot
    {
        public LegacyCustomerSnapshot(string accountId, string displayName, int baseLicenseFee)
        {
            AccountId = accountId;
            DisplayName = displayName;
            BaseLicenseFee = baseLicenseFee;
        }

        public string AccountId { get; }

        public string DisplayName { get; }

        public int BaseLicenseFee { get; }
    }

    public static class LegacyCustomerRepository
    {
        public static LegacyCustomerSnapshot Load(int numericId)
        {
            return new LegacyCustomerSnapshot("CUST-" + numericId.ToString("0000"), "Contoso Billing", 900);
        }
    }

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

    public static class LegacyBatchFormatter
    {
        public static string Join(string separator, params string[] values)
        {
            return string.Join(separator, values);
        }
    }
}
