"""The slides of the YesSql indexes video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code comes
from the documentation page "How YesSql works" and from the Orchard Core source, trimmed with // ...
"""
from brand import H, W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "AliasPart", "AliasPartIndex", "ContentItem", "ContentItemIndex", "ContentItemIndexProvider", "Controller",
    "DataMigration", "DescribeContext", "IActionResult", "IServiceCollection", "ISession", "IndexProvider",
    "MapIndex", "Migrations", "MyController", "ProductIndex", "ProductIndexProvider", "ProductPart", "ReduceIndex",
    "StartupBase", "Startup", "Task", "User", "UserByRoleNameIndex", "UserIndex",
}

# The documentation page "How YesSql works", with the Published flag that AliasPartIndex has for the versions.
PRODUCT_INDEX = """
public class ProductIndex : MapIndex
{
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public bool Published { get; set; }
}
"""

PRODUCT_PROVIDER = """
public class ProductIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<ProductIndex>()
            .When(contentItem => contentItem.Has<ProductPart>())
            .Map(contentItem =>
            {
                var part = contentItem.As<ProductPart>();

                return new ProductIndex
                {
                    Sku = part.Sku,
                    Price = part.Price,
                    Published = contentItem.Published,
                };
            });
    }
}
"""

# OrchardCore.Alias/Indexes/AliasPartIndex.cs
ALIAS_INDEX = """
public class AliasPartIndex : MapIndex
{
    public string ContentItemId { get; set; }
    public string Alias { get; set; }
    public bool Latest { get; set; }
    public bool Published { get; set; }
}
"""

ALIAS_MAP = """
context.For<AliasPartIndex>()
    // ...
    .Map(contentItem =>
    {
        // ...
        if (!contentItem.TryGet<AliasPart>(out var part) ||
            string.IsNullOrEmpty(part.Alias))
        {
            return null;
        }

        return new AliasPartIndex
        {
            Alias = part.Alias.ToLowerInvariant(),
            // ...
        };
    });
"""

STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddIndexProvider<ProductIndexProvider>();

        services.AddDataMigration<Migrations>();
    }
}
"""

MIGRATION = """
public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<ProductIndex>(table => table
            .Column<string>("Sku", column => column.WithLength(64))
            .Column<decimal>("Price")
            .Column<bool>("Published")
        );

        await SchemaBuilder.AlterIndexTableAsync<ProductIndex>(table => table
            .CreateIndex("IDX_ProductIndex_DocumentId",
                "DocumentId",
                "Published",
                "Price")
        );

        return 1;
    }
}
"""

CONTROLLER = """
public sealed class MyController : Controller
{
    private readonly ISession _session;

    public MyController(ISession session)
    {
        _session = session;
    }

    public async Task<IActionResult> Cheap()
    {
        // Query documents through an index.
        var cheapProducts = await _session
            .Query<ContentItem, ProductIndex>(index =>
                index.Published && index.Price < 10)
            .ListAsync();

        return View(cheapProducts);
    }
}
"""

# OrchardCore.ContentFields/Services/DefaultUserPickerResultProvider.cs
PICKER = """
var query = _session.Query<User>();

if (!searchContext.DisplayAllUsers)
{
    var roles = searchContext.Roles.Select(x => _roleManager.NormalizeKey(x));
    query.With<UserByRoleNameIndex>(x => x.RoleName.IsIn(roles));
}

if (!string.IsNullOrEmpty(searchContext.Query))
{
    query.With<UserIndex>(x => x.NormalizedUserName.Contains(_userManager.NormalizeName(searchContext.Query)));
}

var users = await query.Take(50).ListAsync();
"""

# OrchardCore.Users.Core/Indexes/UserByRoleNameIndex.cs
REDUCE_INDEX = """
public class UserByRoleNameIndex : ReduceIndex
{
    public string RoleName { get; set; }
    public int Count { get; set; }
}
"""

REDUCE_MAP = """
context.For<UserByRoleNameIndex, string>()
    .Map(user =>
    {
        // ...
        return user.RoleNames.Select(x => new UserByRoleNameIndex
        {
            RoleName = NormalizeKey(x),
            Count = 1,
        });
    })
    .Group(index => index.RoleName)
    // ...
"""

REDUCE_REDUCE = """
context.For<UserByRoleNameIndex, string>()
    // ...
    .Group(index => index.RoleName)
    .Reduce(group => new UserByRoleNameIndex
    {
        RoleName = group.Key,
        Count = group.Sum(x => x.Count),
    })
    .Delete((index, map) =>
    {
        index.Count -= map.Sum(x => x.Count);
        return index.Count > 0 ? index : null;
    });
"""

REDUCE_MIGRATION = """
await SchemaBuilder.CreateReduceIndexTableAsync<UserByRoleNameIndex>(
    table => table
        .Column<string>("RoleName")
        .Column<int>("Count"));
"""

# OrchardCore.ContentManagement/Records/ContentItemIndex.cs
CONTENT_ITEM_PROVIDER = """
public class ContentItemIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<ContentItemIndex>()
            .ExclusiveToDocumentType()
            .SingleRecordPerDocument()
            .Map(contentItem =>
            {
                // ...
            });
    }
}
"""

# OrchardCore.ContentFields/Services/DefaultContentPickerResultProvider.cs
OPT_OUT = """
if (!string.IsNullOrEmpty(searchContext.Query))
{
    query.NoIndexOptimizations();
    query.With<ContentItemIndex>(x =>
        x.DisplayText.Contains(searchContext.Query) ||
        x.ContentType.Contains(searchContext.Query));
}
"""

DOCUMENT_CONTENT = """
{
  "ContentItemId": "4tavbc...",
  "DisplayText": "My blog post",
  // ...
}
"""

QUERYABLE = "Indexes make documents queryable"


def code(image, box, title, source, size=24, highlight=None, language="csharp"):
    draw_code(image, box, title, language, source, types=TYPES, highlight=highlight, size=size)


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Documents in a relational database")
        bullets(image, [
            ("1", "No ORM", "Orchard Core does not use Entity Framework"),
            ("2", "Documents", "Content items, users, settings and workflows are YesSql documents"),
            ("3", "A relational database", "SQL Server, SQLite, MySQL or PostgreSQL"),
        ])
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "A document is a row of JSON")
        code(image, (96, 240, 760, 640), "Content column", DOCUMENT_CONTENT, size=28, language="json")
        note_cards(image, [
            ("Id", "42"),
            ("Type", "OrchardCore.ContentManagement.ContentItem, ..."),
            ("Version", "3"),
        ], (800, 240, W - 96, 900))
        return image

    if step_id in ("why-3", "why-4"):
        image = slide_base(chapter_title, label, QUERYABLE)
        bullets(image, [
            ("1", "A plain class", "The properties you want to query"),
            ("2", "A regular SQL table", "Kept by YesSql, one for each index"),
            ("3", "The same transaction", "Rows are recomputed when a document is created, updated or deleted"),
            ("4", "Only indexed properties", "Everything else stays only in the JSON document"),
        ], active={0, 1} if step_id == "why-3" else {2, 3})
        return image

    if step_id == "map-1":
        image = slide_base(chapter_title, label, "1. The index is a class")
        code(image, (96, 240, 1100, 600), "ProductIndex.cs", PRODUCT_INDEX, size=28)
        note_cards(image, [
            ("MapIndex", "Each row belongs to one document"),
            ("Sku, Price", "The columns you query on"),
            ("Published", "Tells the versions apart"),
        ], (1150, 240, W - 96, 900))
        return image

    if step_id == "map-2":
        image = slide_base(chapter_title, label, "2. The provider fills it")
        code(image, (96, 240, W - 96, 1030), "ProductIndexProvider.cs", PRODUCT_PROVIDER, size=25,
             highlight=set(range(1, 20)) - {15})
        return image

    if step_id == "map-3":
        image = slide_base(chapter_title, label, "3. One row for each version")
        code(image, (96, 240, 1160, 1030), "ProductIndexProvider.cs", PRODUCT_PROVIDER, size=25, highlight={15})
        note_cards(image, [
            ("One document", "For each version of a content item"),
            ("Published", "True for the published version"),
        ], (1200, 240, W - 96, 900))
        return image

    if step_id == "map-4":
        image = slide_base(chapter_title, label, "4. No row for a document")
        code(image, (96, 240, 760, 560), "AliasPartIndex.cs", ALIAS_INDEX, size=22)
        code(image, (800, 240, W - 96, 900), "AliasPartIndex.cs", ALIAS_MAP, size=22,
             highlight={6, 7, 8, 9, 10})
        return image

    if step_id == "setup-1":
        image = slide_base(chapter_title, label, "Register the provider")
        code(image, (96, 240, 1160, 680), "Startup.cs", STARTUP, size=25)
        note_cards(image, [
            ("AddIndexProvider", "A singleton provider"),
            ("AddScopedIndexProvider", "A provider at the session level"),
        ], (1200, 240, W - 96, 900))
        return image

    if step_id in ("setup-2", "setup-3"):
        image = slide_base(chapter_title, label, "Create the table" if step_id == "setup-2" else "Index the columns you filter on")
        lines = {3, 4, 5, 6, 7, 8, 9, 18} if step_id == "setup-2" else {11, 12, 13, 14, 15, 16}
        code(image, (96, 240, W - 96, 1030), "Migrations.cs", MIGRATION, size=24, highlight=lines)
        return image

    if step_id == "query-1":
        image = slide_base(chapter_title, label, "Query through the index")
        code(image, (96, 240, W - 96, 1030), "MyController.cs", CONTROLLER, size=24,
             highlight={13, 14, 15, 16})
        return image

    if step_id == "query-2":
        image = slide_base(chapter_title, label, "The session is a unit of work")
        bullets(image, [
            ("1", "SaveAsync and Delete", "Buffered in memory"),
            ("2", "SaveChangesAsync", "Runs the commands in one transaction"),
            ("3", "The shell scope of the tenant", "Commits the session when it ends"),
            ("4", "Any other scope", "The session is not committed for you"),
        ])
        return image

    if step_id == "query-3":
        image = slide_base(chapter_title, label, "Combine indexes")
        code(image, (96, 240, W - 96, 830), "DefaultUserPickerResultProvider.cs", PICKER, size=24)
        return image

    if step_id == "query-4":
        image = slide_base(chapter_title, label, "ISession or IContentManager")
        bullets(image, [
            ("1", "IContentManager", "Loads content items, with their versions and handlers"),
            ("2", "ISession", "Queries your own indexes"),
        ])
        return image

    if step_id == "reduce-1":
        image = slide_base(chapter_title, label, "1. Map and group")
        code(image, (96, 240, 820, 560), "UserByRoleNameIndex.cs", REDUCE_INDEX, size=26)
        code(image, (860, 240, W - 96, 800), "UserByRoleNameIndex.cs", REDUCE_MAP, size=24)
        return image

    if step_id == "reduce-2":
        image = slide_base(chapter_title, label, "2. Reduce and delete")
        code(image, (96, 240, W - 96, 805), "UserByRoleNameIndex.cs", REDUCE_REDUCE, size=24)
        code(image, (96, 825, W - 96, 1052), "Migrations.cs", REDUCE_MIGRATION, size=24)
        return image

    if step_id == "declare-1":
        image = slide_base(chapter_title, label, "Several records for a document")
        bullets(image, [
            ("1", "A map can emit several rows", "More than one record for the same document"),
            ("2", "Grouped by document", "A query that joins the index returns each document once"),
            ("3", "Every match is read", "A page reads all the matches before it can be sorted"),
        ])
        return image

    if step_id == "declare-2":
        image = slide_base(chapter_title, label, "Declare a single record")
        code(image, (96, 240, 1110, 790), "ContentItemIndex.cs", CONTENT_ITEM_PROVIDER, size=24, highlight={7})
        note_cards(image, [
            ("SingleRecordPerDocument", "At most one record for a document"),
            ("Not grouped", "When a query joins only such indexes"),
            ("Throws", "If the map returns several records"),
        ], (1150, 240, W - 96, 900))
        return image

    if step_id == "declare-3":
        image = slide_base(chapter_title, label, "Declare the document type")
        code(image, (96, 240, 1110, 790), "ContentItemIndex.cs", CONTENT_ITEM_PROVIDER, size=24, highlight={6})
        note_cards(image, [
            ("ExclusiveToDocumentType", "Only this type has records"),
            ("A count", "Reads the index tables only"),
            ("ContentItemIndex", "Declares both"),
        ], (1150, 240, W - 96, 900))
        return image

    if step_id == "declare-4":
        image = slide_base(chapter_title, label, "Opt a query out")
        code(image, (96, 240, 1110, 760), "DefaultContentPickerResultProvider.cs", OPT_OUT, size=24, highlight={3})
        note_cards(image, [
            ("NoIndexOptimizations", "Opts one query out"),
            ("SkipDeduplication", "Skips the grouping for one query"),
        ], (1150, 240, W - 96, 900))
        return image

    raise SystemExit(f"No slide for {step_id}")
