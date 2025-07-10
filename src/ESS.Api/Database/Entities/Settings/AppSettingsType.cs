using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ESS.Api.Database.Entities.Settings;

public enum AppSettingsType
{
    [Display(Name = "عمومی")]
    General = 0,

    [Display(Name = "حسابداری")]
    Acc = 1,

    [Display(Name = "کارگزینی")]
    HumanResource = 2,

    [Display(Name = "حقوق و دستمزد")]
    Payroll = 3,
}
