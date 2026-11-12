# Open questions: razor-helpers

What the author could not check from the Orchard Core source and the documentation alone. Each item says what was
checked and what a person has to confirm.

## 1. Spoken forms (run `scripts/pronounce.py`)

The author could not run the voice. The narration spells the identifiers as words ("Get Content Item By Alias Async",
"I Orchard Helper"), and the `spoken` map of `storyboard.json` rewrites these written forms:

| Written in the narration | Spoken as |
|---|---|
| `contentitemid` | content item id |
| `asp-name` | A.S.P. name |
| `asset-src` | asset source |
| `img-width` | image width |
| `img-height` | image height |
| `img-resize-mode` | image resize mode |

Confirm these, and how the voice reads "HTML-like", "HTTP", "URL", "CDN", "Liquid To HTML Async", "Version Options", "Is Right To Left", "Render Section Async" and "Foot Script", which the author left to the voice.

## 2. Compilation of the slide code

Most snippets are copied from views of the Orchard Core source (`TheTheme`, the Taxonomies, Media, Admin and Setup
modules) or from the documentation, and trimmed. These were written for the video from the signatures of the source,
and no one has built them:

- `content-1`: the four calls that load one content item (`GetContentItemByIdAsync`, `GetContentItemByAliasAsync`,
  `GetContentItemBySlugAsync`, `GetContentItemByHandleAsync`) with literal arguments and a made-up id.
- `content-2`: the handle call with `VersionOptions.Latest` (the Alias page shows `VersionOptions.Published`).
- `content-3`: the loop over `GetRecentContentItemsByContentTypeAsync("BlogPost", 3)` that displays each post with
  `Orchard.DisplayAsync(post, "Summary")`.
- `page-2`: `<img src="@Orchard.ResourceUrl("~/TheTheme/images/logo.png")" alt="Logo" />` (the image is an invented file of a theme), and the `@using OrchardCore.Media.Core.Processing` that `ResizeMode.Crop` needs, taken from the view imports of the Media module (`Views/_ViewImports.cshtml:23`); the Media page shows `ResizeMode.Crop` without it.
- `page-4`: `QueryListItemsAsync` with a made-up list id.
- `tag-2`: `<contentitem handle="alias:main-menu" display-type="Detail" />` (the documentation writes `alias=`; the
  code reads `Handle` first and `Alias` only for older views, see `Contents/Shapes.cs:30-42`).
- `tag-3`: the `Title` zone of `AuditTrailSettings.Edit.cshtml:8`, written on three lines instead of one, and the two lines of the `TheAdmin` layout that render it, with the condition around the first one left out.
- `tag-4`: the script, style and resources elements of `TheTheme/Views/Layout.cshtml`, in a new order and without the other resources. Confirm in the source of a rendered page that the style link is in the head and the bootstrap script is before the closing body tag. The code of `RenderStylesheet`, `RenderHeadScript` and `RenderFootScript` says so, and the documentation says that style tag helpers always render in the header unless `at` is `Inline`.
- `tag-5`: `<img asset-src="@Model.Paths[0]" ... />`, with the `@` that the documentation leaves out, and `Model.Field.MediaTexts[0]` where the documentation writes `Model.MediaTexts[0]` (`DisplayMediaFieldViewModel` has no `MediaTexts` property; the field has).

To confirm: copy the views of `TheTheme` into a scratch theme of a throwaway checkout, paste each snippet in a view with
the references of `helper-4` (the module's project, and the directives of `tag-1`), build, and open a page that
renders it. Check in particular that `<contentitem handle=...>` renders the menu, and that `asset-src` with `@` gives
the public URL of a media file.

## 3. A disabled feature

`helper-4` says that many helpers call services their module registers, so the module's feature has to be enabled on
the site. This was read from the code: `ContentQueryAsync` gets `IQueryManager` (registered by the Queries Core
feature), `MarkdownToHtmlAsync` requires `IMarkdownDisplayService` (registered by the Markdown feature) and
`LiquidToHtmlAsync` requires `ILiquidTemplateManager` (registered by the Liquid feature), all from
`HttpContext.RequestServices`. Not checked on a running site: what a view shows when the feature is disabled (an
exception, or a null). The narration does not say.

## 4. Documentation points that could not be run

- The Media page writes `asset-src="Model.Paths[0]"`. For an attribute bound to a `string` property, Razor passes the
  text as it is, so the expression needs an `@`. This follows Razor's rules and was not run.
- The DisplayManagement page documents a `<duration>` tag helper. No `HtmlTargetElement` of the source targets
  `duration` (searched in all of `src`), so the element was left out of the video.
