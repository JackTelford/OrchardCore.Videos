"""The slides of the Build a Blog video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

RECIPE = [
    ("1", "Content types", "Blog, Blog Post, Category, Tag, Article, Page and widgets."),
    ("2", "Starter content", "A blog with one post, an About article and a Main Menu."),
    ("3", "Two taxonomies", "Categories, with Travel. Tags, with Earth, Exploration and Space."),
    ("4", "The Blog Theme", "The theme that displays the blog, its posts and its menu."),
]

PARTS = [
    ("1", "Blog", "A list of posts, most recent first, and the home page of the site."),
    ("2", "Blog Post", "A title, a subtitle, a banner image and a body."),
    ("3", "Category", "One per post, chosen from the Categories taxonomy."),
    ("4", "Tags", "Any number per post, chosen from the Tags taxonomy."),
]

TASKS = [
    ("1", "Add a category", "A term of the Categories taxonomy, with an icon."),
    ("2", "Write a post", "Title, banner image, body, category and tags."),
    ("3", "See the tags", "Terms of the Tags taxonomy, created from a post."),
    ("4", "Add a menu item", "A link in the Main Menu, published with the menu."),
]


def slide(step_id, chapter_title, label):
    if step_id == "pieces-1":
        image = slide_base(chapter_title, label, "What the Blog recipe sets up")
        bullets(image, RECIPE)
        return image

    if step_id in ("pieces-2", "pieces-3"):
        image = slide_base(chapter_title, label, "A blog, its posts and their terms")
        bullets(image, PARTS, active={0, 1} if step_id == "pieces-2" else {2, 3})
        return image

    if step_id == "pieces-4":
        image = slide_base(chapter_title, label, "What you do in this video")
        bullets(image, TASKS)
        return image

    raise SystemExit(f"No slide for {step_id}")
