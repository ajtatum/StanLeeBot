using BabouExtensions;

namespace StanLeeBot.Web.Helpers
{
    public static class StringExtensions
    {
        public static string ToNullIfEmpty(this string source)
        {
            return source.IsNullOrWhiteSpace() ? null : source;
        }
    }
}
