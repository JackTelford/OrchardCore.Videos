# Recording plan: Sitemaps for Localized Content and Razor Pages

A developer video. Four chapters of slides are drawn by `slides.py` and need no recording. Four clips are recorded in a
browser: `02-features`, `02-sitemap`, `03-types` and `03-sitemap`. Every label below was read in the Orchard Core
source at commit `92db4a83`, with the file and line given after "Label from"; paths are relative to the root of the
`OrchardCMS/OrchardCore` checkout. Selectors stand for `scene.Page.Locator(...)`.

## The site it needs

- **Orchard Core**: a checkout at commit `92db4a83`, or a later one whose labels were checked again.
- **A throwaway host project named `OrchardSite`**, because the video is about Razor Pages of a web project. Its name
  is the area of its Razor Pages (see OPEN-QUESTIONS.md, question 1), so keep it exactly `OrchardSite`.
  - Create `src/OrchardSite/` next to `src/OrchardCore.Cms.Web/`. Do not add it to the solution, and delete it after
    the recording.
  - `src/OrchardSite/OrchardSite.csproj`, derived from `OrchardCore.Cms.Web.csproj` without NLog, OpenTelemetry, user
    secrets and the unit test access:

    Nullable reference types are left off, unlike in `OrchardCore.Cms.Web.csproj`: the property `Id` of the page below
    is a `string` without an initial value, and with nullable types on it raises warning CS8618 (the repository-root
    `Directory.Build.props` was not in the copy of the source that was read; if warnings are errors there, this keeps
    the build clean).

    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Web">
      <Import Project="..\OrchardCore\OrchardCore.Application.Cms.Core.Targets\OrchardCore.Application.Cms.Core.Targets.props" />
      <PropertyGroup>
        <TargetFrameworks>$(CommonTargetFrameworks)</TargetFrameworks>
        <RazorRuntimeCompilation>false</RazorRuntimeCompilation>
        <ImplicitUsings>enable</ImplicitUsings>
      </PropertyGroup>
      <ItemGroup>
        <Folder Include="wwwroot\" />
      </ItemGroup>
      <Target Name="EnsureWwwRootExists" BeforeTargets="Build;Publish">
        <MakeDir Directories="$(MSBuildProjectDirectory)\wwwroot" />
      </Target>
      <ItemGroup>
        <ProjectReference Include="..\OrchardCore\OrchardCore.Application.Cms.Targets\OrchardCore.Application.Cms.Targets.csproj" />
      </ItemGroup>
      <Import Project="..\OrchardCore\OrchardCore.Application.Cms.Core.Targets\OrchardCore.Application.Cms.Core.Targets.targets" />
      <Import Project="..\OrchardCore\OrchardCore.Application.Targets\OrchardCore.Application.Targets.targets" />
      <Import Project="..\OrchardCore.Build\OrchardCore.Commons.targets" />
    </Project>
    ```

  - `src/OrchardSite/Program.cs`, from `OrchardCore.Cms.Web/Program.cs` plus the options of the video's slide:

    ```csharp
    using OrchardCore.Sitemaps;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddOrchardCms()
        .AddSetupFeatures("OrchardCore.AutoSetup");

    builder.Services.Configure<SitemapsRazorPagesOptions>(options =>
    {
        options.ConfigureContentType("DecoupledBlogPost", o =>
        {
            o.PageName = "DecoupledBlogPost";
            o.RouteValues = contentItem => new
            {
                area = "OrchardSite", // The name of the web project.
                id = contentItem.ContentItemId,
            };
        });
    });

    var app = builder.Build();

    app.UseStaticFiles();

    app.UseOrchardCore();

    await app.RunAsync();
    ```

  - `src/OrchardSite/Pages/DecoupledBlogPost.cshtml`, the page of the decoupled CMS guide
    (`src/docs/guides/decoupled-cms/README.md`, "Loading a Blog Post from its identifier"), named after the page:

    ```html
    @page "/blogpost/{id}"
    @using OrchardCore
    @inject IOrchardHelper Orchard

    @{
        var blogPost = await Orchard.GetContentItemByIdAsync(Id);
    }

    <h1>This is the blog post: @blogPost.DisplayText</h1>

    @functions
    {
        [FromRoute]
        public string Id { get; set; }
    }
    ```

  - `src/OrchardSite/Pages/_ViewImports.cshtml` holds one line: `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`.
- **Run it** on `http://localhost:5010` (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an empty folder outside
  both repositories, and `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. Provision it with Auto Setup through
  environment variables (`OrchardCore__AutoSetup__Tenants__0__...`, documented in
  `src/docs/reference/modules/AutoSetup/README.md`):

  | Variable suffix | Value |
  |---|---|
  | `ShellName` | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>`, the value of `VIDEO_PASSWORD`, chosen outside git |
  | `DatabaseProvider` | `Sqlite` |
  | `RecipeName` | `Blank` (the "Blank site" recipe: `src/OrchardCore.Themes/TheAdmin/Recipes/blank.recipe.json`) |

  The Blank site recipe enables Autoroute, Contents, Content Types, Deployment, Title and Templates (which brings the
  Liquid pattern of the Autoroute part), so the seed below needs no more features than the five it lists.

  This video is not about Setup, so it is recorded with Auto Setup and with a `state.json` made by
  `Recorder setup` (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`).
- **External services**: none. Nothing calls out of the machine.
- **Admin list layout**: the defaults, which are `List` with `Buttons`
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`; the host project has no `appsettings.json`
  that changes them). The clips click "Manage Sources" and "Edit" buttons.

### Seed data, outside the capture

Do these once as `admin` before the first take, with the browser of the recorder or any browser. Use only the values
below.

1. **Features.** Open Tools, Deployments, JSON Import (`/Admin/DeploymentPlan/Import/Json`), paste this recipe in the
   JSON box, and select "Import". A notification "Recipe imported successfully!" must show.

   ```json
   {
     "steps": [
       {
         "name": "feature",
         "enable": [
           "OrchardCore.Localization",
           "OrchardCore.ContentLocalization",
           "OrchardCore.Sitemaps",
           "OrchardCore.ContentLocalization.Sitemaps",
           "OrchardCore.Sitemaps.RazorPages"
         ]
       }
     ]
   }
   ```

2. **Settings, content types and content.** Open JSON Import again, paste this recipe, and select "Import". The same
   notification must show. It sets the Base url, makes English and French the supported cultures, defines the content
   types Article (localized, routed by Autoroute) and Decoupled Blog Post (no Autoroute part), and creates one
   localized article in two cultures and two posts.

   ```json
   {
     "steps": [
       {
         "name": "settings",
         "BaseUrl": "https://www.example.com",
         "LocalizationSettings": { "DefaultCulture": "en", "SupportedCultures": [ "en", "fr" ] }
       },
       {
         "name": "ContentDefinition",
         "ContentTypes": [
           {
             "Name": "Article",
             "DisplayName": "Article",
             "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
             "ContentTypePartDefinitionRecords": [
               { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
               {
                 "PartName": "AutoroutePart", "Name": "AutoroutePart",
                 "Settings": {
                   "ContentTypePartSettings": { "Position": "1" },
                   "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "{{ Model.ContentItem | display_text | slugify }}" }
                 }
               },
               { "PartName": "LocalizationPart", "Name": "LocalizationPart", "Settings": { "ContentTypePartSettings": { "Position": "2" } } }
             ]
           },
           {
             "Name": "DecoupledBlogPost",
             "DisplayName": "Decoupled Blog Post",
             "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true, "Draftable": true, "Versionable": true } },
             "ContentTypePartDefinitionRecords": [
               { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } }
             ]
           }
         ],
         "ContentParts": []
       },
       {
         "name": "content",
         "Data": [
           {
             "ContentItemId": "4zn3x0pdqxryon8j1dybv7zxg0", "ContentType": "Article", "DisplayText": "Welcome to Maple Street Market",
             "Latest": true, "Published": true, "Author": "admin",
             "TitlePart": { "Title": "Welcome to Maple Street Market" },
             "AutoroutePart": { "Path": "en/welcome" },
             "LocalizationPart": { "LocalizationSet": "4vqjxxvbothd0wtflhbmuaptt0", "Culture": "en" }
           },
           {
             "ContentItemId": "42vdhldsun7av8bppxx71c9jeq", "ContentType": "Article", "DisplayText": "Bienvenue au Maple Street Market",
             "Latest": true, "Published": true, "Author": "admin",
             "TitlePart": { "Title": "Bienvenue au Maple Street Market" },
             "AutoroutePart": { "Path": "fr/bienvenue" },
             "LocalizationPart": { "LocalizationSet": "4vqjxxvbothd0wtflhbmuaptt0", "Culture": "fr" }
           },
           {
             "ContentItemId": "4qa3qifpuw23ydesok50llsd9d", "ContentType": "DecoupledBlogPost", "DisplayText": "Spring Planting Calendar",
             "Latest": true, "Published": true, "Author": "admin",
             "TitlePart": { "Title": "Spring Planting Calendar" }
           },
           {
             "ContentItemId": "42veyx0gsaxmknuj0ewu1rfc04", "ContentType": "DecoupledBlogPost", "DisplayText": "Weekend Market Hours",
             "Latest": true, "Published": true, "Author": "admin",
             "TitlePart": { "Title": "Weekend Market Hours" }
           }
         ]
       }
     ]
   }
   ```

   The two recipes are separate because the second one uses the parts and settings of the features that the first one
   enables. Both were written from the recipe documentation and the Blog recipe, and were not run when this plan was
   written (OPEN-QUESTIONS.md, question 4).
3. **The sitemap.** Open Tools, Search Engine Optimization, Sitemaps, and select "Add Sitemap". Enter Name
   `Site Pages` and Permalink `sitemap.xml`, leave Enabled checked, and select "Create". In the list, select "Manage
   Sources" on Site Pages, then "Add Source", then "Add" on the card "Sitemap Content Types". Leave "Index All Content
   Types" checked and select "Create".
4. **Check the seed** (see "Checks before recording").
5. **Snapshot.** Stop the site, copy its `App_Data` folder and `state.json` aside, and start the site again. Restore
   the copy before every full take. The built sitemaps are cached outside `App_Data`, in `wwwroot/Default/sm-cache` of
   the host project (`src/OrchardCore.Modules/OrchardCore.Sitemaps/Cache/DefaultSitemapCacheProvider.cs:182`: the web
   root, the tenant name, then `sm-cache`), so a restored `App_Data` keeps the cache of the last take. After a restore
   that changes the seed, select "Purge All" on Tools, Search Engine Optimization, Sitemaps Cache. Delete the host
   project, with that folder, after the recording.

Label from: Tools > Search Engine Optimization > Sitemaps `src/OrchardCore.Modules/OrchardCore.Sitemaps/AdminMenu.cs:18-22`;
"Add Sitemap" `.../OrchardCore.Sitemaps/Views/Admin/List.cshtml:30`; Name, Permalink, Enabled, "Create"
`.../Views/Admin/Create.cshtml:17,25,43,50`; "Manage Sources" `.../Views/SitemapListEntry.Buttons.SummaryAdmin.cshtml:6`;
"Add Source" `.../Views/Admin/Display.cshtml:18`; "Add" `.../Views/SitemapSource.Thumbnail.cshtml:8`;
"Sitemap Content Types" `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.Thumbnail.cshtml:3`;
"Index All Content Types" `.../ContentTypesSitemapSource.Edit.cshtml:14`; "Create" `.../OrchardCore.Sitemaps/Views/Source/Create.cshtml:25`;
Tools > Deployments > JSON Import `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30`; "Import"
`.../Views/Import/Json.cshtml:21`; "Recipe imported successfully!" `.../Controllers/ImportController.cs:185`;
"Base url" `src/OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:34`; the Base url prefixes the
addresses `src/OrchardCore.Modules/OrchardCore.Sitemaps/Controllers/SitemapController.cs:77`.

### Checks before recording

- `/sitemap.xml` (type it in the address bar of any browser) shows an XML tree with four `url` elements: the two
  articles, each with two `xhtml:link` elements (`hreflang` `en` and `fr`, `href` `https://www.example.com/en/welcome`
  and `https://www.example.com/fr/bienvenue`), and the two posts, with a `loc` of
  `https://www.example.com/blogpost/4qa3qifpuw23ydesok50llsd9d` and `https://www.example.com/blogpost/42veyx0gsaxmknuj0ewu1rfc04`.
  The root element has `xmlns:xhtml="http://www.w3.org/1999/xhtml"`.
- `/blogpost/4qa3qifpuw23ydesok50llsd9d` opens the page "This is the blog post: Spring Planting Calendar". If the
  `loc` of a post is only `https://www.example.com` with nothing after it, or the page does not open, stop and read
  OPEN-QUESTIONS.md, question 1: the area in the route values does not match the area of the page.
- Tools, Features lists "Localized Content Item Sitemaps" and "Sitemaps for Decoupled Razor Pages" with a "Disable"
  button each.
- If the sitemap shows older content after a change to the seed, select "Purge All" on Tools, Search Engine
  Optimization, Sitemaps Cache (`.../OrchardCore.Sitemaps/Views/SitemapCache/List.cshtml:20`), then open the sitemap
  again.

## Order of the clips

No clip depends on another one, and none changes the site except that the first request of `/sitemap.xml` builds and
caches the sitemap. Record them in the order of the video: `02-features`, `02-sitemap`, `03-types`, `03-sitemap`. No
snapshot has to be restored between two clips; restore it before a new full take, and purge the sitemap cache if the
seed changed.

## Chapters of slides

Nothing to record. Each slide step is drawn by `slides.py`:

- `routes-1`: three cards, "Is the type routeable?", "What is the URL?" and "Which provider answers?".
- `routes-2`: the interface `IRouteableContentTypeProvider` with two cards, "Autoroute" and "Razor Pages".
- `loc-1`: `SitemapsStartup` of the Content Localization module, with the two cards "Metadata provider" and "Query provider".
- `loc-2`: `ApplyExtendedMetadataAsync` of `SitemapUrlHrefLangExtendedMetadataProvider`, trimmed.
- `raz-0`: three cards, "Autoroute provider", "Decoupled site" and "Razor Pages provider".
- `raz-1`: the options in `Program.cs`, with `ConfigureContentType` and `PageName` bright, and two cards.
- `raz-2`: the options with `RouteValues` bright, next to the Razor Page `Pages/DecoupledBlogPost.cshtml`.
- `raz-3`: `GetRouteAsync` of `RazorPagesContentTypeProvider`, trimmed.
- `ext-1`: the registration of `AutorouteContentTypeProvider`, with two cards.
- `ext-2`: `AutorouteContentTypeProvider`, trimmed.

## Clip `02-features` (chapter 02)

Starts on `/Admin/Features` (`scene.OpenAsync`), signed in as admin, with the admin menu collapsed, and the search box empty.

`loc-3`
- Actions: `await scene.TypeAsync(scene.Page.Locator("#search-box"), "sitemaps")`. Then
  `var feature = scene.Page.GetByText("Localized Content Item Sitemaps", new() { Exact = true });`, wait for it with
  `await feature.WaitForAsync()`, and `await scene.HighlightAsync(feature)`. Hold until the narration ends. The text
  must be matched exactly: the description under the name, "Provides support for localized content item sitemaps.",
  contains the same words, and `HighlightAsync` draws nothing when a locator matches more than one element. Keep the
  actions shorter than the narration.
- On screen at the end of the narration: the Features page with "sitemaps" in the search box and four rows, in two
  categories: under "Content Management", "Sitemaps", "Sitemaps for Decoupled Razor Pages" and "Sitemaps Cleanup";
  under "Internationalization", "Localized Content Item Sitemaps" with its identifier
  `OrchardCore.ContentLocalization.Sitemaps` readable next to the name. The search keeps a row when its name,
  identifier or description contains the text; these four are the only features of the source whose name, identifier or
  description contains "sitemaps". The first three rows show the badge "Enabled" and a "Disable" button. "Sitemaps
  Cleanup" is not enabled by the seed: it shows the badge "Disabled" and an "Enable" button, which is expected (do not
  enable it). A green ring is around "Localized Content Item Sitemaps".
- Label from: Tools > Features `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-29`; search box
  `.../Features/Views/Admin/Features.cshtml:27`; what the search matches (name, identifier and description, lower case)
  `.../Features/Drivers/FeatureEntryDisplayDriver.cs:20` and `.../Features/Assets/ts/features-index.ts:101`; name and
  identifier `.../Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`; description
  `.../FeatureEntry.Description.SummaryAdmin.cshtml`; feature names and categories
  `src/OrchardCore.Modules/OrchardCore.ContentLocalization/Manifest.cs:30,33`, `.../OrchardCore.Sitemaps/Manifest.cs:12,18`,
  `:23,29` and `:34,40`; badges "Enabled" and "Disabled" `.../FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9,15`; "Disable"
  `.../Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53` and "Enable" `:40`.

## Clip `02-sitemap` (chapter 02)

Starts on `/Admin/Sitemaps/List` (`scene.OpenAsync`), with the list showing Site Pages.

`loc-4`
- Actions: `await scene.NavigateAsync("/sitemap.xml")`. Wait until the text `hreflang` is shown
  (`scene.Page.GetByText("hreflang").First`). Highlight the first `xhtml:link` line of the English article:
  `await scene.HighlightAsync(scene.Page.GetByText("hreflang").First)`. Hold.
  The browser draws the XML as a tree and may show a line "This XML file does not appear to have any style information";
  that is expected. If the text locators do not match the elements of the tree, look at the page with
  `Recorder probe /sitemap.xml "() => document.body.innerText"` and use the text that it shows.
- On screen at the end of the narration: the sitemap as a tree, all four `url` elements expanded, the English article
  with its `loc` `https://www.example.com/en/welcome` and two `xhtml:link` elements (`hreflang="en"`, `hreflang="fr"`),
  the French article with its `loc` `https://www.example.com/fr/bienvenue` and the same two links, and a ring around the
  first `hreflang`.
- Label from: the addresses start with the Base url `.../OrchardCore.Sitemaps/Controllers/SitemapController.cs:77`;
  the elements `src/OrchardCore.Modules/OrchardCore.ContentLocalization/Sitemaps/SitemapUrlHrefLangExtendedMetadataProvider.cs:56-60`.

`loc-5`
- Actions: `await scene.ClearHighlightsAsync()`, then
  `await scene.HighlightAsync(scene.Page.GetByText("xmlns:xhtml").First)`, and hold.
- On screen at the end of the narration: the same tree, with a ring around the `xmlns:xhtml` attribute of the root
  `urlset` element, and the two articles still visible under it.
- Label from: `.../ContentLocalization/Sitemaps/SitemapUrlHrefLangExtendedMetadataProvider.cs:12-13,27` (the namespace attribute
  that the provider returns) and `src/OrchardCore.Modules/OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceBuilder.cs:36-39`
  (it is added to the root element).

## Clip `03-types` (chapter 03)

Starts on `/Admin/Sitemaps/List` (`scene.OpenAsync`). Nothing is saved: the clip leaves the page without selecting "Save".

`raz-4`
- Actions: `await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Manage Sources" }))`;
  then, on the source row ("Sitemap Content Types", with the badge "All"),
  `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item").GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }))`
  (the rows of the sources are the `li.list-group-item` of the page, and "Edit" must match the whole name: a name
  match is a substring match otherwise); then
  `await scene.ClickAsync(scene.Page.Locator("input.content-types-sitemap-index-all"))` to clear "Index All Content
  Types"; then
  `var selection = scene.Page.Locator("#index-selected-row");`, `await selection.GetByText("Select Content Types").WaitForAsync()`,
  and `await scene.HighlightAsync(selection.GetByText("Decoupled Blog Post", new() { Exact = true }))`. Hold.
  The scope `#index-selected-row` is needed: the page holds a second, hidden card "Select Content Types" (for "Limit
  Items") with the same type names, and `HighlightAsync` draws nothing when a locator matches more than one element.
  Clearing the box shows `#index-selected-row` through the script `content-types-sitemap-source.js` (it sets the
  style of the row when the box changes).
- On screen at the end of the narration: the page "Edit Sitemap Source", "Index All Content Types" cleared, the card
  "Select Content Types" with two rows, "Article" and "Decoupled Blog Post", both unchecked, and a ring around "Decoupled Blog Post".
- Label from: "Manage Sources" `.../OrchardCore.Sitemaps/Views/SitemapListEntry.Buttons.SummaryAdmin.cshtml:6`; "Edit" of
  a source `.../Views/SitemapSource.SummaryAdmin.cshtml:9`; "Edit Sitemap Source" `.../Views/Source/Edit.cshtml:6`;
  "Index All Content Types" `src/OrchardCore.Modules/OrchardCore.Contents/Views/Items/ContentTypesSitemapSource.Edit.cshtml:13-14`;
  "Select Content Types" `:73` (the hidden copy is at `:139`); the type names `:90` (the display name of the type; the
  hidden copy is at `:156`); the rows shown and hidden `:30,69,135` and
  `src/OrchardCore.Modules/OrchardCore.Contents/wwwroot/Scripts/content-types-sitemap-source/content-types-sitemap-source.js`;
  the list comes from the routeable types `src/OrchardCore.Modules/OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceDriver.cs:27-40`.

## Clip `03-sitemap` (chapter 03)

Starts on `/sitemap.xml` (`scene.OpenAsync`), the sitemap as a tree.

`raz-5`
- Actions: `await scene.HighlightAsync(scene.Page.GetByText("/blogpost/").First)`. Hold. Keep the actions shorter than
  the narration.
- On screen at the end of the narration: the sitemap as a tree with the two post elements visible, their `loc`
  `https://www.example.com/blogpost/4qa3qifpuw23ydesok50llsd9d` and `https://www.example.com/blogpost/42veyx0gsaxmknuj0ewu1rfc04`,
  each followed by `lastmod`, `changefreq` and `priority` and no `xhtml:link`, and a ring around the first address.
- Label from: the address is built by `src/OrchardCore.Modules/OrchardCore.Sitemaps/Services/RazorPagesContentTypeProvider.cs:37`
  (the Base url, then the link to the page); the posts have no `LocalizationPart`, so no links are added
  (`.../ContentLocalization/Sitemaps/SitemapUrlHrefLangExtendedMetadataProvider.cs:35-39`).

## Checks for private data

- The only domain on screen is `example.com`, and the only names are Maple Street Market, Spring Planting Calendar
  and Weekend Market Hours. The user is `admin` with `admin@example.com`.
- No password is typed or shown: the clips start signed in, with the `state.json` of `Recorder setup`. The password is
  `<password>` in this plan and in `VIDEO_PASSWORD`, outside git.
- The address bar is not part of the frame. No path of this machine, no connection string and no `App_Data` content is shown.
- Look at the contact sheet of every clip, and OCR the result as the skill asks, before assembling.
