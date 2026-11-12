# Recording plan: Lucene: Full-Text Search With No External Service

This plan is for the person who records the video. It is complete: every choice is made here. The narration is in
`storyboard.json`; this file says what the browser does while each step is spoken, and what must be on screen when its
narration ends.

Conventions in this file:

- A selector written in backticks stands for `scene.Page.Locator(...)`. Label-based lookups are written as
  `GetByLabel("...")`, which stands for `scene.Page.GetByLabel("...")`.
- `<password>` is the password chosen outside git and passed in `VIDEO_PASSWORD`. It is never written down, and never
  shown: no clip opens a page that shows it.
- `file:line` references are in the Orchard Core source at commit 92db4a83 (`src/OrchardCore.Modules/...` and
  `src/OrchardCore/...`). A label is spelled exactly as that line spells it.
- This is an administrator video: no clip shows code, a query, a configuration file or a command line. The clips never
  open the Lucene Query page (Search, Queries, Run Lucene Query) or the Query Index action of an index.

## The site it needs

| | |
|---|---|
| Orchard Core | A checkout of `OrchardCMS/OrchardCore` at the commit above or a later one in which these labels still read the same. Run `src/OrchardCore.Cms.Web`. |
| URL | `http://localhost:5010` (`VIDEO_BASE_URL`) |
| App data | `ORCHARD_APP_DATA` points at an empty folder outside both repositories. |
| Provisioning | Auto Setup, so no setup or login form is recorded. This video's subject is not the Setup screen. |
| Recipe | Blog |
| Tenant | Default |
| Site name | Orchard Core |
| Time zone | America/Los_Angeles |
| Database | SQLite (the Auto Setup default connection string is empty) |
| User | `admin`, email `admin@example.com`, password `<password>` |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (for example `http://127.0.0.1:9`), so the site never calls out. No external service is used by this video. |

The Auto Setup settings, as environment variables (values in quotes are literal):

```bash
export OrchardCore__AutoSetup__AutoSetupPath=""
export OrchardCore__AutoSetup__Tenants__0__ShellName="Default"
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone="America/Los_Angeles"
export OrchardCore__AutoSetup__Tenants__0__AdminUsername="admin"
export OrchardCore__AutoSetup__Tenants__0__AdminEmail="admin@example.com"
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider="Sqlite"
export OrchardCore__AutoSetup__Tenants__0__DatabaseConnectionString=""
export OrchardCore__AutoSetup__Tenants__0__DatabaseTablePrefix=""
export OrchardCore__AutoSetup__Tenants__0__RecipeName="Blog"
```

(The keys are those of `src/docs/reference/modules/AutoSetup/README.md`; the module reads the section `AutoSetup`, or the older
`OrchardCore_AutoSetup`: `OrchardCore.Modules/OrchardCore.AutoSetup/Startup.cs:26-29, 67`. The first request to `/` must provision
the site, with no setup form.)

Run the site, then sign the recorder in once:

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/lucene-search/recorder
export VIDEO_ROOT=<repo>/videos/lucene-search VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
export MSYS_NO_PATHCONV=1
<bin>/Recorder setup
```

### Features: before the recording

- The Blog recipe has already enabled the features this video relies on: Indexing, Queries, Contents, Content Types,
  Deployment, Features and the rest of the recipe's list (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`).
- Enable **Search** (outside the capture): open `/Admin/Features`, type `Search` in `#search-box`, and click
  `#btn-enable-OrchardCore_Search` (feature "Search", `OrchardCore.Modules/OrchardCore.Search/Manifest.cs:12`). The search
  page of clip `05-search` needs it.
- **Do not enable Lucene before clip `02-enable`.** Clip `02-enable` enables it on camera.
- **Never enable Lucene Worker, Lucene (Obsolete) or Lucene Content Picker.** The Lucene Worker feature pulls in
  Lucene (Obsolete), and it is only described on a slide. The Indexing Worker feature stays disabled too.
- Do not change the Site Search settings: clip `05-search` opens the search page of the index by its name.

### Seed data: four fake articles (outside the capture)

Open `/Admin/DeploymentPlan/Import/Json` (Tools, Deployments, JSON Import: `OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30`),
paste the text below into the **JSON** field (`OrchardCore.Modules/OrchardCore.Deployment/Views/Import/Json.cshtml:15`), and
click **Import** (same file, line 21). It creates four published Articles. Every name and sentence is invented.

```json
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "ujzde8gxd6ncf10epf91dhodzd",
          "ContentItemVersionId": "oc9is0j8ht9lgmxg9edn581u33",
          "ContentType": "Article",
          "DisplayText": "Starting a kitchen garden",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-03-03T09:00:00Z",
          "ModifiedUtc": "2025-03-03T09:00:00Z",
          "PublishedUtc": "2025-03-03T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Starting a kitchen garden"
          },
          "AutoroutePart": {
            "Path": "starting-a-kitchen-garden",
            "SetHomepage": false
          },
          "HtmlBodyPart": {
            "Html": "<p>A kitchen garden needs sun, water and a plan. Begin with herbs such as basil and chives, then add tomatoes once the soil is warm.</p>"
          }
        },
        {
          "ContentItemId": "xtplpft75v2seh60kvj50ce9uv",
          "ContentItemVersionId": "w53efr4edt2sywb3wkh5dnsipz",
          "ContentType": "Article",
          "DisplayText": "Composting for beginners",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-03-05T09:00:00Z",
          "ModifiedUtc": "2025-03-05T09:00:00Z",
          "PublishedUtc": "2025-03-05T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Composting for beginners"
          },
          "AutoroutePart": {
            "Path": "composting-for-beginners",
            "SetHomepage": false
          },
          "HtmlBodyPart": {
            "Html": "<p>Kitchen scraps, dry leaves and a little patience turn into rich compost. Spread it on the garden soil in early spring.</p>"
          }
        },
        {
          "ContentItemId": "z5fk2z9ri19r0wyojfljooa5lq",
          "ContentItemVersionId": "saj08xui6d39zzzzg4zdmen2kh",
          "ContentType": "Article",
          "DisplayText": "Choosing tools for the garden",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-03-07T09:00:00Z",
          "ModifiedUtc": "2025-03-07T09:00:00Z",
          "PublishedUtc": "2025-03-07T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Choosing tools for the garden"
          },
          "AutoroutePart": {
            "Path": "choosing-tools-for-the-garden",
            "SetHomepage": false
          },
          "HtmlBodyPart": {
            "Html": "<p>A sturdy spade, a hand fork and sharp pruning shears cover most jobs. Keep the blades clean and dry after each use.</p>"
          }
        },
        {
          "ContentItemId": "vdgaj8gxbenyjqwx4hh5344tfj",
          "ContentItemVersionId": "gvq4k7bn7xj8b7tfq7xkwo886v",
          "ContentType": "Article",
          "DisplayText": "Winter reading list",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2025-03-10T09:00:00Z",
          "ModifiedUtc": "2025-03-10T09:00:00Z",
          "PublishedUtc": "2025-03-10T09:00:00Z",
          "Author": "admin",
          "TitlePart": {
            "Title": "Winter reading list"
          },
          "AutoroutePart": {
            "Path": "winter-reading-list",
            "SetHomepage": false
          },
          "HtmlBodyPart": {
            "Html": "<p>Five long novels for short days, from a sea voyage to a quiet village mystery.</p>"
          }
        }
      ]
    }
  ]
}
```

Check: open Content, Content Items (`OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:68`). The list shows the four articles
next to the About article of the recipe. If the import is refused, create the same four Articles by hand from the
same titles and paths (Article, Title, Autoroute path, body), and publish them.

The word "garden" appears in the title or the body of the first three articles and nowhere else on the site (the recipe
content has no such word). The fourth article, "Winter reading list", is the one a search for "garden" must not list.

### Snapshots

The clips change the site, and each one needs the state the previous one left. Take these copies of the `App_Data`
folder (stop the site first, copy the folder and `state.json`, start the site again):

| Copy | Taken when | Holds |
|---|---|---|
| S0 | After Auto Setup, the Search feature, the seed articles and `Recorder setup` | Lucene disabled, no index |
| S1 | After clip `02-enable` | Lucene enabled, no index |
| S2 | After clip `03-create`, once the index holds the three garden articles (see "Wait conditions") | Lucene enabled, index `search` |

To record everything again, restore S0 and record the clips in order. To record one clip again, restore the copy that
precedes it: `02-enable` from S0, `03-create` from S1, `04-fields` and `05-search` from S2.

## The order of the clips

Record in this order. Each clip needs the one before it, except where noted:

1. `02-enable` starts from S0. **Changes the site** (enables Lucene).
2. `03-create` starts from S1. **Changes the site** (creates the index `search`). It needs Lucene enabled.
3. `04-fields` starts from S2 (it only needs Lucene enabled). **Does not change the site**: it never saves.
4. `05-search` starts from S2. **Does not change the site.** It needs the index `search` and the Search feature.

The slide clips `01-how` and `06-servers` need no recording. The cards need none either.

### Wait conditions

- After clip `03-create`, the index is filled in the background (`OrchardCore/OrchardCore.Indexing.Core/DefaultIndexProfileManager.cs:209`).
  Before taking S2, open `/search/search?terms=garden` outside the capture, signed in, and reload until the page lists
  "Starting a kitchen garden", "Composting for beginners" and "Choosing tools for the garden", and not "Winter reading
  list". Allow up to two minutes.
- If a title is still missing after two minutes, open `/Admin/indexing`, click **Actions** on the row `search`
  (`OrchardCore.Modules/OrchardCore.Admin/Views/AdminListActions.cshtml:32`), choose **Reset**, confirm with **Yes**, and
  wait again. (Reset restarts the indexing from the beginning: `OrchardCore.Modules/OrchardCore.Indexing/Views/IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml`.)
  Do this before the S2 copy, never during a take.
- Whenever a step below says "wait until", the wait is on that exact condition, with a limit of 30 seconds, and the
  condition is checked before the capture starts when the step is the first of its clip.

## Clip `02-enable`

Starts on `/Admin/Features`, signed in (`state.json`). The admin menu is collapsed. Nothing is typed yet.
Route: `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:58`.

- `enable-1` (menu path: Tools, Features: `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`)
  - Actions: `TypeAsync(#search-box, "Lucene", delay: 90)`. The search box has the placeholder "Search"
    (`OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`). It filters the rows by name, id and description
    (`OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`).
  - Wait until the rows whose name does not contain Lucene are hidden (the count "Showing {0} of {1} features" has dropped).
  - On screen when the narration ends: the box holds "Lucene", and under the heading "Search" the rows Lucene,
    Lucene (Obsolete), Lucene Worker and Lucene Content Picker, each with the badge "Disabled" and a blue **Enable** button.
  - Source of the labels: feature names in `OrchardCore.Modules/OrchardCore.Lucene/Manifest.cs:12, 25, 33, 41`; category "Search" on the same file.
- `enable-2`
  - Define `Row(id)` as in the skill's example: `.list-group-item` filtered with `Has` set to `#btn-enable-<id>`.
  - Actions: `HighlightAsync(Row("OrchardCore_Lucene"))`, hold 3 seconds, `ClearHighlightsAsync()`, then
    `HighlightAsync(Row("OrchardCore_Search_Lucene"))` (Lucene (Obsolete)), hold 2 seconds, `ClearHighlightsAsync()`, then
    `HighlightAsync(Row("OrchardCore_Search_Lucene_ContentPicker"))`, hold, and leave the ring in place.
  - On screen when the narration ends: the Lucene Content Picker row is ringed, with its description "Provides a Lucene
    content picker field editor." visible; the three descriptions were visible while ringed.
  - Source: descriptions in `OrchardCore.Modules/OrchardCore.Lucene/Manifest.cs:13, 26, 42`; shown by `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Description.SummaryAdmin.cshtml:8`.
- `enable-3`
  - Actions: `ClearHighlightsAsync()`, `ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_Lucene)`. Enabling restarts
    the tenant, so the page can take several seconds to load: that time is part of the clip, and the step holds until
    the narration ends. When the page has loaded, `TypeAsync(#search-box, "Lucene", delay: 60)` again so the Lucene rows
    are the visible ones.
  - Wait until the page shows the success message and `#btn-disable-OrchardCore_Lucene` exists.
  - On screen when the narration ends: a green message at the top reading "The feature Lucene was enabled." (it reads
    "The following features were enabled: ..." if dependencies were enabled too: see OPEN-QUESTIONS.md), and the Lucene
    row with the badge "Enabled" and a red **Disable** button.
  - Source: button `OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; message
    `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`; badge `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.
  - After the clip, take the copy S1.

## Clip `03-create`

Starts on `/Admin/indexing` (Search, Indexes: `OrchardCore.Modules/OrchardCore.Indexing/AdminMenu.cs:19-22`; route
`OrchardCore.Modules/OrchardCore.Indexing/Controllers/AdminController.cs:60`), from S1. The list is empty and says "Nothing here! There are
no indexes at the moment." (`Views/Admin/Index.cshtml:40`).

Lookups used below:

- `Add` button of the source: `#modalAddIndex .card` filtered by the text "Content in Lucene", then its link with the text "Add".
- Form fields, by label: `GetByLabel("Index Name")`, `GetByLabel("Name", Exact)`, `GetByLabel("Culture")`,
  `GetByLabel("Analyzer", Exact)`, `GetByLabel("Store Source Data")`. "Analyzer" must be an exact match: the page has a
  second label, "Query analyzer" (`OrchardCore.Modules/OrchardCore.Lucene/Views/LuceneQuerySettings.Edit.cshtml:12`), that a
  substring match would also find.
- Content types: `.content-types` and, inside it, `GetByLabel("Article", Exact)`, `GetByLabel("Blog Post", Exact)`,
  `GetByLabel("Page", Exact)`.
- The order of the editors on the page is not relied on (see OPEN-QUESTIONS.md): before each highlight, scroll to the
  element with `ScrollAsync`, clear the rings first, and highlight the element itself.

Steps:

- `create-1`
  - Actions: `ClickAsync(button.create)` (the button "Add index": `Views/Admin/Index.cshtml:31`). Wait until `#modalAddIndex`
    is shown with the title "Available Sources" (line 61). `HighlightAsync` the card "Content in Lucene" under the
    heading "Lucene" (line 74), hold 2 seconds, `ClearHighlightsAsync()`, then
    `ClickAndWaitForNavigationAsync` on the card's **Add** link (`Views/IndexProfile.Link.cshtml:29`).
  - On screen when the narration ends: the page "New 'Content in Lucene' index" (`Views/Admin/Create.cshtml:6`) with an
    empty form and its **Save** button at the bottom.
  - Source of the card text: `OrchardCore.Modules/OrchardCore.Lucene/Startup.cs:98-99`; provider heading `OrchardCore/OrchardCore.Lucene.Core/ServiceCollectionExtensions.cs:60`.
- `create-2`
  - Actions: `TypeAsync(GetByLabel("Index Name"), "search", delay: 90)`. The field "Name" fills in as the letters are
    typed, because it is still untouched (`OrchardCore.Modules/OrchardCore.Indexing/Assets/js/technical-name-generator.js:17-26`; the
    markup is in `Views/IndexProfileFields.Edit.cshtml:6-18`). `HighlightAsync(GetByLabel("Index Name"))`, then
    `HighlightAsync(GetByLabel("Name", Exact))`.
  - Do not type in "Name": it keeps the value "search" that was filled in.
  - On screen when the narration ends: "Index Name" and "Name" both hold `search`, each with a ring.
  - Source: labels "Index Name" and "Name" `Views/IndexProfileFields.Edit.cshtml:6, 13`; the field is locked after saving
    (`asp-is-disabled`, line 8).
- `create-3`
  - Actions: `ClearHighlightsAsync()`, scroll to the "Content types" label, `ClickAsync` on the checkboxes
    "Article", then "Blog Post", then "Page" (labels from the display names of the content types of the Blog recipe), then
    scroll to the "Culture" list and `HighlightAsync(GetByLabel("Culture"))`; it shows "Any culture" and stays unchanged.
  - Leave the checkbox "Index Latest version" (below the content types, with the hint "Check to index draft if it exists,
    otherwise only the published version is indexed.") unchecked. The narration does not describe it.
  - On screen when the narration ends: the checkboxes Article, Blog Post and Page are checked, the other types are not,
    and the Culture list is ringed, showing "Any culture".
  - Source: `OrchardCore.Modules/OrchardCore.Indexing/Views/ContentIndexMetadata.Edit.cshtml:6-9, 17-19, 31-33`; the checkboxes
    `OrchardCore.Modules/OrchardCore.Contents/Views/Shared/Components/SelectContentTypes/Default.cshtml`.
- `create-4`
  - Actions: `ClearHighlightsAsync()`, scroll to the "Analyzer" label, `HighlightAsync(GetByLabel("Analyzer", Exact))`, hold 3
    seconds, `ClearHighlightsAsync()`, `HighlightAsync(GetByLabel("Store Source Data"))`. Do not change either.
  - On screen when the narration ends: "Analyzer" shows its default value (it is the only entry of the list), and "Store
    Source Data" is ringed and unchecked, with its hint "Check to store the fields in the index for future retrieval."
  - Source: `OrchardCore.Modules/OrchardCore.Lucene/Views/LuceneIndexProfile.Edit.cshtml:4, 15, 16`; default `OrchardCore.Modules/OrchardCore.Lucene/Drivers/LuceneIndexProfileDisplayDriver.cs:53`.
- `create-5`
  - Actions: `ClearHighlightsAsync()`, scroll to the heading "Default Query Settings", then `HighlightAsync(#DefaultQueryFields)`
    (the "Query field names" block). Do not tick or untick anything. The settings above it ("Query analyzer", "Version",
    "Allow Lucene queries") stay on screen unchanged: the narration only says to leave them as they are.
  - On screen when the narration ends: the ringed list "Query field names" with its checked entry for the
    full-text field (`Content.ContentItem.FullText`) and the unchecked entries `ContentItemId` and `ContentItemVersionId`, and the
    hint "The fields to use for query. Leave it blank to query all fields."
  - Source: `OrchardCore.Modules/OrchardCore.Lucene/Views/LuceneQuerySettings.Edit.cshtml:7, 37, 38, 61`; the full-text entry is checked
    for a new index by `Drivers/LuceneIndexProfileDisplayDriver.cs:82`.
- `create-6`
  - Actions: `ClearHighlightsAsync()`, scroll to the bottom, `ClickAndWaitForNavigationAsync` on the **Save** button
    (`Views/Admin/Create.cshtml:16`).
  - Wait until the Indexes list shows the row `search`.
  - On screen when the narration ends: the Indexes list with the green message "An index has been created
    successfully. The synchronizing process was triggered in the background." (`Controllers/AdminController.cs:257`) and a row
    with the name `search`, the badges "Lucene" and "Content" (`Views/IndexProfile.DefaultTags.SummaryAdmin.cshtml:6-12`), the
    buttons **Edit** and **Delete**, and **Actions**.
  - After the clip, take S2 once the wait conditions above are met.

## Clip `04-fields`

Starts on `/Admin/ContentTypes/List` (Design, Content Definition, Content Types: `OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25`; route
`Controllers/AdminController.cs:72`), from S2. Lucene is enabled, so the Lucene Index settings appear for the administrator.
**This clip never saves.** Do not click **Save**; leave each page by ending the clip.

- `fields-1`
  - Actions: `ClickAndWaitForNavigationAsync` on `a[role="btn-edit-Article"]` (the button **Edit** of the row Article:
    `Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`). On the page "Edit Content Type - Article" (`Views/Admin/Edit.cshtml:63`),
    scroll to the heading "Full-text" (`h3`), and `HighlightAsync` the heading.
  - On screen when the narration ends: the Full-text section with its hint "The full-text content is used in search
    queries to find content items from free-text searches." and its three options.
  - Source: `OrchardCore.Modules/OrchardCore.Contents/Views/FullTextAspectSettings.Edit.cshtml:15-16, 27, 37, 47`.
- `fields-2`
  - Actions: `ClearHighlightsAsync()`, `HighlightAsync(GetByLabel("Include display text"))`, hold 2 seconds,
    `ClearHighlightsAsync()`, `HighlightAsync(GetByLabel("Include body parts"))`.
  - On screen when the narration ends: "Include display text" and "Include body parts" are checked; "Use custom full-text" is unchecked.
  - Source: defaults `OrchardCore.Modules/OrchardCore.Contents/Models/FullTextAspectSettings.cs:17-21`.
- `fields-3`
  - Actions: `ClearHighlightsAsync()`, scroll to the heading "Parts" (`Views/Admin/Edit.cshtml:144`), and
    `ClickAndWaitForNavigationAsync` on the **Edit** button of the Title row of `#parts` (`#parts li` filtered by the text
    "Title", then its link "Edit": line 163). On the page "Edit Part - Title" (`Views/Admin/EditTypePart.cshtml:22`), scroll to
    the heading "Lucene Index settings", `HighlightAsync(GetByLabel("Include this element in the index"))`, then
    `ClickAsync` that checkbox so that the options below it appear. Do not save.
  - On screen when the narration ends: under the heading "Lucene Index settings", the checkbox "Include this element in
    the index" is checked and ringed, with its hint "Check to include the value of this element in the index.", and the options
    "Keyword" and "Stored" have appeared below it.
  - Source: `OrchardCore.Modules/OrchardCore.Lucene/Views/LuceneContentIndexSettings.Edit.cshtml:5, 13, 14`; part name `OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:60`.
- `fields-4`
  - Actions: `ClearHighlightsAsync()`, `HighlightAsync(GetByLabel("Keyword", Exact))`, hold 3 seconds, `ClearHighlightsAsync()`,
    `HighlightAsync(GetByLabel("Stored", Exact))`.
  - On screen when the narration ends: "Stored" is ringed, with its hint "Check to be able to retrieve the original value from the
    index."; the hint of "Keyword" ("Check to index as a keyword value (indexed as a single token).") was visible while it was ringed.
  - Source: `LuceneContentIndexSettings.Edit.cshtml:19, 20, 24, 25`.
- `fields-5`
  - Actions: `ClearHighlightsAsync()`, `HighlightAsync` the heading "Lucene Index settings" (the `h4` of the settings,
    `LuceneContentIndexSettings.Edit.cshtml:5`; its text reads "Lucene Index settings" on the page). Nothing is clicked.
  - On screen when the narration ends: the heading "Lucene Index settings" ringed, with the checked "Include this element in the
    index" and the two options under it.
  - Source of the narration: the settings are shown only with the permission "Manage Lucene Indexes"
    (`OrchardCore/OrchardCore.Lucene.Core/LuceneSearchPermissions.cs:8`, checked in
    `OrchardCore.Modules/OrchardCore.Lucene/Settings/ContentTypePartIndexSettingsDisplayDriver.cs:27`). The Administrator role holds it
    (`OrchardCore.Modules/OrchardCore.Lucene/Permissions.cs`, default stereotypes).
- `fields-6`
  - Actions: `ClearHighlightsAsync()` and hold. No click: the **Save** button of the page is not used and not ringed.
  - On screen when the narration ends: the same "Edit Part - Title" page, unchanged from `fields-5`, still unsaved.
  - Source of the narration: the full-text field is built by `OrchardCore.Modules/OrchardCore.Contents/Indexing/FullTextContentIndexHandler.cs:19-36`
    whatever the Lucene Index settings of a part say (the settings only matter to `ContentItemIndexCoordinator.cs:57-77`, which adds
    one more field), the display text is in it by default (`FullTextAspectSettings.cs:17-21`), and the search reads the fields of
    "Query field names" (`OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:41-69`), where `create-5` left the full-text field.
  - After the clip, open the Title part of Article again and check that "Include this element in the index" is unchecked.
    If it is checked, restore S2.

## Clip `05-search`

Starts on `/search/search` (the search page of the index named `search`: route `OrchardCore.Modules/OrchardCore.Search/Controllers/SearchController.cs:64`, the index
is found by its name at line 102), signed in as `admin`, from S2. Before the capture, `OpenAsync("/search/search?terms=garden")` and wait
until the three garden articles are listed (see "Wait conditions"), then `OpenAsync("/search/search")`, so the clip starts on the empty search page.
The signed-in administrator holds the permission to query the index (`OrchardCore.Modules/OrchardCore.Search/Controllers/SearchController.cs:110`).

- `search-1`
  - Actions: `MoveToAsync(input[name="Terms"])`, `HighlightAsync(input[name="Terms"], "/search/search")`. The recorder shows no
    address bar, so the caption of the ring writes the address the narration speaks: the page address of this index, the word
    search, a slash, and the index name `search`. The caption is a page address of the site, not code.
  - On screen when the narration ends: the search page of the site with an empty heading, the field with the placeholder
    "Enter your search term(s)" ringed with the caption "/search/search", and the button **Search**.
  - Source: `OrchardCore.Modules/OrchardCore.Search/Views/Search-Form.cshtml:10, 15, 16`; the route `search/{index?}` and the lookup of the
    index by its name: `OrchardCore.Modules/OrchardCore.Search/Controllers/SearchController.cs:64, 102`. The Search feature has no menu
    entry or link to this page, so the address is the way to it.
- `search-2`
  - Actions: `ClearHighlightsAsync()`, `TypeAsync(input[name="Terms"], "garden", delay: 90)`.
  - On screen when the narration ends: the field holds "garden"; nothing has been submitted yet.
- `search-3`
  - Actions: `ClickAndWaitForNavigationAsync` on the button **Search** of the form (`form:has(input[name="Terms"]) button[type="submit"]`).
  - Wait until the page shows three results.
  - On screen when the narration ends: the results list with the summaries of "Starting a kitchen garden", "Composting for beginners"
    and "Choosing tools for the garden" (in the order the page gives them), and the field still holding "garden". The list does
    not show "Winter reading list", and does not show the message "There are no such results."
  - Source: `OrchardCore.Modules/OrchardCore.Search/Views/Search-Results.cshtml:35, 43`; the field searched by default is the full-text
    field, selected in `create-5` (`OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:50`).
- `search-4`
  - Actions: `ClearHighlightsAsync()`, `MoveToAsync(input[name="Terms"])` and hold. No click, no typing.
  - On screen when the narration ends: the same results page, unchanged.
  - Source of the narration: the page asks for the permission "Query any index" on the index
    (`OrchardCore.Modules/OrchardCore.Search/Controllers/SearchController.cs:110`; label `OrchardCore/OrchardCore.Indexing.Core/IndexingPermissions.cs:10`),
    which a role also gets with the permission "Query '{0}' Index" with the Name of the index in place of `{0}`, so "Query 'search' Index"
    here (same file, line 15; `OrchardCore/OrchardCore.Indexing.Core/Handlers/IndexingAuthorizationHandler.cs:34`). Only the Administrator
    role holds "Query any index" by default (`OrchardCore.Modules/OrchardCore.Search/Permissions.cs:18-24`), so a visitor who is not signed in
    needs it granted to the role Anonymous. No clip opens the Roles page: confirm the exact labels there (OPEN-QUESTIONS.md, 7).
- `search-5`
  - Actions: hold. No click, no typing.
  - On screen when the narration ends: the same results page, unchanged.
  - Source of the narration: with no index in the address, the page uses the index named in the settings, and shows the warning
    "No default search index has been configured." when there is none (`SearchController.cs:83-97`). The settings are in
    Settings, Search, Site Search (`OrchardCore.Modules/OrchardCore.Search/AdminMenu.cs:25-27`), field "Default search index"
    (`Views/SearchSettings.Edit.cshtml:6`). No clip opens them, and the plan never saves them.

## Slide steps

`slides.py` draws these; nothing is recorded.

- `why-1`: the heading "Searching needs an index" and four cards: Find by words, An index, A separate service, Inside your site.
- `why-2`: the heading "Where a Lucene index lives" and three cards: One folder for each tenant, On the server, Kept up to date.
- `why-3`: the heading "What the Lucene module adds" and four cards with check marks: Lucene indexes, Analyzer and stored values, What is indexed, A source for site search.
- `servers-1`: the heading "One copy of the index on each server" and three cards: Local files, A local record, Changes elsewhere.
- `servers-2`: the heading "Keeping the indexes up to date" and three cards: The database, Indexes Content Updater, Indexing Worker.
- `servers-3`: the heading "Lucene Worker" and two cards: On the Features page, No task of its own.
- `servers-4`: the heading "One server or several" and two cards: One server, Several servers.

(Their step ids are `why-1`, `why-2`, `why-3`, `servers-1`, `servers-2`, `servers-3` and `servers-4`; the cards `intro` and `outro` have no recording either.)

## Checks for private data

- The only account on screen is `admin` (email `admin@example.com`, never shown in these pages). The articles are invented.
  The index list shows the author `admin`.
- No clip shows the sign-in page, the user list, a token or the password.
- After assembling, OCR the video at one frame per second and at every scene change (ffmpeg `select='gt(scene,0.02)'`), then
  at 5 frames per second, and search the text for: any email address that does not end in `@example.com`, a phone number,
  a real person's name, a host name other than `localhost`, and `<password>`'s actual value. The Blog recipe's sample
  post ("Man must explore, and this is exploration at its greatest") and the stock text of the About article are
  part of the recipe and not personal data, but none of these clips shows them.
- Check the contact sheet of every clip (`review.py`): no error message, no empty list where results are expected.
