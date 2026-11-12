# Open questions: URL Rewriting

What was checked in the Orchard Core source (commit `92db4a83`) and the documentation, and what a person must confirm on a
running site before or while recording. The narration states only what the source shows; each item below is something the
source cannot show. `M/` stands for `src/OrchardCore.Modules/OrchardCore.UrlRewriting/` and `C/` for
`src/OrchardCore/OrchardCore.UrlRewriting.Core/`.

## Needs a running site

1. **Do the documented patterns match as typed?** The video types `^/about-us$` with the substitution `/about`, and
   `^/img/(.*)$` with the substitution `/media/$1`, exactly as the documentation's recipe example writes them. Checked:
   each rule becomes one line `RewriteRule <pattern> <substitution> [flags]` (`C/Services/UrlRedirectRuleSource.cs:39-53`,
   `C/Services/UrlRewriteRuleSource.cs:39-53`) that is handed to the Apache mod_rewrite reader of ASP.NET Core
   (`options.AddApacheModRewrite`). The matching itself is in that library, not in the Orchard Core source, so whether the
   leading slash is part of the text matched could not be verified in the Orchard Core source. A second check, in the
   ASP.NET Core repository (`dotnet/aspnetcore`, branch `main`, which is not necessarily the framework version that Orchard Core builds
   against at this commit, and not in the Orchard Core source), points the same way: the test
   `src/Middleware/Rewrite/test/ApacheModRewrite/ModRewriteMiddlewareTest.cs` uses `RewriteRule ^/$ /homepage.html` and
   expects the request `/` to be rewritten, and `ApacheModRewriteRule.ApplyRule` hands the request path to the match
   without removing its first slash. So `^/about-us$` and `^/img/(.*)$` should match as typed. Confirm on a running site
   that `/about-us` ends on `/about` and that `/img/about-bg.jpg` returns the image. If the leading slash must be left out,
   the narration of `redirect-3` and `rewrite-2` ("a caret, then /about-us, then a dollar sign") and the `pattern-1`
   slide need a different pattern.
2. **Does the rewritten address reach the Media files?** The module registers its middleware before every other module
   (`src/OrchardCore/OrchardCore.Abstractions/OrchardCoreConstants.cs:92-94`), so the rewrite should run before the Media
   static files. Confirm that `/img/about-bg.jpg` shows the image that `/media/about-bg.jpg` shows, with the address
   unchanged.
3. **Is the query string kept by the redirect?** The step `redirect-test-2` says that `?ref=newsletter` is still attached
   after the redirect, because Query String Policy is "Append Query String from Original Request" (the QSA flag,
   `C/Services/UrlRedirectRuleSource.cs:77-80`). In the ASP.NET Core repository (`dotnet/aspnetcore`, branch `main`, not necessarily the framework version that Orchard Core builds against
   at this commit, and not in the Orchard Core source), the Apache reader parses `NC`, `QSA`, `QSD`, `L` and `R=<code>` (`src/Middleware/Rewrite/src/ApacheModRewrite/RuleBuilder.cs`),
   and the redirect action sends the original query string in the `Location` header unless `QSD` is set
   (`src/Middleware/Rewrite/src/UrlActions/RedirectAction.cs`); the rewrite action clears it only when `QSD` is set
   (`.../UrlActions/RewriteAction.cs`). Confirm that the browser ends on `/about?ref=newsletter`. If it does not,
   the narration of `redirect-test-2` and the label text in the plan must change.
4. **Does reordering, or a bulk delete, apply before the site restarts?** Saving a rule (through its display driver) and
   deleting one row request a restart of the tenant (`M/Drivers/RewriteRulesDisplayDriver.cs:75`,
   `M/Controllers/AdminController.cs:251`). Dragging a row (`M/Endpoints/Rules/SortRulesEndpoint.cs`) and the bulk
   **Delete** (`M/Controllers/AdminController.cs:261-293`) save the document without requesting a restart, and the rules
   are read when the request pipeline of the tenant is built (`C/Services/RewriteOptionsConfiguration.cs:26-51`,
   `C/Extensions/ApplicationBuilderExtensions.cs:12-14`, `M/Startup.cs:47-49`). By the source, a new order or a bulk delete
   therefore applies the next time the tenant restarts. The video says only that the order is saved and that it is the order in which the rules are
   checked. It does not say that a new order or a bulk delete takes effect at once. Confirm what happens on a running site.
5. **Does the drag work in the headless browser?** The list uses SortableJS with a handle (`M/Assets/js/sortable-rules.js:46-54`).
   Confirm that the drag of `manage-3` moves the row and posts to `/url-rewriting/resort`. The plan has a fallback.
6. **Admin menu and Features list as the recorder sees them.** The labels come from the menu builders
   (`M/AdminMenu.cs:18-19`, `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`). Confirm that Tools opens
   a section holding Features and URL Rewriting, that the locators in the plan find them, and that typing "URL Rewriting"
   in the Features search box leaves the row visible.
7. **The rows of the List layout.** `manage-1` names a drag handle, a checkbox, the rule name, its source, the creation
   time, **Edit** and **Delete**. They come from the row views (`M/Views/RewriteRule.*.SummaryAdmin.cshtml`). Confirm
   that the List layout shows all of them on one row in the recording size.
8. **The role permission.** `enable-3` says that users need the permission "Manage URLs rewriting rules" and that the
   Administrator role has it by default (`C/UrlRewritingPermissions.cs:8`,
   `M/Services/UrlRewritingPermissionProvider.cs:15-22`). Confirm the label in the permissions of a role if the
   Roles page is open at any point.

## Needs a person's ear

9. **Pronunciation.** The storyboard's `spoken` map makes the voice say the addresses with "slash" (for example
    "/about-us" as "slash about-us"), "$1" as "dollar one", "Found (302)" as "Found, three oh two" and "Moved
    Permanently (301)" as "Moved Permanently, three oh one". Check each with `scripts/pronounce.py` before narrating, and
    check that the voice reads "dot star", "img" (as in "Serve media from img") and "about-bg.jpg" in a natural way.
