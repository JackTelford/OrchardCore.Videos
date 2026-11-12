"""The slides of the Lucene and Elasticsearch queries video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The queries are those of the Blog theme (TheBlogTheme/Recipes/Snippets), laid out on fewer lines.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

WHY_INDEX = [
    ("1", "An index answers a JSON query", "You send the question, and the index returns documents."),
    ("2", "Every caller keeps a copy", "Templates, scripts and clients each hold the JSON."),
    ("3", "A saved query keeps one copy", "One name, one template, one place to change it."),
]

WHY_SOURCES = [
    ("1", "Lucene", "Runs inside your site. No other service."),
    ("2", "Elasticsearch", "Runs on a server that your site connects to."),
    ("check", "The same fields", "Index, Return Content Items and Query."),
]

RESULTS = [
    ("1", "Unchecked: documents", "Lucene: the stored fields. Elasticsearch: the source data."),
    ("2", "The Run pages", "Always show documents, never content items."),
    ("3", "Checked: content items", "Loaded from the database by version ID, in the order of the results."),
    ("4", "Elasticsearch", "Reads that ID from the source data: Store Source Data."),
]

BLOG_RECIPES = [
    ("1", "Blog - Lucene Query", "Replaces RecentBlogPosts with a Lucene query."),
    ("2", "Blog - Elasticsearch Query", "Replaces RecentBlogPosts with an Elasticsearch query."),
    ("check", "Examples to read", "Each one reads its Template from a file."),
]

LUCENE_QUERY = """
{
  "query": {
    "bool": {
      "must": [
        { "term": { "Content.ContentItem.ContentType": "BlogPost" } },
        { "term": { "Content.ContentItem.Published": "true" } }
      ]
    }
  },
  "sort": [
    { "Content.ContentItem.CreatedUtc": { "order": "desc" } }
  ],
  "size": 3
}
"""

LUCENE_TEMPLATE = """
{
  "query": {
    "bool": {
      "must": [
        { "term": { "Content.ContentItem.ContentType": "BlogPost" } },
        { "term": { "Content.ContentItem.Published": "true" } }
      ]
    }
  },
  "sort": [
    { "Content.ContentItem.CreatedUtc": { "order": "desc" } }
  ],
  "size": {{ limit | default: 3 }}
}
"""

LUCENE_DIFFERENCES = """
{
  "query": {
    "bool": {
      "must": [
        // ...
        { "term": { "Content.ContentItem.Published": "true" } }
      ]
    }
  },
  "sort": [
    { "Content.ContentItem.CreatedUtc": { "order": "desc" } }
  ],
  "size": 3
}
"""

ELASTICSEARCH_DIFFERENCES = """
{
  "query": {
    "bool": {
      "must": [
        // ...
        { "term": { "Content.ContentItem.Published": true } }
      ]
    }
  },
  "sort": [
    { "Content.ContentItem.CreatedUtc": "desc" }
  ],
  "size": 3
}
"""

CONNECTION = """
{
  "OrchardCore": {
    "Search": {
      "Elasticsearch": {
        "ConnectionType": "SingleNodeConnectionPool",
        "Url": "http://localhost",
        "Ports": [
          9200
        ]
        // ...
      }
    }
  }
}
"""

RECIPE = """
{
  "steps": [
    {
      "name": "queries",
      "Queries": [
        {
          "Source": "Lucene",
          "Name": "RecentBlogPosts",
          "Index": "Search",
          "Template": "[file:text('Snippets/recentBlogPosts.Lucene.json')]",
          // ...
          "ReturnContentItems": true
        }
      ]
    }
  ]
}
"""

SCHEMA = """
{
    "type": "ContentItem/BlogPost"
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "One search question, one saved copy")
        bullets(image, WHY_INDEX)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Two query sources")
        bullets(image, WHY_SOURCES)
        return image

    if step_id == "tmpl-1":
        image = slide_base(chapter_title, label, "Published blog posts, by creation date")
        draw_code(image, (96, 240, W - 96, 860), "recentBlogPosts.Lucene.json", "json", LUCENE_QUERY, size=26)
        return image

    if step_id == "tmpl-2":
        image = slide_base(chapter_title, label, "A parameter with a default")
        draw_code(image, (96, 240, 1250, 860), "Template", "json", LUCENE_TEMPLATE, highlight={13}, size=22)
        note_cards(image, [
            ("limit", "The value the caller passes."),
            ("default: 3", "Used when none is passed."),
        ], (1310, 300, W - 96, 760))
        return image

    if step_id == "docs-1":
        image = slide_base(chapter_title, label, "What Return Content Items changes")
        bullets(image, RESULTS, active={0, 1})
        return image

    if step_id == "docs-2":
        image = slide_base(chapter_title, label, "What Return Content Items changes")
        bullets(image, RESULTS, active={2, 3})
        return image

    if step_id == "connect-1":
        image = slide_base(chapter_title, label, "Connect the site to Elasticsearch")
        draw_code(image, (96, 240, 1250, 860), "appsettings.json", "json", CONNECTION, size=26)
        note_cards(image, [
            ("Url", "Required for the menu entry."),
            ("Ports", "9200 when left out."),
        ], (1310, 300, W - 96, 760))
        return image

    if step_id == "differ-1":
        image = slide_base(chapter_title, label, "The same query for each source")
        draw_code(image, (96, 240, 936, 780), "recentBlogPosts.Lucene.json", "json", LUCENE_DIFFERENCES,
                  highlight={6, 11}, size=22)
        draw_code(image, (984, 240, W - 96, 780), "recentBlogPosts.Elasticsearch.json", "json",
                  ELASTICSEARCH_DIFFERENCES, highlight={6, 11}, size=22)
        draw = ImageDraw.Draw(image)
        draw.ellipse((100, 872, 118, 890), fill=GREEN)
        draw.text((136, 881), "The bright lines are the ones that differ.", font=font(30), fill=TEXT, anchor="lm")
        draw.text((96, 960), "Lucene: Published is a string, and the sort is an object with an order.", font=font(28),
                  fill=MUTED, anchor="lm")
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "The Queries recipe step")
        draw_code(image, (96, 240, W - 96, 940), "blog.lucene.query.recipe.json", "json", RECIPE, size=24)
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "Recipes of the Blog theme")
        bullets(image, BLOG_RECIPES)
        return image

    if step_id == "recipe-3":
        image = slide_base(chapter_title, label, "The Schema of a query")
        draw_code(image, (96, 240, 1000, 500), "Schema", "json", SCHEMA, size=28)
        note_cards(image, [
            ("Optional", "Empty means no GraphQL field."),
            ("Content items", "ContentItem/ and the type name."),
        ], (1060, 300, W - 96, 760))
        return image

    raise SystemExit(f"No slide for {step_id}")
