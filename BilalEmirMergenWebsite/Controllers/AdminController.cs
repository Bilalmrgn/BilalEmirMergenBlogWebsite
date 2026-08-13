using System.Security.Claims;
using BilalEmirMergenWebsite.Data;
using BilalEmirMergenWebsite.Dtos;
using BilalEmirMergenWebsite.Models;
using BilalEmirMergenWebsite.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BilalEmirMergenWebsite.Controllers;

[Route("admin")]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwords;
    private readonly IImageService _images;
    public AdminController(AppDbContext context, IPasswordService passwords, IImageService images) { _context = context; _passwords = passwords; _images = images; }

    [AllowAnonymous, HttpGet("login")] public IActionResult Login() => User.Identity?.IsAuthenticated == true ? RedirectToAction(nameof(Dashboard)) : View();
    [AllowAnonymous, HttpPost("login"), ValidateAntiForgeryToken] public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        var users = await _context.AdminUsers.Where(user => (user.Email == email || user.Username == email) && user.Role == "Admin").ToListAsync();
        var admin = users.FirstOrDefault(user => _passwords.Verify(password, user.PasswordHash));
        if (admin is not null)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, admin.Id), new Claim(ClaimTypes.Name, admin.Username), new Claim(ClaimTypes.Email, admin.Email), new Claim(ClaimTypes.Role, admin.Role) };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) });
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Dashboard));
        }
        ModelState.AddModelError("", "Invalid email or password."); return View();
    }
    [HttpPost("logout"), ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); HttpContext.Session.Clear(); return Redirect("/en"); }

    [HttpGet("localized/{type}/{id}")]
    public async Task<IActionResult> LocalizedValues(string type, string id)
    {
        object? entity = type switch { "about"=>await _context.AboutSections.FindAsync(id),"academic"=>await _context.AcademicFoundations.FindAsync(id),"experience"=>await _context.Experiences.FindAsync(id),"education"=>await _context.Educations.FindAsync(id),"category"=>await _context.SkillCategories.FindAsync(id),"skill"=>await _context.Skills.FindAsync(id),"concept"=>await _context.EngineeringConcepts.FindAsync(id),"language"=>await _context.SpokenLanguages.FindAsync(id),"project"=>await _context.Projects.FindAsync(id),"article"=>await _context.Articles.FindAsync(id),_=>null};
        if (entity is null) return NotFound();
        var values = entity.GetType().GetProperties().Where(property=>property.PropertyType == typeof(string) || property.PropertyType == typeof(int) || property.PropertyType == typeof(bool)).ToDictionary(property=>property.Name,property=>property.GetValue(entity)?.ToString()??string.Empty);
        return Json(values);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(string tab = "overview", string? editId = null, string? editor = null)
    {
        var model = new DashboardViewModel { ActiveTab=tab, EditId=editId, Editor=editor };
        switch (tab)
        {
            case "overview":
                model.Projects = await _context.Projects.AsNoTracking().Select(x=>new Project{Id=x.Id,IsActive=x.IsActive}).ToListAsync();
                model.Articles = await _context.Articles.AsNoTracking().Select(x=>new Article{Id=x.Id,IsActive=x.IsActive,PublishedAt=x.PublishedAt}).ToListAsync();
                model.Experiences = await _context.Experiences.AsNoTracking().Select(x=>new Experience{Id=x.Id,CompanyName=x.CompanyName,RoleEn=x.RoleEn,RoleTr=x.RoleTr}).ToListAsync();
                model.Skills = await _context.Skills.AsNoTracking().Select(x=>new Skill{Id=x.Id,Name=x.Name}).ToListAsync();
                model.SiteVisits = await _context.Analytics.AsNoTracking().CountAsync(x=>x.EventType=="site_visit");
                break;
            case "about": model.AboutSections=await _context.AboutSections.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "services": model.Services=await _context.Services.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "certificates": model.Certificates=await _context.Certificates.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "academic": model.AcademicFoundations=await _context.AcademicFoundations.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "experience": model.Experiences=await _context.Experiences.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "education": model.Educations=await _context.Educations.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "skills": model.Categories=await _context.SkillCategories.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync();model.Skills=await _context.Skills.AsNoTracking().Include(x=>x.Category).OrderBy(x=>x.SortOrder).ToListAsync();break;
            case "concepts": model.Concepts=await _context.EngineeringConcepts.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "languages": model.Languages=await _context.SpokenLanguages.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "socials": model.Socials=await _context.Socials.AsNoTracking().OrderBy(x=>x.SortOrder).ToListAsync(); break;
            case "settings": model.Settings=await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync()??new SiteSettings(); break;
            case "projects":
                model.Projects=await _context.Projects.AsNoTracking().OrderBy(x=>x.SortOrder).Select(x=>new Project{Id=x.Id,Title=x.Title,TitleEn=x.TitleEn,TitleTr=x.TitleTr,TitleAr=x.TitleAr,Status=x.Status,SortOrder=x.SortOrder,IsActive=x.IsActive,IsFeatured=x.IsFeatured,Slug=x.Slug}).ToListAsync();
                if(!string.IsNullOrWhiteSpace(editId)){var full=await _context.Projects.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==editId);if(full is not null){var index=model.Projects.FindIndex(x=>x.Id==editId);if(index>=0)model.Projects[index]=full;}}
                break;
            case "articles":
                model.Articles=await _context.Articles.AsNoTracking().OrderBy(x=>x.SortOrder).ThenByDescending(x=>x.CreatedAt).Select(x=>new Article{Id=x.Id,Title=x.Title,TitleEn=x.TitleEn,TitleTr=x.TitleTr,TitleAr=x.TitleAr,ContentLanguage=x.ContentLanguage,SortOrder=x.SortOrder,IsActive=x.IsActive,IsFeatured=x.IsFeatured,IsInternal=x.IsInternal,Slug=x.Slug,CreatedAt=x.CreatedAt}).ToListAsync();
                if(!string.IsNullOrWhiteSpace(editId)){var full=await _context.Articles.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==editId);if(full is not null){var index=model.Articles.FindIndex(x=>x.Id==editId);if(index>=0)model.Articles[index]=full;}}
                break;
        }
        return View(model);
    }

    [HttpPost("settings/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(SiteSettings input, IFormFile? cvEnglish, IFormFile? cvTurkish, IFormFile? profileImage)
    {
        NormalizePostedBooleans(input);
        input.NormalizeText();
        if(!ModelState.IsValid)return Invalid("settings"); var entity=await _context.SiteSettings.FirstOrDefaultAsync()??new SiteSettings();
        var oldEn=entity.CvEnglishFile; var oldTr=entity.CvTurkishFile; var oldProfile=entity.ProfileImage;
        entity.Id="main";entity.BrandName=input.BrandName;entity.FullName=input.FullName;entity.TitleEn=input.TitleEn;entity.SubtitleEn=input.SubtitleEn;entity.HeadlineEn=input.HeadlineEn;entity.IntroEn=input.IntroEn;entity.LocationEn=input.LocationEn;entity.IsAvailable=input.IsAvailable;entity.AvailabilityEn=input.AvailabilityEn;entity.Email=input.Email;entity.ContactTitleEn=input.ContactTitleEn;entity.ContactTextEn=input.ContactTextEn;entity.SeoTitleEn=input.SeoTitleEn;entity.SeoDescriptionEn=input.SeoDescriptionEn;entity.CanonicalBaseUrl=input.CanonicalBaseUrl;entity.OpenGraphImage=input.OpenGraphImage;
        try{entity.CvEnglishFile=await _images.SavePdfAsync(cvEnglish,"cv")??input.CvEnglishFile.Or(oldEn); entity.CvTurkishFile=await _images.SavePdfAsync(cvTurkish,"cv")??input.CvTurkishFile.Or(oldTr);entity.ProfileImage=await _images.SaveImageAsync(profileImage,"profile")??input.ProfileImage.Or(oldProfile);}catch(InvalidOperationException error){return Invalid("settings",error.Message);}
        if(_context.Entry(entity).State==EntityState.Detached)_context.SiteSettings.Add(entity); await _context.SaveChangesAsync(); return Saved("settings");
    }

    [HttpPost("experience/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveExperience(Experience input, IFormFile? logoFile)
    {
        NormalizePostedBooleans(input);
        input.NormalizeText();
        if(!ModelState.IsValid)return Invalid("experience");
        var existing=string.IsNullOrWhiteSpace(input.Id)?null:await _context.Experiences.FirstOrDefaultAsync(x=>x.Id==input.Id);var entity=existing??new Experience();var oldLogo=entity.CompanyLogoUrl;_context.Entry(entity).CurrentValues.SetValues(input);entity.Id=existing?.Id??Guid.NewGuid().ToString();try{entity.CompanyLogoUrl=await _images.SaveImageAsync(logoFile,"companies")??input.CompanyLogoUrl.Or(oldLogo);}catch(InvalidOperationException error){return Invalid("experience",error.Message);}if(existing is null)_context.Experiences.Add(entity);if(!ModelState.IsValid)return Invalid("experience");await _context.SaveChangesAsync();return Saved("experience");
    }
    [HttpPost("about/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAbout(AboutSection input,IFormFile? imageFile)
    {
        NormalizePostedBooleans(input);
        input.NormalizeText();
        var existing=string.IsNullOrWhiteSpace(input.Id)?null:await _context.AboutSections.FirstOrDefaultAsync(x=>x.Id==input.Id);var entity=existing??new AboutSection();var oldImage=entity.ImageUrl;_context.Entry(entity).CurrentValues.SetValues(input);entity.Id=existing?.Id??Guid.NewGuid().ToString();try{entity.ImageUrl=await _images.SaveImageAsync(imageFile,"about")??input.ImageUrl.Or(oldImage);}catch(InvalidOperationException error){return Invalid("about",error.Message);}if(existing is null)_context.AboutSections.Add(entity);await _context.SaveChangesAsync();return Saved("about");
    }
    [HttpPost("education/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEducation(Education input, IFormFile? logoFile)
    {
        NormalizePostedBooleans(input);
        input.NormalizeText();
        if(!ModelState.IsValid)return Invalid("education");
        var existing=string.IsNullOrWhiteSpace(input.Id)?null:await _context.Educations.FirstOrDefaultAsync(x=>x.Id==input.Id);var entity=existing??new Education();var oldLogo=entity.LogoUrl;_context.Entry(entity).CurrentValues.SetValues(input);entity.Id=existing?.Id??Guid.NewGuid().ToString();try{entity.LogoUrl=await _images.SaveImageAsync(logoFile,"education")??input.LogoUrl.Or(oldLogo);}catch(InvalidOperationException error){return Invalid("education",error.Message);}if(existing is null)_context.Educations.Add(entity);if(!ModelState.IsValid)return Invalid("education");await _context.SaveChangesAsync();return Saved("education");
    }
    [HttpPost("category/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(SkillCategory input)
    {
        NormalizePostedBooleans(input);
        input.NormalizeText();
        ModelState.Remove(nameof(SkillCategory.Id));
        if (!ModelState.IsValid) return Invalid("skills");
        var existing = string.IsNullOrWhiteSpace(input.Id) ? null : await _context.SkillCategories.FindAsync(input.Id);
        var entity = existing ?? new SkillCategory { Id = Guid.NewGuid().ToString(), SortOrder = (await _context.SkillCategories.MaxAsync(item => (int?)item.SortOrder) ?? 0) + 1 };
        var preservedOrder = entity.SortOrder;
        _context.Entry(entity).CurrentValues.SetValues(input);
        entity.SortOrder = existing is null ? preservedOrder : input.SortOrder;
        entity.Icon = input.Icon.Or("layers-3");
        entity.AccentColor = input.AccentColor.Or("#2F9CF4");
        if (existing is null) _context.SkillCategories.Add(entity);
        await _context.SaveChangesAsync();
        return Saved("skills", existing is null ? $"{entity.NameEn} category was added." : "Category updated.");
    }

    [HttpPost("skill/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSkill(Skill input)
    {
        NormalizePostedBooleans(input);
        input.NormalizeText();
        ModelState.Remove(nameof(Skill.Id));
        if (!ModelState.IsValid) return Invalid("skills");
        if (string.IsNullOrWhiteSpace(input.CategoryId) || !await _context.SkillCategories.AnyAsync(item => item.Id == input.CategoryId)) return Invalid("skills", "Choose a valid category.");
        var existing = string.IsNullOrWhiteSpace(input.Id) ? null : await _context.Skills.FindAsync(input.Id);
        var entity = existing ?? new Skill { Id = Guid.NewGuid().ToString(), SortOrder = (await _context.Skills.Where(item => item.CategoryId == input.CategoryId).MaxAsync(item => (int?)item.SortOrder) ?? 0) + 1 };
        var preservedOrder = entity.SortOrder;
        _context.Entry(entity).CurrentValues.SetValues(input);
        entity.SortOrder = existing is null ? preservedOrder : input.SortOrder;
        entity.Icon = input.Icon.Or("code-2").Replace("simple-icons:", string.Empty, StringComparison.OrdinalIgnoreCase);
        entity.IconType = input.IconType.Or("simple-icons");
        entity.BrandColor = input.BrandColor.Or("#2F9CF4");
        if (existing is null) _context.Skills.Add(entity);
        await _context.SaveChangesAsync();
        return Saved("skills", existing is null ? $"{entity.Name} was added successfully." : "Technology updated.");
    }
    [HttpPost("concept/save"), ValidateAntiForgeryToken] public async Task<IActionResult> SaveConcept(EngineeringConcept input) => await Upsert(input,_context.EngineeringConcepts,"concepts");
    [HttpPost("academic/save"), ValidateAntiForgeryToken] public async Task<IActionResult> SaveAcademic(AcademicFoundation input) => await Upsert(input,_context.AcademicFoundations,"academic");
    [HttpPost("language/save"), ValidateAntiForgeryToken] public async Task<IActionResult> SaveLanguage(SpokenLanguage input) => await Upsert(input,_context.SpokenLanguages,"languages");
    [HttpPost("social/save"), ValidateAntiForgeryToken] public async Task<IActionResult> SaveSocial(Social input) => await Upsert(input,_context.Socials,"socials");
    [HttpPost("service/save"), ValidateAntiForgeryToken] public async Task<IActionResult> SaveService(Service input) => await Upsert(input,_context.Services,"services");
    [HttpPost("certificate/save"), ValidateAntiForgeryToken] public async Task<IActionResult> SaveCertificate(Certificate input) => await Upsert(input,_context.Certificates,"certificates");

    [HttpPost("project/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProject(Project input, string tagsText, IFormFile? imageFile, List<IFormFile>? galleryFiles)
    {
        NormalizePostedBooleans(input);
        input.NormalizeText();
        tagsText ??= string.Empty;
        ModelState.Remove(nameof(Project.Title));ModelState.Remove(nameof(Project.Slug));if(string.IsNullOrWhiteSpace(input.TitleEn)&&string.IsNullOrWhiteSpace(input.TitleTr)&&string.IsNullOrWhiteSpace(input.Title))return Invalid("projects","At least one title is required."); var existing=string.IsNullOrWhiteSpace(input.Id)?null:await _context.Projects.FirstOrDefaultAsync(x=>x.Id==input.Id); var entity=existing??new Project(); var oldImage=entity.ImageUrl; var created=entity.CreatedAt;
        _context.Entry(entity).CurrentValues.SetValues(input); entity.Id=existing?.Id??Guid.NewGuid().ToString(); entity.CreatedAt=existing is null?DateTime.UtcNow:created;entity.UpdatedAt=DateTime.UtcNow; entity.Title=input.TitleEn.Or(input.TitleTr.Or(input.Title)); entity.Description=input.DescriptionEn.Or(input.DescriptionTr.Or(input.Description)); entity.Slug=Slug(input.Slug.Or(entity.Title)); entity.Tags=tagsText.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).ToList(); try{entity.ImageUrl=await _images.SaveImageAsync(imageFile,"projects")??input.ImageUrl.Or(oldImage);var uploaded=await _images.SaveImagesAsync(galleryFiles,"project-gallery");if(uploaded.Count>0)entity.Screenshots=string.Join(',',Csv(input.Screenshots).Concat(uploaded).Distinct());}catch(InvalidOperationException error){return Invalid("projects",error.Message);}
        if(existing is null)_context.Projects.Add(entity); await _context.SaveChangesAsync(); return Saved("projects");
    }

    [HttpPost("article/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveArticle(Article input, IFormFile? coverFile)
    {
        NormalizePostedBooleans(input);
        input.NormalizeText();
        ModelState.Remove(nameof(Article.Title));ModelState.Remove(nameof(Article.Slug));if(string.IsNullOrWhiteSpace(input.TitleEn)&&string.IsNullOrWhiteSpace(input.TitleTr)&&string.IsNullOrWhiteSpace(input.Title))return Invalid("articles","At least one title is required."); var existing=string.IsNullOrWhiteSpace(input.Id)?null:await _context.Articles.FirstOrDefaultAsync(x=>x.Id==input.Id); var entity=existing??new Article(); var oldCover=entity.CoverImage; var views=entity.Views; var created=entity.CreatedAt;
        _context.Entry(entity).CurrentValues.SetValues(input); entity.Id=existing?.Id??Guid.NewGuid().ToString(); entity.Views=views; entity.CreatedAt=existing is null?DateTime.UtcNow:created;entity.UpdatedAt=DateTime.UtcNow; entity.Title=input.TitleEn.Or(input.TitleTr.Or(input.Title)); entity.Summary=input.SummaryEn.Or(input.SummaryTr.Or(input.Summary)); entity.Content=input.ContentEn.Or(input.ContentTr.Or(input.Content)); entity.Slug=Slug(input.Slug.Or(entity.Title)); try{entity.CoverImage=await _images.SaveImageAsync(coverFile,"blog")??input.CoverImage.Or(oldCover);}catch(InvalidOperationException error){return Invalid("articles",error.Message);}
        if(existing is null)_context.Articles.Add(entity); await _context.SaveChangesAsync(); return Saved("articles");
    }

    [HttpPost("reorder/{type}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(string type, [FromBody] IReadOnlyList<ReorderItem> items)
    {
        if(items.Count==0)return BadRequest();
        switch(type)
        {
            case "experience": await ApplyOrder(_context.Experiences,items); break;
            case "education": await ApplyOrder(_context.Educations,items); break;
            case "category": await ApplyOrder(_context.SkillCategories,items); break;
            case "skill": await ApplyOrder(_context.Skills,items); break;
            case "project": await ApplyOrder(_context.Projects,items); break;
            case "language": await ApplyOrder(_context.SpokenLanguages,items); break;
            case "article": await ApplyOrder(_context.Articles,items); break;
            case "social": await ApplyOrder(_context.Socials,items); break;
            case "about": await ApplyOrder(_context.AboutSections,items); break;
            default:return NotFound();
        }
        await _context.SaveChangesAsync();
        return Ok(new{message="Order updated."});
    }

    [HttpPost("delete/{type}/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string type,string id)
    {
        object? entity=type switch { "about"=>await _context.AboutSections.FindAsync(id),"service"=>await _context.Services.FindAsync(id),"certificate"=>await _context.Certificates.FindAsync(id),"academic"=>await _context.AcademicFoundations.FindAsync(id),"experience"=>await _context.Experiences.FindAsync(id),"education"=>await _context.Educations.FindAsync(id),"category"=>await _context.SkillCategories.FindAsync(id),"skill"=>await _context.Skills.FindAsync(id),"concept"=>await _context.EngineeringConcepts.FindAsync(id),"language"=>await _context.SpokenLanguages.FindAsync(id),"project"=>await _context.Projects.FindAsync(id),"article"=>await _context.Articles.FindAsync(id),"social"=>await _context.Socials.FindAsync(id),_=>null};
        if(entity is null){TempData["Error"]="The item no longer exists.";return RedirectToAction(nameof(Dashboard),new{tab=TabFor(type)});}
        _context.Remove(entity);
        try{await _context.SaveChangesAsync();}
        catch(DbUpdateException){TempData["Error"]="This item is in use and cannot be deleted.";return RedirectToAction(nameof(Dashboard),new{tab=TabFor(type)});}
        return Saved(TabFor(type),"Item deleted.");
    }

    private async Task<IActionResult> Upsert<T>(T input,DbSet<T> set,string tab) where T:class
    {
        NormalizePostedBooleans(input); input.NormalizeText(); ModelState.Remove("Id");if(!ModelState.IsValid)return Invalid(tab); var id=(string?)typeof(T).GetProperty("Id")?.GetValue(input); var existing=string.IsNullOrWhiteSpace(id)?null:await set.FindAsync(id); if(existing is null){typeof(T).GetProperty("Id")?.SetValue(input,Guid.NewGuid().ToString());set.Add(input);}else _context.Entry(existing).CurrentValues.SetValues(input); await _context.SaveChangesAsync(); return Saved(tab);
    }
    private void NormalizePostedBooleans<T>(T input) where T:class
    {
        if(!Request.HasFormContentType)return;
        foreach(var property in typeof(T).GetProperties().Where(property=>property.PropertyType==typeof(bool)&&property.CanWrite))
        {
            if(!Request.Form.TryGetValue(property.Name,out var values))continue;
            property.SetValue(input,values.Any(value=>bool.TryParse(value,out var parsed)&&parsed));
        }
    }
    private RedirectToActionResult Saved(string tab,string message="Changes saved."){TempData["Success"]=message;return RedirectToAction(nameof(Dashboard),new{tab});}
    private RedirectToActionResult Invalid(string tab,string? message=null){TempData["Error"]=message??string.Join(" ",ModelState.Values.SelectMany(x=>x.Errors).Select(x=>x.ErrorMessage));return RedirectToAction(nameof(Dashboard),new{tab});}
    private static string TabFor(string type)=>type switch{"about"=>"about","service"=>"services","certificate"=>"certificates","academic"=>"academic","experience"=>"experience","education"=>"education","category" or "skill"=>"skills","concept"=>"concepts","language"=>"languages","project"=>"projects","article"=>"articles","social"=>"socials",_=>"overview"};
    private static async Task ApplyOrder<T>(DbSet<T> set,IReadOnlyList<ReorderItem> items) where T:class,IOrderedContent
    {
        var order=items.ToDictionary(item=>item.Id,item=>item.DisplayOrder);var ids=order.Keys.ToList();var entities=await set.Where(item=>ids.Contains(EF.Property<string>(item,"Id"))).ToListAsync();foreach(var entity in entities)entity.SortOrder=order[(string)entity.GetType().GetProperty("Id")!.GetValue(entity)!];
    }
    private static IEnumerable<string> Csv(string value)=>value.Split(new[]{',','\n'},StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
    private static string Slug(string value){var normalized=value.ToLowerInvariant().Replace('ı','i').Replace('ğ','g').Replace('ü','u').Replace('ş','s').Replace('ö','o').Replace('ç','c');return System.Text.RegularExpressions.Regex.Replace(normalized,"[^a-z0-9]+","-").Trim('-');}
}

public class DashboardViewModel
{
    public string ActiveTab{get;set;}="overview";public string? EditId{get;set;}public string? Editor{get;set;}public SiteSettings Settings{get;set;}=new();public List<AboutSection> AboutSections{get;set;}=new();public List<Service> Services{get;set;}=new();public List<Certificate> Certificates{get;set;}=new();public List<AcademicFoundation> AcademicFoundations{get;set;}=new();public List<Experience> Experiences{get;set;}=new();public List<Education> Educations{get;set;}=new();public List<SkillCategory> Categories{get;set;}=new();public List<Skill> Skills{get;set;}=new();public List<EngineeringConcept> Concepts{get;set;}=new();public List<SpokenLanguage> Languages{get;set;}=new();public List<Project> Projects{get;set;}=new();public List<Article> Articles{get;set;}=new();public List<Social> Socials{get;set;}=new();public int SiteVisits{get;set;}
}

public sealed record TechnologyIconViewModel(string Name,string Icon,string IconProvider,string IconUrl,string BrandColor,int Size=48);
public sealed record RichTextEditorViewModel(string Name,string Value,string Label,string HelpText,string MinHeight="360px");

public record SettingsLocaleViewModel(SiteSettings Settings,string Suffix);
public record OrderedTableViewModel(string Type,IEnumerable<(string Id,string Title,string Meta,int Order,bool Active)> Rows);
