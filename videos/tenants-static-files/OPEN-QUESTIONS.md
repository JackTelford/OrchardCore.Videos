# Open questions: Static Files per Tenant

What was checked in the Orchard Core source (commit `92db4a83`) is written in `recording-plan.md` as file and line
references. The points below were not verified there, or could only be read from the code. None of them is stated in
the narration as a fact beyond what the source shows; a person must confirm them on a running site before the
recording, and say in the pull request what was found.

## Behaviour to confirm on a running site

1. **When the folder appears.** `Startup.cs:66-78` creates `wwwroot` when the `ITenantFileProvider` singleton is first
   resolved, which `Configure` does (`Startup.cs:88`) when the tenant's pipeline is built, on the first request to the
   tenant after the feature is enabled. The documentation says "once enabled on a tenant, a folder wwwroot is created".
   The narration only says that the feature creates the folder when it is missing. Remove `<app-data>/Sites/Maple/wwwroot`,
   enable the feature on Maple from the Default tenant, and check whether the folder exists straight after the redirect or
   only after the first request to `/bakery`.
2. **A new file needs no reload.** The tenant's provider is a physical file provider that reads the disk on each
   request, so a file added afterwards should be served at once. The narration does not say it. Add a file while
   the tenant runs and request it.
3. **Cache header.** `Startup.cs:97-99` sets `Cache-Control` to `public, max-age=...` for 30 days and `s-max-age=...` for
   365.25 days. `s-max-age` is not the standard directive name (`s-maxage`, which the application's own static files
   use in `ServiceCollectionExtensions.cs:286`), so a shared cache may ignore it. The narration says only "for 30 days".
   Read the header of `/bakery/robots.txt` in the browser's network tool and confirm its value.
4. **Version hash in the URL.** `ShellFileVersionProvider` (`OrchardCore.Mvc.Core/ShellFileVersionProvider.cs:25-35`)
   looks in every `IStaticFileProvider` of the tenant, and `Mvc.Core/Startup.cs:150` says so in a comment. Confirm that
   `<img src="~/images/logo.png" asp-append-version="true" />` renders `/bakery/images/logo.png?v=...` when
   `images/logo.png` exists in the tenant folder, and no `v` when it does not. The `code-3` slide shows this output.
5. **The SEO module.** `src/docs/reference/modules/Seo/README.md:26` says a `robots.txt` in the tenant folder takes
   precedence over the robots settings, and `RobotsSettingsDisplayDriver.cs:51-56` shows the warning "A physical
   robots.txt file is found for the current site. Until removed, the settings below will have no effect." On Maple,
   enable the SEO feature (the Blog recipe does not), open Settings, Search, Search Engine Optimization, Robots
   (`OrchardCore.Seo/AdminMenu.cs:27-33`), and check the warning, and that `/bakery/robots.txt` still shows the file.
   This works only if `IStaticFileProvider` resolves to the tenant provider: it is registered after the application's own
   (`Startup.cs:80-83`), but that order was read, not run.
6. **The application's wwwroot first.** `OrchardCore.Cms.Web/Program.cs:26-28` calls `app.UseStaticFiles()` before
   `app.UseOrchardCore()`, so a file in the application's `wwwroot` folder is answered before the tenant pipeline runs, for
   the same request path. A host `wwwroot/robots.txt` therefore hides the `/robots.txt` of the Default tenant and of a
   tenant on its own host name, but not `/bakery/robots.txt`, which is another path. Inside a tenant's pipeline the
   application's web root is also reachable, but only under the application's module prefix
   (`ApplicationStaticFileProvider.cs:15-34`, registered at `ServiceCollectionExtensions.cs:247,269`), never at the
   plain path of a file. The narration therefore limits the claim to the Orchard Core CMS web project and to the same
   request path. The documentation says "content root folder". Put `wwwroot/robots.txt` in the host and `Sites/Default/wwwroot/robots.txt`
   in the Default tenant (with the feature enabled), and check which one `/robots.txt` returns. A host with another
   `Program.cs` may differ.
7. **Unknown file types.** `ServeUnknownFileTypes = true` with `application/octet-stream` as the default
   (`Startup.cs:93-94`). Request a file with an extension the framework does not know, such as `notes.abc`, and check the
   `Content-Type`.
8. **Removing a tenant.** `ShellSiteFolderRemovingHandler.cs:32-39` deletes `<app-data>/Sites/<Tenant>` recursively. Removal
   is allowed only when `Tenants:TenantRemovalAllowed` is true (`TenantsOptions`); confirm that the `wwwroot` folder is gone
   after a removal. The narration does not say when removal is allowed.

## Recording details to confirm

9. **The admin menu of the Default tenant.** `AdminMenu.cs:30-33` names the items **Multi-Tenancy** and **Tenants**; the
   selectors `li.menu-multitenancy` and the link under it were not tried. Use `Recorder probe` on `/Admin` to adjust them,
   or fall back to `NavigateAsync("/Admin/Tenants")`.
10. **The Features button and the Enable button.** The id `btn-enable-OrchardCore_Tenants_FileProvider` assumes that
    `Html.GenerateIdFromName` turns the dots into underscores; check it, and check that `a[href*="/Admin/Features/Maple"]`
    matches only the Maple row. The Tenants list shows buttons only with the default **List layout** and actions layout
    (`src/docs/reference/modules/Admin/README.md`, "Admin List Layouts"); with the actions menu the Features entry is in the menu.
11. **No tenant name on the Features page.** `Features.cshtml:5-9` computes the heading "Features for {0} tenant" but the page
    renders only the breadcrumb **Features** (`Features.cshtml:15`). The plan names the tenant with a caption on the search box.
12. **Auto Setup with a URL prefix.** The documentation shows `RequestUrlPrefix` for a tenant of Auto Setup; confirm that
    `/bakery` and `/garden` install the tenants with the Blog recipe, and that both show **Running** on the Tenants list.
13. **The zoom of a text file.** The init script of `04-serve` sets `document.body.style.zoom`. Check on the contact sheet that
    the ring and its caption fit the enlarged text; the plan gives two fallbacks.

## Pronunciation to confirm

The `spoken` entries of `storyboard.json` are new spellings, to check with `scripts/pronounce.py` before narrating:
`robots.txt` as "robots dot text", `/bakery/robots.txt` as "slash bakery slash robots dot text" (and the same for
`/garden/robots.txt`), `wwwroot` as "W.W.W. root", `App_Data` as "App Data", `favicon` as "fav icon", `~/` as
"tilde slash", `asp-append-version` as "A.S.P. append version", `application/octet-stream` as "application slash
octet-stream", `ITenantFileProvider` as "I Tenant File Provider", `IStaticFileProvider` as "I Static File Provider",
and `UseStaticFiles` as "Use Static Files".
