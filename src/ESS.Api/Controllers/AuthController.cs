using ESS.Api.Database.DatabaseContext;
using ESS.Api.Database.Entities.Users;
using ESS.Api.DTOs.Auth;
using ESS.Api.DTOs.Users;
using ESS.Api.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ESS.Api.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    ApplicationDbContext applicationDbContext,
    ApplicationIdentityDbContext identityDbContext,
    TokenProvider tokenProvider
    ) : ControllerBase
{

    [HttpPost("register")]
    public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto registerUserDto)
    {

        #region AmardIaf Validation
        //Implementation
        #endregion

        using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();
        applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

        var identityUser = new IdentityUser
        {
            UserName = registerUserDto.NationalCode,
            PhoneNumber = registerUserDto.PhoneNumber,
        };
        IdentityResult identityResult = await userManager.CreateAsync(identityUser);

        if (!identityResult.Succeeded)
        {
            var extensions = new Dictionary<string, object?>
            {
                {
                    "error",
                    identityResult.Errors.ToDictionary(e => e.Code , e => e.Description)
                }
            };

            return Problem(
                detail: "Unable to register user, Please try again!",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extensions);
        }

        User user = registerUserDto.ToEntity();
        user.IdentityId = identityUser.Id;

        applicationDbContext.Users.Add(user);
        await applicationDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.PhoneNumber);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        return Ok(accessTokens);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokensDto>> Login(LoginUserDto loginUserDto)
    {
        #region 2FA Authentication
        //Implementation
        #endregion
        IdentityUser? identityUser = await userManager.FindByNameAsync(loginUserDto.NationalCode);

        if (identityUser is null)
        {
            return Unauthorized();
        }

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.PhoneNumber!);

        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        return Ok(accessTokens);

    }
}
