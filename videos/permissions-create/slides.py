"""The slides of the permissions-create video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is trimmed from the Roles module and the documentation of the Roles module (// ... marks what is left out).
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "Permission", "LocalizationSource", "PermissionStereotype", "IPermissionProvider", "IEnumerable", "Task",
    "IAuthorizationService", "Controller", "IActionResult", "StartupBase", "IServiceCollection", "NavigationBuilder",
    "AdminNavigationProvider", "ValueTask", "OrchardCoreConstants", "MyPermissions", "WidgetsController",
    "Permissions", "Startup", "AdminMenu",
}

# The panel of a slide with notes: code on the left, note cards on the right.
CODE_BOX_X1 = 1250
NOTES_X0 = 1290

IDEA_PARTS = [
    ("1", "Permission", "A name that stands for one right, such as ManageWidgets."),
    ("2", "Role", "Holds a set of permission names."),
    ("3", "User", "Receives the permissions of every role the user belongs to."),
]

IDEA_ROLES = [
    ("1", "Anonymous", "Applies to every visitor."),
    ("2", "Authenticated", "Applies to every signed-in user."),
    ("3", "Administrator", "Users in this role pass every permission check."),
]

IDEA_JOBS = [
    ("1", "Declare", "A Permission with a unique name and a description."),
    ("2", "Provide", "An IPermissionProvider, registered in Startup."),
    ("3", "Check", "IAuthorizationService.AuthorizeAsync, where the code needs it."),
]

DECLARE = """
using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace MyModule;

public static class MyPermissions
{
    public static readonly Permission ManageWidgets = new(
        "ManageWidgets",
        LocalizationSource.Create("Manage widgets", typeof(MyPermissions)));
}
"""

DECLARE_OPTIONS = """
public static class MyPermissions
{
    public static readonly Permission ManageWidgets = new(
        "ManageWidgets",
        LocalizationSource.Create("Manage widgets", typeof(MyPermissions)),
        isSecurityCritical: true);

    public static readonly Permission ViewWidgets = new(
        "ViewWidgets",
        LocalizationSource.Create("View widgets", typeof(MyPermissions)),
        [ManageWidgets]);
}
"""

DECLARE_CATEGORY = """
public static readonly Permission ManageWidgets = new(
    "ManageWidgets",
    LocalizationSource.Create("Manage widgets", typeof(MyPermissions)))
{
    Category = "Widgets",
};
"""

PROVIDER = """
using OrchardCore;
using OrchardCore.Security.Permissions;

namespace MyModule;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
        [MyPermissions.ManageWidgets, MyPermissions.ViewWidgets];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _allPermissions,
        },
    ];
}
"""

STARTUP = """
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace MyModule;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
    }
}
"""

RECIPE = """
{
  "steps": [
    {
      "name": "Roles",
      "Roles": [
        {
          "Name": "Editor",
          "Description": "Grants users the ability to edit content.",
          "PermissionBehavior": "Add",
          "Permissions": [ "ManageWidgets" ]
        }
      ]
    }
  ]
}
"""

CONTROLLER = """
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyModule;

public sealed class WidgetsController(IAuthorizationService authorizationService) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!await authorizationService.AuthorizeAsync(User, MyPermissions.ManageWidgets))
        {
            return Forbid();
        }

        // ...
        return View();
    }
}
"""

VIEW = """
@using Microsoft.AspNetCore.Authorization
@using MyModule
@inject IAuthorizationService AuthorizationService

@if (await AuthorizationService.AuthorizeAsync(User, MyPermissions.ManageWidgets))
{
    <a asp-action="Create" class="btn btn-secondary">@T["Add Widget"]</a>
}
"""

MENU = """
using OrchardCore.Navigation;

namespace MyModule;

public sealed class AdminMenu : AdminNavigationProvider
{
    // ...

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Widgets"], widgets => widgets
                .Action("Index", "Admin", "MyModule")
                .Permission(MyPermissions.ManageWidgets)
            );

        return ValueTask.CompletedTask;
    }
}
"""


def code_height(code, size):
    """The height of a code panel that holds the code: the window bar, the lines, and a margin, down to y = 1030 at most."""
    return min(74 + len(code.strip("\n").split("\n")) * int(size * 1.42) + 36, 790)


def with_notes(image, title, code, language, notes, highlight, size=24, types=TYPES, wide=False):
    """A code panel on the left, cards on the right. wide: a wider panel, for long lines."""
    code_x1, notes_x0 = (1330, 1370) if wide else (CODE_BOX_X1, NOTES_X0)
    draw_code(image, (96, 240, code_x1, 240 + code_height(code, size)), title, language, code,
              types=types, highlight=highlight, size=size)
    note_cards(image, notes, (notes_x0, 240, W - 96, 1030))
    return image


def slide(step_id, chapter_title, label):
    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Permissions, roles and users")
        bullets(image, IDEA_PARTS)
        return image
    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Roles that apply without an assignment")
        bullets(image, IDEA_ROLES)
        return image
    if step_id == "idea-3":
        image = slide_base(chapter_title, label, "Three jobs for a module")
        bullets(image, IDEA_JOBS)
        return image

    if step_id == "declare-1":
        image = slide_base(chapter_title, label, "Declare it in a static class")
        return with_notes(image, "MyPermissions.cs", DECLARE, "csharp", [
            ("A name", "What roles store."),
            ("A description", "Shown in the roles editor."),
        ], highlight={6, 7, 8, 9, 10, 11})
    if step_id == "declare-2":
        image = slide_base(chapter_title, label, "The name is the identity")
        return with_notes(image, "MyPermissions.cs", DECLARE, "csharp", [
            ("Unique", "Across every module."),
            ("Ignoring case", "ManageWidgets, managewidgets."),
            ("Duplicates", "Logged, and flagged."),
        ], highlight={8, 9})
    if step_id == "declare-3":
        image = slide_base(chapter_title, label, "A context for translation")
        return with_notes(image, "MyPermissions.cs", DECLARE, "csharp", [
            ("Declaring type", "typeof(MyPermissions)"),
            ("PO context", "MyModule.MyPermissions"),
        ], highlight={1, 10})
    if step_id == "declare-4":
        image = slide_base(chapter_title, label, "A security critical permission")
        return with_notes(image, "MyPermissions.cs", DECLARE_OPTIONS, "csharp", [
            ("isSecurityCritical", "Set to true."),
            ("Roles editor", "Security Critical badge."),
        ], highlight={3, 4, 5, 6})
    if step_id == "declare-5":
        image = slide_base(chapter_title, label, "An implied permission")
        return with_notes(image, "MyPermissions.cs", DECLARE_OPTIONS, "csharp", [
            ("ManageWidgets", "Also grants ViewWidgets."),
            ("ViewWidgets", "Passes for either one."),
        ], highlight={8, 9, 10, 11})
    if step_id == "declare-6":
        image = slide_base(chapter_title, label, "A heading of your own")
        return with_notes(image, "MyPermissions.cs", DECLARE_CATEGORY, "csharp", [
            ("Category", "Groups the permissions."),
            ("No Category", "The feature name is used."),
        ], highlight=None)

    if step_id == "provide-1":
        image = slide_base(chapter_title, label, "List the permissions")
        return with_notes(image, "Permissions.cs", PROVIDER, "csharp", [
            ("IPermissionProvider", "Lists the permissions."),
            ("GetPermissionsAsync", "Returns every one."),
        ], highlight={6, 8, 9, 11, 12}, size=22)
    if step_id == "provide-2":
        image = slide_base(chapter_title, label, "Default roles: stereotypes")
        return with_notes(image, "Permissions.cs", PROVIDER, "csharp", [
            ("Role name", "Editor, in this example."),
            ("Granted when", "Enabled, or role created."),
        ], highlight={14, 15, 16, 17, 18, 19, 20, 21}, size=22)
    if step_id == "provide-3":
        image = slide_base(chapter_title, label, "Stereotypes only add")
        return with_notes(image, "Permissions.cs", PROVIDER, "csharp", [
            ("Add only", "Never removes a permission."),
            ("Administrator", "Passes every check anyway."),
        ], highlight={16, 17, 18, 19, 20}, size=22)
    if step_id == "provide-4":
        image = slide_base(chapter_title, label, "Register the provider")
        return with_notes(image, "Startup.cs", STARTUP, "csharp", [
            ("Startup", "Registers the provider."),
            ("Feature enabled", "Permissions are listed."),
        ], highlight={11}, size=26)
    if step_id == "provide-5":
        image = slide_base(chapter_title, label, "Grant it from a recipe")
        return with_notes(image, "Roles recipe step", RECIPE, "json", [
            ("Replace", "The default behavior."),
            ("Add", "Keeps existing permissions."),
            ("Description", "Set by every entry."),
        ], highlight=None, size=26)

    if step_id == "check-1":
        image = slide_base(chapter_title, label, "Check in a controller")
        draw_code(image, (96, 240, W - 96, 240 + code_height(CONTROLLER, 26)), "WidgetsController.cs", "csharp",
                  CONTROLLER, types=TYPES, highlight={6, 10, 11, 12, 13}, size=26)
        return image
    if step_id == "check-2":
        image = slide_base(chapter_title, label, "What a check accepts")
        return with_notes(image, "WidgetsController.cs", CONTROLLER, "csharp", [
            ("Holds it", "A role of the user has it."),
            ("Implied", "Or a permission implying it."),
            ("Administrator", "Always passes."),
        ], highlight={10, 11, 12, 13}, size=22, wide=True)
    if step_id == "check-3":
        image = slide_base(chapter_title, label, "Check in a view")
        return with_notes(image, "Index.cshtml", VIEW, "cshtml", [
            ("The view", "Hides what is not allowed."),
            ("The controller", "Checks every action."),
        ], highlight=None, size=24)
    if step_id == "check-4":
        image = slide_base(chapter_title, label, "Require it on a menu item")
        return with_notes(image, "AdminMenu.cs", MENU, "csharp", [
            ("Permission", "Set on the item."),
            ("Not allowed", "The item is not shown."),
        ], highlight={12, 13, 14, 15}, size=24)

    raise SystemExit(f"No slide for {step_id}")
