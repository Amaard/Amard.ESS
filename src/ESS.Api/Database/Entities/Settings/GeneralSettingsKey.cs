using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ESS.Api.Database.Entities.Settings;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneralSettingsKey
{
    [EnumMember(Value = "paySlipImageFolder")]
    PaySlipImageFolder
}
