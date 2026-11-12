# Open questions: Starter Recipes

What was checked in the Orchard Core source (commit `92db4a83`) and the documentation, and what a person must confirm
on a running site before or while recording. The narration states only what the source shows; each item below says
which sentence depends on it. Nothing here was run: the author reads source and does not start a site.

## 1. The Setup screen (`setup-1`, `setup-2`, `setup-3`)

- **Checked in the source:** the Recipe list is built from the setup recipes that are not tagged `hidden` and is sorted
  by recipe name (`src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:146`,
  `Controllers/SetupController.cs:56-61`); the entry that is selected when the screen opens is the first recipe whose
  tags contain `default`, which only the Software as a Service recipe has
  (`src/OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:10`). Choosing an entry replaces the button text
  (`Assets/ts/setup.ts:53-62`).
- **Confirm on screen:** the button reads "Software as a Service" when the page opens; the six entries are Agency,
  Blank site, Blog, Coming Soon, Headless site, Software as a Service, in this order, each with its description;
  all six are visible inside the 1536x864 viewport without scrolling; an application that carries a recipe of its own
  would change the list, so check that the recording application does not.
- **Confirm the language and the time zone:** the Setup screen takes its default culture from the installed UI culture of
  the machine and supports a list of cultures (`OrchardCore.Setup/Startup.cs:17-41,73-78`); check that every label of
  `setup-1` to `setup-3` is the English one. "Default Time Zone" is preselected from the time zone of the
  machine (`SetupController.cs:73`), see the checks for private data in the recording plan.

## 2. Auto Setup with four tenants (all clips of site B)

- **Checked in the source:** the Default tenant is installed on its first request, then the settings of the other
  tenants are created, and each of them is installed on its first request
  (`src/OrchardCore.Modules/OrchardCore.AutoSetup/AutoSetupMiddleware.cs`, `Services/AutoSetupService.cs`).
- **Confirm:** that environment variables with the `OrchardCore__AutoSetup__Tenants__N__...` names install all four
  tenants with their URL prefixes on a single `Sqlite` configuration, that each warm-up request answers `200`, and how
  long the Blog and Agency installations take (they copy media files). The recording plan has never been run.
- **Checked in the source:** the Coming Soon recipe lists `OrchardCore.Tenants`, a feature that only the Default tenant can
  have (`OrchardCore.Tenants/Manifest.cs:15`). The feature step enables only the features that the shell reports as
  available (`OrchardCore.Features/Recipes/Executors/FeatureStep.cs:25-28`, `DefaultTenantOnlyFeatureValidationProvider.cs`),
  so the `soon` tenant skips it instead of failing. **Confirm** that `http://localhost:5010/soon` answers `200` after the
  warm-up.

## 3. Blank site and Headless site (`blank-1`, `blank-2`, `headless-1`, `headless-2`)

- **Checked in the source:** Blank site sets the admin theme and an empty site theme and has no `HomeRoute` setting
  (`src/OrchardCore.Themes/TheAdmin/Recipes/blank.recipe.json:65-68`); Headless site sets the home route to the
  admin (`headless.recipe.json:99-105`) and grants the Authenticated role `ViewContent`, `ExecuteGraphQL` and
  `ExecuteApiAll` (`headless.recipe.json:86-94`), whose labels are "View all content", "Execute GraphQL." (the period is part of the label,
  and the slide `headless-2` writes it) and "Execute Api - All queries" (`src/OrchardCore/OrchardCore.Contents.Core/CommonPermissions.cs:29`,
  `OrchardCore.Apis.GraphQL.Abstractions/GraphQLPermissions.cs:12`, `OrchardCore.Queries.Abstractions/QueryPermissions.cs:19`).
- **Not narrated, because it is not verified:** what a visitor sees at the address of a Blank site (the home route
  transformer returns nothing when no home route is set, `OrchardCore.HomeRoute/Routing/HomeRouteTransformer.cs`, so a
  not-found page is expected), and what a signed-out visitor sees at the address of a Headless site (the admin home
  page is behind `[Authorize]`, so a redirect to the sign-in page is expected). Both are covered by slides only.
- **Checked in the documentation and source:** a decoupled site keeps the front end and the back end in the same web application, with pages
  written by developers (`src/docs/guides/decoupled-cms/README.md:7`), which `blank-2` says; a separate application that reads the
  content through an API is the headless model of `headless-1`. The Authenticated role applies to every signed-in user
  (`OrchardCore.Roles/Services/RolesPermissionsHandler.cs:47-49`, and the role's description in `headless.recipe.json`), which `headless-2` says.
- **Confirm on a running Headless site:** the "Authenticated" role is listed under Access Control, Roles and holds the
  three permissions above; the menu path "Access Control, Roles" is from `OrchardCore.Roles/AdminMenu.cs:18-19`.

## 4. The Blog recipe (`blog-1`, `blog-2`, `blog-3`, `blog-4`)

- **Checked in the source:** the home route is the blog item, the content types the recipe defines are Article, Blog
  Post, Blog, Container, Blockquote, Image, Paragraph, Raw Html, Page, Tag and Category, and the content items are a main
  menu (Home, About), the Tags and Categories taxonomies, the blog, one blog post, the About article and a footer widget
  (`TheBlogTheme/Recipes/blog.recipe.json`). The six optional recipes are not setup recipes
  (`"issetuprecipe": false` in each `blog.*.recipe.json`), and Tools, Recipes lists the non-setup recipes of the
  available features (`src/OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:161-167`).
- **Confirm:** the header, post preview and "About" page look as `recording-plan.md` says; the navigation bar of the
  Blog theme is visible at the top of the page when the `About` link is clicked (`blog-3` scrolls back to the top first);
  on a signed-in site, Tools, Recipes shows the six `Blog - ...` recipes under "The Blog Theme" (not recorded).

## 5. The Agency recipe (`agency-1`, `agency-2`)

- **Checked in the source:** the landing page is the home route, its title is the site name given at setup, and its
  Services, Portfolio, About, Team and Clients parts are Bag parts filled with sample content
  (`agency.recipe.json:99-105`, the `LandingPage` type and `content` step; `landingpage.liquid`).
- **Decision for a maintainer:** the sample content includes the names of placeholder people and the names of well-known
  companies as client logos. The plan keeps both out of every frame; if a maintainer prefers to show the whole page,
  the stock names must be replaced off camera first, because this programme shows fake data only.
- **Confirm:** the "Tell Me More" link scrolls to the Services section, the Portfolio top is reachable by one scroll,
  and the Team heading is below the lower edge of the frame at that scroll position.

## 6. The Coming Soon recipe (`soon-1`, `soon-2`, `soon-3`)

- **Checked in the source:** the page has the heading, the message, the email field (placeholder "Enter email
  address..."), the button "Notify Me!", a ReCaptcha widget and three social icons; the form posts to the workflow "User
  Registration", which validates the form, registers a user, sends a confirmation email and redirects to the home
  page (`TheComingSoonTheme/Recipes/comingsoon.recipe.json:451-454`, the `WorkflowType` step from line 507 on; the
  activity in `src/OrchardCore.Modules/OrchardCore.Users/Workflows/Activities/RegisterUserTask.cs`). The warning
  "ReCaptcha settings are not configured." is shown when the Site Key or the Secret Key is empty
  (`OrchardCore.ReCaptcha/Views/Items/ReCaptchaPart.cshtml:9`, `OrchardCore.ReCaptcha.Core/Configuration/ReCaptchaSettings.cs`);
  the settings are at Settings, Security, reCaptcha, with the fields "Site Key" and "Secret Key"
  (`OrchardCore.ReCaptcha/AdminMenu.cs:26-28`, `Views/ReCaptchaSettings.Edit.cshtml:4,13`).
- **Not narrated, because it is not verified:** that a submission completes. Sending the confirmation email needs an
  email provider, and the ReCaptcha check needs keys; neither is set up, and the form is never submitted on camera.
- **Confirm:** the warning is displayed under the email field (the widgets render in the order of the recipe: validation summary, Register, ReCaptcha; the whole item sits inside the form, `src/OrchardCore.Modules/OrchardCore.Forms/Drivers/FormContentDisplayDriver.cs:14-15`); the white text stays readable on the theme's background in the
  recorder's Chromium, which may not play the H.264 background video (`TheComingSoonTheme/Views/layout.liquid:24`);
  the icons are drawn (Font Awesome is served by the site itself); the page's launch month in the message is not a
  real person's data.

## 7. Software as a Service (`saas-1`, `saas-2`)

- **Checked in the source:** the recipe enables the Tenants feature, activates "The Default Theme" and sets the home route
  to the Home controller of that theme, whose view shows "Welcome to Orchard Core, your site has been successfully set
  up." (`saas.recipe.json:13-50`, `TheTheme/Views/Home/Index.cshtml:2`). Multi-Tenancy, Tenants appears on the Default
  tenant only, the Add Tenant form has a "Recipe" field listing all the setup recipes
  (`OrchardCore.Tenants/Views/Shared/_TenantRecipes.cshtml`), sorted by display name, unlike the Setup screen, which
  sorts by recipe name; the form does not apply the `hidden` filter of the Setup screen
  (`OrchardCore.Tenants/Controllers/AdminController.cs:332`), so a site that has the hidden "OpenApi Client Generation"
  recipe lists it there. The narration only says "a Recipe list" and the slide does not list its entries.
- **Confirm:** the message is displayed in a blue alert on the home page of the Default tenant.

## 8. Names, labels and pronunciation

- The theme names ("The Blog Theme", "The Agency Theme", "The Coming Soon Theme", "The Default Theme", "The Admin
  Theme") are the `Name` of each theme manifest and appear as card titles under Design, Themes.
- Confirm with `scripts/pronounce.py` before narrating: "GraphQL", "OpenID Connect", "Lucene", "Elasticsearch",
  "SQL", and "ReCaptcha" next to "reCaptcha". None is added to the storyboard's `spoken` map, since a spelling
  must be confirmed by a person first.
