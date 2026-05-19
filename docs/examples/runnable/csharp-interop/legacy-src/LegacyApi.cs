namespace Legacy.Tools
{
    public static class LegacyApi
    {
        public static string Echo(string value)
        {
            return value;
        }
    }

    public static class LegacyParams
    {
        public static string Join(string separator, params string[] values)
        {
            return string.Join(separator, values);
        }
    }
}
