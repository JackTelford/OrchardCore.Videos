# Open questions: Health Checks

What the author verified in the source, and what a person must confirm on a running site before or while recording.
Nothing below is stated in the narration as a fact that the source does not support.

## 1. Spoken spellings (before narrating)

`storyboard.json` has a `spoken` map with eight entries. A person confirms each one with `scripts/pronounce.py`
before the narration is generated:

| Written | Spoken as | Why |
|---|---|---|
| `/health/live` | slash health slash live | the route, in `enable-2` and `fail-2` |
| `IHealthCheck` | I Health Check | `checks-2` |
| `CheckHealthAsync` | Check Health Async | `checks-2` |
| `AddHealthChecks` | Add Health Checks | `checks-3` |
| `AddCheck` | Add Check | `checks-3` |
| `RequireFeatures` | Require Features | `checks-3` |
| `ShowDetails` | Show Details | `resp-2`, `resp-3`, `detail-1`, `sec-1` |
| `Url` | U.R.L. | `resp-3` ("The Url setting is the route") |

Also listen for how the voice says "Redis" and "Twilio" (no entry yet).

## 2. Behavior the narration states that the Orchard Core source does not show

- **Status codes.** The narration says Healthy and Degraded return 200 OK and Unhealthy returns 503 Service Unavailable
  in the compact response, and that the compact body is the plain word of the status. This is the ASP.NET Core default
  that `MapHealthChecks(url)` uses (`OrchardCore.HealthChecks/Startup.cs:54` passes no options) and what the module's
  documentation states. Confirm outside the capture, with the endpoint of the recording site in both modes:
  `curl --include http://localhost:5010/health/live` (status line, `Healthy` or `Unhealthy`, and, in detailed mode, JSON
  with status `200`). The recording cannot show a status code, since the capture has no address bar or network panel.
- **A tenant without a check reports Healthy.** Taken from the documentation and from the empty report of
  ASP.NET Core. In the Orchard Core source only `OrchardCore.HealthChecks`, `OrchardCore.Redis` and `OrchardCore.Sms`
  call `AddHealthChecks` or `AddCheck`, and the Blog recipe (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`)
  enables none of the three, nor a feature that depends on SMS or Redis. `enable-2` confirms it on camera: if the page
  shows anything else, the step and `why-2` are wrong.

## 3. What the recording depends on

- **The Enable button.** The plan uses `#btn-enable-OrchardCore_HealthChecks` and `#btn-enable-OrchardCore_Redis`. The
  view builds the id with `Html.GenerateIdFromName(featureId)` (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`),
  which MVC writes with an underscore in place of each dot. Confirm the ids on the page. Confirm also that searching
  for `Health Checks` leaves the row *Health Checks* (the search looks in the name, the id and the description,
  `Drivers/FeatureEntryDisplayDriver.cs:20`) and that no other row's description contains the phrase.
- **The endpoint right after enabling.** Enabling a feature recycles the tenant, and the endpoint is mapped when the
  tenant's pipeline is built (`Startup.cs`, method `Configure`). Confirm that the first request to `/health/live`
  after the redirect answers `Healthy` and not `404 Not Found`, and how long it takes.
- **A 503 response in Chromium.** The narration of `fail-2` assumes that Chromium draws the body `Unhealthy` of a `503`
  response. Chromium normally draws its own error page only when the body is empty; confirm that it draws the body
  here. If it draws an error page, `fail-2` needs another way to show the result (for example the detailed response
  first), and the narration of `fail-2` and `detail-1` changes with it.
- **The error pages of the Diagnostics feature.** The Blog recipe enables `OrchardCore.Diagnostics`, whose
  `DiagnosticsStartupFilter.cs:24` calls `UseStatusCodePagesWithReExecute("/Error/{0}")`. The ASP.NET Core middleware
  behind it leaves a response alone when it has started or has a content type, and the health check writes
  `text/plain` (or `application/json`) and a body, so the `503` of `fail-2` should show the word `Unhealthy` and not
  an Orchard error page. This is the behavior of ASP.NET Core, not of the Orchard Core source; confirm it on the
  recording site, together with the previous item.
- **A plain-text and a JSON page.** The plan enlarges the text with `document.querySelector('pre')`, which assumes that
  Chromium wraps a plain-text or JSON response in a `pre` element. Confirm the element, and the look of a JSON response
  in the recorder's Chromium (a "Pretty-print" checkbox, or a raw line). Confirm that `HighlightAsync` draws the ring
  and the caption correctly on these pages, whose background is white.
- **The escaped apostrophes.** The detailed response is serialized with `JsonSerializer.Serialize(response)` and no
  options (`OrchardCore.HealthChecks/Services/DefaultHealthCheckResponseWriter.cs:27`), whose default encoder writes an
  apostrophe as `\u0027`. The description of the Redis check contains three (*Couldn't*, and the two around
  *'Configuration'*, `RedisHealthCheck.cs:23`). Confirm what the page shows, and that the escapes do not distract from the narration of `detail-1`, which does not read them.
- **Redis without a configuration.** The source says that without a `Redis` section the tenant logs an error, keeps
  running, and registers `NullRedisService` (`OrchardCore.Redis/Startup.cs:35-44`), so `Redis Health Check` returns
  *Unhealthy* with the description about the `Configuration` string (`RedisHealthCheck.cs:23`). Confirm on the
  recording site that enabling the feature shows no error page, and that no Redis server is needed.
- **The SMS check.** `checks-1` describes it from the source only (`OrchardCore.Sms/HealthChecks/SmsHealthCheck.cs`:
  it sends a request with the configured Twilio credentials to the Twilio service). It is not enabled or shown in any
  clip; the recording site must keep the SMS feature disabled so that nothing calls out.
- **Rate Limits.** `sec-2` says that an endpoint policy of the Rate Limits module can limit repeated requests to the
  path. This is from the documentation of the Health Checks module and from the Rate Limits documentation, which says
  that an endpoint policy matches the requests whose path starts with the configured one. The source agrees: the
  policy matches with `Request.Path.StartsWithSegments(policy.Path)`
  (`OrchardCore.RateLimits/RateLimiterOptionsConfigurations.cs:154`), and the feature calls `UseRateLimiter()` in its
  `Configure` method (`OrchardCore.RateLimits/Startup.cs:54`). It was not tried on a site.
- **Tenant addresses and tenant-specific settings.** `why-3` (host name and URL prefix) and `resp-3` (a tenant name in
  the key of an environment variable) come from the documentation of the module and of the configuration. The source
  agrees on the key (`src/OrchardCore/OrchardCore/Shell/ShellSettingsManager.cs:65,115` adds the section named for the
  tenant to the `OrchardCore` section), but no clip uses a second tenant, a prefix or a host name, so none of it was tried on a site.
