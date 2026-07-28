using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BilalEmirMergenWebsite.Services
{
    public class SqlDatabaseService : IDatabaseService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string ArticlesCacheKey = "Articles_List";
        private const string ProjectsCacheKey = "Projects_List";
        private const string SocialsCacheKey = "Socials_List";
        private const string ArticleCachePrefix = "Article_Slug_";

        public SqlDatabaseService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public bool IsDemo => false;

        // Auth
        public async Task<bool> SignInAsync(string email, string password)
        {
            try
            {
                var hashedPassword = HashPassword(password);
                var user = await _context.AdminUsers
                    .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashedPassword);
                return user != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignInAsync Hatası: {ex}");
                return false;
            }
        }

        public Task SignOutAsync()
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            // Controller uses Session state, so returning true here allows fallback flow
            return Task.FromResult(true);
        }

        public string GetCurrentUserEmail()
        {
            return "admin@bilal.com";
        }

        // Articles
        public async Task<List<Article>> GetArticlesAsync()
        {
            if (!_cache.TryGetValue(ArticlesCacheKey, out List<Article>? articles))
            {
                try
                {
                    articles = await _context.Articles
                        .AsNoTracking()
                        .OrderByDescending(a => a.CreatedAt)
                        .Select(a => new Article
                        {
                            Id = a.Id,
                            Title = a.Title,
                            Slug = a.Slug,
                            Summary = a.Summary,
                            CoverImage = a.CoverImage,
                            Views = a.Views,
                            CreatedAt = a.CreatedAt
                        })
                        .ToListAsync();

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(1))
                        .SetAbsoluteExpiration(TimeSpan.FromDays(1));

                    _cache.Set(ArticlesCacheKey, articles, cacheEntryOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetArticlesAsync Hatası: {ex}");
                    return new List<Article>();
                }
            }
            return articles ?? new List<Article>();
        }

        public async Task<Article?> GetArticleByIdAsync(string id)
        {
            try
            {
                return await _context.Articles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetArticleByIdAsync Hatası: {ex}");
                return null;
            }
        }

        public async Task<Article?> GetArticleBySlugAsync(string slug)
        {
            try
            {
                return await _context.Articles
                    .FirstOrDefaultAsync(a => a.Slug == slug);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetArticleBySlugAsync Hatası: {ex}");
                return null;
            }
        }

        public async Task IncrementArticleViewsAsync(string slug)
        {
            try
            {
                var article = await _context.Articles.FirstOrDefaultAsync(a => a.Slug == slug);
                if (article != null)
                {
                    article.Views++;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"IncrementArticleViewsAsync Hatası: {ex}");
            }
        }

        public async Task<Article> AddArticleAsync(Article article)
        {
            if (string.IsNullOrEmpty(article.Id))
            {
                article.Id = Guid.NewGuid().ToString();
            }
            article.CreatedAt = DateTime.UtcNow;
            article.Views = 0;

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            _cache.Remove(ArticlesCacheKey);
            return article;
        }

        public async Task<Article> UpdateArticleAsync(string id, Article article)
        {
            var existing = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (existing != null)
            {
                existing.Title = article.Title;
                existing.Slug = article.Slug;
                existing.Summary = article.Summary;
                existing.Content = article.Content;
                existing.CoverImage = article.CoverImage;
                await _context.SaveChangesAsync();
                _cache.Remove(ArticlesCacheKey);
                return existing;
            }
            return article;
        }

        public async Task DeleteArticleAsync(string id)
        {
            try
            {
                var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
                if (article != null)
                {
                    _context.Articles.Remove(article);
                    await _context.SaveChangesAsync();
                    _cache.Remove(ArticlesCacheKey);
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"DeleteArticleAsync Hatası: {ex}");
            }
        }

        // Projects
        public async Task<List<Project>> GetProjectsAsync()
        {
            if (!_cache.TryGetValue(ProjectsCacheKey, out List<Project>? projects))
            {
                try
                {
                    projects = await _context.Projects
                        .AsNoTracking()
                        .OrderByDescending(p => p.CreatedAt)
                        .ToListAsync();

                    foreach (var p in projects)
                    {
                        p.Description = StripBase64Images(p.Description);
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(1))
                        .SetAbsoluteExpiration(TimeSpan.FromDays(1));

                    _cache.Set(ProjectsCacheKey, projects, cacheEntryOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetProjectsAsync Hatası: {ex}");
                    return new List<Project>();
                }
            }
            return projects ?? new List<Project>();
        }

        public async Task<Project?> GetProjectByIdAsync(string id)
        {
            try
            {
                return await _context.Projects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetProjectByIdAsync Hatası: {ex}");
                return null;
            }
        }

        public async Task<Project> AddProjectAsync(Project project)
        {
            if (string.IsNullOrEmpty(project.Id))
            {
                project.Id = Guid.NewGuid().ToString();
            }
            project.CreatedAt = DateTime.UtcNow;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            _cache.Remove(ProjectsCacheKey);
            return project;
        }

        public async Task<Project> UpdateProjectAsync(string id, Project project)
        {
            var existing = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (existing != null)
            {
                existing.Title = project.Title;
                existing.Description = project.Description;
                existing.ImageUrl = project.ImageUrl;
                existing.ProjectUrl = project.ProjectUrl;
                existing.Tags = project.Tags;
                await _context.SaveChangesAsync();
                _cache.Remove(ProjectsCacheKey);
                return existing;
            }
            return project;
        }

        public async Task DeleteProjectAsync(string id)
        {
            try
            {
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
                if (project != null)
                {
                    _context.Projects.Remove(project);
                    await _context.SaveChangesAsync();
                    _cache.Remove(ProjectsCacheKey);
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"DeleteProjectAsync Hatası: {ex}");
            }
        }

        // Socials
        public async Task<List<Social>> GetSocialsAsync()
        {
            if (!_cache.TryGetValue(SocialsCacheKey, out List<Social>? socials))
            {
                try
                {
                    socials = await _context.Socials.ToListAsync();
                    _cache.Set(SocialsCacheKey, socials, TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetSocialsAsync Hatası: {ex}");
                    return new List<Social>();
                }
            }
            return socials ?? new List<Social>();
        }

        public async Task<Social> AddSocialAsync(Social social)
        {
            if (string.IsNullOrEmpty(social.Id))
            {
                social.Id = Guid.NewGuid().ToString();
            }
            _context.Socials.Add(social);
            await _context.SaveChangesAsync();
            _cache.Remove(SocialsCacheKey);
            return social;
        }

        public async Task<Social> UpdateSocialAsync(string id, Social social)
        {
            var existing = await _context.Socials.FirstOrDefaultAsync(s => s.Id == id);
            if (existing != null)
            {
                existing.Name = social.Name;
                existing.Icon = social.Icon;
                existing.Url = social.Url;
                await _context.SaveChangesAsync();
                _cache.Remove(SocialsCacheKey);
                return existing;
            }
            return social;
        }

        public async Task DeleteSocialAsync(string id)
        {
            try
            {
                var social = await _context.Socials.FirstOrDefaultAsync(s => s.Id == id);
                if (social != null)
                {
                    _context.Socials.Remove(social);
                    await _context.SaveChangesAsync();
                    _cache.Remove(SocialsCacheKey);
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"DeleteSocialAsync Hatası: {ex}");
            }
        }

        // Stats / Analytics
        public async Task<int> GetTotalViewsAsync()
        {
            try
            {
                return await _context.Articles.SumAsync(a => a.Views);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTotalViewsAsync Hatası: {ex}");
                return 0;
            }
        }

        public async Task<int> GetSiteVisitsAsync()
        {
            try
            {
                return await _context.Analytics
                    .CountAsync(a => a.EventType == "page_view");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetSiteVisitsAsync Hatası: {ex}");
                return 0;
            }
        }

        public async Task IncrementSiteVisitsAsync(string path)
        {
            try
            {
                var analytics = new Analytics
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "page_view",
                    PagePath = path,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Analytics.Add(analytics);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"IncrementSiteVisitsAsync Hatası: {ex}");
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private static string StripBase64Images(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            try
            {
                return System.Text.RegularExpressions.Regex.Replace(
                    html,
                    @"<img[^>]*src=[""']data:image/[^""']+;base64,[^""']+[""'][^>]*>",
                    string.Empty,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }
            catch
            {
                return html;
            }
        }
    }
}
