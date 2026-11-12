# Recording plan: shells-and-configuration

Developer video, about 6 minutes of narration. Almost all of it is slides, which `slides.py` draws and which need no
recording. One chapter has a browser recording: the clip `03-tenants` (two steps), which shows the Tenants page of the
Default tenant and the Edit page of a tenant that is prepared in the configuration. Nothing in this video shows code
in a browser: every file, JSON and command is a slide.

Selectors below stand for `scene.Page.Locator(...)`. Labels and selectors were read from the source of
`OrchardCMS/OrchardCore` at commit `92db4a83` (the paths are relative to `src/OrchardCore.Modules/OrchardCore.Tenants`).
Check each selector once with `Recorder probe` on the running site before writing the scene.

## The site it needs

- A checkout of `OrchardCMS/OrchardCore`, built from `src/OrchardCore.Cms.Web`, running on `http://localhost:5010`.
- `ORCHARD_APP_DATA` points at an empty folder outside both repositories. This folder is the `App_Data` folder of the
  site. Call it `<app-data>` below.
- Auto Setup provisions the Default tenant on its first request, through environment variables (no setup screen, no
  sign-in form is recorded):

  ```bash
  export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
  export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
  export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
  export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
  export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
  export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
  export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
  export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
  ```

  The password is `<password>`: chosen outside git and passed in `VIDEO_PASSWORD`. The recorder signs in as `admin`
  (`VIDEO_USER_NAME=admin`).
- Block outbound calls: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address. No external service is needed, and none
  has a stand-in: the Azure Blob Storage and database providers of the last chapter appear on slides only.
- The Tenants feature is enabled on the Default tenant, outside the capture (see "Seed data").
- Recorded with Auto Setup, with `state.json` from `Recorder setup`. This is not a video about the Setup screen.

## Seed data

Create these outside the capture. All the values are invented.

1. **Before the first start of the site**, create the file `<app-data>/appsettings.json` with this content. It is the
   global tenant configuration file of the slides: a section named for a tenant, with a State. The site reads this file
   once when it starts, so if the site already ran, stop it, create the file, and start it again.

   ```json
   {
     "OrchardCore": {
       "Acme": {
         "State": "Uninitialized",
         "RequestUrlPrefix": "acme",
         "Category": "Demo",
         "Description": "The Acme sample site.",
         "DatabaseProvider": "Sqlite"
       }
     }
   }
   ```

   The slide `prepare-1` shows the same values in an `appsettings.json`.
2. Start the site with the Auto Setup variables above and open `http://localhost:5010/` once, so Auto Setup sets up the
   Default tenant with the Blog recipe.
3. Run `Recorder setup` (signs in as `admin` and writes `state.json`, which is git-ignored).
4. Enable the Tenants feature on the Default tenant: **Tools > Features**, search for `Tenants`, select the feature
   named **Tenants** (category Infrastructure, id `OrchardCore.Tenants`), and enable it (label from
   `OrchardCore.Features/AdminMenu.cs:26-27`, `OrchardCore.Tenants/Manifest.cs:11-12`). A `PrepareAsync` in the scenes can
   do the same with a page `fetch`.
5. Open `http://localhost:5010/Admin/Tenants` and check the page: it lists **Default** (Running) and **Acme**
   (Uninitialized, with the badges **Demo** and **Sqlite**, and a **Setup** button). If Acme is missing, the file of
   step 1 was not read: check its place and its JSON, and start the site again.
6. Stop the site and copy `<app-data>` and `state.json` aside. This is the snapshot to restore before a new take.

Do not set up the Acme tenant, and do not save its Edit page: the clip must find it Uninitialized.

## Order of the clips and what changes the site

There is one clip to record: `03-tenants`. It does not depend on another clip. It reads the site and changes nothing
(it opens the Edit page and leaves it without saving), so it can be recorded again without restoring the snapshot, as
long as Acme is still Uninitialized. After a take, check that `<app-data>/Sites/Acme` does not exist.

The slide steps need no recording; `assemble.py` draws them from `slides.py`.

## Clip `03-tenants`

Chapter 03, "Preparing a tenant". The slide `prepare-1` plays before this clip.

Starts on `/Admin/Tenants` (`OpenAsync`, not recorded), signed in as `admin`. Before `StartCaptureAsync`, expand the
**Multi-Tenancy** group of the admin menu so that **Tenants** is visible and selected, and scroll to the top of the
page. The default list layout is the List layout (`OrchardCore.Admin.Abstractions/AdminListOptions.cs:30`) with action
buttons (`:36`), so a row shows the name, then its badges, then **Edit**, **Setup** and **Reload**.

Name the row of Acme once and reuse it:

- row of Acme: `li.list-group-item` that has the text `Acme`. If the probe shows another element, use the closest
  container of the link `Acme` that also contains the **Setup** button.
- state badge: `span.badge` with the text `Uninitialized`, in that row.
- category badge: `span.badge` with the text `Demo`, in that row.
- database badge: `span.badge` with the text `Sqlite`, in that row.
- **Setup** button: `#btn-setup-Acme`.
- **Edit** button: `a.edit` in that row (its `href` ends with `/Admin/Tenants/Edit/Acme`).

### `tenants-1`

Narration: "With the Tenants feature enabled in the Default tenant, the Tenants page under Multi-Tenancy lists the
prepared tenant. Acme is Uninitialized, with its category and database provider from the configuration, and a Setup
button."

Actions (shorter than the narration):

1. `MoveToAsync` the link `Acme` of the row (about 1 s).
2. `HighlightAsync` the category badge with the caption `Demo` and the database badge with the caption `Sqlite`
   (hold about 2 s), then `ClearHighlightsAsync`.
3. `HighlightAsync` the state badge with the caption `Uninitialized`, then, after about 1.5 s, `ClearHighlightsAsync`
   and `HighlightAsync` `#btn-setup-Acme`.

On screen when the narration ends: the Tenants page with the row of Acme: the name **Acme**, the badges **Demo**,
**Sqlite** and **Uninitialized**, and a green ring around the **Setup** button. The Default row (Running) can be
visible. No dialog, no error, no notification.

Labels: **Multi-Tenancy** and **Tenants** `AdminMenu.cs:30,33`; the page title **Tenants** `Views/Admin/Index.cshtml:9`;
**Uninitialized** `Views/ShellSettingsEntry.State.SummaryAdmin.cshtml:12`; the category badge
`Views/ShellSettingsEntry.Category.SummaryAdmin.cshtml:8`; the database badge
`Views/ShellSettingsEntry.Database.SummaryAdmin.cshtml:8`; **Setup** `Views/ManageTenantActionButtons.cshtml:24`.

### `tenants-2`

Narration: "Let's open Edit. It shows the same values: the Url Prefix and the Description, and under Database Presets,
the database provider that the setup of this tenant uses."

Actions (shorter than the narration):

1. `ClearHighlightsAsync`, then `ClickAndWaitForNavigationAsync` on the **Edit** button of the Acme row. Wait until
   the breadcrumb **Edit Tenant** is visible.
2. `HighlightAsync` the **Url Prefix** field (`#RequestUrlPrefix`, value `acme`) with the caption `Url Prefix`, hold
   about 1.5 s, `ClearHighlightsAsync`.
3. `HighlightAsync` the **Description** field (`#Description`, value `The Acme sample site.`), hold about 1.5 s,
   `ClearHighlightsAsync`.
4. `ScrollAsync(0, 400)` so the **Database Presets** part is visible, then `HighlightAsync` the drop-down list
   `#DatabaseProvider` (selected value `Sqlite`) with the caption `Database provider`.

On screen when the narration ends: the **Edit Tenant** page with the name **Acme** (read-only), the Url Prefix
`acme`, the Description `The Acme sample site.` and, below, **What type of database to use?** set to **Sqlite**, with
a ring around it. Do not click **Save**: leave the page open.

Labels: **Edit** `Views/ManageTenantActionButtons.cshtml:9`; **Edit Tenant** `Views/Admin/Edit.cshtml:5`; **Category**
`Views/Shared/_TenantInfo.cshtml:4`; **Url Prefix** `Views/Shared/_TenantInfo.cshtml:12`; **Description**
`Views/Shared/_TenantInfo.cshtml:21`; **Database Presets** and **What type of database to use?**
`Views/Shared/_TenantDatabaseInfo.cshtml:18,35` (line 35 is the drop-down list; line 26 is the read-only variant). The
fields show the values of the configuration because the Edit action reads them from the settings of the tenant
(`Controllers/AdminController.cs:458-511`). The option text of the drop-down list is the name of the provider,
**Sqlite** (`OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:53`).

The heading **Database Presets** and the drop-down list appear only while no `DatabaseProvider` is set at the root of
the `OrchardCore` section: otherwise the page shows a read-only field and no heading
(`_TenantDatabaseInfo.cshtml:94-125`, `AdminController.cs:764-794`). The seed above does not set one at the root; the
Acme section holds its own. If the probe shows a read-only field, look for a `DatabaseProvider` in the host
configuration or in an environment variable and remove it.

## The slide steps

One line each: what the slide shows. Each slide is drawn by `slides.py`.

- `shell-1`: two cards, **Settings** (the state, the URL prefix and the URL host) and **Configuration** (read through
  `IShellConfiguration`).
- `shell-2`: the `App_Data` tree with `tenants.json` and the `Sites` folder (`Default`, `Acme`), and two cards that
  name the settings file and the configuration file of a tenant.
- `shell-3`: two cards, `tenants.json` and a configuration section with a State.
- `shell-4`: the `ORCHARD_APP_DATA` environment variable, with a relative and an absolute path.
- `layers-1`: four cards for the layers; the first, **Host application**, is bright.
- `layers-2`: the same four cards; **Files in App_Data** is bright.
- `layers-3`: the same four cards; **Section named for the tenant** is bright.
- `layers-4`: the same four cards; **File of the tenant** is bright.
- `prepare-1`: a JSON file with a section `Acme` under `OrchardCore` (State, RequestUrlPrefix, Category, Description,
  DatabaseProvider), and three cards: State, Uninitialized, Other values.
- `override-1`: a JSON file with a `Media` section under `Default`, and two cards: No State, Only Default.
- `override-2`: three environment variable names, for every tenant, the Default tenant and the tenant named Acme.
- `override-3`: three cards: a tenant section, a module section, and the same path.
- `shared-1`: a JSON file with `ConnectionString` and `DatabaseProvider` at the root of `OrchardCore`, and a `Default`
  section with a `TablePrefix`; two cards.
- `shared-2`: a JSON file with the `Tenants` section (`RequireTablePrefix`, `TablePrefixPattern`, `SchemaPattern`);
  three cards.
- `shared-3`: two cards: not copied into tenants, one place to change.
- `modules-1`: a C# `Startup` class that takes `IShellConfiguration` and binds a section with `Configure`.
- `modules-2`: a JSON section `MyService` / `MyProvider` and the matching environment variable.
- `modules-3`: C# code that calls `ConfigureServices` on the Orchard Core builder and `PostConfigure`.
- `stores-1`: three cards: `App_Data`, Azure Blob Storage, Database.
- `stores-2`: C# host code with `AddAzureShellsConfiguration`, and the name of the package.
- `stores-3`: the `Shells` / `Azure` JSON options, with the connection string, the container name and the base path in
  bright.
- `stores-4`: the tree of the container (`tenants.json`, the root `appsettings.json` files, the `Sites` folder) and
  three cards.
- `stores-5`: C# host code with `AddDatabaseShellsConfiguration`, and the `Shells` / `Database` JSON options.
- `stores-6`: the Azure and Database options side by side, with `MigrateFromFiles` in bright in each, and a line that
  says the options are read from the host configuration.
- `stores-7`: C# host code that adds the database provider only outside Development, and two cards: meant for, not
  meant for.

## Checks for private data

- The clip shows the names `Default` and `Acme`, the badges, and the text of the Edit page. It must not show a machine
  path, a user name of this machine, a real e-mail address, or a password. The admin e-mail, if it appears, is
  `admin@example.com`.
- Look at the contact sheet of `03-tenants` (`review.py`): the frame at the end of each step must show what its
  narration describes, with no error and no empty list. Acme must be listed, with its three badges.
- OCR the final video at one frame per second and at every scene change, and search for the machine's user name, any
  real domain, `@` addresses other than `@example.com`, and the value of `VIDEO_PASSWORD`.
- The slides contain only invented names (`Acme`, `MyService`, `hostcontainer`), empty connection strings and `...`:
  no real connection string, key or token. Keep it so when a slide changes.
