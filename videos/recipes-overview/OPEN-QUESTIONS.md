# Open questions: Recipes: Set Up and Update Sites From a File

Nothing here is stated in the narration as a fact beyond what the source shows. Each item says what was checked in the
Orchard Core source (commit `92db4a83`) and what a person must confirm on a running site before or while recording.

1. **Technical text on the Recipes page.** Checked: the list shows every recipe's description in full
   (`Views/RecipeEntry.Description.SummaryAdmin.cshtml`). The `Media API` and `OpenApi` recipes
   (`OrchardCore.Modules/OrchardCore.Media/Recipes`, `OrchardCore.Modules/OrchardCore.OpenApi/Recipes`) have long descriptions
   that mention a schema file name, an OAuth flow and local web addresses, which an admin video should not show for long. The
   plan types the search term at once in `browse-1` and starts `04-run` on an already filtered list. Confirm on the contact
   sheets that the unfiltered list is on screen for about a second at most; if it is longer, decide with the maintainer
   whether to record with the list filtered before the capture starts. The descriptions of `Blog - Lucene Query` and the other
   `... Query` recipes name a query (`RecentBlogPosts`) and appear in `browse-1` and `browse-3`: it is a name in the user
   interface, not code, but the maintainer may prefer a take that does not hold it for long.
2. **Which recipes and which group headings the build lists.** Checked: the page lists the recipes of every module and theme
   the application contains (`AdminController.GetRecipesAsync` uses `GetAvailableFeaturesAsync`, which is every feature, not
   only the enabled ones), grouped by the name of the first feature of the module or theme (`AdminController.cs:81`), and
   leaves out setup recipes and recipes tagged `hidden` (`:164-165`). The names expected from the files are
   `Admin` (see below), `The Blog Theme` (six recipes: `Blog - Elasticsearch Query`, `Blog - Elasticsearch Search`,
   `Blog - Lucene Query`, `Blog - Lucene Search`, `Blog - SQL Query`, `Blog - SQL Search`), `Media`, `Menu`, `OpenApi` and
   `Workflows`. Confirm the headings and the six `Blog - ...` rows on the running site (`browse-1`, `browse-3`, `run-1`). The
   narration names one heading only, `The Blog Theme`, and says that groups have headings, not that a heading is the module
   that provides the recipes. **Why `Admin`:** the heading is the name of the first feature (in the order of dependencies) whose
   folder (`Areas/<module>`) is contained, as plain text and with case, in the path of the recipe
   (`AdminController.cs:81`). The path of the Admin Dashboard sample is `Areas/OrchardCore.AdminDashboard/Recipes`, which contains
   `Areas/OrchardCore.Admin`, and the Admin module (one feature, named `Admin`, `OrchardCore.Admin/Manifest.cs:4`) is a dependency
   of Admin Dashboard (`OrchardCore.AdminDashboard/Manifest.cs:9-15`), so it comes first. Confirm on the running site whether the
   row `Admin Dashboard Widget Sample` sits under the heading `Admin` or `Admin Dashboard`. Neither is a problem for the
   narration; the plan and the checks of `04-run` must match what the page shows.
3. **The Setup screen's list.** Checked: it shows the setup recipes that are not tagged `hidden`, sorted by recipe name, and
   pre-selects the one tagged `default` (`SetupController.cs:56-61`). From the files that is `Agency`, `Blank site`, `Blog`,
   `Coming Soon`, `Headless site` and `Software as a Service` (pre-selected). Confirm the list on the running setup site, and that
   the Recipe field is shown (it is hidden when the recipe is preset by configuration, `Index.cshtml:124`). The narration names
   `Blank site`, `Blog` and `Headless site` only.
4. **A second run adds a second widget** (`care-2`). Checked: the sample recipe gives its widget a content item id from a
   generated identifier (`dashboard-widgets-samples.recipe.json:16`) and the content step imports by id
   (`OrchardCore.Contents/Recipes/ContentStep.cs`, `DefaultContentManager.ImportAsync`), so every run creates one more. Confirm by
   running the recipe twice on a throwaway copy and counting the widgets on the Dashboard.
5. **A failed recipe** (`care-1`). Checked: the executor stops at the first failing step and throws
   (`OrchardCore.Recipes.Core/Services/RecipeExecutor.cs`), and the page shows `The recipe '{0}' failed to run due to the
   following errors: {1}`, or `Unexpected error occurred while running the '{0}' recipe.` for other exceptions
   (`AdminController.cs:148,154`); both name the recipe. The narration says only that the steps before the failing one have
   already run. Also checked: a recipe runs each step in a scope of its own by default
   (`RecipeDescriptor.RequireNewScope` is `true`, `OrchardCore.Recipes.Abstractions/Models/RecipeDescriptor.cs:17`;
   `RecipeExecutor.ExecuteStepInternalAsync`, `:194-226`), and the page does not set it, so a step that has completed is
   saved before the next one starts. Not checked on a running site: that the saved effects of the earlier steps are visible
   after the failure. Confirm on a throwaway site with a recipe whose second step fails.
6. **The site reloads after a recipe** (`run-3`). Checked: `Execute` calls `ReleaseShellContextAsync` after the recipe, and the
   next request rebuilds the site (`AdminController.cs:140`). Confirm that the page after `Yes` is slow enough to need the
   off-camera wait described in the plan, and that the message is on screen long enough (it hides after 5 seconds).
7. **The Dashboard widget** (`run-4`). Checked: `/Admin` is the route of the Dashboard once Admin Dashboard is enabled
   (`OrchardCore.AdminDashboard/Startup.cs:59`), and the widget's title is the `TitlePart` text `Orchard Core`
   (`Views/TitlePart.DetailAdmin.cshtml`). Confirm that the widget shows its title and the four links, and that the Dashboard shows
   one widget only. The admin menu has no `Dashboard` item; the plan opens the Dashboard by the `Dashboard` link at the start of
   the breadcrumb (`DashboardBreadcrumbProvider.cs:36`), which links to the admin prefix. Confirm that the breadcrumb is shown on
   the Recipes page of the fresh site (`Breadcrumb.cshtml` draws it only when the trail is enabled); if it is not, the plan
   navigates to `/Admin` instead and the narration ("Open the Dashboard") still holds.
8. **Menu mechanics.** Checked: `Tools` and `Access Control` are first-level groups without a page, so the admin theme draws
   them as buttons that expand (`TheAdmin/Views/NavigationItemLink-admin.cshtml:24,29,33-36`), and `Recipes`, `Roles` are links
   under them. The `Roles` link points to `/Admin/Roles/Index` (route template `Roles/{action}/{id?}`,
   `OrchardCore.Roles/Controllers/AdminController.cs:22`), the `Recipes` link to `/Admin/Recipes`. Confirm
   the accessible names used by the locators of `who-1` and `browse-1`, and that both groups are closed when each clip starts.
9. **The permission list** (`who-2`, `who-3`). Checked: the group title is the feature name `Recipes`
   (`OrchardCore.Roles/Controllers/AdminController.cs:412-427`), the row text is `Manage Recipes`, and the badge and its tooltip
   are in `_RolePermissions.cshtml:125`. The legend also shows the feature id beside the title, `(OrchardCore.Recipes)`
   (`_RolePermissions.cshtml:58-62`). Confirm that searching `recipes` leaves only that group, that the tooltip shows when the
   cursor rests on the badge in the headless browser, and that `Editor` has the permission unchecked after the Blog recipe.
   Scope of the permission: `Manage Recipes` gates the Recipes page and its `Run` action only
   (`OrchardCore.Recipes/Controllers/AdminController.cs:65,115`); `Package Import` and `JSON Import` run a recipe under the
   separate, also security critical permission `Import Data` (`OrchardCore.Deployment.Core/DeploymentPermissions.cs:12`,
   `OrchardCore.Deployment/AdminMenu.cs:27,32`). The narration (`who-1`, `who-3`) therefore speaks of the Recipes page.
10. **The layout selector of the Recipes page.** Checked: the page offers a List and Grid selector beside the search box only when
    the admin settings allow users to choose (`Views/Admin/Index.cshtml:19-23`), and the default layout is List
    (`AdminListOptions.cs:30`). Confirm the page of the fresh site, and do not touch the selector if it is there.
11. **Where a recipe can also run** (`what-4`). Checked: `Tools`, `Deployments` holds `Package Import` and `JSON Import`
    (`OrchardCore.Deployment/AdminMenu.cs:19-30`); both write the recipe as the package's recipe file and run it with the recipe
    executor (`Controllers/ImportController.cs`, `OrchardCore.Recipes/Services/RecipeDeploymentTargetHandler.cs`). The video only
    names the menu and does not open these pages, to keep a recipe's text off the screen. Confirm the labels on the running site. Recipes also run elsewhere (the Recipe field when
    a tenant is created, `OrchardCore.Tenants/Views/Shared/_TenantRecipes.cshtml`; Auto Setup; the `recipes` step; data
    migrations), which is why `what-4` says "several places" and names three.
12. **Pronunciation.** No `spoken` entry is set. Check with `scripts/pronounce.py` how the voice reads `Lucene` (`browse-1`,
    `browse-3`) and the recipe name `Blog - Lucene Search` (`browse-3`, the hyphen between the words), and add an entry to
    `spoken` if either is misread.
