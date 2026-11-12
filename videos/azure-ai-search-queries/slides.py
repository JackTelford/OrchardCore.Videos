"""The slides of the Azure AI Search Queries video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, slide_base

TYPES = {
    "AddQuerySource", "AzureAIClientFactory", "AzureAISearchQueryDisplayDriver", "AzureAISearchQueryHandler",
    "AzureAISearchQueryMetadata", "AzureAISearchQueryResults", "AzureAISearchQuerySource", "AzureAISearchQueryViewModel",
    "BuildEditorContext", "ContentItem", "ContentItemIndex", "DataQueryContextBase", "DisplayDriver", "FluidValue",
    "IDictionary", "IDisplayResult", "IEnumerable", "IIndexProfileStore", "IQueryHandler", "IQueryResults",
    "IQuerySource", "IServiceCollection", "ISearchService", "InitializingQueryContext", "InvalidOperationException",
    "KeyValuePair", "NullEncoder", "Query", "QueryHandlerBase", "SearchDocument", "SearchOptions", "SelectListItem",
    "StartupBase", "Task", "VectorSearchOptions", "VectorizedQuery",
}

SOURCES = [
    ("1", "Lucene", "Queries a Lucene index."),
    ("2", "Elasticsearch", "Queries an Elasticsearch index."),
    ("3", "SQL", "Queries the SQL database."),
]

RUNNERS = [
    ("1", "Web API", "api/queries/{name}, with the parameters as a JSON object."),
    ("2", "Liquid", "Queries.RecentBlogPosts | query, with the parameters as named arguments."),
    ("3", "Razor", "Orchard.QueryAsync and Orchard.ContentQueryAsync."),
    ("4", "Scripts", "executeQuery(name, parameters)."),
]

REGISTRATIONS = [
    ("1", "Lucene", "AddQuerySource<LuceneQuerySource>(LuceneQuerySource.SourceName)"),
    ("2", "Elasticsearch", "AddQuerySource<ElasticsearchQuerySource>(ElasticsearchQuerySource.SourceName)"),
    ("3", "SQL", "AddQuerySource<SqlQuerySource>(SqlQuerySource.SourceName)"),
    ("4", "Azure AI Search", "AddSearchService<AzureAISearchService>(...) for the site search page. No AddQuerySource."),
]

QUERY_SOURCE = """
public interface IQuerySource
{
    string Name { get; }

    Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters);
}
"""

QUERY_RESULTS = """
public interface IQueryResults
{
    IEnumerable<object> Items { get; set; }
}
"""

SOURCE_FIND = """
public sealed class AzureAISearchQuerySource : IQuerySource
{
    public const string SourceName = AzureAISearchConstants.ProviderName;
    // ... constructor and Name

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

        // ...
"""

SOURCE_SEARCH = """
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
"""

METADATA = """
public sealed class AzureAISearchQueryMetadata
{
    public string Index { get; set; }

    public string Template { get; set; }
}
"""

HANDLER = """
public sealed class AzureAISearchQueryHandler : QueryHandlerBase
{
    public override Task InitializingAsync(InitializingQueryContext context)
        => UpdateQueryAsync(context);

    // ... UpdatingAsync does the same

    private static Task UpdateQueryAsync(DataQueryContextBase context)
    {
        // ... copy Index and Template from context.Data, then context.Query.Put(metadata)
    }
}
"""

DRIVER = """
public sealed class AzureAISearchQueryDisplayDriver : DisplayDriver<Query>
{
    // ... constructor: IIndexProfileStore and IStringLocalizer

    public override async Task<IDisplayResult> EditAsync(Query query, BuildEditorContext context)
    {
        // ... returns null unless query.Source is the Azure AI Search source

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

    // ... UpdateAsync validates the fields, and stores them with query.Put(...)
}
"""

MANIFEST = """
[assembly: Module(
    Name = "Azure AI Search Queries",
    Author = "Acme",
    Version = "0.0.1",
    Dependencies = ["OrchardCore.Queries", "OrchardCore.Search", "OrchardCore.AzureAI"],
    Category = "Search"
)]
"""

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddQuerySource<AzureAISearchQuerySource>(AzureAISearchQuerySource.SourceName);
        services.AddDisplayDriver<Query, AzureAISearchQueryDisplayDriver>();
        services.AddScoped<IQueryHandler, AzureAISearchQueryHandler>();
    }
}
"""

VECTOR = """
var index = await _indexProfileStore.FindByIndexNameAndProviderAsync("products", AzureAISearchConstants.ProviderName)
    ?? throw new InvalidOperationException("The 'products' Azure AI Search index profile doesn't exist.");

var client = _clientFactory.CreateSearchClient(index.IndexFullName);

var options = new SearchOptions
{
    Size = 5,
    VectorSearch = new VectorSearchOptions
    {
        Queries =
        {
            new VectorizedQuery(embedding)
            {
                Fields = { "embedding" },
                KNearestNeighborsCount = 5,
            },
        },
    },
};

var response = await client.SearchAsync<SearchDocument>("*", options, cancellationToken);
// ...
"""


def slide(step_id, chapter_title, label):
    if step_id == "queries-1":
        image = slide_base(chapter_title, label, "Saved queries and their sources")
        bullets(image, SOURCES)
        return image

    if step_id == "queries-2":
        image = slide_base(chapter_title, label, "A saved query runs by its name")
        bullets(image, RUNNERS)
        return image

    if step_id == "queries-3":
        image = slide_base(chapter_title, label, "Azure AI Search registers no query source")
        bullets(image, REGISTRATIONS, active={3})
        return image

    if step_id == "source-1":
        image = slide_base(chapter_title, label, "The contract of a query source")
        draw_code(image, (96, 240, W - 96, 590), "IQuerySource.cs", "csharp", QUERY_SOURCE, types=TYPES, size=28)
        draw_code(image, (96, 630, W - 96, 900), "IQueryResults.cs", "csharp", QUERY_RESULTS, types=TYPES, size=28)
        return image

    if step_id == "source-2":
        image = slide_base(chapter_title, label, "1. Find the index of the query")
        draw_code(image, (96, 240, W - 96, 1030), "AzureAISearchQuerySource.cs", "csharp", SOURCE_FIND, types=TYPES, size=22)
        return image

    if step_id == "source-3":
        image = slide_base(chapter_title, label, "2. Search, then load the items")
        draw_code(image, (96, 240, W - 96, 1030), "AzureAISearchQuerySource.cs", "csharp", SOURCE_SEARCH, types=TYPES, size=22)
        return image

    if step_id == "settings-1":
        image = slide_base(chapter_title, label, "The settings and the handler")
        draw_code(image, (96, 240, W - 96, 530), "AzureAISearchQueryMetadata.cs", "csharp", METADATA, types=TYPES, size=22)
        draw_code(image, (96, 560, W - 96, 1030), "AzureAISearchQueryHandler.cs", "csharp", HANDLER, types=TYPES, size=22)
        return image

    if step_id == "settings-2":
        image = slide_base(chapter_title, label, "The editor of the query")
        draw_code(image, (96, 240, W - 96, 1030), "AzureAISearchQueryDisplayDriver.cs", "csharp", DRIVER, types=TYPES, size=22)
        return image

    if step_id == "startup-1":
        image = slide_base(chapter_title, label, "The manifest and the startup")
        draw_code(image, (96, 240, W - 96, 560), "Manifest.cs", "csharp", MANIFEST, types=TYPES, size=22)
        draw_code(image, (96, 590, W - 96, 1000), "Startup.cs", "csharp", STARTUP, types=TYPES, size=22)
        return image

    if step_id in ("beyond-1", "beyond-2"):
        image = slide_base(chapter_title, label, "Vector and hybrid searches")
        lines = {1, 2, 4, 22} if step_id == "beyond-1" else {4, 22} | set(range(6, 21))
        draw_code(image, (96, 240, W - 96, 1045), "ProductVectorSearchService.cs", "csharp", VECTOR, types=TYPES,
                  highlight=lines, size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
