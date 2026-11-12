# Recording plan: diagnostics-error-pages

Two clips are recorded with the recorder: `02-builtin` (the built-in pages) and `04-admin` (a template written in the
admin). The other chapters are slides drawn by `slides.py`. Nothing in the video shows a terminal, a file tree or code
in a browser: the theme view, the Liquid view and the recipe are slides.

In this plan a selector such as `#Name` stands for `scene.Page.Locator("#Name")`. Every label and every file below was
read in the Orchard Core source at commit `92db4a83`; the file and line is given after each action. A file without a
folder is in `src/OrchardCore.Modules/OrchardCore.Diagnostics` for the clip `02-builtin`, and in
`src/OrchardCore.Modules/OrchardCore.Templates` for the clip `04-admin`. The SaaS recipe is
`src/OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json`. Use that commit, or a later one on which those lines
still read the same.

## The site it needs

| What | Value |
|---|---|
| Host | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Environment | `Development` or `Production`: neither clip causes an exception, and both give the same pages |
| `ORCHARD_APP_DATA` | An empty folder outside both repositories |
| Setup | Auto Setup from environment variables (below). No setup form and no login form are recorded |
| Recipe | SaaS. It sets the site theme `TheTheme` and the admin theme `TheAdmin` (`saas.recipe.json:39-42`). Do not use the Blog or the Agency recipe: their themes already carry a view `Views/HttpError-NotFound.liquid` (`TheBlogTheme`, `TheAgencyTheme`), which replaces the built-in Not Found page, so `02-builtin` would not show it |
| Tenant | `Default` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Administrator | user `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`, never typed on camera) |
| Database | Sqlite |
| Outbound calls | Blocked: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site calls no external service |

Auto Setup variables (the names are those of the Auto Setup documentation):

```bash
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=SaaS
```

The recorder signs in once, outside the capture (`Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`),
and keeps the session in `state.json`, which is git-ignored.

### Features to enable before recording

One: **Templates** (id `OrchardCore.Templates`, `Manifest.cs:9-15` of `OrchardCore.Templates`). The SaaS recipe enables
**Orchard Diagnostics** (`saas.recipe.json:17`, id `OrchardCore.Diagnostics`) but not Templates. After Auto Setup and
before `Recorder setup`, outside the capture: open **Tools > Features** (`AdminMenu.cs:26-27` of `OrchardCore.Features`),
find the row **Templates** (category Development; type "Templates" in the search box), select **Enable** on that row,
and confirm if the page asks. Its dependency, Liquid, is enabled with it. Then check that **Templates** and **Orchard
Diagnostics** are both listed as enabled. Do not enable anything else: the clips must not show other menu entries that
the narration does not mention. Do this before snapshot "A" below, so every take starts with Templates enabled.

### Settings, external services, seed data

- Settings: none. Leave every setting at the value the SaaS recipe gives it.
- External services: none.
- Seed data: none. There is no demo content: the SaaS recipe creates none. The only data of the video is the template
  that `04-admin` creates on camera.
- The path `/missing-page` must not exist. Before recording, open it once outside the capture: it must answer with the
  Not Found page (heading "The page could not be found.").

## Order of the clips and snapshots

1. Run Auto Setup, enable **Templates** in **Tools > Features** (see "Features to enable before recording"), run
   `Recorder setup`, then stop the site.
2. Copy the site's `App_Data` folder and `state.json` aside. This is the snapshot "A". Start the site again.
3. Record `02-builtin`. It changes nothing on the site.
4. Record `04-admin`. It **changes the site**: it creates the template `HttpError__NotFound`. From then on `/missing-page`
   shows the heading of that template instead of the built-in page.
5. To take `02-builtin` again after `04-admin`, stop the site, restore snapshot "A", start the site, then record.
   To take `04-admin` again, do the same, or delete the template first (**Design > Templates**, then **Delete** on the
   `HttpError__NotFound` row; if a dialog asks for confirmation, confirm it, and wait for the message "Template deleted
   successfully.", `TemplateController.cs:322`).

`02-builtin` and `04-admin` do not depend on each other except for the state above. No `PrepareAsync` is needed.

## Clip `02-builtin`

Starts on `/` (the home page of `TheTheme`, a welcome message), opened with `scene.OpenAsync("/")` before `StartCaptureAsync()`. The
session is the signed-in administrator; the front-end pages look the same for an anonymous visitor.

`builtin-1`
- Actions: `scene.NavigateAsync("/missing-page")`. Wait until `h1` with the text "The page could not be found." is
  visible. Then `scene.HighlightAsync(h1, "/missing-page")`.
- On screen when the narration ends: the heading "The page could not be found." inside the layout of `TheTheme`
  (its navigation bar with the site name `Orchard Core` above the heading), with a green ring and the caption "/missing-page".
- Where it comes from: the text is `Views/HttpError-NotFound.cshtml:1` of `OrchardCore.Diagnostics`; the page is drawn
  by `DiagnosticsStartupFilter.cs:29` (`/Error/{0}`) and `DiagnosticsController.cs:9-14`.

`builtin-2`
- Actions: `scene.ClearHighlightsAsync()`, then `scene.NavigateAsync("/Error/403")`. Wait until `h1` with the text
  "You do not have access to this resource." is visible. Then `scene.HighlightAsync(h1, "/Error/403")`.
- On screen when the narration ends: the heading "You do not have access to this resource." in the same layout,
  with a green ring and the caption "/Error/403".
- Where it comes from: the text is `Views/HttpError-Forbidden.cshtml:1`; the route is `Startup.cs:17-22` of
  `OrchardCore.Diagnostics` (`Error/{status?}`); the Users module uses `/Error/403` as its access denied path
  (`OrchardCore.Users/Startup.cs:131`).

`builtin-3`
- Actions: `scene.ClearHighlightsAsync()`, then `scene.NavigateAsync("/Error/405")`. Wait until `h1` with the text
  "An error occurred while executing this request." is visible. Then `scene.HighlightAsync(h1, "/Error/405")`.
- On screen when the narration ends: the heading "An error occurred while executing this request." in the same
  layout, with a green ring and the caption "/Error/405".
- Where it comes from: the text is `Views/HttpError.cshtml:3`. The status 405 has no view of its own, so the fallback
  view `HttpError` is used (`HttpErrorShapeViewModel.cs:19`, `HttpErrorAlternatesFactory.cs:16-17`).

## Clip `04-admin`

Starts on `/Admin` (the admin home), opened with `scene.OpenAsync("/Admin")` before `StartCaptureAsync()`. The admin
menu is left as the page shows it: opening **Design** is part of the first step.

`admin-1`
- Actions:
  1. Click the admin menu entry labeled "Design" (`scene.ClickAsync`).
  2. Click the entry labeled "Templates" that appears under it (`scene.ClickAndWaitForNavigationAsync`). The Templates
     list opens at `/Admin/Templates`.
  3. Click the link labeled "Add Template" (`a.btn.create`, `scene.ClickAndWaitForNavigationAsync`). Wait until `#Name`
     is visible and `#Content_editor .monaco-editor` is present.
  - If the menu entries cannot be found with those labels, skip clicks 1 and 2 and use
    `scene.NavigateAsync("/Admin/Templates")`, then do click 3. Do not change anything else.
- On screen when the narration ends: the page with the breadcrumb "Create Template", the empty fields Name,
  Description and Content (a code editor), and the Save button.
- Where it comes from: "Design" and "Templates" are `AdminMenu.cs:19-20` of `OrchardCore.Templates`; "Add Template"
  is `Views/TemplateCreateButton.cshtml:6`; "Create Template" is `Views/Template/Create.cshtml:8`.

`admin-2`
- Actions:
  1. `scene.TypeAsync(#Name, "HttpError__NotFound", 40)`. Leave Description empty.
  2. Wait 500 ms, then `scene.HighlightAsync(#Content_editor, "Liquid")`.
- On screen when the narration ends: the Name field holding `HttpError__NotFound`, and a green ring with the caption
  "Liquid" around the Content editor, above the hint "The Liquid template."
- Where it comes from: the labels "Name" and "Content" and the hints "The name of the template." and "The Liquid
  template." are `Views/Template/Create.cshtml:18`, `:22`, `:35`, `:40`.

`admin-3`
- Actions:
  1. `scene.ClearHighlightsAsync()`.
  2. Click inside the editor (`scene.ClickAsync(#Content_editor .monaco-editor)`), so it has the focus.
  3. Insert this exact text with `scene.Page.Keyboard.InsertTextAsync(...)`, on one line:
     `<h2 class="text-danger">{{ "We could not find that page." | t }}</h2>`
  4. Check that the hidden field `#Content` holds exactly that text. If it does not, select all in the editor
     (`Control+A`), press `Delete`, type the text with `scene.Page.Keyboard.TypeAsync(text, 25)`, and check again. If it
     still differs, stop and report it (see `OPEN-QUESTIONS.md`, question 5).
  5. Click the button labeled "Save" (`button.save`, `scene.ClickAndWaitForNavigationAsync`). Wait until a link with
     the text "HttpError__NotFound" is visible in the list.
- On screen when the narration ends: the Templates list with one row, `HttpError__NotFound` (the success message
  `The "HttpError__NotFound" template has been created.` may still be visible above it).
- Where it comes from: "Save" is `Views/Template/Create.cshtml:53`; the message is `TemplateController.cs:204`;
  the filter `t` is `OrchardCore.Liquid/Startup.cs:44`; the redirect to the list is `TemplateController.cs:212`.

`admin-4`
- Actions: `scene.NavigateAsync("/missing-page")`. Wait until `h2.text-danger` with the text
  "We could not find that page." is visible. Then `scene.HighlightAsync(h2, "/missing-page")`.
- On screen when the narration ends: the heading "We could not find that page." in red (the `text-danger` class of
  Bootstrap, which `TheTheme` loads), inside the same theme layout as in `builtin-1`, with a green ring and the caption "/missing-page".
  The built-in heading "The page could not be found." must not be on the page.
- Where it comes from: a template of the admin is looked up before a view of a theme or of a module, for the same
  name (`OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:349-373`), and
  `TemplatesShapeBindingResolver.cs:36-65` returns it for the shape type `HttpError__NotFound`.

## Slide steps

These steps are not recorded. Each line says what the slide shows.

- `flow-1`: "From a status to a page", four cards (An error status, The Error route, The HttpError shape, A template);
  the first two are bright.
- `flow-2`: "The shape and its alternates", the constructor of `HttpErrorShapeViewModel` and the method that returns
  the two alternates `HttpError__{code}` and `HttpError__{status}`.
- `flow-3`: "The views of the module", the six shape names with their view files, and three cards (statuses with a
  view, any other status, a status of your own).
- `theme-1`: "A view in the theme", the Razor view of the theme above the Razor view of the module it replaces, and
  three cards (shape name, file name, folder).
- `theme-2`: "The same page in Liquid", a Liquid view that uses the `t` filter, and three cards.
- `theme-3`: "The fallback view", the view `HttpError.cshtml` with its model, and two cards (Code, HttpStatusCode).
- `recipe-1`: "Templates in a recipe", the `Templates` recipe step with `HttpError__NotFound`, and a card for the All
  Templates deployment step.
- `rank-1`: "Which name is tried first", three cards: the name of the status, the number, the fallback.
- `rank-2`: "Which source wins for a name", three cards: a template from the admin, a view in the active theme, a view in
  the module.
- `limits-1`: "No page for a file path", the middleware that disables the status code pages for a known file extension,
  and a card.
- `limits-2`: "Unhandled exceptions", the exception handler registered outside Development and the route of the Error
  action, with two cards (Other environments, Development).

## Checks for private data

- The only name and email in the site are `admin` and `admin@example.com`. The admin user name appears at the top of
  the admin pages: that is expected.
- The password is never typed or shown: the sign-in is done once, outside the capture, and kept in `state.json`.
- The captions show paths only (`/missing-page`, `/Error/403`, `/Error/405`), never a host name or a port.
- Look at every contact sheet: the pages must show no real person, no real data of any kind (the site has no
  demo content), no error message other than the error pages of the video, and no developer exception page.
- Scan the finished video by OCR for the password, "@" and any host name other than `example.com`.
