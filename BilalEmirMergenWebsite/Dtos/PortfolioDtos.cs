using System.ComponentModel.DataAnnotations;

namespace BilalEmirMergenWebsite.Dtos;

public sealed record LoginRequest(string Email, string Password);
public sealed record TokenResponse(string AccessToken, DateTime ExpiresAtUtc, string TokenType = "Bearer");
public sealed record ReorderItem(string Id, int DisplayOrder);

public sealed record AboutDto(
    string Eyebrow,
    string Title,
    string Introduction,
    string Content,
    string ImageUrl,
    string CvUrl,
    string Location,
    string Email,
    bool IsAvailable,
    string Availability);

public sealed record ExperienceDto(
    string Id,
    string JobTitle,
    string Company,
    string Location,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    string Description,
    IReadOnlyList<string> BulletPoints,
    IReadOnlyList<string> Technologies,
    string CompanyLogoUrl,
    string Icon,
    string TimelineColor,
    int DisplayOrder);

public sealed record EducationDto(
    string Id,
    string School,
    string Degree,
    string Department,
    string Location,
    DateTime? StartDate,
    DateTime? EndDate,
    string Description,
    IReadOnlyList<string> BulletPoints,
    string LogoUrl,
    bool IsExchangeProgram,
    string TimelineColor,
    int DisplayOrder);

public sealed record SkillDto(string Id, string Name, string Icon, string IconProvider, string IconUrl, string BrandColor, string Description, int Level, decimal? YearsOfExperience, int DisplayOrder);
public sealed record SkillCategoryDto(string Id, string Name, string Icon, string AccentColor, int DisplayOrder, IReadOnlyList<SkillDto> Skills);

public sealed record ProjectDto(
    string Id,
    string Title,
    string Slug,
    string Category,
    string ShortDescription,
    string FullDescription,
    string Thumbnail,
    IReadOnlyList<string> GalleryImages,
    IReadOnlyList<string> Technologies,
    string GitHubUrl,
    string DemoUrl,
    string CaseStudyUrl,
    bool Featured,
    int DisplayOrder);

public sealed record LanguageDto(string Id, string Language, string Level, int Percentage, string Icon, int DisplayOrder);

public sealed record BlogPostDto(
    string Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    string CoverImage,
    string Category,
    IReadOnlyList<string> Tags,
    DateTime PublishedAt,
    int ReadingTimeMinutes,
    string SeoTitle,
    string SeoDescription,
    bool Featured,
    int DisplayOrder);

public sealed record SocialLinkDto(string Id, string Name, string Icon, string Url, int DisplayOrder);

public sealed class UploadRequest
{
    [Required] public IFormFile File { get; set; } = null!;
}
