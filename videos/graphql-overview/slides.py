"""The slides of the GraphQL overview video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"ObjectGraphType", "AutoroutePart", "AutorouteQueryObjectType", "StartupBase", "IServiceCollection",
         "ISchemaBuilder", "ContentItemQuery", "ISchema", "FieldType", "ContentItemInterface", "Task",
         "IStringLocalizer"}

FIXED_SHAPE = [
    ("1", "The server decides the shape", "Every response holds the same fields."),
    ("2", "Fields you never use", "They travel to the client anyway."),
    ("3", "Related data, more requests", "Each related item needs another call."),
]

MODULE = [
    ("check", "One endpoint", "Clients send their queries to /api/graphql."),
    ("check", "Built from your content", "The schema follows content types, parts and fields."),
    ("check", "No code for a content type", "A content type you add appears in the schema."),
    ("check", "Permission checks", "Every request is checked before it runs."),
]

SCHEMA = [
    ("1", "A content type is a query field", "BlogPost becomes blogPost. Types with a stereotype, like Widget, are left out."),
    ("2", "A part with fields or a GraphQL type is an object", "AutoroutePart becomes autoroute, with a path field inside it."),
    ("3", "A part named like its type is collapsed", "BlogPost puts subtitle directly on blogPost."),
    ("4", "Settings can change it", "The content type editor can hide a type or a part, or collapse a part."),
]

REBUILD = [
    ("1", "Change a content type", "Add a part or a field in the content type editor."),
    ("2", "Send the next request", "The module notices the change and builds the schema again."),
    ("3", "Reload GraphiQL", "The Explorer lists the change."),
]

WHO = [
    ("1", "The Execute GraphQL permission", "Every request needs it. The Administrator role has it."),
    ("2", "A session cookie", "The signed-in user of the admin, as in GraphiQL."),
    ("3", "A bearer token", "Issued through the OpenID Connect module."),
    ("4", "Anonymous users", "They cannot run queries by default."),
]

CONTENT_PERMISSIONS = [
    ("1", "View permissions", "A query returns only the content items the user can view."),
    ("2", "Permissions of a content type", "They apply to the query of that content type."),
]

QUERY = """
{
  blogPost {
    displayText
    subtitle
  }
}
"""

RESPONSE = """
{
  "data": {
    "blogPost": [
      {
        "displayText": "A Lake Before Breakfast",
        "subtitle": "Early mornings by the water"
      }
    ]
  }
}
"""

ARGUMENTS = """
{
  blogPost(
    where: { displayText_contains: "Alps" }
    orderBy: { createdUtc: DESC }
    first: 10
    skip: 0
    status: PUBLISHED
  ) {
    displayText
  }
}
"""

CONTENT_ITEM = """
{
  contentItem(contentItemId: "...") {
    displayText
  }
}
"""

POST_JSON = """
curl https://example.com/api/graphql \\
  -H "Content-Type: application/json" \\
  -H "Authorization: Bearer <token>" \\
  -d '{ "query": "{ blogPost { displayText } }" }'
"""

GET_QUERY = """
curl -g "https://example.com/api/graphql?query={blogPost{displayText}}" \\
  -H "Authorization: Bearer <token>"
"""

POST_GRAPHQL = """
curl https://example.com/api/graphql \\
  -H "Content-Type: application/graphql" \\
  -H "Authorization: Bearer <token>" \\
  -d '{ blogPost { displayText } }'
"""

VARIABLES = """
{
  "query": "query ($first: Int) { blogPost(first: $first) { displayText } }",
  "variables": { "first": 2 }
}
"""

LIST_RESPONSE = """
{
  "data": {
    "blogPost": [
      { "displayText": "A Lake Before Breakfast" },
      { "displayText": "Packing Light for Long Walks" },
      { "displayText": "Planning a Week in the Alps" }
    ]
  }
}
"""

OPTIONS = """
{
  "OrchardCore": {
    "Apis": {
      "GraphQL": {
        "ExposeExceptions": false,
        "MaxDepth": 50,
        "MaxComplexity": 100,
        "DefaultNumberOfResults": 100,
        "MaxNumberOfResults": 1000,
        "MaxNumberOfResultsValidationMode": "Enabled"
      }
    }
  }
}
"""

QUERY_TYPE = """
public class AutorouteQueryObjectType : ObjectGraphType<AutoroutePart>
{
    public AutorouteQueryObjectType(IStringLocalizer<AutorouteQueryObjectType> S)
    {
        Name = "AutoroutePart";
        Description = S["Custom URLs (permalinks) for your content item."];

        Field(x => x.Path).Description(S["The permalinks for your content item."]);
    }
}
"""

STARTUP = """
[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddObjectGraphType<AutoroutePart, AutorouteQueryObjectType>();
        // ...
    }
}
"""

SCHEMA_BUILDER = """
public sealed class ContentItemQuery : ISchemaBuilder
{
    // ...

    public Task<string> GetIdentifierAsync()
        => Task.FromResult(string.Empty);

    public Task BuildAsync(ISchema schema)
    {
        var field = new FieldType
        {
            Name = "ContentItem",
            // ...
        };
        schema.Query.AddField(field);
        return Task.CompletedTask;
    }
}
"""

REGISTRATION = """
services.AddSingleton<ISchemaBuilder, ContentItemQuery>();
"""


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A fixed response shape")
        bullets(image, FIXED_SHAPE)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The client asks, the response matches")
        draw_code(image, (96, 240, 900, 700), "Query", "javascript", QUERY, types=TYPES, size=26)
        draw_code(image, (940, 240, W - 96, 700), "Response", "json", RESPONSE, types=TYPES, size=24)
        note_cards(image, [("One request", "The response has the same shape as the query.")], (96, 740, W - 96, 1000))
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The GraphQL module")
        bullets(image, MODULE)
        return image

    if step_id == "schema-1":
        image = slide_base(chapter_title, label, "From content definitions to a schema")
        bullets(image, SCHEMA)
        return image

    if step_id == "schema-2":
        image = slide_base(chapter_title, label, "Arguments of a content type query")
        draw_code(image, (96, 240, 1150, 730), "Query", "javascript", ARGUMENTS, types=TYPES, size=26)
        draw_code(image, (96, 750, 1150, 1030), "One item by id", "javascript", CONTENT_ITEM, types=TYPES, size=26)
        note_cards(image, [
            ("where", "Filters the items."),
            ("orderBy, first, skip", "Sort and page the items."),
            ("status", "Which versions are read."),
        ], (1200, 240, W - 96, 1000))
        return image

    if step_id == "schema-3":
        image = slide_base(chapter_title, label, "The schema keeps up")
        bullets(image, REBUILD)
        return image

    if step_id == "http-1":
        image = slide_base(chapter_title, label, "POST or GET to the endpoint")
        draw_code(image, (96, 240, W - 96, 500), "POST: the query in a JSON body", "bash", POST_JSON, types=TYPES, size=30)
        draw_code(image, (96, 540, W - 96, 720), "GET: the query in the query string", "bash", GET_QUERY, types=TYPES, size=30)
        return image

    if step_id == "http-2":
        image = slide_base(chapter_title, label, "Other ways to send a query")
        draw_code(image, (96, 240, W - 96, 500), "POST: the body is the query", "bash", POST_GRAPHQL, types=TYPES, size=30)
        draw_code(image, (96, 540, W - 96, 790), "POST: variables in the JSON body", "json", VARIABLES, types=TYPES, size=28)
        return image

    if step_id == "http-3":
        image = slide_base(chapter_title, label, "The response")
        draw_code(image, (96, 240, 1150, 700), "Response", "json", LIST_RESPONSE, types=TYPES, size=25)
        note_cards(image, [
            ("200", "data holds the result."),
            ("400", "errors describes the problem."),
            ("401", "A permission is missing."),
        ], (1200, 240, W - 96, 1000))
        return image

    if step_id == "http-4":
        image = slide_base(chapter_title, label, "Who can run a query")
        bullets(image, WHO)
        return image

    if step_id == "http-5":
        image = slide_base(chapter_title, label, "Content permissions apply as well")
        bullets(image, CONTENT_PERMISSIONS)
        return image

    if step_id == "config-1":
        image = slide_base(chapter_title, label, "Options of the GraphQL module")
        draw_code(image, (96, 240, 1150, 800), "appsettings.json", "json", OPTIONS, types=TYPES, size=24)
        note_cards(image, [
            ("MaxDepth, MaxComplexity", "Limit the cost of a query."),
            ("DefaultNumberOfResults", "Used when first is missing."),
            ("ExposeExceptions", "When true, clients see stack traces."),
        ], (1200, 240, W - 96, 1000))
        return image

    if step_id == "ext-1":
        image = slide_base(chapter_title, label, "1. Expose your own part")
        draw_code(image, (96, 240, W - 96, 632), "AutorouteQueryObjectType.cs", "csharp", QUERY_TYPE, types=TYPES, size=22)
        draw_code(image, (96, 652, W - 96, 1012), "Startup.cs", "csharp", STARTUP, types=TYPES, size=22)
        return image

    if step_id == "ext-2":
        image = slide_base(chapter_title, label, "2. Add queries with ISchemaBuilder")
        draw_code(image, (96, 240, W - 96, 900), "ContentItemQuery.cs", "csharp", SCHEMA_BUILDER, types=TYPES, size=22)
        draw_code(image, (96, 920, W - 96, 1032), "Startup.cs", "csharp", REGISTRATION, types=TYPES, size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
