namespace Common
{
    public static class Extensions
    {

        public static decimal? ToNullableDecimal(this string s)
        {
            decimal i;
            if (decimal.TryParse(s, out i)) return i;
            return null;
        }


        public static int? ToNullableInt(this string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }

        public static string TrimAndNullIfEmpty(this string value, bool toLower = false)
        {
            return string.IsNullOrWhiteSpace(value) ? null : toLower ? value.Trim().ToLower() : value.Trim();
        }

        public static string RemoveAllSpace(this string value)
        {
            return value.Replace(" ", "");
        }
    }
}