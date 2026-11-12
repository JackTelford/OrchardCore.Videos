"""The slides of the Setup video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

FLOW = [
    ("1", "Open the address", "A site that is not set up shows the Setup screen."),
    ("2", "Answer the questions", "Site, recipe, time zone, database and super user."),
    ("3", "Select Finish Setup", "Orchard Core builds the site from your answers."),
    ("4", "Sign in to the admin", "Use the super user you created."),
]

BUILDS = [
    ("1", "Checks the database", "It tests the connection, then creates its tables."),
    ("2", "Runs the recipe", "It turns on features and can add starting content."),
    ("3", "Saves your answers", "The site name and the time zone become site settings."),
    ("4", "Creates the super user", "The user joins the Administrator role."),
]

SERVER_DATABASES = [
    ("1", "Sql Server, MySql and Postgres", "They use a database server and a database that already exists."),
    ("2", "Connection String", "Required. It tells Orchard Core how to reach the database."),
    ("3", "Table Prefix", "Optional. It lets several sites share one database."),
    ("4", "Table Schema", "Optional. Blank means the default schema of the server."),
]

MESSAGES = [
    ("!", "The connection does not work", "The provided connection string is invalid or server is unreachable."),
    ("!", "The database is taken", "The provided database, table prefix and schema are already in use."),
]

PRESETS = [
    ("1", "Recipe", "The Recipe question is left out."),
    ("2", "Database type", "The question about the type of database is left out."),
    ("3", "Database settings", "Type, connection string, table prefix and schema are all left out."),
]


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "An empty site asks first")
        bullets(image, FLOW, active={0, 1})
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "An empty site asks first")
        bullets(image, FLOW, active={2, 3})
        return image

    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "What Finish Setup does")
        bullets(image, BUILDS)
        return image

    if step_id == "db-2":
        image = slide_base(chapter_title, label, "Databases that use a server")
        bullets(image, SERVER_DATABASES, active={0, 1})
        return image

    if step_id == "db-3":
        image = slide_base(chapter_title, label, "Databases that use a server")
        bullets(image, SERVER_DATABASES, active={2, 3})
        return image

    if step_id == "db-4":
        image = slide_base(chapter_title, label, "When setup can't finish")
        bullets(image, MESSAGES)
        return image

    if step_id == "db-5":
        image = slide_base(chapter_title, label, "Answers your host can preset")
        bullets(image, PRESETS)
        return image

    raise SystemExit(f"No slide for {step_id}")
