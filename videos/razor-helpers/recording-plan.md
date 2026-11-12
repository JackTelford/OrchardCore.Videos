# Recording plan: Razor Helpers and Tag Helpers for Orchard Core Themes

This video has **no browser recording**. Every step after the opening card is a slide drawn by `slides.py` (a code
panel or cards), and the opening and closing cards are drawn by `assemble.py`. There is no site to run, no Auto Setup,
no tenant, no sign-in (`state.json`), no seed data, no external service and no `Scenes.cs`: `Recorder` is not used.

The code on the slides comes from the Orchard Core source (the `TheTheme` theme, the Taxonomies and Media modules, and
the helpers themselves) and from the documentation. A theme author never types it on camera.

## What to do

1. Install the prerequisites of the skill (`pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`).
2. Confirm the spoken forms listed in `OPEN-QUESTIONS.md` with `scripts/pronounce.py`, before narrating.
3. Narrate: `python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/razor-helpers`.
4. Assemble: `python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/razor-helpers`.
5. Look at one frame of every slide of the build (the code must be readable, nothing cut off at the edge of a panel),
   and do the checks at the end of this file.

Order of the clips: the order of the storyboard. No clip depends on another one, and no clip changes a site, so no
snapshot has to be restored.

## Cards

- `intro`: the opening card, the title "Razor Helpers and Tag Helpers" over the logo.
- `outro`: the closing card, with the subtitle "docs.orchardcore.net".

## Clip 01-helper (slides)

- `helper-1`: heading "Orchard is a property of the view's base class". A code panel `_ViewImports.cshtml` holds five lines from `src/OrchardCore.Themes/TheTheme/Views/_ViewImports.cshtml:1-5`, with the first line (the inherits directive) bright. A second panel `Layout.cshtml` holds the html element of `src/OrchardCore.Themes/TheTheme/Views/Layout.cshtml:16`, trimmed to its `lang` and `dir` attributes, which read the culture name and direction from the `Orchard` property of the view.
- `helper-2`: heading "Without that base class, inject IOrchardHelper". Three panels: `BlogPost.cshtml`, a Razor Page that injects the helper (from `src/docs/guides/decoupled-cms/README.md`); `MyClass.cs`, a constructor that takes it (from `src/docs/reference/modules/Razor/README.md`); and `IOrchardHelper.cs`, the interface with its one member, the HTTP context (`src/OrchardCore/OrchardCore.Abstractions/IOrchardHelper.cs`).
- `helper-3`: heading "Each module defines its own helpers". Four cards: Contents, Alias and Autoroute; Media and Resource Management; Liquid, Markdown, Shortcodes and Sanitizer; Queries, Lists and Taxonomies.
- `helper-4`: heading "A reference and an enabled feature". Two cards: "Reference the module" and "Enable the feature".

## Clip 02-content (slides)

- `content-1`: heading "Load one content item". A code panel with the four calls that load one item by id, alias, slug and handle (the signatures are in `src/OrchardCore/OrchardCore.Contents.Core/Extensions/ContentOrchardHelperExtensions.cs`, `src/OrchardCore.Modules/OrchardCore.Alias/Razor/AliasPartRazorHelperExtensions.cs` and `src/OrchardCore.Modules/OrchardCore.Autoroute/Razor/AutoroutePartRazorHelperExtensions.cs`), and two cards: the handles `alias:main-menu` and `slug:myblog/my-blog-post`.
- `content-2`: heading "Published by default, drafts on request". Two panels: the alias helper with and without the `latest` argument (`src/docs/reference/modules/Alias/README.md`, section Razor), and the handle helper with `VersionOptions.Latest`.
- `content-3`: heading "List content items". Two panels: a loop over the three newest blog posts, and the source of `GetRecentContentItemsByContentTypeAsync` (`src/OrchardCore/OrchardCore.Contents.Core/Extensions/ContentOrchardHelperExtensions.cs:102`).
- `content-4`: heading "Render an item with DisplayAsync". Two panels: `TermContentItem.cshtml` (`src/OrchardCore.Modules/OrchardCore.Taxonomies/Views/TermContentItem.cshtml:3`) and the loop of `TermPart.cshtml` in the same folder (lines 6-13).
- `content-5`: heading "DisplayAsync needs the view's Orchard property". Two panels: `IOrchardDisplayHelper` (`src/OrchardCore/OrchardCore.DisplayManagement/IOrchardDisplayHelper.cs`) and the signature of `DisplayAsync`, in the file `OrchardRazorHelperExtensions.cs` (`src/OrchardCore/OrchardCore.ContentManagement.Display/Razor/OrchardRazorHelperExtensions.cs:17`, class `ContentOrchardRazorHelperExtensions`).

## Clip 03-page (slides)

- `page-1`: heading "Culture helpers". One panel: the html element with `CultureName()` and `CultureDir()` (`src/OrchardCore.Modules/OrchardCore.Setup/Views/_Layout.cshtml:5`, and `src/OrchardCore.Themes/TheTheme/Views/Layout.cshtml:16`), and the `IsRightToLeft()` branch that picks a stylesheet (`TheTheme/Views/Layout.cshtml:26-35`).
- `page-2`: heading "URL helpers". Two panels: the loop of `src/OrchardCore.Modules/OrchardCore.Media/Views/MediaField.cshtml:14-16` with `AssetUrl` and `IsImageFile`, and one line each for `AssetUrl` with a resize and `ResourceUrl` (`src/docs/reference/modules/Media/README.md`), under the `@using OrchardCore.Media.Core.Processing` that `ResizeMode` needs (`ResizeMode` is declared in `src/OrchardCore/OrchardCore.Media.Abstractions/Processing/ResizeMode.cs`).
- `page-3`: heading "Text helpers". Two panels: the four text helpers (`LiquidToHtmlAsync`, `MarkdownToHtmlAsync`, `ShortcodesToHtmlAsync`, `SanitizeHtml`, from the Liquid, Markdown, Shortcodes and Sanitizer pages of the documentation) and `MarkdownToHtmlAsync` with `sanitize: false`.
- `page-4`: heading "Data helpers". Three panels: `ContentQueryAsync` in a loop (`src/docs/reference/modules/Queries/README.md`), `QueryListItemsAsync`, and the loop of `src/OrchardCore.Modules/OrchardCore.Taxonomies/Views/TaxonomyField.cshtml:11` with `GetTaxonomyTermAsync`.

## Clip 04-tag (slides)

- `tag-1`: heading "Turn on a set of tag helpers". The panel `_ViewImports.cshtml` of `helper-1`, with the four add Tag Helper lines bright, and two cards: "Add the directive" and "Reference the assembly".
- `tag-2`: heading "Shapes, menus and content items". A panel with the shape, menu and content item elements (`TheTheme/Views/Layout.cshtml:47` and `:53`, `src/docs/reference/modules/DisplayManagement/README.md`, `src/OrchardCore.Modules/OrchardCore.Contents/Shapes.cs:30-42`, which reads the `Handle` property), and a panel with the directive of the content item element.
- `tag-3`: heading "Content for a zone of the layout". Two panels: the zone element of `src/OrchardCore.Modules/OrchardCore.AuditTrail/Views/AuditTrailSettings.Edit.cshtml:8` (the `Title` zone, written on three lines), and the lines `:91` and `:93` of `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml` that render it with `RenderSectionAsync` (line 91 sits inside the `DisplayTitlesInTopbar` condition of lines 89-92, which the panel leaves out as `@* ... *@`; with the setting on, the layout renders the same zone at line 64) (the zone is rendered in `RazorPage.cs:349`, `src/OrchardCore/OrchardCore.DisplayManagement/Razor/RazorPage.cs`). The `HeadMeta` zone of `AdminBranding.cshtml` is not used: the `link` element inside it registers the link with the resource manager and writes nothing in the zone (`OrchardCore.ResourceManagement/TagHelpers/LinkTagHelper.cs:83`).
- `tag-4`: heading "Register and render resources". One panel: the script, style and resources elements of `src/OrchardCore.Themes/TheTheme/Views/Layout.cshtml:24`, `:33`, `:37`, `:41` and `:72`. A style renders with `<resources type="Header" />` whatever its `at` value, unless `at` is `Inline` (`OrchardCore.ResourceManagement/ResourceManager.cs`, `RenderStylesheet`; `src/docs/reference/modules/Resources/README.md`, section Specify location), which is why the panel shows no style with `at="Foot"`.
- `tag-5`: heading "Media attributes on the image element". Two panels: the image element with `asset-src`, `img-width`, `img-height` and `img-resize-mode` (`src/docs/reference/modules/Media/README.md`, `src/OrchardCore.Modules/OrchardCore.Media/TagHelpers`; the `alt` reads `Model.Field.MediaTexts`, because `DisplayMediaFieldViewModel` has no `MediaTexts` property), and the Media directive.

## Checks for private data

- The slides and the narration hold only invented names: `main-menu`, `myblog/my-blog-post`, `BlogPost`, `MyShape`, `RecentBlogPosts` and the id `4mexample0contentitem0id00`. Look at every frame of the build for anything else.
- No email address, no password, no token, no real person, no real company and no domain other than `docs.orchardcore.net` appears on a slide. The sign-in password of other videos plays no part here.
- No frame shows the machine's folders: the file names in the window bars are the names of views and classes, never paths.
- Run the OCR of the skill (step 6 of `SKILL.md`) on the assembled video, as for every video, and search the text for email addresses, `password` and `token`. The `@` of Razor code is expected on the code slides.
