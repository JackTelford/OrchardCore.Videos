"""The slides of the Queries module video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is taken from OrchardCore.Queries (and its SQL source), trimmed with // ... to what the narration talks about.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import MUTED, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {
    "Entity", "Query", "IQuerySource", "IQueryResults", "IQueryHandler", "QueryHandlerBase", "SqlQuerySource",
    "SqlQueryMetadata", "SqlQueryDisplayDriver", "SqlQueryHandler", "DisplayDriver", "IDisplayResult",
    "UpdateEditorContext", "InitializingQueryContext", "UpdatingQueryContext", "DataQueryContextBase",
    "IServiceCollection", "StartupBase", "Task", "IDictionary",
}

PROBLEMS = [
    ("1", "Copied", "The same SQL or search text lives in templates, scripts and controllers."),
    ("2", "Edited everywhere", "When the question changes, every copy has to be found and edited."),
]

FEATURES = [
    ("1", "Queries Core Services", "The query manager, the recipe step and the Liquid filter."),
    ("2", "Queries", "The screens, the permissions and the API."),
    ("3", "SQL Queries", "A source that queries the SQL database."),
    ("4", "Lucene and Elasticsearch", "Sources that query an index."),
]

RUN = [
    ("1", "Liquid", "Queries.RecentBlogPosts | query"),
    ("2", "Razor", 'await Orchard.ContentQueryAsync("RecentBlogPosts")'),
    ("3", "Scripts", "executeQuery(name, parameters)"),
    ("4", "HTTP API", "GET or POST api/queries/{name}"),
]

PERMISSIONS = [
    ("1", "Manage queries", "Includes the two below. Administrator and Editor have it by default."),
    ("2", "Execute Api - All queries", "Runs every query through the API."),
    ("3", "Execute Api - RecentBlogPosts", "One permission per query. Without it, the API answers Not Found."),
]

DEPLOYMENT = [
    ("1", "All Queries", "Exports every query as a Queries step."),
    ("2", "Queried Content Items", "Exports the content items one query returns. Needs Deployment and Contents."),
]

QUERY = """
public class Query : Entity
{
    public string Name { get; set; }

    public string Source { get; set; }

    public string Schema { get; set; }

    public bool ReturnContentItems { get; set; }
}
"""

RECIPE = """
{
  "steps": [
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
  ]
}
"""

SOURCE = """
public sealed class SqlQuerySource : IQuerySource
{
    public const string SourceName = "Sql";

    // ...

    public string Name
        => SourceName;

    public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
    {
        var template = string.Empty;

        if (query.TryGet<SqlQueryMetadata>(out var metadata))
        {
            template = metadata.Template;
        }

        // ...
    }
}
"""

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddDisplayDriver<Query, SqlQueryDisplayDriver>();
        services.AddQuerySource<SqlQuerySource>(SqlQuerySource.SourceName);
        // ...
        services.AddScoped<IQueryHandler, SqlQueryHandler>();
    }
}
"""

ADD_QUERY_SOURCE = """
public static IServiceCollection AddQuerySource<TSource>(this IServiceCollection services, string sourceName)
    where TSource : class, IQuerySource
{
    // ...
    services.AddScoped<TSource>();
    services.AddScoped<IQuerySource>(sp => sp.GetService<TSource>());
    services.AddKeyedScoped<IQuerySource>(sourceName, (sp, key) => sp.GetService<TSource>());
    return services;
}
"""

DRIVER = """
public sealed class SqlQueryDisplayDriver : DisplayDriver<Query>
{
    // ...

    public override async Task<IDisplayResult> UpdateAsync(Query query, UpdateEditorContext context)
    {
        if (query.Source != SqlQuerySource.SourceName)
        {
            return null;
        }

        // ...

        query.ReturnContentItems = viewModel.ReturnDocuments;
        query.Put(new SqlQueryMetadata()
        {
            Template = viewModel.Query,
        });

        return await EditAsync(query, context);
    }
}
"""

LINK = """
<div class="col">
    <div class="card h-100">
        <div class="card-body">
            <h4>@T["SQL"]</h4>
            <p class="hint">@T["Queries the SQL database."]</p>
        </div>
        <div class="card-footer text-muted text-sm-end">
            <a class="btn btn-primary btn-sm"
               asp-route-action="Create"
               asp-route-controller="Admin"
               asp-route-id="@Model.Name"
               asp-area="OrchardCore.Queries">@T["Add"]</a>
        </div>
    </div>
</div>
"""

HANDLER = """
public sealed class SqlQueryHandler : QueryHandlerBase
{
    public override Task InitializingAsync(InitializingQueryContext context)
        => UpdateQueryAsync(context);

    public override Task UpdatingAsync(UpdatingQueryContext context)
        => UpdateQueryAsync(context);

    private static Task UpdateQueryAsync(DataQueryContextBase context)
    {
        if (context.Query.Source != SqlQuerySource.SourceName)
        {
            return Task.CompletedTask;
        }

        var template = context.Data[nameof(SqlQueryMetadata.Template)]?.GetValue<string>();
        // ...
    }
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without saved queries")
        bullets(image, PROBLEMS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "A saved query")
        draw_code(image, (96, 240, 960, 760), "Query.cs", "csharp", QUERY, types=TYPES, size=28)
        note_cards(image, [
            ("Name", "The technical name callers use."),
            ("Source", "Decides how the query runs."),
            ("Schema", "The result shape for GraphQL."),
        ], (1010, 240, W - 96, 1030))
        draw = ImageDraw.Draw(image)
        draw.text((96, 820), "ReturnContentItems: content items, or any other rows.", font=font(30), fill=MUTED, anchor="lm")
        draw.text((96, 876), "A source stores its own values on the query itself.", font=font(30), fill=MUTED, anchor="lm")
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Features of the module")
        bullets(image, FEATURES)
        return image

    if step_id == "run-1":
        image = slide_base(chapter_title, label, "Run a query by name")
        bullets(image, RUN)
        return image

    if step_id == "run-2":
        image = slide_base(chapter_title, label, "Who can run a query")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "The Queries recipe step")
        draw_code(image, (96, 240, W - 96, 880), "blog.recipe.json", "json", RECIPE, types=TYPES, size=22)
        draw = ImageDraw.Draw(image)
        draw.text((96, 940), "An existing query with the same name is updated.", font=font(30), fill=MUTED, anchor="lm")
        draw.text((96, 996), "A missing Name or Source is reported as an error.", font=font(30), fill=MUTED, anchor="lm")
        return image

    if step_id == "deploy-1":
        image = slide_base(chapter_title, label, "Queries in a deployment plan")
        bullets(image, DEPLOYMENT)
        return image

    if step_id == "source-1":
        image = slide_base(chapter_title, label, "1. Implement IQuerySource")
        draw_code(image, (96, 240, W - 96, 1030), "SqlQuerySource.cs", "csharp", SOURCE, types=TYPES, size=24)
        return image

    if step_id == "source-2":
        image = slide_base(chapter_title, label, "2. Register it with a name")
        draw_code(image, (96, 240, W - 96, 660), "Startup.cs", "csharp", STARTUP, types=TYPES, highlight={1, 2, 3, 4, 7, 10, 11}, size=22)
        draw_code(image, (96, 680, W - 96, 1040), "ServiceCollectionExtensions.cs", "csharp", ADD_QUERY_SOURCE, types=TYPES, size=22)
        return image

    if step_id == "source-3":
        image = slide_base(chapter_title, label, "3. Edit it with a display driver")
        draw_code(image, (96, 240, W - 96, 1030), "SqlQueryDisplayDriver.cs", "csharp", DRIVER, types=TYPES, size=23)
        return image

    if step_id == "source-4":
        image = slide_base(chapter_title, label, "4. Add its card to Add Query")
        draw_code(image, (96, 240, 1010, 1030), "Query-Sql.Link.cshtml", "html", LINK, types=TYPES, size=22)
        note_cards(image, [
            ("SQL", "Query-Sql.Link.cshtml"),
            ("Lucene", "Query-Lucene.Link.cshtml"),
            ("Elasticsearch", "Query-Elasticsearch.Link.cshtml"),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id == "source-5":
        image = slide_base(chapter_title, label, "5. Read recipe data in a handler")
        draw_code(image, (96, 240, W - 96, 1030), "SqlQueryHandler.cs", "csharp", HANDLER, types=TYPES, size=25)
        return image

    raise SystemExit(f"No slide for {step_id}")
