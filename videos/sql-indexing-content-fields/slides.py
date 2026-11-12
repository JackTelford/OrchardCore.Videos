"""The slides of the Content Fields Indexing video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import (ACCENT, BORDER, GREEN, MARGIN, MUTED, PANEL, TEXT, W, bullets, draw_code, font, note_cards,
                   slide_base)

TYPES = {"ContentItem", "ContentItemIndex", "ContentFieldIndex", "MapIndex", "TextFieldIndex", "NumericFieldIndex",
         "MultiTextField", "MultiTextFieldIndex", "TextHelper", "DateTime", "ISession", "EventQueries", "IEnumerable",
         "Task"}

WHY_PROBLEM = [
    ("1", "The fields are inside the document", "Each version of a content item is a document, with its fields in it."),
    ("2", "ContentItemIndex has no field values", "It has the type, the dates, the owner and the display text."),
]

WHY_SOLUTION = [
    ("check", "A table for each indexed field type", "Text, Numeric, Boolean, Date, Link, Html and others."),
    ("check", "A row for each field value", "Written when a content item is saved."),
    ("check", "Two features", "Content Fields Indexing (SQL), and its User Picker feature."),
    ("check", "Three ways to query it", "SQL queries, C# with ISession, and GraphQL filters."),
]

VERSIONS = [
    ("1", "Published", "The version that visitors see."),
    ("2", "Latest", "The newest saved version, which can be a draft."),
    ("3", "Neither", "An older version has no rows."),
]

GRAPHQL_FACTS = [
    ("check", "Two features", "GraphQL, and Content Fields Indexing (SQL)."),
    ("check", "Fields with a filter", "Boolean, Date, DateTime, Numeric, Text and Time."),
    ("check", "Suffixes refine a filter", "_gt, _gte, _lt, _lte, _contains, _starts_with and _ends_with."),
]

# Each table with the columns that hold its value.
TABLES_LEFT = [
    ("TextFieldIndex", "Text, BigText"),
    ("NumericFieldIndex", "Numeric"),
    ("BooleanFieldIndex", "Boolean"),
    ("DateFieldIndex", "Date"),
    ("DateTimeFieldIndex", "DateTime"),
    ("TimeFieldIndex", "Time"),
]

TABLES_RIGHT = [
    ("LinkFieldIndex", "Url, BigUrl, Text, BigText"),
    ("HtmlFieldIndex", "Html"),
    ("MultiTextFieldIndex", "Value, BigValue"),
    ("ContentPickerFieldIndex", "SelectedContentItemId"),
    ("UserPickerFieldIndex", "SelectedUserId"),
]

CONTENT_ITEM_INDEX = """
public class ContentItemIndex : MapIndex
{
    // ...
    public bool Published { get; set; }
    public bool Latest { get; set; }
    public string ContentType { get; set; }
    public DateTime? ModifiedUtc { get; set; }
    // ...
    public string Owner { get; set; }
    // ...
    public string DisplayText { get; set; }
}
"""

CONTENT_FIELD_INDEX = """
public abstract class ContentFieldIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string ContentItemVersionId { get; set; }
    public string ContentType { get; set; }
    public string ContentPart { get; set; }
    public string ContentField { get; set; }
    public bool Published { get; set; }
    public bool Latest { get; set; }
}
"""

TEXT_FIELD_INDEX = """
public class TextFieldIndex : ContentFieldIndex
{
    public const int MaxTextSize = 766;

    public string Text { get; set; }
    public string BigText { get; set; }
}
"""

MULTI_TEXT_PROVIDER = """
return fieldDefinitions
    .GetContentFields<MultiTextField>(contentItem)
    .SelectMany(pair => pair.Field.Values
        .Select(value => (pair.Definition, Value: value)))
    .Select(pair => new MultiTextFieldIndex
    {
        // ...
        Value = TextHelper.Truncate(pair.Value, MultiTextFieldIndex.MaxValueSize),
        BigValue = pair.Value,
    });
"""

SOFT_DELETE = """
// Remove index records of soft deleted items.
if (!contentItem.Published && !contentItem.Latest)
{
    return null;
}
"""

SESSION_QUERY = """
using OrchardCore.ContentFields.Indexing.SQL;
using OrchardCore.ContentManagement;
using YesSql;

public class EventQueries(ISession session)
{
    public async Task<IEnumerable<ContentItem>> GetSmallEventsAsync(decimal maxCapacity) =>
        await session
            .Query<ContentItem, NumericFieldIndex>(index =>
                index.ContentType == "Event" &&
                index.ContentField == "Capacity" &&
                index.Published &&
                index.Numeric < maxCapacity)
            .ListAsync();
}
"""

RAZOR = """
@using OrchardCore.ContentFields.Indexing.SQL
@using OrchardCore.ContentManagement
@using YesSql
@inject ISession Session

@{
    var events = await Session
        .Query<ContentItem, TextFieldIndex>(index =>
            index.ContentType == "Event" && index.ContentField == "Venue")
        .ListAsync();
}
"""

LIQUID = """
{% assign venues = Queries.EventVenues | query %}
{% for venue in venues %}
  {{ venue.Text }}
{% endfor %}
"""

GRAPHQL = """
product(where: {price: {amount_gt: 10}}) {
    contentItemId
    displayText
    price {
        amount
    }
}
"""


def table_card(draw, box, name, columns, accent=False):
    """A rounded card with a table name on the left and the columns of its value on the right."""
    x0, y0, x1, y1 = box
    draw.rounded_rectangle(box, radius=14, fill=PANEL, outline=ACCENT if accent else BORDER, width=2)
    middle = (y0 + y1) // 2
    draw.text((x0 + 28, middle), name, font=font(30, "Bold"), fill=TEXT, anchor="lm")
    draw.text((x1 - 28, middle), columns, font=font(28), fill=GREEN, anchor="rm")


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Field values have no columns")
        bullets(image, WHY_PROBLEM, top=240)
        draw_code(image, (MARGIN, 580, W - MARGIN, 1030), "ContentItemIndex.cs", "csharp", CONTENT_ITEM_INDEX,
                  types=TYPES, size=22)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Content Fields Indexing")
        bullets(image, WHY_SOLUTION)
        return image

    if step_id == "tables-1":
        image = slide_base(chapter_title, label, "The columns every field table shares")
        draw_code(image, (MARGIN, 240, 1150, 740), "ContentFieldIndex.cs", "csharp", CONTENT_FIELD_INDEX,
                  types=TYPES, size=28)
        note_cards(image, [
            ("Content Part", "The part's name on the type."),
            ("Published", "The version that visitors see."),
            ("Latest", "The newest saved version."),
        ], (1200, 240, W - MARGIN, 1030))
        draw = ImageDraw.Draw(image)
        draw.text((MARGIN, 800), "Each row also has an Id, and a DocumentId:", font=font(30), fill=TEXT, anchor="lm")
        draw.text((MARGIN, 850), "the key of the document that the row indexes.", font=font(30), fill=MUTED, anchor="lm")
        return image

    if step_id == "tables-2":
        image = slide_base(chapter_title, label, "A table and value columns for each field type")
        draw = ImageDraw.Draw(image)
        column_width = (W - 2 * MARGIN - 48) // 2
        for column, rows in enumerate((TABLES_LEFT, TABLES_RIGHT)):
            x0 = MARGIN + column * (column_width + 48)
            for index, (name, columns) in enumerate(rows):
                y0 = 240 + index * 112
                table_card(draw, (x0, y0, x0 + column_width, y0 + 96), name, columns,
                           accent=name == "UserPickerFieldIndex")
        draw.text((MARGIN + column_width + 48, 240 + 5 * 112 + 48),
                  "Created by the User Picker feature.", font=font(26), fill=MUTED, anchor="lm")
        return image

    if step_id == "tables-3":
        image = slide_base(chapter_title, label, "One row for each value, and a column for long text")
        draw_code(image, (MARGIN, 240, W - MARGIN, 570), "TextFieldIndex.cs", "csharp", TEXT_FIELD_INDEX,
                  types=TYPES, size=24)
        draw_code(image, (MARGIN, 600, W - MARGIN, 1030), "MultiTextFieldIndexProvider.cs", "csharp",
                  MULTI_TEXT_PROVIDER, types=TYPES, size=24)
        return image

    if step_id == "tables-4":
        image = slide_base(chapter_title, label, "Which versions have rows")
        bullets(image, VERSIONS)
        draw_code(image, (MARGIN, 780, W - MARGIN, 1030), "TextFieldIndexProvider.cs", "csharp", SOFT_DELETE,
                  types=TYPES, size=24)
        return image

    if step_id == "code-1":
        image = slide_base(chapter_title, label, "Query an index class with ISession")
        draw_code(image, (MARGIN, 240, W - MARGIN, 1030), "EventQueries.cs", "csharp", SESSION_QUERY,
                  types=TYPES, size=26)
        return image

    if step_id == "code-2":
        image = slide_base(chapter_title, label, "The same tables in Razor and Liquid")
        draw_code(image, (MARGIN, 240, W - MARGIN, 700), "Content-Event.cshtml", "cshtml", RAZOR, types=TYPES, size=24)
        draw_code(image, (MARGIN, 730, W - MARGIN, 1030), "Liquid, with the saved SQL query EventVenues", "html",
                  LIQUID, types=TYPES, size=24)
        return image

    if step_id == "graphql-1":
        image = slide_base(chapter_title, label, "Filters from the field tables")
        bullets(image, GRAPHQL_FACTS)
        return image

    if step_id == "graphql-2":
        image = slide_base(chapter_title, label, "Filter on a numeric field")
        draw_code(image, (MARGIN, 240, 1150, 760), "GraphQL query", "javascript", GRAPHQL, types=TYPES, size=28)
        note_cards(image, [
            ("price", "The Price Part of the type."),
            ("amount", "The Amount field."),
            ("_gt", "Is greater than."),
        ], (1200, 240, W - MARGIN, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
