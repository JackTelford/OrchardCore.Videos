# Open questions: blazor-cms

What was checked in the Orchard Core source, and what a person must confirm on a running site before or while
recording. Nothing below is stated in the narration as a fact that was not read in the source or in the guide
(`src/docs/guides/create-blazor-cms/README.md`).

## 1. Where the sample site comes from, and which Orchard Core version it uses

- **Checked:** the guide builds its site with the project template `occms` and a Razor class library, and its text
  pins `--version 3.0.1` (guide:635) and a `.NET 8 SDK` prerequisite (guide:9), while the source being documented
  targets `net11.0` (`src/OrchardCore.Build/TargetFrameworks.props:11`) and the template does too (`template.json:22`).
  The guide's companion sample (OrchardCore.Samples) is not available to this programme.
- **Confirm:** that a host and a library built from the template of the release being recorded, with every
  `OrchardCore.*` package at the same version, build and run (`recording-plan.md`, "Build the host"). If a
  `ProjectReference` setup like `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj` is preferred to packages, say so
  in the plan and keep one choice for both projects.

## 2. NavLink clicks: full page load or enhanced navigation

- **Checked:** the guide's `App.razor` loads `_framework/blazor.web.js` (guide:199). The behavior of a link click in
  that setup (a full page load, or an enhanced navigation that patches the page) is Blazor's, not Orchard Core's, and
  is not described in the guide.
- **Confirm:** which one happens for the **Orchard Core** link in the sidebar, on the default tenant and under
  `/tenant01/`. A full page load needs `ClickAndWaitForNavigationAsync`; an enhanced navigation needs `ClickAsync` and a
  wait on the heading (steps `run-2` and `site-2`), or the screencast freezes.

## 3. Database fields of the tenant form and of its Setup screen

- **Checked:** the Create Tenant form shows **What type of database to use?** as a list under **Database Presets**
  unless a database provider is preset, and a **Table Prefix** that is required only when the configuration asks for
  it (`OrchardCore.Tenants/Views/Shared/_TenantDatabaseInfo.cshtml`). The Setup screen asks for the database unless a
  preset exists (`OrchardCore.Setup/Views/Setup/Index.cshtml:165`).
- **Confirm:** what the form shows on the recorded site (the guide's screenshot shows `Sqlite` selected). If the list
  shows `None`, choose `Sqlite` in `tenant-2` and again on the Setup screen (off camera). If a table prefix is
  required, enter `tenant01` and note it in the plan; the narration of `tenant-2` does not mention it.

## 4. What the root path does on the recorded site

- **Checked:** the narration (`lib-3`) gives the guide's reason for the Home page listening on `/home`: Orchard Core also
  listens on `/`, for example to show the Setup screen (guide:733-735). In the source, the Setup screen is mapped to the
  empty pattern (`OrchardCore.Setup/Startup.cs:89-93`) and the home route to `/` (`OrchardCore.HomeRoute/Startup.cs:23`).
  The Headless site recipe sets the home route to the admin (`headless.recipe.json:100-104`), but the narration does not
  claim that this is the reason: the guide serves its Blazor Home page from `/` on a Headless site (guide:624).
  A Blazor page on `/` next to the Setup route is not tried in the source.
- **Confirm:** that with Auto Setup, the first request to `/` provisions the default tenant (and is not answered by a
  Blazor page), and that after setup `/` goes to the admin or its sign-in page, not to a Blazor page.

## 5. The labels of the parts in the editor and in Add Parts

- **Checked:** the editor of a content item labels its fields **Title** (`TitlePart.Edit.cshtml:12`) and **Alias**
  (`AliasPart.Edit.cshtml:10`). The label of the Markdown field is the display name of the part
  (`ContentTypePartExtensions.DisplayName`, which falls back to the part name without the `Part` suffix), and the Add
  Parts list uses `ContentPartDefinition.DisplayName()` (`ContentPartSettingsExtensions.cs:54-64`). The guide says to
  add `TitlePart`, `AliasPart` and `MarkdownBodyPart`; the list may show them as `Title`, `Alias` and `MarkdownBody`.
- **Confirm:** the label shown above the Markdown editor in `content-3`. The narration never names it ("a body written
  in Markdown"). The video creates the type with a recipe, so Add Parts is never on screen.

## 6. Anything the guide states about Blazor itself

The guide says, and the narration repeats, that Razor components are not supported in Orchard Core modules (guide:151-155),
that a component renders on the server unless it has a render mode (guide:988-1022), and that an interactive component
keeps a WebSocket connection to the server (guide:1026). None of this is in the Orchard Core source.

- **Confirm:** on the recorded site, that `StreamRendering` shows `Loading...` before the content (`page-4`; it is
  not on camera, the half-second delay is too short to record), and that the WebSocket connection appears in the
  network tab as `inter-4` says. Neither is on camera.

## 7. Unknown or unpublished alias

- **Checked in code:** `IContentHandleManager.GetContentItemIdAsync` returns null when no provider matches
  (`ContentHandleManager.cs`), `IContentManager.GetAsync` returns null for an empty id
  (`DefaultContentManager.cs:104-107`), and `PopulateAspectAsync` throws `ArgumentNullException` for a null item
  (`DefaultContentManager.cs:889`). The guide's `Content.razor` calls it without a check, so `/content/<unknown alias>`
  fails with an exception and the `Loading...` markup is never the answer for it. `page-2` says so.
- **Confirm:** on the recorded site, by opening `/content/unknown`, that the page fails as described. This is not on camera.

## 8. Claims taken from the project files

- "One package reference to OrchardCore.ContentManagement brings I Content Manager, I Content Handle Manager and I Site
  Service" (`page-1`): read from the project references of `OrchardCore.ContentManagement`
  (`OrchardCore.ContentManagement.csproj`: the Abstractions, Infrastructure.Abstractions, ... projects). **Confirm**
  that the published package declares them as dependencies, by building a library with only that reference.
- "The Markdown body part sanitizes its HTML by default, through its Sanitize Html setting" (`page-3`): read from
  `MarkdownBodyPartSettings.cs` (default `true`), `MarkdownBodyPartHandler.cs` and
  `MarkdownBodyPartSettings.Edit.cshtml:7` (the label **Sanitize Html**).
- "Each tenant stores its own content" (`site-2`): the tenant has its own tables or database (the **Table Prefix** hint
  of the tenant form reads "when tenants share the same database"). **Confirm** that the two items differ per tenant
  (the check at the end of "Between 06-tenant and 07-site").

## 9. The slide of the tenant middleware

`base-1` shows a trimmed copy of `ModularTenantRouterMiddleware.cs:28-37`, where `RequestPathBase` is an internal
property of the shell settings. It explains where the path base comes from; a viewer cannot compile it. A reviewer who
wants only compilable code can drop the panel and keep the three notes below it.

## 10. Differences from the guide that the video avoids

- The guide's `NavMenu.razor` of the multi-tenancy section links to `content` (guide:768), while the route needs an
  alias (`/content/{alias}`, guide:649): the video uses `content/orchard-core` (guide:712, 716).
- The guide gives two stylesheet links in `App.razor` (guide:193 and guide:829): the plan uses the second.
- The guide puts the interactive button on a "BaseUrl" page that it never shows (guide:861, 992, 1014): the video uses
  its own `Interactive` page, and never mentions the BaseUrl page.
- The guide links to `https://docs.orchardcore.net/en/latest/docs/guides/create-cms-application/` (guide:18), with an extra
  `docs` segment; the page is `src/docs/guides/create-cms-application/README.md`.

## 11. Static files and the base URI under the tenant's prefix

- **Checked:** `App.razor` links `OrchardCore.Resources/Styles/bootstrap.min.css` (the file exists in
  `OrchardCore.Resources/wwwroot/Styles/`) and `_content/OCBlazorLib/OCBlazorLib.bundle.scp.css` with relative URLs, and
  sets the base tag from `NavigationManager.BaseUri` (`base-2`, guide:827-847). The tenant middleware adds the prefix to
  `PathBase` (`ModularTenantRouterMiddleware.cs:28-37`). That `BaseUri` then holds the prefix, and that both style sheets
  are served under `/tenant01/`, is Blazor's and the static file middleware's behavior, not read in the source.
- **Confirm:** on `/tenant01/home`, with the base tag in the page source set to the tenant's URL, that the sidebar and the
  page are styled as on the default site and that the sidebar links point under `/tenant01/` (`site-1`, `base-2`).

## 12. Where the Edit button is in the Content Items list

- **Checked:** the list shows the type badge as the technical name (`MarkdownPage`, `ContentsType_SummaryAdmin.cshtml:6`),
  which `content-2` says; the default admin list layout is List with the actions as Buttons
  (`OrchardCore.Admin.Abstractions/AdminListOptions.cs`).
- **Confirm:** that the recorded site shows the **Edit** button on the row (`content-3`), and not an actions menu.
