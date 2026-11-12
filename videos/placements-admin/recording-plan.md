# Recording plan: placements-admin

How to record the three browser clips of this video (`02-enable`, `03-create`, `04-shortcuts`) without deciding
anything. The other chapters are slides, drawn by `slides.py`, and cards: they need no recording.

In this plan a selector such as `#ShapeType` stands for `scene.Page.Locator("#ShapeType")`. Selectors are read from the
Orchard Core views and are not tested on a running site: if one does not match, the visible label next to it is the
reference. Source references are `file:line` in the Orchard Core source at commit `92db4a83`, where `P` is
`src/OrchardCore.Modules/OrchardCore.Placements`, `F` is `src/OrchardCore.Modules/OrchardCore.Features`, `T` is
`src/OrchardCore.Modules/OrchardCore.ContentTypes` and `C` is `src/OrchardCore.Modules/OrchardCore.Contents`.

## The site it needs

| | |
|---|---|
| Orchard Core | A checkout of `OrchardCMS/OrchardCore` (the commit above or a later `main`), `src/OrchardCore.Cms.Web`, on `http://localhost:5010` |
| App_Data | An empty folder outside both repositories (`ORCHARD_APP_DATA`) |
| Provisioning | Auto Setup (see the `orchardcore-tester` skill of the Orchard Core repository, `references/autosetup.md`): no setup screen is recorded, so no mask script is needed |
| Recipe | Blog |
| Site name | Orchard Core |
| Time zone | `America/Los_Angeles` |
| Tenant | Default |
| User | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed as `VIDEO_PASSWORD`; `VIDEO_USER_NAME=admin`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, as in `references/recording.md` |
| External services | None |
| Sign-in | `Recorder setup` once, outside the capture (`state.json` is not committed) |

### Features before recording

The Blog recipe enables `OrchardCore.Placements` and leaves `OrchardCore.Placements.FileStorage` disabled
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:67`). The clip `02-enable` enables Placements on
camera, so disable it first, outside the capture:

1. Open `/Admin/Features` (Tools, Features).
2. Type `placements` in the search box.
3. On the row whose code reads `OrchardCore.Placements`, click `Disable`, then `Yes` in the confirmation dialog
   (`F/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`, `data-ok-text="Yes"`).
4. Wait until the same row shows an `Enable` button.

Leave every other feature as the Blog recipe set it. In particular `OrchardCore.Placements.FileStorage` stays disabled
for all three clips, and the Deployment features are not used on camera.

### Seed data

None to create. The clips use what the Blog recipe creates (`blog.recipe.json:1000`):

- the content type **Blog Post** (technical name `BlogPost`), whose own part `BlogPost` holds the fields **Subtitle**
  (a Text Field, technical name `Subtitle`) and **Banner Image** (a Media Field, technical name `Image`);
- one content item of that type, titled `Man must explore, and this is exploration at its greatest`, with the
  subtitle `Problems look mighty small from 150 miles up`.

No placement exists at the start. Do not add any placement, content type or content item outside the capture.

### Snapshots

Stop the site, copy its `App_Data` folder aside after each of these states, and restore the copy before a new take:

| Snapshot | State | Used to record |
|---|---|---|
| S0 | Fresh Blog site, Placements disabled, signed in | `02-enable` |
| S1 | After `02-enable`: Placements enabled, no placement | `03-create` |
| S2 | After `03-create`: one placement, `MediaField_Edit` | `04-shortcuts` |
| S3 | After `04-shortcuts`: two placements, `MediaField_Edit` and `TextField_Edit` | (final state) |

## Order

Record in this order, each clip starting from the snapshot named above: `02-enable`, `03-create`, `04-shortcuts`.
`03-create` needs the Placements pages that `02-enable` enables, and `04-shortcuts` ends on a list that holds the
placement `03-create` saved. All three change the site: `02-enable` enables a feature (the tenant restarts),
`03-create` and `04-shortcuts` save a placement. To record one clip again, restore the snapshot that precedes it.

The admin preferences remember the last choice of the split Save button (`and continue`) in the browser profile
(`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/actionGroup.ts`). The clips only click `Save`: if the button reads
`Save and continue`, the profile remembers an earlier choice; use a clean profile.

## Clip `02-enable`

Starts on `/Admin/Features` (Tools, Features), signed in, from S0, with the search box empty. The capture starts
before the first step.

- `enable-1` (narration: the feature, Tools, Features, the Development category, "search for placements")
  - Actions: click the search box `#search-box` (placeholder `Search`, `F/Views/Admin/Features.cshtml:27`) and type
    `placements` slowly. The list filters as you type, with no Enter key. Highlight the group heading
    `Development` (`h3` with that text, `F/Views/Admin/Features.cshtml:88`).
  - Visible when the narration ends: the search box reads `placements`; under the heading `Development` there are
    exactly two rows, `Placements` with the id `OrchardCore.Placements`, and `Placements file storage` with the id
    `OrchardCore.Placements.FileStorage`; both have an `Enable` button; no other category is listed. The counter
    reads `Showing 2 of ...`.
  - Labels from: `P/Manifest.cs:11-22` (names, ids, category `Development`), `F/Views/Admin/Features.cshtml:27,59,88`.
- `enable-2` (narration: Placements adds the admin pages and saves rules in the database; Placements file storage
  depends on it and saves them in a file; "Let's enable Placements")
  - Actions: highlight the description of the first row (`The Placements module provides a way to define shape
    placement in admin UI.`), then the description of the second row (`Stores Placements in a local file.`), then
    clear the highlights. Last, click `Enable` on the first row, the one whose code reads exactly
    `OrchardCore.Placements` (suggested selector: `.list-group-item:has(code:text-is("OrchardCore.Placements")) a:text-is("Enable")`).
    Use `ClickAndWaitForNavigationAsync`. Wait off camera (`OffCameraAsync`) until the first row shows a `Disable`
    button: enabling restarts the tenant, so the page takes a few seconds to come back.
  - Visible when the narration ends: the Features page reloaded, the filter either kept or cleared, and the
    `Placements` row with a `Disable` button. Do not click `Placements file storage`.
  - Labels from: `F/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (`Enable`),
    `F/Views/FeatureEntry.Description.SummaryAdmin.cshtml`, `P/Manifest.cs:13,20`.

After the clip: Design, Placements exists in the admin menu. Take snapshot S1.

## Clip `03-create`

Starts on `/Admin/Placements` (Design, Placements), from S1. The list is empty. Navigation inside the clip always
goes through `ClickAndWaitForNavigationAsync` or `NavigateAsync`.

- `create-1` (narration: Design, Placements lists the placements; empty until you add one; "select Add Placement")
  - Actions: highlight the empty message (`Nothing here! There are no placements at the moment.`), clear it, click
    the `Add Placement` button (`a.create`).
  - Visible when the narration ends: the page `Create Placement` (breadcrumb `Placements` > `Create Placement`), the
    `Shape type` field empty, and the `Placement rules` code editor holding one empty rule.
  - Labels from: `P/Views/Admin/Index.cshtml:34` (empty message), `:25` (`Add Placement`), `P/Views/Admin/Edit.cshtml:13`
    (`Create Placement`), `:26` (`Shape type`), `:35` (`Placement rules`); admin menu `P/AdminMenu.cs:18-19`.
- `create-2` (narration: a placement is a shape type and its rules; "Let's enter MediaField_Edit")
  - Actions: type `MediaField_Edit` into the `Shape type` field `#ShapeType` (it is focused when the page opens; use
    `TypeAsync`). Highlight the hint under it (`The type of the shape.`).
  - Visible when the narration ends: `MediaField_Edit` in the `Shape type` field, the hint visible.
  - Labels from: `P/Views/Admin/Edit.cshtml:26,30`.
- `create-3` (narration: a JSON array in a code editor, one empty rule; saving it as it is shows an error)
  - Actions: highlight the code editor (`.CodeMirror`). It shows the template of one empty rule,
    `[` newline `  {}` newline `]` (`P/Controllers/AdminController.cs:138-144`; see OPEN-QUESTIONS.md). Clear the
    highlight, then click `Save` (`button[name="submit"][value="Save"]`, `P/Views/Admin/Edit.cshtml:50`) with
    `ClickAndWaitForNavigationAsync`.
  - Visible when the narration ends: the page `Create Placement` again, with a red error notification reading
    `A valid placement must contain either place, shape, wrappers or alternates. Please correct the placements at
    positions: 1.`, the shape type still `MediaField_Edit`, and the editor still holding the empty rule.
  - Labels from: `P/Controllers/AdminController.cs:230` (the error), `:341-347` (what makes a rule empty).
- `create-4` (narration: replace the rule with one for the Image field of the BlogPost part; the differentiator
  `BlogPost-Image`; a place of `-` hides the shape)
  - Actions: click inside the editor (`ClickAsync` on `.CodeMirror`), press `Control+A`, then type the following on one
    line with the keyboard (`scene.Page.Keyboard.TypeAsync(text, new() { Delay = 40 })`), not with `scene.TypeAsync`,
    which would click the editor again and drop the selection:

    ```
    [{ "place": "-", "differentiator": "BlogPost-Image" }]
    ```

    The editor has no automatic closing of brackets or quotes (`P/Assets/ts/placement-edit.ts`), so the text appears
    exactly as typed. Dismiss nothing else: the error notification may stay on screen.
  - Visible when the narration ends: the editor holds the single line above, and nothing after it.
  - Source of the format: `PlacementNode` property names `place` and `differentiator`
    (`src/OrchardCore/OrchardCore.DisplayManagement/Descriptors/ShapePlacementStrategy/PlacementFile.cs`), and the
    hide value `-` (`PlacementInfo.cs:8`).
- `create-5` (narration: Save returns to the list; the placement appears under its shape type; Edit, Delete, the
  search box)
  - Actions: click `Save` (`ClickAndWaitForNavigationAsync`). On the list, highlight in turn the row's shape type
    link `MediaField_Edit`, then its `Edit` button, then its `Delete` button, then the search box `#search-box`
    (placeholder `Search`). Do not click `Edit` or `Delete`. Clear the highlights before the step ends.
  - Visible when the narration ends: the Placements list (`/Admin/Placements`) with a green notification
    `The "MediaField_Edit" placement have been saved.` (the wording is the code's), one row `MediaField_Edit` with the
    buttons `Edit` and `Delete`, and the search box.
  - Labels from: `P/Controllers/AdminController.cs:240` (notification), `P/Views/ShapePlacement.Buttons.SummaryAdmin.cshtml:10,17`
    (`Edit`, `Delete`), `P/Views/ShapePlacement.Fields.SummaryAdmin.cshtml` (the link), `P/Views/Admin/Index.cshtml:20`.
- `create-6` (narration: the result in the editor of a blog post; the Banner Image field is gone)
  - Actions: `NavigateAsync("/Admin/Contents/ContentItems/BlogPost")`, the list of the items of the type `BlogPost`.
    Do not use Content, Content Items without a type: the Blog recipe does not make Blog Post listable, so the
    unfiltered list does not hold this item (see the sources below). In the row titled
    `Man must explore, and this is exploration at its greatest`, click `Edit` (`ClickAndWaitForNavigationAsync`;
    the title link opens the same page). On the editor page, scroll so that the `Subtitle` field and the field after
    it are both visible, and highlight the `Subtitle` field.
  - Visible when the narration ends: the editor of the blog post, with the `Subtitle` field (value `Problems look
    mighty small from 150 miles up`) followed directly by the `Tags` field. There is no `Banner Image` field. Clear
    highlights before scrolling.
  - Labels and sources: `C/Controllers/AdminController.cs:67` (the route `Contents/ContentItems/{contentTypeId?}`),
    `C/Services/DefaultContentsAdminListFilterProvider.cs:145` (a type named in the route is listed even if it is not
    listable) and `:182` (the unfiltered list skips types that are not listable), `blog.recipe.json:188-196` (Blog Post
    has no `Listable` setting), `C/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13` (`Edit`), field display names in
    `blog.recipe.json` (`Subtitle`, `Banner Image`, `Tags`).

After the clip, take snapshot S2.

## Clip `04-shortcuts`

Starts on `/Admin/ContentTypes/List` (Design, Content Definition, Content Types), from S2.

- `shortcut-1` (narration: the settings of a content part, a part in a type and a field link to placements; open
  Content Types, edit the Blog Post type, edit its Subtitle field)
  - Actions: on the list, click `Edit` in the row of `Blog Post` (`ClickAndWaitForNavigationAsync`; the row is the one
    that contains the text `Blog Post`, suggested selector `.list-group-item:has-text("Blog Post") a:text-is("Edit")`).
    On the page `Edit Content Type - Blog Post`, scroll to the `Fields` list, and click `Edit` on the `Subtitle` row
    (suggested selector `#fields .list-group-item:has-text("Subtitle") a:text-is("Edit")`).
  - Visible when the narration ends: the field settings page, titled `"Subtitle" settings`, with the `Display Name`
    and `Technical Name` fields and the `Edit placements` button below them.
  - Labels from: `T/AdminMenu.cs:23-25`, `T/Views/Admin/List.cshtml`, `T/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`,
    `T/Views/Admin/Edit.cshtml:63,88,100`, `T/Views/Admin/EditField.cshtml:30,48,56,63`.
- `shortcut-2` (narration: the Edit placements button; its menu lists the field, detail views, summary views and the
  admin editor)
  - Actions: click the `Edit placements` button (`button:text-is("Edit placements")`) to open its menu. Highlight the
    menu.
  - Visible when the narration ends: the open menu with four entries, in this order:
    `Placement for the Subtitle field in a BlogPost`,
    `Placement for the Subtitle field in a BlogPost in detail views`,
    `Placement for the Subtitle field in a BlogPost in summary views`,
    `Placement in admin editor for the Subtitle field in a BlogPost`.
  - Labels from: `P/Views/PlacementSettings.cshtml:5`, `P/Settings/PlacementContentPartFieldDefinitionDisplayDriver.cs:33,42,51,59`.
- `shortcut-3` (narration: the editor entry opens Edit Placement with the shape type TextField_Edit and a rule with
  the differentiator BlogPost-Subtitle already filled in)
  - Actions: clear the highlight, click the fourth entry, `Placement in admin editor for the Subtitle field in a
    BlogPost` (`ClickAndWaitForNavigationAsync`). Highlight the `Shape type` field, then the `differentiator` line of
    the rule.
  - Visible when the narration ends: the page `Edit Placement` (breadcrumb `Placements` > `Edit Placement`), the
    `Shape type` field disabled and reading `TextField_Edit`, and the editor holding one rule with the property
    `differentiator` set to `BlogPost-Subtitle` and no `place` (see OPEN-QUESTIONS.md for the exact text). The buttons
    `Save`, `Delete` and `Cancel` are at the bottom. The address is
    `/Admin/Placements/Edit/TextField_Edit?differentiator=BlogPost-Subtitle&returnUrl=...`.
  - Labels from: `P/Views/Admin/Edit.cshtml:13,26`, `P/Controllers/AdminController.cs:160-173` (the generated rule),
    `P/Settings/PlacementContentPartFieldDefinitionDisplayDriver.cs:56-60` (shape type `TextField_Edit`, differentiator
    `BlogPost-Subtitle`).
- `shortcut-4` (narration: the rule still needs a place; add a place of `-`, which hides the shape, and save)
  - Actions: clear the highlights, click inside the editor, press `Control+A`, type on one line with the keyboard
    (`scene.Page.Keyboard.TypeAsync(text, new() { Delay = 40 })`):

    ```
    [{ "place": "-", "differentiator": "BlogPost-Subtitle" }]
    ```

    then click `Save` (`ClickAndWaitForNavigationAsync`). Saving redirects to the page the shortcut came from.
  - Visible when the narration ends: the field settings page `"Subtitle" settings` again, with a green notification
    `The "TextField_Edit" placement have been saved.`
  - Labels from: `P/Controllers/AdminController.cs:240,263` (the notification, the redirect to the return URL).
- `shortcut-5` (narration: Save returns to the field settings; in Design, Placements the list holds both
  placements, sorted by shape type)
  - Actions: `NavigateAsync("/Admin/Placements")`. Highlight the two rows in turn, then clear.
  - Visible when the narration ends: the Placements list with two rows in this order, `MediaField_Edit` and
    `TextField_Edit`, each with `Edit` and `Delete` buttons.
  - Labels from: `P/Controllers/AdminController.cs:87` (sorted by shape type), `P/AdminMenu.cs:18-19`.

After the clip, take snapshot S3 (the state to keep if the video is recorded again from the end).

## The slides and the cards

Nothing to record. `slides.py` draws each slide step:

- `intro`: the opening card, drawn from the chapter title and subtitle.
- `why-1`: a code panel `placement.json` with a rule for `MediaField_Edit`, and three cards (shape type, rules, where).
- `why-2`: three check cards: edited in the admin, saved with the site, the same format.
- `why-3`: three numbered cards: location, alternates and wrappers, and how each combines with a theme or a module.
- `why-4`: three numbered cards: every matching rule applies (in this module), the last place wins, alternates and wrappers add up.
- `store-1`: the `IPlacementStore` interface and its default registration in `Startup.cs`.
- `store-2`: the registration that Placements file storage uses, the `PlacementsDocument.json` content, and the path of
  the file for the `Default` tenant.
- `recipe-1`: a recipe with the `Placements` step, and three cards on what the step holds.
- `recipe-2`: three numbered cards: the Deployment module, the All Placements step, the recipe format.
- `outro`: the closing card.

## Checks for private data

- The site is fresh and every record comes from the Blog recipe or from this plan: no mask script is needed. The only
  address in the plan is `admin@example.com`, and it is not shown in these clips.
- OCR the finished video at one frame per second and at every scene change (`select='gt(scene,0.02)'`), then search for
  `@` and for any name other than `admin`, the only user of the site.
- The password never appears on screen or in a file of this folder.
