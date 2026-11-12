"""The slides of the GraphiQL video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

IDEAS = [
    ("1", "A page of the admin", "Tools, GraphiQL, when the GraphQL feature is enabled"),
    ("2", "The same endpoint", "Queries go to /api/graphql, the path your applications call"),
    ("3", "Your session", "Queries run as the user who is signed in"),
]

# The body GraphiQL posts, in the form the documentation describes: query, operationName and variables.
REQUEST = """
{
  "query": "query Posts($first: Int) { blogPost(first: $first) { displayText } }",
  "operationName": "Posts",
  "variables": { "first": 2 }
}
"""

RESPONSE = """
{
  "data": {
    "blogPost": [
      { "displayText": "A field guide to quiet places" },
      { "displayText": "Notes from the launch pad" }
    ]
  }
}
"""

# The address of the page after GraphiQL opens such a query: encodeURIComponent of the query and of the variables.
ADDRESS = """
# Line breaks added for reading.
https://example.com/Admin/GraphQL
  ?query=query%20Posts(%24first%3A%20Int)
  %20%7B%20blogPost(first%3A%20%24first)
  %20%7B%20displayText%20%7D%20%7D
  &variables=%7B%22first%22%3A2%7D
"""

ADDRESS_NOTES = [
    ("query", "The text of the query"),
    ("variables", "The values, as JSON"),
    ("operationName", "Which operation to run"),
]

# From OrchardCore.Apis.GraphQL, Assets/js/App.tsx.
FETCHER = """
function fetcher(params: object): Promise<{ data?: unknown }> {
    const introspectionUrl = getIntrospectionUrl();
    return fetch(
        introspectionUrl,
        {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            body: JSON.stringify(params),
            credentials: 'include'
        }
    )
    // ...
}
"""

FETCHER_NOTES = [
    ("POST", "The query goes in the body"),
    ("credentials", "Your session is included"),
    ("Any status", "The body is always shown"),
]

CLIENT = """
curl -X POST https://example.com/api/graphql \\
  -H "Content-Type: application/json" \\
  -H "Authorization: Bearer <access-token>" \\
  -d '{ "query": "{ blogPost { displayText } }" }'
"""

CLIENT_NOTES = [
    ("Credentials", "Cookie or access token"),
    ("Permission", "Execute GraphQL"),
]

STATUS_NOTES = [
    ("200", "A query without errors"),
    ("400", "A query that fails"),
    ("401", "A permission is missing"),
]

CODE_BOX = (96, 240, 1190, 860)
NOTES_BOX = (1230, 240, W - 96, 1030)


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "One page, one endpoint")
        bullets(image, IDEAS)
        return image
    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "One run is one request")
        draw_code(image, (96, 240, W - 96, 560), "POST /api/graphql", "json", REQUEST, size=24)
        draw_code(image, (96, 600, W - 96, 1030), "The response", "json", RESPONSE, size=24)
        return image
    if step_id == "vars-1":
        image = slide_base(chapter_title, label, "The address keeps your work")
        draw_code(image, CODE_BOX, "Address", "bash", ADDRESS, size=24)
        note_cards(image, ADDRESS_NOTES, NOTES_BOX)
        return image
    if step_id == "code-1":
        image = slide_base(chapter_title, label, "What the play button does")
        draw_code(image, (96, 240, 1190, 920), "App.tsx", "javascript", FETCHER, types={"Promise"}, size=24)
        note_cards(image, FETCHER_NOTES, NOTES_BOX)
        return image
    if step_id == "code-2":
        image = slide_base(chapter_title, label, "The same request from your code")
        draw_code(image, (96, 240, 1190, 640), "Terminal", "bash", CLIENT, size=26)
        note_cards(image, CLIENT_NOTES, NOTES_BOX)
        return image
    if step_id == "code-3":
        image = slide_base(chapter_title, label, "The HTTP status")
        draw_code(image, (96, 240, 1190, 640), "Terminal", "bash", CLIENT, size=26)
        note_cards(image, STATUS_NOTES, NOTES_BOX)
        return image
    raise SystemExit(f"No slide for {step_id}")
