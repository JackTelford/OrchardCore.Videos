# Recording plan: Create an Admin Theme: Restyle the Admin

Written for a person who records this video without having to decide anything. A selector written below stands for
`scene.Page.Locator(...)`, since the recorder's methods take locators. `file:line` references point to the Orchard Core
source at commit `92db4a83` (`src/` of `OrchardCMS/OrchardCore`).

The video has five slide chapters, two recorded chapters and two cards. Only two clips are recordings: `05-enable`
and `06-result`. Everything else is drawn by `slides.py` from the code in this folder, so the recorder does not show
a code editor or a terminal.

## The site it needs

- **Source:** a throwaway checkout of `OrchardCMS/OrchardCore` at commit `92db4a83` or a later `main`, and its
  `src/OrchardCore.Cms.Web` project. Never record a production site.
- **Auto Setup**, with these environment variables (no setup or sign-in form is recorded):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (the same value as `VIDEO_PASSWORD`, chosen outside git) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Data folder:** `ORCHARD_APP_DATA` points at an empty folder outside both repositories.
- **Outbound calls:** set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls a service. The
  video needs no external service and no local stand-in.
- **Tenant and time zone:** the `Default` tenant, `America/Los_Angeles`.
- **Features to enable:** none. The Blog recipe enables `OrchardCore.Themes`, `OrchardCore.Admin`,
  `OrchardCore.Settings`, `TheBlogTheme` and `TheAdmin`, and sets TheAdmin as the admin theme
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:84`).
- **Settings to save:** none. Do not save any settings page while recording.
- **Seed data:** none. The video shows the Themes page, the admin top bar and the General settings page of the
  fresh Blog site. The only extra is the theme project below.

### The theme project (built before the first start, outside the capture)

The video shows slides of this theme and then the theme itself in the admin, so the files below must match
the slides. Create the folder `src/OrchardCore.Themes/MyAdminTheme/` in the throwaway checkout. Nothing here is
committed to this repository.

1. Create the project with `dotnet new octheme` in that folder (the folder name gives the project name
   `MyAdminTheme`), then replace `MyAdminTheme.csproj` with the one below. The template's `PackageReference` lines
   would restore released packages instead of the checkout's projects.
2. **Delete `Views/Layout.liquid`.** The template adds it
   (`src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Theme/Views/Layout.liquid`), and the
   video says to delete it.
3. Replace `Manifest.cs`, and add the three other files.
4. Copy `src/OrchardCore.Themes/TheAdmin/wwwroot/Theme.png` to `wwwroot/Theme.png` of the new project, so the card of
   the theme has a thumbnail (a file of the same checkout, not a new image).
5. Add this line to the `ProjectReference` item group of `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`:
   `<ProjectReference Include="..\OrchardCore.Themes\MyAdminTheme\MyAdminTheme.csproj" />`

`MyAdminTheme.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Theme.Targets\OrchardCore.Theme.Targets.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.DisplayManagement\OrchardCore.DisplayManagement.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.ResourceManagement\OrchardCore.ResourceManagement.csproj" />
  </ItemGroup>

</Project>
```

`Manifest.cs` (the slide of `create-2` shows exactly this):

```csharp
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "MyAdminTheme",
    Author = "My name",
    Website = "https://example.com",
    Version = "0.0.1",
    Description = "My Orchard Core Admin theme.",
    Tags = new [] { "admin" },
    BaseTheme = "TheAdmin"
)]
```

`Views/_ViewImports.cshtml` (the slide of `template-1`):

```cshtml
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>

@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement
```

`Views/AdminBranding.cshtml` (the slides of `template-2`, `template-3` and `template-4`). The module
`OrchardCore.Admin` provides the original (`OrchardCore.Modules/OrchardCore.Admin/Views/AdminBranding.cshtml`), and the
favicon link of that original is not part of this version:

```cshtml
<style asp-src="~/MyAdminTheme/Styles/theme.css"
       depends-on="TheAdminLayout,bootstrap,the-admin"
       at="Head"></style>

<a class="ta-navbar-brand navbar-brand"
   asp-route-area="OrchardCore.Admin"
   asp-controller="Admin" asp-action="Index">
    <span>@Site.SiteName</span>
</a>
```

`wwwroot/Styles/theme.css` (the horizontal layout of `docs/reference/themes/admin-theme.md`; the slide of `forms-3`
shows an excerpt of it):

```css
/* Horizontal form layout for admin editors */
.ocat-wrapper {
    --ocat-gutter-x: 1.5rem;
    display: flex;
    flex-wrap: wrap;
    margin-right: calc(-0.5 * var(--ocat-gutter-x));
    margin-left: calc(-0.5 * var(--ocat-gutter-x));
    margin-bottom: 1rem;
}

.ocat-wrapper > *,
.ocat-limited-wrapper > * {
    box-sizing: border-box;
    flex-shrink: 0;
    width: 100%;
    max-width: 100%;
    padding-right: calc(var(--ocat-gutter-x) * 0.5);
    padding-left: calc(var(--ocat-gutter-x) * 0.5);
}

.ocat-label {
    flex: 0 0 auto;
    width: 25%;
    padding-top: calc(0.375rem + var(--bs-border-width));
    padding-bottom: calc(0.375rem + var(--bs-border-width));
    text-align: end;
}

.ocat-end {
    flex: 0 0 auto;
    width: 75%;
}

.ocat-end-offset {
    flex: 0 0 auto;
    width: 75%;
    margin-inline-start: 25%;
}

.ocat-start {
    flex: 0 0 auto;
    width: 25%;
}

.ocat-limited-wrapper {
    --ocat-gutter-x: 1.5rem;
    display: flex;
    flex-wrap: wrap;
    margin-right: calc(-0.5 * var(--ocat-gutter-x));
    margin-left: calc(-0.5 * var(--ocat-gutter-x));
    margin-bottom: 1rem;
}

.ocat-limited {
    flex: 0 0 auto;
    max-width: 33.333%;
    width: 33.333%;
}
```

### Build and start

```bash
cd <OrchardCore checkout>
dotnet build src/OrchardCore.Cms.Web -c Release
export ORCHARD_APP_DATA=<empty folder outside both repositories>
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9
# the Auto Setup variables of the table above, then:
dotnet run --project src/OrchardCore.Cms.Web -c Release --no-build --urls http://localhost:5010
```

Open `http://localhost:5010` once, outside the capture, so Auto Setup runs. Stop the site by its process ID, never by
image name.

### Sign in the recorder, and take the first snapshot

```bash
cd .agents/skills/orchardcore-video-producer/scripts/recorder
dotnet build -c Release -p:VideoScenes=<repo>/videos/admin-theme-create/recorder
export VIDEO_ROOT=<repo>/videos/admin-theme-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
MSYS_NO_PATHCONV=1 <bin>/Recorder setup
```

`Recorder setup` saves `state.json` (git-ignored). Then stop the site and copy the `App_Data` folder aside as
**snapshot A**: the Blog site, `MyAdminTheme` built into the web application but not enabled, TheAdmin current.

Before the first take, check the order of the stylesheets once (`OPEN-QUESTIONS.md`, question 3): start the site again,
make `MyAdminTheme` current on the Themes page, read the `link` tags in the head of an admin page, then stop the site and
restore **snapshot A**.

## Clip order, and what changes the site

| Order | Clip | Starts from | Changes the site? |
|---|---|---|---|
| 1 | `05-enable` | snapshot A | **Yes.** It enables `MyAdminTheme` and makes it the admin theme. Restore snapshot A before every new take of this clip. |
| 2 | `06-result` | the state `05-enable` leaves (snapshot **B**) | No, as long as no page is saved. |

After the first good take of `05-enable`, stop the site and copy `App_Data` aside as **snapshot B**. Record
`06-result` from snapshot B, and restore snapshot B before every new take of it. To record `05-enable` again,
restore snapshot A (the data folder and nothing else; the `state.json` stays valid because the data protection keys
are in `App_Data`).

The slide chapters (`01-how`, `02-create`, `03-template`, `04-forms`, `07-more`) and the two cards need no site.
There is no `PrepareAsync`: nothing is seeded by script.

## Clip `05-enable` (chapter 05, Make it the admin theme)

**Starts on:** `/Admin` (the admin home page), opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync()`.
The admin theme is TheAdmin. In the left menu, no section is expanded by hand.

- `enable-1`
  - **Actions:**
    1. Click the menu item with the text `Design` in `#ta-left-sidebar` (`OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs:18`).
    2. Click the item with the text `Themes` under it, with `ClickAndWaitForNavigationAsync`
       (`OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs:21`). The page is `/Admin/Themes`.
    3. Wait until `.theme-card` is visible (`OrchardCore.Modules/OrchardCore.Themes/Views/ThemeEntry.SummaryAdmin.cshtml:1`).
    4. Scroll the card whose `h4.card-title` says `MyAdminTheme` into view (use `ScrollAsync`; no highlight is active
       while scrolling), and highlight the card with `HighlightAsync(card)` where `card` is the locator
       `.theme-card` with the text `MyAdminTheme`.
  - **Visible at the end:** the Themes page; the first two cards are the current ones (`The Admin Theme` with the
    text `This is the current Admin theme`, and `The Blog Theme` with `This is the current Site theme`, in the order of
    the available features), the cards of the other themes follow; the card of `MyAdminTheme` is in view with a green ring. The left menu shows `Design` open with `Themes` selected.
  - **Labels:** `Design` and `Themes` (`OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs:18` and `:21`); the cards are
    sorted with the current themes first (`OrchardCore.Modules/OrchardCore.Themes/Controllers/AdminController.cs:92`).
- `enable-2`
  - **Actions:** `ClearHighlightsAsync()`, then highlight the tags line of the card: the `.list-group-item` of the
    card that contains `i.fa-tags`, with the caption `admin`.
  - **Visible at the end:** the card of `MyAdminTheme` showing the author `My name`, the website
    `https://example.com`, the tags line with `admin` (ringed) and the version `0.0.1`, and in its footer the buttons
    `Make Current` and `Enable`.
  - **Labels:** the card lists author, website, tags and version
    (`OrchardCore.Modules/OrchardCore.Themes/Views/Items/ThemeEntry-Attributes.SummaryAdmin.cshtml:6-35`); the buttons are
    `Make Current` (`ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7`) and `Enable`
    (`ThemeEntry-ButtonsToggleState.SummaryAdmin.cshtml:16`).
- `enable-3`
  - **Actions:** `ClearHighlightsAsync()`, `PauseAsync(2500)` so the confirmation messages appear while the
    narration says "reports that", then click the button `Make Current` inside the card of `MyAdminTheme`
    (`card.GetByRole(AriaRole.Button, new() { Name = "Make Current" })`) with `ClickAndWaitForNavigationAsync`. The
    site restarts its tenant; wait until `.toast-body` is visible (the wait is part of the step; keep the whole step
    shorter than its narration, or move the wait into `OffCameraAsync` with the condition "the text
    `was set as the default Admin theme` is visible in a `.toast-body`").
  - **Visible at the end of the narration:** the Themes page rendered by `MyAdminTheme` (the top bar shows the site
    name `Orchard Core` without the logo, and the page layout is the one of TheAdmin). The two green
    messages are visible in the upper right corner while the narration says "reports that": `MyAdminTheme was enabled`
    and `MyAdminTheme was set as the default Admin theme`. They disappear after 5 seconds
    (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`), so the click must come
    late enough for them to be on screen during the second half of the narration, and early enough for the page to be
    loaded before it ends. Record the step again with another `PauseAsync` value until both hold.
  - **Labels:** `Make Current` (`ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7`); the messages
    (`OrchardCore.Modules/OrchardCore.Themes/Controllers/AdminController.cs:145` and `:148`).
- `enable-4`
  - **Actions:** highlight the text `This is the current Admin theme` inside the card of `MyAdminTheme` (the
    `span.text-muted` of the card footer). The card is now among the first cards, so it is in view; scroll up if not.
  - **Visible at the end:** the card of `MyAdminTheme` with the ringed text `This is the current Admin theme`
    and no `Make Current` button in its footer; the card of `The Admin Theme` offers `Make Current` and `Disable`.
  - **Label:** `This is the current Admin theme` (`ThemeEntry-Current.SummaryAdmin.cshtml:8`).

## Clip `06-result` (chapter 06, The restyled admin)

**Starts on:** `/Admin`, opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync()`. The site is in
snapshot B: `MyAdminTheme` is the admin theme.

- `result-1`
  - **Actions:** highlight the brand in the top bar: `.ta-navbar-brand`, with the caption `AdminBranding`. Hold.
  - **Visible at the end:** the admin home page; the top bar shows `Orchard Core` as plain text, without the logo
    image, ringed; the sidebar menu and the top bar icons are still those of TheAdmin.
  - **Source:** `Views/AdminBranding.cshtml` of the theme (above); the layout renders it at
    `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:15` and `:59`.
- `result-2`
  - **Actions:** `ClearHighlightsAsync()`. Click `Settings` in `#ta-left-sidebar`
    (`OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36`), then `General`
    (`OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:39`), with `ClickAndWaitForNavigationAsync`. The page is
    `/Admin/Settings/general`. If the menu path differs on the running site (see `OPEN-QUESTIONS.md`), use
    `NavigateAsync("/Admin/Settings/general")`. Highlight the three labels `Site name`, `Page title format` and
    `Base url`, one `HighlightAsync(label)` each (`label.ocat-label` with that text).
  - **Visible at the end:** the General settings page in the horizontal layout: the labels `Site name`,
    `Page title format` and `Base url` at the left, right-aligned, each with its input to the right, three ringed
    labels.
  - **Labels:** `Site name`, `Page title format`, `Base url`
    (`OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:16`, `:25`, `:34`).
- `result-3`
  - **Actions:** `ClearHighlightsAsync()`, then `ScrollAsync(0, 400)` to show `Default Time Zone` and `Page size`
    (fields of limited width), and hold. Do not click `Save`.
  - **Visible at the end:** the lower part of the General settings page, the labels of `Default Time Zone` and
    `Page size` at the left of their fields (`Settings-Site.Edit.cshtml:43` and `:54`).

## Slide steps (drawn by `slides.py`, nothing to record)

- `how-1`: three cards, "Two current themes" bright.
- `how-2`: the same cards, "The Admin attribute" bright.
- `how-3`: the same cards, "A base theme" bright.
- `create-1`: the terminal commands that create the project, add it to the solution and reference it from the web
  application (the layout of the guide: `MySite` and `MyAdminTheme` side by side).
- `create-2`: `Manifest.cs` with the `Tags` and `BaseTheme` lines bright, and two cards.
- `create-3`: the file tree of the theme with `Layout.liquid` bright, and one card.
- `template-1`: `Views/_ViewImports.cshtml`.
- `template-2`: `Views/AdminBranding.cshtml` with the link markup bright.
- `template-3`: `Views/AdminBranding.cshtml` with the three lines of the `style` tag bright, the file tree of the theme
  (`wwwroot/Styles/theme.css`), and one card.
- `template-4`: `Views/AdminBranding.cshtml` with the `depends-on` line bright, and two cards.
- `forms-1`: four cards for the `ocat` classes, the first three bright.
- `forms-2`: the same cards, the fourth bright.
- `forms-3`: the stylesheet of the horizontal layout.
- `more-1`: two cards, templates of TheAdmin and placement.
- `more-2`: the `Hint.Admin` template.
- `more-3`: a controller with the `Admin` attribute.

The two cards (`intro` and `outro`) need nothing.

## Checks for private data

- The site is new and holds fake data only: the site name `Orchard Core`, the user `admin` (`admin@example.com`, which
  no recorded page shows), the author `My name` and the website `https://example.com` of the theme. The sample content of
  the Blog recipe is not shown.
- No page shows a password, a machine path, a user name of the machine, a connection string or a token. The Themes
  page shows `The Orchard Core Team` and `https://orchardcore.net` for the themes of Orchard Core, which are public.
- After assembling, OCR the video at one frame per second and at every scene change, and search the text for the
  user name of the machine, `localhost` paths, any e-mail address other than `@example.com`, and the password.
- Do not commit `state.json`, a copy of `App_Data`, the theme project files above, or the MP4.
