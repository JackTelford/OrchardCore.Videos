"""The slides of the Liquid basics video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The Liquid
code comes from the Blog theme and the Blog recipe of Orchard Core, and from the documentation of the Liquid module,
trimmed with <!-- ... -->. brand.py has no Liquid lexer, so the panels use the HTML one.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

# The excerpt of Content-BlogPost.liquid (TheBlogTheme/Views) that the video explains and then shows on the page.
POST_HEADER = """
<h1>{{ Model.ContentItem.DisplayText }}</h1>
<h2 class="subheading">{{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}</h2>
<!-- ... -->
{% assign format = "MMMM dd, yyyy" | t %}
<!-- ... -->
{{ "Posted by" | t }} <a href="#">{{ Model.ContentItem.Author }}</a>
"""

# The same file, one line further down, and the sanitize_html example of the documentation of the Liquid module.
RAW_LINE = """
<!-- ... -->
{{ "on {0}" | t: dateTime | raw }}
"""

SANITIZE = """
{% capture output %}
  <span class="text-primary">{{ Model.ContentItem.DisplayText }}</span>
{% endcapture %}
{{ output | sanitize_html | raw }}
"""

CONTENT_PATH = """
{{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}
"""

CONTENT_ITEM = """
{{ Model.ContentItem.DisplayText }}
{{ Model.ContentItem.ContentType }}
{{ Model.ContentItem.CreatedUtc }}
{{ Model.ContentItem.Author }}
"""

PATTERN = """
{{ Model.ContentItem | display_text | slugify }}
"""

TEXT_AND_URLS = """
{{ "Hello {0}!" | t: "John" }}
{{ '~/some-page' | href }}
{{ '~/some-page' | absolute_url }}
"""

TIME_ZONES = """
{{ Model.ContentItem.CreatedUtc | local | date: "%c" }}
{{ "now" | local | date: "%c" }}
{{ "now" | utc | date: "%c" }}
"""

IMAGE_HEADER = """
{% assign imagePath = Model.ContentItem.Content.BlogPost.Image.Paths.first %}
{% if imagePath == nil %}
    <!-- ... -->
{% else %}
    <!-- ... -->
{% endif %}
"""

OPTIONS = """
{
  "OrchardCore": {
    "Liquid": {
      "AllowParentheses": true,
      "AllowLiquidTag": true,
      "AllowTrailingQuestionMark": true
    }
  }
}
"""

WHERE = [
    ("1", "Files in a theme or a module", "Views/Layout.liquid and Views/Content-BlogPost.liquid in the Blog theme"),
    ("2", "Templates written in the admin", "The Templates module: shape templates that you write on the site"),
    ("3", "Content and settings", "The Liquid part, the Liquid field and the Autoroute patterns"),
    ("4", "Two permissions", "Manage Liquid templates, and Manage templates for the Templates module: both security-critical"),
]

SYSTEM_OBJECTS = [
    ("C", "Culture", "Name, NativeName, DisplayName, Dir, SupportedCultures, DefaultCulture"),
    ("R", "Request", "Path, QueryString, Query, Headers, Cookies, Method, RouteValues"),
    ("E", "Environment", "IsDevelopment, IsStaging, IsProduction, Name"),
    ("H", "HttpContext, TrackingConsent", "Items; CanTrack, HasConsent, IsConsentNeeded"),
]

FEATURE_OBJECTS = [
    ("S", "Settings adds Site", "SiteName, PageSize, TimeZoneId, BaseUrl, HomeRoute and more"),
    ("C", "Contents adds Content", "Loads content items by identifier or handle"),
    ("U", "Users adds User", "User.Identity.Name, and the filters has_permission, is_in_role and user_email. Needs the Liquid feature too"),
]

CONTENT_WALK = [
    ("1", "Content", "The parts and fields of the content item, as a document"),
    ("2", "BlogPost", "A part named after the content type: it holds the fields that you add to the type"),
    ("3", "Subtitle, Text", "A field, and the value of that field"),
]

CHAIN = [
    ("1", "Model.ContentItem", "The content item that the pattern builds a URL for"),
    ("2", "display_text", "Returns the title of the content item"),
    ("3", "slugify", "Turns the title into a segment of a URL, such as this-is-some-text"),
]

FILTER_SOURCES = [
    ("1", "Standard Liquid", "date, upcase, default, append, replace, join, where and the other standard filters"),
    ("2", "The Liquid module", "t, slugify, href, absolute_url, local, utc, json, html_class, liquid"),
    ("3", "Contents and Users", "display_text, display_url, has_permission, is_in_role, user_email"),
    ("4", "Other features", "markdownify (Markdown), shortcode (Shortcodes), transliterate (Localization)"),
]

URL_FILTERS = [
    ("1", "t: translates the text", "Each numbered placeholder receives an argument, in order"),
    ("2", "href: a URL for the tenant", "/some-page, with the URL prefix of the tenant when it has one"),
    ("3", "absolute_url: a complete URL", "Adds the scheme, the host and the port, such as https://example.com/some-page"),
]

TIME_FILTERS = [
    ("1", "local: UTC to local", "Converts a UTC date and time to the site time zone, or to the time zone of the signed-in user"),
    ("2", "utc: local to UTC", "Converts a local date and time to UTC"),
]

TAG_SOURCES = [
    ("1", "form, antiforgerytoken, helper, block", "Liquid Core Services: the ASP.NET Core tag helpers, and the token field of a form"),
    ("2", "cache", "Liquid Core Services: keeps the output of a block when Dynamic Cache is enabled"),
    ("3", "script, style, link, meta, resources", "The Resources module: the tags for scripts, styles and other resources"),
]

CORE = [
    ("1", "Always enabled", "Liquid Core Services is a feature that you can't turn off"),
    ("2", "The parser and the core objects", "Culture, Request, Environment, HttpContext, TrackingConsent and the sanitize_html filter"),
    ("3", "The parser options", "Read from the Liquid section of the configuration, all off by default"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("where-"):
        image = slide_base(chapter_title, label, "Where Liquid runs")
        bullets(image, WHERE, active={"where-1": {0}, "where-2": {1, 2}, "where-3": {3}}[step_id])
        return image

    if step_id in ("syntax-1", "syntax-2"):
        image = slide_base(chapter_title, label, "Output, filters and tags" if step_id == "syntax-1" else "Properties")
        draw_code(image, (96, 240, 1330, 700), "Content-BlogPost.liquid", "html", POST_HEADER, size=24,
                  highlight=None if step_id == "syntax-1" else {1, 2})
        if step_id == "syntax-1":
            note_cards(image, [("{{ }}", "Prints a value"), ("|", "Applies a filter"), ("{% %}", "Runs a tag")],
                       (1370, 300, W - 96, 1030))
        else:
            note_cards(image, [("A dot", "Reads a property"), ("Content", "Holds parts and fields"),
                               ("Text", "The value of a field")], (1370, 300, W - 96, 1030))
        return image

    if step_id == "syntax-3":
        image = slide_base(chapter_title, label, "Encoding and the raw filter")
        draw_code(image, (96, 240, W - 96, 520), "Content-BlogPost.liquid", "html", RAW_LINE, size=28)
        draw_code(image, (96, 560, W - 96, 1030), "Sanitize, then write the markup as it is", "html", SANITIZE, size=28)
        return image

    if step_id == "objects-1":
        image = slide_base(chapter_title, label, "Model and the content item")
        draw_code(image, (96, 240, 900, 640), "Liquid", "html", CONTENT_ITEM, size=28)
        note_cards(image, [("Model", "The shape being displayed: the view model"), ("ContentItem", "The content item of a content page")],
                   (940, 300, W - 96, 1030))
        return image

    if step_id == "objects-2":
        image = slide_base(chapter_title, label, "The content document")
        draw_code(image, (96, 240, W - 96, 400), "Content-BlogPost.liquid", "html", CONTENT_PATH, size=30)
        bullets(image, CONTENT_WALK, top=450)
        return image

    if step_id == "objects-3":
        image = slide_base(chapter_title, label, "Objects of Liquid Core Services")
        bullets(image, SYSTEM_OBJECTS)
        return image

    if step_id == "objects-4":
        image = slide_base(chapter_title, label, "Objects that features add")
        bullets(image, FEATURE_OBJECTS)
        return image

    if step_id == "filters-1":
        image = slide_base(chapter_title, label, "Chaining filters")
        draw_code(image, (96, 240, W - 96, 400), "Autoroute pattern of the Blog recipe", "html", PATTERN, size=30)
        bullets(image, CHAIN, top=450)
        return image

    if step_id in ("filters-2", "filters-5", "filters-6"):
        image = slide_base(chapter_title, label, "Where filters come from")
        bullets(image, FILTER_SOURCES, active={"filters-2": {0, 1}, "filters-5": {2}, "filters-6": {3}}[step_id])
        return image

    if step_id == "filters-3":
        image = slide_base(chapter_title, label, "Text and URLs")
        draw_code(image, (96, 240, W - 96, 500), "Liquid", "html", TEXT_AND_URLS, size=28)
        bullets(image, URL_FILTERS, top=540)
        return image

    if step_id == "filters-4":
        image = slide_base(chapter_title, label, "Time zones")
        draw_code(image, (96, 240, W - 96, 500), "Liquid", "html", TIME_ZONES, size=28)
        bullets(image, TIME_FILTERS, top=540)
        return image

    if step_id == "tags-1":
        image = slide_base(chapter_title, label, "Standard tags")
        draw_code(image, (96, 240, 1190, 640), "Content-BlogPost.liquid", "html", IMAGE_HEADER, size=24)
        note_cards(image, [("assign", "Stores a value"), ("if, else", "Chooses what to render"),
                           ("for", "Repeats a block for each item")], (1230, 300, W - 96, 1030))
        return image

    if step_id in ("tags-2", "tags-3"):
        image = slide_base(chapter_title, label, "Tags that features add")
        bullets(image, TAG_SOURCES, active={0} if step_id == "tags-2" else {1, 2})
        return image

    if step_id == "parser-1":
        image = slide_base(chapter_title, label, "Liquid Core Services")
        bullets(image, CORE)
        return image

    if step_id == "parser-2":
        image = slide_base(chapter_title, label, "Parser options")
        draw_code(image, (96, 240, 860, 700), "appsettings.json", "json", OPTIONS, size=26)
        note_cards(image, [("AllowParentheses", "Allows parentheses in templates"), ("AllowLiquidTag", "Turns on the liquid tag"),
                           ("AllowTrailingQuestionMark", "A name may end with a question mark")], (900, 300, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
