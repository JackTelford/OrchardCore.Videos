"""The slides of the Admin Templates video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = set()

SHAPES_DRAW_THE_ADMIN = [
    ("1", "Shapes draw the admin", "The top bar, the menu and the notifications are shapes."),
    ("2", "A view overrides a shape", "Normally a view file in a custom admin theme."),
    ("3", "A change is a code change", "You change code, then deploy the site."),
]

WRITTEN_IN_THE_ADMIN = [
    ("check", "Written in the admin", "A Liquid template, under Design, Admin Templates."),
    ("check", "Named after a shape", "The name decides which shape it replaces."),
    ("check", "Stored in the database", "No code change, and no deployment."),
]

ADMIN_AND_FRONT_END = [
    ("1", "Admin Templates", "Used on admin pages only."),
    ("2", "Templates", "Used on front end pages only."),
    ("3", "A template wins over a view", "A view file of the same name is ignored."),
    ("4", "Delete it to go back", "Without the template, the view file is used."),
]

NAMING = [
    ("1", "Shape type", "VisitSiteNavbarItem"),
    ("2", "Alternate", "Notification_Header__Mention is the view Notification-Mention.Header.cshtml"),
    ("3", "Model", "In Liquid, the shape is Model, and a template reads its properties."),
]

ADMIN_SHAPES = [
    ("1", "AdminBranding", "The logo and the site name in the top bar."),
    ("2", "VisitSiteNavbarItem", "The Visit Site link in the top bar."),
    ("3", "Message", "One notification."),
    ("4", "NotifyMessages", "The container of the notifications."),
]

RECIPE = """
{
  "steps": [
    {
      "name": "AdminTemplates",
      "AdminTemplates": {
        "VisitSiteNavbarItem": {
          "Description": "Visit Site link with a text label",
          "Content": "[file:text('AdminTemplates/VisitSiteNavbarItem.liquid')]"
        }
      }
    }
  ]
}
"""

DEPLOYMENT = [
    ("1", "All Admin Templates", "A deployment step in the Development category: adds all admin templates to the plan."),
    ("2", "Export templates as files", "Writes each template to a Liquid file, instead of inline."),
    ("3", "The result", "An AdminTemplates recipe step, with each Content pointing to its file."),
]

SAVE = [
    ("1", "Checked on save", "The name and the content are filled in, and no other template has the name."),
    ("2", "Not checked on save", "Whether the Liquid can be parsed."),
    ("3", "Found when the shape is drawn", "A parse error is shown in place of the markup."),
]

PERMISSIONS = [
    ("1", "Manage admin templates", "Granted to Administrator by default."),
    ("2", "Manage Liquid templates", "Granted to Administrator and Editor by default."),
    ("3", "Security critical", "A user needs both to open Admin Templates."),
]

FEATURE_PROFILE = """
[
  {
    "Rule": "Exclude",
    "Expression": "OrchardCore.AdminTemplates"
  }
]
"""


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Changing the admin the usual way")
        bullets(image, SHAPES_DRAW_THE_ADMIN)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Admin Templates")
        bullets(image, WRITTEN_IN_THE_ADMIN)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "The admin and the front end stay apart")
        bullets(image, ADMIN_AND_FRONT_END)
        return image

    if step_id == "shapes-1":
        image = slide_base(chapter_title, label, "A template is named after a shape")
        bullets(image, NAMING)
        return image

    if step_id == "shapes-2":
        image = slide_base(chapter_title, label, "Admin shapes the documentation overrides")
        bullets(image, ADMIN_SHAPES)
        return image

    if step_id == "ship-1":
        image = slide_base(chapter_title, label, "The AdminTemplates recipe step")
        draw_code(image, (96, 240, W - 96, 900), "recipe.json", "json", RECIPE, types=TYPES, size=28)
        return image

    if step_id == "ship-2":
        image = slide_base(chapter_title, label, "The All Admin Templates deployment step")
        bullets(image, DEPLOYMENT)
        return image

    if step_id == "good-1":
        image = slide_base(chapter_title, label, "What Save checks")
        bullets(image, SAVE)
        return image

    if step_id == "good-2":
        image = slide_base(chapter_title, label, "Two permissions")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "good-3":
        image = slide_base(chapter_title, label, "Keep it away from a tenant")
        draw_code(image, (96, 240, 1060, 640), "Rules", "json", FEATURE_PROFILE, types=TYPES, size=28)
        note_cards(image, [
            ("Where", "Multi-Tenancy, Feature Profiles"),
            ("Rules", "Processed in the order given."),
            ("Applies to", "Tenants that use the profile."),
        ], (1100, 240, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
