# Open questions

What the author could not confirm from the source or the documentation alone. Each item says what was checked and what
a person must confirm on a running site. The narration states nothing in this list as a fact beyond what is written
under "What the narration says". Source paths are relative to `src/OrchardCore.Modules/` of `OrchardCMS/OrchardCore`
unless they start with `src/`.

## Pronunciation (confirm with `scripts/pronounce.py` before narrating)

- `robots.txt` is spoken as "robots dot T.X.T." through the storyboard's `spoken` map. This spelling is a guess.
  The map replaces text as plain strings, so a sentence that ends with the word would end with two periods; the
  narration does not end a sentence with it.
- Check how the voice reads "SEO" (expected: letter by letter), "Base url" and "Sitemap".

## The Features page (`02-enable`)

- Checked: the message of the `Enable` action is "The feature {1} was enabled." with the name of the requested
  feature only (`OrchardCore.Features/Controllers/AdminController.cs:247` and `:317`,
  `src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:80`), so "The feature SEO was enabled." is
  expected whatever else is enabled. Confirm the rendered text. The narration says only that a message confirms that
  the SEO feature was enabled.
- The selectors in `recording-plan.md` (`figcaption#tools button`, `ul[data-title='Tools']`,
  `a[href*='/Features/OrchardCore.Seo/Enable']`) come from `NavigationItem-admin.cshtml`,
  `FeatureEntry.Buttons.SummaryAdmin.cshtml` and the route `Features/{id}/Enable/{tenant?}`, not from a running page.
  Probe them. The Enable link is posted by script (`data-url-af="UnsafeUrl"`); confirm `ClickAndWaitForNavigationAsync` follows it.
- Confirm that typing SEO in the search box leaves the SEO row visible and the Enable button reachable (the filter
  matches the name, the id and the description of each feature: `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs`).

## The Robots page (`04-settings`)

- Checked: labels, hints and defaults from `RobotsSettings.Edit.cshtml` and
  `src/OrchardCore/OrchardCore.Seo.Abstractions/RobotsSettings.cs` (both boxes checked, Additional rules empty).
  Confirm the rendered page: the breadcrumb says "Settings" (`Settings/Views/Admin/Index.cshtml`),
  and no other heading is expected.
- Confirm that the success message appears after Save as "Site settings updated successfully."
  (`Settings/Controllers/AdminController.cs:119`) and that the text area keeps the typed rule.
- The zoom of the plain-text page (`document.documentElement.style.zoom = '1.6'`) is a suggestion. Confirm that it works
  with the recorder's cursor and highlight layers, and that the text is readable in the 1080p frame.

## Who can open the page (not in the narration)

- The menu item and the settings editor check the permission "Manage SEO related settings"
  (`src/OrchardCore/OrchardCore.Seo.Abstractions/SeoConstants.cs:12`), which the Administrator role has by default. The
  Settings controller also checks `ManageGroupSettings` for the group `robotsSettings` (`Settings/Controllers/AdminController.cs:53`),
  and the SEO module does not register that group with `AddSiteSettingsPermission`. A role other than Administrator may
  therefore need "Manage settings" as well. Confirm with a test role. The narration says nothing about permissions.

## Sitemaps (slides `sitemaps-1`, `sitemaps-2`)

- Checked: `Sitemaps/Services/SitemapsRobotsProvider.cs` returns nothing when the site has no Base url (`:25-30`) or when
  `SitemapsRobotsSettings` has never been stored or the box is cleared (`:32-35`). It lists enabled sitemap indexes
  first, then the enabled sitemaps that no enabled index contains (`:47-82`). The Sitemaps checkbox and provider are
  registered only when both Sitemaps and SEO are enabled (`Sitemaps/Startup.cs:135-143`). The narration says
  exactly this and nothing more. Confirm on a site with Sitemaps enabled, a Base url, one index containing one sitemap
  and one sitemap outside it: no Sitemap line before the first save of the Robots page, two lines after, and none
  when Include all sitemaps is cleared.
- The order of lines in the file when more than one provider contributes (SEO and Sitemaps) is not narrated:
  `RobotsMiddleware.cs:43` walks the providers in reverse registration order, and the result was not observed.

## A robots.txt file among the static files (slide `file-1`)

- Checked: `RobotsMiddleware.cs:30-39` leaves an existing file to the static file providers, and
  `RobotsSettingsDisplayDriver.cs:51-56` shows the warning "A physical robots.txt file is found for the current site.
  Until removed, the settings below will have no effect." when `IStaticFileProvider` reports the file. A file in the
  web project's `wwwroot` is served by `app.UseStaticFiles()` in `src/OrchardCore.Cms.Web/Program.cs:26`, and a file in a
  tenant's `wwwroot` folder is served by the Static File Provider feature (`Tenants/Startup.cs:56-106`), both before the
  generated file is reached.
- Not confirmed: when the Robots page shows the warning. Both the middleware and the page ask for a single
  `IStaticFileProvider`, so the container gives them the one registered last. The module and application provider
  answers only for paths that start with a module or application name
  (`src/OrchardCore/OrchardCore.Abstractions/Modules/ApplicationStaticFileProvider.cs`), the Media feature also
  registers a provider (`Media/Startup.cs:115`) and the Blog recipe enables Media. The narration therefore says only that
  the page "can" show the warning. To confirm, on a site with SEO enabled, check what `/robots.txt` returns and whether the
  warning shows for each of three placements of a `robots.txt` file: in the web project's `wwwroot`; in the tenant's
  `wwwroot` folder with Static File Provider enabled; in the Media library root.

## What the narration says that rests on the source only

- The file is built from the settings: `SiteSettingsRobotsProvider.cs:21-43` (the first box adds the line shown in its
  hint, the second adds the line for the admin path, Additional rules follows, a cleared box adds nothing).
- "The file is public, so anyone can read the rules you add": the route `/robots.txt` has no authorization
  (`OrchardCore.Seo/Startup.cs:47-57`).
