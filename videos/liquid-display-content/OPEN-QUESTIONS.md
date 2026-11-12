# Open questions: liquid-display-content

Everything the narration states was read in the source of `OrchardCMS/OrchardCore` (the commit of `catalog/features.json`)
or in the documentation. Nothing was run on a site. A person must confirm the points below on a running site before
recording. Paths are from the `src` folder of that repository.

## 1. Pronunciations

`storyboard.json` has a `spoken` map. None of these spellings was heard. Check each with
`scripts/pronounce.py`, and change it if the voice misreads it:

- `Model.Content.MarkdownBodyPart` as "Model dot Content dot Markdown Body Part", `Model.ContentItem` as "Model dot Content
  Item", `Model.Content` as "Model dot Content".
- `Content-BlogPost.liquid` as "Content Blog Post dot liquid", and `Content__BlogPost` as "Content double underscore Blog
  Post".
- `shape_build_display`, `shape_remove_item`, `shape_stringify`, `shape_render`, `shape_new`, `render_section` and
  `render_body` with spaces instead of underscores.
- `content_item_id` and `contentitemid` as "content item I.D." (spelled with dots, as `references/storyboard.md` asks),
  `contentitem` as "content item", and `DateTime` as "Date Time".

The map is applied with `str.replace` in its order, so the longer keys come first (`Model.Content.MarkdownBodyPart`, then
`Model.ContentItem`, then `Model.Content`; `contentitemid` before `contentitem`). Keep that order if a key is added.

## 2. Slide examples that were not run

The code of the slides comes from The Blog Theme (`OrchardCore.Themes/TheBlogTheme/Views`) and from the Liquid
documentation. Four slides combine documented pieces into an example that no theme file contains. Every snippet of every
slide was parsed with Fluid (the parser of Liquid), with the tags registered as `LiquidViewParser.cs` registers them, and
all parse; none was rendered. Paste each of the four into a template under Design, Templates of a Blog site, display a
page with it, and check the result:

- `items-2`: `Content["slug:blog/post-1"]` with `shape_build_display: "Summary" | shape_render`. Read in
  `OrchardCore/OrchardCore.Autoroute.Core/Services/AutorouteHandleProvider.cs` (the `slug:` prefix adds the leading `/`, and
  `OrchardCore/OrchardCore.ContentManagement.Abstractions/Routing/AutorouteEntry.cs` normalizes the path of an entry to a
  leading `/` without a trailing one),
  `OrchardCore.Modules/OrchardCore.Contents/Startup.cs:112-129` (the `Content` accessor loads the published item, and
  returns `nil` when no handle matches) and `OrchardCore.Modules/OrchardCore.Contents/Liquid/BuildDisplayFilter.cs`
  (`nil` in, `nil` out; `ShapeRenderFilter.cs` writes nothing for `nil`). The narration also names the `contentitemid:`
  handle (`OrchardCore.Modules/OrchardCore.Contents/Services/ContentItemIdHandleProvider.cs`): confirm that
  `Content["contentitemid:<id>"]` loads an item.
- `items-3`: `contentitem handle: "slug:blog/post-1", display_type: "Summary"`. Read in
  `OrchardCore.Modules/OrchardCore.Contents/Liquid/ContentItemTag.cs` and `OrchardCore.Modules/OrchardCore.Contents/Shapes.cs:30-78`.
- `make-4` and `zones-4`: `shape_add_alternates` on the shape returned by `shape_build_display`, and `{% layout "CustomLayout" %}`.
  Read in `OrchardCore.DisplayManagement.Liquid/Tags/AddAlternatesTag.cs`, `LayoutTag.cs`, and
  `OrchardCore/OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:349-412` (the alternates are tried from the last added,
  and the earlier ones are the fallback). The narration says a template for the alternate draws the shape "if one exists":
  confirm that the name `Content_Summary__Featured` finds a template named `Content-Featured.Summary.liquid` in a theme
  (read in `OrchardCore.DisplayManagement/Descriptors/ShapeTemplateStrategy/BasicShapeTemplateHarvester.cs`).

## 3. What a signed-in session shows on the front end

The clip `02-page` is recorded with the session of `state.json`. In the templates read for this video
(`Layout.liquid`, `Content-Blog.liquid`, `Content-BlogPost.liquid`, `Content-BlogPost.Summary.liquid`, `Blog-ListPart.liquid`,
`Widget-RawHtml.liquid`) nothing is shown only to a signed-in user, and `ContentsMetadata.cshtml` only sets the page title.
Look for an admin bar, a notification in the `Messages` zone or an edit link on the contact sheet of the first take. If one
shows, record without `state.json`.

## 4. Layout of the pages at 1536x864

The scroll distances in `recording-plan.md` (200, 300, 400 and 2000 pixels) are starting values: the height of the masthead and of the
post preview depends on the CSS of the theme, which was not rendered. Rehearse once, and check these end frames:

- `page-3`: the whole post preview fits in the viewport with its ring.
- `page-4`: the footer fits in the viewport.
- `page-5`: the first paragraph of the body is in view after the last scroll, and the actions end before the narration
  does (the step has about 12 seconds for a scroll, a click and a page load).

Also check what the navigation bar shows at that width (a menu button or the menu links), since `page-1` does not narrate it.

## 5. The date and the badges of the post

"Posted by admin on <date>" uses the `DateTime` shape, the site time zone and the translation of `MMMM dd, yyyy` by the `t`
filter (`Content-BlogPost.Summary.liquid:11-13`). The date is the day of the setup. The category and tag badges after it come
from the taxonomy items of the recipe (Travel, Earth, Exploration, Space); confirm that they show on the preview.

## 6. Priority of an admin template over a theme file

`content-3` says a template created in the admin takes priority over a theme file of the same name. This is read in
`DefaultHtmlDisplay.GetShapeBindingAsync` (the binding resolvers, which include
`OrchardCore.Templates/Services/TemplatesShapeBindingResolver.cs`, are asked before the shape table of the theme, for each
alternate). Confirm it once: add a template named `Content__BlogPost` under Design, Templates and open `/blog/post-1`.
