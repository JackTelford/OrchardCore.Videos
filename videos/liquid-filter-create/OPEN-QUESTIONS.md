# Open questions: Write a Liquid Filter

What was checked while writing the video, and what a person must confirm on a running site before or while recording.
Everything the narration says as a fact was read in the source of `OrchardCMS/OrchardCore` at commit `92db4a83` or in
`src/docs/reference/modules/Liquid/README.md`. Nothing was run on a site.

## What was checked

- **The code of the slides and of the module compiles.** Every code panel of `slides.py` and the three C# files of
  `recording-plan.md` were compiled in a scratch project outside the repository. It referenced the packages
  `OrchardCore.Liquid.Abstractions`, `OrchardCore.Infrastructure.Abstractions` and `OrchardCore.Abstractions`
  `4.0.0-preview-19223` (built from commit `93b42158`) and `Fluid.Core` `3.0.0-beta.9`, on `net10.0`, with no warning.
- **The behavior of the filters, run through Fluid in a console program** (the same options builder, the same
  `AddLiquidFilter`, a stand-in for `ISiteService`): `{{ "orchard core" | shout }}` writes `ORCHARD CORE`;
  `with_site_name` writes `About | Orchard Core`, and `About - Orchard Core` with the argument `" - "`; a missing
  argument is nil (`At(0).IsNil()`); `StringValue.Create(text)` is written HTML encoded, and
  `StringValue.Create(html, encode: false)` as it is; a filter that returns `NilValue.Instance` writes nothing; a
  registration that configures `TemplateOptions` makes the first use of `IOptions<TemplateOptions>` throw
  `Configuring 'TemplateOptions' isn't supported as it is immutable, configure 'TemplateOptionsBuilder' instead.`
  (`src/OrchardCore/OrchardCore.Liquid.Abstractions/TemplateOptionsAccessor.cs:19-22`).
- **The rest is read from the source**: the registration and the scoped resolution of a filter
  (`ServiceCollectionExtensions.cs:22-27`, `LiquidFilterDelegateResolver.cs:15`), that the options are built once
  (`TemplateOptionsAccessor.cs:25`), who reads them (templates, workflows and queries: `LiquidViewTemplate.cs`,
  `LiquidWorkflowExpressionEvaluator.cs`, `SqlQuerySource.cs`, `LuceneQuerySource.cs`, `ElasticsearchQuerySource.cs`),
  the `liquid` filter returning `encode: false` (`OrchardCore.Liquid/Filters/LiquidFilter.cs:21`), the `local` filter
  returning `NilValue.Instance` (`LocalTimeZoneFilter.cs:33,48`), and the menu paths and labels of the Templates module.

## To confirm on a running site

1. **The module builds in the checkout.** The harness used `net10.0` packages; the source at `92db4a83` builds for
   `net11.0` (`src/OrchardCore.Build/TargetFrameworks.props`) and runs the Orchard Core analyzers on every project
   (`OrchardCore.Commons.props`). Build `Videos.LiquidFilters` inside `src/OrchardCore.Modules` and note any warning or
   error. The csproj of `recording-plan.md` follows `OrchardCore.Liquid.csproj`.
2. **The module's feature id and its Enable link.** `recording-plan.md` assumes the feature id is the assembly name,
   `Videos.LiquidFilters`, and the selector `a[href*="Videos.LiquidFilters/Enable"]` (route `Features/{id}/Enable/{tenant?}`,
   `OrchardCore.Features/Controllers/AdminController.cs:226`). Confirm both on `/Admin/Features`.
3. **The page `/about` after saving the template.** Expected: the navigation bar, the main menu, and the single line
   `ABOUT / About - Orchard Core` in the main column, with no masthead. This follows from the order of the alternates
   (`ContentAutorouteShapeTableProvider.cs:17-27`, `DefaultHtmlDisplay.cs:355-373`) and from the template resolver
   being asked before the files of the theme (`TemplatesShapeBindingResolver.cs:41-58`). If the theme's own template is
   shown instead, the narration of `template-1` and `template-4` is wrong: report it, do not record.
4. **Monaco keeps the typed text.** The value of `textarea.template-editor-value` must equal the line of
   `recording-plan.md` after `template-3`. Auto-closing of `{`, `"` and the suggestion list could change the text. If it
   does, `Keyboard.InsertTextAsync` inserts text without key events; the suggestion list is then not shown, and the
   second sentence of `template-2` ("the editor suggests the names of the registered filters, so shout is on the
   list") must be removed from the narration.
5. **The suggestion list shows `shout`.** The narration of `template-2` is based on the source only: the filter names
   of the site are added to the editor's list by `GetIntellisenseEndpoint.cs:75`, and the list opens when a space is
   typed after a pipe inside `{{ }}` (`liquid-intellisense.ts:139-153`). Confirm that the list is on screen while
   `shout` is typed, and that the selectors `.suggest-widget.visible` and `#Content_editor .monaco-editor` match.
6. **Pronunciation.** Check with `scripts/pronounce.py` the spellings of the `spoken` map in `storyboard.json`
   (`ILiquidFilter`, `ISiteService`, `ProcessAsync`, `ToStringValue`, `with_site_name`, `NilValue`) and listen to
   "slugify", "shout" and "Fluid". The map is applied in order, so the longer key `TemplateOptionsBuilder` comes before
   `TemplateOptions`, and `ToStringValue` before `StringValue`.
7. **The page of the documentation.** `src/docs/reference/modules/Liquid/README.md` already ends with a **Video**
   section that embeds another video. Where the marker of this video goes (under the section "Extending Liquid from a
   module") is for the documentation pull request to decide.
