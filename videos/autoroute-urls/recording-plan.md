# Recording plan: Autoroute

This plan tells a person how to record every clip of this video without deciding anything. A selector written here
stands for `scene.Page.Locator(...)` (or the `GetBy...` helper named next to it), because the recorder's methods take
locators. Source references name the file and line where the label on screen comes from, in the Orchard Core checkout
read at commit 92db4a83: `Autoroute/Views/...` stands for `src/OrchardCore.Modules/OrchardCore.Autoroute/Views/...` (the
same for every other module), and a reference that starts with `src/` is a full path.

The video has two slide steps (`why-1`, `why-2`), two cards (`intro`, `outro`) and four recorded clips (`02-add`,
`03-settings`, `04-create`, `05-change`), 25 steps in all.

## 1. The site it needs

Record a fresh local site with fake data only. It needs no mask script.

| Item | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, built and run on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| App data | its own empty `ORCHARD_APP_DATA` folder, outside both repositories |
| Setup | Auto Setup through environment variables: no setup form and no login form is recorded. The site is not an uninstalled site, so `state.json` is used |
| Recipe | `Blog` (`OrchardCore__AutoSetup__Tenants__0__RecipeName`). The recipe enables Autoroute (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:50`) |
| Other Auto Setup values | `ShellName` Default, `SiteName` Orchard Core, `SiteTimeZone` America/Los_Angeles, `DatabaseProvider` Sqlite, `AdminUsername` admin, `AdminEmail` admin@example.com, `AdminPassword` the password chosen outside git |
| Credentials | user `admin`, email `admin@example.com`, password `<password>`: the value of `VIDEO_PASSWORD` (and `VIDEO_USER_NAME=admin`), chosen outside git and never written in any file |
| Features to enable before recording | none: Autoroute, Content Types, Contents, Title and Home Route are enabled by the Blog recipe. Do not disable any of them |
| Settings | none. Leave the site settings as the recipe leaves them (the home page stays the blog, and no Base URL is set, so the Permalink field has no address in front of it) |
| External services | none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address so the site never calls anything outside |
| Browser | the recorder's own Chromium (1536x864 CSS pixels, device scale 1.25, light color scheme, en-US). The admin menu stays as the site shows it |

Environment variable names: `OrchardCore__AutoSetup__Tenants__0__<Parameter>` (see
`src/docs/reference/modules/AutoSetup/README.md`).

### The seed data (create it outside the capture, before the first snapshot)

The video needs one content type that has a title and no Autoroute part, and no content item of that type.

1. Run `Recorder setup` once, to sign in as admin and save `state.json` (git-ignored).
2. Open `/Admin/ContentTypes/List` and select **Create new type** (`ContentTypes/Views/Admin/List.cshtml:20`).
3. Enter Display Name **Event**. The Technical Name fills itself in as `Event`. Select **Create**
   (`ContentTypes/Views/Admin/Create.cshtml`, labels "Display Name", "Technical Name", "Create"). The next page is
   **Add Parts**.
4. Tick **Title** (the Title part, `Title/Migrations.cs:31`) and select **Add Selected Parts**
   (`ContentTypes/Views/Admin/AddPartsTo.cshtml:47`). Do not tick Autoroute. The Edit Content Type page now lists the
   Event part and the Title part.
5. A type made on this screen is creatable, listable, draftable and versionable
   (`ContentTypes/Services/ContentDefinitionService.cs:85-89`), so nothing else is needed.
6. Create no Event content item. The Blog recipe's own items (About, Blog, one blog post, the menu and the taxonomies)
   stay as they are; they hold only Lorem ipsum text and the user name admin.

Copy the whole `App_Data` folder and `state.json` aside as **snapshot 1** (stop the site first).

## 2. Order, dependencies and snapshots

The four clips are recorded in the order they play, and each one starts from the state the previous one left:

| Clip | Needs | Changes the site | Snapshot to restore before a new take |
|---|---|---|---|
| `02-add` | snapshot 1 | yes: attaches the Autoroute part to Event | snapshot 1 |
| `03-settings` | `02-add` | yes: saves the Autoroute settings of Event | snapshot 2 (taken after `02-add`) |
| `04-create` | `03-settings` | yes: creates and publishes the event Spring Planting Workshop | snapshot 3 (taken after `03-settings`) |
| `05-change` | `04-create` | yes: changes the title and the permalink of that event | snapshot 4 (taken after `04-create`) |

After each clip, stop the site, copy `App_Data` aside as the next snapshot, and start the site again. To record one
clip again, stop the site, restore the snapshot listed for it (with `state.json`), and start the site.

`03-settings` is a full take of a page that the end of `02-add` already shows: it opens the Edit Part page directly,
so it can be recorded again without recording `02-add` again, as long as snapshot 2 is restored.

The slides (`why-1`, `why-2`) and the cards (`intro`, `outro`) are drawn by `slides.py` and `assemble.py`, and need no
site.

## 3. Clips

### Clip `02-add` (chapter 02, "Add Autoroute to a content type")

Starts on `/Admin/Features` (`scene.OpenAsync("/Admin/Features")`, not recorded), signed in as admin, with snapshot 1
restored. Call `StartCaptureAsync()` once the search box (`#search-box`) is visible. The feature list is the one
rendered by `OrchardCore.Features/Views/Admin/Features.cshtml`.

Define the row of the feature the way the Features example of the recorder does:
`Row = scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-disable-OrchardCore_Autoroute, #btn-enable-OrchardCore_Autoroute") }).First`.

- `add-1`
  - Actions: `TypeAsync(scene.Page.Locator("#search-box"), "autoroute", delay: 70)`. Wait until the list shows the
    row `Row` (visible). Then `MoveToAsync(Row, -200)` and `HighlightAsync(Row, "Enabled")`. Do not select Disable.
  - On screen when the narration ends: the Features page with "autoroute" in the search box and the Autoroute row
    ringed, showing its Enabled badge and its (unused) Disable button.
  - Labels: "Search" `Features/Views/Admin/Features.cshtml:27`; "Enabled" `Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`;
    "Enable"/"Disable" `Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40,53`; the menu path Tools, Features
    `Features/AdminMenu.cs:26-27`; Autoroute enabled by the recipe `blog.recipe.json:50`.
- `add-2`
  - Actions: `ClearHighlightsAsync()`. `NavigateAsync("/Admin/ContentTypes/List")` while the narration reads the menu
    path (the Content Types page opens; its breadcrumb reads "Content Types"). Wait until the row of Event is visible,
    then `ClickAndWaitForNavigationAsync(scene.Page.Locator("a[role='btn-edit-Event']"))`. Wait until the heading
    "Parts" (`h3`) is visible.
  - On screen when the narration ends: the Edit Content Type page of Event (breadcrumb "Edit Content Type - Event"),
    with the Parts list showing the Event part and Title, and the buttons Add Parts and Add Named Part below it.
  - Labels: Design, Content Definition, Content Types `ContentTypes/AdminMenu.cs:23-25`; "Content Types"
    `ContentTypes/Views/Admin/List.cshtml:6`; "Edit" `ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`;
    "Edit Content Type - {0}" `ContentTypes/Views/Admin/Edit.cshtml:63`; "Parts" `Edit.cshtml:144`.
- `add-3`
  - Actions: `HighlightAsync(scene.Page.Locator("#parts"), "Parts")` (`Edit.cshtml:146`), hold while the first sentence
    plays, then `ClearHighlightsAsync()` and
    `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Parts", Exact = true }))`
    (the element is a link that carries `role="button"`, so a role lookup for a link finds nothing).
  - On screen when the narration ends: the Add Parts page (breadcrumb "Add Parts"; the text "Choose the Parts to add
    to this Content Type.") with the list of parts and a button "Add & Configure" on each row.
  - Labels: "Add Parts" `Edit.cshtml:201`; "Choose the Parts to add to this Content Type." `Admin/AddPartsTo.cshtml:18`.
- `add-4`
  - Actions: define `AutorouteRow = scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "Provides a custom url for your content item." })`
    (the description comes from `Autoroute/Migrations.cs:23`). `await AutorouteRow.ScrollIntoViewIfNeededAsync()`,
    `HighlightAsync(AutorouteRow)`, hold about 4 seconds, `ClearHighlightsAsync()`, then
    `ClickAndWaitForNavigationAsync(AutorouteRow.Locator("button[name='submit.AddAndConfigure']"))`. Wait until the
    breadcrumb "Edit Part - Autoroute" is visible. Do not tick any checkbox and do not select Add Selected Parts.
  - On screen when the narration ends: the Edit Part page, "Edit Part - Autoroute", with the confirmation
    The "Autoroute" part has been added. and the first fields of the settings (Pattern, Allow custom path).
  - Labels: "Autoroute" is the part name without its Part suffix (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:60`);
    "Add & Configure" `Admin/AddPartsTo.cshtml:36`; the confirmation `ContentTypes/Controllers/AdminController.cs:355`;
    "Edit Part - {0}" `Admin/EditTypePart.cshtml:22`. The button label is "Add & Configure": the storyboard writes it
    that way, so the subtitles match the screen, and its `spoken` entry makes the voice say "Add and Configure".

### Clip `03-settings` (chapter 03, "Choose the Autoroute settings")

Starts on `/Admin/ContentTypes/Event/ContentParts/AutoroutePart/Edit` (`OpenAsync`, not recorded), with snapshot 2
restored. Wait until the label "Pattern" and the Pattern field are visible (the field is a Liquid editor that loads a
moment after the page), then `StartCaptureAsync()`.

Never click inside the Pattern field and never highlight it: the narration names it, and the viewer sees it as it is.
Leave the Container Settings checkboxes unchecked.

- `settings-1`
  - Actions: `MoveToAsync(scene.Page.Locator("label.ocat-label", new() { HasTextString = "Pattern" }))`. No click.
  - On screen when the narration ends: the Edit Part page, "Edit Part - Autoroute", with the cursor on the label
    "Pattern", the Pattern field with its default content, and the checkboxes below it, all unchecked.
  - Labels: "Pattern" `Autoroute/Views/AutoroutePartSettings.Edit.cshtml:14`, hint `:17`; the default pattern
    `Autoroute/Models/AutoroutePartSettings.cs:15-16`; "Edit Part - {0}" `ContentTypes/Views/Admin/EditTypePart.cshtml:22`.
- `settings-2`
  - Actions: `ClickAsync(scene.Page.GetByLabel("Allow custom path"))`. The click comes first because the narration
    opens with "Let's turn on Allow custom path".
  - On screen when the narration ends: "Allow custom path" checked, with its hint "Check to allow users to set a
    custom path, otherwise it will be automatically generated."
  - Labels: `AutoroutePartSettings.Edit.cshtml:25-26`. The Permalink field is disabled when the box is cleared:
    `Autoroute/Views/AutoroutePart.Edit.cshtml:37`.
- `settings-3`
  - Actions: `ClickAsync(scene.Page.GetByLabel("Allow path updates"))`.
  - On screen when the narration ends: "Allow custom path" and "Allow path updates" checked, with the hint of the
    second one: "Check to allow users to refresh the path once a content item is already published."
  - Labels: `AutoroutePartSettings.Edit.cshtml:35-36`. The editor label the narration names, "Refresh the path", is
    `AutoroutePart.Edit.cshtml:87`.
- `settings-4`
  - Actions: `ClickAsync(scene.Page.GetByLabel("Show homepage options"))`.
  - On screen when the narration ends: "Show homepage options" checked, with its hint about users with the
    appropriate permission. Do not touch any homepage setting of the site.
  - Labels: `AutoroutePartSettings.Edit.cshtml:45-46`; the editor label "Set as homepage" `AutoroutePart.Edit.cshtml:72`;
    the permission that gates it `Autoroute/Views/AutoroutePart.Edit.cshtml:55` and `Autoroute/Permissions.cs:9`
    (the Administrator role has it by default).
- `settings-5`
  - Actions: `ClickAsync(scene.Page.GetByLabel("Allow disabled"))`.
  - On screen when the narration ends: "Allow custom path", "Allow path updates", "Show homepage options" and "Allow
    disabled" checked, with the hint of the last one: "Check to allow the content items of this content type to
    disable autoroute."
  - Labels: `AutoroutePartSettings.Edit.cshtml:55-56`. The editor label the narration names, "Disabled", is
    `AutoroutePart.Edit.cshtml:21`; a disabled item has no path in the index (`src/OrchardCore/OrchardCore.Autoroute.Core/Indexes/AutoroutePartIndex.cs:135`).
- `settings-6`
  - Actions: define `Heading = scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Container Settings" })`, then
    `await Heading.ScrollIntoViewIfNeededAsync()`, `HighlightAsync(Heading)`, hold about 5 seconds,
    `ClearHighlightsAsync()`, then
    `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }))`
    (the click scrolls to the button by itself). Wait until the Parts list (`#parts`) is visible.
  - On screen when the narration ends: the Edit Content Type page of Event with the Parts list (Event, Title,
    Autoroute) and the confirmation The "Autoroute" part settings have been saved.
  - Labels: "Container Settings" `AutoroutePartSettings.Edit.cshtml:64`; "Save" `ContentTypes/Views/Admin/EditTypePart.cshtml:111`;
    the confirmation `ContentTypes/Controllers/AdminController.cs:1084`.

### Clip `04-create` (chapter 04, "Create a content item")

Starts on `/Admin/Contents/ContentItems` (`OpenAsync`, not recorded), with snapshot 3 restored. Wait until the New
button (`#new-dropdown`) is visible, then `StartCaptureAsync()`.

- `create-1`
  - Actions: `ClickAsync(scene.Page.Locator("#new-dropdown"))`, then
    `ClickAndWaitForNavigationAsync(scene.Page.Locator("#new-dropdown + .dropdown-menu a.dropdown-item", new() { HasTextString = "Event" }))`.
    Do not use a plain `.dropdown-menu a.dropdown-item` lookup: the Content Type filter of the same page is a
    bootstrap-select list whose menu also holds an "Event" entry (`Contents/Views/ContentsAdminListFilters.cshtml:8`).
    Wait until the Title field is visible.
  - On screen when the narration ends: the editor for an Event with an empty Title and an empty Permalink field.
  - Labels: the menu path Content, Content Items `Contents/AdminMenu.cs:65,68`; "New" `Contents/Views/ContentsAdminListCreate.cshtml:20`
    (a dropdown, because several types can be created); the item "Event" is the display name of the type.
- `create-2`
  - Actions: `TypeAsync(scene.Page.GetByLabel("Title", new() { Exact = true }), "Spring Planting Workshop", delay: 60)`.
    Then `HighlightAsync(scene.Page.Locator(".input-group", new() { Has = scene.Page.GetByLabel("Permalink") }))`
    (no caption), and hold. Do not type in the Permalink field.
  - On screen when the narration ends: Title "Spring Planting Workshop", the empty Permalink field ringed with its hint
    "The url of the content item. Leave empty to auto-generate it.", and the checkboxes "Disabled", "Set as homepage"
    and "Refresh the path" on the form.
  - Labels: "Title" `Title/Views/TitlePart.Edit.cshtml:12`; "Permalink" `AutoroutePart.Edit.cshtml:30`, hint
    `:46`; "Disabled" `:21`; "Set as homepage" `:72`; "Refresh the path" `:87`.
- `create-3`
  - Actions: `ClearHighlightsAsync()`, then
    `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }))`.
    Wait until a row that contains "Spring Planting Workshop" is visible in the list.
  - On screen when the narration ends: the Content Items page with the confirmation Your Event has been published.
    and the row "Spring Planting Workshop". (The form comes from the New menu, which passes a return address, so Publish
    returns to the list: `Contents/Controllers/AdminController.cs:795-797`.)
  - Labels: "Publish" `Contents/Views/Content.PublishButton.cshtml:19`; the confirmation `AdminController.cs:473`.
  - The sentence about a duplicate (a dash and a number added) is not shown on screen: it is read from
    `Autoroute/Handlers/AutoroutePartHandler.cs:435-437` and `:455-488`.
- `create-4`
  - Actions: define `ItemRow = scene.Page.Locator(".list-group-item, tr").Filter(new() { HasText = "Spring Planting Workshop" }).First`.
    `ClickAndWaitForNavigationAsync(ItemRow.Locator("a.edit"))`. Wait until the Permalink field has the value
    `spring-planting-workshop`. Then `HighlightAsync(scene.Page.Locator(".input-group", new() { Has = scene.Page.GetByLabel("Permalink") }))`.
  - On screen when the narration ends: the editor of the event, with the Permalink field ringed and holding
    `spring-planting-workshop`, and the link icon (external link) at the end of the field.
  - Labels: "Edit" `Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`; the link icon, shown only for a published
    item, `AutoroutePart.Edit.cshtml:38-41`.
- `create-5`
  - Actions: `ClearHighlightsAsync()`. Define `Link = scene.Page.Locator("a.text-info:has(i.fa-external-link)")`. Open
    the link in its new tab: `var popup = await scene.Page.Context.RunAndWaitForPageAsync(() => scene.ClickAsync(Link))`,
    then `await scene.SwitchToAsync(popup)`. Wait until `popup.Locator("h1").First` shows "Spring Planting Workshop" (use
    `.First`: the page can hold more than one `h1`). Then `HighlightAsync(popup.Locator("h1").First, "/spring-planting-workshop")`. The caption shows the address, because
    the capture has no address bar.
  - On screen when the narration ends: the front end of the site with the event page, its heading "Spring Planting
    Workshop" ringed, and the caption `/spring-planting-workshop`.
  - Labels: no UI label. With the Blog theme, the Event is drawn by the theme's `Content.liquid`, which prints the title
    in a masthead `h1` (`src/OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:9`) and renders `Model.Content`; the
    Title part's own `h1` (`Title/Views/TitlePart.cshtml`, location Detail, Header) may not be drawn at all. The page is
    served at the permalink by `Autoroute/Routing/AutorouteTransformer.cs`.
  - This is the last step of the clip: leave the browser on that page.

### Clip `05-change` (chapter 05, "Change a permalink")

Starts on `/Admin/Contents/ContentItems` (`OpenAsync`, not recorded), with snapshot 4 restored. Wait until the row of
the event is visible, then `StartCaptureAsync()`. Define `ItemRow` as in `create-4`, with `HasText = "Spring Planting Workshop"`
(it is used by `change-1` only; `change-2` and the steps after it use `ItemRowDay`, defined in `change-2`), and `PermalinkGroup` as
`scene.Page.Locator(".input-group", new() { Has = scene.Page.GetByLabel("Permalink") })`. To replace the text of a
field: `ClickAsync(field)`, then `scene.Page.Keyboard.PressAsync("Control+A")`, then
`scene.Page.Keyboard.PressAsync("Delete")`, then `TypeAsync(field, text)`. The field must be empty before `TypeAsync`,
because `TypeAsync` clicks the field first (which drops the selection) and then adds the text where the cursor is.

- `change-1`
  - Actions: `ClickAndWaitForNavigationAsync(ItemRow.Locator("a.edit"))`. Wait until the Title field is visible. Replace
    the title with `Spring Planting Day`. Do not publish yet.
  - On screen when the narration ends: the editor with Title "Spring Planting Day" and the Permalink field still
    holding `spring-planting-workshop`.
  - Labels: "Edit" `ContentsButtonEdit_SummaryAdmin.cshtml:13`; "Title" `TitlePart.Edit.cshtml:12`.
- `change-2`
  - Actions: `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }))`.
    Wait for the list and the confirmation Your Event has been published. Define
    `ItemRowDay = scene.Page.Locator(".list-group-item, tr").Filter(new() { HasText = "Spring Planting Day" }).First`
    (the row now reads "Spring Planting Day"), then `ClickAndWaitForNavigationAsync(ItemRowDay.Locator("a.edit"))`. Wait
    until the Permalink field has the value `spring-planting-workshop`, then `HighlightAsync(PermalinkGroup)`.
  - On screen when the narration ends: the editor with Title "Spring Planting Day" and the Permalink field ringed,
    still holding `spring-planting-workshop`.
  - Why: the path is generated only while it is empty (`Autoroute/Handlers/AutoroutePartHandler.cs:397-403`). Publish
    returns to the list because the Edit button passes a return address (`ContentsButtonEdit_SummaryAdmin.cshtml:13`)
    and `Contents/Controllers/AdminController.cs:858` redirects to it.
- `change-3`
  - Actions: `ClearHighlightsAsync()`. `ClickAsync(scene.Page.GetByLabel("Refresh the path"))`. Hold about 4 seconds so
    the hint is readable. Then `ClickAndWaitForNavigationAsync` on the Publish button as in `change-2`. Wait for the
    confirmation Your Event has been published.
  - On screen when the narration ends: the Content Items page with the confirmation and the row "Spring Planting Day".
  - Labels: "Refresh the path" `AutoroutePart.Edit.cshtml:87`, hint `:88` ("Check to refresh the path. Warning: the
    previous URL won't be accessible anymore."). The box is shown because "Allow path updates" was checked in
    `03-settings` (`AutoroutePart.Edit.cshtml:81`). Emptying the path on update: `Autoroute/Drivers/AutoroutePartDisplayDriver.cs:103-107`.
- `change-4`
  - Actions: `ClickAndWaitForNavigationAsync(ItemRowDay.Locator("a.edit"))`. Wait until the Permalink field has the value
    `spring-planting-day`, then `HighlightAsync(PermalinkGroup)`.
  - On screen when the narration ends: the Permalink field ringed and holding `spring-planting-day`, with the Title
    "Spring Planting Day" above it.
  - Why: the pattern runs again on an empty path: `AutoroutePartHandler.cs:145-149` and `:397-437`.
- `change-5`
  - Actions: `ClearHighlightsAsync()`. Replace the content of the Permalink field (`scene.Page.GetByLabel("Permalink")`)
    with `spring planting` (with a space). Then
    `ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }))`
    (the form comes back on the same page). Wait until the message starting "Please do not use any of the following
    characters in your permalink" is visible, then `HighlightAsync` on that message (a `.field-validation-error`
    element near the Permalink field).
  - On screen when the narration ends: the editor, with the typed Permalink `spring planting` and the message
    "Please do not use any of the following characters in your permalink: ... No spaces, or consecutive slashes, are
    allowed (please use dashes or underscores instead)." ringed.
  - The same message is also listed in the validation summary at the top of the form
    (`Contents/Views/Admin/Edit.cshtml:53-55`); ring the one beside the Permalink field.
  - Labels: the message `Autoroute/Models/AutoroutePartExtensions.cs:17-18` (a space is refused: `:27-46`); the
    other message the narration refers to is "Your permalink is already in use." (`AutoroutePartDisplayDriver.cs:127`
    and `AutoroutePartHandler.cs:119-135`, `ValidatingAsync`), which is not shown on screen.
- `change-6`
  - Actions: `ClearHighlightsAsync()`. Replace the content of the Permalink field with `spring-planting`. Click
    Publish as in `change-5` and wait for the list and the confirmation Your Event has been published. Then
    `ClickAndWaitForNavigationAsync(ItemRowDay.Locator("a.edit"))`, wait until the Permalink field has the value
    `spring-planting`, and `HighlightAsync(PermalinkGroup)`.
  - On screen when the narration ends: the editor with the Permalink field ringed and holding `spring-planting`.
  - Why: a typed path is kept when "Allow custom path" is on: `AutoroutePartDisplayDriver.cs:98-101`.

## 4. Slides, cards

- `why-1`: a slide titled "Without Autoroute" with three cards: an address built from an identifier, hard to read, hard
  to guess. Drawn by `slides.py`.
- `why-2`: a slide titled "With Autoroute" with three cards: a part of the content type, a pattern, a permalink for
  each saved item. Drawn by `slides.py`.
- `intro`, `outro`: the opening and closing cards from the storyboard. No recording.

## 5. Checks for private data

Before assembling, look at every contact sheet and check:

- No password, token or key appears on any frame. The sign-in is not recorded.
- Only the fake data of this plan is visible: the content type Event, the items "Spring Planting Workshop" and "Spring
  Planting Day", and the Blog recipe's own sample items (Lorem ipsum text). The user name admin is the only user name.
- The Content Items list (`04-create`, `05-change`) and the front-end page show no real name or company. Look at the
  footer and the menu of the front-end page in particular.
- No error page or error message appears, except the permalink message of `change-5`, which is intended.
- OCR the video at one frame per second and at each scene change, and search for real names, emails (only
  `@example.com` may appear), phone numbers and tokens.
- Do not commit `state.json`, an `App_Data` copy, an MP4 or an audio file.
