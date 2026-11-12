# Recording plan: Elasticsearch

Read this file and `OPEN-QUESTIONS.md`; nothing else is needed to record the video. In this file a selector stands for
`scene.Page.Locator(...)`, and a source reference is a path under the root of `OrchardCMS/OrchardCore` at the commit the
video was written against (92db4a83). Locators are matched by the text on screen first; the ids named here come from
the source and are the preferred hook when the page has them.

The video has three recordings (`02-enable`, `03-create`, `04-search`) and two chapters made of slides (`01-fit`,
`05-tools`). It is an **admin** video: every recording shows the admin UI and the front end. Never open the pages named
"Index Info" or "Run Elasticsearch Query" on camera: both are JSON editors (see `OPEN-QUESTIONS.md`, question 1).

## 1. The site it needs

### The Orchard Core site

Run `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout (the branch of the video's `source.feature`, here
`main`), built and started as `references/recording.md` of the skill describes, on `http://localhost:5010`, with an
empty `ORCHARD_APP_DATA` folder outside both repositories, provisioned by Auto Setup (see the `orchardcore-tester` skill of
that repository, `references/autosetup.md`, for the host registration). It is a fresh site with fake data only, so it
needs no mask script.

| Setting | Value |
|---|---|
| Tenant | `Default` |
| Recipe | `Blog` (it enables Indexing, Queries, Deployment, Contents and the `Article` content type) |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Database | `Sqlite` |
| Admin user | `admin`, email `admin@example.com`, password `<password>` (passed in `VIDEO_PASSWORD`, chosen outside git, never written in the repository) |

Environment variables of the site process (Git Bash):

```bash
export ORCHARD_APP_DATA=<empty folder outside both repositories>
export ASPNETCORE_URLS=http://localhost:5010
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
# The connection to the Elasticsearch server (see below). Set it before the first start.
export OrchardCore__Search__Elasticsearch__Url=http://localhost
export OrchardCore__Search__Elasticsearch__Ports__0=9200
# No outbound calls, except to the local Elasticsearch server.
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9 NO_PROXY=localhost,127.0.0.1
```

The connection (`Search:Elasticsearch` section, `src/OrchardCore/OrchardCore.Elasticsearch.Core/Services/ElasticsearchConnectionOptionsConfigurations.cs:13`)
must exist **before** the Elasticsearch feature is enabled in `02-enable`: the Elasticsearch services need it as soon as
they are loaded. Do not set `Username` or `Password`: the stand-in server has security turned off.

### The external service: Elasticsearch in Docker

The stand-in is a real Elasticsearch server in a throwaway container. Use the image (name and tag) of the
`docker-compose.yml` next to the Elasticsearch documentation page
(`src/docs/reference/modules/Elasticsearch/docker-compose.yml`, the `image` of the service `es01`: tag `9.5.6` at the
commit above), a single node, with security turned off (as that file does), and 9200 published. Read the image from that
file, so that nothing is typed by hand:

```bash
# <oc> is the root of the Orchard Core checkout
ES_IMAGE=$(grep -m1 'image:' <oc>/src/docs/reference/modules/Elasticsearch/docker-compose.yml | awk '{print $2}')
docker run -d --name oc-video-es -p 9200:9200 \
  -e discovery.type=single-node \
  -e xpack.security.enabled=false -e xpack.security.http.ssl.enabled=false \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  "$ES_IMAGE"
curl -s http://localhost:9200            # wait until it answers with the cluster name and version
```

Elasticsearch needs `vm.max_map_count` of at least 262144. On Linux run `sudo sysctl -w vm.max_map_count=262144`. With
Docker on WSL2, set `kernelCommandLine = "sysctl.vm.max_map_count=262144"` under `[wsl2]` in `%userprofile%\.wslconfig`,
run `wsl --shutdown`, and start Docker again (as the documentation page explains). Remove the container when the video
is recorded: `docker rm -f oc-video-es`.

### Features before recording

Open **Tools > Features** once, outside the capture, and check:

- **Indexing** is enabled (the Blog recipe enables it). If it is not, enable it now, before creating the articles below:
  a content item only reaches an index if Indexing was enabled when it was saved.
- **Elasticsearch**, **Elasticsearch (Obsolete)**, **Elasticsearch Worker**, **Elasticsearch Content Picker** and **Search**
  are all **disabled**. `02-enable` enables Elasticsearch and Search on camera. Enable nothing else.

### Settings

None. **Settings > Search > Site Search** must still have no default search index. Leave the admin list layout at its
default (List, with the Edit and Delete buttons and an Actions menu on every row).

### The fake data (create it outside the capture)

Four published `Article` content items. Open **Tools > Deployments > JSON Import** (`/Admin/DeploymentPlan/Import/Json`,
`src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30`), paste the text below into the box labeled **JSON**, and
select **Import**:

```json
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "4c7qk3m9v2xd8hs1tb5ng6wzae",
          "ContentType": "Article",
          "DisplayText": "Harvest Notes From Willow Creek Orchard",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": { "Title": "Harvest Notes From Willow Creek Orchard" },
          "AutoroutePart": { "Path": "harvest-notes-from-willow-creek-orchard" },
          "HtmlBodyPart": { "Html": "<p>The apple harvest at Willow Creek Orchard starts in early autumn. Pickers fill the first crates before sunrise.</p>" }
        },
        {
          "ContentItemId": "4m2x8dv5qh1tz9kb3ra7jf6ycn",
          "ContentType": "Article",
          "DisplayText": "Pressing Cider In Small Batches",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": { "Title": "Pressing Cider In Small Batches" },
          "AutoroutePart": { "Path": "pressing-cider-in-small-batches" },
          "HtmlBodyPart": { "Html": "<p>A small press turns the autumn harvest into cider. We press once a week and bottle on Fridays.</p>" }
        },
        {
          "ContentItemId": "4r9t3hb7wk5ne2xq8vd1ma6zjg",
          "ContentType": "Article",
          "DisplayText": "Planting Pear Trees In Winter",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": { "Title": "Planting Pear Trees In Winter" },
          "AutoroutePart": { "Path": "planting-pear-trees-in-winter" },
          "HtmlBodyPart": { "Html": "<p>Bare-root pear trees go into the ground in winter. Dig wide, water well and stake each tree.</p>" }
        },
        {
          "ContentItemId": "4f6y1nc8sa3vx5pq2kb9td7hwm",
          "ContentType": "Article",
          "DisplayText": "Visiting The Orchard Shop",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": { "Title": "Visiting The Orchard Shop" },
          "AutoroutePart": { "Path": "visiting-the-orchard-shop" },
          "HtmlBodyPart": { "Html": "<p>The orchard shop sells apples, pears and jars of honey from Tuesday to Saturday.</p>" }
        }
      ]
    }
  ]
}
```

Check **Content > Content Items**: the four articles are listed as published. The Blog recipe also created its own
`Article`, **About** (published, placeholder text only); it is the fifth article of the site, and the index of
`03-create` covers it too. The search term of the video is `harvest`: it is in the title and body of the first seed
article and in the body of the second one, and in no other article.

### Sign in and snapshot

1. `Recorder setup` (user `admin`, password from `VIDEO_PASSWORD`) saves `state.json` (git-ignored).
2. Stop the site. Copy its `App_Data` folder and `state.json` aside as **snapshot S0** (features disabled, four articles
   saved, no index). Start the site again. The Elasticsearch container holds no index at this point.

## 2. Order of the clips

Record in the storyboard's order; each clip starts from the state the previous one left:

| Clip | Starts from | Changes the site |
|---|---|---|
| `02-enable` | S0 | Enables the Elasticsearch and Search features. |
| `03-create` | the end of `02-enable` (call it S1) | Creates the `articles` index in the site **and on the Elasticsearch server**. |
| `04-search` | the end of `03-create` (S2) | Sets the default search index. |

`01-fit` and `05-tools` are slides (nothing to record). To record a clip again, restore the snapshot it starts from:
stop the site, put back its `App_Data` and `state.json`, and start it again. The Elasticsearch server must match the
snapshot. S0 and S1 have no index: before recording `03-create` again, delete the index with
`curl -X DELETE http://localhost:9200/default_articles` (or remove and start the container again). S2 has the index and
its documents: before recording `04-search` again, keep the container as it is and run the `_count` check below. Take S1
after `02-enable` and S2 after `03-create` (once the count below is right), the same way as S0.

Between `03-create` and `04-search`, outside the capture, wait until the index holds the articles:

```bash
curl -s "http://localhost:9200/default_articles/_count"      # expect "count":5
```

The index is filled in the background: repeat the command for up to a minute before drawing a conclusion. The count is
5 when the four seed articles and **About** are indexed. A count of 4 is acceptable if only **About** is missing (the
search for `harvest` does not depend on it). If it stays below 4, in particular at 0, the articles were saved before
Indexing was enabled: open each one under **Content > Content Items**, select **Edit**, then **Publish**, and check the
count again.

## 3. The clips

Before each `StartCaptureAsync`, expand the admin menu groups that the clip's first step names (clicking them is not
part of the story), so the step only clicks the last entry. Keep the actions of a step shorter than its narration.

### `02-enable` (recording, page `/Admin`, menu group **Tools** expanded)

Open the page with `scene.OpenAsync("/Admin")`, expand **Tools**, then `StartCaptureAsync()`.

**`enable-1`**
- Actions: `ClickAndWaitForNavigationAsync` on the admin menu entry **Features**. Then `TypeAsync` on the filter box
  (`#search-box`, placeholder **Search**) with `Elasticsearch` (delay 60), and wait until the list shows the filtered rows.
- Visible at the end: the **Features** page, the filter box holding `Elasticsearch`, and the rows **Elasticsearch**,
  **Elasticsearch (Obsolete)**, **Elasticsearch Worker** and **Elasticsearch Content Picker** under the **Search** heading,
  each followed by its id (`OrchardCore.Elasticsearch`, `OrchardCore.Search.Elasticsearch`,
  `OrchardCore.Search.Elasticsearch.Worker`, `OrchardCore.Search.Elasticsearch.ContentPicker`). If another row is also
  listed, leave it: the narration names the four.
- Labels: `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:27` (Tools, Features);
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27` (filter box);
  `src/OrchardCore.Modules/OrchardCore.Elasticsearch/Manifest.cs:12, 25, 33 and 41` (the four names).

**`enable-2`**
- Actions: `HighlightAsync` on the **Elasticsearch** row; `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync` on
  the **Enable** button of the **Elasticsearch** row (id `btn-enable-OrchardCore_Elasticsearch`; if the id differs, the
  **Enable** button inside the row whose id text is exactly `OrchardCore.Elasticsearch`). Wait until the text
  `The feature Elasticsearch was enabled.` is shown.
- Visible at the end: the **Features** page, reloaded, with the green message "The feature Elasticsearch was enabled."
- Labels: `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable);
  `src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317` (the message).

**`enable-3`**
- Actions: `TypeAsync` on `#search-box` with `Search` (clear the box first); wait until the row **Search**
  (id text `OrchardCore.Search`, description "Provides frontend search capabilities against indexes.") is listed;
  `HighlightAsync` on it; `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync` on its **Enable** button
  (`btn-enable-OrchardCore_Search`, or the button inside that row). Wait until `The feature Search was enabled.` is shown.
- Visible at the end: the green message "The feature Search was enabled."
- Labels: `src/OrchardCore.Modules/OrchardCore.Search/Manifest.cs:13` (the description); same files as `enable-2`.

**`enable-4`**
- Actions: expand the admin menu group **Search**, then its group **Queries** (`ClickAsync` on each, if collapsed);
  `HighlightAsync` on the entry **Run Elasticsearch Query** with no caption. Do not select it.
- Visible at the end: the admin menu open on Search > Queries, listing **All Queries**, **Run SQL Query** (the Blog
  recipe enables SQL Queries) and **Run Elasticsearch Query**, the last one inside a green ring.
- Labels: `src/OrchardCore.Modules/OrchardCore.Elasticsearch/AdminMenu.cs:34` (Run Elasticsearch Query; the entry exists
  only when the connection holds a valid address, line 24); `src/OrchardCore.Modules/OrchardCore.Queries/AdminMenu.cs:18-22`
  (Search, Queries, All Queries); `src/OrchardCore.Modules/OrchardCore.Queries/Sql/AdminMenu.cs:20` (Run SQL Query).

### `03-create` (recording, page `/Admin`, menu group **Search** expanded)

Open the page with `scene.OpenAsync("/Admin")`, expand **Search**, then `StartCaptureAsync()`.

**`create-1`**
- Actions: `ClickAndWaitForNavigationAsync` on the admin menu entry **Indexes** (inside Search); wait until the heading
  **Indexes** and the button **Add index** are shown; `HighlightAsync` on **Add index**.
- Visible at the end: the **Indexes** page with the message "Nothing here! There are no indexes at the moment." and the
  button **Add index** inside a green ring.
- Labels: `src/OrchardCore.Modules/OrchardCore.Indexing/AdminMenu.cs:19-22` (Search, Indexes);
  `src/OrchardCore.Modules/OrchardCore.Indexing/Views/Admin/Index.cshtml:31` (Add index) and `:40` (empty message).

**`create-2`**
- Actions: `ClearHighlightsAsync()`; `ClickAsync` on **Add index**; wait until the dialog **Available Sources** is shown;
  `HighlightAsync` on the card **Content in Elasticsearch** (below the heading **Elasticsearch**; its hint reads
  "Create an Elasticsearch index based on site contents."); `ClearHighlightsAsync()`; `ClickAndWaitForNavigationAsync`
  on the **Add** button of that card.
- Visible at the end: the editor page titled **New 'Content in Elasticsearch' index**, with the field **Index Name** at the top.
- Labels: `src/OrchardCore.Modules/OrchardCore.Indexing/Views/Admin/Index.cshtml:61` (Available Sources);
  `src/OrchardCore.Modules/OrchardCore.Elasticsearch/Startup.cs:114-115` (card title and hint);
  `src/OrchardCore/OrchardCore.Elasticsearch.Core/ServiceCollectionExtensions.cs:36` (the heading Elasticsearch);
  `src/OrchardCore.Modules/OrchardCore.Indexing/Views/IndexProfile.Link.cshtml:29` (Add);
  `src/OrchardCore.Modules/OrchardCore.Indexing/Views/Admin/Create.cshtml:6` (the title).

**`create-3`**
- Actions: `TypeAsync` on the field **Index Name** with `articles` (delay 70); `HighlightAsync` on **Index Name**, then,
  after about 3 seconds, `ClearHighlightsAsync()` and `HighlightAsync` on **Name**.
- Visible at the end: **Index Name** holds `articles` and **Name** holds `articles` (it was filled in by the page); the
  hint "A unique name for the index." is under **Name**; the ring is on **Name**.
- Labels: `src/OrchardCore.Modules/OrchardCore.Indexing/Views/IndexProfileFields.Edit.cshtml:6` (Index Name), `:14` and `:18`
  (Name and its hint). The filling in of **Name** is `src/OrchardCore.Modules/OrchardCore.Indexing/Assets/js/technical-name-generator.js`.

**`create-4`**
- Actions: `ClearHighlightsAsync()`; scroll (`ScrollAsync(0, 300)`) until **Content types** and **Culture** are both on
  screen; `ClickAsync` on the checkbox labeled **Article** (the checkbox whose value is `Article`, under **Content types**);
  `HighlightAsync` on the select **Culture** (it shows **Any culture**).
- Visible at the end: **Content types** with **Article** checked (and no other type), **Culture** on **Any culture** inside
  a green ring, and **Index Latest version** unchecked.
- Labels: `src/OrchardCore.Modules/OrchardCore.Indexing/Views/ContentIndexMetadata.Edit.cshtml:6` (Culture), `:9` (Any
  culture), `:17` (Content types), `:31` (Index Latest version).

**`create-5`**
- Actions: `ClearHighlightsAsync()`; scroll until the label **Analyzer** (match the label text exactly, since **Query
  analyzer** also contains the word) and the checkbox **Store Source Data** are on screen; `HighlightAsync` on the select
  **Analyzer** (it shows `standard`), then `ClearHighlightsAsync()` and `HighlightAsync` on **Store Source Data**.
  The page does not fix the order of the content-type block and this block: scroll to whichever comes first.
- Visible at the end: the select **Analyzer** showing `standard`, and the checkbox **Store Source Data** checked, inside a
  green ring, with its hint "Check to store the fields in the index for future retrieval."
- Labels: `src/OrchardCore.Modules/OrchardCore.Elasticsearch/Views/ElasticsearchIndexProfile.Edit.cshtml:6` (Analyzer)
  and `:17` (Store Source Data).

**`create-6`**
- Actions: `ClearHighlightsAsync()`; scroll until the heading **Default Query Settings** is on screen with **Search type**
  and **Query field names** below it; `HighlightAsync` on the select **Search type**, then `ClearHighlightsAsync()` and
  `HighlightAsync` on the list under **Query field names**.
- Visible at the end: **Default Query Settings**; **Search type** showing **Multi-Match Query (Default)**; the list
  **Query field names** with the entry `Content.ContentItem.FullText` checked. If that entry is not checked on the site,
  stop and report it (`OPEN-QUESTIONS.md`, question 4): the narration says it is.
- Labels: `src/OrchardCore.Modules/OrchardCore.Elasticsearch/Views/ElasticsearchQuerySettings.Edit.cshtml:6`
  (Default Query Settings), `:18` (Search type), `:35` (Query field names);
  `src/OrchardCore.Modules/OrchardCore.Elasticsearch/Drivers/ElasticsearchIndexProfileDisplayDriver.cs:73`
  (Multi-Match Query (Default)).

**`create-7`**
- Actions: `ClearHighlightsAsync()`; scroll to the bottom; `ClickAndWaitForNavigationAsync` on **Save**. Wait until
  the text `An index has been created successfully.` and the row `articles` are shown; `MoveToAsync` the badges of the row.
- Visible at the end: the **Indexes** page with the green message "An index has been created successfully. The
  synchronizing process was triggered in the background.", and the row **articles** with the badges **Elasticsearch**
  and **Content**.
- Labels: `src/OrchardCore.Modules/OrchardCore.Indexing/Views/Admin/Create.cshtml:16` (Save);
  `src/OrchardCore.Modules/OrchardCore.Indexing/Controllers/AdminController.cs:257` (the message);
  `src/OrchardCore.Modules/OrchardCore.Indexing/Views/IndexProfile.DefaultTags.SummaryAdmin.cshtml:5-12` (the badges);
  `src/OrchardCore/OrchardCore.Indexing.Core/IndexingConstants.cs:5` (the type `Content`).

### `04-search` (recording, page `/Admin`, menu groups **Settings** and **Search** expanded)

Run the `_count` check of section 2 first. Open the page with `scene.OpenAsync("/Admin")`, expand **Settings** and its
group **Search**, then `StartCaptureAsync()`.

**`search-1`**
- Actions: `HighlightAsync` on the entry **Site Search** (no caption); `ClearHighlightsAsync()`;
  `ClickAndWaitForNavigationAsync` on it; `SelectAsync` on **Default search index** with the label `articles`; wait about
  1 second; `ClickAndWaitForNavigationAsync` on **Save**. Wait until `Site settings updated successfully.` is shown.
- Visible at the end: the **Site Search** settings page with **Default search index** on `articles`, the field
  **Placeholder** empty, and the green message "Site settings updated successfully."
- Labels: `src/OrchardCore.Modules/OrchardCore.Search/AdminMenu.cs:25-27` (Settings, Search, Site Search);
  `src/OrchardCore.Modules/OrchardCore.Search/Views/SearchSettings.Edit.cshtml:6` (Default search index) and `:9`;
  `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (Save);
  `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119` (the message).

**`search-2`**
- Actions: `NavigateAsync("/search")`; `TypeAsync` on the search box (placeholder "Enter your search term(s)") with
  `harvest` (delay 70); `ClickAndWaitForNavigationAsync` on the button **Search**.
- Visible at the end: the results page, with the search box holding `harvest`.
- Labels: `src/OrchardCore.Modules/OrchardCore.Search/Views/Search-Form.cshtml:10` (the placeholder) and `:16` (Search);
  the route `search/{index?}` is in `src/OrchardCore.Modules/OrchardCore.Search/Controllers/SearchController.cs`.

**`search-3`**
- Actions: `MoveToAsync` the first result's title, then the second one's; `HighlightAsync` on the list of results
  (the `ul` with the class `list-group`), no caption.
- Visible at the end: the two articles that contain the word, "Harvest Notes From Willow Creek Orchard" and "Pressing
  Cider In Small Batches" (in either order), inside a green ring. If a different number of results is shown, stop and
  report it: the index is not the one this plan describes.
- Labels: the titles are the fake data of section 1; the list is
  `src/OrchardCore.Modules/OrchardCore.Search/Views/Search-Results.cshtml`.

**`search-4`**
- Actions: none. The step holds until the narration ends. Leave the results and the ring as they are.
- Visible at the end: the same results page as at the end of `search-3`.
- Labels: the permissions are named `Query any index` and `Query '{0}' Index` in
  `src/OrchardCore/OrchardCore.Indexing.Core/IndexingPermissions.cs:10` and `:15`.

### The slides

Drawn by `slides.py`; nothing to record.

- `fit-1`: heading "Elasticsearch runs on its own server", four cards: a cloud service, your own machines, Docker, and a server that is already running.
- `fit-2`: heading "The site connects to it", three cards: the connection is part of the site configuration, it holds a server address (and credentials), and Run Elasticsearch Query stays hidden until the address is valid.
- `fit-3`: heading "Index, then search", four cards: an index is a copy of the content of the chosen types on the server, it is kept current, the search page reads it, and Lucene can run beside Elasticsearch.
- `tools-1`: heading "More Elasticsearch tools", four cards, the first one bright: Query Index and Index Info.
- `tools-2`: the same slide, the second card bright: Run Elasticsearch Query.
- `tools-3`: the same slide, the third card bright: Elasticsearch Index settings, with "Include this element in the index".
- `tools-4`: the same slide, the fourth card bright: Synchronize, Reset, Rebuild.

## 4. Checks for private data

- The only account on screen is `admin` / `admin@example.com`; the site name is `Orchard Core`. The articles are the four
  fake ones above (a made-up orchard) and **About**, the placeholder article of the Blog recipe.
- The Elasticsearch server runs on `localhost` with security turned off: no key, user name or password is typed or shown.
  The docker command and the environment variables are never on camera.
- Look at every contact sheet (`review.py`): no error page, no "Elasticsearch is not configured" page, no empty results.
- OCR the finished video at one frame per second, at every scene change, and at five frames per second, as `SKILL.md` step 6
  says, and search for real names, email addresses other than `admin@example.com`, and any token.
- Stop the site by its process id, remove the container (`docker rm -f oc-video-es`), and keep the snapshots outside git.
