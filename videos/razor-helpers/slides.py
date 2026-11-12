"""The slides of the Razor helpers and tag helpers video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the Orchard Core source (TheTheme, the Taxonomies and Media modules, the helpers themselves) and from the
documentation, trimmed to what the narration talks about.
"""
from brand import H, MARGIN, W, bullets, draw_code, font, mono, note_cards, slide_base

TYPES = {"IOrchardHelper", "IOrchardDisplayHelper", "MyClass", "HttpContext", "ContentItem", "VersionOptions",
         "IEnumerable", "Task", "IHtmlContent", "IDisplayHelper", "IUpdateModel", "ResizeMode"}

BOTTOM = H - 40  # nothing is drawn below this line


def panel(image, y, title, language, code, size=24, x0=MARGIN, x1=W - MARGIN, highlight=None):
    """Draws a code panel at y, as high as its lines need, and returns the y below it. Stops when a line is wider than
    the panel or the panel ends below the slide."""
    lines = code.strip("\n").split("\n")
    code_font = mono(size)
    widest = max(code_font.getlength(line) for line in lines)
    if widest > x1 - x0 - 60:
        raise SystemExit(f"{title}: a line is {widest:.0f} px wide, the panel holds {x1 - x0 - 60}")
    y1 = y + 74 + len(lines) * int(size * 1.42) + 22
    if y1 > BOTTOM:
        raise SystemExit(f"{title}: the panel ends at {y1}, below {BOTTOM}")
    draw_code(image, (x0, y, x1, y1), title, language, code, types=TYPES, highlight=highlight, size=size)
    return y1


def heading(chapter_title, label, text):
    if font(52, "Bold").getlength(text) > W - 2 * MARGIN:
        raise SystemExit(f"The heading is too wide: {text}")
    return slide_base(chapter_title, label, text)


def cards(image, items, top):
    """Bullet cards, after checking that their texts fit."""
    for _, title, text in items:
        if font(36, "Bold").getlength(title) > 1540 or font(28).getlength(text) > 1540:
            raise SystemExit(f"A card is too wide: {title}")
    if top + 150 + 176 * (len(items) - 1) > BOTTOM:
        raise SystemExit("The cards end below the slide")
    bullets(image, items, top=top)


def notes(image, items, top):
    """Note cards across the slide, after checking that their texts fit."""
    for title, text in items:
        if font(34, "Bold").getlength(title) > W - 2 * MARGIN - 80 or font(30).getlength(text) > W - 2 * MARGIN - 80:
            raise SystemExit(f"A note is too wide: {title}")
    if top + 180 + 220 * (len(items) - 1) > BOTTOM:
        raise SystemExit("The notes end below the slide")
    note_cards(image, items, (MARGIN, top, W - MARGIN, BOTTOM))


VIEW_IMPORTS = """
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement
@addTagHelper *, OrchardCore.Menu
"""

HTML_ELEMENT = """
<html lang="@Orchard.CultureName()" dir="@Orchard.CultureDir()">
"""

RAZOR_PAGE = """
@page "/blogpost/{id}"
@using OrchardCore
@inject IOrchardHelper Orchard

@{
    var blogPost = await Orchard.GetContentItemByIdAsync(Id);
}

@* ... *@
"""

CONSTRUCTOR = """
public class MyClass
{
    private IOrchardHelper _orchard;

    public MyClass(IOrchardHelper orchard)
    {
        _orchard = orchard;
    }
}
"""

INTERFACE = """
namespace OrchardCore;

public interface IOrchardHelper
{
    HttpContext HttpContext { get; }
}
"""

MODULES = [
    ("1", "Contents, Alias and Autoroute", "Load content items by id, alias, slug or handle."),
    ("2", "Media and Resource Management", "Build the URLs of assets, resized images and resources."),
    ("3", "Liquid, Markdown, Shortcodes and Sanitizer", "Render a string as markup, or clean it."),
    ("4", "Queries, Lists and Taxonomies", "Run a saved query, read a list, find a term."),
]

LOAD_ONE = """
@{
    var contentItemId = "4mexample0contentitem0id00";

    var byId = await Orchard.GetContentItemByIdAsync(contentItemId);
    var byAlias = await Orchard.GetContentItemByAliasAsync("main-menu");
    var bySlug = await Orchard.GetContentItemBySlugAsync("myblog/my-blog-post");
    var byHandle = await Orchard.GetContentItemByHandleAsync("slug:myblog/my-blog-post");
}
"""

HANDLES = [
    ("1", "alias:main-menu", "The alias of a content item."),
    ("2", "slug:myblog/my-blog-post", "The path of the page of a content item."),
]

BY_ALIAS = """
@{
    var publishedItem = await Orchard.GetContentItemByAliasAsync("main-menu");
    var latestItem = await Orchard.GetContentItemByAliasAsync("main-menu", latest: true);
}
"""

BY_HANDLE = """
@using OrchardCore.ContentManagement

@{
    var item = await Orchard.GetContentItemByHandleAsync(
        "alias:main-menu",
        VersionOptions.Latest);
}
"""

RECENT_USAGE = """
@foreach (var post in await Orchard.GetRecentContentItemsByContentTypeAsync("BlogPost", 3))
{
    @await Orchard.DisplayAsync(post, "Summary")
}
"""

RECENT_SOURCE = """
public static async Task<IEnumerable<ContentItem>> GetRecentContentItemsByContentTypeAsync(
    this IOrchardHelper orchardHelper, string contentType, int maxContentItems = 10)
    => await orchardHelper.QueryContentItemsAsync(query => query
        .Where(x => x.ContentType == contentType && x.Published == true)
        .OrderByDescending(i => i.CreatedUtc)
        .Take(maxContentItems));
"""

DISPLAY_ONE = """
@model dynamic

@await Orchard.DisplayAsync(Model.TermContentItem as ContentItem, "Summary")
"""

DISPLAY_LOOP = """
@foreach (var contentItem in Model.ContentItems)
{
    var contentItemSummary = await Orchard.DisplayAsync(contentItem, "Summary");

    <li class="list-group-item">
        @await DisplayAsync(contentItemSummary)
    </li>
}
"""

DISPLAY_HELPER = """
public interface IOrchardDisplayHelper : IOrchardHelper
{
    IDisplayHelper DisplayHelper { get; }
}
"""

DISPLAY_SIGNATURE = """
public static async Task<IHtmlContent> DisplayAsync(
    this IOrchardDisplayHelper orchardDisplayHelper,
    ContentItem content,
    string displayType = "",
    string groupId = "",
    IUpdateModel updater = null)
{
    // ...
}
"""

CULTURE = """
<html lang="@Orchard.CultureName()" dir="@Orchard.CultureDir()">

@* ... *@

@if (Orchard.IsRightToLeft())
{
    <style asp-name="bootstrap-rtl" version="5" at="Head"></style>
}
else
{
    <style asp-name="bootstrap" version="5" at="Head"></style>
}
"""

ASSET_LOOP = """
@for (int i = 0; i < paths.Length; i++)
{
    var path = paths[i];
    var url = Orchard.AssetUrl(path);

    if (Orchard.IsImageFile(path))
    {
        // ...
    }
}
"""

URLS = """
@using OrchardCore.Media.Core.Processing

@Orchard.AssetUrl(Model.Paths[0], width: 100, height: 240, resizeMode: ResizeMode.Crop)
<img src="@Orchard.ResourceUrl("~/TheTheme/images/logo.png")" alt="Logo" />
"""

TEXT = """
@await Orchard.LiquidToHtmlAsync((string)Model.ContentItem.Content.Paragraph.Content.Html)
@await Orchard.MarkdownToHtmlAsync((string)Model.ContentItem.Content.MarkdownParagraph.Content.Markdown)
@Html.Raw(await Orchard.ShortcodesToHtmlAsync((string)Model.ContentItem.Content.RawHtml.Content.Html))
@Orchard.SanitizeHtml((string)Model.ContentItem.HtmlBodyPart.Html)
"""

MARKDOWN_RAW = """
@await Orchard.MarkdownToHtmlAsync((string)Model.ContentItem.Content.MarkdownParagraph.Content.Markdown, sanitize: false)
"""

QUERY_LOOP = """
@foreach (var contentItem in await Orchard.ContentQueryAsync("RecentBlogPosts"))
{
    @await Orchard.DisplayAsync(contentItem)
}
"""

LIST_ITEMS = """
@{
    var listContentItemId = "4mexample0contentitem0id00";
    var items = await Orchard.QueryListItemsAsync(listContentItemId);
}
"""

TERM_LOOP = """
@foreach (var contentItemId in Model.TermContentItemIds)
{
    var term = await Orchard.GetTaxonomyTermAsync(Model.TaxonomyContentItemId, contentItemId);
    <span class="taxonomy-term">@term</span>
}
"""

SHAPE_ELEMENTS = """
@{
    var intValue = 1;
    var stringValue = "a";
}

<shape type="Branding" />
<shape type="MyShape" prop-foo="@intValue" prop-bar="@stringValue" />
<menu alias="alias:main-menu" cache-id="main-menu" cache-fixed-duration="00:05:00" />
<contentitem handle="alias:main-menu" display-type="Detail" />
"""

CONTENTS_DIRECTIVE = """
@addTagHelper *, OrchardCore.Contents.TagHelpers
"""

ZONE = """
<zone name="Title" position="before">
    <h1 class="float-start pe-2">@T["Audit Trail"]</h1>
</zone>
"""

TITLE_SECTION = """
@* ... *@
@await RenderSectionAsync("Title", required: false)
@await RenderBodyAsync()
"""

RESOURCES = """
<script asp-name="theme-head" version="1" at="Head"></script>
<style asp-name="bootstrap" version="5" at="Head"></style>
<script asp-name="bootstrap" version="5" at="Foot"></script>

<resources type="Header" />
<resources type="FootScript" />
"""

IMAGE = """
<img asset-src="@Model.Paths[0]" alt="@Model.Field.MediaTexts[0]"
     img-width="100" img-height="240" img-resize-mode="Crop" />
"""

MEDIA_DIRECTIVE = """
@addTagHelper *, OrchardCore.Media
"""


def slide(step_id, chapter_title, label):
    # Chapter 1: the Orchard helper.
    if step_id == "helper-1":
        image = heading(chapter_title, label, "Orchard is a property of the view's base class")
        y = panel(image, 240, "_ViewImports.cshtml", "html", VIEW_IMPORTS, size=28, highlight={1})
        panel(image, y + 40, "Layout.cshtml", "html", HTML_ELEMENT, size=28)
        return image

    if step_id == "helper-2":
        image = heading(chapter_title, label, "Without that base class, inject IOrchardHelper")
        left = panel(image, 240, "BlogPost.cshtml", "html", RAZOR_PAGE, size=22, x1=950)
        right = panel(image, 240, "MyClass.cs", "csharp", CONSTRUCTOR, size=22, x0=990)
        panel(image, max(left, right) + 40, "IOrchardHelper.cs", "csharp", INTERFACE, size=24)
        return image

    if step_id == "helper-3":
        image = heading(chapter_title, label, "Each module defines its own helpers")
        cards(image, MODULES, 250)
        return image

    if step_id == "helper-4":
        image = heading(chapter_title, label, "A reference and an enabled feature")
        notes(image, [
            ("Reference the module", "In the project of the theme, so the method compiles."),
            ("Enable the feature", "On the site, so the services the helper calls exist."),
        ], 300)
        return image

    # Chapter 2: content items.
    if step_id == "content-1":
        image = heading(chapter_title, label, "Load one content item")
        y = panel(image, 240, "Razor", "csharp", LOAD_ONE, size=24)
        cards(image, HANDLES, y + 30)
        return image

    if step_id == "content-2":
        image = heading(chapter_title, label, "Published by default, drafts on request")
        y = panel(image, 240, "By alias", "csharp", BY_ALIAS, size=26)
        panel(image, y + 40, "By handle", "csharp", BY_HANDLE, size=26)
        return image

    if step_id == "content-3":
        image = heading(chapter_title, label, "List content items")
        y = panel(image, 240, "Razor", "csharp", RECENT_USAGE, size=26)
        panel(image, y + 40, "ContentOrchardHelperExtensions.cs", "csharp", RECENT_SOURCE, size=24)
        return image

    if step_id == "content-4":
        image = heading(chapter_title, label, "Render an item with DisplayAsync")
        y = panel(image, 240, "TermContentItem.cshtml", "html", DISPLAY_ONE, size=26)
        panel(image, y + 40, "TermPart.cshtml", "html", DISPLAY_LOOP, size=26)
        return image

    if step_id == "content-5":
        image = heading(chapter_title, label, "DisplayAsync needs the view's Orchard property")
        y = panel(image, 240, "IOrchardDisplayHelper.cs", "csharp", DISPLAY_HELPER, size=26)
        panel(image, y + 40, "OrchardRazorHelperExtensions.cs", "csharp", DISPLAY_SIGNATURE, size=26)
        return image

    # Chapter 3: helpers for the page.
    if step_id == "page-1":
        image = heading(chapter_title, label, "Culture helpers")
        panel(image, 240, "Layout.cshtml", "html", CULTURE, size=26)
        return image

    if step_id == "page-2":
        image = heading(chapter_title, label, "URL helpers")
        y = panel(image, 240, "MediaField.cshtml", "html", ASSET_LOOP, size=26)
        panel(image, y + 40, "Razor", "html", URLS, size=24)
        return image

    if step_id == "page-3":
        image = heading(chapter_title, label, "Text helpers")
        y = panel(image, 240, "Razor", "csharp", TEXT, size=22)
        panel(image, y + 40, "Markdown without sanitizing", "csharp", MARKDOWN_RAW, size=22)
        return image

    if step_id == "page-4":
        image = heading(chapter_title, label, "Data helpers")
        y = panel(image, 240, "Queries", "csharp", QUERY_LOOP, size=24)
        y = panel(image, y + 30, "Lists", "csharp", LIST_ITEMS, size=24)
        panel(image, y + 30, "TaxonomyField.cshtml", "html", TERM_LOOP, size=24)
        return image

    # Chapter 4: tag helpers.
    if step_id == "tag-1":
        image = heading(chapter_title, label, "Turn on a set of tag helpers")
        y = panel(image, 240, "_ViewImports.cshtml", "html", VIEW_IMPORTS, size=28, highlight={2, 3, 4, 5})
        notes(image, [
            ("Add the directive", "It names the assembly of the tag helpers, in the view imports file."),
            ("Reference the assembly", "The project or package that holds them, in the project of the theme."),
        ], y + 40)
        return image

    if step_id == "tag-2":
        image = heading(chapter_title, label, "Shapes, menus and content items")
        y = panel(image, 240, "Layout.cshtml", "html", SHAPE_ELEMENTS, size=24)
        panel(image, y + 40, "_ViewImports.cshtml", "html", CONTENTS_DIRECTIVE, size=24)
        return image

    if step_id == "tag-3":
        image = heading(chapter_title, label, "Content for a zone of the layout")
        y = panel(image, 240, "AuditTrailSettings.Edit.cshtml", "html", ZONE, size=26)
        panel(image, y + 40, "Layout.cshtml", "html", TITLE_SECTION, size=26)
        return image

    if step_id == "tag-4":
        image = heading(chapter_title, label, "Register and render resources")
        panel(image, 240, "Layout.cshtml", "html", RESOURCES, size=26)
        return image

    if step_id == "tag-5":
        image = heading(chapter_title, label, "Media attributes on the image element")
        y = panel(image, 240, "Razor", "html", IMAGE, size=26)
        panel(image, y + 40, "_ViewImports.cshtml", "html", MEDIA_DIRECTIVE, size=26)
        return image

    raise SystemExit(f"No slide for {step_id}")
