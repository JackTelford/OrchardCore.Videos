# Recording plan: Starter Recipes

How to record this video without deciding anything. This file is for the recorder, so it holds selectors, scene calls,
file and line references into the Orchard Core source, and shell commands. A selector written here stands for
`scene.Page.Locator(...)`. File references are relative to the root of the `OrchardCMS/OrchardCore` checkout at commit
`92db4a83` and look like `path:line`.

The narration is generated first (`narrate.py`), and each step's actions must be shorter than its narration. The
seconds below are estimates from the word counts (about 0.4 s a word); the real values are in `durations.json`.

## What the video shows

Five of the eight chapters are recordings of browser screens, in five clips: the Setup screen (`02-setup`), and the
front ends of the Blog (`04-blog`), Agency (`05-agency`), Coming Soon (`06-soon`) and Software as a Service
(`07-saas`) recipes. Every other step is a slide drawn by `slides.py`. No clip signs in, so **do not run
`Recorder setup` and keep no `state.json`**: every page is seen as an anonymous visitor. No clip changes the site.

## The sites it needs

The video needs two throwaway sites, one after the other. Both run `src/OrchardCore.Cms.Web` of the checkout with the
`Development` environment, on port 5010, with their own empty `App_Data` folder outside both repositories.

Set these in the shell that starts each site (Git Bash; replace the folder):

```bash
export ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5010
export ORCHARD_APP_DATA=<an empty folder outside both repositories>
export HTTP_PROXY=http://127.0.0.1:9 HTTPS_PROXY=http://127.0.0.1:9 NO_PROXY=localhost,127.0.0.1
dotnet run --project src/OrchardCore.Cms.Web -c Release --no-launch-profile
```

The dead proxy address stops the site from calling any outside service. The recorder runs in another shell that has
none of the proxy variables, with `VIDEO_ROOT` set to this video's folder, `VIDEO_BASE_URL=http://localhost:5010` and
`MSYS_NO_PATHCONV=1`. The browser is the recorder's Chromium at 1536x864, scale 1.25, light color scheme, `en-US`.

### Site A: a site that is not set up (only for `02-setup`)

This site is recorded **without Auto Setup, without `state.json`**.

1. Use an empty `ORCHARD_APP_DATA` folder and none of the `OrchardCore__AutoSetup__*` environment variables of
   site B.
2. `src/OrchardCore.Cms.Web` has a user-secrets id, and user secrets can hold an Auto Setup configuration that would
   install the site on the first request. Run `dotnet user-secrets list --project src/OrchardCore.Cms.Web` and make
   sure no `OrchardCore:AutoSetup` entry is listed (remove it with `dotnet user-secrets remove` if there is one).
   `appsettings.json` has its Auto Setup section commented out (`src/OrchardCore.Cms.Web/appsettings.json:250`).
3. Start the site. `http://localhost:5010/` shows the Setup screen (`src/OrchardCore.Modules/OrchardCore.Setup/Startup.cs`, the
   route with an empty pattern). Never press **Finish Setup**: the clip only opens the Recipe list and chooses an
   entry.
4. Stop the site (by its process ID, never by image name) and delete this `App_Data` folder when the clip is recorded.

### Site B: Auto Setup with four tenants (for the four front-end clips)

Use a second empty `ORCHARD_APP_DATA` folder. Site B also runs in the `Development` environment, so user secrets are
loaded here too and merge with the environment variables below: run `dotnet user-secrets list --project
src/OrchardCore.Cms.Web` first, and remove every `OrchardCore:AutoSetup` entry, or tenants from the secrets would be
mixed with the four tenants of this table. Auto Setup installs each tenant on its first request
(`src/OrchardCore.Modules/OrchardCore.AutoSetup/AutoSetupMiddleware.cs`), and creates the settings of the other
tenants once the Default tenant is installed. The same fake administrator is used for the four tenants, although no
clip signs in. The password is `<password>`: set `AdminPassword` from the shell variable `VIDEO_PASSWORD`, which is
chosen outside git and must never be written in a file of the repository.

Set `OrchardCore__AutoSetup__AutoSetupPath` to an empty value, and for each tenant `N` set
`OrchardCore__AutoSetup__Tenants__N__<Name>` with these values:

| N | ShellName | RequestUrlPrefix | SiteName | RecipeName |
| --- | --- | --- | --- | --- |
| 0 | `Default` | (empty) | `Orchard Core` | `SaaS` |
| 1 | `blog` | `blog` | `Sample Blog` | `Blog` |
| 2 | `agency` | `agency` | `Sample Agency` | `Agency` |
| 3 | `soon` | `soon` | `Sample Coming Soon` | `ComingSoon` |

The same values for the four tenants: `SiteTimeZone` = `America/Los_Angeles`, `AdminUsername` = `admin`,
`AdminEmail` = `admin@example.com`, `AdminPassword` = `<password>` (from `VIDEO_PASSWORD`), `DatabaseProvider` =
`Sqlite`, and an empty `DatabaseConnectionString`, `DatabaseTablePrefix` and `RequestUrlHost`. The option names are
those of `src/OrchardCore.Modules/OrchardCore.AutoSetup/Options/TenantSetupOptions.cs`. The recipe names are the
`name` values of the recipe files, not their display names (`src/OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:2`,
`TheBlogTheme/Recipes/blog.recipe.json:2`, `TheAgencyTheme/Recipes/agency.recipe.json:2`,
`TheComingSoonTheme/Recipes/comingsoon.recipe.json:2`).

**Warm up off camera, so no installation ever happens during a capture.** With the site running, request each tenant
once and wait for the final page, in this order (the Default tenant first, since it creates the other tenants):

```bash
curl -sL -o /dev/null -w "%{http_code}\n" http://localhost:5010/
curl -sL -o /dev/null -w "%{http_code}\n" http://localhost:5010/blog
curl -sL -o /dev/null -w "%{http_code}\n" http://localhost:5010/agency
curl -sL -o /dev/null -w "%{http_code}\n" http://localhost:5010/soon
```

Every request must print `200`. Then check the four pages by eye in a browser: `/` shows a message that begins with
"Welcome to Orchard Core", `/blog` shows a header titled "Blog", `/agency` shows "Welcome To Our Studio!", and `/soon`
shows "Coming Soon". Do not save anything and do not sign in. Stop the site, copy the `App_Data` folder aside, and
start the site again from the copy before recording.

Nothing in the four clips changes the site. If a take ever submits a form or signs in, stop the site, delete the
`App_Data` folder, and repeat the warm-up.

External requests of the browser are not blocked by the site's proxy. The Agency and Coming Soon themes ask Google
Fonts for fonts (`TheAgencyTheme/Views/Layout.liquid:9-10`, `TheComingSoonTheme/Views/layout.liquid:12-14`); if the
recording machine is offline, the pages use a fallback font, which is accepted. Do not install fonts to change it.

### Nothing else to prepare

- **Features to enable before recording:** none. Each recipe enables its own features during setup, and showing
  that is the point of the video.
- **Settings to save:** none. Email and reCaptcha are deliberately left unconfigured, because the Coming Soon page
  shows a message about the missing reCaptcha settings (see `soon-2`).
- **Seed data to create:** none. The recipes create all the content the clips show: do not add, edit or delete any
  content item, and do not change the site name, the time zone or the theme.
- **External services and local stand-ins:** none are needed, so there are no stand-ins. No mail server, payment
  provider or reCaptcha service is called, because no form is submitted. The browser's own requests for Google
  Fonts may fail on an offline machine; that is accepted.

## Order of the clips

| Order | Clip | Site | Depends on |
| --- | --- | --- | --- |
| 1 | `02-setup` | A (not set up) | nothing; record it first, then stop site A and delete its `App_Data` |
| 2 | `04-blog` | B | the warm-up |
| 3 | `05-agency` | B | the warm-up |
| 4 | `06-soon` | B | the warm-up |
| 5 | `07-saas` | B | the warm-up |

The clips of site B do not depend on each other and can be recorded again one at a time. None of them changes the
site, so no snapshot has to be restored between takes. The slide clips (`01-recipe`, `03-core`, `04-blog-more`,
`06-soon-more`, `07-saas-more`, `08-choose`) are drawn by `slides.py` and need no recording.

## 01-recipe (slides)

- `recipe-1`: slide "What a recipe does", four cards: Enables features, Activates themes, Creates roles, Creates content.
- `recipe-2`: slide "You choose once, at setup", three cards: One recipe, More features later, Another theme later.
- `recipe-3`: slide "Which recipes you see", two cards: Core recipes (Blank site and Headless site), Starter theme recipes.

## 02-setup (clip, site A)

Starts on `/` of site A, the Setup screen, opened with `scene.OpenAsync("/")` before `StartCaptureAsync()`. Window
title "Setup" (`src/OrchardCore.Modules/OrchardCore.Setup/Views/_Layout.cshtml:10`) and heading "Setup"
(`src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:102`). The "Default Time Zone" field is filled in
from the time zone of the recording machine (`Controllers/SetupController.cs:73`), so set the machine to a neutral
time zone before the take (see the checks for private data). The list
is a Bootstrap drop-down, not a native select, so it is drawn open in the capture. Entries are sorted by recipe name
(`Index.cshtml:146`) and hidden recipes are left out (`Controllers/SetupController.cs:56-59`), which gives, with
their `data-recipe-name`: Agency (`Agency`), Blank site (`Blank`), Blog (`Blog`), Coming Soon (`ComingSoon`), Headless
site (`Headless`), Software as a Service (`SaaS`). Locators: the button `#recipeButton`; an entry
`#recipes .dropdown-item[data-recipe-name='<name>']`.

- `setup-1` (about 14 s). `PauseAsync(4000)`; `MoveToAsync(#recipeButton)`; `PauseAsync(2500)`; `ClickAsync(#recipeButton, pauseAfter: 600)`.
  At the end of the narration the Recipe list is open under the Recipe button, with its six entries visible, each
  with a small gray description, and the button still reads "Software as a Service". Labels: "Setup"
  (`Index.cshtml:102`), "Recipe" (`Index.cshtml:130`), the button and the entries (`Index.cshtml:141-148`).
- `setup-2` (about 11 s). For each entry in the order Agency, Blank, Blog, ComingSoon, Headless, SaaS:
  `MoveToAsync(entry)` then `PauseAsync(700)`. At the end the cursor rests on the "Software as a Service" entry and
  the list is still open. Descriptions come from `displayName` and `description` of the recipe files
  (`TheTheme/Recipes/saas.recipe.json:3-4`, `TheAdmin/Recipes/blank.recipe.json:3-4`, `TheAdmin/Recipes/headless.recipe.json:3-4`,
  `TheBlogTheme/Recipes/blog.recipe.json:3-4`, `TheAgencyTheme/Recipes/agency.recipe.json:3-4`,
  `TheComingSoonTheme/Recipes/comingsoon.recipe.json:3-4`).
- `setup-3` (about 12 s). `PauseAsync(4300)`, so the click falls after the first sentence;
  `ClickAsync(#recipes .dropdown-item[data-recipe-name='Blog'], pauseAfter: 600)`. At the end the list is closed
  and the Recipe button reads "Blog" (`Assets/ts/setup.ts:53-62` replaces its text). Never click "Finish Setup"
  (`Index.cshtml:256`) and do not scroll.

## 03-core (slides)

- `blank-1`: slide "Blank site", three check cards: Content management features, No site theme, No home page.
- `blank-2`: slide "When to use Blank site", three cards: Build your own theme, Run a decoupled site, Change the theme later.
- `headless-1`: slide "Headless site", four check cards: Content management features, GraphQL, OpenID Connect, Admin as the home page.
- `headless-2`: slide "Review the Authenticated role", four cards: "View all content", "Execute GraphQL." (with the period), "Execute Api - All queries", "Where to review". The
  card titles are the permission descriptions that the role editor shows (`OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:14`):
  "View all content" (`CommonPermissions.cs:29`), "Execute GraphQL." with its final period (`GraphQLPermissions.cs:12`) and
  "Execute Api - All queries" (`QueryPermissions.cs:19`).

## 04-blog (clip, site B)

Starts on `scene.OpenAsync("/blog")`, the home page of the Blog tenant (the home route is the blog item,
`TheBlogTheme/Recipes/blog.recipe.json:114`). The page is seen signed out. The screen has the fixed navigation bar
with the site name "Sample Blog" and the menu "Home" and "About"
(`TheBlogTheme/Views/Layout.liquid:25-32`, `blog.recipe.json:834-845`), then a header with the title and description
(`TheBlogTheme/Views/Content-Blog.liquid:14-16`) and below it the post previews.

- `blog-1` (about 13 s). `PauseAsync(5000)`, so the highlight falls on the second sentence; `HighlightAsync(.site-heading)` (no caption). At the end the header
  shows the title "Blog" and the description "This is the description of your blog", highlighted.
- `blog-2` (about 11 s). `ClearHighlightsAsync()`; read the top of the first post preview with
  `scene.Page.EvaluateAsync<double>("document.querySelector('.post-preview').getBoundingClientRect().top")`, then
  `ScrollAsync(0, top - 140)`; `PauseAsync(800)`; `HighlightAsync(.post-preview .post-meta)` (no caption). At the end the first
  post preview shows its title, its subtitle ("Problems look mighty small from 150 miles up"), and a line "Posted by
  admin on <date>" with the badges "Travel", "Earth", "Exploration" and "Space", highlighted
  (`TheBlogTheme/Views/Content-BlogPost.Summary.liquid:3-31`).
- `blog-3` (about 11 s). `ClearHighlightsAsync()`; scroll back to the top (`ScrollAsync(0, -scrollY)`, with `scrollY` read
  from `window.scrollY`); `PauseAsync(2500)`; `ClickAndWaitForNavigationAsync(#mainNav a:text-is("About"))`. At the end the
  "About" article is open, with its header titled "About" and its subtitle "This is what I do."
  (`blog.recipe.json:1079`, `TheBlogTheme/Views/Content-Article.liquid:15-16`), and the menu still shows "Home" and "About".

## 04-blog-more (slides)

- `blog-4`: slide "Optional recipes of The Blog Theme", four cards: SQL, Lucene, Elasticsearch (with the six display names `Blog - SQL Search`, `Blog - SQL Query`, `Blog - Lucene Search`, `Blog - Lucene Query`, `Blog - Elasticsearch Search`, `Blog - Elasticsearch Query`, from `TheBlogTheme/Recipes/blog.*.recipe.json:3`), and Where to run them (Tools, Recipes: `src/OrchardCore.Modules/OrchardCore.Recipes/AdminMenu.cs:18-19`).

## 05-agency (clip, site B)

Starts on `scene.OpenAsync("/agency")`, the landing page of the Agency tenant (home route:
`TheAgencyTheme/Recipes/agency.recipe.json:100`). The page is seen signed out.

- `agency-1` (about 13 s). `PauseAsync(10500)`, so the highlight falls on the words "Tell Me More button"; `HighlightAsync(a.btn:text-is("Tell Me More"))` (no caption). At the end the
  top of the page shows the navigation bar with the site name "Sample Agency" and the menu links Services, Portfolio,
  About, Team and Contact; the header with "Welcome To Our Studio!", the site name in capitals as a large heading
  ("SAMPLE AGENCY"), and the highlighted "Tell Me More" button (the theme draws the site name of the navigation bar
  (`TheAgencyTheme/Assets/TheAgencyTheme/src/scss/components/_navbar.scss:24`), the heading and the button in capitals
  (`text-uppercase` in `landingpage.liquid:6-7`); the menu links keep the case written above)
  (`TheAgencyTheme/Recipes/Snippets/landingpage.liquid:3-8`, `TheAgencyTheme/Views/Layout.liquid:23-25`,
  the menu items of `agency.recipe.json:1005-1045`: "Services", "Portfolio", "About", "Team", "Contact").
- `agency-2` (about 13 s). `ClearHighlightsAsync()`; `ClickAsync(a.btn:text-is("Tell Me More"), pauseAfter: 1800)` (an anchor on
  the same page that scrolls to `#services`); `PauseAsync(2000)`; read the top of `#portfolio` with
  `scene.Page.EvaluateAsync<double>("document.querySelector('#portfolio').getBoundingClientRect().top")` and
  `ScrollAsync(0, top - 90)`. **Never scroll further**: the Team section (`#team`, `landingpage.liquid:109`) and the
  Clients logos (`landingpage.liquid:150-155`) must not appear in any frame. At the end the "Portfolio" section is at
  the top of the page with its first projects, below the navigation bar.

## 06-soon (clip, site B)

Starts on `scene.OpenAsync("/soon")`, the Coming Soon page (home route: `comingsoon.recipe.json:97`). The page is
seen signed out. It shows, over the theme's background, the heading "Coming Soon", a paragraph about the launch month,
an email field with the placeholder "Enter email address...", the button "Notify Me!", the message "ReCaptcha settings
are not configured." under the email field, and social icons (`TheComingSoonTheme/Recipes/Snippets/Content__ComingSoon.liquid:4-18`,
`Widget__Register.liquid:3-5`, `comingsoon.recipe.json:451-454`, `src/OrchardCore.Modules/OrchardCore.ReCaptcha/Views/Items/ReCaptchaPart.cshtml:9`).
The theme's background is a video file (`TheComingSoonTheme/Views/layout.liquid`, the `bg-video` element); the
recorder's browser may not play it, and a dark background is accepted.

- `soon-1` (about 12 s). `PauseAsync(6500)`, so the highlights follow the second sentence; `HighlightAsync(h1)`, `PauseAsync(1100)`, `ClearHighlightsAsync()`; then the same three calls with
  `.masthead-content p` and with `.input-group-newsletter`; then `HighlightAsync(.social-icons)`, which stays on. At the
  end the social icons are highlighted.
- `soon-2` (about 15 s). `ClearHighlightsAsync()`; `TypeAsync(.input-group-newsletter input, "visitor@example.com", delay: 55)`;
  `MoveToAsync(#submit-button)` without clicking; `PauseAsync(1500)`; `HighlightAsync(.alert.alert-warning)` (no caption). **Never click
  "Notify Me!" and never press Enter in the field**: the form starts a workflow that registers a user and sends an
  email. At the end the typed address is in the field, the cursor is on the "Notify Me!" button, and the warning
  "ReCaptcha settings are not configured." is highlighted. If the page has no `.alert.alert-warning`, stop: a reCaptcha
  key was saved somewhere, and the site must be installed again.

## 06-soon-more (slides)

- `soon-3`: slide "Behind the form", three cards: User Registration workflow, Where to find it (Design, Workflows: `src/OrchardCore.Modules/OrchardCore.Workflows/AdminMenu.cs:18-19`), Features the recipe enables.

## 07-saas (clip, site B)

Starts on `scene.OpenAsync("/")`, the home page of the Default tenant, installed with the Software as a Service recipe
(home route: `TheTheme/Recipes/saas.recipe.json:45`). The page is seen signed out.

- `saas-1` (about 14 s). `PauseAsync(3000)`; `HighlightAsync(.alert-primary)` (no caption). At the end the page of
  The Default Theme shows a highlighted blue message, "Welcome to Orchard Core, your site has been successfully set
  up." (`TheTheme/Views/Home/Index.cshtml:2`), under the navigation bar.

## 07-saas-more (slides)

- `saas-2`: slide "Tenants", three cards: Tenants feature, Multi-Tenancy, Tenants (`src/OrchardCore.Modules/OrchardCore.Tenants/AdminMenu.cs:30-33`; the button "Add Tenant": `Views/Admin/Index.cshtml:27`; the "Recipe" field of the form: `Views/Shared/_TenantRecipes.cshtml:4`), and Recipe list.

## 08-choose (slides)

- `choose-1`: slide "Which recipe fits", four cards, the first two bright (You design the front end: Blank site; Other applications read your content: Headless site).
- `choose-2`: the same slide, the last two bright (A theme with sample content: Blog, Agency, or Coming Soon; Several sites in one installation: Software as a Service).
- `choose-3`: slide "Change course later", three cards: Tools, Features (`OrchardCore.Features/AdminMenu.cs:26-27`), Design, Themes (`OrchardCore.Themes/AdminMenu.cs:18-21`), Tools, Recipes.

## Checks for private data

- Every email on a page or in a field is `admin@example.com` or `visitor@example.com`. The user name is `admin`. No real
  person, company, address or phone number is typed or shown by the recording itself.
- The Agency recipe creates stock sample content with the names of placeholder people (Team) and the names of
  well-known companies (Clients). **They must not appear in any frame**: the `agency-2` actions stop at the Portfolio
  section, and the contact sheet must be checked for the Team and Clients sections. If a frame shows either, record
  the clip again with a shorter scroll. The narration names none of them.
- No page shows a password, a token or an API key. The Setup screen is recorded with empty fields; its password
  fields are never filled.
- The Setup screen of `02-setup` shows "Default Time Zone" with the time zone of the recording machine
  (`SetupController.cs:73`), which tells where the machine is. Set the operating system to Pacific Time
  (`America/Los_Angeles`, the zone of the tenants of site B) for this take and restore it afterward; check the field in
  the contact sheet.
- After recording, run the OCR scans of the skill (one frame per second and at every scene change) on the four clips
  and search the text for anything that is not listed above.
