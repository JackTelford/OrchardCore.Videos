# Recording plan: Health Checks

Read this file and `references/recording.md` of the skill, and record without deciding anything. Three of the
storyboard's clips are recordings: `02-enable`, `05-compact` and `05-detailed`. Every other step is a slide drawn by
`slides.py`. A selector below stands for `scene.Page.Locator(...)`. The labels were read in the Orchard Core source at
commit `92db4a83`; the paths after "Source" are relative to `src/OrchardCore.Modules/` in `OrchardCMS/OrchardCore`
(`OrchardCore.HealthChecks.Abstractions/` is in `src/OrchardCore/`).

The recorder shows only the page: the address bar is not part of the capture. A step that opens `/health/live`
therefore highlights the page with the route as its caption, so the viewer sees which address answers.

## The site it needs

| | |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web`, built from a checkout that has the Health Checks module (it is referenced by `OrchardCore.Application.Cms.Core.Targets`, as are Redis and SMS) |
| Run | on `http://localhost:5010` (`VIDEO_BASE_URL` otherwise), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`) |
| Auto Setup | environment variables `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` (see `src/docs/reference/modules/AutoSetup/README.md`). No Setup screen and no sign-in form is recorded |
| Sign-in | user `admin`, email `admin@example.com`, password `<password>`: chosen outside git, passed in `VIDEO_PASSWORD`. `Recorder setup` saves `state.json` (git-ignored) |
| Tenant | the Default tenant only, with no URL prefix and no host name. There is no second tenant |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address such as `http://127.0.0.1:9`. No clip calls an external service: the Redis feature has no configuration and the SMS feature stays disabled |
| Features before recording | none to enable. The Blog recipe enables neither Health Checks nor Redis, and the clips enable them on camera. Check on `/Admin/Features` that both are `Disabled` before the first take |
| Configuration | no `HealthChecks` section and no `Redis` section anywhere (no `OrchardCore__HealthChecks__*` or `OrchardCore__Redis__*` variable, nothing in `appsettings*.json` or user secrets), except `OrchardCore__HealthChecks__ShowDetails=true` for the clip `05-detailed` (below) |
| Seed data | none. The clips show the admin Features page and the endpoint, and no content item |
| `PrepareAsync` | not needed |

## Order, snapshots and restarts

The clips depend on each other. Record them in this order, with the snapshots between them (a snapshot is a copy of
the `App_Data` folder and of `state.json`, taken while the site is stopped):

| # | Clip | Starts from | Changes the site | Snapshot |
|---|---|---|---|---|
| 1 | `02-enable` | Snapshot A: the fresh Auto Setup site, signed in, both features disabled | Enables **Health Checks** | Snapshot B only if the next clip is recorded in a separate run (see 1) |
| 2 | `05-compact` | The state clip 1 leaves (Snapshot B when it was taken): Health Checks enabled, Redis disabled, no `ShowDetails` | Enables **Redis** | Take snapshot C afterwards |
| 3 | `05-detailed` | Snapshot C, with the site restarted with `OrchardCore__HealthChecks__ShowDetails=true` | Nothing | None |

1. Record clips 1 and 2 in one run: `Recorder record 02-enable 05-compact`. Between them, the site is still running
   and nothing needs a restart (an enabled feature recycles the tenant by itself). No snapshot B exists after such a
   run: to record only `05-compact` again, restore snapshot A and record both clips again. To keep a snapshot B
   instead, record the clips in two runs: after `02-enable`, stop the site, copy `App_Data` and `state.json` aside as
   snapshot B, start the site again, and record `05-compact`.
2. Stop the site (by its process ID), copy `App_Data` aside as snapshot C, set
   `OrchardCore__HealthChecks__ShowDetails=true` in the environment of the site, start it again on the same `App_Data`,
   and wait until `/Admin` answers. The route and the response writer are chosen when the tenant starts, so a
   running site does not pick the variable up. Then `Recorder record 05-detailed`.
3. To record a clip again, stop the site, restore the snapshot it starts from (including `state.json`), and start it
   again with the environment the clip expects: without `ShowDetails` for the first two clips, with it for the third.
4. Slides need no recording. Run `narrate.py` before recording, so the recorder holds each step for its narration.

## Clip `02-enable`

**Starts on:** `/Admin/Features`, opened with `scene.OpenAsync` before `StartCaptureAsync`, signed in. Wait until
`#search-box` is visible. The admin menu may stay as it is.

`enable-1`
- Type `Health Checks` into `#search-box` (the placeholder is "Search"; `delay: 70`). The list filters as you type,
  and the line "Showing {0} of {1} features" shrinks to the matching rows (`Views/Admin/Features.cshtml:57-59`).
- Wait until `#btn-enable-OrchardCore_HealthChecks` is visible. This is the **Enable** button of the row *Health Checks*
  `OrchardCore.HealthChecks`, in the **Infrastructure** group (MVC turns the dot of the feature id into an underscore; if
  the id differs, use the **Enable** button inside the row whose name is *Health Checks*).
- Highlight that row without a caption, then clear the highlight:
  `scene.HighlightAsync(scene.Page.Locator("[data-filter-value^='Health Checks OrchardCore.HealthChecks ']").First)`,
  then `scene.ClearHighlightsAsync()`. Every row carries `data-filter-value` set to its name, id and description
  separated by spaces (`Drivers/FeatureEntryDisplayDriver.cs:20`), so this selector finds the row *Health Checks* only.
- Click **Enable** with `scene.ClickAndWaitForNavigationAsync`. It is a link that the page sends as a POST (`data-url-af`), and the page redirects to the
  Features page. Enabling a feature recycles the tenant, so the redirect can take a few seconds: if the step then runs
  longer than its narration, type faster or cut the wait with `scene.OffCameraAsync`, as `SKILL.md` describes.
- Wait until the text *The feature Health Checks was enabled.* is visible.
- On screen when the narration ends: the Features page with the green notification *The feature Health Checks was
  enabled.* at the top.
- Source: menu *Tools* > *Features* `OrchardCore.Features/AdminMenu.cs:26-27`; page title and search placeholder
  `Views/Admin/Features.cshtml:15,27`; button `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (id and label);
  group heading `Views/Admin/Features.cshtml:88`; search matches the name, id and description
  `Drivers/FeatureEntryDisplayDriver.cs:20`; message `Controllers/AdminController.cs:317`; feature name and
  category `OrchardCore.HealthChecks/Manifest.cs:4,9`.

`enable-2`
- `scene.NavigateAsync("/health/live")`. The tenant has just restarted, so this first request can take a moment;
  wait until the page text is exactly `Healthy`
  (`scene.Page.WaitForFunctionAsync("() => document.body.innerText.trim() === 'Healthy'")`).
- The page is plain text, which Chromium draws very small. Enlarge it so it reads at video size:
  `scene.Page.EvaluateAsync("() => { const pre = document.querySelector('pre'); pre.style.fontSize = '48px'; pre.style.margin = '64px'; }")`
  (if there is no `pre`, apply the same style to `document.body`).
- `scene.HighlightAsync(scene.Page.Locator("pre"), "/health/live")`.
- On screen when the narration ends: a white page with the word *Healthy* in large type, a green ring around it and the
  caption */health/live*. No error page.
- Source: route and default `OrchardCore.HealthChecks.Abstractions/HealthChecksOptions.cs:11`; the endpoint is mapped
  without a check `OrchardCore.HealthChecks/Startup.cs:54`; the empty report is *Healthy* (docs,
  `src/docs/reference/modules/HealthChecks/README.md`, "Health Checks" intro).

## Clip `05-compact`

**Starts on:** `/Admin/Features` (opened before the capture), signed in, from snapshot B. Health Checks shows as
*Enabled*, Redis as *Disabled*. Wait until `#search-box` is visible.

`fail-1`
- Type `Redis` into `#search-box` (`delay: 70`). Several rows remain (Redis, Redis Cache, Redis Bus and so on): act
  only on the row *Redis* `OrchardCore.Redis`.
- Wait until `#btn-enable-OrchardCore_Redis` is visible, highlight its row with the caption `Redis`
  (`scene.HighlightAsync(scene.Page.Locator("[data-filter-value^='Redis OrchardCore.Redis ']").First, "Redis")`: the
  space before the closing quote leaves out the rows *Redis Cache*, *Redis Bus* and *Redis Lock*), clear the
  highlight, and click the button with `scene.ClickAndWaitForNavigationAsync`.
- Wait until the text *The feature Redis was enabled.* is visible. Do not configure Redis: there is no
  `Redis` section, so the tenant logs an error and keeps running without Redis (expected).
- On screen when the narration ends: the Features page with the notification *The feature Redis was enabled.*
- Source: feature id and name `OrchardCore.Redis/Manifest.cs:11-12`; the check name `Redis Health Check`
  `OrchardCore.Redis/HealthChecks/Extensions/RedisHealthCheckExtensions.cs:13`; it is registered only with Health Checks
  enabled `OrchardCore.Redis/HealthChecks/Startup.cs:6`; no configuration gives a `NullRedisService`
  `OrchardCore.Redis/Startup.cs:43`; message `OrchardCore.Features/Controllers/AdminController.cs:317`.

`fail-2`
- `scene.NavigateAsync("/health/live")`. Wait until the page text is exactly `Unhealthy`
  (`scene.Page.WaitForFunctionAsync("() => document.body.innerText.trim() === 'Unhealthy'")`). The response status is
  `503`, and Playwright's navigation does not fail on it.
- Enlarge the text and highlight it exactly as in `enable-2`, with the caption `/health/live`.
- On screen when the narration ends: the word *Unhealthy* in large type, with the green ring and the caption
  */health/live*. If Chromium shows its own error page instead of the body, stop and read `OPEN-QUESTIONS.md`.
- Source: an unhealthy compact response is `503` (docs, "Compact response"; ASP.NET Core default mapping, used because
  `OrchardCore.HealthChecks/Startup.cs:54` passes no options); the check reports *Unhealthy*
  `OrchardCore.Redis/HealthChecks/RedisHealthCheck.cs:23`. The Blog recipe enables `OrchardCore.Diagnostics`, whose
  `DiagnosticsStartupFilter.cs:29` re-executes error status codes to `/Error/{0}`; ASP.NET Core skips that for a
  response that has a content type or has started, and the health check writes `text/plain` and a body, so the page
  must show the word, not Orchard's error page (see `OPEN-QUESTIONS.md`).

## Clip `05-detailed`

**Starts on:** `/Admin/Features` (opened before the capture), from snapshot C, with the site restarted with
`OrchardCore__HealthChecks__ShowDetails=true` (see "Order, snapshots and restarts"). Health Checks and Redis are both
*Enabled*. Before recording, open `/health/live` once outside the capture and confirm that it answers with JSON and
not with the word *Unhealthy*.

`detail-1`
- `scene.NavigateAsync("/health/live")`. Wait until the page text contains `"Status":"Unhealthy"`, with or without a
  space after the colon (`scene.Page.WaitForFunctionAsync("() => /\"Status\":\\s*\"Unhealthy\"/.test(document.body.innerText)")`).
- Enlarge the text with `fontSize = '34px'` and `margin = '48px'` (the response is a long line that wraps), and
  highlight the `pre` with the caption `ShowDetails: true`.
- On screen when the narration ends: the JSON on a white page, with `"Status":"Unhealthy"` first, then `"Duration"`,
  then `"HealthChecks"` holding one entry with `"Name":"Redis Health Check"`, `"Status":"Unhealthy"` and a
  `"Description"`. The three apostrophes of the description (*Couldn't*, and the two around *'Configuration'*) appear
  as `\u0027`: that is the response as the module writes it (the default JSON encoder escapes the apostrophe), so do
  not retouch it. If Chromium offers a *Pretty-print* checkbox, leave it as it is.
- Source: `OrchardCore.HealthChecks/Startup.cs:36-50` (the detailed route and its `200 OK` for every status);
  `OrchardCore.HealthChecks/Services/DefaultHealthCheckResponseWriter.cs:15-27` (the properties `Status`, `Duration`,
  `HealthChecks`, `Name`, `Status`, `Description`); `OrchardCore.Redis/HealthChecks/RedisHealthCheck.cs:23` (the
  description).

## Slide steps

| Step | What the slide shows |
|---|---|
| `why-1` | "Who asks whether the site is running": cards for a load balancer, a container platform and an uptime monitor, and one for the Health Checks module |
| `why-2` | "What the module registers": the endpoint and the response writer, then no check of its own and no administration pages |
| `why-3` | "The address of a tenant": three cards with `https://example.com/health/live`, the same with the prefix `customer-a`, and the host `customer-a.example.com` |
| `checks-1` | "Checks from Orchard Core features": Redis Health Check, SMS Health Check, and the rule that both features are required |
| `checks-2` | "1. Implement IHealthCheck": the `DependencyHealthCheck` class of the documentation |
| `checks-3` | "2. Register it in a startup class": the `Startup` class with `RequireFeatures`, `AddHealthChecks` and `AddCheck`, and three notes |
| `resp-1` | "The compact response": the `curl` command, the body `Healthy`, and the statuses Healthy 200 OK, Degraded 200 OK, Unhealthy 503 Service Unavailable |
| `resp-2` | "The detailed response": the JSON of the documentation, and three notes: every status returns 200 OK, read `Status`, no exception, data or tags |
| `resp-3` | "Configure the endpoint": the configuration section, three environment variables, and three notes on `Url`, `ShowDetails` and reloading the tenant or restarting the application |
| `sec-1` | "Keep the endpoint private": no authentication, detailed descriptions, restrict access |
| `sec-2` | "One endpoint, every check": every registration runs, liveness and readiness, Rate Limits |

## Checks for private data

- The only data on screen is the admin Features page of a fresh site and the endpoint's text. No content item, user
  list or setting is shown.
- The user menu and any page title may show `admin` and `Orchard Core`; nothing else about a person. The email
  `admin@example.com` is not displayed by any step.
- The password is never typed on camera: the sign-in comes from `state.json`. No `<password>` value is written in any
  file of the repository.
- The caption and the slides show only `example.com` addresses and the invented tenant `customer-a`.
- OCR the video at one frame per second and at every scene change, as `SKILL.md` describes, and search for the
  machine's user name, its host name, any path of the `App_Data` folder, and real addresses. The JSON of
  `05-detailed` has no host name; confirm it.
- Do not commit `state.json`, a copy of `App_Data`, a mask script, an MP4 or an audio file.
