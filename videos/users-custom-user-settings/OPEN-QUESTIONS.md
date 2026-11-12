# Open questions: Custom User Settings

Everything in the narration was read from the source and the documentation. These points could not be run, so a person
must confirm them on the running site while recording, and correct `storyboard.json` or `recording-plan.md` if the
site differs. Source paths are relative to `src/` of the OrchardCore checkout named in `catalog/features.json`.

1. **Order of the saves in the content type editor** (`section-6`, `section-7`, `section-8`). The source says a new
   type starts with Creatable, Listable, Draftable, Versionable and Securable checked
   (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:85-89`), and that a save which carries the stereotype
   `CustomUserSettings` leaves Creatable, Listable, Draftable and Versionable unchanged, because the stereotype hides
   them (`OrchardCore.ContentTypes/Editors/ContentTypeSettingsDisplayDriver.cs:76-102`, `OrchardCore.Users/Startup.cs:685-693`).
   Securable is not hidden. Confirm: (a) after the first Save, the list row of User Profile has no **Create New User
   Profile** button; (b) after the second Save, opening **Edit** again shows no Creatable, Listable, Draftable or
   Versionable box, and still shows Securable, unchecked. The narration claims that the editor hides those four and
   leaves their values alone once the stereotype is saved.
2. **Stereotype value** (`section-7`). The Stereotype box is a text box with a suggestion list
   (`OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml:77-87`); the suggestion has the value
   `CustomUserSettings` and the label "Custom User Settings". Confirm that typing `CustomUserSettings` is accepted, and
   that the list row then shows a badge with that text (`ContentTypeEntry.DefaultTags.SummaryAdmin.cshtml:15`). The
   narration says "Custom User Settings without spaces"; confirm the voice makes that clear (the screen shows the
   typed value).
3. **Name and order of the tabs** (`user-1`, `user-2`). The source puts the section at `Content:10#<display name>`
   (`OrchardCore.Users/Drivers/CustomUserSettingsDisplayDriver.cs:90`) and the fields of the account in the ungrouped
   zone, whose tab is called "Content" (`OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:14`). Confirm that the tab
   row of the Edit User page has the account tab first and **User Profile** second, that the first one is selected when
   the page opens, and that no other tab is added to the page by other features of the site (the narration only says
   the User Profile tab is beside the tab with the account fields). Also confirm that the Create User page (`user-5`)
   shows the same tab row.
4. **Field editors** (`user-2`, `user-3`). Confirm the labels **Nickname** (text box) and **Newsletter** (check box) in
   the tab, and the selectors given for them in `recording-plan.md`. The wrapper class is built from the part name
   `UserProfile` and the field name through `HtmlClassify`, which splits camel case first, so it should read
   `field-wrapper-user-profile-nickname` and `field-wrapper-user-profile-newsletter`
   (`OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:14-31,77-110`). If a probe shows another spelling, change the
   selectors of both steps.
5. **Who sees the tab** (`perm-3`, `perm-4`). The claims come from the source only: the tab is rendered when the signed-in
   user has the permission `ManageOwnCustomUserSettings_UserProfile`, which Manage Users implies
   (`OrchardCore.Users/CustomUserSettingsPermissions.cs:10-13`, `OrchardCore.Users/Drivers/CustomUserSettingsDisplayDriver.cs:90-92`),
   and the pages that edit a user have their own permissions (`OrchardCore.Users/Controllers/AdminController.cs:325-355`).
   The clip does not sign in as a second user. If possible, sign in as `priya` after `05-permissions` and confirm that
   the User Profile tab appears on the profile page (the **Profile** entry of the user menu), and that it is missing
   before the permission is allowed. Also confirm the group title and the row text on the Edit Role page.
6. **Deployment** (`deploy-1` to `deploy-4`). Confirm the labels in the **Available Steps** window (the card
   **Custom User Settings**, its button **Add**), that **Include all custom user settings.** is checked when the step
   page opens, and that the plan then lists the step with the badge **All**. The statement that clearing the box shows a
   check box `UserProfile` is read from the view (`CustomUserSettingsDeploymentStep.Fields.Edit.cshtml:16-19`); the
   recording does not clear the box (see question 9). The statement that an import fills in the sections of the users
   with the same user ID and skips the others is read from `OrchardCore.Users/Recipes/CustomUserSettingsStep.cs:37-48`;
   the plan is not executed in the recording. If a person runs a plan on a second site, confirm it.
7. **Seeded users** (all clips). Confirm that the Users list shows no **Unconfirmed** badge next to `jamie` and `priya`
   (the badge is drawn by `OrchardCore.Users/Views/UserFields.cshtml` for unconfirmed addresses), and that the list uses
   the default List layout, with the **Edit** button in the row.
8. **Timing**. Several steps combine two or three page loads with a narration of about seven seconds (`perm-1`,
   `deploy-1`, `section-4`, `section-5`). If the actions do not fit once `durations.json` exists, shorten the pauses
   first, and only then ask for a longer narration.
9. **The Include all custom user settings box and the admin menu** (`deploy-3`). The check box has
   `data-bs-toggle="collapse" data-bs-target=".collapse"`
   (`OrchardCore.Users/Views/Items/CustomUserSettingsDeploymentStep.Fields.Edit.cshtml:8-9`). That selector matches every
   element with the class `collapse` on the page, and the groups of the admin menu are `ul.collapse` too
   (`OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml:93`). Bootstrap toggles all the elements a collapse
   toggle targets, so clearing the box may also open or close menu groups. This was read from the source and not run.
   Confirm on the running site; if the menu stays as it is, the clip may clear the box, wait until `.collapse.show` is
   visible, show the `UserProfile` check box, and check the box again before **Create**. Until then the plan only
   highlights the label.
