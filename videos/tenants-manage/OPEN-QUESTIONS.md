# Open questions: Tenants: Edit, Disable, Reload and Remove

What the author could not confirm without a running site. The narration says only what the code shows; each item below
says what was read, and what a person must check on the site while recording. File paths are under `src/` of
`OrchardCore` at commit `92db4a83`.

## 1. What a visitor sees at the address of a disabled tenant

- **Narration:** `disable-2` says the tenant "stops serving its site, while its settings and data stay as they are".
- **Read:** a disabled tenant is not added to the running shell table (`OrchardCore/OrchardCore/Shell/ShellHost.cs`,
  `CanRegisterShell`, lines 518-521), so no tenant matches its prefix. Disabling only changes the state in its
  settings (`AsDisabled`), and nothing deletes its data.
- **Not read:** the page a visitor gets. The Default tenant, which answers every address that no other tenant claims,
  probably answers with its own "not found" page.
- **To confirm:** open `http://localhost:5010/harbor` in another tab while Harbor is Disabled, and check that it no
  longer shows the Harbor site. The video does not show this page. If it shows the site, `disable-2` is wrong.

## 2. The Actions menu needs two or more selected tenants

- **Narration:** `bulk-1` says that with two or more selected, the toolbar shows how many and an Actions menu appears.
- **Read:** the list uses the bulk select list component of the admin, bundled in
  `OrchardCore.Modules/OrchardCore.Tenants/wwwroot/Scripts/tenants-admin-lists/tenants-admin-list.js`: it shows the
  `Actions` menu and the text "N selected" when more than one box is ticked, and hides the filters. Its TypeScript
  source is a package outside this repository, so the bundle was read.
- **To confirm:** tick Juniper alone (no menu), then Maple as well (menu and "2 selected"). The words after the number
  come from the page (`data-selected-text`, "selected").

## 3. The bootstrap-select filters

- **Used by:** `list-4`.
- **Read:** the State filter is a `bootstrap-select` picker titled **State** (`Views/TenantsFilters.cshtml:13`), not a
  native drop-down. It submits the list when its value changes.
- **To confirm:** with `Recorder probe`, the selector of the picker's button and of its **Uninitialized** item. The plan
  has a fallback (`/Admin/Tenants?Options.Status=Uninitialized`), which changes the address bar but not the page.

## 4. The layout of the rows

- **Read:** the defaults are the List layout and the Buttons layout of actions
  (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`), and every action of a tenant is in the
  `Actions` zone, so it is a button.
- **To confirm:** at 1536 by 864 pixels, the five buttons and the badges of a row fit without hiding a button, and no
  ellipsis menu replaces the buttons. If the buttons wrap onto a second line, the rings of `list-2` and `disable-3`
  still work, but check the contact sheet.

## 5. Badges of the rows

- **Narration:** `list-1` says each row shows badges for its state and, when they are set, its database provider and
  recipe.
- **Read:** the database badge shows the `DatabaseProvider` of the tenant's settings, the recipe badge its
  `RecipeName` (`ShellSettingsEntry.Database.SummaryAdmin.cshtml`, `ShellSettingsEntry.Recipe.SummaryAdmin.cshtml`).
  Auto Setup stores both for the tenants it creates (`OrchardCore.AutoSetup/Services/AutoSetupService.cs:70-76`), and
  nothing clears `RecipeName` once the tenant is set up. The Default tenant has no recipe badge in this seed, because
  Auto Setup only stores it for the other tenants.
- **To confirm:** the row of Harbor shows `Sqlite`, `Blog` and **Running**, and the row of Default shows `Sqlite` and
  **Running** without a recipe badge. The sentence of `list-1` is worded for both cases ("when they are set"). If the
  row of Harbor shows no recipe badge, nothing in the narration is wrong, but `list-1` highlights the badges of
  Harbor: look at the contact sheet.

## 6. How the voice says "Url Prefix"

- **Read:** `Url Prefix` is the label of the field (`Views/Shared/_TenantInfo.cshtml:12`). The storyboard speaks it as
  "U.R.L. Prefix" through its `spoken` map, as the storyboard reference spells initialisms; the captions keep the
  written label.
- **To confirm:** a person listens to the sample from `pronounce.py` for "U.R.L. Prefix" and for "Hostname" before the
  first narration is generated; if the voice already says "Url" as the letters, remove the `spoken` entry.

## 7. Removing a tenant set up by Auto Setup

- **Read:** removing a disabled tenant creates a shell with every installed feature, runs the removal handlers of the
  features, then deletes the database tables (the file, for SQLite), the tenant's folder and web root folder, and its
  settings (`OrchardCore/OrchardCore/Shell/Removing/ShellRemovalManager.cs`, and the three handlers next to it, and
  `OrchardCore/OrchardCore.Data.YesSql/Removing/ShellDbTablesRemovingHandler.cs`). A tenant that is still active is
  refused with "The tenant 'X' should be 'Disabled' or 'Uninitialized'.".
- **To confirm:** how long `remove-2` takes on the recording machine, that the success message appears (and not the
  refusal above) after waiting 15 seconds, and that the row is gone. The 15 seconds is an empirical margin: the source only refuses
  while the tenant is in use in an active scope (`ShellRemovalManager.cs:45-50`), and sets no delay. If the refusal
  appears after 15 seconds, wait longer and note how long it took.

## 8. Reload has no visible effect

- **Read:** `Reload` posts to `AdminController.cs:677-704`, which reloads the settings, releases the running tenant and
  redirects to the list; there is no message. The next request builds the tenant again.
- **To confirm:** the redirect lands on `/Admin/Tenants` with no error. Nothing else changes on screen, which is what
  `reload-2` says.

## 9. The Features button

- **Read:** the **Features** button is added to the row of a running tenant other than Default when the Features module
  is enabled (`Startup.cs`, `TenantFeatureProfilesStartup`; `Services/TenantFeatureShapeTableProvider.cs`). The `SaaS`
  recipe enables the Features module.
- **To confirm:** the button is in the rows of Acme, Harbor, Juniper and Maple, and not in the row of Default.

## 10. Auto Setup of five tenants

- **Read:** Auto Setup creates the other tenants when the Default tenant is set up, and sets each one up at its first
  request (`OrchardCore.AutoSetup/AutoSetupMiddleware.cs:129-145`, `Services/AutoSetupService.cs`).
- **To confirm:** each of `/acme`, `/harbor`, `/juniper` and `/maple` ends on a page of that tenant after a redirect,
  and `/Admin/Tenants` then lists Pilot as **Uninitialized** and the four others as **Running**.

## 11. The extra fields of the Edit page of a tenant that is not set up

- **Narration:** `edit-5` says that, while a tenant is not set up, its Edit page also offers the recipe and the
  database settings, and that a tenant that is set up no longer shows them.
- **Read:** the **Recipe** field and the database fields are drawn only when the tenant is uninitialized
  (`AdminController.cs:493-509`, `Views/Admin/Edit.cshtml:24-32`), and the **Recipe** field only when no database
  preset is configured for the whole application (`CanEditDatabasePresets`, `AdminController.cs:508`, `Edit.cshtml:26`).
  The seed sets no such preset, so they should be drawn.
- **To confirm:** the Edit page of Pilot shows **Recipe** (`Blog`), the heading **Database Presets** and **What type of
  database to use?** (`Sqlite`), and the Edit page of Acme shows none of them. If Pilot's page lacks them, `edit-5` is
  wrong: check whether the site was started with a database provider set for the whole application.

## 12. The Invalid and Initializing states

- **Left out of the narration:** `states-2` names Initializing and no longer names Invalid.
- **Read:** `TenantState` has the values `Uninitialized`, `Initializing`, `Running`, `Disabled` and `Invalid`
  (`OrchardCore/OrchardCore.Abstractions/Shell/Models/TenantState.cs`), and the list has a badge for each
  (`Views/ShellSettingsEntry.State.SummaryAdmin.cshtml:11-25`). Only `Initializing` is ever assigned: by the setup
  service while a tenant is being set up (`OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:132`). Nothing
  assigns `Invalid`, and the host does not create a shell for a tenant in that state
  (`OrchardCore/OrchardCore/Shell/ShellHost.cs`, `CanCreateShell`, lines 508-514), so a tenant whose settings say
  `Invalid` is probably not listed at all.
- **To confirm:** nothing needs recording. A person who wants Invalid in a video must first find a way to see that
  badge on a running site; until then the video does not mention it.
- **Initializing** is visible only while a setup runs, so the video shows no row in that state; `states-2` only names it.

## 13. How long the success message of the removal stays

- **Narration:** `remove-2` says that a message says the tenant Maple was removed, and points to the log file.
- **Read:** the message is a success notification with a dismissal of 5000 ms
  (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`), and the toast hides itself
  (`data-bs-autohide="true"`, `OrchardCore.Themes/TheAdmin/Views/Message.cshtml`), so it does not stay until closed.
- **To confirm:** on the take, the toast is drawn while the narration reads the sentence about the message, and the
  contact sheet has a frame with the toast and the list together.

## 14. The example in the hint of the Hostname field

- **Shown in:** `edit-1` and `edit-2`, as the ring on **Hostname** lists its two hints.
- **Read:** the first hint says that if the host is one domain, the tenant site URL is another domain
  (`Views/Shared/_TenantInfo.cshtml:34`). The host and the URL it gives do not name the same domain, so the example
  reads as a mistake of the product text. The narration does not read the hint; `edit-2` says only that several
  domains can be separated by commas, which the second hint says.
- **To confirm:** nothing to record. The hint is on screen during `edit-2` and is not narrated.
