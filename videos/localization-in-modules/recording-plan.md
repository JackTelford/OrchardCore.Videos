# Recording plan: Localize a Module: Strings, Plurals and Extraction

A developer video. Of its 26 steps, 3 are a recording (the clip `04-plural-screen`), 21 are slides drawn by `slides.py`,
and 2 are the opening and closing cards. Only the clip needs a site. Nothing in this video needs a module, a PO file or
a second culture: every other screen is a slide.

The recorder drives a browser only. The commands, the project file and the code of the slides are drawn by
`slides.py` and are never typed in a terminal on camera.

## The site it needs

Only for the clip `04-plural-screen`.

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout on `http://localhost:5010`, with
  `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, as in `references/recording.md`.
- **Auto Setup**, with these environment variables (the host registers the `OrchardCore.AutoSetup` feature, `src/OrchardCore.Cms.Web/Program.cs:9`, as the
  `orchardcore-tester` skill of the Orchard Core repository describes). The recording is made on a fresh site: do not
  use Setup and do not copy an existing `App_Data`.

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`, the value of `VIDEO_PASSWORD` chosen outside git |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

  Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so that the site calls no outside service.
- **Sign-in**: run `Recorder setup` once with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`; it saves
  `state.json` (git-ignored). The clip starts signed in.
- **Features to enable**: none. The Blog recipe enables `OrchardCore.Contents`, which provides the page of the clip
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). The Localization feature stays
  disabled: without it the default localizers return the English forms and choose between the singular and the plural
  form with the English rule. The summary of the clip calls `T.Plural` in a view, which goes through the default HTML
  localizer (`src/OrchardCore/OrchardCore/Localization/NullHtmlLocalizer.cs:12` and `:37` to `:39`; the string
  localizer does the same at `NullStringLocalizer.cs:11` and `:30`). The clip shows exactly that.
- **Settings, external services, local stand-ins**: none.
- **Fake seed data**: none to create. The clip uses the content items that the Blog recipe creates. Five of them have
  a listable type and appear in the Content Items list: the Menu "Main Menu", the Taxonomies "Tags" and "Categories",
  the Blog "Blog" and the Article "About". `Listable` is set for the type `Menu` in
  `src/OrchardCore.Modules/OrchardCore.Menu/Migrations/menu.recipe.json:11`, for `Taxonomy` in
  `src/OrchardCore.Modules/OrchardCore.Taxonomies/Migrations.cs:27`, and for `Article` and `Blog` in the
  `ContentDefinition` step of the Blog recipe, at lines 139 and 251 (it also sets it for `Page`, at line 451, but the
  recipe creates no Page). The other items of the recipe, the blog post "Man must explore, and this is exploration at
  its greatest" and the Footer, have a type that is not listable. The list therefore holds five items, and a search for
  `About` matches the display text of one item only.

## Order of the clips

Only one clip, so no order and no dependency. It does not change the site: the search only adds a query to the
address of the page. No snapshot of `App_Data` is needed for a new take.

Narrate before recording (`scripts/narrate.py videos/localization-in-modules`), since the recorder holds each step for
the length of its narration.

## Clip `04-plural-screen`

Starts on `/Admin/Contents/ContentItems`, opened with `scene.OpenAsync("/Admin/Contents/ContentItems")` before
`StartCaptureAsync()`, so that the page is already loaded and the list shows when the capture starts. This is the page
titled **Manage Content** (`Views/Admin/List.cshtml:17` and `:35`) that the menu item **Content Items** under
**Content** opens (`src/OrchardCore.Modules/OrchardCore.Contents/AdminMenu.cs:65` and `:68`,
route `Contents/ContentItems/{contentTypeId?}` in `Controllers/AdminController.cs:67`). Leave the admin menu as it
comes.

`items-1`
: Actions: `await scene.HighlightAsync(scene.Page.Locator("label#items"), "Plural")` after a short
  `scene.PauseAsync(600)`. Keep the actions shorter than the narration.
  On screen when the narration ends: the page title **Manage Content**, and the summary line above the list, ringed in
  green with the caption "Plural", reading "5 items / 5 items in total" (one row for each of the five items
  listed above).
  Label from: `Views/ContentsAdminListSummary.cshtml:7` (`T.Plural(..., "{0} item", "{0} items")` and
  `" / {0} item in total"`, `" / {0} items in total"`), shown in the Summary zone of the list
  (`Drivers/ContentOptionsDisplayDriver.cs:36`, `Views/Admin/List.cshtml:43` for the form that wraps the page).

`items-2`
: Actions, in this order: `await scene.ClearHighlightsAsync()` (highlights are fixed-position and the page is about to
  change); `await scene.TypeAsync(scene.Page.Locator(".action-bar input[type=\"search\"]"), "About")`; then
  `await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"))`. The search field is the one with
  the placeholder `Search`; if the selector matches nothing, use `scene.Page.GetByPlaceholder("Search")`.
  Wait until the list has reloaded: `await scene.Page.Locator("label#items").WaitForAsync()` and the text of the label
  contains `1 item`.
  On screen when the narration ends: the search box holding `About`, and a list reduced to one row, the Article
  "About".
  Label from: `Views/ContentsAdminListSearch.cshtml:28` (the `Search` placeholder); the plain text searches the display
  text of the items (`Services/DefaultContentsAdminListFilterProvider.cs:296` and `:298`, `WithDefaultTerm`, `DisplayText.Contains`);
  Enter submits the page's form through its first hidden submit button (`Views/ContentsAdminList.cshtml:15`).

`items-3`
: Actions: `await scene.HighlightAsync(scene.Page.Locator("label#items"), "Singular")`, then hold for the rest of
  the narration with `scene.HoldAsync`.
  On screen when the narration ends: the summary line ringed in green with the caption "Singular", and reading
  "1 item / 1 item in total".
  Label from: `Views/ContentsAdminListSummary.cshtml:7`.

## Slides and cards

These steps have no recording. The line says what the slide shows; the code of each slide is in `slides.py`.

- `intro` (card): the opening card, with the title and the subtitle of chapter 00.
- `how-1`: heading "How a string is found"; four cards, the first three bright: the text is the message ID, the
  context is the owner, a missing translation leaves the text as written.
- `how-2`: the same slide with the fourth card bright: nothing to register, text as written without the Localization
  feature.
- `loc-1`: `MyService.cs`, a service that keeps an `IStringLocalizer` in the field `S` and calls `S["Hello {0}", name]`.
- `loc-2`: three cards for `IStringLocalizer<T>`, `IHtmlLocalizer<T>` and `IViewLocalizer` (the last two bright), and a
  line of `LocalizationSettings.Edit.cshtml` that calls `T`.
- `ctx-1`: `MyService.cs` with its constructor, and the entry of `fr.po` whose message context is
  `MyNamespace.MyService`.
- `ctx-2`: a view with `@T["Welcome"]`, and the entry of `fr.po` whose message context is `MyModule.Views.Welcome`.
- `ctx-3`: a service creating localizers with `IStringLocalizerFactory`, and two entries of `fr.po`: one with the
  context `MySharedContext`, one without a context.
- `plural-1`: `@T.Plural(count, "{0} book", "{0} books")`, then the call of the Features module with
  `H.Plural(features.Length, ...)`, and two lines on placeholders zero and one.
- `plural-2`: the header of a Czech PO file with its `Plural-Forms` line, and an entry with three forms.
- `plural-3`: three cards: placeholder zero is the count, languages count differently, the translation places the
  count.
- `src-1`: `MyPermissions.cs`, a `Permission` described with `LocalizationSource.Create("Manage widgets",
  typeof(MyPermissions))` (a static class cannot be a generic type argument), and three cards on what a source holds.
- `src-2`: the `Localize` extension called on `IStringLocalizerFactory` and `IHtmlLocalizerFactory`, a deferred plural,
  and two notes.
- `src-3`: `MyModuleLocalizationSources.cs`, and the two entries of `fr.po` for a common source and a module source.
- `note-1`: `S.Note(...)` with `Arg(...)`, a `// TRANSLATORS:` comment, and the first entry of the template they
  produce.
- `note-2`: four cards, the analyzer rules `OCLOC101` to `OCLOC104` with their severities.
- `ext-1`: `MyModule.csproj` with the `PackageReference` to `OrchardCore.Localization.Build`, and two cards.
- `ext-2`: the two commands that update templates, and what a plain build does.
- `ext-3`: the template `MyModule.pot`, and two lines on how it is written and embedded.
- `ext-4`: four cards: C# calls and `LocalizationSource` declarations, Razor views and pages, Liquid files, `OCLOC`
  diagnostics.
- `ext-5`: `[SkipLocalizationExtraction]` on a forwarding method, and three cards.
- `ext-6`: the build command with `LocalizationCatalogOutputPath` and a comment line on where each template is
  copied (a subfolder named after its target framework), and three cards for the platform workflow.
- `outro` (card): the closing card, with the subtitle `docs.orchardcore.net`.

## Checks for private data

- The only recorded screen is the Content Items list of a fresh site made by Auto Setup. Open every contact sheet
  (`review.py`) and check that the rows are the recipe's "Main Menu", "Tags", "Categories", "Blog" and "About" only,
  with no other name, and that the user menu shows `admin`.
- Run the OCR scan of the skill (one frame per second, every scene change, then 5 frames per second) and search for
  `@` followed by anything other than `example.com`, for a password, and for a path of the machine.
- The password is never typed on camera: the sign-in comes from `state.json`, and `<password>` never appears in the
  captured frames.
- No slide holds a real name: the code uses `MyModule`, `MyService`, `MyNamespace`, and the names of the Orchard Core
  source.
