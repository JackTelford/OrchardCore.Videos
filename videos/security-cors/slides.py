"""The slides of the CORS video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

ORIGIN = [
    ("1", "Scheme", "The protocol of the address: https or http"),
    ("2", "Host", "The name of the server, such as portal.example.com"),
    ("3", "Port", "Optional in the address, such as 8443"),
    ("4", "Any difference is another origin", "Another scheme, host or port makes a call cross-origin"),
]

DECISION = [
    ("1", "A page calls your site", "The page belongs to another origin, such as a partner site"),
    ("2", "The browser looks for CORS headers", "Your site lists the origins, methods and headers it allows"),
    ("3", "A match: the page can use the response", "The browser hands the response to the page"),
    ("4", "No match, or no policy: blocked", "A site with no policy sends no CORS headers"),
]

NOT = [
    ("1", "Not a sign-in", "CORS does not identify the visitor"),
    ("2", "Not permissions", "Your APIs still decide who may do what"),
    ("3", "Not a way to block tools", "Clients that are not browsers do not enforce CORS"),
]

NARROW = [
    ("1", "Only what the client needs", "List its origins, methods and headers"),
    ("2", "Origins match exactly", "Scheme, host and port, with no path and no trailing slash"),
    ("3", "HTTPS in production", "List every trusted origin when you allow credentials"),
    ("4", "Few exposed headers", "Only the response headers the client reads"),
]

POLICIES = [
    ("1", "One default policy", "It applies to the whole site: Set as default policy"),
    ("2", "The other policies", "Registered by name, so a module can select one for an endpoint"),
]

DEPLOYMENT = [
    ("1", "The Deployment feature", "Enable it to build deployment plans"),
    ("2", "The Cors settings step", "Add it to a plan to export the policies"),
]

CHECKS = [
    ("1", "Is the call cross-origin?", "Calls to the same origin do not need CORS"),
    ("2", "Is the feature enabled?", "On the site that receives the request"),
    ("3", "Is the policy the default?", "Other policies apply only when a module selects them"),
    ("4", "Does the origin match exactly?", "Scheme, host and port, with no path or slash"),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "An origin is a scheme, a host and a port")
        bullets(image, ORIGIN)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "How a browser decides")
        bullets(image, DECISION)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "What CORS is not")
        bullets(image, NOT)
        return image

    if step_id == "narrow-1":
        image = slide_base(chapter_title, label, "Keep each policy narrow")
        bullets(image, NARROW)
        return image

    if step_id == "narrow-2":
        image = slide_base(chapter_title, label, "The default policy and the others")
        bullets(image, POLICIES)
        return image

    if step_id == "narrow-3":
        image = slide_base(chapter_title, label, "Move policies to another site")
        bullets(image, DEPLOYMENT)
        return image

    if step_id == "narrow-4":
        image = slide_base(chapter_title, label, "When a browser reports a CORS failure")
        bullets(image, CHECKS)
        return image

    raise SystemExit(f"No slide for {step_id}")
