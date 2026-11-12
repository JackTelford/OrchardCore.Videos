# Recording plan: github-login

The video shows the admin screens of the GitHub Authentication feature, from enabling it to the GitHub button on the
login page. GitHub itself is never opened: the round trip needs a real GitHub app, so the video stops before the
redirect. Read this file from top to bottom; it holds every decision.

In this file a selector stands for `scene.Page.Locator(...)` (the recorder's methods take locators), and
`file:line` points into the Orchard Core source (`src/OrchardCore.Modules/...`) the label or behavior comes from.
Narrate first (`narrate.py`): the recorder holds each step for the length of its narration.

## The site it needs

| What | Value |
|---|---|
| Host | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). The host registers the Auto Setup feature, as the `orchardcore-tester` skill of that repository describes. |
| Auto Setup | Environment variables: `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog`. `<password>` is the value of `VIDEO_PASSWORD`, chosen outside git. |
| Tenant | The default tenant only. No tenant prefix. |
| Recorder sign-in | `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`; `Recorder setup` saves `state.json` (git-ignored). |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (for example `http://127.0.0.1:9`), so the site never reaches GitHub. |
| External service | GitHub. There is no stand-in: no clip reaches it, and no clip clicks the GitHub button. |
| Features before recording | Only what the Blog recipe enables (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, which includes Users, Settings, Features and Security). **GitHub Authentication and External Authentication are disabled. Secrets is disabled**, so the Client secret field is a plain password box (the Secrets module would add a drop-down in front of it: `OrchardCore.Secrets/Views/Shared/SecretInput.cshtml:26-27`). |
| Settings before recording | No GitHub settings saved. |
| Seed data | None to create outside the capture. The values typed on camera are made up: Client ID `example-client-id`, Client secret `example-client-secret`. |
| Not used | No Auto Setup exception: this is not a Setup screen video, so the site is installed by Auto Setup and `state.json` is used by the admin clips. |

Record at the recorder's default viewport (1536x864 CSS pixels, scale 1.25, light color scheme, `en-US`). Open every
page with `scene.OpenAsync(...)` before `StartCaptureAsync()`, so no loading is recorded.

## Order of the clips, and what each one changes

Record the clips in this order. Each one starts from the state the previous one left, so a snapshot of `App_Data` is
taken after each clip that changes the site. To record a clip again, stop the site, restore the snapshot named in
"Starts from", and start the site again.

| Order | Clip | Changes the site? | Starts from | Leaves |
|---|---|---|---|---|
| 1 | `03-enable` | Yes: enables GitHub Authentication, and External Authentication with it | `snapshot-0` | `snapshot-1` |
| 2 | `04-configure` | Yes: saves the GitHub settings, which restarts the tenant | `snapshot-1` | `snapshot-2` |
| 3 | `05-login` | No | `snapshot-2` | unchanged |

- `snapshot-0`: the fresh site after Auto Setup and `Recorder setup`, with the site stopped. Copy its `App_Data` and
  `state.json` aside.
- `snapshot-1` and `snapshot-2`: copy `App_Data` aside after clips 1 and 2, with the site stopped.
- Only `05-login` runs signed out. It does not use `state.json`: start its scene with a state path that does not
  exist, for example `Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Scenes.Root, "signed-out.json"), Path.GetDirectoryName(directory), durations)`.
  `Scene.StartAsync` loads a state file only when it exists (`Scene.cs`).
- The slide steps (`01-how`, `02-register`, `06-return`) are drawn by `slides.py`; nothing is recorded for them.
- To record `04-configure` alone and `snapshot-1` is missing: restore `snapshot-0`, open **Tools, Features**, search
  for `github`, select **Enable** on the **GitHub Authentication** row, and take the snapshot.

## Clip `03-enable`

Starts on: `/Admin` (`scene.OpenAsync("/Admin")`, the dashboard), signed in as admin, the admin menu collapsed (do not
expand anything before `StartCaptureAsync()`: the narration of `enable-1` says where Features is, so opening the menu
is part of the story). GitHub Authentication is disabled.

Helpers used by the steps:
`Menu(text) = scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = text, Exact = true })`
(a menu group with children and no page of its own is a button:
`TheAdmin/Views/NavigationItemLink-admin.cshtml:24,29`; **Features** under it is a link),
`Row() = scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator("#btn-enable-OrchardCore_GitHub_Authentication, #btn-disable-OrchardCore_GitHub_Authentication") }).First`
(the button id is the feature id with dots turned into underscores:
`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40,53`).

- `enable-1`: open Tools, Features, then type `github` in the search box.
  1. `await scene.ClickAsync(Menu("Tools"), pauseAfter: 500);`
  2. `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav").GetByRole(AriaRole.Link, new() { Name = "Features", Exact = true }));`
  3. `await scene.TypeAsync(scene.Page.Locator("#search-box"), "github", delay: 90);`
  - If **Tools** is a link and not a button in this build of the admin, locate it with
    `scene.Page.Locator("#left-nav").GetByText("Tools", new() { Exact = true })`. If the path cannot be clicked at all,
    use `await scene.NavigateAsync("/Admin/Features");` (`OrchardCore.Features/Controllers/AdminController.cs:57`).
  - On screen when the narration ends: the Features page with `github` in the search box, a group headed **GitHub**,
    and its row **GitHub Authentication** (the id `OrchardCore.GitHub.Authentication` beside the name, the text
    "Authenticates users with their GitHub Account.", the **Disabled** badge, the **Enable** button); the Tools group
    open in the admin menu.
  - Labels: Tools, Features `OrchardCore.Features/AdminMenu.cs:26-27`; search box `OrchardCore.Features/Views/Admin/Features.cshtml:27`;
    name, category and description `OrchardCore.GitHub/Manifest.cs:12-21`; the id beside the name
    `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:20`; badge `OrchardCore.Features/Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`.
    The search matches the name, the id and the description of a row (`Drivers/FeatureEntryDisplayDriver.cs:20`).
- `enable-2`: point at the dependency badge, then select Enable.
  1. `await scene.MoveToAsync(Row().Locator(".direct-dependencies .badge"));`
     `await scene.HighlightAsync(Row().Locator(".direct-dependencies .badge"), "Dependency");` and wait with
     `await scene.PauseAsync(1800);`. The badge reads **External Authentication**.
  2. `await scene.ClearHighlightsAsync();`
  3. `await scene.ClickAndWaitForNavigationAsync(Row().Locator("#btn-enable-OrchardCore_GitHub_Authentication"));`
  - On screen when the narration ends: the click ripple on **Enable**; the page may already be reloading. The reload
    (the tenant restarts because a feature changed) can spill into the start of `enable-3`; that is expected.
  - Labels: badge `OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:11-17` (name of the
    dependency from `OrchardCore.Users/Manifest.cs:23-24`); button `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`.
- `enable-3`: after the reload the search box is empty. Wait until the page shows the Disable button, then search again.
  1. `await Row().Locator("#btn-disable-OrchardCore_GitHub_Authentication").WaitForAsync();`
  2. `await scene.TypeAsync(scene.Page.Locator("#search-box"), "github", delay: 60);`
  3. `await scene.HighlightAsync(Row());`
  - On screen when the narration ends: the **GitHub Authentication** row with the **Enabled** badge and a red
    **Disable** button where **Enable** was, ringed in green. A notification about the enabled feature may show at
    the top of the page; it is not read in the narration.
  - Labels: `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`; `FeatureEntry.Buttons.SummaryAdmin.cshtml:53`.
- End of the clip: `await scene.ClearHighlightsAsync();`. Stop the site and take `snapshot-1`.

## Clip `04-configure`

Starts on: `/Admin` (the dashboard), signed in as admin, the admin menu collapsed (do not expand anything before
`StartCaptureAsync()`: opening the menu is part of the story). GitHub Authentication is enabled; no GitHub settings
are saved.

Helper: `Menu(text) = scene.Page.Locator("#left-nav").GetByRole(AriaRole.Button, new() { Name = text, Exact = true })`.
The three groups are buttons because they have children and no page of their own
(`TheAdmin/Views/NavigationItemLink-admin.cshtml:24,29`); **GitHub** is a link.

- `config-1`: open the menu path, ending on the GitHub page.
  1. `await scene.ClickAsync(Menu("Settings"), pauseAfter: 500);`
  2. `await scene.ClickAsync(Menu("Security"), pauseAfter: 500);`
  3. `await scene.ClickAsync(Menu("Authentication"), pauseAfter: 500);`
  4. `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a").Filter(new() { HasTextString = "GitHub" }).First);`
  - If a group is a link and not a button in this build of the admin, locate it with
    `scene.Page.Locator("#left-nav").GetByText(text, new() { Exact = true })` instead. If the path cannot be clicked
    at all, use `await scene.NavigateAsync("/Admin/Settings/OrchardCore.GitHub.Authentication");` (the address of the
    page: `OrchardCore.Settings/Controllers/AdminController.cs:50`, group id `OrchardCore.GitHub/GithubConstants.cs:7`).
  - On screen when the narration ends: the GitHub settings page, with the yellow notice **The website might be
    restarted upon saving the settings, potentially leading to temporary unresponsiveness during the process.** at
    the top, the heading **GitHub Authentication Settings**, and the fields below it.
  - Labels: Settings, Security, Authentication, GitHub `OrchardCore.GitHub/AdminMenuGitHubLogin.cs:25-28`; notice and
    heading `OrchardCore.GitHub/Views/GithubAuthenticationSettings.Edit.cshtml:7,15`.
- `config-2`: `await scene.TypeAsync(scene.Page.GetByLabel("Client ID", new() { Exact = true }), "example-client-id", delay: 60);`
  - On screen when the narration ends: **Client ID** (marked required) holding `example-client-id`, and the hint
    under it.
  - Label: `GithubAuthenticationSettings.Edit.cshtml:20` (required marker: the class `ocat-label-required`, and
    `ViewModels/GithubAuthenticationSettingsViewModel.cs:8`).
- `config-3`: `await scene.TypeAsync(scene.Page.GetByLabel("Client secret", new() { Exact = true }), "example-client-secret", delay: 60);`
  - On screen when the narration ends: **Client secret** shows dots, not the text; its placeholder **Enter a new
    value** is gone; the hint **The secret for your app.** is under it.
  - Labels: `GithubAuthenticationSettings.Edit.cshtml:29,32`; the password box and its placeholder
    `OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:96-104` (Secrets must be disabled).
- `config-4`: do not type.
  1. `var callback = scene.Page.GetByLabel("CallbackPath", new() { Exact = true });`
  2. `await scene.MoveToAsync(callback);`
  3. `await scene.HighlightAsync(callback, "Empty: /signin-github");`
  - On screen when the narration ends: **CallbackPath** empty, showing the placeholder `/signin-github`, ringed in
    green with the caption, and its hint "The request path within the application's base path where the user-agent
    will be returned. The middleware will process this request when it arrives."
  - Labels: `GithubAuthenticationSettings.Edit.cshtml:37,39,41`. The default path: `Configuration/GitHubAuthenticationOptionsConfiguration.cs:59,74-77`.
- `config-5`: do not select the check box.
  1. `await scene.ClearHighlightsAsync();`
  2. `var tokens = scene.Page.GetByLabel("Store tokens", new() { Exact = true });`
  3. `await scene.MoveToAsync(tokens);` and `await scene.HighlightAsync(tokens);`
  - On screen when the narration ends: **Store tokens**, cleared, ringed in green, with the hint **Store external
    authentication tokens for reuse**. If the check box or the **Save** button is below the fold at 1536x864, call
    `await scene.ScrollAsync(0, 400);` right after `ClearHighlightsAsync()` (highlights are fixed-position) and
    before locating anything else.
  - Labels: `GithubAuthenticationSettings.Edit.cshtml:49,50`.
- `config-6`:
  1. `await scene.ClearHighlightsAsync();`
  2. `await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));`
  3. `await scene.OffCameraAsync(() => scene.Page.GetByPlaceholder("The value is securely stored. Enter a new value to update it, or leave it blank to keep it.").WaitForAsync());`
     (specific to the saved state: before saving, the placeholder reads **Enter a new value**; saving restarts the
     tenant, which can take a few seconds)
  4. `await scene.HighlightAsync(scene.Page.GetByLabel("Client secret", new() { Exact = true }));`
  - On screen when the narration ends: the reloaded GitHub settings page, the notification **Site settings updated
    successfully.** at the top (if it has faded by then, the narration still holds: the secret field shows the
    placeholder), **Client ID** still holding `example-client-id`, and **Client secret** empty with the placeholder
    **The value is securely stored. Enter a new value to update it, or leave it blank to keep it.**, ringed in green.
  - Labels: **Save** `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; the notification
    `OrchardCore.Settings/Controllers/AdminController.cs:119`; the placeholder `SecretInputTagHelper.cs:103`.
- End of the clip: `await scene.ClearHighlightsAsync();`. Stop the site and take `snapshot-2`.

## Clip `05-login`

Starts on: `/Login` (`scene.OpenAsync("/Login")`), **signed out** (a scene without `state.json`, see above), after the
tenant has restarted with the settings of `snapshot-2`. Wait until the page shows the **GitHub** button before
`StartCaptureAsync()`: `await scene.Page.Locator("button[name='provider'][value='GitHub']").WaitForAsync();`. The login
page uses the admin theme unless the Login settings select the site theme (`OrchardCore.Users/Services/UsersThemeSelector.cs`); leave that setting at its default.

- `login-1`:
  1. `var heading = scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Use another service to log in" });`
  2. `var section = scene.Page.Locator("section").Filter(new() { Has = heading });`
  3. `await scene.MoveToAsync(heading);` and `await scene.HighlightAsync(section);`
  - On screen when the narration ends: the login page with the **Log in** form on the left, and on the right the
    section **Use another service to log in** with one **GitHub** button, the section ringed in green.
  - Labels: `OrchardCore.Users/Views/Account/Login.cshtml:27,38,43`.
- `login-2`: do not click the button (it would leave the site for github.com).
  1. `await scene.ClearHighlightsAsync();`
  2. `var github = scene.Page.Locator("button[name='provider'][value='GitHub']");`
  3. `await scene.MoveToAsync(github);` and `await scene.HighlightAsync(github, "Sends the browser to GitHub");`
  - On screen when the narration ends: the **GitHub** button ringed in green with the caption, the page still on
    `/Login`.
  - Sources: the button is shown only when the settings hold a Client ID and a secret
    (`OrchardCore.GitHub/Configuration/AuthenticationOptionsConfiguration.cs:32-41`); the form posts the provider to
    `ExternalAuthentications/ExternalLogin` (`Login.cshtml:40`, `OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:72-84`).
    The text of the button comes from the GitHub handler package (see OPEN-QUESTIONS.md).
- End of the clip: `await scene.ClearHighlightsAsync();`.

## Slide steps

Drawn by `slides.py`; nothing to record.

- `how-1`: "Signing in with GitHub", three numbered cards, all bright: the visitor chooses GitHub, the person signs in at
  GitHub, GitHub returns to your site.
- `how-2`: "What your site needs", three cards: Client ID, Client secret, Callback path.
- `register-1`: "Register the app on GitHub", four cards, the first three bright: open the GitHub Developer
  applications, create an OAuth app, set the Authorization Callback URL (the example address on example.com).
- `register-2`: the same four cards, the fourth bright: take the Client ID and the client secret.
- `return-1`: "When GitHub sends the person back", four cards, the first two bright: a linked user is signed in, a
  matching email address asks for a password.
- `return-2`: the same four cards, the third bright: a person without an account registers.
- `return-3`: the same four cards, the fourth bright: a signed-in user can add GitHub from External Logins.

## Checks for private data

- The only name on screen is `admin`; the only email is `admin@example.com`, and only if the admin page shows it.
- The Client ID and the secret are the made-up values above; the secret is never visible (dots, then a placeholder).
- No real GitHub account, app, address or token is on screen, and no clip opens github.com. Slides use only
  example.com.
- No password appears: `<password>` is typed by the recorder from `VIDEO_PASSWORD`, outside the capture, and
  `state.json` stays out of git.
- OCR the finished video at one frame per second and at every scene change, and search the text for `@` addresses
  other than `example.com`, for machine names and for paths of this machine.
