"""The slides of the SQL Queries video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"MapIndex", "DateTime"}

# OrchardCore.ContentManagement/Records/ContentItemIndex.cs, without its constants.
INDEX_CLASS = """
public class ContentItemIndex : MapIndex
{
    // ...
    public long DocumentId { get; set; }
    public string ContentItemId { get; set; }
    public string ContentItemVersionId { get; set; }
    public bool Published { get; set; }
    public bool Latest { get; set; }
    public string ContentType { get; set; }
    public DateTime? ModifiedUtc { get; set; }
    public DateTime? PublishedUtc { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public string Owner { get; set; }
    public string Author { get; set; }
    public string DisplayText { get; set; }
}
"""

# The Queries step of TheBlogTheme/Recipes/blog.recipe.json, without the Schema of the entry.
RECIPE_STEP = """
{
  "name": "queries",
  "Queries": [
    {
      "Source": "Sql",
      "Name": "RecentBlogPosts",
      "Template": "SELECT DocumentId FROM ContentItemIndex WHERE ContentType='BlogPost' ORDER BY CreatedUtc DESC LIMIT @limit:3",
      // ...
      "ReturnContentItems": true
    }
  ]
}
"""

STATEMENT = [
    ("1", "Filter rows", "A where clause on the type, the published flag, the dates, the owner, or the author."),
    ("2", "Sort and page", "Order by a column, then limit the rows and skip the first ones with offset."),
    ("3", "Choose columns", "Return the values you need, or only DocumentId to get content items."),
]

RESULTS = [
    ("1", "Rows of values", "Each row is an object with the columns you selected."),
    ("2", "Content items", "Return Documents loads the items for the DocumentId values."),
]

CONVERSIONS = [
    ("1", "LIMIT and OFFSET", "Written once, rendered in the form each database expects."),
    ("2", "ORDER BY random()", "Uses the random order of the database in use."),
    ("3", "Date functions", "second, minute, hour, day, month, year, and now() in UTC."),
]

RECIPE_RULES = [
    ("check", "The name exists", "The query with that name is updated."),
    ("check", "The name is free", "A query is created from the Source. A source that no feature provides is an error."),
    ("check", "No name, or no source", "The entry is reported as an error and skipped."),
]

PERMISSIONS = [
    ("1", "Manage SQL Queries", "Opens Run SQL Query. Marked Security Critical."),
    ("2", "Administrator and Editor", "Include it in their default permissions."),
    ("3", "Manage queries", "Creates, edits, and deletes saved queries."),
]

CARE = [
    ("1", "Direct table access", "The statement reads the tables of the site."),
    ("2", "No content permissions", "They are not applied to the rows it returns."),
    ("3", "Manage queries too", "It also saves SQL queries and runs them through the API."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "The ContentItemIndex table")
        draw_code(image, (96, 240, 1130, 1030), "ContentItemIndex.cs", "csharp", INDEX_CLASS, types=TYPES, size=26)
        note_cards(image, [
            ("One row", "for each version of an item."),
            ("Table name", "ContentItemIndex"),
            ("Columns", "have the property names."),
        ], (1170, 300, W - 96, 1030))
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "One SELECT statement")
        bullets(image, STATEMENT)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Two kinds of result")
        bullets(image, RESULTS)
        return image

    if step_id == "convert-1":
        image = slide_base(chapter_title, label, "What the SQL parser converts")
        bullets(image, CONVERSIONS)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "The Queries recipe step")
        draw_code(image, (96, 240, W - 96, 760), "recipe step", "json", RECIPE_STEP, size=22)
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "What the step does with each entry")
        bullets(image, RECIPE_RULES)
        return image

    if step_id == "access-1":
        image = slide_base(chapter_title, label, "Who can run SQL")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "access-2":
        image = slide_base(chapter_title, label, "Grant it with care")
        bullets(image, CARE)
        return image

    raise SystemExit(f"No slide for {step_id}")
