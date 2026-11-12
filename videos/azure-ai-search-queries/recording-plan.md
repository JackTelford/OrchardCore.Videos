# Recording plan: azure-ai-search-queries

Read this file from top to bottom; nothing else is needed to record the video. It records two clips, `04-create`
and `05-run`. The other chapters are slides, drawn by `slides.py`, and the cards need no recording. Narrate first
(`narrate.py`), then record.

Orchard Core has no Azure AI Search query source (the only `AddQuerySource` calls are in the Lucene, Elasticsearch
and SQL Queries features). The video therefore shows a small module that adds one, so the site needs that module
(section 1.2), an Azure AI Search service, and a stand-in for that service (section 1.1), because the service has no
local emulator. The module's code is the code on the slides.

## 1. The site it needs

### 1.0 Tools

- An `OrchardCMS/OrchardCore` checkout at commit `92db4a83` of `main` (or a later commit where every file this plan
  cites under `src/` still exists), and the .NET SDK that its `src/OrchardCore.Build/TargetFrameworks.props` asks
  for (`CommonTargetFrameworks`).
- The ASP.NET Core development certificate, trusted: `dotnet dev-certs https --check --trust` must print "A trusted
  certificate was found". The Azure SDK refuses an endpoint that is not `https`, and the site only accepts the
  stand-in's certificate if the machine already trusts it. Do not add any certificate made for this video to a
  trust store; if the development certificate is not trusted, stop and ask (see OPEN-QUESTIONS.md).
- Python and the recorder, as in the skill. Narrate before recording.

### 1.1 The Azure AI Search stand-in

A tiny ASP.NET Core app that answers the five calls the site makes to the service, over `https://localhost:7443`,
with the development certificate, and keeps everything in memory. Outside both repositories, make a folder
`AzureSearchStandIn` with these two files, then run `dotnet run` in it and leave it running while you record.

`AzureSearchStandIn.csproj` (use the target framework of an installed SDK):

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>disable</Nullable>
  </PropertyGroup>
</Project>
```

`Program.cs`:

```csharp
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:7443");
var app = builder.Build();

var indexes = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
var documents = new Dictionary<string, List<JsonObject>>(StringComparer.OrdinalIgnoreCase);

app.Use(async (context, next) =>
{
    Console.WriteLine($"{context.Request.Method} {context.Request.Path}");
    await next();
});

// Creates an index: the body is the index definition.
app.MapPost("/indexes", async (HttpRequest request) =>
{
    var definition = await JsonNode.ParseAsync(request.Body) as JsonObject;
    var name = definition?["name"]?.GetValue<string>();

    if (string.IsNullOrEmpty(name))
    {
        return Results.BadRequest();
    }

    indexes[name] = definition;
    documents[name] = [];

    return Results.Json(definition.DeepClone(), statusCode: 201);
});

// Reads an index definition: 404 when the index does not exist.
app.MapGet("/indexes('{name}')", (string name) =>
    indexes.TryGetValue(name, out var definition)
        ? Results.Json(definition.DeepClone())
        : Results.Json(new JsonObject { ["error"] = new JsonObject { ["message"] = $"No index with the name '{name}' was found." } }, statusCode: 404));

app.MapDelete("/indexes('{name}')", (string name) =>
{
    documents.Remove(name);

    return indexes.Remove(name) ? Results.NoContent() : Results.NotFound();
});

// Adds, replaces or deletes documents.
app.MapPost("/indexes('{name}')/docs/search.index", async (string name, HttpRequest request) =>
{
    if (!documents.TryGetValue(name, out var stored))
    {
        return Results.NotFound();
    }

    var body = await JsonNode.ParseAsync(request.Body) as JsonObject;
    var results = new JsonArray();

    foreach (var action in body["value"].AsArray())
    {
        var document = action.AsObject().DeepClone().AsObject();
        var kind = document["@search.action"]?.GetValue<string>() ?? "upload";
        document.Remove("@search.action");

        var key = document["ContentItemId"]?.GetValue<string>() ?? document.Select(x => x.Value?.ToString()).FirstOrDefault() ?? string.Empty;
        stored.RemoveAll(x => x["ContentItemId"]?.GetValue<string>() == key);

        if (kind != "delete")
        {
            stored.Add(document);
        }

        results.Add(new JsonObject { ["key"] = key, ["status"] = true, ["errorMessage"] = null, ["statusCode"] = 200 });
    }

    return Results.Json(new JsonObject { ["value"] = results });
});

// Searches: a document matches when any of its values contains any of the words, ignoring case.
app.MapPost("/indexes('{name}')/docs/search.post.search", async (string name, HttpRequest request) =>
{
    if (!documents.TryGetValue(name, out var stored))
    {
        return Results.NotFound();
    }

    var body = await JsonNode.ParseAsync(request.Body) as JsonObject ?? new JsonObject();
    var text = body["search"]?.GetValue<string>() ?? "*";
    var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var select = body["select"]?.GetValue<string>()?.Split(',', StringSplitOptions.TrimEntries);
    var skip = body["skip"]?.GetValue<int>() ?? 0;
    var top = body["top"]?.GetValue<int>() ?? 50;

    var matches = stored
        .Where(x => text == "*" || words.Any(w => x.Any(p => p.Value?.ToString().Contains(w, StringComparison.OrdinalIgnoreCase) == true)))
        .ToList();

    var value = new JsonArray();

    foreach (var match in matches.Skip(skip).Take(top))
    {
        var hit = new JsonObject { ["@search.score"] = 1.0 };

        foreach (var property in match.Where(p => select is null || select.Contains(p.Key)))
        {
            hit[property.Key] = property.Value?.DeepClone();
        }

        value.Add(hit);
    }

    var result = new JsonObject();

    if (body["count"]?.GetValue<bool>() == true)
    {
        result["@odata.count"] = matches.Count;
    }

    result["value"] = value;

    return Results.Json(result);
});

// Not part of the Azure API: shows how many documents each index holds.
app.MapGet("/stats", () => Results.Json(documents.ToDictionary(x => x.Key, x => x.Value.Count)));

app.Run();
```

The author ran these two files against the Azure SDK package `Azure.Search.Documents` (11.7.0) in a console app:
creating, reading and deleting an index, uploading, searching and deleting documents all worked (OPEN-QUESTIONS.md
says what is still untested).

Check it: `curl https://localhost:7443/stats` prints `{}`. Every request is logged to its console, which is the
place to look when the site cannot reach it. A document matches when any of its values contains one of the words
of the search text, ignoring case; a search for `*` matches every document. It stores nothing on disk, so
restarting it empties the "index": see section 2 to fill it again.

### 1.2 The module

The module is a throwaway project, never committed to the Orchard Core repository. Create the folder
`src/OrchardCore.Modules/Acme.AzureAIQueries` inside the checkout, add it to `.git/info/exclude`, and create these
files in it. Its code is what the slides `source-2`, `source-3`, `settings-1`, `settings-2` and `startup-1` show.

`Acme.AzureAIQueries.csproj` (modeled on `src/OrchardCore.Modules/OrchardCore.Elasticsearch/OrchardCore.Elasticsearch.csproj`):

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Module.Targets\OrchardCore.Module.Targets.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.DisplayManagement\OrchardCore.DisplayManagement.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ResourceManagement\OrchardCore.ResourceManagement.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ContentManagement\OrchardCore.ContentManagement.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Liquid.Abstractions\OrchardCore.Liquid.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Indexing.Abstractions\OrchardCore.Indexing.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Search.Abstractions\OrchardCore.Search.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Queries.Core\OrchardCore.Queries.Core.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.AzureAI.Core\OrchardCore.AzureAI.Core.csproj" />
  </ItemGroup>

</Project>
```

`Manifest.cs`:

```csharp
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure AI Search Queries",
    Author = "Acme",
    Website = "https://example.com",
    Version = "0.0.1",
    Description = "Saved queries that search an Azure AI Search index.",
    Dependencies = ["OrchardCore.Queries", "OrchardCore.Search", "OrchardCore.AzureAI"],
    Category = "Search"
)]
```

`AzureAISearchQueryMetadata.cs`:

```csharp
namespace Acme.AzureAIQueries;

public sealed class AzureAISearchQueryMetadata
{
    public string Index { get; set; }

    public string Template { get; set; }
}
```

`AzureAISearchQueryResults.cs`:

```csharp
using OrchardCore.Queries;

namespace Acme.AzureAIQueries;

public sealed class AzureAISearchQueryResults : IQueryResults
{
    public IEnumerable<object> Items { get; set; }

    public long Count { get; set; }
}
```

`AzureAISearchQueryViewModel.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Acme.AzureAIQueries;

public class AzureAISearchQueryViewModel
{
    public string Index { get; set; }

    public string Query { get; set; }

    public bool ReturnContentItems { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Indexes { get; set; }
}
```

`AzureAISearchQuerySource.cs` (modeled on `OrchardCore.Elasticsearch.Core/Services/ElasticsearchQuerySource.cs` and
on the loading of `OrchardCore.Search/Controllers/SearchController.cs`, lines 115 and 202 to 251):

```csharp
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AzureAI;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Queries;
using OrchardCore.Search.Abstractions;
using YesSql;
using YesSql.Services;

namespace Acme.AzureAIQueries;

public sealed class AzureAISearchQuerySource : IQuerySource
{
    public const string SourceName = AzureAISearchConstants.ProviderName;

    private readonly IIndexProfileStore _indexProfileStore;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly TemplateOptions _templateOptions;

    public AzureAISearchQuerySource(
        IIndexProfileStore indexProfileStore,
        ILiquidTemplateManager liquidTemplateManager,
        ISession session,
        IServiceProvider serviceProvider,
        IOptions<TemplateOptions> templateOptions)
    {
        _indexProfileStore = indexProfileStore;
        _liquidTemplateManager = liquidTemplateManager;
        _session = session;
        _serviceProvider = serviceProvider;
        _templateOptions = templateOptions.Value;
    }

    public string Name
        => SourceName;

    public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
    {
        var results = new AzureAISearchQueryResults { Items = [] };

        if (!query.TryGet<AzureAISearchQueryMetadata>(out var metadata) || string.IsNullOrEmpty(metadata.Index))
        {
            return results;
        }

        var index = await _indexProfileStore.FindByNameAsync(metadata.Index);

        if (index is null || index.ProviderName != AzureAISearchConstants.ProviderName)
        {
            return results;
        }

        var terms = await _liquidTemplateManager.RenderStringAsync(metadata.Template, NullEncoder.Default,
            parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

        var searchService = _serviceProvider.GetKeyedService<ISearchService>(AzureAISearchConstants.ProviderName);
        var found = await searchService.SearchAsync(index, terms, 0, 10);

        if (!found.Success || found.ContentItemIds.Count == 0)
        {
            return results;
        }

        var ids = found.ContentItemIds;
        var items = found.Latest
            ? await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId.IsIn(ids) && x.Latest).ListAsync()
            : await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId.IsIn(ids) && x.Published).ListAsync();

        var ordered = items.OrderBy(x => ids.IndexOf(x.ContentItemId)).ToList();
        results.Count = ordered.Count;
        results.Items = query.ReturnContentItems ? ordered : ordered.Select(x => new { x.ContentItemId, x.DisplayText });

        return results;
    }
}
```

`AzureAISearchQueryHandler.cs` (modeled on `OrchardCore.Elasticsearch.Core/Services/ElasticsearchQueryHandler.cs`):

```csharp
using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;

namespace Acme.AzureAIQueries;

public sealed class AzureAISearchQueryHandler : QueryHandlerBase
{
    public override Task InitializingAsync(InitializingQueryContext context)
        => UpdateQueryAsync(context);

    public override Task UpdatingAsync(UpdatingQueryContext context)
        => UpdateQueryAsync(context);

    private static Task UpdateQueryAsync(DataQueryContextBase context)
    {
        if (context.Query.Source != AzureAISearchQuerySource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = context.Query.GetOrCreate<AzureAISearchQueryMetadata>();

        metadata.Index = context.Data[nameof(metadata.Index)]?.GetValue<string>() ?? metadata.Index;
        metadata.Template = context.Data[nameof(metadata.Template)]?.GetValue<string>() ?? metadata.Template;
        context.Query.Put(metadata);

        return Task.CompletedTask;
    }
}
```

`AzureAISearchQueryDisplayDriver.cs` (modeled on `OrchardCore.Elasticsearch/Drivers/ElasticsearchQueryDisplayDriver.cs`):

```csharp
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.AzureAI;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries;

namespace Acme.AzureAIQueries;

public sealed class AzureAISearchQueryDisplayDriver : DisplayDriver<Query>
{
    private readonly IIndexProfileStore _indexProfileStore;

    internal readonly IStringLocalizer S;

    public AzureAISearchQueryDisplayDriver(
        IIndexProfileStore indexProfileStore,
        IStringLocalizer<AzureAISearchQueryDisplayDriver> stringLocalizer)
    {
        _indexProfileStore = indexProfileStore;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(Query query, BuildEditorContext context)
    {
        if (query.Source != AzureAISearchQuerySource.SourceName)
        {
            return null;
        }

        var metadata = query.GetOrCreate<AzureAISearchQueryMetadata>();
        var indexes = await _indexProfileStore.GetByProviderAsync(AzureAISearchConstants.ProviderName);

        return Initialize<AzureAISearchQueryViewModel>("AzureAISearchQuery_Edit", model =>
        {
            model.Index = metadata.Index;
            model.Query = metadata.Template;
            model.ReturnContentItems = query.ReturnContentItems;
            model.Indexes = indexes.Select(x => new SelectListItem(x.Name, x.Name)).ToArray();
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(Query query, UpdateEditorContext context)
    {
        if (query.Source != AzureAISearchQuerySource.SourceName)
        {
            return null;
        }

        var model = new AzureAISearchQueryViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Index,
            m => m.Query,
            m => m.ReturnContentItems);

        if (string.IsNullOrWhiteSpace(model.Index))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Index), S["The index field is required"]);
        }

        if (string.IsNullOrWhiteSpace(model.Query))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Query), S["The query field is required"]);
        }

        query.ReturnContentItems = model.ReturnContentItems;
        query.Put(new AzureAISearchQueryMetadata
        {
            Index = model.Index,
            Template = model.Query,
        });

        return await EditAsync(query, context);
    }
}
```

`Startup.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;

namespace Acme.AzureAIQueries;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddQuerySource<AzureAISearchQuerySource>(AzureAISearchQuerySource.SourceName);
        services.AddDisplayDriver<Query, AzureAISearchQueryDisplayDriver>();
        services.AddScoped<IQueryHandler, AzureAISearchQueryHandler>();
    }
}
```

`Views/_ViewImports.cshtml` (modeled on the one of `OrchardCore.Elasticsearch`):

```cshtml
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement
@using Microsoft.Extensions.Localization
@using Microsoft.AspNetCore.Mvc.Localization
@using Acme.AzureAIQueries
```

`Views/Query-AzureAISearch.Link.cshtml`, the card of the Available Query Types dialog (modeled on
`OrchardCore.Queries/Views/Query-Sql.Link.cshtml`):

```cshtml
@model dynamic

<div class="col">
    <div class="card h-100">
        <div class="card-body">
            <h4>@T["Azure AI Search"]</h4>
            <p class="hint">@T["Queries an Azure AI Search index."]</p>
        </div>
        <div class="card-footer text-muted text-sm-end">
            <a class="btn btn-primary btn-sm" asp-route-action="Create" asp-route-controller="Admin" asp-route-id="@Model.Name" asp-area="OrchardCore.Queries">@T["Add"]</a>
        </div>
    </div>
</div>
```

`Views/AzureAISearchQuery.Edit.cshtml`, the editor (modeled on `OrchardCore.Elasticsearch/Views/ElasticQuery.Edit.cshtml`):

```cshtml
@model AzureAISearchQueryViewModel

<div class="ocat-wrapper">
    <label asp-for="Index" class="ocat-label ocat-label-required">@T["Index"]</label>
    <div class="ocat-end">
        <select asp-for="Index" class="form-select" asp-items="Model.Indexes">
            <option value="">@T["Select an index"]</option>
        </select>
        <span asp-validation-for="Index"></span>
        <span class="hint">@T["The Azure AI Search index to search on."]</span>
    </div>
</div>

<div class="ocat-wrapper">
    <div class="ocat-end-offset">
        <div class="form-check">
            <input type="checkbox" class="form-check-input" asp-for="ReturnContentItems" checked="@Model.ReturnContentItems" />
            <label class="form-check-label" asp-for="ReturnContentItems">@T["Return Content Items"]</label>
            <span class="hint dashed">@T["Check to return the corresponding content items."]</span>
        </div>
    </div>
</div>

<div class="ocat-wrapper">
    <label asp-for="Query" class="ocat-label ocat-label-required">@T["Query"]</label>
    <div class="ocat-end">
        <textarea asp-for="Query" rows="4" class="form-control"></textarea>
        <span asp-validation-for="Query"></span>
        <span class="hint">@T["A Liquid template that renders the search text. The parameters of the call are available by name."]</span>
    </div>
</div>
```

Then reference the module from the host: in `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`, add
`<ProjectReference Include="..\OrchardCore.Modules\Acme.AzureAIQueries\Acme.AzureAIQueries.csproj" />` to the
`ItemGroup` that holds the other `ProjectReference` lines. This edit is local: never commit it, and restore the file
with `git checkout` when the recording is done. Build `src/OrchardCore.Cms.Web`; the module has not been built by
the author of this plan, so fix compile errors against the sources cited above if there are any (OPEN-QUESTIONS.md).

### 1.3 The tenant

Run `src/OrchardCore.Cms.Web` on `http://localhost:5010` (`ASPNETCORE_URLS`), with `ORCHARD_APP_DATA` pointing at an
empty folder outside both repositories, and these environment variables, so that Auto Setup installs the site on
the first request (`src/docs/reference/modules/AutoSetup/README.md`):

| Variable | Value |
|---|---|
| `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
| `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
| `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
| `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
| `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
| `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
| `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
| `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

The Blog recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`) enables Queries, SQL Queries and
Indexing, and creates one blog post and the SQL query `RecentBlogPosts`.

Outbound calls are blocked except the stand-in: `HTTP_PROXY=http://127.0.0.1:9`, `HTTPS_PROXY=http://127.0.0.1:9`
and `NO_PROXY=localhost,127.0.0.1`. If the site cannot reach the stand-in with these set (the stand-in console
shows no request), check this first.

### 1.4 The Azure AI Search settings

Set these variables too, before starting the site, so that the settings come from the configuration and no page
that shows a key is recorded (`src/docs/reference/modules/AzureAISearch/README.md`; the section is read in
`OrchardCore.AzureAI.Core/Services/AzureAISearchDefaultOptionsConfigurations.cs`, lines 44 to 62):

| Variable | Value |
|---|---|
| `OrchardCore__Search__AzureAISearch__Endpoint` | `https://localhost:7443` |
| `OrchardCore__Search__AzureAISearch__Credential__Key` | `standin-key-0000` (any fake text; the stand-in ignores it) |

### 1.5 Sign in, features, index and posts (outside the capture)

1. Start the stand-in, then the site. Open `http://localhost:5010` once so that Auto Setup runs, and wait for the
   home page.
2. `Recorder setup` signs in as `admin` with `VIDEO_PASSWORD` and saves `state.json` (never committed).
3. Enable the features: Tools > Deployments > JSON Import (the "JSON" field, then the "Import" button;
   `OrchardCore.Deployment/AdminMenu.cs:30`, `Views/Import/Json.cshtml:15,21`) with this recipe. The field is a
   CodeMirror editor: paste into it, or set its value with `CodeMirror.setValue`.

   ```json
   {
     "steps": [
       {
         "name": "feature",
         "enable": [
           "OrchardCore.AzureAI",
           "OrchardCore.Search",
           "OrchardCore.Indexing.Worker",
           "Acme.AzureAIQueries"
         ]
       }
     ]
   }
   ```

   Check in Tools > Features that "Azure AI Search", "Search", "Indexing Worker" and "Azure AI Search Queries" are enabled.
4. Import three fake blog posts with Tools > Deployments > JSON Import, the same way:

   ```json
   {
     "steps": [
       {
         "name": "content",
         "Data": [
           {
             "ContentType": "BlogPost",
             "ContentItemId": "4f1a8c2d9b3e7a60c5d1e8f2b4",
             "DisplayText": "Orchard Core search tips",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": { "Title": "Orchard Core search tips" },
             "AutoroutePart": { "Path": "orchard-core-search-tips" },
             "MarkdownBodyPart": { "Markdown": "Use an Azure AI Search index to find the content of your site." },
             "BlogPost": {}
           },
           {
             "ContentType": "BlogPost",
             "ContentItemId": "7b2e5d9a1c4f8e03a6b9d2c7e1",
             "DisplayText": "Planning the winter reading list",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": { "Title": "Planning the winter reading list" },
             "AutoroutePart": { "Path": "planning-the-winter-reading-list" },
             "MarkdownBodyPart": { "Markdown": "Books for the long evenings, in the order of the shelves." },
             "BlogPost": {}
           },
           {
             "ContentType": "BlogPost",
             "ContentItemId": "9c6d3a7e2f1b5084d3e6a9f1c8",
             "DisplayText": "A visit to the orchard in autumn",
             "Latest": true,
             "Published": true,
             "Author": "admin",
             "TitlePart": { "Title": "A visit to the orchard in autumn" },
             "AutoroutePart": { "Path": "a-visit-to-the-orchard-in-autumn" },
             "MarkdownBodyPart": { "Markdown": "Apples, pears and a late harvest." },
             "BlogPost": {}
           }
         ]
       }
     ]
   }
   ```

5. Create the index: Search > Indexes (`OrchardCore.Indexing/AdminMenu.cs:19-22`), "Add index", then the "Add" button
   of the "Content in Azure AI Search" card under "Azure AI Search" (`OrchardCore.AzureAI/Startup.cs:94-95`,
   `OrchardCore.Indexing/Views/Admin/Index.cshtml:31,61`). In the "New 'Content in Azure AI Search' index" page: type
   `blogposts` in "Index Name"; the "Name" field fills itself, so select its text and type `BlogPostsAI`; tick "Blog
   Post" under "Content types"; leave "Index Latest version" unchecked and the other fields as they are; "Save"
   (`OrchardCore.Indexing/Views/IndexProfileFields.Edit.cshtml:6,14,18`, `ContentIndexMetadata.Edit.cshtml:17,31`; the
   check boxes of "Content types" are labeled with the display names of the content types, here "Blog Post":
   `OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml`). The
   message "An index has been created successfully. The synchronizing process was triggered in the background."
   appears (`OrchardCore.Indexing/Controllers/AdminController.cs:257`).
6. Wait until `curl https://localhost:7443/stats` prints `{"default-blogposts":4}` (the three posts and the post of the
   Blog recipe). The stand-in names the index as the site sends it, which is the full name of the index: the tenant
   name in lowercase, a hyphen and the "Index Name" (`OrchardCore.AzureAI.Core/Services/AzureAISearchIndexNameProvider.cs:23-45`,
   `AzureAISearchIndexNamingHelper.cs` `TryGetSafePrefix`), so `default-blogposts` for the tenant `Default`, not
   `blogposts`. The Indexing Worker runs every minute (`ContentIndexingBackgroundTask.cs`), so wait up to three
   minutes. If the number stays at 0, open the stand-in console: a request that fails there tells why.

### 1.6 The permission (between the two clips)

The API clip runs as an anonymous visitor. After `04-create` has created BlogSearch: Access Control > Roles, "Edit"
of the "Anonymous" role (the page title is "Edit 'Anonymous' Role", `OrchardCore.Roles/Views/Admin/Edit.cshtml:6`). In the
table with the columns "Permission", "Allow" and "Effective" (`_RolePermissions.cshtml:68-70`), tick the check box of
the "Allow" column in the row "Execute Api - BlogSearch" (`#Checkbox\.ExecuteApi_BlogSearch`, `_RolePermissions.cshtml:131`;
the field "Search permissions" at line 49 finds the row). The permission is created with the query:
`OrchardCore.Queries/Permissions.cs:33`, `OrchardCore.Queries.Abstractions/QueryPermissions.cs:21-25`. Then click "Save"
(`Edit.cshtml:33`). Check without signing in: a private window on
`http://localhost:5010/api/queries/BlogSearch?parameters=%7B%22terms%22%3A%22orchard%22%7D` shows the JSON, not a 404.
The API answers 404 when the permission is missing (`QueryApiController.cs:52-56`). The permission is also granted to a
role that holds "Manage queries" or "Execute Api - All queries", which imply it (`QueryPermissions.cs:19-25`,
`OrchardCore.Infrastructure/Security/DefaultPermissionGrantingService.cs:37-39`); the narration of `run-4` says so.

## 2. Order and snapshots

1. Do section 1 up to step 6, then stop the site and copy its `App_Data` folder and `state.json` aside (snapshot A:
   the site with the index and the posts, without the query BlogSearch).
2. Record `04-create` from snapshot A. It creates the query BlogSearch: it changes the site.
3. Do section 1.6, then stop the site and copy `App_Data` aside again (snapshot B: with the query and the permission).
4. Record `05-run` from snapshot B. It changes nothing.

To record a clip again, restore its snapshot (stop the site, copy the folder back, start the site), start the
stand-in if it was restarted, and fill it again: Search > Indexes, the actions menu of "BlogPostsAI", "Rebuild",
"Yes", then wait for `{"default-blogposts":4}` at `https://localhost:7443/stats`. Without a restore, `04-create` can also be
repeated after deleting BlogSearch from the Queries list ("Delete").

## 3. Clip `04-create`

Opens `/Admin/Queries/Index` (Search > Queries > All Queries: `OrchardCore.Queries/AdminMenu.cs:18-23`) before the
capture. The list shows the query `RecentBlogPosts` of the Blog recipe. `StartCaptureAsync()`, then:

- `create-1`: click the button "Add Query" (`button[data-bs-target="#modalAddQuery"]`,
  `OrchardCore.Queries/Views/Admin/Index.cshtml:25`). Wait until `#modalAddQuery` is shown. On screen when the
  narration ends: the dialog "Available Query Types" (`Index.cshtml:50`) with the cards "SQL" and "Azure AI Search"
  (hint "Queries an Azure AI Search index.", from the module's `Query-AzureAISearch.Link.cshtml`), and a green
  highlight ring around the "Azure AI Search" card (`HighlightAsync` on `#modalAddQuery .card` that has the heading
  "Azure AI Search").
- `create-2`: `ClearHighlightsAsync()`, then click the "Add" button of the "Azure AI Search" card
  (`ClickAndWaitForNavigationAsync`). Wait for the page "New AzureAISearch query"
  (`OrchardCore.Queries/Views/Admin/Create.cshtml:4`). Type `BlogSearch` into the field labeled "Name"
  (`TypeAsync`, `OrchardCore.Queries/Views/Query.Fields.Edit.cshtml:4`; the field has the focus already). On screen
  when the narration ends: the title "New AzureAISearch query", the "Name" field with `BlogSearch`, and the hint
  "The technical name of the query." (`Query.Fields.Edit.cshtml:8`).
- `create-3`: `ScrollAsync(0, 300)` so that the fields "Index", "Return Content Items" and "Query" are visible (check
  the contact sheet; scroll more or less if one is cut off). Choose `BlogPostsAI` in the drop-down list labeled
  "Index" (`SelectAsync` on `select[name$="Index"]`; label from the module's `AzureAISearchQuery.Edit.cshtml`). On
  screen when the narration ends: "BlogPostsAI" selected in "Index", with its hint "The Azure AI Search index to
  search on."
- `create-4`: click the text area labeled "Query" and type `{{ terms }}` (`TypeAsync`, delay 60). Then
  `HighlightAsync` on the check box "Return Content Items", which stays unchecked. On screen when the narration ends:
  the text area holding `{{ terms }}`, and the ringed, unchecked "Return Content Items" check box.
- `create-5`: `ClearHighlightsAsync()`, then click the button "Save" (`button.save`,
  `OrchardCore.Queries/Views/Query.Fields.Buttons.cshtml:3`) with `ClickAndWaitForNavigationAsync`. The list opens with
  the message "Query created successfully." (`OrchardCore.Queries/Controllers/AdminController.cs:162`). On screen
  when the narration ends: the list with `BlogSearch` and `RecentBlogPosts`, and the message.

## 4. Clip `05-run`

Opens `/Admin/Queries/Index` before the capture (the list shows BlogSearch). `StartCaptureAsync()`, then:

- `run-1`: `NavigateAsync` to `/api/queries/BlogSearch?parameters=%7B%22terms%22%3A%22orchard%22%7D`
  (`OrchardCore.Queries/Controllers/QueryApiController.cs:10,29-33`). Wait until the page text contains
  "Orchard Core search tips". `HighlightAsync` on `body` with the caption
  `GET /api/queries/BlogSearch?parameters={"terms":"orchard"}`. On screen when the narration ends: the JSON of the
  response, with the ringed caption.
- `run-2`: no action, hold. The response lists two items, "Orchard Core search tips" and "A visit to the orchard in
  autumn", each with a content item id and a display text (the property names are not narrated; the way the browser
  shows them may differ, and the contact sheet must show both display texts and no other property of the posts).
- `run-3`: `ClearHighlightsAsync()`, then `NavigateAsync` to
  `/api/queries/BlogSearch?parameters=%7B%22terms%22%3A%22autumn%22%7D`, wait for "A visit to the orchard in autumn",
  and `HighlightAsync` on `body` with the caption `GET /api/queries/BlogSearch?parameters={"terms":"autumn"}`. On
  screen when the narration ends: the JSON with only that post.
- `run-4`: `ClearHighlightsAsync()`, no other action, hold on the same response. The narration describes the
  permission granted in section 1.6, and names the two permissions that imply it ("Manage queries" and "Execute Api -
  All queries"); the screen keeps the one-item response and shows none of them.

## 5. Slides

Drawn by `slides.py` from the code above, nothing to record:

- `queries-1`: three cards, the sources Lucene, Elasticsearch and SQL, with the hint of each card of the Available Query Types dialog.
- `queries-2`: four cards, the ways to run a saved query: Web API, Liquid, Razor, Scripts.
- `queries-3`: four cards, the three `AddQuerySource` registrations and the Azure AI Search registration without one (the fourth is bright).
- `source-1`: the code of `IQuerySource` and `IQueryResults`.
- `source-2`: the first half of `ExecuteQueryAsync`: the metadata and the index profile.
- `source-3`: the second half: the search text, the search service, and the loading of the content items.
- `settings-1`: the metadata class and the query handler.
- `settings-2`: the display driver's `EditAsync`.
- `startup-1`: the manifest and the startup.
- `beyond-1`, `beyond-2`: the vector search sample of the Azure AI Search documentation, with the client factory lines, then the search options lines, bright.

## 6. Private data

- The only people and addresses are `admin`, `admin@example.com` and the fake posts above. The stand-in key is
  `standin-key-0000`. No page that shows a key is recorded: do not open Settings > Search > Azure AI Search.
- The password is not on any screen of the clips. The sign-in is `state.json`, which stays out of git.
- OCR the video at one frame per second and at every scene change, and search for the password, `admin@example.com`
  and `standin-key`. None of them may appear.

## 7. Afterwards

Stop the site and the stand-in by their process ids. Delete the folder `src/OrchardCore.Modules/Acme.AzureAIQueries`,
restore `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj` with `git checkout`, delete `AzureSearchStandIn`, the
copies of `App_Data` and the environment variables.
