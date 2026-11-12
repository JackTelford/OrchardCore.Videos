"""The slides of the User Time Zone and Culture video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import bullets, slide_base

PROBLEM = [
    ("1", "The site's time zone", "Dates and times are shown in the time zone of the site."),
    ("2", "The site's culture", "Month names, dates and numbers are written in the culture the site selects."),
    ("3", "People elsewhere", "An editor in Paris reads Los Angeles time."),
]

FEATURES = [
    ("check", "User Time Zone", "Adds the Default Time Zone field to a user."),
    ("check", "User Localization", "Adds the Default User Culture field to a user."),
    ("check", "Personal", "A choice applies to that user only."),
    ("check", "Optional", "Users who choose nothing keep the site's settings."),
]

TIME_ZONE = [
    ("1", "The user's choice", "Default Time Zone on the page of the user."),
    ("2", "The site's setting", "Default Time Zone in Settings, General."),
    ("3", "The server", "The Local to server option uses the time zone of the server."),
]

CULTURE = [
    ("1", "The user's choice", "Default User Culture on the page of the user."),
    ("2", "Use site's culture", "The site decides, as for a visitor who is not signed in."),
    ("3", "The choices", "The cultures listed in Settings, Localization, Cultures."),
]

WHO = [
    ("1", "Administrators", "Access Control, Users, Edit: any user."),
    ("2", "Users themselves", "Profile in the user menu: Edit own user information and Access admin panel."),
    ("3", "Visitors", "Not signed in: no personal preference applies."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "One site, many places")
        bullets(image, PROBLEM)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Two features, two fields")
        bullets(image, FEATURES)
        return image

    if step_id == "order-1":
        image = slide_base(chapter_title, label, "Which time zone applies")
        bullets(image, TIME_ZONE)
        return image

    if step_id == "order-2":
        image = slide_base(chapter_title, label, "Which culture applies")
        bullets(image, CULTURE)
        return image

    if step_id == "order-3":
        image = slide_base(chapter_title, label, "Who sets the preferences")
        bullets(image, WHO)
        return image

    raise SystemExit("No slide for " + step_id)
