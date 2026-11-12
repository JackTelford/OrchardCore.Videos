"""The slides of the Lucene video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

NEED = [
    ("1", "Find by words", "Visitors type a few words and expect the matching pages."),
    ("2", "An index", "A copy of your text, organized so that words are found quickly."),
    ("3", "A separate service", "Elasticsearch and Azure AI Search keep the index outside your site."),
    ("4", "Inside your site", "Lucene keeps the index in files that belong to your site."),
]

WHERE = [
    ("1", "One folder for each tenant", "Each tenant keeps its Lucene indexes in a folder of its own."),
    ("2", "On the server", "The files sit with the site, with no connection to set up."),
    ("3", "Kept up to date", "Publishing, unpublishing or removing content updates the index."),
]

ADDS = [
    ("check", "Lucene indexes", "Created and managed on the Indexes page."),
    ("check", "Analyzer and stored values", "Options for how text is analyzed and kept."),
    ("check", "What is indexed", "Settings on the parts and fields of content types."),
    ("check", "A source for site search", "The Search feature queries a Lucene index."),
]

LOCAL = [
    ("1", "Local files", "Each server keeps its own Lucene files for every index."),
    ("2", "A local record", "Each server also keeps its own record of how far it has indexed."),
    ("3", "Changes elsewhere", "An index has to learn about changes made on the other servers."),
]

TASK = [
    ("1", "The database", "Content changes are recorded in the database."),
    ("2", "Indexes Content Updater", "A background task that runs every minute."),
    ("3", "Indexing Worker", "The feature that provides the task."),
]

WORKER = [
    ("1", "On the Features page", "Lucene Worker is listed in the Search category, with Lucene."),
    ("2", "No task of its own", "The Indexes Content Updater task comes from Indexing Worker."),
]

SERVERS = [
    ("1", "One server", "The site updates its own index when content changes."),
    ("2", "Several servers", "The Indexes Content Updater task reaches the other servers."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Searching needs an index")
        bullets(image, NEED)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Where a Lucene index lives")
        bullets(image, WHERE)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What the Lucene module adds")
        bullets(image, ADDS)
        return image

    if step_id == "servers-1":
        image = slide_base(chapter_title, label, "One copy of the index on each server")
        bullets(image, LOCAL)
        return image

    if step_id == "servers-2":
        image = slide_base(chapter_title, label, "Keeping the indexes up to date")
        bullets(image, TASK)
        return image

    if step_id == "servers-3":
        image = slide_base(chapter_title, label, "Lucene Worker")
        bullets(image, WORKER)
        return image

    if step_id == "servers-4":
        image = slide_base(chapter_title, label, "One server or several")
        bullets(image, SERVERS)
        return image

    raise SystemExit(f"No slide for {step_id}")
