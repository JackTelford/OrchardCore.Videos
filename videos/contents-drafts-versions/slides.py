"""The slides of the Drafts and Versions video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

PUBLISHED_AND_DRAFT = [
    ("1", "Published", "The version that visitors see on the site."),
    ("2", "Draft", "A version you are working on. Visitors do not see it."),
]

EDIT_A_PUBLISHED_ITEM = [
    ("1", "Edit a published item, then Save Draft", "The published version stays as it is."),
    ("2", "A separate version", "Your changes are stored in a draft."),
    ("3", "Publish the draft", "It becomes the live version. The version it replaces stays stored."),
]

BADGES = [
    ("check", "Published", "A live version exists, and visitors can see the page."),
    ("D", "Draft", "The last saved version is not live. Visitors do not see it."),
    ("+", "Published and Draft", "A live version, and a draft with changes that wait."),
]

SETTINGS = [
    ("1", "Draftable", "Adds the Save Draft button to the editor."),
    ("2", "Versionable", "Keeps the published version while you edit a draft."),
    ("check", "The Article type in this video",
     "From the Blog recipe. It has both settings, like the types you create in the admin."),
]

PERMISSIONS = [
    ("1", "Save Draft", "Needs one of: Edit content for others, Edit own content. Publish permissions include them."),
    ("2", "Publish, Publish Draft and Unpublish",
     "Need one of: Publish or unpublish content for others, Publish or unpublish own content."),
    ("3", "Discard Draft", "Needs one of: Delete content for others, Delete own content."),
    ("4", "A user who can only edit", "Prepares changes that someone else with publish permission releases."),
]

OLDER_VERSIONS = [
    ("1", "Stored in the database", "Older versions stay after you publish a draft."),
    ("2", "Audit Trail", "When enabled and set to record a content type, records that type's changes and can restore a version as a draft."),
    ("3", "Content Version Pruning", "Can remove old versions of the content types you choose."),
]


def slide(step_id, chapter_title, label):
    if step_id == "versions-1":
        image = slide_base(chapter_title, label, "Published and draft")
        bullets(image, PUBLISHED_AND_DRAFT)
        return image
    if step_id == "versions-2":
        image = slide_base(chapter_title, label, "Editing a published item")
        bullets(image, EDIT_A_PUBLISHED_ITEM)
        return image
    if step_id == "versions-3":
        image = slide_base(chapter_title, label, "Badges in the Content Items list")
        bullets(image, BADGES)
        return image
    if step_id == "versions-4":
        image = slide_base(chapter_title, label, "Two content type settings")
        bullets(image, SETTINGS)
        return image
    if step_id == "limits-1":
        image = slide_base(chapter_title, label, "Who can do what")
        bullets(image, PERMISSIONS)
        return image
    if step_id == "limits-2":
        image = slide_base(chapter_title, label, "Where older versions go")
        bullets(image, OLDER_VERSIONS)
        return image
    raise SystemExit(f"No slide for {step_id}")
