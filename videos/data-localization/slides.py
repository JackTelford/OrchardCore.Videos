"""The slides of the Dynamic Translations video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

KINDS = [
    ("1", "Content type names", "Blog Post, Blog: the types you create."),
    ("2", "Content field names", "Subtitle, Tags: the labels of the fields on the editing page."),
    ("3", "Custom admin menu items", "The top-level items of the admin menus you build."),
    ("4", "Permissions", "Descriptions and categories, shown on the role editor."),
]

LOOKUP = [
    ("1", "A page asks for a name", "For example Blog Post, in the category Content Types."),
    ("2", "The culture of the request decides", "A request in French uses the French translations."),
    ("3", "A translation exists", "The page shows it: Article de blog."),
    ("4", "No translation", "The page shows the original text: Blog Post."),
]

TOOLS = [
    ("1", "PO files", "The fixed text of the interface and of the modules."),
    ("2", "Data Localization", "Names from the site's data: types, fields, menus, permissions."),
    ("3", "Content Localization", "The content items themselves, such as pages and posts."),
]

PERMISSIONS = [
    ("1", "Manage all dynamic translations", "Edit the translations of every culture."),
    ("2", "View dynamic translations and statistics", "Read-only access: the inputs of the editor are disabled."),
    ("3", "Manage French (France) translations", "One permission for each supported culture."),
]

DEFAULTS = [
    ("1", "Administrator", "Manage all dynamic translations."),
    ("2", "Editor", "View dynamic translations and statistics."),
    ("3", "A user who can only view", "You have read-only access to dynamic translations."),
]

DEPLOYMENT = [
    ("1", "Translations", "A deployment step that exports translations."),
    ("2", "Cultures and categories", "Include all cultures, or choose the cultures and categories to export."),
    ("3", "Import the result", "Import the exported file on a site where Data Localization is enabled."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Names that depend on your site's data")
        bullets(image, KINDS)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "How a name is shown")
        bullets(image, LOOKUP)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Which tool translates what")
        bullets(image, TOOLS)
        return image

    if step_id == "access-1":
        image = slide_base(chapter_title, label, "Permissions of Data Localization")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "access-2":
        image = slide_base(chapter_title, label, "Who gets what by default")
        bullets(image, DEFAULTS)
        return image

    if step_id == "move-1":
        image = slide_base(chapter_title, label, "Move translations to another site")
        bullets(image, DEPLOYMENT)
        return image

    raise SystemExit(f"No slide for {step_id}")
