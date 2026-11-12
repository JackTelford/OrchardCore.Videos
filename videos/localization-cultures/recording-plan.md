# Recording plan: localization-cultures

Everything a person needs to record this video without deciding anything. The narration is in `storyboard.json`; narrate
it first (`narrate.py`), because the recorder holds each step for the length of its narration. In this plan a selector
stands for `scene.Page.Locator(...)`, and a "label source" is a file of the Orchard Core source tree at the commit the
catalog was read from. Anything marked "confirm" is also listed in `OPEN-QUESTIONS.md`.

## The site it needs

- The base site of `references/recording.md`: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout on
  `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`), provisioned by
  Auto Setup with the **Blog** recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user name `admin`,
  email `admin@example.com`, and the password chosen outside git (`VIDEO_PASSWORD`; write `<password>` anywhere it is
  mentioned). The tenant is the default one, named `Default`. Outbound calls are blocked with `HTTP_PROXY` and
  `HTTPS_PROXY` set to a dead address. This is a fresh site, so it needs no mask script.
- **The language of the recording machine.** A site starts with `CultureInfo.InstalledUICulture` as its only supported
  and default culture (`OrchardCore.Localization/Models/LocalizationSettings.cs`). It is the language the operating
  system was installed with, which is not always the display language a user chose. Record on a machine whose operating
  system was installed in English (United States), so that the first row of the Cultures table reads `en-US`: the narration of
  `cultures-2` says "English as used in the United States", and `french-3` relies on English being a supported culture. If the first row reads something else, make
  the starting state by hand after clip `02-enable` (see "Order" below).
- **Features to enable before recording:** none. The Localization feature is enabled on camera in `02-enable`. Leave
  every other localization feature disabled (Content Localization, Content Culture Picker, Data Localization, User
  Localization, Admin Culture Picker, Content Language Header): the admin menu and the navbar must show nothing else.
- **Settings before recording:** none. The Cultures page is first opened in `02-enable`.
- **External services:** none.
- **Fake seed data, created outside the capture, before the site is started:** one small sample French translation
  file for the labels of the Cultures page. Save it as UTF-8 at
  `<ORCHARD_APP_DATA>/Sites/Default/Localization/fr.po` (the tenant location of
  `OrchardCore.Localization/ModularPoFileLocationProvider.cs`, and of `src/docs/reference/modules/Localize/README.md`).
  Create the `Localization` folder if it is missing. The contexts (`msgctxt`) are those of the module's own translation
  templates (`OrchardCore.Localization.pot`, `OrchardCore.Settings.pot`). A PO file is read once per culture: restart
  the site after changing it. The text is invented for the recording and does not come from any real translation.

  ```po
  msgid ""
  msgstr ""
  "Language: fr\n"
  "MIME-Version: 1.0\n"
  "Content-Type: text/plain; charset=UTF-8\n"
  "Content-Transfer-Encoding: 8bit\n"

  msgctxt "OrchardCore.Settings.Views.Admin.Index"
  msgid "Settings"
  msgstr "Paramètres"

  msgctxt "OrchardCore.Settings.Views.Settings-Reload.Wrapper"
  msgid "The website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the process."
  msgstr "Le site web peut redémarrer lors de l'enregistrement des paramètres, ce qui peut le rendre momentanément indisponible."

  msgctxt "OrchardCore.Settings.Views.SiteSettings.SaveButton"
  msgid "Save"
  msgstr "Enregistrer"

  msgctxt "OrchardCore.Localization.Views.LocalizationSettings.Edit"
  msgid "Define which cultures are supported by the site."
  msgstr "Définissez les cultures prises en charge par le site."

  msgctxt "OrchardCore.Localization.Views.LocalizationSettings.Edit"
  msgid "Fall back to parent culture(s)"
  msgstr "Utiliser la culture parente en repli"

  msgctxt "OrchardCore.Localization.Services.LocalizationJSLocalizer"
  msgid "Culture"
  msgstr "Langue"

  msgctxt "OrchardCore.Localization.Services.LocalizationJSLocalizer"
  msgid "Default culture"
  msgstr "Culture par défaut"

  msgctxt "OrchardCore.Localization.Services.LocalizationJSLocalizer"
  msgid "Set as default"
  msgstr "Définir par défaut"

  msgctxt "OrchardCore.Localization.Services.LocalizationJSLocalizer"
  msgid "Remove culture"
  msgstr "Retirer la culture"

  msgctxt "OrchardCore.Localization.Services.LocalizationJSLocalizer"
  msgid "Add culture"
  msgstr "Ajouter la culture"
  ```

- Sign the recorder in once (`Recorder setup`), with the Settings and Tools groups of the admin menu **collapsed** when
  `state.json` is saved: the menu script restores the groups the user last left open
  (`TheAdmin/Views/NavigationItem-admin.cshtml`), and `02-enable` opens them on camera.

## Order and snapshots

Record the clips in this order. The slide clips (`01-why`, `04-ask`, `05-next`) and the cards need no site.

1. **Snapshot A**: the fresh site, right after Auto Setup, with `fr.po` in place and the recorder signed in. Copy the
   `App_Data` folder and `state.json` aside.
2. `02-enable` changes the site: it enables the Localization feature (the tenant restarts). Restore snapshot A before
   every take.
3. **Snapshot B**: the site after a take of `02-enable`, with Localization enabled and the default settings. If the
   first row of the Cultures table is not `en-US`, first open Settings, Localization, Cultures, choose `en-US` in the
   list under the table, click Add culture, select the `en-US` radio button, click the trash button of the other row,
   check that the Fall back to parent culture(s) box is empty, click Save, and wait for the page to return.
   Copy the `App_Data` folder aside after that. Both `03-cultures` and `04-french` need this state or a later one.
4. `03-cultures` changes the site: it saves three supported cultures and checks Fall back to parent culture(s).
   Restore snapshot B before every take.
5. **Snapshot C**: the site after a take of `03-cultures`. `04-french` needs it: the supported cultures must be
   `en-US`, `fr` and `de`, with `en-US` as the default.
6. `04-french` does not change the site. Take it as often as needed from snapshot C.

After a restore, start the site and wait until `/Admin` answers before the next take. Every save of settings and every
feature change restarts the tenant, so the first request after it is slow.

## Chapter 00, the opening card

- `intro`: a card. The title is "Cultures: Make Your Site Multilingual" and the subtitle is "Choose the languages your
  site supports". Nothing to record.

## Chapter 01, `01-why` (slides)

- `why-1`: the slide "What a culture is", four cards: A language; A language with a region; A parent culture; One culture
  per request. All four are bright.
- `why-2`: the slide "Supported and default", three cards: Supported cultures; Default culture; Starting point. All
  three are bright.

## Chapter 02, `02-enable` (clip)

Starts on `/Admin`, the dashboard, opened before the capture with the admin menu collapsed (the menu opens in this
clip). Changes the site: it enables Localization.

- `enable-1`: click the Tools group of the admin menu (`#tools .item-label`; or the button named "Tools" inside
  `#left-nav`), then click the **Features** link of the group and wait for the page
  (`ClickAndWaitForNavigationAsync` on `#left-nav` + link named "Features", exact). Keep the pauses short: the
  actions must end before the narration. On screen at the end of the step: the Features page, with the heading
  "Features", the search box and the features grouped under category headings. Label sources:
  `OrchardCore.Settings/AdminMenu.cs:32-34` (Tools), `OrchardCore.Features/AdminMenu.cs:26-27` (Features),
  `OrchardCore.Features/Views/Admin/Features.cshtml:15` (the heading).
- `enable-2`: type `Localization` in the search box (`TypeAsync` on `#search-box`, placeholder "Search"). The list is
  filtered as you type (the search matches the name, the id and the description of a feature, so several rows stay:
  Localization, Content Localization, Data Localization, and others). Wait until the Enable button of the Localization
  feature is visible (`#btn-enable-OrchardCore_Localization`; confirm the id with `Recorder probe`, since the dots of the
  feature id become underscores). Highlight the category heading **Internationalization**
  (`h3` with that text), hold, clear the highlight, then highlight the Enable button of the row named Localization.
  On screen at the end: the filtered list, with the ring around the Enable button of the Localization row. Label
  sources: `OrchardCore.Features/Views/Admin/Features.cshtml:27` (Search), `OrchardCore.Localization/Manifest.cs:11-16`
  (the feature name Localization and its category Internationalization),
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable).
- `enable-3`: clear the highlight, move the cursor onto the Enable button (`MoveToAsync`), then click it and cut the
  reload out of the clip: `OffCameraAsync` around a `WaitForNavigationAsync` whose action clicks the same button, with
  the wait for the success message as the last line of the cut
  (`.toast` containing "The feature Localization was enabled.", timeout 60 seconds: the tenant restarts). On screen at the
  end: the Features page again, unfiltered, with a green message at the top right reading "The feature Localization was
  enabled." The message hides itself after five seconds, so the narration of this step (13 words) must start right after
  the cut. Label source: `OrchardCore.Features/Controllers/AdminController.cs:317`; the five seconds:
  `OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs` (`SuccessAsync`).
- `enable-4`: click **Settings** (`#settings .item-label`), then **Localization** (`#localization .item-label`), then
  the **Cultures** link (`#cultures a`; or the link named "Cultures" inside `#left-nav`) with
  `ClickAndWaitForNavigationAsync`. Use a short `pauseAfter` (300) for the first two clicks so the actions end before the
  narration. On screen at the end: the Cultures page, address `/Admin/Settings/localization`, with the Settings group
  and its Localization entry open in the admin menu. Label sources:
  `OrchardCore.Settings/AdminMenu.cs:36-37` (Settings), `OrchardCore.Localization/AdminMenu.cs:34-40`
  (Settings, Localization, Cultures and their ids).

## Chapter 03, `03-cultures` (clip)

Starts on `/Admin/Settings/localization`, opened before the capture. Wait until the table of cultures is drawn
(`.localization-settings-wrapper table`): it is built by a script after the page loads. Restore snapshot B before every
take. Changes the site: it saves the settings.

The editor's controls, all inside `.localization-settings-wrapper`: the table (`table`), a row per supported culture
(`tbody tr`), its radio button (`input[type=radio]`), its trash button (`a[title="Remove culture"]`), the list under the
table (`select`) and the Add culture button (`a.btn-success`).

- `cultures-1`: highlight, one after the other and clearing each, the heading **Settings** (`h1`), the warning above the
  form (`.alert-warning`), and the label "Define which cultures are supported by the site." (`label.ocat-label`). On screen
  at the end: the top of the page with the three elements seen. Label sources:
  `OrchardCore.Settings/Views/Admin/Index.cshtml:4` (the heading), `OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:4`
  (the warning, "The website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness
  during the process."), `OrchardCore.Localization/Views/LocalizationSettings.Edit.cshtml:31` (the label).
- `cultures-2`: highlight the table header (`thead`), then the one row of the table (`tbody tr`). On screen at the end:
  the table headed **Culture** and **Default culture** (a third column without a heading holds the trash buttons), one
  row reading `en-US`, its radio button selected. Label source: `OrchardCore.Localization/Services/LocalizationJSLocalizer.cs:16-17`, and the table template in
  `OrchardCore.Localization/wwwroot/Scripts/localization-settings/localization-settings.js`.
- `cultures-3`: click the list under the table and choose the value `fr` (`SelectAsync` with the value `fr`, not the
  text: the option text is the culture name followed by its display name in parentheses, drawn from the machine's
  culture data, for example "fr (French)"). On screen at the end: the list shows `fr` and its display name in
  parentheses. Label source: the option template in `localization-settings.js` (`{{ culture.Name }} ({{ culture.DisplayName }})`).
- `cultures-4`: click **Add culture** (`a.btn-success`): a row `fr` joins the table. Then choose the value `de` in the
  list, and click **Add culture** again: a row `de` joins. Leave whatever the list shows after that. On screen at the
  end: a table of three rows, in this order: `en-US`, `fr`, `de`. Label source:
  `OrchardCore.Localization/Services/LocalizationJSLocalizer.cs:20`.
- `cultures-5`: click the radio button of the `fr` row (`tbody tr:nth-child(2) input[type=radio]`), pause 700 ms, then
  click the radio button of the `en-US` row (`tbody tr:nth-child(1) input[type=radio]`). On screen at the end: the radio
  button of the `en-US` row is selected, the others are empty. The radio buttons have no visible text: their label has the
  title "Set as default" (`LocalizationJSLocalizer.cs:18`), which the narration does not read.
- `cultures-6`: highlight the trash button of the `de` row (`tbody tr:nth-child(3) a[title="Remove culture"]`) and clear
  the highlight at the end. Do not click it. On screen at the end: the table of three rows, unchanged. Label source:
  `LocalizationJSLocalizer.cs:19` (the title "Remove culture"); the rule that the first culture becomes the default when
  the default is removed: `OrchardCore.Localization/Drivers/LocalizationSettingsDisplayDriver.cs:121-124`.
- `cultures-7`: highlight the label **Fall back to parent culture(s)** (`GetByLabel`, or `label.form-check-label`),
  clear the highlight, then click the check box. On screen at the end: the box is checked. Label source:
  `OrchardCore.Localization/Views/LocalizationSettings.Edit.cshtml:42`. The behavior the narration states (a text with no
  translation is looked up in the parent culture): `OrchardCore.Localization/Startup.cs:56` copies the setting into
  `FallBackToParentUICultures`, which `OrchardCore.Localization.Core/PortableObject/PortableObjectStringLocalizerFactory.cs:31`
  passes to `PortableObjectStringLocalizer.GetTranslation` (`PortableObjectStringLocalizer.cs:173-183`).
- `cultures-8`: move the cursor onto **Save** (`button.save`), then click it and cut the reload out of the clip
  (`OffCameraAsync` around a `WaitForNavigationAsync` whose action clicks Save, then wait for a `.toast` containing "Site
  settings updated successfully.", timeout 60 seconds). On screen at the end: the Cultures page again, with a green
  message "Site settings updated successfully." at the top right (it hides itself after five seconds, so the narration of
  this step, 12 words, must start right after the cut). The warning of the next step is already on screen too, because
  the display driver adds it (`LocalizationSettingsDisplayDriver.cs:132`) in the same request as the green message
  (`AdminController.cs:119`), before the redirect; it stays after the green message has gone. Label sources:
  `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (Save), `OrchardCore.Settings/Controllers/AdminController.cs:119`
  (the message).
- `cultures-9`: no click. Wait until a `.toast.message-warning` containing "The site has been restarted for the settings
  to take effect." is visible (it stays until it is closed: `WarningAsync` sets no delay in `NotifierExtensions.cs`), and
  highlight the toast for a moment. On screen at the end: the warning at the top right, the table with the rows `en-US`,
  `fr` and `de` with `en-US` selected, and the box Fall back to parent culture(s) checked. The green message is gone,
  because five seconds have passed since the page loaded. Label source: `OrchardCore.Localization/Drivers/LocalizationSettingsDisplayDriver.cs:132`.

## Chapter 04, `04-ask` (slides) and `04-french` (clip)

- `ask-1`: the slide "Asking for a culture": two cards (The language of the browser; The culture parameter) and an address
  bar below them reading `example.com/Admin/Settings/localization?culture=fr`, with `?culture=fr` in green, and the line
  "fr is the name of the French culture."
- `ask-2`: the slide "Seeing it on this site": three cards (One request; A sample French file; Without the parameter).

`04-french` starts on `/Admin/Settings/localization`, opened before the capture, after snapshot C (English). Wait for the
table before starting the capture. The site is not changed.

- `french-1`: `NavigateAsync("/Admin/Settings/localization?culture=fr")`, then wait until the Save button reads
  "Enregistrer" (`button.save`). On screen at the end: the Cultures page in French: the heading "Paramètres", the column
  titles "Langue" and "Culture par défaut", the button "Ajouter la culture", the box "Utiliser la culture parente en
  repli" (checked), the button "Enregistrer". The three rows still read `en-US`, `fr`, `de`. Source of the request
  culture: `src/docs/guides/install-localization-files/README.md` ("Enabling localization and testing the site").
- `french-2`: highlight, one after the other and clearing each, the heading (`h1`), the table header (`thead`), the
  Add culture button (`a.btn-success`), the check box label (`label.form-check-label`) and the Save button (`button.save`).
  On screen at the end: the same French page, no highlight.
- `french-3`: `NavigateAsync("/Admin/Settings/localization")`, then wait until the Save button reads "Save". On screen at
  the end: the Cultures page in English again, with the three rows. The recorder's browser context sets the locale
  `en-US` (`scripts/recorder/Scene.cs:110`), so the browser asks for English, which is a supported culture.

## Chapter 05, `05-next` (slides)

- `next-1`: the slide "What builds on the list", four cards (Content Localization; Content Culture Picker; Data
  Localization; Admin Culture Picker); the first two are bright, the others dimmed.
- `next-2`: the same slide, with the last two bright.
- `next-3`: the slide "Around the Cultures page", three cards (Manage supported culture; Culture settings; Recipes).

## Chapter 99, the closing card

- `outro`: a card. The subtitle is `docs.orchardcore.net`. Nothing to record.

## Checks for private data

- Every row, name and message on these pages comes from a fresh site and from this plan: nothing real is entered. The
  only user is `admin`; the only email, `admin@example.com`, is not shown on these pages.
- OCR the video at one frame per second and at every scene change, as `SKILL.md` asks, and search for the name of the
  recording machine, user folders and paths (the Features page and the notification toasts must not show any), and for any
  email other than `admin@example.com`. The French text of `04-french` is the sample file above.
- The address bar of the browser is never part of the capture. The only address shown in the video is the one drawn on
  the slide `ask-1`.
