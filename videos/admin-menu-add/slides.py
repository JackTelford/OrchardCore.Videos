"""The slides of the "Add Admin Menu Entries to Your Module" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"INavigationProvider", "IStringLocalizer", "NavigationBuilder", "ValueTask", "AdminMenu", "String",
         "StringComparison", "ActionResult", "Controller", "DemoNavController", "StartupBase", "IServiceCollection",
         "AdminNavigationProvider", "Permissions"}

BUILD = [
    ("1", "My Root View", "A top-level item. It has no page of its own."),
    ("2", "Child One", "Opens the ChildOne action of the DemoNav controller."),
    ("3", "Child Two", "Opens the ChildTwo action of the DemoNav controller."),
]

NEEDS = [
    ("1", "A site", "An Orchard Core CMS site that runs the module."),
    ("2", "A module", "A project that the site references."),
    ("3", "OrchardCore.Admin", "The package that the module references."),
]

POSITIONS = [
    ("1", "Numbers", "Sorted by value: 1, 2, 10."),
    ("2", "before and after", "Anchors at the low end and at the high end of the numbers."),
    ("3", "PrefixPosition()", "Text built from the caption: after the numbers, sorted by the caption text."),
    ("4", "start and end", "First and last of all. Tools is end.50, Settings is end.100."),
]

CONTROLLER = """
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace MyModule.Controllers;

[Admin]
public sealed class DemoNavController : Controller
{
    public ActionResult ChildOne()
    {
        return View();
    }

    public ActionResult ChildTwo()
    {
        return View();
    }
}
"""

VIEW_ONE = """
<p>View One</p>
"""

VIEW_TWO = """
<p>View Two</p>
"""

PROVIDER_CLASS = """
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace MyModule;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    // ...
}
"""

PROVIDER_BUILD = """
public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
{
    // We want to add our menus to the "admin" menu only.
    if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
    {
        return ValueTask.CompletedTask;
    }

    builder
        .Add(S["My Root View"], S["My Root View"].PrefixPosition(), rootView => rootView
            .Add(S["Child One"], S["Child One"].PrefixPosition(), childOne => childOne
                .Action("ChildOne", "DemoNav", new { area = "MyModule" }))
            .Add(S["Child Two"], S["Child Two"].PrefixPosition(), childTwo => childTwo
                .Action("ChildTwo", "DemoNav", new { area = "MyModule" })));

    return ValueTask.CompletedTask;
}
"""

STARTUP = """
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace MyModule;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenu>();
    }

    // ...
}
"""

BACKGROUND_TASKS = """
public sealed class AdminMenu : AdminNavigationProvider
{
    // ...

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Background Tasks"], S["Background Tasks"].PrefixPosition(), backgroundTasks => backgroundTasks
                    .Action("Index", "BackgroundTask", "OrchardCore.BackgroundTasks")
                    .Permission(Permissions.ManageBackgroundTasks)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
"""

# The code panel of the controller and the cards beside it.
CODE_BOX = (96, 240, 1000, 1030)
CARDS_BOX = (1040, 300, W - 96, 1030)


def slide(step_id, chapter_title, label):
    if step_id == "build-1":
        image = slide_base(chapter_title, label, "A root item with two children")
        bullets(image, BUILD)
        return image

    if step_id == "build-2":
        image = slide_base(chapter_title, label, "What the module needs")
        bullets(image, NEEDS)
        return image

    if step_id in ("pages-1", "pages-2"):
        image = slide_base(chapter_title, label, "A controller for the pages")
        if step_id == "pages-1":
            draw_code(image, CODE_BOX, "DemoNavController.cs", "csharp", CONTROLLER, types=TYPES, highlight={6, 7}, size=24)
            note_cards(image, [
                ("Admin theme", "The page uses the admin theme."),
                ("Access admin panel", "The user needs this permission."),
            ], CARDS_BOX)
        else:
            draw_code(image, CODE_BOX, "DemoNavController.cs", "csharp", CONTROLLER, types=TYPES,
                      highlight={9, 10, 11, 12, 13, 14, 15, 16, 17}, size=24)
            note_cards(image, [
                ("ChildOne", "/Admin/MyModule/DemoNav/ChildOne"),
                ("ChildTwo", "/Admin/MyModule/DemoNav/ChildTwo"),
            ], CARDS_BOX)
        return image

    if step_id == "pages-3":
        image = slide_base(chapter_title, label, "Two views")
        draw_code(image, (96, 300, 930, 500), "Views/DemoNav/ChildOne.cshtml", "html", VIEW_ONE, size=28)
        draw_code(image, (990, 300, W - 96, 500), "Views/DemoNav/ChildTwo.cshtml", "html", VIEW_TWO, size=28)
        draw = ImageDraw.Draw(image)
        draw.text((96, 600), "Each view holds one paragraph, so the page shows which item opened it.",
                  font=font(30), fill=TEXT, anchor="lm")
        draw.text((96, 660), "The files are in the Views/DemoNav folder of the module.",
                  font=font(30), fill=MUTED, anchor="lm")
        return image

    if step_id == "provider-1":
        image = slide_base(chapter_title, label, "1. Implement INavigationProvider")
        draw_code(image, (96, 240, W - 96, 1030), "AdminMenu.cs", "csharp", PROVIDER_CLASS, types=TYPES, size=26)
        return image

    if step_id in ("provider-2", "provider-3", "provider-4"):
        headings = {
            "provider-2": "2. Build the admin menu only",
            "provider-3": "3. Add the items to the builder",
            "provider-4": "4. Point the items at the pages",
        }
        lines = {
            "provider-2": {1, 2, 3, 4, 5, 6, 7},
            "provider-3": {9, 10, 11, 13},
            "provider-4": {10, 12, 14},
        }
        image = slide_base(chapter_title, label, headings[step_id])
        draw_code(image, (96, 240, W - 96, 1030), "AdminMenu.cs", "csharp", PROVIDER_BUILD, types=TYPES,
                  highlight=lines[step_id], size=26)
        return image

    if step_id.startswith("position-"):
        index = int(step_id[-1])
        image = slide_base(chapter_title, label, "The order of positions")
        bullets(image, POSITIONS, active={1: {0, 1}, 2: {2}, 3: {3}}[index])
        return image

    if step_id == "register-1":
        image = slide_base(chapter_title, label, "5. Register the provider")
        draw_code(image, (96, 240, W - 96, 1030), "Startup.cs", "csharp", STARTUP, types=TYPES, highlight={11}, size=28)
        return image

    if step_id.startswith("beyond-"):
        index = int(step_id[-1])
        headings = {
            1: "The base class",
            2: "Adding to an existing menu",
            3: "Limiting who sees an item",
        }
        lines = {1: {1, 5}, 2: {8, 9, 10}, 3: {11}}
        image = slide_base(chapter_title, label, headings[index])
        draw_code(image, (96, 240, W - 96, 1030), "OrchardCore.BackgroundTasks/AdminMenu.cs", "csharp",
                  BACKGROUND_TASKS, types=TYPES, highlight=lines[index], size=24)
        return image

    raise SystemExit(f"No slide for {step_id}")
