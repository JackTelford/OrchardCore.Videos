# Open questions: custom-settings

The narration states only what the source and the documentation say. These points need a person to confirm them on a
running site (Orchard Core source `src/OrchardCore.Modules/`, the commit the content was written from; a path that
starts with `src/` is relative to the checkout) before or while recording. Each one says what was checked and what to look at. If something differs, change the narration and the
recording plan, and run `python tools/validate_content.py custom-settings` again.

## Things that decide how a step is recorded

1. **How long a notification stays on screen.** Steps `enable-2`, `type-3`, `fields-2`, `fields-3`, `opts-2`,
   `stereo-2`, `edit-3`, `perm-4` and `deploy-3` end on a page with a success message. The code only adds the message
   (`OrchardCore.Features/Controllers/AdminController.cs:317`, `OrchardCore.ContentTypes/Controllers/AdminController.cs:162,225,768`,
   `OrchardCore.Settings/Controllers/AdminController.cs:119`, `OrchardCore.Roles/Controllers/AdminController.cs:213`,
   `OrchardCore.Deployment/Controllers/StepController.cs:112`). Check on the running admin theme whether it stays or
   fades. If it fades before the narration ends, record the click later in the step so the message is on screen when the
   narration ends.
2. **How the admin menu opens.** The menu labels are in the source (`Tools`, `Features`, `Design`, `Content Definition`,
   `Content Types`, `Settings`, `Access Control`, `Roles`, `Deployments`, `Plans`), but how the redesigned admin menu
   expands a group (a click, a flyout) was not checked. If four clicks do not fit in the narration of `type-1` or
   `deploy-1`, expand the groups before the capture, as the plan says.
3. **The Settings entry appears without a restart.** `OrchardCore.CustomSettings/AdminMenu.cs:26-52` builds the entry from
   the content types on each request, so it should be listed in the **Settings** group right after `stereo-2`.
   Check that it is, and where it sits among the sections that other features add (step `edit-1` rings it).
4. **The Features search.** Step `enable-1` expects the search text `Custom Settings` to leave one row. The search is a
   lowercase substring match over the name, the id and the description of each feature
   (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`,
   `OrchardCore.Features/Assets/ts/features-index.ts:88-101`), and no other manifest of the source contains the text
   `custom settings` (Custom User Settings reads "custom user settings"), so one row is expected under the heading
   **Settings** (`OrchardCore.CustomSettings/Manifest.cs`, `Category = "Settings"`). Check that the running site
   shows exactly that row.
5. **Disabling the feature in the preparation.** The Blog recipe enables Custom Settings, and nothing depends on it
   (no other `Manifest.cs` lists it). Check that the **Disable Feature** dialog lists no dependent feature and that
   enabling it again in `enable-2` gives the message for one feature.

## Labels and values to read on screen

6. **Technical Name.** The box is filled by a script on key up and on blur
   (`OrchardCore.ContentTypes/wwwroot/Scripts/autocomplete/autocomplete.js`). In `type-2`, `fields-2` and `fields-3` check
   that it reads `AnnouncementBanner`, `BannerText` and `ShowBanner` before the click on **Create** or **Save**.
7. **Field Type labels.** The radio buttons show the field type names split into words
   (`OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml:62`). Check that they read **Text Field** and **Boolean Field**,
   and that the radio button checked by default (the first one in the list) is not **Text Field**, so `fields-2` has to click it.
8. **The Add Field page title.** The title is **Add New Field To** and the part name in quotes
   (`AddFieldTo.cshtml:6-8,20`). The part is named after the type and has no display name of its own, so the title
   should read `Add New Field To "AnnouncementBanner"` (`DisplayName` falls back to the part name,
   `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63`).
   Check it on screen. The narration does not quote the title.
9. **The five checkboxes of a fresh type.** `OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:83-91` sets
   Creatable, Draftable, Versionable, Listable and Securable when a type is created, and
   `OrchardCore.ContentTypes/Views/ContentTypeSettings.Edit.cshtml` shows them. Check in `opts-1` that all five are
   checked. After `stereo-2`, open the type once more outside the capture: **Creatable**, **Listable**, **Draftable**
   and **Versionable** should be gone and **Securable** should remain
   (`OrchardCore.CustomSettings/Startup.cs:34-40`,
   `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentTypeDefinitionDriverOptions.cs:13`). The narration of
   `opts-2` says the editor hides "most of these options" once a stereotype is saved.
10. **The order of the two saves.** `ContentTypeSettingsDisplayDriver.UpdateAsync` applies the checkboxes with the
    options of the stereotype posted in the same request (`OrchardCore.ContentTypes/Editors/ContentTypeSettingsDisplayDriver.cs:52-100`),
    so a save that both clears the boxes and sets the stereotype leaves them as they were. That is why `opts-2` saves
    before `stereo-1` sets the stereotype. If the list in `opts-2` still offers **Create New Announcement Banner**, the
    boxes were not cleared.
11. **The permission group and row.** Check that the Editor role's page shows one group titled **Custom Settings** with
    `(OrchardCore.CustomSettings)` beside the title, and the row **Manage Custom Settings - Announcement Banner**
    (`OrchardCore.CustomSettings/Permissions.cs:11-13,38`, `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:55-62`),
    and that the checkbox id is `Checkbox.ManageCustomSettings_AnnouncementBanner`.
12. **Roles of the Blog recipe.** The recipe creates Moderator, Editor, Author and Contributor
    (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, `Roles` step). Check that **Editor** is in the list and that
    its **Allow** box for the new permission is unchecked before `perm-4`.
13. **Deployment plan screens.** Check that **Filter** in **Available Steps** leaves one card (**Custom Settings**), that
    the step's editor opens with **Include all custom settings.** checked
    (`OrchardCore.CustomSettings/Deployment/CustomSettingsDeploymentStep.cs:17`), and that the plan lists the step with
    the badge **All** (`CustomSettingsDeploymentStep.Fields.Summary.cshtml:8-12`).

## Statements taken from the documentation or from the code, not seen running

14. **"Templates and code can read them from the site settings"** (`edit-3`). It comes from the documentation page
    (`src/docs/reference/modules/CustomSettings/README.md`: the Liquid `Site.Properties` object and
    `GetCustomSettingsAsync`, which exists in `src/OrchardCore/OrchardCore.Contents.Core/SiteServiceExtensions.cs:14`). The video shows
    no template and no front-end page. Nobody has run a template that reads these values for this video.
15. **"Marking a type that already has content items breaks those items"** (`idea-4`). It comes from the warning in the
    same documentation page. The code was not traced to confirm it.
16. **"No role receives it automatically, but the Administrator role has every permission"** (`perm-1`). The first half
    follows from `GetDefaultStereotypes` returning nothing (`OrchardCore.CustomSettings/Permissions.cs:40-41`). The second
    half is the text the role editor shows for the Administrator role
    (`OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:32`). Open the Administrator role once to see that text. The name of that role defaults to Administrator and a site can change
    it (`src/OrchardCore/OrchardCore.Roles.Core/SystemRoleOptions.cs:8`); the recorded site keeps the default.
17. **"The step carries the values only. To move the content type as well, add an Update Content Definitions step"** (`deploy-4`).
    `OrchardCore.CustomSettings/Deployment/CustomSettingsDeploymentSource.cs:17-37` writes the values of each section,
    and the Update Content Definitions step is described in
    `OrchardCore.ContentTypes/Views/Items/ContentDefinitionDeploymentStep.Fields.Thumbnail.cshtml`. Execute the plan once
    outside the capture and look at the exported file to confirm that it holds the values and no content type
    definition. The file is not shown in the video.
18. **"The saved values are stored under that name"** (`type-2`). `CustomSettingsDisplayDriver.cs:70` stores the section in
    the site settings under the technical name of the type. Nothing on screen shows this.
19. **"They do not appear in the list of content items"** (`idea-3`). The values are put into the site settings as a content
    item object that is never saved as a content item (`OrchardCore.CustomSettings/Drivers/CustomSettingsDisplayDriver.cs:70`
    assigns it to the site properties; `CustomSettingsService.GetSettingsAsync` only calls `NewAsync`). So the list
    under Content should not hold it. After `04-edit`, open the content items list once outside the capture and check
    that no Announcement Banner item is there.
20. **"If it is not enabled on your site"** (`enable-1`). The Agency, Blank, Blog and Headless recipes enable Custom Settings
    (`src/OrchardCore.Themes/*/Recipes/*.recipe.json`, the `feature` step), so most sites have it on. The video shows the
    steps for a site where it is off; the preparation of the recording plan turns it off.
