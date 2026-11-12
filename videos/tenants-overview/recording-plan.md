# Recording plan: Tenants: Run Many Sites From One Application

This plan is for the person who records the clips. It is written so that nothing has to be decided while recording.
In this file a selector stands for `scene.Page.Locator(...)`, and a source reference points to the Orchard Core source
(`src/OrchardCore.Modules/...` of `OrchardCMS/OrchardCore`). The selectors are derived from the views, not tested
against a running site: confirm each one in the browser first (see `OPEN-QUESTIONS.md`).

The video has four recorded clips (`03-enable`, `04-list`, `05-actions`, `06-add`) and two chapters of slides
(`01-concepts`, `02-module`). The opening and closing cards need nothing recorded.

## The site it needs

A fresh site on the Default tenant, with three more tenants. The video shows the admin only; it never shows a
setup screen, a sign-in, or a database.

- **Run** `src/OrchardCore.Cms.Web` of an Orchard Core checkout on `http://localhost:5010`, with `ORCHARD_APP_DATA`
  pointing at an empty folder outside both repositories, and `HTTP_PROXY` / `HTTPS_PROXY` set to a dead address
  (for example `http://127.0.0.1:9`), so the site never calls an outside service.
- **Provision it with Auto Setup**, through environment variables (the names are the ones of the Auto Setup
  documentation, `src/docs/reference/modules/AutoSetup/README.md`). The Default tenant uses the Blog recipe; the two
  extra entries are created as uninitialized tenants when the Default tenant is installed, and each one is installed
  by the first request to its own URL prefix (`AutoSetupMiddleware.cs`, `AutoSetupService.cs`).

  | Entry | ShellName | SiteName | RecipeName | RequestUrlPrefix |
  | --- | --- | --- | --- | --- |
  | 0 | `Default` | `Orchard Core` | `Blog` | (empty) |
  | 1 | `Bakery` | `Corner Bakery` | `Blog` | `bakery` |
  | 2 | `Studio` | `Harbor Light Studio` | `Agency` | `studio` |

  For all three entries: `SiteTimeZone` `America/Los_Angeles`, `AdminUsername` `admin`, `AdminEmail`
  `admin@example.com`, `AdminPassword` `<password>` (the one passed in `VIDEO_PASSWORD`, chosen outside git),
  `DatabaseProvider` `Sqlite`, `DatabaseConnectionString`, `DatabaseTablePrefix` and `DatabaseSchema` empty. The
  variable for entry 1 and its prefix is `OrchardCore__AutoSetup__Tenants__1__RequestUrlPrefix`, and so on for the
  other parameters. Do not set a database provider in the root configuration: the Add Tenant form of `06-add` must
  show the "Database Presets" heading and the "What type of database to use?" list, which it only does when no
  provider is preset (`Views/Shared/_TenantDatabaseInfo.cshtml:15-21` for the heading, `:32-46` for the list).
- **Sign the recorder in** as `admin` with `Recorder setup`, once, with a browser that has never used the
  list layout selector of the admin lists. The sign-in file must not hold the cookie
  `orchardcore_admin_list_layout`; the lists must open in the default List layout with the Buttons actions
  (`AdminListOptions.cs`). Never click the List/Grid selector while recording.
- **No other site settings**, no feature profile (so the Add Tenant form shows no "Feature profile" field), and no
  other admin setting changed. Tenant removal stays off (the default), so no Remove button appears.

### Features to enable before recording

None before `03-enable`. The video enables `OrchardCore.Tenants` on camera in that clip. Every later clip needs it
enabled (see the snapshots below). The other features of the module stay disabled.

### The seed data, created outside the capture

Fake data only. Step 1 is done before `03-enable` is recorded (it goes into snapshot A). Steps 2 to 5 are done
after it, once the Tenants feature is on, in the admin of the Default tenant under Multi-Tenancy, Tenants (they go
into snapshot C).

1. **Install Default, Bakery and Studio, in this order.** The settings of Bakery and Studio exist only after the
   Default tenant is installed (`AutoSetupMiddleware.cs:133-143`), and until then a request to `/bakery` is answered by
   the Default tenant, which is the catch-all (`RunningShellTable.cs:74-78`) and gets installed instead. So request the
   home page first, then each prefix, and wait for each answer:
   `curl -s -L --noproxy localhost -o /dev/null -w "%{http_code}" http://localhost:5010/`, then the same for
   `http://localhost:5010/bakery`, then for `http://localhost:5010/studio` (a status of 200 when it is done; it can
   take several seconds each). Then sign the recorder in (`Recorder setup`), so that `state.json` belongs to the
   installed site.
2. **Edit Bakery** (Edit button): Category `Customers`, Description `Website of a corner bakery.`, Save. Keep its
   Url Prefix `bakery`.
3. **Edit Studio**: Category `Customers`, Description `Portfolio of a photography studio.`, clear Url Prefix, set
   Hostname `studio.example.com`, Save. This tenant is never opened: its host name does not resolve on this machine.
4. **Disable Studio**: its Disable button, then Ok in the confirmation.
5. **Add Workshop** (Add Tenant): Name `Workshop`, Category `Internal`, Url Prefix `workshop`, Description
   `Practice site for the editors.`, Recipe `Blank site`, the database list left on `None`. Select Create (not
   Create & Setup). It stays uninitialized: never open it.

Check the list afterwards. Sorted by name it must show four rows:

| Name | Badges | State | Below the name |
| --- | --- | --- | --- |
| Bakery | Customers, Sqlite, Blog | Running | Website of a corner bakery. |
| Default | Sqlite (it may show no recipe badge) | Running | nothing |
| Studio | Customers, Sqlite, Agency | Disabled | Portfolio of a photography studio. and studio.example.com |
| Workshop | Internal, Blank | Uninitialized | Practice site for the editors. |

Open Bakery with its View button once, outside the capture, to warm it up and to check that its home page shows the
site name "Corner Bakery". Do not open Studio: its Agency recipe holds sample team members and client brand names, and
its masthead says "Welcome To Our Studio!", which is confusing next to the tenant named Studio. It must never be on
screen.

## Snapshots, order and dependencies

Clips are recorded in this order, which is also the order they play in. Only `03-enable` changes the site.

1. Install the Default, Bakery and Studio tenants (seed step 1, the home page first), sign the recorder in, and stop
   the site. Copy its `App_Data` aside as **snapshot A** (Tenants disabled) and `state.json` next to it.
2. Start the site from snapshot A and record **`03-enable`**. It enables the Tenants feature. To record it again,
   stop the site, restore snapshot A, start the site.
3. Seed steps 2 to 5, stop the site, and copy `App_Data` aside as **snapshot C** (Tenants enabled, the four rows
   above).
4. Start the site from snapshot C and record **`04-list`**, **`05-actions`** and **`06-add`** in any order: none of
   them saves anything (`04-list` and `06-add` never submit an action; `05-actions` only opens the Bakery site in
   another tab). Restore snapshot C before a new take only if a take went wrong and changed a tenant.

## The clips

Every clip starts signed in as `admin` (from `state.json`), with the admin menu collapsed, on the page named below, and
`StartCaptureAsync()` is called once that page is ready. The narration lengths are about the number of words at
150 words per minute; keep the actions of a step shorter than that.

### Chapter 01, `01-concepts` (slides, nothing to record)

- `idea-1`: three cards: One application, Many tenants, Kept apart.
- `idea-2`: three cards about the Default tenant: The first tenant, Manages the others, Cannot be disabled.
- `idea-3`: three requests (`example.com`, `example.com/bakery`, `studio.example.com`) with an arrow to the tenant
  that answers each one: Default, Bakery, Studio.
- `idea-4`: three cards: Uninitialized, Running, Disabled. The tenant has two more states, Initializing (while its
  setup runs) and Invalid (its settings are invalid), which the State menu of the list does not offer; the slide and
  the narration leave them out and do not say that three states exist (`TenantState.cs`,
  `ShellSettingsEntry.State.SummaryAdmin.cshtml:14-25`).

### Chapter 02, `02-module` (slides, nothing to record)

- `module-1`: the four features of the module as cards, Tenants bright.
- `module-2`: the same cards, Tenant Feature Profiles and Distributed Tenants bright.
- `module-3`: the same cards, Static File Provider bright.
- `module-4`: three cards about the permissions Manage tenants and Manage tenant feature profiles, and the
  Administrator role.

### Chapter 03, `03-enable` (about 22 seconds of narration)

Starts on `/Admin` (the dashboard), from snapshot A. Changes the site: it enables the Tenants feature.

- `enable-1` (about 10 seconds). Actions: click the menu group Tools, then click Features
  (`ClickAndWaitForNavigationAsync`); once the Features page is shown, type `tenants` into the search box
  (`#search-box`, placeholder "Search") with `TypeAsync`. Wait until the summary line (`#list-summary`) shows fewer
  features than the total. On screen at the end of the narration: the Features page, `tenants` in the search box, and
  the matching features in the list, grouped under category headings, the Tenants row among them with an Enable
  button. The search matches the name, the id and the description of a feature, so the rows are the four features of
  the module and, probably, the Auto Setup row, whose description contains the word tenants (it has no Enable button).
  The narration says only that the features of the module are among the results. Source:
  `OrchardCore.Features/AdminMenu.cs:26-27`, `OrchardCore.Features/Views/Admin/Features.cshtml`,
  `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`, `OrchardCore.AutoSetup/Manifest.cs:8`.
- `enable-2` (about 4 seconds). Actions: with `MoveToAsync`, move the cursor to the Infrastructure heading above the
  Tenants row, then to the Enable button of the Tenants row (the row is the list item that holds the `code` text
  `OrchardCore.Tenants`; the button id is `btn-enable-OrchardCore_Tenants`, check it in the browser). Start the click in
  the second half of the narration, and cut the rebuild out of the clip:
  `await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.ClickAsync(button)));` then wait until
  the toast "The feature Tenants was enabled." is on screen. The click is a POST that redirects to the Features page,
  and only `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` and `NavigateAsync` restart the screencast, so a
  plain `ClickAsync` followed by a wait for the toast inside `OffCameraAsync` would leave the rest of the clip frozen
  (`Scene.cs`, and the pitfalls of `references/recording.md`). The site rebuilds the tenant, which can take a few
  seconds. On screen at the end: the Features page after the reload, with the toast. Source:
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable), `OrchardCore.Tenants/Manifest.cs:14`
  (category Infrastructure), `OrchardCore.Features/Controllers/AdminController.cs:56-66` (the message).
- `enable-3` (about 8 seconds). Actions: the toast from `enable-2` is on screen; click the menu group Multi-Tenancy
  so that its item Tenants shows. On screen at the end: the toast "The feature Tenants was enabled." and the
  expanded Multi-Tenancy group with Tenants in it. Source: `OrchardCore.Tenants/AdminMenu.cs:30-38`.

### Chapter 04, `04-list` (about 38 seconds of narration)

Starts on `/Admin` (the dashboard), from snapshot C.

- `list-1` (about 16 seconds). Actions: click the menu group Multi-Tenancy, then click Tenants
  (`ClickAndWaitForNavigationAsync`); then move the cursor over the name Bakery, its badges and its description, and
  over the Studio row where the host name shows under the description. On screen at the end: the list with the four
  rows, no scrolling needed, each with its name and State badge. A row shows a badge for its category, database
  provider or recipe only when the tenant has one: Default has no category and, probably, no recipe badge, and
  Workshop has no database badge. Source: `Views/Admin/Index.cshtml`,
  `Views/ShellSettingsEntry.SummaryAdmin.cshtml`, `ShellSettingsEntry.Category.SummaryAdmin.cshtml`,
  `ShellSettingsEntry.Database.SummaryAdmin.cshtml`, `ShellSettingsEntry.Recipe.SummaryAdmin.cshtml`,
  `ShellSettingsEntry.State.SummaryAdmin.cshtml`, `ShellSettingsEntry.Description.SummaryAdmin.cshtml`.
- `list-2` (about 14 seconds). Actions: in turn, first `HighlightAsync` on the search box (`#search-box`, placeholder
  "Search"), then on the three menus at the right of the toolbar (the buttons titled Category, State and Sort in
  `div.filter`; do not open them), and finally click the Disabled badge of the Studio row
  (`li:has(a.text-break:text-is("Studio")) a[href*="Options.Status=Disabled"]`, with `ClickAndWaitForNavigationAsync`).
  Call `ClearHighlightsAsync()` before the click. On screen at the end: the list filtered to the Disabled tenants,
  which is the Studio row alone. Source: `Views/Admin/Index.cshtml:22-27`, `Views/TenantsFilters.cshtml`,
  `Views/ShellSettingsEntry.State.SummaryAdmin.cshtml`.
- `list-3` (about 9 seconds). Actions: `NavigateAsync("/Admin/Tenants")` to show the whole list again, tick the
  checkbox of the Bakery row and the checkbox of the Studio row (`#tenantNames-Bakery`, `#tenantNames-Studio`), then
  click the Actions button of the toolbar (`#bulk-action-menu-button`). On screen at the end: both boxes ticked
  and the open Actions menu with Disable and Enable. Do not choose either item. Source:
  `OrchardCore.Admin/Views/AdminListToolbar.cshtml:67`, `Controllers/AdminController.cs:218-222`,
  `Views/ShellSettingsEntry.Checkbox.SummaryAdmin.cshtml`.

### Chapter 05, `05-actions` (about 44 seconds of narration)

Starts on `/Admin/Tenants`, from snapshot C. In this clip a row is `li:has(a.text-break:text-is("<name>"))` and its buttons
are in the last column of the row. The Features and the Enable buttons both have the class `enable`: find buttons by
their text, for example `a.btn:text-is("Features")`.

- `actions-1` (about 13 seconds). Actions: `HighlightAsync` on the buttons of the Default row (Edit, View and Reload),
  as one ring around the column of buttons. On screen at the end: the Default row with a ring around Edit, View and
  Reload, and no Disable button. Source: `Views/ManageTenantActionButtons.cshtml`.
- `actions-2` (about 15 seconds). Actions: `ClearHighlightsAsync()`, then `HighlightAsync` on the Disable button and
  then on the Features button of the Bakery row. Before the last sentence, subscribe to the `Page` event of the browser
  context, click the View button of the Bakery row (it opens a tab, `target="_blank"`), and call `SwitchToAsync` on
  the new page. Wait for the page to load. On screen at the end: the home page of the Corner Bakery site in the other
  tab. Source:
  `Views/ManageTenantActionButtons.cshtml`, `Views/ManageFeaturesActionButton.cshtml`.
- `actions-3` (about 17 seconds). Actions: `SwitchToAsync` back to the admin page (keep the `IPage` of the admin tab
  in a variable before `SwitchToAsync` moves to the new tab in `actions-2`), `ClearHighlightsAsync()`, then
  `HighlightAsync` on the Enable button of the Studio row and on the Setup button of the Workshop row (do not click
  them). The Studio row has only Edit, Enable and Reload, and the Workshop row only Edit, Setup and Reload: View,
  Disable and Features are for Running tenants. On screen at the end: the list, rings on Enable (Studio) and Setup
  (Workshop), and no Remove button on any row. Source: `Views/ManageTenantActionButtons.cshtml:10-29`,
  `Views/ManageFeaturesActionButton.cshtml:4`.

### Chapter 06, `06-add` (about 36 seconds of narration)

Starts on `/Admin/Tenants`, from snapshot C. Nothing is saved: the clip ends with Cancel.

- `add-1` (about 12 seconds). Actions: click Add Tenant (`ClickAndWaitForNavigationAsync`); in the form, `TypeAsync`
  `Gallery` in Name, `Customers` in Category, and `Website of an art gallery.` in Description. On screen at the end: the
  Create Tenant form with those three fields filled. Source: `Views/Admin/Index.cshtml:27`,
  `Views/Admin/Create.cshtml`, `Views/Shared/_TenantInfo.cshtml`.
- `add-2` (about 11 seconds). Actions: `TypeAsync` `gallery` in Url Prefix and leave Hostname empty; `HighlightAsync`
  on the Url Prefix and Hostname fields. On screen at the end: both fields ringed, Url Prefix filled. Source:
  `Views/Shared/_TenantInfo.cshtml:11-37`, `Services/TenantValidator.cs:79-90`.
- `add-3` (about 13 seconds). Actions: `SelectAsync` the Recipe list to `Blank site`; show the "Database Presets"
  heading and its list (scroll with `ScrollAsync` if the form is taller than the screen, then scroll back); call
  `ClearHighlightsAsync()`, `HighlightAsync` on Create and on Create & Setup in the bottom action bar; in the last
  three seconds click Cancel (`a.cancel`, `ClickAndWaitForNavigationAsync`). On screen at the end: the list of tenants
  again, without a Gallery row. Source: `Views/Shared/_TenantRecipes.cshtml`, `Views/Shared/_TenantDatabaseInfo.cshtml`,
  `Views/Admin/Create.cshtml:28-31`.

## Checks for private data

- Every address, host name and email on screen is `example.com`, `localhost`, `studio.example.com` or
  `admin@example.com`. The Hostname field of the Add Tenant form shows a hint with the host `orchardproject.net`
  in the product's own text: that is expected and is not private data.
- Names on screen: Orchard Core, Corner Bakery, Bakery, Studio, Workshop, Gallery, admin, and the sample text of the
  Blog recipe on the Bakery site (`actions-2`): the menu Home and About, the blog title and the description "This is
  the description of your blog", the post "Man must explore, and this is exploration at its greatest" with its subtitle
  "Problems look mighty small from 150 miles up", its tags (Earth, Exploration, Space) and category (Travel), the line
  "Posted by admin on" followed by the date of the day (the author is the user `admin`), and the footer text
  "Copyright © Your Website 2021" (`Recipes/Snippets/footer.html` of the Blog theme). None of them is a real person's
  data. Harbor Light Studio, the sample team members
  and the client brand names of the Agency recipe must not appear anywhere. No other person, company, phone number or
  address.
- No password is typed or shown in any clip: the sign-in comes from `state.json`, which is never committed.
- OCR the final video at one frame per second and at every scene change, and search for any `@` that is not
  `@example.com`, and for any name that is not in the list above. The Bakery site (`actions-2`) must also be scanned.
- The address bar is never visible (headless capture), so ports and local paths do not show.
