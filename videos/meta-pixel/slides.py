"""The slides of the Meta Pixel video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

HOW = [
    ("1", "The pixel", "A script in the browser of each visitor reports page views to Meta."),
    ("2", "The Conversions API", "Your server sends events, such as Purchase or Lead, to Meta."),
    ("3", "The Pixel Identifier", "Both ways use the identifier of your pixel."),
]

PAGES = [
    ("1", "Front end only", "The pixel is added to the pages visitors see, not to the admin pages."),
    ("2", "A page view per page", "Each time a page loads, it reports a page view to Meta."),
    ("3", "Nothing without an identifier", "While the Pixel Identifier is empty, no pixel is added."),
    ("4", "Tracking not allowed", "For a visitor who cannot be tracked, no pixel is added."),
]

WHERE = [
    ("1", "The Workflows feature", "The task is available when the Workflows feature is enabled."),
    ("2", "Design, Workflows", "Workflows are listed under Design, Workflows."),
    ("3", "The Meta category", "The workflow editor holds the Meta Conversions API Event task there."),
]

SETTINGS = [
    ("1", "Event Name", "Required. A standard name such as Purchase or Lead, or a custom name."),
    ("2", "Action Source", "Where the conversion took place. Website is the default."),
    ("3", "Event Source URL", "Optional. The browser address where the event happened."),
    ("4", "Event ID", "Optional. A unique id that lets Meta deduplicate an event the browser also reports."),
]

OUTCOMES = [
    ("check", "Done", "Meta accepted the event."),
    ("!", "Failed", "Meta did not accept it, or the Pixel Identifier or the access token is missing."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Two ways to report to Meta")
        bullets(image, HOW, active={0, 1})
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Two ways to report to Meta")
        bullets(image, HOW, active={0, 1, 2})
        return image

    if step_id == "page-1":
        image = slide_base(chapter_title, label, "What the pixel does on your pages")
        bullets(image, PAGES, active={0, 1})
        return image

    if step_id == "page-2":
        image = slide_base(chapter_title, label, "What the pixel does on your pages")
        bullets(image, PAGES, active={2, 3})
        return image

    if step_id == "wf-1":
        image = slide_base(chapter_title, label, "Where to find the task")
        bullets(image, WHERE)
        return image

    if step_id == "wf-2":
        image = slide_base(chapter_title, label, "Meta Conversions API Event")
        bullets(image, SETTINGS, active={0, 1})
        return image

    if step_id == "wf-3":
        image = slide_base(chapter_title, label, "Meta Conversions API Event")
        bullets(image, SETTINGS, active={2, 3})
        return image

    if step_id == "wf-4":
        image = slide_base(chapter_title, label, "Two outcomes")
        bullets(image, OUTCOMES)
        return image

    raise SystemExit(f"No slide for {step_id}")
