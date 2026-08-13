namespace BilalEmirMergenWebsite.Services;

public interface IImageService
{
    Task<string?> SaveImageAsync(IFormFile? file, string folder, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> SaveImagesAsync(IEnumerable<IFormFile>? files, string folder, CancellationToken cancellationToken = default);
    Task<string?> SavePdfAsync(IFormFile? file, string folder, CancellationToken cancellationToken = default);
}

public sealed class ImageService : IImageService
{
    private const long MaxImageBytes = 8 * 1024 * 1024;
    private const long MaxPdfBytes = 12 * 1024 * 1024;
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp" };
    private readonly IWebHostEnvironment _environment;

    public ImageService(IWebHostEnvironment environment) => _environment = environment;

    public async Task<string?> SaveImageAsync(IFormFile? file, string folder, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > MaxImageBytes) throw new InvalidOperationException("Images must be 8 MB or smaller.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageExtensions.Contains(extension)) throw new InvalidOperationException("Upload a PNG, JPG, or WebP image.");
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("The uploaded file is not a valid image.");

        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var read = await input.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        if (!HasValidImageSignature(header, read, extension)) throw new InvalidOperationException("The uploaded image content does not match its file type.");
        input.Position = 0;

        return await SaveAsync(input, folder, extension, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> SaveImagesAsync(IEnumerable<IFormFile>? files, string folder, CancellationToken cancellationToken = default)
    {
        var saved = new List<string>();
        foreach (var file in files?.Where(item => item.Length > 0).Take(12) ?? Enumerable.Empty<IFormFile>())
        {
            var path = await SaveImageAsync(file, folder, cancellationToken);
            if (path is not null) saved.Add(path);
        }
        return saved;
    }

    public async Task<string?> SavePdfAsync(IFormFile? file, string folder, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > MaxPdfBytes) throw new InvalidOperationException("PDF files must be 12 MB or smaller.");
        if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Upload a PDF file.");
        await using var input = file.OpenReadStream();
        var header = new byte[5];
        var read = await input.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        if (read != 5 || !System.Text.Encoding.ASCII.GetString(header).Equals("%PDF-", StringComparison.Ordinal)) throw new InvalidOperationException("The uploaded file is not a valid PDF.");
        input.Position = 0;
        return await SaveAsync(input, folder, ".pdf", cancellationToken);
    }

    private async Task<string> SaveAsync(Stream input, string folder, string extension, CancellationToken cancellationToken)
    {
        var safeFolder = new string(folder.Where(character => char.IsLetterOrDigit(character) || character is '-' or '_').ToArray());
        if (string.IsNullOrWhiteSpace(safeFolder)) throw new InvalidOperationException("Invalid upload destination.");
        var relative = $"/uploads/{safeFolder}/{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_environment.WebRootPath, relative.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var output = File.Create(path);
        await input.CopyToAsync(output, cancellationToken);
        return relative;
    }

    private static bool HasValidImageSignature(byte[] header, int read, string extension) => extension switch
    {
        ".png" => read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
        ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
        ".webp" => read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
        _ => false
    };
}
