# Recording plan: recipes-write

The video is a developer video. Its chapters 01 to 05 and 07 are slides, drawn by `slides.py` from the storyboard: they
need no recording. Only chapter 06, the clip `06-run`, is a recording of the admin UI and of the front end. Everything
a recorder needs is on this page: it holds no choices. The `Orchard Core source` references below are paths in the
`OrchardCMS/OrchardCore` checkout the catalog was read from (commit `92db4a83`), as `file:line`.

Selectors below stand for `scene.Page.Locator(...)`, since the recorder's methods take locators.

## 1. The site it needs

A throwaway local site, on a fresh `App_Data`, with fake data only.

| | |
|---|---|
| Source | A checkout of `OrchardCMS/OrchardCore` at the commit above, run from `src/OrchardCore.Cms.Web` |
| Port | `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Data folder | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Provisioning | Auto Setup through environment variables (the web project registers it: `src/OrchardCore.Cms.Web/Program.cs:9`); see the `orchardcore-tester` skill of that repository |
| Tenant | `Default` |
| Recipe | `Blog` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| User | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site calls no outside service |
| External services | None |

The Auto Setup environment variables (names from `src/docs/reference/modules/AutoSetup/README.md`):

```
OrchardCore__AutoSetup__Tenants__0__ShellName=Default
OrchardCore__AutoSetup__Tenants__0__SiteName=Orchard Core
OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

No feature needs to be enabled before the recording: the recipe enables what it needs, and the Blog recipe
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`) already enables every feature that the `feature` step of
the recipe lists. The step therefore enables no feature that is missing (the features manager skips a dependency that is
already enabled: `src/OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:163-168`). It may still update the
shell descriptor and reload the tenant, because the features manager first marks every enabled feature that is only
enabled by dependency for disabling (`ShellDescriptorFeaturesManager.cs:50-56`); the first request after the step can
therefore be slow (OPEN-QUESTIONS.md, item 15). Do not enable anything else.

## 2. The module that holds the recipe (seed, made outside the capture)

The recipe has to live in the `Recipes` folder of a module or a theme of the site
(`src/OrchardCore/OrchardCore.Recipes.Core/Services/RecipeHarvester.cs:61`). Add one module, `Bakery.Starter`, to the
throwaway checkout. None of these files is committed anywhere: they exist only on the machine that records.

1. Create the folder `src/OrchardCore.Modules/Bakery.Starter/` in the checkout, with these four files.

   `Bakery.Starter.csproj` (modeled on `src/OrchardCore.Modules/OrchardCore.Recipes/OrchardCore.Recipes.csproj`):

   ```xml
   <Project Sdk="Microsoft.NET.Sdk.Razor">

     <PropertyGroup>
       <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
     </PropertyGroup>

     <ItemGroup>
       <FrameworkReference Include="Microsoft.AspNetCore.App" />
     </ItemGroup>

     <ItemGroup>
       <ProjectReference Include="..\..\OrchardCore\OrchardCore.Module.Targets\OrchardCore.Module.Targets.csproj" />
     </ItemGroup>

   </Project>
   ```

   `Manifest.cs` (the properties are those of `src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Module/Manifest.cs`):

   ```csharp
   using OrchardCore.Modules.Manifest;

   [assembly: Module(
       Name = "Bakery Starter",
       Author = "Greenfield Bakery",
       Website = "https://example.com",
       Version = "0.0.1",
       Description = "Starter content for the Greenfield Bakery site.",
       Category = "Content Management"
   )]
   ```

   `Recipes/bakery-pages.recipe.json`:

   ```json
   {
     "name": "BakeryPages",
     "displayName": "Bakery Pages",
     "description": "Creates the Bakery Page content type and a home page.",
     "author": "Greenfield Bakery",
     "website": "https://example.com",
     "version": "1.0.0",
     "tags": [ "bakery", "content" ],
     // The variables are evaluated the first time a step reads them, and then kept.
     "variables": {
       "homePageId": "[js:uuid()]",
       "timestamp": "[js:new Date().toISOString()]"
     },
     "steps": [
       {
         "name": "feature",
         "enable": [
           "OrchardCore.Contents",
           "OrchardCore.ContentTypes",
           "OrchardCore.ContentFields",
           "OrchardCore.Title",
           "OrchardCore.Html",
           "OrchardCore.HomeRoute",
           "TheAdmin",
           "TheBlogTheme"
         ],
         "disable": []
       },
       {
         "name": "themes",
         "admin": "TheAdmin",
         "site": "TheBlogTheme"
       },
       {
         "name": "settings",
         "SiteName": "Greenfield Bakery",
         "HomeRoute": {
           "Action": "Display",
           "Controller": "Item",
           "Area": "OrchardCore.Contents",
           "ContentItemId": "[js:variables('homePageId')]"
         }
       },
       {
         "name": "ContentDefinition",
         "ContentTypes": [
           {
             "Name": "BakeryPage",
             "DisplayName": "Bakery Page",
             "Settings": {
               "ContentTypeSettings": {
                 "Creatable": true,
                 "Draftable": true,
                 "Versionable": true,
                 "Listable": true
               }
             },
             "ContentTypePartDefinitionRecords": [
               {
                 "PartName": "TitlePart",
                 "Name": "TitlePart",
                 "Settings": { "ContentTypePartSettings": { "Position": "0" } }
               },
               {
                 "PartName": "HtmlBodyPart",
                 "Name": "HtmlBodyPart",
                 "Settings": { "ContentTypePartSettings": { "Position": "1" } }
               },
               {
                 "PartName": "BakeryPage",
                 "Name": "BakeryPage",
                 "Settings": { "ContentTypePartSettings": { "Position": "2" } }
               }
             ]
           }
         ],
         "ContentParts": [
           {
             "Name": "BakeryPage",
             "Settings": {},
             "ContentPartFieldDefinitionRecords": [
               {
                 "FieldName": "TextField",
                 "Name": "OpeningHours",
                 "Settings": {
                   "ContentPartFieldSettings": { "DisplayName": "Opening Hours", "Position": "0" },
                   "TextFieldSettings": { "Hint": "When the bakery is open." }
                 }
               }
             ]
           }
         ]
       },
       {
         "name": "content",
         "data": [
           {
             "ContentItemId": "[js:variables('homePageId')]",
             "ContentType": "BakeryPage",
             "DisplayText": "Welcome to Greenfield Bakery",
             "Latest": true,
             "Published": true,
             "CreatedUtc": "[js:variables('timestamp')]",
             "ModifiedUtc": "[js:variables('timestamp')]",
             "PublishedUtc": "[js:variables('timestamp')]",
             "Owner": "[js:parameters('AdminUserId')]",
             "Author": "[js:parameters('AdminUsername')]",
             "TitlePart": { "Title": "Welcome to Greenfield Bakery" },
             "HtmlBodyPart": { "Html": "[file:text('Snippets/welcome.html')]" },
             "BakeryPage": { "OpeningHours": { "Text": "Tuesday to Sunday, 7 am to 3 pm" } }
           }
         ]
       }
     ]
   }
   ```

   `Recipes/Snippets/welcome.html` (one line):

   ```html
   <p>Fresh bread and pastries, baked every morning.</p>
   ```

   The slides of the video show trimmed parts of this recipe (`// ...` marks what a slide leaves out). Keep the property
   names and values identical to the ones on the slides. The panels titled `example.recipe.json`,
   `Properties of a step` and `greenfield-site.recipe.json` show examples that are not part of this file.

2. Add the module to the web project: in `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`, add to the `ItemGroup`
   that holds the `ProjectReference` lines:

   ```xml
   <ProjectReference Include="..\OrchardCore.Modules\Bakery.Starter\Bakery.Starter.csproj" />
   ```

3. Stop the site if it runs (by its process ID, never by image name), build `src/OrchardCore.Cms.Web`, set the
   environment of section 1, and start it. The first request to `http://localhost:5010/` runs Auto Setup with the Blog
   recipe. Wait until the Blog home page loads.
4. Sign the recorder in once, outside the capture: `Recorder setup` with `VIDEO_USER_NAME=admin` and
   `VIDEO_PASSWORD=<password>`. It writes `state.json` in the video folder (git-ignored).
5. Do not run the recipe. Stop the site and copy its `App_Data` folder aside, with `state.json`: this is the snapshot.
   Start the site again.

The module stays **disabled**. Orchard Core lists the recipes of every available module, enabled or not
(`src/OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:164-166`, and
`src/OrchardCore/OrchardCore/Shell/ShellFeaturesManager.cs:26`). If the recipe is missing from Tools > Recipes,
write it in `OPEN-QUESTIONS.md` before changing anything else.

## 3. Order of the clips, and what changes the site

Only one clip is recorded: `06-run`. It **changes the site**: it saves the site name, the home route, a content type, a
content part and a content item. Before every new take, stop the site, restore the snapshot (`App_Data` and
`state.json`), and start the site again. A take that was started and then abandoned counts as a take: restore first.

Run `narrate.py` before recording, so that `durations.json` holds the length of each step.

## 4. Clip `06-run`

Starts on `/Admin/Recipes` (the page of Tools > Recipes; the route is declared in
`src/OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:19` and `:61`). Open it before
`StartCaptureAsync`, signed in through `state.json`. The menu is not part of the story: the narration names it and the
page is already open. The admin menu stays as it is.

The page lists the recipes of every module and theme, one group per module, with the search box on top. Leave the
layout of the list as the site shows it.

### `run-1`

Narration: the page, grouped by module; searching for Bakery; the Bakery Starter module holds the recipe.

Actions:

1. `TypeAsync` into the search box `#search-box` (placeholder "Search"; it takes the focus on load) the text
   `Bakery`, typed visibly. The page filters on key up (`Assets/ts/recipes-index.ts:7`), and `TypeAsync` presses the keys
   one after the other, so every letter filters the list.
2. Wait until exactly one group is left visible: the group whose `h4` reads "Bakery Starter".
3. `HighlightAsync` on that `h4`, with no caption, then `ClearHighlightsAsync`.

When the narration ends, the screen shows: the search box with "Bakery"; one group headed "Bakery Starter"; inside it the
row "Bakery Pages" with its description "Creates the Bakery Page content type and a home page.", the tag badges
"bakery" and "content", and the "Run" button (a small `btn-primary` button: the code does not set a colour name for it).
No other group is visible.

Labels from the source: the group heading is the name of the module's feature, which is the `Name` of its manifest
(`src/OrchardCore.Modules/OrchardCore.Recipes/Views/Admin/Index.cshtml:34`,
`src/OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:81`,
`src/OrchardCore/OrchardCore/Extensions/Features/FeaturesProvider.cs:87`); "Search"
(`Views/Admin/Index.cshtml:15`); the display name and the description
(`Views/RecipeEntry.Fields.SummaryAdmin.cshtml`, `Views/RecipeEntry.Description.SummaryAdmin.cshtml:6-8`); the tag
badges (`Views/RecipeEntry.DefaultTags.SummaryAdmin.cshtml:11-15`); "Run"
(`Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:18`).

### `run-2`

Narration: select Run, confirm with Yes; the steps run in order.

Actions:

1. `ClickAsync` the Run button `#btn-run-BakeryPages` (its id is `btn-run-` followed by the recipe's name;
   `Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:6`), with `pauseAfter: 900`.
2. Wait until the confirmation dialog `#confirmRemoveModal` is visible: title "Recipes" (`.modal-title`), text "Are you
   sure you want to run the recipe?" (`.modal-body p`), buttons "Yes" (`#modalOkButton`) and "No"
   (`#modalCancelButton`). The texts come from `Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:13-16`, the markup of the
   dialog from `src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-36`. Hold on it for about one second.
3. `ClickAndWaitForNavigationAsync` the dialog's "Yes" button `#modalOkButton`. It submits a POST form that the page
   script builds from the antiforgery token of the page (`TheAdmin.ts:114-187`), then the browser lands on
   `/Admin/Recipes` again.
4. The site reloads its services after a recipe, so the first page can take several seconds:
   `OffCameraAsync` waiting until the page shows the text "has been run successfully". The condition is specific to the
   new page, so the wait is cut out of the clip.

When the narration ends, the screen shows the Tools > Recipes page again, with the success notification under the
header. (Keep the step's actions shorter than its narration: the off-camera wait costs no time in the clip.)

### `run-3`

Narration: the success notification names the recipe; a failed step is reported with its errors.

Actions: `HighlightAsync` on the notification (the element that contains "has been run successfully"), then hold.

When the narration ends, the screen shows the notification "The recipe 'Bakery Pages' has been run successfully." with
the highlight ring of the recorder around it. If the notification shows an error instead, stop: the take is void, and the cause goes in
`OPEN-QUESTIONS.md`.

Label from the source: `src/OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:142` (success,
with the recipe's display name) and `:148` (failure, with the errors of the failed step).

### `run-4`

Narration: the home page shows the Bakery Page the recipe created.

Actions:

1. `ClearHighlightsAsync`, then `NavigateAsync("/")`.
2. `OffCameraAsync` waiting until the heading text "Welcome to Greenfield Bakery" is visible (the first request after
   a reload can be slow).
3. Hold on the page without scrolling, so the title, the text and the opening hours are all in view.

When the narration ends, the screen shows the home page of the site, in the Blog theme, with the site name "Greenfield
Bakery" in the navigation bar (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`, `Site.SiteName`), the title
"Welcome to Greenfield Bakery" in the page header (`Views/Content.liquid`, `Model.ContentItem.DisplayText`), the text
"Fresh bread and pastries, baked every morning." and the opening hours "Tuesday to Sunday, 7 am to 3 pm". The text field
shows its text only, with no label "Opening Hours" (`src/OrchardCore.Modules/OrchardCore.ContentFields/Views/TextField.cshtml`).
If a part is missing from the page, write it in `OPEN-QUESTIONS.md`.

Source: the values come from the recipe of section 2; the home route is set by the `settings` step
(`src/OrchardCore.Modules/OrchardCore.Settings/Recipes/SettingsStep.cs:108-110`).

## 5. Cards

- `intro` (chapter 00) and `outro` (chapter 99): the opening and the closing card, drawn by `assemble.py` from the
  chapter titles and subtitles. Nothing to record.

## 6. Slides

These steps are slides drawn by `slides.py`. Nothing to record; each line says what the slide shows.

- `recipe-1`: the folder of a module with its `Recipes` folder, the recipe file and a `Snippets` folder, and three cards
  (file name, folder, subfolders).
- `recipe-2`: three cards: properties, variables, steps.
- `recipe-3`: a `steps` array with a `feature` step and a `themes` step, and three cards (name, other properties,
  unknown name).
- `header-1`: the recipe header with the name, display name and description lines bright.
- `header-2`: the same header with the tags, author, website and version lines bright.
- `header-3`: three cards: a setup recipe, a recipe without the property, a recipe tagged hidden.
- `header-4`: a header with a comment and a trailing comma.
- `vars-1`: three properties of a step (a `js` script, a `file` script and a script with an unknown prefix), and three
  cards for the brackets and the two prefixes.
- `vars-2`: a `variables` object, and the line of a step that reads one of its variables.
- `vars-3`: three cards: first read, kept, one identifier.
- `vars-4`: a variable that calls `configuration` with a default, and a property that calls `parameters`.
- `vars-5`: the `Html` property that reads a file, the file `Snippets/welcome.html`, and one card (a variable cannot
  read a file).
- `steps-1`: the `feature` step with its `enable` and `disable` lists.
- `steps-2`: the `themes` step, and two cards (enable first; missing or empty).
- `steps-3`: the `settings` step with `SiteName` and `HomeRoute`, and two cards.
- `content-1`: the `ContentDefinition` step with the BakeryPage type and its three parts.
- `content-2`: the `ContentParts` entry with the text field OpeningHours.
- `content-alter`: three cards: an existing type or part is altered, settings are merged, the settings of a type merge
  only with a DisplayName.
- `content-3`: the `content` step with one item and its flags.
- `content-4`: the part data of the item: owner, author, title, HTML body and the opening hours.
- `more-1`: three cards: the Roles, Templates and Layers steps.
- `more-2`: a `steps` array that ends with the `ReloadTenant` step, and two cards.
- `reuse-1`: the `recipes` step of another recipe (`greenfield-site.recipe.json`) with the name `BakeryPages`, and three
  cards.
- `reuse-2`: a data migration class that runs a recipe with the recipe migrator, and the line of the `Startup` class that
  registers the migration.
- `reuse-3`: two cards: a recipe run by a data migration (the import waits), a recipe run from Tools, Recipes (the
  import is immediate).

## 7. Checks for private data

Before the clip goes into the video, look at the contact sheet of `06-run` (`review.py`), then OCR the video at one
frame per second and at every scene change, and search the text.

- The only person is `admin`; the only email is `admin@example.com`; the only business is Greenfield Bakery, and the
  only domain is `example.com` (and `localhost`).
- No password appears anywhere: not in the address bar, not in a form, not in the notification.
- No path of the machine, no user folder name and no real site name appears on screen.
- The Auto Setup environment variables, `state.json` and the copies of `App_Data` stay out of git.
