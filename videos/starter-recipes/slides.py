"""The slides of the Starter Recipes video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

WHAT = [
    ("1", "Enables features", "Turns on the features the site needs."),
    ("2", "Activates themes", "Sets the admin theme and, in most recipes, the site theme."),
    ("3", "Creates roles", "Some recipes add roles such as Editor and Author."),
    ("4", "Creates content", "Content types, content items, and settings."),
]

CHOOSE_ONCE = [
    ("1", "One recipe", "Chosen on the Setup screen, in the Recipe list."),
    ("2", "More features later", "Tools, Features turns on what the recipe left off."),
    ("3", "Another theme later", "Design, Themes makes a different theme current."),
]

WHICH = [
    ("1", "Core recipes", "Blank site and Headless site."),
    ("2", "Starter theme recipes", "Blog, Agency, Coming Soon, and Software as a Service."),
]

BLANK = [
    ("check", "Content management features", "Enabled, with the admin theme active."),
    ("check", "No site theme", "The front end is yours to design."),
    ("check", "No home page", "No page is set as the home page."),
]

BLANK_USE = [
    ("1", "Build your own theme", "Start from an empty front end."),
    ("2", "Run a decoupled site", "Developers write the pages, and Orchard Core manages the content."),
    ("3", "Change the theme later", "Or start from another recipe and switch themes afterward."),
]

HEADLESS = [
    ("check", "Content management features", "Enabled, with the admin theme active."),
    ("check", "GraphQL", "A content API for other applications."),
    ("check", "OpenID Connect", "Authorization server, management, and token validation."),
    ("check", "Admin as the home page", "The home page of the site is the admin."),
]

HEADLESS_ROLE = [
    ("1", "View all content", "Permission granted to the Authenticated role."),
    ("2", "Execute GraphQL.", "Permission granted to the Authenticated role."),
    ("3", "Execute Api - All queries", "Permission granted to the Authenticated role."),
    ("4", "Where to review", "Access Control, Roles."),
]

BLOG_OPTIONAL = [
    ("1", "SQL", "Blog - SQL Search and Blog - SQL Query"),
    ("2", "Lucene", "Blog - Lucene Search and Blog - Lucene Query"),
    ("3", "Elasticsearch", "Blog - Elasticsearch Search and Blog - Elasticsearch Query"),
    ("4", "Where to run them", "Tools, Recipes"),
]

SOON_BEHIND = [
    ("1", "User Registration workflow", "Validates the form, registers a user, and sends a confirmation email."),
    ("2", "Where to find it", "Design, Workflows."),
    ("3", "Features the recipe enables", "Email, Forms, ReCaptcha, Workflows, and Users Registration."),
]

TENANTS = [
    ("1", "Tenants feature", "Enabled by the recipe."),
    ("2", "Multi-Tenancy, Tenants", "Add Tenant opens the form that creates a tenant."),
    ("3", "Recipe list", "Each tenant can start from a recipe of its own."),
]

FITS = [
    ("1", "You design the front end", "Blank site"),
    ("2", "Other applications read your content", "Headless site"),
    ("3", "A theme with sample content", "Blog, Agency, or Coming Soon"),
    ("4", "Several sites in one installation", "Software as a Service"),
]

LATER = [
    ("1", "Tools, Features", "Enable more features."),
    ("2", "Design, Themes", "Make another theme current."),
    ("3", "Tools, Recipes", "Run recipes that are not starter recipes."),
]


def slide(step_id, chapter_title, label):
    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "What a recipe does")
        bullets(image, WHAT)
        return image
    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "You choose once, at setup")
        bullets(image, CHOOSE_ONCE)
        return image
    if step_id == "recipe-3":
        image = slide_base(chapter_title, label, "Which recipes you see")
        bullets(image, WHICH)
        return image
    if step_id == "blank-1":
        image = slide_base(chapter_title, label, "Blank site")
        bullets(image, BLANK)
        return image
    if step_id == "blank-2":
        image = slide_base(chapter_title, label, "When to use Blank site")
        bullets(image, BLANK_USE)
        return image
    if step_id == "headless-1":
        image = slide_base(chapter_title, label, "Headless site")
        bullets(image, HEADLESS)
        return image
    if step_id == "headless-2":
        image = slide_base(chapter_title, label, "Review the Authenticated role")
        bullets(image, HEADLESS_ROLE)
        return image
    if step_id == "blog-4":
        image = slide_base(chapter_title, label, "Optional recipes of The Blog Theme")
        bullets(image, BLOG_OPTIONAL)
        return image
    if step_id == "soon-3":
        image = slide_base(chapter_title, label, "Behind the form")
        bullets(image, SOON_BEHIND)
        return image
    if step_id == "saas-2":
        image = slide_base(chapter_title, label, "Tenants")
        bullets(image, TENANTS)
        return image
    if step_id == "choose-1":
        image = slide_base(chapter_title, label, "Which recipe fits")
        bullets(image, FITS, active={0, 1})
        return image
    if step_id == "choose-2":
        image = slide_base(chapter_title, label, "Which recipe fits")
        bullets(image, FITS, active={2, 3})
        return image
    if step_id == "choose-3":
        image = slide_base(chapter_title, label, "Change course later")
        bullets(image, LATER)
        return image
    raise SystemExit(f"No slide for {step_id}")
