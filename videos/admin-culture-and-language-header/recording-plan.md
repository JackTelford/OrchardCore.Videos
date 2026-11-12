# Recording plan: Admin Culture Picker and the Content-Language Header

This plan is for the person who records the video. Read it from top to bottom: it lists the site, the seed, the order
of the clips and, for every clip, what to do in every step and what the screen must show when the narration of that
step ends. A selector in this plan stands for `scene.Page.Locator(...)`. A path such as `Localization/AdminMenu.cs:38`
is relative to `src/OrchardCore.Modules/OrchardCore.Localization` of the Orchard Core checkout, unless it starts with
another module name or `src/`. Every label below was read from that source, in the checkout the catalog was made from.

Three clips are recordings (`02-enable`, `03-cultures`, `04-pick`). Chapters 01 and 05 are slides that `slides.py`
draws; they need no recording. The video needs no `Recorder prepare`: the features are enabled on camera.

## The site it needs

- **Run**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with its own
  `ORCHARD_APP_DATA` folder outside both repositories, provisioned by Auto Setup: recipe **Blog**, site name
  **Orchard Core**, time zone `America/Los_Angeles`, tenant `Default`. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead
  address so the site calls nothing outside. The video uses no external service.
- **User**: `admin`, email `admin@example.com`, password `<password>`: the password is chosen outside git and passed in
  `VIDEO_PASSWORD`. Run `Recorder setup` once, before any culture is chosen, and keep that `state.json` (it is
  git-ignored). Never save `state.json` again after the admin culture was changed: it would carry the culture cookie
  into the next take.
- **Features at the start**: the Blog recipe does not enable `OrchardCore.Localization` (its `Recipes/blog.recipe.json`
  in `src/OrchardCore.Themes/TheBlogTheme` has no such step), so the site starts without the features of this video.
  Check on the Features page that Localization, Admin Culture Picker and Content Language Header are all **Disabled**.
- **Machine**: a site without saved localization settings supports one culture, the installed UI culture of the machine,
  which is the language the operating system was installed with, not necessarily the display language chosen by the
  user (`CultureInfo.InstalledUICulture`, `Models/LocalizationSettings.cs:15-26`). Check it before recording:
  `pwsh -NoProfile -Command "[System.Globalization.CultureInfo]::InstalledUICulture.Name"` must print `en-US`. The
  narration of `cultures-1` says the site supports a single culture. After `02-enable` open Settings, Localization,
  Cultures: the table must list only `en-US`, and it must be the default. If it lists another culture, fix the machine,
  not the site.
- **Notifications**: a success notification stays on screen for 5 seconds after the page loads and is then removed
  (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, `TheAdmin/Views/Message.cshtml:11-20`,
  `TheAdmin/Assets/js/NotifyMessages/NotifyMessages.ts`); a warning notification has no delay and stays until it is
  closed. Both are a `div.toast` in `.oc-notify-toast-container`, with the class `message-success` or `message-warning`.
  The clips below say where the timing matters.
- **Browser**: the recorder's Chromium uses the locale `en-US`, so the front end is served in `en-US`, the default
  culture, in every clip.
- **Fake data**: the Blog recipe's sample content. No other record is created. Nothing in this video needs a real name.

### Seed: a small French translation file (outside the capture)

The source tree and the Blog recipe install no translation, so without a file the admin stays in English after the
switch. The seed is one small PO file with a few labels of the pages on screen.

1. Start the site once and let Auto Setup finish, then stop it (the site must be stopped: the docs say a PO file is read
   again only after a restart).
2. Create the folder `Sites/Default/Localization` inside the `ORCHARD_APP_DATA` folder and write `fr.po` in it, saved as
   UTF-8 (it holds accented letters), with the
   text below. The location is `ORCHARD_APP_DATA/Sites/<tenant name>/Localization/<culture name>.po`
   (`Localization/ModularPoFileLocationProvider.cs:45,65`; `src/OrchardCore/OrchardCore/Shell/ShellOptionsSetup.cs`).
   The file is named after the culture the picker sets (`fr`), exactly: the lookup uses the culture name.
3. Take the first snapshot of `ORCHARD_APP_DATA` (see "Snapshots"), then start the site.

The entries have no `msgctxt`, so each one applies to every view and service that uses the label
(`src/OrchardCore/OrchardCore.Localization.Core/PortableObject/PortableObjectStringLocalizer.cs:224`). A label without an
entry stays in English (same file, line 50). The seed does not translate "Visit Site" (the selector of `pick-4`
relies on its icon, and the tooltip stays readable) or the yellow warning of the Cultures page (`pick-3` shows it).

```po
msgid ""
msgstr ""
"Content-Type: text/plain; charset=UTF-8\n"
"Language: fr\n"

msgid "Content"
msgstr "Contenu"

msgid "Content Items"
msgstr "Éléments de contenu"

msgid "Content Definition"
msgstr "Définition du contenu"

msgid "Design"
msgstr "Conception"

msgid "Themes"
msgstr "Thèmes"

msgid "Menus"
msgstr "Menus"

msgid "Widgets"
msgstr "Widgets"

msgid "Media"
msgstr "Médias"

msgid "Security"
msgstr "Sécurité"

msgid "Users"
msgstr "Utilisateurs"

msgid "Roles"
msgstr "Rôles"

msgid "Settings"
msgstr "Paramètres"

msgid "General"
msgstr "Général"

msgid "Localization"
msgstr "Localisation"

msgid "Cultures"
msgstr "Cultures"

msgid "Tools"
msgstr "Outils"

msgid "Features"
msgstr "Fonctionnalités"

msgid "Deployments"
msgstr "Déploiements"

msgid "Recipes"
msgstr "Recettes"

msgid "Save"
msgstr "Enregistrer"

msgid "Define which cultures are supported by the site."
msgstr "Définissez les cultures prises en charge par le site."

msgid "Culture"
msgstr "Culture"

msgid "Default culture"
msgstr "Culture par défaut"

msgid "Add culture"
msgstr "Ajouter une culture"

msgid "Fall back to parent culture(s)"
msgstr "Utiliser la ou les cultures parentes"

msgid "Site settings updated successfully."
msgstr "Les paramètres du site ont été mis à jour."
```

Where each English label comes from (all of them are `S["..."]`, `H["..."]` or `T["..."]` strings, matched by the text
only):

| Label | Source |
|---|---|
| Content, Content Items | `OrchardCore.Contents/AdminMenu.cs:65,68` |
| Content Definition, Design | `OrchardCore.ContentTypes/AdminMenu.cs:24,23` |
| Themes | `OrchardCore.Themes/AdminMenu.cs:21` |
| Menus | `OrchardCore.Menu/AdminMenu.cs:34` |
| Widgets | `OrchardCore.Layers/AdminMenu.cs:27` |
| Media | `OrchardCore.Media/AdminMenu.cs:18` |
| Security | `OrchardCore.Cors/AdminMenu.cs:19` (also in the Users and Roles menus) |
| Users, Roles | `OrchardCore.Users/AdminMenu.cs:30`, `OrchardCore.Roles/AdminMenu.cs:19` |
| Settings | `OrchardCore.Admin/AdminMenu.cs:26`, and the breadcrumb `OrchardCore.Settings/Views/Admin/Index.cshtml:4` |
| General | `OrchardCore.Settings/AdminMenu.cs:39` |
| Localization, Cultures | `Localization/AdminMenu.cs:35,38` |
| Tools | `OrchardCore.Features/AdminMenu.cs:26` |
| Features | `OrchardCore.Features/AdminMenu.cs:27` |
| Deployments, Recipes | `OrchardCore.Deployment/AdminMenu.cs:19`, `OrchardCore.Recipes/AdminMenu.cs:19` |
| Save | `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` |
| Define which cultures are supported by the site. | `Localization/Views/LocalizationSettings.Edit.cshtml:31` |
| Fall back to parent culture(s) | `Localization/Views/LocalizationSettings.Edit.cshtml:42` |
| Culture, Default culture, Add culture | `Localization/Services/LocalizationJSLocalizer.cs:16,17,20` |
| Site settings updated successfully. | `OrchardCore.Settings/Controllers/AdminController.cs:119` |

Some menu entries listed above may not exist in the site; an unused entry does no harm.

### Snapshots

`02-enable` and `03-cultures` change the site, and `04-pick` needs the result of both. Copy `ORCHARD_APP_DATA` aside
(with the site stopped) at three moments, and restore a copy before recording a clip again:

| Copy | Taken | State |
|---|---|---|
| S1 | after the seed, before `02-enable` | Auto Setup done, `fr.po` in place, the three features disabled |
| S2 | after `02-enable` | the three features enabled, one supported culture |
| S3 | after `03-cultures` | `en-US` (default) and `fr` supported |

To record `02-enable` again, restore S1; `03-cultures`, S2; `04-pick`, S3. Start the site again after a restore. To
record everything again, restore S1 and record the three clips in order.

## Order of the clips

1. `02-enable`: enables Localization (as the dependency), Admin Culture Picker and Content Language Header. Changes the
   site. Needs S1.
2. `03-cultures`: saves the localization settings (`en-US`, `fr`); the tenant restarts. Changes the site. Needs S2.
3. `04-pick`: changes only the cookie of the recorder's browser. Needs S3. Every take starts without the culture cookie.

The slide chapters (01 and 05) and the cards (00 and 99) are made by `assemble.py`.

## Chapter 01: slides

Drawn by `slides.py`; nothing to record.

- `why-1`: heading "What is a culture?", two cards (a language and its region; listed in the settings).
- `why-2`: heading "The Admin Culture Picker", three cards; the first two are bright (a drop-down in the top bar that
  lists the supported cultures; the admin follows the choice: translated labels and the text direction).
- `why-3`: heading "The Content Language Header", two cards (no screen and no settings; a Content-Language header that
  names the language of the responses of the site).
- `why-4`: heading "What the picker needs", three cards (the Localization feature; two supported cultures; translations
  in PO files).

## Chapter 02: clip `02-enable`

**Starts on**: `/Admin` (the admin home), signed in with `state.json`. The left menu is collapsed: if a group is open,
close it before the capture. Open the page with `OpenAsync`, then `StartCaptureAsync`.

Sources for the whole clip: the page title, search box and category heading `OrchardCore.Features/Views/Admin/Features.cshtml:15,27,88`;
the menu `OrchardCore.Features/AdminMenu.cs:26-28`; the buttons and badges
`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`, `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`,
`FeatureEntry.DefaultTags.SummaryAdmin.cshtml`; the search matches the name, the id and the description
(`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`); the feature names and descriptions `Localization/Manifest.cs:12-29`.

### `enable-1`

- Click the group **Tools** in the left menu (a `button.item-label` of `#left-nav` with the text "Tools").
- Click the link **Features** under it (`#left-nav a.item-label` with the text "Features", `href` ending in
  `/Admin/Features`), with `ClickAndWaitForNavigationAsync`.
- Type `Admin Culture Picker` into the search box (`#search-box`, placeholder "Search") with `TypeAsync`.
- **Visible when the narration ends**: the Features page with "Admin Culture Picker" in the search box; one row under the
  heading **Internationalization**: the name "Admin Culture Picker", the id `OrchardCore.Localization.AdminCulturePicker`,
  a dependency badge "Localization", the description "Provides a culture picker shape for the admin area.", the status
  "Disabled" and the button **Enable**. No other row matches the search.

### `enable-2`

- Highlight the badge **Localization** of the row (`HighlightAsync` on the `.direct-dependencies .badge` of the visible
  row), hold about two seconds, then `ClearHighlightsAsync()`.
- Click **Enable** in the row (`a.btn.btn-primary` with the text "Enable"; its id is `btn-enable-` followed by the
  feature id with dots replaced by underscores, `btn-enable-OrchardCore_Localization_AdminCulturePicker`) with
  `ClickAndWaitForNavigationAsync`. Enabling restarts the tenant: wait until `#search-box` is visible again and the
  notification below is on screen.
- **Timing**: the narration ends with "A message confirms that the feature was enabled." and the notification lasts only
  5 seconds (see "Notifications"). Click while the narrator says "click Enable" (about 7 seconds into the step), so the
  page has loaded when the last sentence starts. Measure the restart on a trial take and shorten the `PauseAsync` before
  the click by the restart time if the notification appears after the last sentence has started.
- **Visible when the narration ends**: the Features page again, search box empty, and the green notification "The feature
  Admin Culture Picker was enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`). The message names only the
  requested feature: Localization is enabled with it, silently
  (`OrchardCore.Features/Controllers/AdminController.cs:247`, `src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:79`).

### `enable-3`

- Type `Content Language Header` into `#search-box` (the notification of the previous step may still be on screen).
- Highlight the badge **Localization** of the single visible row for about 1.5 seconds while the narrator says "the same
  dependency", then `ClearHighlightsAsync()`.
- Click **Enable** in the single visible row (`a.btn.btn-primary`, text "Enable", id
  `btn-enable-OrchardCore_Localization_ContentLanguageHeader`) with `ClickAndWaitForNavigationAsync`; wait for
  `#search-box` and the notification. The restart may outlast the narration of this step: that only adds a pause.
- **Visible when the narration ends**: the Features page and, if the page has loaded less than 5 seconds before, the
  notification "The feature Content Language Header was enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`;
  the narration does not refer to it). Before the click the row showed the name
  "Content Language Header", the description "Adds the Content-Language HTTP header, which describes the language(s)
  intended for the audience." (`Localization/Manifest.cs:21`), the badge "Localization" and the button "Enable".

### `enable-4`

- Click `#search-box`, press Escape (it empties the box), and type `Localization`.
- Highlight the status badge of the three rows "Localization", "Admin Culture Picker" and "Content Language Header"
  (`HighlightAsync`; the badge text is "Enabled", `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`).
- **Visible when the narration ends**: the filtered list with the three rows "Localization", "Admin Culture Picker" and
  "Content Language Header", each with the badge **Enabled** (other features whose name, id or description holds
  "Localization" may appear, disabled). The summary line "Showing N of M features" is above the list. Clear the
  highlights at the end.

After this clip, copy the site's data aside as **S2**.

## Chapter 03: clip `03-cultures`

**Starts on**: `/Admin/Features`, signed in. The left menu is collapsed (close any open group). `OpenAsync`, then
`StartCaptureAsync`. Restore S2 before a new take.

Sources: the menu `Localization/AdminMenu.cs:34-38` (route `Settings/{groupId}` with the group `localization`:
`OrchardCore.Settings/Controllers/AdminController.cs:50`, `Localization/Drivers/LocalizationSettingsDisplayDriver.cs:23`); the
editor `Localization/Views/LocalizationSettings.Edit.cshtml:31-42` and the table, list and button drawn by
`Localization/wwwroot/Scripts/localization-settings/localization-settings.js` (built from
`Localization/Assets/ts/localization-settings.ts`); the warning
`OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`; the messages
`Localization/Drivers/LocalizationSettingsDisplayDriver.cs:132` and `OrchardCore.Settings/Controllers/AdminController.cs:119`.

### `cultures-1`

- Click the group **Settings** in the left menu (`button.item-label`, text "Settings"), then the group **Localization**
  under it, then the link **Cultures** (`a.item-label`, `href` ending in `/Admin/Settings/localization`), the last one
  with `ClickAndWaitForNavigationAsync`.
- Highlight the top bar's right side (`ul.user-top-navbar`) for about two seconds: it holds the quick navigation icon
  (`OrchardCore.Admin/Views/QuickNavigationNavbarItem.cshtml`, shown by default), the Visit Site icon and the user menu,
  and no culture picker. Then `ClearHighlightsAsync()`.
- **Visible when the narration ends**: the Cultures page with the yellow warning "The website might be restarted upon
  saving the settings, potentially leading to temporary unresponsiveness during the process."; the label "Define which
  cultures are supported by the site."; the table with the columns "Culture" and "Default culture" and a single row
  `en-US` whose radio button is selected; the drop-down list with the button **Add culture**; the check box "Fall back
  to parent culture(s)"; the button **Save**. The top bar has no culture picker.

### `cultures-2`

- Choose `fr` in the drop-down list of the editor (`.localization-settings-wrapper select`, `SelectAsync` with the value
  `fr`; the option reads "fr (French)").
- Click **Add culture** (`a.btn.btn-success`, text "Add culture"). It is a link without navigation: use `ClickAsync`. The
  table now lists `en-US` (default) and `fr`; leave `en-US` as the default.
- Click **Save** (`button.save`, text "Save") with `ClickAndWaitForNavigationAsync`. Saving restarts the tenant: wait until
  the page shows the notifications below.
- **Visible when the narration ends**: the Cultures page after the save, with the table of `en-US` (default) and `fr`.
  The save can take a few seconds; if it outlasts the narration, the notification of `cultures-3` is already there.

### `cultures-3`

- Highlight the warning notification (`.toast.message-warning`; it stays until closed) "The site has been restarted for
  the settings to take effect." for about two seconds, clear it, then highlight the culture picker button `#oc-culture-picker` in the top bar (it reads "English
  (United States)"), and hold.
- **Visible when the narration ends**: the warning "The site has been restarted for the settings to take effect." (the
  green "Site settings updated successfully." disappears 5 seconds after the page loads and is not required), and the top bar with the drop-down
  **English (United States)** (`Localization/Views/AdminCulturePicker.cshtml:8-16`: the name of the current culture,
  written by .NET, so check it reads "English (United States)" on the recording machine). The picker appears because the
  site now supports two cultures (`Localization/Drivers/AdminCulturePickerNavbarDisplayDriver.cs:36`).

After this clip, copy the site's data aside as **S3**.

## Chapter 04: clip `04-pick`

**Starts on**: `/Admin/Settings/localization`, signed in, in English, with the picker in the top bar. The recorder's
browser has no cookie named `admin_culture_...` (delete it if it has). `OpenAsync`, then `StartCaptureAsync`. Restore
S3 before a new take (only the site matters: the culture is a cookie of the browser).

Sources: the picker `Localization/Views/AdminCulturePicker.cshtml` (button `#oc-culture-picker` line 8, list items
`.language-menu-item` with `data-language-value` lines 19-25); the click handler
`Localization/Assets/ts/admin-culture-selector.ts:11-12` (it writes the cookie and reloads the page); the cookie
`Localization/AdminCookieCultureProvider.cs:11,23,28,31`; the Visit Site icon
`OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:2`.

### `pick-1`

- Click the picker button `#oc-culture-picker` (it reads "English (United States)").
- **Visible when the narration ends**: the open list under the picker with a single entry, **français** (the current
  culture is left out of the list, `Localization/Views/AdminCulturePicker.cshtml:22`). The names are written by .NET:
  check them on the recording machine.

### `pick-2`

- Click the entry **français** (`.language-menu-item` with `data-language-value="fr"`) with
  `ClickAndWaitForNavigationAsync`: the script writes the cookie and reloads the page. Wait until the picker button reads
  "français" and the left menu shows "Paramètres".
- **Visible when the narration ends**: the Cultures page in French: the picker "français"; the left menu groups in French
  (for example "Contenu", "Conception", "Paramètres", "Outils"); the breadcrumb "Paramètres"; the label "Définissez les
  cultures prises en charge par le site."; the table headers "Culture" and "Culture par défaut"; the buttons "Ajouter une
  culture" and "Enregistrer".

### `pick-3`

- Highlight the yellow warning of the page: the paragraph `p.alert.alert-warning` whose text starts with "The website
  might be restarted" (a filter on that text keeps the selector off the notifications). It is still in English: the
  wrapper `Settings_Wrapper__Reload` that `Localization/Drivers/LocalizationSettingsDisplayDriver.cs:69` adds
  (`src/OrchardCore/OrchardCore.DisplayManagement/Extensions/BuildShapeContextExtensions.cs:6`) renders
  `OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`, a label the seed does not translate. Hold about three
  seconds.
- **Visible when the narration ends**: a green ring around the English warning "The website might be restarted upon
  saving the settings, ..." in the middle of the French labels. Clear the highlights with `ClearHighlightsAsync()`
  before the next step.

### `pick-4`

- Move the cursor to the Visit Site icon, the external-link icon of the top bar (`ul.user-top-navbar a.nav-link` that
  holds `i.fa-external-link`) with `MoveToAsync`, hold one second so the tooltip "Visit Site" shows, then click it with
  `ClickAndWaitForNavigationAsync`. The link has no `target`: it opens the front end in the same tab.
- **Visible when the narration ends**: the home page of the Blog front end, in English, with no admin bar of the French
  admin. Wait for the page to load completely.

### `pick-5`

- Go back to the admin with `NavigateAsync("/Admin/Settings/localization")`.
- **Visible when the narration ends**: the Cultures page in French again (the same labels as in `pick-2`), with the picker
  reading "français": the cookie keeps the choice from page to page of the admin.

### `pick-6`

- Click the picker button `#oc-culture-picker` ("français"); the list shows a single entry, **English (United States)**
  (`.language-menu-item` with `data-language-value="en-US"`). Click it with `ClickAndWaitForNavigationAsync`. Wait until
  the picker reads "English (United States)".
- **Visible when the narration ends**: the Cultures page in English again, the picker reading "English (United States)".

## Chapter 05: slides

Drawn by `slides.py`; nothing to record. The Content-Language header cannot be shown in a recording: the recorder captures
the page, not the network.

- `header-1`: heading "The Content-Language header", three cards; the last two are bright (it describes the language
  intended for the audience; it carries the culture the site uses to render the response).
- `header-2`: heading "What the header carries", two cards (a page in English; an admin page in français).

## Checks for private data

- The user shown in the user menu and anywhere else is `admin` with `admin@example.com`; no other name, email, phone number
  or token appears. The site name is "Orchard Core". The password is never typed on camera (the session comes from
  `state.json`).
- No machine name, Windows user name or folder path appears: the recording shows the browser page only.
- The French file holds labels only. `ORCHARD_APP_DATA` copies stay on the recording machine, outside git.
- Look at every contact sheet (`review.py`), then OCR the video at one frame per second and at every scene change, as
  `SKILL.md` asks, and search for names, emails and tokens other than the ones above.
- Commit no MP4, audio, `state.json`, mask script or copy of `ORCHARD_APP_DATA`.
