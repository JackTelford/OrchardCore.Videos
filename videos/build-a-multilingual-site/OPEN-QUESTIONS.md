# Open questions: Build a Multilingual Site

Everything in the narration was read in the Orchard Core source (commit 92db4a83) and in the documentation pages of the
tracker entry. The points below were **not** seen on a running site. Confirm each one in the first take, and change the
narration or the plan if the site behaves differently. Source paths are relative to `src/OrchardCore.Modules/` unless
they start with `src/`.

## Things the narration states that depend on the machine

1. **The first row of the culture table (`cultures-1`, "Here it is en-US").** The table starts from
   `CultureInfo.InstalledUICulture` (`OrchardCore.Localization/Models/LocalizationSettings.cs:97-108`), the language the
   operating system was installed with, because neither the Blog recipe nor the setup saves culture settings
   (checked: `OrchardCore.Settings/Recipes/SettingsStep.cs`, `OrchardCore.Setup`). This is not necessarily the display
   language chosen in the Windows settings. Confirm the row reads `en-US` on the recording machine. If it does not,
   record on a machine installed with English (United States); changing the display language may not change the value.
2. **The text "Current Culture: English (United States) | en-US" and the menu entries "French (France) | fr-FR" and
   "français (France)".** They are built from `CultureInfo.DisplayName` and `NativeName`
   (`OrchardCore.ContentLocalization/Views/LocalizationPart.Edit.cshtml:12`, `OrchardCore.ContentLocalization/Views/ContentCulturePicker.cshtml:2,16`).
   The English names depend on the UI culture of the process and on the culture data of the machine. Confirm the exact
   texts; the narration says "French (France)", "English (United States)" and "français (France)".
3. **The drop-down of the cultures table.** The labels "Culture", "Default culture" and "Add culture" come from
   `OrchardCore.Localization/Services/LocalizationJSLocalizer.cs:16-20`, and the layout (the table with the columns
   Culture, Default culture and an empty one, then a native drop-down whose entries read `fr-FR (French (France))`, then
   the green **Add culture** link) from the built script
   `OrchardCore.Localization/wwwroot/Scripts/localization-settings/localization-settings.js`. The TypeScript source of the
   component is not part of the checkout. Confirm on screen that the layout is as described. The recorder cannot draw
   the open list of a native drop-down (`SelectAsync` only changes the value), so `cultures-2` says "choose fr-FR in
   the list of cultures" and does not say that the list opens.

## Behavior to confirm on the site

4. **How The Default Theme draws this site.** `src/OrchardCore.Themes/TheTheme/Views/Layout.cshtml:13,54` draws the
   navigation bar shape, which carries the culture picker, and `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`
   does not. What was not seen: how the Blog recipe's home page, the About article (title, text, subtitle, image) and the
   main menu look under The Default Theme. If a page looks broken, say so before assembling; the narration of `try-3`
   describes only "its own title and text".
5. **Where the French page lands.** The picker redirects with `Url.Action("Display", "Item", ...)`
   (`OrchardCore.ContentLocalization/Controllers/ContentCulturePickerController.cs:70`). Read in the source:
   `OrchardCore.Autoroute/Routing/AutorouteValuesAddressScheme.cs` turns that link into the permalink of the item, so the
   address is `/a-propos` for the French page and `/about` for the English one. Not seen on a running site. Confirm that
   the French page opens and shows the French text (the address bar is not recorded).
6. **The toggle label after switching (`try-3`).** The narration says the menu shows "français (France)" on the French
   page. It follows from the request culture: `ContentRequestCultureProvider` is added as an initial provider
   (`OrchardCore.ContentLocalization/Startup.cs:69`) and returns the culture of the translation found for the address
   (`/a-propos`, see 5), whatever the settings. The cookie of the picker (**Set Cookie**, on by default) is only the
   fallback. Confirm the label on the running site. Likewise on the home page after `try-5`, which has no culture of its
   own, the toggle should read "français (France)" because of the cookie.
7. **The Localization part in the Article editor (`translate-2`).** The panel is drawn by `LocalizationPart.Edit.cshtml`
   at the default position of the part, so it should be below the other parts. Confirm that it is reached by scrolling
   and is collapsed at first.
8. **The list of content items (`translate-1`, `translate-7`).** The plan relies on fewer than ten rows, so that About and
   À propos are on the first page and no search is needed. The Blog recipe creates a menu, two taxonomies, a blog, a blog
   post and the About article. If the list holds more or pages differently, restore the seed.
9. **The Culture filter (`translate-8`).** The filter is a bootstrap-select with the title "Culture", the header "Filter by
   culture" and the entries "All cultures", `en-US`, `fr-FR`
   (`OrchardCore.ContentLocalization/Views/ContentsAdminList-LocalizationPartFilter.cshtml:4`,
   `OrchardCore.ContentLocalization/Drivers/LocalizationContentsAdminListDisplayDriver.cs:43`). The query is
   `OrchardCore.ContentLocalization/Services/LocalizationPartContentsAdminListFilterProvider.cs`. Read in the built script
   `OrchardCore.Contents/wwwroot/Scripts/contents-admin-list/contents-admin-list.js`: choosing an entry submits the form
   by itself (`changed.bs.select` on `.selectpicker:not(.nosubmit)`). Not seen: the markup bootstrap-select generates, so
   the plan gives two locators for the toggle button (`.bootstrap-select.culture-selectpicker button.dropdown-toggle` and
   `button[title='Culture']`). Confirm that one of them matches exactly one button.
10. **Saving the type (`part-3`).** `AddPartsToPOST` adds the part to the content definition and redirects
    (`OrchardCore.ContentTypes/Controllers/AdminController.cs:355-376`), so the part is already stored when the page
    returns; **Save** stores the order of the parts and shows "Content type updated successfully." (`:225`). The narration
    only says "Press Save", which holds either way. Nothing to confirm except the two messages.
11. **The messages.** "The feature Content Culture Picker was enabled." lists only the feature that was clicked, not its
    dependencies (read: `OrchardCore.Features/Controllers/AdminController.cs:246,317` and
    `src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:74-80`, which notifies with the features it was
    given). The narration of `enable-3` says only that a message confirms it. Confirm the text, and that the three rows are
    marked Enabled after the restart. The row of Content Culture Picker names only its direct dependencies (Autoroute and
    Content Localization); Localization is behind the "Show indirect dependencies" button, which `enable-2` does not use.
12. **Typing accented characters.** `À propos` and `café-jardin` go through Playwright's keyboard into an input and into
    the WYSIWYG body editor (Trumbowyg). Confirm they appear unchanged; if not, insert the text with
    `scene.Page.Keyboard.InsertTextAsync` and keep the delay for the other fields.
13. **The admin menu.** The labels (**Tools**, **Features**, **Settings**, **Localization**, **Cultures**, **Design**,
    **Content Definition**, **Content Types**, **Themes**, **Content**, **Content Items**, **Content Culture**,
    **Content Culture Picker**) come from the `AdminMenu` classes. The way TheAdmin opens and closes the sections (the
    redesigned menu) was not seen; the plan says "click the entry" for each level.

14. **The entries of the Localizations menu (`translate-4`, `translate-5`).** The view writes the `title` attribute of the
    entries without quotes (`OrchardCore.ContentLocalization/Views/LocalizationPart.SummaryAdminLinks.cshtml:17,21`), so the
    plan selects the entry by its text (`fr-FR`). Confirm that the menu shows "French (France) | fr-FR" with a plus sign,
    and "English (United States) | en-US" with a pencil on the French copy.
15. **The Themes page (`picker-1`).** The card of The Default Theme has two buttons, **Make Current** and **Enable**,
    because the Blog recipe does not enable that theme (`blog.recipe.json:31-85`). Confirm that the locator of the plan
    picks **Make Current** only, and that the page then shows the two messages.

## Pronunciation

16. **`spoken` entries.** `en-US` is spoken as "E.N. U.S.", `fr-FR` as "F.R. F.R." and `a-propos` as "a propos". They
    follow the rule of `references/storyboard.md` but were not checked with `scripts/pronounce.py`. A person must listen
    to `cultures-1`, `cultures-2`, `translate-6`, `translate-7` and `translate-8`, and to "français" and "À propos" in
    `translate-6`, `try-2`, `try-3` and `try-5`.
