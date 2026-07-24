using BilalEmirMergenWebsite.Models;

namespace BilalEmirMergenWebsite.Services
{
    public class MockDatabaseService : IDatabaseService
    {
        private static readonly List<Article> _articles = new()
        {
            new Article
            {
                Id = "1",
                Title = "ASP.NET Core ve Supabase ile Blog Geliştirmek",
                Slug = "aspnet-core-ve-supabase-ile-blog-gelistirmek",
                Summary = "Bu makalede, modern bir .NET 9 web uygulamasına popüler PostgreSQL bulut çözümü Supabase'i nasıl entegre edeceğinizi öğrenin.",
                Content = "# ASP.NET Core ve Supabase\n\nC# dünyasında serverless mimariler gün geçtikçe daha popüler hale geliyor. Bu makalede Supabase istemcisini .NET projelerinize nasıl dahil edeceğinizi anlatıyoruz.\n\n## 1. Paket Kurulumu\nİlk adımda NuGet üzerinden `supabase-csharp` paketini kuruyoruz.\n\n## 2. Model Tanımları\nTablolarımızı C# sınıfları olarak temsil etmek için `BaseModel` ve attribute yapısını kullanıyoruz.\n\n## 3. Servis Tescili\nProgram.cs'de DI (Dependency Injection) olarak servisimizi kaydediyoruz.",
                Views = 135,
                CoverImage = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=800&q=80",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Article
            {
                Id = "2",
                Title = "Yapay Zeka ve Yazılım Geliştiriciliğinin Geleceği",
                Slug = "yapay-zeka-ve-yazilim-gelistiriciligi",
                Summary = "Yapay zeka asistanları kod yazma süreçlerimizi nasıl değiştiriyor? Geliştiriciler olarak nasıl konumlanmalıyız?",
                Content = "# Yapay Zeka ve Yazılım\n\nLLM modellerinin (Gemini, Copilot vb.) kod yazma yetenekleri her geçen gün artıyor. Peki bu durum geliştiricilerin sonu mu, yoksa yeni bir çağın başlangıcı mı?\n\n## Yeni Rolümüz: Kod Tasarımcısı\nArtık sadece kod yazan değil, kodun kalitesini, mimarisini ve entegrasyonunu denetleyen konumdayız.",
                Views = 78,
                CoverImage = "https://images.unsplash.com/photo-1677442136019-21780efad99a?auto=format&fit=crop&w=800&q=80",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        private static readonly List<Project> _projects = new()
        {
            new Project
            {
                Id = "1",
                Title = "E-Ticaret Analiz Platformu",
                Description = "Büyük ölçekli satış verilerini işleyen ve grafiklerle sunan web dashboard uygulaması.",
                ImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?auto=format&fit=crop&w=800&q=80",
                ProjectUrl = "https://github.com/Bilalmrgn",
                Tags = new List<string> { "ASP.NET Core", "Supabase", "React" }
            },
            new Project
            {
                Id = "2",
                Title = "Kişisel Blog Motoru",
                Description = "C# ve Markdown uyumlu, performansı optimize edilmiş kişisel içerik yönetim sistemi.",
                ImageUrl = "https://images.unsplash.com/photo-1499750310107-5fef28a66643?auto=format&fit=crop&w=800&q=80",
                ProjectUrl = "https://github.com/Bilalmrgn",
                Tags = new List<string> { ".NET 9", "CSS3", "JavaScript" }
            }
        };

        private static int _siteVisits = 421;
        private static bool _isAuthenticated = false;
        private static string _currentUserEmail = string.Empty;

        public bool IsDemo => true;

        // Auth
        public Task<bool> SignInAsync(string email, string password)
        {
            if (email == "admin@bilal.com" && password == "admin123")
            {
                _isAuthenticated = true;
                _currentUserEmail = email;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task SignOutAsync()
        {
            _isAuthenticated = false;
            _currentUserEmail = string.Empty;
            return Task.CompletedTask;
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            return Task.FromResult(_isAuthenticated);
        }

        public string GetCurrentUserEmail() => _currentUserEmail;

        // Articles
        public Task<List<Article>> GetArticlesAsync()
        {
            return Task.FromResult(_articles.OrderByDescending(a => a.CreatedAt).ToList());
        }

        public Task<Article?> GetArticleBySlugAsync(string slug)
        {
            var article = _articles.FirstOrDefault(a => a.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(article);
        }

        public Task IncrementArticleViewsAsync(string slug)
        {
            var article = _articles.FirstOrDefault(a => a.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            if (article != null)
            {
                article.Views++;
            }
            return Task.CompletedTask;
        }

        public Task<Article> AddArticleAsync(Article article)
        {
            article.Id = Guid.NewGuid().ToString();
            article.CreatedAt = DateTime.UtcNow;
            article.Views = 0;
            _articles.Add(article);
            return Task.FromResult(article);
        }

        public Task<Article> UpdateArticleAsync(string id, Article article)
        {
            var existing = _articles.FirstOrDefault(a => a.Id == id);
            if (existing != null)
            {
                existing.Title = article.Title;
                existing.Slug = article.Slug;
                existing.Summary = article.Summary;
                existing.Content = article.Content;
                existing.CoverImage = article.CoverImage;
            }
            return Task.FromResult(existing ?? article);
        }

        public Task DeleteArticleAsync(string id)
        {
            var article = _articles.FirstOrDefault(a => a.Id == id);
            if (article != null)
            {
                _articles.Remove(article);
            }
            return Task.CompletedTask;
        }

        // Projects
        public Task<List<Project>> GetProjectsAsync()
        {
            return Task.FromResult(_projects.OrderByDescending(p => p.CreatedAt).ToList());
        }

        public Task<Project> AddProjectAsync(Project project)
        {
            project.Id = Guid.NewGuid().ToString();
            project.CreatedAt = DateTime.UtcNow;
            _projects.Add(project);
            return Task.FromResult(project);
        }

        public Task DeleteProjectAsync(string id)
        {
            var project = _projects.FirstOrDefault(p => p.Id == id);
            if (project != null)
            {
                _projects.Remove(project);
            }
            return Task.CompletedTask;
        }

        // Stats
        public Task<int> GetTotalViewsAsync()
        {
            return Task.FromResult(_articles.Sum(a => a.Views));
        }

        public Task<int> GetSiteVisitsAsync()
        {
            return Task.FromResult(_siteVisits);
        }

        public Task IncrementSiteVisitsAsync(string path)
        {
            _siteVisits++;
            return Task.CompletedTask;
        }
    }
}
