# Open questions: Decoupled CMS with Razor Pages

What could not be verified from the Orchard Core source (commit `92db4a83`) or the documentation page of the guide, and what a person has to confirm
on a running site before recording. `file:line` references are relative to the root of the Orchard Core checkout.

Checked in the source and therefore not open (they are in the narration and the plan as facts): the labels of every screen the video shows, including "MarkdownBody"
(not "Markdown Body" as the guide writes it: `OrchardCore.ContentTypes/Views/Admin/AddPartsTo.cshtml:33` prints the part name without the suffix "Part"); that "Technical Name" is filled
in from "Display Name" and gives `BlogPost` (the compiled script `OrchardCore.ContentTypes/wwwroot/Scripts/autocomplete/autocomplete.js` keeps the letters of the display name);
that "Add Selected Parts" appears when a check box is ticked (`OrchardCore.ContentTypes/Assets/js/add-parts-to.ts`); that "New Blog Post" is a button and not a "New" list on a site set up with the Blank site recipe
(the only `Creatable()` type of the modules is in `OrchardCore.Taxonomies/Migrations.cs:26`, a module the recipe does not enable, and the type made by the administration is creatable:
`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:85`; `OrchardCore.Contents/Views/ContentsAdminListCreate.cshtml:12-15`); that Sqlite is the default of the Setup screen and
hides the connection string (`OrchardCore.Setup/Controllers/SetupController.cs:225`, `OrchardCore.Setup/Assets/ts/setup.ts`); that `/` shows the page of the host when no home route is set (`OrchardCore.HomeRoute/Routing/HomeRouteTransformer.cs`,
and the Blank site recipe sets none).

## 1. Claims that come from the documentation only

The documentation says the following, and the source has no code that proves it, so confirm each when `03-setup` and `04-type` are recorded:

- after "Finish Setup", the site shows the home page of the Razor Pages template (heading "Welcome"), and the Setup does not sign the super user in (the plan runs
  `Recorder setup` afterwards; if the user is already signed in, `Recorder setup` is still harmless). The route is verified (see above); the markup of the template is not in this source;
- the Razor Pages template of the installed SDK may differ from the one the guide was written for (for instance, a template that adds `app.MapStaticAssets()` and
  `WithStaticAssets()`, which the guide's list of lines to remove and the slide of `add-3` do not mention). The plan replaces `Program.cs` as a whole, so the host is the one
  of the slide `add-2`; confirm that the template's layout renders without those calls, because the end of `setup-4` shows the template's home page.

## 2. The host project and the code of the slides were not built

No host project could be built while writing this content. To confirm when the host project of `recording-plan.md` is built:

- the project file follows `OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj` with `OrchardCore.Application.Cms.Core.Targets` instead of `OrchardCore.Application.Cms.Targets`;
  if the build needs an adjustment, fix the plan. The `Directory.Build.props` and the package versions of the repository are not part of the source this content was checked against.
  Nullable reference types are off and warnings are not errors in the host, because the checkout probably treats
  warnings as errors (`OrchardCore.Cms.Web.csproj` sets `WarningsNotAsErrors` for 612 and 618, the obsolete warnings) and the pages of the guide would raise nullable warnings;
- `Pages/BlogPost.cshtml` (versions B and C of the plan, which are the code of the slides) compiles. Points to look at: `@using OrchardCore` also imports the
  obsolete `OrchardCore.ContentRazorHelperExtensions` of `OrchardCore.Contents` (`OrchardCore.Contents/Razor/ContentRazorHelperExtensions.cs:7`), whose methods have the same names as
  the global `ContentOrchardHelperExtensions` (`OrchardCore.Contents.Core/Extensions/ContentOrchardHelperExtensions.cs`). An obsolete warning (CS0618) is likely, because the
  generated page declares its `using` directives inside its namespace, where the `OrchardCore` namespace is searched before the global one; an ambiguity is also possible. If the
  warning appears, a maintainer may want the guide to avoid the deprecated class; and a Razor
  Page property with `[FromRoute]` and no `[BindProperty]`, as in the documentation, has to bind the route value on a GET request;
- the page wraps the result of `MarkdownToHtmlAsync` (which is a `<p>` already) in a `<p>`, as the guide does. The plan's locator for the body of `render-5` does not depend on how the
  browser repairs the nested paragraphs; confirm the rendered markup, and that the slide code still reads as the guide does;
- the snippet of `preview-4` (`HttpContext.Features.Get<ContentPreviewFeature>()?.Previewing == true`) is the check that Orchard Core's own code uses
  (`OrchardCore.Indexing.Core/Handlers/IndexingContentHandler.cs:65`), not code from the documentation: confirm that it compiles in a Razor Page, and that it is `true` in the
  preview window of `08-preview` (the same `HttpContext` is used when the pipeline runs again, `OrchardCore.ContentPreview/PreviewStartupFilter.cs`);
- the slide of `render-3` shows the shape of the output of `ConsoleLog` (`OrchardCore.ContentManagement.Display/Razor/OrchardRazorHelperExtensions.cs:31`, with the
  well-known properties of `ConvertContentItem`, `:69-89`) with fake values. Confirm in the browser console of the host (environment `Development`) that `Content` holds
  `BlogPost`, `MarkdownBodyPart` and `TitlePart`, as the documentation's screenshot of the console shows. It is `null` in the `Production` environment (`:39`);
- the Razor Pages of the host are compiled at build. `OrchardCore.Mvc.Core/Startup.cs:111-127` adds runtime compilation only in the `Development` environment and only when a `refs` folder is
  next to the application, so a replaced `BlogPost.cshtml` may or may not be picked up without a restart. The plan stops and starts the site after each replacement, which works either way.

## 3. The administration screens that were read in the source but not seen

The labels are verified in the source (see the plan for each `file:line`). What the source does not show:

- how the menu items "Design", "Content Definition", "Content Types", "Content" and "Content Items" appear in `TheAdmin` (collapsed menu, fly-outs, the quick navigation).
  The plan gives the address to open when a label cannot be clicked;
- that the Content Types list shows other types besides Blog Post (the Menu and widget types of the features of the recipe): `type-1` only says the list is the place where
  content types are defined.

## 4. Publish stays on the page only without a `returnUrl`

`alias-2` and `alias-6` open the edit page of the blog post by its address, without a `returnUrl`, because then "Publish" returns to the same edit page
(`OrchardCore.Contents/Controllers/AdminController.cs:841`), where the Alias field can be seen. Opened from the Content Items list, the page carries a `returnUrl`, "Publish" returns to
the list, and a second button, labeled "and continue" in its menu (`Views/Content.PublishButton.cshtml:24`), stays on the page. The markup gives "Publish and continue" as the label it takes
after it is chosen (`:18`), but the script of the administration theme that sets it was not read, so the narration does not mention it. The documentation says "Publish (and continue)".

## 5. The preview, in a headless browser

- `08-preview` records two tabs of one browser context (the editor and the preview window) that talk through a `BroadcastChannel`. The scripts are read (`OrchardCore.ContentPreview/Assets/js/contentpreview.edit.js`,
  `OrchardCore.ContentPreview/Assets/ts/preview-swap.ts`): the preview window announces `ready`, the editor posts the draft, and the window loads it in the hidden one of two iframes and brings that one to the front.
  Confirm that this works in the recorder's headless Chromium, and that the preview refreshes after the title is typed. If it does not,
  record `preview-3` with the editor and the preview window opened in the order given, and say so in the pull request.
- The look of the generic preview of `preview-1` (`OrchardCore.ContentPreview/Controllers/PreviewController.cs:180`, the view `Views/Preview/Render.cshtml` and the display shapes
  of a Blank site without a site theme) is not known. The narration only says that it is a generic page, not the Razor Page. If it shows nothing readable, add the text of the post to the
  wait condition of the plan or change the narration.
- The Pattern field of the Preview part and of the Alias part is a CodeMirror editor (`CodeMirror.fromTextArea` with the Liquid mode, bundled in `OrchardCore.Alias/wwwroot/Scripts/alias-part-settings/alias-part-settings.js`;
  no option closes brackets by itself). The plan types into the editor and reads its value from the editor, not from the hidden text area. Confirm that the typed pattern is exactly
  `/blogpost/{{ ContentItem.Content.AliasPart.Alias }}`.
- The preview finds the item through its alias in the database (`AliasPartContentHandleProvider.cs`), so the page of version C only renders the draft of a content item that has been saved with
  that alias. The narration does not say what happens for a content item that has never been saved.

## 6. Spellings for the voice, to confirm with `scripts/pronounce.py`

The `spoken` map of `storyboard.json` uses spellings that were not checked with the voice (the script needs the speech service, and writes audio). Confirm each one, and change it
if the voice reads it badly:

- `Program dot C.S.` (the form of `references/storyboard.md`), `Blog Post dot C.S.H.T.M.L.`;
- `Add Orchard C.M.S.`, and `Orchard Core dot Application dot C.M.S. dot Core dot Targets`;
- `dot net new web app` for the command, `I Orchard Helper`, `Get Content Item By Id Async`, `Get Content Item By Handle Async`, `Markdown To HTML Async`, `Console Log`,
  `Content Preview Feature`, `Markdown Body Part`, `Markdown Body`, `Display Text`, `Blog Post`, `The Admin`, `App Data`;
- words left as they are written, which the voice may read badly: `Sqlite`, `slugify`, `Razor`, `Liquid`, `CMS`, `HTTPS`, `Markdown`, `cshtml` (in "BlogPost.cshtml" only through the spoken map above).

## 7. The package on screen

The slide of `add-1` shows `dotnet add package OrchardCore.Application.Cms.Core.Targets` without a version, so the video does not date. The recorded host references the checkout's
projects instead of a package (see the plan), because the package of the checked commit may not be published. If a maintainer prefers the package on screen, the guide's project file
(it pins `Version="3.0.1"` while the source builds `4.0.0`, `OrchardCore.Build/OrchardCore.Commons.props:5`) is out of step with the source: see the documentation issues reported with this video.
