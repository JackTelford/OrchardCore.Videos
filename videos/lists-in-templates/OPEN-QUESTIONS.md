# Open questions: Lists in Templates

Everything in the narration comes from the Orchard Core source at commit `92db4a83` and its documentation. Nothing was run:
the points below need a person with a running site, who sets the answer here or in the narration before the video is
recorded.

## 1. Does the seeded template render?

The block of the template (section 2.3 of `recording-plan.md`) uses only filters whose code was read:
`container`, `list_count`, `list_items`, `display_url`, `display_text`, and `Model.ContentItem` in a `Content__*`
template. What was not run:

- `{% assign blog = Model.ContentItem | container %}`, then `blog | list_count`, `blog | list_items` and
  `blog | display_url`. `ContainerFilter` returns an `ObjectValue` that holds the `ContentItem`
  (`OrchardCore.Lists/Liquid/ContainerFilter.cs:30`), and the two list filters accept such a value
  (`ListCountFilter.cs:23`, `ListItemsFilter.cs:23`).
- `{% assign posts = blog | list_items %}` followed by `{% for post in posts %}`: `ListItemsFilter` returns
  `FluidValue.Create(listItems, ctx.Options)` for an `IEnumerable<ContentItem>` (`ListItemsFilter.cs:34`).

Confirm that `/blog/a-week-in-the-highlands` shows **More in Blog**, **3 published posts** and three links. Steps
`template-1`, `template-2`, `site-2` and `site-3` depend on it.

## 2. The count and the draft

`site-2` and `site-3` say that the blog also holds a draft and that `list_count` reports three. By the code, `list_count`
and `list_items` filter on `ContentItemIndex.Published` by default (`OrchardCore.Lists/Helpers/ListQueryHelpers.cs:15,25`),
so a post that was only saved as a draft is not counted. Confirm on the page that the count is three while the admin
list of the blog shows four posts.

## 3. Pronunciation

The storyboard's `spoken` map spells identifiers for the voice. None of the spellings was generated or heard. A person
checks each one with `scripts/pronounce.py` before narrating: `list_items` as "list items", `list_count` as "list count",
`display_url` as "display U.R.L.", `display_text` as "display text", `Content__BlogPost` as "Content double underscore
Blog Post", `Content-BlogPost.liquid`, `Blog-ListPart.liquid`, `Model.ContentItems`, `Model.Pager`, `ListPart` as "List
Part", `ContainedPart` as "Contained Part", `ListPartViewModel`, `QueryListItemsAsync`, `QueryListItemsCountAsync`, and
`Autoroute` as "Auto route".

## 4. Statements taken from the code only

- `container-2`: a list that is not published makes `container` return the item itself. The filter loads the container
  with `IContentManager.GetAsync(containerId)` (`ContainerFilter.cs:26`), which loads the published version when no
  version option is given (`OrchardCore.ContentManagement/DefaultContentManager.cs:109`). Not shown on screen.
- `part-2`: `Blog-ListPart.liquid` applies to the Blog content type only. The alternate `Blog__ListPart` is built by
  `ContentPartAlternatesFactory` (`{ContentType}__{PartType}`), and the documentation says the same
  (`src/docs/reference/modules/Lists/README.md`, section Template). Not tried on a second content type.
- `template-3`: an admin template with the name of a theme file takes priority. In `DefaultHtmlDisplay.GetShapeBindingAsync`
  the binding resolvers, among them `TemplatesShapeBindingResolver`, run before the shape table for each alternate. The
  admin template is used instead of the theme file as a whole, which is why the narration says it must be complete; the
  seeded template of the plan is the theme's `Content-BlogPost.liquid` with the block added. Not tried with both present.
- `container-2`: the exception text is `A Content Item was expected` (`ContainerFilter.cs:20`). How a failed render shows
  it (an error page, an empty shape) was not checked, so the narration only says that the filter raises an exception.
- `items-2`: the sort is by `ContainedPartIndex.Order`, then by the index row id (`ListQueryHelpers.cs:23-24`). The narration
  says "sorted by the order stored in each ContainedPart" and nothing about the direction. All four seeded posts have
  `Order` 0, because ordering is not enabled in the Blog recipe, so the three links of `site-3` appear in index row order.
  Note that order if you want to describe it.

## 5. The seed in the admin

- Plan 2.1 and 2.2 rely on the code of the screens: clicking the title of the blog opens the `Display` page (buttons
  **Edit Blog** and **Create Blog Post**, no **List Blog Post Items**), and the editor opened by **Create Blog Post** shows
  **Publish** and **Save Draft** as the default entries of a button group (`Content.PublishButton.cshtml:15-19`,
  `Content.SaveDraftButton.cshtml:15-19`). The script that handles `data-action-group` was not read, so check that the
  button used is the one named in the plan and not a remembered alternative such as **Publish and continue**.
- The admin list of the blog sorts by creation date, newest first, when ordering is not enabled (`ContainerService.cs`,
  `QueryContainedItemsAsync`). Confirm that *Packing list for the coast* is the first row.
