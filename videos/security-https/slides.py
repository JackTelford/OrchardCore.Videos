"""The slides of the HTTPS video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

PLAIN = [
    ("!", "Plain HTTP", "The data between the browser and the site travels as readable text."),
    ("check", "HTTPS", "The connection is encrypted, so the data cannot be read along the way."),
]

REDIRECT = [
    ("1", "Request over HTTP", "A visitor opens an address that starts with http."),
    ("2", "Redirect", "The site answers with a redirect to the HTTPS address."),
    ("3", "Request over HTTPS", "The browser follows the redirect, and the connection is encrypted."),
]

SETTINGS = [
    ("1", "Require HTTPS", "Redirects requests that arrive over HTTP to HTTPS."),
    ("2", "HSTS mode", "Controls when the HSTS header is sent to browsers."),
    ("3", "Require HTTPS Permanent", "A permanent redirect instead of a temporary one."),
    ("4", "SSL Port", "The port to redirect to. When empty, the site tries to work it out."),
]

HSTS = [
    ("H", "The HSTS header", "Tells browsers to use HTTPS only, for 365 days, subdomains included."),
    ("1", "Disabled", "The header is never sent."),
    ("2", "Enabled", "The header is sent in every environment."),
    ("3", "From environment", "Enabled in Production, disabled otherwise."),
]

HTTPS_ONLY = [
    ("check", "Opened over HTTPS", "The settings can be edited and saved."),
    ("!", "Opened over plain HTTP", "The changes you save are not applied."),
    ("!", "The message", "For safety, changing HTTPS-only settings over HTTP has been prevented."),
]

AFTER_SAVE = [
    ("1", "The saved settings are applied", "Saving reloads the tenant. The page warns the website might be restarted."),
    ("2", "Requests over HTTP are redirected", "Every page of the site is covered, not only the admin."),
]

TENANTS = [
    ("1", "The settings belong to the tenant", "Each tenant of a site has its own HTTPS settings."),
    ("2", "Saving reloads the tenant", "The page warns that the website might be restarted."),
]

LOAD_BALANCER = [
    ("1", "The load balancer handles HTTPS", "It passes plain HTTP to the site."),
    ("2", "The site sees HTTP requests", "The page ignores the changes you save."),
    ("3", "Reverse Proxy settings", "They can forward the protocol the client used."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Plain HTTP and HTTPS")
        bullets(image, PLAIN)
        return image

    if step_id == "what-1":
        image = slide_base(chapter_title, label, "From HTTP to HTTPS")
        bullets(image, REDIRECT)
        return image

    if step_id == "what-2":
        image = slide_base(chapter_title, label, "The four settings")
        bullets(image, SETTINGS)
        return image

    if step_id == "what-3":
        image = slide_base(chapter_title, label, "HSTS mode")
        bullets(image, HSTS)
        return image

    if step_id == "what-4":
        image = slide_base(chapter_title, label, "Open the page over HTTPS")
        bullets(image, HTTPS_ONLY)
        return image

    if step_id == "after-1":
        image = slide_base(chapter_title, label, "After you save")
        bullets(image, AFTER_SAVE)
        return image

    if step_id == "after-2":
        image = slide_base(chapter_title, label, "Tenants and restarts")
        bullets(image, TENANTS)
        return image

    if step_id == "after-3":
        image = slide_base(chapter_title, label, "Behind a load balancer")
        bullets(image, LOAD_BALANCER)
        return image

    raise SystemExit(f"No slide for {step_id}")
