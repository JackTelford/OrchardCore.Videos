# Recording plan: Liquid Part and Liquid Field

This plan lets a person record the two browser clips of the video without deciding anything. The slides are drawn by
`slides.py` and need no recording. Selectors below stand for `scene.Page.Locator(...)`; the recorder's methods take
locators. Source paths refer to commit `92db4a83` of `OrchardCMS/OrchardCore`, the one the labels were read from. A path
that starts with `src/` is relative to the root of the checkout. Any other path is relative to `src/OrchardCore.Modules`
when it starts with a module name (`OrchardCore.Contents/...`), to `src` when it starts with `OrchardCore.Themes/`, and to
`src/OrchardCore` when it starts with `OrchardCore.ContentManagement.Abstractions/`.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at a commit that has the Liquid
  part and the Liquid field (`src/OrchardCore.Modules/OrchardCore.Liquid`), on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Provisioning**: Auto Setup, no setup or login form recorded (see the `orchardcore-tester` skill of that repository,
  `references/autosetup.md`). The values:

  | Setting | Value |
  | --- | --- |
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`, chosen outside git; the recorder gets it as `VIDEO_PASSWORD` |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Recipe**: Blog (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). It enables Contents, Content Types,
  Content Fields, Title, Html, Content Preview and the site theme TheBlogTheme. The Liquid feature is enabled with
  Contents, which depends on it (`OrchardCore.Contents/Manifest.cs:14`). **No feature has to be enabled**, and no
  setting has to be saved.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` once, outside the capture.
  The sign-in is saved as `state.json` (git-ignored). The user `admin` is in the Administrator role, which holds the Manage Liquid
  templates permission by default (`src/OrchardCore/OrchardCore.Liquid.Abstractions/Permissions.cs`).
- **External services**: none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address so the site never calls out.
- **Window**: the recorder's viewport (1536 x 864, light color scheme, `en-US`). Leave the admin menu collapsed; no
  clip uses it.

### The seed data (created outside the capture)

The only seed is one content type. Create it by hand in the browser, or with a `PrepareAsync` that drives the same pages:

1. Open `/Admin/ContentTypes/Create` (`OrchardCore.ContentTypes/Controllers/AdminController.cs:94`). Display Name
   `Notice`. The Technical Name fills itself with `Notice`. Choose Create.
2. The site opens the Add Parts page of the new type. Check the part labeled `Title` (description: Provides a Title for
   your content item.) and choose Add Selected Parts.
3. The Edit Content Type page of Notice opens. Its Parts list holds `Title` and `Notice` (the part for the fields of
   the type itself, with the hint "Contains the fields for the current type"). It holds no Liquid part and no field:
   the clips add them.

No content item exists for Notice before recording, and no other content is created or edited. The Blog recipe's own
sample content stays as it is.

### Snapshots

- **S0**: after the seed. Copy `App_Data` and `state.json` aside. Restore S0 before every full take and before a new
  take of `02-part`.
- **S1**: after `02-part`. The type Notice has the Liquid part, and the published content item "Open House" exists.
  Copy `App_Data` and `state.json` aside. Restore S1 before a new take of `03-field`.

## Order of the clips

1. `02-part` (starts from S0, changes the site: adds the Liquid part to Notice, creates and publishes "Open House").
2. `03-field` (starts from S1; **depends on `02-part`**: it needs the Notice type and the item "Open House". It changes
   the site: adds the field Closing Note and republishes the item).

The clips play in the same order as they are recorded. The chapters `01-idea`, `04-code`, `05-render` and `06-safe` are
slides and `00-intro` and `99-outro` are cards.

## Clip `02-part` (chapter 02, The Liquid part)

**Starts on** `/Admin/ContentTypes/Edit/Notice` (`OrchardCore.ContentTypes/Controllers/AdminController.cs:167`), opened
before the capture with `OpenAsync`, scrolled so the Parts heading and the Add Parts button are visible. The page title
reads "Edit Content Type - Notice" (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`).

### `part-1`

- Actions: move to the `Add Parts` button under the Parts list (`a[role=button]` with the text "Add Parts") and
  `ClickAndWaitForNavigationAsync` on it. Source of the label: `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:201`;
  the heading "Parts" is at line 144.
- Wait until: the sentence "Choose the Parts to add to this Content Type." is shown.
- Visible when the narration ends: the Add Parts page, with the sentence "Choose the Parts to add to this Content Type."
  (`OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:18`) and the list of parts with a checkbox each.

### `part-2`

- Actions: `HighlightAsync` on the list row whose text contains "Provides a Liquid formatted body for your content item."
  (`li.list-group-item` filtered with `HasText`; the row's label is `Liquid`). Then click that row's checkbox
  (`input[type=checkbox]`). The button "Add Selected Parts" appears (it is hidden until a part is checked,
  `AddPartsTo.cshtml:47`); `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` on it.
- Labels: the row label is the part name without "Part" (`OrchardCore.ContentTypes/ViewModels/EditPartViewModel.cs:27`);
  the description is `OrchardCore.Liquid/Migrations.cs:20`.
- Wait until: the Parts list of the Edit Content Type page shows the row `Liquid`.
- Visible when the narration ends: the Edit Content Type page of Notice again, with the green notice that a part has
  been added (`OrchardCore.ContentTypes/Controllers/AdminController.cs:372`) and the Parts list holding `Title`,
  `Liquid` (with its description) and `Notice`.

### `part-3`

- Actions: `NavigateAsync("/Admin/Contents/ContentItems")` (the list of Content > Content Items,
  `OrchardCore.Contents/AdminMenu.cs:68`, route `OrchardCore.Contents/Controllers/AdminController.cs:67`). Click the
  `New` drop-down button (`#new-dropdown`, text "New", `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:20`),
  then the item `Notice` (`a.dropdown-item`) with `ClickAndWaitForNavigationAsync`. On the page "New Notice", `TypeAsync`
  into the Title field (the input labeled "Title", `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`) the text
  `Open House`. Scroll so the editor labeled "Body" and its hint are visible (`ScrollAsync`).
- Wait until: the editor labeled "Body" is shown (the code editor is drawn after the page loads).
- Visible when the narration ends: the title "Open House" in the Title field, and below it the editor labeled "Body"
  (an empty code editor with line numbers) with the hint "The body of the content item, in Liquid format." and the
  "See documentation" link (`OrchardCore.Liquid/Views/LiquidPart.Edit.cshtml:14-19`).

### `part-4`

- Actions: `ClickAsync` on the code editor of the Body (`.ocat-wrapper:has(textarea.liquid-part-editor) .CodeMirror`),
  then type this text on one line with `scene.Page.Keyboard.TypeAsync` and a delay of 40 ms, no line break (the
  recorder's `TypeAsync` calls `PressSequentially` on its locator, which is not the editor's input):

  ```
  <p>Welcome to {{ Site.SiteName }}. Updated {{ ContentItem.ModifiedUtc | local | date: "%B %d, %Y" }}.</p>
  ```

  The editor is configured with line numbers, active-line styling, bracket matching and the Liquid mode, and no auto-close
  (`OrchardCore.Liquid/wwwroot/Scripts/liquid-part/liquid-part.js`; the field's editor is configured the same way at
  `OrchardCore.Liquid/Views/LiquidField.Edit.cshtml:39-45`), so the text lands as typed. Check that on the first take.
- Visible when the narration ends: the whole line in the editor, with no horizontal scroll bar covering the end of it.
- Source of the names: `Site` is registered in `OrchardCore.Settings/Startup.cs:84`; the filter `local` in
  `OrchardCore.Liquid/Startup.cs:78`; `ContentItem` is set by `OrchardCore.Liquid/Services/LiquidShapes.cs:53-56`.

### `part-5`

- Actions: `ClickAsync` inside the Body editor, press `Control+End` (`scene.Page.Keyboard.PressAsync`) so the caret is
  after the template, type the text ` {% if true %}` with `scene.Page.Keyboard.TypeAsync` and a delay of 40 ms (a space,
  then the tag, which is left open on purpose), and `ClickAndWaitForNavigationAsync` on the `Publish` button (`button.publish`,
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`). The page comes back with an error instead of the list.
- Wait until: the validation summary (`.validation-summary-errors`, or the alert at the top of the form) is shown.
- Visible when the narration ends: the error summary at the top of the form, showing a message that starts "The Liquid
  Body doesn't contain a valid Liquid expression." (`OrchardCore.Liquid/Drivers/LiquidPartDisplayDriver.cs:67`; the
  summary is drawn by `OrchardCore.Contents/Views/Admin/Create.cshtml`), and the form still on screen. The item is
  not saved. See OPEN-QUESTIONS.md, question 2, for what the form holds after the error.

### `part-6`

- Actions, in this order:
  1. If the Body editor still holds the template with the extra tag: `ClickAsync` in it, press `Control+End`, and
     press `Backspace` 14 times, which removes ` {% if true %}`. If the editor came back empty: `ClickAsync` on it and type
     the template of `part-4` again with `scene.Page.Keyboard.TypeAsync`.
  2. If the Title field came back empty, `TypeAsync` `Open House` into it.
  3. `ClickAndWaitForNavigationAsync` on `Publish`. The Content Items list opens with the notice "Your Notice has been
     published." (`OrchardCore.Contents/Controllers/AdminController.cs:473`).
  4. On the row "Open House", click `View` (`a.btn.view`, `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`).
     It opens a new tab (`target="_blank"`): get it with `scene.Page.Context.RunAndWaitForPageAsync(...)` around the
     click, then `SwitchToAsync(page)`.
- Visible when the narration ends: the public page `/Contents/ContentItems/<id>`
  (`OrchardCore.Contents/Startup.cs:223`) in TheBlogTheme: the header with "Open House"
  (`OrchardCore.Themes/TheBlogTheme/Views/Content.liquid:9`) and below it the paragraph "Welcome to Orchard Core. Updated
  <date>." with the date written as month name, day and year. Wait for the paragraph to be shown before the narration
  ends.

## Clip `03-field` (chapter 03, The Liquid field)

**Starts on** `/Admin/ContentTypes/Edit/Notice`, opened before the capture, scrolled so the Fields heading and the Add
Field button are visible. The Fields list is empty. State S1 is required.

### `field-1`

- Actions: move to the `Add Field` button under the Fields heading (`a[role=button]`, text "Add Field",
  `OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:137`; heading at line 88) and `ClickAndWaitForNavigationAsync` on
  it. On the page "Add New Field To "Notice"" (`OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml`), `TypeAsync`
  `Closing Note` into the input labeled "Display Name" (line 38). Wait until the input labeled "Technical Name"
  (line 46) holds `ClosingNote`: the page fills it as the display name is typed
  (`OrchardCore.ContentTypes/Assets/js/autocomplete.ts`).
- Visible when the narration ends: the Display Name `Closing Note` and the Technical Name filled in below it.

### `field-2`

- Actions: scroll to the "Field Type" list (line 54) and click the radio labeled `Liquid Field` (the label is the
  field name split into words, `AddFieldTo.cshtml:62`). Then click `Save` (`button.save`, text "Save", line 71) with
  `ClickAndWaitForNavigationAsync`.
- Visible when the narration ends: the Edit Content Type page of Notice, with the notice `The field "Closing Note" has
  been added.` (`OrchardCore.ContentTypes/Controllers/AdminController.cs:768`) and the Fields list holding "Closing
  Note", with the words "Liquid Field" next to it.

### `field-3`

- Actions: `NavigateAsync("/Admin/Contents/ContentItems")`; on the row "Open House" click `Edit`
  (`a.btn.edit`, `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`) with
  `ClickAndWaitForNavigationAsync`. Scroll to the editor labeled `Closing Note` and `ClickAsync` on its code editor
  (`div.field-wrapper-notice-closing-note .CodeMirror`: the wrapper class is built from the part name and the field name,
  `OrchardCore.Liquid/Views/LiquidField.Edit.cshtml:20`; the Body editor of the Liquid part is another CodeMirror on the
  same page, so keep the locator scoped to this wrapper). Then `scene.Page.Keyboard.TypeAsync`, with a delay of 40 ms:

  ```
  <p><a href="{{ ContentItem | display_url }}">Link to this notice</a></p>
  ```

- Labels: the editor is labeled with the display name of the field
  (`OrchardCore.Liquid/Views/LiquidField.Edit.cshtml:21`) and its hint is at line 25. The filter `display_url` is
  `OrchardCore.Contents/Liquid/DisplayUrlFilter.cs`.
- Wait until: the code editor under the label "Closing Note" is shown.
- Visible when the narration ends: the label "Closing Note", the hint "The content of the field, in Liquid format.", and
  the typed line in the editor.

### `field-4`

- Actions: `ClickAndWaitForNavigationAsync` on `Publish`; the Content Items list opens with the notice "Your Notice has
  been published.". Click `View` on the row "Open House" (new tab, as in `part-6`) and `SwitchToAsync`. Scroll
  (`ScrollAsync`) until the paragraph of the part and the link "Link to this notice" are both visible.
- Visible when the narration ends: the public page with the paragraph "Welcome to Orchard Core. Updated <date>." and,
  on the page too, the link "Link to this notice". The link's address ends with the id of the item.

## The slides

Every slide is drawn by `slides.py`; nothing is recorded for them.

- `idea-1`: the heading "Liquid inside content" and four cards; the first two, Liquid part and Liquid field, are bright.
- `idea-2`: the same cards; the last two, "Stored as submitted" and "Other body types", are bright. The last card names
  the components that keep Liquid as text: `HtmlBodyPart`, `HtmlField`, `MarkdownBodyPart` and `MarkdownField`.
- `code-1`: the migration code that attaches the part and adds the field (`WithPart`, `WithField`, `OfType`), with the
  `Creatable` and `Listable` settings the admin gives a new type.
- `render-1`: `LiquidShapes.cs` with the method that describes the three shapes bright and the rest dimmed.
- `render-2`: the same file with the rendering method bright (the shape passed in as the model, the content item passed
  in as `ContentItem`).
- `render-3`: the three Razor views of the part, its summary and the field, with the `@using` line of the summary and the
  field view.
- `render-4`: the part handler's body aspect and the GraphQL field `liquid`.
- `safe-1`: the `ManageLiquidTemplates` permission and its default roles.
- `safe-2`: the authorization handler, with three notes about edit and publish, the whole item, and nested content.

## Checks for private data

- Only the fake values of this plan appear: the user `admin`, the email `admin@example.com`, the site name "Orchard
  Core", the title "Open House". The password is never typed on camera: the sign-in comes from `state.json`.
- The Blog recipe's sample content is the only other content on the site; confirm that no real name, email address,
  phone number or token shows on the Content Items list (the "Author" column or the Owner of each row), or in any
  notification.
- Look at the contact sheet of each clip (`review.py`): no error other than the one of `part-5`, no empty list, and no
  localhost port or machine path in a visible address bar (the capture shows the page only).
- Search the OCR of the finished video for `@` and for digits: only `admin@example.com`-style values may remain.
