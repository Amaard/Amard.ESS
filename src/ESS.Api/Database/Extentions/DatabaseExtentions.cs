using System.Runtime.CompilerServices;
using ESS.Api.Database.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ESS.Api.Database.Extentions;

public static class DatabaseExtentions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
       using IServiceScope scope = app.Services.CreateScope();
       await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
       await using ApplicationIdentityDbContext identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Application database migrations applied successfully. ");

            await identityDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Identity database migrations applied successfully. ");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An Error occurred while applying database migrations. ");
            throw;
        }
    }
}
