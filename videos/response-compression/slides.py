"""The slides of the Response Compression video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Startup", "StartupBase", "IApplicationBuilder", "IEndpointRouteBuilder", "IServiceProvider",
         "IServiceCollection"}

EXCHANGE = """
# The request
GET / HTTP/1.1
Accept-Encoding: gzip

# The response
HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Content-Encoding: gzip
"""

RECIPE = """
{
  "steps": [
    {
      "name": "Feature",
      "enable": [
        "OrchardCore.ResponseCompression"
      ]
    }
  ]
}
"""

STARTUP = """
// ...

public sealed class Startup : StartupBase
{
    public override int Order => -5;

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseResponseCompression();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddResponseCompression(options => options.EnableForHttps = true);
    }
}
"""

PROGRAM = """
var app = builder.Build();

// ...

app.UseStaticFiles();

app.UseOrchardCore();

// ...
"""

CURL = """
# Ask for gzip
curl --insecure --silent --output /dev/null --dump-header - \\
  --header "Accept-Encoding: gzip" https://localhost:5001/

# Ask for Brotli
curl --insecure --silent --output /dev/null --dump-header - \\
  --header "Accept-Encoding: br" https://localhost:5001/
"""

HOW = [
    ("check", "Compresses well", "HTML, CSS, JavaScript, JSON and XML."),
    ("2", "Gains little", "Files that are already compressed, such as PNG images."),
    ("3", "Costs server time", "Compressing a response takes processing time."),
]

REQUEST_RULES = [
    ("1", "The request", "Its Accept-Encoding header names a format the middleware supports."),
    ("2", "The response", "Its content type is on the list of types to compress."),
    ("3", "The default list", "HTML, CSS, JavaScript, JSON, XML, plain text and WebAssembly."),
]

CHECK = [
    ("1", "No Accept-Encoding header", "The response is not compressed."),
    ("2", "Several formats, no weights", "The middleware prefers Zstandard, then Brotli, then gzip."),
    ("3", "A web server in front", "It can compress too."),
]

WHEN = [
    ("1", "In front of the application", "A reverse proxy or a web server, such as IIS or Nginx, or a CDN."),
    ("2", "In the application", "This module, when nothing in front of the application compresses."),
]

RISK = [
    ("1", "EnableForHttps is true", "HTTPS responses are compressed too."),
    ("!", "The risk", "Dynamically generated, secured responses can be exposed to attacks such as BREACH."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Asking for, and sending, compressed content")
        draw_code(image, (96, 240, W - 96, 760), "An HTTP exchange", "bash", EXCHANGE, size=30)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What compression is for")
        bullets(image, HOW)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "The Feature recipe step")
        draw_code(image, (96, 240, 1100, 800), "site.recipe.json", "json", RECIPE, size=30)
        note_cards(image, [
            ("name", "The step is Feature."),
            ("enable", "The identifiers to turn on."),
        ], (1160, 280, W - 96, 1030))
        return image

    if step_id == "code-1":
        image = slide_base(chapter_title, label, "ConfigureServices")
        draw_code(image, (96, 240, W - 96, 880), "Startup.cs", "csharp", STARTUP, types=TYPES, size=24,
                  highlight={3, 4, 12, 13, 14, 15, 16})
        return image

    if step_id == "code-2":
        image = slide_base(chapter_title, label, "Configure and the order")
        draw_code(image, (96, 240, W - 96, 880), "Startup.cs", "csharp", STARTUP, types=TYPES, size=24,
                  highlight={3, 4, 5, 7, 8, 9, 10, 16})
        return image

    if step_id == "code-3":
        image = slide_base(chapter_title, label, "What the middleware compresses")
        bullets(image, REQUEST_RULES)
        return image

    if step_id == "code-4":
        image = slide_base(chapter_title, label, "Outside the tenant pipeline")
        draw_code(image, (96, 240, W - 96, 800), "Program.cs", "csharp", PROGRAM, size=30)
        return image

    if step_id == "check-1":
        image = slide_base(chapter_title, label, "One format per request")
        draw_code(image, (96, 240, 1200, 800), "Terminal", "bash", CURL, size=22)
        note_cards(image, [
            ("Accept-Encoding: gzip", "Content-Encoding: gzip"),
            ("Accept-Encoding: br", "Content-Encoding: br"),
        ], (1250, 280, W - 96, 1030))
        return image

    if step_id == "check-2":
        image = slide_base(chapter_title, label, "When the result is unexpected")
        bullets(image, CHECK)
        return image

    if step_id == "when-1":
        image = slide_base(chapter_title, label, "Where to compress")
        bullets(image, WHEN)
        return image

    if step_id == "when-2":
        image = slide_base(chapter_title, label, "Compression over HTTPS")
        bullets(image, RISK)
        return image

    raise SystemExit(f"No slide for {step_id}")
