# Recording plan: Tenants: Create a Tenant and Set It Up

Read this file and nothing else to record the video. A selector below (`#Name`, `a.btn.create`) stands for
`scene.Page.Locator("...")`. A `file:line` is in the Orchard Core source (`src/OrchardCore.Modules/...`, `src/OrchardCore/...`
or `src/OrchardCore.Themes/...`) and says where a label or a behavior comes from. Narration and captions are in
`storyboard.json`; the steps below use its step ids.

The video follows the administration of the **Default** tenant, and ends on the Setup screens of two tenants it creates
(Maplewood and Riverside). Every name is invented. The password is never written here: use the value of
`VIDEO_PASSWORD`, written `<password>` below.

## 1. The site

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). |
| Default tenant | Provisioned once by Auto Setup with the **SaaS** recipe (display name "Software as a Service"). That recipe enables `OrchardCore.Tenants` (`OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:28`). `OrchardCore.Features`, which the Features button of a running tenant needs, is always enabled (`OrchardCore.Features/Manifest.cs`, `IsAlwaysEnabled = true`), so no recipe has to enable it. The Blog recipe does not enable `OrchardCore.Tenants`, so it can't be used for the Default tenant unless the prepare step enables the feature. |
| Site name, time zone | `Orchard Core`, `America/Los_Angeles`, database `Sqlite`. |
| Time zone of the site process | **`America/Los_Angeles`**, for every run, because the Setup screen of a tenant opens on the time zone of the server (`OrchardCore.Setup/Controllers/SetupController.cs:73`) and shows it in the first row from `setup-1` on, and again on the Setup screen of Riverside at the end of `06-shortcut`. On Linux and macOS start the site with `TZ=America/Los_Angeles` in its environment. On Windows a process has no time zone of its own: set the time zone of the machine to Pacific Time (and back afterwards). Check it on the Setup screen of Maplewood before recording `04-setup`: the list must open on `America/Los_Angeles`. |
| Super user of Default | user `admin`, email `admin@example.com`, password `<password>` (`VIDEO_PASSWORD`). |
| Outbound calls | Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so nothing calls out. |
| Admin lists | The defaults: the List layout and the Buttons actions layout (`OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`). Don't change Settings, Admin. With the Menu actions layout the buttons of a row are inside a drop-down and every click below changes. |
| Root database settings | None. Set no `OrchardCore__DatabaseProvider` or `OrchardCore__ConnectionString` variable and no `DatabaseProvider` in a configuration file: when the root configuration names a provider, the Create Tenant form shows it read-only instead of a list (`OrchardCore.Tenants/Controllers/AdminController.cs:764-791`), and the video shows the list. |
| Tenant options | None. Set no `OrchardCore__Tenants__TenantRemovalAllowed`, `RequireTablePrefix`, `TablePrefixPattern` or `SchemaPattern` variable (`OrchardCore.Tenants/TenantsOptions.cs`, `Startup.cs:51`). With removal allowed, an Uninitialized row gets a "Remove" button (`ManageTenantActionButtons.cshtml:26`), and the buttons named below change; with a table prefix pattern, the Table Prefix field is read-only. |
| Feature profiles | None exist, so the Feature profile field is not on the Create Tenant form (`OrchardCore.Tenants/Views/Shared/_TenantFeatureProfile.cshtml:3`). Don't create any. |
| Features to enable | None. The SaaS recipe is enough. |

### Auto Setup and the Setup screen

The Setup screen of a tenant is part of the subject, so the recording runs **without Auto Setup**:

1. Provision the Default tenant once, with Auto Setup, in a first run:

   ```bash
   export ORCHARD_APP_DATA=<empty folder outside both repositories>
   TZ=America/Los_Angeles \
   OrchardCore__AutoSetup__AutoSetupPath= \
   OrchardCore__AutoSetup__Tenants__0__ShellName=Default \
   OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core" \
   OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles \
   OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin \
   OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com \
   OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD" \
   OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite \
   OrchardCore__AutoSetup__Tenants__0__RecipeName=SaaS \
   dotnet run --project src/OrchardCore.Cms.Web --no-build --urls http://localhost:5010
   ```

   Request `http://localhost:5010/` once and wait for the log line "The AutoSetup successfully provisioned the site".
2. Stop the site (by its process ID) and **start it again without any `OrchardCore__AutoSetup__` variable** (keep `TZ=America/Los_Angeles`
   on Linux and macOS; on Windows the machine is already set to Pacific Time, see "Time zone of the site process"). From here
   on, no tenant is listed in the Auto Setup configuration, so the Auto Setup middleware does nothing for the tenants of
   the video: the middleware only acts on a tenant whose name is in its list (`OrchardCore.AutoSetup/AutoSetupMiddleware.cs:81,95`),
   and Maplewood and Riverside are set up on their real Setup screens.
3. `Recorder setup` signs in as `admin` and writes `state.json`. It holds the session of the **Default** tenant only
   (the clips start in its administration). The Setup screens of Maplewood and Riverside don't use it: no tenant that is set up on camera
   has a sign-in, and the session of Default exists only because the clips start in its administration.

### Seed data

There is none. The Default tenant is the only tenant when the recording starts. Every tenant, field value and user of
the video is typed on camera, with the fake values listed in each clip:

| Item | Value |
|---|---|
| Tenant name | `Maplewood`, then `Riverside` |
| Category | `Customers` |
| Description | `Website of Maplewood Bakery` |
| Url Prefix | `maplewood`, then `riverside` |
| Hostname | empty (always) |
| Site name | `Maplewood Bakery` |
| Super user of Maplewood | user `admin`, email `admin@example.com`, password `<password>` |

Leave Hostname empty on camera: the Setup button and Create & Setup send the browser to the first host of the tenant
(`OrchardCore.Tenants/HttpContextExtensions.cs:12-21`), which doesn't resolve on `localhost`.

## 2. Order, dependencies and snapshots

Record in this order. Each clip starts from the state the previous one left.

| Clip | Needs | Changes the site |
|---|---|---|
| `02-create` | Snapshot A (only Default) | Yes: creates the tenant Maplewood (Uninitialized) |
| `03-list` | Snapshot B (Maplewood Uninitialized) | No |
| `04-setup` | Snapshot B | Yes: sets Maplewood up (Running), creates its database |
| `05-open` | Snapshot C (Maplewood Running) | No |
| `06-shortcut` | Snapshot C | Yes: creates the tenant Riverside (Uninitialized) |

Snapshots (stop the site, copy the whole `App_Data` folder and `state.json`, start the site):

- **A**: after step 3 of "Auto Setup and the Setup screen" above (the site started without Auto Setup, and `state.json` written by `Recorder setup`), so that `state.json` and the Data Protection keys of `App_Data` belong together.
- **B**: after recording `02-create`.
- **C**: after recording `04-setup`.

A retake of `02-create` restores A. A retake of `03-list` or `04-setup` restores B. A retake of `05-open` or
`06-shortcut` restores C. Stop the site before copying or restoring, and start it again afterwards. Restore the whole
`App_Data` folder, not only `Sites`: it holds the Data Protection keys that make `state.json` and the Setup tokens valid.

Between `02-create` and `04-setup` the 24 hours of the Setup token don't matter: the list makes a fresh token each time
it loads (`OrchardCore.Tenants/Controllers/AdminController.cs:159-162`).

## 3. Slides

Chapter 01 has three slide steps, drawn by `slides.py` (no recording).

- `stages-1`: the heading "Two stages" with four cards; the first two are bright: "Create" (the Default tenant saves the
  name, the address and the presets) and "Uninitialized" (the tenant is saved, and its setup has not run).
- `stages-2`: the same four cards; the last two are bright: "Set up" (the Setup screen creates the database and runs the
  recipe) and "Running" (the tenant answers requests as a site of its own).
- `stages-3`: the heading "What you need" with three check cards: the Tenants feature, the Manage tenants permission,
  and the Multi-Tenancy menu.

Sources of what the slides say: `OrchardCore.Tenants/Manifest.cs:10-16` (`DefaultTenantOnly`), `OrchardCore.Tenants/Permissions.cs:8,20-27`
(Manage tenants, granted to Administrator), `OrchardCore.Tenants/AdminMenu.cs:24,30-35` (the menu exists on the Default
tenant only, needs ManageTenants), `OrchardCore.Setup.Core/Services/SetupService.cs:132,303` (Initializing, then Running).

## 4. The clips

Every clip uses a scene as in `references/recording.md`: `OpenAsync(<start page>)`, `StartCaptureAsync()`, then one
`StepAsync` per step below. Keep the actions of a step shorter than its narration. Highlights are fixed on the screen:
call `ClearHighlightsAsync()` before scrolling or navigating.

### Clip `02-create` (chapter 02, "Add a tenant")

**Starts on** `/Admin` (the administration of the Default tenant), signed in with `state.json`, the admin menu visible.

- `create-1`
  - Actions: in `#adminMenu`, the group "Multi-Tenancy" is a button (`button.item-label.nav-group-label`, text "Multi-Tenancy"), because
    the item has no page of its own; it has no separate `button.nav-toggle` (that one exists only for an item with a page and children). The
    group starts collapsed: when its button has `aria-expanded="false"`, click it with `ClickAsync` (no navigation) and wait until the link
    `a.item-label` with the text "Tenants" under it is visible. Click that link with `ClickAndWaitForNavigationAsync`. The list opens.
    Move the cursor to `a.btn.create` ("Add Tenant") and click it with `ClickAndWaitForNavigationAsync`.
  - Visible at the end of the narration: the Create Tenant form (title "Create Tenant", breadcrumb "Tenants"), `#Name` empty
    and focused.
  - Labels: "Multi-Tenancy" `OrchardCore.Tenants/AdminMenu.cs:30` (no `.Action(...)`, so `OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:24,29,69,73,76-86`
    draws the whole row as the toggle button); "Tenants" `AdminMenu.cs:33`; "Add Tenant"
    `OrchardCore.Tenants/Views/Admin/Index.cshtml:27`; "Create Tenant" `OrchardCore.Tenants/Views/Admin/Create.cshtml:6`.
    The list shows one row, "Default" (narration: "The list holds one tenant").
- `create-2`
  - Actions: `TypeAsync(#Name, "Maplewood")`, then `HighlightAsync(#Name)` without caption.
  - Visible at the end: `Maplewood` in the Name field, with its hint "The name of the tenant." below.
  - Labels: "Name" `Create.cshtml:15`, hint `Create.cshtml:19`. The Name is read-only on the Edit form
    (`OrchardCore.Tenants/Views/Admin/Edit.cshtml:16`), and spaces are rejected (`OrchardCore.Tenants/Services/TenantValidator.cs:70,225`).
- `create-3`
  - Actions: `ClearHighlightsAsync()`. `TypeAsync(#Category, "Customers")`. `TypeAsync(#Description, "Website of Maplewood Bakery")`
    (Description is below Url Prefix on the page; the scene scrolls to it). Press nothing else.
  - Visible at the end: `Customers` in Category and `Website of Maplewood Bakery` in Description.
  - Labels: "Category" `OrchardCore.Tenants/Views/Shared/_TenantInfo.cshtml:4`; "Description" `_TenantInfo.cshtml:21`.
    The list shows both (`ShellSettingsEntry.Category.SummaryAdmin.cshtml`, `ShellSettingsEntry.Description.SummaryAdmin.cshtml`),
    and the list has a Category filter (`OrchardCore.Tenants/Views/TenantsFilters.cshtml:12`).
- `create-4`
  - Actions: `TypeAsync(#RequestUrlPrefix, "maplewood")`. Then `HighlightAsync(#RequestUrlPrefix)` and `HighlightAsync(#RequestUrlHost)`
    (no caption yet). Leave `#RequestUrlHost` (Hostname) empty.
  - Visible at the end: `maplewood` in Url Prefix, an empty Hostname, both fields ringed, and the hints under them.
  - Labels: "Url Prefix" `_TenantInfo.cshtml:12`; "Hostname" `_TenantInfo.cshtml:30`. The hostname hints (`_TenantInfo.cshtml:34-35`)
    are not read in the narration.
- `create-5`
  - Actions: keep both rings; add the caption "Prefix, hostname, or both" to one of them (`HighlightAsync(locator, caption)`).
    No typing.
  - Visible at the end: the two ringed fields with the caption.
  - Rule from the code: a tenant can't repeat the host and prefix of another tenant (`TenantValidator.cs:84-89`); the
    Default tenant has an empty host and an empty prefix, so a tenant with both empty is rejected with "A tenant with the same
    host and prefix already exists." Not shown on camera. Check it once outside the capture (see `OPEN-QUESTIONS.md`).
- `create-6`
  - Actions: `ClearHighlightsAsync()`. `SelectAsync(#RecipeName, "Blog")` (the option value and text are both `Blog`).
  - Visible at the end: the Recipe list showing `Blog`, with the hint "Optionally specify which presets should be used for the
    Setup experience."
  - Labels: "Recipe" `_TenantRecipes.cshtml:4`; "Select a recipe" `_TenantRecipes.cshtml:7`; hint `_TenantRecipes.cshtml:15`.
    The list holds the recipes with `issetuprecipe` (`AdminController.cs:332`), sorted by display name.
- `create-7`
  - Actions: scroll down to the heading "Database Presets". `SelectAsync(#DatabaseProvider, "SqlConnection")` (its text is
    "Sql Server"). The fields Connection string (`#ConnectionString`), Table Prefix (`#TablePrefix`) and Table Schema
    (`#Schema`) appear. Type nothing in them.
  - Visible at the end: "Database Presets", "What type of database to use?" showing `Sql Server`, and the three fields.
  - Labels: "Database Presets" `OrchardCore.Tenants/Views/Shared/_TenantDatabaseInfo.cshtml:18`; "What type of database to use?"
    `_TenantDatabaseInfo.cshtml:35`; "Connection string" (lower-case s on this form) `_TenantDatabaseInfo.cshtml:86`; "Table Prefix"
    `_TenantDatabaseInfo.cshtml:70`, hint `:75`; "Table Schema" `_TenantDatabaseInfo.cshtml:102`. The fields show and hide in
    `OrchardCore.Tenants/wwwroot/Scripts/tenant.js:23-40`. Providers: `OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:52-55`
    (Sql Server, Sqlite, MySql, Postgres). Under Connection string the form prints the product's own sample, "The connection string to your
    database instance, e.g. Server=localhost;Database=Orchard;User Id=username;Password=password." (`OrchardCoreBuilderExtensions.cs:23`):
    that is interface text, not a credential.
- `create-8`
  - Actions: `SelectAsync(#DatabaseProvider, "Sqlite")`. The three fields disappear. Keep Sqlite selected: the next steps need it.
  - Visible at the end: `Sqlite` selected, no Connection string, Table Prefix or Table Schema field.
  - Sqlite has no connection string and no table prefix (`OrchardCoreBuilderExtensions.cs:53`). The first option of the list is labeled "None"
    (`_TenantDatabaseInfo.cshtml:38`, empty value, which also hides the three fields): the narration says the type can stay on None, and then the
    Setup screen asks for the database (`SetupController.cs:253-263`, `Index.cshtml:173`). Don't select None on camera.
- `create-9`
  - Actions: scroll to the buttons if needed. `HighlightAsync(button[value="createAndSetup"])` ("Create & Setup") for about
    two seconds, `ClearHighlightsAsync()`, then click `button.create` ("Create") with `ClickAndWaitForNavigationAsync`.
  - Visible at the end: the list of tenants with the row Maplewood (and Default).
  - Labels: "Create" `Create.cshtml:29`; "Create & Setup" `Create.cshtml:30` (the value is `createAndSetup`, `AdminController.cs:31`).
    Create redirects to the list (`AdminController.cs:440`). If the form shows an error instead, stop and fix the cause.

**Leaves the site with** the tenant Maplewood, Uninitialized. Take snapshot B.

### Clip `03-list` (chapter 03, "The tenant in the list")

**Starts on** `/Admin/Tenants`, from snapshot B. Nothing is clicked in this clip, so it doesn't change the site.

The row of a tenant is the element `.list-group-item` that contains the name (`OrchardCore.Tenants/Views/Admin/Index.cshtml:37`).

- `list-1`
  - Actions: `HighlightAsync(<the Maplewood row>)`; then move the cursor over the badges in this order: `Customers`,
    `Sqlite`, `Blog`, `Uninitialized`, and stop on `Uninitialized`.
  - Visible at the end: the Maplewood row with the title "Maplewood", the description "Website of Maplewood Bakery" under it,
    and the badges `Customers`, `Sqlite`, `Blog` and `Uninitialized` (the last one ringed).
  - Sources: `ShellSettingsEntry.Category.SummaryAdmin.cshtml`, `ShellSettingsEntry.Database.SummaryAdmin.cshtml` (the provider value
    `Sqlite`), `ShellSettingsEntry.Recipe.SummaryAdmin.cshtml` (the recipe name `Blog`), `ShellSettingsEntry.State.SummaryAdmin.cshtml`
    (label "Uninitialized"), all in `OrchardCore.Tenants/Views/`.
- `list-2`
  - Actions: `ClearHighlightsAsync()`. Move to the name link "Maplewood" in the row and ring it. Then move to `#btn-setup-Maplewood`
    (button "Setup") and ring it. Don't click.
  - Visible at the end: the row buttons "Edit", "Setup" and "Reload", with Setup ringed.
  - Sources: `ManageTenantActionButtons.cshtml:9,24,30` (a button "Setup" for an Uninitialized tenant, with `id="btn-setup-<Name>"`);
    the name link `ShellSettingsEntry.Fields.SummaryAdmin.cshtml` and the Setup button both use the address with the token
    (`HttpContextExtensions.cs:30-33`); the token lasts 24 hours and is made each time the list loads (`AdminController.cs:159-162`);
    the Setup screen answers Not Found without a valid token (`OrchardCore.Setup/Controllers/SetupController.cs:63-66,265-278`).
    The address bar is not recorded, so the token is narrated, not shown.

### Clip `04-setup` (chapter 04, "Set up the tenant")

**Starts on** `/Admin/Tenants`, from snapshot B, with the Maplewood row visible.

- `setup-1`
  - Actions: click `#btn-setup-Maplewood` with `ClickAndWaitForNavigationAsync`. The Setup screen of Maplewood opens in the same tab.
    `TypeAsync(#SiteName, "Maplewood Bakery")`.
  - Visible at the end: the Setup screen (heading "Setup", "Please answer a few questions to configure your site."), the question
    "What is the name of your site?" with `Maplewood Bakery` typed, and its hint "This is used as the default title of your pages."
  - Labels: `OrchardCore.Setup/Views/Setup/Index.cshtml:102-103,118,121`.
- `setup-2`
  - Actions: move the cursor slowly from the site name field down to the legend "Super User", over the place where a Recipe
    list and the database questions would be. No typing.
  - Visible at the end: the first row with only the site name and Default Time Zone (opened on `America/Los_Angeles`, see "Time zone of the
    site process" in section 1); **no** Recipe list (`#recipeButton`) and **no**
    "What type of database to use?" (`#DatabaseProvider`), Table Prefix, Connection String or Table Schema; then the "Super User" group.
    Before recording, check that `#recipeButton` and `#DatabaseProvider` are absent from the page; if one is present, the presets
    of Maplewood were not saved (retake `02-create`).
  - Source: the recipe picker is hidden when the tenant has a recipe preset (`Index.cshtml:124`, `SetupController.cs:234-238`);
    the database section is hidden when the provider is preset and needs no connection string (`Index.cshtml:165`,
    `SetupController.cs:241-251`, Sqlite: `OrchardCoreBuilderExtensions.cs:53`).
- `setup-3`
  - Actions: `SelectAsync(#SiteTimeZone, "Europe/Amsterdam")` (a zone other than the one it opened on, because the narration says you can choose
    another), then `HighlightAsync(#SiteTimeZone)`.
  - Visible at the end: Default Time Zone showing the zone `Europe/Amsterdam`, with its hint "Determines the default time zone used when
    displaying and editing dates and times." The list opens on the time zone of the server (`SetupController.cs:73`), which the site process
    runs in as `America/Los_Angeles` (section 1), so no time zone of the recording machine is on screen.
  - Label: `Index.cshtml:159,162`. The option text is "(GMT-08:00) America/Los_Angeles (-08:00)" or the same with the offsets of the
    date (`OrchardCore/Modules/Model/TimeZone.cs`).
- `setup-4`
  - Actions: `ClearHighlightsAsync()`. `TypeAsync(#UserName, "admin")`, `TypeAsync(#Email, "admin@example.com")`,
    `TypeAsync(#Password, "<password>")`, `TypeAsync(#PasswordConfirmation, "<password>")`. **Never click** `#togglePassword`,
    `#togglePasswordConfirmation` or `#toggleConnectionString`: the fields stay masked.
  - Visible at the end: the "Super User" group with its hint "The super user has all the rights. It should be used only during Setup and
    for disaster recovery.", the User Name `admin`, the Email `admin@example.com` and two masked password fields. The password strength
    pop-over may be open on the Password field: it lists the rules and no value.
  - Labels: "Super User" `Index.cshtml:218`, hint `:219`; "User Name" `:222`; "Email" `:227`; "Password" `:234`; "Password Confirmation" `:244`.
    `<password>` must meet the rules the pop-over lists.
- `setup-5`
  - Actions: `HighlightAsync(#SubmitButton)` (the button "Finish Setup"). Don't click yet.
  - Visible at the end: the button "Finish Setup" ringed, the whole filled form above it.
  - Label: "Finish Setup" `Index.cshtml:256`. While Setup runs the tenant is Initializing (`SetupService.cs:132`) and every request
    to it gets a 503 response "The requested tenant is currently initializing." (`OrchardCore/Modules/ModularTenantContainerMiddleware.cs:37-42`).
- `setup-6`
  - Actions: `ClearHighlightsAsync()`. Move the cursor onto `#SubmitButton` (`MoveToAsync`). Then run the click **and** its navigation inside
    the off-camera wait, because the whole setup runs inside the request of Finish Setup (`OrchardCore.Setup.Core/Services/SetupService.cs:89-111,
    303`, then the redirect `SetupController.cs:209`), so the slow part is the navigation itself:

    ```csharp
    scene.Page.SetDefaultTimeout(180_000);   // Scene.cs:123 sets 20 seconds, and the navigation wait below uses the default
    await scene.OffCameraAsync(async () =>
    {
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#SubmitButton"));   // Scene.cs:149-157: restarts the screencast on the new page
        await scene.Page.Locator("a.navbar-brand", new() { HasText = "Maplewood Bakery" }).WaitForAsync();
    });
    scene.Page.SetDefaultTimeout(20_000);
    ```

    `OffCameraAsync` (`Scene.cs:288-304`) holds 0.9 s on camera, then drops the frames until the lambda ends: the click ripple is inside the cut,
    so the video goes from the cursor on the button to the home page. The Setup screen has no `a.navbar-brand`, so the check can't pass before
    the home page loads. After the lambda, the scene holds on the home page until the narration ends.
  - Visible at the end: the home page of Maplewood (blog theme): the navigation bar shows `Maplewood Bakery` at the top left, and a blog
    with a sample post below.
  - Sources: Setup redirects to the home page of the tenant (`SetupController.cs:209`); the Blog recipe makes a Blog and a post
    (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:965-1058`) and the home route shows the Blog (`:113-121`); the theme prints the
    site name in `a.navbar-brand` (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27`). Setup does not sign the super user in
    (`OrchardCore.Users/Services/SetupEventHandler.cs:21-37`).

**Leaves the site with** the tenant Maplewood, Running, with its database in `App_Data/Sites/Maplewood`. Take snapshot C.

### Clip `05-open` (chapter 05, "Open the site")

**Starts on** `/Admin/Tenants`, from snapshot C.

- `open-1`
  - Actions: `HighlightAsync(<the Maplewood row>)`, then move over the buttons "View", "Disable", "Reload" and "Features" in that order.
    Don't click.
  - Visible at the end: the Maplewood row with the badge `Running`, and the buttons "Edit", "View", "Disable", "Reload" and "Features"; no
    "Setup" button.
  - Sources: `ManageTenantActionButtons.cshtml:9-30` (View `:12`, Disable `:16`, Reload `:30`; Setup only when Uninitialized, `:24`);
    "Features" `OrchardCore.Tenants/Views/ManageFeaturesActionButton.cshtml` (shown for a running tenant that isn't Default, when the
    Features feature is enabled, which it always is: `Services/TenantFeatureShapeTableProvider.cs:9`, `OrchardCore.Features/Manifest.cs`); "Running" `ShellSettingsEntry.State.SummaryAdmin.cshtml`.
- `open-2`
  - Actions: `ClearHighlightsAsync()`. The button "View" (`a.view`) of the Maplewood row opens the site in another tab (`target="_blank"`). The
    recorder has no helper for a new tab: wrap the click in Playwright's `scene.Page.RunAndWaitForPopupAsync(() => scene.ClickAsync(<the View button>))`,
    then call `await scene.SwitchToAsync(popup)`, which makes the new tab the captured page (`Scene.cs:191-212`: it stops the screencast of the
    old page, sets `Page`, waits for the network to be idle and starts the screencast again).
  - Visible at the end: the home page of Maplewood in the second tab, with `Maplewood Bakery` in the navigation bar.
  - Source: `ManageTenantActionButtons.cshtml:12` (the link has no token: `appendQuery: false`).
- `open-3`
  - Actions: on the popup page, `NavigateAsync("/maplewood/Admin")`. Don't type anything.
  - Visible at the end: the sign-in page of Maplewood, with the heading "Log in", the fields "Username or email address" and "Password", and the
    button "Log in". Empty fields.
  - Labels: `OrchardCore.Users/Views/Account/Login.cshtml:27`, `OrchardCore.Users/Views/LoginFormCredentials.cshtml:7,13`, `LoginForm.Edit.cshtml:9`.
    The administration is under `Admin` unless `OrchardCore:Admin:AdminUrlPrefix` is set (`OrchardCore.Admin.Abstractions/AdminOptions.cs:5`); this
    site doesn't set it. See `OPEN-QUESTIONS.md` for what to confirm.

### Clip `06-shortcut` (chapter 06, "Create & Setup")

**Starts on** `/Admin/Tenants/Create` (the Create Tenant form, empty; opened before the capture starts), from snapshot C.

- `shortcut-1`
  - Actions: `TypeAsync(#Name, "Riverside")`, `TypeAsync(#RequestUrlPrefix, "riverside")`, `SelectAsync(#RecipeName, "Blog")`, scroll down,
    `SelectAsync(#DatabaseProvider, "Sqlite")`. Leave Category, Description and Hostname empty.
  - Visible at the end: Name `Riverside`, Url Prefix `riverside`, Recipe `Blog`, and the database type `Sqlite` with no connection fields.
- `shortcut-2`
  - Actions: scroll to the buttons if needed. Click `button[value="createAndSetup"]` ("Create & Setup") with `ClickAndWaitForNavigationAsync`.
    Fill nothing on the Setup screen.
  - Visible at the end: the Setup screen of Riverside (heading "Setup", "What is the name of your site?" empty, no Recipe list, no database
    questions, Default Time Zone opened on `America/Los_Angeles`, the "Super User" group below).
  - Source: with the value `createAndSetup` the controller saves the tenant and redirects to its address with the token
    (`AdminController.cs:426-437`).

**Leaves the site with** the tenant Riverside, Uninitialized. The clip ends there; don't finish the setup.

## 5. Checks for private data

Before assembling, look at every contact sheet (`review.py`) and OCR the result as `SKILL.md` says (one frame per second and at
every scene change). Look for:

- A password: the Password and Password Confirmation fields must show dots only. No password appears in a pop-over, in a caption or in
  a field of the Login page.
- Any email address other than `admin@example.com`, and any name other than `admin`, `Maplewood`, `Maplewood Bakery`, `Riverside`, `Customers`.
- A real machine name, user folder or time zone of this computer: the Default Time Zone list opens on the time zone of the server, in the first
  row of the Setup screen, from `setup-1` through `setup-6` and in `shortcut-2`. The site process must run in `America/Los_Angeles` (section 1), so
  that this row never shows the time zone of the recording machine. Look at the first frame of `setup-1` and of `shortcut-2` for it.
- The Create Tenant form shows the product's own hint text with example addresses (under Url Prefix and Hostname) and, for Sql Server, the
  sample connection string with `User Id=username;Password=password` under Connection string: that is the interface, not private data (a
  password check by OCR may report the word `Password` there).
- The Recipe list of the Create Tenant form: if it holds a recipe whose name is not a product name the video should show (see `OPEN-QUESTIONS.md`),
  only `Blog` is selected on camera.
- No `state.json`, snapshot, `App_Data` folder or mask script is added to git.
