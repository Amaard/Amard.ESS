using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ESS.Api.Database.Entities.Settings;

public enum GeneralSettingsKey
{
    [EnumMember(Value = "PaySlipImg")]
    PaymentReportImageFolderPath
}

public static class GeneralSettingsKeyExtensions
{
    public static bool IsValidKey(string key)
    {
        var enumValues = Enum.GetValues<GeneralSettingsKey>();

        foreach (var enumValue in enumValues)
        {
            var field = typeof(GeneralSettingsKey).GetField(enumValue.ToString());
            var enumMemberAttribute = field?.GetCustomAttribute<EnumMemberAttribute>();

            if (string.Equals(enumMemberAttribute?.Value, key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
