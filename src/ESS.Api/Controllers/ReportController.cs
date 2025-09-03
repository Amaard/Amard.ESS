﻿using ESS.Api.Database.DatabaseContext;
using ESS.Api.Database.Entities.Settings;
using ESS.Api.Database.Entities.Users;
using ESS.Api.DTOs.Reports;
using ESS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ESS.Api.Controllers;

[EnableRateLimiting("default")]
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

        string fileNamePattern = $"{await GetPersonalCodeAsync()}-{reportQuery.Year}-{reportQuery.Month}-{reportQuery.Level}";

        return await GetReportFileAsync(AppSettingsKey.PaymentReportImageFolderPath, fileNamePattern, "PaymentReport");
    }

    [HttpGet("personnel-file")]
    public async Task<IActionResult> GetPersonnelFileReport([FromQuery] PersonnelFileReportQuery reportQuery)
    {
        if (reportQuery.Year <= 0)
        {
            return BadRequest("Invalid query parameters. Year must be positive values.");
        }

        string fileNamePattern = $"h-{await GetPersonalCodeAsync()}-{reportQuery.Year}";

        return await GetReportFileAsync(AppSettingsKey.PersonnelFileImageFolderPath, fileNamePattern, "PersonnelFileReport");
    }

    private async Task<string> GetPersonalCodeAsync()
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        string? personalCode = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.PersonalCode)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(personalCode))
        {
            throw new InvalidOperationException("Personal code not found.");
        }

        return personalCode;
    }
    private async Task<IActionResult> GetReportFileAsync(string settingsKey, string expectedFileNamePattern, string reportType)
    {
        try
        {
            // Get folder path from settings
            string? imageFolderLocation = await dbContext.AppSettings
                .Where(s => s.Key == settingsKey)
                .Select(s => s.Value)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(imageFolderLocation))
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: $"{reportType} folder path not configured.");
            }

            if (!Directory.Exists(imageFolderLocation))
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: $"{reportType} folder does not exist.");
            }

            // Find matching file
            string? foundImagePath = FindMatchingFile(imageFolderLocation, expectedFileNamePattern);

            if (string.IsNullOrEmpty(foundImagePath))
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"{reportType} not found for the specified parameters: {expectedFileNamePattern}");
            }

            if (!System.IO.File.Exists(foundImagePath))
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    detail: $"{reportType} file exists in directory but is not accessible.");
            }

            // Read and return file
            return await ReadAndReturnFileAsync(foundImagePath);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("User not authenticated.");
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: $"Error accessing {reportType.ToLower()} folder: {ex.Message}");
        }
    }
    private string? FindMatchingFile(string folderLocation, string expectedFileNamePattern)
    {
        return Directory.GetFiles(folderLocation)
            .FirstOrDefault(file =>
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file).ToLowerInvariant();

                return fileNameWithoutExtension.Equals(expectedFileNamePattern, StringComparison.OrdinalIgnoreCase) &&
                       _allowedImageExtensions.Contains(extension);
            });
    }
    private async Task<IActionResult> ReadAndReturnFileAsync(string filePath)
    {
        try
        {
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            string fileName = Path.GetFileName(filePath);
            string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

            string contentType = fileExtension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".tiff" => "image/tiff",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            return File(fileBytes, contentType, fileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: "Access denied to report file.");
        }
        catch (IOException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: $"Error reading report file: {ex.Message}");
        }
    }
}
