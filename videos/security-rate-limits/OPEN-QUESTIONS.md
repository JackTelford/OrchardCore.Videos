# Open questions: security-rate-limits

The narration, the slides and `video.json` hold only what the Orchard Core source (commit 92db4a83) and the
documentation page `src/docs/reference/modules/RateLimits/README.md` state. The items below were not run on a site. A
person with the site from `recording-plan.md` confirms each one on the first take; where the answer differs from what is
written, the plan is corrected (and the narration, if a claim changes).

## Page structure the recorder relies on

1. **Ids and classes.** `recording-plan.md` uses `#search-box` (Features), `#search-text`, `a.create`, `#Name`,
   `#Description`, `select[id$='Scope']`, `input[id$='Path']`, `input[id$='PermitLimit']`, `input[id$='WindowSeconds']`,
   `input[id$='QueueLimit']`, `button[data-bs-target='#modalAddLimiter']`, `button[data-enable-policy-id]`,
   `button[data-disable-policy-id]`, `button[data-clone-policy-id]`, `#modalOkButton`, `#actions`,
   `input[name='policyIds']`, `a.dropdown-item[data-action='Remove']`, and in the sidebar `#left-nav a` (text `Rate Limits`) and
   `#left-nav button` (text `Tools`). They are read from the views
   (`OrchardCore.RateLimits/Views`, `OrchardCore.Features/Views/Admin/Features.cshtml`, `OrchardCore.Themes/TheAdmin`)
   and the scripts under `Assets/ts`, not from a rendered page. Run `Recorder probe` on each page and fix a selector
   that does not match.
2. **The row of a policy.** The plan calls it "the list item that contains the link". The List layout renders each
   policy with `admin-list-list item-css-class="list-group-item"` (`Views/Admin/Index.cshtml:40`); confirm the element
   and that the Edit button, the Actions drop-down and the checkbox are inside it.
3. **Features search.** The search on the Features page filters as you type (`features-index.ts`). Confirm that the text
   `Rate Limits` leaves only the Rate Limits row. If another feature's description contains the phrase, type
   `OrchardCore.RateLimits` instead (the narration says "search for Rate Limits"; the viewer sees the typed text, so keep
   the narration and the typed text equal if the plan changes). Confirm the id of the Enable link
   (`btn-enable-` followed by an id generated from the feature id).

## Things shown on screen that the source does not settle

4. **Read-only Policies on a Blog recipe site.** Expected, from the code: under **Route limits**, two entries ordered by
   route name (`Controllers/AdminController.cs:668`), each with a **POST** badge: `/Login` and `/ResendEmailConfirmation`.
   The Users module registers both when Rate Limits is on (`OrchardCore.Users/Startup.cs:276-289`), and maps both routes
   with a literal pattern (`Startup.cs:194-203` and `:260-269`; the default sign-in path is `Login`,
   `OrchardCore.Users.Abstractions/UserOptions.cs:5`), which `FormatPath` (`AdminController.cs:715`) turns into the paths.
   The other Users limits (registration, password reset, two-factor, external login) belong to features that the Blog
   recipe does not enable, and no feature registers a group limit, so no **Group limits** list is expected. The narration
   says only that the Users feature limits the sign-in route; correct `readonly-2`'s on-screen check if the entries
   differ.
5. **The author badge.** The policy rows show the author's name next to the date badge when `Author` is set
   (`Views/Items/RateLimitPolicy.DefaultMeta.SummaryAdmin.cshtml:15`). The Default Global Policy gets it from the recipe
   parameter `AdminUsername` (`Migrations/default-global-policy.recipe.json:11`). When the feature is enabled long after
   the setup, the setup-time provider has nothing (`OrchardCore.Recipes.Core/Services/RecipeEnvironmentFeatureProvider.cs:21`),
   but the Users module provides the super user's name (`OrchardCore.Users/Services/RecipeEnvironmentSuperUserProvider.cs:27-43`,
   registered at `OrchardCore.Users/Startup.cs:153`), so the row should read `admin`. A missing parameter gives no author
   (`OrchardCore.Recipes.Core/ParametersMethodProvider.cs:19`). Confirm what the row shows; it must read `admin` or be
   absent. `Blog limit` and its clone take the signed-in user name (`AdminController.cs:523` and `:592`), also `admin`.
6. **The error page of a rejected request (`test-3`).** The server sends `429` with no body
   (`RateLimiterOptionsConfigurations.cs:43`). What Chromium draws for an empty `429` page, and whether Playwright's
   `GotoAsync` raises `net::ERR_HTTP_RESPONSE_CODE_FAILURE` for it, are not known. The plan has a fallback
   (`catch`, then `scene.SwitchToAsync(scene.Page)`). If the page is blank rather than an error page, the narration
   still holds, but the plan's "on screen" line changes.
7. **The path match.** `/blog` is meant to match `/blog` and `/blog/post-1`, and not `/blogger`. The rule is
   `Request.Path.StartsWithSegments(policy.Path, StringComparison.OrdinalIgnoreCase)`
   (`RateLimiterOptionsConfigurations.cs:154`), ASP.NET Core code that is not in this source: it matches whole path
   segments, so the narration says "the paths below it", not "paths that start with". The documentation states it with
   `/api` and `/api/users`. The form's hint shows the example `/api/` with a trailing slash
   (`Views/Admin/RateLimitPolicy.Fields.cshtml:68`), which the documentation does not use: with whole-segment matching a
   path with a trailing slash is not expected to match the paths below it, so confirm it on a running site and keep to
   `/blog` in the video. After the take, request `/blog/post-1` three times and `/blog` once inside a minute to confirm
   that they share the budget (the narration says "the blog and the paths below it, such as its posts").
8. **How many requests a visit sends to `/blog`.** The limiter counts every request whose path starts with `/blog`. The
   video relies on one request per visit (images are under `/media`, scripts and styles are static files, neither is
   counted by this policy). Look at the network log of a visit: if a visit sends a second request under `/blog`, the
   fourth visit is rejected too early or too late, and the number of visits or the permit limit changes (and
   `test-2`, `create-5`, `create-6` with them).
9. **The length of the restart.** Enabling and disabling a policy restarts the tenant (`Controllers/AdminController.cs:223-246`
   and `:252-275`). How long the first request after it takes decides how long the `OffCameraAsync` waits last; no step
   shows the restart.
10. **The description on the row of the Default Global Policy.** The row should show the whole description of the seeded
    policy (`Views/RateLimitPolicy.Description.SummaryAdmin.cshtml`). If the List layout truncates it, `default-1`
    still holds (the narration paraphrases its first sentence).

## Wording to check before narrating

11. `scripts/pronounce.py` for: "/blog" (should be said "slash blog"), "429" ("four twenty-nine"), "IP address", "HTTP",
    "Ok" (the label of the confirmation button) and "Blog limit (1)" (the brackets should not be read aloud). Add a
    `spoken` entry only after a person has confirmed the sample.
12. `shared-2` ends with a pointer to a separate video on reverse proxy settings. It stays true only while a video on
    that subject is part of the programme; the tracker has one in the same series.
