using System.ComponentModel.DataAnnotations;

namespace BilalEmirMergenWebsite.Models;

public interface IOrderedContent
{
    int SortOrder { get; set; }
    bool IsActive { get; set; }
}

public abstract class OrderedContent : IOrderedContent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AboutSection : OrderedContent
{
    public string EyebrowEn { get; set; } = string.Empty;
    public string EyebrowTr { get; set; } = string.Empty;
    public string EyebrowAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleTr { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string ShortTextEn { get; set; } = string.Empty;
    public string ShortTextTr { get; set; } = string.Empty;
    public string ShortTextAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class Experience : OrderedContent
{
    [Required] public string CompanyName { get; set; } = string.Empty;
    public string CompanyUrl { get; set; } = string.Empty;
    [Required] public string RoleEn { get; set; } = string.Empty;
    public string RoleTr { get; set; } = string.Empty;
    public string RoleAr { get; set; } = string.Empty;
    public string LocationEn { get; set; } = string.Empty;
    public string LocationTr { get; set; } = string.Empty;
    public string LocationAr { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string BulletPointsEn { get; set; } = string.Empty;
    public string BulletPointsTr { get; set; } = string.Empty;
    public string BulletPointsAr { get; set; } = string.Empty;
    public string Icon { get; set; } = "briefcase-business";
    public string Technologies { get; set; } = string.Empty;
    public string CompanyLogoUrl { get; set; } = string.Empty;
    public string TimelineColor { get; set; } = "#2F9CF4";
}

public class Education : OrderedContent
{
    [Required] public string SchoolName { get; set; } = string.Empty;
    public string DegreeEn { get; set; } = string.Empty;
    public string DegreeTr { get; set; } = string.Empty;
    public string DegreeAr { get; set; } = string.Empty;
    public string FieldEn { get; set; } = string.Empty;
    public string FieldTr { get; set; } = string.Empty;
    public string FieldAr { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string LocationEn { get; set; } = string.Empty;
    public string LocationTr { get; set; } = string.Empty;
    public string LocationAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
    public string SchoolUrl { get; set; } = string.Empty;
    public string BulletPointsEn { get; set; } = string.Empty;
    public string BulletPointsTr { get; set; } = string.Empty;
    public string BulletPointsAr { get; set; } = string.Empty;
    public bool IsExchangeProgram { get; set; }
    public string TimelineColor { get; set; } = "#9B6CF0";
}

public class SkillCategory : OrderedContent
{
    [Required] public string NameEn { get; set; } = string.Empty;
    public string NameTr { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Icon { get; set; } = "layers-3";
    public string AccentColor { get; set; } = "#2F9CF4";
    public List<Skill> Skills { get; set; } = new();
}

public class Skill : OrderedContent
{
    [Required] public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "code-2";
    public string IconUrl { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string IconType { get; set; } = "simple-icons";
    public string BrandColor { get; set; } = "#2F9CF4";
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    [Required] public string CategoryId { get; set; } = string.Empty;
    public SkillCategory? Category { get; set; }
    [Range(0, 100)] public int Level { get; set; } = 80;
    [Range(0, 80)] public decimal? YearsOfExperience { get; set; }
}

public class EngineeringConcept : OrderedContent
{
    [Required] public string NameEn { get; set; } = string.Empty;
    [Required] public string NameTr { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Icon { get; set; } = "boxes";
    public string LinkUrl { get; set; } = string.Empty;
}

public class AcademicFoundation : OrderedContent
{
    public string TitleEn { get; set; } = string.Empty;
    public string TitleTr { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Icon { get; set; } = "book-open";
}

public class SpokenLanguage : OrderedContent
{
    [Required] public string NameEn { get; set; } = string.Empty;
    public string NameTr { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string LevelEn { get; set; } = string.Empty;
    public string LevelTr { get; set; } = string.Empty;
    public string LevelAr { get; set; } = string.Empty;
    public string CefrLevel { get; set; } = string.Empty;
    [Range(0, 100)] public int Percentage { get; set; } = 80;
    public string Icon { get; set; } = "languages";
}

public class SiteSettings
{
    public string Id { get; set; } = "main";
    public string BrandName { get; set; } = "BEM.dev";
    public string FullName { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleTr { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string SubtitleEn { get; set; } = string.Empty;
    public string SubtitleTr { get; set; } = string.Empty;
    public string SubtitleAr { get; set; } = string.Empty;
    public string HeadlineEn { get; set; } = string.Empty;
    public string HeadlineTr { get; set; } = string.Empty;
    public string HeadlineAr { get; set; } = string.Empty;
    public string IntroEn { get; set; } = string.Empty;
    public string IntroTr { get; set; } = string.Empty;
    public string IntroAr { get; set; } = string.Empty;
    public string LocationEn { get; set; } = string.Empty;
    public string LocationTr { get; set; } = string.Empty;
    public string LocationAr { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string AvailabilityEn { get; set; } = string.Empty;
    public string AvailabilityTr { get; set; } = string.Empty;
    public string AvailabilityAr { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    public string ContactTitleEn { get; set; } = string.Empty;
    public string ContactTitleTr { get; set; } = string.Empty;
    public string ContactTitleAr { get; set; } = string.Empty;
    public string ContactTextEn { get; set; } = string.Empty;
    public string ContactTextTr { get; set; } = string.Empty;
    public string ContactTextAr { get; set; } = string.Empty;
    public string CvEnglishFile { get; set; } = string.Empty;
    public string CvTurkishFile { get; set; } = string.Empty;
    public string SeoTitleEn { get; set; } = string.Empty;
    public string SeoTitleTr { get; set; } = string.Empty;
    public string SeoTitleAr { get; set; } = string.Empty;
    public string SeoDescriptionEn { get; set; } = string.Empty;
    public string SeoDescriptionTr { get; set; } = string.Empty;
    public string SeoDescriptionAr { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;
    [Url] public string CanonicalBaseUrl { get; set; } = "https://bilalmergen.com";
    public string OpenGraphImage { get; set; } = "/og-portfolio-light.png";
}

public class Service : OrderedContent
{
    public string TitleEn { get; set; } = string.Empty;
    public string TitleTr { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Icon { get; set; } = "braces";
    public string AccentColor { get; set; } = "#22d3ee";
}

public class Certificate : OrderedContent
{
    public string NameEn { get; set; } = string.Empty;
    public string NameTr { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool DoesNotExpire { get; set; }
    public string CredentialId { get; set; } = string.Empty;
    public string CredentialUrl { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string CertificateImage { get; set; } = string.Empty;
    public string OrganizationLogo { get; set; } = string.Empty;
}
