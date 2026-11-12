"""The slides of the "Create a Liquid Widget Without Writing a Module" video. assemble.py calls slide() for each step
of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the Orchard Core source (Widget.cshtml, Widget.Wrapper.cshtml) and from the documentation page of the
guide, trimmed to what the narration talks about.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

# Names drawn as types in the code panels.
TYPES = {"String", "Model"}

IDEA = [
    ("1", "A content type", "Liquid Widget, with the Liquid part for its body"),
    ("2", "The Widget stereotype", "Layers and Flow parts list the types that have it"),
    ("3", "A template", "Optional: Widget__LiquidWidget changes its markup"),
]

CAUTION = [
    ("!", "Stored exactly as typed", "The body can hold HTML, CSS and JavaScript"),
    ("!", "Guarded by a permission", "Manage Liquid templates: Administrator and Editor roles by default"),
]

ALTERNATES = [
    ("1", "Stereotype", "The fallback for every stereotype"),
    ("2", "Widget", "The shape of every widget"),
    ("3", "Widget__LiquidWidget", "Only the Liquid Widget type, and the most specific"),
]

CLASSES = [
    ("1", "In a layer", "Classes: widget, widget-liquid-widget"),
    ("2", "In a Flow part", "Also widget-align-* and widget-size-*"),
]

STANDARD = """
<div class="@string.Join(" ", Model.Classes.ToArray())">
    @* header and meta ... *@
    <div class="widget-body">
        @await DisplayAsync(Model.Content)
    </div>
    @* footer ... *@
</div>
"""

WRAPPER = """
@{
    var layerMetadata = Model.LayerMetadata;
    // ...
}

<div class="widget-container">
    @if (layerMetadata.RenderTitle)
    {
        <div class="widget-container-title">
            <h2>@Model.Widget.DisplayText</h2>
        </div>
    }

    @await DisplayAsync(Model.Content)
</div>
"""

TEMPLATE = """
<article class="{{ Model.Classes | join: " " }} text-center">
    <h3>{{ Model.ContentItem.DisplayText }}</h3>
    {{ Model.Content.LiquidPart | shape_render }}
</article>
"""

THEME_LIQUID = """
<article class="{{ Model.Classes | join: " " }}">
    {{ Model.Content.LiquidPart | shape_render }}
</article>
"""

THEME_RAZOR = """
<article class="@String.Join(" ", Model.Classes.ToArray())">
    @await DisplayAsync(Model.Content.LiquidPart)
</article>
"""


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "A Liquid widget has three pieces")
        bullets(image, IDEA, active={0, 1})
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Before you create one")
        bullets(image, CAUTION)
        return image

    if step_id == "shape-1":
        image = slide_base(chapter_title, label, "Which template renders a widget")
        bullets(image, ALTERNATES)
        return image

    if step_id == "shape-2":
        image = slide_base(chapter_title, label, "The standard Widget template")
        draw_code(image, (96, 240, 1150, 760), "Widget.cshtml", "cshtml", STANDARD, types=TYPES, size=28)
        note_cards(image, [
            ("The outer div", "Carries the widget classes"),
            ("The widget-body div", "Holds the rendered parts"),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "shape-3":
        image = slide_base(chapter_title, label, "The wrapper of the Layers module")
        draw_code(image, (96, 240, 1150, 900), "Widget.Wrapper.cshtml", "cshtml", WRAPPER, types=TYPES, size=26)
        note_cards(image, [
            ("Outside your template", "The wrapper stays in place"),
            ("Render title", "Prints the title in the wrapper"),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "shape-4":
        image = slide_base(chapter_title, label, "The template of the Liquid Widget type")
        draw_code(image, (96, 240, 1150, 640), "Widget__LiquidWidget", "html", TEMPLATE, types=TYPES, size=28)
        note_cards(image, [
            ("Model.Classes", "The widget classes"),
            ("Model.ContentItem", "The content item"),
            ("Model.Content", "The shapes of the parts"),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "The same template, in two places")
        bullets(image, CLASSES)
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "The same override in a theme")
        draw_code(image, (96, 240, W - 96, 560), "Widget-LiquidWidget.liquid", "html", THEME_LIQUID, types=TYPES, size=28)
        draw_code(image, (96, 620, W - 96, 940), "Widget-LiquidWidget.cshtml", "cshtml", THEME_RAZOR, types=TYPES, size=28)
        return image

    raise SystemExit(f"No slide for {step_id}")
