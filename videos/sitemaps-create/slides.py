"""The slides of the Sitemaps video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

WHAT_IT_LISTS = [
    ("1", "Address", "The location of each page of your site."),
    ("2", "Last modified", "The date the page last changed."),
    ("3", "Change Frequency", "How often the page is likely to change."),
    ("4", "Priority", "How the page ranks against the other pages of your site."),
]

PARTS_OF_A_SITEMAP = [
    ("1", "Name", "How the sitemap is listed in the admin."),
    ("2", "Permalink", "The address where it is served. It ends in .xml."),
    ("3", "Enabled", "When unchecked, the sitemap is not served."),
    ("4", "Sources", "They decide which addresses the sitemap lists."),
]

KINDS_OF_SOURCE = [
    ("1", "Sitemap Content Types", "An entry for each content item of the content types you choose."),
    ("2", "Sitemap Custom Path", "An entry for a single address that you type."),
]

ROBOTS_FILE = [
    ("check", "The SEO feature is enabled", "It serves the robots file of the site."),
    ("check", "The site has no robots file of its own", "A robots file on disk is served as it is."),
    ("check", "The Base url is set", "Sitemaps are listed with their full address."),
    ("check", "Include all sitemaps is checked and saved", "Settings, Search, Search Engine Optimization, Robots."),
]


def slide(step_id, chapter_title, label):
    if step_id == "what-1":
        image = slide_base(chapter_title, label, "What a sitemap lists")
        bullets(image, WHAT_IT_LISTS)
        return image

    if step_id == "what-2":
        image = slide_base(chapter_title, label, "A sitemap in Orchard Core")
        bullets(image, PARTS_OF_A_SITEMAP)
        return image

    if step_id == "what-3":
        image = slide_base(chapter_title, label, "Two kinds of source")
        bullets(image, KINDS_OF_SOURCE)
        return image

    if step_id == "reach-1":
        image = slide_base(chapter_title, label, "Sitemaps in the robots file")
        bullets(image, ROBOTS_FILE, active={0, 1, 2})
        return image

    if step_id == "reach-2":
        image = slide_base(chapter_title, label, "Sitemaps in the robots file")
        bullets(image, ROBOTS_FILE)
        return image

    raise SystemExit(f"No slide for {step_id}")
