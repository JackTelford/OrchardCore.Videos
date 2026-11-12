"""The slides of the "Queries in Liquid, Razor and the API" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"IOrchardHelper", "ContentItem", "Dictionary"}

LIQUID_LOOP = """
{% assign recentBlogPosts = Queries.RecentBlogPosts | query %}
{% for item in recentBlogPosts %}
{{ item | display_text }}
{% endfor %}
"""

LIQUID_PARAMETERS = """
{% assign posts = Queries.RecentBlogPosts | query: limit: 5 %}
{% for item in posts %}
{{ item | display_text }}
{% endfor %}
"""

RAZOR_LOOP = """
@foreach (var contentItem in await Orchard.ContentQueryAsync("RecentBlogPosts"))
{
    @await Orchard.DisplayAsync(contentItem, "Summary")
}
"""

RAZOR_PARAMETERS = """
@{
    var parameters = new Dictionary<string, object> { ["limit"] = 5 };
}
@foreach (var contentItem in await Orchard.ContentQueryAsync("RecentBlogPosts", parameters))
{
    @await Orchard.DisplayAsync(contentItem, "Summary")
}
"""

RAZOR_INJECT = """
@inject OrchardCore.IOrchardHelper Orchard
"""

REQUESTS = """
# GET, with the default parameters
curl https://example.com/api/queries/RecentBlogPosts

# GET, with the parameters in the query string
curl "https://example.com/api/queries/RecentBlogPosts?parameters=%7B%22limit%22%3A2%7D"

# POST, with the parameters in the body
curl -X POST https://example.com/api/queries/RecentBlogPosts \\
  -H "Content-Type: application/json" \\
  -d '{ "limit": 2 }'
"""

UNSAFE_SQL = """
SELECT DocumentId
FROM UserIndex
WHERE UserName LIKE '%{{ input }}%'
"""

SAFE_SQL = """
SELECT DocumentId
FROM UserIndex
WHERE UserName LIKE @pattern
"""

BEARER_REQUEST = """
curl https://example.com/api/queries/RecentBlogPosts \\
  -H "Authorization: Bearer <access token>"
"""

SQL_RESULT = """
{
  "items": [ ... ]
}
"""

LUCENE_RESULT = """
{
  "items": [ ... ],
  "count": 1231
}
"""

ITEMS = [
    ("1", "The query returns content items", "Use their properties and filters, such as display_text."),
    ("2", "The query returns rows", "Each item has the columns of the row as its properties."),
]

MISSING = [
    ("1", "No query has that name", "Queries.Name is nil, and so is the result of the query filter."),
    ("2", "Every Liquid template", "With the Queries feature enabled, the Queries property is a global value."),
]

METHODS = [
    ("1", "ContentQueryAsync", "Content items. Results without a content item ID are skipped."),
    ("2", "QueryAsync", "The items as the query returns them, content items or rows. null for an unknown name."),
]

PERMISSIONS = [
    ("1", "Execute Api - All queries", "Runs every query."),
    ("2", "Execute Api - RecentBlogPosts", "Runs one query, named after it."),
    ("3", "Manage queries", "Includes both."),
    ("!", "Without the permission: Not Found", "The endpoint does not answer Unauthorized."),
]

CALLERS = [
    ("1", "Liquid", "Queries.RecentBlogPosts | query: limit: 5"),
    ("2", "Razor", "Orchard.ContentQueryAsync(\"RecentBlogPosts\", parameters)"),
    ("3", "Web API", "GET or POST /api/queries/RecentBlogPosts"),
]


def slide(step_id, chapter_title, label):
    if step_id == "liquid-1":
        image = slide_base(chapter_title, label, "Run a query with the query filter")
        draw_code(image, (96, 240, 1216, 640), "Content-BlogPost.liquid", "html", LIQUID_LOOP, types=TYPES, size=28)
        note_cards(image, [
            ("Queries.Name", "the saved query, by name"),
            ("query filter", "runs it, returns the items"),
        ], (1256, 260, W - 96, 700))
        return image

    if step_id == "liquid-2":
        image = slide_base(chapter_title, label, "Parameters are named arguments")
        draw_code(image, (96, 240, 1216, 640), "Any Liquid template", "html", LIQUID_PARAMETERS, types=TYPES, size=28)
        note_cards(image, [
            ("limit: 5", "replaces the default of 3"),
            ("Names", "are case-sensitive"),
        ], (1256, 260, W - 96, 700))
        return image

    if step_id == "liquid-3":
        image = slide_base(chapter_title, label, "What each item is")
        bullets(image, ITEMS)
        return image

    if step_id == "liquid-4":
        image = slide_base(chapter_title, label, "An unknown name, and where it works")
        bullets(image, MISSING)
        return image

    if step_id == "razor-1":
        image = slide_base(chapter_title, label, "ContentQueryAsync")
        draw_code(image, (96, 240, W - 96, 560), "A Razor view", "cshtml", RAZOR_LOOP, types=TYPES, size=28)
        note_cards(image, [
            ("ContentQueryAsync", "returns the content items of the query."),
            ("DisplayAsync", "renders each content item, here with the Summary display type."),
        ], (96, 600, W - 96, 1030))
        return image

    if step_id == "razor-2":
        image = slide_base(chapter_title, label, "Pass parameters in a dictionary")
        draw_code(image, (96, 240, W - 96, 700), "A Razor view", "cshtml", RAZOR_PARAMETERS, types=TYPES, size=28)
        return image

    if step_id == "razor-3":
        image = slide_base(chapter_title, label, "ContentQueryAsync or QueryAsync")
        bullets(image, METHODS)
        return image

    if step_id == "razor-4":
        image = slide_base(chapter_title, label, "Where the Orchard helper comes from")
        draw_code(image, (96, 240, W - 96, 380), "Any other view", "cshtml", RAZOR_INJECT, types=TYPES, size=28)
        note_cards(image, [
            ("Orchard Core Razor base class", "has the Orchard property."),
            ("Any other view", "injects IOrchardHelper."),
        ], (96, 440, W - 96, 900))
        return image

    if step_id == "api-1":
        image = slide_base(chapter_title, label, "GET or POST api/queries/{name}")
        draw_code(image, (96, 240, W - 96, 800), "Requests", "bash", REQUESTS, types=TYPES, size=26)
        return image

    if step_id == "api-2":
        image = slide_base(chapter_title, label, "Parameters are safe, output expressions are not")
        draw_code(image, (96, 240, 940, 480), "Unsafe: an output expression", "bash", UNSAFE_SQL, size=26)
        draw_code(image, (980, 240, W - 96, 480), "Safe: a SQL parameter", "bash", SAFE_SQL, size=26)
        note_cards(image, [
            ("Output expression", "renders user input into the SQL text: SQL injection."),
            ("SQL parameter", "the value of the caller reaches the database as a parameter."),
        ], (96, 540, W - 96, 1000))
        return image

    if step_id == "api-3":
        image = slide_base(chapter_title, label, "The permissions")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "api-4":
        image = slide_base(chapter_title, label, "A bearer token, not the cookie")
        draw_code(image, (96, 240, W - 96, 460), "Request with a token", "bash", BEARER_REQUEST, types=TYPES, size=28)
        note_cards(image, [
            ("No token", "the request is anonymous: the permissions of the Anonymous role apply."),
            ("Sign-in cookie", "the query endpoint does not use it."),
        ], (96, 520, W - 96, 960))
        return image

    if step_id == "api-5":
        image = slide_base(chapter_title, label, "The result")
        draw_code(image, (96, 240, 940, 520), "SQL query", "json", SQL_RESULT, size=28)
        draw_code(image, (980, 240, W - 96, 520), "Lucene or Elasticsearch query", "json", LUCENE_RESULT, size=28)
        note_cards(image, [
            ("Content items", "when the query returns them."),
            ("Rows", "when it does not: an object with the columns of the row."),
        ], (96, 580, W - 96, 1020))
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "One saved query, three callers")
        bullets(image, CALLERS)
        return image

    raise SystemExit(f"No slide for {step_id}")
