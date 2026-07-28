using BilalEmirMergenWebsite.Models;

namespace BilalEmirMergenWebsite.Services
{
    public interface IDatabaseService
    {
        bool IsDemo { get; }
        
        // Auth
        Task<bool> SignInAsync(string email, string password);
        Task SignOutAsync();
        Task<bool> IsAuthenticatedAsync();
        string GetCurrentUserEmail();

        // Articles
        Task<List<Article>> GetArticlesAsync();
        Task<Article?> GetArticleBySlugAsync(string slug);
        Task<Article?> GetArticleByIdAsync(string id);
        Task IncrementArticleViewsAsync(string slug);
        Task<Article> AddArticleAsync(Article article);
        Task<Article> UpdateArticleAsync(string id, Article article);
        Task DeleteArticleAsync(string id);

        // Projects
        Task<List<Project>> GetProjectsAsync();
        Task<Project?> GetProjectByIdAsync(string id);
        Task<Project> AddProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(string id, Project project);
        Task DeleteProjectAsync(string id);

        // Socials
        Task<List<Social>> GetSocialsAsync();
        Task<Social> AddSocialAsync(Social social);
        Task<Social> UpdateSocialAsync(string id, Social social);
        Task DeleteSocialAsync(string id);

        // Stats / Analytics
        Task<int> GetTotalViewsAsync();
        Task<int> GetSiteVisitsAsync();
        Task IncrementSiteVisitsAsync(string path);
    }
}
