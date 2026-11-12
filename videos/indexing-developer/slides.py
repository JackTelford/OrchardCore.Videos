"""The slides of the Indexing in code video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code of the
Html Body, Alias, Text, Numeric and Content Item Index Coordinator handlers, and the registrations, come from the Orchard
Core source; the Product classes are invented examples built on the same API.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "AliasPart", "AliasPartIndexHandler", "BackgroundTaskAttribute", "BuildDocumentIndexContext", "BuildFieldIndexContext",
    "BuildPartIndexContext", "CancellationToken", "ContentFieldIndexHandler", "ContentItem", "ContentItemIndexCoordinator",
    "ContentPartIndexHandler", "CreateIndexingTaskContext", "DocumentIndex", "DocumentIndexOptions", "HtmlBodyPart",
    "HtmlBodyPartIndexHandler", "IBackgroundTask", "IContentFieldIndexHandler", "IContentPartIndexHandler",
    "IDocumentIndexHandler", "IDocumentIndexManager", "IIndexManager", "IIndexNameProvider", "IIndexingTaskManager",
    "IndexingTaskManager", "IndexProfileEntryContext", "IndexProfileHandlerBase", "IndexProfileResetContext",
    "IndexProfileSynchronizedContext", "IServiceCollection", "IServiceProvider", "NamedIndexingService", "NumericField",
    "NumericFieldIndexHandler", "NumericFieldSettings", "Product", "ProductDocumentIndexManager", "ProductIndexHandler",
    "ProductIndexManager", "ProductIndexNameProvider", "ProductIndexProfileHandler", "ProductIndexingBackgroundTask",
    "ProductIndexingService", "RecordIndexingTask", "RecordIndexingTaskTypes", "Task", "TextField",
    "TextFieldIndexHandler", "LuceneContentIndexSettings", "IndexingOptionsEntry", "LuceneIndexMetadata", "LuceneIndexMap", "Term",
}

FLOW = [
    ("1", "Index profile", "A provider and a type, such as Lucene and Content"),
    ("2", "Task", "An update or a deletion of a record, kept in the task log"),
    ("3", "Document", "Values added by every IDocumentIndexHandler"),
    ("4", "Index", "The provider writes the document to the index"),
]

KEYS = [
    ("1", "The part name", "HtmlBodyPart"),
    ("2", "The content type name, then the part name", "Article.HtmlBodyPart"),
]

HTML_BODY = """
public class HtmlBodyPartIndexHandler : ContentPartIndexHandler<HtmlBodyPart>
{
    public override Task BuildIndexAsync(HtmlBodyPart part, BuildPartIndexContext context)
    {
        var options = context.Settings.ToOptions() | DocumentIndexOptions.Sanitize;

        foreach (var key in context.Keys)
        {
            context.DocumentIndex.Set(key, part.Html, options);
        }

        return Task.CompletedTask;
    }
}
"""

TO_OPTIONS = """
// LuceneContentIndexSettings
public DocumentIndexOptions ToOptions()
{
    var options = DocumentIndexOptions.None;

    if (Stored)
    {
        options |= DocumentIndexOptions.Store;
    }

    if (Keyword)
    {
        options |= DocumentIndexOptions.Keyword;
    }

    return options;
}
"""

ALIAS_OPTIONS = """
// AliasPartIndexHandler ignores the settings of the provider.
var options = DocumentIndexOptions.Keyword | DocumentIndexOptions.Store;
"""

REGISTER_PART = """
services.AddScoped<IContentPartIndexHandler, HtmlBodyPartIndexHandler>();
"""

PART_GATE = """
// ContentItemIndexCoordinator skips a part that is not included,
// unless it is named after the content type.
if (contentTypeDefinition.Name != partTypeName && !typePartIndexSettings.Included)
{
    continue;
}
"""

TEXT_FIELD = """
public class TextFieldIndexHandler : ContentFieldIndexHandler<TextField>
{
    public override Task BuildIndexAsync(TextField field, BuildFieldIndexContext context)
    {
        var options = context.Settings.ToOptions();

        foreach (var key in context.Keys)
        {
            context.DocumentIndex.Set(key, field.Text, options);
        }

        return Task.CompletedTask;
    }
}
"""

NUMERIC_FIELD = """
public class NumericFieldIndexHandler : ContentFieldIndexHandler<NumericField>
{
    public override Task BuildIndexAsync(NumericField field, BuildFieldIndexContext context)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<NumericFieldSettings>();
        var options = context.Settings.ToOptions();
        var isInteger = settings.Scale == 0;

        foreach (var key in context.Keys)
        {
            if (isInteger)
            {
                context.DocumentIndex.Set(key, (int?)field.Value, options);
                continue;
            }

            context.DocumentIndex.Set(key, field.Value, options);
        }

        return Task.CompletedTask;
    }
}
"""

REGISTER_FIELD = """
services.AddScoped<IContentFieldIndexHandler, TextFieldIndexHandler>();
"""

FIELD_GATE = """
// ContentItemIndexCoordinator: a field needs its own Include setting.
if (!partFieldIndexSettings.Included)
{
    continue;
}
"""

DOCUMENT_HANDLER = """
public interface IDocumentIndexHandler
{
    Task BuildIndexAsync(BuildDocumentIndexContext context);

    // ...
}
"""

COORDINATOR = """
public class ContentItemIndexCoordinator : IDocumentIndexHandler
{
    public async Task BuildIndexAsync(BuildDocumentIndexContext context)
    {
        if (context.Record is not ContentItem contentItem)
        {
            return;
        }

        // Calls the part and field index handlers of the content type.
    }
}
"""

PRODUCT_HANDLER = """
public sealed class ProductIndexHandler : IDocumentIndexHandler
{
    public Task BuildIndexAsync(BuildDocumentIndexContext context)
    {
        if (context.Record is not Product product)
        {
            return Task.CompletedTask;
        }

        context.DocumentIndex.Set("Product.Name", product.Name, DocumentIndexOptions.Store);
        context.DocumentIndex.Set("Product.Sku", product.Sku, DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        return Task.CompletedTask;
    }
}
"""

REGISTER_DOCUMENT = """
services.AddScoped<IDocumentIndexHandler, ProductIndexHandler>();
"""

LUCENE_SOURCE = """
services.AddLuceneIndexingSource("Product", o =>
{
    o.DisplayName = S["Products in Lucene"];
    o.Description = S["Creates a Lucene index based on products."];
});
"""

OWN_PROVIDER = """
services.AddIndexingSource<
    ProductIndexManager,
    ProductDocumentIndexManager,
    ProductIndexNameProvider>(
    "Products", // The provider name.
    "Product",  // The type.
    o => { /* DisplayName and Description. */ });
"""

CREATE_TASK = """
await _indexingTaskManager.CreateTaskAsync(
    new CreateIndexingTaskContext(
        product.Id,
        ProductIndexingService.Category,
        RecordIndexingTaskTypes.Update));
"""

INDEXING_SERVICE = """
public sealed class ProductIndexingService : NamedIndexingService
{
    public const string Category = "Product";

    // The constructor passes Category as the name ...

    protected override async Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(
        IndexProfileEntryContext entry, RecordIndexingTask task)
    {
        var product = await _catalog.FindAsync(task.RecordId);

        if (product is null)
        {
            return null;
        }

        return new BuildDocumentIndexContext(
            new DocumentIndex(product.Id), product, [Category],
            entry.DocumentIndexManager.GetContentIndexSettings());
    }
}
"""

PROFILE_HANDLER = """
public sealed class ProductIndexProfileHandler : IndexProfileHandlerBase
{
    public override Task SynchronizedAsync(IndexProfileSynchronizedContext context)
        => context.IndexProfile.Type == ProductIndexingService.Category
            ? _indexingService.ProcessRecordsAsync([context.IndexProfile.Id])
            : Task.CompletedTask;

    public override Task ResetAsync(IndexProfileResetContext context)
        => context.IndexProfile.Type == ProductIndexingService.Category
            ? _serviceProvider
                .GetRequiredKeyedService<IDocumentIndexManager>(context.IndexProfile.ProviderName)
                .SetLastTaskIdAsync(context.IndexProfile, 0)
            : Task.CompletedTask;
}
"""

REGISTER_SOURCE = """
services.AddScoped<ProductIndexingService>();
services.AddIndexProfileHandler<ProductIndexProfileHandler>();
services.AddSingleton<IBackgroundTask, ProductIndexingBackgroundTask>();
"""

REGISTER_CARDS = [
    ("1", "Indexing service", "A scoped service"),
    ("2", "Profile handler", "Added with AddIndexProfileHandler"),
    ("3", "Background task", "An IBackgroundTask singleton"),
]

BACKGROUND_TASK = """
[BackgroundTask(Title = "Products Indexer", Schedule = "* * * * *")]
public sealed class ProductIndexingBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        => serviceProvider.GetRequiredService<ProductIndexingService>()
            .ProcessRecordsForAllIndexesAsync();
}
"""


LUCENE_MAPPINGS = """
// LuceneContentIndexProfileHandler, for the Content type only:
var LuceneMetadata = index.GetOrCreate<LuceneIndexMetadata>();

var map = new LuceneIndexMap()
{
    KeyFieldName = ContentIndexingConstants.ContentItemIdKey,
};

// ...
LuceneMetadata.IndexMappings = map;
"""

LUCENE_KEY = """
// LuceneIndexManager.AddOrUpdateDocumentsAsync
writer.UpdateDocument(new Term(metadata.IndexMappings.KeyFieldName, document.Id), /* ... */);
"""


def marks(code, *needles):
    """The 1-based numbers of the lines of code that hold any of the needles, to highlight them."""
    lines = code.strip("\n").split("\n")
    return {number for number, line in enumerate(lines, 1) if any(needle in line for needle in needles)}


def slide(step_id, chapter_title, label):
    if step_id.startswith("how-"):
        active = {"how-1": {0}, "how-2": {1, 2, 3}, "how-3": {1, 2}}[step_id]
        image = slide_base(chapter_title, label, "From a change to an index")
        bullets(image, FLOW, active=active)
        return image

    if step_id == "part-1":
        image = slide_base(chapter_title, label, "A handler for the Html Body part")
        draw_code(image, (96, 240, W - 96, 900), "HtmlBodyPartIndexHandler.cs", "csharp", HTML_BODY, types=TYPES, size=28)
        return image

    if step_id == "part-2":
        image = slide_base(chapter_title, label, "The keys of a part")
        bullets(image, KEYS)
        return image

    if step_id == "part-3":
        image = slide_base(chapter_title, label, "Options from the settings of the provider")
        draw_code(image, (96, 240, W - 96, 850), "LuceneContentIndexSettings.cs", "csharp", TO_OPTIONS, types=TYPES, size=22)
        draw_code(image, (96, 880, W - 96, 1040), "AliasPartIndexHandler.cs", "csharp", ALIAS_OPTIONS, types=TYPES, size=22)
        return image

    if step_id == "part-4":
        image = slide_base(chapter_title, label, "Register it, and include the part")
        draw_code(image, (96, 240, W - 96, 400), "Startup.cs", "csharp", REGISTER_PART, types=TYPES, size=28)
        draw_code(image, (96, 440, W - 96, 870), "ContentItemIndexCoordinator.cs", "csharp", PART_GATE, types=TYPES, size=26)
        return image

    if step_id == "field-1":
        image = slide_base(chapter_title, label, "A handler for the Text field")
        draw_code(image, (96, 240, W - 96, 900), "TextFieldIndexHandler.cs", "csharp", TEXT_FIELD, types=TYPES, size=28)
        return image

    if step_id == "field-2":
        image = slide_base(chapter_title, label, "The settings of the field definition")
        draw_code(image, (96, 240, W - 96, 1040), "NumericFieldIndexHandler.cs", "csharp", NUMERIC_FIELD, types=TYPES, size=22,
                  highlight=marks(NUMERIC_FIELD, "var settings", "var isInteger", "if (isInteger)", "(int?)field.Value",
                                  "Set(key, field.Value", "continue;"))
        return image

    if step_id == "field-3":
        image = slide_base(chapter_title, label, "Register it, and include the field")
        draw_code(image, (96, 240, W - 96, 400), "Startup.cs", "csharp", REGISTER_FIELD, types=TYPES, size=28)
        draw_code(image, (96, 440, W - 96, 760), "ContentItemIndexCoordinator.cs", "csharp", FIELD_GATE, types=TYPES, size=26)
        return image

    if step_id == "record-1":
        image = slide_base(chapter_title, label, "One interface for every document")
        draw_code(image, (96, 240, W - 96, 525), "IDocumentIndexHandler.cs", "csharp", DOCUMENT_HANDLER, types=TYPES, size=24)
        draw_code(image, (96, 555, W - 96, 1020), "ContentItemIndexCoordinator.cs", "csharp", COORDINATOR, types=TYPES, size=22)
        return image

    if step_id == "record-2":
        image = slide_base(chapter_title, label, "A handler for your own records")
        draw_code(image, (96, 240, W - 96, 800), "ProductIndexHandler.cs", "csharp", PRODUCT_HANDLER, types=TYPES, size=22)
        draw_code(image, (96, 840, W - 96, 1000), "Startup.cs", "csharp", REGISTER_DOCUMENT, types=TYPES, size=26)
        return image

    if step_id == "source-1":
        image = slide_base(chapter_title, label, "A source for a built-in provider")
        draw_code(image, (96, 240, 1156, 700), "Startup.cs", "csharp", LUCENE_SOURCE, types=TYPES, size=24)
        note_cards(image, [
            ("Provider", "Lucene, for example"),
            ("Type", "The source: Product"),
            ("Add index", "Lists name and description"),
        ], (1196, 240, W - 96, 1040))
        return image

    if step_id == "source-2":
        image = slide_base(chapter_title, label, "A provider of your own")
        draw_code(image, (96, 240, 1156, 700), "Startup.cs", "csharp", OWN_PROVIDER, types=TYPES, size=24)
        note_cards(image, [
            ("IIndexManager", "Creates, rebuilds, deletes"),
            ("IDocumentIndexManager", "Writes documents, task id"),
            ("IIndexNameProvider", "Builds the full name"),
        ], (1196, 240, W - 96, 1040))
        return image

    if step_id == "source-3":
        image = slide_base(chapter_title, label, "Record a change in the task log")
        draw_code(image, (96, 240, 1156, 620), "ProductService.cs", "csharp", CREATE_TASK, types=TYPES, size=24)
        note_cards(image, [
            ("Record id", "Up to 26 characters"),
            ("Category", "Up to 50 characters"),
        ], (1196, 240, W - 96, 1040))
        return image

    if step_id == "source-4":
        image = slide_base(chapter_title, label, "Process the tasks of your category")
        draw_code(image, (96, 240, W - 96, 1000), "ProductIndexingService.cs", "csharp", INDEXING_SERVICE, types=TYPES, size=22)
        return image

    if step_id == "source-5":
        image = slide_base(chapter_title, label, "Run it, and tie it to the index profile")
        draw_code(image, (96, 240, W - 96, 720), "ProductIndexProfileHandler.cs", "csharp", PROFILE_HANDLER, types=TYPES, size=21)
        draw_code(image, (96, 750, W - 96, 1030), "ProductIndexingBackgroundTask.cs", "csharp", BACKGROUND_TASK, types=TYPES, size=21)
        return image

    if step_id == "source-6":
        image = slide_base(chapter_title, label, "Register the three classes")
        draw_code(image, (96, 240, W - 96, 480), "Startup.cs", "csharp", REGISTER_SOURCE, types=TYPES, size=28)
        bullets(image, REGISTER_CARDS, top=520)
        return image

    if step_id == "source-7":
        image = slide_base(chapter_title, label, "Check what the provider reads")
        draw_code(image, (96, 240, W - 96, 620), "LuceneContentIndexProfileHandler.cs", "csharp", LUCENE_MAPPINGS, types=TYPES, size=22)
        draw_code(image, (96, 660, W - 96, 830), "LuceneIndexManager.cs", "csharp", LUCENE_KEY, types=TYPES, size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
