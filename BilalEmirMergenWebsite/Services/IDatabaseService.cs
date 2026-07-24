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
        Task IncrementArticleViewsAsync(string slug);
        Task<Article> AddArticleAsync(Article article);
        Task<Article> UpdateArticleAsync(string id, Article article);
        Task DeleteArticleAsync(string id);

        // Projects
        Task<List<Project>> GetProjectsAsync();
        Task<Project> AddProjectAsync(Project project);
        Task DeleteProjectAsync(string id);

        // Stats / Analytics
        Task<int> GetTotalViewsAsync();
        Task<int> GetSiteVisitsAsync();
        Task IncrementSiteVisitsAsync(string path);
    }
}
