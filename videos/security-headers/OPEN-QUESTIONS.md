# Open questions: security-headers

What was checked in the Orchard Core source and what a person must confirm on a running site. Nothing here was run:
the author read the source (`src/OrchardCore.Modules/OrchardCore.Security`, `OrchardCore.Settings`, `OrchardCore.Features`,
`src/OrchardCore/OrchardCore.Navigation.Core`, `src/OrchardCore/OrchardCore.DisplayManagement`, the Blog recipe) and the
documentation page `src/docs/reference/modules/Security/README.md`. Paths below are relative to the Orchard Core root.

## To confirm on a running site (before or while recording)

1. **The headers a fresh Blog site sends.** The narration (`why-4`) says the site always sends X-Content-Type-Options
   as nosniff and Referrer-Policy as no-referrer, and sends Content-Security-Policy and Permissions-Policy only after a
   value is saved. Read from `Startup.cs:38-41`, `ContentTypeOptionsHeaderPolicyProvider.cs`,
   `ReferrerHeaderPolicyProvider.cs` and the `_policy != null` tests in the two other providers. Run the probe of
   `recording-plan.md` on the defaults snapshot and compare. The module's middleware runs early in the site's pipeline
   (`OrchardCoreConstants.cs:66`), but a file the host serves before the site's pipeline (the host calls
   `UseStaticFiles()` in `Program.cs`) probably carries none of these headers; the probe reads a page (`/`), and the
   narration speaks of pages.
   After `04-permissions` and `05-referrer`, the site must still send no Content-Security-Policy: both saves post the
   empty boxes of the first tab, and the code relies on the model binder turning an empty box into nothing
   (`SecuritySettings.cs:20-25`). If a header with empty directives appears, stop at the first take and report it.
2. **The values each saving clip leaves.** `recording-plan.md` lists the expected header after `03-csp`, `04-permissions`
   and `05-referrer`. In particular `frame-ancestors 'self';object-src 'none'` (order and the missing space after the
   semicolon come from `SecurityHeaderDefaults.ContentSecurityPolicyNames` and the `;` separator) and
   `geolocation=(self "https://maps.example.com")` (from `PermissionsHeaderPolicyProvider.FormatAllowList`).
3. **The admin still works after `03-csp` is saved.** The headers apply to the admin as well. Frame Ancestors `'self'`
   and Object Source `'none'` were chosen because they restrict nothing the admin is known to use (the content
   preview frame is of the same site). Confirm that the admin pages, the recorder's cursor and highlights, and the
   toasts still work after the save. Do not narrow Default Source, Script Source or Style Source to make the video
   "more visible": the admin has inline style blocks (for example `NotifyMessages.cshtml:2` in TheAdmin) and inline
   script blocks in several views, and the recorder draws its cursor and highlights with inline code.
4. **Upgrade Insecure Requests on `http://localhost`.** The video only highlights the checkbox. Its effect on a site
   served over plain HTTP on localhost was not checked, so it is never checked on camera.
5. **Where the restart warning sits.** `AddTenantReloadWarningWrapper()` (`SecuritySettingsDisplayDriver.cs:57`) wraps the
   editor shape with `Settings-Reload.Wrapper.cshtml`. The narration (`open-3`) only says "a warning on the page". Check
   whether it is above the tabs or inside the first tab, and whether it stays visible on every tab, then adjust the
   highlight of `open-3` in `recording-plan.md`.
6. **After Save.** `AdminController.IndexPost` redirects to the same page, and the tabs are drawn again. The plan assumes
   the first tab (Content Security Policy) is selected after the redirect, and that the toast reads "Site settings updated
   successfully." for 5 seconds. Confirm, and confirm how long the redirect takes while the tenant restarts (the plan
   asks for a new take above 4 seconds).
7. **Selectors.** The menu classes (`li.oc-navigation-admin-settings-security-security_headers`, ...) come from
   `NavigationManager.AddNavigationClasses` and the release notes; the group buttons of the left menu come from
   `NavigationItem-admin.cshtml` and `NavigationItemText.cshtml`. The row selectors (`tr:has(td:text-is("..."))`), the
   radios (`input[name="geolocation"][value="Self"]`), the box (`#geolocationAllowedOrigins`), the list
   (`select[name$="ReferrerPolicy"]`), the tabs (`ul[role=tablist] a[role=tab]`) and the Features row
   (`[data-filter-value^="Security OrchardCore.Security"]`) were derived from the Razor views and `GroupShapes.cs`, not from
   a rendered page. Check each one finds exactly one element.
8. **The Features search.** Typing "Security" also shows other features whose name, id or description holds that word.
   The narration does not count them. If the filtered list looks crowded on the running site, tell the author: the
   narration says "search for Security", so a different search word would need a change of `open-1`.
9. **Pronunciation.** The storyboard's `spoken` map changes "X-Content-Type-Options" to "X Content Type Options",
   "X-Frame-Options" to "X Frame Options" and "nosniff" to "no sniff". `scripts/pronounce.py` was not run (no network use).
   A person listens to these, to "Referrer", to "HTTPS" and to "Frame Ancestors" before narrating for good.

## Statements that come from the web standards, not from the Orchard Core source

These are in the narration because the video cannot be understood without them. Each is the usual meaning of the value,
and none is a statement about Orchard Core. A person who knows them well confirms the wording.

- `why-1`, `why-2`, `why-3`: the browser follows the security headers; X-Content-Type-Options makes the browser use the
  declared file type instead of guessing it; Permissions Policy and Referrer Policy limit browser features and what a link
  reveals. The source supports the module's own wording only: the hint texts of the three tabs and the descriptions in
  `ConfigurationSchema.json`.
- `csp-3`: `'self'` stands for the site itself, `'none'` allows nothing, sources are separated by spaces. The schema's
  examples show the quotes and the space-separated list; the meaning of the two words is the standard one.
- `csp-1`, `csp-2`: most rows of the first tab name a kind of resource (the schema calls them fetch directives), and
  Default Source is their fallback (`ConfigurationSchema.json`: "The fallback sources for the fetch directives that are
  not defined"). Base URI, Form Action, Frame Ancestors and Sandbox are not kinds of resource and do not fall back to
  Default Source, which is why `csp-1` says "most rows".
- `csp-4`: Frame Ancestors makes the older X-Frame-Options header obsolete (the documentation page says so, and so does
  `ConfigurationSchema.json`); Object Source controls plugins (`ConfigurationSchema.json`: "plugins loaded by the
  <object> and <embed> elements").
- `csp-6`: what a policy does not allow, the browser does not load (the hint says the policy controls "resources the user
  agent is allowed to load").
- `ref-2`: No referrer sends nothing; Same origin sends the address of the page to pages of the same site and nothing to
  others.

## Things the source says that the documentation does not (also in the result of the author)

- **None in the Permissions Policy omits the feature from the header** (`SecuritySettings.cs:36-52`,
  `SecurityHeadersOptions.cs:54-62`; `ConfigurationSchema.json` says the same). So the narration says "None leaves the
  feature out of the header", never "None blocks the feature". Whether None was meant to mean an empty list (which blocks
  the feature) is a question for the maintainers; there is no way to write "camera=()" from the admin.
- **X-Content-Type-Options has no field.** The update always writes the default (`SecuritySettingsDisplayDriver.cs:110`).
- **The module does not register its settings group with the Settings module** (no call to `AddSiteSettingsPermission`
  for the group "SecurityHeaders", unlike Email in `OrchardCore.Email/Startup.cs:21`). `AdminController.Index` and
  `IndexPost` (`OrchardCore.Settings/Controllers/AdminController.cs:53,85`) require `ManageGroupSettings`, which
  "Manage settings" implies (`OrchardCore.Settings.Core/SettingsPermissions.cs:21`); `SiteSettingsAuthorizationHandler`
  grants it only for registered groups, and no other handler of `PermissionRequirement` in the source deals with it for
  this group. The driver also requires "Manage Security Headers Settings" (`SecuritySettingsDisplayDriver.cs:52,99`). So
  the page takes both permissions, and `know-1` says so, together with the menu item (`AdminMenu.cs:29`) and the default
  of the Administrator role (`SecurityPermissions.cs:18-25`, `OrchardCore.Settings/Permissions.cs:17-24`). The source
  gives no reason to doubt it, but it was not run: confirm with a role that has only the Security permission (the menu
  item shows and the page is refused) and with a role that has both (the page opens).
- **A host must call `ConfigureSecuritySettings()` for configuration to apply** (`OrchardCoreBuilderExtensions.cs:9`,
  not called by `src/OrchardCore.Cms.Web/Program.cs`). `know-2` says "when the site is set up to read them" for that
  reason. When it applies, it also empties the Content Security Policy and Permissions Policy saved in the admin, even
  for a directive or a feature the configuration does not mention (`OrchardCoreBuilderExtensions.cs:17-18`).
- **With configuration in use, the first tab and the other tabs read different sources.** When the host calls
  `ConfigureSecuritySettings()`, the Content Security Policy rows show the saved settings
  (`SecuritySettingsDisplayDriver.cs:72`) while the two checkboxes and the other tabs show the configured values. The
  video says only that configured values take precedence (`know-2`) and does not show it, because the default host does
  not call the method.

## For the maintainers

- The documentation page already embeds a YouTube video (`nYfNq8sTIAg`, under "## Video"). A maintainer decides whether
  this video replaces it or sits beside it; `embeddedIn` of this video lists the page.
