"""The slides of the Remote Deployment video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

SITES = [
    ("1", "Source site", "Holds the deployment plan and a remote instance."),
    ("2", "Destination site", "Holds a remote client and imports the package."),
    ("3", "Shared key", "A client name and an Api Key, the same on both sites."),
]

EXECUTE = [
    ("1", "Build", "The source builds a package from the steps of the plan."),
    ("2", "Send", "It posts the package with a client name and an Api Key."),
    ("3", "Check", "The destination compares both with its remote client."),
    ("4", "Import", "If they match, the destination imports the package at once."),
]

BEFORE = [
    ("check", "Features", "Deployment and Remote Deployment, enabled on both sites."),
    ("check", "Network", "The source reaches the destination, over HTTPS on a real site."),
    ("check", "Destination features", "The features that the steps of the plan need are enabled."),
]

URL_PARTS = [
    ("1", "Site address", "https://example.com"),
    ("2", "Tenant prefix", "/staging, only if the destination tenant has a prefix"),
    ("3", "Import path", "/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import"),
    ("=", "Together", "https://example.com/staging/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import"),
]

SAFE = [
    ("1", "Use HTTPS", "The client name and Api Key travel with the package."),
    ("2", "One key per pairing", "Use a different Api Key for each source and destination."),
    ("3", "Rotate keys", "Change them regularly, and after any suspected leak."),
    ("4", "Clean up", "Delete unused clients and instances. Treat packages as sensitive."),
]

PERMISSIONS = [
    ("1", "Manage remote instances", "Opens the Remote Instances list."),
    ("2", "Manage remote clients", "Opens the Remote Clients list."),
    ("3", "Manage deployment plans", "Needed for the Plans list and to open a plan."),
    ("4", "Export Data", "Needed for the Plans list and to execute a plan."),
]

FAILS = [
    ("1", "The Url", "Check the tenant prefix and the path of the endpoint."),
    ("2", "Client name and Api Key", "They must match on both sites."),
    ("3", "Reverse proxy", "It must allow large uploads."),
    ("4", "Destination features", "The steps of the plan need their features enabled."),
]


def slide(step_id, chapter_title, label):
    if step_id == "how-1":
        image = slide_base(chapter_title, label, "Two sites, two records")
        bullets(image, SITES, active={0, 1})
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "What Execute does")
        bullets(image, EXECUTE)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "Before you start")
        bullets(image, BEFORE)
        return image

    if step_id == "url-1":
        image = slide_base(chapter_title, label, "The Url of a remote instance")
        bullets(image, URL_PARTS)
        return image

    if step_id == "sec-1":
        image = slide_base(chapter_title, label, "Keep the connection safe")
        bullets(image, SAFE, active={0, 1})
        return image

    if step_id == "sec-2":
        image = slide_base(chapter_title, label, "Keep the connection safe")
        bullets(image, SAFE, active={2, 3})
        return image

    if step_id == "sec-3":
        image = slide_base(chapter_title, label, "Permissions")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "fix-1":
        image = slide_base(chapter_title, label, "If a deployment fails")
        bullets(image, FAILS, active={0, 1, 2})
        return image

    if step_id == "fix-2":
        image = slide_base(chapter_title, label, "If a deployment fails")
        bullets(image, FAILS, active={3})
        return image

    raise SystemExit(f"No slide for {step_id}")
