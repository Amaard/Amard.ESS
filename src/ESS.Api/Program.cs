using ESS.Api;
using ESS.Api.Database.Extentions;



WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddApiServices()
       .AddErrorHandling()
       .AddDatabase()
       .AddObservability()
       .AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

await app.RunAsync();
