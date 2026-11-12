"""The slides of the "Sitemaps for Localized Content and Razor Pages" video. assemble.py calls slide() for each step of
a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the Orchard Core source and the documentation, trimmed with // ... where the narration does not go.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "IRouteableContentTypeProvider", "IRouteableContentTypeCoordinator", "ContentTypeDefinition", "SitemapBuilderContext",
    "ContentItem", "ContentItemsQueryContext", "ISitemapContentItemExtendedMetadataProvider", "IContentItemsQueryProvider",
    "SitemapUrlHrefLangExtendedMetadataProvider", "LocalizedContentItemsQueryProvider", "StartupBase", "IServiceCollection",
    "ServiceDescriptor", "XElement", "XAttribute", "SitemapsRazorPagesOptions", "SitemapsRazorPagesContentTypeOption",
    "RazorPagesContentTypeProvider", "AutorouteContentTypeProvider", "AutoroutePart", "ContentItemMetadata", "Task",
    "IEnumerable", "FeatureAttribute", "RequireFeatures", "SitemapStartup", "SitemapsStartup", "IOrchardHelper",
}

ROUTES_PROVIDER = """
public interface IRouteableContentTypeProvider
{
    // The content types the provider can route.
    Task<IEnumerable<ContentTypeDefinition>> ListRoutableTypeDefinitionsAsync();

    // The route of a content item, when building a sitemap.
    Task<string> GetRouteAsync(
        SitemapBuilderContext context, ContentItem contentItem);
}
"""

LOCALIZED_STARTUP = """
[Feature("OrchardCore.ContentLocalization.Sitemaps")]
public sealed class SitemapsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<
            ISitemapContentItemExtendedMetadataProvider,
            SitemapUrlHrefLangExtendedMetadataProvider>();

        services.Replace(ServiceDescriptor.Scoped<
            IContentItemsQueryProvider,
            LocalizedContentItemsQueryProvider>());
    }
}
"""

HREFLANG = """
public async Task<bool> ApplyExtendedMetadataAsync(
    SitemapBuilderContext context, ContentItemsQueryContext queryContext,
    ContentItem contentItem, XElement url)
{
    // ... localizedContentParts: the items of the same localization set
    foreach (var localizedPart in localizedContentParts)
    {
        // ... continue when the SitemapMetadataAspect says Exclude
        var hrefValue = await _routeableContentTypeCoordinator
            .GetRouteAsync(context, localizedPart.ContentItem);

        var linkNode = new XElement(s_extendedNamespace + "link",
            new XAttribute("rel", "alternate"),
            new XAttribute("hreflang", localizedPart.Culture),
            new XAttribute("href", hrefValue));
        url.Add(linkNode);
    }
    return true;
}
"""

RAZOR_OPTIONS = """
builder.Services.Configure<SitemapsRazorPagesOptions>(options =>
{
    options.ConfigureContentType("DecoupledBlogPost", o =>
    {
        o.PageName = "DecoupledBlogPost";
        o.RouteValues = contentItem => new
        {
            area = "OrchardSite", // The name of the web project.
            id = contentItem.ContentItemId,
        };
    });
});
"""

RAZOR_PAGE = """
@page "/blogpost/{id}"
@using OrchardCore
@inject IOrchardHelper Orchard

@{
    var blogPost = await Orchard.GetContentItemByIdAsync(Id);
}

<h1>This is the blog post: @blogPost.DisplayText</h1>

@* ... *@
"""

RAZOR_PROVIDER = """
public class RazorPagesContentTypeProvider : IRouteableContentTypeProvider
{
    // ... _options, _contentDefinitionManager and the constructor

    public Task<string> GetRouteAsync(SitemapBuilderContext context, ContentItem contentItem)
    {
        var option = _options.ContentTypeOptions
            .FirstOrDefault(o => o.ContentType == contentItem.ContentType);
        if (option != null && option.RouteValues != null)
        {
            var pageName = string.IsNullOrEmpty(option.PageName) ? option.ContentType : option.PageName;
            // ... the page name starts with a slash

            var url = context.HostPrefix + context.UrlHelper.Page(pageName, option.RouteValues.Invoke(contentItem));
            return Task.FromResult(url);
        }

        return Task.FromResult<string>(null);
    }

    // ... ListRoutableTypeDefinitionsAsync: the types of the options
}
"""

AUTOROUTE_STARTUP = """
[RequireFeatures("OrchardCore.Sitemaps")]
public class SitemapStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<
            IRouteableContentTypeProvider,
            AutorouteContentTypeProvider>();
    }
}
"""

AUTOROUTE_PROVIDER = """
public class AutorouteContentTypeProvider : IRouteableContentTypeProvider
{
    // ... _contentDefinitionManager, _contentManager and the constructor

    public async Task<IEnumerable<ContentTypeDefinition>> ListRoutableTypeDefinitionsAsync()
    {
        return (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(ctd => ctd.Parts.Any(p => p.Name == nameof(AutoroutePart)));
    }

    public async Task<string> GetRouteAsync(SitemapBuilderContext context, ContentItem contentItem)
    {
        var ctd = (await ListRoutableTypeDefinitionsAsync())?.FirstOrDefault(ctd => ctd.Name == contentItem.ContentType);
        if (ctd != null)
        {
            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            var routes = contentItemMetadata.DisplayRouteValues;
            return context.HostPrefix + context.UrlHelper.Action(routes["Action"].ToString(), routes);
        }
        return null;
    }
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "routes-1":
        image = slide_base(chapter_title, label, "Two answers for every content item")
        bullets(image, [
            ("1", "Is the type routeable?", "A provider lists the content types it can route."),
            ("2", "What is the URL?", "A provider returns the route of a content item."),
            ("3", "Which provider answers?", "The coordinator asks each one, and keeps the first route."),
        ])
        return image

    if step_id == "routes-2":
        image = slide_base(chapter_title, label, "IRouteableContentTypeProvider")
        draw_code(image, (96, 240, 1160, 660), "IRouteableContentTypeProvider.cs", "csharp", ROUTES_PROVIDER, types=TYPES, size=22)
        note_cards(image, [
            ("Autoroute", "Types with an AutoroutePart."),
            ("Razor Pages", "Types of SitemapsRazorPagesOptions."),
        ], (1200, 240, W - 96, 700))
        return image

    if step_id == "loc-1":
        image = slide_base(chapter_title, label, "Localized Content Item Sitemaps")
        draw_code(image, (96, 240, 1160, 780), "Startup.cs", "csharp", LOCALIZED_STARTUP, types=TYPES, size=22)
        note_cards(image, [
            ("Metadata provider", "Adds the language alternates."),
            ("Query provider", "Replaces the default one."),
        ], (1200, 240, W - 96, 700))
        return image

    if step_id == "loc-2":
        image = slide_base(chapter_title, label, "SitemapUrlHrefLangExtendedMetadataProvider")
        draw_code(image, (96, 240, W - 96, 1030), "SitemapUrlHrefLangExtendedMetadataProvider.cs", "csharp", HREFLANG,
                  types=TYPES, size=24)
        return image

    if step_id == "raz-0":
        image = slide_base(chapter_title, label, "Content that Autoroute does not route")
        bullets(image, [
            ("1", "Autoroute provider", "Lists the content types that have an AutoroutePart."),
            ("2", "Decoupled site", "Your own Razor Pages display the content, with no Autoroute part."),
            ("3", "Razor Pages provider", "Lists the content types you configure, and builds their URLs."),
        ])
        return image

    if step_id == "raz-1":
        image = slide_base(chapter_title, label, "SitemapsRazorPagesOptions")
        draw_code(image, (96, 240, 1160, 740), "Program.cs", "csharp", RAZOR_OPTIONS, types=TYPES, size=22,
                  highlight={1, 2, 3, 4, 5, 11, 12})
        note_cards(image, [
            ("Content type", "The technical name of the type."),
            ("PageName", "The page; the type name if empty."),
        ], (1200, 240, W - 96, 700))
        return image

    if step_id == "raz-2":
        image = slide_base(chapter_title, label, "RouteValues")
        draw_code(image, (96, 240, 960, 740), "Program.cs", "csharp", RAZOR_OPTIONS, types=TYPES, size=22,
                  highlight={6, 7, 8, 9, 10})
        draw_code(image, (1000, 240, W - 96, 740), "Pages/DecoupledBlogPost.cshtml", "html", RAZOR_PAGE, types=TYPES, size=22)
        return image

    if step_id == "raz-3":
        image = slide_base(chapter_title, label, "RazorPagesContentTypeProvider")
        draw_code(image, (96, 240, W - 96, 1030), "RazorPagesContentTypeProvider.cs", "csharp", RAZOR_PROVIDER,
                  types=TYPES, size=22)
        return image

    if step_id == "ext-1":
        image = slide_base(chapter_title, label, "Register a provider")
        draw_code(image, (96, 240, 1160, 700), "Startup.cs", "csharp", AUTOROUTE_STARTUP, types=TYPES, size=22)
        note_cards(image, [
            ("Scoped", "One per request, like the built-in ones."),
            ("Every provider", "Listed together; the first route wins."),
        ], (1200, 240, W - 96, 700))
        return image

    if step_id == "ext-2":
        image = slide_base(chapter_title, label, "The Autoroute provider")
        draw_code(image, (96, 240, W - 96, 1030), "AutorouteContentTypeProvider.cs", "csharp", AUTOROUTE_PROVIDER,
                  types=TYPES, size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
