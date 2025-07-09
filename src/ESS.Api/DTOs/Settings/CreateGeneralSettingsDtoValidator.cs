using ESS.Api.Database.Entities.Settings;
using FluentValidation;

namespace ESS.Api.DTOs.Settings;

public sealed class CreateGeneralSettingsDtoValidator : AbstractValidator<CreateGeneralSettingsDto>
{
    public CreateGeneralSettingsDtoValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("Key is required.")
            .Must(GeneralSettingsKeyExtensions.IsValidKey)
            .WithMessage("Invalid key provided.");
        RuleFor(x => x.Value)
            .MaximumLength(2000)
            .WithMessage("Value cannot exceed 2000 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");
    }
}
