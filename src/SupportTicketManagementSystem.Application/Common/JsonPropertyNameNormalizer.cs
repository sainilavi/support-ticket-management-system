using System.Text.Json;

namespace SupportTicketManagementSystem.Application.Common;

public static class JsonPropertyNameNormalizer
{
    public static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        if (propertyName.Contains('.'))
        {
            return string.Join('.', propertyName.Split('.').Select(ToCamelCase));
        }

        return JsonNamingPolicy.CamelCase.ConvertName(propertyName);
    }
}
