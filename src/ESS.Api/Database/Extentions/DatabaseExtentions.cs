﻿using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace ESS.Api.Database.Extentions;

public static class DatabaseExtentions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
       using IServiceScope scope = app.Services.CreateScope();
       await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Database migrations applied successfully. ");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An Error occurred while applying database migrations. ");
            throw;
        }
    }
}
