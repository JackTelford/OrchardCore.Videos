# Open questions: permissions-create

The video was written from the Orchard Core source (`main`, commit `92db4a83`) and its documentation. No site was
run and no code was compiled while writing it. Everything below is what a person must confirm on a running site, or
by compiling, before the video is recorded. Paths are relative to `src/` in the Orchard Core checkout.

## On the roles editor (clip `04-roles`)

1. **The group heading.** The narration of `roles-2` says the group shows the feature's name and its identifier. The
   view prints the feature name and, in parentheses, the feature id (`OrchardCore.Modules/OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:57-63`),
   and the feature of the Roles permission provider should be `OrchardCore.Roles` (`Controllers/AdminController.cs:290-319`,
   which takes the last enabled feature of the provider's type). A type without a `Feature` attribute belongs to the
   feature whose id is the module id (`OrchardCore/OrchardCore/Extensions/ExtensionManager.cs:418-422`), here
   `OrchardCore.Roles`, named **Roles** (`OrchardCore.Modules/OrchardCore.Roles/Manifest.cs`), and the documentation
   lists the permission under **Roles (OrchardCore.Roles)**. Confirm on screen that the heading reads **Roles** with
   **(OrchardCore.Roles)**, and not **Roles Core Services**.
2. **The search.** Confirm that typing `roles` in **Search permissions** leaves exactly the group above with the two rows
   **Manage Roles** and **View Roles**, as the recording plan expects. The source has no other description with the
   word "roles" (checked with a text search of every permission description of the modules), but the permissions that
   depend on role names (`OrchardCore/OrchardCore.Users.Core/UsersPermissions.cs`, "Edit users in {0} role") change with
   the roles of the site.
3. **The tooltip of the Effective box.** The **Effective** boxes carry a Bootstrap tooltip, "This permission is implied
   by: Manage Roles" for View Roles on the `Manager` role (`_RolePermissions.cshtml:98` and `:136`). Whether it shows
   when the recorder hovers the box in the headless browser is not known. The narration of `roles-4` does not mention the
   tooltip, so the clip does not need it. If it renders, a hover could be added, and the narration would then have to
   be reviewed.
4. **The seed role.** Confirm that saving the `Manager` role with only **Manage Roles** ticked leaves **View Roles**
   not ticked under **Allow** and ticked under **Effective** (`Controllers/AdminController.cs:451`, `GetEffectivePermissions`,
   uses the authorization service with a user who has the role's claims, and `OrchardCore/OrchardCore.Roles.Core/RolesPermissions.cs:9`
   makes Manage Roles imply View Roles).

## On the behavior the slides describe (read in the code, not run)

5. **Default roles (`provide-2`).** The narration says a role with the name of a stereotype is granted its permissions
   when the feature is first enabled, or when the role is created. This is read in
   `OrchardCore.Modules/OrchardCore.Roles/Services/RoleUpdater.cs` (`InstalledAsync` and `RoleCreatedAsync`, and
   `UpdatePermissions` at line 204, which only adds). To confirm on a site: enable a feature with a permission provider
   that names the Editor role (for example Placements, `OrchardCore.Modules/OrchardCore.Placements/Permissions.cs:27`) on
   a site where Editor exists, and open the Editor role.
6. **The Administrator entry (`provide-3`).** Every built-in provider lists the Administrator role in its stereotypes
   (for example `OrchardCore.Modules/OrchardCore.Roles/Permissions.cs:20`), but the only reader of the stereotypes,
   `RoleUpdater`, skips the administrator role (`Services/RoleUpdater.cs:206-210`), and users in that role pass every check
   through `OrchardCore.Modules/OrchardCore.Settings/Services/SuperUserHandler.cs:35-38`. The narration says only
   that the role needs no entry. If the Administrator entry has a purpose that is not visible in the source (a
   convention, a tool that reads it), someone who knows it should say so, and `provide-3` could name it.
7. **Anonymous and Authenticated (`idea-2`).** The narration says the Anonymous role applies to every visitor, signed in
   or not, and the Authenticated role to every signed-in user (`OrchardCore.Modules/OrchardCore.Roles/Services/RolesPermissionsHandler.cs:43-49`;
   the comment at `Controllers/AdminController.cs:516` says the same). The documentation and the role description say
   "non-authenticated users" for Anonymous: this is reported as a documentation issue. To confirm on a site: grant a
   permission to Anonymous, sign in as a user without that permission, and check the Effective column of that user's role.
   The Roles list that `roles-1` opens shows the description of the Anonymous role, "A system role representing all
   non-authenticated users", while `idea-2` says Anonymous applies to every visitor. The mismatch is known and is
   what the documentation issue reports; the narration follows the code, so do not change it to match the screen.
8. **Duplicate names (`declare-2`).** A duplicate is logged as an error at startup (`OrchardCore/OrchardCore.Infrastructure/Security/OrchardCoreBuilderExtensions.cs:59`)
   and the roles editor shows **Duplicate Permissions Detected** (`Views/Admin/_DuplicatedPermissions.cshtml:4`, filled at
   `Controllers/AdminController.cs:501`). Not run: confirm with two providers that declare the same name.
9. **Menu items (`check-4`).** The navigation drops an item and its children when the user lacks the item's permission
   (`OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:218-253`). Not run.

## On the code of the slides

10. **Compile it.** The code of the slides (`MyPermissions`, `Permissions`, `Startup`, `WidgetsController`, the view and
    `AdminMenu`) follows the Orchard Core source and the documentation, but was not compiled. Put it in a throwaway module
    and build it once. Points worth a look: the collection expression `[ManageWidgets]` in the `Permission` constructor
    (the same form as `RolesPermissions.cs:9`), `new(...) { Category = "Widgets" }` (`Permission.Category` has a public
    setter, `OrchardCore/OrchardCore.Infrastructure.Abstractions/Security/Permissions/Permission.cs:115`), and the
    controller written with a primary constructor.
11. **The recipe slide (`provide-5`).** `PermissionBehavior` is read as a string by the Roles step
    (`OrchardCore/OrchardCore.Abstractions/Json/JOptions.cs:19` registers the string enum converter), and the default
    is Replace (`OrchardCore.Modules/OrchardCore.Roles/Recipes/RolesStep.cs:55`). The step also assigns the description of
    the role from the entry (`RolesStep.cs:53`), which is why the slide's entry has a `Description`. Not run: import the
    recipe of the slide on a site where the `Editor` role exists and open the role: **ManageWidgets** is not a permission
    of any module on the site, so use a permission that exists (for example `ManageRoles`) to see the **Allow** box
    ticked, and check that the other permissions of the role are still ticked.

## On the voice

12. **Pronunciations.** The `spoken` map of `storyboard.json` rewrites identifiers for the voice, for example
    `IPermissionProvider` as "I Permission Provider", `AuthorizeAsync` as "Authorize Async", `ImpliedBy` as "Implied By",
    `LocalizationSource.Create` as "Localization Source dot Create", and "PO files" as "P.O. files". No audio was
    generated while writing: check each with `scripts/pronounce.py`, and have a person confirm the spellings, before
    narrating.
