# Open questions: Tenants: Create a Tenant and Set It Up

Everything the narration says was read in the Orchard Core source (commit 92db4a83) or in `src/docs`. What follows could not be
checked without a running site, or is a choice a person makes. Confirm each item while preparing the recording; none of them changes
a sentence of the narration unless the answer is "no".

## Pronunciation (check with `scripts/pronounce.py`, do not invent a spelling)

- "Url Prefix": does the voice say "U.R.L." or a word ("earl")? The label is written "Url Prefix" on screen.
- "Sqlite", "MySql", "Postgres", "Sql Server": the labels of the database list. Does the voice read "Sqlite" as "sequel-ite" or letter by letter?
- "Uninitialized", "Maplewood", "Riverside": expected to be read correctly.
- "Create & Setup" is written with an ampersand; `storyboard.json` maps it to "Create and Setup" for the voice. The captions keep the
  ampersand, as the button is labeled.

## Checks on the running site

1. **Admin menu.** `AdminMenu.cs:30-37` builds "Multi-Tenancy" without an action, with the child "Tenants". The admin theme draws such an item as a
   button that toggles its children (`NavigationItemLink-admin.cshtml:24,29,76-86`) and renders nothing expanded at load
   (`aria-expanded="false"`). `create-1` clicks that button, then the link "Tenants". Confirm on screen that the group starts collapsed on the
   first visit of the recording browser, and that the button, not a separate chevron, is what opens it.
2. **Both fields empty.** `create-5` says a tenant needs a prefix, a hostname, or both, because the Default tenant has neither. Outside the
   capture, press Create with only a Name and confirm the message "A tenant with the same host and prefix already exists." appears under
   Url Prefix (`TenantValidator.cs:84-89`), and confirm the Default row shows no host or prefix.
3. **The Recipe list of the Create Tenant form.** The form lists every recipe with `issetuprecipe` (`AdminController.cs:332`), while the Setup
   screen hides the ones tagged `hidden` (`SetupController.cs:56-60`). `OrchardCore.Cms.Web/Recipes/openapi-generation-setup.recipe.json` is
   tagged `hidden`. Check whether the Create Tenant list of the recorded site shows a recipe such as "OpenApi Client Generation" that the
   Setup screen would not offer. The video selects "Blog" and names no other recipe. If the list shows a hidden recipe, decide whether the
   source should filter it (a possible issue for the Orchard Core repository, not for the documentation, which says nothing about it).
4. **Where the browser goes for `/maplewood/Admin`** (`open-3`). The narration says to go to the Admin address and sign in. Confirm that
   the anonymous request lands on the sign-in page of Maplewood (heading "Log in") and that the session of the Default tenant in
   `state.json` is not accepted by Maplewood. If the administration opens, the cookie of Default is being accepted: record the clip in a
   fresh browser context without `state.json`.
5. **Time zone** (`setup-1` to `setup-6`, `shortcut-2`). The Setup screen opens on the time zone of the server (`SetupController.cs:73`, `Clock.cs`
   `GetSystemTimeZone`), which would show the time zone of the recording machine in the first row. The plan runs the site process in
   `America/Los_Angeles`: with `TZ` on Linux and macOS, with the time zone of the machine on Windows. Not verified: that NodaTime's system default
   follows `TZ` in the .NET runtime of the recording machine, and that changing the Windows time zone is acceptable there. Confirm that the first frame
   of `setup-1` shows `America/Los_Angeles`. Also confirm that `Europe/Amsterdam` (selected in `setup-3`) is in the list, and that the option
   text is "(GMT<offset>) <time zone id> (<offset>)" (`TimeZone.cs`) with the option selected by value.
6. **Length of Setup.** Measure how long Finish Setup takes with the Blog recipe on Sqlite. `setup-6` sets the default timeout of the page to 180 seconds and
   runs the click and its navigation off camera; adjust the timeout if the machine is slower.
7. **Home page of Maplewood.** The narration says the home page is a blog with a sample post and that the site name appears at the top. The
   source says so (`blog.recipe.json:965-1008`, `Layout.liquid:27`); confirm on screen that the name is visible at 1536 x 864.
8. **The buttons of a running row** (`open-1`). The narration names View, Disable, Reload and Features. The Features button is added by
   `TenantFeatureShapeTableProvider` when the Features feature is enabled, and that feature is always enabled (`OrchardCore.Features/Manifest.cs`),
   so no recipe is needed. Confirm the five buttons (with Edit) are on one line in the Buttons layout at the recording width, and that none is
   hidden in a drop-down.
9. **Password pop-over** (`setup-4`). Confirm the pop-over on the Password field lists the rules only, and shows no typed value.
10. **Initializing and Service Unavailable** (`setup-5`). The narration says that while Setup runs the tenant is Initializing and answers
    requests with Service Unavailable. Read in `SetupService.cs:132` (the comment says so) and `ModularTenantContainerMiddleware.cs:37-42` (the 503
    response). Not verified: that the settings object changed by `AsInitializing()` is the one the running shell table matches. While a take of
    `04-setup` runs, request `http://localhost:5010/maplewood` from another browser and confirm the 503 response. It is not on camera.

## Choices for a person

- The video uses the SaaS recipe for the Default tenant because it enables the Tenants feature. The alternative, the Blog recipe plus
  enabling "Tenants" in the prepare step, gives a Default tenant whose home page is a blog; nothing in the narration depends on it.
- The second tenant (Riverside) stays Uninitialized at the end of the recording. If a retake of `06-shortcut` is needed, restore snapshot C:
  the name Riverside can't be created twice.
