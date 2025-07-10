using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ESS.Api.Database.Entities.Settings;

public enum AppSettingsKey
{
    [EnumMember(Value = "PaymentReportImageFolderPath")]
    PaymentReportImageFolderPath
}

public static class AppSettingsKeyExtensions
{
    public static bool IsValidKey(string key)
    {
        var enumValues = Enum.GetValues<AppSettingsKey>();

        foreach (var enumValue in enumValues)
        {
            var field = typeof(AppSettingsKey).GetField(enumValue.ToString());
            var enumMemberAttribute = field?.GetCustomAttribute<EnumMemberAttribute>();

            if (string.Equals(enumMemberAttribute?.Value, key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
