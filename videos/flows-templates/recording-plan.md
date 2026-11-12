# Recording plan: flows-templates

**Flow and Bag Parts: Templates and Alternates for Their Widgets.** A developer video of 22 steps: the opening card, 16
slides in six chapters, two short browser clips, and the closing card. Only the two clips need the browser; the slides
and the cards are drawn by `slides.py` and the scripts of the skill. A selector below stands for
`scene.Page.Locator(...)` (or `GetByLabel`), since the recorder's methods take locators. Source paths are relative to
`src/OrchardCore.Modules` of the Orchard Core checkout, unless they start with `src/`.

## The site it needs

- **Run** `src/OrchardCore.Cms.Web` on `http://localhost:5010`, with its own `App_Data` folder outside both
  repositories (`ORCHARD_APP_DATA`), and outbound calls blocked (`HTTP_PROXY` and `HTTPS_PROXY` set to a dead address).
  The site does not call any external service, so there is no local stand-in to start.
- **Auto Setup**, so that no setup form and no sign-in form is recorded (variables of
  `src/docs/reference/modules/AutoSetup/README.md`):
  - `OrchardCore__AutoSetup__Tenants__0__ShellName` = `Default`
  - `OrchardCore__AutoSetup__Tenants__0__SiteName` = `Orchard Core`
  - `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` = `America/Los_Angeles`
  - `OrchardCore__AutoSetup__Tenants__0__AdminUsername` = `admin`
  - `OrchardCore__AutoSetup__Tenants__0__AdminEmail` = `admin@example.com`
  - `OrchardCore__AutoSetup__Tenants__0__AdminPassword` = `<password>` (chosen outside git, the same value as
    `VIDEO_PASSWORD`)
  - `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` = `Sqlite`
  - `OrchardCore__AutoSetup__Tenants__0__RecipeName` = **`Agency`**
  The section is `OrchardCore:AutoSetup` (`OrchardCore.AutoSetup/Startup.cs:24-29`; the older section
  `OrchardCore_AutoSetup` is deprecated, so do not use its names). Auto Setup runs on the first request of the site.
- **Why the Agency recipe and not the Blog recipe.** The Blog recipe has no content type with a Bag Part. The Agency
  recipe (`OrchardCore.Themes/TheAgencyTheme/Recipes/agency.recipe.json`, display name "Agency") defines the content
  type `Landing Page` with the named Bag Parts `Services`, `Portfolio`, `About`, `Team` and `Clients` (lines 169-295),
  and enables Flows, Content Types, Settings and Templates (the `feature` step). Nothing has to be enabled or created
  before recording, and nothing is seeded. The recipe fetches nothing from the network: its `media` step copies files
  from the theme's own `wwwroot` (`SourcePath` entries only), so the dead proxy does not stop the setup.
- **Sign in** once, outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`
  saves `state.json` (never committed). Do not write the password anywhere in this repository.
- **Display**: Chromium 1536x864, device scale 1.25, light color scheme, `en-US`, as the skill specifies.
- **No mask script** is needed, because no page of the clips shows content items. See "Pages that must never appear".

## Pages that must never appear

The Agency recipe seeds content items (clients, team members, portfolio projects) whose names are not fake data of
this programme. The two clips only visit the pages below, which list definitions and settings, never content. Do not
open, and do not let a step pass through: the front end (`/`), the Contents list, the editor of the `Landing Page`
content item, the Media library, the Menus, and the admin dashboard.

## Order of the clips, and what each one changes

1. Start the site, run `Recorder setup`, stop the site, and **copy `App_Data` and `state.json` aside** (the snapshot).
2. `03-display` reads only. It does not change the site.
3. `06-debug` **changes the site**: it saves the setting Write shape debug information. Before another take of
   `06-debug`, stop the site, restore the snapshot, and start it again (or uncheck the setting and save it).
4. The two clips do not depend on each other. Every other step is a slide or a card.

## Clip `03-display` (chapter 03, after the slide `bag-1`)

Starts on `/Admin/ContentTypes/Edit/LandingPage` (route `ContentTypes/Edit/{id}`,
`OrchardCore.ContentTypes/Controllers/AdminController.cs:167`), opened with `scene.OpenAsync`, not recorded. The page
title is "Edit Content Type - Landing Page" (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`). Before
`StartCaptureAsync`, scroll the page until the heading **Parts** (`Edit.cshtml:144`) is near the top of the viewport and
the list `#parts` (line 146) is fully visible, so that the rows Services, Portfolio, About, Team and Clients can be
read. The rows follow the positions of the recipe (`agency.recipe.json`): Title (0) and Autoroute (1) come first, then
the five Bag Parts (3 to 7), and the part of the type itself, Landing Page (8), comes last (`Edit.cshtml:32-42`). For
example:
`await scene.Page.EvaluateAsync("document.querySelector('#parts').scrollIntoView({ block: 'center' })")`.
Then `await scene.StartCaptureAsync()`.

- `display-1` (about 13 s of narration). Actions:
  1. `await scene.HighlightAsync(scene.Page.Locator("#parts li").Filter(new() { HasText = "Services" }))`, without a
     caption. The row shows the part's display name "Services" (`agency.recipe.json:219`) and its Edit and Remove
     buttons.
  2. `await scene.PauseAsync(600)`.
  - Visible at the end of the narration: the parts list with a green ring around the Services row; the Portfolio,
    About, Team and Clients rows are visible below it.
  - Labels: "Parts" and "Edit" from `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:144` and `:163`. The Edit
    button is a link written with `role="button"`, so a locator by role finds it as a button, not as a link.
- `display-2` (about 11 s of narration). Actions:
  1. `await scene.ClearHighlightsAsync()`.
  2. `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#parts li").Filter(new() { HasText = "Services" }).GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true }))`.
     The page becomes `/Admin/ContentTypes/LandingPage/ContentParts/Services/Edit`, titled "Edit Part - Services"
     (`OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:22`).
  3. Wait until `scene.Page.GetByLabel("Display Type", new() { Exact = true })` is visible.
  4. Scroll the field to the middle of the viewport (highlights are cleared, since they are fixed on the screen):
     `await scene.Page.GetByLabel("Display Type", new() { Exact = true }).EvaluateAsync("e => e.scrollIntoView({ block: 'center' })")`,
     then `await scene.PauseAsync(400)`. The hint sits right under the field, so both are inside the viewport.
  5. `await scene.HighlightAsync(scene.Page.GetByLabel("Display Type", new() { Exact = true }))`. `HighlightAsync`
     scrolls the field into view by itself, but it does not bring the hint into view: do step 4 first.
  - Visible at the end of the narration: the text field Display Type with the grey placeholder **Summary**, a green
    ring around it, and the hint "The display type to use when rendering the content items. Default is Summary."
    under it. Nothing is typed and nothing is saved: leave the page with the Cancel button or by closing the scene.
  - Labels: "Edit" as for `display-1`; "Display Type", the placeholder `Summary` and the hint come from
    `OrchardCore.Flows/Views/BagPartSettings.Edit.cshtml:40-43`.

## Clip `06-debug` (chapter 06, before the slide `source-1`)

Starts on `/Admin/Settings/debugging` (route `Settings/{groupId}`,
`OrchardCore.Settings/Controllers/AdminController.cs:50`; the group id is `debugging`,
`OrchardCore.Settings/Drivers/DebugSettingsDisplayDriver.cs:14`), opened with `scene.OpenAsync`, not recorded. The page
has the single option "Write shape debug information", unchecked, and the Save button. The admin menu path is
Settings, then Debugging (`OrchardCore.Settings/AdminMenu.cs:36` and `:46`). Then `await scene.StartCaptureAsync()`.

- `debug-1` (about 7 s of narration). Actions:
  1. `await scene.HighlightAsync(scene.Page.GetByLabel("Write shape debug information"))`.
  The admin menu is not highlighted, whatever state it is in.
  - Visible at the end of the narration: the checkbox with its label "Write shape debug information" and the hint
    "Wrap rendered shapes in HTML comments to help identify the template or binding that produced each fragment.",
    with a green ring around the checkbox. The checkbox is still unchecked.
  - Labels: `OrchardCore.Settings/Views/DebugSettings.Edit.cshtml:7` and `:9`.
- `debug-2` (about 9 s of narration). Actions:
  1. `await scene.ClearHighlightsAsync()`.
  2. `await scene.ClickAsync(scene.Page.GetByLabel("Write shape debug information"))`, which checks it.
  3. Save the settings. The controller suspends the shell release request only when the model is invalid
     (`OrchardCore.Settings/Controllers/AdminController.cs:106-127`), which suggests that a valid save reloads the
     tenant, so the redirect that shows the notification can take several seconds. Always cut the wait out of the clip, so
     there is nothing to time: with `save` = `scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true })`,
     run `await scene.MoveToAsync(save)`, then
     `await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => save.ClickAsync()))`.
  4. Inside the same `OffCameraAsync` call, after the navigation, wait until the text "Site settings updated
     successfully." is visible
     (`await scene.Page.GetByText("Site settings updated successfully.").WaitForAsync()`).
  - Visible at the end of the narration: the checkbox checked and the notification "Site settings updated
    successfully.".
  - Labels: "Save" from `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; the notification from
    `OrchardCore.Settings/Controllers/AdminController.cs:119`.
- After the take, restore the snapshot (see the order above).

## The slides and the cards (drawn, nothing to record)

Chapter cards of 2.6 s are drawn by the scripts for the chapters 01 to 06. In a slide, "bright" lines are the ones the
narration talks about; the others are dimmed.

- `intro` (card): the opening card, "Templates for Flow and Bag Parts".
- `flow-1` (slide): `FlowPart.cshtml` of the Flows module, trimmed; the loop and the skipped items are bright.
- `flow-2` (slide): the same code; the `BuildDisplayAsync` call is bright.
- `flow-3` (slide): the same code; the `FlowMetadata` block that adds the classes is bright.
- `widget-1` (slide): `Widget-Paragraph.liquid` of The Blog Theme, with three cards: the file, the alternate and the classes.
- `widget-2` (slide): two cards: a dash is two underscores; a dot adds a display type.
- `widget-3` (slide): four cards with the classes of a widget in a Flow.
- `bag-1` (slide): `BagPart.cshtml` of the Flows module, trimmed; the line with `Model.Settings.DisplayType` is bright.
- `items-1` (slide): two cards: a Paragraph widget renders as Widget; a Service item renders as Content.
- `items-2` (slide): two cards: `Widget-Paragraph.liquid` does not apply to Summary; the Summary file does.
- `part-1` (slide): four cards with the file names that target the Bag Part shape.
- `part-2` (slide): the `Location` lines of `FlowPartDisplayDriver.cs` and `BagPartDisplayDriver.cs`.
- `part-3` (slide): the placement rule that sets the shape of an empty Flow, and the two empty template files.
- `part-4` (slide): `FlowPart.cshtml` of The Default Theme, trimmed, and three cards.
- `direct-1` (slide): the landing page template of the Agency recipe; the `for` line is bright.
- `direct-2` (slide): the same code; the `shape_build_display` line and the `HtmlBodyPart` line are bright.
- `source-1` (slide): the shape debug comments that the code writes around the Widget shape of a Paragraph widget (not
  the example of the documentation, which spells them differently), and the `console_log` filter.
- `outro` (card): the closing card, "docs.orchardcore.net".

## Checks for private data

- Before recording, look at the two start pages in a normal browser: they show definitions and one setting, never
  content items, and the only personal value is the user name `admin` in the admin top bar.
- After the take, look at the contact sheet of each clip: no content item name, no email address other than
  `admin@example.com`, no token, and no password. The password field is never visible in a clip.
- OCR the final video at one frame per second and at every scene change, as the skill describes, and search for
  e-mail addresses and for the names of the Agency seed content.
- Do not publish if a frame shows a content item, the Contents list or the front end of the site.
