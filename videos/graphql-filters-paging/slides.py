"""The slides of the "GraphQL: Filter, Sort and Page Content Items" video. assemble.py calls slide() for each step of
a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The C# comes
from src/OrchardCore/OrchardCore.ContentManagement.GraphQL/Queries/ContentItemsFieldType.cs, and the JSON from the
documentation of the GraphQL module, trimmed to what the narration talks about.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "QueryArguments", "QueryArgument", "ContentItemWhereInput", "ContentItemOrderByInput", "IntGraphType",
    "PublicationStatusGraphType", "PublicationStatusEnum", "PublicationStatusEnum", "IQuery", "ContentItem",
}

ARGUMENTS = """
Arguments = new QueryArguments(
    new QueryArgument<ContentItemWhereInput> { Name = "where" }, // ...
    new QueryArgument<ContentItemOrderByInput> { Name = "orderBy" }, // ...
    new QueryArgument<IntGraphType> { Name = "first" }, // ...
    new QueryArgument<IntGraphType> { Name = "skip" }, // ...
    new QueryArgument<PublicationStatusGraphType>
    {
        Name = "status",
        DefaultValue = PublicationStatusEnum.Published,
        // ...
    });
"""

RESOLVE = """
// ...
query = FilterVersion(query, context.GetArgument<PublicationStatusEnum>("status"));
query = FilterContentType(query, context);
query = OrderBy(query, context);

var contentItemsQuery = await FilterWhereArgumentsAsync(query, where, context, session);
contentItemsQuery = PageQuery(contentItemsQuery, context);
// ...
"""

SETTINGS = """
{
  "OrchardCore": {
    "Apis": {
      "GraphQL": {
        "DefaultNumberOfResults": 100,
        "MaxNumberOfResults": 1000,
        "MaxNumberOfResultsValidationMode": "Default"
      }
    }
  }
}
"""

DEFAULTS = [
    ("1", "status", "PUBLISHED: the published version of each item"),
    ("2", "orderBy", "createdUtc descending: the newest items first"),
    ("3", "first", "100 items, from the DefaultNumberOfResults setting"),
]

OPERATORS = [
    ("=", "Every field", "displayText, and displayText_not"),
    ("Aa", "Strings", "_contains, _starts_with, _ends_with and _in, each with a _not version"),
    ("ID", "Identifiers", "contentItemId_in and contentItemId_not_in"),
    ("<>", "Dates", "createdUtc_gt, _gte, _lt, _lte, _in and _not_in"),
]

COMBINATORS = [
    ("1", "AND", "Every condition holds. Keys side by side in the where argument work this way."),
    ("2", "OR", "At least one condition of the list holds: OR: [ ... ]"),
    ("3", "NOT", "Negates the condition it contains: NOT: [ ... ]"),
]

FIELDS = [
    ("1", "Enable the feature", "Content Fields Indexing (SQL) adds a database index for these fields"),
    ("2", "Supported fields", "BooleanField, DateField, DateTimeField, NumericField, TextField, TimeField"),
    ("3", "Suffixes of their type", "Text takes the string suffixes, numbers and dates take _gt and _lt"),
]

MODES = [
    ("1", "Enabled", "A first above the maximum fails validation with an error"),
    ("2", "Disabled", "The value is logged, and the query is not rejected"),
    ("3", "Default", "Enabled in the Development environment, Disabled in any other"),
]


def slide(step_id, chapter_title, label):
    if step_id == "args-1":
        image = slide_base(chapter_title, label, "Five arguments on every content type query")
        draw_code(image, (96, 240, 1150, 700), "ContentItemsFieldType.cs", "csharp", ARGUMENTS, types=TYPES, size=24)
        note_cards(image, [
            ("where and orderBy", "filter and sort the items"),
            ("first and skip", "take a page of them"),
            ("status", "picks the version of each"),
        ], (1200, 300, W - 96, 1030))
        return image

    if step_id == "args-2":
        image = slide_base(chapter_title, label, "What a query returns without arguments")
        bullets(image, DEFAULTS)
        return image

    if step_id == "args-3":
        image = slide_base(chapter_title, label, "One database query, with the page taken last")
        draw_code(image, (96, 240, W - 96, 660), "ContentItemsFieldType.cs", "csharp", RESOLVE, types=TYPES, size=28,
                  highlight={7})
        bullets(image, [("check", "The page comes last", "first and skip count within the filtered, sorted list")], top=720)
        return image

    if step_id == "ops-1":
        image = slide_base(chapter_title, label, "Filter operators by kind of field")
        bullets(image, OPERATORS)
        return image

    if step_id == "ops-2":
        image = slide_base(chapter_title, label, "Combine conditions")
        bullets(image, COMBINATORS)
        return image

    if step_id == "ops-3":
        image = slide_base(chapter_title, label, "Filter on the fields of your own parts")
        bullets(image, FIELDS)
        return image

    if step_id == "limits-1":
        image = slide_base(chapter_title, label, "Settings for the number of results")
        draw_code(image, (96, 240, 1150, 760), "GraphQL configuration section", "json", SETTINGS, types=TYPES, size=24)
        note_cards(image, [
            ("Default", "used when first is missing"),
            ("Maximum", "the largest first allowed"),
            ("Mode", "Default, Enabled or Disabled"),
        ], (1200, 300, W - 96, 1030))
        return image

    if step_id == "limits-2":
        image = slide_base(chapter_title, label, "The validation mode")
        bullets(image, MODES)
        return image

    raise SystemExit(f"No slide for {step_id}")
