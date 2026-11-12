"""The slides of the data-and-yessql video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py (its colors, fonts, headings, cards and code panels), so the slides look like
those of every other Orchard Core video. The two tables are drawn with ImageDraw and the same colors and fonts.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import (BORDER, DIM, GREEN, MARGIN, MUTED, PANEL, PANEL_DIM, TEXT, W, bullets, draw_code, font, mono,
                   note_cards, slide_base)

TYPES = {"ContentItemIndex", "MapIndex", "ISession", "ContentItem", "Controller", "IActionResult", "PagesController",
         "Task", "IDbConnectionAccessor", "IStore", "CustomTableReader", "CustomRow", "CancellationToken",
         "CommandDefinition", "IReadOnlyList"}

DOCS_CARDS = [
    ("1", "No Entity Framework", "Orchard Core stores its data with YesSql instead."),
    ("2", "A document database interface", "Objects are saved as documents, over a relational database."),
    ("3", "What it stores", "Content items, users, settings and workflows."),
]

DOCUMENT_HEADERS = ["Id", "Type", "Content", "Version"]
DOCUMENT_ROW = [
    "42",
    "OrchardCore.ContentManagement.ContentItem,\nOrchardCore.ContentManagement.Abstractions",
    '{ "ContentItemId": "4tavbc...",\n  "DisplayText": "My blog post",\n  ... }',
    "3",
]

CONTENT_ITEM_INDEX = """
public class ContentItemIndex : MapIndex
{
    // ...
    public long DocumentId { get; set; }
    public string ContentItemId { get; set; }
    public string ContentItemVersionId { get; set; }
    public bool Published { get; set; }
    public bool Latest { get; set; }
    public string ContentType { get; set; }
    // ...
    public string Owner { get; set; }
    public string Author { get; set; }
    public string DisplayText { get; set; }
}
"""

INDEX_KINDS = [
    ("1", "Map index", "Each row maps to one document, like ContentItemIndex."),
    ("2", "Reduce index", "Each row aggregates a group of documents, like a SQL GROUP BY."),
]

SESSION_CARDS = [
    ("1", "SaveAsync and Delete", "Buffered in memory by the session."),
    ("2", "SaveChangesAsync", "Flushes the changes and commits the transaction."),
    ("3", "End of the shell scope", "Orchard Core commits the session; an exception cancels it."),
]

SESSION_QUERY = """
public sealed class PagesController : Controller
{
    private readonly ISession _session;

    public PagesController(ISession session)
    {
        _session = session;
    }

    public async Task<IActionResult> Index()
    {
        // The filter runs as SQL on the index table.
        var pages = await _session
            .Query<ContentItem, ContentItemIndex>(index => index.ContentType == "Page" && index.Published)
            .ListAsync();

        return View(pages);
    }
}
"""

WHICH_ONE = [
    ("1", "IContentManager", "For content items: loading, versions and handlers."),
    ("2", "ISession", "For documents, and for queries on your own indexes."),
]

PROVIDER_HEADERS = ["Setup list", "Provider value", "Connection string", "Prefix and schema"]
PROVIDER_ROWS = [
    ["Sql Server", "SqlConnection", "Required", "Offered"],
    ["Sqlite", "Sqlite", "Not used", "Not offered"],
    ["MySql", "MySql", "Required", "Offered"],
    ["Postgres", "Postgres", "Required", "Offered"],
]

BEFORE_SETUP = [
    ("1", "Create the database", "SQL Server, MySQL and PostgreSQL: before you run Setup."),
    ("2", "Grant permissions", "The account creates and modifies tables."),
    ("3", "Orchard Core does the rest", "It validates the connection, then creates its tables."),
]

TENANT_CONFIGURATION = """
{
  "OrchardCore": {
    "MyTenant": {
      "State": "Uninitialized",
      "ConnectionString": "...",
      "DatabaseProvider": "SqlConnection"
    }
  }
}
"""

SQLITE_CARDS = [
    ("1", "A file per tenant", "In the folder of the tenant, under App_Data and Sites."),
    ("2", "DatabaseName", "The file name. Setup uses OrchardCore.db when none is given."),
    ("3", "Chosen per tenant", "Each tenant can use a different file name."),
]

POOLING = """
{
  "OrchardCore": {
    "Data": {
      "Sqlite": {
        "UseConnectionPooling": false
      }
    }
  }
}
"""

YESSQL_OPTIONS = """
{
  "OrchardCore": {
    "YesSql": {
      "CommandsPageSize": 500,
      "QueryGatingEnabled": true,
      "EnableThreadSafetyChecks": false,
      "IsolationLevel": "ReadCommitted",
      "ConcurrentIndexCreationEnabled": true,
      "IncludeQueryTextInTelemetry": false
    }
  }
}
"""

TABLE_OPTIONS = """
{
  "OrchardCore": {
    "Data": {
      "TableOptions": {
        "DefaultDocumentTable": "Document",
        "DefaultTableNameSeparator": "_",
        "DefaultIdentityColumnSize": "Int64"
      }
    }
  }
}
"""

SQL_CARDS = [
    ("1", "A data migration", "Creates your own tables, so the schema is applied consistently."),
    ("2", "IDbConnectionAccessor", "Creates a connection for the current tenant."),
    ("3", "IStore", "The SQL dialect, the schema and the table prefix."),
]

TABLE_READER = """
public sealed class CustomTableReader
{
    // ... the constructor receives IDbConnectionAccessor and IStore.

    public async Task<IReadOnlyList<CustomRow>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = _dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var configuration = _store.Configuration;
        var tableName = configuration.SqlDialect.QuoteForTableName(
            $"{configuration.TablePrefix}CustomTable",
            configuration.Schema);

        var command = new CommandDefinition(
            $"SELECT * FROM {tableName};",
            cancellationToken: cancellationToken);

        return (await connection.QueryAsync<CustomRow>(command)).AsList();
    }
}
"""

TRANSACTION = """
await using var connection = _dbConnectionAccessor.CreateConnection();
await connection.OpenAsync(cancellationToken);
await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

try
{
    await connection.ExecuteAsync(new CommandDefinition(
        firstCommand, transaction: transaction, cancellationToken: cancellationToken));
    await connection.ExecuteAsync(new CommandDefinition(
        secondCommand, transaction: transaction, cancellationToken: cancellationToken));

    await transaction.CommitAsync(cancellationToken);
}
catch
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
"""


def lines_with(code, *needles):
    """The 1-based numbers of the lines of a code panel that hold one of the needles."""
    lines = code.strip("\n").split("\n")
    return {number for number, line in enumerate(lines, 1) if any(needle in line for needle in needles)}


def table(image, top, headers, rows, widths, fonts, active=None, row_padding=22):
    """A table in the style of the slide cards. fonts: one font per column. A cell may hold several lines.
    active: the indexes of the columns shown bright, the others are dimmed; None shows them all bright."""
    draw = ImageDraw.Draw(image)
    head_font = font(28, "Bold")
    x = MARGIN
    for index, (header, width) in enumerate(zip(headers, widths)):
        on = active is None or index in active
        draw.rounded_rectangle((x, top, x + width - 8, top + 66), radius=12, fill=PANEL if on else PANEL_DIM,
                               outline=GREEN if on else BORDER, width=2)
        draw.text((x + 24, top + 33), header, font=head_font, fill=TEXT if on else DIM, anchor="lm")
        x += width
    y = top + 82
    for row in rows:
        height = max(len(cell.split("\n")) * int(fonts[i].size * 1.45) for i, cell in enumerate(row)) + 2 * row_padding
        x = MARGIN
        for index, (cell, width) in enumerate(zip(row, widths)):
            on = active is None or index in active
            draw.rounded_rectangle((x, y, x + width - 8, y + height), radius=12, fill=PANEL if on else PANEL_DIM,
                                   outline=BORDER if on else PANEL_DIM, width=2)
            line_y = y + row_padding
            for line in cell.split("\n"):
                draw.text((x + 24, line_y), line, font=fonts[index], fill=TEXT if on else DIM)
                line_y += int(fonts[index].size * 1.45)
            x += width
        y += height + 14
    return y


def caption(image, y, lines):
    """Short sentences under a table, each with a green dot."""
    draw = ImageDraw.Draw(image)
    for line in lines:
        draw.ellipse((MARGIN + 4, y - 7, MARGIN + 18, y + 7), fill=GREEN)
        draw.text((MARGIN + 40, y), line, font=font(32), fill=TEXT, anchor="lm")
        y += 58


def document_slide(image, active):
    widths = [120, 640, 790, 178]
    fonts = [font(30), mono(22), mono(22), font(30)]
    bottom = table(image, 250, DOCUMENT_HEADERS, [DOCUMENT_ROW], widths, fonts, active=active)
    return bottom


def slide(step_id, chapter_title, label):
    if step_id == "docs-1":
        image = slide_base(chapter_title, label, "A document database over SQL")
        bullets(image, DOCS_CARDS)
        return image

    if step_id == "docs-2":
        image = slide_base(chapter_title, label, "A document is a row of JSON")
        bottom = document_slide(image, None)
        caption(image, bottom + 50, ["The default document table is named Document.",
                                     "Content holds the object, serialized to JSON."])
        return image

    if step_id == "docs-3":
        image = slide_base(chapter_title, label, "No schema to migrate")
        bottom = document_slide(image, {2})
        caption(image, bottom + 50, ["Adding a field or part changes the JSON in Content.",
                                     "The tables stay as they are."])
        return image

    if step_id in ("index-1", "index-2"):
        image = slide_base(chapter_title, label, "Indexes make documents queryable")
        draw_code(image, (96, 240, 1100, 900), "ContentItemIndex.cs", "csharp", CONTENT_ITEM_INDEX, types=TYPES,
                  size=27)
        notes = [("A plain C# class", "The properties you query on.")]
        if step_id == "index-2":
            notes += [("Regular SQL tables", "YesSql stores the rows there."),
                      ("Same transaction", "Rows follow each save and delete.")]
        note_cards(image, notes, (1150, 240, W - 96, 1030))
        return image

    if step_id == "index-3":
        image = slide_base(chapter_title, label, "Two kinds of index")
        bullets(image, INDEX_KINDS)
        return image

    if step_id in ("session-1", "session-2"):
        image = slide_base(chapter_title, label, "ISession: a unit of work")
        bullets(image, SESSION_CARDS, active={0, 1} if step_id == "session-1" else {2})
        return image

    if step_id == "session-3":
        image = slide_base(chapter_title, label, "Query through an index")
        draw_code(image, (96, 240, W - 96, 1030), "PagesController.cs", "csharp", SESSION_QUERY, types=TYPES, size=26,
                  highlight=lines_with(SESSION_QUERY, "The filter runs", "var pages", ".Query<", ".ListAsync"))
        return image

    if step_id == "session-4":
        image = slide_base(chapter_title, label, "Which one to use")
        bullets(image, WHICH_ONE)
        return image

    if step_id == "providers-1":
        image = slide_base(chapter_title, label, "Database providers")
        widths = [440, 440, 440, 408]
        fonts = [font(32)] * 4
        bottom = table(image, 250, PROVIDER_HEADERS, PROVIDER_ROWS, widths, fonts, row_padding=26)
        caption(image, bottom + 36, ["Sqlite is selected by default."])
        return image

    if step_id == "providers-2":
        image = slide_base(chapter_title, label, "Before you run Setup")
        bullets(image, BEFORE_SETUP)
        return image

    if step_id == "tenant-1":
        image = slide_base(chapter_title, label, "Tenant settings")
        draw_code(image, (96, 240, 1100, 800), "Configuration", "json", TENANT_CONFIGURATION, types=TYPES, size=28)
        note_cards(image, [("Four tenant settings", "Provider, connection string, prefix, schema"),
                           ("Provider and connection string", "When set, Setup doesn't ask for them."),
                           ("Prefix and schema", "Hidden along with them.")], (1150, 240, W - 96, 1030))
        return image

    if step_id == "sqlite-1":
        image = slide_base(chapter_title, label, "SQLite")
        bullets(image, SQLITE_CARDS)
        return image

    if step_id == "sqlite-2":
        image = slide_base(chapter_title, label, "Connection pooling")
        draw_code(image, (96, 240, 1100, 800), "Configuration", "json", POOLING, types=TYPES, size=28,
                  highlight=lines_with(POOLING, "UseConnectionPooling"))
        note_cards(image, [("On by default", "A connection can keep the file open."),
                           ("Off for backups", "Pooling off can reduce performance.")], (1150, 240, W - 96, 1030))
        return image

    if step_id.startswith("options-"):
        image = slide_base(chapter_title, label, "YesSql options")
        highlight = {
            "options-1": lines_with(YESSQL_OPTIONS, "OrchardCore", "YesSql"),
            "options-2": lines_with(YESSQL_OPTIONS, "CommandsPageSize", "QueryGatingEnabled", "IsolationLevel"),
            "options-3": lines_with(YESSQL_OPTIONS, "ConcurrentIndexCreationEnabled"),
            "options-4": lines_with(YESSQL_OPTIONS, "IncludeQueryTextInTelemetry"),
        }[step_id]
        draw_code(image, (96, 240, 1100, 830), "Configuration", "json", YESSQL_OPTIONS, types=TYPES, size=26,
                  highlight=highlight)
        note_cards(image, [("Any tenant source", "Bound to YesSqlOptions."),
                           ("Tenant settings file", "Omit the outer OrchardCore.")], (1150, 240, W - 96, 1030))
        return image

    if step_id in ("tables-1", "tables-2"):
        image = slide_base(chapter_title, label, "Table naming presets")
        highlight = lines_with(TABLE_OPTIONS, "DefaultDocumentTable", "DefaultTableNameSeparator") \
            if step_id == "tables-1" else lines_with(TABLE_OPTIONS, "DefaultTableNameSeparator", "DefaultIdentityColumnSize")
        draw_code(image, (96, 240, 1100, 800), "Configuration", "json", TABLE_OPTIONS, types=TYPES, size=26,
                  highlight=highlight)
        note_cards(image, [("Copied at setup", "The presets become tenant settings."),
                           ("Set them first", "Later changes rename nothing.")], (1150, 240, W - 96, 1030))
        return image

    if step_id == "sql-1":
        image = slide_base(chapter_title, label, "Your own SQL")
        bullets(image, SQL_CARDS)
        return image

    if step_id == "sql-2":
        image = slide_base(chapter_title, label, "Quote the table name")
        draw_code(image, (96, 240, W - 96, 1030), "CustomTableReader.cs", "csharp", TABLE_READER, types=TYPES, size=23,
                  highlight=lines_with(TABLE_READER, "_store.Configuration", "QuoteForTableName", "TablePrefix",
                                       "configuration.Schema"))
        return image

    if step_id == "sql-3":
        image = slide_base(chapter_title, label, "Related writes in a transaction")
        draw_code(image, (96, 240, W - 96, 1030), "CustomTableWriter.cs", "csharp", TRANSACTION, types=TYPES, size=24)
        return image

    raise SystemExit(f"No slide for {step_id}")
