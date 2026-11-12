# Recording plan: users-ticket-store

A developer video. Its slides are drawn by `slides.py`; the only browser recording is the clip `02-enable`, on the Features
page. The clip does not change the site: it never clicks Enable (see "Why the clip does not click Enable" below).

In this plan a selector in backticks, such as `#search-box`, stands for `scene.Page.Locator("#search-box")`. File
references are in the Orchard Core source (`src/...` of `OrchardCMS/OrchardCore`).

## The site it needs

- **Orchard Core:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, built and run on
  `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` in an empty folder outside both repositories
  (`ORCHARD_APP_DATA`).
- **Provisioning:** Auto Setup through environment variables, so no setup form is recorded (see `references/recording.md`
  and the Auto Setup documentation, `src/docs/reference/modules/AutoSetup/README.md`). `OrchardCore.Cms.Web` already adds the
  feature (`src/OrchardCore.Cms.Web/Program.cs:9`, `.AddSetupFeatures("OrchardCore.AutoSetup")`), so the variables are all it
  needs. Tenant `Default`, only that tenant.

  | Setting | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | the value of `VIDEO_PASSWORD`, chosen outside git |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Outbound calls:** set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (`http://127.0.0.1:9`) so the site never calls
  an outside service.
- **Features to enable before recording:** none. The Blog recipe enables `OrchardCore.Features` and `OrchardCore.Users`
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:38` and `:46`), which is all the clip needs.
  `OrchardCore.Users.Authentication.CacheTicketStore` must be **disabled** when the clip is recorded: its row has to show
  the status Disabled and an Enable button. Do not enable Redis or any other feature.
- **Settings:** none to save. Leave the admin list settings as Auto Setup creates them, so the Features page opens in its
  default layout with the actions as buttons (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30` and
  `:36`: `List` and `Buttons`). The Blog recipe stores no admin list settings, and the `List` section of the admin
  configuration is commented out in `src/OrchardCore.Cms.Web/appsettings.json:19` to `:27`.
- **External services:** none. Redis and Azure Storage appear on slides only, so no emulator is needed.
- **Seed data:** none. The site holds only the Blog recipe's own content and the user `admin`; no person, company, email
  or phone number has to be created. The only email on any page is `admin@example.com`.
- **Sign-in:** outside the capture, run `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD` (the password
  chosen outside git). It saves `state.json`, which is git-ignored.

## Order, dependencies and snapshots

- One clip is recorded: `02-enable`. The other clips of the storyboard (`01-why`, `02-recipe`, `03-how`, `04-cache`) are
  slides and need no recording.
- `02-enable` depends on nothing, and no clip depends on it.
- **No clip changes the site.** `02-enable` only types in a search box and moves the cursor, so a take can follow
  another with no snapshot restore. Still, copy `App_Data` and `state.json` aside after `Recorder setup`, and restore the
  copy if the feature was enabled by mistake (it must read Disabled at the start of every take).
- Start the site, run `Recorder setup`, then record. After a restart of the site or a restore of `App_Data`, run
  `Recorder setup` again.

### Why the clip does not click Enable

Enabling the feature while signed in with a cookie issued before the store existed is not verified (see
`OPEN-QUESTIONS.md`, question 1): the page that loads after the click could be the sign-in page. The narration of
`enable-3` therefore says what the button does, and the clip ends with the cursor and a green ring on Enable, with no
click. Never click Enable during a take.

## Clip `02-enable`

**Starts on:** `/Admin/Features`, opened with `scene.OpenAsync("/Admin/Features")` before `StartCaptureAsync`. The page
is titled Features (`src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:15`, the breadcrumb) and
sits in the admin menu under Tools, then Features
(`src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26` and `:27`). Before the capture, check that the menu on
the left shows Tools open with Features as the current entry; if Tools is closed, click Tools and wait for the Features
entry to show. The search box has the focus on load (`autofocus`, `Features.cshtml:27`). The page must list the Security
category and the Users Authentication Ticket Store row with the status Disabled.

Selectors used below (`row` is `[data-filter-value*='CacheTicketStore']`, set by
`src/OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20` on the row of the feature):

| Name | Selector | Label from |
|---|---|---|
| search box | `#search-box` | `Features.cshtml:27` (placeholder "Search") |
| category heading | `.feature-group:not(.d-none) h3` | `Features.cshtml:88` (the category, "Security") |
| row | `[data-filter-value*='CacheTicketStore']` | `FeatureEntryDisplayDriver.cs:20` |
| name | `row` `.fw-semibold` (first) | `Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11` |
| identifier | `row` `code` (first) | `Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11` |
| description | `row` `.admin-list-secondary` | `Views/FeatureEntry.Description.SummaryAdmin.cshtml:8` |
| dependency badge | `row` `.direct-dependencies .badge` (first, text "Users") | `Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12` |
| status badge | `row` `.metadata .badge` (first, text "Disabled") | `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15` |
| Enable button | `row` `a.btn-primary` (text "Enable") | `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` |

The name, the identifier and the description come from the manifest of the feature
(`src/OrchardCore.Modules/OrchardCore.Users/Manifest.cs:114` to `:120`), and the category from `Category = "Security"`
(`Manifest.cs:119`).

### Step `enable-1`

Actions:

1. `scene.TypeAsync(search box, "ticket")`. The list filters as the text is typed.
2. Wait until exactly one row is left:
   `scene.Page.WaitForFunctionAsync("() => document.querySelectorAll('.feature-group [data-filter-value]:not(.d-none)').length === 1")`.
3. `scene.PauseAsync(1500)`, then `scene.HighlightAsync(category heading)` without a caption.

On screen when the narration ends: the Features page with the search box holding `ticket`, the summary line reading
"Showing 1 of" followed by the total and "features" (the text the search script fills in comes from
`Features.cshtml:57`; `:59` is the text as loaded), the category heading Security with a green ring, and a single row,
Users Authentication Ticket Store. No other category is shown.

### Step `enable-2`

Actions, in this order, each followed by `scene.PauseAsync(1400)` and `scene.ClearHighlightsAsync()` (start with
`ClearHighlightsAsync()` to remove the ring of the previous step):

1. `scene.HighlightAsync(name)`.
2. `scene.HighlightAsync(identifier)`: the id `OrchardCore.Users.Authentication.CacheTicketStore` in monospace.
3. `scene.HighlightAsync(description)`.
4. `scene.HighlightAsync(dependency badge)`: the badge reads Users.
5. `scene.HighlightAsync(status badge)`: the badge reads Disabled. Do not clear this last ring.

On screen when the narration ends: the single row, with the green ring on the status badge Disabled. The actions take
about 8 seconds, shorter than the narration.

### Step `enable-3`

Actions:

1. `scene.ClearHighlightsAsync()`.
2. `scene.MoveToAsync(Enable button)`.
3. `scene.HighlightAsync(Enable button)`. **Do not click.**

On screen when the narration ends: the single row, the status still Disabled, the cursor on the Enable button, and a
green ring around it. The feature has not been enabled, and there is no notification on the page.

## Slides

These steps have no recording. `slides.py` draws them; check each once with the in-memory render described in the
storyboard reference, and look at the frame in the assembled video.

- `why-1`: "The ticket in the cookie", four cards; the first two (one claim for each role, one for each permission) are
  bright.
- `why-2`: the same four cards; the last two (a cookie that grows, requests that fail) are bright.
- `why-3`: "With the ticket store", three check cards: the ticket stays on the server, a small cookie, the same ticket.
- `recipe-1`: "Enable it from a recipe": a recipe with the `Feature` step enabling the ticket store, and two notes.
- `how-1`: "1. Register the cookie configuration": the feature manifest, and `CacheTicketStoreStartup`
  (`src/OrchardCore.Modules/OrchardCore.Users/CacheTicketStoreStartup.cs`).
- `how-2`: "2. Set the session store": `CookieAuthenticationOptionsConfigure.Configure`
  (`src/OrchardCore.Modules/OrchardCore.Users/CookieAuthenticationOptionsConfigure.cs:26` to `:32`).
- `how-3`: "3. Store the ticket": `CacheTicketStore.StoreAsync`
  (`src/OrchardCore/OrchardCore.Users.Core/Authentication/CacheTicketStore.cs:81` to `:99`).
- `how-4`: "4. Retrieve the ticket": `CacheTicketStore.RetrieveAsync` (`CacheTicketStore.cs:58` to `:79`).
- `how-5`: "5. Remove and renew": `CacheTicketStore.RemoveAsync` and `RenewAsync` (`CacheTicketStore.cs:35` to `:56`).
- `cache-1`: "The default: a memory cache": the tenant registration of `MemoryDistributedCache`
  (`src/OrchardCore/OrchardCore.Infrastructure/Cache/OrchardCoreBuilderExtensions.cs:49` and `:50`) and two notes.
- `cache-2`: "Share the tickets with Redis": the Redis section of the configuration, a recipe enabling Redis and Redis Cache
  (`src/OrchardCore.Modules/OrchardCore.Redis/Manifest.cs:10` to `:23`), and a note about a missing or invalid configuration
  (`src/OrchardCore.Modules/OrchardCore.Redis/Startup.cs:62` to `:77`).
- `cache-3`: "Share the Data Protection keys too": the data protector of `CacheTicketStore`
  (`CacheTicketStore.cs:31` to `:33`) and the names of the two features that share the keys, Distributed Data Protection
  (Redis) (`src/OrchardCore.Modules/OrchardCore.Redis/Manifest.cs:41` to `:47`) and Distributed Data Protection (Azure
  Storage) (`src/OrchardCore.Modules/OrchardCore.DataProtection.Azure/Manifest.cs:4`).

## Checks for private data

- OCR the clip: the only email on the page may be `admin@example.com`, if the user menu shows it at all.
- No real name, company, address, phone number, token or password appears. The password is typed by `Recorder setup`
  outside the capture and is never in the clip.
- `state.json`, the `App_Data` copy and any mask script stay outside git.
- The slides hold only invented values: `localhost:6379`, the prefix `MyApp:`, and public Orchard Core identifiers.
