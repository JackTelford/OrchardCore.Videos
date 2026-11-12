"""The slides of the HTTP Workflows video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

WHAT = [
    ("1", "HTTP Request event", "Starts a workflow when a request reaches its URL."),
    ("2", "Filter Incoming HTTP Request", "Starts a workflow when a request reaches a controller action."),
    ("3", "HTTP Response and HTTP Redirect", "Answer the caller of the workflow."),
    ("4", "HTTP Request task", "Calls a web API, and the workflow reads the response."),
]

FILTER_FIELDS = [
    ("1", "HTTP Method", "The HTTP method to match."),
    ("2", "Controller", "The controller name to match."),
    ("3", "Action", "The action name to match."),
    ("4", "Area", "The area name to match."),
]

REDIRECT_FIELDS = [
    ("1", "Location", "The URL to redirect to. With Liquid support."),
    ("2", "Permanent", "Checked, the redirect is a 301. Unchecked, it is a 302."),
]

MOVE = [
    ("1", "Export", "Download the workflow as a recipe."),
    ("2", "Import", "Import the recipe on the other site."),
    ("3", "URLs", "Each HTTP Request event gets its URL on that site."),
]

READ_BODY = """
// The body of the request, parsed as JSON.
const request = JSON.parse(readBody());

// Keep the product code for the activities that follow.
setProperty('sku', request.sku);
setOutcome('Done');
"""

READ_OTHERS = """
const fromQuery = queryString('sku');       // ?sku=A-100
const fromForm = requestForm('sku');        // a posted form field
const fromAny = deserializeRequestData();   // query string, form or JSON body
const context = httpContext();              // the HttpContext of the request
"""

ANSWER = """
// Answers the caller with text.
responseWrite('Stock checked.');
"""

SIGNAL_LIQUID = """
Approve the order: {{ 'Approved' | signal_url }}
"""

SIGNAL_SCRIPT = """
const link = signalUrl('Approved');
"""


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "What the HTTP activities do")
        bullets(image, WHAT, active={0, 1} if step_id == "why-1" else {2, 3})
        return image

    if step_id == "read-1":
        image = slide_base(chapter_title, label, "Read the body of the request")
        draw_code(image, (96, 240, W - 96, 640), "Script", "javascript", READ_BODY, size=30)
        return image

    if step_id == "read-2":
        image = slide_base(chapter_title, label, "Read the other parts of the request")
        draw_code(image, (96, 240, W - 96, 600), "Script", "javascript", READ_OTHERS, size=28)
        return image

    if step_id == "read-3":
        image = slide_base(chapter_title, label, "Answer the request")
        draw_code(image, (96, 240, 1150, 520), "Script", "javascript", ANSWER, size=28)
        note_cards(image, [
            ("HTTP Response", "A status code and a body."),
            ("HTTP Redirect", "A 301 or a 302."),
            ("Nothing written", "The caller gets a 202."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "signal-1":
        image = slide_base(chapter_title, label, "Make the link that resumes it")
        draw_code(image, (96, 240, 1150, 520), "HTTP Response: Content", "html", SIGNAL_LIQUID, size=28)
        draw_code(image, (96, 560, 1150, 800), "Script", "javascript", SIGNAL_SCRIPT, size=28)
        note_cards(image, [
            ("Signal event", "Name: Approved"),
            ("Waits until", "the link is opened."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "signal-2":
        image = slide_base(chapter_title, label, "What the link carries")
        draw_code(image, (96, 240, 1150, 520), "HTTP Response: Content", "html", SIGNAL_LIQUID, size=28)
        draw_code(image, (96, 560, 1150, 800), "Script", "javascript", SIGNAL_SCRIPT, size=28)
        note_cards(image, [
            ("A protected token", "Valid for seven days."),
            ("A path on the site", "Add the site address."),
            ("The same name", "Resumes the Signal."),
        ], (1200, 240, W - 96, 1030))
        return image

    if step_id == "more-1":
        image = slide_base(chapter_title, label, "Filter Incoming HTTP Request")
        bullets(image, FILTER_FIELDS)
        return image

    if step_id == "more-2":
        image = slide_base(chapter_title, label, "HTTP Redirect")
        bullets(image, REDIRECT_FIELDS)
        return image

    if step_id == "more-3":
        image = slide_base(chapter_title, label, "Move a workflow to another site")
        bullets(image, MOVE)
        return image

    raise SystemExit(f"No slide for {step_id}")
