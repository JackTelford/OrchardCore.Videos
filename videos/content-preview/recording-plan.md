# Recording plan: Content Preview

Everything a person needs to record this video without deciding anything. The narration is in `storyboard.json`; run
`narrate.py` first, since the recorder holds each step for the length of its narration. The labels below were read
from the Orchard Core source at commit 92db4a83 (paths are relative to the root of that checkout); a label that a
running site spells differently is a mistake in this plan, so write it in OPEN-QUESTIONS.md.

The video has seven chapters. Chapters 01 and 05 are slides, drawn by `slides.py`; chapters 00 and 99 are cards. Only
chapters 02, 03 and 04 are recordings, and all three are browser recordings.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore` on `main`, on
  `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Auto Setup** (see the `orchardcore-tester` skill, `references/autosetup.md`): recipe **Blog**, site name
  **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>`
  (the value of `VIDEO_PASSWORD`, chosen outside git, never written in this repository). No setup screen and no login
  form is recorded. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address so the site never calls an outside service.
- **Themes**: the Blog recipe sets the site theme to `TheBlogTheme` and the admin theme to `TheAdmin`. Leave both.
  Leave the admin setting for the layout of actions at its default, so the **Preview** button is in the action bar
  and not behind the ellipsis menu (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/actionBar.ts`).
- **Sign-in**: run `Recorder setup` once (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD`), which saves `state.json`
  (git-ignored). Every browser context of a clip opens signed in, and so does every tab a clip opens.
- **Features**: the Blog recipe enables **Content Preview** (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`,
  the `feature` step). Clip `02-enable` records the enabling, so before recording anything, disable it: Tools,
  Features, search "Content Preview", **Disable**, **Yes** in the confirmation. Nothing else depends on it. Clip
  `03-live` needs it enabled, which `02-enable` does. Clip `04-draft` does not: the **Preview Draft** button belongs
  to the Contents module (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:21-23`, the page
  `OrchardCore.Contents/Controllers/ItemController.cs`), so it is on Article 2 while the feature is disabled.
- **External services**: none. No mail, SMS, search or payment service is used.
- **No mask script**: the site is made from scratch, with fake data only.

### The fake data to create outside the capture

Create these with the admin UI (Content, Content Items, **New**, **Article**) or with `fetch` calls in a page, before
the first take. Use the **Article** type: it is listable and draftable in the Blog recipe, so it appears in the list
and gets a **Preview Draft** button. The **Blog Post** type is not listable and never appears in the list.

| | Article 1 | Article 2 |
|---|---|---|
| Title | `Autumn Open Day` | `Winter Workshop Series` |
| Subtitle | `Tours, seeds and warm cider` | `Pruning and planting in the greenhouse` |
| Body | `Join us on Saturday at the greenhouse for guided tours, a seed swap and warm cider. Doors open at ten in the morning, and entry is free.` | `Each workshop runs for two hours in the heated greenhouse. Bring gloves and a notebook.` |
| State | Published, no draft | Published, then a draft |

1. Create Article 1 and select **Publish**.
2. Create Article 2 and select **Publish**.
3. Open Article 2 again (**Edit** in the list), replace the Subtitle with `Pruning, planting and evening sessions`,
   and select **Save Draft**. The list now shows **Published** and **Draft** badges on Article 2 and a **Preview
   Draft** button.
4. Leave Article 1 without a draft: clip `03-live` edits it, and its editor must open on the published values.

The Blog recipe also creates an **Article** named About, a **Blog** named Blog, the **Taxonomy** items Tags and
Categories, and a blog post. The first four appear in the list (generic placeholder text, no names): Taxonomy is
listable (`OrchardCore.Taxonomies/Migrations.cs:27`). The blog post is not listable and stays off camera. With the two
fake articles, the list holds six rows.

Stop the site, copy its `App_Data` folder aside (call it **S0**: Content Preview disabled, both articles in place),
and start the site again.

## Order of the clips

1. `02-enable`: starts from S0. It changes the site (Content Preview becomes enabled).
2. `03-live`: needs Content Preview enabled, so it follows `02-enable`. It changes nothing, as long as **Publish**
   and **Save Draft** are never selected.
3. `04-draft`: needs Article 2's draft, and nothing else: it works with Content Preview disabled or enabled, so it
   can also be recorded from S0. It changes nothing.

`03-live` and `04-draft` do not depend on each other. To record `02-enable` again, stop the site, restore S0, and
start it again. To record `03-live` or `04-draft` again, nothing needs restoring.

Record on a viewport of 1536 by 864 CSS pixels, as the recorder does. Each clip opens before the capture starts, so
the first frame is the page already loaded.

## Clip 02-enable (chapter 02, Enable Content Preview)

- **Starts on**: `/Admin/Features`, signed in, Content Preview disabled. Nothing is typed in the search box.
- **Locators**: the search box is `#search-box` (placeholder "Search"); the row of the feature is the
  `.list-group-item` that holds `#btn-enable-OrchardCore_ContentPreview` before it is enabled, and
  `#btn-disable-OrchardCore_ContentPreview` afterwards.

`enable-1`
- Actions: `scene.TypeAsync(scene.Page.Locator("#search-box"), "Content Preview", delay: 70)`. Then
  `scene.HighlightAsync(row, "Content Preview")`, where `row` is the row of the feature, which also puts the
  "Content Management" heading above it in view.
- On screen when the narration ends: the Features page, the search box holding "Content Preview", the heading
  "Content Management", and the Content Preview row with its **Enable** button, highlighted. No other category is
  visible.
- Labels: the admin menu path Tools, Features comes from `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`;
  the placeholder "Search" from `OrchardCore.Features/Views/Admin/Features.cshtml:27`; the category heading from
  `Features.cshtml:88` and the feature's category "Content Management" from
  `src/OrchardCore.Modules/OrchardCore.ContentPreview/Manifest.cs:10`; the name "Content Preview" from
  `Manifest.cs:4`.

`enable-2`
- Actions: `scene.ClearHighlightsAsync()`, then `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btn-enable-OrchardCore_ContentPreview"))`.
  The link is marked `data-url-af="UnsafeUrl"`, so the admin script posts it (the action is `[HttpPost]`,
  `OrchardCore.Features/Controllers/AdminController.cs:225-227`; `TheAdmin.ts:118`) to
  `/Admin/Features/OrchardCore.ContentPreview/Enable`, and the action redirects to `/Admin/Features`. Wait
  until the notification "The feature Content Preview was enabled." is on the page. The page reloads with an empty
  search box, so type "Content Preview" in it again (`scene.TypeAsync`, delay 70) and highlight the row, which now
  shows a **Disable** button and the badge "Enabled".
- On screen when the narration ends: the notification "The feature Content Preview was enabled." at the top of the
  page, the search box holding "Content Preview", and the row of the feature with the badge "Enabled" and the
  **Disable** button. If the admin theme removes the notification before the narration ends, say so in
  OPEN-QUESTIONS.md: the frame at the end of the step then shows the row without the message.
- Labels: **Enable** from `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the
  notification from `OrchardCore.Features/Controllers/AdminController.cs:317`; the badge "Enabled" from
  `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.

## Clip 03-live (chapter 03, Preview while you edit)

Two browser pages are involved: the editor, and the preview window that **Preview** opens (`target="_blank"`, so a
new page of the same context). The recorder captures one page at a time, so each step names the page it captures.
Keep a reference to both:

```
var list = scene.Page;                                  // the content items list, then the editor, in one tab
var preview = await scene.Context.RunAndWaitForPageAsync(() => scene.ClickAsync(scene.Page.Locator("#previewButton")));
await scene.SwitchToAsync(preview);                     // capture the preview window
await scene.SwitchToAsync(list);                        // capture the editor tab again
```

Both pages are in one context, so the editor and the preview window share the browser storage and the
`BroadcastChannel` they talk through. Do not close either page until the clip ends.

- **Starts on**: `/Admin/Contents/ContentItems`, signed in. The list shows Article 1 ("Autumn Open Day") and Article 2.
- **Wait condition for the preview window** (use it inside `scene.OffCameraAsync`, so a slow first render is cut out):
  the preview window holds two iframes, `#iframe-a` and `#iframe-b`, and the one on top has `style.zIndex` of
  `999990`. Wait with `preview.WaitForFunctionAsync` until that iframe's `contentDocument` has an `h1` (and, for the
  Subtitle, a `.subheading`) with the expected text. Allow up to 15 seconds: the editor sends the form 500 ms after
  the last key, and a background tab can run its timers late.

`live-1`
- Actions: `scene.ClickAndWaitForNavigationAsync(` the **Edit** button of the row that holds "Autumn Open Day" `)`,
  that is `scene.Page.Locator("li.list-group-item, tr").Filter(new() { HasText = "Autumn Open Day" }).Locator("a.edit")`.
  Wait for `#previewButton` to be visible. Then `scene.HighlightAsync(scene.Page.Locator("#previewButton"), "Preview")`.
- On screen when the narration ends: the editor "Edit Article" with the Title "Autumn Open Day" and the action bar at
  the bottom with the **Preview** button highlighted (a blue-green "info" button). Because the editor was opened from
  the list (it carries `returnUrl`) and Article 1 is published, the bar reads **Publish**, **Unpublish** and **Save
  Draft** (each with a drop-down arrow), **Delete**, **Preview**, **Cancel**
  (`ContentsDriver.cs:110-119`, `ContentPreviewDriver.cs:14`, `Content.Edit.cshtml:30`).
- Labels: the menu path Content, Content Items from `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65-68`;
  **Edit** from `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`; the page title "Edit Article"
  from `OrchardCore.Contents/Views/Admin/Edit.cshtml:39` (`T["Edit {0}"]`); **Preview** from
  `OrchardCore.ContentPreview/Views/ContentPreview.Button.cshtml:13`, placed after the other actions by
  `OrchardCore.ContentPreview/Drivers/ContentPreviewDriver.cs:14`.

`live-2`
- Actions: `scene.ClearHighlightsAsync()`, then click **Preview** inside `RunAndWaitForPageAsync` (above). Call
  `scene.SwitchToAsync(preview)`, and inside `scene.OffCameraAsync` wait until the top iframe shows the `h1`
  "Autumn Open Day" and the `.subheading` "Tours, seeds and warm cider".
- On screen when the narration ends: the preview window with the article heading "Autumn Open Day" and the subtitle
  "Tours, seeds and warm cider". No "Preview Disconnected" message, no error.
- Labels: as in `live-1`. The preview window is `OrchardCore.ContentPreview/Views/Preview/Index.cshtml`.

`live-3`
- Actions: `scene.SwitchToAsync(list)` (the editor). Click the Title field, press `Control+A`, and type
  `Autumn Open Day: Free Entry` with `PressSequentiallyAsync` (delay 45), so the typed text replaces the title. The
  Title is `scene.Page.GetByLabel("Title", new() { Exact = true })`. The Subtitle field comes after the body in the
  editor (the parts sit in the order Title, Permalink, body, then the Article part with Subtitle and Banner Image, as
  the Position values of the Article type in `blog.recipe.json` give), so scroll down with `scene.ScrollAsync(0, 400)`,
  repeated until the Subtitle field (`GetByLabel("Subtitle", new() { Exact = true })`) is in the middle of the screen
  and clear of the action bar. Click it, press `Control+A`, and type `Tours, seed swaps and warm cider`. Then scroll
  back to the top by the same total. Clear any highlight before scrolling. Do not touch the body.
- On screen when the narration ends: the editor scrolled to the top, with the Title field holding "Autumn Open Day:
  Free Entry" and the action bar at the bottom. The Subtitle change is visible in the preview in `live-4`.
- Labels: **Title** from `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; **Subtitle** is the
  display name of the `Subtitle` text field of the Article part in the Blog recipe; both inputs carry the class
  `content-preview-text` (`TitlePart.Edit.cshtml:14`, `OrchardCore.ContentFields/Views/TextField.Edit.cshtml:22`),
  which sends an update to the preview. Only these two fields are edited: whether the body's editor updates the
  preview is not asserted (see OPEN-QUESTIONS.md).

`live-4`
- Actions: `scene.SwitchToAsync(preview)`, and inside `scene.OffCameraAsync` wait until the top iframe shows the `h1`
  "Autumn Open Day: Free Entry" and the `.subheading` "Tours, seed swaps and warm cider".
- On screen when the narration ends: the preview window with the heading "Autumn Open Day: Free Entry" and the
  subtitle "Tours, seed swaps and warm cider".
- Labels: none beyond those above.

`live-5`
- Actions: `scene.SwitchToAsync(list)`, scroll back to the bottom if needed, and click **Cancel**:
  `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.cancel"))`. The editor was opened from the list, so it
  has a **Cancel** button that returns to the list. Highlight the row "Autumn Open Day".
- On screen when the narration ends: the content items list, the row "Autumn Open Day" (the original title) with its
  badge "Published" and no "Draft" badge.
- Labels: **Cancel** from `OrchardCore.Contents/Views/Content.Edit.cshtml:30`; the badge from
  `OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:12`.

`live-6`
- Actions: `scene.ClearHighlightsAsync()`, `scene.SwitchToAsync(preview)`, and inside `scene.OffCameraAsync` wait
  until `#notConnectedWarning` has `display: block`.
- On screen when the narration ends: the preview window, with the alert "Preview Disconnected" at the top right over
  the last rendered page ("Autumn Open Day: Free Entry"). The editor sends that message when its page is hidden or
  left (the `pagehide` event), which **Cancel** does by loading the list.
- Labels: "Preview Disconnected" from `OrchardCore.ContentPreview/Views/Preview/Index.cshtml:11`; the message is posted
  by `OrchardCore.ContentPreview/Assets/js/contentpreview.edit.js:122` and shown by
  `OrchardCore.ContentPreview/Assets/ts/preview-swap.ts:82`. The sentence "Opening the article in the editor again
  reconnects the window" is not recorded. It follows the messages of the code: the editor posts `hello` when its page
  loads (`contentpreview.edit.js:52`), an open window answers `ready` (`preview-swap.ts:54-57`), the editor then sends
  the form (`contentpreview.edit.js:43-44`), and the `token` message hides the alert (`preview-swap.ts:59-62`).
  Confirm it by hand (OPEN-QUESTIONS.md).

## Clip 04-draft (chapter 04, Preview a saved draft)

Again two pages: the list, and the tab that a button opens. Use the same pattern: `scene.Context.RunAndWaitForPageAsync`
around a click, then `scene.SwitchToAsync(tab)`; return with `scene.SwitchToAsync(list)`.

- **Starts on**: `/Admin/Contents/ContentItems`, signed in, with both rows "Winter Workshop Series" and "Autumn Open Day"
  visible together without scrolling (the other rows are About, Blog, Tags and Categories; the list's sort order is not
  assumed). No
  filter, nothing typed in the search box.
- **Row locator**: `scene.Page.Locator("li.list-group-item, tr").Filter(new() { HasText = "<title>" })`.

`draft-1`
- Actions: highlight the badge "Draft" in the row "Winter Workshop Series" (`scene.HighlightAsync(row.GetByText("Draft", new() { Exact = true }))`)
  and its **Preview Draft** button (`row.Locator("a.preview")`). Then clear the highlights, and highlight the buttons of
  the row "Autumn Open Day" (`row.Locator("a.edit")` and `row.Locator("a.view")`) to show that it has no **Preview Draft**.
- On screen when the narration ends: both rows. "Winter Workshop Series" shows the badges "Published" and "Draft" and
  the buttons **Edit**, **View** and **Preview Draft**; "Autumn Open Day" shows "Published" and the buttons **Edit**
  and **View**, with no **Preview Draft**. Both rows also have the **Actions** drop-down
  (`OrchardCore.Admin/Views/AdminListActions.cshtml`), which stays closed.
- Labels: **Preview Draft** from `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:23`, shown only
  when the item has a draft and the user may preview it (line 21); **Draft** and **Published** from
  `OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:12,19`.

`draft-2`
- Actions: `scene.ClearHighlightsAsync()`, click `row.Locator("a.preview")` of "Winter Workshop Series" inside
  `RunAndWaitForPageAsync`, `scene.SwitchToAsync(tab)`, and inside `scene.OffCameraAsync` wait until the page has a
  `.subheading` with the text "Pruning, planting and evening sessions".
- On screen when the narration ends: a page of the site (theme `TheBlogTheme`) with the heading "Winter Workshop
  Series" and the subtitle "Pruning, planting and evening sessions", which is the draft's subtitle.
- Labels: the page is `/Contents/ContentItems/<id>/Preview`
  (`src/OrchardCore.Modules/OrchardCore.Contents/Startup.cs:227-232`,
  `OrchardCore.Contents/Controllers/ItemController.cs:44-69`, which loads the latest version).

`draft-3`
- Actions: `scene.SwitchToAsync(list)`, click `row.Locator("a.view")` of "Winter Workshop Series" inside
  `RunAndWaitForPageAsync`, `scene.SwitchToAsync(tab2)`, and inside `scene.OffCameraAsync` wait until the page has a
  `.subheading` with the text "Pruning and planting in the greenhouse".
- On screen when the narration ends: the published page, with the heading "Winter Workshop Series" and the earlier
  subtitle "Pruning and planting in the greenhouse".
- Labels: **View** from `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`.

## The slides

`slides.py` draws these. Each slide has the heading in the header's chapter title and a green label "CHAPTER N".

- `ways-1`: "Check a change before you publish", two cards: "The editor shows fields" and "Content Preview shows the page".
- `ways-2`: "Two ways to preview", two cards: "Preview" (in the editor) and "Preview Draft" (in the content items list,
  which works without the Content Preview feature).
- `who-1`: "Who can preview", four cards: "Preview content", "Edit content for others", "Publish or unpublish content for
  others", "Access Control, Roles". The permission labels come from
  `src/OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:17,21,33`; the menu path from
  `src/OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19`.
- `limits-1`: "Limits of live preview", three cards; the first and third are bright, the second dimmed.
- `part-1`: "Preview through a page of your site", two cards: "Edit the content type" (Design, Content Definition, Content
  Types; `OrchardCore.ContentTypes/AdminMenu.cs:23-25`) and "Add Parts" (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:201`),
  where the part is listed as "Preview" (its name without the suffix "Part": `EditPartViewModel` takes
  `ContentPartDefinition.DisplayName()`, `ContentPartSettingsExtensions.cs:54-60`).
- `part-2`: "How the Preview part works", three cards: "Preview part", "Pattern" (the label comes from
  `OrchardCore.ContentPreview/Views/PreviewPartSettings.Edit.cshtml:14`), and "Preview button".

The Pattern setting holds a Liquid expression. The video does not show it, because an admin video shows no Liquid.
Do not record a clip for chapter 05.

## Checks for private data

- Every title, subtitle and body on screen is from the table above, or is the Blog recipe's generic placeholder text.
- The user is `admin` with the address `admin@example.com`; the list shows "admin" as the author. No other user exists.
- No password, token or `state.json` appears on screen. The sign-in is saved before the capture starts.
- The tabs opened by the clips show only site pages. The recorder captures page content, not the browser's address bar.
- OCR the finished video at one frame per second and at every scene change, as the skill asks, and search for any
  name, email address or number that is not in this plan.
