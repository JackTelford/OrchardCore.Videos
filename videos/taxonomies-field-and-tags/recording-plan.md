# Recording plan: Taxonomy Field: Categories and Tags on Your Content

This plan is for the person who records the clips. It lists the site, the seed data, the order of the clips, and for
every step of every recorded clip the actions, what must be on screen when the narration of that step ends, and where
each label comes from. A selector written here stands for `scene.Page.Locator(...)`. Source paths are relative to
`src/` of `OrchardCMS/OrchardCore`; the labels were read at commit `92db4a83`, so compare them with the checkout you
run if it is a later one. Nothing in this plan needs a real person, a real address or a real password.

After the opening card, the video has five chapters: one made of slides (no recording), then four recorded clips.

| Chapter | Clip | Kind | Starts on |
|---|---|---|---|
| 01 | `01-idea` (steps `idea-1`, `idea-2`, `idea-3`) | slides | none |
| 02 | `02-category` | recording | Content Types list |
| 03 | `03-tags` | recording | Blog Post type editor |
| 04 | `04-edit` | recording | Blog list |
| 05 | `05-site` | recording | Blog list |

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, run on `http://localhost:5010`
  with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Auto Setup** (no setup form and no login form on camera): set these environment variables before the first start,
  with the names the Auto Setup documentation uses (`docs/reference/modules/AutoSetup/README.md`):
  `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
  `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
  `...__AdminPassword=<password>` (the password chosen outside git, the same value as `VIDEO_PASSWORD`),
  `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog`.
- **Recipe**: Blog. It enables the Taxonomies feature (`OrchardCore.Taxonomies`, `themes/TheBlogTheme/Recipes/blog.recipe.json:72`),
  sets TheBlogTheme as the site theme and TheAdmin as the admin theme (`blog.recipe.json` step `themes`), and creates two
  taxonomies: **Categories** (term Travel, term type Category, `blog.recipe.json:926-940`) and **Tags** (terms Earth,
  Exploration, Space, term type Tag, `blog.recipe.json:860-902`). Check at **Tools, Features** that Taxonomies is enabled
  (`OrchardCore.Features/AdminMenu.cs:26-27`). No other feature has to be enabled.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls out.
- **Sign-in**: `Recorder setup` once, as `admin` with `VIDEO_PASSWORD`; it saves `state.json` (never committed).
  The Administrator role holds the **Manage taxonomies** permission by default (`OrchardCore.Taxonomies/Permissions.cs:8,22`),
  which `edit-6` relies on.
- **Browser**: the recorder's Chromium (1536x864 CSS pixels, light color scheme, `en-US`).
- No external service is involved, so there is no local stand-in to run.

### The seed data (create it outside the capture, in this order)

The Blog recipe already gives Blog Post a Tags field and a Category field (`blog.recipe.json:644-672`). The video
adds them itself, so the seed removes them first and prepares the terms and the post they act on.

1. **Remove the two fields from Blog Post.** Open **Design, Content Definition, Content Types**, click **Edit** on
   Blog Post, and in the **Fields** list click **Remove** on **Tags**, then on **Category** (confirm with **Ok** if a
   confirmation appears). Each removal shows `The "<name>" field has been removed.`
   (`OrchardCore.ContentTypes/Controllers/AdminController.cs:952`). Result: the **Fields** list holds only **Subtitle** and
   **Banner Image**. Do not touch the Subtitle and Banner Image fields.
2. **Add two terms to the Categories taxonomy and publish it.** Open `/Admin/Contents/ContentItems/Taxonomy`
   (route `Contents/ContentItems/{contentTypeId?}`, `OrchardCore.Contents/Controllers/AdminController.cs:67`), click **Edit**
   on **Categories**.
   - On the **Travel** row click **Add** (`OrchardCore.Taxonomies/Views/Content.TermAdmin.cshtml:36`). In the term form, set
     **Title** to `Trains`; for **Icon** click the arrow button next to the icon (the toggle of the icon picker,
     `button.icon-picker-toggle`, `OrchardCore.ContentFields/Views/TextField-IconPicker.Edit.cshtml`) and click the picker item
     `.iconpicker-item[title*='fa-train']`. The picker has no search box (`inputSearch: false`), so scroll the list to it; the
     library lists it as `fas fa-train` (`OrchardCore.Resources/wwwroot/Scripts/fontawesome-iconpicker.js:4848`). Click **Publish**.
   - Back on the Categories editor, click **Add Category** at the top (`OrchardCore.Taxonomies/Views/TaxonomyPart.Edit.cshtml:48`).
     Set **Title** to `Food`; for **Icon** pick `.iconpicker-item[title*='fa-utensils']` the same way (`fas fa-utensils`,
     `fontawesome-iconpicker.js:5055`). Click **Publish**.
   - Back on the Categories editor click **Publish** (the taxonomy itself). Terms added from the admin go into a draft of
     the taxonomy (`OrchardCore.Taxonomies/Controllers/AdminController.cs`, `CreatePost`), and the front end reads the published one.
   - Check: the tree shows **Travel** with **Trains** indented under it, and **Food** at the top level.
3. **Create the post.** Click **Blog** in the admin menu (`blog.recipe.json:1197`), then **Create Blog Post**
   (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:52`). Set:
   - **Title**: `A Night Train Across the Alps`
   - **Subtitle**: `Seven hours, one window seat`
   - the body (click into the body editor and type): `The sleeper leaves Zurich at dusk and reaches Vienna before breakfast.`
   - leave **Banner Image** empty (the theme falls back to its default picture).

   Click **Publish**. There is no Category or Tags field to fill, because step 1 removed them. The post's address is
   `/blog/a-night-train-across-the-alps` (pattern at `blog.recipe.json:216`); confirm it by opening the post once.
4. **Check the Tags taxonomy**: Earth, Exploration and Space only. There must be no `Night Train` tag.
5. **Snapshot A.** Stop the site, copy its `App_Data` folder and `state.json` aside (to a folder outside both
   repositories), start the site again.

## Recording order, dependencies and snapshots

`02-category`, then `03-tags`, then `04-edit`, then `05-site`. Each clip starts from what the previous one left.

| Clip | What it changes on the site | Before a new take |
|---|---|---|
| `02-category` | Adds the Category field to Blog Post | Restore snapshot A |
| `03-tags` | Adds the Tags field to Blog Post | Restore snapshot A, then record `02-category` again, or restore a copy taken after `02-category` |
| `04-edit` | Creates the `Night Train` tag in the Tags taxonomy (stored at once), sets the post's terms, publishes the post | Restore snapshot B (below) |
| `05-site` | Nothing (it only reads) | Nothing, if `04-edit` is in its final state |

Take **snapshot B** after `03-tags` and before `04-edit`: stop the site, copy `App_Data` and `state.json` aside (outside both repositories), start it again. On a
retake of `04-edit` without that restore, the tag `Night Train` already exists, so Enter selects it instead of creating
it and `edit-6` shows something different.

For every clip: narrate first (`narrate.py`), because the recorder holds each step for the length of its narration.
Navigation stops the screencast unless it goes through `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` or
`NavigateAsync`, so every click that loads a page below uses one of them. Keep each step's actions shorter than its
narration (a step is about 2.6 words per second long); the steps most at risk are `category-3` and `tags-1`, where
the typing delay should be 30 ms and the clicks should not pause.

## Slides (no recording)

Drawn by `slides.py`; nothing to record.

- `idea-1`: three cards, "Taxonomy", "Terms" and "Taxonomy Field", each with one line.
- `idea-2`: two cards, "Standard editor" and "Tags editor".
- `idea-3`: three cards, "One taxonomy", "Rules for the selection" (Required, Unique and Leaves only) and "A display mode".

## Clip `02-category`

Starts on **`/Admin/ContentTypes/List`** (the **Content Types** page,
`OrchardCore.ContentTypes/Views/Admin/List.cshtml:6`), opened with `OpenAsync` before the capture. Expand **Design**, then
**Content Definition** in the admin menu before `StartCaptureAsync`, so **Content Types** is visible in the menu
(`OrchardCore.ContentTypes/AdminMenu.cs:23-25`). Collapse nothing else.

- `category-1`: `ClickAndWaitForNavigationAsync` on the **Edit** button of the Blog Post row,
  `a[role='btn-edit-BlogPost']` (`OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; the
  button label is "Edit"). The page that opens is `/Admin/ContentTypes/Edit/BlogPost`, titled **Edit Content Type - Blog
  Post** (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`).
  *On screen at the end:* the **Fields** heading (`Edit.cshtml:88`) with two rows, **Subtitle** and **Banner Image**, and the
  **Add Field** button below them (`Edit.cshtml:137`).
- `category-2`: `ClickAndWaitForNavigationAsync` on `a:has-text('Add Field')` (`Edit.cshtml:137`). On the form
  (`OrchardCore.ContentTypes/Views/Admin/AddFieldTo.cshtml`), `TypeAsync` into `input[data-displayname]` (label **Display
  Name**, line 38) the text `Category`, delay 30 ms, then press Tab. The **Technical Name** (`input[data-name]`, line 46) fills
  in by itself (`OrchardCore.ContentTypes/Assets/js/autocomplete.ts`).
  *On screen at the end:* **Display Name** `Category` and **Technical Name** `Category`, and the **Field Type** list
  (line 54) below them. The success message does not appear yet.
- `category-3`: `ScrollAsync(0, 500)` until the **Taxonomy Field** radio is visible in the **Field Type** list. The label
  "Taxonomy Field" is the field's name `TaxonomyField` shown through `CamelFriendly()` (`AddFieldTo.cshtml:62`), and the
  first radio of the list is selected by default (`AddFieldTo.cshtml`, `checked="@(i == 0)"`), so click the radio for
  Taxonomy Field explicitly: `input[type='radio'][value='TaxonomyField']`. Then `ClickAndWaitForNavigationAsync` on
  `button[value='SaveAndConfigure']`, labeled **Save & Configure** (`AddFieldTo.cshtml:72`).
  *On screen at the end:* the settings page titled `"Category" settings` (`OrchardCore.ContentTypes/Views/Admin/EditField.cshtml:30`),
  with the message `The field "Category" has been added.` (`OrchardCore.ContentTypes/Controllers/AdminController.cs:768`) and the
  first settings, **Placeholder** and **Hint**, on screen.
- `category-4`: `SelectAsync` on `select[name$='TaxonomyContentItemId']` (label **Taxonomy**, with the option "Select a
  Taxonomy" first, `OrchardCore.Taxonomies/Views/TaxonomyFieldSettings.Edit.cshtml:30-33`), value `Categories`. Then
  `TypeAsync` into `textarea[name$='Hint']` (label **Hint**, line 22) the text `Choose the one category that fits the post.`
  Leave **Placeholder** empty.
  *On screen at the end:* **Taxonomy** reads `Categories`; the **Hint** holds the typed sentence.
- `category-5`: `ClickAsync` on `input[type='checkbox'][name$='Unique']` (label **Unique**, `TaxonomyFieldSettings.Edit.cshtml:56`)
  and on `input[type='checkbox'][name$='LeavesOnly']` (label **Leaves only**, line 66). Leave
  `input[type='checkbox'][name$='Required']` (label **Required**, line 46) unchecked. The `type='checkbox'` part is needed:
  a checkbox bound with `asp-for` is followed, at the end of the form, by a hidden input of the same name, so
  `input[name$='Unique']` alone matches two elements and the recorder's strict locators fail on it.
  `HighlightAsync` each of the three labels in turn (`label.form-check-label:has-text('Required')`, then `'Unique'`, then
  `'Leaves only'`), then `ClearHighlightsAsync`.
  *On screen at the end:* **Required** unchecked, **Unique** and **Leaves only** checked, with their hints
  ("Whether only one element can be selected.", "Whether leaves are the only items that can be selected.").
- `category-6`: `ScrollAsync` down until the two questions are visible: **What type of editor should be used?**
  (`select[data-field-editor-select]`, `EditField.cshtml:79`) and **What type of display mode should be used?**
  (`select[data-field-display-select]`, `EditField.cshtml:100`). Both read **Standard** (`TaxonomyField.Option.cshtml:4`,
  `TaxonomyField.DisplayOption.cshtml:4`); change nothing. The narration reads the two labels one after the other:
  `HighlightAsync` the editor question for about 3.5 seconds, `ClearHighlightsAsync`, `HighlightAsync` the display mode
  question for about 3.5 seconds, `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` on
  `button.save[value='Save']` (**Save**, `EditField.cshtml:120`), in the last 2 seconds of the narration.
  *On screen at the end:* the Blog Post type editor with the message `The "Category" field settings have been saved.`
  (`AdminController.cs:908`).
- `category-7`: no click. `HighlightAsync` the **Category** row in the **Fields** list for the whole step.
  *On screen at the end:* the **Fields** list with **Subtitle**, **Banner Image** and **Category**, the last one followed by
  the grey text "Taxonomy Field" (`Edit.cshtml`, the `CamelFriendly()` of the field name) and with no badge.

## Clip `03-tags`

Starts on **`/Admin/ContentTypes/Edit/BlogPost`** (the Blog Post type editor with the Category field now listed), opened
with `OpenAsync` before the capture.

- `tags-1`: `ClickAndWaitForNavigationAsync` on `a:has-text('Add Field')`; `TypeAsync` the text `Tags` into
  `input[data-displayname]` (delay 30 ms); `ScrollAsync(0, 500)` until the Taxonomy Field radio is visible; `ClickAsync`
  on `input[type='radio'][value='TaxonomyField']`; `ClickAndWaitForNavigationAsync` on `button[value='SaveAndConfigure']`.
  *On screen at the end:* the settings page titled `"Tags" settings` with the message `The field "Tags" has been added.`
  The Technical Name was `Tags`: the theme's templates look the field up by this exact name, so check that it reads `Tags`
  before saving.
- `tags-2`: `SelectAsync` on `select[name$='TaxonomyContentItemId']`, value `Tags`. Leave the three checkboxes as they are
  (all unchecked on a new field) and `HighlightAsync` their three labels in turn (`label.form-check-label:has-text('Required')`,
  then `'Unique'`, then `'Leaves only'`), about 1.5 seconds each, at the moment the narration reads "Required, Unique and
  Leaves only stay off"; `ClearHighlightsAsync`. Then `HighlightAsync` the empty **Placeholder** input
  (`TaxonomyFieldSettings.Edit.cshtml:14`) for the last 5 seconds of the step, then `ClearHighlightsAsync`. Do not click any
  checkbox: with **Unique** off the Tags editor allows several tags (the editor passes `:multiple="${!e.unique}"`,
  `OrchardCore.Taxonomies/wwwroot/Scripts/taxonomy-field-tags/taxonomy-field-tags.js`), which `edit-4` to `edit-7` rely on.
  *On screen at the end:* **Taxonomy** reads `Tags`; **Required**, **Unique** and **Leaves only** unchecked; **Placeholder**
  is empty and ringed.
- `tags-3`: `ScrollAsync` until both the **Leaves only** row and **What type of editor should be used?** are on screen,
  then `SelectAsync` on `select[data-field-editor-select]`, value `Tags` (the option is labeled **Tags**,
  `TaxonomyField-Tags.Option.cshtml:4`). The extra setting **Open** appears (`OrchardCore.Taxonomies/Views/TaxonomyFieldTagsEditorSettings.Edit.cshtml:7`).
  It is not inside the box of the editor question: its driver puts it in the `Content` zone, right after the settings of the
  field (`TaxonomyFieldTagsEditorSettingsDriver.cs`, `Startup.cs:66`, rendered by `EditField.cshtml:74`), so **Open** appears
  between **Leaves only** and the editor question, and pushes the question down. It is hidden until the editor is set to Tags
  by the script of `OrchardCore.ContentTypes/Assets/js/edit-field-selector.ts` (it hides every `.field-editor` element, then
  shows `.field-editor-tags`). Keep the question in the lower half of the screen before the select, so that **Open** appearing
  above it is in the frame.
  *On screen at the end:* the editor question reads **Tags** and the **Open** checkbox is visible above it.
- `tags-4`: `HighlightAsync` the **Open** checkbox (`input[type='checkbox'][name$='Open']`, and its label
  `label.form-check-label:has-text('Open')`) and its hint "Whether new tags can be created inline. Note: When
  creating tags inline only the display text term property will be set." (`TaxonomyFieldTagsEditorSettings.Edit.cshtml:8`).
  It is checked by default (`OrchardCore.Taxonomies/Settings/TaxonomyFieldTagsEditorSettings.cs:14`); if it is not, click it
  at the start of the step. Do not uncheck it.
  *On screen at the end:* **Open** checked and ringed, with its hint readable.
- `tags-5`: `ClearHighlightsAsync`; `ScrollAsync` to **What type of display mode should be used?**
  (`select[data-field-display-select]`), `SelectAsync` value `Tags` (**Tags**, `TaxonomyField-Tags.DisplayOption.cshtml:4`);
  then `ClickAndWaitForNavigationAsync` on `button.save[value='Save']`, in the last 2 seconds of the narration.
  *On screen at the end:* the Blog Post type editor with the message `The "Tags" field settings have been saved.`
- `tags-6`: no click. `HighlightAsync` the two badges of the **Tags** row for the first half of the step (the display-mode
  badge with the tv icon and the editor badge with the pen icon, both reading `Tags`, `Edit.cshtml:119,123`), then
  `ClearHighlightsAsync` and `HighlightAsync` the **Category** row, which has no badge.
  *On screen at the end:* the **Fields** list: **Subtitle**, **Banner Image**, **Category** (no badge), **Tags** (two badges).

## Clip `04-edit`

Starts on the **Blog list**: click **Blog** in the admin menu once before the capture (`blog.recipe.json:1197`), note its
address (`/Admin/Contents/ContentItems/<blog id>/Display`) and open it with `OpenAsync`. The list has the columns Title, Type,
Status and Last modified (`OrchardCore.Lists/Views/ListPartDetailAdmin.cshtml`) and a row for **A Night Train Across the Alps**
(and the recipe's own post). Leave the admin menu as it is, with the **Blog** entry visible at the top level: the narration of
`edit-1` says that the list comes from the Blog menu, so the entry must be on screen.

- `edit-1`: `HighlightAsync` the **Blog** entry of the admin menu for the first sentence of the narration
  (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "Blog", Exact = true }).First`), then `ClearHighlightsAsync`; then
  `ClickAndWaitForNavigationAsync` on the **Edit** button of the row of `A Night Train Across the Alps`:
  `scene.Page.Locator(".list-group-item").Filter(new() { HasText = "A Night Train Across the Alps" }).Locator("a.edit")`
  (the admin lists use the List layout unless the site is configured otherwise, `AdminListOptions.DefaultLayout`, whose
  rows are `list-group-item` elements; the buttons of a row are shown as buttons, `DefaultActionsLayout`, so no menu has
  to be opened; the button is labeled "Edit", `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`).
  *On screen at the end:* the editor titled **Edit Blog Post** (`OrchardCore.Contents/Views/Admin/Edit.cshtml:39`) with the
  **Title** `A Night Train Across the Alps` at the top.
- `edit-2`: `ScrollAsync` until the **Category** field fills the screen. The fields come after the body, in the order
  Subtitle, Banner Image, Category, Tags (new fields take the next position, `OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:494`).
  *On screen at the end:* the label **Category**, the hint sentence from `category-4`, and three radio buttons
  (radio because **Unique** is on, `OrchardCore.Taxonomies/Views/TaxonomyField.Edit.cshtml:72`): **Travel**, **Trains**
  indented under it (`TaxonomyField.Edit.cshtml:38`), and **Food**. None is selected.
- `edit-3`: `HighlightAsync` the **Travel** radio, which is disabled (`TaxonomyField.Edit.cshtml:83`, because **Leaves only** is
  on and Travel has a child); after about 3 seconds `ClearHighlightsAsync` and `ClickAsync` the label **Trains**
  (`label.form-check-label:has-text('Trains')`).
  *On screen at the end:* the **Trains** radio selected; **Travel** disabled; **Food** unselected.
- `edit-4`: `ScrollAsync` until the **Tags** field is visible: an empty box with the placeholder `Type to search`
  (`OrchardCore.Taxonomies/Services/TaxonomiesJSLocalizer.cs:16`, shown because the **Placeholder** setting is empty). Pause 2
  seconds on the empty box. `ClickAsync` the placeholder `.tags .multiselect__placeholder` (a click on it opens the list and
  puts the cursor in the search field), then `TypeAsync` into `.tags input.multiselect__input` the text `Expl` (delay 60 ms).
  The list narrows to **Exploration**, followed by a second entry `Expl` that offers to create a tag with that name (the
  editor adds such an entry for any typed text that is not the exact name of a tag, library file
  `OrchardCore.Resources/wwwroot/Scripts/vue-multiselect.js`, `filteredOptions`); click the **Exploration** entry
  (`.tags .multiselect__option:has-text('Exploration')`), not the `Expl` one.
  *On screen at the end:* a chip `Exploration` inside the box. The list is still open and the search field still has the
  cursor: the editor passes `close-on-select` equal to the **Unique** setting, which is off for this field
  (`OrchardCore.Taxonomies/wwwroot/Scripts/taxonomy-field-tags/taxonomy-field-tags.js`, the `:close-on-select` binding), so a
  selection does not close it.
- `edit-5`: `TypeAsync` into `.tags input.multiselect__input` the text `Night Train` (delay 60 ms). The list is still open
  from `edit-4`, and a click on the search field itself leaves it open (only the arrow and the placeholder toggle it). Do
  not press Enter yet.
  *On screen at the end:* the typed text `Night Train` in the box and, in the open list, a single entry `Night Train` that
  creates the tag; it is the highlighted entry, so it carries the hint `Press enter to create a tag`
  (`TaxonomiesJSLocalizer.cs:20`, drawn by the library's `data-select` attribute on the highlighted option; the entry exists
  because **Open** is on and the user may manage taxonomies, `OrchardCore.Taxonomies/Views/TaxonomyField-Tags.Edit.cshtml:22`).
- `edit-6`: `scene.Page.Keyboard.PressAsync("Enter")`, then wait until `.tags .multiselect__tag:has-text('Night Train')` is
  visible (the creation is a POST to `Tag/Create`, `OrchardCore.Taxonomies/Controllers/TagController.cs`; the chip is added
  from its response, so the wait ends when the chip shows).
  *On screen at the end:* two chips in the box, `Exploration` and `Night Train`, with the typed text gone.
- `edit-7`: `ScrollAsync` to the bottom of the editor; `ClickAndWaitForNavigationAsync` on the **Publish** button
  (`button.publish`, `OrchardCore.Contents/Views/Content.PublishButton.cshtml:19`). It returns to the Blog list.
  *On screen at the end:* the Blog list with the message `Your Blog Post has been published.`
  (`OrchardCore.Contents/Controllers/AdminController.cs:967`) and the row `A Night Train Across the Alps`.

## Clip `05-site`

Starts on the **Blog list** (the same address as `04-edit`), opened with `OpenAsync` before the capture.

- `site-1`: click the **View** button of the row of `A Night Train Across the Alps`
  (`scene.Page.Locator(".list-group-item").Filter(new() { HasText = "A Night Train Across the Alps" }).Locator("a.view")`,
  labeled "View", `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`). It has `target="_blank"`, so the
  post opens in a new tab: wrap the click in `scene.Page.Context.RunAndWaitForPageAsync(...)` to catch the new page, then
  call `SwitchToAsync` on it.
  *On screen at the end:* the post on the site (`/blog/a-night-train-across-the-alps`), with the heading
  `A Night Train Across the Alps` and the subtitle `Seven hours, one window seat`.
- `site-2`: `ScrollAsync` down past the body text until the badges are visible. Do not click a badge.
  *On screen at the end:* below the body text, one badge for the category `Trains` (with its icon) and badges for the tags
  `Exploration` and `Night Train`. These come from the theme's templates
  (`OrchardCore.Themes/TheBlogTheme/Views/Content-BlogPost.liquid:31-32`, `BlogPost-Category.liquid`,
  `BlogPost-Tags-TaxonomyField-Tags.Display.liquid`).

## Checks for private data

- The only account is `admin` with `admin@example.com`; the post shows "Posted by admin". No other user exists.
- The only names are the seed data above (`A Night Train Across the Alps`, `Zurich`, `Vienna`, `Trains`, `Food`,
  `Night Train`) and the recipe's own content. No real person, company, address, phone number or token is typed anywhere.
- The password is never typed on camera (the sign-in is done by `Recorder setup`), and `<password>` is a placeholder only.
- The address bar is not part of the frame; the host is `localhost`. No path of this machine appears on screen.
- After assembling, OCR the video at one frame per second and at every scene change, then at 5 frames per second, and
  search for any email address other than `admin@example.com`, any name not listed above, and any token.
