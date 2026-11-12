# Recording plan: security-rate-limits

A person who reads only this file can record the video. In this file a selector such as `#search-text` stands for
`scene.Page.Locator("#search-text")`, since the recorder's methods take locators. `file:line` references are in the
Orchard Core source at commit 92db4a83. A path that begins with a module name (`OrchardCore.Users/...`,
`OrchardCore.Features/...`) is relative to `src/OrchardCore.Modules`; `OrchardCore.Themes/...` is relative to `src`; a
path that begins with `Views`, `Controllers`, `Services` or `Migrations`, or a bare file name, is in the module
`src/OrchardCore.Modules/OrchardCore.RateLimits`. The ids and classes of the pages come from the views and are not
checked on a running site: confirm each one with `Recorder probe` on the first take (see OPEN-QUESTIONS.md).

Narrate before recording (`narrate.py`), since the recorder holds each step for the length of its narration. The
seconds quoted for the steps are estimates at 150 words per minute; `durations.json` has the real ones. Keep the
actions of a step shorter than its narration.

## The site it needs

- **Source:** a checkout of `OrchardCMS/OrchardCore` that has the Rate Limits module (`OrchardCore.RateLimits`),
  running `src/OrchardCore.Cms.Web` on `http://localhost:5010` (`VIDEO_BASE_URL`).
- **App_Data:** an empty folder outside both repositories (`ORCHARD_APP_DATA`).
- **Auto Setup** (no setup form and no sign-in form is recorded): tenant `Default`, site name `Orchard Core`, time zone
  `America/Los_Angeles`, recipe `Blog`, database provider `Sqlite`, user `admin`, email `admin@example.com`, password
  `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git and never written in a file of this repository).
  The option names are in `src/docs/reference/modules/AutoSetup/README.md` of the Orchard Core checkout.
- **Outbound calls:** set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address. This video needs no external service,
  no mail server, no SMS and no payment provider.
- **Features to enable before recording:** none. The Blog recipe enables Users (`OrchardCore.Users`), which the
  clip `09-readonly` needs. **Rate Limits must be off** at the start: it is turned on, on camera, in `02-enable`.
  Do not enable any other feature.
- **Settings:** none. Leave Configuration, Settings, Admin at its defaults: the policy list is the List layout, and the
  row actions are the Edit button and the Actions menu (the `Buttons` layout). If the host sets another admin list
  layout in its configuration, remove it, or the rows in this plan do not match.
- **Seed data:** none outside the capture. The Blog recipe's sample content is the only data: the blog at `/blog` and
  its post at `/blog/post-1`. Everything else is typed on camera, with the fake values in this plan: the policy name
  `Blog limit`, the description `Limits requests to the blog.`, the path `/blog`, the numbers 3, 60 and 0.
  The description must not contain `/blog`, because `manage-1` searches for that text and must match the path only.
- **Sign-in:** once, outside the capture: `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`, `Recorder setup`. This
  writes `state.json`, which is git-ignored.
- **Browser:** the recorder's own (Chromium, 1536x864, light, en-US). Every request, including the visits to `/blog`,
  is made signed in as `admin`, so the limiter counts it against that user.

## Order of the clips, and snapshots

The clips depend on one another and are recorded in the order they play: `02-enable`, `03-default`, `06-create`,
`07-test`, `08-manage`, `09-readonly`. Chapters 01, 04, 05, 10 and 11 are slides (drawn by `slides.py`, nothing to
record). Take a snapshot (copy `App_Data` and `state.json` aside, with the site stopped) at these points, and restore
the one a take needs before recording it:

| Snapshot | Taken | Site state | Needed by |
|---|---|---|---|
| A | after Auto Setup and `Recorder setup` | Blog recipe only, Rate Limits **off**, signed in | `02-enable` |
| B | after `02-enable` | Rate Limits on, Default Global Policy the only policy | `03-default`, `06-create`, `09-readonly` |
| C | after `06-create` | Blog limit saved, **Disabled**, with its Fixed window limiter | `07-test` |
| D | after `07-test` | Blog limit **Enabled** | `08-manage` |

Which clips change the site:

- `02-enable` enables the feature, which adds the Default Global Policy (a migration) and restarts the tenant.
- `03-default` changes nothing: leave every page without saving.
- `06-create` adds the policy Blog limit.
- `07-test` enables Blog limit, which restarts the tenant, and sends requests to `/blog`. The limiter counts in memory:
  restoring snapshot C and starting the site gives empty counters. To take `07-test` again without restoring, wait 60
  seconds after the last request to `/blog`, or restart the site.
- `08-manage` clones, disables and deletes policies, and leaves only the Default Global Policy (the state of snapshot B).
- `09-readonly` changes nothing.

Stop the site before copying or restoring `App_Data`, and start it again before the next take.

**Restarts.** Enabling a feature, and enabling, disabling or deleting an enabled policy, restart the tenant: the page
that follows loads slowly. Click with `ClickAndWaitForNavigationAsync` or `WaitForNavigationAsync`, which keep the
capture going on the new page. If the wait lasts more than about two seconds on a take, wrap the click and its wait in
`scene.OffCameraAsync`, with a condition that only the new state meets (for instance the badge **Enabled** on the row),
so the cut is clean. A step whose click is followed by a restart does its click early enough for the new page to be on
screen when the narration ends.

## Slides

These steps have no recording; `slides.py` draws them, one image per step.

- `why-1`: Floods of requests. Three cards: every request costs work, a flood slows everyone, repeated attempts.
- `why-2`: Who shares a budget. Three cards: a signed-in user, an anonymous visitor (by IP address), a shared address.
- `why-3`: What happens over the budget. Three cards; the last two bright: over the budget, and the 429 response.
- `types-1`: Three types of policy. Four cards; Global and Endpoint bright.
- `types-2`: Three types of policy. The same four cards; Group and Limiters bright.
- `limiters-1`: Four types of limiter. Fixed window and Sliding window bright.
- `limiters-2`: Four types of limiter. Concurrency and Token bucket bright.
- `shared-1`: Many visitors, one address. Three cards: an office or a school, a mobile carrier, signed-in users.
- `shared-2`: Behind a reverse proxy. Three cards: the address of the proxy, one shared budget, trust only your proxies.
- `move-1`: Moving policies between sites. Two cards: Deployment plan, Recipe.

## Clips

### 02-enable (chapter 02, Enable Rate Limits)

Starts on `/Admin/Features` (`scene.OpenAsync`), signed in, search box empty, Rate Limits not enabled. The admin menu
stays open as it is.

- `enable-1` (about 10 s). Type `Rate Limits` into the search box (`#search-box`, placeholder `Search`;
  `OrchardCore.Features/Views/Admin/Features.cshtml:27`). The list filters as you type (the page's script reacts to the `input`
  event; Enter does nothing). Wait until the list shows the **Rate Limits** row under the **Security** heading, with its
  description *Provides a way to manage rate limiting to the website.* and a blue **Enable** link. Highlight the row
  (`HighlightAsync`), then clear the highlight.
  On screen when the narration ends: the Features page with the search text, the Security heading, and only the Rate
  Limits row, with its Enable link.
  Labels: Rate Limits and Security, `OrchardCore.RateLimits/Manifest.cs:12-14`; Enable,
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the menu path Tools, Features,
  `OrchardCore.Features/AdminMenu.cs:26-27`.
- `enable-2` (about 10 s). Click the **Enable** link of that row (`a[id^='btn-enable-']` that is visible,
  `ClickAndWaitForNavigationAsync`). The site reloads the Features page with the message *The feature Rate Limits was
  enabled.* (`OrchardCore.Features/Controllers/AdminController.cs:317`); the first page after the restart is slow (see
  Restarts). The sidebar already shows **Tools** open, because the Features page belongs to it, and the new entry
  **Rate Limits** is in it (`OrchardCore.RateLimits/AdminMenu.cs:18-19`): click that link (`#left-nav a` with the text
  `Rate Limits`, `ClickAndWaitForNavigationAsync`) so the screen follows the narration. If the group is collapsed,
  click its **Tools** button first (`#left-nav button` with the text `Tools`); if the sidebar is compact or hidden, use
  `scene.NavigateAsync("/Admin/RateLimits")` instead (see OPEN-QUESTIONS.md, item 1).
  On screen when the narration ends: the page titled **Rate Limit Policies** with one row, **Default Global Policy**,
  badge **Enabled**, and the **Add Policy** button above the list.
  Labels: Rate Limit Policies, `OrchardCore.RateLimits/Views/Admin/Index.cshtml:7`; Add Policy, same file, line 26; the
  seeded policy, `OrchardCore.RateLimits/Migrations/default-global-policy.recipe.json:8` and
  `Migrations/GlobalRateLimitsMigrations.cs:29`; the menu path Tools, Rate Limits, `AdminMenu.cs:18-19`.

### 03-default (chapter 03, The default policy)

Starts on `/Admin/RateLimits` (`scene.OpenAsync`), from snapshot B. Changes nothing.

- `default-1` (about 13 s). No click. Highlight the row of **Default Global Policy**: its name (a link), the badge
  **Enabled**, the line *Applies to every tenant request.* and the description *Limits how many requests each signed-in
  user, or each IP address for anonymous visitors, can send per minute. ...* (the whole description is on the row; it
  is long, let the highlight cover the name, the badge and the first lines).
  On screen when the narration ends: the row, highlighted, with those three texts readable.
  Labels: `Views/RateLimitPolicy.DefaultTags.SummaryAdmin.cshtml:8`, `Views/RateLimitPolicy.Description.SummaryAdmin.cshtml:12`,
  `Migrations/default-global-policy.recipe.json:9`.
- `default-2` (about 11 s). Clear the highlight. Click **Edit** on the row (`a.btn` with the text `Edit`,
  `ClickAndWaitForNavigationAsync`; `Views/RateLimitPolicy.Buttons.SummaryAdmin.cshtml:6`). The page **Edit Rate
  Limit Policy** opens. Highlight the **Policy type** select (disabled, showing **Global**) and then the yellow warning
  (`.alert-warning`).
  On screen when the narration ends: the Policy type field with *The policy type is read-only while this policy is
  enabled. Disable it first to retarget the policy.* and the warning *This policy is enabled and cannot be modified. To
  modify it, disable it first or create another policy.*
  Labels: `Views/Admin/Edit.cshtml:4`, `Views/Admin/RateLimitPolicy.Fields.cshtml:31` and `:42`,
  `Controllers/AdminController.cs:31`.
- `default-3` (about 12 s). Clear highlights. If the **Limiters** card is not fully visible, `ScrollAsync(0, 400)`.
  Highlight the limiter in the card: **Fixed window**, with its summary *1000 requests every 60 seconds, queue limit 0*.
  The **Add Limiter** button and the limiter's **Edit** and **Delete** buttons are disabled: let the highlight cover the
  card header and the limiter. Do not click Save; the next clip opens its own page.
  On screen when the narration ends: the Limiters card with Add Limiter (disabled), Fixed window and the summary.
  Labels: `Views/Admin/Edit.cshtml:31` and `:37`; `Services/FixedWindowRateLimiterSource.cs:32` and `:52`;
  `Views/Items/RateLimitLimiter.Buttons.SummaryAdmin.cshtml` (the disabled Edit and Delete);
  values `Migrations/default-global-policy.recipe.json:20-22`.

### 06-create (chapter 06, Create a policy)

Starts on `/Admin/RateLimits` (`scene.OpenAsync`), from snapshot B: the Default Global Policy is the only row.

- `create-1` (about 9 s). Click **Add Policy** (`a.create`, `ClickAndWaitForNavigationAsync`). The page **New Rate Limit
  Policy** opens, with **Policy name** focused. Type `Blog limit` into **Policy name** (`#Name`), then type
  `Limits requests to the blog.` into **Description** (`#Description`, a text area).
  On screen when the narration ends: the form with both texts typed, **Policy type** still **Global**.
  Labels: `Views/Admin/Index.cshtml:26`, `Views/Admin/Create.cshtml:4`, `Views/Admin/RateLimitPolicy.Fields.cshtml:11`
  and `:20`.
- `create-2` (about 11 s). `scene.SelectAsync` on the **Policy type** select (`select[id$='Scope']`, the selector the
  page's own script uses, `Assets/ts/rate-limit-policy-fields.ts:136`) with `Endpoint`. The **Request path** field
  (`input[id$='Path']`) appears below. Type `/blog` into it (`scene.TypeAsync`). Do not open the **Group** type: its
  hint names code, and the video does not show it.
  On screen when the narration ends: Policy type **Endpoint**, **Request path** `/blog`, and the hint *Matches requests
  that start with this path, ...*.
  Labels: `Controllers/AdminController.cs:609-611` (Global, Endpoint, Group), `Views/Admin/RateLimitPolicy.Fields.cshtml:31`
  and `:53`.
- `create-3` (about 11 s). Click **Save and Continue** (`ClickAndWaitForNavigationAsync`). The page **Edit Rate Limit
  Policy** opens with the message *Policy created successfully.*, a **Limiters** card that says *Add one or more limiters
  before enabling this policy.*, and an **Add Limiter** button. In the last two seconds of the step, click **Add Limiter**
  (`button[data-bs-target='#modalAddLimiter']`): the dialog **Available Limiter Types** opens.
  On screen when the narration ends: the dialog opening over the Edit page.
  Labels: `Views/Admin/Create.cshtml:16` and `:22`, `Controllers/AdminController.cs:196`, `Views/Admin/Edit.cshtml:31`,
  `:37`, `:42` and `:75`.
- `create-4` (about 8 s). The dialog lists four cards in this order: **Concurrency**, **Fixed window**, **Sliding
  window**, **Token bucket** (sorted by name, `Controllers/AdminController.cs:647`), each with a description and a
  **Learn more** link and an **Add** button. Move the cursor over the cards (`MoveToAsync`), without clicking **Learn
  more** (it opens another tab). In the last two seconds, click **Add** in the **Fixed window** card
  (`.modal .card` that contains the heading `Fixed window`, then `a.btn` with the text `Add`;
  `ClickAndWaitForNavigationAsync`).
  On screen when the narration ends: the page **Add 'Fixed window' Limiter**, with a blue box that holds the description
  *Allows a set number of requests during each time period, then resets the count all at once.* and a **Learn more** link.
  Labels: `Views/RateLimitLimiter.Link.cshtml:10` and `:18`, `Services/FixedWindowRateLimiterSource.cs:32` and `:35`,
  `Views/Limiter/Create.cshtml:11`.
- `create-5` (about 11 s). The three fields start at 0. For each, clear it first (`FillAsync("")`, which is
  instant) and then type with `scene.TypeAsync`: **Permit limit** (`input[id$='PermitLimit']`) `3`, **Window seconds**
  (`input[id$='WindowSeconds']`) `60`. Leave **Queue limit** (`input[id$='QueueLimit']`) at `0` and put the cursor on
  it (`MoveToAsync`) while the narration says it. In the last two seconds, click **Add Limiter**
  (`button[type=submit]` with the text `Add Limiter`, `ClickAndWaitForNavigationAsync`).
  On screen when the narration ends: the Edit page again with the message *Limiter added successfully.* (or the form
  with 3, 60 and 0 typed, if the click is still loading).
  Labels: `Views/FixedWindowRateLimiter.Edit.cshtml:4`, `:13`, `:22` and `:26`, `Views/Limiter/Create.cshtml:31`,
  `Controllers/LimiterController.cs:141`.
- `create-6` (about 10 s). Scroll if needed (`ScrollAsync`, before any highlight), then highlight the limiter now listed in
  the **Limiters** card: **Fixed window**, *3 requests every 60 seconds, queue limit 0*, with its **Edit** and **Delete**
  buttons. Below the card the text *This policy is disabled. You can save changes without restarting the website. ...*
  is visible. Clear the highlight. In the last two seconds click **Save** (`button[type=submit]` with the text `Save`, `ClickAndWaitForNavigationAsync`).
  The list opens with *Policy saved successfully.*
  On screen when the narration ends: **Rate Limit Policies** with two rows, **Blog limit** with the badge **Disabled** and
  the line *Matches requests starting with '/blog'.*, and **Default Global Policy**.
  Labels: `Services/FixedWindowRateLimiterSource.cs:52`, `Views/Admin/Edit.cshtml:60` and `:66`,
  `Controllers/AdminController.cs:509`, `Views/RateLimitPolicy.DefaultTags.SummaryAdmin.cshtml:8`,
  `Views/RateLimitPolicy.Description.SummaryAdmin.cshtml:13`.

### 07-test (chapter 07, Enable and test a policy)

Starts on `/Admin/RateLimits` (`scene.OpenAsync`), from snapshot C. Do not open `/blog` before the capture: the first
visit must be the first request to that path since the policy was enabled.

- `test-1` (about 9 s). In the row of **Blog limit** (the list item that contains the link `Blog limit`), click
  **Actions** (the drop-down button), then **Enable** (`button[data-enable-policy-id]`). The dialog **Enable Policy**
  opens: *Are you sure you want to enable this policy? This website will restart after enabling it.* with the buttons
  **Ok** (red) and **Cancel**. Let the dialog stay about two seconds, then click **Ok** (`#modalOkButton`,
  `WaitForNavigationAsync`). The tenant restarts (see Restarts); the Rate Limit Policies page that loads shows the badge
  **Enabled** on the Blog limit row.
  On screen when the narration ends: the dialog, or the list after the restart (either is fine, as long as the dialog was
  on screen while the narration said it).
  Labels: `Views/RateLimitPolicy.ActionsMenuItems.SummaryAdmin.cshtml:26-27`, **Ok** and **Cancel** in
  `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`, `Controllers/AdminController.cs:246`.
- `test-2` (about 8 s). Highlight the badge **Enabled** of the Blog limit row (briefly). Then
  `scene.NavigateAsync("/blog")` three times, with `PauseAsync(500)` after each. Each visit shows the blog's home page.
  The three visits must finish inside the narration and inside one 60 second window with the fourth one of `test-3`.
  On screen when the narration ends: the blog page (the third visit).
  Labels: the Blog recipe's path, `OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:979`; the matching rule,
  `RateLimiterOptionsConfigurations.cs:154`.
- `test-3` (about 11 s). `scene.NavigateAsync("/blog")` a fourth time. The server answers `429 Too Many Requests` with no
  body (`RateLimiterOptionsConfigurations.cs:43`). Chromium shows its own error page for it (expected: *This page
  isn't working* and *HTTP ERROR 429*; confirm on the first take). If `NavigateAsync` throws a `PlaywrightException`
  that mentions `ERR_HTTP_RESPONSE_CODE_FAILURE`, catch it and call `scene.SwitchToAsync(scene.Page)`, which starts the
  capture again on the error page. Hold the page for the rest of the narration (`HoldAsync`).
  On screen when the narration ends: the error page for the rejected request, not the blog.

### 08-manage (chapter 08, Manage policies)

Starts on `/Admin/RateLimits` (`scene.OpenAsync`), from snapshot D: **Blog limit** (Enabled) and **Default Global Policy**.

- `manage-1` (about 8 s). Type `/blog` into the search box (`#search-text`, placeholder `Search policies`) and press
  Enter (`scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"))`). The list reloads with the box
  still holding `/blog`, a **Clear** button beside it, and only **Blog limit** in the list.
  On screen when the narration ends: the search box with `/blog` and one row, Blog limit.
  Labels: `Views/Admin/Index.cshtml:15`, `:17` and `:24`; the fields searched, `Controllers/AdminController.cs:756-763`.
- `manage-2` (about 8 s). In the row of Blog limit, click **Actions**, then **Clone** (`button[data-clone-policy-id]`,
  `scene.WaitForNavigationAsync`; no dialog). The page **Edit Rate Limit Policy** opens for the copy, with the message
  *Policy cloned successfully.* and the **Policy name** `Blog limit (1)`.
  On screen when the narration ends: that page, with the name `Blog limit (1)`, Endpoint and `/blog`.
  Labels: `Views/RateLimitPolicy.ActionsMenuItems.SummaryAdmin.cshtml:11`, `Controllers/AdminController.cs:322` and `:773-800`.
- `manage-3` (about 11 s). No click until the last two seconds. Highlight, one after the other, the **Policy type** select
  (enabled), the **Request path** field (editable), and the **Add Limiter** button (enabled); the text *This policy is
  disabled. You can save changes without restarting the website. ...* is at the bottom of the page. Clear the highlight,
  then click **Cancel** (`a.btn-secondary` with the text `Cancel`, `ClickAndWaitForNavigationAsync`) to return to the list.
  On screen when the narration ends: the list with three rows (Blog limit, Blog limit (1), Default Global Policy), or
  the edit page just before the click.
  Labels: `Views/Admin/Edit.cshtml:37`, `:60` and `:67`.
- `manage-4` (about 8 s). In the row whose link is exactly `Blog limit` (not `Blog limit (1)`), click **Actions**, then
  **Disable** (`button[data-disable-policy-id]`). The dialog **Disable Policy** opens: *Are you sure you want to disable
  this policy? This website will restart after disabling it.* Click **Ok** (`#modalOkButton`,
  `WaitForNavigationAsync`); the tenant restarts (see Restarts) and the badge on that row reads **Disabled**. The
  message *Policy disabled successfully.* is shown.
  On screen when the narration ends: the list with Blog limit and Blog limit (1) both **Disabled**.
  Labels: `Views/RateLimitPolicy.ActionsMenuItems.SummaryAdmin.cshtml:17-19`, `Controllers/AdminController.cs:275`.
- `manage-5` (about 10 s). Tick the checkbox of Blog limit and of Blog limit (1) (`input[name='policyIds']` in each row).
  The toolbar reads *2 selected* and an **Actions** drop-down appears above the list (`#actions`). Open it (`#actions
  button.dropdown-toggle`): **Enable**, **Disable**, **Delete**. Click **Delete** (`a.dropdown-item[data-action='Remove']`).
  The dialog **Delete Policies** opens: *Are you sure you want to delete the selected policies? If any selected policy is
  enabled, this website will restart after deletion.* Click **Ok** (`#modalOkButton`, `WaitForNavigationAsync`). Both are
  disabled, so there is no restart.
  On screen when the narration ends: the list with only **Default Global Policy**, and the message *2 policies were
  deleted successfully.*
  Labels: `Views/RateLimitsToolbar.cshtml:37` and `:46`, `Controllers/AdminController.cs:122-126` and `:423`,
  `Views/RateLimitsToolbar.cshtml:21` and `:28`.

### 09-readonly (chapter 09, Read-only policies)

Starts on `/Admin/RateLimits` (`scene.OpenAsync`), from snapshot B or after `08-manage`: Default Global Policy only.

- `readonly-1` (about 9 s). `scene.ScrollAsync(0, 600)` until the card **Read-only Policies** is on screen, under the
  list. Highlight its header and the sentence *These policies are set by features and are read-only.*
  On screen when the narration ends: the card, with its header and that sentence.
  Labels: `Views/Admin/Index.cshtml:69` and `:78`. The Users feature registers the sign-in route limit:
  `OrchardCore.Users/Startup.cs:276-289` (it applies when Rate Limits is on).
- `readonly-2` (about 13 s). Clear the highlight, then highlight the **Route limits** heading and the first entry: the
  path (expected `/Login`) with a **POST** badge. Keep the cursor on the badge.
  On screen when the narration ends: the **Route limits** list, with the sign-in entry and its POST badge.
  Labels: `Views/Admin/Index.cshtml:82-96`; the path is built by `Controllers/AdminController.cs:715`
  (`FormatPath`); the route name, `OrchardCore.Users/RateLimitRouteNames.cs:7`.

## Checks for private data

- Every name, email and password on screen is fake: the user is `admin` with `admin@example.com`, the site is `Orchard
  Core`, the policy names are `Blog limit`, `Blog limit (1)` and `Default Global Policy`. The password `<password>`
  never appears on screen (the sign-in is not recorded).
- The Blog recipe's sample content shows no real person's data; look at the `/blog` pages of `test-2` anyway.
- After assembling, OCR the video at one frame per second and at every scene change, then at five frames per second,
  and search the text for the machine's user name, a real email address, a host name other than `localhost`, and any
  token. The Chromium error page of `test-3` must not show a real address (it shows `localhost`).
- The author badge on the policy rows (user name next to the calendar badge) must read `admin`, or be absent.
- `state.json`, `App_Data` copies and the password stay out of git.
