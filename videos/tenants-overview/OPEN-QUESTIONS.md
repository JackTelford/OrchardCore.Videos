# Open questions: Tenants: Run Many Sites From One Application

The narration states only what the source and the documentation show. The points below were read from the code but
not seen on a running site. A person with a running site confirms each one before recording; none of them changes a
sentence of the narration unless the answer is "no".

## The site and the seed data

1. **Auto Setup creates the extra tenants.** Read: `AutoSetupMiddleware.cs` installs the Default tenant on the first
   request, then calls `CreateTenantSettingsAsync` for every other entry (uninitialized, with its prefix, recipe
   and database), and `AutoSetupMiddleware` installs each one on the first request to its prefix. Not run. If Bakery
   and Studio do not appear, create them with Add Tenant (Name, Url Prefix `bakery` / `studio`, Recipe `Blog` /
   `Agency`, Create & Setup) and finish the setup screen with site name, user `admin`, `admin@example.com` and
   `<password>`, the Sqlite database.
2. **Badges of the Default tenant.** `SetupService` does not store a recipe name, and `AutoSetupService` stores one
   only for the extra entries, so the Default row should show the Sqlite badge and no recipe badge. Confirm. The
   narration only says rows have badges "for its category, database provider, recipe, and state".
3. **Editing Studio.** Clearing the Url Prefix and setting the Hostname `studio.example.com` on a running tenant is
   allowed by `AdminController.Edit` and the validator. Confirm the row then shows the host name under the description
   (`ShellSettingsEntry.Description.SummaryAdmin.cshtml`) and that Disable works on it.

## `03-enable`

4. **Search on the Features page.** `features-index.ts` filters the rows by name, id and description. Confirm that
   typing `tenants` leaves the Tenants row with an Enable button on screen, and that the row's button id is
   `btn-enable-OrchardCore_Tenants` (`GenerateIdFromName` of the MVC html helper turns the dot into an underscore).
   The search text also matches the description of a feature, and the description of the Auto Setup module contains
   the word tenants (`OrchardCore.AutoSetup/Manifest.cs:8`), so an Auto Setup row probably appears among the results,
   without an Enable button (`AdminController.CreateEntry` excludes it). Whether that row is listed on the recorded
   site depends on the runtime feature list: confirm it. The narration says only that the features of the module are
   among the results.
5. **The message after Enable.** The code writes "The feature Tenants was enabled." as a toast
   (`Message.cshtml` of TheAdmin). Confirm its text, that it is on screen after the redirect, and for how long it
   stays (the view hides it automatically only when the notifier sets a delay). `enable-3` assumes it is still
   visible when the Multi-Tenancy group is expanded.
6. **The menu after Enable.** Confirm that the first page after the redirect already has the Multi-Tenancy group
   (`AdminMenu.cs` adds it for the Default tenant only), with Tenants as its item.

## `04-list`

7. **The Actions menu of the bulk selection.** The toolbar (`AdminListToolbar.cshtml`) renders the Actions button hidden
   (`d-none`); the Tenants list starts the shared `bulk-select-list` component, whose source is not in this tree. The
   older `list-management.js` shows the button once the selection reaches a minimum, and hides the item count and the
   filters while it is shown; the release notes say the same for the Background Tasks list. Confirm that ticking
   the Bakery and Studio boxes shows the Actions button, and what the toolbar then looks like. The narration says only
   that the Actions menu offers Disable and Enable for the selected tenants (`TenantsBulkAction`,
   `AdminController.Index(BulkActionViewModel)`): the controller skips the Default tenant on Disable and warns for a
   tenant that is already in the target state, and a "Bulk Action" confirmation precedes the change, so the narration
   does not claim that the choice is applied to every selected tenant.
8. **The Category, State and Sort menus.** They are bootstrap-select pickers (the native select is hidden). Their
   header texts are "Filter by category", "Filter by state" and "Sort by", and their button titles Category, State and
   Sort (`TenantsFilters.cshtml`). Whether choosing a value submits the filter by itself depends on the shared script;
   `tenants-admin-list.ts` does not do it. The clip therefore never opens a picker and filters with the State badge, a
   plain link to `?Options.Status=Disabled`. Confirm the pickers' button locators for the rings of `list-2`.
9. **Search.** The controller matches the search text against the tenant name, the host name and the URL prefix only,
   not the description or the category (`AdminController.Index`). The narration says exactly that.

## `05-actions`

10. **The View tab.** The View button has `target="_blank"`. Confirm that the recorder can follow the new tab with
    `SwitchToAsync`, and that the Bakery site (Blog recipe, outside calls blocked, so its web fonts do not load) finishes
    loading in a few seconds, shows "Corner Bakery", and shows none of the sample text the plan does not list.
11. **The Features button.** `ManageFeaturesActionButton.cshtml` shows it for a Running tenant other than Default
    (class `enable`, like the Enable button), and `OrchardCore.Features` `AdminController.ExecuteAsync` applies the
    Features page to that tenant. The clip does not click it: the page computes the title "Features for {0} tenant"
    but its breadcrumb always reads Features, so the page may not show whose features it lists. Confirm before
    recording a longer clip.
12. **Remove is hidden.** `ManageTenantActionButtons.cshtml` shows Remove only when tenant removal is allowed in the
    configuration (default: off) and the tenant is Uninitialized or Disabled. Confirm that no Remove button appears
    on the recorded site.

## `06-add`

13. **The database section.** Without a preset database provider in the root configuration the form shows the
    "Database Presets" heading and the "What type of database to use?" list; with `None` selected, `tenant.js` hides the
    table prefix, the connection string and the schema. Confirm the form fits the screen, or scroll as the plan says.
14. **Feature profile.** The "Feature profile" field appears only when at least one feature profile exists
    (`_TenantFeatureProfile.cshtml`). The recorded site has none, so the narration does not mention the field.

## What the slides say that depends on the site

15. **`idea-3` slide.** The request `example.com` reaching Default assumes the Default tenant has no host name and no
    prefix, which is the case on the recorded site. `RunningShellTable.Match` falls back to the Default tenant only
    when it is a catch-all. The narration does not claim the fallback.

## Pronunciation

16. **"Url Prefix".** The storyboard has a `spoken` entry that says "Url Prefix" as "URL Prefix", because the label is
    written in mixed case on screen and a voice may read "Url" as a word. A person confirms it with `pronounce.py`
    before the narration is generated.
17. **"Create & Setup".** The label of the button is written with an ampersand (`Views/Admin/Create.cshtml:30`) and
    `add-3` reads it. A voice normally says "and"; confirm with `pronounce.py` that it does, and add a `spoken` entry
    ("Create & Setup" as "Create and Setup") if it does not.
