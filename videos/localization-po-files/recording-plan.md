# Recording plan: Translate the UI: Install and Write PO Files

This plan is for the person who records the video. It needs no decision: every value is given. In this file a selector
stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. Source references are
`file:line` in `OrchardCMS/OrchardCore` (the plan was checked against commit `92db4a83`).

The video has five slide clips (drawn by `slides.py`, nothing to record) and two recordings:

| Chapter | Clip | Kind | Steps |
| --- | --- | --- | --- |
| 01 | `01-po` | slide | `po-1`, `po-2` |
| 02 | `02-install` | slide | `install-1`, `install-2`, `install-3` |
| 03 | `03-enable` | clip | `enable-1` to `enable-5` |
| 04 | `04-query` | slide | `result-1` |
| 04 | `04-result` | clip | `result-2`, `result-3`, `result-4` |
| 05 | `05-where` | slide | `where-1` to `where-4` |
| 06 | `06-write` | slide | `write-1` to `write-5` |

Nothing of Crowdin is recorded: it is a third-party site, its pages change, and the site must not call outside
services. The download and the extraction are shown on slides only.

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore`, run from `src/OrchardCore.Cms.Web` on
  `http://localhost:5010`, with its own empty `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Provisioning**: Auto Setup through environment variables, so no setup or sign-in form is recorded (see the
  `orchardcore-tester` skill of the Orchard Core repository). The password is chosen outside git and kept in
  `VIDEO_PASSWORD`; this plan writes it as `<password>`.

  ```bash
  export ORCHARD_APP_DATA=<empty folder outside both repositories>
  export ASPNETCORE_URLS=http://localhost:5010
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword="<password>"
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9   # a dead address: no outbound call
  cd <OrchardCore>/src/OrchardCore.Cms.Web
  dotnet build -c Release
  # prepare Localization now, as "The stand-in French files" says, then start the site without building again:
  dotnet run -c Release --no-build --no-launch-profile
  ```

  Build first and run with `--no-build`: the build of an application that references the CMS targets can copy translation
  files into `Localization` (see "The stand-in French files"), and a `dotnet run` that builds would copy them again.

  The Blog recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`) does not enable
  `OrchardCore.Localization`, so the site starts without the feature: the first clip enables it.
- **Tenant**: `Default`. **Time zone**: `America/Los_Angeles`. **Culture of the machine**: the Localization feature
  starts with `CultureInfo.InstalledUICulture` as the default and only supported culture, or `en` when the runtime reports
  none (`OrchardCore.Localization/Models/LocalizationSettings.cs:15-27`). That is the language the operating system is
  installed with, which can differ from the display language of the signed-in account. The Cultures table of `03-enable`
  must list `en-US` as the default, so check it first: `[System.Globalization.CultureInfo]::InstalledUICulture.Name`
  in PowerShell must print `en-US`. If it prints another culture, record on a machine where it is `en-US`; do not edit
  the table on camera.
- **Features to enable before recording**: none. `OrchardCore.Localization` is enabled on camera in `03-enable`.
- **Settings to save before recording**: none.
- **External services**: none.

### The stand-in French files

The real French files change with the community translations and cover the whole admin. The recording uses a stand-in
that has the same layout as the downloaded `fr.zip` (a folder named `fr`, one PO file for a module) and translates a
few texts of the Features page. Create it **before the site starts**, in the Orchard Core checkout, never in this
repository:

`<OrchardCore>/src/OrchardCore.Cms.Web/Localization/fr/OrchardCore.Features.po` (the `Localization` folder exists
after a build: `OrchardCore.Cms.Web.csproj:36`), saved as UTF-8:

```po
msgid ""
msgstr ""
"Project-Id-Version: OrchardCore.Features\n"
"Language: fr\n"
"MIME-Version: 1.0\n"
"Content-Type: text/plain; charset=UTF-8\n"
"Content-Transfer-Encoding: 8bit\n"

msgctxt "OrchardCore.Features.Views.Admin.Features"
msgid "Features"
msgstr "Fonctionnalités"

msgctxt "OrchardCore.Features.Views.Admin.Features"
msgid "Search"
msgstr "Rechercher"

msgctxt "OrchardCore.Features.Views.Admin.Features"
msgid "Bulk Actions"
msgstr "Actions groupées"

msgctxt "OrchardCore.Features.Views.Admin.Features"
msgid "Showing {0} of {1} features"
msgstr "{0} sur {1} fonctionnalités affichées"

msgctxt "OrchardCore.Features.Views.FeatureEntry.Buttons.SummaryAdmin"
msgid "Enable"
msgstr "Activer"
```

Before creating it, look at `<OrchardCore>/src/OrchardCore.Cms.Web/Localization` after the build. `OrchardCore.Application.Cms.Targets`
references the `OrchardCore.Translations.All` package (`OrchardCore.Application.Cms.Targets.csproj:42`), and the CMS targets
copy the translation files that a package declares into `Localization` after a build
(`OrchardCore.Application.Cms.Core.Targets.targets:34-44`). The source does not show what that package declares. If the
folder holds a `fr` folder, a `fr.po` file or any other translation, delete everything inside `Localization`: the stand-in
must be the only French file, since the real files translate the whole admin, menu included, and `result-3` would not hold.
Then create the stand-in and start the site with `--no-build`.

The contexts and the original texts are those of `OrchardCore.Features/OrchardCore.Features.pot` (the entries of the
`Views.Admin.Features` and `Views.FeatureEntry.Buttons.SummaryAdmin` contexts). A PO file is read when the dictionary of its culture is first needed and then kept, so create or change
the file before the first request in French. Delete the file, and the `fr` folder, when the recording is over.

There is no seed data to create: the clips show administration pages only, and the Blog recipe's content is never on
screen.

## Order, dependencies and snapshots

1. Start the site with the stand-in file in place, let Auto Setup finish, and stop the site. Copy the `ORCHARD_APP_DATA`
   folder aside as **snapshot 0** (outside both repositories).
2. Run `Recorder setup`, then record `03-enable`. **It changes the site**: it enables Localization and saves the
   cultures (the tenant restarts). To take it again, stop the site, restore snapshot 0, start the site, and run
   `Recorder setup` again.
3. After a good take of `03-enable`, stop the site and copy `ORCHARD_APP_DATA` aside as **snapshot 1**.
4. Record `04-result`. It depends on `03-enable` (Localization enabled, `fr` supported) and **does not change the
   site**: a new take only needs the site running from snapshot 1.

`04-result` needs the stand-in file in place, as does a restored snapshot (the file is not part of `App_Data`).

## Clip `03-enable`

Starts on `/Admin` (`OpenAsync("/Admin")`), the dashboard, English, with the admin menu collapsed. Start the capture
there. The menu is `#left-nav`; a group is opened by clicking its label.

### `enable-1`

- Click the **Tools** label in the admin menu (`#left-nav figcaption#tools`), then click the **Features** link in the
  group that opens (`#left-nav a:has-text("Features")`) with `ClickAndWaitForNavigationAsync`. The page is `/Admin/Features`.
- `TypeAsync(#search-box, "Localization")`, then wait until `#btn-enable-OrchardCore_Localization` is visible.
- On screen when the narration ends: the Features page with `Localization` in the search box, and the row of the
  Localization feature, with its **Enable** button, under the **Internationalization** heading.
- Labels: `OrchardCore.Features/AdminMenu.cs:26` (Tools), `AdminMenu.cs:27` (Features),
  `OrchardCore.Features/Views/Admin/Features.cshtml:27` (the search box, `T["Search"]`, English when the page has no culture).
  The other rows that match the search (for example Content Localization) may stay in the list; that is expected.

### `enable-2`

- `HighlightAsync(#btn-enable-OrchardCore_Localization)`, hold it for about 1.5 seconds, `ClearHighlightsAsync()`, then
  `MoveToAsync(#btn-enable-OrchardCore_Localization)` (on camera), and run the click and the reload off camera, because
  enabling a feature restarts the tenant and the reload can take several seconds:

  ```csharp
  await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(
      () => scene.Page.Locator("#btn-enable-OrchardCore_Localization").ClickAsync()));
  ```

  `WaitForNavigationAsync` inside `OffCameraAsync` restarts the screencast on the new page, so the clip is not frozen.
- Then wait until `#btn-disable-OrchardCore_Localization` is in the page (`state: attached`).
- On screen when the narration ends: the Features page again (the search box is empty after the reload), with a
  success notice that the feature was enabled. Do not scroll.
- Labels: `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable);
  `OrchardCore.Localization/Manifest.cs:10-16` (name `Localization`, category `Internationalization`).

### `enable-3`

- Click **Settings** (`#left-nav figcaption#settings`), then **Localization** (`#left-nav figcaption#localization`),
  then the **Cultures** link (`#left-nav a:has-text("Cultures")`) with `ClickAndWaitForNavigationAsync`. The page is
  `/Admin/Settings/localization`.
- Wait until `.localization-settings-wrapper table` is visible and has a row for `en-US` (the editor is drawn by a script).
- On screen when the narration ends: the Cultures page with its table (columns **Culture** and **Default culture**),
  one row `en-US`, the culture list with an **Add culture** button, and the **Fall back to parent culture(s)** box,
  cleared. Above the editor there is also a yellow alert paragraph, `The website might be restarted upon saving the
  settings, potentially leading to temporary unresponsiveness during the process.`: it is not the notice of `enable-5`.
- Labels: `OrchardCore.Localization/AdminMenu.cs:34` (Settings), `:35` (Localization), `:38` (Cultures);
  `Services/LocalizationJSLocalizer.cs:15-20` (Culture, Default culture, Add culture);
  `Views/LocalizationSettings.Edit.cshtml:42` (Fall back to parent culture(s)); the alert paragraph comes from
  `OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`, added by `LocalizationSettingsDisplayDriver.cs:69`.

### `enable-4`

- `SelectAsync(.localization-settings-wrapper select.form-select, "fr")`. The option reads `fr (French)` (the editor
  writes the culture name, then its display name in parentheses: `wwwroot/Scripts/localization-settings/localization-settings.js`,
  the `<option>` of the template; the display name comes from `CultureInfo.DisplayName`, in the UI culture of the request,
  which is English here).
- `ClickAsync(.localization-settings-wrapper a.btn-success)` (**Add culture**), then wait until the table has a second row, `fr`.
- `MoveToAsync(button.save)` (**Save**, on camera), then run the click and the reload off camera, because saving the
  cultures restarts the tenant:

  ```csharp
  await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(
      () => scene.Page.Locator("button.save").ClickAsync()));
  ```

  After the cut the page is the Cultures page again, with the notices of `enable-5`.
- On screen when the narration ends: the Cultures page after the save, the table with `en-US` and `fr`, and `fr` not selected as default (the radio of `en-US` stays on).
- Labels: `Services/LocalizationJSLocalizer.cs:20` (Add culture); `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (Save).

### `enable-5`

- No navigation. Wait until `.toast.message-warning` shows the text `The site has been restarted for the settings to take
  effect.`, then `HighlightAsync(.toast.message-warning)` and hold until the narration ends.
- On screen when the narration ends: the Cultures page after the save, the table with `en-US` and `fr`, and the warning notice
  (it stays: only the success notice, `Site settings updated successfully.`, hides itself after five seconds).
- Labels: `OrchardCore.Localization/Drivers/LocalizationSettingsDisplayDriver.cs:132` (the notice);
  `OrchardCore.Settings/Controllers/AdminController.cs:119` (the success notice).

## Slide `04-query`

- `result-1`: the slide "Test a culture": a code panel with `/Admin/Features?culture=fr`, and two cards, "Query string:
  Applies to one request" and "Supported cultures: fr is listed on Cultures".

## Clip `04-result`

Starts on `/Admin/Features` (`OpenAsync("/Admin/Features")`), in English, from snapshot 1 (or straight after a good
take of `03-enable`). The sidebar is the admin menu, English. Start the capture there.

### `result-2`

- `NavigateAsync("/Admin/Features?culture=fr")`. Wait until `#search-box[placeholder="Rechercher"]` is in the page.
- The narration opens with a sentence that says the recording uses a small French file (about seven seconds): `HoldAsync`
  on the French page with no highlight until it ends, then start the highlights.
- Highlight in this order, about 1.5 seconds each, clearing between them: the page title
  (`.oc-breadcrumb-title:visible`, first match, reads `Fonctionnalités`), the search box (`#search-box`, placeholder
  `Rechercher`), the bulk actions button (`#bulk-action-menu-button`, reads `Actions groupées`), the first enable button
  (`a[id^="btn-enable-"]` first match, reads `Activer`) and the count (`#list-summary`, reads `N sur M fonctionnalités affichées`
  with the real numbers of the list).
- On screen when the narration ends: the Features page in French as listed above, with the highlight on the count.
  `ClearHighlightsAsync()` before the next step.
- Expect the breadcrumb trail above the title (`ol.breadcrumb`) to hold one item only, the page name, in French
  (`Fonctionnalités`), with the French title right under it. `Features.cshtml` declares no ancestor
  (`OrchardCore.Features/Views/Admin/Features.cshtml:15`), and the only breadcrumb provider, which adds a Dashboard item,
  belongs to the Admin Dashboard feature (`OrchardCore.AdminDashboard/Services/DashboardBreadcrumbProvider.cs`), which the Blog recipe
  does not enable. The trail is not built from the admin menu. The narration does not mention it.
- Labels: `OrchardCore.Features/Views/Admin/Features.cshtml:5` and `:15` (the title), `:27` (Search), `:34` (Bulk Actions),
  `:59` (Showing {0} of {1} features); `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable).
  The French texts are those of the stand-in file above.

### `result-3`

- `HighlightAsync(#left-nav)` and hold it until the narration ends.
- On screen when the narration ends: the admin menu in English (**Tools**, **Features**, **Settings**, and the rest
  of the groups), next to the French page. The stand-in file has no entry for the contexts of the menu
  (`OrchardCore.Features/AdminMenu.cs:26-27`, `OrchardCore.Settings/AdminMenu.cs:32` and `:36`), which is what the narration
  says. The community files do translate menu texts, so the narration says "the file", the stand-in, and never "the community files".
- If any menu label shows in French, stop: the stand-in file is not the one in the checkout, or another French file is
  installed. Fix the folders and record again.

### `result-4`

- `ClearHighlightsAsync()`, then `HoldAsync` on the French page until the narration ends. No action.
- On screen when the narration ends: the Features page in French, as in `result-2`.

## Slides

- `po-1`: "The anatomy of a PO entry": a code panel with a comment line, `msgctxt`, `msgid` and `msgstr` of the Search
  entry, and three cards that name the three keywords.
- `po-2`: "One set of files for each culture": three cards (named after a culture, loaded for the request, no translation, no change).
- `install-1`: "Download French from Crowdin": three numbered cards (Select French, Download or Upload, Download translations).
- `install-2`: "Extract it into Localization": a file tree (`MySite/Localization/fr/` and four module files) and three cards.
- `install-3`: "Publish the files": the `PublishTranslationFiles` target of `OrchardCore.Application.Cms.Core.Targets.targets` on top, the
  project file's item group that includes `Localization` as content below it, and three cards (CMS targets, Other projects, Docker).
- `where-1`: "Where Orchard Core looks": four cards of locations, the first three bright.
- `where-2`: the same four cards, the fourth (split by module) bright.
- `where-3`: "Later files replace earlier ones": the four cards, all bright and numbered in load order.
- `where-4`: "Files and cultures": cards for `fr.po`, `fr-CA.po` and Fall back to parent culture(s).
- `write-1`: "The context of a service": the `MyService` class on the left, its PO entry and a card on the right.
- `write-2`: "The context of a view": the `search-box` input of the Features view, the PO entry, and two cards.
- `write-3`: "Entries without a context": an entry without `msgctxt`, an entry with an empty translation, and two cards.
- `write-4`: "One tenant only": a file tree with `Localization/fr.po`, `Localization/fr/` and `App_Data/Sites/Default/Localization/fr.po`, and three cards
  (tenant folder, site folder, culture folder).
- `write-5`: "Placeholders, and restarting": the entry of the Showing {0} of {1} features text, and two cards.

## Checks for private data

- The admin user is `admin` with `admin@example.com`; the password is never typed on camera (the recorder signs in from
  `state.json`), and it is not written in this repository.
- The pages show only administration screens and the feature list: no blog post, no name of a real person, no email other
  than `@example.com`.
- The stand-in file holds only translations of interface texts.
- OCR every frame (one per second, and at every scene change) and search for the machine's user name, the path of the
  checkout, real email addresses and tokens before publishing. The Windows display language is not shown on screen.
- Delete the stand-in file and the `fr` folder from the Orchard Core checkout, and the `App_Data` copies, when the recording is over.
