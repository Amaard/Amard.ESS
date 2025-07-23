using ESS.Api.Database.DatabaseContext;
using ESS.Api.Database.Entities.Settings;
using ESS.Api.Database.Entities.Users;
using ESS.Api.DTOs.Reports;
using ESS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESS.Api.Controllers;

[Authorize(Roles = Roles.Employee)]
[ApiController]
[Route("reports")]
public sealed class ReportController(ApplicationDbContext dbContext, UserContext userContext) : ControllerBase
{
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".tiff", ".bmp" };

    [HttpGet("payment")]
    public async Task<IActionResult> GetPaymentReport([FromQuery] PaymentReportQuery reportQuery)
    {

        if (reportQuery.Year <= 0 || reportQuery.Month < 1 || reportQuery.Month > 12 || reportQuery.Level <= 0)
        {
            return BadRequest("Invalid query parameters. Year, Month (1-12), and Level must be positive values.");
        }

        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("User not authenticated.");
        }

        string? personalCode = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.PersonalCode)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(personalCode))
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: "Personal code not found.");
        }

        string? imageFolderLocation = await dbContext.AppSettings
            .Where(s => s.Key == AppSettingsKey.PaymentReportImageFolderPath)
            .Select(s => s.Value)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(imageFolderLocation))
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Payment report image folder path not configured.");
        }

        if (!Directory.Exists(imageFolderLocation))
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Payment report image folder does not exist.");
        }

        string expectedFileNamePattern = $"{personalCode}-{reportQuery.Year}-{reportQuery.Month}-{reportQuery.Level}";

        string? foundImagePath = null;
        string? foundFileName = null;

        try
        {
            var matchingFiles = Directory.GetFiles(imageFolderLocation)
                .FirstOrDefault(file =>
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    string extension = Path.GetExtension(file).ToLowerInvariant();

                    return fileNameWithoutExtension.Equals(expectedFileNamePattern, StringComparison.OrdinalIgnoreCase) &&
                            _allowedImageExtensions.Contains(extension);
                });

            if (matchingFiles != null)
            {
                foundImagePath = matchingFiles;
                foundFileName = Path.GetFileName(matchingFiles);
            }
        }
        catch (Exception ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: $"Error accessing payment report folder: {ex.Message}");
        }

        if (string.IsNullOrEmpty(foundImagePath))
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Payment report not found for the specified parameters: {expectedFileNamePattern}");
        }

        if (!System.IO.File.Exists(foundImagePath))
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Payment report file exists in directory but is not accessible.");
        }

        try
        {
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(foundImagePath);

            string fileExtension = Path.GetExtension(foundImagePath).ToLowerInvariant();
            string contentType = fileExtension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".tiff" => "image/tiff",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            return File(fileBytes, contentType, foundFileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: "Access denied to payment report file.");
        }
        catch (IOException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: $"Error reading payment report file: {ex.Message}");
        }
    }
}
