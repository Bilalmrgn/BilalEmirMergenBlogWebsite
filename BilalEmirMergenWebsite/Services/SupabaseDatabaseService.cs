using BilalEmirMergenWebsite.Models;

namespace BilalEmirMergenWebsite.Services
{
    public class SupabaseDatabaseService : IDatabaseService
    {
        private readonly Supabase.Client _client;

        public SupabaseDatabaseService(Supabase.Client client)
        {
            _client = client;
        }

        public bool IsDemo => false;

        // Auth
        public async Task<bool> SignInAsync(string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignIn(email, password);
                return session != null && !string.IsNullOrEmpty(session.AccessToken);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            try
            {
                await _client.Auth.SignOut();
            }
            catch (Exception) { }
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            var isAuth = _client.Auth.CurrentSession != null;
            return Task.FromResult(isAuth);
        }

        public string GetCurrentUserEmail()
        {
            return _client.Auth.CurrentUser?.Email ?? string.Empty;
        }

        // Articles
        public async Task<List<Article>> GetArticlesAsync()
        {
            try
            {
                var response = await _client
                    .From<Article>()
                    .Order(a => a.CreatedAt, Postgrest.Constants.Ordering.Descending)
                    .Get();
                return response.Models;
            }
            catch (Exception)
            {
                return new List<Article>();
            }
        }

        public async Task<Article?> GetArticleBySlugAsync(string slug)
        {
            try
            {
                var response = await _client
                    .From<Article>()
                    .Filter(a => a.Slug, Postgrest.Constants.Operator.Equals, slug)
                    .Single();
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task IncrementArticleViewsAsync(string slug)
        {
            try
            {
                var article = await GetArticleBySlugAsync(slug);
                if (article != null)
                {
                    article.Views++;
                    await _client
                        .From<Article>()
                        .Where(a => a.Id == article.Id)
                        .Update(article);
                }
            }
            catch (Exception) { }
        }

        public async Task<Article> AddArticleAsync(Article article)
        {
            article.Id = Guid.NewGuid().ToString();
            article.CreatedAt = DateTime.UtcNow;
            article.Views = 0;
            var response = await _client.From<Article>().Insert(article);
            return response.Models.FirstOrDefault() ?? article;
        }

        public async Task<Article> UpdateArticleAsync(string id, Article article)
        {
            var existing = await _client.From<Article>().Where(a => a.Id == id).Single();
            if (existing != null)
            {
                existing.Title = article.Title;
                existing.Slug = article.Slug;
                existing.Summary = article.Summary;
                existing.Content = article.Content;
                existing.CoverImage = article.CoverImage;
                await _client.From<Article>().Where(a => a.Id == id).Update(existing);
                return existing;
            }
            return article;
        }

        public async Task DeleteArticleAsync(string id)
        {
            try
            {
                await _client.From<Article>().Where(a => a.Id == id).Delete();
            }
            catch (Exception) { }
        }

        // Projects
        public async Task<List<Project>> GetProjectsAsync()
        {
            try
            {
                var response = await _client
                    .From<Project>()
                    .Order(p => p.CreatedAt, Postgrest.Constants.Ordering.Descending)
                    .Get();
                return response.Models;
            }
            catch (Exception)
            {
                return new List<Project>();
            }
        }

        public async Task<Project> AddProjectAsync(Project project)
        {
            project.Id = Guid.NewGuid().ToString();
            project.CreatedAt = DateTime.UtcNow;
            var response = await _client.From<Project>().Insert(project);
            return response.Models.FirstOrDefault() ?? project;
        }

        public async Task<Project> UpdateProjectAsync(string id, Project project)
        {
            var existing = await _client.From<Project>().Where(p => p.Id == id).Single();
            if (existing != null)
            {
                existing.Title = project.Title;
                existing.Description = project.Description;
                existing.ImageUrl = project.ImageUrl;
                existing.ProjectUrl = project.ProjectUrl;
                existing.Tags = project.Tags;
                await _client.From<Project>().Where(p => p.Id == id).Update(existing);
                return existing;
            }
            return project;
        }

        public async Task DeleteProjectAsync(string id)
        {
            try
            {
                await _client.From<Project>().Where(p => p.Id == id).Delete();
            }
            catch (Exception) { }
        }

        // Socials
        public async Task<List<Social>> GetSocialsAsync()
        {
            try
            {
                var response = await _client
                    .From<Social>()
                    .Get();
                return response.Models;
            }
            catch (Exception)
            {
                return new List<Social>();
            }
        }

        public async Task<Social> AddSocialAsync(Social social)
        {
            social.Id = Guid.NewGuid().ToString();
            var response = await _client.From<Social>().Insert(social);
            return response.Models.FirstOrDefault() ?? social;
        }

        public async Task<Social> UpdateSocialAsync(string id, Social social)
        {
            var existing = await _client.From<Social>().Where(s => s.Id == id).Single();
            if (existing != null)
            {
                existing.Name = social.Name;
                existing.Icon = social.Icon;
                existing.Url = social.Url;
                await _client.From<Social>().Where(s => s.Id == id).Update(existing);
                return existing;
            }
            return social;
        }

        public async Task DeleteSocialAsync(string id)
        {
            try
            {
                await _client.From<Social>().Where(s => s.Id == id).Delete();
            }
            catch (Exception) { }
        }

        // Stats
        public async Task<int> GetTotalViewsAsync()
        {
            try
            {
                var articles = await GetArticlesAsync();
                return articles.Sum(a => a.Views);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> GetSiteVisitsAsync()
        {
            try
            {
                var result = await _client
                    .From<Analytics>()
                    .Filter(a => a.EventType, Postgrest.Constants.Operator.Equals, "page_view")
                    .Count(Postgrest.Constants.CountType.Exact);
                return result;
            }
            catch (Exception)
            {
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
                await _client.From<Analytics>().Insert(analytics);
            }
            catch (Exception) { }
        }
    }
}
