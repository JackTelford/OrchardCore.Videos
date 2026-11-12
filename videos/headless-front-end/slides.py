"""The slides of the "Headless: Serve Content to Any Front End" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The request
and the preflight command follow the HTTP format of the GraphQL documentation and the curl test of the CORS
documentation; the recipe steps follow the Media API recipes of the Media module, with the values of this video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = set()

IDEA = [
    ("1", "Orchard Core", "Stores, edits and publishes the content."),
    ("2", "Your front end", "A separate app that reads the content over HTTP."),
    ("3", "The browser", "Runs the app and decides which sites it may call."),
]

QUESTIONS = [
    ("1", "How does the app read the content?", "GraphQL: one endpoint for queries."),
    ("2", "How does the browser allow the call?", "CORS: policies for the origins you trust."),
    ("3", "How does the app prove who it is?", "OpenID Connect: tokens issued and validated by Orchard Core."),
]

REQUEST_HEADERS = """
POST /api/graphql
Content-Type: application/json
Authorization: Bearer <access token>
"""

REQUEST_BODY = """
{
  "query": "{ blogPost { displayText } }"
}
"""

PREFLIGHT = """
curl -i -X OPTIONS "https://cms.example.com/api/graphql" \\
  -H "Origin: https://app.example.com" \\
  -H "Access-Control-Request-Method: POST" \\
  -H "Access-Control-Request-Headers: authorization,content-type"
"""

FLOW = [
    ("1", "Sign in", "The app sends the visitor to the authorization endpoint."),
    ("2", "Code", "Orchard Core returns a code to the redirect address."),
    ("3", "Token", "The app exchanges the code, and its proof key, at the token endpoint."),
    ("4", "Call", "The app sends the access token in the Authorization header."),
]

RECIPE_FEATURES = """
{
  "steps": [
    {
      "name": "feature",
      "enable": [
        "OrchardCore.Apis.GraphQL",
        "OrchardCore.Cors",
        "OrchardCore.OpenId.Management",
        "OrchardCore.OpenId.Server",
        "OrchardCore.OpenId.Validation"
      ]
    },
    // ...
  ]
}
"""

RECIPE_CORS = """
{
  "name": "settings",
  "CorsSettings": {
    "Policies": [
      {
        "Name": "Frontend",
        "AllowAnyOrigin": false,
        "AllowedOrigins": [ "https://app.example.com" ],
        "AllowAnyHeader": false,
        "AllowedHeaders": [ "Content-Type", "Authorization" ],
        "AllowAnyMethod": false,
        "AllowedMethods": [ "GET", "POST" ],
        "AllowCredentials": false,
        "IsDefaultPolicy": true
      }
    ]
  }
}
"""

RECIPE_VALIDATION = """
{
  "name": "OpenIdValidationSettings",
  "Tenant": "Default"
}
"""

RECIPE_APPLICATION = """
{
  "name": "OpenIdApplication",
  "ClientId": "frontend",
  "DisplayName": "Sample Front End",
  "Type": "public",
  "ConsentType": "explicit",
  "AllowAuthorizationCodeFlow": true,
  "RequireProofKeyForCodeExchange": true,
  "RedirectUris": "https://app.example.com/callback.html",
  "ScopeEntries": [
    { "Name": "profile" },
    { "Name": "roles" }
  ]
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Content in Orchard Core, front end anywhere")
        bullets(image, IDEA)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Three questions, three modules")
        bullets(image, QUESTIONS)
        return image

    if step_id in ("request-1", "request-2"):
        first = step_id == "request-1"
        image = slide_base(chapter_title, label, "A query over HTTP")
        draw_code(image, (96, 240, 1100, 520), "Request", "bash", REQUEST_HEADERS, size=28,
                  highlight={1, 2} if first else {3})
        draw_code(image, (96, 560, 1100, 860), "Body", "json", REQUEST_BODY, size=28,
                  highlight=None if first else set())
        if first:
            notes = [
                ("POST or GET", "JSON body, or the query string."),
                ("One response", "JSON: data, and errors if any."),
                ("Content type", "application/json"),
            ]
        else:
            notes = [
                ("Access token", "Sent in the Authorization header."),
                ("No token", "Runs as an anonymous visitor."),
                ("Permissions", "The caller's roles decide."),
            ]
        note_cards(image, notes, (1140, 260, W - 96, 900))
        return image

    if step_id == "cors-6":
        image = slide_base(chapter_title, label, "Test a preflight request")
        draw_code(image, (96, 240, W - 96, 560), "Terminal", "bash", PREFLIGHT, size=27)
        note_cards(image, [
            ("Preflight", "The browser asks first, with an OPTIONS request."),
            ("Response", "The headers show the origins, methods and headers allowed."),
        ], (96, 620, W - 96, 1030))
        return image

    if step_id == "cors-7":
        image = slide_base(chapter_title, label, "What CORS is, and is not")
        note_cards(image, [
            ("Enforced by browsers", "Server-to-server clients are not restricted by it."),
            ("Not authentication", "Keep sign-in and permissions in place."),
            ("Narrow policies", "Allow only the origins, methods and headers the app needs."),
        ], (96, 260, W - 96, 900))
        return image

    if step_id in ("flow-1", "flow-2"):
        image = slide_base(chapter_title, label, "Authorization code flow with PKCE")
        bullets(image, FLOW, active={0, 1} if step_id == "flow-1" else {2, 3})
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Features and CORS in a recipe")
        draw_code(image, (96, 240, 860, 900), "recipe.json", "json", RECIPE_FEATURES, size=24)
        draw_code(image, (900, 240, W - 96, 960), "recipe.json", "json", RECIPE_CORS, size=22)
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "OpenID Connect in a recipe")
        draw_code(image, (96, 240, 860, 480), "recipe.json", "json", RECIPE_VALIDATION, size=26)
        note_cards(image, [
            ("Tenant", "The tenant that runs the server."),
            ("Order", "After the feature step."),
        ], (96, 540, 860, 1030))
        draw_code(image, (900, 240, W - 96, 900), "recipe.json", "json", RECIPE_APPLICATION, size=24)
        return image

    raise SystemExit(f"No slide for {step_id}")
