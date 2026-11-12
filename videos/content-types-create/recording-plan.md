# Recording plan: Content Types, Create a Type and Add Parts and Fields

A recorder who reads only this file can make the video. Selectors are `scene.Page.Locator(...)` calls; `GetByLabel` and
`GetByRole` stand for the same methods of `scene.Page`. Every label is written as the code spells it, with the file and
line of the Orchard Core source it comes from (paths are relative to `src/OrchardCore.Modules/OrchardCore.ContentTypes`
unless they start with another module name).

The video is an admin video: the screen shows the admin UI only. Never open the settings of the Title part, and never
scroll a page far enough to show an editor with HTML or Liquid in it (see "Private data and code on screen").

## The site it needs

| Item | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web`, run on `http://localhost:5010`, with its own empty `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`) |
| Setup | Auto Setup, no setup screen and no sign-in screen on camera |
| Tenant | `Default` |
| Recipe | `Blog` (recipe file `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`) |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| User | `admin`, email `admin@example.com`, password `<password>` (passed in `VIDEO_PASSWORD`, chosen outside git) |
| Database | SQLite |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls out |
| Browser | the recorder's Chromium (1536x864, light color scheme, `en-US`) |
| Sign-in | `Recorder setup` once, outside the capture, which writes `state.json` (never committed) |

Auto Setup environment variables (the password is a placeholder here; use the value of `VIDEO_PASSWORD`):

```bash
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="<password>"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

### Running the recorder

Write the recorder's scenes file `recorder/Scenes.cs` of this video from this plan (one method per clip, one step per step
id), build the recorder with it, and sign it in once, outside the capture (`<repo>` is the path of the checkout, `<bin>`
the folder of the built recorder):

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/content-types-create/recorder
export VIDEO_ROOT=<repo>/videos/content-types-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
<bin>/Recorder setup
```

Narrate first (`scripts/narrate.py videos/content-types-create`), then record the clips in the order of the next section:
`<bin>/Recorder record 02-create`, then `03-parts`, `04-fields`, `05-order` and `06-item`. The video has no seed data and no
`prepare` step.

### Features

The Blog recipe enables every feature the video needs: Content Types (`OrchardCore.ContentTypes`), Content Fields
(`OrchardCore.ContentFields`), Title (`OrchardCore.Title`), Contents and Admin. Nothing is enabled before recording. The
pages the video shows belong to the Content Types feature, which is the one the Blog recipe lists under
"Content Management" in `blog.recipe.json` (the `feature` step, `OrchardCore.ContentTypes`).

### Settings

None are saved. Leave the admin settings at their defaults: the lists use the List layout with the Buttons actions layout
(`OrchardCore.Admin.Abstractions/AdminListOptions.cs`, `DefaultLayout` and `DefaultActionsLayout`), which puts the four
buttons of a content type on its row. The "New" menu of the admin stays off (it is off by default,
`OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:18`), so the video creates the item from the row of the type.

The admin lists also show a selector for the layout (List or Grid), because the setting "Let users choose the layout of a
list" is on by default (`AdminSettings.cs:49`, `OrchardCore.Admin/Configuration/AdminListOptionsConfiguration.cs:40`,
`OrchardCore.Admin/AdminListShapeTableProvider.cs:80`). Never click it: the choice is kept in a cookie, so a Grid choice
saved in `state.json` would change the rows of every list. If a take shows the Grid layout, remove the cookie from
`state.json` and sign in again.

### External services

None. No local stand-in is needed.

### Seed data

None is created outside the capture. The Blog recipe's own types (Article, BlogPost, Blog, Container, Blockquote, Image,
Paragraph, RawHtml, Page, Tag, Category) are on the site, among others that the enabled features create in their
migrations; no recipe or migration of the source creates a type named `Event` (searched), and `Event` is not a reserved
name (`OrchardCore.ContentManagement.Abstractions/Utilities/StringExtensions.cs:14-27`). Of the recipe's types only Article,
Blog and Page are Creatable (`blog.recipe.json`), so only their rows have a "Create New" button (this is why `create-2`
uses the Article row); the row of the `Event` type gets one because a created type is Creatable.

The fake values typed on camera:

| Where | Value |
|---|---|
| Type, Display Name | `Event` |
| Type, Technical Name | filled in by the page from the Display Name; it must read `Event` |
| Part | `Title` |
| Field 1, Display Name and type | `Venue`, Text Field |
| Field 2, Display Name and type | `Starts`, Date Time Field; Required ticked; Hint `Local time at the venue.` |
| Field 3, Display Name and type | `Capacity`, Numeric Field |
| Item, Title | `Community Meetup` |
| Item, Starts | `2031-03-14T18:30` (shown as 03/14/2031, 06:30 PM) |
| Item, Venue | `Riverside Hall` |
| Item, Capacity | `120` |

## Order of the clips, and what changes the site

The clips depend on each other: record them in the order of the storyboard. Every clip from `02-create` to `06-item`
changes the site, so a snapshot must be restored before a new take.

| Clip | Needs | Changes the site |
|---|---|---|
| `01-model` (slides) | nothing | nothing |
| `02-create` | the site after Auto Setup, with no `Event` type | creates the `Event` type |
| `03-parts` | `Event` exists, with no part attached | attaches the Title part |
| `04-fields` | `Event` with the Title part | adds the fields Venue, Starts (configured) and Capacity |
| `05-order` | `Event` with the three fields | stores the order of the parts and of the fields |
| `06-item` | the order stored by `05-order` | creates and publishes one `Event` item |
| `07-recap` (slides) | nothing | nothing |

Snapshots (stop the site first, by its process ID, never by image name; copy the `App_Data` folder and `state.json`
aside; start the site again):

1. `snap-0`: after Auto Setup and `Recorder setup`, before any clip.
2. `snap-2`: after `02-create`. `snap-3`: after `03-parts`. `snap-4`: after `04-fields`. `snap-5`: after `05-order`.

To record clip N again, stop the site, restore `snap-(N-1)` (restore `snap-0` for `02-create`), and start the site.

## How every clip starts

Each clip opens its first page before `StartCaptureAsync` (not recorded), so a clip can be recorded alone from the right
snapshot. Open pages with `scene.OpenAsync`, never with a link that is part of the story. Scroll positions chosen before
the capture are not recorded. Clear highlights (`ClearHighlightsAsync`) before every scroll: highlights are fixed on the
screen. Navigate only with `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` or `NavigateAsync`.

The pattern used below: a step whose narration ends with "Select X" holds the click on X for the start of the next step,
where the narration describes the result. A step whose actions run longer than its narration needs shorter typing
(`delay: 40`).

---

## Slides

Slide steps are drawn by `slides.py`; there is nothing to record.

- `model-1`: the heading "How a content type is built" with four cards; the first card, "Content type", is bright.
- `model-2`: the same slide; the second card, "Parts", is bright.
- `model-3`: the same slide; the third card, "Fields", and the fourth, "The part named like the type", are bright.
- `recap-1`: the heading "Creating a content type" with four cards: Create the type, Add parts, Add fields, Set the order.

---

## Clip `02-create`

Starts on `/Admin` (the admin home), with the admin menu in its initial state. No `Event` type exists. Chapter 02.

Path of the pages: `/Admin/ContentTypes/List`, then `/Admin/ContentTypes/Create`, then the Add Parts page
(`/Admin/ContentTypes/AddPartsTo/Event`). Set `MSYS_NO_PATHCONV=1` in Git Bash when probing these paths.

- `create-1`: the menu path. The admin menu is rendered fully collapsed on `/Admin` (the default behavior of the menu is
  "Focused": `OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:29`, `AdminMenuBehavior.cs`;
  `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts:157`). "Design" and "Content Definition" have children and
  no page of their own, so the theme renders each as a `button` that expands its section
  (`OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml`, `isToggleOnlyItem`); "Content Types" is a link.
  Click, in this order, with `var menu = scene.Page.Locator("#left-nav");`:
  `menu.GetByRole(AriaRole.Button, new() { Name = "Design", Exact = true })`, then
  `menu.GetByRole(AriaRole.Button, new() { Name = "Content Definition", Exact = true })`, then
  `menu.GetByRole(AriaRole.Link, new() { Name = "Content Types", Exact = true })` with `ClickAndWaitForNavigationAsync`.
  Before each click call `WaitForAsync()` on its locator, because a section opens with a transition. Never expand a menu
  section during `Recorder setup`. Then move the cursor over the list. Visible when the narration ends: the Content Types
  page, titled "Content Types", with one row per type (Article, Blog, and so on), each with a technical-name badge, and
  the `#search-box` at the top. Labels: `AdminMenu.cs:23-25` (Design, Content Definition, Content Types),
  `Views/Admin/List.cshtml:6` (page title), `Views/ContentTypeEntry.DefaultTags.SummaryAdmin.cshtml:11` (technical name
  badge). If a locator matches nothing on the running site, `Recorder probe` lists the links of the page.
- `create-2`: no click. Move to the row of the "Article" type (the list is sorted by Display Name, so it is among the first
  rows; scroll to it with highlights cleared if it is not in view) and highlight its four buttons with
  `HighlightAsync(scene.Page.Locator("[role='btn-edit-Article']"))` and the same for `[role='btn-list-Article']`,
  `[role='btn-delete-Article']` and the "Create New Article" button `#btn-create-Article`. Visible: that row with its
  buttons ringed, reading "Create New Article", "List Items", "Edit" and "Delete". Labels:
  `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19, 22, 24, 30/36`. Do not click Delete.
- `create-3`: `ClearHighlightsAsync`; scroll to the top of the page. Highlight the "Create new type" button
  (`#btnCreate`); do not click it yet. Visible: the Content Types page with the button ringed. The permissions the
  narration names: `OrchardCore.Contents.Core/ContentTypesPermissions.cs:8, 10` ("View content types", "Edit content
  types") and `Permissions.cs:17-25` (the Administrator role has both). Label: `Views/Admin/List.cshtml:20`
  ("Create new type").
- `create-4`: `ClearHighlightsAsync`. Click the "Create new type" button (`#btnCreate`) with
  `ClickAndWaitForNavigationAsync`. On the page titled "New Content Type" type `Event` into `#DisplayName` with
  `TypeAsync(scene.Page.Locator("#DisplayName"), "Event", delay: 90)`. Visible: Display Name `Event`, and Technical Name
  `Event` filled in by the page, with the Create button below. Labels: `Views/Admin/Create.cshtml:5` ("New Content
  Type"), `:14` ("Display Name"), `:22` ("Technical Name"). The page fills the Technical Name from the Display Name on key
  up and blur (`Assets/js/autocomplete.ts`). Press Tab after typing so the blur runs (`scene.Page.Keyboard.PressAsync("Tab")`).
- `create-5`: highlight the Technical Name field (`#Name`, one element on this page) with no caption, then highlight the Create button
  (`scene.Page.Locator("button.create")`). Do not click yet. Visible: the filled form with the Create button ringed.
  Rules read in the narration: `Controllers/AdminController.cs:132-149` (starts with a letter, valid characters,
  reserved, already used); the Technical Name is read-only once the type exists: `Views/ContentType.Edit.cshtml:6`.
- `create-6`: `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(scene.Page.Locator("button.create"))`. Visible:
  the page titled "Add Parts" (`Views/Admin/AddPartsTo.cshtml:10`) with the notification `The "Event" content type has been
  created.` (`Controllers/AdminController.cs:162`) and the line "Choose the Parts to add to this Content Type."
  (`Views/Admin/AddPartsTo.cshtml:18`). Keep the cursor still while the narration ends.

---

## Clip `03-parts`

Starts on `/Admin/ContentTypes/AddPartsTo/Event` (snapshot `snap-2`). The `Event` type exists with no part attached.
Chapter 03.

- `parts-1`: highlight the list of parts (`scene.Page.Locator("ul.list-group")`), then tick the Title part with
  `ClickAsync(scene.Page.GetByLabel("Title", new() { Exact = true }))`. Visible when the narration ends: the Title row
  ticked, with its description "Provides a Title for your content item.", and the "Add Selected Parts" button shown at
  the bottom of the page. Labels: `Views/Admin/AddPartsTo.cshtml:30-33`; the display name of a part is its name without
  the `Part` suffix (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-64`)
  and the description is set in `OrchardCore.Title/Migrations.cs:33`. The button appears once a box is ticked:
  `Assets/js/add-parts-to.ts:9`.
- `parts-2`: `ClearHighlightsAsync`. Highlight `#add-selected-parts-button`, then the "Add & Configure" button of the
  Title row (`scene.Page.Locator("button[name='submit.AddAndConfigure'][value='TitlePart']")`). Do not click either. Visible: the "Add Selected Parts" button ringed. Labels:
  `Views/Admin/AddPartsTo.cshtml:36` ("Add & Configure"), `:47` ("Add Selected Parts"). Behavior read in the narration:
  `Controllers/AdminController.cs:337-358` (Add & Configure adds one part and opens its settings).
- `parts-3`: `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync(scene.Page.Locator("#add-selected-parts-button"))`.
  Visible: the page titled "Edit Content Type - Event" (`Views/Admin/Edit.cshtml:63`), scrolled to the top, with the
  notification about the added part (it reads `The "TitlePart" part has been added.`, with the technical name of the part,
  because the message joins the names of the selected parts: `Controllers/AdminController.cs:372`; it is not read in the
  narration), the Technical Name (read-only) and Display Name, and the checkboxes "Creatable",
  "Listable", "Draftable", "Versionable" and "Securable" ticked. Highlight the "Creatable" and "Listable" labels
  (`GetByLabel("Creatable")`, `GetByLabel("Listable")`). Labels: `Views/ContentTypeSettings.Edit.cshtml:17-69`. The
  defaults: `Services/ContentDefinitionService.cs:85-91`. Do not change any setting.
- `parts-4`: `ClearHighlightsAsync`, then scroll down to the "Parts" heading with `ScrollAsync(0, ...)` (the "Fields"
  heading, with no field in its list and the "Add Field" button, is above it). Highlight the Title row (`ul#parts li`
  with the text "Title") and, inside it, its "Edit" and "Remove" buttons. Visible: the Parts list with the single row
  Title and its buttons "Edit" and "Remove", and the buttons "Add Parts" and "Add Named Part" below the list. The part named
  Event is not listed yet: a type's own part is listed only once it holds a field (`Views/Admin/Edit.cshtml:25-30` skips
  a part with no definition, and the definition is stored with the first field: see `OPEN-QUESTIONS.md`). Labels:
  `Views/Admin/Edit.cshtml:144` ("Parts"), the "Edit" (`:163`) and "Remove" (`:174`) buttons of a part, `:201-202`
  ("Add Parts", "Add Named Part"). Do not click Edit (it opens the settings of the part), Remove, Add Parts or Add Named Part.

---

## Clip `04-fields`

Starts on `/Admin/ContentTypes/Edit/Event` (snapshot `snap-3`). Before `StartCaptureAsync`, scroll so the "Fields" heading
(`GetByRole(AriaRole.Heading, new() { Name = "Fields" })`) is near the top of the screen. Chapter 04.

- `fields-1`: click "Add Field" (`scene.Page.Locator("a", new() { HasText = "Add Field" })`, the button under the Fields
  list) with `ClickAndWaitForNavigationAsync`. On the page titled `Add New Field To "Event"` type `Venue` into
  `#DisplayName` (`delay: 70`) and press Tab. Visible: Display Name `Venue`, Technical Name `Venue`, and the start of
  the "Field Type" list. Labels: `Views/Admin/Edit.cshtml:137` ("Add Field"), `Views/Admin/AddFieldTo.cshtml:20` (page
  title), `:38` ("Display Name"), `:46` ("Technical Name"), `:54` ("Field Type"). The Technical Name fills in like on the
  type page (`Views/Admin/AddFieldTo.cshtml:18`, the same `autocomplete` script).
- `fields-2`: scroll down so the whole Field Type list and the buttons are visible. Highlight the Field Type list, then
  select "Text Field" with `ClickAsync(scene.Page.GetByLabel("Text Field", new() { Exact = true }))`. The first radio
  button of the list is selected when the page opens (`Views/Admin/AddFieldTo.cshtml:61`, `checked="@(i == 0)"`), so the
  click on "Text Field" is required. Highlight the "Save" button (`button[name='submit.Save'][value='Save']`). Visible:
  "Text Field" selected and the Save button ringed. Do not click Save. The field type labels are the field class names
  with spaces added (`Views/Admin/AddFieldTo.cshtml:62`): "Text Field", "Numeric Field", "Date Time Field".
- `fields-3`: `ClearHighlightsAsync`, click "Save" with `ClickAndWaitForNavigationAsync`. The page returns to the type
  (`Controllers/AdminController.cs:780-783`, the `returnUrl`) with the notification `The field "Venue" has been added.`
  (`:768`). Scroll so that the Fields list and the Parts list are both in view: Venue is listed under Fields, with the
  hint "Text Field", and the Parts list now holds Title and Event. Highlight the Event row
  (`ul#parts li` with the text "Contains the fields for the current type"). Visible: the Fields list with Venue, the
  Parts list with Title and Event, and the Event row ringed, with its hint "Contains the fields for the current type"
  and no buttons. Labels: `Views/Admin/Edit.cshtml:88` ("Fields"), `:144` ("Parts"), `:180` (the hint). The part appears
  here because the first field stores its definition (`OPEN-QUESTIONS.md`).
- `fields-4`: `ClearHighlightsAsync`; scroll to the Fields list. Click "Add Field"
  (`ClickAndWaitForNavigationAsync(scene.Page.Locator("a", new() { HasText = "Add Field" }))`), type `Starts` into
  `#DisplayName` with `delay: 40` and press Tab, scroll to the Field Type list, select "Date Time Field"
  (`ClickAsync(scene.Page.GetByLabel("Date Time Field", new() { Exact = true }), pauseAfter: 300)`), and highlight the
  button "Save & Configure" (`button[value='SaveAndConfigure']`). Visible: "Date Time Field" selected and
  "Save & Configure" ringed. Label: `Views/Admin/AddFieldTo.cshtml:72`. The actions are many for the narration, which is
  why the typing uses `delay: 40` and the clicks `pauseAfter: 300`.
- `fields-5`: `ClearHighlightsAsync`, click "Save & Configure" with `ClickAndWaitForNavigationAsync`
  (`Controllers/AdminController.cs:770-778`). The page is titled `"Starts" settings` (`Views/Admin/EditField.cshtml:30`).
  Highlight the Technical Name field (read-only, `Views/Admin/EditField.cshtml:56`, `input#Name[readonly]`), tick "Required"
  (`ClickAsync(scene.Page.GetByLabel("Required", new() { Exact = true }))`), and type `Local time at the venue.` into the
  "Hint" text area (`TypeAsync(scene.Page.GetByLabel("Hint", new() { Exact = true }), "Local time at the venue.", delay: 40)`).
  The labels are matched exactly because other enabled features can add sections to this page. Visible: the Technical Name ringed
  earlier, "Required" ticked, and the Hint filled; the "Save" button in view at the bottom. Labels:
  `OrchardCore.ContentFields/Views/DateTimeFieldSettings.Edit.cshtml:7` ("Required"), `:14` ("Hint"). Do not open the
  drop-down lists of editors and display modes. Do not scroll further than the Hint: other settings sections below it
  depend on the enabled features (see `OPEN-QUESTIONS.md`).
- `fields-6`: `ClearHighlightsAsync`, click "Save" (`button[name='submit.Save'][value='Save']`) with
  `ClickAndWaitForNavigationAsync`; the page returns to the type (`Controllers/AdminController.cs:911-914`), with the
  notification `The "Starts" field settings have been saved.` (`:908`). Scroll to the Fields list: Venue and Starts. Click
  "Add Field" with `ClickAndWaitForNavigationAsync(scene.Page.Locator("a", new() { HasText = "Add Field" }))`, type
  `Capacity` into `#DisplayName` with `delay: 40`, press Tab, select "Numeric Field"
  (`ClickAsync(scene.Page.GetByLabel("Numeric Field", new() { Exact = true }), pauseAfter: 300)`), and click "Save"
  (`button[name='submit.Save'][value='Save']`) with `ClickAndWaitForNavigationAsync`. Visible: the type page with the
  Fields list Venue, Starts, Capacity (each with the hint of its field type), and the notification for Capacity. This
  step holds the most actions; if they still run past the end of the narration, the next step simply starts later.
- `fields-7`: `ClearHighlightsAsync`; scroll so the whole Fields list is visible. Highlight the "Edit" and "Remove" buttons
  of the Venue row (`ul#fields li` with the text "Venue", then the `a` elements "Edit" and "Remove"). Do not click. Visible:
  the Fields list with the Venue row's buttons ringed. Labels: `Views/Admin/Edit.cshtml:100` ("Edit"), `:112` ("Remove").
  Edit opens the settings of the field (`Controllers/AdminController.cs:790-826`); Remove deletes the field from its part
  (`:928-960`).

---

## Clip `05-order`

Starts on `/Admin/ContentTypes/Edit/Event` (snapshot `snap-4`). Before `StartCaptureAsync`, scroll so the whole Fields list
and the Parts list are in view. Chapter 05.

- `order-1`: drag the Starts row above the Venue row:
  `DragToAsync(scene.Page.Locator("ul#fields li").Filter(new() { HasText = "Starts" }), scene.Page.Locator("ul#fields li").Filter(new() { HasText = "Venue" }), 0, -10)`
  (drop on the upper edge of the Venue row). The lists are sortable with Sortable
  (`Assets/js/content-type-sortable.ts:4, 10`; rows are `li` elements of `ul#fields` and `ul#parts`). Visible: the Fields
  list reads Starts, Venue, Capacity. If the order did not change, drag again before the step ends; do not use
  `scene.Page.DragAndDropAsync`. The Parts list (Title, Event) is not dragged.
- `order-2`: click "Save" (`button[name='submit.Save'][value='Save']`, the main button of the split button group in the
  bottom bar) with `ClickAndWaitForNavigationAsync`. Do not use the drop-down item "and continue". The page goes to the
  Content Types list (`Controllers/AdminController.cs:228-230`) with the notification "Content type updated successfully."
  (`:225`). Visible: the Content Types list with the Event row, and the notification. Label: `Views/Admin/Edit.cshtml:215`.

---

## Clip `06-item`

Starts on `/Admin/ContentTypes/List` (snapshot `snap-5`). The Event type is saved with the order stored. Chapter 06.

- `item-1`: move to the Event row and highlight it (`.list-group-item` with the text "Event" and the button `#btn-create-Event`);
  then highlight the "Create New Event" button. Do not click yet. Visible: the Event row, with the technical-name badge
  `Event`, and its "Create New Event" button ringed. Label: `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:19`
  (shown because the type is Creatable).
- `item-2`: `ClearHighlightsAsync`, click `#btn-create-Event` with `ClickAndWaitForNavigationAsync`. The page is titled
  "New Event" (`OrchardCore.Contents/Views/Admin/Create.cshtml:26`). Highlight the Starts label (the asterisk) and the hint
  under its input. Visible: the editor with Title, then Starts (with an asterisk, and the hint "Local time at the venue."
  under it), Venue and Capacity. Labels: `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12` ("Title"),
  `OrchardCore.ContentFields/Views/DateTimeField.Edit.cshtml:11, 22`, `TextField.Edit.cshtml:20`,
  `NumericField.Edit.cshtml:14`. The red asterisk is drawn by `OrchardCore.Themes/TheAdmin/Assets/scss/components/_ocat.scss:27-34`.
- `item-3`: `ClearHighlightsAsync`. Type `Community Meetup` into the Title input
  (`TypeAsync(scene.Page.GetByLabel("Title", new() { Exact = true }), "Community Meetup", delay: 40)`). Set the Starts
  input with `var starts = scene.Page.Locator("input[type='datetime-local']"); await scene.MoveToAsync(starts); await
  starts.FillAsync("2031-03-14T18:30");` (the scene has no method for this input, so the drawn cursor is moved to it first
  and the value is set in one go; the field then reads 03/14/2031, 06:30 PM). Type `Riverside Hall` into the Venue input
  (`GetByLabel("Venue", new() { Exact = true })`) and `120` into the Capacity input
  (`GetByLabel("Capacity", new() { Exact = true })`), both with `delay: 40`. Highlight the "Publish" button (`button.publish`). Do not click
  yet. Visible: the filled form, with Save Draft and Publish at the bottom, Publish ringed. Labels:
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7` ("Publish"), `Content.SaveDraftButton.cshtml:7` ("Save Draft").
- `item-4`: `ClearHighlightsAsync`, click `button.publish` with `ClickAndWaitForNavigationAsync`. The page returns to the
  Content Types list (`OrchardCore.Contents/Controllers/AdminController.cs:795-798`) with the notification "Your Event has
  been published." (`:473`). Hold for two seconds, then click "List Items" on the Event row (`[role='btn-list-Event']`)
  with `ClickAndWaitForNavigationAsync`. Visible: the content list filtered on Event, showing the item "Community Meetup".
  Label: `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:22`.

---

## Checks for private data and code on screen

- The only account is `admin` / `admin@example.com`; the user menu may show `admin`. No real name, email, phone number,
  address or token is typed or seeded. The password is never typed on camera (the session comes from `state.json`).
- The site is fresh; no mask script is needed. Still OCR the video at one frame per second and at every scene change
  (`select='gt(scene,0.02)'`), then at 5 frames per second, and search for anything that is not in the table of fake values.
- The Date Time Field settings page (`fields-5`) and the item editor (`item-2`, `item-3`) must show no Liquid, HTML or
  JSON. The Title part's settings page is not opened. If a settings section with a code-like editor is visible at the
  end of a step, scroll it out of the frame before the step starts, or record the step again.
- No error message, empty list or validation message is visible at the end of any step.
- The password for the recording never appears in a shell history that is shared, in this file, or in a commit.
