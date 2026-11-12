"""The slides of the Media in Templates video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The Liquid and Razor panels use the "html" and "cshtml" lexers of brand.py.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IImageProcessingEngine", "ImageProcessingResult", "ImageProcessingCommands", "MyImageProcessingEngine",
         "Stream", "Task", "CancellationToken", "ServiceDescriptor", "ResizeMode", "Format"}


def panel(image, title, language, code, size, top=240, types=()):
    """A code panel at full width, as tall as its lines need, starting at top."""
    lines = len(code.strip("\n").split("\n"))
    bottom = min(1030, top + 74 + lines * int(size * 1.42) + 36)
    draw_code(image, (96, top, W - 96, bottom), title, language, code, types=types, size=size)
    return bottom


# From a path to an image.
STAGES = [
    ("1", "A path in the library", "home-bg.jpg, as a media field stores it"),
    ("2", "An asset URL", "/media/home-bg.jpg, with the URL prefix of the tenant and the CDN base URL"),
    ("3", "A resized URL", "/media/home-bg.jpg?width=480&height=240&rmode=crop&token=..."),
    ("4", "A cached file", "The first request resizes the image, the next ones read the cache"),
]

CONFIGURATION = """
{
  "OrchardCore": {
    "Media": {
      "SupportedSizes": [
        16, 32, 50, 100, 160,
        240, 480, 600, 1024, 2048
      ],
      "UseTokenizedQueryString": true
    }
  }
}
"""

# asset_url and img_tag.
LIQUID_URL = """
{% assign image = Model.ContentItem.Content.Blog.Image.Paths.first %}

{{ image | asset_url }}
<!-- /media/home-bg.jpg -->

{{ image | asset_url | img_tag: alt: 'Home', class: 'figure-img', data_order: 1 }}
<!-- <img src="/media/home-bg.jpg" alt="Home" class="figure-img" data-order="1" /> -->
"""

# resize_url.
LIQUID_RESIZE = """
{% assign path = 'home-bg.jpg' %}

{{ path | asset_url | resize_url: width: 480, height: 240 }}
<!-- /media/home-bg.jpg?width=480&height=240&token=... -->

{{ path | asset_url | resize_url: width: 480 }}
<!-- /media/home-bg.jpg?width=480&token=... -->
"""

MODES = [
    ("1", "max", "Fits inside the size and keeps the proportions. This is the default."),
    ("2", "crop", "Fills the size and cuts what falls outside."),
    ("3", "pad", "Fits inside the size and fills the rest with the background color."),
    ("4", "stretch", "Fills the size and ignores the proportions."),
]

LIQUID_ENCODING = """
{{ path | asset_url | resize_url: width: 480, height: 480, mode: 'pad', bgcolor: '404041' }}
<!-- /media/home-bg.jpg?width=480&height=480&rmode=pad&bgcolor=404041&token=... -->

{{ path | asset_url | resize_url: width: 480, format: 'webp', quality: 50 }}
<!-- /media/home-bg.jpg?width=480&format=webp&quality=50&token=... -->
"""

LIQUID_FIELD = """
{% assign field = Model.ContentItem.Content.BlogPost.Image %}
{% for path in field.Paths %}
  {% assign anchor = field.Anchors[forloop.index0] %}
  <img src="{{ path | asset_url | resize_url: width: 480, height: 240, mode: 'crop', anchor: anchor }}"
       alt="{{ field.MediaTexts[forloop.index0] }}" />
{% endfor %}
"""

LIQUID_PROFILE = """
{{ path | asset_url | resize_url: profile: 'thumbnail' }}
<!-- /media/home-bg.jpg?width=160&height=160&rmode=crop&token=... -->

{{ path | asset_url | resize_url: profile: 'thumbnail', mode: 'max' }}
<!-- /media/home-bg.jpg?width=160&height=160&rmode=max&token=... -->
"""

# From MediaField.cshtml in the Media module.
RAZOR_DISPLAY = """
@for (int i = 0; i < paths.Length; i++)
{
    var path = paths[i];
    var url = Orchard.AssetUrl(path);

    if (Orchard.IsImageFile(path))
    {
        // ...
        <img src="@url" alt="@alt" />
    }
    else
    {
        // ...
        <a href="@url">@text</a>
    }
}
"""

RAZOR_ASSET = """
@using OrchardCore.Media.Core.Processing

<img src="@Orchard.AssetUrl(path, width: 480, height: 240, resizeMode: ResizeMode.Crop)" alt="..." />

@Orchard.AssetUrl(path, width: 480, format: Format.WebP, quality: 50)
@Orchard.AssetUrl(path, width: 480, height: 480, resizeMode: ResizeMode.Pad, bgColor: "404041")
@Orchard.AssetUrl(path, width: 480, height: 240, resizeMode: ResizeMode.Crop, anchor: anchor)
"""

RAZOR_PROFILE = """
@using OrchardCore.Media.Core.Processing

@await Orchard.AssetProfileUrlAsync(path, "thumbnail")

@await Orchard.AssetProfileUrlAsync(path, "thumbnail", resizeMode: ResizeMode.Max)
"""

TAG_IMPORTS = """
@addTagHelper *, OrchardCore.Media
"""

TAG_BASIC = """
<img asset-src="@Model.Field.Paths[0]" alt="@Model.Field.MediaTexts[0]" />

<a asset-href="reports/summary.pdf">Download</a>
"""

TAG_RESIZE = """
<img asset-src="@Model.Field.Paths[0]" alt="..."
     img-width="480" img-height="240" img-resize-mode="Crop" />

<img asset-src="@Model.Field.Paths[0]" alt="..."
     img-width="480" img-quality="50" img-format="WebP" />

<img asset-src="@Model.Field.Paths[0]" alt="..."
     img-width="480" img-height="480" img-resize-mode="Pad" img-bgcolor="404041" />
"""

TAG_PROFILE = """
@using OrchardCore.Media.Fields

<img asset-src="@Model.Field.Paths[0]" alt="..." img-profile="thumbnail" />

<img asset-src="@Model.Field.Paths[0]" alt="..." img-profile="thumbnail" img-resize-mode="Max" />

<img asset-src="@Model.Field.Paths[0]" alt="..." img-width="480" img-height="240"
     img-resize-mode="Crop" img-anchor="@Model.Field.GetAnchors().FirstOrDefault()" />

<img asset-src="@Model.Field.Paths[0]" alt="..." asp-append-version="true" />
"""

VERSIONS = """
{{ path | asset_url | append_version | img_tag }}

@Orchard.AssetUrl(path, appendVersion: true)

<img asset-src="@Model.Field.Paths[0]" alt="..." asp-append-version="true" />
"""

ENGINE = """
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Media.Processing;

public class MyImageProcessingEngine : IImageProcessingEngine
{
    public Task<ImageProcessingResult> ProcessAsync(
        Stream input,
        ImageProcessingCommands commands,
        CancellationToken cancellationToken = default)
    {
        // ...
    }
}

// In ConfigureServices, after the services of the Media module are configured:
services.Replace(ServiceDescriptor.Singleton<IImageProcessingEngine, MyImageProcessingEngine>());
"""


def slide(step_id, chapter_title, label):
    if step_id in ("path-1", "path-2"):
        image = slide_base(chapter_title, label, "From a path to an image")
        bullets(image, STAGES, active={0, 1} if step_id == "path-1" else {2, 3})
        return image

    if step_id == "path-3":
        image = slide_base(chapter_title, label, "Signed commands and supported sizes")
        draw_code(image, (96, 240, 900, 800), "appsettings.json", "json", CONFIGURATION, size=26)
        note_cards(image, [
            ("With a valid token", "Any width and height, and an anchor"),
            ("Without a token", "Only the supported sizes"),
            ("With an invalid token", "The original file is served"),
        ], (940, 240, W - 96, 1030))
        return image

    if step_id == "liquid-1":
        image = slide_base(chapter_title, label, "asset_url and img_tag")
        panel(image, "Image.liquid", "html", LIQUID_URL, 30)
        return image

    if step_id == "liquid-2":
        image = slide_base(chapter_title, label, "resize_url")
        panel(image, "Image.liquid", "html", LIQUID_RESIZE, 30)
        return image

    if step_id == "liquid-3":
        image = slide_base(chapter_title, label, "Resize modes")
        bullets(image, MODES, active={1, 2, 3})
        return image

    if step_id == "liquid-4":
        image = slide_base(chapter_title, label, "Background color, quality and format")
        panel(image, "Image.liquid", "html", LIQUID_ENCODING, 27)
        return image

    if step_id == "liquid-5":
        image = slide_base(chapter_title, label, "Media text and anchors")
        panel(image, "Image.liquid", "html", LIQUID_FIELD, 27)
        return image

    if step_id == "profile-4":
        image = slide_base(chapter_title, label, "The profile argument")
        panel(image, "Image.liquid", "html", LIQUID_PROFILE, 30)
        return image

    if step_id == "razor-1":
        image = slide_base(chapter_title, label, "The Orchard helper")
        panel(image, "MediaField.cshtml", "cshtml", RAZOR_DISPLAY, 28)
        return image

    if step_id == "razor-2":
        image = slide_base(chapter_title, label, "AssetUrl and its options")
        panel(image, "Image.cshtml", "cshtml", RAZOR_ASSET, 26, types=TYPES)
        return image

    if step_id == "razor-3":
        image = slide_base(chapter_title, label, "Media profiles in Razor")
        panel(image, "Image.cshtml", "cshtml", RAZOR_PROFILE, 30, types=TYPES)
        return image

    if step_id == "tag-1":
        image = slide_base(chapter_title, label, "asset-src and asset-href")
        bottom = panel(image, "_ViewImports.cshtml", "cshtml", TAG_IMPORTS, 30)
        panel(image, "MediaField.cshtml", "cshtml", TAG_BASIC, 30, top=bottom + 40)
        return image

    if step_id == "tag-2":
        image = slide_base(chapter_title, label, "Resize attributes")
        panel(image, "MediaField.cshtml", "cshtml", TAG_RESIZE, 28)
        return image

    if step_id == "tag-3":
        image = slide_base(chapter_title, label, "Profiles, anchors and versions")
        panel(image, "MediaField.cshtml", "cshtml", TAG_PROFILE, 25)
        return image

    if step_id == "end-1":
        image = slide_base(chapter_title, label, "Versioned URLs")
        panel(image, "Liquid, Razor and tag helpers", "html", VERSIONS, 28)
        return image

    if step_id == "end-2":
        image = slide_base(chapter_title, label, "A custom image engine")
        panel(image, "MyImageProcessingEngine.cs", "csharp", ENGINE, 26, types=TYPES)
        return image

    raise SystemExit(f"No slide for {step_id}")
