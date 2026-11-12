"""The slides of the Content API video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import ACCENT, BORDER, DIM, MUTED, PANEL, PANEL_DIM, TEXT, W, bullets, draw_code, font, note_cards, slide_base

MARGIN = 96
BOTTOM = 1030

ROUTES = """
GET     /api/content/{contentItemId}
POST    /api/content
DELETE  /api/content/{contentItemId}
"""

ITEM = """
{
  "ContentItemId": "4kd8y3m1q6x7vz2p5n9w0c4tga",
  "ContentType": "Article",
  "DisplayText": "Planting notes",
  "Latest": true,
  "Published": true,
  "TitlePart": {
    "Title": "Planting notes"
  },
  "HtmlBodyPart": {
    "Html": "<p>Seedlings go out in May.</p>"
  }
  // ...
}
"""

TOKEN_REQUEST = """
curl https://example.com/connect/token \\
  -d grant_type=client_credentials \\
  -d client_id=content-api-client \\
  -d client_secret=<client secret>
"""

AUTHORIZATION_HEADER = """
Authorization: Bearer <access token>
"""

CREATE_REQUEST = """
curl -X POST https://example.com/api/content \\
  -H "Authorization: Bearer <access token>" \\
  -H "Content-Type: application/json" \\
  -d '{
    "ContentType": "Article",
    "TitlePart": { "Title": "Planting notes" },
    "HtmlBodyPart": { "Html": "<p>Seedlings go out in May.</p>" }
  }'
"""

CREATE_RESPONSE = """
// 200 OK
{
  "ContentItemId": "4kd8y3m1q6x7vz2p5n9w0c4tga",
  "ContentType": "Article",
  "DisplayText": "Planting notes",
  "Latest": true,
  "Published": true,
  "TitlePart": { "Title": "Planting notes" },
  "HtmlBodyPart": { "Html": "<p>Seedlings go out in May.</p>" }
  // ...
}
"""

DRAFT_REQUEST = """
curl -X POST "https://example.com/api/content?draft=true" \\
  -H "Authorization: Bearer <access token>" \\
  -H "Content-Type: application/json" \\
  -d '{ "ContentType": "Article", "TitlePart": { "Title": "Greenhouse log" } }'
"""

DRAFT_RESPONSE = """
// 200 OK
{
  "ContentItemId": "5vq2n8t4c1m7x3w9z6k0d2hrja",
  "ContentType": "Article",
  "DisplayText": "Greenhouse log",
  "Latest": true,
  "Published": false
  // ...
}
"""

READ_REQUEST = """
curl https://example.com/api/content/4kd8y3m1q6x7vz2p5n9w0c4tga \\
  -H "Authorization: Bearer <access token>"
"""

READ_RESPONSE = """
// 200 OK, or 404 Not Found when the item has no published version
{
  "ContentItemId": "4kd8y3m1q6x7vz2p5n9w0c4tga",
  "ContentType": "Article",
  "DisplayText": "Planting notes",
  "Published": true
  // ...
}
"""

UPDATE_REQUEST = """
curl -X POST https://example.com/api/content \\
  -H "Authorization: Bearer <access token>" \\
  -H "Content-Type: application/json" \\
  -d '{
    "ContentItemId": "4kd8y3m1q6x7vz2p5n9w0c4tga",
    "TitlePart": { "Title": "Spring planting notes" }
  }'
"""

UPDATE_DRAFT_REQUEST = """
curl -X POST "https://example.com/api/content?draft=true" \\
  -H "Authorization: Bearer <access token>" \\
  -H "Content-Type: application/json" \\
  -d '{
    "ContentItemId": "4kd8y3m1q6x7vz2p5n9w0c4tga",
    "Article": { "Subtitle": { "Text": "Revised in the greenhouse" } }
  }'
"""

UPDATE_DRAFT_RESPONSE = """
// 200 OK: the draft version
{
  "ContentItemId": "4kd8y3m1q6x7vz2p5n9w0c4tga",
  "ContentType": "Article",
  "DisplayText": "Spring planting notes",
  "Latest": true,
  "Published": false,
  "Article": { "Subtitle": { "Text": "Revised in the greenhouse" } }
  // ...
}
"""

DELETE_REQUEST = """
curl -X DELETE https://example.com/api/content/4kd8y3m1q6x7vz2p5n9w0c4tga \\
  -H "Authorization: Bearer <access token>"
"""

DELETE_RESPONSE = """
// 200 OK, with the JSON of the removed item
{
  "ContentItemId": "4kd8y3m1q6x7vz2p5n9w0c4tga",
  "ContentType": "Article"
  // ...
}
"""

UNAUTHORIZED = """
// 401 Unauthorized
{
  "status": 401,
  "title": "Authentication required"
  // ...
}
"""

FORBIDDEN = """
// 403 Forbidden
{
  "status": 403,
  "title": "Access forbidden"
  // ...
}
"""

FEATURES = [
    ("1", "Enable two features", "OpenID Connect Authorization Server and OpenID Connect Token Validation"),
    ("2", "Allow the flow in the server settings", "Settings, OpenID Connect, Authorization Server: Allow Client Credentials Flow"),
    ("3", "Add an application", "Confidential client, Allow Client Credentials Flow, and a role"),
    ("4", "Grant permissions to the role", "Access content via the api, and the permissions of each call"),
]

TOOLS = [
    ("check", "Content API", "Read, create, update and delete one content item per call."),
    ("check", "GraphQL API", "Query lists of content items."),
]

CALL_PERMISSIONS = [
    ("GET", "View all content"),
    ("POST to create", "Edit content for others, Publish or unpublish content for others"),
    ("POST to update", "Edit content for others"),
    ("DELETE", "Delete content for others"),
]

STATUS_CODES = [
    ("200", "OK", "The item as JSON"),
    ("400", "Bad Request", "No body, an unknown ContentType, or a failed validation"),
    ("401", "Unauthorized", "No valid access token"),
    ("403", "Forbidden", "A permission is missing"),
    ("404", "Not Found", "No published item has this ID"),
]


def code_height(code, size):
    """The height of a code panel for this code: the window bar, the lines, and some air."""
    return 74 + len(code.strip("\n").split("\n")) * int(size * 1.42) + 40


def panel(image, top, title, language, code, size=26, left=MARGIN, right=W - MARGIN):
    """A code panel sized for its code. Returns the y below it."""
    bottom = top + code_height(code, size)
    if bottom > BOTTOM:
        raise SystemExit(f"The panel {title!r} ends at {bottom}, below {BOTTOM}")
    draw_code(image, (left, top, right, bottom), title, language, code, size=size)
    return bottom


def permission_table(image, top=250):
    """Every call needs one permission, then one or two more for the item."""
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle((MARGIN, top, W - MARGIN, top + 90), radius=16, fill=PANEL, outline=ACCENT, width=2)
    draw.text((MARGIN + 40, top + 45), "Every call", font=font(34, "Bold"), fill=ACCENT, anchor="lm")
    draw.text((MARGIN + 290, top + 45), "Access content via the api", font=font(34, "Bold"), fill=TEXT, anchor="lm")
    y = top + 130
    for verb, permissions in CALL_PERMISSIONS:
        draw.rounded_rectangle((MARGIN, y, W - MARGIN, y + 110), radius=16, fill=PANEL, outline=BORDER, width=2)
        draw.text((MARGIN + 40, y + 55), verb, font=font(34, "Bold"), fill=ACCENT, anchor="lm")
        draw.text((MARGIN + 450, y + 55), permissions, font=font(30), fill=TEXT, anchor="lm")
        y += 140


def status_rows(image, active, top=250):
    """One row per status code. The rows in active are bright, the others are dimmed."""
    draw = ImageDraw.Draw(image)
    y = top
    for index, (code, name, text) in enumerate(STATUS_CODES):
        on = index in active
        draw.rounded_rectangle((MARGIN, y, W - MARGIN, y + 130), radius=16, fill=PANEL if on else PANEL_DIM,
                               outline=ACCENT if on else BORDER, width=2)
        draw.text((MARGIN + 40, y + 65), code, font=font(54, "Bold"), fill=ACCENT if on else DIM, anchor="lm")
        draw.text((MARGIN + 250, y + 40), name, font=font(36, "Bold"), fill=TEXT if on else DIM, anchor="lm")
        draw.text((MARGIN + 250, y + 92), text, font=font(28), fill=MUTED if on else DIM, anchor="lm")
        y += 150


def two_cards(image, left, right, top):
    """Two cards side by side, each of (title, text)."""
    draw = ImageDraw.Draw(image)
    middle = W // 2
    for (title, text), (x0, x1) in zip((left, right), ((MARGIN, middle - 24), (middle + 24, W - MARGIN))):
        draw.rounded_rectangle((x0, top, x1, top + 180), radius=16, fill=PANEL, outline=ACCENT, width=2)
        draw.text((x0 + 40, top + 64), title, font=font(34, "Bold"), fill=TEXT, anchor="lm")
        draw.text((x0 + 40, top + 118), text, font=font(30), fill=MUTED, anchor="lm")


def slide(step_id, chapter_title, label):
    if step_id == "api-1":
        image = slide_base(chapter_title, label, "Three endpoints")
        draw_code(image, (MARGIN, 250, 1010, 250 + code_height(ROUTES, 28)), "api/content", "bash", ROUTES, size=28)
        note_cards(image, [
            ("GET", "Reads the published version"),
            ("POST", "Creates or updates an item"),
            ("DELETE", "Removes an item"),
        ], (1050, 250, W - MARGIN, BOTTOM))
        return image

    if step_id == "api-2":
        image = slide_base(chapter_title, label, "One item per call")
        bullets(image, TOOLS)
        return image

    if step_id == "api-3":
        image = slide_base(chapter_title, label, "A content item as JSON")
        draw_code(image, (MARGIN, 250, 1060, 250 + code_height(ITEM, 26)), "Article", "json", ITEM, size=26)
        note_cards(image, [
            ("Case sensitive names", "ContentType, not contentType"),
            ("One property per part", "TitlePart, HtmlBodyPart"),
            ("Set by the server", "Published, Latest and the dates"),
        ], (1100, 250, W - MARGIN, BOTTOM))
        return image

    if step_id == "auth-1":
        image = slide_base(chapter_title, label, "Before the first call")
        bullets(image, FEATURES, active={0, 1})
        return image

    if step_id == "auth-2":
        image = slide_base(chapter_title, label, "Before the first call")
        bullets(image, FEATURES, active={2, 3})
        return image

    if step_id == "auth-3":
        image = slide_base(chapter_title, label, "Get a token, send a token")
        bottom = panel(image, 250, "Token request", "bash", TOKEN_REQUEST, size=28)
        panel(image, bottom + 40, "Header of every call", "bash", AUTHORIZATION_HEADER, size=28)
        return image

    if step_id == "perm-3":
        image = slide_base(chapter_title, label, "Permissions of each call")
        permission_table(image)
        return image

    if step_id == "create-1":
        image = slide_base(chapter_title, label, "Create an item")
        panel(image, 250, "Request", "bash", CREATE_REQUEST, size=26)
        return image

    if step_id == "create-2":
        image = slide_base(chapter_title, label, "The response is the stored item")
        panel(image, 250, "Response", "json", CREATE_RESPONSE, size=26)
        return image

    if step_id == "create-3":
        image = slide_base(chapter_title, label, "Save a draft instead")
        bottom = panel(image, 250, "Request", "bash", DRAFT_REQUEST, size=26)
        panel(image, bottom + 40, "Response", "json", DRAFT_RESPONSE, size=26)
        return image

    if step_id == "read-1":
        image = slide_base(chapter_title, label, "Read an item")
        bottom = panel(image, 250, "Request", "bash", READ_REQUEST, size=26)
        panel(image, bottom + 40, "Response", "json", READ_RESPONSE, size=26)
        return image

    if step_id == "update-1":
        image = slide_base(chapter_title, label, "Update an item")
        panel(image, 250, "Request", "bash", UPDATE_REQUEST, size=26)
        return image

    if step_id == "update-2":
        image = slide_base(chapter_title, label, "Publish the changes, or keep a draft")
        bottom = panel(image, 250, "Request", "bash", UPDATE_DRAFT_REQUEST, size=26)
        two_cards(image, ("Without the parameter", "The changes are published."),
                  ("With draft set to true", "The changes wait as a draft."), bottom + 40)
        return image

    if step_id == "update-3":
        image = slide_base(chapter_title, label, "The response is the saved version")
        panel(image, 250, "Response", "json", UPDATE_DRAFT_RESPONSE, size=26)
        return image

    if step_id == "delete-1":
        image = slide_base(chapter_title, label, "Delete an item")
        bottom = panel(image, 250, "Request", "bash", DELETE_REQUEST, size=26)
        panel(image, bottom + 40, "Response", "json", DELETE_RESPONSE, size=26)
        return image

    if step_id == "delete-2":
        image = slide_base(chapter_title, label, "Which items a delete finds")
        note_cards(image, [
            ("Item with a published version", "200 OK, and its JSON"),
            ("Item with only a draft", "404 Not Found"),
            ("ID that does not exist", "404 Not Found"),
        ], (MARGIN, 260, W - MARGIN, BOTTOM))
        return image

    if step_id == "status-1":
        image = slide_base(chapter_title, label, "Status codes")
        status_rows(image, active={0, 1, 2, 3})
        return image

    if step_id == "status-2":
        image = slide_base(chapter_title, label, "Status codes")
        status_rows(image, active={1, 4})
        return image

    if step_id == "status-3":
        image = slide_base(chapter_title, label, "Problem details bodies")
        bottom = panel(image, 250, "Without a valid token", "json", UNAUTHORIZED, size=28)
        panel(image, bottom + 40, "Without a permission", "json", FORBIDDEN, size=28)
        return image

    raise SystemExit(f"No slide for {step_id}")
