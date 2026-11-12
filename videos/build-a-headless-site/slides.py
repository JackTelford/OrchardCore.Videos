"""The slides of the Build a Headless Site video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = set()

IDEA_PARTS = [
    ("1", "Orchard Core", "Stores and manages the content in the admin."),
    ("2", "An HTTP API", "Serves the content to any client that asks for it."),
    ("3", "The front end", "A separate application, on its own address."),
]

IDEA_MODULES = [
    ("1", "GraphQL", "One endpoint answers the requests."),
    ("2", "Queries", "Saved queries with a schema become fields of that endpoint."),
    ("3", "CORS", "Tells browsers which addresses may call the site."),
]

IDEA_CLOSED = [
    ("Execute GraphQL", "Anonymous users do not have it."),
    ("Execute Api - RecentBlogPosts", "One permission for each saved query."),
    ("Execute GraphQL Mutations", "Writing is a separate permission."),
]

REQUESTS = """
POST /api/graphql
Content-Type: application/json

{ "query": "{ blogPost { displayText } }" }

POST /api/graphql
Content-Type: application/graphql

{ blogPost { displayText } }

GET /api/graphql?query={blogPost{displayText}}
"""

REQUEST_NOTES = [
    ("JSON body", "The query is a property."),
    ("Query text body", "The body is the query."),
    ("Query string", "A GET with a query parameter."),
]

SEND = """
// app.js
const endpoint = "http://localhost:5010/api/graphql";
const query = "{ recentBlogPosts { displayText publishedUtc } }";

async function loadPosts() {
  try {
    const response = await fetch(endpoint, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ query }),
    });
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    const { data } = await response.json();
    // ...
  } catch (error) {
    // ...
  }
}
"""

DRAW = """
// app.js
async function loadPosts() {
  try {
    // ...
    for (const post of data.recentBlogPosts) {
      const item = document.createElement("li");
      item.textContent = `${post.displayText} (${post.publishedUtc.slice(0, 10)})`;
      posts.append(item);
    }
  } catch (error) {
    message.textContent = `The request failed: ${error.message}`;
  }
}
"""

MARKUP = """
<!-- ... -->
<button id="load">Load posts</button>
<p id="message"></p>
<ul id="posts"></ul>
<script src="app.js"></script>
"""

CORS_RECIPE = """
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Cors" ] },
    {
      "name": "settings",
      "CorsSettings": {
        "Policies": [
          {
            "Name": "Frontend",
            "AllowedOrigins": [ "https://app.example.com" ],
            "AllowedHeaders": [ "Content-Type" ],
            "AllowedMethods": [ "POST" ],
            "AllowCredentials": false,
            "IsDefaultPolicy": true
          }
        ]
      }
    }
  ]
}
"""

CORS_NOTES = [
    ("Feature step", "Enables OrchardCore.Cors."),
    ("Settings step", "CorsSettings holds the policies."),
    ("Deployment plan", "The Cors settings step exports it."),
]

QUERY_RECIPE = """
{
  "name": "queries",
  "Queries": [
    {
      "Source": "Sql",
      "Name": "RecentBlogPosts",
      "Template": "SELECT DocumentId FROM ContentItemIndex ... LIMIT @limit:3",
      "Schema": "{ \\"type\\": \\"ContentItem/BlogPost\\" }",
      "ReturnContentItems": true
    }
  ]
}
"""

LIMITS = """
{
  "OrchardCore": {
    "Apis": {
      "GraphQL": {
        "ExposeExceptions": false,
        "MaxDepth": 20,
        "MaxComplexity": 200,
        "DefaultNumberOfResults": 20,
        "MaxNumberOfResults": 100,
        "MaxNumberOfResultsValidationMode": "Enabled"
      }
    }
  }
}
"""

LIMIT_NOTES = [
    ("Query size", "MaxDepth and MaxComplexity."),
    ("Result size", "Enabled mode enforces the maximum."),
    ("Exceptions", "Off outside Development."),
]

BEYOND = [
    ("1", "Private content", "The client signs in with a cookie or an OAuth token."),
    ("2", "Writes", "Mutations need Execute GraphQL Mutations."),
    ("3", "REST", "A saved query is served at api/queries/RecentBlogPosts."),
    ("4", "Headless site recipe", "Enables GraphQL, Queries and OpenID at setup."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Content here, front end elsewhere")
        bullets(image, IDEA_PARTS)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Three modules connect them")
        bullets(image, IDEA_MODULES)
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "The API is closed by default")
        note_cards(image, IDEA_CLOSED, (96, 250, W - 96, 1030))
        return image

    if step_id == "front-1":
        image = slide_base(chapter_title, label, "Ways to send a query")
        draw_code(image, (96, 240, 1180, 900), "Requests", "bash", REQUESTS, types=TYPES, size=26)
        note_cards(image, REQUEST_NOTES, (1230, 250, W - 96, 1030))
        return image

    if step_id == "front-2":
        image = slide_base(chapter_title, label, "1. Send the query")
        draw_code(image, (96, 240, W - 96, 1010), "app.js", "javascript", SEND, types=TYPES, size=24)
        return image

    if step_id == "front-3":
        image = slide_base(chapter_title, label, "2. Draw the posts")
        draw_code(image, (96, 240, W - 96, 760), "app.js", "javascript", DRAW, types=TYPES, size=24)
        draw_code(image, (96, 790, W - 96, 1040), "index.html", "html", MARKUP, types=TYPES, size=24)
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "Create the policy with a recipe")
        draw_code(image, (96, 240, 1180, 1000), "recipe.json", "json", CORS_RECIPE, types=TYPES, size=24)
        note_cards(image, CORS_NOTES, (1230, 250, W - 96, 1030))
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "Create a saved query with a recipe")
        draw_code(image, (96, 240, W - 96, 760), "recipe.json", "json", QUERY_RECIPE, types=TYPES, size=26)
        return image

    if step_id == "more-3":
        image = slide_base(chapter_title, label, "Limits for a public endpoint")
        draw_code(image, (96, 240, 1100, 860), "appsettings.json", "json", LIMITS, types=TYPES, size=26)
        note_cards(image, LIMIT_NOTES, (1150, 250, W - 96, 1030))
        return image

    if step_id in ("more-4", "more-5"):
        image = slide_base(chapter_title, label, "Beyond public reads")
        bullets(image, BEYOND, active={0, 1} if step_id == "more-4" else {2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")
