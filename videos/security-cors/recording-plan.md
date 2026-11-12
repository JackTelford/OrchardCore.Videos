# Recording plan: CORS: Allow Other Sites to Call Your APIs

This plan is for the person who records the video. It holds everything needed to record each clip without deciding
anything. Selectors stand for `scene.Page.Locator(...)`. File and line references are in the Orchard Core source at
commit `92db4a83` (`src/OrchardCore.Modules/OrchardCore.Cors` is written `Cors/` below, and
`src/OrchardCore.Modules/OrchardCore.Features` is written `Features/`).

The video has three recordings (`02-enable`, `03-create`, `04-unsafe`), two slide chapters (`01-how`, `05-narrow`)
and the opening and closing cards. The recordings show the admin only. The video does not demonstrate a call from
another site, because the scene API navigates only below its base address.

## The site it needs

- A fresh Orchard Core site, built from `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout at the commit
  above, on `http://localhost:5010`, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- Provisioned by Auto Setup (see the `orchardcore-tester` skill of that repository, `references/autosetup.md`), so no
  setup or sign-in form is recorded:
  - tenant: Default, with the **Blog** recipe;
  - site name: `Orchard Core`; time zone: `America/Los_Angeles`;
  - user name `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed to the recorder
    in `VIDEO_PASSWORD`; never written in a file of this repository).
- `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls an outside service. This video uses no
  external service and needs no local stand-in.
- Features to enable before recording: none. **CORS Configuration** (`OrchardCore.Cors`) must be disabled when clip
  `02-enable` starts, because that clip enables it. The video has no `PrepareAsync`.
- Settings and seed data: none. The site has no CORS policies at the start; clip `03-create` creates the only one,
  `Partner portal`, with these fake values:

  | Field | Value |
  |---|---|
  | Policy name | `Partner portal` |
  | Set as default policy | selected |
  | Allow credentials | cleared |
  | Allow any origin | cleared |
  | Allowed origins | `https://portal.example.com` |
  | Allow any header | cleared |
  | Allowed headers | `Content-Type` |
  | Allow any method | cleared |
  | Allowed methods | `GET` |
  | Exposed headers | empty |

- The admin lists keep their defaults (the List layout, with the Buttons layout for actions). The Features page of
  clip `02-enable` lists the actions of a row as buttons.
- Sign in once outside the capture (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`), which saves
  `state.json` (git-ignored).

## Order of the clips and snapshots

The clips depend on each other, and they play in the order they are recorded:

1. `02-enable` starts from snapshot 0 (setup done, signed in, **CORS Configuration** disabled). It changes the site:
   it enables the feature, and the tenant reloads.
2. `03-create` starts from snapshot 1 (the state `02-enable` leaves). It changes the site: it saves the policy
   `Partner portal`, and the tenant reloads.
3. `04-unsafe` starts from snapshot 2 (the state `03-create` leaves). It does not change the site: the admin saves
   nothing when it shows the warning. The page that the clip ends on shows an unsaved list, so never reuse it: open
   `/Admin/Cors` again for any later take.

The slide chapters (`01-how`, `05-narrow`) and the cards (`00-intro`, `99-outro`) need no recording.

Snapshots: stop the site (by its process ID, never by image name), copy its `App_Data` folder and `state.json` aside,
and start it again.

- Snapshot 0: after Auto Setup and `Recorder setup`, before any clip.
- Snapshot 1: after recording `02-enable`.
- Snapshot 2: after recording `03-create`.

To record one clip again, restore the snapshot it starts from (snapshot 0 for `02-enable`, 1 for `03-create`,
2 for `04-unsafe`), with its `state.json`, and start the site. To record everything again, restore snapshot 0.

## Scrolling

The policy editor is taller than the 1536 by 864 viewport. To bring a part of the page into view, call this and then
`PauseAsync(800)`; the scroll is smooth, and it makes the result the same on every take:

`scene.Page.Locator(<selector>).EvaluateAsync("e => e.scrollIntoView({ behavior: 'smooth', block: 'center' })")`

Below, "scroll to X" means that call on the selector of X. Clear highlights before scrolling.

## Selectors of the policy editor

The editor is a Vue app mounted in `#corsAdmin` (`Cors/Assets/ts/cors-admin-index.ts`, the compiled script is what the
page loads). In the editor, the cards under the policy name are `#corsAdmin .card`, in this order (zero-based index):

| Index | Card | Contains |
|---|---|---|
| 0 | Details | Policy name (a text box), Set as default policy |
| 1 | Credentials | Allow credentials |
| 2 | Origins | Allow any origin, Allowed origins (a text box and Add Origin) |
| 3 | Headers | Allow any header, Allowed headers (a text box and Add Header) |
| 4 | Methods | Allow any method, Allowed methods (a text box and Add Method) |
| 5 | Exposed headers | Exposed headers (a text box and Add Header) |

(ts lines 97, 119, 135, 154, 173 and 192.) Check boxes are clicked through their labels. The ids of the check boxes are
misleading: Allow any origin is `allowed-origins`, Allow any header is `allowed-headers`, and Allow any method is
`allowed-methods`.

| Control | Selector |
|---|---|
| Set as default policy | `label[for='set-default-policy']` |
| Allow credentials | `label[for='allow-credentials']` |
| Allow any origin | `label[for='allowed-origins']` |
| Allow any header | `label[for='allowed-headers']` |
| Allow any method | `label[for='allowed-methods']` |
| Policy name text box | `#corsAdmin .card >> nth=0 >> input[type=text]` |
| Allowed origins text box, and its button | `#corsAdmin .card >> nth=2 >> input[type=text]`, `#corsAdmin .card >> nth=2 >> button:has-text('Add Origin')` |
| Allowed headers text box, and its button | `#corsAdmin .card >> nth=3 >> input[type=text]`, `#corsAdmin .card >> nth=3 >> button:has-text('Add Header')` |
| Allowed methods text box, and its button | `#corsAdmin .card >> nth=4 >> input[type=text]`, `#corsAdmin .card >> nth=4 >> button:has-text('Add Method')` |
| Save | `#corsAdmin .action-bar-bottom button.btn-primary` |
| Add a policy | `#corsAdmin button:has-text('Add a policy')` |
| Edit, in the row of a policy | `#corsAdmin ul.list-group > li >> button:has-text('Edit')` |
| Empty-list message | `#list-alert:not(.d-none)` |

Never press Enter in a text box of the editor: it sits inside the form of the page. Use the Add Origin, Add Header and
Add Method buttons. Saving submits the form of the page, so click Save with `ClickAndWaitForNavigationAsync`.

The button labels come from `{{ t.Add }} {{ optionType }}` (ts line 71) with the labels Add, Origin, Header and Method
of `Cors/Services/CorsJSLocalizer.cs` (lines 21, 39, 45 and 51).

## Notifications

Notifications are toasts (`.toast`) of the admin theme (`OrchardCore.Themes/TheAdmin/Views/Message.cshtml`). A success
message disappears 5 seconds after the page loads (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`).
A warning stays until it is closed. Wait for a message by its text, for example `text=The CORS settings have updated successfully.`

## Clips

### `02-enable` (chapter 02, Enable CORS)

Starts on: `/Admin`, the dashboard of the admin, signed in as `admin`. The capture starts here. Snapshot 0.

- `enable-1`
  - Actions: click the **Tools** entry of the admin menu to expand it; click **Features** under it, with
    `ClickAndWaitForNavigationAsync` (its link is `a[href$='/Admin/Features']`); click the search box (`#search-box`, placeholder
    `Search`) and type `CORS` with `TypeAsync`; wait until the row `[data-filter-value*='OrchardCore.Cors']` is visible and
    every row that does not match is hidden; `HighlightAsync` that row.
  - On screen at the end of the narration: the Features page, the search box holding `CORS`, a **Security** group with the
    row **CORS Configuration** (`OrchardCore.Cors`), its description `Enables configuration of CORS settings.`, a
    **Disabled** badge and the **Enable** button; the green ring around the row.
  - Labels: Tools and Features, `Features/AdminMenu.cs:26-27`; the search box, `Features/Views/Admin/Features.cshtml:27`;
    the group heading is the category `Security`, `Cors/Manifest.cs:9` and `Features/Controllers/AdminController.cs:91`;
    name and description, `Cors/Manifest.cs:4,8`; the Disabled badge, `Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml`.
- `enable-2`
  - Actions: clear the highlight; click **Enable** in that row (`[data-filter-value*='OrchardCore.Cors'] >> a:has-text('Enable')`; its
    id starts with `btn-enable-`) with `ClickAndWaitForNavigationAsync`. The tenant restarts to load the feature, so the
    response can take a few seconds. The page stays still meanwhile, which is accepted in the video. The step ends when the page and its
    message have loaded: add no pause after the click.
  - On screen at the end of the narration: the Features page, loaded afresh by the redirect that follows the action
    (`Features/Controllers/AdminController.cs:255`, `GetNextUrl` at `:287-310`): the search box is empty and the list starts at the top, so
    the row of CORS Configuration, which sits in the Security group lower down, is out of view. The success toast
    `The feature CORS Configuration was enabled.` is on screen (it disappears 5 seconds after the page loads, so the step must end within 4 seconds of the page loading). Do not type in the search box again: the
    step shows the message only.
  - Labels: Enable, `Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the message,
    `Features/Controllers/AdminController.cs:317`.
- `enable-3`
  - Actions: click **Settings** in the admin menu to expand it, then **Security**, then **Cross-Origin Resource Sharing**
    (`a[href$='/Admin/Cors']`) with `ClickAndWaitForNavigationAsync`. The entry exists only because the feature is enabled, and the
    menu is built again after the tenant restart.
  - On screen at the end of the narration: the page **CORS Settings** (its breadcrumb), with the menu path
    Settings, Security, Cross-Origin Resource Sharing open in the admin menu.
  - Labels: `Cors/AdminMenu.cs:18-20`; the breadcrumb, `Cors/Views/Admin/Index.cshtml:11`; the permission, `Cors/Permissions.cs:8`
    (shown as `Managing Cors Settings` on the role editor, `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:121`);
    the Administrator role gets it by default, `Cors/Permissions.cs:19`.
- `enable-4`
  - Actions: `HighlightAsync` on `p.alert.alert-warning` (the text `The current tenant will be reloaded when the CORS is executed.`);
    `PauseAsync(4000)`; `ClearHighlightsAsync`; `HighlightAsync` on `#list-alert:not(.d-none)`.
  - On screen at the end of the narration: the page with the **Search** box and the **Add a policy** button, and a green ring around
    the blue message `Nothing here! There are no CORS policies for the moment.`
  - Labels: `Cors/Views/Admin/Index.cshtml:13` and `:14` (the information message under it);
    `Cors/Services/CorsJSLocalizer.cs:16-17,24`.

### `03-create` (chapter 03, Create a policy)

Starts on: `/Admin/Cors` (open it before the capture), with the list empty. Snapshot 1. The capture starts here.

- `create-1`
  - Actions: click **Add a policy**; wait until the heading `New policy` (`#corsAdmin h3`) is visible; scroll to the
    Exposed headers card (index 5), `PauseAsync(800)`, so the six cards pass in view.
  - On screen at the end of the narration: the lower part of the editor, with the Exposed headers card and the
    **Save** and **Cancel** buttons. (The cards above it were on screen during the scroll.)
  - Labels: Add a policy, `Cors/Services/CorsJSLocalizer.cs:17`; New policy, `Cors/Assets/ts/cors-admin-index.ts:283`;
    the card titles, `Cors/Services/CorsJSLocalizer.cs:26,31,35,41,47,53`.
- `create-2`
  - Actions: scroll to the Details card (index 0); clear the policy name (`FillAsync("")` on the text box of card 0); type `Partner portal`
    with `TypeAsync` into it (the heading above follows what is typed); click `label[for='set-default-policy']` so the box is selected.
  - On screen at the end of the narration: the Details card with **Policy name** holding `Partner portal` and **Set as
    default policy** selected.
  - Labels: Details, Policy name, Set as default policy, `Cors/Services/CorsJSLocalizer.cs:26,28,30`.
- `create-3`
  - Actions: scroll to the Credentials card (index 1); `HighlightAsync` on its check box row for 2 seconds
    (`#corsAdmin .card >> nth=1 >> .form-check`), `ClearHighlightsAsync`; click `label[for='allow-credentials']` to clear it.
  - On screen at the end of the narration: the Credentials card with **Allow credentials** cleared and its hint
    `Allows credentials to be transported in requests. This setting is not allowed in combination with Allow Any Origin.`
    visible. (The box is selected before the click: a new policy starts with it selected, `Cors/Assets/ts/cors-admin-index.ts:290`.)
  - Labels: Credentials, Allow credentials and its hint, `Cors/Services/CorsJSLocalizer.cs:31,33,34`.
- `create-4`
  - Actions: scroll to the Origins card (index 2); `HighlightAsync` on `label[for='allowed-origins']` (Allow any origin is already cleared)
    for 2 seconds, `ClearHighlightsAsync`; type `https://portal.example.com` with `TypeAsync` into the text box of card 2; click its **Add Origin** button.
  - On screen at the end of the narration: the Origins card with **Allow any origin** cleared and the list under
    **Allowed origins** holding `https://portal.example.com` with a **Delete** button; the text box is empty.
  - Labels: Origins, Allow any origin, Allowed origins, `Cors/Services/CorsJSLocalizer.cs:35,37,40`; Add Origin, see "Selectors of the policy editor".
- `create-5`
  - Actions: scroll to the Headers card (index 3); click `label[for='allowed-headers']` (clears Allow any header); type
    `Content-Type` into the text box of card 3 and click **Add Header**; scroll to the Methods card (index 4); click
    `label[for='allowed-methods']` (clears Allow any method); type `GET` into the text box of card 4 and click **Add Method**.
  - On screen at the end of the narration: the Methods card with **Allow any method** cleared and `GET` listed under **Allowed
    methods**; the Headers card, with `Content-Type` listed, is just above it in view or scrolled off. Both
    check boxes were selected before the clicks (`Cors/Assets/ts/cors-admin-index.ts:287,289`).
  - Labels: Headers, Allow any header, Allowed headers, `Cors/Services/CorsJSLocalizer.cs:41,43,46`; Methods, Allow any
    method, Allowed methods, `:47,49,52`.
- `create-6`
  - Actions: scroll to the Exposed headers card (index 5); `HighlightAsync` on it (`#corsAdmin .card >> nth=5`).
  - On screen at the end of the narration: the Exposed headers card with the hint `Sets response header
    'Access-Control-Expose-Headers'.`, an empty list and a green ring around it.
  - Labels: Exposed headers and its hints, `Cors/Services/CorsJSLocalizer.cs:53-55`.
- `create-7`
  - Actions: `ClearHighlightsAsync`; scroll to `#corsAdmin .action-bar-bottom`; click **Save**
    (`#corsAdmin .action-bar-bottom button.btn-primary`) with `ClickAndWaitForNavigationAsync`; wait for the text
    `The CORS settings have updated successfully.`. Saving releases the tenant, so the response can take a few seconds; the page stays still meanwhile, which is accepted in the video.
  - On screen at the end of the narration: the **CORS Settings** page with the list holding one row, **Partner portal**, with
    the **Default Policy** badge and the **Edit** and **Delete** buttons. The success toast is visible a few seconds after
    the page loads and is gone by the end of the narration, which is expected.
  - Labels: Save, `Cors/Services/CorsJSLocalizer.cs:22`; the message, `Cors/Controllers/AdminController.cs:144`; Default Policy,
    Edit and Delete, `Cors/Services/CorsJSLocalizer.cs:18-20`. The tenant reload, `Cors/Controllers/AdminController.cs:142`.
  - After the clip: stop the site and keep snapshot 2.

### `04-unsafe` (chapter 04, An unsafe combination)

Starts on: `/Admin/Cors` (open it before the capture), with the list holding `Partner portal`. Snapshot 2. The capture
starts here.

- `unsafe-1`
  - Actions: click **Edit** in the row of `Partner portal`; scroll to `label[for='allow-credentials']`; click it (selects
    Allow credentials); scroll to `label[for='allowed-origins']`; click it (selects Allow any origin).
  - On screen at the end of the narration: the Credentials card and the Origins card in view together, with **Allow
    credentials** and **Allow any origin** both selected, and the hint `Allows requests from any origin. This will
    bypass any origin that is configured below.`
  - Labels: Edit, `Cors/Services/CorsJSLocalizer.cs:19`; Allow any origin and its hint, `:37-38`.
- `unsafe-2`
  - Actions: scroll to `#corsAdmin .action-bar-bottom`; click **Save** with `ClickAndWaitForNavigationAsync`; wait for the text
    `Affected policies`; `HighlightAsync` on the warning toast (the `.toast.message-warning` element).
  - On screen at the end of the narration: the **CORS Settings** page with the warning toast, which starts
    `Specifying AllowAnyOrigin and AllowCredentials is an insecure configuration` and ends with `Affected policies: Partner
    portal`, and a green ring around it. The list underneath shows `Partner portal` because the page was drawn
    from the posted values; nothing was saved.
  - Labels: the warning, `Cors/Controllers/AdminController.cs:130`; nothing is saved because the action returns before
    `UpdateSettingsAsync` (lines 132 and 140); a `*` entry counts as any origin, `:149-150`.
  - Do not click anything else: end the clip here. The site is unchanged, so snapshot 2 is still the state of the site.

## Slides and cards

- `intro`: the opening card, "CORS: Allow Other Sites to Call Your APIs" over the subtitle "Choose which sites may call your site from a browser".
- `how-1`: the slide "An origin is a scheme, a host and a port", with four cards: Scheme, Host, Port, and "Any difference is another origin".
- `how-2`: the slide "How a browser decides", with four cards: a page calls your site; the browser looks for CORS headers; a match lets the page use the response; no match, or no policy, is blocked.
- `how-3`: the slide "What CORS is not", with three cards: not a sign-in, not permissions, not a way to block tools.
- `narrow-1`: the slide "Keep each policy narrow", with four cards: only what the client needs; origins match exactly; HTTPS in production; few exposed headers.
- `narrow-2`: the slide "The default policy and the others", with two cards: one default policy; the other policies are registered by name.
- `narrow-3`: the slide "Move policies to another site", with two cards: the Deployment feature, and the Cors settings step. Sources: `Cors/Startup.cs` (`DeploymentStartup`, step name `Cors settings`), `OrchardCore.Deployment/Manifest.cs:4`.
- `narrow-4`: the slide "When a browser reports a CORS failure", with four cards: is the call cross-origin; is the feature enabled; is the policy the default; does the origin match exactly.
- `outro`: the closing card, "CORS: Allow Other Sites to Call Your APIs" over the subtitle "docs.orchardcore.net".

## Checks for private data

- The only account on screen is `admin`; the only origin typed is `https://portal.example.com`; the only policy is `Partner portal`.
  No other name, email address, phone number or token is on the site, because it is a fresh Auto Setup site.
- Look at every contact sheet (`shots/<clip>.jpg`) and OCR the frames at one frame per second and at every scene
  change. Check the first frame of `02-enable` (the dashboard) and the admin bar for any name other than `admin`.
- Confirm no password, and no `state.json`, is in `videos/security-cors/`; `git status` must list only the files of the format.
