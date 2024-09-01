using System.Text.RegularExpressions;

namespace SportWeb.Services
{
    public class KebabCaseParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null)
                return null;

            var str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return str;

            // Добавляем тире перед заглавными буквами и приводим к нижнему регистру
            return Regex.Replace(str, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
        }
    }
}
