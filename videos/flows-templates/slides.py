"""The slides of the "Flow and Bag Parts: Templates and Alternates for Their Widgets" video.

assemble.py calls slide() for each step of a "slide" clip. Everything visual comes from brand.py, so the slides look
like those of every other Orchard Core video. The code is taken from the Orchard Core source (the Flows module, the
display drivers, the themes and the Agency recipe), trimmed with "// ..." where it is cut.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"FlowMetadata", "FlowPartViewModel", "BagPartViewModel", "OrchardCoreConstants"}

# FlowPart.cshtml of the Flows module, trimmed. One line is wrapped for the slide.
FLOW_PART = """
@foreach (var widget in Model.FlowPart.Widgets)
{
    // ... skips a type that is not a widget, and, for a securable type, a widget the user may not view

    var widgetContent = await ContentItemDisplayManager.BuildDisplayAsync(
        widget,
        Model.BuildPartDisplayContext.Updater,
        Model.BuildPartDisplayContext.DisplayType,
        Model.BuildPartDisplayContext.GroupId);

    if (widget.TryGet<FlowMetadata>(out var flowMetadata))
    {
        widgetContent.Classes.Add("widget");
        widgetContent.Classes.Add("widget-" + widget.ContentItem.ContentType.HtmlClassify());
        widgetContent.Classes.Add("widget-align-" + flowMetadata.Alignment.ToString().ToLowerInvariant());
        widgetContent.Classes.Add("widget-size-" + flowMetadata.Size);
    }

    @await DisplayAsync(widgetContent)
}
"""

# Views/Widget-Paragraph.liquid of The Blog Theme.
WIDGET_PARAGRAPH = """
<article class="{{ Model.Classes | join: " " }}">
    {{ Model.Content.Paragraph-Content | shape_render }}
</article>
"""

# BagPart.cshtml of the Flows module, trimmed. One line is wrapped for the slide.
BAG_PART = """
<section class="flow">
    @foreach (var item in Model.BagPart.ContentItems)
    {
        var itemContent = await ContentItemDisplayManager.BuildDisplayAsync(
            item,
            Model.BuildPartDisplayContext.Updater,
            Model.Settings.DisplayType ?? "Summary",
            Model.BuildPartDisplayContext.GroupId);

        @await DisplayAsync(itemContent)
    }
</section>
"""

# FlowPartDisplayDriver.cs and BagPartDisplayDriver.cs of the Flows module, trimmed.
FLOW_DRIVER = """
return Initialize<FlowPartViewModel>(hasItems ? "FlowPart" : "FlowPart_Empty", m =>
{
    // ...
})
.Location(OrchardCoreConstants.DisplayType.Detail, "Content");
"""

BAG_DRIVER = """
return Initialize<BagPartViewModel>(hasItems ? "BagPart" : "BagPart_Empty", m =>
{
    // ...
})
.Location(OrchardCoreConstants.DisplayType.Detail, "Content")
.Location(OrchardCoreConstants.DisplayType.Summary, "Content");
"""

# From the documentation of the Flows module (Empty Flows and Bags).
EMPTY_PLACEMENT = """
{
    "FlowPart_Empty": [
        {
            "shape": "FlowPart"
        }
    ]
}
"""

# Views/FlowPart.cshtml of The Default Theme, trimmed.
THEME_FLOW_PART = """
<section class="flow row">
    @foreach (var widget in Model.FlowPart.Widgets)
    {
        // ... the same checks, classes and rendering as the module
    }
</section>
"""

# The landing page template of the Agency recipe, trimmed.
LANDING_PAGE = """
{% for service in Model.ContentItem.Content.Services.ContentItems %}
    {% assign serviceDisplay = service | shape_build_display %}
    <div class="col-md-4">
        <!-- ... icon -->
        <h4 class="service-heading">{{ service.DisplayText }}</h4>
        <div class="text-muted">{{ serviceDisplay.Content.HtmlBodyPart | shape_render }}</div>
    </div>
{% endfor %}
"""

# The comments that DefaultHtmlDisplay.cs writes around the Widget shape of a Paragraph widget in a theme: the binding
# name is the lowercase shape type of the file, and the binding source is the path of the file in its extension.
DEBUG_COMMENTS = """
<!--shape-start type:Widget bindings:widget__paragraph => Areas/TheBlogTheme/Views/Widget-Paragraph.liquid (liquid) -->
...
<!--shape-end type:Widget -->
"""

CONSOLE_LOG = """
{{ Model.Content | shape_render }}
{{ Model.Content | console_log }}
"""


def slide(step_id, chapter_title, label):
    if step_id.startswith("flow-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "The Flow Part template builds each widget")
        highlight = {1: {1, 2, 3, 20}, 2: {5, 6, 7, 8, 9}, 3: {11, 12, 13, 14, 15, 16, 17}}[index]
        draw_code(image, (96, 240, W - 96, 1020), "FlowPart.cshtml", "csharp", FLOW_PART, types=TYPES,
                  highlight=highlight, size=24)
        return image

    if step_id == "widget-1":
        image = slide_base(chapter_title, label, "A template for Paragraph widgets")
        draw_code(image, (96, 240, W - 96, 470), "Widget-Paragraph.liquid", "html", WIDGET_PARAGRAPH, size=30)
        bullets(image, [
            ("1", "The file", "Views/Widget-Paragraph.liquid, in The Blog Theme"),
            ("2", "The alternate", "Widget__Paragraph: the shape Widget, for the content type Paragraph"),
            ("3", "The classes", "Model.Classes keeps the widget, alignment and size classes of the Flow"),
        ], top=500)
        return image

    if step_id == "widget-2":
        image = slide_base(chapter_title, label, "From a file name to an alternate")
        bullets(image, [
            ("-", "A dash is two underscores", "Widget-Paragraph.liquid is the alternate Widget__Paragraph"),
            (".", "A dot adds a display type", "Widget-Paragraph.Summary.liquid is the alternate Widget_Summary__Paragraph"),
        ], top=400)
        return image

    if step_id == "widget-3":
        image = slide_base(chapter_title, label, "The classes of a widget in a Flow")
        bullets(image, [
            ("1", "widget", "Added to every widget that has Flow metadata"),
            ("2", "widget-paragraph", "The content type, in lowercase, with dashes between words"),
            ("3", "widget-align-justify", "The alignment: left, center, right, justify or inherit"),
            ("4", "widget-size-100", "The size chosen in the editor: 25, 33, 50, 66, 75 or 100"),
        ])
        return image

    if step_id == "bag-1":
        image = slide_base(chapter_title, label, "The Bag Part template builds each item")
        draw_code(image, (96, 240, 1296, 760), "BagPart.cshtml", "html", BAG_PART, types=TYPES, highlight={7}, size=22)
        note_cards(image, [
            ("Display Type", "The setting of the Bag Part"),
            ("Summary", "Used when it has no value"),
        ], (1340, 240, W - 96, 700))
        return image

    if step_id == "items-1":
        image = slide_base(chapter_title, label, "The shape of each item")
        bullets(image, [
            ("1", "A Paragraph widget renders as Widget",
             "Widget_Summary__Paragraph, the file Widget-Paragraph.Summary.liquid"),
            ("2", "A Service item has no stereotype and renders as Content",
             "Content_Summary__Service, the file Content-Service.Summary.liquid"),
        ], top=400)
        return image

    if step_id == "items-2":
        image = slide_base(chapter_title, label, "Name the template for the display type")
        bullets(image, [
            ("x", "Widget-Paragraph.liquid", "Targets Detail: it does not apply to a Bag Part that renders Summary"),
            ("check", "Widget-Paragraph.Summary.liquid", "Targets Summary: it applies to that Bag Part"),
        ], top=400)
        return image

    if step_id == "part-1":
        image = slide_base(chapter_title, label, "Alternates of the Bag Part shape")
        bullets(image, [
            ("1", "BagPart.liquid", "The BagPart shape: every Bag Part"),
            ("2", "LandingPage-BagPart.liquid", "Every Bag Part of the content type LandingPage"),
            ("3", "LandingPage-Services.liquid", "Only the Bag Part named Services"),
            ("4", "LandingPage-Services.Summary.liquid", "That Bag Part, in the Summary display type"),
        ])
        return image

    if step_id == "part-2":
        image = slide_base(chapter_title, label, "Where each part displays by default")
        draw_code(image, (96, 240, W - 96, 540), "FlowPartDisplayDriver.cs", "csharp", FLOW_DRIVER, types=TYPES,
                  highlight={5}, size=28)
        draw_code(image, (96, 580, W - 96, 920), "BagPartDisplayDriver.cs", "csharp", BAG_DRIVER, types=TYPES,
                  highlight={5, 6}, size=28)
        return image

    if step_id == "part-3":
        image = slide_base(chapter_title, label, "Empty Flows and empty Bags")
        draw_code(image, (96, 240, 1040, 700), "placement.json", "json", EMPTY_PLACEMENT, size=32)
        note_cards(image, [
            ("FlowPart.Empty.cshtml", "An empty file in the Flows module"),
            ("BagPart.Empty.cshtml", "An empty file in the Flows module"),
        ], (1090, 240, W - 96, 700))
        return image

    if step_id == "part-4":
        image = slide_base(chapter_title, label, "Replacing the template of the part")
        draw_code(image, (96, 240, 1190, 640), "FlowPart.cshtml (The Default Theme)", "csharp", THEME_FLOW_PART,
                  size=28)
        note_cards(image, [
            ("You take over", "The permission check"),
            ("You take over", "The widget classes"),
            ("The Default Theme", "Adds row to the section"),
        ], (1230, 240, W - 96, 900))
        return image

    if step_id in ("direct-1", "direct-2"):
        image = slide_base(chapter_title, label, "Reading the Bag Part directly")
        draw_code(image, (96, 240, W - 96, 700), "Content__LandingPage (Agency recipe)", "html", LANDING_PAGE,
                  highlight={1} if step_id == "direct-1" else {2, 6}, size=27)
        return image

    if step_id == "source-1":
        image = slide_base(chapter_title, label, "Shape debug information")
        draw_code(image, (96, 240, W - 96, 560), "Page source", "html", DEBUG_COMMENTS, size=24)
        draw_code(image, (96, 600, W - 96, 860), "Template, outside the Production environment", "html", CONSOLE_LOG,
                  size=28)
        return image

    raise SystemExit(f"No slide for {step_id}")
