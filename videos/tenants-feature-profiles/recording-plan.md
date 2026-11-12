# Recording plan: Tenant Feature Profiles

Read this file and nothing else to record the video. Paths such as `Tenants/Views/...` are in the Orchard Core source
at commit `92db4a83`: `Tenants/` is `src/OrchardCore.Modules/OrchardCore.Tenants/`, `Features/` is
`src/OrchardCore.Modules/OrchardCore.Features/`, `Modules/` is `src/OrchardCore.Modules/`, `Themes/` is
`src/OrchardCore.Themes/` and `Core/` is `src/OrchardCore/OrchardCore/`. A selector written here stands for
`scene.Page.Locator(...)`. Anything this plan could not confirm from the source is listed in `OPEN-QUESTIONS.md`:
read it before the first take.

The video has four recordings (`03-enable`, `04-create`, `05-assign`, `06-effect`) and six slides. The opening card,
the closing card and the chapter cards need nothing from the recorder.

## The site

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` at commit `92db4a83` (or a later `main` where the labels below are unchanged), on `http://localhost:5010` |
| App_Data | An empty folder outside both repositories, passed as `ORCHARD_APP_DATA` |
| Setup | Auto Setup from environment variables (below). No setup or sign-in form is recorded |
| Default tenant | Recipe **SaaS** (Software as a Service), site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed as `VIDEO_PASSWORD`) |
| Other tenants | `Bluebird` (URL prefix `bluebird`) and `Maplewood` (URL prefix `maplewood`), recipe **Blog**, same user and password |
| Features enabled before recording | Whatever the SaaS and Blog recipes enable. That includes **Tenants** and **Features** on the Default tenant. **Tenant Feature Profiles** must be disabled: the first recording enables it |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls no outside service |
| External services | None |
| Browser | The recorder's Chromium (1536x864, light, `en-US`), signed in with `state.json` on the Default tenant only. The video never signs in to Bluebird or Maplewood |

The video uses the SaaS recipe for the Default tenant because the Blog recipe does not enable the **Tenants** feature
(`Themes/TheBlogTheme/Recipes/blog.recipe.json`), and the SaaS recipe does (`Themes/TheTheme/Recipes/saas.recipe.json`).
The tenants use the Blog recipe: it enables neither Workflows nor Email, so both stay available and disabled on them.

## Seed data (made outside the capture, in this order)

1. Set the environment variables, with `<password>` replaced by the password chosen outside git. The section is read
   from `OrchardCore:AutoSetup` (`Modules/OrchardCore.AutoSetup/Startup.cs:26`).

   ```bash
   export ORCHARD_APP_DATA=<empty folder>
   export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
   export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
   export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
   export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
   export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
   export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
   export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
   export OrchardCore__AutoSetup__Tenants__0__RecipeName=SaaS

   export OrchardCore__AutoSetup__Tenants__1__ShellName=Bluebird
   export OrchardCore__AutoSetup__Tenants__1__SiteName="Bluebird Bakery"
   export OrchardCore__AutoSetup__Tenants__1__SiteTimeZone=America/Los_Angeles
   export OrchardCore__AutoSetup__Tenants__1__AdminUsername=admin
   export OrchardCore__AutoSetup__Tenants__1__AdminEmail=admin@example.com
   export OrchardCore__AutoSetup__Tenants__1__AdminPassword=<password>
   export OrchardCore__AutoSetup__Tenants__1__DatabaseProvider=Sqlite
   export OrchardCore__AutoSetup__Tenants__1__RecipeName=Blog
   export OrchardCore__AutoSetup__Tenants__1__RequestUrlPrefix=bluebird

   export OrchardCore__AutoSetup__Tenants__2__ShellName=Maplewood
   export OrchardCore__AutoSetup__Tenants__2__SiteName="Maplewood Garden Center"
   export OrchardCore__AutoSetup__Tenants__2__SiteTimeZone=America/Los_Angeles
   export OrchardCore__AutoSetup__Tenants__2__AdminUsername=admin
   export OrchardCore__AutoSetup__Tenants__2__AdminEmail=admin@example.com
   export OrchardCore__AutoSetup__Tenants__2__AdminPassword=<password>
   export OrchardCore__AutoSetup__Tenants__2__DatabaseProvider=Sqlite
   export OrchardCore__AutoSetup__Tenants__2__RecipeName=Blog
   export OrchardCore__AutoSetup__Tenants__2__RequestUrlPrefix=maplewood
   ```

   Leave `FeatureProfile` out of every tenant: the video assigns it on camera.
2. Start the site. Request `http://localhost:5010/` and wait until it returns the Default tenant's home page. That
   sets up the Default tenant and creates the other two tenants as uninitialized
   (`Modules/OrchardCore.AutoSetup/AutoSetupMiddleware.cs:133`, the `IsDefault` branch).
3. Request `http://localhost:5010/bluebird` and then `http://localhost:5010/maplewood`. The first request of each sets
   the tenant up. Wait until each returns a home page. A `503` with "The requested tenant is not initialized." means
   the tenant was set up by another request and its shell is being reloaded
   (`Modules/OrchardCore.AutoSetup/AutoSetupMiddleware.cs:120-126`): ask again.
4. Run `Recorder setup` (user `admin`, password from `VIDEO_PASSWORD`), which saves `state.json`.
5. Open `/Admin/Tenants` and select **Edit** on Bluebird. Type the **Description** `Bluebird Bakery website` and
   select **Save**. Do the same for Maplewood with the **Description** `Maplewood Garden Center website`. (An initialized
   tenant's Edit Tenant page shows no recipe or database fields: `Tenants/Views/Admin/Edit.cshtml:24-32`.)
6. Check the starting state, then do not touch the site again until the snapshot:
   - `/Admin/Tenants` lists Default, Bluebird and Maplewood, each with the **Running** badge, and no badge with a
     layer icon on any row.
   - The admin menu has **Multi-Tenancy** with only **Tenants** under it.
   - `/Admin/Features`: typing `OrchardCore.Workflows` in the search box lists four features under **Workflows**;
     typing `Tenant Feature Profiles` lists one feature, with an **Enable** button.
   - `/Admin/Features/Bluebird` and `/Admin/Features/Maplewood` list the same four Workflows features, and
     typing `OrchardCore.Email` lists **Email**, **SMTP Email Provider** and **Azure Communication Services Email**.
7. Stop the site (by its process ID). Copy `ORCHARD_APP_DATA` and `state.json` aside as **snapshot S0**.

## Order of the clips, and the snapshots

Every recording starts from the state the previous one left, so record them in the order they play:
`03-enable`, `04-create`, `05-assign`, `06-effect`. Clip `06-effect` only reads. The others change the site.

| Clip | Needs | Changes the site | Snapshot to restore before a new take |
|---|---|---|---|
| `03-enable` | S0 | Enables Tenant Feature Profiles on the Default tenant | S0 |
| `04-create` | S3 | Creates the profile `Basic sites` | S3 |
| `05-assign` | S4 | Sets the profile of Bluebird | S4 |
| `06-effect` | S5 | Nothing | S5 (or none) |

After recording a clip, stop the site and copy `ORCHARD_APP_DATA` and `state.json` aside: after `03-enable` that is
**S3**, after `04-create` **S4**, after `05-assign` **S5**. To record a clip again, stop the site, restore the
snapshot in the table, and start the site again. For a full take, restore S0 first.

Each recording opens its first page before the capture starts (`scene.OpenAsync`), then starts the capture. Keep every
step's actions shorter than its narration, and take every wait from the condition written in the step.

## Recordings

### `03-enable` (chapter 3, "Enable the feature")

Starts on `/Admin` (the dashboard of the Default tenant). The admin menu is visible, with **Tools** collapsed.

**`enable-1`**

- Actions: move to the **Tools** entry of the admin menu and click it to expand it, then click **Features** under it
  (the page loads: `ClickAndWaitForNavigationAsync`). Click `#search-box` and type `Tenant Feature Profiles`.
- At the end of the narration: the **Features** page, with `Tenant Feature Profiles` in the search box and one feature
  left: **Tenant Feature Profiles**, with its ID `OrchardCore.Tenants.FeatureProfiles` beside it and an **Enable**
  button, under the heading **Infrastructure**.
- Labels: **Tools** and **Features** `Features/AdminMenu.cs:26-27`; the search box (placeholder **Search**)
  `Features/Views/Admin/Features.cshtml:27`; the search matches a feature's name, ID and description
  `Features/Drivers/FeatureEntryDisplayDriver.cs:20`; the category `Tenants/Manifest.cs:35-39`.
- If the menu markup makes the two clicks unreliable, move the cursor over **Tools** and use
  `scene.NavigateAsync("/Admin/Features")`. The page is the same.

**`enable-2`**

- Actions: `scene.HighlightAsync` on the ID next to the feature name (the `code` element inside
  `[data-filter-value*="Tenant Feature Profiles"]`), with the caption `Feature ID`.
- At the end of the narration: the ring is around `OrchardCore.Tenants.FeatureProfiles`, and the name **Tenant Feature
  Profiles** is beside it.
- Labels: the name followed by the ID `Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`.

**`enable-3`**

- Actions: `scene.ClearHighlightsAsync()`. Click the **Enable** button of the row (`a[id^="btn-enable-"]`,
  `Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`) with `ClickAndWaitForNavigationAsync`. Wait off camera
  (`scene.OffCameraAsync`) until the text `The feature Tenant Feature Profiles was enabled.` is visible, because the
  site restarts the tenant. Then click **Multi-Tenancy** in the admin menu to expand it and highlight **Feature
  Profiles** under it.
- At the end of the narration: the **Features** page with the success notice `The feature Tenant Feature Profiles was
  enabled.`, and the admin menu expanded to **Tenants** and **Feature Profiles** under **Multi-Tenancy** (the menu sets
  their order, which is not checked here), with a ring around **Feature Profiles**.
- Labels: the notice `Features/Controllers/AdminController.cs:317`; **Multi-Tenancy** and **Feature Profiles**
  `Tenants/FeatureProfilesAdminMenu.cs:31-33`. The text of **Multi-Tenancy** is shown with a building icon in front of
  it (`Tenants/Views/NavigationItemText-multitenancy.Id.cshtml`). The permission **Manage tenant feature profiles** is
  `Tenants/Permissions.cs:9`; the Administrator role has it by default (`Tenants/Permissions.cs:21-28`); it is
  not shown on screen.

### `04-create` (chapter 4, "Create a feature profile")

Starts on `/Admin` with the admin menu visible and **Multi-Tenancy** collapsed.

**`create-1`**

- Actions: click **Multi-Tenancy** in the admin menu, click **Feature Profiles** under it
  (`ClickAndWaitForNavigationAsync`; the address is `/Admin/TenantFeatureProfiles`), highlight the empty-list message
  for a moment, clear the highlight, then click **Add Feature Profile** (`ClickAndWaitForNavigationAsync`).
- At the end of the narration: the **Create Feature Profile** page, with a breadcrumb back to **Feature Profiles**,
  an empty **Name** field and the **Rules** editor below it.
- Labels: menu `Tenants/FeatureProfilesAdminMenu.cs:31-33`; list page title **Feature Profiles**
  `Tenants/Views/FeatureProfiles/Index.cshtml:6`; empty message **Nothing here! There are no feature profiles for the
  moment.** `Index.cshtml:34`; **Add Feature Profile** `Index.cshtml:25`; **Create Feature Profile**
  `Tenants/Views/FeatureProfiles/Create.cshtml:4`.

**`create-2`**

- Actions: `scene.TypeAsync(scene.Page.Locator("input[name='Name']"), "Basic sites")`.
- At the end of the narration: **Name** shows `Basic sites`, with the hint **The name of the feature profile.** under
  it.
- Labels: **Name** and its hint `Tenants/Views/FeatureProfiles/EditorTemplates/FeatureProfileViewModel.cshtml:12,16`.

**`create-3`**

- Actions: highlight the editor (`.monaco-json-model-editor-container`), then scroll down just enough to show the hint
  under the editor (clear the highlights before scrolling with `ScrollAsync`), and highlight the hint (the
  `span.hint` inside `.monaco-json-model-editor`; the page has a second `span.hint`, the one under **Name**, which is
  not this one).
- At the end of the narration: the **Rules** label, the editor (empty, unless the page fills it: see
  `OPEN-QUESTIONS.md`) and the hint **The feature rules. Here is an example if a rule that would exclude
  'OrchardCore.Flows' feature.** with its example below it, with a ring around the hint.
- Labels: **Rules** and the hint `FeatureProfileViewModel.cshtml:22,30`.

**`create-4`**

- Actions: `scene.ClearHighlightsAsync()`, scroll back so the whole editor is visible, then put these three rules in
  the editor with the method below, so that the editor shows exactly this:

  ```json
  [
    {
      "Rule": "Exclude",
      "Expression": "OrchardCore.Workflows"
    },
    {
      "Rule": "Exclude",
      "Expression": "OrchardCore.Email.*"
    },
    {
      "Rule": "Include",
      "Expression": "OrchardCore.Email.Smtp"
    }
  ]
  ```

  Method (the editor is Monaco, which closes brackets and quotes and indents by itself; the first method that works
  is the one to use):
  1. Run `Recorder probe /Admin/TenantFeatureProfiles/Create "() => typeof window.monaco"` once, before the take. If
     it prints `object`, set the text with `scene.Page.EvaluateAsync` in this step: replace the value of the first
     model of `window.monaco.editor.getModels()` with the text above, as one JSON string, then click the editor so it
     takes focus.
  2. Otherwise click the editor and type the compact text with `scene.TypeAsync` at the default delay:
     `[{"Rule":"Exclude","Expression":"OrchardCore.Workflows"},{"Rule":"Exclude","Expression":"OrchardCore.Email.*"},{"Rule":"Include","Expression":"OrchardCore.Email.Smtp"}]`
     then press `Shift+Alt+F` (Format Document) so the editor shows the layout above.
  3. Either way, check before moving on that no bracket or quote is doubled, and that
     `() => document.querySelector('.monaco-json-model-editor-value').value` parses to the three rules above. If it
     does not, restore the snapshot and take the step again.
- At the end of the narration: the editor holds the three rules, in the layout above, with no red underline.
- Labels: the rule names **Exclude** and **Include** are the ones the rules accept (`Core/Shell/ServiceCollectionExtensions.cs:73,83`); the feature names are **Workflows** `Modules/OrchardCore.Workflows/Manifest.cs`, **SMTP Email
  Provider** `Modules/OrchardCore.Email.Smtp/Manifest.cs` and **Azure Communication Services Email**
  `Modules/OrchardCore.Email.Azure/Manifest.cs`.

**`create-5`**

- Actions: scroll to the bottom of the form, click **Save** (the main button, not the arrow beside it;
  `button.save[value='Save']`, `ClickAndWaitForNavigationAsync`).
- At the end of the narration: the **Feature Profiles** list with one row, `Basic sites`, with a blue **Edit** button
  and a red **Delete** button on its right, and the **Search** box and **Add Feature Profile** button above it.
- Labels: **Save** `FeatureProfileViewModel.cshtml:53`; **Edit** and **Delete**
  `Tenants/Views/FeatureProfileEntry.Buttons.SummaryAdmin.cshtml:6,8`; the search matches the name
  `Tenants/Controllers/FeatureProfilesController.cs:77-78`.

**`create-6`**

- Actions: click **Edit** on the `Basic sites` row (`ClickAndWaitForNavigationAsync`), wait until the editor shows the
  rules, then click **Cancel** (`ClickAndWaitForNavigationAsync`).
- At the end of the narration: back on the **Feature Profiles** list, with the `Basic sites` row. (In the middle of the
  step the **Edit Feature Profile** page showed `Basic sites` and the three rules.)
- Labels: **Edit Feature Profile** `Tenants/Views/FeatureProfiles/Edit.cshtml:4`; **Cancel**
  `FeatureProfileViewModel.cshtml:62`.

### `05-assign` (chapter 5, "Assign a profile to a tenant")

Starts on `/Admin/Tenants`: the **Tenants** page, in the List layout, with three rows (Default, Bluebird, Maplewood).
The Bluebird row carries no profile badge.

**`assign-1`**

- Actions: highlight the Bluebird row (`.list-group-item` with the text `Bluebird`), clear the highlight, then click
  **Edit** in that row (`a.btn.edit`, `ClickAndWaitForNavigationAsync`).
- At the end of the narration: the **Edit Tenant** page of Bluebird, with the **Name** field (read-only) showing
  `Bluebird`.
- Labels: **Tenants** `Tenants/Views/Admin/Index.cshtml:9`; **Edit** `Tenants/Views/ManageTenantActionButtons.cshtml:9`;
  **Edit Tenant** `Tenants/Views/Admin/Edit.cshtml:5`.

**`assign-2`**

- Actions: scroll to the **Feature profile** field (`select[name='FeatureProfiles']`), highlight it, choose the option
  `Basic sites` with `scene.SelectAsync` (one option only), clear the highlight, scroll to the buttons and click
  **Save** (`button.save`, `ClickAndWaitForNavigationAsync`).
- At the end of the narration: the **Tenants** page again (Save returns to it).
- Labels: **Feature profile** and its hint **Optionally specify which feature profile should be applied to this
  tenant.** `Tenants/Views/Shared/_TenantFeatureProfile.cshtml:6,9`; the field is hidden when no profile exists
  `_TenantFeatureProfile.cshtml:3`; the same partial is on the **Create Tenant** page (the page behind the **Add Tenant** button of the
  Tenants page) `Tenants/Views/Admin/Create.cshtml:6,26`; **Save** `Tenants/Views/Admin/Edit.cshtml:37`.

**`assign-3`**

- Actions: highlight the badge in the Bluebird row (`.badge[title='Feature Profile']`, the layer icon followed by
  `Basic sites`), then highlight the Maplewood row (`.list-group-item` with the text `Maplewood`).
- At the end of the narration: the Bluebird row shows the badge `Basic sites` with the **Running** badge, the Maplewood
  row shows no profile badge, and both highlights are visible.
- Labels: the badge `Tenants/Views/ProfileFeatureTags.cshtml:24`.

### `06-effect` (chapter 6, "Check the result")

Starts on `/Admin/Features` (the Features page of the Default tenant, search box empty).

**`effect-1`**

- Actions: `scene.TypeAsync(scene.Page.Locator("#search-box"), "OrchardCore.Workflows")`.
- At the end of the narration: four features under the heading **Workflows**, each with its ID: **HTTP Workflows
  Activities**, **Session Workflows Activities**, **Timer Workflows Activities** and **Workflows**, each with an
  **Enable** button.
- Labels: the names `Modules/OrchardCore.Workflows/Manifest.cs`.

**`effect-2`**

- Actions: `scene.NavigateAsync("/Admin/Tenants")`, highlight the **Features** button in the Bluebird row (inside the
  `.list-group-item` with the text `Bluebird`: `a.btn` with the text `Features`), clear the highlight, click it
  (`ClickAndWaitForNavigationAsync`; the address is `/Admin/Features/Bluebird`), then type `OrchardCore.Workflows` in
  `#search-box`.
- At the end of the narration: the **Features** page of Bluebird, `OrchardCore.Workflows` in the search box and the
  message **Nothing here! Your search or filters returned no results.**
- Labels: the **Features** button `Tenants/Views/ManageFeaturesActionButton.cshtml:6` (shown only for a running tenant
  that is not the Default tenant, and only while the Features feature is enabled on the Default tenant); the message
  `Features/Views/Admin/Features.cshtml:105`; the page title **Features** `Features.cshtml:15`.

**`effect-3`**

- Actions: click `#search-box`, press `Escape` (it clears the box: the `Escape` handler of `Features/Assets/ts/features-index.ts`), then type
  `OrchardCore.Email`.
- At the end of the narration: two features under the heading **Communication**: **Email** (`OrchardCore.Email`) and
  **SMTP Email Provider** (`OrchardCore.Email.Smtp`). **Azure Communication Services Email** is not listed.
- Labels: the names `Modules/OrchardCore.Email/Manifest.cs`, `Modules/OrchardCore.Email.Smtp/Manifest.cs`,
  `Modules/OrchardCore.Email.Azure/Manifest.cs`.

**`effect-4`**

- Actions: `scene.NavigateAsync("/Admin/Tenants")`, click **Features** in the Maplewood row (same locator as in
  `effect-2`; the address is `/Admin/Features/Maplewood`), then type `OrchardCore.Workflows` in `#search-box`.
- At the end of the narration: the **Features** page of Maplewood with the four Workflows features listed, as on the
  Default tenant.

## Slides

The slides come from `slides.py`. Each one holds the narration of its step for the same length of time.

- `why-1`: "Without feature profiles", two cards: the same features for every tenant, enabled by the tenant's administrator.
- `why-2`: "With feature profiles", four cards, the first two bright: a named list of rules, assigned to a tenant.
- `why-3`: the same four cards, the last two bright: shared by tenants, optional.
- `rules-1`: "Each rule has two parts", three cards: Rule, Expression, Wildcards.
- `rules-2`: "The last matching rule decides", four cards: the two rules in order, the reverse order, no match.
- `rules-3`: "Dependencies follow the exclusion", three cards: the excluded Workflows feature, the features that depend on it, every other feature.

## Checks for private data

Before the take, after the take (look at the contact sheet of every clip) and again on the finished video:

- The only email address on screen is `admin@example.com` (user list and profile menus included). The only user is
  `admin`.
- The tenants are `Default`, `Bluebird` and `Maplewood`, the sites are `Orchard Core`, `Bluebird Bakery` and
  `Maplewood Garden Center`. No real company, person, domain or path of a machine is visible.
- The password is never typed on camera and is not in any file of the repository.
- The address bar is not part of the recording; no connection string, secret or token appears on the pages of this
  video.
- The profile is `Basic sites`, with the three rules above and nothing else; no leftover profile or tenant from an
  earlier take is listed. If one is, restore the snapshot and record again.
