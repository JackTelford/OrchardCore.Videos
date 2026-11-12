# Recording plan: Alias Part: Give Content Items a Stable Name

Everything a person needs to record this video without deciding anything. Paths such as `src/OrchardCore.Modules/...`
are in the `OrchardCMS/OrchardCore` source (commit `92db4a83`). A selector written below stands for
`scene.Page.Locator(...)`. The narration is in `storyboard.json`; narrate it first (`scripts/narrate.py`), because the
recorder holds each step for the length of its narration.

The video has eight chapters between its cards: two are slide chapters (`01-why`, `07-rules`, nothing to record) and
six are recordings (`02-feature`, `03-add`, `04-generate`, `05-unique`, `06-disabled`, `08-use`).

## The site it needs

A fresh site, provisioned by Auto Setup, with the **Blog** recipe. The Blog recipe is the site because it already
enables the Alias feature, gives the Article content type a title and a permalink (and no alias), and creates the
`About` article and the `Main Menu` menu, whose alias is `main-menu`, all with sample text only
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`: features at lines 30-78, the Article type at 130-186,
the Main Menu at 817-854, the `About` article at 1060-1087).

| Setting | Value |
|---|---|
| Recipe | `Blog` |
| Tenant | `Default` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Admin user | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Database | `Sqlite`, in its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`) |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`) |

Auto Setup is given through environment variables, so no setup or sign-in form is recorded
(`OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
`...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
`...__AdminPassword=$VIDEO_PASSWORD`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog`). Set `HTTP_PROXY` and
`HTTPS_PROXY` to a dead address, so the site calls no outside service. See `references/recording.md` of the skill.

**Features.** Enable nothing, and disable nothing. The Blog recipe enables everything the video shows (Alias, Content
Types, Contents, Features, Menu, Title, Autoroute, the Blog theme). Never click **Disable** on the Alias feature: the
Menu feature depends on it, and the confirmation would list Menu as a dependent feature.

**Settings.** Leave every setting at its default. The part settings of the Alias part on the Article type start with
no stored values, so the screen shows the defaults: **Options** is `Alias is editable` and **Pattern** holds the
default pattern (`src/OrchardCore.Modules/OrchardCore.Alias/Settings/AliasPartSettings.cs:13-20`). Clip `03-add`
saves them as they are.

**Admin lists.** Leave the admin list settings at their defaults: the layout **List** with the actions drawn as
buttons (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs`, `DefaultLayout` and
`DefaultActionsLayout`). The selectors of this plan for the rows of the Features, Content Types and Content Items lists
(`.list-group-item`, the Edit button inside the row) are those of that layout, and its rows have no column headings.

**Seed data.** None to create. The data is the recipe's own: the `About` article (it has no alias: the Article type has
no Alias part before clip `03-add`) and the `Main Menu` (alias `main-menu`). The items the clips create on camera are
fake: `Contact Us` (later `Contact`) and `Help Center` (never saved).

**Browser.** Chromium at 1536x864, light color scheme, `en-US`, the admin menu expanded. Sign in once outside the
capture (`Recorder setup`), which saves `state.json` (git-ignored). Every clip starts from that file. In this plan the
entries of the admin menu are located as `#left-nav .item-label:has(span.title:text-is("<label>"))`, and the labels
of the sections open or close them without leaving the page; confirm both on the running site.

## Order of the clips, and what changes the site

Take a snapshot of `App_Data` and of `state.json` after `Recorder setup` ("snapshot A"), and a second one after clip
`03-add` ("snapshot B"). Record in this order.

| Order | Clip | Starts on | Changes the site? | Needs |
|---|---|---|---|---|
| 1 | `02-feature` | `/Admin` | No | Snapshot A |
| 2 | `03-add` | `/Admin` | **Yes**: attaches the Alias part to the Article type | Snapshot A |
| 3 | `04-generate` | `/Admin` | **Yes**: creates the article `Contact Us`, renames it `Contact`, and changes its alias twice | Snapshot B |
| 4 | `05-unique` | `/Admin/Contents/ContentItems` | No: the article `Help Center` is never saved | Snapshot B (the Alias part on Article, option `Alias is editable`) |
| 5 | `06-disabled` | `/Admin/ContentTypes/Edit/Article` | **Yes**: sets the option `Alias is generated and input is disabled`, and publishes `About`, which gets the alias `about` | Snapshot B, and `About` without an alias |
| 6 | `08-use` | `/Admin` | No | Any snapshot of a Blog site |

- To record the whole video again, stop the site, restore snapshot A, start the site, and record clips 1 to 6.
- To record `04-generate`, `05-unique` or `06-disabled` again, stop the site, restore snapshot B and start it again:
  clip `04-generate` expects no article titled `Contact Us` and the option `Alias is editable`; clip `06-disabled`
  expects `About` to have no alias and the option `Alias is editable`.
- Clips `02-feature`, `05-unique` and `08-use` change nothing and can be recorded again at any time (clip `05-unique`
  only after snapshot B, so the Article type has the Alias part).

Before every clip, check that no section of the admin menu is open on `/Admin`.

## Slide steps (nothing to record)

- `intro`: the opening card, the logo over "The Orchard Core Alias part" and "Give content items a stable name".
- `why-1`: slide "A content item ID can differ between sites", three cards: Generated, Different on each site, Hard to share.
- `why-2`: slide "An alias is a name you choose", three cards with check marks: A name you choose, Stored with the
  item, Not a web address.
- `rules-1`: slide "The rules of an alias", four cards: One item per alias, A number when taken, Up to 735
  characters, Case ignored when finding.
- `outro`: the closing card, "The Orchard Core Alias part" and "docs.orchardcore.net".

## Clip `02-feature`

Starts on `/Admin` (opened before the capture), the menu expanded, every section closed.

- `feature-1`: click the Tools label, then `ClickAndWaitForNavigationAsync` on the Features entry. Wait until
  `#search-box` is shown. Click it and type `alias`; wait until the list shows only the Alias row (the rows that do
  not match are hidden by the page's own script). Highlight the category heading `h3:text-is("Content Management")`
  for 1.5 seconds and clear it. Then highlight the status badge of the Alias row
  (`.list-group-item:has(span.fw-semibold:text-is("Alias")) .badge:has-text("Enabled")`). Do not click the row's
  Disable button.
  *Visible at the end:* the Features page, `alias` in the search box, the heading Content Management, the Alias row
  (the name Alias with its feature id, the description "The alias module enables content items to have custom logical
  identifier.", the dependency badge Content Types, the badge Enabled and a Disable button), and a ring on the
  Enabled badge.
  *Labels:* Tools and Features `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; the search box
  `.../OrchardCore.Features/Views/Admin/Features.cshtml:27` (placeholder "Search"); the category heading
  `Features.cshtml:88`; the name and the id `.../Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`; the badges
  `.../Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml` (dependencies) and
  `.../Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` (Enabled); the Disable and Enable buttons
  `.../Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53` and `:40`; the feature's name, description and category
  `src/OrchardCore.Modules/OrchardCore.Alias/Manifest.cs:4-10`; that the Blog recipe enables the feature
  `blog.recipe.json:48`. The page is the **Features** page, reached from **Tools**, not from Configuration.

## Clip `03-add`

Starts on `/Admin` (opened before the capture), the menu expanded, every section closed. This clip changes the site.

- `add-1`: click the Design label, then the Content Definition label (a section inside Design), then
  `ClickAndWaitForNavigationAsync` on the Content Types entry. Wait for the list of types (one row per type; the columns Display name and Technical name are headings of the Grid
  layout only, `src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/List.cshtml:32-33`).
  `ClickAndWaitForNavigationAsync` on `[role="btn-edit-Article"]` (the Edit button of the Article row). Wait
  until the headings Fields and Parts are shown. Move the cursor slowly over the Parts list, from the first part to
  the last. *Visible at the end:* the page "Edit Content Type - Article" with its Parts list (the parts the recipe gave the type: a title part,
  Autoroute, a body part and Article; confirm the exact names on screen) and the buttons Add Parts and Add Named Part
  under it. The Alias part is not in the list yet.
  *Labels:* Design, Content Definition and Content Types `src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25`;
  the Edit button `.../Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`; the page title
  `.../Views/Admin/Edit.cshtml:63`; the heading Parts `:144`; Add Parts `:201`. The recipe's Article type
  `blog.recipe.json:130-186`.
- `add-2`: `ClickAndWaitForNavigationAsync` on `a:text-is("Add Parts")`. Wait for the page "Add Parts" (the text
  "Choose the Parts to add to this Content Type."). Move to the Alias row
  (`li.list-group-item:has(label.form-check-label:text-is("Alias"))`; the parts are sorted by display name, so the
  row is near the top; scroll if it is not on screen) and highlight it. `ClickAndWaitForNavigationAsync` on its
  `button:has-text("Add & Configure")`. Do not tick the check box. *Visible at the end:* the page "Edit Part - Alias",
  the green notice `The "Alias" part has been added.`, a second green box (`Some part settings common to all types can
  also be defined at the Part Definition level.`, `.../ContentTypes/Views/Admin/EditTypePart.cshtml:28-30`), and the
  settings of the part (Options, Pattern) starting below.
  *Labels:* the page and the text `.../ContentTypes/Views/Admin/AddPartsTo.cshtml:10,18`; the part's display name
  "Alias" is its name with the ending "Part" trimmed
  (`src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-64`,
  used by `.../ContentTypes/ViewModels/EditPartViewModel.cs:15-20,26-31`); its description "Provides a way to define custom
  aliases for content items." is set in `src/OrchardCore.Modules/OrchardCore.Alias/Migrations.cs:21-23`; the button
  `AddPartsTo.cshtml:36`; the notice `.../ContentTypes/Controllers/AdminController.cs:355`; the title "Edit Part - Alias"
  `.../Views/Admin/EditTypePart.cshtml:22`.
- `add-3`: highlight the Options select (`.ocat-limited-wrapper:has(label:text-is("Options")) select`). With
  `SelectAsync`, choose `Alias is generated and input is disabled`, hold 1.2 seconds, then choose `Alias is editable`
  again (a headless browser draws no open list, so the closed select shows each label in turn). *Visible at the end:*
  the select showing `Alias is editable`, with a ring around it, and the Pattern field below, which holds the default
  pattern (a Liquid expression: it is on screen whatever the plan does, see OPEN-QUESTIONS.md, section 4). Under the
  Pattern field the page also draws a select, "What type of editor should be used?", with the option `Standard`
  (`.../Alias/Views/AliasPart.Option.cshtml`, `EditTypePart.cshtml:67-82`); the video does not use it.
  *Labels:* `src/OrchardCore.Modules/OrchardCore.Alias/Views/AliasPartSettings.Edit.cshtml:14,17,18`.
- `add-4`: clear the ring. Highlight only the label `label:text-is("Pattern")` (not the field: the narration never reads the
  default pattern that the field holds, and the ring stays off it). Clear the ring. Click `button.save:text-is("Save")` with
  `ClickAndWaitForNavigationAsync`. *Visible at the end:* the page "Edit Content Type - Article" with the green notice
  `The "Alias" part settings have been saved.` and the Alias part at the end of the Parts list.
  *Labels:* Pattern `AliasPartSettings.Edit.cshtml:23`; Save `.../ContentTypes/Views/Admin/EditTypePart.cshtml:111`; the
  notice `.../ContentTypes/Controllers/AdminController.cs:1084`; the new part goes last because it gets the next
  position (`.../ContentTypes/Services/ContentDefinitionService.cs:143-148,481-492`). The default pattern is
  generated from the display text, lowercased, with hyphens (`.../Alias/Settings/AliasPartSettings.cs:13-14`,
  `src/OrchardCore/OrchardCore/Modules/Services/SlugService.cs:17-65`).

Take snapshot B here.

## Clip `04-generate`

Starts on `/Admin` (opened before the capture), the menu expanded, every section closed. This clip changes the site.
The list of content items shows the recipe's items. In this clip, the row of an item is
`.list-group-item:has-text("<title>")` and its Edit button is `a.btn.edit` inside the row (if the Edit button sits in
the row's actions menu instead, open the menu first); the Publish button is the first `button.publish`, labelled
Publish.

- `generate-1`: click the Content label, then `ClickAndWaitForNavigationAsync` on the Content Items entry. Wait for the
  page "Manage Content". Click `#new-dropdown` (the button New), then `ClickAndWaitForNavigationAsync` on
  `.dropdown-menu a:text-is("Article")`. Wait for the page "New Article". Click `#TitlePart_Title` and type
  `Contact Us`. Scroll down until the Alias field (`#AliasPart_Alias`) is in the middle of the screen, and highlight it.
  Leave it empty. *Visible at the end:* the page "New Article" with the title Contact Us above, the Alias field empty,
  and under it the hint "The alias of the content item. Leave empty to re-generate it.", with a ring on the field.
  *Labels:* Content and Content Items `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65-68`; the page
  "Manage Content" `.../Contents/Views/Admin/List.cshtml:17`; the New button
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:19-20`;
  the page title `.../Contents/Views/Admin/Create.cshtml:26`; Title `src/OrchardCore.Modules/OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`;
  Alias and the hint `src/OrchardCore.Modules/OrchardCore.Alias/Views/AliasPart.Edit.cshtml:10,15,18`. The field is
  at the end of the editor because the part was added last (see `add-4`). The field's id comes from the part's name
  as the prefix (`src/OrchardCore/OrchardCore.ContentManagement.Display/ContentDisplay/ContentPartDisplayDriverTPart.cs:201-213`);
  if it differs, use `input[name$="Alias"]`.
- `generate-2`: clear the ring. `ClickAndWaitForNavigationAsync` on Publish. Wait until the list is shown with the
  green notice `Your Article has been published.` and the row `Contact Us`. `ClickAndWaitForNavigationAsync` on the
  Edit button of that row. Scroll until the Alias field is in the middle of the screen and highlight it.
  *Visible at the end:* the page "Edit Article", the title Contact Us, and the Alias field holding `contact-us`, with a
  ring.
  *Labels:* Publish `.../Contents/Views/Content.PublishButton.cshtml:7,19`; the notice
  `.../Contents/Controllers/AdminController.cs:473`; Edit `.../Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`;
  the page title `.../Contents/Views/Admin/Edit.cshtml:39`. The alias is generated when the item is created
  (`.../Alias/Handlers/AliasPartHandler.cs:53-54,90-124`).
- `generate-3`: clear the ring. Scroll to the top. Click `#TitlePart_Title`, press `Control+A`, and type `Contact`.
  `ClickAndWaitForNavigationAsync` on Publish. In the list, `ClickAndWaitForNavigationAsync` on the Edit button of the
  row `Contact`. Scroll to the Alias field and highlight it. *Visible at the end:* the title Contact and the Alias
  field still holding `contact-us`, with a ring. *Why:* the alias is computed only when it is empty
  (`AliasPartHandler.cs:92-96`).
- `generate-4`: clear the ring. Click `#AliasPart_Alias`, press `Control+A`, then `Delete`. Hold 0.8 seconds, with the
  field empty. `ClickAndWaitForNavigationAsync` on Publish. In the list, `ClickAndWaitForNavigationAsync` on the Edit
  button of the row `Contact`. Scroll to the Alias field and highlight it. *Visible at the end:* the Alias field
  holding `contact`, with a ring. *Why:* an empty alias is computed again from the current title
  (`AliasPartHandler.cs:90-124`); no other item has `contact`, so no number is added.

## Clip `05-unique`

Starts on `/Admin/Contents/ContentItems` (opened before the capture). This clip changes nothing: the article is
never saved.

- `unique-1`: click `#new-dropdown`, then `ClickAndWaitForNavigationAsync` on `.dropdown-menu a:text-is("Article")`.
  Click `#TitlePart_Title` and type `Help Center`. Scroll to the Alias field, click it and type `main-menu`.
  `ClickAndWaitForNavigationAsync` on Publish (the form is posted and the same page is drawn again, with errors).
  Scroll to the Alias field and highlight it. *Visible at the end:* the page "New Article" with a red summary at the
  top of the form and, under the Alias field (holding `main-menu`), the message
  `Your alias is already in use. "main-menu"`, with a ring on the field. The title is still `Help Center`. Leave the
  page open until the clip ends; do not publish again.
  *Labels:* the message `src/OrchardCore.Modules/OrchardCore.Alias/Models/AliasPartExtensions.cs:21`, drawn by the
  editor (`.../Alias/Drivers/AliasPartDisplayDriver.cs:33-41`) under the field
  (`.../Alias/Views/AliasPart.Edit.cshtml:13`) and in the summary
  (`.../Contents/Views/Admin/Create.cshtml:34`). The check compares the alias with every alias in the site, whatever
  the content type (`AliasPartExtensions.cs:26-29`), and the alias `main-menu` belongs to the Main Menu
  (`blog.recipe.json:852`). Type the alias in lowercase, as written.

## Clip `06-disabled`

Starts on `/Admin/ContentTypes/Edit/Article` (opened before the capture; the page "Edit Content Type - Article", with
the Alias part in the Parts list), the menu expanded. This clip changes the site.

- `disabled-1`: `ClickAndWaitForNavigationAsync` on `a[href*="/ContentParts/AliasPart/Edit"]` (the Edit button of the
  Alias row in the Parts list). Wait for the page "Edit Part - Alias". With `SelectAsync`, choose
  `Alias is generated and input is disabled` in the Options select
  (`.ocat-limited-wrapper:has(label:text-is("Options")) select`). `ClickAndWaitForNavigationAsync` on
  `button.save:text-is("Save")`. *Visible at the end:* the page "Edit Content Type - Article" with the green notice
  `The "Alias" part settings have been saved.`
  *Labels:* the Edit button `.../ContentTypes/Views/Admin/Edit.cshtml:163`; the option
  `.../Alias/Views/AliasPartSettings.Edit.cshtml:18`; Save and the notice as in `add-4`.
- `disabled-2`: `NavigateAsync("/Admin/Contents/ContentItems")`. In the row `About`, `ClickAndWaitForNavigationAsync`
  on the Edit button. Scroll to the Alias field and highlight it: it is empty and disabled (grayed). Clear the ring.
  `ClickAndWaitForNavigationAsync` on Publish. In the list, `ClickAndWaitForNavigationAsync` on the Edit button of the
  row `About`. Scroll to the Alias field and highlight it. *Visible at the end:* the Alias field holding `about`,
  disabled, with a ring. *Why:* the input is disabled when the option is `GeneratedDisabled`
  (`.../Alias/Views/AliasPart.Edit.cshtml:12`); the item gets its alias when it is saved with an empty alias
  (`AliasPartHandler.cs:56-57,90-124`); the part is added to an item that does not store it when the item is loaded
  (`src/OrchardCore/OrchardCore.ContentManagement/Handlers/ContentPartHandlerCoordinator.cs:112-124`). The `About`
  article is `blog.recipe.json:1060-1087`.

## Clip `08-use`

Starts on `/Admin` (opened before the capture), the menu expanded, every section closed.

- `use-1`: click the Content label, then `ClickAndWaitForNavigationAsync` on the Menus entry. Wait for the list of
  menus (the page titled Menu, with the row `Main Menu`). `ClickAndWaitForNavigationAsync` on the Edit button of that
  row. Scroll so that the Title and Alias fields are both on screen; highlight `#AliasPart_Alias`. Do not change
  anything and do not click Publish. *Visible at the end:* the page "Edit Menu", the title Main Menu, the Alias field
  holding `main-menu`, with a ring, and the menu items below.
  *Labels:* Content and Menus `src/OrchardCore.Modules/OrchardCore.Menu/AdminMenu.cs:33-34`; the page title
  `.../Contents/Views/Admin/Edit.cshtml:39`; the Menu type has the Alias part right after the Title part
  (`src/OrchardCore.Modules/OrchardCore.Menu/Migrations/menu.recipe.json:26-37`); the Menu feature depends on Alias
  (`src/OrchardCore.Modules/OrchardCore.Menu/Manifest.cs:13`). Never change this alias on camera: the front end keeps a
  cached copy of the menu for five minutes (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:32`).
- `use-2`: clear the ring. `NavigateAsync("/")`. Wait until `#mainNav` is shown, then highlight the links in it
  (`#mainNav a:text-is("Home")`, then `#mainNav a:text-is("About")`), about 1.5 seconds each. *Visible at the end:* the
  home page of the Blog theme, with the navigation bar holding the site name Orchard Core and the links Home and About,
  a ring on About.
  *Labels:* the layout asks for the menu by the alias `main-menu`
  (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:32`, resolved by
  `src/OrchardCore.Modules/OrchardCore.Menu/MenuShapes.cs:36-40`); the links Home and About are the menu items of the
  recipe (`blog.recipe.json:830-848`).

## Checks for private data

- Everything on screen is fake: the recipe's sample text, the titles `Contact Us`, `Contact` and `Help Center`, the
  aliases `contact-us`, `contact`, `about` and `main-menu`, and the user `admin` with the email `admin@example.com`.
- Never show the sign-in form or type the password; the recorder signs in from `state.json` (git-ignored).
- The user menu in the top bar shows the name `admin`; no other person's name or email exists on a fresh site.
- Read the contact sheet of every clip (`review.py`), then OCR the video at one frame per second and at every scene
  change, and search for any name, email or domain other than `admin`, `admin@example.com`, `example.com` and
  `orchardcore.net`.
- Do not commit `state.json`, the `App_Data` snapshots, the audio, the clips or the MP4.
