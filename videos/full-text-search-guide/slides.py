"""The slides of the Implement Full-Text Search video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"Startup", "StartupBase", "IServiceCollection", "LuceneOptions", "LuceneAnalyzer", "LuceneConstants",
         "StandardAnalyzer"}

FLOW = [
    ("1", "Content item", "Display text, body parts and custom Liquid make its full-text value."),
    ("2", "Index", "The value is stored in the Content.ContentItem.FullText field."),
    ("3", "Settings and permission", "Site Search picks the index, and a permission lets visitors query it."),
    ("4", "Search page", "/search matches the terms and draws the results with shapes."),
]

OPTIONS = [
    ("1", "Include display text", "The display text of the content item. Selected by default."),
    ("2", "Include body parts", "The text of the body parts. Selected by default."),
    ("3", "Use custom full-text", "The output of a Liquid template is added to the value."),
]

SHAPES = [
    ("1", "Search-List", "Arranges the page: the form, the results and the pager."),
    ("2", "Search-Form", "Draws the search box and the Search button."),
    ("3", "Search-Results", "Draws the matching content items, one after the other."),
    ("4", "Same name in your theme", "A template with the same name overrides the shape."),
]

ANALYZER = """
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Lucene.Services;
using OrchardCore.Modules;
using OrchardCore.Search.Lucene;

namespace OrchardCore.Lucene;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.Configure<LuceneOptions>(o =>
            o.Analyzers.Add(new LuceneAnalyzer(LuceneConstants.DefaultAnalyzer,
                new StandardAnalyzer(LuceneConstants.DefaultVersion))));
        // ...
    }
}
"""

FIELD = """
{{ Model.Content.BlogPost.Subtitle.Text }}
"""

WIDGETS = """
{{ Model.Content.FlowPart.Widgets | full_text }}
"""

RESULTS = """
{% if Model.ContentItems != null and Model.ContentItems.size > 0 %}
    <ul class="list-group">
        {% for item in Model.ContentItems %}
            <li class="list-group-item">
                {% comment %} ... {% endcomment %}
                {{ item | shape_build_display: "SearchSummary" | shape_render }}
            </li>
        {% endfor %}
    </ul>
{% else %}
    <p class="alert alert-warning">{{ "There are no such results." | t }}</p>
{% endif %}
"""

SUMMARY = """
<div class="post-preview">
    {% a display_for:Model.ContentItem %}
        <h2 class="post-title">{{ Model.ContentItem.DisplayText }}</h2>
        <h3 class="post-subtitle">{{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}</h3>
    {% enda %}
</div>
"""

RECIPE_INDEX = """
{ "name": "feature", "enable": [ "OrchardCore.Search", "OrchardCore.Lucene" ] },
{
  "name": "CreateOrUpdateIndexProfile",
  "indexes": [
    {
      "IndexName": "Search",
      "ProviderName": "Lucene",
      "Type": "Content",
      "Name": "Search",
      "Properties": {
        // ...
        "LuceneIndexDefaultQueryMetadata": { "DefaultSearchFields": [ "Content.ContentItem.FullText" ] }
      },
      "IndexedContentTypes": [
        "Article",
        "BlogPost"
        // ...
      ]
    }
  ]
}
"""

RECIPE_ACCESS = """
{
  "name": "Settings",
  "SearchSettings": { "DefaultIndexProfileName": "Search" }
},
{
  "name": "RebuildIndex",
  "includeAll": false,
  "indexNames": [ "Search" ]
},
{
  "name": "Roles",
  "Roles": [
    {
      "Name": "Anonymous",
      "Permissions": [ "ViewContent", "QueryIndex_Search" ]
    }
  ]
}
"""


def slide(step_id, chapter_title, label):
    if step_id in ("flow-1", "flow-2"):
        image = slide_base(chapter_title, label, "From a content item to a results page")
        bullets(image, FLOW, active={0, 1} if step_id == "flow-1" else {2, 3})
        return image

    if step_id == "analyzer-1":
        image = slide_base(chapter_title, label, "Register an analyzer")
        draw_code(image, (96, 240, 1250, 1020), "Startup.cs", "csharp", ANALYZER, types=TYPES, size=24)
        note_cards(image, [
            ("The name", "is listed under Analyzer."),
            ("The instance", "analyzes the text."),
        ], (1300, 260, W - 96, 1000))
        return image

    if step_id == "options-1":
        image = slide_base(chapter_title, label, "The Full-text section of a content type")
        bullets(image, OPTIONS)
        return image

    if step_id == "widgets-1":
        image = slide_base(chapter_title, label, "Liquid in a custom full-text template")
        draw_code(image, (96, 280, 1250, 470), "A field of the content item", "html", FIELD, size=28)
        draw_code(image, (96, 520, 1250, 710), "The widgets of a Flow part", "html", WIDGETS, size=28)
        note_cards(image, [
            ("Model", "is the content item."),
            ("full_text", "adds each item's text."),
        ], (1300, 300, W - 96, 1000))
        return image

    if step_id == "templates-1":
        image = slide_base(chapter_title, label, "The shapes of the search page")
        bullets(image, SHAPES)
        return image

    if step_id == "templates-2":
        image = slide_base(chapter_title, label, "Draw each result with another display type")
        draw_code(image, (96, 240, W - 96, 715), "Search-Results.liquid", "html", RESULTS, size=22)
        draw_code(image, (96, 740, W - 96, 1040), "Content-BlogPost.SearchSummary.liquid", "html", SUMMARY, size=22)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Recipe steps: features and index")
        draw_code(image, (96, 240, W - 96, 1040), "blog.lucene.search.recipe.json", "json", RECIPE_INDEX, size=22)
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "Recipe steps: settings, rebuild and permission")
        draw_code(image, (96, 240, W - 96, 1040), "blog.lucene.search.recipe.json", "json", RECIPE_ACCESS, size=24)
        return image

    raise SystemExit(f"No slide for {step_id}")
