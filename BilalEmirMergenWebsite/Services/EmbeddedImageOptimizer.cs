using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BilalEmirMergenWebsite.Data;
using Microsoft.EntityFrameworkCore;

namespace BilalEmirMergenWebsite.Services;

public static partial class EmbeddedImageOptimizer
{
    public static async Task OptimizeAsync(AppDbContext database, IWebHostEnvironment environment, ILogger logger)
    {
        var uploadDirectory = Path.Combine(environment.WebRootPath, "uploads", "optimized");
        Directory.CreateDirectory(uploadDirectory);
        var changed = false;

        var projects = await database.Projects.Where(item =>
            item.Description.Contains("data:image") || item.DescriptionEn.Contains("data:image") ||
            item.DescriptionTr.Contains("data:image") || item.DescriptionAr.Contains("data:image")).ToListAsync();
        foreach (var item in projects)
        {
            item.Description = Replace(item.Description, uploadDirectory, out var c1); changed |= c1;
            item.DescriptionEn = Replace(item.DescriptionEn, uploadDirectory, out var c2); changed |= c2;
            item.DescriptionTr = Replace(item.DescriptionTr, uploadDirectory, out var c3); changed |= c3;
            item.DescriptionAr = Replace(item.DescriptionAr, uploadDirectory, out var c4); changed |= c4;
        }

        var articles = await database.Articles.Where(item =>
            item.Content.Contains("data:image") || item.ContentEn.Contains("data:image") ||
            item.ContentTr.Contains("data:image") || item.ContentAr.Contains("data:image")).ToListAsync();
        foreach (var item in articles)
        {
            item.Content = Replace(item.Content, uploadDirectory, out var c1); changed |= c1;
            item.ContentEn = Replace(item.ContentEn, uploadDirectory, out var c2); changed |= c2;
            item.ContentTr = Replace(item.ContentTr, uploadDirectory, out var c3); changed |= c3;
            item.ContentAr = Replace(item.ContentAr, uploadDirectory, out var c4); changed |= c4;
        }

        if (!changed) return;
        await database.SaveChangesAsync();
        logger.LogInformation("Embedded portfolio images were moved to /uploads/optimized without removing content.");
    }

    private static string Replace(string html, string uploadDirectory, out bool changed)
    {
        changed = false;
        if (string.IsNullOrWhiteSpace(html) || !html.Contains("data:image", StringComparison.OrdinalIgnoreCase)) return html;
        var replaced = false;
        var result = DataImageRegex().Replace(html, match =>
        {
            try
            {
                var bytes = Convert.FromBase64String(Regex.Replace(match.Groups["data"].Value, "\\s", string.Empty));
                var extension = match.Groups["type"].Value.ToLowerInvariant() switch { "jpeg" or "jpg" => "jpg", "gif" => "gif", "webp" => "webp", _ => "png" };
                var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                var fileName = $"{hash}.{extension}";
                var path = Path.Combine(uploadDirectory, fileName);
                if (!File.Exists(path)) File.WriteAllBytes(path, bytes);
                replaced = true;
                return $"/uploads/optimized/{fileName}";
            }
            catch (FormatException) { return match.Value; }
        });
        changed = replaced;
        return result;
    }

    [GeneratedRegex(@"data:image/(?<type>png|jpe?g|gif|webp);base64,(?<data>[A-Za-z0-9+/=\r\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DataImageRegex();
}
