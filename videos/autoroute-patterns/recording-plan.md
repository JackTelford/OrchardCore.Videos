# Recording plan: Autoroute patterns

Audience: developer. The video has three recorded clips (`03-edit`, `04-result`, `05-container`) and four slide
chapters that need no recording (`01-idea`, `02-vocabulary`, `06-code`, `07-lookup`). A selector written below stands
for `scene.Page.Locator(...)`. Source paths are relative to the root of `OrchardCMS/OrchardCore`
(`src/OrchardCore.Modules/...`, `src/docs/...`, `src/OrchardCore.Themes/...`). Labels were read from the source at the
commit named in `catalog/SOURCE.md` (92db4a837d); the screen has the last word, see `OPEN-QUESTIONS.md`.

## The site it needs

- **Host**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout of `main`, on `http://localhost:5010`, with
  its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). Block outbound calls with
  `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Provisioning**: Auto Setup, so no Setup or sign-in form is recorded. The variable names are those of
  `src/docs/reference/modules/AutoSetup/README.md` (section "User Secrets and Environment Variables"):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`, the value of `VIDEO_PASSWORD`, chosen outside git |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Tenant**: Default only.
- **Recipe**: Blog (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). It enables everything the video
  needs, so no feature is enabled by hand: Autoroute, Contents, Content Types, Content Fields, Lists, Title, Deployment
  and Admin Menu (`blog.recipe.json` lines 32 to 80; Liquid comes with Contents). It also creates the Blog content
  item titled `Blog`, the Blog Post type with its container pattern, and the admin menu entry `Blog`
  (`blog.recipe.json` line 1197).
- **Settings, external services**: none. No outbound call is needed.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` once, outside the capture.

### Seed data, created outside the capture

Create the Product content type, with a Text field named Color, from **Tools > Deployments > JSON Import**
(`/Admin/DeploymentPlan/Import/Json`; `OrchardCore.Deployment/AdminMenu.cs` line 30). The page has a JSON editor
(CodeMirror) and an Import button (`OrchardCore.Deployment/Views/Import/Json.cshtml` lines 15 and 21). Set the editor's
value to the text below, with `page.EvaluateAsync` and `document.querySelector('.CodeMirror').CodeMirror.setValue(...)`,
click Import, and wait for the notification "Recipe imported successfully!" (`OrchardCore.Deployment/Controllers/ImportController.cs`
line 185). Do it in the recorder's `PrepareAsync(IBrowser browser)`. It receives a browser, not a scene: open a context with
`StorageStatePath = Scenes.StatePath` (the sign-in of `Recorder setup`), a page in it, and use that page.

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Product",
          "DisplayName": "Product",
          "Settings": {
            "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true }
          },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
            {
              "PartName": "AutoroutePart",
              "Name": "AutoroutePart",
              "Settings": {
                "ContentTypePartSettings": { "Position": "1" },
                "AutoroutePartSettings": {
                  "AllowCustomPath": false,
                  "AllowUpdatePath": true,
                  "Pattern": "{{ ContentItem | display_text | slugify }}"
                }
              }
            },
            { "PartName": "Product", "Name": "Product", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
          ]
        }
      ],
      "ContentParts": [
        {
          "Name": "Product",
          "Settings": {},
          "ContentPartFieldDefinitionRecords": [
            {
              "FieldName": "TextField",
              "Name": "Color",
              "Settings": { "ContentPartFieldSettings": { "DisplayName": "Color", "Position": "0" } }
            }
          ]
        }
      ]
    }
  ]
}
```

What the seed gives the clips: a Product type whose Autoroute part has the pattern
`{{ ContentItem | display_text | slugify }}`, **Allow custom path** off (the Permalink field is disabled) and
**Allow path updates** on (the **Refresh the path** checkbox appears). No Product content item exists. All values are
invented. Nothing else is seeded: the Blog Post type and the Blog come from the recipe.

Then open `/Admin/Contents/ContentTypes/Product/Create` once and check that the editor shows a **Title** field and a
**Color** field, then leave without saving.

## Order, dependencies, snapshots

Record in this order: `03-edit`, `04-result`, `05-container`. The slide chapters are drawn from `slides.py`.

| Snapshot | Taken | Used before |
|---|---|---|
| S0 | After the seed (and the sign-in), before any clip | `03-edit` |
| S1 | After a good take of `03-edit` | `04-result`, `05-container` |

Stop the site, copy its `App_Data` folder and `state.json` aside for each snapshot, and restore the copy before a new
take.

| Clip | Changes the site | Depends on |
|---|---|---|
| `03-edit` | Saves a new pattern on the Product type (two lines, with the Color field) | S0 |
| `04-result` | Creates two Product content items and regenerates the path of the second | `03-edit` (the saved pattern with the color) |
| `05-container` | Creates one Blog Post in the Blog | Nothing from the other clips |

To take `04-result` again, restore S1. To take `05-container` again, restore S1 as well (or delete the post).

## Clip `03-edit`

**Starts on** `/Admin/ContentTypes/List`, opened with `scene.OpenAsync` before `StartCaptureAsync`. The Product row must
be visible. Open the admin menu path **Design > Content Definition > Content Types** before the capture, so the menu
shows where the page is (the item opens by itself on this page; if it does not, click Design, then Content Definition).
Label sources: `OrchardCore.ContentTypes/AdminMenu.cs` lines 23 to 25.

`edit-1`
- Click the Edit button of the Product row, selector `a[role='btn-edit-Product']`, with
  `ClickAndWaitForNavigationAsync` (`OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml` line 24).
  The page is titled **Edit Content Type - Product** (`Views/Admin/Edit.cshtml` line 63).
- Wait until the heading **Parts** is visible (`Views/Admin/Edit.cshtml` line 144).
- Click the Edit button of the Autoroute row of the Parts list, selector
  `a[href$='/ContentTypes/Product/ContentParts/AutoroutePart/Edit']`, with `ClickAndWaitForNavigationAsync`
  (`Views/Admin/Edit.cshtml` line 163; the route is `ContentTypes/{id}/ContentParts/{name}/Edit`, `AdminController.cs`
  line 966). The row is named **Autoroute**: the part name without `Part` (`Views/Admin/Edit.cshtml` line 153;
  `OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentTypePartExtensions.cs` line 7 falls back to
  `OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs` line 54, because neither the
  part nor the type part sets a display name and the part's name is not the type's name).
- If the Edit button of the Product row cannot be found, use `NavigateAsync("/Admin/ContentTypes/Edit/Product")`.
- Wait until `.CodeMirror` is visible.
- **On screen when the narration ends**: the page **Edit Part - Autoroute** (`OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml`
  line 22) with the label **Pattern** and, in the editor, the single line `{{ ContentItem | display_text | slugify }}`.

`edit-2`
- No navigation. `HighlightAsync(scene.Page.Locator(".CodeMirror"), "Pattern")`, and keep it until the narration ends.
- **On screen**: the green ring around the Pattern editor, the line `{{ ContentItem | display_text | slugify }}` readable,
  and under it the hint "The pattern used to render the custom url of this content type. With Liquid support."
  (`OrchardCore.Autoroute/Views/AutoroutePartSettings.Edit.cshtml` lines 14 and 17).
- The Pattern field is a CodeMirror editor, not a text box: `Assets/ts/autoroute-part-settings.ts` lines 7 to 11 hands the
  textarea to `initLiquidPatternEditor` (`@orchardcore/bloom/components/liquid-pattern-editor`), which the built
  `wwwroot/Scripts/autoroute-editors/autoroute-part-settings.js` shows to be `CodeMirror.fromTextArea` with mode `liquid`
  and line numbers on, and no automatic closing of brackets. Never type into the textarea locator; it is hidden.

`edit-3`
- `ClearHighlightsAsync()`, then click the text of the first line of the editor, selector `.CodeMirror-line` (first).
- Press `Control+End` with `scene.Page.Keyboard`, then `Enter`, then type
  `-{{ ContentItem.Content.Product.Color.Text | slugify }}` with `scene.Page.Keyboard.TypeAsync(text, new() { Delay = 40 })`.
  Use the keyboard, not `scene.TypeAsync`, because `TypeAsync` clicks first and would move the cursor to the middle of the line.
- **On screen when the narration ends**: line 1 `{{ ContentItem | display_text | slugify }}` and line 2
  `-{{ ContentItem.Content.Product.Color.Text | slugify }}`, with the line numbers 1 and 2 of the editor.
- Nothing is saved yet.

`edit-4`
- Press `Control+End`, then `BackSpace` twice, which removes the closing `}}` of line 2 and leaves
  `-{{ ContentItem.Content.Product.Color.Text | slugify ` unterminated.
- Click Save, selector `button.save`, with `ClickAndWaitForNavigationAsync`
  (`EditTypePart.cshtml` line 111). The page stays on **Edit Part - Autoroute** with a validation summary.
- If no error appears (the page redirects with the success notification), the unterminated output was not refused and
  the pattern is stored. Restore S0, redo `edit-1` to `edit-3`, and in `edit-4` replace line 2 of the editor by the
  single tag `{% if ContentItem %}`, which is never closed, instead of removing the braces. Then save. In `edit-5`,
  put the `edit-3` text of line 2 back. Write in `OPEN-QUESTIONS.md` which input produced the message.
- **On screen when the narration ends**: the validation summary at the top of the form with the text
  "Pattern doesn't contain a valid Liquid expression. Details: ..." (the details vary), and the editor still holding the
  rejected pattern. Source: `OrchardCore.Autoroute/Settings/AutoroutePartSettingsDisplayDriver.cs` line 60. The setting
  is not stored: `OrchardCore.ContentTypes/Controllers/AdminController.cs` line 1079 cancels the document store when the model state is invalid.

`edit-5`
- Click at the end of line 2 of the editor (click the text of the last line, then `Control+End`), type `}}` with
  `scene.Page.Keyboard`, so that line 2 is `-{{ ContentItem.Content.Product.Color.Text | slugify }}` again (or restore
  the `edit-3` text if the fallback input was used).
- The page that refused the pattern is drawn again from the posted form, so the editor should still hold the rejected text:
  the textarea is bound with `asp-for="Pattern"`, which prefers the posted value in the model state to the stored one
  (read from the source, not run: see `OPEN-QUESTIONS.md`). If the editor shows only the stored line 1, press `Control+End`, `Enter` and type the `edit-3`
  line again with the keyboard instead of typing `}}`.
- Click Save, selector `button.save`, with `ClickAndWaitForNavigationAsync`. The page redirects to **Edit Content Type - Product**.
- Click Save late enough that the notification is still on screen when the narration ends (pause about 4 seconds
  after the typing).
- **On screen when the narration ends**: **Edit Content Type - Product** with the success notification
  `The "Autoroute" part settings have been saved.` (`OrchardCore.ContentTypes/Controllers/AdminController.cs` line 1084;
  the redirect is `Edit`, line 1087). If the notification has gone, click Save later in the step.

After the clip: take snapshot S1.

## Clip `04-result`

**Starts on** `/Admin/Contents/ContentTypes/Product/Create` (route `Contents/ContentTypes/{id}/Create`,
`OrchardCore.Contents/Controllers/AdminController.cs` line 407), opened before `StartCaptureAsync`. The page shows the
fields **Title**, **Color** and the Autoroute part with the label **Permalink**.

`result-1`
- `TypeAsync(scene.Page.GetByLabel("Title", new() { Exact = true }), "Trail Mug")` (label: `OrchardCore.Title/Views/TitlePart.Edit.cshtml`
  line 12), then `TypeAsync(scene.Page.GetByLabel("Color", new() { Exact = true }), "Forest Green")`
  (the label is the field's display name, `OrchardCore.ContentFields/Views/TextField.Edit.cshtml` lines 9 and 20).
- `HighlightAsync(scene.Page.GetByLabel("Permalink"), "Permalink")` for a moment: the input is disabled, with the hint
  "The url of the content item. It will be automatically generated."
  (`OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml` lines 30 and 50).
- `ClearHighlightsAsync()`, then click Publish, selector `button.publish`, with `ClickAndWaitForNavigationAsync`
  (`OrchardCore.Contents/Views/Content.PublishButton.cshtml` line 7). With no `returnUrl` the Create action redirects to the
  Edit page of the new item (`OrchardCore.Contents/Controllers/AdminController.cs` lines 769 to 807).
  Click it with about 3 seconds of narration left.
- **On screen when the narration ends**: the Edit page of the product, with the notification "Your Product has been
  published." (`AdminController.cs` line 473), or the filled form just before the click.

`result-2`
- No navigation. `HighlightAsync(scene.Page.GetByLabel("Permalink"), "Permalink")`.
- **On screen when the narration ends**: the Permalink input holding `trail-mug-forest-green`, with the green ring.
  The Title shows `Trail Mug` and Color shows `Forest Green`.

`result-3`
- `ClearHighlightsAsync()`, `NavigateAsync("/Admin/Contents/ContentTypes/Product/Create")`, type Title `Trail Mug` and
  Color `Forest Green` as in `result-1`, click Publish, selector `button.publish`, with `ClickAndWaitForNavigationAsync`.
- Then `HighlightAsync(scene.Page.GetByLabel("Permalink"), "Permalink")`.
- **On screen when the narration ends**: the Edit page of the second product, the Permalink input holding
  `trail-mug-forest-green-1`. This value is derived from `AutoroutePartHandler.GenerateUniqueAbsolutePathAsync`
  (`OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs` line 456): the first free version is 1. Check it on screen.

`result-4`
- `ClearHighlightsAsync()`. On this same page (the second product), clear the Color input with
  `scene.Page.GetByLabel("Color", new() { Exact = true }).FillAsync("")`, then `TypeAsync(...)` with `Slate Blue`.
- Check the **Refresh the path** checkbox, `scene.Page.GetByLabel("Refresh the path")`, with `ClickAsync`
  (`OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml` line 87; it is shown because Allow path updates is on;
  hint: "Check to refresh the path. Warning: the previous URL won't be accessible anymore.").
- Click Publish, selector `button.publish`, with `ClickAndWaitForNavigationAsync`. The page redirects to the Edit page.
- Then `HighlightAsync(scene.Page.GetByLabel("Permalink"), "Permalink")`.
- **On screen when the narration ends**: the Permalink input holding `trail-mug-slate-blue`, the Color input
  `Slate Blue`, and the **Refresh the path** checkbox unchecked again after the reload.
  The driver sets the path to empty when the box is checked (`OrchardCore.Autoroute/Drivers/AutoroutePartDisplayDriver.cs`
  lines 103 to 107), and the handler generates it again on update (`AutoroutePartHandler.cs` lines 145 to 149).

After the clip: nothing to clean up if S1 is restored for the next take.

## Clip `05-container`

**Starts on** `/Admin/ContentTypes/BlogPost/ContentParts/AutoroutePart/Edit`, opened before `StartCaptureAsync`
(**Edit Part - Autoroute**, reached from the Blog Post type). The Pattern editor shows the recipe's pattern
`{{ Model.ContentItem | container | display_text | slugify }}/{{ Model.ContentItem | display_text | slugify }}`
(`blog.recipe.json` line 216). It is longer than the editor, so the end of the line is cut off: that is fine, the
slide of the chapter "What a pattern can use" shows the whole text. **Do not save** this page.

`container-1`
- `HighlightAsync(scene.Page.Locator(".CodeMirror"), "Pattern")` and keep it until the narration ends.
- **On screen when the narration ends**: the ring around the Pattern editor, with the start of the container pattern.

`container-2`
- `ClearHighlightsAsync()`. Go to the Blog page with the admin menu entry **Blog** (a link in the admin menu, added by
  the recipe, `blog.recipe.json` line 1197), using `ClickAndWaitForNavigationAsync`. If the menu is collapsed, expand it
  before `StartCaptureAsync`. Its address is `/Admin/Contents/ContentItems/<Blog content item id>/Display`; read the
  link's `href` before the capture, and use `NavigateAsync` with it if the click is not reliable.
- Click the button **Create Blog Post**, selector `a:has-text('Create Blog Post')`, with `ClickAndWaitForNavigationAsync`
  (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml` line 52; the Blog allows only the Blog Post type, so the
  button is not a drop-down). The editor opens at `/Admin/Contents/ContentTypes/BlogPost/Create` with the query string
  `ListPart.ContainerId=<Blog content item id>` and a `returnUrl`.
- `TypeAsync(scene.Page.GetByLabel("Title", new() { Exact = true }), "Notes from the Orchard")`.
- Because the query string holds a `returnUrl`, Publish is a split button
  (`OrchardCore.Contents/Views/Content.PublishButton.cshtml` lines 12 to 26). Click the drop-down toggle, selector
  `button.publish + button.dropdown-toggle`, then the item **and continue**, selector `button.publish-continue`, with
  `ClickAndWaitForNavigationAsync`. This publishes and stays on the Edit page of the post. (The plain Publish button would
  return to the Blog page.)
- Do the clicks in the last seconds of the narration of this step.
- Choosing **and continue** stores it as the default action of this split button in the browser's admin preferences
  (`OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/actionGroup.ts`), so the main button reads **Publish and continue** on
  the next page that has a `returnUrl`. The recorder opens every take from `state.json` in a new context, so nothing
  carries over from one take to the next; in a browser used by hand, clear the site's data before a new take.
- **On screen when the narration ends**: the Blog Post editor with the Title `Notes from the Orchard`, or the Edit page
  after publishing.

`container-3`
- No navigation. `HighlightAsync(scene.Page.GetByLabel("Permalink"), "Permalink")`.
- **On screen when the narration ends**: the Edit page of the post, the Permalink input (enabled for this type) holding
  `blog/notes-from-the-orchard`, with the green ring. The notification "Your Blog Post has been published." can still be
  visible.
- The second sentence of the narration (a post created outside the blog) is not shown on screen: it comes from
  `OrchardCore.Lists/Liquid/ContainerFilter.cs`, which returns the item itself when it has no `ContainedPart`.

## Slides (drawn by `slides.py`, no recording)

- `idea-1`: four cards, "A Liquid pattern per content type" bright.
- `idea-2`: the same cards, "Rendered while the path is empty" bright.
- `idea-3`: the same cards, "Unique" and "Valid characters" bright.
- `use-1`: a code panel of Liquid patterns, the two lines with `ContentItem` and `Model.ContentItem` bright, and two
  cards about the `display_text` and `slugify` filters.
- `use-2`: the same panel, the container pattern bright, and two cards about the `container` filter.
- `use-3`: the same panel, the Color field pattern bright, and two cards about the `Content` property.
- `code-1`: a C# data migration that sets the pattern with `WithSettings(new AutoroutePartSettings { ... })`.
- `code-2`: the `ContentDefinition` recipe step of the Article type of the Blog recipe, with the `AutoroutePartSettings` object.
- `lookup-1`: two Liquid lines, `Content["slug:..."]` and `Content.Slug["..."]`, with two cards.
- `lookup-2`: Razor code calling `GetContentItemBySlugAsync` with and without the second argument, with two cards.

## Checks for private data

- Every name, title and value on screen is invented: `Trail Mug`, `Forest Green`, `Slate Blue`, `Notes from the Orchard`.
  The Blog content comes from the Blog recipe and holds no real person: the Blog page of `container-2` lists the recipe's
  sample post "Man must explore, and this is exploration at its greatest" (`blog.recipe.json` line 1002), a quotation
  with no name next to it.
- The only account is `admin` with `admin@example.com`. The password is the value of `VIDEO_PASSWORD`, never typed on
  camera (the sign-in is `state.json`), and never written in a file of this repository.
- Look at every contact sheet (`review.py`) for an error page, a stack trace, or the sign-in form.
- OCR the final video at one frame per second and at every scene change, and search for anything that is not
  `admin`, `example.com` or one of the invented values above.
- `state.json`, any mask script and every copy of `App_Data` stay outside git.
