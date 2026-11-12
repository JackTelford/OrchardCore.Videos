# Recording plan: Meta Pixel

This plan is for the person who records the video. It needs no decision: every site setting, value to type, click and
wait is written here. A selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators.
`Orchard Core source` below means a checkout of `OrchardCMS/OrchardCore` at the commit the labels were read from
(`92db4a83`); the paths are relative to its `src/OrchardCore.Modules/` folder, except where the folder is named.

The video has eight slide steps that need no recording (`why-1`, `why-2`, `page-1`, `page-2`, `wf-1`, `wf-2`, `wf-3`,
`wf-4`) and three recordings: `02-enable`, `03-pixel` and `05-capi`.

## 1. The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an Orchard Core checkout, run on `http://localhost:5010` (`VIDEO_BASE_URL`), as `references/recording.md` of the skill describes |
| App data | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup, so no setup or sign-in form is recorded (the `orchardcore-tester` skill of the Orchard Core repository describes it): `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName="Orchard Core"`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` |
| Password | `<password>` is chosen outside git and passed in `VIDEO_PASSWORD`; never write it in this repository |
| Tenant | `Default` only |
| Culture | `en-US`, light color scheme, as the recorder sets them |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (for example `http://127.0.0.1:9`) for the site process, so the site never calls `graph.facebook.com`. None of the three recordings makes the site call it anyway: no workflow runs and no event is sent |
| The recorder's browser | Never lets a page of the site reach Meta. The pixel script comes from a Meta host, and the proxy variables above only cover the site process. Add these two lines to the scene of every clip, before `OpenAsync`: `await scene.Page.RouteAsync("**/connect.facebook.net/**", route => route.AbortAsync());` and `await scene.Page.RouteAsync("**/*.facebook.com/**", route => route.AbortAsync());` |
| Features to enable before recording | None. The Blog recipe enables none of the Meta features. `Meta Pixel` must be **disabled** when `02-enable` starts: that clip enables it. Leave `Workflows` disabled too; nothing in the video opens it |
| Settings to save before recording | None. The Meta Pixel settings must be empty when `03-pixel` starts |
| External services and stand-ins | None needed: nothing in the three recordings contacts Meta |
| Seed data | None. The pages of the Blog recipe are enough, and no recording opens one |
| `PrepareAsync` | Not needed; there is nothing to seed |
| Sign-in | Sign in once outside the capture (`Recorder setup`), so `state.json` (git-ignored) holds the session |

The fake values typed during the recordings:

| Field | Value | Notes |
|---|---|---|
| Pixel Identifier | `123456789012345` | A made-up value, not the identifier of a real pixel |
| Conversions API Access Token | `example-access-token-0000` | A made-up value. The field is a password field: the recording shows dots, never the text |
| Test Event Code | `TEST12345` | The example value of the Orchard Core documentation |

## 2. The order of the clips

Record in the order the video plays: `02-enable`, then `03-pixel`, then `05-capi`. Each one starts from the state the
previous one left:

| Clip | Needs | Leaves |
|---|---|---|
| `02-enable` | A fresh site, `Meta Pixel` disabled | `Meta Pixel` enabled |
| `03-pixel` | `Meta Pixel` enabled, settings empty | The Pixel Identifier saved |
| `05-capi` | The Pixel Identifier saved | The Conversions API Access Token and the Test Event Code saved |

All three change the site. Take a snapshot (stop the site, copy `App_Data` and `state.json` aside) at these points:

1. `S0`: the fresh site, signed in, before `02-enable`.
2. `S1`: after `02-enable`.
3. `S2`: after `03-pixel`.

To record a clip again, stop the site, restore the snapshot taken before it (`S0` for `02-enable`, `S1` for
`03-pixel`, `S2` for `05-capi`), and start the site again. The slide steps need no recording: `slides.py` draws them.

## 3. 02-enable

Starts on `/Admin/Contents/ContentItems` (opened before the capture with `OpenAsync`). Leave the admin menu as it
loads: collapsed. Opening it is part of the story.

Labels used in this clip, with their source:

| On screen | Source |
|---|---|
| `Tools`, `Features` (menu) | `OrchardCore.Features/AdminMenu.cs:26-27` |
| `Search` (placeholder of the search box `#search-box`) | `OrchardCore.Features/Views/Admin/Features.cshtml:27` |
| `Showing {0} of {1} features` | `OrchardCore.Features/Views/Admin/Features.cshtml:59` |
| `Meta` (the category heading) | `OrchardCore.Facebook/Manifest.cs:48`, shown by `OrchardCore.Features/Views/Admin/Features.cshtml:88` |
| `Meta Pixel` and its description, `Provides a way to enable Meta Pixel tracking for your site.` | `OrchardCore.Facebook/Manifest.cs:47-49`, shown by `FeatureEntry.Fields.SummaryAdmin.cshtml:11` and `FeatureEntry.Description.SummaryAdmin.cshtml:8` |
| `Enable` (button of the row) | `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` |
| `The feature Meta Pixel was enabled.` (a toast that hides after 5 seconds) | `OrchardCore.Features/Controllers/AdminController.cs:317`, `OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71` |

- `enable-1`: Click the `Tools` button of the left menu (`scene.Page.GetByRole(AriaRole.Button, new() { Name = "Tools", Exact = true })`), then click the `Features` link under it with `ClickAndWaitForNavigationAsync` (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "Features", Exact = true })`), during the first sentence. Wait until `#search-box` is visible. When the second sentence starts, click `#search-box` and type `Meta Pixel` with `TypeAsync` (the page filters its rows while you type, so no Enter is needed). When the third sentence starts, highlight the `Meta` heading that is left on the filtered page (`HighlightAsync` on `scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Meta", Exact = true })`). The search comes before the highlight on purpose: the unfiltered page lists every category in turn, and the `Meta` heading is far below the first screen. At the end of the narration the screen shows the `Features` page, `Meta Pixel` in the search box, `Showing 1 of ... features` above the list, the ringed `Meta` heading, and the `Meta Pixel` row with its `Enable` button and a `Disabled` badge.
- `enable-2`: Highlight the description of the `Meta Pixel` row (the text `Provides a way to enable Meta Pixel tracking for your site.`) for the first sentence, then clear the highlight and move the cursor over the row's `Enable` button with `MoveToAsync` (`scene.Page.Locator("#btn-enable-OrchardCore_Facebook_Pixel")`: the link's id is `btn-enable-` followed by the feature id with its dots turned into underscores, `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; use the id, because the `Bulk Actions` menu of the page holds another link named `Enable`; `Recorder probe` confirms the id). About 2.5 seconds before this step's narration ends (the length is in `durations.json`), click it with `ClickAndWaitForNavigationAsync`. The page reloads, and the toast of `enable-3` must still be on screen while `enable-3` reads its first sentence, because it hides itself 5 seconds after the page loads: adjust the moment of the click on the first take until it does. At the end of the narration the screen shows the `Features` page with the toast `The feature Meta Pixel was enabled.`.
- `enable-3`: The toast is already showing; hold on it for the first sentence. Then click the `Settings` button of the left menu, then the `Integrations` button under it (both only expand the menu; neither loads a page), so the `Meta Pixel` link appears under `Integrations`, and highlight that link (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "Meta Pixel", Exact = true })`). At the end of the narration the screen shows the expanded menu `Settings`, `Integrations`, `Meta Pixel` with the ring around `Meta Pixel`. The toast has hidden by then; the `Meta Pixel` row further down the page shows `Enabled` and a `Disable` button. Source of the menu labels: `OrchardCore.Facebook/AdminMenuPixel.cs:26-28`.

## 4. 03-pixel

Starts on `/Admin/Contents/ContentItems` (opened before the capture). The left menu is collapsed.

Labels used in this clip, with their source:

| On screen | Source |
|---|---|
| `Settings`, `Integrations`, `Meta Pixel` (menu) | `OrchardCore.Facebook/AdminMenuPixel.cs:26-28` |
| `Pixel Identifier` (label) | `OrchardCore.Facebook/Views/FacebookPixelSettings.Edit.cshtml:15` |
| `The Pixel ID found on the Meta Data Sources portal.` (hint) | `OrchardCore.Facebook/Views/FacebookPixelSettings.Edit.cshtml:19` |
| `Save` | `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` |
| `Site settings updated successfully.` (a toast that hides after 5 seconds) | `OrchardCore.Settings/Controllers/AdminController.cs:119` |

The page is `/Admin/Settings/facebook-pixel` (`OrchardCore.Settings/Controllers/AdminController.cs:50`, and
`FacebookConstants.PixelSettingsGroupId` in `OrchardCore.Facebook/FacebookConstants.cs:11`).

- `pixel-1`: Click the `Settings` button of the left menu, then the `Integrations` button, then click the `Meta Pixel` link with `ClickAndWaitForNavigationAsync`. Wait until the `Pixel Identifier` field is visible (`scene.Page.GetByLabel("Pixel Identifier", new() { Exact = true })`). Highlight the field and the hint under it. At the end of the narration the screen shows the page with an empty `Pixel Identifier` field, the hint `The Pixel ID found on the Meta Data Sources portal.` under it, and the `Conversions API` section below, still unfilled.
- `pixel-2`: Clear the highlight. Type `123456789012345` into `Pixel Identifier` with `TypeAsync` (delay 70 ms), then press Tab. At the end of the narration the field shows `123456789012345`.
- `pixel-3`: Move to the `Save` button (`scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true })`) and click it with `ClickAndWaitForNavigationAsync`, at the start of the step. The page loads again with the toast `Site settings updated successfully.`, which shows for 5 seconds: the narration reads it in its second sentence. At the end of the narration the screen shows the page with `123456789012345` in the field; the toast may have hidden already.

## 5. 05-capi

Starts on `/Admin/Settings/facebook-pixel` (opened before the capture). The `Pixel Identifier` field already shows
`123456789012345`; the token and the test code fields are empty.

Labels used in this clip, with their source (all in `OrchardCore.Facebook/Views/FacebookPixelSettings.Edit.cshtml`):

| On screen | Line |
|---|---|
| `Conversions API` (heading) | 25 |
| `Optional server-side event delivery to Meta, in addition to the browser-side pixel. Configure this to let workflows send events (e.g. Purchase, Lead) directly from the server.` (hint under the heading) | 26 |
| `Conversions API Access Token` (label; a password field, line 33) | 31 |
| `The access token generated for this dataset in the Events Manager's Conversions API settings.` (hint) | 35 |
| `Test Event Code` (label) | 40 |
| `Optional. Found on the Events Manager's Test Events tab. When set, every Conversions API event includes it, so events show up in the test console instead of counting as production data. Remove it before going live.` (hint) | 44 |

`Save` and its toast are the ones of `03-pixel`.

- `capi-1`: Highlight the `Conversions API` heading and the hint under it (`scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Conversions API", Exact = true })`). At the end of the narration the screen shows the heading and its hint, with the `Conversions API Access Token` and `Test Event Code` fields below, both empty.
- `capi-2`: Clear the highlight. Type `example-access-token-0000` into `Conversions API Access Token` with `TypeAsync` (`scene.Page.GetByLabel("Conversions API Access Token", new() { Exact = true })`, delay 40 ms), then press Tab. Highlight the field. At the end of the narration the field shows dots, not the text, and the hint `The access token generated for this dataset in the Events Manager's Conversions API settings.` is visible under it.
- `capi-3`: Clear the highlight. Type `TEST12345` into `Test Event Code` (`scene.Page.GetByLabel("Test Event Code", new() { Exact = true })`, delay 70 ms), then press Tab. Highlight the field. At the end of the narration the field shows `TEST12345` and its hint, which ends with `Remove it before going live.`, is visible.
- `capi-4`: Keep the highlight on `Test Event Code` for the first sentence. Clear it, move to `Save`, and click it with `ClickAndWaitForNavigationAsync` when the second sentence starts. At the end of the narration the screen shows the page after saving: `123456789012345` in `Pixel Identifier`, dots in the token field, `TEST12345` in `Test Event Code`, and the toast `Site settings updated successfully.` if it has not hidden yet.

## 6. The slide steps

Each line says what `slides.py` draws; nothing is recorded for them.

- `why-1`: heading `Two ways to report to Meta`; three cards, the first two bright: `The pixel`, `The Conversions API`.
- `why-2`: the same heading and cards, all three bright, the third being `The Pixel Identifier`.
- `page-1`: heading `What the pixel does on your pages`; four cards, the first two bright: `Front end only`, `A page view per page`.
- `page-2`: the same heading and cards, the last two bright: `Nothing without an identifier`, `Tracking not allowed`.
- `wf-1`: heading `Where to find the task`; three cards, all bright: `The Workflows feature`, `Design, Workflows`, `The Meta category`.
- `wf-2`: heading `Meta Conversions API Event`; four cards, the first two bright: `Event Name`, `Action Source`.
- `wf-3`: the same heading and cards, the last two bright: `Event Source URL`, `Event ID`.
- `wf-4`: heading `Two outcomes`; two cards: `Done` (a check mark) and `Failed`.

Where the slide words come from:

| Slide text | Source |
|---|---|
| `Event Name`, `Action Source`, `Event Source URL`, `Event ID` and their hints | `OrchardCore.Facebook/Views/Items/MetaConversionsApiEventTask.Fields.Edit.cshtml:7, 27, 35, 44` |
| `Website` is the default of `Action Source` | `OrchardCore.Facebook/Activities/MetaConversionsApiEventTask.cs:60` |
| `Meta Conversions API Event` (the card of the task) and its description | `OrchardCore.Facebook/Views/Items/MetaConversionsApiEventTask.Fields.Thumbnail.cshtml:2-4` |
| The category `Meta` | `OrchardCore.Facebook/FacebookLocalizationSources.cs`, used by `Activities/MetaConversionsApiEventTask.cs:40` |
| `Done` and `Failed` | `OrchardCore.Facebook/Activities/MetaConversionsApiEventTask.cs:71` |
| `Done` when the request succeeded (an HTTP success status); `Failed` when the name or the Pixel Identifier is missing, when no token was ever saved, when the token cannot be decrypted, or when Meta does not accept the request. A token field saved empty through the settings page is stored as an encrypted empty text, so that case is not caught by the check for a missing token: the request goes out and ends in `Failed` only if Meta rejects it (see `OPEN-QUESTIONS.md`) | `OrchardCore.Facebook/Services/MetaConversionsApiService.cs:45-123`, `Drivers/FacebookPixelSettingsDisplayDriver.cs:87-91`, `Activities/MetaConversionsApiEventTask.cs:94-99` |
| `Design`, `Workflows` | `OrchardCore.Workflows/AdminMenu.cs:18-19` |
| The pixel is added to front-end pages only, once per page, with a page view; nothing without a Pixel Identifier; nothing when tracking is not allowed | `OrchardCore.Facebook/Filters/FacebookPixelFilter.cs:19, 34-44` |

## 7. Checks after recording

Run these outside the capture.

1. After `03-pixel`, the home page holds the pixel code with the fake identifier. Use `curl`, not the browser, so no
   page loads the pixel script (run it from a shell without proxy variables):
   `curl -s http://localhost:5010/ | grep -c "fbq('init', '123456789012345')"` must print `1`.
   The code is added to the head of a page when the theme renders its head scripts, which the Blog theme does.
2. After `02-enable`, `/Admin/Settings/facebook-pixel` opens for the `admin` user with only `Meta Pixel` enabled (that
   is, `Meta Core Components` is still disabled) and shows the `Pixel Identifier` field. If it answers 404 or 403,
   stop and read `OPEN-QUESTIONS.md`.
3. Nothing in the recordings contacts Meta: the site log shows no request to `graph.facebook.com`, and the browser
   made no request to `connect.facebook.net` that was not aborted.

### Checks for private data

- Only the fake values of section 1 are typed. Search the contact sheets (`shots/<clip>.jpg`) and an OCR of the video
  for the real Pixel Identifier, access token or test code of any Meta account you use elsewhere: there must be none.
- The access token is never visible as text: its field is a password field, and the video shows dots. Confirm on
  every frame after `capi-2`.
- The only account on screen is `admin` (the user menu), with the email `admin@example.com` if it shows. The
  password never appears: the session comes from `state.json`.
- No real name, email, domain or phone number appears. The Blog recipe's sample content is not opened in any clip.
- No `state.json`, `App_Data` copy, mask script, audio or MP4 is committed.
