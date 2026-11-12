# Open questions: localization-cultures

What was checked in the Orchard Core source (commit 92db4a83), and what a person must confirm on a running site before or
while recording. Nothing below is stated in the narration beyond what the source shows; where a sentence leans on
framework behavior or on a recording choice, it is listed here.

## To confirm on the running site

1. **The first culture of a fresh site.** `LocalizationSettings` starts with `CultureInfo.InstalledUICulture` as the
   default and only supported culture (`OrchardCore.Localization/Models/LocalizationSettings.cs`). The narration says
   "the culture of the system the site runs on" (`why-2`, `cultures-2`) and, in `cultures-2`, "English as
   used in the United States" (`french-3` says English back, which needs the first row to be English too). Confirm that the recording machine shows `en-US` in the first row; if not, follow the
   starting-state steps in `recording-plan.md`.
2. **The heading of the page.** `cultures-1` says the page is headed "Settings". The view sets the breadcrumb title to
   "Settings" for every settings group (`OrchardCore.Settings/Views/Admin/Index.cshtml:4`), and the breadcrumb renders the
   title as an `h1` in the admin (`BreadcrumbTagHelper`). Confirm that the page shows "Settings", not "Cultures" or
   "Localization", and what the breadcrumb trail above it shows.
3. **The text of the options.** The list under the table builds each option as the culture name followed by its display
   name in parentheses (`localization-settings.js`). The display name comes from the culture data of the machine, so
   confirm "fr" and "de" read as expected. The recorder selects by value (`fr`, `de`), never by the text.
4. **Fall back to parent culture(s).** `cultures-7` states only the lookup of a text in the parent culture. The code
   copies the setting into `FallBackToParentUICultures` (`Startup.cs:56`), which the portable object localizer reads
   (`PortableObjectStringLocalizerFactory.cs:31`) and uses to try `culture.Parent` when a text has no translation
   (`PortableObjectStringLocalizer.cs:173-183`). The setting is also copied into `FallBackToParentCultures`, an ASP.NET
   Core option that decides which culture a request gets (the comment on the setting says "in case the current culture is
   not determined", `LocalizationSettingsViewModel.cs`); the narration does not state that part, because it is not in this
   source tree. To confirm the narrated part: support `fr` and `fr-CA`, put a text in `fr.po` that `fr-CA.po` lacks, and open
   a page showing it with `?culture=fr-CA`: it should read French with the box checked and English with it unchecked.
   Then confirm the request-culture effect separately before ever narrating it: with `fr` supported and `fr-CA` not, send
   "Accept-Language: fr-CA" with the box checked and unchecked, and compare the `lang` attribute written by
   `TheAdmin/Views/Layout.cshtml`.
5. **The default culture.** The narration says the default is "the culture used when nothing asks for another"
   (`why-2`). The code sets it as the default of the request localization (`Startup.cs`:
   `SetDefaultCulture(defaultCulture)`). Confirm with a request asking for a culture that is not supported (for example
   "Accept-Language: ja") that the page uses the default culture.
6. **The language of the browser, and the culture parameter.** `ask-1`, `ask-2` and `french-3` rely on
   `src/docs/guides/install-localization-files/README.md`, which says a browser set to French gets the French texts and
   that adding the culture parameter to an address simulates it for the current request. Confirm that the request
   culture also follows the browser (the recorder's browser asks for `en-US`), and that the parameter does not change
   the culture of the next request.
7. **The sample French file.** Confirm that, with `fr` supported, every label listed in `french-2` appears in French:
   the heading, the column titles, the Add culture button, the check box label and the Save button. The contexts come from
   `OrchardCore.Localization.pot` and `OrchardCore.Settings.pot`. If a label stays in English, fix the sample file, not the
   narration; if the warning above the form also changes, that is expected. Also confirm that the admin menu stays in
   English (it is not translated by the sample file).
8. **The messages.** `NotifierExtensions.cs` gives `SuccessAsync` a delay of 5000 ms and gives `WarningAsync` none, so the
   green message hides itself and the warning stays. Confirm that the toasts behave so in the recorder (they are drawn by
   `TheAdmin/Views/Message.cshtml`). Both messages come back on the same page after the save (the driver adds the warning
   at `LocalizationSettingsDisplayDriver.cs:132`, the controller adds the green message at `AdminController.cs:119`), so
   `cultures-8` shows the warning too; confirm what the recorder captures at the end of each step.
9. **Selectors.** Confirm with `Recorder probe`: the id of the Enable button (`btn-enable-OrchardCore_Localization`, built
   by `Html.GenerateIdFromName` from the feature id), the ids `tools`, `settings`, `localization` and `cultures` of the
   admin menu entries (`NavigationItem-admin.cshtml` moves the id of an entry with children to its caption), and the
   classes of the editor (`localization-settings-wrapper`, `btn-success`).
10. **Access to the page.** The menu entry and the editor require the permission Manage supported culture
    (`LocalizationPermissions.ManageCultures`); the page itself is also guarded by the settings permission of
    `OrchardCore.Settings` (`AdminController.Index`), and the Localization module registers no settings permission of its
    own. The narration only says that the Administrator role has Manage supported culture by default
    (`OrchardCore.Localization/Permissions.cs`). A role with only that permission was not tested.
11. **Not recorded.** Saving with an empty list is refused with the message "A culture is required"
    (`LocalizationSettingsDisplayDriver.cs:111`), and removing the default culture makes the first culture of the list the
    default (`:121-124`). The second is narrated in `cultures-6` while the trash button is highlighted but not clicked.
    Confirm both on the site if the narration is ever extended.

## Voice

- `culture(s)` is spoken as "cultures" through the storyboard's `spoken` map (the label "Fall back to parent culture(s)"
  keeps its written form in the captions). A person must confirm the spoken form with `pronounce.py` before narrating.
- The narration avoids culture codes; the codes `en-US`, `fr` and `de` are only on screen. Check that "Internationalization",
  "Localization" and "parent culture" are spoken naturally.

## Documentation

The disagreements between the documentation and the code are reported with the result of the writing session
(docIssues), not in `catalog/DOC-ISSUES.md`.
