using BilalEmirMergenWebsite.Dtos;
using BilalEmirMergenWebsite.Models;
using FluentValidation;

namespace BilalEmirMergenWebsite.Validation;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(model => model.Email).NotEmpty().MaximumLength(200);
        RuleFor(model => model.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}

public sealed class ExperienceValidator : AbstractValidator<Experience>
{
    public ExperienceValidator()
    {
        RuleFor(model => model.CompanyName).NotEmpty().MaximumLength(160);
        RuleFor(model => model.RoleEn).NotEmpty().MaximumLength(160);
        RuleFor(model => model.StartDate).NotEmpty();
        RuleFor(model => model.TimelineColor).Matches("^#[0-9a-fA-F]{6}$").When(model => !string.IsNullOrWhiteSpace(model.TimelineColor));
        RuleFor(model => model.EndDate).GreaterThanOrEqualTo(model => model.StartDate).When(model => model.EndDate.HasValue && !model.IsCurrent);
    }
}

public sealed class ProjectValidator : AbstractValidator<Project>
{
    public ProjectValidator()
    {
        RuleFor(model => model.TitleEn).NotEmpty().MaximumLength(180);
        RuleFor(model => model.Slug).MaximumLength(180);
        RuleFor(model => model.ShortDescriptionEn).MaximumLength(600);
        RuleFor(model => model.ProjectUrl).Must(BeUrlOrEmpty).WithMessage("Demo URL must be a valid absolute URL.");
        RuleFor(model => model.GitHubUrl).Must(BeUrlOrEmpty).WithMessage("GitHub URL must be a valid absolute URL.");
        RuleFor(model => model.CaseStudyUrl).Must(BeUrlOrEmpty).WithMessage("Case study URL must be a valid absolute URL.");
    }

    private static bool BeUrlOrEmpty(string value) => string.IsNullOrWhiteSpace(value) || Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
}

public sealed class ArticleValidator : AbstractValidator<Article>
{
    public ArticleValidator()
    {
        RuleFor(model => model.TitleEn).NotEmpty().MaximumLength(200);
        RuleFor(model => model.Slug).MaximumLength(180);
        RuleFor(model => model.SummaryEn).MaximumLength(600);
        RuleFor(model => model.SeoTitle).MaximumLength(70);
        RuleFor(model => model.SeoDescription).MaximumLength(170);
    }
}

public sealed class SocialValidator : AbstractValidator<Social>
{
    public SocialValidator()
    {
        RuleFor(model => model.Name).NotEmpty().MaximumLength(80);
        RuleFor(model => model.Icon).NotEmpty().MaximumLength(80);
        RuleFor(model => model.Url)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeContactUrl)
            .WithMessage("Enter a valid https://, mailto: or tel: address.");
    }

    private static bool BeContactUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" or "mailto" or "tel") return true;
        return value.StartsWith("/", StringComparison.Ordinal) && !value.StartsWith("//", StringComparison.Ordinal);
    }
}
