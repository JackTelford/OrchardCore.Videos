"""The slides of the Lucene Query Syntax video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, slide_base

TYPES = {"MultiFieldQueryParser", "QueryParser", "ParseException", "LuceneIndexDefaultQueryMetadata", "JsonObject",
         "Query", "LuceneQueryContext", "ILuceneQueryService"}

# LuceneSearchService.SearchAsync, trimmed.
PARSER = """
// ...
if (index.TryGet<LuceneIndexDefaultQueryMetadata>(out var queryMetadata))
{
    defaultVersion = queryMetadata.DefaultVersion;
    defaultSearchFields = queryMetadata.DefaultSearchFields;
}
// ...
var analyzer = _analyzerManager.CreateAnalyzer(analyzerName);
var queryParser = new MultiFieldQueryParser(defaultVersion, defaultSearchFields, analyzer);

try
{
    var query = queryParser.Parse(term);
    result.ContentItemIds = await _searchQueryService.ExecuteQueryAsync(query, index.IndexName, start, size);
    result.Success = true;
}
catch (ParseException e)
{
    _logger.LogError(e, "Incorrect Lucene search query syntax provided in search.");
}
"""

# The query_string example of the Lucene module's documentation.
QUERY_STRING = """
{
  "query": {
    "query_string": {
      "query": "\\"exploration\\"",
      "default_field": "Content.ContentItem.FullText"
    }
  }
}
"""

# QueryStringQueryProvider.CreateQuery, trimmed.
PROVIDER = """
var queryString = query["query"]?.Value<string>();
var defaultField = query["default_field"]?.Value<string>() ?? "";

var queryParser = new QueryParser(
    context.DefaultVersion, defaultField, context.DefaultAnalyzer);
return queryParser.Parse(queryString);
"""

SHAPES = [
    ("1", "Default fields", "Bare words search Content.ContentItem.FullText, unless the index names others."),
    ("2", "Standard analyzer", "Terms are lowercased, and common English words such as and, or and the are dropped."),
    ("3", "Parse errors", "A query that cannot be parsed is logged, and the page lists no results."),
]

FIELDS = [
    ("1", "Display text", "Content.ContentItem.DisplayText.Analyzed"),
    ("2", "Full text", "Content.ContentItem.FullText"),
    ("3", "A content part field", "BlogPost.Subtitle, once Include this element in the index is checked."),
    ("4", "Lowercase title, one value", "Content.ContentItem.DisplayText.Normalized"),
]

EXTRAS = [
    ("1", "Match all", "*:* matches every document in the index, whatever its content type."),
    ("2", "Parse errors", "Run Lucene Query shows an error that starts with Invalid query. The search page lists no results."),
]

HABITS = [
    ("1", "Capital letters", "AND, OR and NOT are operators. and, or and not are dropped as common words."),
    ("2", "Brackets for ranges", "[p TO r] is a range. Without brackets, p and r are two ordinary terms."),
    ("3", "A space before a minus", "quince -pruning excludes a word. quince-pruning does not."),
    ("4", "No leading wildcard", "*ince cannot be parsed, so the page lists no results."),
]


def slide(step_id, chapter_title, label):
    if step_id == "parse-1":
        image = slide_base(chapter_title, label, "How the search page parses a query")
        draw_code(image, (96, 240, W - 96, 1030), "LuceneSearchService.cs", "csharp", PARSER, types=TYPES, size=24)
        return image

    if step_id == "parse-2":
        image = slide_base(chapter_title, label, "Three things shape every search")
        bullets(image, SHAPES)
        return image

    if step_id == "fields-1":
        image = slide_base(chapter_title, label, "Field names in the index")
        bullets(image, FIELDS, active={0, 1, 2})
        return image

    if step_id == "saved-1":
        image = slide_base(chapter_title, label, "The same syntax in a query_string query")
        draw_code(image, (96, 240, W - 96, 600), "A Lucene query", "json", QUERY_STRING, types=TYPES, size=24)
        draw_code(image, (96, 630, W - 96, 1030), "QueryStringQueryProvider.cs", "csharp", PROVIDER, types=TYPES, size=24)
        return image

    if step_id == "saved-2":
        image = slide_base(chapter_title, label, "Match all, and parse errors")
        bullets(image, EXTRAS)
        return image

    if step_id == "habits-1":
        image = slide_base(chapter_title, label, "Four habits that avoid surprises")
        bullets(image, HABITS)
        return image

    raise SystemExit(f"No slide for {step_id}")
