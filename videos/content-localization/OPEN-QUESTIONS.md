# Open questions: content-localization

What was checked in the Orchard Core source (commit in `catalog/features.json`), and what a person must confirm on a
running site before or while recording. The narration states only what the source shows; each item below is a
behavior or a look the source cannot settle. (The Culture drop-down of the Content Items list reloads the list by
itself: the list script clicks the filter button on every selection, see `manage-4` in recording-plan.md.)

## To confirm before recording

1. **Where the Localization section sits in the Article editor (`original-2`, `translate-1`, `translate-4`).** The
   part is appended after the existing parts of the type
   (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:481-492`), so it should be
   the last block of the editor, collapsed. Confirm that it is below the fold, and how far to scroll.
2. **Permalink of the French copy (`translate-3`, `translate-5`, `site-2`).** The source clears the path of the copy
   (`src/OrchardCore.Modules/OrchardCore.Autoroute/Handlers/AutorouteContentLocalizationHandler.cs:14-15`) and
   generates it from the title on save, in the culture of the item
   (`src/OrchardCore.Modules/OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:397-440`). Confirm that the
   Permalink field is empty in the editor of the copy and that the published address is `/a-propos`. The narration says
   nothing about the address itself; the caption in `site-2` must show the real one.
3. **Default permissions (`perm-3`).** By the source, the roles named Administrator, Editor, Author and Contributor
   receive the default permissions of the module when the feature is installed
   (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Permissions.cs:22-45`,
   `src/OrchardCore.Modules/OrchardCore.Roles/Services/RoleUpdater.cs:44-95`). The handler runs as a deferred task of the
   request that enabled the feature, in a new scope
   (`src/OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:119-128`), and the Administrator role is
   skipped because it has every permission (`RoleUpdater.cs:206-210`). The source does not show the state of the roles
   after the restart, only the code that sets it. Confirm on the Roles page, after
   `02-enable`, that Author has "Localize own content" checked and "Localize content for others" unchecked, that
   Contributor is the same, and that Editor has both. If not, `perm-3` must change.
4. **Names of the cultures.** The editor shows "English (United States) | en-US", "French (France) | fr-FR" and
   "Spanish (Spain) | es-ES" from `CultureInfo.DisplayName`
   (`LocalizationPart.Edit.cshtml:12,34,38`). These names come from the runtime's culture data (ICU); a machine with
   invariant globalization shows other text. Confirm the three names on the recording machine, and in the options of
   the Cultures drop-down ("fr-FR (French (France))").
5. **Seed: starting cultures table.** Without saved settings, the default culture and the one supported culture are
   those of the operating system (`src/OrchardCore.Modules/OrchardCore.Localization/Services/LocalizationService.cs:17-20`),
   so the Cultures page of the seed may start with a row other than en-US. The plan says to remove it.
6. **Admin menu.** The plan clicks Tools, Features; Settings, Localization, Cultures; Design, Content Definition,
   Content Types; Access Control, Roles. The labels are in the menu builders cited in recording-plan.md. How the sidebar
   opens its sections (focused or not) was not read; if clicking through is awkward, open the pages by URL and keep the
   highlight on the menu label.
7. **Features search (`enable-1`).** The search box filters the page as you type. The source does not say whether the
   Content Culture Picker and Localized Content Item Sitemaps rows also appear for "Content Localization" (their
   dependencies name it). The plan keeps the Content Localization row in view either way.
8. **Links that post.** The Localize links carry `data-url-af="UnsafeUrl"` and the action accepts only POST
   (`src/OrchardCore.Modules/OrchardCore.ContentLocalization/Controllers/AdminController.cs:36-38`), so a script turns
   the click into a POST. Confirm that `ClickAndWaitForNavigationAsync` follows it, in the editor card and in the
   **Localizations** menu of the list.
9. **Return to the list.** The **Edit** button of the list passes `returnUrl`, and **Publish** and **Save Draft**
    then redirect to it (`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:858`), also for
    the copy created by Localize (the controller passes `returnUrl` on). Confirm that every step that says "back in the
    list" does return to `/Admin/Contents/ContentItems/Article`.
10. **Voice.** No spoken-form entry was added. Listen to "Localizations", "localization set" and "permalink" in the
    first narration and add a spelling to the storyboard only if the voice gets one wrong.

## Not claimed in the narration

- What a visitor sees in other languages: without the Content Culture Picker feature, the request culture does not
  follow the content item (the provider is registered only by that feature,
  `src/OrchardCore.Modules/OrchardCore.ContentLocalization/Startup.cs:69`), so the video says only that each
  translation has its own address.
- Whether a draft translation can be previewed on the site: not shown and not said.
- Cultures removed from the settings after translations exist, the Lists and Contained-item handlers, the
  localization-set content picker field, the Liquid filter, GraphQL and the sitemap feature: outside this video.
