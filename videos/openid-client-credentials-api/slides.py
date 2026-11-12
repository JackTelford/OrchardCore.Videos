"""The slides of the "Call the Orchard Core API With Client Credentials" video. assemble.py calls slide() for each
step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"HttpClient", "FormUrlEncodedContent", "JsonElement", "AuthenticationHeaderValue", "StringContent",
         "Encoding", "Uri", "Environment", "Console", "JsonSerializer", "Dictionary"}

ONE_PROGRAM = [
    ("1", "Runs unattended", "A scheduled import, a build step or another service."),
    ("2", "Signs in as itself", "As an application that you register on your site."),
    ("3", "Proves it with a secret", "A client ID and a client secret, instead of a password."),
]

FLOW = [
    ("1", "Ask for a token", "The program sends its client ID and secret to the token endpoint."),
    ("2", "Receive an access token", "The site answers with a token for that application."),
    ("3", "Call the API", "The token goes in the Authorization header, as a bearer token."),
    ("4", "Roles set the access", "The permissions of the application's roles are copied into the token."),
]

TOKEN = """
using System.Net.Http.Json;
using System.Text.Json;

using var http = new HttpClient { BaseAddress = new Uri("https://cms.example.com/") };
var secret = Environment.GetEnvironmentVariable("CLIENT_SECRET")!;

var response = await http.PostAsync("connect/token", new FormUrlEncodedContent(
    new Dictionary<string, string>
    {
        ["grant_type"] = "client_credentials",
        ["client_id"] = "content-sync",
        ["client_secret"] = secret,
    }));
response.EnsureSuccessStatusCode();

var token = await response.Content.ReadFromJsonAsync<JsonElement>();
var accessToken = token.GetProperty("access_token").GetString();
"""

CREATE = """
using System.Net.Http.Headers;
using System.Text;
// ...

http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", accessToken);

var body = JsonSerializer.Serialize(new
{
    ContentType = "Article",
    TitlePart = new { Title = "Hello from the sync job" },
});

var created = await http.PostAsync("api/content",
    new StringContent(body, Encoding.UTF8, "application/json"));
created.EnsureSuccessStatusCode();
"""

BODY = """
{
  "ContentType": "Article",
  "TitlePart": { "Title": "Hello from the sync job" }
}
"""

READ_DELETE = """
var item = await created.Content.ReadFromJsonAsync<JsonElement>();
var id = item.GetProperty("ContentItemId").GetString();

// Read it back.
var again = await http.GetFromJsonAsync<JsonElement>($"api/content/{id}");
Console.WriteLine(again.GetProperty("DisplayText").GetString());

// Delete it. This needs Delete content for others.
var deleted = await http.DeleteAsync($"api/content/{id}");
Console.WriteLine(deleted.StatusCode);
"""

PERMISSIONS = [
    ("1", "Read: GET api/content/{contentItemId}",
     "Access content via the api, and View all content"),
    ("2", "Create: POST api/content",
     "Access content via the api, and Publish or unpublish content for others"),
    ("3", "Update: POST api/content with a ContentItemId",
     "Access content via the api, and Edit content for others"),
    ("4", "Delete: DELETE api/content/{contentItemId}",
     "Access content via the api, and Delete content for others"),
]

RESPONSES = [
    ("1", "401 Unauthorized", "No accepted token. Check Token Validation, then request another token."),
    ("2", "403 Forbidden", "A valid token, but its role lacks a permission, such as Access content via the api."),
    ("3", "404 Not Found, 400 Bad Request", "Unknown item, unknown content type, or a body that fails validation."),
]

TOKENS = [
    ("1", "Role changes do not reach issued tokens", "Request another token to pick up the changed permissions."),
    ("2", "Token request refused", "Allow Client Credentials Flow must be checked in the server settings and in the application."),
]


def slide(step_id, chapter_title, label):
    if step_id == "flow-1":
        image = slide_base(chapter_title, label, "A program, not a person")
        bullets(image, ONE_PROGRAM)
        return image

    if step_id in ("flow-2", "flow-3"):
        image = slide_base(chapter_title, label, "The client credentials flow")
        bullets(image, FLOW, active={0, 1, 2} if step_id == "flow-2" else {1, 3})
        return image

    if step_id in ("token-1", "token-2"):
        image = slide_base(chapter_title, label, "Post to the token endpoint")
        highlight = set(range(4, 15)) if step_id == "token-1" else {5, 14, 16, 17}
        draw_code(image, (96, 240, W - 96, 1030), "Program.cs", "csharp", TOKEN, types=TYPES, size=25,
                  highlight=highlight)
        return image

    if step_id == "call-1":
        image = slide_base(chapter_title, label, "Create a content item")
        draw_code(image, (96, 240, W - 96, 1030), "Program.cs", "csharp", CREATE, types=TYPES, size=25)
        return image

    if step_id == "call-2":
        image = slide_base(chapter_title, label, "The body of the request")
        draw_code(image, (96, 240, 1130, 640), "Request body", "json", BODY, types=TYPES, size=26)
        note_cards(image, [
            ("ContentType, ContentItemId", "These names are case sensitive."),
            ("A ContentItemId updates", "An existing item is changed."),
            ("?draft=true", "Saves a draft, without publishing."),
        ], (1180, 260, W - 96, 1030))
        return image

    if step_id == "call-3":
        image = slide_base(chapter_title, label, "Read and delete it")
        draw_code(image, (96, 240, W - 96, 1030), "Program.cs", "csharp", READ_DELETE, types=TYPES, size=25)
        return image

    if step_id == "call-4":
        image = slide_base(chapter_title, label, "The permissions of each call")
        bullets(image, PERMISSIONS)
        return image

    if step_id in ("fail-1", "fail-2"):
        image = slide_base(chapter_title, label, "The answers of the Content API")
        bullets(image, RESPONSES, active={0, 1} if step_id == "fail-1" else {2})
        return image

    if step_id in ("fail-3", "fail-4"):
        image = slide_base(chapter_title, label, "Tokens and the token request")
        bullets(image, TOKENS, active={0} if step_id == "fail-3" else {1})
        return image

    raise SystemExit(f"No slide for {step_id}")
