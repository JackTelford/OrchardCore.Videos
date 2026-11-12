# Recording plan: Debugging settings

How to record the clips of `storyboard.json` without having to decide anything. Source references are to the Orchard
Core checkout the video was written from. A selector below stands for `scene.Page.Locator(...)`.

The chapters are: `01-why` (slides), `02-enable`, `03-read` and `04-disable` (recordings), `05-more` (slides). The
opening and closing cards need no recording.

## The site it needs

- A fresh site made by Auto Setup, as in `references/recording.md`: `src/OrchardCore.Cms.Web` with its own
  `ORCHARD_APP_DATA` folder outside both repositories, on `http://localhost:5010`, and outbound calls blocked with a dead
  `HTTP_PROXY` and `HTTPS_PROXY`.
- Auto Setup values (environment variables `OrchardCore__AutoSetup__Tenants__0__...`): `ShellName` Default, `SiteName`
  Orchard Core, `SiteTimeZone` America/Los_Angeles, `AdminUsername` admin, `AdminEmail` admin@example.com,
  `AdminPassword` the value of `VIDEO_PASSWORD` (a password chosen outside git, written here as `<password>`),
  `DatabaseProvider` Sqlite, `RecipeName` Blog. The video is not about the Setup screen, so it is recorded on a site
  that Auto Setup has already provisioned, signed in with the state that `Recorder setup` saves.
- Tenant: Default, with no URL prefix.
- Features to enable before recording: none. The Settings module is always enabled
  (`OrchardCore.Settings/Manifest.cs:10`), and the Blog recipe gives the site a front end and a theme. Do not enable the
  Templates feature, the Dynamic Cache feature or the Admin Bar: the page source then holds only what the Blog recipe
  renders.
- Settings: leave every setting at what the recipe saves. In particular **Write shape debug information** is unchecked
  at the start (`OrchardCore.Settings/DebugSettings.cs:5` is false by default).
- External services: none.
- The sign-in: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` writes `state.json`
  (git-ignored). The clips are recorded signed in as admin.

## Seed data

None beyond what the Blog recipe creates. Do not add content items, users or roles. The page source of the home page
shows only the recipe's own text.

## Order, dependencies and snapshots

The clips change one setting of the site, so they are recorded in this order, and each one needs the state the
previous one left.

| Clip | Needs the option | Changes the site | Leaves the option |
|---|---|---|---|
| `02-enable` | off | yes: checks **Write shape debug information** and saves | on |
| `03-read` | on | no | on |
| `04-disable` | on | yes: clears the option and saves | off |

1. Provision the site, run `Recorder setup`, stop the site, and copy its `App_Data` folder and `state.json` aside as
   **snapshot A** (option off).
2. Start the site and record `02-enable`. Stop the site and copy `App_Data` aside as **snapshot B** (option on).
3. Start the site on snapshot B and record `03-read`. Record `04-disable` right after it, on the same state.
4. To take `02-enable` again, restore snapshot A. To take `03-read` or `04-disable` again, restore snapshot B.
   For a full take, restore snapshot A and record the three clips in the order above.

`01-why` and `05-more` are slides and need no site.

## The page-source view

The recorder drives headless Chromium, which draws no page-source window. The steps of `03-read` and `04-disable`
that show the source of a page turn the document into a plain text view of its own HTML, without navigating, so the
cursor and highlight overlays of the recorder keep working. Add this helper to the video's `Scenes.cs` and call it
where the steps say `ShowSource`:

```csharp
private static Task ShowSourceAsync(Scene scene) => scene.Page.EvaluateAsync("""
    async () => {
      const text = await (await fetch(location.href, { credentials: 'omit' })).text();
      const escaped = text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
      const marked = escaped.replace(/&lt;!--shape-(?:start|end)\b[\s\S]*?--&gt;/g, m => '<span class="c">' + m + '</span>');
      document.head.innerHTML = '<meta charset="utf-8"><style>html,body{margin:0;background:#fff;color:#1f1f23}'
        + 'pre{margin:0;padding:16px 20px;font:15px/1.5 Consolas,"Courier New",monospace;white-space:pre-wrap;word-break:break-all}'
        + '.c{background:#d3f0de;border-radius:3px}</style>';
      document.body.className = '';
      document.body.removeAttribute('style');
      document.body.innerHTML = '<pre id="source"></pre>';
      document.getElementById('source').innerHTML = marked;
      window.scrollTo(0, 0);
    }
    """);
```

The request omits the cookies, so the source is the page as an anonymous visitor receives it: no admin bar and no
anti-forgery token of the signed-in session. The comments are shown on a light green background, which makes them
easy to find. After a call, wait for the `#source` element to be visible.

## Clip `02-enable`

Start page: `/Admin`, signed in as admin (open it before `StartCaptureAsync`). Before the capture, check that the
group **Settings** of the admin menu is collapsed; if it is open, click it once so it closes. The menu is part of the
story of the first step.

- `enable-1`: click the group **Settings** in the admin menu with the selector
  `#left-nav button.nav-group-label:has(.title:text-is("Settings"))` (`ClickAsync`, `pauseAfter: 700`), then click
  **Debugging** with `#left-nav a.item-label:has(.title:text-is("Debugging"))` (`ClickAndWaitForNavigationAsync`).
  When the narration ends, the page **Debugging** (`/Admin/Settings/debugging`) is open, the group **Settings** is
  expanded in the menu with **Debugging** marked as the current page, and the option is on screen. Labels:
  `OrchardCore.Settings/AdminMenu.cs:36` (Settings) and `:46` (Debugging).
- `enable-2`: `HighlightAsync` on the row `div.form-check:has(input[name$="WriteShapeDebugInformation"])`, hold about
  4 seconds, `ClearHighlightsAsync`, then `HighlightAsync` on the hint `span.hint:has-text("Wrap rendered shapes")`. When
  the narration ends, the hint is ringed, and the box **Write shape debug information** is visible and unchecked.
  Labels: `OrchardCore.Settings/Views/DebugSettings.Edit.cshtml:7` (label) and `:9` (hint).
- `enable-3`: `ClearHighlightsAsync`; `ClickAsync` on `input[name$="WriteShapeDebugInformation"]`; `PauseAsync(700)`;
  `ClickAndWaitForNavigationAsync` on `button.save` (the button **Save**); then wait until `.toast.message-success` is
  visible. When the narration ends, the page has reloaded with the box checked, and the green message **Site settings
  updated successfully.** is still on screen. If the message disappears before the narration ends, wait with
  `PauseAsync` before clicking **Save**. Labels: `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (Save)
  and `OrchardCore.Settings/Controllers/AdminController.cs:119` (the message).

## Clip `03-read`

Start page: `/` (the home page of the site), signed in as admin, opened before `StartCaptureAsync`, with the option on
(snapshot B).

- `read-1`: `ScrollAsync(0, 500)`, `PauseAsync(800)`, `ScrollAsync(0, -500)`. When the narration ends, the top of the home
  page is on screen, rendered as usual, with no sign of the comments.
- `read-2`: `ShowSource`, then wait until `#source` is visible. When the narration ends, the page source is on screen at
  its first line, with at least one comment on a light green background. Stop and read `OPEN-QUESTIONS.md` if no comment
  is tinted. Comment format: `OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:244` (the check of the
  option), `:290-295` (the start comment) and `:297-300` (the end comment).
- `read-3`: `HighlightAsync` on `#source .c:has-text("shape-start")` (`.First`) with the caption `Start comment`, and keep
  it until the narration ends. When the narration ends, the first start comment is ringed and its words are readable:
  `shape-start type:`, `bindings:`, the arrow, the source of the template, and the kind in brackets.
  `DefaultHtmlDisplay.cs:302-308` builds the part after `bindings:`, and `:310-325` the kind.
- `read-4`: `ClearHighlightsAsync`; `HighlightAsync` on the first two start comments (`#source .c:has-text("shape-start")`,
  `.Nth(0)` and `.Nth(1)`, no caption); about 6 seconds later `ClearHighlightsAsync` and `HighlightAsync` on the first end
  comment (`#source .c:has-text("shape-end")`, `.First`) with the caption `End comment`. If that end comment is more than
  one screen below the first start comment, skip the first two rings and scroll with `ScrollAsync` until it is in view
  before ringing it. When the narration ends, the first end comment is ringed, and the words `shape-end type:` are
  readable.

## Clip `04-disable`

Start page: `/Admin/Settings/debugging`, signed in as admin, opened before `StartCaptureAsync`, with the box checked
(snapshot B). The admin menu is collapsed or open as the last clip left it; either is fine.

- `off-1`: `HighlightAsync` on the checked row (`div.form-check:has(input[name$="WriteShapeDebugInformation"])`) and
  `PauseAsync(5500)` while the first sentence plays; `ClearHighlightsAsync`; `ClickAsync` on
  `input[name$="WriteShapeDebugInformation"]` to clear it; `PauseAsync(700)`; `ClickAndWaitForNavigationAsync` on
  `button.save`; wait until `.toast.message-success` is visible. When the narration ends, the page has reloaded with the
  box cleared, and the message **Site settings updated successfully.** is on screen.
- `off-2`: `NavigateAsync("/")`, then `ShowSource` and wait until `#source` is visible. When the narration ends, the
  source of the home page is on screen at its first line, and no comment has a light green background.

## Slides

- `why-1`: the slide "A page is built from shapes" with three cards, Shape, Binding and Tree, all bright.
- `why-2`: the slide "Which template drew this part?" with four cards: A module, A theme, The admin, The question.
- `why-3`: the slide "Shape debug information" with four check cards, ending with Off by default.
- `perm-1`: the slide "Who can open the Debugging page" with three cards: Manage debugging settings, Manage settings,
  Administrator role. Source: `OrchardCore.Settings.Core/SettingsPermissions.cs:13-18` and
  `OrchardCore.Settings/Permissions.cs` (the Administrator role gets all three by default).
- `rel-1`: the slide "Resource Debug Mode" with four cards: On the General page, From environment, Enabled, Disabled.
  Labels: `OrchardCore.Settings/Views/Settings-Resources.Edit.cshtml:45-51`.

## Checks for private data

- The only account on screen is `admin` with `admin@example.com`; no other person, email address or phone number
  appears. The password never appears: it is typed outside the capture (`Recorder setup`).
- Read the contact sheet of `03-read` and `04-disable`: the page source must show only the Blog recipe's fake text and
  the comments, with no absolute path of this machine, no cookie, no token and no real host name. The comments name
  templates with paths that start with the module or theme name, and code bindings with a type name; none of them is an
  absolute path. If one is, stop and report it.
- OCR the video at one frame per second and at every scene change, as the skill describes, and search for real names,
  email addresses and tokens before publishing.
