# Open questions: flows-templates

Everything in the narration was read from the Orchard Core source (commit `92db4a83`) and the documentation pages of
the tracker entry. The points below were not run on a site. A person must confirm them on a running site before the
recording, or the narration or the slide named in each point must change. Paths are relative to `src` of the Orchard
Core checkout.

## Pronunciation (a person must confirm each spelling)

The storyboard's `spoken` map holds new spellings. Check each one with `scripts/pronounce.py` and keep it, or change
it:

- File names: `Widget-Paragraph.Summary.liquid` and `Widget-Paragraph.liquid` ("Widget dash Paragraph dot liquid"),
  `LandingPage-BagPart.liquid`, `LandingPage-Services.liquid`, and `FlowPart.cshtml` ("Flow Part dot C.S.H.T.M.L.").
- Shape names: `FlowPart_Empty` and `BagPart_Empty` ("Flow Part underscore Empty"), and `FlowPart`, `BagPart`,
  `LandingPage`, `HtmlBodyPart` (spaces added between the words).
- Liquid names: `Model.Classes`, `Model.ContentItem.Content.Services.ContentItems`, `shape_build_display`,
  `shape_render`, `console_log` (underscores said as "underscore"), and the class names `widget-paragraph` and
  `widget-size-100` ("dash").

The map is applied with `str.replace` in the order written, so the longer names come before the names they contain.

## Behavior read from code, to confirm on a site

1. **A Flow Part is not displayed in Summary by default.** `OrchardCore.Flows/Drivers/FlowPartDisplayDriver.cs:70` sets
   a location for `Detail` only (the Bag Part, `BagPartDisplayDriver.cs:72-73`, sets `Detail` and `Summary`).
   `OrchardCore/OrchardCore.DisplayManagement/Views/ShapeResult.cs:348-407` (the hidden check is at line 404) and `Descriptors/PlacementInfo.cs:274` hide
   a shape that has no location for the display type, and the display builds set no default zone. Confirm with a
   content type that has a Flow Part, shown in a list with the Summary display type. The narration of `part-2` says
   that "By default, a Flow Part displays only in the Detail display type, while a Bag Part displays in both Detail and
   Summary"; if a site shows a Flow in Summary, change that sentence and the slide.
2. **The text of the shape debug comments.** `OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:290-308`
   writes `<!--shape-start type:{type} bindings:{binding name} => {binding source} ({razor|liquid|code}) -->` and
   `<!--shape-end type:{type} -->`. The binding name is the lowercase shape type that `BasicShapeTemplateHarvester.cs:42-58`
   builds from the file name (`Widget-Paragraph.liquid` gives `widget__paragraph`), and the binding source is the path of
   the file below its extension, `Areas/<extension id>/Views/<file>`
   (`ShapeTemplateBindingStrategy.cs:73-78`, `OrchardCore.Abstractions/Modules/Module.cs:91`). The slide `source-1` shows
   the two comments that follow from this code for a Paragraph widget of The Blog Theme. They were not captured from a
   running site. Capture the real comments of a Flow Part and of a Bag Part, and replace the slide if they differ. The
   example of `src/docs/reference/modules/DisplayManagement/README.md` spells the binding name, the path and the end
   comment differently from the code, so the slide does not use it.
3. **`console_log` outside Production.** `OrchardCore.ContentManagement.Display/Liquid/ConsoleLogFilter.cs:25` returns
   nothing when `IsProduction()`, and the Razor helper `ConsoleLog` does the same
   (`Razor/OrchardRazorHelperExtensions.cs:39`). Confirm that the console shows the alternates of a shape after
   `shape_render` in the Development environment.
4. **Alternates of the part shapes.** `LandingPage-BagPart.liquid`, `LandingPage-Services.liquid` and
   `LandingPage-Services.Summary.liquid` are derived from `ContentPartAlternatesFactory.cs` (the alternates
   `LandingPage__BagPart`, `LandingPage__Services` and `LandingPage_Summary__Services`) and from the file name rule of
   `BasicShapeTemplateHarvester.cs:42-57`. `Content-Service.Summary.liquid` (`Content_Summary__Service`) comes from
   `ContentItemAlternatesFactory.cs:76-77`. The shape debug information of a page with these files confirms them.
   The same check covers `items-2` ("Widget-Paragraph.liquid targets the Detail display type, so it does not apply to a
   Bag Part that renders Summary"): it follows from the alternates the display manager adds, but another display
   handler could add alternates of its own.
5. **Display Type saved as an empty text.** `BagPart.cshtml:9` uses `??`, so only a missing value falls back to
   Summary. The editor binds an empty text box to a missing value, so the narration says "when that setting has no
   value". A recipe that stores an empty string would render the items as Detail.
6. **The Services bag of the Agency recipe renders on the front end with its own template.** The video shows the code
   of the landing page template (`OrchardCore.Themes/TheAgencyTheme/Recipes/Snippets/landingpage.liquid`) on a slide
   only. It is not recorded, because the page also shows seeded names.

## Timing and layout of the two clips

- `06-debug`: the controller suspends the shell release only when the model is invalid, which suggests that a valid
  save of the site settings reloads the tenant (not run), so the redirect after Save can take several seconds, while `debug-2` is about 9 s of narration. The plan always
  cuts the wait with `OffCameraAsync`. A person must confirm on the recording machine that the clip still shows the
  checked box and the notification when the narration ends.

- `03-display`: the position of the **Parts** list on the page "Edit Content Type - Landing Page", and of the field
  Display Type on "Edit Part - Services", depends on the height of the settings above them. A person must check that
  the Services row, and then the field with its placeholder and hint, fit in the 1536x864 viewport after the scroll
  written in `recording-plan.md`.
- `06-debug`: the narration says "open Settings, Debugging" while the clip shows the page itself (the plan opens it
  by its address and highlights only the option). Confirm that this matches the menu path on a running site.
