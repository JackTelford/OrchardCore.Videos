"""The slides of the Placement video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the Orchard Core source (the Title module's placement.json, TextFieldDisplayDriver) and from the Placement
documentation page, trimmed to what the narration talks about.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, MUTED, PANEL, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"TextFieldDisplayDriver", "ContentFieldDisplayDriver", "TextField", "IDisplayResult", "BuildFieldDisplayContext",
         "DisplayTextFieldViewModel", "OrchardCoreConstants", "MyPartDisplayDriver", "ContentPartDisplayDriver", "MyPart",
         "BuildPartEditorContext", "MyPartViewModel"}

# The default location of a shape, set by its display driver (OrchardCore.ContentFields, TextFieldDisplayDriver).
DRIVER = """
public sealed class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
{
    // ...

    public override IDisplayResult Display(TextField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayTextFieldViewModel>(GetDisplayShapeType(context), model =>
        {
            // ...
        })
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
        .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
    }
}
"""

OVERRIDE = [
    ("1", "Move", "To another zone, or to another position in the zone."),
    ("2", "Hide", "The shape is left out of the page."),
    ("3", "Group", "Into tabs, cards and columns, in the editors."),
]

FINDERS = [
    ("1", "Shape type", "The property name of the rule, such as TextField or ContentPart."),
    ("2", "Differentiator", "Tells the shapes of one type apart, such as Article-Subtitle."),
    ("3", "Display type", "Detail, Summary, DetailAdmin, SummaryAdmin, or Edit in the editors."),
    ("4", "Content item and request", "The contentType, contentPart and path filters."),
]

# OrchardCore.Title/placement.json.
FILE = """
{
  "TitlePart": [
    {
      "place": "Header:5",
      "displayType": "DetailAdmin",
      "contentType": "DashboardWidget"
    }
  ]
}
"""

FILE_NOTES = [
    ("Shape types", "are the property names."),
    ("Rules", "are the items of each array."),
    ("Location", "the root of a module, theme or startup project."),
]

RULE = """
{
  "TextField": [
    {
      "displayType": "Detail",
      "differentiator": "Article-MyTextField",
      "contentType": [ "Page", "BlogPost" ],
      "contentPart": [ "HtmlBodyPart" ],
      "path": [ "/mypage" ],
      "place": "Content",
      "alternates": [ "TextField_Title" ],
      "wrappers": [ "TextField_Title" ],
      "shape": "AnotherShape"
    }
  ]
}
"""

RULE_NOTES = [
    ("Filters", "displayType to path"),
    ("Placement information", "place to shape"),
]

FILTERS = [
    ("1", "displayType, differentiator", "Must equal the display type and the differentiator of the shape."),
    ("2", "contentType", "A content type or a stereotype. Art* matches every name starting with Art."),
    ("3", "contentPart", "Matches the content items that have a part with that name."),
    ("4", "path", "The request path. /blog/* matches every path starting with /blog/."),
]

INFORMATION = [
    ("1", "place", "The location of the shape. A dash hides it."),
    ("2", "alternates", "Adds alternate shape types to the shape."),
    ("3", "wrappers", "Adds wrapper shapes around the shape."),
    ("4", "shape", "Replaces the shape type with the one you name."),
]

PARTS = """
{
  "TitlePart": [
    { "differentiator": "TitlePart", "place": "-" }
  ],
  "BagPart": [
    { "differentiator": "Services", "place": "-" }
  ]
}
"""

PARTS_NOTES = [
    ("TitlePart", "A part with its own driver."),
    ("Services", "The name given to a BagPart."),
]

CONTENT_PART = """
{
  "ContentPart": [
    { "differentiator": "GalleryPart", "place": "MyGalleryZone" }
  ],
  "ContentPart_Summary": [
    { "differentiator": "GalleryPart", "place": "MyGalleryZone" }
  ]
}
"""

FIELDS = """
{
  "TextField": [
    { "differentiator": "Article-Subtitle", "place": "Content:2" },
    { "differentiator": "Address-City", "place": "Content:3" }
  ]
}
"""

FIELDS_NOTES = [
    ("Article-Subtitle", "The Subtitle field of the Article content type."),
    ("Address-City", "The City field of a part named Address."),
]

EDITORS = """
{
  "ContentPart_Edit": [
    { "differentiator": "LandingPage-Services", "place": "-" }
  ]
}
"""

EDITORS_NOTES = [
    ("TextField_Edit", "A field editor, with the differentiator of the field."),
    ("ContentPart_Edit", "The whole row of a part: label, description and editor."),
]

DISPLAY_MODE = """
{
  "TextField_Display": [
    {
      "place": "Content:1",
      "differentiator": "Blog-MyField-TextField_Display__Header"
    }
  ]
}
"""

DISPLAY_MODE_NOTES = [
    ("TextField_Display", "The shape type, without the display mode."),
    ("Blog-MyField-TextField_Display__Header", "The part, the field and the full shape type."),
]

ZONES = [
    (":", "Content:5", "The Content zone, at position 5."),
    (".", "Parts.Article:1", "Inside a zone: the Article part, in the editor."),
    ("/", "/Content:5", "A zone of the layout."),
    ("-", "-", "Hides the shape."),
]

POSITION_CHIPS = [("start", 200), ("before", 220), ("0   1   1.5   2   10", 560), ("after", 220), ("end", 200)]
POSITION_NOTES = [
    "Positions compare as numbers: 2 comes before 10.",
    "A dot makes sub-positions: 1.5 fits between 1 and 2.",
    "No position counts as 0, or as the part's position in an editor.",
]

GROUPS = [
    ("#", "Tab", "Parts:0#Media;0 puts the shape in a tab named Media."),
    ("%", "Card", "Parts:0%Media;0 puts the shape in a card named Media."),
    ("|", "Column", "Parts:0|Media_9;0 puts the shape in a column, 9 of 12 wide."),
    (";", "Group position", "The number after the semicolon orders the groups."),
]

TABS = """
{
  "MediaField_Edit": [
    {
      "place": "Parts:0#Media;0",
      "contentType": [ "Article" ]
    }
  ],
  "HtmlField_Edit": [
    {
      "place": "Parts:0#Content;1",
      "contentType": [ "Article" ]
    }
  ]
}
"""

TABS_NOTES = [
    ("Media tab", "group position 0: first."),
    ("Content tab", "group position 1: second."),
    ("No group", "joins the Content tab."),
]

# The Location overloads of ShapeResult, and the PlacementLocationBuilder, in the Placement documentation page.
FLUENT = """
public sealed class MyPartDisplayDriver : ContentPartDisplayDriver<MyPart>
{
    public override IDisplayResult Edit(MyPart part, BuildPartEditorContext context)
    {
        return Initialize<MyPartViewModel>("MyPart_Edit", model =>
            {
                // ...
            })
            // The same place as a string: "Parameters:5#Settings;1%Details;2|Left_9;3"
            .Location(l => l
                .Zone("Parameters", "5")
                .Tab("Settings", "1")
                .Card("Details", "2")
                .Column("Left", "3", "9"));
    }
}
"""

SITE_FILE = """
{
  "TextField_Edit": [
    {
      "contentType": "Article",
      "differentiator": "Article-Subtitle",
      "place": "Parts:0.5"
    }
  ],
  "MediaField_Edit": [
    {
      "contentType": "Article",
      "differentiator": "Article-Image",
      "place": "Parts:4#Media;1"
    }
  ]
}
"""

SITE_NOTES = [
    ("Subtitle", "Parts zone, position 0.5."),
    ("Banner Image", "A tab named Media."),
]

PRECEDENCE = [
    ("1", "The startup project", "Wins over the active theme and the modules."),
    ("2", "The active theme", "The front end theme, or the admin theme in the admin."),
    ("3", "The modules", "A module that depends on another wins over it."),
]

CHECKS = [
    ("1", "The last matching rule wins", "Inside one file, a later rule overrides an earlier one."),
    ("2", "Unknown properties are ignored", "A misspelled filter is dropped, so the rule matches more shapes."),
    ("3", "Check the names first", "The shape type, the differentiator and the display type."),
]


def lines_with(code, text):
    """The 1-based numbers of the lines of a code panel that contain text."""
    return {number for number, line in enumerate(code.strip("\n").split("\n"), start=1) if text in line}


def slide(step_id, chapter_title, label):
    if step_id == "places-1":
        image = slide_base(chapter_title, label, "A driver sets the default location")
        draw_code(image, (96, 240, W - 96, 860), "TextFieldDisplayDriver.cs", "csharp", DRIVER, types=TYPES, size=25,
                  highlight=lines_with(DRIVER, ".Location("))
        return image

    if step_id == "places-2":
        image = slide_base(chapter_title, label, "A placement file overrides it")
        bullets(image, OVERRIDE)
        return image

    if step_id == "places-3":
        image = slide_base(chapter_title, label, "How a rule finds a shape")
        bullets(image, FINDERS)
        return image

    if step_id == "file-1":
        image = slide_base(chapter_title, label, "A placement.json file")
        draw_code(image, (96, 240, 980, 760), "OrchardCore.Title/placement.json", "json", FILE, size=26)
        note_cards(image, FILE_NOTES, (1020, 240, W - 96, 1030))
        return image

    if step_id == "file-2":
        image = slide_base(chapter_title, label, "A rule: filters and placement information")
        draw_code(image, (96, 240, 1160, 900), "placement.json", "json", RULE, size=24)
        note_cards(image, RULE_NOTES, (1200, 240, W - 96, 1030))
        return image

    if step_id in ("file-3", "file-4"):
        image = slide_base(chapter_title, label, "The filters")
        bullets(image, FILTERS, active={0, 1} if step_id == "file-3" else {2, 3})
        return image

    if step_id == "file-5":
        image = slide_base(chapter_title, label, "Placement information")
        bullets(image, INFORMATION)
        return image

    if step_id == "target-1":
        image = slide_base(chapter_title, label, "Targeting a part")
        draw_code(image, (96, 240, 1130, 700), "placement.json", "json", PARTS, size=26)
        note_cards(image, PARTS_NOTES, (1170, 240, W - 96, 1030))
        return image

    if step_id == "target-2":
        image = slide_base(chapter_title, label, "A dynamic part")
        draw_code(image, (96, 240, W - 96, 700), "placement.json", "json", CONTENT_PART, size=26)
        return image

    if step_id == "target-3":
        image = slide_base(chapter_title, label, "Targeting a field")
        draw_code(image, (96, 240, W - 96, 540), "placement.json", "json", FIELDS, size=26)
        note_cards(image, FIELDS_NOTES, (96, 570, W - 96, 1030))
        return image

    if step_id == "target-4":
        image = slide_base(chapter_title, label, "Targeting an editor")
        draw_code(image, (96, 240, W - 96, 540), "placement.json", "json", EDITORS, size=26)
        note_cards(image, EDITORS_NOTES, (96, 570, W - 96, 1030))
        return image

    if step_id == "target-5":
        image = slide_base(chapter_title, label, "A field with a display mode")
        draw_code(image, (96, 240, W - 96, 600), "placement.json", "json", DISPLAY_MODE, size=24)
        note_cards(image, DISPLAY_MODE_NOTES, (96, 630, W - 96, 1030))
        return image

    if step_id == "place-1":
        image = slide_base(chapter_title, label, "The place: a zone and a position")
        bullets(image, ZONES)
        return image

    if step_id == "place-2":
        image = slide_base(chapter_title, label, "How positions sort")
        draw = ImageDraw.Draw(image)
        x = 96
        gap = (W - 2 * 96 - sum(width for _, width in POSITION_CHIPS)) // (len(POSITION_CHIPS) - 1)
        for index, (text, width) in enumerate(POSITION_CHIPS):
            draw.rounded_rectangle((x, 330, x + width, 450), radius=16, fill=PANEL, outline=ACCENT, width=2)
            draw.text((x + width // 2, 390), text, font=font(38, "Bold"), fill=TEXT, anchor="mm")
            x += width
            if index < len(POSITION_CHIPS) - 1:
                draw.text((x + gap // 2, 390), "<", font=font(44, "Bold"), fill=MUTED, anchor="mm")
                x += gap
        y = 600
        for note in POSITION_NOTES:
            draw.ellipse((100, y - 7, 114, y + 7), fill=ACCENT)
            draw.text((136, y), note, font=font(34), fill=TEXT, anchor="lm")
            y += 76
        return image

    if step_id == "place-3":
        image = slide_base(chapter_title, label, "Groups in the editor")
        bullets(image, GROUPS)
        return image

    if step_id == "place-4":
        image = slide_base(chapter_title, label, "Tabs in a placement file")
        draw_code(image, (96, 240, 1180, 900), "placement.json", "json", TABS, size=25)
        note_cards(image, TABS_NOTES, (1220, 240, W - 96, 1030))
        return image

    if step_id == "place-5":
        image = slide_base(chapter_title, label, "The same place, in code")
        draw_code(image, (96, 240, W - 96, 1030), "MyPartDisplayDriver.cs", "csharp", FLUENT, types=TYPES, size=26)
        return image

    if step_id == "rules-1":
        image = slide_base(chapter_title, label, "placement.json in the startup project")
        draw_code(image, (96, 240, 1180, 900), "placement.json", "json", SITE_FILE, size=25)
        note_cards(image, SITE_NOTES, (1220, 240, W - 96, 1030))
        return image

    if step_id == "wins-1":
        image = slide_base(chapter_title, label, "Which placement file wins")
        bullets(image, PRECEDENCE)
        return image

    if step_id == "wins-2":
        image = slide_base(chapter_title, label, "When a rule does not behave")
        bullets(image, CHECKS)
        return image

    raise SystemExit(f"No slide for {step_id}")
