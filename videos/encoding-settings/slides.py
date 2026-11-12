"""The slides of the encoding settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is taken from the documentation page guides/encoding-settings and from the Orchard Core source
(OrchardCore.Cms.Web/Program.cs, OrchardCore.Title, OrchardCore.Google), trimmed with // ...
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import MARGIN, MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"WebApplication", "WebEncoderOptions", "TextEncoderSettings", "UnicodeRanges", "HtmlEncoder", "UrlEncoder",
         "JavaScriptEncoder", "MyService", "TitleMetaWeblogDriver", "MetaWeblogDriver", "XRpcStruct", "XmlRpcContext",
         "ContentItem", "GoogleAnalyticsFilter", "IAsyncResultFilter", "ResultExecutingContext",
         "ResultExecutionDelegate", "HtmlContentBuilder", "Task"}

ENCODERS = [
    ("1", "HtmlEncoder", "Encodes text that is written into HTML."),
    ("2", "UrlEncoder", "Encodes values that are placed in a URL."),
    ("3", "JavaScriptEncoder", "Encodes values that are placed in JavaScript."),
    ("4", "The default safe list", "Basic Latin only, which is ASCII. Everything else is encoded."),
]

TEXT_TO_WRITE = """
<h1>Café Münster</h1>
"""

SOURCE_DEFAULT = """
<h1>Caf&#xE9; M&#xFC;nster</h1>
"""

SOURCE_WIDER = """
<h1>Café Münster</h1>
"""

REGISTRATION = [
    ("1", "Registered once", "Orchard Core calls AddWebEncoders() for the host."),
    ("2", "Taken from injection", "Liquid templates and the shape renderer ask for an HtmlEncoder."),
    ("3", "One setting", "WebEncoderOptions decides which characters stay as they are."),
]

PROGRAM = """
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrchardCms();

builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

var app = builder.Build();
// ...
app.UseOrchardCore();

await app.RunAsync();
"""

SHARED = [
    ("1", "UnicodeRanges.All", "The whole Basic Multilingual Plane."),
    ("2", "HtmlEncoder", "Uses the options of WebEncoderOptions."),
    ("3", "UrlEncoder", "Uses the same options."),
    ("4", "JavaScriptEncoder", "Uses the same options."),
]

CAUTION = [
    ("1", "Cautious by default", "ASP.NET Core documents it as protection against browser bugs."),
    ("2", "Allow what you use", "Name the ranges your text needs, instead of all of them."),
]

RANGES = """
builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(
        UnicodeRanges.BasicLatin,
        UnicodeRanges.Latin1Supplement);
});
"""

HOST = [
    ("1", "The host application", "The Program.cs file of your web application holds the call."),
    ("2", "Shared with every tenant", "Singleton services of the host are shared across tenant containers."),
    ("3", "The encoders are singletons", "Every tenant uses the encoders of the host."),
]

MY_SERVICE = """
public class MyService
{
    private readonly HtmlEncoder _htmlEncoder;

    public MyService(HtmlEncoder htmlEncoder)
    {
        _htmlEncoder = htmlEncoder;
    }
}
"""

TITLE_DRIVER = """
public sealed class TitleMetaWeblogDriver : MetaWeblogDriver
{
    private readonly HtmlEncoder _encoder;

    public TitleMetaWeblogDriver(HtmlEncoder encoder)
    {
        _encoder = encoder;
    }

    public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
    {
        // ...
        rpcStruct.Set("title", _encoder.Encode(titlePart.Title));
    }
}
"""

STATIC_OR_INJECTED = [
    ("No", "HtmlEncoder.Default", "Always the default safe list: Basic Latin only."),
    ("check", "An injected HtmlEncoder", "Follows the ranges you configured."),
]

ANALYTICS = """
public sealed class GoogleAnalyticsFilter : IAsyncResultFilter
{
    private readonly JavaScriptEncoder _jsEncoder;
    private readonly UrlEncoder _urlEncoder;

    public GoogleAnalyticsFilter(
        // ...
        JavaScriptEncoder jsEncoder,
        UrlEncoder urlEncoder)
    {
        // ...
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // ...
        _resourceManager.RegisterHeadScript(new HtmlContentBuilder([
            s_preamble, _urlEncoder.Encode(settings.TrackingID),
            s_middle, _jsEncoder.Encode(settings.TrackingID), s_end]));
    }
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "The encoders of ASP.NET Core")
        bullets(image, ENCODERS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "What the page source holds")
        draw_code(image, (96, 250, W - 96, 480), "The text to write", "html", TEXT_TO_WRITE, size=36)
        draw_code(image, (96, 530, W - 96, 760), "Page source, default encoders", "html", SOURCE_DEFAULT, size=36)
        draw = ImageDraw.Draw(image)
        draw.text((MARGIN, 850), "Each accented letter is written as a character code.", font=font(34), fill=TEXT,
                  anchor="lm")
        draw.text((MARGIN, 910), "The browser shows the same text.", font=font(34), fill=MUTED, anchor="lm")
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "One setting reaches the rendering")
        bullets(image, REGISTRATION)
        return image

    if step_id == "config-1":
        image = slide_base(chapter_title, label, "Configure WebEncoderOptions")
        draw_code(image, (96, 240, W - 96, 990), "Program.cs", "csharp", PROGRAM, types=TYPES, size=26,
                  highlight={1, 2, 3, 9, 10, 11, 12})
        return image

    if step_id == "config-2":
        image = slide_base(chapter_title, label, "One call for three encoders")
        bullets(image, SHARED)
        return image

    if step_id == "config-3":
        image = slide_base(chapter_title, label, "Allow only what you use")
        bullets(image, CAUTION)
        return image

    if step_id == "config-4":
        image = slide_base(chapter_title, label, "Name the ranges")
        draw_code(image, (96, 240, 1210, 640), "Program.cs", "csharp", RANGES, types=TYPES, size=26)
        note_cards(image, [
            ("BasicLatin", "The ASCII range."),
            ("Latin1Supplement", "Letters such as é and ü."),
        ], (1260, 240, W - 96, 700))
        return image

    if step_id == "config-5":
        image = slide_base(chapter_title, label, "One setting for the host")
        bullets(image, HOST)
        return image

    if step_id == "config-6":
        image = slide_base(chapter_title, label, "The same title, two page sources")
        draw_code(image, (96, 290, 930, 520), "Default encoders", "html", SOURCE_DEFAULT, size=32)
        draw_code(image, (990, 290, W - 96, 520), "BasicLatin and Latin1Supplement", "html", SOURCE_WIDER, size=32)
        draw = ImageDraw.Draw(image)
        draw.text((MARGIN, 640), "The browser shows the same text either way.", font=font(34), fill=TEXT, anchor="lm")
        draw.text((MARGIN, 700), "The source is shorter and more readable.", font=font(34), fill=MUTED, anchor="lm")
        return image

    if step_id == "inject-1":
        image = slide_base(chapter_title, label, "1. Take the encoder in the constructor")
        draw_code(image, (96, 240, 1210, 760), "MyService.cs", "csharp", MY_SERVICE, types=TYPES, size=28)
        note_cards(image, [
            ("From the container", "Not from a static property."),
            ("Your ranges", "Apply to this encoder."),
        ], (1260, 240, W - 96, 700))
        return image

    if step_id == "inject-2":
        image = slide_base(chapter_title, label, "2. Call Encode on it")
        draw_code(image, (96, 240, W - 96, 850), "TitleMetaWeblogDriver.cs", "csharp", TITLE_DRIVER, types=TYPES,
                  size=24)
        return image

    if step_id == "inject-3":
        image = slide_base(chapter_title, label, "Static or injected")
        bullets(image, STATIC_OR_INJECTED)
        return image

    if step_id == "inject-4":
        image = slide_base(chapter_title, label, "The same for URLs and JavaScript")
        draw_code(image, (96, 240, W - 96, 1040), "GoogleAnalyticsFilter.cs", "csharp", ANALYTICS, types=TYPES,
                  size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
