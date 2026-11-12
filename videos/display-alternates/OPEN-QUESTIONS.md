# Open questions: Shape Alternates

What was checked in the source (Orchard Core commit `92db4a83`), and what a person must confirm on a running site before
or while recording. Nothing below is narrated as a fact unless the source shows it; the items are the places where the
source shows less than a running site would.

## To confirm on the first take

1. **The look of an event page before any template.** The plan says the page of an `Event` is rendered by the theme's own
   `Content.liquid`: a banner with `post-bg.jpg`, the title, and the body below, with nothing under the title. Source: the
   alternates of the shape are `Content__Event` only (`OrchardCore.ContentManagement.Display/ContentItemAlternatesFactory.cs:77`),
   no template has that name, so the shape type `Content` is used (`DefaultHtmlDisplay.cs`, end of `GetShapeBindingAsync`), and
   a theme's template wins over a module's of the same name (`OrchardCore.DisplayManagement/Extensions/ThemeExtensionDependencyStrategy.cs`,
   `Descriptors/FeatureShapeDescriptor.cs:109-111`). Open `/events/harvest-festival` on snapshot S0 and compare with the
   narration of `before-1` and `before-2`.
2. **A template file added to `Views/` is found only after a build.** The files of a theme are embedded and listed in the
   assembly at build time (`OrchardCore.Module.Targets/OrchardCore.Module.Targets.targets:52-85`), the site reads that list
   (`OrchardCore.Abstractions/Modules/Module.cs:32-35`, `ModuleEmbeddedFileProvider.cs:52-55`), and the shape table is built
   once and cached (`OrchardCore.DisplayManagement/Descriptors/DefaultShapeTableManager.cs`, `_shapeTableCache`). The plan
   therefore stops the site, copies the file and starts it again with `dotnet run`, never with `--no-build`. A person must
   confirm on the first take that the incremental build picks up a new file in `Views/` (the debug comment of the page shows
   the file). The narration says nothing about it.
3. **The two Liquid templates render as described.** They are `Content.liquid` of the theme with two changes (the banner
   image, in the same `url('{{ "..." | href }}')` form as the original, and one `span` under the title), and without the two
   comment lines of the original. Check that
   the banner image changes, that the word under the title is legible in the banner (the `subheading` class is styled by the
   theme's CSS under `.site-heading`), and that the body of the event still shows below. If a word is not legible, change it
   in `slides.py` (`TEMPLATE`), in the plan, and in the files, together.
4. **The exact text of the debug comments.** The source writes
   `<!--shape-start type:{type} bindings:{name} => {source} ({engine}) -->` and `<!--shape-end type:{type} -->`
   (`DefaultHtmlDisplay.cs:290-312`). For a template file the binding name is the lower-case name the file harvests to
   (`BasicShapeTemplateHarvester.cs:46,52,57`), so `content__slug__events__night__market`, and the source is the path of the
   file inside the site, which the code builds as `Areas/` + the module id + `/Views/` + the file name
   (`Application.cs:12`, `ShapeTemplateBindingStrategy.cs:75-78`, `ModuleEmbeddedFileProvider.cs:44-52`); for this theme it
   should read `Areas/TheBlogTheme/Views/Content-Slug-events-night-market.liquid`, which was not run. The slide `output-1`
   shortens that path to `...`. Read the real comment (see "Checks before
   the first take" in the plan) and, if the text differs, change `COMMENTS` in `slides.py`.
5. **The notification after saving the debugging setting.** The page answers "Site settings updated successfully."; the
   narration of `debug-1` does not depend on it. Check only that the check box is checked when the page reloads.
6. **Pronunciation.** `scripts/pronounce.py` was not run (the writing session does not generate audio). The `spoken` map of the
   storyboard holds guesses to confirm: the file names spoken with "dot", `events/night-market` as "events slash night market",
   `Autoroute` as "Auto route", `OnDisplaying` as "On Displaying", `AddShapeTableProvider` as "Add Shape Table Provider", `cshtml` as "C.S.H.T.M.L.". The
   `spoken` entries are applied one after the other in the order of the map (`narrate.py`, `spoken()`), so the map lists the
   longest names first.

## Found in the source and left out of the narration

7. **Alias, slug and content item id alternates of the Summary display type.** The Templates reference lists
   `Content_Summary__Alias__[alias]` and `Content_Summary__Slug__[slug]`. The handlers that add the alias, slug and id
   alternates are registered with `builder.Describe("Content")` (`OrchardCore.Alias/Services/ContentAliasShapeTableProvider.cs:12`,
   `OrchardCore.Autoroute/Services/ContentAutorouteShapeTableProvider.cs:12`, `OrchardCore.Contents/Shapes.cs:14`). The shape of
   a content item in the Summary display type has the type `Content_Summary` (`ContentItemDisplayManager.cs:55-68`), and a
   descriptor is found only for the exact type or for the part before the first `__` (`DefaultHtmlDisplay.cs:327-346`), so those
   handlers appear not to run for it. The narration therefore says the path and alias alternates are added for an item "shown on
   its own page", and the Summary display type is used only for the content type alternate. Confirm with the debug comments of an
   item listed in the Summary display type (for example on the blog page of the Blog recipe).
8. **The id alternate is not a stable name.** `OrchardCore.Contents/Shapes.cs:25` adds `Content__` followed by
   `contentItem.Id`, which is the database key of the content item version (`long`), not the `ContentItemId`
   (`OrchardCore.ContentManagement.Abstractions/ContentItem.cs:19`). The Templates reference does not list it. It is not in the
   video.
9. **Priority between the alternates added by different modules.** The type alternate `Content__Event` is added when the item
   shape is built (`ContentItemDisplayManager.cs:76-83`). The Contents module adds its own alternates in `OnDisplaying`
   (`Content_Detail`, `Content__Event` again, `Content__` + the database id, `Content_Detail__Event`, `Content_Detail__` + the
   database id; `OrchardCore.Contents/Shapes.cs:24-25`), and `AlternatesCollection.Add` ignores an alternate that is already in
   the list (`AlternatesCollection.cs:48`), so the second `Content__Event` changes nothing. The order of these handlers is fixed:
   Alias and Autoroute depend on `OrchardCore.ContentTypes` (their `Manifest.cs`), `OrchardCore.ContentTypes` depends on
   `OrchardCore.Contents` (`OrchardCore.ContentTypes/Manifest.cs:9`), `DependencyOrdering.Order` puts a feature after its
   dependencies, and the descriptors of a shape type are combined in that order (`DefaultShapeTableManager.cs:136`,
   `FeatureShapeDescriptor.cs`, `ShapeDescriptorIndex`). The `Shapes` class has no `Feature` attribute, so it belongs to the main
   feature `OrchardCore.Contents` only (`ExtensionManager.cs`, `GetSourceFeatureNameForType`). So the path and alias alternates are always
   after the alternates of the Contents module, and `DefaultHtmlDisplay.GetShapeBindingAsync` tries the last first, which is why
   `Content-Slug-events-night-market.liquid` wins over `Content-Event.liquid`. Not checked: whether the alias
   alternate comes before or after the path alternate (the two features do not depend on each other, so it follows from the
   general ordering of features); the narration does not rank them. The plan's checks of the debug comments confirm the winner on a running site.
10. **Razor files.** `type-1` says a Razor file with the same name and the extension `cshtml` works the same way. The harvester
    treats both extensions alike (`ShapeTemplateBindingStrategy.cs`), and both view engines are registered
    (`RazorShapeTemplateViewEngine.cs:46`, `LiquidShapeTemplateViewEngine.cs:49`). It was not rendered on a site. If both
    `Content-Event.cshtml` and `Content-Event.liquid` exist in the same theme, which one wins was not checked; the video does
    not show that case.
11. **Alternates added inside a template.** `own-2` says Orchard Core picks the template first and only then runs it, so a
    template cannot change the choice made for itself. Source: `DefaultHtmlDisplay.ExecuteAsync` (line 153) resolves the binding with
    `GetShapeBindingAsync` before `ProcessAsync` runs it. It was not tried on a site.
12. **The key of the content step in the seed recipe.** The `Blog` recipe writes its content step with `"Data"`
    (`blog.recipe.json`, step `content`), and the seed in the plan does the same. `ContentStepModel`
    (`OrchardCore.Contents/Recipes/ContentStep.cs`) declares a property named `Data`; whether `data` also binds was not checked.
13. **Shapes without comments.** `debug-2` says the source holds a pair of comments around each shape that Orchard Core renders
    for that request, not around every shape: a shape that is empty, already rendered, or whose content comes from a cache
    gets none (`DefaultHtmlDisplay.cs:55-70`, `139-142`, `262-265`). The Blog recipe enables Dynamic Cache and the layout of
    the theme caches its menu, so the page holds shapes without comments. Confirm only that the `Content` shape of the page
    has its pair (see "Checks before the first take" in the plan).
