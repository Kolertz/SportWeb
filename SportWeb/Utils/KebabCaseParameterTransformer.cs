using System.Text.RegularExpressions;

namespace SportWeb.Services
{
    public partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null)
                return null;

            var str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return str;

            // Добавляем тире перед заглавными буквами и приводим к нижнему регистру
            return MyRegex().Replace(str, "$1-$2").ToLowerInvariant();
        }

        [GeneratedRegex("([a-z])([A-Z])")]
        private static partial Regex MyRegex();
    }
}