using System.Security.Cryptography;
using ESS.Api.Database.Entities.Users;
using ESS.Api.DTOs.Auth;

namespace ESS.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto)
    {
        return new User
        {
            Id = $"u_{Guid.CreateVersion7()}",
            Name = "Amirreza Ghasemi", //Needs to change,
            NationalCode = dto.NationalCode,
            PhoneNumber = dto.PhoneNumber,
            PersonalCode = "1000",
            CreatedAt = DateTime.UtcNow,
        };
    }
}
