# Recording plan: signalr

For the person who records this video with `Recorder`. The video is for developers: its code is on slides (drawn by
`slides.py`, nothing to record), and two chapters are browser recordings, `02-features` and `03-live`. Everything a
recording needs is below; nothing is left to decide. Paths of the form `src/...` are in the Orchard Core checkout the
site runs from (the labels were read at commit `92db4a83`). A selector written as `.file-item` stands for
`scene.Page.Locator(".file-item")`, since the scene's methods take locators.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout (the `main` branch, or
  commit `92db4a83`), built in Release and started with `dotnet run --no-build -c Release` on `http://localhost:5010`.
- **Setup**: Auto Setup through environment variables, so no Setup screen and no login form is recorded. The tenant is
  `Default`, the site name is **Orchard Core**, the time zone is `America/Los_Angeles`, the database is Sqlite, the
  recipe is **Blog**, the user is `admin` with the email `admin@example.com`. The password is chosen outside git and
  passed to both Auto Setup and the recorder as `<password>`.
  See the `orchardcore-tester` skill, `references/autosetup.md`.
- **App_Data**: `ORCHARD_APP_DATA` points at an empty folder outside both repositories.
- **Redis, before the site starts.** The backplane is silently inactive when the site starts without a Redis
  configuration, and a variable set after the site has started does not reach it. Run Redis in Docker, and set the
  configuration in the environment of the site:

  ```bash
  docker run -d --name ocvideo-redis -p 6379:6379 redis:7
  docker exec ocvideo-redis redis-cli ping          # must print PONG
  export OrchardCore__Redis__Configuration=localhost:6379
  ```

  The same variable is documented in `src/docs/reference/modules/AutoSetup/README.md` (the Redis section). The
  video's own feature, SignalR Redis Backplane, reads this configuration
  (`src/OrchardCore.Modules/OrchardCore.SignalR.Redis/Startup.cs:31` returns when Redis is not configured).
- **Environment of the site** (Git Bash), all set before it starts:

  ```bash
  export ORCHARD_APP_DATA=<an empty folder outside both repositories>
  export ASPNETCORE_URLS=http://localhost:5010
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9 NO_PROXY=localhost,127.0.0.1
  export OrchardCore__Redis__Configuration=localhost:6379
  ```

  `HTTP_PROXY` and `HTTPS_PROXY` point at a dead address, so the site never calls an outside service. Redis is on
  `localhost`, which `NO_PROXY` keeps out of the proxy.
- **Recorder**: `VIDEO_BASE_URL=http://localhost:5010`, `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`. Run
  `Recorder setup` once (it writes `state.json`, which is never committed).
- **External services**: Redis (the container above) is the only one. The Azure SignalR Service is not recorded: the
  Azure backplane appears on slides only, with a connection string on `example.com`.
- **Enabled features at the start**: those of the Blog recipe only. SignalR, Media SignalR, Redis and both backplanes
  are not enabled.

### Seed data (outside the capture)

The video needs no content of its own. The Blog recipe puts three image files in the root folder of the Media
Library: `about-bg.jpg`, `home-bg.jpg` and `post-bg.jpg` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`,
the `media` step). They are images of the theme, with no person and no text. Do not upload anything.

The one preparation step, run once after the site is set up and before the first clip (a `PrepareAsync` in
`recorder/Scenes.cs`, or by hand, not recorded):

1. Open `/Admin/Features`, type `media signalr` in `#search-box`, and click `#btn-enable-OrchardCore_Media_SignalR`.
   The page reloads with the message "The feature Media SignalR was enabled." Enabling it also enables SignalR, which
   Media SignalR depends on (`src/OrchardCore.Modules/OrchardCore.Media/Manifest.cs:100-104`).
2. Check the result: type `signalr` in `#search-box`. `#btn-disable-OrchardCore_SignalR` and
   `#btn-disable-OrchardCore_Media_SignalR` exist, `#btn-enable-OrchardCore_SignalR_Redis` and
   `#btn-enable-OrchardCore_SignalR_Azure` exist.
3. Stop the site and copy its `App_Data` folder aside as snapshot **S1** (with `state.json`). Start the site again.

## Order of the clips, and what changes the site

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-features` | S1 | Yes: enables SignalR Redis Backplane and the Redis feature. After it, stop the site and copy `App_Data` aside as snapshot **S2**, then start the site again |
| 2 | `03-live` | S2 | Yes: renames `post-bg.jpg` to `post-header.jpg` |

- `03-live` needs the state `02-features` leaves (Redis backplane enabled, Redis running): record it second.
- To record `02-features` again: stop the site, restore S1, start the site, record it, then make S2 again.
- To record `03-live` again: stop the site, restore S2, start the site, record it. Renaming the file back by hand also
  works, but restoring S2 is the reliable way.
- Before each clip, check that Redis answers (`docker exec ocvideo-redis redis-cli ping`) and that the log of the site
  has no line starting with `'Redis' features are not active` (the Redis module logs it when the configuration is
  missing or invalid: `src/OrchardCore.Modules/OrchardCore.Redis/Startup.cs:41`).

## Clip `02-features` (chapter 02, Three features)

Starts on `/Admin/Features` (`scene.OpenAsync("/Admin/Features")`, signed in, before `StartCaptureAsync`). The page
title is **Features** (`src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:5`), reached from the admin
menu as **Tools**, **Features** (`.../OrchardCore.Features/AdminMenu.cs:26-27`). The list is in its default layout, so a
row is `.list-group-item`. The helper used by the steps:

```csharp
ILocator Row(string featureId)   // featureId with dots as underscores: "OrchardCore_SignalR_Redis"
    => scene.Page.Locator(".list-group-item")
        .Filter(new() { Has = scene.Page.Locator($"#btn-enable-{featureId}, #btn-disable-{featureId}") }).First;
```

The ids of the buttons are `btn-enable-` or `btn-disable-` followed by the feature id with dots turned into
underscores (`.../Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` and `:53`). Nothing is expanded or collapsed
before the capture.

`features-1`
- Actions: `TypeAsync(#search-box, "signalr", delay: 90)`. The list filters as the text is typed (name, id and
  description are searched, ignoring case: `.../Assets/ts/features-index.ts:88-97`,
  `.../Drivers/FeatureEntryDisplayDriver.cs:20`). Then move the cursor over `Row("OrchardCore_SignalR")`,
  `Row("OrchardCore_SignalR_Redis")`, `Row("OrchardCore_SignalR_Azure")` and `Row("OrchardCore_Media_SignalR")`, one
  after the other, in the order the narration names them (about 1 s each).
- On screen when the narration ends: the search box holds `signalr`; four rows are listed. Under the heading
  **Content Management**: **Media SignalR**. Under **Infrastructure**: **SignalR**, **SignalR Azure Backplane**,
  **SignalR Redis Backplane** (the headings are the categories of the manifests). Each row shows its name and its id
  in a code style (`OrchardCore.SignalR`, ...), its description, and a status badge: **Enabled** for SignalR and Media
  SignalR, **Disabled** for the two backplanes.
- Labels from: `src/OrchardCore.Modules/OrchardCore.SignalR/Manifest.cs:13-16`,
  `OrchardCore.SignalR.Redis/Manifest.cs:13-16`, `OrchardCore.SignalR.Azure/Manifest.cs:13-16`,
  `OrchardCore.Media/Manifest.cs:97-99`, `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`,
  `.../FeatureEntry.DefaultMeta.SummaryAdmin.cshtml`.

`features-2`
- Actions: `HighlightAsync(Row("OrchardCore_SignalR"), "No pages of its own")`, hold about 3 s,
  `ClearHighlightsAsync()`. Then `HighlightAsync(Row("OrchardCore_SignalR_Azure").Locator(".direct-dependencies"), "Depends on SignalR")`,
  hold about 3 s, `ClearHighlightsAsync()`. Then
  `HighlightAsync(Row("OrchardCore_SignalR_Redis").Locator(".direct-dependencies"), "Depends on Redis and SignalR")`
  and leave it until the narration ends.
- On screen when the narration ends: the Redis backplane row with a green ring round its dependency badges, which read
  **Redis** and **SignalR** (the badges are sorted by name: `.../Controllers/AdminController.cs:134-139`,
  `.../Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12-19`).
- Labels from: the manifests above (`Dependencies`).

`features-3`
- Actions: `ClearHighlightsAsync()`, then
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("#btn-enable-OrchardCore_SignalR_Redis"))`. The shell restarts,
  so the new page can take a few seconds; the scene waits for it (default timeout 20 s). When the page has loaded,
  `TypeAsync(#search-box, "redis", delay: 40)`, then
  `HighlightAsync(Row("OrchardCore_Redis"), "Enabled with it")`.
- On screen when the narration ends: the confirmation message **The feature SignalR Redis Backplane was enabled.**
  at the top of the page (it names only the feature that was requested:
  `.../Controllers/AdminController.cs:317`); the search box holds `redis`; the rows of **Redis**, **Redis Cache**, ...
  and **SignalR Redis Backplane** with the status **Enabled** on **Redis** and on **SignalR Redis Backplane**, and a ring
  round the **Redis** row. If the confirmation message has already faded by then, see OPEN-QUESTIONS.md, question 4.
- Labels from: `.../Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (**Enable**),
  `.../Controllers/AdminController.cs:317` (the message), `src/OrchardCore.Modules/OrchardCore.Redis/Manifest.cs`
  (the **Redis** feature).

## Clip `03-live` (chapter 03, Live updates in the Media Library)

Starts on **Media**, **Library** (`/Admin/Media`: `src/OrchardCore.Modules/OrchardCore.Media/AdminMenu.cs:18-21`,
`.../Controllers/AdminController.cs:43`), with two tabs open on it, both made before `StartCaptureAsync`:

```csharp
const string Connected = "SignalR connected to MediaHub";
var first = scene.Page;

// The gallery logs Connected with console.debug when its connection to the hub is up, then subscribes to the folder
// it shows. Wait for it around each navigation, so the message is not missed.
await first.RunAndWaitForConsoleMessageAsync(
    () => scene.OpenAsync("/Admin/Media"),
    new() { Predicate = message => message.Text.Contains(Connected), Timeout = 20_000 });
await first.Locator(".file-item").First.WaitForAsync();

var second = await scene.Context.NewPageAsync();
await second.RunAndWaitForConsoleMessageAsync(
    () => second.GotoAsync(scene.BaseUrl + "/Admin/Media", new() { WaitUntil = WaitUntilState.NetworkIdle }),
    new() { Predicate = message => message.Text.Contains(Connected), Timeout = 20_000 });
await second.Locator(".file-item").First.WaitForAsync();

await scene.PauseAsync(1000);   // the SubscribePath calls of both tabs have returned
```

The message is printed by the gallery (`.../Assets/media-gallery/src/services/SignalR.ts:110-113`). If it never appears
(OPEN-QUESTIONS.md, question 7), replace both `RunAndWait...` calls by the plain `OpenAsync` and `GotoAsync` calls and
add `await scene.PauseAsync(3000)` before the `PauseAsync(1000)`. The hub is `/hubs/media`
(`src/OrchardCore.Modules/OrchardCore.Media/Startup.cs:776`); it exists once Media SignalR is enabled (the
preparation). The clip is recorded after `02-features`, so that the Redis backplane is active while the tabs talk to
the hub.

The file list is the table view (the default). A row is `.file-item`; its name is in `.file-name-cell`. The rows of the
root folder are the three files of the Blog recipe; the clip uses `post-bg.jpg`:

```csharp
ILocator Post(IPage page) => page.Locator(".file-item").Filter(new() { HasText = "post-bg.jpg" });
```

`live-1`
- Actions: on the first tab, `HighlightAsync(Post(first), "First tab")`, hold about 2 s, `ClearHighlightsAsync()`, then
  `SwitchToAsync(second)` and `HighlightAsync(Post(second), "Second tab")`.
- On screen when the narration ends: the second tab, showing the same root folder of the **Library** with the same
  files (**Name**, **Last modified**, **Size** columns), and a ring with the caption **Second tab** round the row of
  `post-bg.jpg`.
- Labels from: `.../Services/MediaJSLocalizer.cs:37-39` (column headers), `src/OrchardCore.Modules/OrchardCore.Media/AdminMenu.cs`.

`live-2`
- Actions, on the second tab: `ClearHighlightsAsync()`; click the menu button of the row,
  `Post(second).Locator("a.action-button")` (the three dots, `.../components/FileMenu.vue:3`); in the menu that opens
  (`#overlay_file_menu`), click the item whose label is **Rename**
  (`scene.Page.Locator("#overlay_file_menu").GetByText("Rename", new() { Exact = true })`,
  `.../components/FileMenu.vue:60`); in the dialog **Rename file**, empty the field with `FillAsync("")` on
  `input[name=rename]`, then `TypeAsync(input[name=rename], "post-header.jpg", delay: 55)`; click `#btn-submit`
  (its label is **Ok**); then wait (at most 10 s) until
  `second.Locator(".file-item").Filter(new() { HasText = "post-header.jpg" })` is visible.
- On screen when the narration ends: the dialog is closed and the second tab lists `post-header.jpg` where
  `post-bg.jpg` was.
- Labels from: `.../Services/MediaJSLocalizer.cs:41-42` (**Rename**, **Rename file**), `:54` (**File name**),
  `:69` (**Ok**); `.../components/ModalFileAction.vue:31` (the field) and `:48` (the button).

`live-3`
- Actions: `SwitchToAsync(first)`; define `var renamed = first.Locator(".file-item").Filter(new() { HasText = "post-header.jpg" })`;
  wait (at most 10 s) until `renamed` is visible; then `HighlightAsync(renamed, "Updated without a reload")`.
  Do not reload the first tab.
- On screen when the narration ends: the first tab, with `post-header.jpg` in the list, no `post-bg.jpg`, and a ring with
  the caption **Updated without a reload** round the row. The gallery loads the list again when the hub sends
  `MediaChanged` (`.../Assets/media-gallery/src/services/SignalR.ts:65-68`).
- Labels from: the same files as `live-2`.

After the clip, stop the site; the next take of this clip starts from S2.

## Slide steps (nothing to record)

Each slide is drawn by `slides.py`, one image per step.

- `why-1`: three cards, "A connection stays open", "The server sends", "A hub receives the connections".
- `why-2`: four cards, "Hosting", "The client", "Backplanes", "Hubs stay with their modules".
- `host-1`: `WorkflowsHub.cs` (the class with the `Authorize` attribute) and, beside it, the registration of its policy
  (`src/OrchardCore.Modules/OrchardCore.Workflows/RealTime/WorkflowsHub.cs:12-18`, `RealTimeStartup.cs:20-25`).
- `host-2`: the manifest of Media SignalR with its `Dependencies`, and the `RealTimeStartup` class of Workflows with
  `RequireFeatures` and `MapHub` (`OrchardCore.Media/Manifest.cs:96-106`, `OrchardCore.Workflows/RealTime/RealTimeStartup.cs:13-31`).
- `host-3`: the `HubOptions<MyHub>` and `HubOptions` registrations of the documentation
  (`src/docs/reference/modules/SignalR/README.md`, "Configuring hub options").
- `send-1`: `MediaHub.SubscribePath` (`OrchardCore.Media/Hubs/MediaHub.cs:22-30`).
- `send-2`: `MediaSignalREventHandler`, with `IHubContext<MediaHub>` and the `MediaChanged` message
  (`OrchardCore.Media/Hubs/MediaSignalREventHandler.cs:6-12` and `:51-56`).
- `client-1`: the two script tags of the documentation's "Server-rendered pages" section
  (`<script asp-name="signalr" at="Foot">` is also in `OrchardCore.Workflows/Views/Workflow/Details.cshtml:104`).
- `client-2`: two cards, "Server-rendered pages" and "Bundled apps", the second one bright.
- `scale-1`: three cards, "Instance A", "Instance B", "Backplane".
- `scale-2`: the `Redis` section of the configuration, and three cards on the channel prefix
  (`OrchardCore.SignalR.Redis/Startup.cs:83-87`).
- `scale-3`: the `SignalR:Azure` section with a connection string on `example.com`, and four notes
  (`OrchardCore.SignalR.Azure/Startup.cs:62-81`, `AzureSignalROptionsExtension.cs`).
- `scale-4`: three cards, "One backplane per tenant", "No connection string", "In-memory fallback"
  (`OrchardCore.SignalR.Redis/Startup.cs:38-46`, `OrchardCore.SignalR.Azure/Startup.cs:46-58`).

## After recording

Stop the site by its process ID, and remove the Redis container (`docker rm -f ocvideo-redis`). Delete the copies of
`App_Data` and `state.json` when the video is published.

## Checks for private data

- The only account on the screen is `admin` (`admin@example.com`). No password is typed or shown.
- The Media Library shows three theme images by name only; the clip opens no image. No other file is uploaded.
- The Features page shows module names and ids only.
- The captions drawn by the recorder are the ones written above: "First tab", "Second tab", "Updated without a reload",
  "No pages of its own", "Depends on SignalR", "Depends on Redis and SignalR", "Enabled with it".
- The address bar is not part of the capture. The site name is **Orchard Core**.
- Scan the result with OCR, as `SKILL.md` step 6 says, for any name, email address other than `admin@example.com`,
  address or token before publishing.
