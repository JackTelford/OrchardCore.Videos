"""The slides of the Shortcode Templates video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The Liquid
templates are the examples of the documentation of the Shortcodes module (src/docs/reference/modules/Shortcodes), with
the filter name the code registers: sanitize_html.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

# Names colored as types in the code panels.
TYPES = {"Orchard", "ShortcodesToHtmlAsync", "AddShortcode"}

BODY = """
<p>[primary text="Read this first."]</p>
<p>[primary]Read this first.[/primary]</p>
"""

PAGE = """
<p><span class="text-primary">Read this first.</span></p>
<p><span class="text-primary">Read this first.</span></p>
"""

# The 'primary' example of the documentation.
PRIMARY = """
{% capture output %}
{% if Args.Text != nil %}
<span class="text-primary">{{ Args.Text }}</span>
{% else %}
<span class="text-primary">{{ Content }}</span>
{% endif %}
{% endcapture %}
{{ output | sanitize_html | raw }}
"""

# The 'display_text' example of the documentation.
DISPLAY_TEXT = """
{{ Context.ContentItem.DisplayText }}
"""

DISPLAY_TEXT_BODY = """
<p>You are reading [display_text].</p>
"""

LIQUID_FILTER = """
{{ Model.ContentItem.Content.RawHtml.Content.Html | shortcode | raw }}
"""

RAZOR_HELPER = """
@Html.Raw(await Orchard.ShortcodesToHtmlAsync((string)Model.ContentItem.Content.RawHtml.Content.Html))
"""

# The ShortcodeTemplates recipe step: the keys are the names of the templates.
RECIPE = """
{
  "steps": [
    {
      "name": "ShortcodeTemplates",
      "ShortcodeTemplates": {
        "display_text": {
          "Hint": "Returns the display text of the content item.",
          "Usage": "[display_text]",
          "Categories": [ "HTML Content" ],
          "Content": "{{ Context.ContentItem.DisplayText }}"
        }
      }
    }
  ]
}
"""

KINDS = [
    ("1", "In code", "A module registers it, for example with AddShortcode."),
    ("2", "As a template", "Liquid that you write in the admin and save with the site."),
    ("check", "One list for editors", "Both kinds appear in the picker of the editor."),
]

VALUES = [
    ("1", "Args", "The named arguments, such as Args.Text."),
    ("2", "Content", "The text between the opening and the closing tag."),
    ("3", "Context", "What the caller passes along, such as Context.ContentItem."),
]

RULES = [
    ("1", "Same name, same shortcode", "A template named image replaces the image shortcode from code."),
    ("2", "Sanitize Html", "An Html Body part cleans the HTML after the shortcodes run."),
]

PERMISSION = [
    ("1", "Manage shortcode templates", "The permission behind Design, Shortcodes."),
    ("2", "Security critical", "Assign it with caution."),
    ("3", "Default roles", "Administrator and Editor."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "A shortcode in the body of an item")
        draw_code(image, (96, 240, 1150, 500), "In the body", "html", BODY, types=TYPES, size=26)
        draw_code(image, (96, 540, 1150, 800), "On the page", "html", PAGE, types=TYPES, size=26)
        note_cards(image, [
            ("Name", "primary"),
            ("Argument", "text"),
            ("Content", "The text between the tags."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Two kinds of shortcodes")
        bullets(image, KINDS)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "What a template receives")
        bullets(image, VALUES)
        return image

    if step_id == "template-1":
        image = slide_base(chapter_title, label, "Args.Text, or Content")
        draw_code(image, (96, 240, 1150, 760), "Content", "html", PRIMARY, types=TYPES, highlight={2, 3, 4, 5, 6}, size=26)
        note_cards(image, [
            ("Args.Text", "The text argument."),
            ("Content", "The text between the tags."),
            ("!= nil", "Tests that the argument exists."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "template-2":
        image = slide_base(chapter_title, label, "Encode, clean, then write the HTML")
        draw_code(image, (96, 240, 1150, 760), "Content", "html", PRIMARY, types=TYPES, highlight={1, 7, 8}, size=26)
        note_cards(image, [
            ("{{ }}", "Encodes its output as HTML."),
            ("sanitize_html", "Removes unsafe markup."),
            ("raw", "Writes the HTML as it is."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "Reading the content item")
        draw_code(image, (96, 240, 1150, 420), "Content of display_text", "html", DISPLAY_TEXT, types=TYPES, size=26)
        draw_code(image, (96, 460, 1150, 640), "In the body", "html", DISPLAY_TEXT_BODY, types=TYPES, size=26)
        note_cards(image, [
            ("Context.ContentItem", "The item being displayed."),
            ("DisplayText", "The title of the item."),
            ("Set by", "Body parts and fields."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "Names and sanitizing")
        bullets(image, RULES)
        return image

    if step_id == "more-3":
        image = slide_base(chapter_title, label, "Rendering shortcodes in your own views")
        draw_code(image, (96, 240, W - 96, 460), "Liquid", "html", LIQUID_FILTER, types=TYPES, size=28)
        draw_code(image, (96, 520, W - 96, 740), "Razor", "cshtml", RAZOR_HELPER, types=TYPES, size=26)
        return image

    if step_id == "deploy-1":
        image = slide_base(chapter_title, label, "The ShortcodeTemplates recipe step")
        draw_code(image, (96, 240, 1150, 900), "recipe.json", "json", RECIPE, types=TYPES, size=25)
        note_cards(image, [
            ("Recipe step", "ShortcodeTemplates"),
            ("Keyed by name", "One entry per template."),
            ("Deployment step", "All Shortcode Templates."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "deploy-2":
        image = slide_base(chapter_title, label, "Who can manage templates")
        bullets(image, PERMISSION)
        return image

    raise SystemExit(f"No slide for {step_id}")
