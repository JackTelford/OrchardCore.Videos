# Open questions: Deployment overview

Everything in the narration was read from the source (`src/OrchardCore.Modules/OrchardCore.Deployment`, the views of
the modules it names, the Blog recipe, and `src/docs/reference/modules/Deployment/README.md`). The points below depend
on a running site, which this session did not have. A person confirms each one before or while recording, and
changes the plan or the narration if the site disagrees.

1. **The Staging tenant.** Checked: `AutoSetupMiddleware.cs` creates the settings of the other tenants after the
   Default tenant is set up, and installs each one on its first request; `TenantSetupOptions.cs` has `RequestUrlPrefix`
   and `DatabaseTablePrefix`. Confirm that `http://localhost:5010/staging` sets itself up with the values of
   `recording-plan.md` (Blog recipe, Sqlite, table prefix `staging`), that its admin is at `/staging/Admin`, and that
   Tools, Features lists **Deployment** as Enabled there.
2. **The sign-in of the Staging tenant.** Checked: the authentication cookie is named for its tenant
   (`OrchardCore.Users/Startup.cs:123`, `orchauth_` plus the tenant name). Confirm
   that a session saved after signing in at `/staging/Login` signs the recorder in at `/staging/Admin`, and that
   `Scene.StartAsync` with the base URL `http://localhost:5010/staging` and `state-staging.json` opens
   `/staging/Admin/...` for the paths of `recording-plan.md`.
3. **The left menu.** `NavigationItem-admin.cshtml` and `NavigationItemLink-admin.cshtml` render a group as
   `button.item-label` with `aria-expanded`, and a page as `a.item-label`, and say the open groups are restored by a
   script. Confirm which groups are open when a page loads in a new browser context (the plan closes them before the
   capture), and that the text of the labels matches exactly (Tools, Deployments, Plans, Package Import, JSON Import,
   Remote Instances, Remote Clients, Settings, General).
4. **The order of the pages under Deployments.** The positions are free text (`am1-Plans`, `am-Package Import`,
   `am-JSON Import`, `am-Remote Instances`, `am-Remote Clients`), and the sort order of `FlatPositionComparer` for
   them was not worked out. The narration names the pages without an order. Confirm the order on screen, and choose
   the order of the rings of `find-2` to follow the narration anyway.
5. **The Features search.** Confirm which rows `Deployment` leaves in Tools, Features, that the row selector of
   `find-1` (a `.list-group-item` holding `#btn-disable-OrchardCore_Deployment`) matches the default layout of the
   page, and that the Deployment row reads **Enabled** with a **Disable** button.
6. **The Add Step dialog.** The categories are built from the steps of the enabled features. Confirm that the Blog
   site lists **Configuration**, **Content Management** and **Deployment**, and that the card **Site Settings** is in
   **Configuration** (`SiteSettingsDeploymentStep.cs`: category Configuration).
7. **The Site Settings editor.** The check boxes are the public properties of `SiteSettings` (18 of them, each
   supported by `SiteSettingsDeploymentSource.cs`), labeled with the property names, for example **PageSize**.
   Confirm that **PageSize** is there and visible without scrolling, and that no other box is ticked by default.
8. **The download.** The **Select** link of the target posts a form (`TheAdmin.ts`), and `ExportFileController.cs`
   answers with a file named after the plan without spaces. Confirm that Playwright's `RunAndWaitForDownloadAsync`
   catches it in the headless browser, that the dialog stays open afterwards, and that the file is `StagingSetup.zip`.
9. **The import.** Confirm that Package Import accepts the zip file, that the message **Deployment package imported.**
   is visible after the redirect despite the tenant reload (`RecipeDeploymentTargetHandler.cs` releases the shell
   before the redirect), how many seconds the first page after the import takes, and that the toast, which hides itself
   after five seconds, is on screen when the narration of `import-3` ends.
10. **The result of the import.** The Settings recipe step sets the Page size from the package (`SettingsStep.cs`).
    Confirm that Settings, General of the Staging site reads 25 afterwards, that the Site name is unchanged, and that
    the page loads without a validation message.
11. **The General settings page.** Confirm that **Page size** is on the first tab (the shapes are placed in
    `Content:1#Site;10`, `DefaultSiteSettingsDisplayDriver.cs:51`) and is visible without scrolling at 1536 by 864.
12. **The starting value of the Staging site.** `SiteService.cs` gives a new site a Page size of 10 and the Blog recipe
    does not change it for the site, but the plan sets the value explicitly. Confirm it reads 10 before `04-import`.
13. **Pronunciation.** Check with `scripts/pronounce.py` how the voice reads **JSON** (in `find-2`, the label JSON Import),
    **zip**, **Page size** and **Staging Setup**. No `spoken` entry exists for JSON; add one if the voice misreads it.
