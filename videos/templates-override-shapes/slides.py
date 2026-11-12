"""The slides of the Templates video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

SHAPES = [
    ("1", "Shape", "A piece of a page, with a type such as Content_Summary."),
    ("2", "Alternates", "More specific names for it, such as Content_Summary__BlogPost."),
    ("3", "View file", "A file of the theme or of a module answers to a name."),
    ("4", "Template", "Liquid stored by the Templates module answers to a name."),
]

ORDER = [
    ("1", "The most specific alternate first", "The alternates of the shape are tried one by one."),
    ("2", "A template with that name", "The Templates module is asked first."),
    ("3", "A view file with that name", "The views of the theme and of the modules come second."),
    ("4", "No match: the next alternate", "The same two checks run again."),
]

WHERE = [
    ("1", "The pages of the site", "Templates are used when the site renders a page."),
    ("2", "Not the admin screens", "Admin Templates is the feature for those."),
    ("3", "The database of the site", "Saving a template needs no build."),
]

PERMISSIONS = [
    ("1", "Manage templates", "A permission of the Templates module."),
    ("2", "Manage Liquid templates", "A permission of the Liquid module."),
    ("3", "Both are required", "Both are security critical. Administrator and Editor roles have both."),
]

NAMES = """
Content__[ContentType]
Content_[DisplayType]__[ContentType]
Content__Slug__[Slug]
Content__Alias__[Alias]
Widget__[ContentType]
[ContentType]_[DisplayType]__[PartType]
[PartType]__[FieldName]
Zone__[ZoneName]
"""

SUMMARY = """
<div class="post-preview">
  <a href="{{ Model.ContentItem | display_url }}">
    <h2 class="post-title">{{ Model.ContentItem.DisplayText }}</h2>
    <h3 class="post-subtitle">
      {{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}
    </h3>
  </a>
  <p class="post-meta">
    Written by {{ Model.ContentItem.Author }}
  </p>
</div>
<hr class="my-4" />
"""

ZONE = """
{% shape_remove_item Model.Content "HtmlBodyPart" %}
{% shape_remove_item Model.Content "Blog-Image" %}
{{ Model.Content | shape_render }}
"""

COMMENTS = """
<!--shape-start type:Content_Summary bindings:Content_Summary__BlogPost => Templates/Content_Summary__BlogPost.liquid (liquid) -->
    ... the HTML of the shape ...
<!--shape-end type:Content_Summary -->
"""

SPECIFIC = [
    ("1", "Content_Summary__BlogPost", "Blog posts in summary views only."),
    ("2", "Content_Summary", "Content types in summary views with no more specific match."),
    ("3", "Content__BlogPost", "Blog posts in detail views."),
]

RECIPE = """
{
  "name": "Templates",
  "Templates": {
    "Content_Summary__BlogPost": {
      "Description": "Summary of a blog post in a list",
      "Content": "[file:text('Templates/Content.Summary-BlogPost.liquid')]"
    }
  }
}
"""

EXPORT = [
    ("1", "All Templates", "The deployment plan step that exports every template."),
    ("2", "Export templates as files", "Each template goes to its own file under Templates."),
    ("3", "The Templates recipe step", "The step exports the same structure the recipe reads."),
]


def slide(step_id, chapter_title, label):
    if step_id in ("idea-1", "idea-2"):
        image = slide_base(chapter_title, label, "A page is built from shapes")
        bullets(image, SHAPES, active={0, 1} if step_id == "idea-1" else {1, 2, 3})
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Which one renders a shape?")
        bullets(image, ORDER)
        return image

    if step_id == "idea-4":
        image = slide_base(chapter_title, label, "Where templates apply")
        bullets(image, WHERE)
        return image

    if step_id == "idea-5":
        image = slide_base(chapter_title, label, "Who can edit templates")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "names-1":
        image = slide_base(chapter_title, label, "Names you can override")
        draw_code(image, (96, 240, 1010, 760), "Names in the documentation", "html", NAMES, size=28)
        note_cards(image, [
            ("Content items", "By type, display type, slug or alias."),
            ("Widgets and zones", "By content type, or by zone name."),
            ("Parts and fields", "By part type, content type or field."),
        ], (1060, 240, W - 96, 900))
        return image

    if step_id == "inside-1":
        image = slide_base(chapter_title, label, "What a template can use")
        draw_code(image, (96, 240, 1110, 780), "Content_Summary__BlogPost", "html", SUMMARY, size=24)
        note_cards(image, [
            ("Model", "The shape being rendered."),
            ("Model.ContentItem", "The content item itself."),
            ("Model.ContentItem.Content", "Data of its parts and fields."),
        ], (1160, 240, W - 96, 900))
        return image

    if step_id == "inside-2":
        image = slide_base(chapter_title, label, "The shapes of a content item")
        draw_code(image, (96, 240, 1110, 560), "Content__Blog", "html", ZONE, size=25)
        note_cards(image, [
            ("Model.Content", "A zone of part and field shapes."),
            ("shape_render", "Draws a shape."),
            ("shape_remove_item", "Removes a shape by name."),
        ], (1160, 240, W - 96, 900))
        return image

    if step_id == "inside-3":
        image = slide_base(chapter_title, label, "The properties depend on the shape")
        bullets(image, [
            ("1", "Content item", "Model.ContentItem and Model.Content."),
            ("2", "Widget", "Those of a content item, and Model.Classes."),
            ("3", "Zone", "Model.ZoneName, Model.Parent and Model.Items."),
        ])
        return image

    if step_id == "debug-2":
        image = slide_base(chapter_title, label, "Shape debug information")
        draw_code(image, (96, 240, W - 96, 520), "Page source", "html", COMMENTS, size=22)
        note_cards(image, [("Shape type", "Content_Summary")], (96, 600, 640, 900))
        note_cards(image, [("Binding name", "The alternate that matched")], (688, 600, 1232, 900))
        note_cards(image, [("Binding source", "Where the markup came from")], (1280, 600, W - 96, 900))
        return image

    if step_id == "debug-3":
        image = slide_base(chapter_title, label, "Choose the most specific name")
        bullets(image, SPECIFIC)
        return image

    if step_id == "move-1":
        image = slide_base(chapter_title, label, "The Templates recipe step")
        draw_code(image, (96, 240, W - 96, 690), "recipe.json", "json", RECIPE, size=25)
        note_cards(image, [("name", "The step is Templates.")], (96, 740, 640, 940))
        note_cards(image, [("Templates", "One entry for each name.")], (688, 740, 1232, 940))
        note_cards(image, [("Content", "Inline, or read from a file.")], (1280, 740, W - 96, 940))
        return image

    if step_id == "move-2":
        image = slide_base(chapter_title, label, "Deployment plans")
        bullets(image, EXPORT)
        return image

    raise SystemExit(f"No slide for {step_id}")
