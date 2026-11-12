# Recording plan: Recipes: Set Up and Update Sites From a File

Read this with `storyboard.json` open. Narrate first (`narrate.py`), so `durations.json` exists and the recorder holds each
step for the length of its narration. Keep the actions of a step shorter than its narration.

Conventions of this plan:

- A selector in quotes, such as `"#search-box"`, stands for `scene.Page.Locator("#search-box")`.
- `file:line` references are in the Orchard Core checkout the site is built from (commit `92db4a83`), under `src/`: a path
  starting with `OrchardCore.Themes/` is `src/OrchardCore.Themes/...`, one starting with `OrchardCore.Recipes.Core/` or
  `OrchardCore.DisplayManagement/` is `src/OrchardCore/...`, `docs/...` is `src/docs/...`, and any other `OrchardCore.<Module>/...`
  path is `src/OrchardCore.Modules/OrchardCore.<Module>/...`.
- Menu labels, buttons and messages are written as the code spells them. If a label differs on the running site, stop and
  write it in `OPEN-QUESTIONS.md` instead of recording.
- The video is an admin video: the capture never shows a recipe file, JSON, code or a command line. The pages below do not.

## The sites it needs

Two throwaway sites built from `src/OrchardCore.Cms.Web`, run one at a time or together on different ports, each with its own
`ORCHARD_APP_DATA` folder outside both repositories. Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead
address, so neither site calls a mail, SMS or payment provider.

### The main site (port 5010): clips `03-browse`, `04-run`, `05-permission`

- Provisioned by Auto Setup, so no setup or login form is recorded: Blog recipe, site name `Orchard Core`, time zone
  `America/Los_Angeles`, user `admin`, email `admin@example.com`, password from `VIDEO_PASSWORD` (chosen outside git, never
  written in a file of this repository). The settings are the `OrchardCore__AutoSetup__Tenants__0__*` environment variables of
  `docs/reference/modules/AutoSetup/README.md` (`ShellName` `Default`, `SiteName`, `SiteTimeZone`, `AdminUsername`,
  `AdminEmail`, `AdminPassword`, `DatabaseProvider` `Sqlite`, `RecipeName` `Blog`).
- Language and culture: the default (English). Admin theme and site theme: the ones the Blog recipe sets.
- Seed, outside the capture, once: sign in, open Tools, Features, search `Admin Dashboard`, and select Enable
  (`"#btn-enable-OrchardCore_AdminDashboard"`; `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
  The Blog recipe does not enable this feature (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). Enabling it
  creates the `Html Dashboard Widget` type only, no widget (`OrchardCore.AdminDashboard/Migrations/dashboard-widgets.recipe.json`).
- Check after the seed: `/Admin` shows the heading `Dashboard`, the button `Manage Dashboard` and the message
  `Click on Manage Dashboard button to add dashboard widget.`
  (`OrchardCore.AdminDashboard/Views/Dashboard/Index.cshtml:28,35,50`): there is no widget yet.
- Nothing else is seeded: the roles `Moderator`, `Editor`, `Author` and `Contributor` come from the Blog recipe
  (`blog.recipe.json:96`), and `Editor` has no permission.
- Sign in once outside the capture (`Recorder setup` with `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD`), which saves `state.json`
  (git-ignored). Stop the site, copy its `App_Data` folder and `state.json` aside (the snapshot), start the site again.

### The setup site (port 5011): clip `02-setup`

- The same build, started with an empty `App_Data` folder, **without any Auto Setup variable**, so `/` shows the Setup screen
  (`OrchardCore.Setup/Startup.cs:92` maps the screen to the root). It is never set up: no field is filled, **Finish Setup is
  never selected**, and no sign-in is saved for it (there is nothing to sign in to).
- Start it with its own address, for example:

  ```bash
  ORCHARD_APP_DATA=<empty folder> ASPNETCORE_URLS=http://localhost:5011 dotnet run --project src/OrchardCore.Cms.Web -c Release --no-launch-profile
  ```

- The recorder takes one `VIDEO_BASE_URL` per run, and it uses `state.json` when the file exists. Record this clip **first**,
  before `Recorder setup` has created `state.json` (or move the file aside), with `VIDEO_BASE_URL=http://localhost:5011`.

## Order of the clips

1. `02-setup` on the setup site (its own run, no `state.json`).
2. `Recorder setup`, then `05-permission` on the main site (it changes nothing).
3. `03-browse` (changes nothing).
4. `04-run` last: **it changes the site** (see below).

`01-what` and `04-care` are slides drawn by `slides.py`: nothing to record.

Which clips change a site:

- `02-setup`: nothing, because the Setup screen is never submitted.
- `05-permission`: nothing; the role is not saved.
- `03-browse`: nothing.
- `04-run`: runs the `Admin Dashboard Widget Sample` recipe, which adds one widget to the dashboard, with a new identifier at
  every run (`dashboard-widgets-samples.recipe.json:16`). A second take on the same site shows two widgets. Stop the site, restore
  the snapshot (`App_Data` and `state.json`), and start it again before every take of `04-run`.

## The clips

### `02-setup` (setup site, `http://localhost:5011/`)

Start page: `scene.OpenAsync("/")`, not recorded: the Setup screen, titled `Setup`, with the text `Please answer a few questions to
configure your site.` (`OrchardCore.Setup/Views/Setup/Index.cshtml:102-103`). The Recipe button shows `Software as a Service`,
because that is the only setup recipe tagged `default` (`SetupController.cs:61`, `OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json`).
Then `scene.StartCaptureAsync()`.

- `setup-1`: move the cursor to `"#recipeButton"` and `scene.HighlightAsync(scene.Page.Locator("#recipeButton"), "Recipe")`.
  Do not click. On screen when the narration ends: the Setup screen, the field `What is the name of your site?` next to the
  field `Recipe` (its button ringed, with the text `Software as a Service`) and the field `Default Time Zone`.
  Labels: `Index.cshtml:118` (`What is the name of your site?`), `:130` (`Recipe`), `:142` (the button).
- `setup-2`: `scene.ClearHighlightsAsync()`, then `scene.ClickAsync(scene.Page.Locator("#recipeButton"))`; the menu opens. Move the
  cursor slowly along the entries without clicking. On screen when the narration ends: the open list `"#recipes .dropdown-menu"`
  with one entry per setup recipe, each showing its name and, in small gray text, its description:
  `Agency`, `Blank site`, `Blog`, `Coming Soon`, `Headless site`, `Software as a Service`, in this order (sorted by recipe name,
  `Index.cshtml:145-149`; names from the `displayName` of `TheAgencyTheme`, `TheAdmin` (`blank`, `headless`), `TheBlogTheme`,
  `TheComingSoonTheme` and `TheTheme` recipes, line 3 of each). Everything fits without scrolling. If the build lists other
  setup recipes, keep the take and add them to `OPEN-QUESTIONS.md`; the narration names only three.
- `setup-3`: `scene.ClickAsync(scene.Page.Locator("#recipes a[data-recipe-name='Blog']"))`; the menu closes and the button text
  becomes `Blog` (`OrchardCore.Setup/Assets/ts/setup.ts:48-62`). On screen when the narration ends: the Recipe button reading
  `Blog`, the rest of the form untouched. Do not fill a field, do not select Finish Setup; end the clip by disposing the scene.

### `05-permission` (main site)

Start page: `scene.OpenAsync("/Admin")`, not recorded: the Dashboard with the sidebar groups **closed** (collapse `Access Control`
and `Tools` if the saved state of the sidebar has them open). Then `scene.StartCaptureAsync()`.

- `who-1`: in the admin menu, `scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Access Control", Exact = true }))`
  (a first-level item without a page of its own is drawn as a button that expands its items,
  `OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24,29,33-36`), then
  `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/Roles/Index']"))` (`Roles`; the route template
  `Roles/{action}/{id?}` of `OrchardCore.Roles/Controllers/AdminController.cs:22` gives `/Admin/Roles/Index`, not `/Admin/Roles`), then
  `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.edit[href$='/Roles/Edit/Editor']"))` (the `Edit` button of the row
  `Editor`). On screen when the narration ends: the page titled `Edit 'Editor' Role`, with the `Description` field, the box
  `Search permissions` and the permission groups. Labels: `OrchardCore.Roles/AdminMenu.cs:18-19` (`Access Control`, `Roles`),
  `Views/RoleEntry.Buttons.SummaryAdmin.cshtml:12` (`Edit`), `Views/Admin/Edit.cshtml:6` (`Edit '{0}' Role`).
  If the three clicks take longer than the narration, remove the pauses (`pauseAfter`), not a click.
- `who-2`: `scene.TypeAsync(scene.Page.Locator("#search-box"), "recipes", delay: 70)`. The permissions filter as you type
  (`Edit.cshtml:15`, `data-client-side-search`). Then move the cursor to
  `"tr[data-text='Manage Recipes'] .badge"`, `scene.HighlightAsync(..., "Security Critical")`, and hold about two seconds so the
  tooltip shows. On screen when the narration ends: one group, titled `Recipes` with the small text `(OrchardCore.Recipes)` beside the title
  (`_RolePermissions.cshtml:58-62`), holding one row `Manage Recipes` with a red
  badge `Security Critical` (ringed) and its tooltip `This permission could allow a user to elevate their other permissions. Grant
  it with extreme consideration.` Labels: `_RolePermissions.cshtml:49` (`Search permissions`), `:119` (`data-text`), `:125`
  (the badge and tooltip); the permission name `Manage Recipes`: `OrchardCore.Recipes.Core/RecipePermissions.cs:8`; the group
  title is the name of the feature, `Recipes` (`OrchardCore.Roles/Controllers/AdminController.cs:412-427`). If another permission
  also matches `recipes`, keep the take and write it in `OPEN-QUESTIONS.md`.
- `who-3`: `scene.ClearHighlightsAsync()`, then `scene.MoveToAsync(scene.Page.Locator("[id='Checkbox.ManageRecipes']"))` and
  `scene.HighlightAsync(scene.Page.Locator("tr[data-text='Manage Recipes']"))`. Do not tick the box and do not select Save. On
  screen when the narration ends: the row `Manage Recipes` ringed, with its `Allow` and `Effective` boxes both unchecked (the
  `Editor` role does not have the permission; only `Administrator` does by default,
  `OrchardCore.Recipes/RecipesPermissionProvider.cs:19-20`). `Allow` and `Effective` are the column titles of
  `_RolePermissions.cshtml:69-70`. End the clip by disposing the scene: the role is not saved.

### `03-browse` (main site)

Start page: `scene.OpenAsync("/Admin")`, not recorded: the Dashboard, sidebar groups closed. Then `scene.StartCaptureAsync()`.

- `browse-1`: `scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Tools", Exact = true }))`, then
  `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/Recipes']"))`, and **at once**
  `scene.TypeAsync(scene.Page.Locator("#search-box"), "lucene", delay: 60)` (the box has `autofocus`). The list of recipes
  is unfiltered for about one second: the descriptions of the `Media API` and `OpenApi` recipes are long and technical, so
  do not pause before typing, and check the contact sheet. On screen when the narration ends: the page titled `Recipes`,
  `lucene` in the search box, one group titled `The Blog Theme` with two rows, `Blog - Lucene Query` and `Blog - Lucene Search`,
  each with a description, the tags `blog` and `lucene` as badges, and a `Run` button. Labels:
  `OrchardCore.Recipes/AdminMenu.cs:18-19` (`Tools`, `Recipes`), `Views/Admin/Index.cshtml:6` (`Recipes`), `:15` (`Search`, the
  placeholder), `:34` (the group heading), `Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:18` (`Run`),
  `Views/RecipeEntry.DefaultTags.SummaryAdmin.cshtml:13` (tags); names, descriptions and tags:
  `OrchardCore.Themes/TheBlogTheme/Recipes/blog.lucene.query.recipe.json` and `blog.lucene.search.recipe.json`, lines 3, 4 and 10;
  group title: `OrchardCore.Themes/TheBlogTheme/Manifest.cs:5` (`The Blog Theme`).
- `browse-2`: highlight, one after the other with `scene.PauseAsync(900)` between them, and clear the previous ring each time:
  the group heading `scene.Page.Locator(".recipe-group h4", new() { HasText = "The Blog Theme" })` (caption `Group`), the row
  `row = scene.Page.Locator(".recipe-group .list-group-item", new() { HasText = "Blog - Lucene Query" })` (caption `Name and
  description`), its first badge `row.Locator(".badge").First` (caption `Tag`), and its button `row.Locator("a.btn-primary")`
  (caption `Run`). Do not use `.First` on the groups or the rows: the search only hides what does not match
  (`display: none`, `Assets/ts/recipes-index.ts:23`), and the groups and rows are sorted by name, so the first group in the page is
  `Admin` and the first row of `The Blog Theme` is `Blog - Elasticsearch Query`, both hidden. Do not scroll, do not click Run.
  On screen when the narration ends: the same filtered list, with the `Run` button of the row `Blog - Lucene Query` ringed.
  The heading of a group is the name of the first feature whose folder is contained in the path of the recipe
  (`AdminController.cs:81`); for the two rows here it is the name of the theme.
- `browse-3`: `scene.ClearHighlightsAsync()`, then `scene.ClickAsync(scene.Page.Locator("#search-box"))`, select the text
  (`scene.Page.Keyboard.PressAsync("Control+A")`) and type with `scene.Page.Keyboard.TypeAsync("blog", new() { Delay = 70 })`.
  Do not press Escape: it would show the whole list. On screen when the narration ends: `blog` in the search box and the group
  `The Blog Theme` with six rows, `Blog - Elasticsearch Query`, `Blog - Elasticsearch Search`, `Blog - Lucene Query`,
  `Blog - Lucene Search`, `Blog - SQL Query` and `Blog - SQL Search`, and **no row named only `Blog`** (the setup recipe `Blog` is
  not listed: `OrchardCore.Recipes/Controllers/AdminController.cs:164`). The filter matches the group name and the recipe name
  (`Drivers/RecipeEntryDisplayDriver.cs:19`, `AdminController.cs:95-100`, `Assets/ts/recipes-index.ts`).

### `04-run` (main site, from the snapshot)

Start page, not recorded: `scene.OpenAsync("/Admin/Recipes")`, then
`scene.Page.Locator("#search-box").PressSequentiallyAsync("dashboard")`, which leaves one group with one
row `Admin Dashboard Widget Sample`. The group is titled `Admin`, not `Admin Dashboard`, if the build behaves as the code reads
(see `OPEN-QUESTIONS.md`, the second question): record what the page shows, the narration does not name the group. The clip starts on this filtered list, so the long descriptions of other recipes never show.
Then `scene.StartCaptureAsync()`.

- `run-1`: `scene.HighlightAsync(scene.Page.Locator(".recipe-group .list-group-item", new() { HasText = "Admin Dashboard Widget Sample" }), "Admin Dashboard Widget Sample")`.
  On screen when the narration ends: the only group (titled `Admin` by the code, see above), the row `Admin Dashboard Widget Sample` ringed, with the description
  `Provides a sample Admin Dashboard Widget.`, no tag, and its `Run` button. Name and description:
  `OrchardCore.AdminDashboard/Recipes/dashboard-widgets-samples.recipe.json:3-4`; group title: the path of the recipe,
  `Areas/OrchardCore.AdminDashboard/Recipes`, contains the folder `Areas/OrchardCore.Admin` of the Admin module, whose single
  feature (`OrchardCore.Admin/Manifest.cs:4`, name `Admin`) comes before the Admin Dashboard feature in the order of
  dependencies (`AdminController.cs:81`, `OrchardCore.AdminDashboard/Manifest.cs:4,9-15`).
- `run-2`: `scene.ClearHighlightsAsync()`, then click the `Run` button of that row:
  `scene.ClickAsync(row.GetByText("Run", new() { Exact = true }))` (the row is the `Locator` of `run-1`; the id of the button is
  `btn-run-` plus the recipe name, but search by text). A dialog opens. On screen when the narration ends: the dialog titled
  `Recipes`, with the question `Are you sure you want to run the recipe?` and the buttons `Yes` and `No`, nothing selected.
  Source: `Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:13-16`; the dialog: `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-38`
  (`"#confirmRemoveModal"`, `"#modalOkButton"`, `"#modalCancelButton"`).
- `run-3`: `scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"))`. The page reloads on `/Admin/Recipes`
  with a green message at the top right: `The recipe 'Admin Dashboard Widget Sample' has been run successfully.`
  (`AdminController.cs:142`). That message hides itself after 5 seconds (`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`,
  `OrchardCore.Themes/TheAdmin/Views/Message.cshtml`), so it is on screen for the first sentence of the step only, which is
  enough. The first request after a recipe rebuilds the site and can take a few seconds: if the reload takes more than about two
  seconds, put only the wait for the message in `scene.OffCameraAsync(() => scene.Page.Locator(".toast.message-success").WaitForAsync())`
  (a condition the page does not meet before the reload). On screen about two seconds into the step: the Recipes page with the
  green message. If the message shows an error, stop: the site is not in the expected state, restore the snapshot.
- `run-4`: open the Dashboard by the first item of the breadcrumb above the title of the Recipes page, the link `Dashboard`
  (`scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("nav[aria-label='Breadcrumb'] a", new() { HasText = "Dashboard" }))`;
  `OrchardCore.AdminDashboard/Services/DashboardBreadcrumbProvider.cs:36` adds it to every admin page when the Admin Dashboard
  feature is enabled, and it links to the admin prefix). If the breadcrumb is not shown on the page (see `OPEN-QUESTIONS.md`),
  use `scene.NavigateAsync("/Admin")` instead. Then wait until `scene.Page.Locator(".dashboard-container", new() { HasText = "Orchard Core" })`
  is visible, then move the cursor over the widget without clicking any link (they leave the site) and
  `scene.HighlightAsync(scene.Page.Locator(".dashboard-container .card").First, "Orchard Core")`. On screen when the narration
  ends: the page titled `Dashboard`, the button `Manage Dashboard`, and one widget titled `Orchard Core` whose list holds the four
  links `Orchard Core site`, `Orchard Core docs`, `Orchard Core repo` and `Orchard Core chat`. Content:
  `dashboard-widgets-samples.recipe.json:18,28,31`; page: `OrchardCore.AdminDashboard/Views/Dashboard/Index.cshtml:28,35`;
  the route is the admin prefix alone (`OrchardCore.AdminDashboard/Startup.cs:59`). The page must show exactly one widget.

### The slides

Drawn by `slides.py`, one image per step; nothing to record.

- `what-1`: the heading `A recipe is a list of steps`, with four cards: `Enable features`, `Set the themes`, `Create roles`,
  `Define and add content`.
- `what-2`: the heading `Why a recipe is a file`, with three cards: `A file you keep`, `The same steps each time`, `Fewer clicks`.
- `what-3`: the heading `What you see of a recipe`, with three cards: `Display name`, `Description`, `Tags`.
- `what-4`: the heading `Places where recipes run`, with three cards: `Setup screen`, `Tools, Recipes`, `Tools, Deployments`.
- `care-1`: the heading `When a step fails`, with three cards: `The recipe stops`, `An error message`, `Earlier steps have run`.
- `care-2`: the heading `Running a recipe again`, with three cards: `Every step runs again`, `Content can be added again`,
  `Run it again on purpose`.

## Checks for private data

Before assembling, look at every contact sheet (`review.py`), then OCR the video at one frame per second and at every scene
change, and search the text for anything that is not fake:

- the only person is the user `admin` and the only email is `admin@example.com`; the Setup screen shows empty fields only;
- no password, token or connection string is on screen (the Setup screen's connection string and password fields stay empty);
- no machine name, user name or folder path of the recording computer (the capture shows the page only, and the address
  is `localhost`: if a frame shows anything else, do not use the take);
- the Recipes page shows recipes of Orchard Core only; if a description shows a local address, a client identifier or a file
  name, the take is not used (see `OPEN-QUESTIONS.md`, the first question);
- the links of the dashboard widget are shown but never followed.

Stop both sites by process id when the recording is done, and delete their `App_Data` folders.
