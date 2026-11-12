# Recording plan: Blazor Server front end for Orchard Core content

How to record `blazor-cms` without deciding anything. The video is a **developer** video: the code is on slides (made by
`slides.py`), and the browser is recorded only for the screens of the story: the admin pages that create the content
and the tenant, and the Blazor site itself. Terminal commands, files and code are never recorded.

Where a selector appears below, it stands for `scene.Page.Locator(...)`. File references (`file:line`) are in the
Orchard Core source (`OrchardCMS/OrchardCore`, `src/OrchardCore.Modules/...`) unless they start with `guide`, which is
`src/docs/guides/create-blazor-cms/README.md` of the same repository. `<password>` is the value of `VIDEO_PASSWORD`,
chosen outside git; it is never written in a file, and it is the same for the default tenant and the tenant.

## The site it needs

There is no Blazor site in the Orchard Core source (`src/OrchardCore.Cms.Web` has no Razor components), and the
guide's companion sample is not kept in this repository, so **the recorder builds the guide's result** as a throwaway
solution outside both repositories. This is the only site the video needs. It is recorded as built here, never from
the sample of another source.

### Build the host (Build A)

Follow the guide section by section, with the deviations listed after the table.

| Guide section | Lines | What to create |
|---|---|---|
| Create Orchard Core CMS Application | guide:16-30 | `dotnet new sln`, `dotnet new occms -o BlazorCms`, `dotnet sln add ./BlazorCms` (solution `OCBlazor`) |
| Create a Blazor App in the Razor Class library | guide:147-175 | library `OCBlazorLib` (`dotnet new razorclasslib`), added to the solution, referenced by `BlazorCms`, with the `FrameworkReference` of `Microsoft.AspNetCore.App` |
| The structure of the Blazor app | guide:177-550 | `App.razor`, `App.razor.css`, `_Imports.razor`, `Layout/MainLayout.razor(.css)`, `Layout/NavMenu.razor(.css)`, `Routes.razor` |
| Pages | guide:554-597 | `Pages/Home.razor`, `Pages/Content.razor` |
| Serving Blazor App from Orchard Core CMS | guide:601-622 | the `.ConfigureServices(...)` and `.Configure(...)` callbacks in `BlazorCms/Program.cs`, in the commented block `// Orchard Specific Pipeline` of the template (`src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Web/Program.cs:24-28`), with `using OCBlazorLib;` at the top of the file, because `routes.MapRazorComponents<App>()` names the `App` component of the library (the guide's snippet leaves the `using` out) |
| Add Orchard Core CMS content to the Blazor Page | guide:628-726 | the package reference, the `@using` lines of `_Imports.razor`, the final `Content.razor` (route, injections, properties, `OnInitializedAsync` with `Task.Delay(500)`, `OnParametersSetAsync`, the `@if (ContentItem == null)` markup) and `@attribute [StreamRendering]` |
| Add multitenancy to the Blazor App | guide:730-775 | `Home.razor` with `@page "/home"`, `NavMenu.razor` with `href="home"` |
| The base URL | guide:803-853 | `App.razor` with `@inject NavigationManager NavManager`, `<base href=@_baseUrl />` and `OnParametersSet` |

Deviations from the guide (all of them are on purpose):

1. **One Orchard Core version.** The host and the library reference Orchard Core packages of the same version, the
   release the video is recorded against. Do not copy `--version 3.0.1` (guide:635): the guide's version is not the
   one of the source. Check with `dotnet list package` that every `OrchardCore.*` package of both projects shows
   the same version. Create the library with the host's target framework (`dotnet new razorclasslib -f <that
   framework>`).
2. **NavMenu links** are `home` (Home) and `content/orchard-core` (Orchard Core). The guide's final `NavMenu.razor`
   (guide:768) links to `content`, which no route matches, because `Content.razor` is `@page "/content/{alias}"`.
3. **Stylesheet.** `App.razor` links `_content/OCBlazorLib/OCBlazorLib.bundle.scp.css` (guide:829), not
   `_content/BlazorCms/styles.css` (guide:193). Both versions of `App.razor` keep the Bootstrap link
   `OrchardCore.Resources/Styles/bootstrap.min.css`.
4. **Auto Setup**, so no setup form is recorded for the default tenant: in `BlazorCms/Program.cs` write
   `.AddSetupFeatures("OrchardCore.AutoSetup")` right after `.AddOrchardCms()`, as `src/OrchardCore.Cms.Web/Program.cs:9`
   does, then chain the guide's `.ConfigureServices(...)` and `.Configure(...)`.
5. **The Interactive page is not in Build A.** It is added in Build B (below), after every other clip is recorded.

### Build B: Build A plus the Interactive page

Copy the build, then add, in `OCBlazorLib`:

- `Components/InteractiveButton.razor`, exactly the component of guide:996-1012;
- `Pages/Interactive.razor`, with `@page "/interactive"`, `<PageTitle>Interactive</PageTitle>` and
  `<OCBlazorLib.Components.InteractiveButton @rendermode="RenderMode.InteractiveServer" />` (guide:1017);
- a NavLink in `NavMenu.razor`: `href="interactive"`, text `Interactive`.

The guide puts the button on a page it calls BaseUrl and never gives. The video does not show or mention that page.
Build B is needed only for clip `08-click`, so that the earlier clips do not show a link that is not explained yet.

### Running it

- Folder of the site's data outside both repositories: `ORCHARD_APP_DATA=<data>` (one folder, kept in the snapshots below).
- `ASPNETCORE_ENVIRONMENT=Development` (the library's style sheet is served as a static web asset), and
  `ASPNETCORE_URLS=http://localhost:5010` (HTTP only: with no HTTPS port, the HTTPS redirection of the template does
  nothing). `VIDEO_BASE_URL=http://localhost:5010`.
- Block outbound calls: `HTTP_PROXY=http://127.0.0.1:9` and `HTTPS_PROXY=http://127.0.0.1:9`. The site calls no external service.
- Auto Setup of the default tenant, by environment variables (`src/docs/reference/modules/AutoSetup/README.md`):

  | Variable (prefix `OrchardCore__AutoSetup__`) | Value |
  |---|---|
  | `Tenants__0__ShellName` | `Default` |
  | `Tenants__0__SiteName` | `Orchard Core` |
  | `Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `Tenants__0__AdminUsername` | `admin` |
  | `Tenants__0__AdminEmail` | `admin@example.com` |
  | `Tenants__0__AdminPassword` | `<password>` |
  | `Tenants__0__DatabaseProvider` | `Sqlite` |
  | `Tenants__0__RecipeName` | `Headless` (the recipe displayed as **Headless site**, `src/OrchardCore.Themes/TheAdmin/Recipes/headless.recipe.json:2-3`) |

- Start the site, open `http://localhost:5010/` once so that Auto Setup runs, then sign the recorder in:
  `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` (it writes `state.json`, git-ignored).

### Features, settings, seed data

- **Features to enable before recording:** none. The Headless site recipe already enables `OrchardCore.Alias`,
  `OrchardCore.Markdown`, `OrchardCore.Title`, `OrchardCore.Contents`, `OrchardCore.ContentTypes`,
  `OrchardCore.Deployment` and `OrchardCore.Features` (`headless.recipe.json:20-61`). **`OrchardCore.Tenants` must be
  disabled** when a take of `06-tenant` starts: the clip enables it.
- **Settings:** none. Leave **Base url** (Settings, General) empty: the narration of `base-3` says it is empty unless set.
- **External services:** none.
- **Seed data**, outside the capture: none for the default tenant. Its content is imported on camera (`content-1`),
  from the recipe below. The tenant's content is imported off camera (see "Between 06-tenant and 07-site"), from the
  same recipe with three values changed.

The recipe of the default tenant (the guide's recipe, guide:44-141, with the content definition first and without the
`Owner` of the item):

```json
{
  "name": "",
  "displayName": "",
  "description": "",
  "author": "",
  "website": "",
  "version": "",
  "issetuprecipe": false,
  "categories": [],
  "tags": [],
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "MarkdownPage",
          "DisplayName": "Markdown Page",
          "Settings": {
            "ContentTypeSettings": {
              "Creatable": true,
              "Listable": true,
              "Draftable": true,
              "Versionable": true,
              "Securable": true
            },
            "FullTextAspectSettings": {},
            "GraphQLContentTypeSettings": {}
          },
          "ContentTypePartDefinitionRecords": [
            { "PartName": "MarkdownPage", "Name": "MarkdownPage", "Settings": {} },
            { "PartName": "AliasPart", "Name": "AliasPart", "Settings": { "ContentTypePartSettings": { "Position": "1" } } },
            { "PartName": "MarkdownBodyPart", "Name": "MarkdownBodyPart", "Settings": { "ContentTypePartSettings": { "Position": "3" } } },
            { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } }
          ]
        }
      ],
      "ContentParts": []
    },
    {
      "name": "Content",
      "data": [
        {
          "ContentItemId": "48swn17t4vn6dwvgkzk970q6dp",
          "ContentItemVersionId": "4x0wa7sfp911brv9gmvdmp7rmf",
          "ContentType": "MarkdownPage",
          "DisplayText": "ORCHARD CORE",
          "Latest": true,
          "Published": true,
          "ModifiedUtc": "2024-01-27T20:26:30.0529958Z",
          "PublishedUtc": "2024-01-27T20:26:30.0704945Z",
          "CreatedUtc": "2024-01-27T20:26:08.2132621Z",
          "Author": "admin",
          "MarkdownPage": {},
          "AliasPart": { "Alias": "orchard-core" },
          "MarkdownBodyPart": {
            "Markdown": "## Introduction \r\nOpen-source, modular, multi-tenant application framework and CMS for ASP.NET Core\r\n\r\n## Features\r\n- Media Management\r\n- Multi-Lingual\r\n- Modular\r\n- Multi-Tenant\r\n- Security\r\n- Workflows"
          },
          "TitlePart": { "Title": "ORCHARD CORE" }
        }
      ]
    }
  ]
}
```

The recipe of the tenant is the same, with `DisplayText` and `TitlePart.Title` set to `ORCHARD CORE FOR TENANT 01`
(the guide sets only `DisplayText`; setting both keeps the title field and the heading the same, and an import does not
recompute the display text: `src/OrchardCore.Modules/OrchardCore.Title/Handlers/TitlePartHandler.cs:33-50` handles
the updated, created, cloned and publishing events, and an editable title keeps a display text that is set), and the
last item of the Markdown list changed from `- Multi-Tenant` to `- Multi-Tenant (as we are seeing)`.

## Order of the clips, and snapshots

A snapshot is a copy of the `<data>` folder and of `state.json`, kept outside git. Stop the site before copying or
restoring, and start it again afterwards.

| Order | Clip | Starts from | Changes the site? | After the take |
|---|---|---|---|---|
| 1 | `02-content` | **R0**: Auto Setup done, `state.json` saved, no Markdown Page type, **Tenants disabled** | Yes: imports the type and the item | snapshot **R1** |
| 2 | `05-run` | R1 | No | |
| 3 | `06-tenant` | R1 | Yes: enables Tenants, creates the tenant `FirstTenant` (uninitialized) | snapshot **R2** |
| 4 | Off camera, "Between 06-tenant and 07-site" | R2 | Yes: sets the tenant up, imports its item | snapshot **R3** |
| 5 | `07-site` | R3 | No | |
| 6 | `08-click`, on **Build B** | R3 | No | |

- Clips `05-run`, `07-site` and `08-click` change nothing: record them again from their snapshot at any time.
- To take `02-content` or `06-tenant` again, restore the snapshot before it (R0 or R1), then repeat what follows.
- A take of `06-tenant` after R3 needs R1 again, and then the off-camera steps again: the tenant cannot be created twice.
- Record every clip but `08-click` on Build A, and `08-click` on Build B, started on the data of R3.
- The slide steps need no recording.

## Between 06-tenant and 07-site (off camera)

Done in a browser that is not recording, on the site of R2:

1. In the Tenants list, select **Setup** in the row of `FirstTenant`. The button opens the tenant's URL with a token
   (valid 24 hours): the Setup screen refuses a tenant URL typed without it
   (`OrchardCore.Tenants/Views/ManageTenantActionButtons.cshtml:24`, `OrchardCore.Setup/Controllers/SetupController.cs:63,88,265-275`).
2. On the Setup screen, the **Recipe** list is not shown (`OrchardCore.Setup/Views/Setup/Index.cshtml:124`). Enter
   **What is the name of your site?** `First Tenant Site` (line 118), **Default Time Zone** `America/Los_Angeles` (line 159); if
   **What type of database to use?** is shown, choose `Sqlite`; **User Name** `admin` (line 222), **Email**
   `admin@example.com` (line 227), **Password** and **Password Confirmation** `<password>` (lines 234, 244). Both password fields
   are password inputs (`OrchardCore.Setup/ViewModels/SetupViewModel.cs:42,45`); select **Finish Setup** (line 256).
3. Open `/tenant01/Admin`: it redirects to the sign-in page of the tenant. Sign in as `admin` with `<password>`.
4. Open `/tenant01/Admin/DeploymentPlan/Import/Json`. Set the editor with the recipe of the tenant
   (`document.querySelector('.CodeMirror').CodeMirror.setValue(json)`) and select **Import**. Wait for the notification
   **Recipe imported successfully!** (`OrchardCore.Deployment/Controllers/ImportController.cs:185`).
5. Open `/tenant01/home`, then select **Orchard Core** in the sidebar: the heading is `ORCHARD CORE FOR TENANT 01`.
   Open `/content/orchard-core` on the default tenant: the heading is `ORCHARD CORE`. Take snapshot R3.

## Clips

### `02-content` (chapter 2, "The content")

- **Starts on:** `/Admin/DeploymentPlan/Import/Json`, signed in (`state.json`), opened with `OpenAsync`, not recorded.
  Before `StartCaptureAsync`: wait for the selector `.CodeMirror`, then set its value to the recipe of the default
  tenant with `scene.Page.EvaluateAsync("json => document.querySelector('.CodeMirror').CodeMirror.setValue(json)", recipe)`.
- `content-1`: `HighlightAsync(.CodeMirror)` for two seconds, `ClearHighlightsAsync()`, then
  `ClickAndWaitForNavigationAsync(button.import)` (the **Import** button). Wait until the notification
  **Recipe imported successfully!** is visible.
  - *Visible when the narration ends:* the **JSON Import** page, the notification **Recipe imported successfully!**, the editor empty.
  - *Labels:* Tools, Deployments, JSON Import: `OrchardCore.Deployment/AdminMenu.cs:18,19,30`; **JSON** and **Import**:
    `OrchardCore.Deployment/Views/Import/Json.cshtml:15,21`; the notification: `ImportController.cs:185`; **Headless site**:
    `headless.recipe.json:3`.
- `content-2`: `NavigateAsync("/Admin/Contents/ContentItems")`; wait until the row `ORCHARD CORE` is visible;
  `HighlightAsync` on the type badge of that row, which reads `MarkdownPage`.
  - *Visible:* the **Manage Content** list with the item `ORCHARD CORE` and the type badge `MarkdownPage`.
  - *Labels:* Content, Content Items: `OrchardCore.Contents/AdminMenu.cs:65,68`; **Manage Content**:
    `OrchardCore.Contents/Views/Admin/List.cshtml:17`; the badge shows the technical name of the type, not its display
    name (`OrchardCore.Contents/Views/ContentsType_SummaryAdmin.cshtml:6`, `@contentItem.ContentType`), so the list reads
    `MarkdownPage`; **Markdown Page**, with a space, is the recipe's `DisplayName`, used by the narration for the type.
    The **Edit** button is on the row with the default list layout (List, actions as Buttons,
    `OrchardCore.Admin.Abstractions/AdminListOptions.cs`); if the site shows the actions in a menu, open the menu first.
- `content-3`: `ClickAndWaitForNavigationAsync(a.edit)` in the row (the **Edit** button,
  `OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`); wait until the **Alias** field has the
  value `orchard-core`; `HighlightAsync` on the **Title** field, then on the **Alias** field.
  - *Visible:* the edit page of the item: **Title** `ORCHARD CORE`, **Alias** `orchard-core`, and the Markdown editor.
  - *Labels:* **Title**: `OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`; **Alias**: `OrchardCore.Alias/Views/AliasPart.Edit.cshtml:10`.

### `05-run` (chapter 5, "Rendering content items")

- **Starts on:** `/home`, opened with `OpenAsync`; wait until the heading `Hello, Orchard!` is visible. Not recorded.
- `run-1`: `HighlightAsync(.sidebar)`, then hold.
  - *Visible:* the Blazor Home page: the sidebar with the brand `BlazorCMS` and the links **Home** and **Orchard Core**,
    and the heading `Hello, Orchard!`.
  - *Labels:* guide:557-567 (`Home.razor`) and guide:407-428 (`NavMenu.razor`); the link texts come from the library, not from Orchard Core.
- `run-2`: `ClickAsync(a.nav-link[href$="content/orchard-core"])`, then wait until the heading `ORCHARD CORE` (`h1`) is
  visible. Use `ClickAndWaitForNavigationAsync` instead if the click is a full page load (see OPEN-QUESTIONS.md, 2).
  - *Visible:* the content page: heading `ORCHARD CORE`, the sub-heading `Introduction`, the sentence about the
    application framework, the sub-heading `Features` and its list of six items.
  - *Labels:* guide:673-726 (`Content.razor`).

### `06-tenant` (chapter 6, "Adding a tenant")

- **Starts on:** `/Admin/Features`, signed in, opened with `OpenAsync`; wait until `#search-box` is visible. Tenants is disabled. Not recorded.
- `tenant-1`: `TypeAsync(#search-box, "Tenants")`; `ClickAndWaitForNavigationAsync(#btn-enable-OrchardCore_Tenants)` (the
  **Enable** button of the **Tenants** feature); when the Features page has loaded again, `TypeAsync(#search-box,
  "Tenants")` and `HighlightAsync(#btn-disable-OrchardCore_Tenants)`.
  - *Visible:* the Features page filtered on `Tenants`, the row of the **Tenants** feature with a **Disable** button (enabled).
    The filter also lists other features whose name contains `Tenants`; `HighlightAsync` goes on the button of the feature
    named **Tenants**, the one whose id is `OrchardCore.Tenants`, as the narration says.
  - *Labels:* Tools, Features: `OrchardCore.Features/AdminMenu.cs:26,27`; the search box: `OrchardCore.Features/Views/Admin/Features.cshtml:27`;
    **Enable**: `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the feature name **Tenants**:
    `OrchardCore.Tenants/Manifest.cs` (`Id = "OrchardCore.Tenants"`, `DefaultTenantOnly = true`).
- `tenant-2`: `NavigateAsync("/Admin/Tenants")`; wait for `a.create` (**Add Tenant**); `ClickAndWaitForNavigationAsync(a.create)`;
  `TypeAsync(#Name, "FirstTenant")`; `TypeAsync(#RequestUrlPrefix, "tenant01")`; `SelectAsync(#RecipeName, "Headless site")`; if
  `#DatabaseProvider` is a list that shows `None`, `SelectAsync(#DatabaseProvider, "Sqlite")` (see OPEN-QUESTIONS.md, 3);
  `ClickAndWaitForNavigationAsync(button.create)` (**Create**). Do not use **Create & Setup**.
  - *Visible:* the **Tenants** list with a row `FirstTenant` (and the row of the default tenant).
  - *Labels:* Multi-Tenancy, Tenants: `OrchardCore.Tenants/AdminMenu.cs:30,33`; **Add Tenant**: `Views/Admin/Index.cshtml:27`;
    **Name** and **Create**: `Views/Admin/Create.cshtml:15,29`; **Url Prefix**: `Views/Shared/_TenantInfo.cshtml:12`;
    **Recipe**: `Views/Shared/_TenantRecipes.cshtml:4`. The Name is `FirstTenant`, as in guide:789; the guide spells the labels
    "Tenant name" and "URL Prefix", the code has **Name** and **Url Prefix**.
- `tenant-3`: `HighlightAsync(#btn-setup-FirstTenant, "Setup")`, then `HighlightAsync` on the state badge of the row. Do not select Setup.
  - *Visible:* the row `FirstTenant` with the badge **Uninitialized** and the buttons **Edit**, **Setup** and **Reload** (**Remove** appears too when
    the configuration allows removing tenants: `OrchardCore.Tenants/TenantsOptions.cs`, `TenantRemovalAllowed`, off unless set).
  - *Labels:* **Uninitialized**: `Views/ShellSettingsEntry.State.SummaryAdmin.cshtml:12`; **Setup**: `Views/ManageTenantActionButtons.cshtml:24`.

### `07-site` (chapter 7, "The base URL")

- **Starts on:** `/home` of the default tenant (`OpenAsync`, not recorded), signed in or not. Build A, snapshot R3.
- `site-1`: `NavigateAsync("/tenant01/home")`; wait until the heading `Hello, Orchard!` is visible; `HighlightAsync(.sidebar)`.
  - *Visible:* the Blazor Home page of the tenant, with the same sidebar and styling as the default site (the Bootstrap
    style sheet and the layout style sheet load under the tenant's prefix).
  - *Labels:* the tenant's name and prefix: `OrchardCore.Tenants/Views/Shared/_TenantInfo.cshtml:12`.
- `site-2`: `ClickAsync(a.nav-link[href$="content/orchard-core"])`, then wait until the heading `ORCHARD CORE FOR TENANT 01` is
  visible (`ClickAndWaitForNavigationAsync` if the click is a full page load, OPEN-QUESTIONS.md, 2).
  - *Visible:* the heading `ORCHARD CORE FOR TENANT 01`, the sub-headings `Introduction` and `Features`, and the list ending with
    `Multi-Tenant (as we are seeing)`.
  - *Labels:* guide:875-988, the recipe of the tenant above.

### `08-click` (chapter 8, "Interactive components"), on Build B

- **Starts on:** `/interactive`, opened with `OpenAsync`, not recorded. Register `scene.Page.WebSocket` before the page opens and wait
  until the page has opened its WebSocket (the circuit of the interactive component); only then call `StartCaptureAsync`.
- `inter-3`: `ClickAsync(button.btn-primary)` (the button with the text `Test`); wait until the text `Clicked!` is visible; hold.
  - *Visible:* the heading `Interactivity test`, the button `Test`, and the text `Clicked! [` followed by the date and time of the click.
  - *Labels:* guide:996-1012 (`InteractiveButton.razor`); the text is the component's, not Orchard Core's.

## The slides

Rendered by `slides.py`, nothing to record. Each line says what the slide shows.

- `idea-1`: three cards: Orchard Core (editors create and publish content items), Blazor (components turn items into pages), one application (components read content through Orchard Core services).
- `idea-2`: three cards: the Razor class library, the web project, and Program.cs.
- `lib-1`: the three terminal commands that create and reference the library, and the framework reference of the library's project file.
- `lib-2`: `App.razor` (Bootstrap style sheet of the Resources module, head outlet, `Routes`, the Blazor script) and `Routes.razor` (router, default layout).
- `lib-3`: `MainLayout.razor`, the two links of `NavMenu.razor`, `Home.razor` with `@page "/home"`, and a card on why the Home page is not at the root (Orchard Core also listens on the root path: the Setup screen is mapped to it, `OrchardCore.Setup/Startup.cs:89-93`, and so is the home route, `OrchardCore.HomeRoute/Startup.cs:23`).
- `serve-1`: `Program.cs` with `using OCBlazorLib;` and the commented block filled in; the `ConfigureServices` lines are bright.
- `serve-2`: the same code with the `Configure` lines bright, and two cards: what `Configure` adds, and that both callbacks run for each tenant.
- `page-1`: the route, the three `@using` lines (in the guide the first and the last are in `_Imports.razor`) and the three `@inject` lines of `Content.razor`, the properties, and the package reference note.
- `page-2`: `OnParametersSetAsync`, with three notes: the alias handle, a null item for an unknown alias, and the exception of `PopulateAspectAsync`.
- `page-3`: the markup of `Content.razor` with the `Loading...` branch and the `MarkupString`, and two notes on HTML and sanitizing.
- `page-4`: `@attribute [StreamRendering]`, `OnInitializedAsync` with the delay, and two notes on streaming.
- `setup-1`: three cards: Setup, Sign in, JSON Import.
- `base-1`: the tenant middleware's path base code (`src/OrchardCore/OrchardCore/Modules/ModularTenantRouterMiddleware.cs:28-37`, trimmed) and three notes on the base tag.
- `base-2`: `App.razor` with the base tag set from `NavigationManager`.
- `base-3`: three cards: `HttpContext`, `ShellScope`, `ISiteService`.
- `inter-1`: `InteractiveButton.razor`.
- `inter-2`: the line that uses the component with the `InteractiveServer` render mode, and the registrations of `Program.cs`.
- `inter-4`: three cards: the developer tools, the WebSocket filter, the Test button.

## Checks for private data

- The pages show only fake data: the user `admin`, `admin@example.com`, the sites `Orchard Core` and `First Tenant Site`, the
  tenant `FirstTenant`, the content of the recipes. No other name, email address or number is on screen.
- The recording shows no address bar, so no token is visible. The Setup token (24 hours) is only in the off-camera
  steps. No clip shows a Setup, sign-in or password field.
- The password `<password>` is never typed on camera, never written in a file, and never part of a command that is
  saved: pass it only through `VIDEO_PASSWORD` and the Auto Setup variable.
- The Tenants list shows the default tenant's row: look at it in the contact sheet. It must show no connection string
  or path of this machine. The `Clicked!` text shows the date and time of the recording, nothing else.
- OCR the video at one frame per second and at every scene change, as the skill asks, and search for any email address
  other than `@example.com`, any path starting with a drive letter, and any word of a real name.
