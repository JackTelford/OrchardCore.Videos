# Recording plan: Geo Point Field: Add Locations to Content Items

A person who reads only this file can record the video. The narration is in `storyboard.json`; run `narrate.py` before
recording, since the recorder holds each step for the length of its narration. In this file a selector such as
`#search-box` stands for `scene.Page.Locator("#search-box")`, and source references (`file:line`) are paths under
`src/OrchardCore.Modules` of `OrchardCMS/OrchardCore`, unless they start with `src/`. The lines are those of the commit
the catalog was read from (`92db4a83`).

Four chapters are recordings (`02-enable`, `03-add`, `04-enter`, `05-check`). Two are slides (`01-idea`, `06-after`),
and the two cards (`00-intro`, `99-outro`) need nothing from the recorder.

## The site

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories. |
| Provisioning | Auto Setup, so no setup or login form is recorded. Recipe **Blog** (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`), site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`; never written in a file of the repository). |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, once; it writes `state.json` (git-ignored). |
| Tenant | The Default tenant only. |
| Outbound calls of the site | Start the site from a shell where `HTTP_PROXY` and `HTTPS_PROXY` point at a dead address (for example `http://127.0.0.1:9`), so the site never calls a mail, SMS or payment provider. Run the recorder from another shell **without** these variables: the browser must reach OpenStreetMap (below). |
| External service | The Leaflet editor of the Geo Point field loads its map background from `https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png` in the browser (`OrchardCore.Spatial/Assets/ts/coordinate-leaflet-field.ts:49`). There is no local stand-in: record with internet access, and wait for the tiles as described in the clips. The Leaflet script and style files are served by the site itself (`OrchardCore.Spatial/ResourceManagementOptionsConfiguration.cs:15-23`). |
| Admin list layout | The default, **List** (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`). Do not change the admin settings. |
| Features before recording | Only those the Blog recipe enables. **Spatial and Lucene must be disabled** at the start: clip `02-enable` enables them on camera. Check it on Tools, Features. |
| Settings | None. |
| Seed data | None. The site has no Page content item at the start (the Blog recipe creates a Menu, two taxonomies, a Blog, a Blog Post, an Article and a Raw Html widget, and no Page). Everything below is typed during the clips. |

### The fake values typed during the clips

| Where | Value |
|---|---|
| Field Display Name | `Location` (the Technical Name fills itself in with the same word) |
| Field Hint | `Where visitors can find our shop.` |
| Page Title | `Find Us` |
| Latitude, after the click on the map | whatever the click gives (replaced in the next step) |
| Latitude | `47.6062` |
| Longitude | `-122.3321` |

The two coordinates are those of a city centre. The shop is invented: the site and the video name no real business,
person, address or email.

## Order, dependencies and snapshots

Record the clips in this order. Each one starts from the state the previous one left.

| Clip | Needs | Changes the site |
|---|---|---|
| `02-enable` | A fresh site (snapshot **S0**) | Yes: enables Spatial and the features it depends on (Lucene and what Lucene needs). |
| `03-add` | `02-enable` (snapshot **S1**) | Yes: adds the Location field to the Page content type, with Required, a Hint and the Leaflet editor. |
| `04-enter` | `03-add` (snapshot **S2**) | Yes: creates and publishes the page Find Us. |
| `05-check` | `04-enter` (snapshot **S3**) | No: it opens the editor and leaves it without saving, then opens the page on the site. |

Stop the site and copy its `App_Data` folder and `state.json` aside at S0, S1, S2 and S3. To record one clip again,
stop the site, restore the snapshot it needs (the one before it in the table), and start the site again. To record
everything again, restore S0.

The slides `01-idea` and `06-after` are drawn by `slides.py` and need no recording.

## Clips

Every clip uses a new scene that starts on the page named below, with the admin menu **groups** expanded before
`StartCaptureAsync` as described (the admin remembers which groups were left open, so the menu keeps its state on the
next page: `src/OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml:16-18`). A group of the admin menu is a
button (`GetByRole(AriaRole.Button, new() { Name = "Tools", Exact = true })`) and a menu entry is a link
(`GetByRole(AriaRole.Link, new() { Name = "Features", Exact = true })`).

A message after a save is a toast (`src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml`). Click early in a step whose
narration mentions the message, so that the toast is on screen during that sentence.

### Chapter 01, `01-idea` (slides)

| Step | The slide shows |
|---|---|
| `idea-1` | The heading "Content that has a place" and three cards: shops, offices and venues; a Geo Point field; two ways to set it. |
| `idea-2` | The heading "Two numbers" and three cards: Latitude, Longitude, and "Empty by default". |
| `idea-3` | The heading "Two editors" and three cards: Standard, Leaflet editor, OpenStreetMap. |

### Chapter 02, clip `02-enable`

Start on `/Admin` (the dashboard). Before `StartCaptureAsync`, click the button "Tools" so its group is open and the link
"Features" is visible. Source of the labels: Tools and Features, `OrchardCore.Features/AdminMenu.cs:26-27`.

- `enable-1`: click the link "Features" with `ClickAndWaitForNavigationAsync`. On the Features page (title "Features",
  `OrchardCore.Features/Views/Admin/Features.cshtml:15`), type `Spatial` in `#search-box` (placeholder "Search",
  `Features.cshtml:27`) with `TypeAsync` and a delay of 90. Wait until the list summary reads "Showing 1 of" followed by
  the total (`Features.cshtml:57-59`), then move the cursor to the dependency badges of the row and highlight them
  (`.direct-dependencies` inside the row, no caption). The row is
  `scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-enable-OrchardCore_Spatial") }).First`.
  **On screen when the narration ends:** under the heading "Content Management", one row with the name "Spatial" and
  the id `OrchardCore.Spatial`, the description "This feature provides the ability to provide spatial locations to content items.",
  the badges "Content Types" and "Lucene" (highlighted: they are the direct dependencies, listed in this order because the
  badges are sorted by name; the indirect ones stay hidden behind the "..." button, `FeatureEntry.DefaultTags.SummaryAdmin.cshtml:20-40`,
  `OrchardCore.Features/Controllers/AdminController.cs:134-163`), and an "Enable" button. Sources: the row,
  `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`, `FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13-18`,
  `FeatureEntry.Description.SummaryAdmin.cshtml:8`, `OrchardCore.Spatial/Manifest.cs:8-9`.
- `enable-2`: clear the highlight, then within the first second click `#btn-enable-OrchardCore_Spatial` with
  `ClickAndWaitForNavigationAsync` (label "Enable", `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`). The page reloads on
  the Features page. The reload empties the search box, so the whole list is shown: as soon as the toast is visible, type
  `Spatial` in `#search-box` again (delay 90) and wait for "Showing 1 of". The site restarts its shell after the click, so the
  Features page can take several seconds to load: if the actions run longer than the narration of the step, record the clip
  again. The click also enables Lucene and what Lucene
  needs, but the message names only the feature that was clicked (`OrchardCore.Features.Core/Services/FeatureService.cs:78-81`
  passes the one feature to the message), so it is the singular text, not the plural "The following features were
  enabled: ...".
  **On screen when the narration ends:** the toast "The feature Spatial was enabled."
  (`OrchardCore.Features/Controllers/AdminController.cs:317`; a toast stays until it is closed, `TheAdmin/Views/Message.cshtml:12`)
  while the second sentence plays, and at the end the row
  of Spatial with an "Enabled" badge and a "Disable" button (`FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`,
  `FeatureEntry.Buttons.SummaryAdmin.cshtml:53`). Clicking Enable also enables the features Spatial depends on, because
  the controller passes `force: true` (`AdminController.cs:247`).

### Chapter 03, clip `03-add`

Start on `/Admin`. Before `StartCaptureAsync`, click the button "Design", then the button "Content Definition" so the
link "Content Types" is visible. Source of the labels: `OrchardCore.ContentTypes/AdminMenu.cs:23-25`.

- `add-1`: click the link "Content Types" with `ClickAndWaitForNavigationAsync` (page "Content Types",
  `OrchardCore.ContentTypes/Views/Admin/List.cshtml:6`). If the Page row is below the fold, call `ScrollIntoViewIfNeededAsync` on it first. Click the "Edit" button of the Page row with
  `ClickAndWaitForNavigationAsync` on `[role="btn-edit-Page"]` (label "Edit",
  `OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; the row's buttons are in the
  List layout's row actions: if the site shows them in a menu, open the menu of the Page row first and note it in
  `OPEN-QUESTIONS.md`). **On screen when the narration ends:** the page "Edit Content Type - Page"
  (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`) with the heading "Fields" and, under it, the button "Add Field"
  (`Edit.cshtml:88`, `:137`; it is a link with `role="button"`, so use
  `GetByRole(AriaRole.Button, new() { Name = "Add Field", Exact = true })`).
- `add-2`: click that "Add Field" button with `ClickAndWaitForNavigationAsync`. On the page "Add New Field To "Page""
  (`OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml:20`), type `Location` in the input of the label "Display Name"
  (`GetByLabel("Display Name", new() { Exact = true })`, `AddFieldTo.cshtml:38`) with `TypeAsync`, then press Tab so the
  Technical Name is computed (`OrchardCore.ContentTypes/Assets/js/autocomplete.ts:6-21`). Then click the radio button
  with the label "Geo Point Field" (`GetByLabel("Geo Point Field", new() { Exact = true })`; the label is the field's type name written with
  spaces, `AddFieldTo.cshtml:62`; it is in the list only when Spatial is enabled). **On screen when the narration
  ends:** Display Name "Location", Technical Name "Location" (the helper keeps the letters of the Display Name and drops spaces and symbols, so `Location` gives
  `Location`; it runs on every key up and when the box loses focus), the radio button "Geo Point Field" selected and in view. Scroll it into view with
  `ScrollAsync` before clicking if the list is below the fold.
- `add-3`: click the button "Save & Configure" (`button[value="SaveAndConfigure"]`, `AddFieldTo.cshtml:72`) with
  `ClickAndWaitForNavigationAsync`. The page `"Location" settings` opens
  (`OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:30`) with the toast "The field "Location" has been added.".
  Move the cursor to the checkbox "Required" and highlight it, then highlight the textarea "Hint"
  (`GetByLabel("Required", new() { Exact = true })`, `GetByLabel("Hint", new() { Exact = true })`). **On screen when the
  narration ends:** both settings in view, with their hints "Whether a value is required." and "The hint text to
  display for this field on the editor." (`OrchardCore.Spatial/Views/GeoPointFieldSettings.Edit.cshtml:7-8`, `:14`, `:17`).
  Below the Hint, because Lucene is enabled and the admin may manage Lucene indexes, the page also shows a block with the
  heading "Lucene Index settings" and the unchecked box "Include this element in the index"
  (`OrchardCore.Lucene/Settings/ContentPartFieldIndexSettingsDisplayDriver.cs:27-35`,
  `OrchardCore.Lucene/Views/LuceneContentIndexSettings.Edit.cshtml:5`, `:13`). Leave it unchecked and do not highlight it:
  the slide of `after-1` names it.
- `add-4`: clear the highlights. Click the checkbox "Required" (`ClickAsync`). Type
  `Where visitors can find our shop.` in the textarea "Hint" with `TypeAsync` (delay 40). **On screen when the
  narration ends:** "Required" ticked and the hint text in the textarea.
- `add-5`: scroll down, past the "Lucene Index settings" block, to the select under the question "What type of editor should be used?"
  (`EditField.cshtml:79`, `select[data-field-editor-select]`). Choose the option with the value `Leaflet` (text "Leaflet editor",
  `OrchardCore.Spatial/Views/GeoPointField-Leaflet.Option.cshtml:4`; the other option is "Standard",
  `GeoPointField.Option.cshtml:4`) with `SelectAsync(select, "Leaflet")`. This page has no drop-down list for a display
  mode. Then click the button "Save" (`button.save`, `EditField.cshtml:120`) with `ClickAndWaitForNavigationAsync`.
  **On screen when the narration ends:** the page "Edit Content Type - Page" (the Save returns to it) with the toast
  `The "Location" field settings have been saved.` (`OrchardCore.ContentTypes/Controllers/AdminController.cs:908`).
- `add-6`: move the cursor to the Location row of the list `#fields` and highlight it (no caption). **On screen when the
  narration ends:** the row with the name "Location", the type "Geo Point Field" beside it and a badge "Leaflet", ringed
  (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:115`, `:121-124`). Clear the highlights.

### Chapter 04, clip `04-enter`

Start on `/Admin`. Before `StartCaptureAsync`, click the button "Content" so the link "Content Items" is visible
(`OrchardCore.Contents/AdminMenu.cs:65-68`).

- `enter-1`: click the link "Content Items" with `ClickAndWaitForNavigationAsync` (page "Manage Content",
  `OrchardCore.Contents/Views/Admin/List.cshtml:17`). Click the button "New" (`#new-dropdown`,
  `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`; it is a drop-down list because several types can be
  created), then the item "Page" of the list that follows the button
  (`scene.Page.Locator("#new-dropdown + .dropdown-menu").GetByRole(AriaRole.Link, new() { Name = "Page", Exact = true })`)
  with `ClickAndWaitForNavigationAsync`. Scope it: the Blog recipe adds a custom admin menu whose group "Content" also holds
  a link "Page" (`TheBlogTheme/Recipes/blog.recipe.json:1279-1280`), so an unscoped locator finds two links. On the page "New Page", type `Find Us` in the input of the label "Title" with
  `TypeAsync` (`GetByLabel("Title", new() { Exact = true })`, `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`).
  Then wait until `.leaflet-tile-loaded` has at least one match (the map is below the fold but starts loading at once).
  **On screen when the narration ends:** the form "New Page" with the title "Find Us" in the first box.
- `enter-2`: scroll down with `ScrollAsync` until the Leaflet editor (`[data-leaflet-editor]`) is fully in view. **On
  screen when the narration ends:** the label "Latitude" and its box (placeholder `00.000000`), the label "Longitude"
  and its box, a red asterisk after both labels, the hint "Where visitors can find our shop." under each box,
  and, under them, a map showing the world with one marker and the "+" and "-" zoom buttons, and tiles drawn
  (`OrchardCore.Spatial/Views/GeoPointField-Leaflet.Edit.cshtml:13-28`, `:35`; the map is centred on its default
  point at the lowest zoom when the boxes are empty, `Assets/ts/coordinate-leaflet-field.ts:40-53`).
- `enter-3`: click the map element `[data-leaflet-map]` with `ClickAsync(map, offsetX: -95, offsetY: -10)`: left of the
  centre and away from the marker. **On screen when the narration ends:** both boxes hold numbers with six decimals,
  the marker has moved to the clicked point, and its popup is open with a text that starts with "Location :"
  (`coordinate-leaflet-field.ts:55-66`). The numbers do not matter: the next step replaces them.
- `enter-4`: click `[data-latitude]` with `ClickAsync`, press `Control+A` with `scene.Page.Keyboard.PressAsync`, then type
  `47.6062` with `PressSequentiallyAsync` (delay 55). Do the same for `[data-longitude]` with `-122.3321`. **On screen
  when the narration ends:** the boxes read `47.6062` and `-122.3321`, and the marker and popup show those values
  (`coordinate-leaflet-field.ts:68-70`). The map does not pan to the marker, and the marker is not dragged: do not
  drag it.
- `enter-5`: scroll to the bottom action bar, move the cursor to the button "Publish"
  (`button.publish`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`), wait one second, then click it with
  `ClickAndWaitForNavigationAsync`. The editor returns to the list of content items
  (`OrchardCore.Contents/Controllers/AdminController.cs:795-798`). **On screen when the narration ends:** the page
  "Manage Content" with the toast "Your Page has been published." (`AdminController.cs:473`) during the first sentence,
  and at the end a row "Find Us" of type Page with the buttons "Edit" and "View"
  (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`, `:18`).

### Chapter 05, clip `05-check`

Start on `/Admin/Contents/ContentItems` (page "Manage Content"), with the row "Find Us" in view. Click nothing before `StartCaptureAsync`.

- `check-1`: click the button "Edit" of the Find Us row (`.list-group-item` that contains the text "Find Us", then its
  `a.btn.edit`) with `ClickAndWaitForNavigationAsync`. Wait until `.leaflet-tile-loaded` has at least one match, then
  scroll to `[data-leaflet-editor]` with `ScrollAsync`. **On screen when the narration ends:** the page "Edit Page" with
  the boxes `47.6062` and `-122.3321` and a map that shows streets and a marker at the centre, at a street-level zoom
  (`coordinate-leaflet-field.ts:43-46`, `:53`). Change nothing and do not save.
- `check-2`: click the link "Manage Content" in the breadcrumb (`OrchardCore.Contents/Views/Admin/Edit.cshtml:40`) with
  `ClickAndWaitForNavigationAsync`. Click the button "View" of the Find Us row (`a.btn.view`, `target="_blank"`) inside
  `scene.Page.Context.RunAndWaitForPageAsync(...)`, and give the new page to `scene.SwitchToAsync(page)`. On the page
  of the site, bring `.field-type-geopointfield` into view with `ScrollIntoViewIfNeededAsync` and highlight it (no
  caption). **On screen when the narration ends:** the front end of the site (the Blog theme) with the heading "Find Us"
  or the page content, and the text `47.606200, -122.332100` ringed
  (`OrchardCore.Spatial/Views/GeoPointField.cshtml:10`: the number format is `N6`). The page address is the one the
  Permalink of the page holds.

### Chapter 06, `06-after` (slide)

| Step | The slide shows |
|---|---|
| `after-1` | The heading "What happens to the location" and three cards: stored with the content item, shown on the site (as text, by the field's template), and a Lucene index (the box "Include this element in the index", then a search by area or by distance). |

## Checks for private data

- Run OCR on the video at one frame per second and at every scene change (ffmpeg `select='gt(scene,0.02)'`), then at
  five frames per second, as `SKILL.md` asks.
- Search the text for: any name other than `admin` and `Find Us`, any email address other than `admin@example.com`, any
  domain other than `localhost`, any token or password.
- The password `<password>` is typed outside the capture (`Recorder setup`) and never appears on screen. The user
  menu of the admin shows the user name `admin` only.
- The map tiles show the streets of a city. They carry the line "OpenStreetMap contributors" and no private data.
- The coordinates are those of a city centre, not of a person's address. The page "Find Us" and the hint are invented.
- Do not commit `state.json`, the clips, the audio, the MP4 or a copy of `App_Data`.
