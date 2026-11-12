"""The slides of the Data Sources video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the documentation of the Data Sources feature (src/docs/reference/modules/DataSources/README.md) and trimmed
with // ....
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "Ticket", "TicketState", "ITicketStore", "SupportDataSource", "TicketsDataSet", "SupportPermissions",
    "RecordDataSource", "RecordDataSet", "IRecordDataSet", "IDataSource", "IDataAggregateSource", "DataSetDescriptor",
    "DataSetSchema", "DataSourceContext", "DataSourceQuery", "DataBatch", "DataField", "DataFieldType",
    "DataFieldReference", "DataConditions", "LocalizationSource", "IAuthorizationService", "IStringLocalizer",
    "StartupBase", "IServiceCollection", "RequireFeatures", "Module", "Task", "CancellationToken",
    "IAsyncEnumerable", "IReadOnlyList", "EnumeratorCancellation", "WarehouseDataSource", "ChangedOrder",
    "OrderChangeQueue", "DataSourcesStartup",
}

CONCEPTS = [
    ("1", "Data source", "A connector with a technical name, a label and a description."),
    ("2", "Data set and schema", "One table, with ordered fields: a name, a label and a type such as Text or Date."),
    ("3", "Batches", "Rows are read 500 at a time by default, so any number of rows can be read."),
    ("4", "Read for a user", "A data source lists, describes and reads only the data sets that user may read."),
]

REGISTERED = [
    ("check", "Listed by every consumer", "The pipeline designer shows its data sets to each user who may read them."),
    ("check", "Reported when unavailable", "A step that reads a data source of a disabled feature says it is not available."),
]

FURTHER = [
    ("1", "IDataAggregateSource", "Group and aggregate in your store; answer null to what you can't answer exactly."),
    ("2", "IDataFileFormat", "AddDataFileFormat registers a file format for the file steps."),
    ("3", "Formula functions", "AddExpressionFunction adds a function that formulas can call."),
    ("4", "Documentation", "Data Sources, Extending data sources, on docs.orchardcore.net."),
]

MANIFEST = """
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Support Reports",
    Author = "The Support Team",
    Version = "1.0.0",
    Dependencies =
    [
        "OrchardCore.DataPipelines.Core",
    ]
)]
"""

DATA_SOURCE = """
public sealed record Ticket(string TicketId, TicketState State, DateTimeOffset OpenedUtc, int Priority, string OwnerId);

public sealed class SupportDataSource : RecordDataSource, IDataSource
{
    public SupportDataSource(
        ITicketStore ticketStore,
        IAuthorizationService authorizationService,
        IStringLocalizer<SupportDataSource> localizer)
    {
        DataSets = [new TicketsDataSet(ticketStore, authorizationService, localizer)];
    }

    public static string Name => "Support";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<SupportDataSource>("Support");

    public static LocalizationSource Description { get; } =
        LocalizationSource.Create<SupportDataSource>("The tickets of the help desk.");

    protected override IEnumerable<IRecordDataSet> DataSets { get; }
}
"""

DATA_SET = """
public sealed class TicketsDataSet : RecordDataSet<Ticket>
{
    public TicketsDataSet(ITicketStore ticketStore, IAuthorizationService authorizationService, IStringLocalizer localizer)
        : base(new DataSetDescriptor("Tickets", localizer["Tickets"], localizer["One row per ticket."]))
    {
        // ...
        var ticketId = AddField("TicketId", localizer["Ticket"], DataFieldType.Text, ticket => ticket.TicketId);
        ticketId.IsIdentifier = true;
        ticketId.IsKeyFilterable = true;

        AddField("State", localizer["State"], DataFieldType.Text, ticket => ticket.State);
        AddField("OpenedUtc", localizer["Opened"], DataFieldType.DateTime, ticket => ticket.OpenedUtc);
        AddField("Priority", localizer["Priority"], DataFieldType.Integer, ticket => ticket.Priority);

        // The owner refers to the users, so consumers can suggest to join them.
        var owner = AddField("OwnerId", localizer["Owner"], DataFieldType.Text, ticket => ticket.OwnerId);
        owner.IsIdentifier = true;
        owner.References.Add(new DataFieldReference("Users", "Users", "UserId"));
    }
"""

CAN_READ = """
public override async Task<bool> CanReadAsync(DataSourceContext context, CancellationToken cancellationToken = default)
    => context.User is not null && await _authorizationService.AuthorizeAsync(context.User, SupportPermissions.ViewTickets);
"""

LOAD = """
protected override async IAsyncEnumerable<Ticket> LoadAsync(
    DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
{
    // Apply the conditions the store understands; the consumer applies every condition again.
    var ticketIds = DataConditions.Keys(query.Conditions, "TicketId");
    var (openedFrom, openedTo) = DataConditions.DateRange(query.Conditions, "OpenedUtc");
    var skip = 0;

    while (true)
    {
        var tickets = await _ticketStore.GetTicketsAsync(ticketIds, openedFrom, openedTo, skip, PageSize, cancellationToken);
        if (tickets.Count == 0)
        {
            yield break;
        }

        foreach (var ticket in tickets)
        {
            yield return ticket;
        }
        skip += tickets.Count;
    }
}
"""

STARTUP = """
using OrchardCore.Modules;

[RequireFeatures("OrchardCore.DataPipelines.Core")]
public sealed class DataSourcesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSource<SupportDataSource>();
    }
}
"""

WAREHOUSE = """
public sealed class WarehouseDataSource : IDataSource
{
    public static string Name => "Warehouse";
    // DisplayName and Description ...

    public async Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(
        DataSourceContext context, CancellationToken cancellationToken = default)
        => await CanReadAsync(context) ? [Describe()] : [];

    public async Task<DataSetSchema> GetSchemaAsync(
        string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
    {
        // The schema is the security boundary: return null for a data set the user may not read.
        if (dataSet != StockDataSet || !await CanReadAsync(context))
        {
            return null;
        }

        return new DataSetSchema { DataSet = Describe(), Fields = [.. GetFields()] };
    }
    // ReadAsync ...
}
"""

WAREHOUSE_READ = """
public async IAsyncEnumerable<DataBatch> ReadAsync(
    DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    // Check the access of the user again: a data source yields nothing to a user who may not read the data set.
    if (query.DataSet != StockDataSet || !await CanReadAsync(query.Context))
    {
        yield break;
    }

    var fields = GetFields();
    // ...

    // Each row holds one value per field, in the order of the fields, with the CLR type of the field type.
    var rows = items
        .Select(item => new object[] { item.Sku, item.Name, (long)item.Quantity, item.UnitPrice })
        .ToList();

    yield return new DataBatch(fields, rows);
    // ...
}
"""

CONFIRM = """
protected override async IAsyncEnumerable<ChangedOrder> LoadAsync(
    DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var changes = await _queue.ListAsync(query.Context.ConsumerId, cancellationToken);

    foreach (var change in changes)
    {
        yield return await LoadOrderAsync(change, cancellationToken);
    }

    var ids = changes.Select(change => change.Id).ToArray();

    // Removed once the run delivered them.
    query.Context.OnSucceeded((services, token) => services.GetRequiredService<OrderChangeQueue>().RemoveAsync(ids, token));
}
"""


def lines_with(code, *needles):
    """The 1-based numbers of the lines of a code panel that hold one of the needles."""
    lines = code.strip("\n").split("\n")
    return {number for number, line in enumerate(lines, start=1) if any(needle in line for needle in needles)}


def lines_from(code, needle, count):
    """The 1-based numbers of count lines of a code panel, from the first line that holds the needle."""
    lines = code.strip("\n").split("\n")
    start = next(number for number, line in enumerate(lines, start=1) if needle in line)
    return set(range(start, start + count))


def slide(step_id, chapter_title, label):
    if step_id == "concept-1":
        image = slide_base(chapter_title, label, "How a data source is organized")
        bullets(image, CONCEPTS, active={0})
        return image

    if step_id == "concept-2":
        image = slide_base(chapter_title, label, "How a data source is organized")
        bullets(image, CONCEPTS, active={1})
        return image

    if step_id == "concept-3":
        image = slide_base(chapter_title, label, "How a data source is organized")
        bullets(image, CONCEPTS, active={2, 3})
        return image

    if step_id == "build-1":
        image = slide_base(chapter_title, label, "1. Depend on the Data Pipelines Core feature")
        draw_code(image, (96, 240, 1160, 720), "Manifest.cs", "csharp", MANIFEST, types=TYPES, size=24,
                  highlight=lines_from(MANIFEST, "Dependencies", 4))
        note_cards(image, [
            ("Feature id", "OrchardCore.DataPipelines.Core"),
            ("Enabled by dependency", "Your module enables it."),
        ], (1200, 300, W - 96, 1030))
        return image

    if step_id == "build-2":
        image = slide_base(chapter_title, label, "2. Derive from RecordDataSource")
        draw_code(image, (96, 240, W - 96, 1030), "SupportDataSource.cs", "csharp", DATA_SOURCE, types=TYPES, size=22)
        return image

    if step_id == "build-3":
        image = slide_base(chapter_title, label, "3. Declare the fields of a data set")
        draw_code(image, (96, 240, W - 96, 1030), "TicketsDataSet.cs", "csharp", DATA_SET, types=TYPES, size=24,
                  highlight=lines_with(DATA_SET, "class TicketsDataSet", "AddField(", "DataSetDescriptor"))
        return image

    if step_id == "build-4":
        image = slide_base(chapter_title, label, "4. Mark keys and references")
        draw_code(image, (96, 240, W - 96, 1030), "TicketsDataSet.cs", "csharp", DATA_SET, types=TYPES, size=24,
                  highlight=lines_with(DATA_SET, "IsIdentifier", "IsKeyFilterable", "References", "The owner refers"))
        return image

    if step_id == "build-5":
        image = slide_base(chapter_title, label, "5. Decide who may read it")
        draw_code(image, (96, 240, W - 96, 520), "TicketsDataSet.cs", "csharp", CAN_READ, types=TYPES, size=24)
        note_cards(image, [
            ("Listed, described and read", "only for a user who passes CanReadAsync."),
            ("A published pipeline", "reads as the user who published it."),
        ], (96, 580, W - 96, 1030))
        return image

    if step_id == "build-6":
        image = slide_base(chapter_title, label, "6. Stream the records")
        draw_code(image, (96, 240, W - 96, 1040), "TicketsDataSet.cs", "csharp", LOAD, types=TYPES, size=22,
                  highlight=lines_with(LOAD, "LoadAsync", "DataSourceQuery query", "skip", "while", "GetTicketsAsync",
                                       "yield", "foreach", "tickets.Count"))
        return image

    if step_id == "build-7":
        image = slide_base(chapter_title, label, "7. Apply the conditions you can")
        draw_code(image, (96, 240, W - 96, 1040), "TicketsDataSet.cs", "csharp", LOAD, types=TYPES, size=22,
                  highlight=lines_with(LOAD, "Apply the conditions", "DataConditions"))
        return image

    if step_id == "register-1":
        image = slide_base(chapter_title, label, "Register the data source")
        draw_code(image, (96, 240, 1160, 720), "Startup.cs", "csharp", STARTUP, types=TYPES, size=24)
        note_cards(image, [
            ("Scoped service", "One instance for each scope."),
            ("Unique name", "Two data sources can't share a name."),
        ], (1200, 300, W - 96, 1030))
        return image

    if step_id == "register-2":
        image = slide_base(chapter_title, label, "What happens next")
        bullets(image, REGISTERED)
        return image

    if step_id in ("direct-1", "direct-2"):
        image = slide_base(chapter_title, label, "Implement IDataSource directly")
        if step_id == "direct-1":
            highlight = (lines_with(WAREHOUSE, "class WarehouseDataSource")
                         | lines_from(WAREHOUSE, "Task<IReadOnlyList<DataSetDescriptor>>", 3)
                         | lines_from(WAREHOUSE, "Task<DataSetSchema> GetSchemaAsync", 2))
        else:
            highlight = lines_from(WAREHOUSE, "Task<DataSetSchema> GetSchemaAsync", 11)
        draw_code(image, (96, 240, W - 96, 1040), "WarehouseDataSource.cs", "csharp", WAREHOUSE, types=TYPES, size=22,
                  highlight=highlight)
        return image

    if step_id == "direct-3":
        image = slide_base(chapter_title, label, "ReadAsync yields batches")
        draw_code(image, (96, 240, W - 96, 1030), "WarehouseDataSource.cs", "csharp", WAREHOUSE_READ, types=TYPES, size=24)
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "Confirm what was read")
        draw_code(image, (96, 240, W - 96, 800), "ChangedOrdersDataSet.cs", "csharp", CONFIRM, types=TYPES, size=22)
        note_cards(image, [("ConsumerId", "{pipelineId}/{stepId}; null in a preview.")], (96, 840, 944, 1030))
        note_cards(image, [("OnSucceeded", "runs if every row was read and the run succeeds.")], (976, 840, W - 96, 1030))
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "More extension points")
        bullets(image, FURTHER)
        return image

    raise SystemExit(f"No slide for {step_id}")
