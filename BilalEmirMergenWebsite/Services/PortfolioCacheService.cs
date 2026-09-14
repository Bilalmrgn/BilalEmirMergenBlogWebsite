using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BilalEmirMergenWebsite.Services;

public interface IPortfolioCacheService
{
    Task<SiteSettings> GetSiteSettingsAsync(AppDbContext? context = null);
    Task<List<Social>> GetSocialsAsync(AppDbContext? context = null);
    Task<List<AboutSection>> GetAboutSectionsAsync(AppDbContext? context = null);
    Task<List<Service>> GetServicesAsync(AppDbContext? context = null);
    Task<List<Experience>> GetExperiencesAsync(AppDbContext? context = null);
    Task<List<Education>> GetEducationsAsync(AppDbContext? context = null);
    Task<List<SkillCategory>> GetSkillCategoriesAsync(AppDbContext? context = null);
    Task<List<EngineeringConcept>> GetEngineeringConceptsAsync(AppDbContext? context = null);
    Task<List<SpokenLanguage>> GetSpokenLanguagesAsync(AppDbContext? context = null);
    Task<List<AcademicFoundation>> GetAcademicFoundationsAsync(AppDbContext? context = null);
    void InvalidateCache();
}

public class PortfolioCacheService : IPortfolioCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromHours(2);
    private const string CachePrefix = "portfolio_cache_";

    public PortfolioCacheService(IMemoryCache cache, IServiceScopeFactory scopeFactory)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
    }

    private async Task<T> FetchAsync<T>(AppDbContext? context, Func<AppDbContext, Task<T>> query)
    {
        if (context != null)
        {
            return await query(context);
        }
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    public async Task<SiteSettings> GetSiteSettingsAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "settings", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == "main") 
                ?? await db.SiteSettings.AsNoTracking().FirstOrDefaultAsync() 
                ?? new SiteSettings());
        }) ?? new SiteSettings();
    }

    public async Task<List<Social>> GetSocialsAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "socials", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.Socials.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync());
        }) ?? new List<Social>();
    }

    public async Task<List<AboutSection>> GetAboutSectionsAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "about", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.AboutSections.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync());
        }) ?? new List<AboutSection>();
    }

    public async Task<List<Service>> GetServicesAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "services", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.Services.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync());
        }) ?? new List<Service>();
    }

    public async Task<List<Experience>> GetExperiencesAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "experiences", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.Experiences.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync());
        }) ?? new List<Experience>();
    }

    public async Task<List<Education>> GetEducationsAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "educations", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.Educations.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync());
        }) ?? new List<Education>();
    }

    public async Task<List<SkillCategory>> GetSkillCategoriesAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "skills", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.SkillCategories.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .Include(x => x.Skills.Where(s => s.IsActive).OrderBy(s => s.SortOrder))
                    .ToListAsync());
        }) ?? new List<SkillCategory>();
    }

    public async Task<List<EngineeringConcept>> GetEngineeringConceptsAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "concepts", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.EngineeringConcepts.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync());
        }) ?? new List<EngineeringConcept>();
    }

    public async Task<List<SpokenLanguage>> GetSpokenLanguagesAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "languages", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.SpokenLanguages.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync());
        }) ?? new List<SpokenLanguage>();
    }

    public async Task<List<AcademicFoundation>> GetAcademicFoundationsAsync(AppDbContext? context = null)
    {
        return await _cache.GetOrCreateAsync(CachePrefix + "academic", async entry =>
        {
            entry.SlidingExpiration = DefaultCacheDuration;
            return await FetchAsync(context, async db =>
                await db.AcademicFoundations.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync());
        }) ?? new List<AcademicFoundation>();
    }

    public void InvalidateCache()
    {
        _cache.Remove(CachePrefix + "settings");
        _cache.Remove(CachePrefix + "socials");
        _cache.Remove(CachePrefix + "about");
        _cache.Remove(CachePrefix + "services");
        _cache.Remove(CachePrefix + "experiences");
        _cache.Remove(CachePrefix + "educations");
        _cache.Remove(CachePrefix + "skills");
        _cache.Remove(CachePrefix + "concepts");
        _cache.Remove(CachePrefix + "languages");
        _cache.Remove(CachePrefix + "academic");
    }
}
