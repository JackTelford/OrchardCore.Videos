"""The slides of the HTML Sanitizer video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IHtmlSanitizerService", "IHtmlContent", "HtmlBodyPart", "HtmlDisplayService", "IHtmlDisplayService",
         "IShortcodeService", "HtmlViewModelBase", "Context", "Task", "TModel"}

THREATS = [
    ("1", "Script elements", "A script element runs when the page loads."),
    ("2", "Event handler attributes", "An attribute such as onclick runs code when the event happens."),
    ("3", "Script addresses", "A link address can be a script that runs when the link is clicked."),
]

STORED = """
<p>Welcome to the site.</p>
<script>alert('Hello');</script>
"""

RENDERED = """
<p>Welcome to the site.</p>
"""

PIPELINE = [
    ("1", "Authored source", "HTML or Markdown, stored as written."),
    ("2", "Markdown to HTML", "Only for the Markdown parts and fields."),
    ("3", "Shortcodes", "Processed in the HTML."),
    ("4", "Sanitizer", "Runs last, so it also cleans what shortcodes produce."),
]

DEFAULTS_ON = [
    ("1", "HtmlBodyPart and HtmlField", "Shortcodes are processed, then the HTML is sanitized."),
    ("2", "MarkdownBodyPart and MarkdownField", "Markdown, then shortcodes, then the sanitizer."),
    ("3", "HtmlMenuItemPart", "The HTML of a menu item is sanitized."),
    ("4", "Sanitize Html", "A setting of each part and field, on by default."),
]

ADDRESSES = [
    ("1", "LinkField, LinkMenuItemPart, HtmlMenuItemPart", "The URL is checked when the item is saved."),
    ("2", "The check", "The URL is placed in a link, which is sanitized and compared."),
    ("3", "The result", "If the sanitizer changes the link, the URL is rejected as invalid."),
    ("4", "Link and HTML menu items", "Checked again when displayed: an unsafe URL is replaced with an empty one."),
]

WAYS = [
    ("1", "Razor", "Orchard.SanitizeHtml takes a string and returns IHtmlContent."),
    ("2", "Liquid", "The sanitize_html filter sanitizes the value it receives."),
    ("3", "C#", "IHtmlSanitizerService has one method: Sanitize."),
]

RAZOR = """
@* Writes the sanitized HTML to the page. *@
@Orchard.SanitizeHtml((string)Model.ContentItem.HtmlBodyPart.Html)
"""

LIQUID = """
{% capture output %}
  <span class="text-primary">{{ Content }}</span>
{% endcapture %}
{{ output | sanitize_html | raw }}
"""

SERVICE = """
public class HtmlDisplayService : IHtmlDisplayService
{
    private readonly IShortcodeService _shortcodeService;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    public HtmlDisplayService(IShortcodeService shortcodeService, IHtmlSanitizerService htmlSanitizerService)
    {
        // ...
    }

    public async Task UpdateModelHtmlAsync<TModel>(TModel model, Context shortcodeContext, bool sanitizeHtml)
        where TModel : HtmlViewModelBase
    {
        // ... Shortcodes are processed here.

        if (sanitizeHtml)
        {
            model.Html = _htmlSanitizerService.Sanitize(model.Html);
        }
    }
}
"""

DEFAULTS = """
// Registered by AddOrchardCms.
services.ConfigureHtmlSanitizer(sanitizer =>
{
    sanitizer.AllowedAttributes.Add("class");
    sanitizer.AllowedTags.Remove("form");

    sanitizer.AllowedSchemes.Add("mailto");
    sanitizer.AllowedSchemes.Add("tel");
});
"""

CONFIGURE = """
builder.Services
    .AddOrchardCms()
    .ConfigureServices(tenantServices =>
        tenantServices.ConfigureHtmlSanitizer(sanitizer =>
        {
            sanitizer.AllowedSchemes.Add("ftp");
        }));
"""

LINK_CHECKS = [
    ("1", "One sanitizer", "The Link Field and the menu items check URLs with the same sanitizer."),
    ("2", "One list of schemes", "A scheme you allow in the configuration is accepted by those checks."),
]

CACHE = [
    ("1", "Results are cached", "Fragments up to 64K characters are cached and reused."),
    ("2", "Each tenant has its own", "A tenant never receives the result of another tenant."),
    ("3", "Same fragment, same result", "Configuration actions and sanitizer event handlers must not vary."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Markup that runs in a browser")
        bullets(image, THREATS)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Stored as written, cleaned when displayed")
        draw_code(image, (96, 240, 940, 500), "Stored source", "html", STORED, size=26)
        draw_code(image, (980, 240, W - 96, 500), "Rendered page", "html", RENDERED, size=26)
        note_cards(image, [
            ("Stored as written", "Saved exactly as the author typed it."),
            ("Cleaned when displayed", "The script element is removed."),
        ], (96, 600, W - 96, 1030))
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "From source to page")
        bullets(image, PIPELINE)
        return image

    if step_id == "idea-4":
        image = slide_base(chapter_title, label, "Sanitized by default")
        bullets(image, DEFAULTS_ON)
        return image

    if step_id == "idea-5":
        image = slide_base(chapter_title, label, "Addresses are checked too")
        bullets(image, ADDRESSES)
        return image

    if step_id == "use-1":
        image = slide_base(chapter_title, label, "Three ways to call the sanitizer")
        bullets(image, WAYS)
        return image

    if step_id == "use-2":
        image = slide_base(chapter_title, label, "Razor: Orchard.SanitizeHtml")
        draw_code(image, (96, 240, W - 96, 480), "Content-Article.cshtml", "cshtml", RAZOR, types=TYPES, size=26)
        note_cards(image, [
            ("Takes a string", "The HTML you want to write to the page."),
            ("Returns IHtmlContent", "Razor writes it as markup, without encoding it."),
        ], (96, 560, W - 96, 1030))
        return image

    if step_id == "use-3":
        image = slide_base(chapter_title, label, "Liquid: sanitize_html")
        draw_code(image, (96, 240, W - 96, 540), "Template.liquid", "html", LIQUID, size=28)
        note_cards(image, [
            ("raw", "Writes the result as markup, without encoding it."),
            ("markdownify", "Always sanitizes the HTML it returns."),
        ], (96, 600, W - 96, 1030))
        return image

    if step_id == "use-4":
        image = slide_base(chapter_title, label, "C#: IHtmlSanitizerService")
        draw_code(image, (96, 240, W - 96, 1030), "HtmlDisplayService.cs", "csharp", SERVICE, types=TYPES, size=23)
        return image

    if step_id == "configure-1":
        image = slide_base(chapter_title, label, "The rules Orchard Core sets")
        draw_code(image, (96, 240, 1130, 720), "Defaults", "csharp", DEFAULTS, types=TYPES, size=26)
        note_cards(image, [
            ("class", "An allowed attribute."),
            ("mailto and tel", "Allowed URL schemes."),
            ("form", "Removed from the allowed tags."),
        ], (1180, 240, W - 96, 1030))
        return image

    if step_id == "configure-2":
        image = slide_base(chapter_title, label, "Change the rules in code")
        draw_code(image, (96, 240, W - 96, 660), "Program.cs", "csharp", CONFIGURE, types=TYPES, size=28)
        note_cards(image, [
            ("The action receives the sanitizer", "It runs for each tenant. Add to its allowed tags, attributes and schemes."),
        ], (96, 720, W - 96, 1030))
        return image

    if step_id == "configure-3":
        image = slide_base(chapter_title, label, "The same rules reach the link checks")
        bullets(image, LINK_CHECKS)
        return image

    if step_id == "configure-4":
        image = slide_base(chapter_title, label, "Results are cached")
        bullets(image, CACHE)
        return image

    raise SystemExit(f"No slide for {step_id}")
