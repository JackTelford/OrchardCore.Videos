# Open questions: placement-files

Everything in the narration was read from the Orchard Core source (commit `92db4a83`) and from
`src/docs/reference/modules/Placement/README.md`. Nothing was run, so the points below are things a person has to
confirm on a running site before the video is recorded or published. Paths are in the `OrchardCMS/OrchardCore`
checkout.

## 1. A restart after adding `placement.json`, and where the startup project's file is read

- **What was checked**: `ShapePlacementParsingStrategy.cs:41-65` reads `placement.json` when the shape table is built,
  and `DefaultShapeTableManager.cs` keeps the built descriptors in a static dictionary (`s_shapeDescriptors`) that is
  never invalidated, so a feature whose descriptors exist is not read again until the process restarts.
  `ModuleEmbeddedFileProvider.Watch` returns a null change token
  (`OrchardCore.Abstractions/Modules/ModuleEmbeddedFileProvider.cs`). The recording plan therefore restarts the site
  between the two takes. The startup project's file is read from the content root
  (`ModuleEmbeddedFileProvider.cs:91-94`, `Application.cs:25-26`). The narration says nothing about restarting.
- **To confirm**: start the site from the `src/OrchardCore.Cms.Web` folder with no file, open the Create page of an
  Article, add the file, reload the page. If the layout changes without a restart, remove the restart from
  `recording-plan.md` and the README; if it needs a restart, nothing changes. Whether `dotnet run` started from
  another folder uses the project folder as its content root is behaviour of the .NET SDK, not of this source: the
  plan starts the site from the project folder and does not rely on it.

## 2. The result of the two rules in the Article editor (`05-after`)

- **What was checked**: the Blog recipe gives the Article type the part positions Title 0, Permalink 1, body 2,
  `Article` part 3 (`blog.recipe.json:131-185`) and puts the field editors in the zone `Parts.Article`
  (`ContentItemDisplayCoordinator.cs:273`). A rule with `Parts:0.5` moves the Subtitle to the `Parts` zone between the
  Title (0) and the Permalink (1) (`FlatPositionComparer.cs`, parts compared one by one); `Parts:4#Media;1` puts the
  Banner Image in a tab named Media. Shapes without a tab belong to the tab named Content (`ZoneShapes.cs:14`), a tab
  without a position sorts before a tab with one (`FlatPositionComparer.cs:154-157`, null is `before.`), so the order
  of the tabs is Content, Media. When the `Article` part has no shape left, its wrapper renders nothing
  (`ContentPart.Edit.cshtml`, `shapes.Count == 0`). The `contentType` filter finds the item on the Create page
  because `ContentItemDisplayManager.cs:117` sets `ContentItem` on the editor shape and `BaseDisplayManager.cs:47-52`
  passes that shape to the filters.
- **To confirm**: on the Create page of an Article, with the file in place: two tabs Content then Media; the Subtitle
  directly under the Title; the Banner Image only in the Media tab; the Permalink and the body still in the Content
  tab; no empty row left behind by the `Article` part. If the order of the tabs is Media first, the narration of
  `after-1` ("two tabs, Content and Media") still holds but the plan's description of the tab bar must be corrected.

## 3. The last matching rule wins inside one file (`wins-2`)

- **What was checked**: `ShapePlacementParsingStrategy.cs:67-101` registers one placement per rule, in file order;
  `ShapeAlterationBuilder.Placement` (line 190) wraps the previous one (`predicate ? location : next`), so the last
  rule registered is tested first. Across features, `ShapeDescriptorIndex.CalculatePlacement`
  (`FeatureShapeDescriptor.cs:158`) walks from the last feature to the first. The documentation does not say which
  rule wins inside a file.
- **To confirm**: put two rules for `TextField_Edit` with the differentiator `Article-Subtitle` in the file, the first
  with `Parts:0.5`, the second with `Parts:5`. The Subtitle should appear after the body (the second rule). Swap the
  rules: it should return under the Title.

## 4. A property placement does not know is ignored (`wins-2`)

- **What was checked**: `PlacementNode` binds `place`, `displayType`, `differentiator`, `alternates`, `wrappers` and
  `shape` (case-insensitively, `JOptions.Default`); every other property goes to `Filters` (extension data, with its
  name as written), and `BuildPredicate` (`ShapePlacementParsingStrategy.cs:126-139`) only adds a condition when a
  registered filter provider has the same key (compared with `StringComparison.Ordinal`); otherwise the predicate is
  returned unchanged.
- **To confirm**: add `"contentType": "Page"` to the Subtitle rule: the rule should no longer apply to an Article.
  Rename the property to `"ContentType": "Page"` (capital C): if the rule then still applies to an Article, the claim
  holds.

## 5. The active theme wins over the modules, and a dependent module over the one it depends on (`wins-1`)

- **What was checked**: the startup project winning is read from `DefaultShapeTableManager.cs:126-130` (the
  application feature is moved last) and `FeatureShapeDescriptor.cs:158` (last feature first). The theme before the
  modules comes from `ThemeExtensionDependencyStrategy.cs` (registered in `OrchardCoreBuilderExtensions.cs:130`): a
  theme depends on every feature that is not a theme, and `DependencyOrdering.cs` puts a feature after the features
  it depends on, so the features of the active theme come after those of every module. Between two modules the order
  is the dependency order, then the priority, then the feature id (`DependencyOrdering.cs`), so only a module that
  depends on another is guaranteed to win over it. Only the active theme and its base themes take part
  (`DefaultShapeTableManager.cs:194-214`).
- **To confirm** (optional, not needed for the recording): put a rule for the same shape in a module and in the
  active theme, with different positions: the theme's position should win.

## 6. Pronunciation (`spoken` map of `storyboard.json`)

Confirm each entry with `scripts/pronounce.py` before narrating, as the skill asks: "placement.json" spoken as
"placement dot JSON"; "TextField_Edit" as "Text Field Edit"; "TextField_Display" as "Text Field Display";
"ContentPart_Edit" as "Content Part Edit"; "ContentPart_Summary" as "Content Part Summary"; "Article-Subtitle" as
"Article dash Subtitle"; "Article-Image" as "Article dash Image"; "Parts.Article" as "Parts dot Article". Also
listen to the identifiers that are left as written: TitlePart, ContentPart, TextField, MediaField, displayType,
differentiator, contentType, contentPart.

## 7. Selectors that come from outside this source

- `.trumbowyg-box` (the rich text editor of the body, used only as a wait condition) belongs to the Trumbowyg
  library, not to the Orchard Core source read here. The plan falls back to waiting for the Banner Image row alone.
- The body row label is the display name of the `HtmlBodyPart` part (`HtmlBodyPart-Wysiwyg.Edit.cshtml:22`), which the
  Blog recipe does not set, so the editor probably shows "HtmlBodyPart". The narration says "the body" and the plan
  selects the row by its class, so nothing depends on the label.
- `:text-is(...)` and `:has(> ...)` are Playwright selector extensions; they need the Playwright version of the
  recorder.
- `.edit-item-parts > ul.nav-tabs` assumes that the tab bar is a direct child of the `edit-item-parts` container
  (`Content.Edit.cshtml`, `GroupShapes.cs:175-204`: the tab container writes the list and the `tab-content` block next
  to each other). If the page nests it deeper, use `.edit-item-parts ul.nav-tabs` and check that it matches one element.

## 8. A dot in a place (`place-1`)

- **What was checked**: the zones of a place are walked in `ShapeResult.cs:535-552`: the first with the zone holder
  (`Zones[name]`), the next ones with a property of the shape that holds an `IShape`; a shape whose parent is not found
  is not added. In the content editor `ContentItemDisplayCoordinator.cs:253` stores the shape of each part in the
  `Parts` zone under the part's name, which is why `Parts.Article` works there. The video shows only that example.
  A nested zone such as `Content.Metadata` (the example in the XML comment of `PlacementInfo`) needs a shape that holds
  a property with that name, and nothing was found that creates one for a content item.
- **To confirm**: add a rule with `"place": "Parts.Article:1"` for `TextField_Edit` with the differentiator
  `Article-Subtitle`: the Subtitle should stay inside the `Article` part, at position 1, instead of moving to the top.

## 9. The `shape` property and the alternates of a field or part (`file-5`)

- **What was checked**: `ShapeResult.cs:507-523` replaces the shape type and clears the alternates and wrappers
  present when the shape is built, then adds those of the rule. The alternates of fields and parts are added later by
  the `Displaying` event (`ContentFieldDisplayDriver.cs:62-69`, `ContentPartDisplayDriverTPart.cs:56-63`), after the
  clear. The narration therefore says only that `shape` replaces the shape type.
- **To confirm**: add `"shape": "AnotherShape"` to a field rule and look at which template the field is rendered with
  (a template named after the part and the field, such as `TextField-Article-Subtitle.cshtml`, in the theme, shows
  whether the field alternates still apply): the alternates named after the part and the field are probably still
  there.

## 10. A place without a position in an editor (`place-2`)

- **What was checked**: `PlacementInfo.cs:15` gives a place without a position the default position of the context,
  and `ContentItemDisplayCoordinator.cs:256` sets that default to the position of the part, which stays in force for
  the editors of its fields (`ShapeResult.cs:380-401`). The comparer itself reads an empty position as 0
  (`FlatPositionComparer.cs:160-163`), which is what the documentation says.
- **To confirm**: change the Subtitle rule to `"place": "Parts"` (no position): the Subtitle should sit at the
  position of the `Article` part (3), after the body, not at the top.

## 11. Layout details that depend on the browser

- The scroll distance of `before-1` (`ScrollAsync(0, 700)`) is a starting value: how much of the editor fits in the
  frame depends on the viewport of the recorder and on the height of the rich text editor.
- After the restart between the two takes, the sign-in saved by `Recorder setup` should still be valid, because the
  site keeps its data protection keys in `App_Data`; if the recorder lands on the sign-in page, run `Recorder setup`
  again.
