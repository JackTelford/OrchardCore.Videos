# Open questions: Media in Templates

Everything in the narration was read from the Orchard Core source at commit `92db4a83`; nothing was run, because no
site was available. Each item below says what was checked in the source and what a person must confirm on a running
site before recording. Items 1 to 4 can change a clip; items 5 and 6 can change the text.

## 1. The seed recipe

Checked: the three steps of the recipe exist with the shapes used in `recording-plan.md`: `MediaProfiles`
(`OrchardCore.Media/Recipes/MediaProfileStep.cs`; same shape as the `banner` profile of the Blog recipe, `Mode` 2 is
`Crop`), `Templates` (`OrchardCore.Templates/Recipes/TemplateStep.cs`: `Content` and `Description` per name) and
`content` (`OrchardCore.Contents/Recipes/ContentStep.cs`). The JSON Import page runs a recipe from pasted text
(`OrchardCore.Deployment/Controllers/ImportController.cs:163-208`), and the editor is a CodeMirror instance
(`wwwroot/Scripts/import-json/import-json.js`).

To confirm: the import ends with `Recipe imported successfully!`; **Design > Templates** lists
`Content__Slug__renditions`; **Media > Profiles** lists `thumbnail`; `/renditions` renders the template. The
template is chosen through the alternate `Content__Slug__renditions`, added for a content item with an Autoroute
part (`OrchardCore.Autoroute/Services/ContentAutorouteShapeTableProvider.cs`). If `/renditions` shows the theme's
default rendering instead, the alternate is not found: check the spelling of the template name against the path of the
page, and tell the maintainers, because `result-1` to `result-3` depend on it.

## 2. The layout of the `/renditions` page

Checked: `TheBlogTheme/Views/Layout.liquid` renders the body in `col-md-10 col-lg-8 col-xl-7`, and it has no header
unless a template writes the `Header` zone; the template writes none. Not checked: how the theme's navigation bar
overlaps the first lines of the body (the template adds `110px` of top padding for it), and whether the grid
(`repeat(auto-fill, minmax(200px, 1fr))`, three columns in that column) and its two rows fit in the 1536 by 864 pixel
viewport without scrolling.

To confirm: all five figures and their URLs are visible at once, with nothing under the navigation bar. If not, change
the numbers in the template (and write the new values in `recording-plan.md`); the narration does not mention the
layout. `result-3` says the first four images "ask for 200 pixels": keep the width and height at 200 or change that
sentence with them.

The Media Profiles list (`profile-1`) uses the List layout of the admin lists, which is the default
(`AdminListOptions.DefaultLayout`) unless the site settings under Configuration, Settings, Admin choose another
(`AdminListOptionsConfiguration.cs`). A `layout` query is ignored unless the site lets users choose a layout, which is
off by default (`AdminListLayoutResolver.cs:31-37`), so the plan passes none. The profile hint is placed in the List
layout and in the Name column of the Grid layout (`MediaProfileEntryDisplayDriver.cs:20-24`). To confirm: the hints
`A banner image (2048px x 600px, cropped)` and `A square thumbnail` appear under the names.

## 3. What the five images look like

Checked in `OrchardCore.Media/Processing/VipsImageProcessingEngine.cs`: `max` fits `home-bg.jpg` (1900 by 1267) in
200 by 133; `crop` fills a 200 by 200 square from the center; `pad` fits it in 200 by 133 and centers it on a 200 by 200
canvas filled with the `bgcolor` (bars above and below); `stretch` forces 200 by 200; the profile `thumbnail` gives 160
by 160 cropped. Not run.

To confirm: the sizes above (for example with `naturalWidth` and `naturalHeight` of each image in the page), that the
bars of `pad` are visible against the page, and that the picture shows no identifiable person.

## 4. Liquid not run

Checked: filter names and registration (`OrchardCore.Media/Startup.cs:84-86`, `Filters/ResizeUrlFilter.cs`,
`Filters/MediaFilters.cs`), the order of the query keys (`Models/MediaCommands.cs`, `GetValues`: width, height, rmode,
rxy, format, bgcolor, quality, autoorient) and where the token goes (`Processing/MediaTokenService.cs`,
`AddTokenToPath`: after the commands). The slides show URLs written from that reading, with `token=...`.

To confirm: the outputs written as comments in the slides `liquid-1`, `liquid-2`, `liquid-4` and `profile-4` match what
the filters return on the site; the template filter chain `{{ url | split: '&token=' | first }}` prints the URL without
its token; the Razor and tag helper snippets of `razor-1` to `tag-3` and `end-1` compile in a theme that references the
`OrchardCore.Media` package. In particular: `asset-src="@Model.Field.Paths[0]"` needs the `@` because `AssetSrc` is a
string property (the README of the Media module omits it, `docs/topics/media/media-picker.md` has it); `AssetUrl`
names its background color parameter `bgColor` and `AssetProfileUrlAsync` names it `bgcolor`
(`Razor/OrchardRazorHelperExtensions.cs`); `img-anchor="@Model.Field.GetAnchors().FirstOrDefault()"` was written for
the video, not taken from the documentation, to avoid an index error on a field without anchors.

## 5. Spoken forms

`storyboard.json` has a `spoken` entry for every identifier of the narration. None was tried with the voice
(`scripts/pronounce.py` creates audio). A person confirms each one before `narrate.py` runs: `asset_url` (asset URL),
`img_tag` (image tag), `resize_url` (resize URL), `append_version`, `data_order` (data underscore order), `data-order`
(data dash order), `AssetUrl`, `AssetProfileUrlAsync` (Asset Profile URL Async), `IsImageFile`, `resizeMode`,
`bgColor` and `bgcolor` (B.G. color), `appendVersion`, `asset-src` (asset source), `asset-href` (asset H ref), the
`img-` attributes (image width, image height, image resize mode, image quality, image format, image B.G. color, image
profile, image anchor), `asp-append-version` (A.S.P. append version), `OrchardCore.Media` and
`OrchardCore.Media.Core.Processing` (Orchard Core dot Media, and so on), `WebP` (web P), `CDN` (C.D.N.), `PNG` (P.N.G.), and the bare words `src`
(source), `href` (H ref) and `img` (image, last, for the plain "img element"). The replacement is a plain text replacement in the order of the file, so a longer key comes
before a key it contains (`img-bgcolor` before `bgcolor`). The narration writes "I Image Processing Engine" directly.

## 6. Where the documentation and the code disagree

These are reported as documentation issues and the text follows the code. Not narrated, because the code and the
documentation disagree and no site was available to settle it: the resize modes `min` and `boxpad` (the engine handles
`min` like `max`, and `boxpad` like `pad`), the list of values for `bgcolor` (the engine reads a 3 or 6 digit
hexadecimal value, and uses white for anything else), and cache busting without tokens. To confirm on a site, request
`/media/home-bg.jpg?width=240&height=240&rmode=min` and the same URL with `rmode=max`, which are valid without a token
because 240 is a supported size, and compare the two images.

The narration follows the code on two points where the documentation says otherwise. `liquid-4` says that quality
applies to JPEG and WebP images (`VipsImageProcessingEngine.cs`, `ResolveFormat`; the documentation says JPG only), and
that an image without a format keeps the format of its source file (`MediaImageProcessingMiddleware.cs:85-89`; the
documentation says an unspecified format falls back to JPEG). To confirm on a site, request
`/media/home-bg.jpg?width=240&format=webp&quality=10` and the same URL with `quality=90` (valid without a token) and
compare the file sizes, then upload a PNG to the library and request it with `?width=240`: the response should be a
PNG. `end-1` says that a version changes the resized file in the cache; the code adds the version to the cache key
(`MediaImageProcessingMiddleware.cs:97-110`), and the documentation says versioning is removed without tokens. To
confirm, request the same resized URL with `&v=a` and with `&v=b` and check that the media cache folder holds two files.
The `append_version` filter exists only while the Resources feature is on (`OrchardCore.Liquid/Startup.cs:143-148`);
the Blog recipe enables it.

## 7. Seen in the source, not used in the video

- The Liquid `a` tag of the Media module (`Liquid/MediaAnchorTag.cs`) matches an argument named `asset-href` in `Match`
  but reads `asset_href` in `WriteToAsync`; whether the tag works as documented was not checked.
- `img_tag` (`Filters/MediaFilters.cs`) writes the values of its arguments into the element without HTML encoding.
- The shortcodes `[image]` and `[asset_url]` (`Shortcodes/`) also resize, but they do not add a token, so on a default
  site only the supported sizes apply to them. The video does not cover shortcodes.
- The URL of an asset is built by `DefaultMediaFileStore.MapPathToPublicUrl`: CDN base URL, then the path base and the
  tenant URL prefix, then the media request path. `path-1` states it from that code; it was not tried with a CDN base
  URL or a tenant prefix.
