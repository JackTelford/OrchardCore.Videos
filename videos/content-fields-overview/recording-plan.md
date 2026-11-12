# Recording plan: Content Fields: Text, Numbers, Dates and Yes or No

For the person who records the video. Read this file and `storyboard.json`; nothing else is needed. In this plan a
selector or a call such as `GetByLabel(...)` stands for `scene.Page.GetByLabel(...)` or `scene.Page.Locator(...)`, since
the recorder's methods take locators. Source references (`file:line`) are in the Orchard Core checkout at commit
92db4a83, under `src/OrchardCore.Modules/` unless a path starts with `OrchardCore.Themes/`.

The video shows the admin UI and the front end of one site. It never shows code. It follows one example from start to
finish: a content type named **Workshop** gets six fields, then one workshop is created and published.

## The site it needs

| What | Value |
| --- | --- |
| Orchard Core | `src/OrchardCore.Cms.Web` of a checkout at or after commit 92db4a83, on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup through environment variables (see the `orchardcore-tester` skill of that checkout, `references/autosetup.md`), so no setup form and no sign-in form is recorded |
| Setup values | Tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, recipe `Blog`, database `Sqlite`, user `admin`, email `admin@example.com`, password from `VIDEO_PASSWORD` (written as `<password>` in every file of the repository, chosen outside git) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (for example `http://127.0.0.1:9`) so the site never calls an outside service |
| Features | None to enable. The Blog recipe enables Content Fields, Content Types, Contents and Title (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:36-80`). **Keep the "User Time Zone" feature disabled**: when it is on, the time zone of the signed-in user replaces the time zone of the site (`OrchardCore.Users/TimeZone/Services/UserTimeZoneSelector.cs:22-36`), and the Date Time Field chapters would not match the narration |
| External services | None |
| Sign-in | `Recorder setup` once, outside the capture, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`. It writes `state.json` (git-ignored) |
| Browser | The recorder's Chromium (1536x864, English, light), see `references/specs.md` |

### Seed data, created outside the capture

All names are invented. Nothing else is created before the first clip.

1. Sign in as `admin` in the recorder's browser, or in any browser.
2. Open `/Admin/ContentTypes/List`. Click **Create new type** (`OrchardCore.ContentTypes/Views/Admin/List.cshtml:20`).
3. In **Display Name** (`Views/Admin/Create.cshtml:14`) type `Workshop`. **Technical Name** (`Create.cshtml:22`) fills in as
   `Workshop`; do not edit it. Click **Create** (`Create.cshtml:30`).
4. The **Add Parts** page opens (`Controllers/AdminController.cs:164`, it redirects to `AddPartsTo`). Tick the check box
   labeled **Title** (`Views/Admin/AddPartsTo.cshtml:33`; the label is the display name of the part, `TitlePart` without
   "Part", `OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-61`). A button **Add Selected Parts**
   (`AddPartsTo.cshtml:47`) appears; click it. The page **Edit Content Type - Workshop** opens.
5. Add nothing else: no fields, no other parts, no content items.
6. Check, and fix the site if one check fails:
   - `/Admin/Features`: **Content Fields** shows **Enabled**; **User Time Zone** shows **Disabled**.
   - `/Admin/ContentTypes/Edit/Workshop`: the **Parts** list holds **Title** and **Workshop**; the **Fields** section is empty and the **Add Field** button is there.

Stop the site and copy its data folder aside as snapshot **S0** (see "Order" below).

### The Workshop example

Used by clips 03 to 08. The technical names are made from the display names by the page; never type in a **Technical Name** box.

| Display Name | Technical Name | Field type | Settings saved on camera |
| --- | --- | --- | --- |
| Speaker | Speaker | Text Field | Required on; Hint `The person who leads the workshop.` |
| Seats | Seats | Numeric Field | Required on; Minimum `1`; Maximum `40`; Default value `20`; Scale stays `0` |
| Registration Open | RegistrationOpen | Boolean Field | Label `Participants can register`; Default value is true on |
| Registration Deadline | RegistrationDeadline | Date Field | none |
| Starts At | StartsAt | Date Time Field | none |
| Doors Open | DoorsOpen | Time Field | none (Step stays empty) |

The content item of clip 07: Title `Planning a Content Model`; Speaker `Jamie Chen`; Seats first `50`, then `25`;
Registration Open stays checked; Registration Deadline `03/12/2030`; Starts At `03/18/2030 02:00 PM`; Doors Open `01:30 PM`.

## Order, dependencies and what changes the site

Record in this order: `02-open`, `03-text`, `04-numeric`, `05-boolean`, `06-dates`, `07-edit`, `08-view`. Each clip starts from the
state the previous one left. After each clip, stop the site and copy the data folder aside as the snapshot named in the table.
To record one clip again, restore the snapshot in the "Needs" column, start the site, and record only that clip.

| Clip | Needs (snapshot) | Changes the site | Leaves (snapshot) |
| --- | --- | --- | --- |
| `02-open` | S0 | no | S0 |
| `03-text` | S0 | adds the field Speaker | S3 |
| `04-numeric` | S3 | adds the field Seats | S4 |
| `05-boolean` | S4 | adds the field Registration Open | S5 |
| `06-dates` | S5 | adds Registration Deadline, Starts At, Doors Open | S6 |
| `07-edit` | S6 | publishes one Workshop | S7 |
| `08-view` | S7 | no | S7 |

Also true for every clip:

- Narrate first (`narrate.py`), then record: a step holds until its narration ends. A step's actions must be shorter than its
  narration. Rough costs: a click 1.2 s, a click that loads a page 2 s, typing 0.9 s plus 55 ms per character.
- Everything before `StartCaptureAsync` is not recorded. Open the page there.
- Clear highlights (`ClearHighlightsAsync`) before scrolling, and at the start of the next step.
- Before every click, every typing and every ring on an element that may be outside the window (the Field Type labels, the buttons at the end of a long page, **View**, the settings boxes), call `BringIntoViewAsync` on it. `ClickAsync` does not scroll.
- Never type in a **Technical Name** box. A script fills it from the **Display Name** as you type, and stops as soon as someone
  types in the box (`OrchardCore.ContentTypes/wwwroot/Scripts/autocomplete/autocomplete.js`).
- After every page that loads, look at the notice at the top: `The field "X" has been added.` and `The "X" field settings have
  been saved.` are expected (`Controllers/AdminController.cs:768` and `:908`). Any other message means the take is wrong.

## Helpers for `Scenes.cs`

These small methods are used by several clips. They are given once here.

```csharp
using System.Text.RegularExpressions;

// The row of the admin menu with this label (a group, or a page).
static ILocator MenuCaption(Scene scene, string label)
    => scene.Page.Locator("#left-nav figcaption.nav-item-row")
        .Filter(new() { HasTextRegex = new Regex($@"^\s*{Regex.Escape(label)}\s*$") }).First;

// Opens a closed group of the admin menu without following its link. The label, or the arrow beside it, carries
// aria-expanded. Clicking an open group would close it, so an open one is left alone.
static async Task OpenGroupAsync(Scene scene, string label, bool onCamera)
{
    var toggle = MenuCaption(scene, label).Locator("[aria-expanded]").First;
    if (await toggle.GetAttributeAsync("aria-expanded") != "false") return;
    if (onCamera) await scene.ClickAsync(toggle);
    else await toggle.ClickAsync();
}

// Follows a page of the admin menu.
static Task FollowMenuAsync(Scene scene, string label)
    => scene.ClickAndWaitForNavigationAsync(MenuCaption(scene, label).Locator("a.item-label").First);

// Enters a value in a date, date-time or time input by keystrokes, starting on its first segment.
// If the input does not show the expected value afterwards, the value is set directly.
static async Task EnterAsync(Scene scene, ILocator input, string expected, params string[] keys)
{
    var box = await input.BoundingBoxAsync();
    await scene.ClickAsync(input, offsetX: 22 - box!.Width / 2, pauseAfter: 200);
    foreach (var key in keys)
    {
        if (key.StartsWith("Arrow")) await scene.Page.Keyboard.PressAsync(key);
        else await scene.Page.Keyboard.TypeAsync(key, new() { Delay = 90 });
    }
    if (await input.InputValueAsync() != expected) await input.FillAsync(expected);
    await scene.PauseAsync(300);
}

// Scrolls so that the top of an element is `top` pixels below the top of the window.
static async Task ScrollToAsync(Scene scene, ILocator element, double top)
{
    var box = await element.BoundingBoxAsync();
    await scene.ScrollAsync(0, box!.Y - top);
}

// ClickAsync and TypeAsync act at the center of the element without scrolling to it, and HighlightAsync scrolls only
// when the element is not visible. Call this first on any element that may be outside the window.
static async Task BringIntoViewAsync(Scene scene, ILocator element)
{
    var box = await element.BoundingBoxAsync();
    if (box!.Y < 100 || box.Y + box.Height > 760) await ScrollToAsync(scene, element, 400);
}
```

The field-adding clips (03 to 06) repeat the same moves. Their names:

- **Add Field** = `ClickAndWaitForNavigationAsync(GetByRole(AriaRole.Button, new() { Name = "Add Field", Exact = true }))`. The element is a link that carries `role="button"` (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:137`), so `AriaRole.Link` finds nothing. The page **Add New Field To "Workshop"** opens (`Views/Admin/AddFieldTo.cshtml:20`).
- **Name it X** = `TypeAsync(GetByLabel("Display Name", new() { Exact = true }), X)`. **Technical Name** (`AddFieldTo.cshtml:46`) fills in by itself.
- **Choose type T** = `BringIntoViewAsync` on the label, then `ClickAsync(GetByText(T, new() { Exact = true }))` on the label in the **Field Type** list (`AddFieldTo.cshtml:54-66`), then check that
  `GetByLabel(T, new() { Exact = true }).IsCheckedAsync()` is true. The first entry of the list is checked when the page opens
  (`AddFieldTo.cshtml:61`, `checked="@(i == 0)"`), so the click is needed for every field type except when it is already the checked one. Click it anyway, so the cursor shows the choice. The labels are the class names with spaces:
  `Text Field`, `Numeric Field`, `Boolean Field`, `Date Field`, `Date Time Field`, `Time Field`.
- **Save & Configure** = `ClickAndWaitForNavigationAsync(GetByRole(AriaRole.Button, new() { Name = "Save & Configure", Exact = true }))` (`AddFieldTo.cshtml:72`).
  The settings page `"X" settings` opens (`Views/Admin/EditField.cshtml:30`).
- **Save** on the settings page = `ClickAndWaitForNavigationAsync(GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }))` (`EditField.cshtml:120`). It returns to **Edit Content Type - Workshop** (the `returnUrl` of the Add Field link, `Controllers/AdminController.cs:911`).

## Clips

### Slides and cards

Drawn by `slides.py` and the cards of `assemble.py`; nothing to record.

- `intro`: the opening card, "Orchard Core Content Fields", subtitle "Text, numbers, dates and yes or no".
- `why-1`: slide "From content types to fields": the cards Content type and Part are bright, Field is dimmed.
- `why-2`: the same slide, the card Field is bright.
- `why-3`: slide "Fields for everyday data": the cards Text Field and Numeric Field are bright.
- `why-4`: the same slide, the cards Boolean Field, and Date Field, Date Time Field and Time Field are bright.
- `outro`: the closing card, "Orchard Core Content Fields", subtitle "docs.orchardcore.net".

### Clip `02-open`: find the feature, open the type

Needs S0. Starts on `/Admin/Features` (`OpenAsync`). Before `StartCaptureAsync` call `OpenGroupAsync(scene, "Tools", false)`, so the admin menu shows **Tools** open with **Features** selected. The clip ends on **Edit Content Type - Workshop**. No change to the site.

Local helper: `ILocator Row(string id) => scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator($"#btn-enable-{id}, #btn-disable-{id}") }).First;`
(the example in `scripts/recorder/Scenes.cs`; here `id` is `OrchardCore_ContentFields`).

#### `open-1`

- Actions: `TypeAsync(Locator("#search-box"), "Content Fields", delay: 90)`; wait until `Row("OrchardCore_ContentFields")` is visible; `HighlightAsync(Row("OrchardCore_ContentFields"), "Enabled")`.
- On screen at the end of the narration: the Features page filtered on "Content Fields", the row **Content Fields** with the badge **Enabled** and a red **Disable** button, ringed in green. Rows of the indexing features, if listed, show **Disabled**.
- Labels from: `OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features); `Views/Admin/Features.cshtml:27` (search box); `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` (Enabled); `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (the Enable button the narration mentions, which is not on screen because the feature is on).

#### `open-2`

- Actions: `ClearHighlightsAsync()`. `OpenGroupAsync(scene, "Design", true)`, then `OpenGroupAsync(scene, "Content Definition", true)` (each opens only when closed), then `FollowMenuAsync(scene, "Content Types")`, which loads **Content Types**. Then `TypeAsync(Locator("#search-box"), "Workshop", delay: 90)`; the list narrows to the row **Workshop**; `HighlightAsync(Locator("a.edit-button[href$='/ContentTypes/Edit/Workshop']"), "Edit")`. Always include `.edit-button`: the name of the type in the row is a plain link to the same address (`Views/ContentTypeEntry.Fields.SummaryAdmin.cshtml`), so `a[href$='/ContentTypes/Edit/Workshop']` alone matches two elements and the click fails. If the row's buttons sit in a menu instead, open that menu first.
- On screen at the end of the narration: the Content Types page with the row **Workshop** and its **Edit** button ringed; the admin menu shows Design, Content Definition, Content Types opened.
- Labels from: `OrchardCore.ContentTypes/AdminMenu.cs:23-25` (Design, Content Definition, Content Types); `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24` (Edit); `Views/Admin/List.cshtml:15` (search box).

#### `open-3`

- Actions: `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync(Locator("a.edit-button[href$='/ContentTypes/Edit/Workshop']"))`; `ScrollToAsync(scene, GetByRole(AriaRole.Heading, new() { Name = "Fields", Exact = true }), 120)`; `HighlightAsync` on the hint text **Contains the fields for the current type** next to the part **Workshop** (`GetByText("Contains the fields for the current type")`), hold 3 s; clear; `HighlightAsync(GetByRole(AriaRole.Button, new() { Name = "Add Field", Exact = true }))`.
- On screen at the end of the narration: **Edit Content Type - Workshop**, the **Fields** heading with no field under it, the **Add Field** button ringed, and, below, the **Parts** list with **Title** and **Workshop**.
- Labels from: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63` (page title), `:88` (Fields), `:137` (Add Field), `:144` (Parts), `:180` (Contains the fields for the current type).

### Clip `03-text`: Speaker, a Text Field

Needs S0 (after `02-open` the site is unchanged). Starts on `/Admin/ContentTypes/Edit/Workshop` (`OpenAsync`), scrolled with
`ScrollToAsync(scene, GetByRole(AriaRole.Heading, new() { Name = "Fields", Exact = true }), 120)` before `StartCaptureAsync`, so **Add Field** is in view. Ends on the same page with Speaker listed. Leaves S3.

#### `text-1`

- Actions: **Add Field**. `HighlightAsync` on the **Display Name** input, hold 1 s, clear; on the **Technical Name** input, hold 1 s, clear; on the **Field Type** list (`Locator("form[action*='AddFieldsTo'] ul.list-group")`), hold 1 s, clear. **Name it** `Speaker`. `HighlightAsync` on the **Technical Name** input (it now shows `Speaker`).
- On screen at the end of the narration: the page **Add New Field To "Workshop"** with **Display Name** `Speaker`, **Technical Name** `Speaker` ringed, and the **Field Type** list below.
- Labels from: `AddFieldTo.cshtml:38` (Display Name), `:46` (Technical Name), `:54` (Field Type).

#### `text-2`

- Actions: `HighlightAsync` on the **Field Type** list, hold 2 s, clear. **Choose type** `Text Field`. **Save & Configure**.
- On screen at the end of the narration: the page `"Speaker" settings`, with the notice `The field "Speaker" has been added.` and the settings of the Text Field starting at **Required**.
- Labels from: `AddFieldTo.cshtml:72` (Save & Configure), `EditField.cshtml:30` (page title); the notice `Controllers/AdminController.cs:768`.

#### `text-3`

- Actions, one after another, each ring held 0.8 s and cleared: `GetByLabel("Required", new() { Exact = true })`, `GetByLabel("Hint", new() { Exact = true })`, `GetByLabel("Placeholder", new() { Exact = true })`, `GetByLabel("Default value", new() { Exact = true })`, `GetByLabel("Minimum length", new() { Exact = true })`, `GetByLabel("Maximum length", new() { Exact = true })`. Then `ScrollToAsync(scene, GetByLabel("What type of editor should be used?", new() { Exact = true }), 300)`; ring `GetByLabel("What type of editor should be used?", new() { Exact = true })` for 1.5 s, clear, ring `GetByLabel("What type of display mode should be used?", new() { Exact = true })` for 1.5 s, clear. Do not open either list, do not change the **Type** list, and never scroll to or open the **Pattern** box (it stays hidden for the type Editable).
- On screen at the end of the narration: the two lists **What type of editor should be used?** and **What type of display mode should be used?**, both showing **Standard**, the second one ringed.
- Labels from: `OrchardCore.ContentFields/Views/TextFieldSettings.Edit.cshtml:19` (Required), `:26` (Hint), `:34` (Placeholder), `:42` (Default value), `:50` (Minimum length), `:58` (Maximum length); `OrchardCore.ContentFields/Views/TextField.Option.cshtml:4` and `TextField.DisplayOption.cshtml:4` (Standard); `OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:79` and `:100`.

#### `text-4`

- Actions: `ScrollAsync(0, -3000)` to the top; `ClickAsync(GetByLabel("Required", new() { Exact = true }))` (the box is checked); `TypeAsync(GetByLabel("Hint", new() { Exact = true }), "The person who leads the workshop.")`; **Save**.
- On screen at the end of the narration: **Edit Content Type - Workshop** with the notice `The "Speaker" field settings have been saved.` and the list under **Fields**: `Speaker` with the dashed hint `Text Field`, and the buttons **Edit** and **Remove**. Scroll to the **Fields** heading (`ScrollToAsync(..., 120)`) if the list is out of view.
- Labels from: `TextFieldSettings.Edit.cshtml:19,26`; `EditField.cshtml:120`; `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:95,115` (the field row: display name, then the field type in `CamelFriendly` form).

### Clip `04-numeric`: Seats, a Numeric Field

Needs S3. Starts like `03-text` (Edit page of Workshop, **Add Field** in view). Leaves S4.

#### `num-1`

- Actions: **Add Field**; **Name it** `Seats`; **Choose type** `Numeric Field`; **Save & Configure**.
- On screen at the end of the narration: the page `"Seats" settings` with the notice `The field "Seats" has been added.` and the Numeric Field settings starting at **Required**.
- Labels from: `AddFieldTo.cshtml:38,54,72`.

#### `num-2`

- Actions, each ring held 1 s and cleared, in this order: `GetByLabel("Scale", new() { Exact = true })` (it shows `0`), `GetByLabel("Minimum", new() { Exact = true })`, `GetByLabel("Maximum", new() { Exact = true })`, `GetByLabel("Default value", new() { Exact = true })`. Scroll down with `ScrollToAsync` on the **Scale** input (`top: 200`) first if the four boxes do not fit in the window.
- On screen at the end of the narration: the four boxes **Scale**, **Minimum**, **Maximum**, **Default value** in view, the last one ringed.
- Labels from: `OrchardCore.ContentFields/Views/NumericFieldSettings.Edit.cshtml:38` (Scale, hint "The number of digits after the decimal point."), `:46` (Minimum), `:54` (Maximum), `:62` (Default value). The placeholder box on this page is labeled **Watermark (placeholder)** (`:30`); the narration does not mention it.

#### `num-3`

- Actions: `ScrollAsync(0, -3000)`; `ClickAsync(GetByLabel("Required", new() { Exact = true }))`; `TypeAsync(GetByLabel("Minimum", new() { Exact = true }), "1")`; `TypeAsync(GetByLabel("Maximum", new() { Exact = true }), "40")`; `TypeAsync(GetByLabel("Default value", new() { Exact = true }), "20")`; press Tab (these boxes change on blur); **Save**.
- On screen at the end of the narration: **Edit Content Type - Workshop** with the notice `The "Seats" field settings have been saved.` and the list under **Fields**: `Speaker` (`Text Field`) and `Seats` (`Numeric Field`).
- Labels from: `NumericFieldSettings.Edit.cshtml:15,46,54,62`; `EditField.cshtml:120`.

### Clip `05-boolean`: Registration Open, a Boolean Field

Needs S4. Starts like `03-text`. Leaves S5.

#### `bool-1`

- Actions: **Add Field**; **Name it** `Registration Open`; **Choose type** `Boolean Field`; **Save & Configure**.
- On screen at the end of the narration: the page `"Registration Open" settings` with the notice `The field "Registration Open" has been added.` and the Boolean Field settings: **Hint**, **Label**, **Default value is true**. There is no **Required** box.
- Labels from: `OrchardCore.ContentFields/Views/BooleanFieldSettings.Edit.cshtml:4` (Hint), `:12` (Label), `:23` (Default value is true).

#### `bool-2`

- Actions, in the order of the narration: `HighlightAsync(GetByLabel("Label", new() { Exact = true }))` and `HoldAsync(2.5)` (first sentence); `ClearHighlightsAsync()`, `HighlightAsync(GetByLabel("Default value is true", new() { Exact = true }))` and `HoldAsync(4.5)` (second sentence); `ClearHighlightsAsync()`; `TypeAsync(GetByLabel("Label", new() { Exact = true }), "Participants can register")`; `ClickAsync(GetByLabel("Default value is true", new() { Exact = true }))` (the box is checked); **Save** (third sentence).
- On screen at the end of the narration: **Edit Content Type - Workshop** with the notice `The "Registration Open" field settings have been saved.` and, under **Fields**, `Speaker`, `Seats` and `Registration Open` (`Boolean Field`).
- Labels from: `BooleanFieldSettings.Edit.cshtml:12,15,23-24`.

### Clip `06-dates`: Registration Deadline, Starts At, Doors Open

Needs S5. Starts like `03-text`. Leaves S6.

#### `dates-1`

- Actions: **Add Field**; **Name it** `Registration Deadline`; **Choose type** `Date Field`; **Save & Configure**; ring `GetByLabel("Required", new() { Exact = true })` for 0.7 s, clear, ring `GetByLabel("Hint", new() { Exact = true })` for 0.7 s, clear; ring the two lists `GetByLabel("What type of editor should be used?", new() { Exact = true })` and `GetByLabel("What type of display mode should be used?", new() { Exact = true })` for 0.7 s each, clear (the narration says "Beyond the two lists"); leave both lists as they are; **Save**.
- On screen at the end of the narration: **Edit Content Type - Workshop** with the notice `The "Registration Deadline" field settings have been saved.` and a fourth entry `Registration Deadline` (`Date Field`) under **Fields**.
- Labels from: `OrchardCore.ContentFields/Views/DateFieldSettings.Edit.cshtml:7,14`; the two lists from `OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:79,100` (the editor list offers **Standard** and **Localized**, `DateField.Option.cshtml`, `DateField-Localized.Option.cshtml`; the display mode list offers **Standard**, `DateField.DisplayOption.cshtml`).

#### `dates-2`

- Actions: **Add Field**; **Name it** `Starts At`; **Choose type** `Date Time Field`; **Save & Configure**; hold the settings page 1 s; **Save**.
- On screen at the end of the narration: **Edit Content Type - Workshop** with the notice `The "Starts At" field settings have been saved.` and a fifth entry `Starts At` (`Date Time Field`).
- Labels from: `OrchardCore.ContentFields/Views/DateTimeFieldSettings.Edit.cshtml:7,14`; the field type label from `AddFieldTo.cshtml:62` (`Date Time Field` is `DateTimeField` spaced by `CamelFriendly`, `OrchardCore.Mvc.Core/Utilities/StringExtensions.cs:15-32`).

#### `dates-3`

- Actions: **Add Field**; **Name it** `Doors Open`; **Choose type** `Time Field`; **Save & Configure**; `HighlightAsync(GetByLabel("Step", new() { Exact = true }))` held 1.5 s, clear; leave **Step** empty; **Save**.
- On screen at the end of the narration: **Edit Content Type - Workshop** with the notice `The "Doors Open" field settings have been saved.` and a sixth entry `Doors Open` (`Time Field`).
- Labels from: `OrchardCore.ContentFields/Views/TimeFieldSettings.Edit.cshtml:14` (Step, hint "The step attribute of the time input allowing to edit seconds and ms.").

#### `dates-4`

- Actions: `ScrollToAsync(scene, GetByRole(AriaRole.Heading, new() { Name = "Fields", Exact = true }), 100)`; `HighlightAsync(Locator("ul#fields"))`, hold until the narration ends.
- On screen at the end of the narration: the six entries in this order, ringed: `Speaker` (`Text Field`), `Seats` (`Numeric Field`), `Registration Open` (`Boolean Field`), `Registration Deadline` (`Date Field`), `Starts At` (`Date Time Field`), `Doors Open` (`Time Field`). The order is the order they were added: `AddFieldToPartAsync` gives each field the next position (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:292`, `:494-519`) and the type editor sorts by it (`Views/Admin/Edit.cshtml:48-55`).
- Labels from: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:88,115`.

### Clip `07-edit`: fill in a workshop

Needs S6. Starts on `/Admin/Contents/ContentItems` (`OpenAsync`). Before `StartCaptureAsync` call `OpenGroupAsync(scene, "Content", false)`, so the admin menu shows **Content** open with **Content Items** selected. Ends on the Content Items list. Leaves S7.

#### `edit-1`

- Actions: `ClickAsync(Locator("#new-dropdown"))` (the **New** button, which opens a menu); `HoldAsync(3.2)` so the open menu is on screen while the first sentence ends; `ClickAndWaitForNavigationAsync(Locator("a.dropdown-item").Filter(new() { HasTextRegex = new Regex("^\\s*Workshop\\s*$") }))`.
- On screen at the end of the narration: the page **New Workshop** with the editor open. (The narration says "Under Content, Content Items" while the menu is already showing that path; the menu opens at the start, and the click on **Workshop** happens during the second sentence.)
- Labels from: `OrchardCore.Contents/AdminMenu.cs:65,68` (Content, Content Items); `Views/ContentsAdminListCreate.cshtml:20` (New); `Views/Admin/Create.cshtml:26` (New {0}).

#### `edit-2`

- Actions: if the **Doors Open** input is below the window, `ScrollToAsync(scene, Locator(".field-wrapper-workshop-doors-open"), 700)` once, so that the six fields are in view and nothing else moves. Then, in the order of the narration: `HoldAsync(8.3)` with no ring (first sentence: the whole editor is in view); then `HighlightAsync(Locator(".field-wrapper-workshop-speaker"))` and `HighlightAsync(Locator(".field-wrapper-workshop-seats"))`, both rings held until the narration ends (second sentence: the two asterisks and the hint).
- On screen at the end of the narration: the editor with the fields `Speaker`, `Seats`, the checkbox `Participants can register` (the Boolean Field shows its **Label**, not its Display Name, `OrchardCore.ContentFields/Views/BooleanField.Edit.cshtml:16`), `Registration Deadline`, `Starts At`, `Doors Open` in that order; `Speaker` and `Seats` carry a red asterisk (both fields are required) and the hint `The person who leads the workshop.` is under the `Speaker` box; both rows are ringed.
- Labels from: `OrchardCore.ContentFields/Views/TextField.Edit.cshtml:20,26` and `OrchardCore.Themes/TheAdmin/Assets/scss/components/_ocat.scss:27-34` (the asterisk); the wrapper class from `TextField.Edit.cshtml:19` (`field-wrapper-<part>-<field>`, lowercase, words separated by hyphens).

#### `edit-3`

- Actions: `ClearHighlightsAsync()`; `HighlightAsync(Locator(".field-wrapper-workshop-seats"))` held 4 s; clear; `HighlightAsync(Locator(".field-wrapper-workshop-registration-open"))`.
- On screen at the end of the narration: `Seats` showing `20` between a `1` and a `40` (the minimum and maximum, `OrchardCore.ContentFields/Views/NumericField.Edit.cshtml:21,26`); a checked box beside `Participants can register`, ringed (`BooleanField.Edit.cshtml:14-16`).
- Labels from: the two views above.

#### `edit-4`

- Actions: `ClearHighlightsAsync()`; `TypeAsync(GetByLabel("Title", new() { Exact = true }), "Planning a Content Model")`; `TypeAsync(GetByLabel("Speaker", new() { Exact = true }), "Jamie Chen")` (first sentence); then, for the second sentence, `ClickAsync(GetByLabel("Seats", new() { Exact = true }))`, `Keyboard.PressAsync("Control+A")` and `Keyboard.TypeAsync("50", new() { Delay = 90 })`.
- On screen at the end of the narration: `Title` = `Planning a Content Model`, `Speaker` = `Jamie Chen`, `Seats` = `50`.
- Labels from: `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12` (Title); `NumericFieldDisplayDriver.cs:94` (a value above the Maximum `40` is refused).

#### `edit-5`

- Actions: ring each of the three inputs `GetByLabel("Registration Deadline")`, `GetByLabel("Starts At")` and `GetByLabel("Doors Open")` for 0.8 s and clear (first sentence: the inputs the browser provides, all three still empty), then `HoldAsync(1.7)`. Then, for the second sentence, the helper `EnterAsync` clicks the first segment of the input and types:
  `EnterAsync(scene, GetByLabel("Registration Deadline"), "2030-03-12", "03122030")`;
  `EnterAsync(scene, GetByLabel("Starts At"), "2030-03-18T14:00", "03182030", "ArrowRight", "0200", "P")`;
  `EnterAsync(scene, GetByLabel("Doors Open"), "13:30", "0130", "P")`.
- On screen at the end of the narration: `Registration Deadline` = `03/12/2030`, `Starts At` = `03/18/2030, 02:00 PM`, `Doors Open` = `01:30 PM`, plus the three values of `edit-4`, with `Seats` = `50`.
- Labels from: `DateField.Edit.cshtml:16`, `DateTimeField.Edit.cshtml:15` and `TimeField.Edit.cshtml:16` (the three input types: `date`, `datetime-local`, `time`, drawn by the browser).

#### `edit-6`

- Actions: `ClickAndWaitForNavigationAsync(GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }))`. The editor loads again with the same values and an error. The message is printed twice, in the validation summary at the top (`OrchardCore.Contents/Views/Admin/Create.cshtml:32-34`) and under the **Seats** box, so a locator on the text alone matches two elements. Let `var message = Locator(".field-wrapper-workshop-seats").GetByText("The value must be less than 40.");` (the wrapper holds only the message under the box). Wait until `message` is visible, then `HighlightAsync(message)`.
- On screen at the end of the narration: the page still **New Workshop**, the URL still the create page, `Seats` = `50`, and the message `The value must be less than 40.` ringed. All the other values are still in their boxes; if one is empty, fill it again before recording `edit-7`.
- Labels from: `OrchardCore.ContentFields/Drivers/NumericFieldDisplayDriver.cs:94` (the message); `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19` (Publish; the page is opened from the Content Items list, so it has a `returnUrl` and shows the split button of lines 11-26, whose other button is named "Toggle Dropdown").

#### `edit-7`

- Actions: `ClearHighlightsAsync()`; `ClickAsync(GetByLabel("Seats", new() { Exact = true }))`, `Keyboard.PressAsync("Control+A")`, `Keyboard.TypeAsync("25", new() { Delay = 90 })`; `ClickAndWaitForNavigationAsync(GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }))`.
- On screen at the end of the narration: the Content Items list (**Manage Content**) with the notice `Your Workshop has been published.` and the row `Planning a Content Model` of type Workshop.
- Labels from: `OrchardCore.Contents/Controllers/AdminController.cs:473` (the notice), `OrchardCore.Contents/Views/Admin/List.cshtml:17` (Manage Content).

### Clip `08-view`: the values on the site

Needs S7. Starts on `/Admin/Contents/ContentItems` (`OpenAsync`). No change to the site. The clip ends on the public page of the item, in a second tab.

#### `view-1`

- Actions: `HighlightAsync(Locator(".list-group-item").Filter(new() { HasText = "Planning a Content Model" }).First)` for 2 s, clear. Click **View** on that row and follow the new tab:
  `var view = Locator(".list-group-item").Filter(new() { HasText = "Planning a Content Model" }).First.Locator("a.view");`
  `var tab = await scene.Page.Context.RunAndWaitForPageAsync(() => scene.ClickAsync(view));`
  `await scene.SwitchToAsync(tab);`
  Then, if the six values are not all in view, `ScrollToAsync(scene, Locator(".field-name-workshop-speaker"), 200)`.
- On screen at the end of the narration: the page of the item on the site (theme TheBlogTheme) with the six values, each alone on its line: `Jamie Chen`, `25`, `True`, a short date, a date and time, a time.
- Labels from: `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18` (View; it opens a new tab, `target="_blank"`); the item's public address is `/Contents/ContentItems/<id>` (`OrchardCore.Contents/Startup.cs:223`).

#### `view-2`

- Actions: `HighlightAsync(Locator(".field-name-workshop-speaker"))` for 2.5 s, clear; `HighlightAsync(Locator(".field-name-workshop-seats"))` for 2.5 s, clear; `HighlightAsync(Locator(".field-name-workshop-registration-open"))`.
- On screen at the end of the narration: the third value, `True`, ringed.
- Labels from: `OrchardCore.ContentFields/Views/TextField.cshtml:9`, `NumericField.cshtml:9`, `BooleanField.cshtml:9` (each prints the value alone, in a `div` with the classes `field field-type-… field-name-<part>-<field>`).

#### `view-3`

- Actions: `ClearHighlightsAsync()`; `HighlightAsync(Locator(".field-name-workshop-registration-deadline"))` for 2.5 s, clear; `HighlightAsync(Locator(".field-name-workshop-starts-at"))` for 3 s, clear; `HighlightAsync(Locator(".field-name-workshop-doors-open"))`.
- On screen at the end of the narration: the last value, the time, ringed. Expected values: a date such as `3/12/2030`; a date and time such as `3/18/2030 2:00:00 PM`; a time such as `13:30:00` (formats come from the culture of the page, see `OPEN-QUESTIONS.md`).
- Labels from: `DateField.cshtml:11` (short date), `DateTimeField.cshtml:9` (local date and time), `TimeField.cshtml:9`.

## Checks for private data

Before the clips are assembled:

- The site is fresh: only the Blog recipe content, the Workshop type, and one Workshop. There is no other user than `admin`.
- Everything typed is in this plan: `Workshop`, `Speaker`, `Jamie Chen`, `Planning a Content Model`, no email address except `admin@example.com`, no password. The password is never typed on camera (the recorder signs in from `state.json`).
- Look at every contact sheet of `review.py` for a real name, an address bar, a browser notification, or a leftover content item.
- OCR the final video at 1 frame per second, at every scene change, and at 5 frames per second, and search for real names, email addresses, phone numbers and tokens, as `SKILL.md` step 6 says.
- Delete the snapshots of the data folder and `state.json` when the video is published; none of them is committed.
