# Open questions: Add Site Settings to Your Module

Everything in the narration was read from the Orchard Core source at commit 92db4a83. Nothing was run. A person
with a running site (or a voice sample) must confirm the points below before or while recording. File references are
in `src/OrchardCore.Modules/OrchardCore.Settings/` unless a path says otherwise.

## On a running site (clip `05-try`)

1. **The admin menu.** `AdminMenu.cs:36-53` adds **Settings** with **General** and **Debugging**, and
   `TheAdmin/Views/NavigationItem-admin.cshtml` renders every group collapsed. Confirm that the Settings button
   matches `#adminMenu button.nav-group-label:has(span.title:text-is("Settings"))`, that the Debugging entry matches
   `#adminMenu a.item-label[href$="/Admin/Settings/debugging"]`, and that the group stays collapsed between takes (the
   menu script restores what the last session left open). Many modules add entries under Settings, so the Debugging entry may be below the fold of the 864 px viewport: confirm
   that scrolling the menu (`#left-nav`) brings it into view before the highlight. If the menu cannot be driven, open
   `/Admin/Settings/debugging` with `NavigateAsync` and change the narration of `try-1`.
2. **The message closes after 5 seconds.** `Controllers/AdminController.cs:119` calls `SuccessAsync` with a localized
   message, and that overload sets `DismissalMilliseconds = 5000`
   (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:70-71`). `TheAdmin/Views/Message.cshtml:11-20`
   turns it into `data-bs-autohide="true"`, and `TheAdmin/wwwroot/js/notify-messages/NotifyMessages.js` removes the
   element when it is hidden. So **Site settings updated successfully.** is on screen for about 5 seconds after the
   redirect, not until the end of the step. The plan saves in `try-4` and rings the message for 2 seconds. Confirm
   on a take that the message is on screen while the narration of `try-4` says "adds the message Site settings
   updated successfully" (about 3 to 6 seconds into the step at the narration's pace), and add a pause before the
   click if it is not.
3. **The label locator.** `GetByLabel("Write shape debug information")` should find the check box, because the label
   uses `asp-for` (`Views/DebugSettings.Edit.cshtml:7`). If it does not, use the `id` the label points to.
4. **The state after Save.** `AdminController.cs:122` redirects to the same group, and the editor reads the saved
   section, so the box should be ticked. Confirm it on screen.

## By ear (narration)

5. **New entries of `spoken` in `storyboard.json`.** `narrate.py` (`spoken()`) replaces them as plain text, in the order
   of the map, so a name that contains another one must come first: the map lists `DebugSettings_Edit` before
   `DebugSettings`, `GetSettingsAsync` before `GetSettings`, and `AddSiteDisplayDriver` before `SiteDisplayDriver`.
   Keep that order if you add an entry. Check them
   with `scripts/pronounce.py` before narrating: "Settings Group I.D." (`SettingsGroupId`), "I Site Service"
   (`ISiteService`), "Get Settings Async" (`GetSettingsAsync`), "On Group" (`OnGroup`), "Request Update"
   (`RequestUpdate`), and the other identifiers written as separate words. Also listen to "the localizer, T" in
   `view-1`, to "JSON" in `class-1`, and to "drop-down" in `class-2`.

## Claims taken from the code only (never run)

6. **`reg-1`: the group permission.** `AddSiteSettingsPermission` (`src/OrchardCore/OrchardCore.Settings.Core/SiteSettingsServiceCollectionExtensions.cs`)
   feeds `Services/SiteSettingsAuthorizationHandler.cs`, which `Controllers/AdminController.cs:53` and `:85` call with
   the group identifier. The narration says only that a user who holds that permission can open the group without
   holding `ManageSettings`. It does not say that the permission is enough on its own: every admin page also needs
   `AccessAdminPanel` (`src/OrchardCore.Modules/OrchardCore.Admin/AdminFilter.cs:58`).
   At this commit only the Settings and Email modules call `AddSiteSettingsPermission`; the ReCaptcha module, for
   example, registers `ManageReCaptchaSettings` as a permission and an admin menu entry but not for its group
   (`OrchardCore.ReCaptcha/Startup.cs`), although the Settings documentation says a module that contributes a
   group must register it. A role with only `ManageReCaptchaSettings` would then get a 403 from the shared
   controller. Confirm with a test role on a running site; it decides whether the documentation or the module is wrong.
7. **`drv-2`: "Without OnGroup, the page of a group skips the shape."** From `src/OrchardCore/OrchardCore.DisplayManagement/Views/ShapeResult.cs:418-434`
   (a shape with no group constraint is skipped when a group is requested). The same file (`:409-416`) shows that a
   placement rule can also give a shape a group, which the video does not cover. Confirm in a scratch module by
   removing `.OnGroup(...)` from a driver.
8. **`drv-4`: rejecting a value.** From `Controllers/AdminController.cs:106-130` (nothing is saved and the form is
   returned when the model state is invalid), `Views/Admin/Index.cshtml:9` (`ValidationSummary`) and
   `src/OrchardCore/OrchardCore.DisplayManagement/Entities/SectionDisplayDriverBase.cs` (the section is written back
   only when the model state is valid). The Debugging group has no validation to show, so the slide uses a model
   error from the General group. Confirm in a scratch module that the message is shown.
9. **`use-2`: the options are built again.** From the comment of `src/OrchardCore/OrchardCore.Abstractions/Options/IOptionsUpdateNotifier.cs`
   (instances wired to a signal-backed change token source are refreshed after the shell scope commits).
10. **`use-3`: Liquid.** `Services/SitePropertiesLiquidMapper.cs` copies only the listed names into
    `Site.Properties`. The video does not show how to read a listed property in a template, because the access path
    through Liquid was not verified.
11. **`use-4`: the recipe.** `Recipes/SettingsStep.cs:119-121` stores a key it does not recognize in the properties
    bag under its own name, and `SiteSettingsPropertyDeploymentSource` (`src/OrchardCore/OrchardCore.Settings.Core/Deployment/`)
    exports a section under the name of its class. Import the recipe of the slide on a scratch site and check that
    the box of the Debugging group is ticked.
