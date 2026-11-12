# Recording plan: Content Definitions in Code

Audience: developer. Eight chapters are slides, drawn by `slides.py` from the code of the SEO module and the
documentation; one chapter (`07`, clip `07-seo`) is a browser recording of the admin. Nothing else is recorded.
In this file a selector stands for `scene.Page.Locator(...)`, and `file:line` points into the Orchard Core source at
commit 92db4a83 (`src/OrchardCore.Modules/...` unless a path says otherwise).

## The site it needs

| | |
|---|---|
| Orchard Core | A build of `src/OrchardCore.Cms.Web` at commit 92db4a83, run on `http://localhost:5010` (`VIDEO_BASE_URL`). |
| App data | `ORCHARD_APP_DATA` in an empty folder outside both repositories. |
| Setup | Auto Setup (`OrchardCore__AutoSetup__...` environment variables, see `AutoSetup/README.md` under `src/docs/reference/modules`): tenant `Default`, site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`), SQLite, recipe `Blog`. No setup form and no sign-in form is recorded, so the video is recorded with a `state.json` made by `Recorder setup`. |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls a service. |
| Features to enable before recording | None. The Blog recipe does not enable the SEO module (`OrchardCore.Seo`): enabling it is the first thing the clip does. Do not enable it, or open its pages, while preparing. |
| Settings | None. |
| External services | None. |
| Seed data | None. The video shows only what the SEO migration creates, so there is no `PrepareAsync` and no fake record to create. |

Check the starting state once, outside the capture, as `admin`:

1. Tools, Features (`/Admin/Features`): the row **SEO** shows an **Enable** button.
2. Design, Content Definition, Content Parts (`/Admin/ContentTypes/ListParts`): no **SeoMeta** row.
3. Design, Content Definition, Content Types (`/Admin/ContentTypes/List`): no **Social Meta Settings** row.

Then stop the site, copy its `App_Data` folder and `state.json` aside (the snapshot "before-seo"), and start the site
again.

## Order of the clips, and what changes the site

There is one clip to record: `07-seo`. It changes the site: the migration of the SEO module stores the part
`SeoMetaPart`, the type and part `SocialMetaSettings`, and a migration version (`Migrations.cs:21-43`). Disabling the
feature afterwards does not undo that (nothing calls `Uninstall`), so for another take stop the site, restore the
snapshot "before-seo", and start the site again.

The slide chapters (`01` to `06` and `08`) need no site and can be rendered in any order.

## Clip `07-seo`

Starts on `/Admin/Features` (opened with `scene.OpenAsync`, before the capture), signed in as `admin`, with the
admin menu collapsed. The search box is empty, and the filters keep their defaults (**Hide on demand** and **Hide
always enabled** selected, **Any status**). Source of the page: `OrchardCore.Features/Views/Admin/Features.cshtml`.

- `seo-1`
  - Actions: `TypeAsync(#search-box, "SEO")` (the placeholder is **Search**, `Features.cshtml:27`; the list filters as
    you type). `ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_Seo)`: the button is the **Enable** link of the
    row (`FeatureEntry.Buttons.SummaryAdmin.cshtml:40`); its id is `btn-enable-` plus the feature id with the dots
    replaced by underscores. The link posts (`Controllers/AdminController.cs:225-256`), the shell descriptor is
    updated and the shell is released (`OrchardCore/Shell/ShellHost.cs:174-175`, called by
    `OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs:133-136`), and the server redirects to
    `/Admin/Features`: the redirected request builds the tenant again and `AutomaticDataMigrations.ActivatingAsync`
    runs the migration, so the page can take several seconds to come back. That wait is inside `ClickAndWaitForNavigationAsync`, so it is on camera: the step only gets a pause, because
    a step that runs longer than its narration adds time and nothing else. Do not try to cut it with `OffCameraAsync`
    after the navigation, since the page has already loaded by then (see `OPEN-QUESTIONS.md`, item 1). When the page is
    back, its search box is empty and the full list is shown, so wait for `#btn-disable-OrchardCore_Seo` (a **Disable**
    button that was not on the page before, `FeatureEntry.Buttons.SummaryAdmin.cshtml:53`) with a timeout of 90
    seconds, then `TypeAsync(#search-box, "SEO")` again so that the row is on screen.
  - Visible when the narration ends: the Features page with the search box holding `SEO` and two rows: **Media
    Slugify** (its description contains "SEO-friendly", and the search matches the name, the id and the description,
    `Drivers/FeatureEntryDisplayDriver.cs:20` and `Assets/ts/features-index.ts:88-101`) and **SEO**, the second with a
    red **Disable** button. The Blog recipe already enables every dependency of SEO (Contents, Media, Shortcodes
    through Shortcode Templates, and Recipes Core Services), so the notification is the singular "The feature SEO was
    enabled." (`Controllers/AdminController.cs:313-317`); it may still be shown at the top and is not required. No
    error message.
  - Labels from: **Features** `OrchardCore.Features/AdminMenu.cs:27`, **Enable** and **Disable**
    `FeatureEntry.Buttons.SummaryAdmin.cshtml:40,53`, the name **SEO** `OrchardCore.Seo/Manifest.cs:4`.
- `seo-2`
  - Actions: `NavigateAsync("/Admin/ContentTypes/ListParts")`, then wait until `[data-type-name="SeoMetaPart"]` is
    visible. The list holds more than ten parts that other enabled features created (for example Alias, Autoroute,
    Flow, List, Title), ordered by display name, and the viewport is 1536x864 (`Scene.Height`), so the row can be
    below the fold. Take `box = await row.BoundingBoxAsync()` (relative to the viewport) and, when `box.Y > 300`, call
    `ScrollAsync(0, box.Y - 260)`, so that the row sits in the upper third of the frame. Then
    `HighlightAsync([data-type-name="SeoMetaPart"], ...)` on the row, and `ClearHighlightsAsync()` before the next
    step.
  - Visible when the narration ends: the page **Content Parts**, scrolled so that the row named **SeoMeta** is in
    the upper third with other parts around it, and, under it, the description "Provides a part that allows SEO meta
    descriptions to be applied to a content item." The row has **Edit** and **Delete** buttons.
  - Labels from: the menu path **Design**, **Content Definition**, **Content Parts** `OrchardCore.ContentTypes/AdminMenu.cs:23,24,30`
    (the page is reached by its URL because the menu is collapsed), the page title
    `Views/Admin/ListParts.cshtml:6`, the row name from `ContentPartDefinition.DisplayName()` (the part name without
    its suffix **Part**), the description from `OrchardCore.Seo/Migrations.cs:25`.
- `seo-3`
  - Actions: `ClickAndWaitForNavigationAsync(#btn-edit-SeoMetaPart)` (`ContentPartEntry.Buttons.SummaryAdmin.cshtml:6`;
    the URL is `/Admin/ContentParts/Edit/SeoMetaPart`). The settings of the part (display name, description,
    attachable and so on) come first, so the **Fields** list is below the fold. Take
    `box = await scene.Page.Locator("#fields").BoundingBoxAsync()` and call `ScrollAsync(0, box.Y - 220)`, so that the
    **Fields** heading (just above the list) and the three rows are in the frame, then `HighlightAsync(#fields)`.
  - Visible when the narration ends: the page **Edit Content Part - SeoMeta** (`Views/Admin/EditPart.cshtml:23`) with
    the list under **Fields**: **Default social image**, **Open graph image** and **Twitter image**, each with the
    hint **Media Field**, in that order.
  - Labels from: `OrchardCore.Seo/Migrations.cs:28,32,36` (the display names), `EditPart.cshtml:46` (**Fields**).
- `seo-4`
  - Actions: `NavigateAsync("/Admin/ContentTypes/List")` and wait until `[data-type-name="SocialMetaSettings"]` is
    visible. The list holds the types of the Blog recipe and of the enabled features, ordered by display name, so
    scroll the row into the upper third as in `seo-2` (`box.Y - 260`) and highlight it. Keep the list on screen for
    about five seconds (the narration says "Under Content Types" first), clear the highlight, then
    `ClickAndWaitForNavigationAsync([role="btn-edit-SocialMetaSettings"])` (`ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`;
    the URL is `/Admin/ContentTypes/Edit/SocialMetaSettings`). On the edit page, about two seconds after the click,
    scroll to the **Fields** list with `ScrollAsync(0, box.Y - 220)` for the box of `#fields`. It has 13 rows, so when
    the narration reaches "which the Parts list marks as", scroll again with `ScrollAsync(0, box.Y - 300)` for the box
    of `#parts` (taken after the first scroll), so that the first row of the **Parts** list and its hint are on
    screen at the end.
  - Visible when the narration ends: the page **Edit Content Type - Social Meta Settings**, scrolled to the **Parts**
    list, whose row reads **Social Meta Settings** (the display name of the type, since the part has the name of the
    type: `OrchardCore.ContentManagement.Abstractions/Metadata/Models/ContentTypePartExtensions.cs:13-18`) with the
    hint **Contains the fields for the current type**. Before that, the **Fields** list was on screen, in the order
    of the `Position` values of the recipe: **Default Meta Description**, **Default social image**, **Open graph
    image**, **Twitter Image**, **Open Graph Type** and the others.
  - Labels from: `Views/Admin/Edit.cshtml:63` (title), `:88` (**Fields**), `:144` (**Parts**), `:180` (the hint),
    `Views/Admin/List.cshtml:6` (**Content Types**), the type name and fields from
    `OrchardCore.Seo/Migrations/socialmetasettings.recipe.json:16-17,44-47,86,158-160` (the fields are ordered by
    `Position`, `Views/Admin/Edit.cshtml:48-55`).

## Slide steps

One image each, drawn by `slides.py`; nothing to record.

- `intro`: the opening card. `outro`: the closing card.
- `why-1`: four cards, the first two bright: "Built in the admin" and "Defined in code".
- `why-2`: the same four cards, the last two bright: "Data migration" and "Recipe".
- `class-1`: the dependencies of the manifest of the SEO module (`Manifest.cs`, `OrchardCore.Seo/Manifest.cs:10-17`),
  with two notes (the dependencies are enabled with the module, `OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:75-79`;
  their migrations run first, `OrchardCore.Data.YesSql/Migration/DataMigrationManager.cs:179-188`).
- `class-2`: `Startup.cs` of the SEO module (`OrchardCore.Seo/Startup.cs:27-32`), the lines that register the migration
  and the part bright.
- `class-3`: the class and the constructor of `Migrations.cs` (`OrchardCore.Seo/Migrations.cs:9-19`).
- `type-1`: the `Product` type with its settings, from the documentation of the Content Types module, the settings bright.
- `type-2`: the same code, the line that attaches `TitlePart` bright.
- `type-3`: the `ProductReview` type and its display name.
- `part-1`: the part `SeoMetaPart` with its first field (`OrchardCore.Seo/Migrations.cs:23-38`).
- `part-2`: the type `Product` with the part `Product`, which holds the field `Price`, from the documentation.
- `recipe-1`: the call of `IRecipeMigrator` (`OrchardCore.Seo/Migrations.cs:40`) and the folder of the module.
- `recipe-2`: the first step of `socialmetasettings.recipe.json` (without the `steps` list around it), trimmed; the
  omitted settings are marked `/* ... */`.
- `recipe-3`: the same step, the display name, the settings and the part record bright.
- `recipe-4`: three cards for the steps `ContentDefinition`, `ReplaceContentDefinition` and `DeleteContentDefinition`,
  which the Content Types feature registers (`OrchardCore.ContentTypes/Startup.cs:67-69`).
- `version-1`: `CreateAsync` and `UpdateFrom1Async` of the SEO module (`OrchardCore.Seo/Migrations.cs:21-50`), the
  method names and the two `return 2;` bright.
- `version-2`: the same code, the recipe call of `CreateAsync` and `UpdateFrom1Async` bright.
- `version-3`: `RemovePart`, `RemoveField` and the two `Delete...DefinitionAsync` calls.
- `cs-1`: the class `SeoMetaPart` (`OrchardCore.Seo/Models/SeoMetaPart.cs:8-42`), trimmed, every omission marked
  `// ...`.
- `cs-2`: the registration of the part, the signature and the shape of `GetContentItemAspectAsync` of
  `OrchardCore.Seo/Handlers/SeoMetaPartHandler.cs` (lines 37-39 and 89) trimmed with `// ...`, and `GetOrCreate`
  (`OrchardCore.ContentManagement.Abstractions/ContentItemExtensions.cs:80-81`).

## Checks for private data

The site is made from scratch with the Blog recipe, so no real name can be on screen. Before assembling, look at every
frame of `07-seo` for anything but: the user `admin`, the email `admin@example.com`, the site name `Orchard Core`, the
names of the features, parts, types and fields above. No password, token or connection string is ever shown. OCR the
video at every scene change and at five frames per second and search for `@` and for any name that is not in the list
above.
