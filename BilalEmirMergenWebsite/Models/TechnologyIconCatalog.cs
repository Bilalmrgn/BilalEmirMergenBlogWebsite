namespace BilalEmirMergenWebsite.Models;

public sealed record TechnologyIconOption(string Name, string Slug, string BrandColor, string Group, params string[] Aliases)
{
    public string SearchText => string.Join(' ', new[] { Name, Slug }.Concat(Aliases)).ToLowerInvariant();
}

public static class TechnologyIconCatalog
{
    public static IReadOnlyList<TechnologyIconOption> All { get; } = new[]
    {
        new TechnologyIconOption("Angular", "angular", "#DD0031", "Frontend", "ng"),
        new TechnologyIconOption("React", "react", "#61DAFB", "Frontend", "reactjs"),
        new TechnologyIconOption("Vue.js", "vuedotjs", "#4FC08D", "Frontend", "vue"),
        new TechnologyIconOption("JavaScript", "javascript", "#F7DF1E", "Frontend", "js"),
        new TechnologyIconOption("TypeScript", "typescript", "#3178C6", "Frontend", "ts"),
        new TechnologyIconOption("HTML5", "html5", "#E34F26", "Frontend", "html"),
        new TechnologyIconOption("CSS3", "css", "#663399", "Frontend", "css"),
        new TechnologyIconOption("Bootstrap", "bootstrap", "#7952B3", "Frontend"),
        new TechnologyIconOption("C#", "csharp", "#512BD4", "Backend", "csharp", "c sharp"),
        new TechnologyIconOption(".NET", "dotnet", "#512BD4", "Backend", "net", "asp.net"),
        new TechnologyIconOption("Node.js", "nodedotjs", "#5FA04E", "Backend", "node", "nodejs"),
        new TechnologyIconOption("Java", "openjdk", "#437291", "Backend", "jdk"),
        new TechnologyIconOption("Python", "python", "#3776AB", "Backend", "py"),
        new TechnologyIconOption("C++", "cplusplus", "#00599C", "Backend", "cpp"),
        new TechnologyIconOption("SQL Server", "microsoftsqlserver", "#CC2927", "Database", "mssql", "sql server"),
        new TechnologyIconOption("PostgreSQL", "postgresql", "#4169E1", "Database", "postgres", "pgsql"),
        new TechnologyIconOption("MySQL", "mysql", "#4479A1", "Database"),
        new TechnologyIconOption("MongoDB", "mongodb", "#47A248", "Database", "mongo"),
        new TechnologyIconOption("Redis", "redis", "#FF4438", "Database", "cache", "caching"),
        new TechnologyIconOption("Git", "git", "#F05032", "Tools"),
        new TechnologyIconOption("GitHub", "github", "#181717", "Tools", "gh"),
        new TechnologyIconOption("Docker", "docker", "#2496ED", "DevOps", "container"),
        new TechnologyIconOption("Kubernetes", "kubernetes", "#326CE5", "DevOps", "k8s"),
        new TechnologyIconOption("Microsoft Azure", "microsoftazure", "#0078D4", "Cloud", "azure"),
        new TechnologyIconOption("AWS", "amazonwebservices", "#232F3E", "Cloud", "amazon web services"),
        new TechnologyIconOption("RabbitMQ", "rabbitmq", "#FF6600", "Messaging", "rabbit", "message queue"),
        new TechnologyIconOption("Apache Kafka", "apachekafka", "#231F20", "Messaging", "kafka"),
        new TechnologyIconOption("Hangfire", "dotnet", "#512BD4", "Messaging", "background jobs"),
        new TechnologyIconOption("Postman", "postman", "#FF6C37", "Tools", "api testing"),
        new TechnologyIconOption("Swagger", "swagger", "#85EA2D", "Tools", "openapi"),
        new TechnologyIconOption("Jenkins", "jenkins", "#D24939", "DevOps", "ci cd"),
        new TechnologyIconOption("GitLab", "gitlab", "#FC6D26", "Tools")
    };
}
