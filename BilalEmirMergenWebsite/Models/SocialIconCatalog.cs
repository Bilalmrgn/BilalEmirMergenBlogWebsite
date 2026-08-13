namespace BilalEmirMergenWebsite.Models;

public sealed record SocialIconOption(string Name, string Icon, params string[] Aliases)
{
    public string SearchText => string.Join(' ', new[] { Name, Icon }.Concat(Aliases)).ToLowerInvariant();
}

public static class SocialIconCatalog
{
    public static IReadOnlyList<SocialIconOption> All { get; } = new[]
    {
        new SocialIconOption("GitHub", "github", "code repository git"),
        new SocialIconOption("LinkedIn", "linkedin", "career professional work"),
        new SocialIconOption("Email", "mail", "email contact envelope"),
        new SocialIconOption("Website", "globe-2", "web portfolio homepage"),
        new SocialIconOption("X / Twitter", "twitter", "x social tweet"),
        new SocialIconOption("Instagram", "instagram", "photo social"),
        new SocialIconOption("YouTube", "youtube", "video channel"),
        new SocialIconOption("Facebook", "facebook", "social"),
        new SocialIconOption("Medium", "book-open", "writing article blog"),
        new SocialIconOption("Dev.to", "code-2", "developer writing blog"),
        new SocialIconOption("Stack Overflow", "layers-3", "developer questions answers"),
        new SocialIconOption("GitLab", "gitlab", "code repository git"),
        new SocialIconOption("CodePen", "codepen", "frontend demos code"),
        new SocialIconOption("Discord", "message-circle", "community chat"),
        new SocialIconOption("Telegram", "send", "message chat"),
        new SocialIconOption("WhatsApp", "message-circle-more", "phone chat"),
        new SocialIconOption("Phone", "phone", "call contact"),
        new SocialIconOption("Calendar", "calendar-days", "meeting schedule"),
        new SocialIconOption("External link", "external-link", "url link"),
        new SocialIconOption("Generic link", "link", "url chain")
    };
}
