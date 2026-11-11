# Recording plan: Content Items: Create, Edit and Publish

This plan is for the person who records the video. Read it from top to bottom once, then follow it clip by clip. Every
label below is the text a reader sees on screen; the source file and line that spells it is given in the
"Source" lines (paths are relative to the `OrchardCMS/OrchardCore` checkout, at commit `92db4a83`, the one this plan
was verified against). Nothing in this plan needs a decision from you. When the site does not match a "visible at
the end" line, stop and write what you saw in `OPEN-QUESTIONS.md` instead of improvising.

The video has five recorded clips (`02-list`, `03-create`, `04-view`, `05-edit`, `06-remove`) and five slide steps
(`01-idea`). The opening card (`00-intro`) and the closing card (`99-outro`) are drawn from the storyboard.

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore` (the commit above, or a later one that still matches this
  plan), running `src/OrchardCore.Cms.Web` on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data`
  folder outside both repositories (`ORCHARD_APP_DATA`). Follow `references/recording.md` of the skill for the
  recorder.
- **Provisioning**: Auto Setup through environment variables, so no setup or login form is recorded. The Default tenant:

  | Variable | Value |
  | --- | --- |
  | `OrchardCore__AutoSetup__AutoSetupPath` | empty |
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`, chosen outside git; the same value as `VIDEO_PASSWORD` |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

  With `AutoSetupPath` empty, the site is installed by the first request to any of its pages (open `/` once and wait
  until it answers). Source: the Auto Setup module documentation (`src/docs/reference/modules/AutoSetup/README.md`); the web project
  already registers the feature (`src/OrchardCore.Cms.Web/Program.cs:9`), so the web project needs no change; the
  recipe is named `Blog` at `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:2`.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address so the site never calls out.
- **Features to enable before recording**: none. The Blog recipe enables everything the clips use: Contents, Title,
  Autoroute, Html, Content Preview, Admin Menu, Taxonomies, Media, and the themes `TheAdmin` (admin) and
  `TheBlogTheme` (site) (`blog.recipe.json:32-81`, `:83-86`). Do not enable Audit Trail or any other feature: the
  clips show the screens of the Blog recipe as they are.
- **Settings**: change none. The clips rely on the defaults of the admin settings: no "New" section in the admin menu
  (`DisplayNewMenu` is off, `src/OrchardCore/OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:18`), the List
  layout of admin lists, and the Buttons layout of row actions
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`).
- **External services**: none, and no local stand-ins.
- **Seed data**: none beyond what the Blog recipe creates. Nothing is created outside the capture. Everything the clips
  create is typed on camera with these fake values:

  | Where | Value |
  | --- | --- |
  | Title | `Spring Garden Checklist` |
  | Subtitle | `Jobs for the first warm weekend` |
  | Body, first take | `Clear the beds, check the tools, and sort the seed packets.` |
  | Body, added in `05-edit` | ` Order compost early.` (starts with a space) |

- **Before the first clip**: sign in once outside the capture and save the session (`Recorder setup`, with
  `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`). The session is `state.json` in the video's folder; it
  is ignored by git and never committed.
- **What the list holds at the start** (the content of the Blog recipe): five rows, the Article `About`, the Blog
  `Blog`, the Taxonomy items `Tags` and `Categories`, and the Menu `Main Menu`, all published, all by `admin`. The list
  shows the items of every listable content type: Article, Blog, Page and Taxonomy are listable
  (`blog.recipe.json:139,251,451`; `src/OrchardCore.Modules/OrchardCore.Taxonomies/Migrations.cs:27`) and so is Menu
  (`src/OrchardCore.Modules/OrchardCore.Menu/Migrations/menu.recipe.json:11`, applied by the Menu migration,
  `OrchardCore.Menu/Migrations.cs:26`). The Blog Post of the recipe and the Footer widget are not listed, because the
  `BlogPost` and `RawHtml` types are not listable (`blog.recipe.json`: neither has `Listable`). Open
  `/Admin/Contents/ContentItems` once before the first take and compare; if the rows differ, write it in
  `OPEN-QUESTIONS.md`.

## Order of the clips, and what changes the site

Record in the order the clips play. Each clip starts from the state the previous one left.

| Clip | Starts on | Needs | Changes the site? | Snapshot to restore before a new take |
| --- | --- | --- | --- | --- |
| `02-list` | `/Admin` | the fresh site | no | none (restore S0 if a new item was added by mistake) |
| `03-create` | `/Admin/Contents/ContentItems` | the fresh site | yes: creates and publishes the Article `Spring Garden Checklist` | S0 |
| `04-view` | `/Admin/Contents/ContentItems` | the Article published by `03-create` | no | S1 |
| `05-edit` | `/Admin/Contents/ContentItems` | the Article published by `03-create`, unchanged | yes: publishes the Article with a longer body | S1 |
| `06-remove` | `/Admin/Contents/ContentItems` | the Article published by `05-edit` | yes: unpublishes, then deletes the Article | S2 |

Snapshots: stop the site, copy its `App_Data` folder (and keep `state.json`), start the site again.

- **S0**: right after Auto Setup and the sign-in, before `02-list`.
- **S1**: right after `03-create`, when its take is accepted.
- **S2**: right after `05-edit`, when its take is accepted.

To record again, stop the site, restore the snapshot from the table, and start it again. `04-view` does not change
the site, so it can be taken again at once.

## The recorder's settings for every clip

- Chromium 1536x864 CSS pixels, scale 1.25, light color scheme, `en-US`, as the skill specifies.
- Keep every step's actions shorter than its narration. Where a step lists typing, use a typing delay of 30 ms.
- Clear highlights before scrolling or navigating (they are fixed-position).
- The bar of buttons at the bottom of an editor is fixed to the bottom of the window
  (`src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_action-bar.scss`, `.action-bar-bottom`), so it is always on
  screen and no step needs to scroll for it. With the default Buttons layout of the actions, **Preview**, **Unpublish**
  and **Delete** are buttons in that bar; `actionBar.ts` moves them into a menu only for the Menu layout and on small
  screens.
- The **and continue** choice is remembered per browser: after it is used once, the main button of that pair reads
  **Publish and continue** or **Save Draft and continue**
  (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/actionGroup.ts`). Never choose it. Before each take, check that the
  buttons read **Publish** and **Save Draft**; if they do not, use a browser profile that has not stored the admin
  preferences.
- The success messages of the site are toasts, fixed to the right edge of the window, that disappear after 5 seconds
  (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml`).
  A step that says "a message confirms" must start its click within the first 1.5 seconds of the step, so the
  message is on screen while that sentence is spoken.
- Two buttons are on screen that the narration does not mention: **Preview** (next to the buttons at the bottom of
  an editor, `src/OrchardCore.Modules/OrchardCore.ContentPreview/Views/ContentPreview.Button.cshtml:13`) and
  **Cancel** (`src/OrchardCore.Modules/OrchardCore.Contents/Views/Content.Edit.cshtml:30`). Do not click them, and do
  not highlight them.
- The list of content items also shows controls the narration does not mention: the **Content Type**, **Show** and
  **Sort** selectors, the bulk actions, and a selector for the layout of the list (List or Grid), which is shown because
  the admin setting `AllowUserListLayoutSelection` is on by default
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/Models/AdminSettings.cs:49`,
  `src/OrchardCore.Modules/OrchardCore.Admin/AdminListShapeTableProvider.cs:80`; the selectors are
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsAdminListFilters.cshtml:8-11`). Do not click, ring or
  hover them; keep the list in the List layout.
- Never check **Set as homepage** (`src/OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:72`).

---

## Chapter 01: What a content item is (slides, `01-idea`)

These slides are drawn from `slides.py`. Nothing is recorded.

- `idea-1`: the slide "Content items and content types" with four cards; the first two, "Content item" and "Content type", are bright.
- `idea-2`: the same slide; the last two cards, "Parts and fields" and "Settings", are bright.
- `idea-3`: the slide "The life cycle of a content item" with four cards: "Draft", "Published", "Unpublished", "Deleted".
- `idea-4`: the slide "Permissions decide the buttons you see" with four cards: "Edit content for others", "Publish or unpublish content for others", "Delete content for others", "Own content permissions"; the first three are bright. The permission names are spelled in `src/OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:17-27`. Which buttons each permission shows: `src/OrchardCore.Modules/OrchardCore.Contents/Drivers/ContentsDriver.cs:110-120`.
- `idea-5`: the same slide; only the last card, "Own content permissions", is bright.

---

## Clip `02-list` (chapter 02: Find your content)

**Starts on**: `/Admin`, opened before the capture starts (`OpenAsync`). The admin menu is visible and not collapsed.
Nothing is selected.

- `list-1`
  - Actions: move to **Content** in the admin menu and click it; then click **Content Items** in the sub-menu with
    `ClickAndWaitForNavigationAsync`. If clicking **Content** already opens the Content Items page, do not click again.
    Wait until the page shows the title **Manage Content** and its rows.
  - Visible when the narration ends: the page **Manage Content** at `/Admin/Contents/ContentItems`, the rows
    `About`, `Blog`, `Tags`, `Categories`, `Main Menu` (in whatever order the site gives), the search box, and the
    **New** button.
  - Source: `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65` ("Content") and `:68` ("Content Items"); the
    Blog recipe adds its own **Content** and **Content Items** entries, which the menu merges with these
    (`blog.recipe.json:1237`, `:1253`; `src/OrchardCore/OrchardCore.Navigation.Core/NavigationManager.cs:74-150`);
    the page title is `src/OrchardCore.Modules/OrchardCore.Contents/Views/Admin/List.cshtml:17`.
- `list-2`
  - Actions: put a green ring on the `About` row's title, then its **Article** badge, its **Published** badge, its time
    badge (a relative time such as "3 minutes ago"), and finally its **Edit** and **View** buttons together. Hold each
    for about 1.5 seconds.
  - Visible when the narration ends: the ring around the **Edit** and **View** buttons of the `About` row.
  - Source: `.../Contents/Views/ContentsTitle_SummaryAdmin.cshtml:10`, `ContentsType_SummaryAdmin.cshtml:6`,
    `ContentsTags_SummaryAdmin.cshtml:12`, `ContentsMeta_SummaryAdmin.cshtml:10-11` (the wording of the relative time is in
    `OrchardCore.DisplayManagement/Shapes/DateTimeShapes.cs:43-112`),
    `ContentsButtonEdit_SummaryAdmin.cshtml:13` (**Edit**) and `:18` (**View**).
- `list-3`
  - Actions: clear the ring; put a ring on the search box (placeholder **Search**) with the caption `Search`, and
    hold it for 2 seconds; clear it; put a ring on the funnel button to its left (its tooltip is **Filters**) with
    the caption `Filters`. Do not click either of them: the Filters menu stays closed.
  - Visible when the narration ends: the ring around the funnel button, with the caption `Filters`.
  - Source: `.../Contents/Views/ContentsAdminListSearch.cshtml:17` (**Filters**), `:28` (the placeholder **Search**).
- `list-4`
  - Actions: clear the ring; click **New** (`#new-dropdown`) to open its menu; hold it open. Do not choose a type.
  - Visible when the narration ends: the **New** menu open, listing the content types the admin can create, sorted by
    name. With the Blog recipe: **Article**, **Blog**, **Page**, **Taxonomy**.
  - Source: `.../Contents/Views/ContentsAdminListCreate.cshtml:19-20` (**New**) and `:25` (the types);
    the sort is `AdminController.cs:861-877` (`GetCreatableTypeOptionsAsync`); the creatable types are
    `Creatable` in `blog.recipe.json` (Article, Blog, Page) and `OrchardCore.Taxonomies/Migrations.cs:26`.

**Ends**: leave the menu open until the clip ends. Nothing was changed on the site.

---

## Clip `03-create` (chapter 03: Create a content item)

**Starts on**: `/Admin/Contents/ContentItems`, opened before the capture starts. Always start from this list and
click **New**: the page then carries a return address, which is what gives **Publish** and **Save Draft** their
"and continue" arrows (`Content.PublishButton.cshtml:5-27`). Do not open the editor by its address.

- `create-1`
  - Actions: click **New**, then click **Article** in its menu (`ClickAndWaitForNavigationAsync`). Wait for the
    editor.
  - Visible when the narration ends: the page **New Article** (breadcrumb **Manage Content** above it), with the
    fields **Title** (focused), **Permalink**, the rich text body editor, **Subtitle** and **Banner Image**.
  - Source: `.../Contents/Views/ContentsAdminListCreate.cshtml:20,25`; the page title is
    `.../Contents/Views/Admin/Create.cshtml:26` ("New" and the type name); the parts and their order (Title 0,
    Permalink 1, body 2, then the fields of the Article part) are `blog.recipe.json:128-190`.
- `create-2`
  - Actions: type `Spring Garden Checklist` in **Title** (`TypeAsync`); click the **Permalink** field and leave it
    empty; put a ring on the **Permalink** field with its hint underneath.
  - Visible when the narration ends: **Title** filled, **Permalink** empty with the hint "The url of the content
    item. Leave empty to auto-generate it.", and the **Set as homepage** checkbox unchecked.
  - Source: `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12` (**Title**);
    `.../OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30` (**Permalink**), `:46` (the hint); the empty
    path is filled from the title when the item is saved
    (`.../OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:397-403`, pattern `blog.recipe.json:158`).
- `create-3`
  - Actions: clear the ring; click inside the body editor (the element `.html-body-part-wysiwyg-editor
    .trumbowyg-editor`, the area under the formatting toolbar) and type `Clear the beds, check the tools, and sort the
    seed packets.`; click **Subtitle**, which sits under the body editor, and type `Jobs for the first warm weekend`;
    scroll down just far enough to show **Banner Image**; put a ring on **Banner Image** and leave it empty.
  - Visible when the narration ends: the body text and the subtitle filled in, and **Banner Image** empty with a ring.
  - Source: the editor shows the parts in the order of their position: Title 0, Permalink 1, the body 2, then the fields
    of the Article part (`blog.recipe.json:145-185`), so the body is above **Subtitle**; the labels **Subtitle** and
    **Banner Image** come from the recipe's Article part (`blog.recipe.json:685`, `:695`; both fields have the position
    0, so confirm that **Subtitle** is the first of the two); the editor is the "Wysiwyg" editor of the body part
    (`blog.recipe.json:171`; `src/OrchardCore.Modules/OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:21,32`).
    The body's own label is not spoken: see `OPEN-QUESTIONS.md`.
- `create-4`
  - Actions: clear the ring; put a ring on **Save Draft** (the bar of buttons is fixed to the bottom of the window)
    for about 2 seconds, clear
    it, then put a ring on **Publish**. Do not click.
  - Visible when the narration ends: the bar of buttons at the bottom with **Publish** (ringed), **Save Draft** and,
    next to them, **Preview** and **Cancel**. No **Delete** or **Unpublish**: the item does not exist yet.
  - Source: `.../Contents/Views/Content.SaveDraftButton.cshtml:19` (**Save Draft**),
    `.../Content.PublishButton.cshtml:19` (**Publish**); which buttons show:
    `.../Contents/Drivers/ContentsDriver.cs:110-120` (Delete needs `Id != 0`, Unpublish needs a published item,
    Save Draft needs a draftable type; the Article type is draftable, `blog.recipe.json:137`).
- `create-5`
  - Actions: clear the ring; click the small arrow on the right of **Publish** (the second half of the split button;
    its hidden text is "Toggle Dropdown"); hold the open menu for 2.5 seconds; press Escape; click the arrow on the
    right of **Save Draft**; hold that open menu for 2.5 seconds. Do not choose either "and continue".
  - Visible when the narration ends: the menu under the **Save Draft** arrow, with one item, **and continue**.
  - Source: `.../Content.PublishButton.cshtml:20-24` and `.../Content.SaveDraftButton.cshtml:20-24` (the item "and
    continue"). What it does: the controller keeps you on the editor when the value is `submit.PublishAndContinue`
    or `submit.SaveAndContinue` (`AdminController.cs:430`, `:458`, and the redirects at `:795-807`).
- `create-6`
  - Actions: press Escape to close the menu; within the first 1.5 seconds of the step click **Publish** (the main
    part of the button, not the arrow) with `ClickAndWaitForNavigationAsync`. After the list loads, put a ring on the
    **Published** badge of the new row.
  - Visible when the narration ends: `/Admin/Contents/ContentItems` with the row `Spring Garden Checklist` at the top
    (the list is sorted by last change), showing the **Article** badge, the **Published** badge, and the buttons
    **Edit** and **View**. While the sentence "A message confirms" is spoken, the toast "Your Article has been
    published." is visible at the right edge of the window (it disappears after 5 seconds).
  - Source: the message is `AdminController.cs:472-473` ("Your {0} has been published." with the type name
    **Article**); the redirect to the list is `AdminController.cs:795-798`; the badge is
    `.../Contents/Views/ContentsTags_SummaryAdmin.cshtml:12`; the order is `OrderByModified` in
    `.../Contents/Services/DefaultContentsAdminListFilterProvider.cs:306-307`.

**Ends**: the site now holds a published Article. Take snapshot **S1**.

---

## Clip `04-view` (chapter 04: View it on your site)

**Starts on**: `/Admin/Contents/ContentItems`, opened before the capture starts. The row `Spring Garden Checklist`
shows **Published**.

- `view-1`
  - Actions: put a ring on the **View** button of the row `Spring Garden Checklist`. Do not click yet.
  - Visible when the narration ends: the ring around **View** on that row.
  - Source: `.../Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18` (the **View** button appears only for an
    item that has a published version).
- `view-2`
  - Actions: clear the ring; click **View**. It opens the page in another tab (`target="_blank"`), so switch the
    capture to that tab (`SwitchToAsync`) and wait until the heading `Spring Garden Checklist` is shown. Then put a
    ring around the heading, with the caption `/spring-garden-checklist`.
  - Visible when the narration ends: the page of the site, in the blog theme: the heading `Spring Garden
    Checklist`, the subtitle `Jobs for the first warm weekend` under it, and the body `Clear the beds, check the
    tools, and sort the seed packets.` below the banner; the site name **Orchard Core** in the navigation bar.
  - Source: `.../Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`;
    `src/OrchardCore.Themes/TheBlogTheme/Views/Content-Article.liquid` (the heading, the subtitle, the body). The
    address is the title with the pattern of the Article type
    (`blog.recipe.json:158`): confirm that the Permalink reads `spring-garden-checklist` in `05-edit` before
    accepting the caption.

**Ends**: nothing was changed on the site.

---

## Clip `05-edit` (chapter 05: Edit and publish changes)

**Starts on**: `/Admin/Contents/ContentItems`, opened before the capture starts, with the Article from `03-create`
published and unchanged (restore **S1** before a new take).

- `edit-1`
  - Actions: click **Edit** on the row `Spring Garden Checklist` (`ClickAndWaitForNavigationAsync`); in the editor,
    click inside the body editor, press Control+End to move to the end of the text, and type ` Order compost early.`
    (starting with a space).
  - Visible when the narration ends: the page **Edit Article**, and the body reading `Clear the beds, check the tools,
    and sort the seed packets. Order compost early.`
  - Source: `.../Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13` (**Edit**);
    `.../Contents/Views/Admin/Edit.cshtml:39` (the page title "Edit" and the type name).
- `edit-2`
  - Actions: within the first 1.5 seconds of the step, click **Publish** (the main part of the button, not the arrow,
    in the fixed bar at the bottom) with `ClickAndWaitForNavigationAsync`; after the list loads, put a ring on the
    **Published** badge of the row `Spring Garden Checklist`. Do not click **Save Draft**.
  - Visible when the narration ends: the list, with the row `Spring Garden Checklist` showing only the **Published**
    badge (ringed) and no **Draft** badge, and the buttons **Edit** and **View**. While the first two sentences are
    spoken, the toast "Your Article has been published." is visible at the right edge of the window.
  - Source: `.../Contents/Views/Content.PublishButton.cshtml:19`; the message is `AdminController.cs:966-967` (the
    editor's Publish is `EditAndPublishPOST`, `:548-551`, which calls `PublishOrUnpublishAsync`); the badges are
    `.../Contents/Views/ContentsTags_SummaryAdmin.cshtml:12` (**Published**) and `:19` (**Draft**, shown when the
    latest version is not the published one, `ContentExtensions.cs:329-332`), so a direct Publish leaves no **Draft**
    badge.
- `edit-3`
  - Actions: click **View** on the row within the first second; switch the capture to the new tab (`SwitchToAsync`);
    wait for the heading `Spring Garden Checklist`.
  - Visible when the narration ends: the page of the site, with the body ending in `Order compost early.`
  - Source: `.../Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`;
    `src/OrchardCore.Themes/TheBlogTheme/Views/Content-Article.liquid`.

**Ends**: the site holds the Article, published, with the longer body. Take snapshot **S2**.

---

## Clip `06-remove` (chapter 06: Unpublish and delete)

**Starts on**: `/Admin/Contents/ContentItems`, opened before the capture starts, with the Article published and
without a draft (restore **S2** before a new take).

- `remove-1`
  - Actions: click the **Actions** button at the end of the row `Spring Garden Checklist`; hold the open menu for 2.5
    seconds (it shows **Unpublish**, **Delete** and **Clone**); click **Unpublish**; the dialog "Are you sure you want to
    unpublish this item?" opens: hold it for 1.5 seconds, then click **Yes**. Wait for the list to reload.
  - Visible when the narration ends: the list, with the row `Spring Garden Checklist` showing only the **Draft** badge
    (no **Published** badge, no **View** button). While the last sentence is spoken, the toast "The Article has been
    unpublished." is visible.
  - Source: `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml:32` (**Actions**);
    `.../Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:35` (**Unpublish**, the dialog text, **Yes**/**No**),
    `:40` (**Delete**), `:45` (**Clone**); the message is `AdminController.cs:756`. These three are the whole menu of a
    published item without a draft: the other contributors to the row's menu (**Download**, **Add to Deployment Plan**,
    **Export to ...**) belong to the features `OrchardCore.Contents.Deployment.Download`, `.AddToDeploymentPlan` and
    `.ExportContentToDeploymentTarget` (`OrchardCore.Contents/Manifest.cs:46-70`), which the Blog recipe does not enable,
    and the Audit Trail entry needs the Audit Trail feature. If another entry shows, stop and write it in
    `OPEN-QUESTIONS.md`.
- `remove-2`
  - Actions: click the **Actions** button of the row again; put a ring on **Publish Draft**; do not click it.
  - Visible when the narration ends: the open menu, listing **Publish Draft**, **Discard Draft**, **Delete** and
    **Clone**, with the ring on **Publish Draft**.
  - Source: `.../Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:19` (**Publish Draft**), `:30` (**Discard
    Draft**), `:40`, `:45`.
- `remove-3`
  - Actions: press Escape to close the menu; clear the ring; click **Edit** on the row
    (`ClickAndWaitForNavigationAsync`); put a ring on the red **Delete** button in the fixed bar at the bottom;
    click it. The dialog "Are you sure you want to delete this content item?" opens, with **Ok** and **Cancel**.
  - Visible when the narration ends: the dialog open over the editor. The bar at the bottom holds **Publish**,
    **Save Draft** and **Delete** (no **Unpublish**: the item is not published).
  - Source: `.../Contents/Views/Content.DeleteButton.cshtml:7-10` (**Delete**, the dialog text, **Ok**, **Cancel**);
    **Unpublish** is shown only for a published item (`.../Contents/Drivers/ContentsDriver.cs:112-114`).
- `remove-4`
  - Actions: click **Ok** and wait for the list; click the **Actions** button of the `About` row, hold the menu for 2
    seconds, then press Escape. Do not click anything in that menu.
  - Visible when the narration ends: the list without the `Spring Garden Checklist` row (it shows `About`, `Blog`,
    `Tags`, `Categories`, `Main Menu`), with the **Actions** menu of the `About` row open (**Unpublish**, **Delete**, **Clone**). While the
    first sentence is spoken, the toast "Your content has been deleted." is visible.
  - Source: `.../Contents/Views/Content.DeleteButton.cshtml:9`; the message is `AdminController.cs:579`; the
    menu is `.../Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:35-45`.

**Ends**: the Article is gone from the list. Nothing else changed.

---

## Checks for private data

The site is built from nothing and holds only the Blog recipe's content, the values in the table above, and the user
`admin` (shown as the author of each row, `.../Contents/Views/ContentsMeta_SummaryAdmin.cshtml:15-17`). So no mask
script is needed.

- Before publishing, OCR the video at one frame per second and at every scene change, then at 5 frames per second, as
  the skill's step 6 describes. Search the text for any name, email address, phone number or token that is not one of:
  `admin`, `Spring Garden Checklist`, the subtitle and body above, the Blog recipe's own text (`About`, `Blog`, `Tags`,
  `Categories`, `Main Menu`, the lorem ipsum text of the About article, its subtitle `This is what I do.`), the text of
  the blog theme around a page (the navigation items `Home` and `About`, `Toggle navigation`, and the footer line
  `Copyright © Your Website 2021` from `TheBlogTheme/Recipes/Snippets/footer.html`), or `Orchard Core`.
- The address bar is not part of the capture. The caption `/spring-garden-checklist` in `view-2` is the only address.
- No password is shown anywhere: the sign-in form is never recorded.
- Confirm that no toast or page shows a machine name, a port, or a local path.
