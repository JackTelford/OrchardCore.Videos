# Recording plan: Import: Package and JSON Import

Everything a person needs to record this video without deciding anything. In this file a selector such as `#left-nav`
stands for `scene.Page.Locator("#left-nav")`, since the recorder's methods take locators. File and line references
(`src/...:NN`) point into the Orchard Core source (`OrchardCMS/OrchardCore`, commit 92db4a83) where each label is
spelled. Check every selector once with `Recorder probe` before recording: they were read from the source, not from a
running site.

## The site it needs

- **Orchard Core:** `src/OrchardCore.Cms.Web` of a checkout of `OrchardCMS/OrchardCore`, run on `http://localhost:5010`
  with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`).
- **Auto Setup** (environment variables, so no setup or sign-in form is recorded): recipe **Blog**, site name
  **Orchard Core**, tenant Default, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password
  `<password>` (chosen outside git, passed to the recorder in `VIDEO_PASSWORD`). This video is not about the Setup
  screen, so the usual state.json sign-in is used.
- **No outbound calls:** set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address before starting the site.
- **Features to enable:** none. The Blog recipe enables **Deployment** (and Contents, Settings and Roles) in its
  `feature` step (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). Open `/Admin/Features` once and
  confirm that **Deployment** is enabled.
- **Settings:** none to save. The site name must still be **Orchard Core** before clip `03-json`.
- **External services:** none.
- **Permissions:** the clips run as `admin`, who is an Administrator and holds the Import Data, Export Data and
  Manage deployment plans permissions by default (`src/OrchardCore.Modules/OrchardCore.Deployment/Permissions.cs:21`).

### Seed data (created outside the capture)

Create a folder outside both repositories, for example `ocvideo-seed`, with these two things. Neither is committed.

**1. The package, `maple-street-package.zip`.** The file name is lowercase `.zip`: the controller tests the name with a
case-sensitive `EndsWith(".zip")` (`src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/ImportController.cs:99`).
`Recipe.json` (this exact capitalization) must sit at the root of the zip, with no folder in front of it: the import
reads `Recipe.json` from the root of the extracted package
(`src/OrchardCore.Modules/OrchardCore.Recipes/Services/RecipeDeploymentTargetHandler.cs:38`). A package exported from
a plan has the same shape (`src/OrchardCore/OrchardCore.Deployment.Abstractions/DeploymentPlanResult.cs`).

Save this as `Recipe.json`:

```json
{
  "name": "maple-street-package",
  "displayName": "Maple Street package",
  "description": "Adds the Spring Menu article.",
  "issetuprecipe": false,
  "steps": [
    {
      "name": "Content",
      "data": [
        {
          "ContentItemId": "4ciy4zmwz8oftfv2ij1a6x769e",
          "ContentType": "Article",
          "DisplayText": "Spring Menu",
          "Latest": true,
          "Published": true,
          "Article": {},
          "TitlePart": {
            "Title": "Spring Menu"
          },
          "AutoroutePart": {
            "Path": "spring-menu"
          },
          "HtmlBodyPart": {
            "Html": "<p>Strawberry tarts and rye loaves arrive in April.</p>"
          }
        }
      ]
    }
  ]
}
```

The dates, the owner and the author are left out on purpose: the import sets them (`ModifiedUtc` to the time of the
import, owner and author to the signed-in user), so the article sorts to the top of Content Items
(`src/OrchardCore/OrchardCore.ContentManagement/Handlers/UpdateContentsHandler.cs:34-52`). Make the zip from the
folder that holds `Recipe.json`:

```bash
python -m zipfile -c maple-street-package.zip Recipe.json
python -m zipfile -l maple-street-package.zip      # must list Recipe.json, and nothing else
```

**2. The recipe text for `json-2`.** Keep it in a variable of the scene, or in a text file outside the repository.
Exactly this text, with these line breaks (eight lines):

```json
{
  "steps": [
    {
      "name": "Settings",
      "SiteName": "Maple Street Bakery"
    }
  ]
}
```

The `Settings` step sets the site name (`src/OrchardCore.Modules/OrchardCore.Settings/Recipes/SettingsStep.cs`, the
`SiteName` case); the step name is matched without regard to case.

### Starting state

- Fresh site from Auto Setup (the Blog recipe), signed in through `state.json`.
- Content, Content Items holds no item titled **Spring Menu**.
- Site name is **Orchard Core**.
- Admin menu groups closed, with the admin setting for the menu behavior left at its default, **Focused**
  (`src/OrchardCore/OrchardCore.Admin.Abstractions/Models/AdminMenuBehavior.cs`; do not change it). In that mode a page
  loads with only the sections that hold its own page open, and the stored state of the menu is ignored
  (`src/OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/sidebar.ts`, `applyNavStateFromPreferences`). So on `/Admin`, the
  dashboard, every group is closed and opening **Content**, **Tools** and **Settings** is on camera, and on any other page
  only the groups that hold that page are open. Nothing has to be cleared from `state.json` for this.
- The first-level groups of the menu are buttons with the class `nav-group-label` (they have no page of their own, so
  a click opens them: `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml`). Find a group with
  `scene.Page.Locator("#left-nav .nav-group-label")` filtered by its exact text, and the entries below it by their exact
  text inside `#left-nav`. Check each with `Recorder probe`.

## Before the first take: a dry run

Neither recipe above was run while this plan was written. On a throw-away copy of the site, import both once:
Tools, Deployments, Package Import with the zip, then Tools, Deployments, JSON Import with the text. Both must show
their success message, **Spring Menu** must be listed, and **Site name** must change. If either fails, write what
happened in `OPEN-QUESTIONS.md` and fix the recipe text above before recording.

While doing so, **time the delay** from the click on **Import** to the moment the success message is shown, for each
import, with a stopwatch or with `Recorder probe`. The page after an import is the first request on a site that has
just released its shell, so the delay is a property of the site and the same on every take. Write the two
measured values in `OPEN-QUESTIONS.md` (item 14). Then:

- **Both delays are 2 seconds or less:** record `package-4` and `json-3` as written below.
- **Either delay is over 2 seconds:** record that step with the form in "A slow page after an import", below.

Then stop the site, delete the copy, and take the snapshot below from the fresh site.

## Order of the clips, and the snapshot

1. Stop the site and copy `App_Data` and `state.json` aside. This is the **snapshot**.
2. Record `02-package`, then `03-json`. They do not depend on each other, so either order works.
3. **Both clips change the site:** `02-package` adds the article **Spring Menu**, and `03-json` changes the site name
   to **Maple Street Bakery**. Before any new take of a clip, stop the site, restore the snapshot, and start the site
   again, so that Content Items has no **Spring Menu** and the site name is **Orchard Core** again.
4. Each import releases the site's shell when its last step has run
   (`src/OrchardCore.Modules/OrchardCore.Recipes/Services/RecipeDeploymentTargetHandler.cs:46`; `IShellHost` builds a new
   shell for the next request), so the page after an import can take a few seconds. After an import, check that the recorder is still signed in.
5. The slides need no recording.

Success messages disappear by themselves after five seconds
(`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`; the toast is built in
`src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml`). The message is therefore on screen near the start of the
step that reads it, and gone when that step's narration ends. That is expected.

### A slow page after an import

The narration of `package-4` and `json-3` starts with "Select Import" and reads the message about three seconds into
the step. The message is shown for five seconds from the moment the page loads, so the page has to load within about
two seconds of the click. Recording the clip again does not change a slow page. When the measured delay is over two
seconds, cut the wait out of the video: the video then jumps from the cursor over **Import** to the loaded page with
its message. Use this form for the action of the step, instead of `ClickAndWaitForNavigationAsync`:

```csharp
var import = scene.Page.Locator("button.import");
await scene.MoveToAsync(import);
await scene.WaitForNavigationAsync(() => scene.OffCameraAsync(async () =>
{
    await import.ClickAsync();
    await scene.Page.Locator(".toast.message-success").WaitForAsync(new() { Timeout = 60000 });
}));
```

The wait is specific to the new state, the success message, which the page being left does not have. Look at the
contact sheet of the clip: the first frame after the jump must show the page with the message, and the video must not
freeze afterwards. If it freezes, write it in `OPEN-QUESTIONS.md` and stop; do not change the narration.

## Clip `02-package`

Starts on `/Admin` (the dashboard), signed in, every menu group closed. Opened before the capture with `scene.OpenAsync("/Admin")`.

- `package-1`: Click the **Content** group in `#left-nav` (the `nav-group-label` button with the exact text
  **Content**), then click **Content Items** with `ClickAndWaitForNavigationAsync`. Wait until the heading
  **Manage Content** is shown. On screen when the narration ends: the Manage Content page with the list of the blog
  site's items (for example the article **About**, the blog **Blog**, the menu **Main Menu** and the taxonomies **Tags**
  and **Categories**; the blog post is not listed, because the Blog Post type is not listable in the recipe) and no row
  titled **Spring Menu**. Look at the list once on the fresh site and write down what it shows.
  Labels: `src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65` (Content), `:68` (Content Items);
  `src/OrchardCore.Modules/OrchardCore.Contents/Views/Admin/List.cshtml:17` (Manage Content).
- `package-2`: Click the **Tools** group in `#left-nav`, then **Deployments**, then **Package Import** (last one with
  `ClickAndWaitForNavigationAsync`). Each by its exact text inside `#left-nav`; the entries are links to
  `/Admin/DeploymentPlan/Import/Index`. Wait until the heading **Import Deployment Package** is shown. On screen when
  the narration ends: the page titled **Import Deployment Package**, the label **Select a file** with an empty file
  field (no file chosen) and the **Import** button below it; the menu path Tools, Deployments, Package Import open
  and **Package Import** highlighted by the theme. Labels:
  `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:18` (Tools), `:19` (Deployments), `:25` (Package
  Import); `src/OrchardCore.Modules/OrchardCore.Deployment/Views/Import/Index.cshtml:2` (Import Deployment Package),
  `:8` (Select a file), `:13` (Import).
- `package-3`: Move the cursor to the file field with `MoveToAsync(scene.Page.GetByLabel("Select a file"))`, then
  choose the file with `scene.Page.GetByLabel("Select a file").SetInputFilesAsync("<seed folder>/maple-street-package.zip")`.
  Do not click the field: a headless browser has no file dialog to show. The scene API has no upload method, so this
  call is the way. On screen when the narration ends: the file field showing `maple-street-package.zip` (only the
  file name, never a folder path), the **Import** button not yet clicked. Source of the label and of the accepted
  types (`.zip,.json`): `Views/Import/Index.cshtml:8-9`.
- `package-4`: Click **Import** (`button.import`) with `ClickAndWaitForNavigationAsync`, then wait up to 60 seconds
  for `.toast.message-success` containing **Deployment package imported.** The click is the first action of the step,
  so the toast is shown while the narration reads its text. On screen when the narration ends: the page titled
  **Import Deployment Package** with an empty file field (the message may have gone). Message:
  `src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/ImportController.cs:117`; button
  `Views/Import/Index.cshtml:13`. If the toast is not visible at least two seconds after the click, record the clip
  again after restoring the snapshot, or, when the dry run measured a delay over two seconds, use the form in "A slow
  page after an import".
- `package-5`: Open **Content Items** again from the menu. The page is Package Import, so only **Tools** and
  **Deployments** are open and the **Content** group is closed: click the **Content** group first, then **Content Items**
  with `ClickAndWaitForNavigationAsync`. Wait until the heading **Manage Content** is shown, then
  `HighlightAsync` on the row titled **Spring Menu**. On screen when the narration ends: the Manage Content page with
  **Spring Menu** (type Article) as the first row, ringed in green. The list sorts by last modification by default
  (`src/OrchardCore.Modules/OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:97-98` and
  `:306-307`, descending by `ModifiedUtc`). If the row is not the first one, stop and write it in `OPEN-QUESTIONS.md`: the narration says it is at the
  top. Clear the highlight before leaving. Labels: as `package-1`.

## Clip `03-json`

Starts on `/Admin`, signed in, every menu group closed, site name **Orchard Core**. Opened before the capture with
`scene.OpenAsync("/Admin")`.

- `json-1`: Click the **Tools** group in `#left-nav`, then **Deployments**, then **JSON Import** (last one with
  `ClickAndWaitForNavigationAsync`; the link goes to `/Admin/DeploymentPlan/Import/Json`). Wait until the heading
  **JSON Import** and `.CodeMirror` are shown. On screen when the narration ends: the page titled **JSON Import**,
  the label **JSON**, an empty code editor with the line number 1 in its margin, and the **Import** button below
  it. Labels: `src/OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30` (JSON Import);
  `Views/Import/Json.cshtml:3` (JSON Import), `:15` (JSON), `:21` (Import); line numbers:
  `src/OrchardCore.Modules/OrchardCore.Deployment/Assets/ts/import-json.ts` (`lineNumbers: true`).
- `json-2`: Click the editor (`.CodeMirror`) with `ClickAsync` so it has focus, then paste the recipe text of the seed
  section in one action, as a paste would: `scene.Page.EvaluateAsync("text => document.querySelector('.CodeMirror').CodeMirror.setValue(text)", recipeText)`.
  If `CodeMirror` is not reachable that way, use `scene.Page.Keyboard.InsertTextAsync(recipeText)` after the click.
  Do not type the text key by key. On screen when the narration ends: the editor holding the eight lines of the
  recipe, numbered 1 to 8, with the step `Settings` and `Maple Street Bakery` readable. The narration describes the
  recipe and never reads it. The editor replaces the `Json` text area
  (`Assets/ts/import-json.ts`), so setting the text area directly shows nothing.
- `json-3`: Click **Import** (`button.import`) with `ClickAndWaitForNavigationAsync`, then wait up to 60 seconds for
  `.toast.message-success` containing **Recipe imported successfully!** The click is the first action of the step.
  On screen when the narration ends: the page titled **JSON Import** with an empty editor (line number 1 only); the
  message may have gone. Message: `Controllers/ImportController.cs:185`. The page returns to an empty editor because
  the action redirects to a fresh page (`ImportController.cs:208`). If the toast is not visible at least two seconds
  after the click, restore the snapshot and record the clip again, or, when the dry run measured a delay over two seconds,
  use the form in "A slow page after an import".
- `json-4`: Click the **Settings** group in `#left-nav`, then **General** (with `ClickAndWaitForNavigationAsync`;
  the link goes to `/Admin/Settings/general`). Wait until the heading **Settings** and the field are shown, then
  `HighlightAsync(scene.Page.GetByLabel("Site name"))`. On screen when the narration ends: the Settings page with the
  **Site name** field showing **Maple Street Bakery**, ringed in green; scroll the field into view first if it is not
  visible, and clear the highlight before scrolling. Labels:
  `src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36` (Settings), `:39` (General);
  `Views/Admin/Index.cshtml:4` (Settings, in that module), `Views/Settings-Site.Edit.cshtml:16` (Site name).

## Slides (no recording)

- `idea-1`: Slide "What an import does": four cards (Recipe, Package, Order, Restart); the card Recipe is bright.
- `idea-2`: The same slide with the card Package bright.
- `idea-3`: The same slide with the cards Order and Restart bright.
- `compare-1`: Slide "Package Import or JSON Import": two cards, one per page, saying what travels with the recipe.
- `fail-1`: Slide "When a step fails": three cards (the import stops, earlier steps have run, Package Import shows the
  errors).
- `perm-1`: Slide "Who can import": four cards (Import Data, Administrators, The menu, Trusted sources); the first
  two are bright.
- `perm-2`: The same slide with the last two cards bright.

The opening and closing cards are drawn from the storyboard; they need no recording either.

## Checks for private data

- The sign-in comes from `state.json`; the password is never typed on camera, and never written in this repository.
- The only account on screen is `admin` (`admin@example.com`). No real name, email, phone number or token appears.
- The file field shows the file name only (`maple-street-package.zip`). Look at the frame of `package-3`: no folder
  path, no user name of the machine.
- The site is fresh, so no mask script is needed. Content shown is the Blog recipe's sample content, **Spring Menu**
  and **Maple Street Bakery**, all invented.
- Run `review.py` on both clips and look at every contact sheet: no error message, no empty list, no browser
  password prompt, no toolbar or dialog of the machine.
- Before publishing, OCR the video as the skill describes (one frame per second, every scene change, then five frames
  per second) and search for the machine's user name, local paths and the real password.
