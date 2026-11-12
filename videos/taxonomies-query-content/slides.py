"""The slides of the "Taxonomies in Code: Query Content Items by Term" video. assemble.py calls slide() for each
step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, mono, note_cards, slide_base

TYPES = {
    "ContentField", "ContentItem", "ContentItemIndex", "DefaultContentsTaxonomyListQueryService",
    "DisplayTaxonomyFieldViewModel", "Func", "IContentManager", "IContentsTaxonomyListFilter",
    "IContentsTaxonomyListQueryService", "IEnumerable", "IOrchardHelper", "IQuery", "ISession", "MapIndex", "Pager",
    "TaxonomyField", "TaxonomyIndex", "Task", "TermContentService", "TermPart",
}

# The id used in the examples is invented.
TERM_ID = "4pgm1krsy9nmyre41y5dj2p7df"

TAXONOMY_FIELD = """
public class TaxonomyField : ContentField
{
    public string TaxonomyContentItemId { get; set; }
    public string[] TermContentItemIds { get; set; } = [];
}
"""

TAXONOMY_INDEX = """
public sealed class TaxonomyIndex : MapIndex
{
    public string TaxonomyContentItemId { get; set; }
    public string ContentItemId { get; set; }
    public string ContentType { get; set; }
    public string ContentPart { get; set; }
    public string ContentField { get; set; }
    public string TermContentItemId { get; set; }
    public bool Published { get; set; }
    public bool Latest { get; set; }
}
"""

HELPER = """
public static async Task<IEnumerable<ContentItem>> QueryCategorizedContentItemsAsync(
    this IOrchardHelper orchardHelper,
    Func<IQuery<ContentItem, TaxonomyIndex>, IQuery<ContentItem>> query)
{
    // ...
    var contentItems = await query(session.Query<ContentItem, TaxonomyIndex>()).ListAsync();

    return await contentManager.LoadAsync(contentItems);
}
"""

HELPER_CALL = """
var items = await Orchard.QueryCategorizedContentItemsAsync(
    query => query.Where(index => index.TermContentItemId == termContentItemId));
"""

RELATED = """
@model OrchardCore.Taxonomies.ViewModels.DisplayTaxonomyFieldViewModel
@using YesSql.Services
@{
    var termContentItemIds = Model.TermContentItemIds;
    var contentItems = await Orchard.QueryCategorizedContentItemsAsync(
        query => query.Where(index => index.Published &&
            index.TermContentItemId.IsIn(termContentItemIds) &&
            index.ContentItemId != Model.Part.ContentItem.ContentItemId));

    foreach (var contentItem in contentItems)
    {
        // ...
    }
}
"""

SERVICE = """
public sealed class TermContentService
{
    private readonly ISession _session;
    private readonly IContentManager _contentManager;

    // ...

    public async Task<IEnumerable<ContentItem>> GetByTermAsync(string termContentItemId)
    {
        var contentItems = await _session
            .Query<ContentItem, TaxonomyIndex>(index =>
                index.TermContentItemId == termContentItemId && index.Published)
            .ListAsync();

        return await _contentManager.LoadAsync(contentItems);
    }
}
"""

TEMPLATE = """
{
  "size": 10,
  "query": {
    "bool": {
      "must": [
        { "term": { "Content.ContentItem.ContentType": "BlogPost" } }
        {% if categories %},
        { "match": { "BlogPost.Category.Ids": { "query": "{{ categories }}", "operator": "or" } } }
        {% endif %}
        {% if tags %},
        { "match": { "BlogPost.Tags.Ids": { "query": "{{ tags }}", "operator": "or" } } }
        {% endif %}
      ]
    }
  }
}
"""

RAZOR_CALL = """
@{
    var parameters = new Dictionary<string, object>
    {
        ["categories"] = string.Join(' ', Model.TermContentItemIds),
    };

    var posts = await Orchard.ContentQueryAsync("GetBlogsByFilter", parameters);
}

<ul>
    @foreach (var post in posts)
    {
        <li>@post.DisplayText</li>
    }
</ul>
"""

RAZOR_DISPLAY = """
<ul>
    @foreach (var post in posts)
    {
        <li>@post.DisplayText</li>
    }
</ul>
"""

LIQUID_CALL = """
{% assign posts = Queries.GetBlogsByFilter | query: categories: "__ID__" %}

{% for post in posts %}
  {{ post | display_text }}
{% endfor %}
""".replace("__ID__", TERM_ID)

LIST_SERVICE = """
public async Task<IQuery<ContentItem>> QueryAsync(TermPart termPart, Pager pager)
{
    IQuery<ContentItem> query = _session.Query<ContentItem>()
        .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId);

    await _contentTaxonomyListFilters.InvokeAsync(
        (filter, q, part) => filter.FilterAsync(q, part), query, termPart, _logger);

    // ...
    return query
        .With<ContentItemIndex>()
        // ...
        .Skip((currentPage - 1) * pager.PageSize)
        .Take(pager.PageSize);
}
"""

LIST_FILTER = """
public interface IContentsTaxonomyListFilter
{
    Task FilterAsync(IQuery<ContentItem> query, TermPart termPart);
}
"""

# The margins of a code panel: its text starts 30 px from the left edge and 74 px below the top.
PAD_X, PAD_TOP, PAD_BOTTOM = 30, 74, 26


def height(code, size):
    """The height a code panel needs for this code at this size, as brand.draw_code lays it out."""
    return PAD_TOP + len(code.strip("\n").split("\n")) * int(size * 1.42) + PAD_BOTTOM


def panel(image, x0, y0, x1, title, language, code, size, y1=None, highlight=None):
    """A code panel that is only as tall as its code. It fails when the code would run out of the panel or off the
    slide, so a slide that renders is a slide that fits."""
    y1 = y1 or y0 + height(code, size)
    box = (x0, y0, x1, y1)
    widest = max(mono(size).getlength(line) for line in code.strip("\n").split("\n"))
    if widest + 2 * PAD_X > x1 - x0:
        raise ValueError(f"The code of {title} is {widest:.0f} px wide, and its panel holds {x1 - x0 - 2 * PAD_X}")
    if height(code, size) > y1 - y0 or y1 > 1060:
        raise ValueError(f"The code of {title} is {height(code, size)} px tall, and its panel holds {y1 - y0}")
    draw_code(image, box, title, language, code, types=TYPES, highlight=highlight, size=size)
    return y1


def slide(step_id, chapter_title, label):
    if step_id == "store-1":
        image = slide_base(chapter_title, label, "What a Taxonomy field stores")
        panel(image, 96, 240, 1130, "TaxonomyField.cs", "csharp", TAXONOMY_FIELD, 28)
        note_cards(image, [
            ("TaxonomyContentItemId", "The taxonomy the terms come from"),
            ("TermContentItemIds", "The ids of the selected terms"),
        ], (1170, 240, W - 96, 1030))
        return image

    if step_id == "store-2":
        image = slide_base(chapter_title, label, "The TaxonomyIndex table")
        panel(image, 96, 240, 1010, "TaxonomyIndex.cs", "csharp", TAXONOMY_INDEX, 26)
        note_cards(image, [
            ("One row per term", "Each selected term of each field"),
            ("Published and Latest", "Flags for the versions of an item"),
        ], (1050, 240, W - 96, 1030))
        return image

    if step_id == "sql-1":
        image = slide_base(chapter_title, label, "QueryCategorizedContentItemsAsync")
        bottom = panel(image, 96, 240, W - 96, "TaxonomyOrchardHelperExtensions.cs", "csharp", HELPER, 25)
        panel(image, 96, bottom + 30, W - 96, "A view", "cshtml", HELPER_CALL, 25)
        return image

    if step_id in ("sql-2", "sql-3"):
        image = slide_base(chapter_title, label, "Related items in a TaxonomyField view")
        highlight = {4, 5, 7, 8} if step_id == "sql-2" else {6}
        panel(image, 96, 240, W - 96, "TaxonomyField.cshtml", "cshtml", RELATED, 26, highlight=highlight)
        return image

    if step_id == "sql-4":
        image = slide_base(chapter_title, label, "Only the selected terms have rows")
        bullets(image, [
            ("1", "Selected terms only", "A row exists for each term an editor selected in the field."),
            ("2", "No rows for parents", "An item filed under a child term has no row for its parent term."),
        ])
        return image

    if step_id == "sql-5":
        image = slide_base(chapter_title, label, "The same query on ISession")
        panel(image, 96, 240, W - 96, "TermContentService.cs", "csharp", SERVICE, 26)
        return image

    if step_id == "keys-1":
        image = slide_base(chapter_title, label, "Term ids in a Lucene index")
        bullets(image, [
            ("1", "BlogPost.Category.Ids", "The ids of the selected terms, indexed as keywords and stored."),
            ("2", "BlogPost.Category.Inherited", "The selected terms and all their parents."),
            ("3", "Include this element in the index", "Only a field with this setting writes its terms."),
        ])
        return image

    if step_id in ("template-1", "template-2"):
        image = slide_base(chapter_title, label, "A Lucene query template")
        highlight = {6, 7, 9, 10, 12} if step_id == "template-1" else {5, 8, 11, 13}
        panel(image, 96, 240, W - 96, "GetBlogsByFilter", "json", TEMPLATE, 24, highlight=highlight)
        return image

    if step_id in ("razor-1", "razor-2"):
        image = slide_base(chapter_title, label, "Call the query by name")
        if step_id == "razor-1":
            panel(image, 96, 240, W - 96, "TaxonomyField.cshtml", "cshtml", RAZOR_CALL, 26, highlight=set(range(1, 9)))
        else:
            bottom = panel(image, 96, 240, W - 96, "TaxonomyField.cshtml", "cshtml", RAZOR_DISPLAY, 26)
            panel(image, 96, bottom + 30, W - 96, "TaxonomyField.liquid", "html", LIQUID_CALL, 24)
        return image

    if step_id in ("filter-1", "filter-2"):
        image = slide_base(chapter_title, label, "The query behind a term page")
        first = step_id == "filter-1"
        bottom = panel(image, 96, 240, W - 96, "DefaultContentsTaxonomyListQueryService.cs", "csharp", LIST_SERVICE, 22,
                       highlight={3, 4, 6, 7} if first else set())
        panel(image, 96, bottom + 24, W - 96, "IContentsTaxonomyListFilter.cs", "csharp", LIST_FILTER, 22)
        return image

    raise SystemExit(f"No slide for {step_id}")
