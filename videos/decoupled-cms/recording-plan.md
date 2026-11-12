# Recording plan: Decoupled CMS with Razor Pages

How to record the clips of `storyboard.json` without deciding anything. Everything in this file was checked against the
Orchard Core source (`OrchardCMS/OrchardCore`) at commit `92db4a83`, called `<checkout>` below; `file:line` references are
relative to its root. In this file a selector stands for `scene.Page.Locator(...)`.

This is a **developer** video: the recorder drives a browser only. The terminal commands, `Program.cs`, `.cshtml` code
and the browser console are slides (`slides.py`) and are not recorded. The eight recorded clips are `03-setup`,
`04-type`, `05-post`, `06-page`, `07-alias-part`, `07-alias-page`, `07-alias-pattern` and `08-preview`.
The first one needs a site that is not set up yet.

## The site it needs

The video is about adding Orchard Core to an ASP.NET Core Razor Pages project, so the site is **not**
`OrchardCore.Cms.Web`: it is a throwaway Razor Pages host, with the Setup screen on camera.

| What | Value |
| --- | --- |
| Host project | `<checkout>/src/OrchardSite/` (a folder of the local checkout, **never added to the solution and never committed**). Delete it when the recording is done |
| Auto Setup | **None.** The Setup screen is recorded (`03-setup`), so the host has no `AddSetupFeatures("OrchardCore.AutoSetup")`, the AutoSetup environment variables are not set, and there is no `state.json` until `Recorder setup` has run (see the order below). |
| Tenant | `Default`, the only one |
| Site name, time zone | `Orchard Notes`, `America/Los_Angeles`. Both are typed or chosen on camera in `03-setup` |
| Recipe | `Blank site` (name `Blank`), chosen on camera in `03-setup` |
| Database | `Sqlite`, the default of the Setup screen |
| Super user | user name `admin`, email `admin@example.com`, password in `VIDEO_PASSWORD` (chosen outside git, never written in a file of the repository, and never shown: the fields show dots) |
| Features to enable | None. The Blank site recipe already enables `OrchardCore.ContentTypes`, `OrchardCore.Contents`, `OrchardCore.Title`, `OrchardCore.Markdown`, `OrchardCore.Alias` and `OrchardCore.ContentPreview` (`src/OrchardCore.Themes/TheAdmin/Recipes/blank.recipe.json`) |
| Settings | None to save |
| External services | None. Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so nothing is ever sent out |
| Environment | `ASPNETCORE_ENVIRONMENT=Development`, `ASPNETCORE_URLS=http://localhost:5010` (`VIDEO_BASE_URL`), `ORCHARD_APP_DATA=<an empty folder outside both repositories>` |

### Create the host project

1. In `<checkout>/src`, run `dotnet new webapp -o OrchardSite` (the Razor Pages template, as the slide of step `add-1` says).
2. Replace `OrchardSite.csproj` with the following. It is the pattern of `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`
   (its `Import` lines and its project reference), with the `OrchardCore.Application.Cms.Core.Targets` project instead of
   `OrchardCore.Application.Cms.Targets`, because the Setup screen of the video lists only the Blank site and Headless site recipes.
   The checkout's own `Directory.Build.props` and package versions then apply to the host. Nullable reference types are off and warnings are not errors,
   because the pages of the guide have a property without an initial value (`Id`, `Slug`) and read a result that can be null, and the call of
   `GetContentItemByIdAsync` can raise an obsolete warning (`OPEN-QUESTIONS.md`, question 2).

   ```xml
   <Project Sdk="Microsoft.NET.Sdk.Web">

     <Import Project="..\OrchardCore\OrchardCore.Application.Cms.Core.Targets\OrchardCore.Application.Cms.Core.Targets.props" />

     <PropertyGroup>
       <TargetFrameworks>$(CommonTargetFrameworks)</TargetFrameworks>
       <Nullable>disable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
       <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
     </PropertyGroup>

     <ItemGroup>
       <ProjectReference Include="..\OrchardCore\OrchardCore.Application.Cms.Core.Targets\OrchardCore.Application.Cms.Core.Targets.csproj" />
     </ItemGroup>

     <Import Project="..\OrchardCore\OrchardCore.Application.Cms.Core.Targets\OrchardCore.Application.Cms.Core.Targets.targets" />
     <Import Project="..\OrchardCore\OrchardCore.Application.Targets\OrchardCore.Application.Targets.targets" />
     <Import Project="..\OrchardCore.Build\OrchardCore.Commons.targets" />

   </Project>
   ```

3. Replace `Program.cs` with the code of the slide of step `add-2` (the bare minimum of the guide):

   ```csharp
   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddOrchardCms();

   var app = builder.Build();

   app.UseStaticFiles();
   app.UseOrchardCore();

   app.Run();
   ```

4. Leave every other file of the template (`Pages/Index.cshtml`, `Pages/Shared/_Layout.cshtml`, `wwwroot`) as the template made it.
5. Build once (`dotnet build src/OrchardSite`). Start the site with `dotnet run --project src/OrchardSite --no-launch-profile`,
   and note the process ID: stop it by that ID, never by image name.

If the build fails, or the Setup screen does not list the Blank site recipe, stop and read `OPEN-QUESTIONS.md` (question 2).

### The two versions of `Pages/BlogPost.cshtml`

The recorder never types code on camera. Between clips, the person recording replaces the file below, **stops the site, and starts
it again** (the Razor Pages are compiled at build; `App_Data` keeps the site). Both versions are the code of the slides.

Version **B** (live for `06-page`; slides `render-1` to `render-4`):

```html
@page "/blogpost/{id}"
@using OrchardCore
@inject IOrchardHelper Orchard

@{
    var blogPost = await Orchard.GetContentItemByIdAsync(Id);
}

<h1>@blogPost.DisplayText</h1>

<p>@await Orchard.MarkdownToHtmlAsync((string) blogPost.Content.MarkdownBodyPart.Markdown)</p>

@functions
{
    [FromRoute]
    public string Id { get; set; }
}
```

Version **C** (live for `07-alias-page`, `07-alias-pattern` and `08-preview`; slide `alias-3`):

```html
@page "/blogpost/{slug}"
@using OrchardCore
@inject IOrchardHelper Orchard

@{
    var blogPost = await Orchard.GetContentItemByHandleAsync($"alias:{Slug}");
}

<h1>@blogPost.DisplayText</h1>

<p>@await Orchard.MarkdownToHtmlAsync((string) blogPost.Content.MarkdownBodyPart.Markdown)</p>

@functions
{
    [FromRoute]
    public string Slug { get; set; }
}
```

There is no `BlogPost.cshtml` before the clip `06-page`: the clips before it never open `/blogpost/...`.

### The fake seed data

Everything is created by the clips themselves, on camera, with these values. Nothing is seeded outside the capture.

| Where | Value |
| --- | --- |
| Site name | `Orchard Notes` |
| Super user | `admin`, `admin@example.com`, `VIDEO_PASSWORD` |
| Content type | Display Name `Blog Post`, Technical Name `BlogPost` (filled in by the page), parts Title and MarkdownBody |
| Blog post title | `Winter Pruning Notes` |
| Blog post body (Markdown) | `Prune apple trees while they are dormant.` |
| First alias (`alias-2`) | `pruning` |
| Generated alias (`alias-6`) | `winter-pruning-notes` (made by the Alias part from the title) |
| Preview pattern (`preview-2`) | `/blogpost/{{ ContentItem.Content.AliasPart.Alias }}` |
| Title typed in the editor (`preview-3`) | ` Revised` added at the end of the title, never saved |

The content item identifier is generated by Orchard Core, and the address of the edit page of the blog post holds it:
`/Admin/Contents/ContentItems/<id>/Edit` (`src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:502`).
After `05-post`, open Content, Content Items, open the blog post with its Edit button, copy the 26 characters between
`/ContentItems/` and `/Edit` in the address bar of an ordinary browser window signed in as `admin`, and use them wherever this plan
says `<id>`. The identifier changes when the site is set up again, and stays the same when a snapshot is restored.

## The order to record them in, and what changes the site

The order of recording differs from the order of playing, because the two versions of `BlogPost.cshtml` take turns and the Setup
screen needs a site that is not set up yet. A snapshot is a copy of the `ORCHARD_APP_DATA` folder, made with the site stopped.

| # | Do | Needs | Changes the site | Snapshot afterwards |
| --- | --- | --- | --- | --- |
| 0 | `ORCHARD_APP_DATA` empty, no `state.json`, site running | | | **S0**: the empty folder |
| 1 | Record `03-setup` | S0 | Yes: sets the site up | |
| 2 | Run `Recorder setup` (signs in as `admin`, saves `state.json`, git-ignored) | the site set up | Yes: a sign-in | **S1** |
| 3 | Record `04-type` | S1 | Yes: the Blog Post type | **S2** |
| 4 | Record `05-post` | S2 | Yes: the blog post | **S3** |
| 5 | Note `<id>`. Put version **B** in `Pages/BlogPost.cshtml`, restart the site | S3 | | |
| 6 | Record `06-page` | S3 and version B | No | |
| 7 | Record `07-alias-part` | S3 | Yes: the Alias part, the alias `pruning` | **S4** |
| 8 | Put version **C** in `Pages/BlogPost.cshtml`, restart the site | S4 | | |
| 9 | Record `07-alias-page` | S4 and version C | No | |
| 10 | Record `07-alias-pattern` | S4 and version C | Yes: the alias becomes `winter-pruning-notes` | **S5** |
| 11 | Record `08-preview` | S5 and version C | Yes: the Preview part with its pattern | **S6** |

To record a clip again: stop the site, restore the snapshot named in its "Needs" column (delete the content of
`ORCHARD_APP_DATA`, copy the snapshot back), put the version of `BlogPost.cshtml` it needs, start the site. A clip that does not
change the site (`06-page`, `07-alias-page`) can be recorded again as it is. `03-setup` needs S0 again, and `Recorder setup`
has to be run after it.

The clips are listed in this order in `Scenes.cs`: `03-setup`, `04-type`, `05-post`, `06-page`, `07-alias-part`,
`07-alias-page`, `07-alias-pattern`, `08-preview`, with `Recorder setup` between the first two. Because `Recorder record all`
would run them one after the other without the file and snapshot changes of steps 5, 7, 8 and 10, record them with the clip names,
one group at a time: `03-setup`; then `04-type 05-post`; then `06-page`; then `07-alias-part`; then `07-alias-page 07-alias-pattern`;
then `08-preview`.

## Conventions for every clip

- Viewport, frame, cursor and ripples are those of the recorder (`references/specs.md`): do nothing special.
- The admin theme is `TheAdmin`, light. When a step says to open menu items, click the labels in the order given. If a label
  cannot be clicked (menu collapsed, or a fly-out), open the page with `NavigateAsync` and the address given instead: the step
  then ends on the same page.
- Pages without the menu steps start before `StartCaptureAsync` with `OpenAsync`, so the capture starts on a loaded page.
- A step's actions must end before its narration does. Type with `delay: 25` to `40`.
- After each click that loads a page, use `ClickAndWaitForNavigationAsync`.
- Wait with a condition, never with a fixed time, unless a step says so.

## The clips

### `03-setup` (chapter 03, "Set up the site", method `SetupAsync`)

Starts on: `OpenAsync("/")` of the **uninstalled** host: the Setup screen (heading "Setup", "Please answer a few questions to configure
your site."). No `state.json`: `Scene.StartAsync` runs without a saved session when the file does not exist. Scroll to the top.

- `setup-1`: type `Orchard Notes` into the field labeled "What is the name of your site?" (`#SiteName`), then `HighlightAsync` it,
  with the caption "Default title of your pages". Source: `OrchardCore.Setup/Views/Setup/Index.cshtml:118` (label) and `:121`
  (hint "This is used as the default title of your pages."), `:103` ("Please answer a few questions to configure your site.").
  On screen when the narration ends: the Setup screen, the site name filled in and ringed.
- `setup-2`: clear the highlight, click the Recipe button (`#recipeButton`), then click the item `a.dropdown-item[data-recipe-name='Blank']`
  (its text starts with "Blank site"). Source: `Index.cshtml:130` (label "Recipe"), `:142` and `:148` (button and items);
  `src/OrchardCore.Themes/TheAdmin/Recipes/blank.recipe.json:3` (display name "Blank site") and `:4` (description "Creates a blank site with content
  management features."). On screen when the narration ends: the Recipe button reads "Blank site", the menu is closed.
- `setup-3`: choose `America/Los_Angeles` in the list labeled "Default Time Zone" (`SelectAsync(#SiteTimeZone, "America/Los_Angeles")`), then `HighlightAsync`
  the list labeled "What type of database to use?" (`#DatabaseProvider`, it reads "Sqlite": do not change it; no connection string field appears), clear
  the highlight, then type `admin` into "User Name" (`#UserName`), `admin@example.com` into "Email" (`#Email`), and `VIDEO_PASSWORD` into "Password"
  (`#Password`) and "Password Confirmation" (`#PasswordConfirmation`). Never click the eye buttons next to the password fields.
  Scroll down just enough to show the "Super User" block and the button. Source: `Index.cshtml:159`, `:162` (hint "Determines the default time zone used
  when displaying and editing dates and times."), `:176`, `:218` ("Super User"), `:222`, `:227`, `:234`, `:244`; the default provider is `Sqlite`
  (`src/OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:53`, `isDefault: true`, no connection string). On screen when the
  narration ends: the whole form filled in, both passwords shown as dots, the "Finish Setup" button visible.
- `setup-4`: click the button labeled "Finish Setup" (`#SubmitButton`, `Index.cshtml:256`) with a visible click. The setup takes some seconds: cut the wait
  with `scene.OffCameraAsync(...)`, whose condition is that the heading "Welcome" (`Page.GetByRole(AriaRole.Heading, new() { Name = "Welcome" })`) is visible,
  then `NavigateAsync("/")` to restart the capture on the home page. On screen when the narration ends: the home page of the Razor Pages template (heading
  "Welcome"), with the template's navigation bar and no Orchard Core screen.

Changes the site: yes (installs it). Restore S0 to record it again.

### `04-type` (chapter 04, "Model a blog post", method `TypeAsync`)

Starts on: `OpenAsync("/Admin")` (the administration with its menu, signed in through `state.json`).

- `type-1`: open the menu item "Design", then "Content Definition", then click "Content Types" (`ClickAndWaitForNavigationAsync`; fallback
  `NavigateAsync("/Admin/ContentTypes/List")`). Source: `src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23`, `:24`, `:25`. On screen when the
  narration ends: the Content Types page (breadcrumb "Content Types", `List.cshtml:6`, the search box, the button "Create new type"), with the list of the
  types the Blank site recipe's features define.
- `type-2`: click "Create new type" (`#btnCreate`, `Views/Admin/List.cshtml:20`), wait for the page "New Content Type", type `Blog Post` into "Display Name"
  (`#DisplayName`), wait until the field "Technical Name" (`#Name`) reads `BlogPost` (the page fills it in while you type), then click "Create" (`button.create`,
  `Views/Admin/Create.cshtml:14`, `:22`, `:30`). Wait for the notification "The "Blog Post" content type has been created."
  (`Controllers/AdminController.cs:162`). On screen when the narration ends: the page titled "Add Parts" (breadcrumb, `Views/Admin/AddPartsTo.cshtml:10`) with that notification
  and the list of parts.
- `type-3`: click the check box of the part labeled "Title", then the one labeled "MarkdownBody" (`li.list-group-item` rows of the list; the label text is
  `partSelection.PartDisplayName`, `AddPartsTo.cshtml:33`; the hint line "Choose the Parts to add to this Content Type." is at `:18`), then click "Add Selected Parts"
  (`#add-selected-parts-button`, `:47`; it is hidden until a check box is ticked). The label of the part is "MarkdownBody", not "Markdown Body" (the part name without the suffix "Part":
  `ContentPartSettingsExtensions.cs:54`; the guide spells it "Markdown Body"). On screen when the narration ends: the page "Edit Content Type - Blog Post" (`Views/Admin/Edit.cshtml:63`).
- `type-4`: scroll down to the "Parts" heading (`Edit.cshtml:144`), `HighlightAsync` the list `#parts` with the caption "Title, MarkdownBody", then clear
  the highlight. The rows read "Title", "MarkdownBody" (each with its description, from `OrchardCore.Title/Migrations.cs:33` and
  `OrchardCore.Markdown/Migrations.cs:27`) and "Blog Post" (the display name of the type, with the hint "Contains the fields for the current type": `Edit.cshtml:153` calls `ContentTypePartExtensions.DisplayName`, which returns the display name of the type for the part
  that has the name of the type; `Edit.cshtml:180`). The rows are sorted by position, which gives Title (1), MarkdownBody (2), Blog Post (5, the default when none is stored): `ContentDefinitionService.cs:143-148` and `:481-492`, `Edit.cshtml:32` and `:42`. Do not click Save. On screen when the
  narration ends: the Parts list with the three rows.

Changes the site: yes (the type with its two parts). Snapshot S2 afterwards.

### `05-post` (chapter 05, "Create a blog post", method `PostAsync`)

Starts on: `OpenAsync("/Admin")`.

- `post-1`: open the menu item "Content", click "Content Items" (`ClickAndWaitForNavigationAsync`; fallback `NavigateAsync("/Admin/Contents/ContentItems")`), then click
  the button "New Blog Post" (`a.btn` with that text: the only creatable type of this site, so the page shows a button, not a "New" list). Source:
  `OrchardCore.Contents/AdminMenu.cs:65`, `:68`; `Views/ContentsAdminListCreate.cshtml:14` ("New {0}" with the display name). On screen when the narration ends:
  the editor "New Blog Post" with the fields "Title" (`TitlePart.Edit.cshtml:12`) and "MarkdownBody" (`MarkdownBodyPart.Edit.cshtml:12`), empty.
- `post-2`: type `Winter Pruning Notes` into "Title" and `Prune apple trees while they are dormant.` into "MarkdownBody" (a text area with the default editor),
  with `delay: 25`, then click "Publish" (the editor is opened from the Content Items list, so its address has a `returnUrl`: the button is the first one of the group of
  `Content.PublishButton.cshtml:19`, whose menu item "and continue" is not used; without a `returnUrl` the button is `:7`). Wait for the notification "Your Blog Post has been published."
  (`Controllers/AdminController.cs:473`). On screen when the narration ends: the Content Items list showing "Winter Pruning Notes" and the notification.

Changes the site: yes (the content item). Snapshot S3 afterwards. Then read `<id>` as described above.

### `06-page` (chapter 06, "Render it in a Razor Page", method `PageAsync`)

Needs `BlogPost.cshtml` version **B** and `<id>`. Starts on: `OpenAsync("/")` (the home page of the template).

- `render-5`: `NavigateAsync("/blogpost/<id>")`. Wait until `main h1` reads `Winter Pruning Notes`. Then `HighlightAsync("main h1")` for about a second,
  clear it, and `HighlightAsync("main p:has-text('Prune apple trees while they are dormant.')")`. A plain `main p` is wrong here: `MarkdownToHtmlAsync` returns its own
  `<p>`, and the page wraps it in a `<p>` (the code of the guide), so the browser builds an empty `<p>` before and after the real one; the locator would match three elements,
  and `HighlightAsync` skips a locator that matches more than one (it prints "nothing to highlight"). Source: the page version B above, whose behavior is in
  `src/OrchardCore/OrchardCore.Contents.Core/Extensions/ContentOrchardHelperExtensions.cs:49` (the published version, `DefaultContentManager.cs:109`) and
  `src/OrchardCore.Modules/OrchardCore.Markdown/Razor/ContentRazorHelperExtensions.cs:15`. On screen when the narration ends: the page of the template with the heading
  "Winter Pruning Notes" and the paragraph "Prune apple trees while they are dormant." (the Markdown rendered as HTML), the paragraph ringed.

Changes the site: no.

### `07-alias-part` (chapter 07, "Friendly addresses with Alias", method `AliasPartAsync`)

Starts on: `OpenAsync("/Admin/ContentTypes/Edit/BlogPost")` (the page "Edit Content Type - Blog Post").

- `alias-1`: click "Add Parts" (`Views/Admin/Edit.cshtml:201`), tick the part labeled "Alias" (`AddPartsTo.cshtml:33`; its description "Provides a way to define custom aliases
  for content items." comes from `OrchardCore.Alias/Migrations.cs:23`), click "Add Selected Parts" (`:47`). Scroll down to "Parts". On screen when the
  narration ends: the page "Edit Content Type - Blog Post" with the parts Title, MarkdownBody and Alias in its Parts list.
- `alias-2`: `NavigateAsync("/Admin/Contents/ContentItems/<id>/Edit")` (opened **without** a `returnUrl`, so "Publish" stays on the page: `AdminController.cs:841`), type
  `pruning` into the field labeled "Alias" (`AliasPart.Edit.cshtml:10`; the hint reads "The alias of the content item. Leave empty to re-generate it.", `:15` and `:18`),
  then click "Publish". Wait for "Your Blog Post has been published.". On screen when the narration ends: the edit page of the post, with `pruning` in the "Alias" field.

Changes the site: yes (the Alias part, the alias `pruning`). Snapshot S4 afterwards.

### `07-alias-page` (chapter 07, method `AliasPageAsync`)

Needs `BlogPost.cshtml` version **C**, and S4. Starts on: `OpenAsync("/")`.

- `alias-4`: `NavigateAsync("/blogpost/pruning")`. Wait until `main h1` reads `Winter Pruning Notes`. `HighlightAsync("main h1")`. Source: version C above
  (`AliasConstants.cs:5` for the `alias:` prefix, `AliasPartContentHandleProvider.cs`). On screen when the narration ends: the same page as in `render-5`, ringed title.

Changes the site: no.

### `07-alias-pattern` (chapter 07, method `AliasPatternAsync`)

Needs version **C** and S4. Starts on: `OpenAsync("/Admin/ContentTypes/Edit/BlogPost")`.

- `alias-5`: scroll to "Parts", click the "Edit" button of the row "Alias" (`Edit.cshtml:163`), on the page "Edit Part - Alias" (`EditTypePart.cshtml:22`)
  `HighlightAsync` the editor of the field "Pattern" (`AliasPartSettings.Edit.cshtml:23`, hint at `:26`). The text area is replaced by a CodeMirror editor
  (`CodeMirror.fromTextArea`, from `alias-part-settings.ts`, bundled in `OrchardCore.Alias/wwwroot/Scripts/alias-part-settings/alias-part-settings.js`), so the visible element
  to ring is `.CodeMirror`, not the hidden text area. It shows `{{ Model.ContentItem.DisplayText | slugify }}`, the default of `OrchardCore.Alias/Settings/AliasPartSettings.cs:13`
  (the hint reads "The pattern used to render the alias of this content type."). Keep the "Options" list (`:14`, "Alias is editable" at `:17`) in view above it. Do not change or
  save anything. On screen when the narration ends: the Pattern editor ringed, with the default pattern readable.
- `alias-6`: `NavigateAsync("/Admin/Contents/ContentItems/<id>/Edit")` (no `returnUrl`), click in the field labeled "Alias", select its text (`Control+A`) and
  press `Backspace`, then click "Publish" with `ClickAndWaitForNavigationAsync` and wait for "Your Blog Post has been published.". The alias is generated because the field
  was empty (`OrchardCore.Alias/Handlers/AliasPartHandler.cs:92`). A ring drawn by `HighlightAsync` is removed when the page reloads, so highlight the hint only after the reload:
  `HighlightAsync` the text "Leave empty to re-generate it." (`AliasPart.Edit.cshtml:14-19`). On screen when the narration ends: the edit page with `winter-pruning-notes` in the
  "Alias" field and the hint "The alias of the content item. Leave empty to re-generate it." ringed.

Changes the site: yes (the alias). Snapshot S5 afterwards.

### `08-preview` (chapter 08, "Preview while you edit", method `PreviewAsync`)

Needs version **C** and S5. Starts on: `OpenAsync("/Admin/Contents/ContentItems/<id>/Edit")` (no `returnUrl`).

- `preview-1`: click the button "Preview" (`#previewButton`, `OrchardCore.ContentPreview/Views/ContentPreview.Button.cshtml:13`; it opens a new tab, `target="_blank"`):
  `scene.Context.RunAndWaitForPageAsync(...)` around the click gives the new page, then `scene.SwitchToAsync(thatPage)`. Wait until the visible iframe of the new page
  shows the text `Prune apple trees while they are dormant.`. There are two iframes (`#iframe-a`, `#iframe-b`, `Views/Preview/Index.cshtml`): the preview window loads each draft in the
  one that is behind and then puts it in front by `style.zIndex` (`999990` is the front one; `Assets/ts/preview-swap.ts:24-36`). Use `Page.WaitForFunctionAsync` with the condition that the
  iframe whose `style.zIndex` is `"999990"` has a `contentDocument.body.innerText` that contains the text. The window asks the editor for the draft when it opens (`ready`, `preview-swap.ts:97`),
  and the editor answers (`contentpreview.edit.js:42-46`). On screen when the narration ends: the preview window
  showing the content item in Orchard Core's generic page (`PreviewController.cs:180`), not the host page: no navigation bar of the template. The exact look is not verified
  (`OPEN-QUESTIONS.md`, question 5).
- `preview-2`: `scene.SwitchToAsync(editorPage)` then `NavigateAsync("/Admin/ContentTypes/AddPartsTo/BlogPost")`, tick the part labeled "Preview" (its description is in
  `OrchardCore.ContentPreview/Migrations.cs:20`), click "Add Selected Parts", on the page "Edit Content Type - Blog Post" click the "Edit" button of the row "Preview", and
  type `/blogpost/{{ ContentItem.Content.AliasPart.Alias }}` into the field labeled "Pattern" (`PreviewPartSettings.Edit.cshtml:14`, hint "The pattern used to render the custom
  preview url of this content type."). The text area of that field is replaced by a CodeMirror editor (`CodeMirror.fromTextArea` in `preview-part-settings.ts`, bundled like the Alias
  one; it has no option that closes brackets by itself), so do not type into the hidden text area: `scene.ClickAsync(Page.Locator(".CodeMirror"))`, then `Page.Keyboard.TypeAsync(pattern, new() { Delay = 30 })`.
  Read what was typed with `Page.EvaluateAsync<string>("() => document.querySelector('.CodeMirror').CodeMirror.getValue()")`; if it is not exactly the pattern, set it with
  `document.querySelector('.CodeMirror').CodeMirror.setValue(...)`. The hidden text area is only updated when the form is submitted, so reading it gives an old value.
  Then click "Save" (`EditTypePart.cshtml:111`) with `ClickAndWaitForNavigationAsync`; the notification reads "The "Preview" part settings have been saved."
  (`Controllers/AdminController.cs:1084`) and the page is "Edit Content Type - Blog Post" again (`:1087`). The click comes at the end of the step, after the pattern has been on screen.
  On screen when the narration ends: the page "Edit Content Type - Blog Post" (it reloads at the top) with that notification.
- `preview-3`: `NavigateAsync("/Admin/Contents/ContentItems/<id>/Edit")`, close the first preview tab, click "Preview" again, switch to
  the new tab (as in `preview-1`) and wait until the visible iframe (`style.zIndex` `"999990"`) shows an `h1` with `Winter Pruning Notes` and a paragraph: it is the Razor Page (`PreviewPartHandler.cs:51`,
  `PreviewStartupFilter.cs:17`, `PreviewController.cs:168`). Switch back to the editor tab, type ` Revised` at the end of "Title" (the Title input has the class
  `content-preview-text`, `TitlePart.Edit.cshtml:14`; the preview is sent 500 ms after the last key, `contentpreview.edit.js:115`), switch to the preview tab and wait until the visible iframe's
  `h1` reads `Winter Pruning Notes Revised`. Do not publish. This step does a lot in the time of its narration: wrap the load of the second preview tab and the wait after typing in
  `scene.OffCameraAsync(...)`, with those conditions, so that the last frame of the step shows the revised heading. On screen when the narration ends: the preview window showing the Razor Page with the heading
  "Winter Pruning Notes Revised".

Changes the site: yes (the Preview part with its pattern; the revised title is never saved). Snapshot S6 afterwards.

## The slides (no recording)

| Step | What the slide shows |
| --- | --- |
| `intro` | The opening card |
| `why-1` | "One web application": three cards (Administration, Front end, Content); the first two are bright |
| `why-2` | The same three cards; the last two are bright |
| `why-3` | "Three terms": content type, content part, content item |
| `add-1` | A terminal panel with `dotnet new webapp` and `dotnet add package`, and three note cards: TheAdmin theme, Recipes, No front-end theme |
| `add-2` | `Program.cs`, with `AddOrchardCms`, `UseStaticFiles` and `UseOrchardCore` bright |
| `add-3` | The template lines to remove (the documentation's list), with two note cards (Razor Pages, Routing) |
| `render-1` | `Pages/BlogPost.cshtml` by identifier: the route, the injection and the call bright |
| `render-2` | The same page with the route and `DisplayText` bright, and a panel with the two addresses: the edit page and the blog post page |
| `render-3` | `ConsoleLog` in a page, a sample of the console output (fake data), and two note cards (Content, In production) |
| `render-4` | The page with the `MarkdownToHtmlAsync` line bright |
| `alias-3` | The page by alias: the route, the `alias:` handle and the `Slug` property bright |
| `preview-4` | A page that checks `ContentPreviewFeature` |
| `sum-1` | "What you built": four cards |
| `outro` | The closing card |

## Checks for private data

- The only name, email and site name on screen are `admin`, `admin@example.com` and `Orchard Notes`; the blog post is `Winter Pruning Notes`. No real name, address, company or
  domain appears: the host is `localhost`.
- The password is typed into fields that show dots: do not click the eye buttons, do not leave a password in the address, in a field or in a shell command that is visible.
- The preview tab and the Setup screen show no machine name or folder path. If a Sqlite path, a user folder or a machine name appears in any frame, record the clip again.
- Run OCR at one frame per second and at every scene change (`SKILL.md`, "Check for private data"), and search it for `@`, for the user name of this computer, and for the
  value of `VIDEO_PASSWORD`.
- `state.json`, the snapshots of `ORCHARD_APP_DATA`, and the folder `<checkout>/src/OrchardSite` are never committed.
