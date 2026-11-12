"""The slides of the Reverse Proxy Settings video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PROXY = [
    ("1", "Visitor", "Connects to the proxy, from the visitor's own address"),
    ("2", "Reverse proxy or load balancer", "Receives each request and passes it on to your site"),
    ("3", "Your site", "Sees the address of the proxy, not the address of the visitor"),
]

HEADERS = [
    ("1", "X-Forwarded-For", "The IP address of the client"),
    ("2", "X-Forwarded-Proto", "The protocol of the client, HTTP or HTTPS"),
    ("3", "X-Forwarded-Host", "The host the client asked for"),
]

AFFECTED = [
    ("1", "Audit Trail", "Can record the client IP address"),
    ("2", "Rate Limits", "Count the requests of anonymous visitors per address"),
    ("3", "Without the headers", "Behind a proxy, every visitor has the address of the proxy"),
]

RISKS = [
    ("1", "IP spoofing", "A sender can choose the address a visitor appears to have"),
    ("2", "Header injection", "Forged forwarded headers can reach your site"),
    ("3", "The rule", "Trust forwarded headers only when your own proxies send them"),
]

TRUSTED = [
    ("1", "Known proxies", "The IP addresses of the proxies you trust"),
    ("2", "Known networks", "The network ranges of the proxies you trust"),
    ("3", "Not on this page", "Set in the Settings recipe step, or in the server configuration"),
]

PRECEDENCE = [
    ("1", "Configuration first", "Its values take precedence over this page"),
    ("2", "A message tells you", "The current settings are coming from configuration sources"),
    ("3", "Saving", "Affects the admin settings, not the configuration"),
]

SCOPE = [
    ("1", "Per tenant", "Each tenant of an installation has its own options"),
    ("2", "Deployment plans", "The Reverse Proxy settings step exports them"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A proxy stands in front of your site")
        bullets(image, PROXY)
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Headers carry the original details")
        bullets(image, HEADERS)
        return image
    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Features that use the visitor's address")
        bullets(image, AFFECTED)
        return image
    if step_id == "safe-1":
        image = slide_base(chapter_title, label, "Trust only your own proxies")
        bullets(image, RISKS)
        return image
    if step_id == "safe-2":
        image = slide_base(chapter_title, label, "Known proxies and known networks")
        bullets(image, TRUSTED)
        return image
    if step_id == "safe-3":
        image = slide_base(chapter_title, label, "When the configuration takes over")
        bullets(image, PRECEDENCE)
        return image
    if step_id == "safe-4":
        image = slide_base(chapter_title, label, "Per tenant, and in deployment plans")
        bullets(image, SCOPE)
        return image
    raise SystemExit(f"No slide for {step_id}")
