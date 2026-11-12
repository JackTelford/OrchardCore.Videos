# Recording plan: Write a Content Part With a Display Driver

The video has slides and two browser recordings. The slides show the code of the Title part (`OrchardCore.Title`), so
the recordings need no module of their own: they run on a stock Orchard Core site, where the Title part already
exists. Nothing is built, and no code is typed on camera.

In this plan a selector stands for `scene.Page.Locator(...)`. Every path is relative to the root of the Orchard Core
checkout the catalog was read from (commit `92db4a83`), and every label is the text of the code at that place.

## The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of the checkout above (or a later one), run on `http://localhost:5010` |
| Data | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup (`OrchardCore__AutoSetup__Tenants__0__...`, see `src/docs/reference/modules/AutoSetup/README.md`), so no setup form is recorded |
| Tenant | `Default` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Recipe | `Blank` (`src/OrchardCore.Themes/TheAdmin/Recipes/blank.recipe.json`: "Blank site"), not the Blog recipe |
| User | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Database | Sqlite |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls no outside service |

Why the Blank recipe: it enables the features the video uses (`OrchardCore.Features`, `OrchardCore.Contents`,
`OrchardCore.ContentTypes` and `OrchardCore.Title`: `blank.recipe.json` lines 26, 42, 43 and 56) and defines no content
type, so after the first clip the Content Items page has exactly one creatable type and shows a single **New Workshop**
button (`src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml` line 14). The Blog recipe
creates several creatable types, and its site theme does not render the `Header` zone where the Title part places its
shape (see OPEN-QUESTIONS.md).

Features to enable before recording: none.

Settings to save: none, except the site theme below.

External services and local stand-ins: none.

### Seed, outside the capture

1. Start the site. Auto Setup installs it on the first request.
2. Run `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`, which saves `state.json` (git-ignored).
3. In the admin, open `/Admin/Themes` (Design, Themes: `src/OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs` line 21).
   On the card titled **The Default Theme** (`src/OrchardCore.Themes/TheTheme/Manifest.cs` line 5), click **Enable**
   (`src/OrchardCore.Modules/OrchardCore.Themes/Views/Items/ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml` line 16),
   then **Make Current**
   (`src/OrchardCore.Modules/OrchardCore.Themes/Views/Items/ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml` line 7). Open `/` in the
   browser: the page renders with that theme and no error.
4. Stop the site and copy its `App_Data` folder and `state.json` aside. This is snapshot **S0**.

The data typed on camera is the only data: the content type **Workshop**, and the content item **Pruning Apple Trees**.

## Order and snapshots

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `06-attach` | S0 | Yes: creates the content type Workshop and attaches the Title part to it, with the behavior "Title is editable and required" |
| 2 | `07-use` | S1 | Yes: creates the content item Pruning Apple Trees |

S1 is the site as `06-attach` leaves it. After recording `06-attach`, stop the site and copy `App_Data` aside as
**S1**. `07-use` depends on `06-attach`. To record `06-attach` again, restore S0 first. To record `07-use` again,
restore S1. To record both, restore S0 and record in the order above. The slide steps need no site.

Narrate before recording (`scripts/narrate.py`): the recorder holds each step for the length of its narration. The
estimates below use 2.5 words per second; check the real lengths in `durations.json`.

## Clip `06-attach`

Starts on `/Admin/ContentTypes/List` (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs`
line 72), opened with `scene.OpenAsync` before `StartCaptureAsync`. The page is the list of content types: no type is
called Workshop, and the button **Create new type** is visible. In the lines below, `CT` stands for
`src/OrchardCore.Modules/OrchardCore.ContentTypes` and `TI` for `src/OrchardCore.Modules/OrchardCore.Title`.

`attach-1` (about 9 s)
- Show the menu path of the narration with a caption: `HighlightAsync(.oc-breadcrumb-title, "Design > Content Definition > Content Types")`,
  `HoldAsync(1.2)`, `ClearHighlightsAsync()`. If `.oc-breadcrumb-title` is not on the page, use the same call on the
  admin menu entry `a:text-is("Content Types")`, after expanding Design and Content Definition before
  `StartCaptureAsync`. The labels come from `CT/AdminMenu.cs` lines 23 to 25.
- `ClickAndWaitForNavigationAsync(#btnCreate)`. The label **Create new type** is in `CT/Views/Admin/List.cshtml` line 20.
- `TypeAsync(#DisplayName, "Workshop", 60)`, then `PauseAsync(500)`. The label **Display Name** is in
  `CT/Views/Admin/Create.cshtml` line 14. The **Technical Name** field (line 22) fills itself with `Workshop`
  (`CT/wwwroot/Scripts/autocomplete/autocomplete.js`); do not type in it.
- `ClickAndWaitForNavigationAsync(form button.create)`. The label **Create** is in `CT/Views/Admin/Create.cshtml` line 30.
- When the narration ends, the page **Add Parts** is visible, with the text "Choose the Parts to add to this Content
  Type." (`CT/Views/Admin/AddPartsTo.cshtml` line 18), the list of parts, and the toast `The "Workshop" content type has
  been created.` if it has not faded yet (`CT/Controllers/AdminController.cs` line 162).

`attach-2` (about 11 s)
- `HighlightAsync(li.list-group-item:has(label:text-is("Title")), "Title")`, `HoldAsync(3.5)`. The row shows the part
  **Title** and its description "Provides a Title for your content item." (`TI/Migrations.cs` line 33; the row is
  labeled with the name of the part without "Part", `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs`
  line 60). Do not tick any check box.
- `ClearHighlightsAsync()`, then
  `ClickAndWaitForNavigationAsync(li.list-group-item:has(label:text-is("Title")) button[name="submit.AddAndConfigure"])`.
  The label **Add & Configure** is in `CT/Views/Admin/AddPartsTo.cshtml` line 36.
- When the narration ends, the page **Edit Part - Title** is visible (`CT/Views/Admin/EditTypePart.cshtml` line 22) with
  the select **Behavior option**, showing "Title is editable", and the toast `The "Title" part has been added.`
  (`CT/Controllers/AdminController.cs` line 355) if it has not faded yet.

`attach-3` (about 11 s)
- `SelectAsync(.ocat-limited-wrapper:has(label:text-is("Behavior option")) select, "Title is editable and required")`.
  The labels are in `TI/Views/TitlePartSettings.Edit.cshtml` lines 15 and 19.
- `HighlightAsync(.ocat-limited-wrapper:has(label:text-is("Behavior option")) select)`, `HoldAsync(1.5)`,
  `ClearHighlightsAsync()`.
- `ClickAndWaitForNavigationAsync(button.save[name="submit.Save"])`. The label **Save** is in
  `CT/Views/Admin/EditTypePart.cshtml` line 111.
- When the narration ends, the page of the content type is visible (title "Edit Content Type - Workshop",
  `CT/Views/Admin/Edit.cshtml` line 63) with the part Title in its list of parts, and the toast
  `The "Title" part settings have been saved.` if it has not faded yet (`CT/Controllers/AdminController.cs` line 1084).

After the clip: stop the site and keep its `App_Data` as snapshot S1.

## Clip `07-use`

Starts on `/Admin/Contents/ContentItems` (`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs`
line 67), opened with `scene.OpenAsync` before `StartCaptureAsync`. The page is **Manage Content**
(`CO/Views/Admin/List.cshtml` line 17), with no content item and the single button **New Workshop**. In the lines
below, `CO` stands for `src/OrchardCore.Modules/OrchardCore.Contents` and `TI` for
`src/OrchardCore.Modules/OrchardCore.Title`.

`use-1` (about 10 s)
- `HighlightAsync(a.btn:text-is("New Workshop"))`, `HoldAsync(1)`, `ClearHighlightsAsync()`. The label is
  `T["New {0}"]` with the display name Workshop (`CO/Views/ContentsAdminListCreate.cshtml` line 14). The menu path of
  the narration, Content, Content Items, is `CO/AdminMenu.cs` lines 65 and 68.
- `ClickAndWaitForNavigationAsync(a.btn:text-is("New Workshop"))`. The address of the editor carries the return address
  of the list (`returnUrl`), which the form keeps (`CO/Views/Admin/Create.cshtml`, the `asp-route-returnUrl` of the form).
- Wait until `#TitlePart_Title` is visible, then `HighlightAsync(#TitlePart_Title, "Title")`. The label **Title** is in
  `TI/Views/TitlePart.Edit.cshtml` line 12.
- When the narration ends, the editor **New Workshop** is visible (`CO/Views/Admin/Create.cshtml` line 26) with an empty
  field **Title** marked as required, ringed in green, and the buttons **Publish** and **Save Draft**.

`use-2` (about 11 s)
- `ClearHighlightsAsync()`, `PauseAsync(600)`.
- `ClickAndWaitForNavigationAsync(button.publish[name="submit.Publish"])`. The label **Publish** is in
  `CO/Views/Content.PublishButton.cshtml` line 7 (line 19 when the page was opened with a return address, as it is
  here). The field stays empty.
- Wait until `span.field-validation-error` holds "A value is required for Title." (`TI/Drivers/TitlePartDisplayDriver.cs`
  line 56), then `HighlightAsync(span.field-validation-error)`. The message is also listed in the validation summary at
  the top of the form (`CO/Views/Admin/Create.cshtml`, `Html.ValidationSummary()`); the highlight is on the one under
  the field.
- When the narration ends, the editor is shown again with the message under the field **Title**, ringed in green. No
  item has been created. The editor keeps the return address, so the next **Publish** goes back to the list.

`use-3` (about 11 s)
- `ClearHighlightsAsync()`, then `TypeAsync(#TitlePart_Title, "Pruning Apple Trees", 60)`.
- Click **Publish** within 3 s of the start of the step, so the toast is on screen while the end of the narration is
  spoken: `ClickAndWaitForNavigationAsync(button.publish[name="submit.Publish"])`. The page returns to Manage Content
  (`CO/Controllers/AdminController.cs` lines 795 to 797, the redirect to the return address).
- Wait until `.toast.message-success` holds "Your Workshop has been published." (`CO/Controllers/AdminController.cs`
  line 473). A success toast fades after 5 s (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs`
  line 71).
- When the narration ends, Manage Content lists one row, "Pruning Apple Trees", of the type Workshop, with the buttons
  **Edit** and **View** (`CO/Views/ContentsButtonEdit_SummaryAdmin.cshtml` lines 13 and 18). The toast is visible if the
  step was fast enough; the row is visible in any case.

`use-4` (about 10 s)
- The button **View** opens the page in a new tab (`target="_blank"`). Get the tab with
  `scene.Page.Context.RunAndWaitForPageAsync(() => scene.ClickAsync(a.btn.view))`, then `SwitchToAsync(thatPage)`.
- Wait until `article.content-item h1` is visible, then `HighlightAsync(article.content-item h1)`. The heading is the
  template `TI/Views/TitlePart.cshtml`, placed in the `Header` zone by the driver (`TI/Drivers/TitlePartDisplayDriver.cs`
  lines 34 and 35); the page is `/Contents/ContentItems/<id>` (`CO/Startup.cs` line 223), rendered with The Default Theme.
- When the narration ends, the page of the item is visible in the theme, with the heading "Pruning Apple Trees"
  ringed in green.

`use-5` (about 11 s)
- `ClearHighlightsAsync()`, `SwitchToAsync(theManageContentPage)`.
- `ClickAndWaitForNavigationAsync(a.btn.edit)`.
- Wait until `#TitlePart_Title` is visible, then `HighlightAsync(#TitlePart_Title)`.
- When the narration ends, the editor **Edit Workshop** is visible (`CO/Views/Admin/Edit.cshtml` line 39) with
  "Pruning Apple Trees" in the field **Title**, ringed in green. The value comes from the display text of the content
  item (`TI/Drivers/TitlePartDisplayDriver.cs` lines 42 and 60).

## Slides

Every slide is drawn by `slides.py` from the Title part of `src/OrchardCore.Modules/OrchardCore.Title`; no site is
needed. The code is the source, trimmed with comments that read "..." where lines are left out. File names below are
under that folder.

- `pieces-1`: three cards, "Data", "An editor", "A display": what a part adds to a content item.
- `pieces-2`: four cards: the class, the display driver, the templates, the registration and the definition.
- `pieces-3`: three cards: the driver creates a shape, its location decides where it goes, a template renders it.
- `class-1`: `Models/TitlePart.cs` in full, with three note cards beside it.
- `class-2`: `ViewModels/TitlePartViewModel.cs` in full, with the `BindNever` lines bright.
- `class-3`: `CreateAsync` of `Migrations.cs` (lines 29 to 39), trimmed, with `AlterPartDefinitionAsync`, `Attachable`
  and `WithDescription` bright.
- `driver-1`: the class and constructor of `Drivers/TitlePartDisplayDriver.cs` (lines 11 to 18), with the class line,
  the localizer and the closing comment bright.
- `driver-2`: `Display` (lines 20 to 36) with the call to `Initialize` bright.
- `driver-3`: `Display` with `GetDisplayShapeType` and the two `Location` calls bright.
- `driver-4`: `Display` with the settings check and `return null` bright, and the two `Location` calls.
- `driver-5`: `Edit` (lines 38 to 47) with `GetEditorShapeType` bright.
- `driver-6`: `UpdateAsync` (lines 49 to 64) with the signature and `TryUpdateModelAsync` bright.
- `driver-7`: `UpdateAsync` with the check of `EditableRequired`, `AddModelError` and `return Edit` bright.
- `templates-1`: three cards: shape types `TitlePart`, `TitlePart_Summary` and `TitlePart_Edit` with their files.
- `templates-2`: `Views/TitlePart.cshtml` and the editor template `Views/TitlePart.Edit.cshtml`, trimmed, with `asp-for` and
  `asp-validation-for` bright.
- `templates-3`: `Views/TitlePart.cshtml` and `Views/TitlePart.Summary.cshtml`.
- `register-1`: `ConfigureServices` of `Startup.cs` (lines 19 to 30), trimmed, with the four registration calls bright.
- `further-1`: four cards, the first two bright: settings and handlers.
- `further-2`: the same four cards, the last two bright: placement and alternates.

## Checks for private data

- Only fake data appears: the type Workshop, the item Pruning Apple Trees, the user `admin`, `admin@example.com`. No real
  name, email address or token is typed or shown.
- The password never appears on screen: the site is signed in by `state.json`, and `<password>` is only an
  environment value.
- Look at every contact sheet (`review.py`): no error page, no stack trace, no leftover content item or content type.
- Before publishing, scan the video by OCR for the strings "example.com", "@" and "password": the only address is
  `admin@example.com`, and the password never shows.
