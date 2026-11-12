# Open questions: Localize a Module

What the author could not confirm from the source or the documentation. A person with a running site and a sample
module confirms each point before the video is recorded. This file is for the recorder and the reviewers, so it may
hold code names and shell commands.

## Pronunciations to confirm with `scripts/pronounce.py`

The author could not run `pronounce.py` (it writes audio). The `spoken` map of `storyboard.json` holds these new
spellings, and a person confirms each one with a sample before the voice is generated:

- `IStringLocalizer`, `IHtmlLocalizer`, `IViewLocalizer`, `IStringLocalizerFactory`, `IHtmlLocalizerFactory`: spoken
  "I String Localizer", "I H.T.M.L. Localizer", "I View Localizer", and the same with "Factory".
- `PO file` spoken "P.O. file", `POT` spoken "P.O.T.", `OCLOC` spoken "O.C.L.O.C.".
- `MyNamespace.MyService` spoken "My Namespace dot My Service", and `MyModule.Views.Welcome` spoken "My Module dot
  Views dot Welcome".
- `Plural-Forms` spoken "Plural Forms", and `Arg helper` spoken "Argument helper".
- The long identifiers `LocalizationSource`, `CommonLocalizationSources`, `UpdateLocalizationCatalog`,
  `UpdateLocalizationCatalogs`, `LocalizationCatalogOutputPath`, `SkipLocalizationExtraction`,
  `OrchardCore.Localization.Build`, `OrchardCore.Abstractions` and `PrivateAssets`, spoken as separate words.
- The Liquid filter `t` (step `loc-2`), which the voice may read as a word or as the letter; and `LocalizationStrict`
  (not narrated) is only on a slide.
- Does the voice say "placeholder zero" and "module ID" naturally?

## The recording (clip `04-plural-screen`)

Read from the source, never run:

- With the Blog recipe, the Content Items list should hold five items, so it reads "5 items / 5 items in total": the
  Menu "Main Menu" (`OrchardCore.Menu/Migrations/menu.recipe.json:11`), the Taxonomies "Tags" and "Categories"
  (`OrchardCore.Taxonomies/Migrations.cs:27`), the Blog "Blog" and the Article "About" (`blog.recipe.json:139` and
  `:251`). The blog post and the Footer have a type that is not listable. This was derived from the recipe and the
  migrations, never from a running site: confirm the count and the five names, and that no other item has a display
  text containing `About` (the search of step `items-2` must leave exactly one row).
- After the search, the summary reads "1 item / 1 item in total". The text comes from
  `Views/ContentsAdminListSummary.cshtml:7`; confirm how the spaces and the slash look on screen, since the second
  part is in a muted span.
- Confirm the selector `.action-bar input[type="search"]` finds the search box, that Enter in it submits the page's
  form and reloads the list, and that `label#items` is unique on the page. If the page offers a layout selector
  (List or Grid), confirm the summary shows in both and record in the default one.
- The page heading is "Manage Content" (`Views/Admin/List.cshtml:17`): confirm the breadcrumb title shows it on
  screen, since step `items-1` names that page.
- The clip relies on the default localizers (no Localization feature). The summary goes through `T.Plural`, so
  `NullHtmlLocalizer.cs:37` handles the plural argument; confirm on screen that the text reads "N items", never the
  name of a type.
- Step `ctx-2` says "module ID" for the first part of a view's context. The extraction uses the assembly name
  (`OrchardCore.Localization.Build.targets:37`), and at run time the context is built from the path of the view under
  the module; the two match when the module ID is the assembly name, which is the default. Confirm with a sample
  module.
- Confirm that the green ring of `HighlightAsync` around `label#items` does not hide the count, and that the
  captions "Plural" and "Singular" fit.

## The slides

- The code of the slides comes from the documentation and from the source, trimmed. It was not compiled. Build it
  once in a sample module (`MyService`, `MyPermissions`, the `Localize` and `Plural` calls of `src-2`, the
  `Note` and `Arg` calls of `note-1`) to confirm it compiles with the usings the documentation shows, and that
  `Plural` on an `IViewLocalizer` needs `Microsoft.AspNetCore.Mvc.Localization` (the documentation names only
  `Microsoft.Extensions.Localization`).
- The template of `ext-3` and `note-1` follows `PotWriter.cs` (notes, then `Format arguments`, then the source
  reference, then the flags). Run `dotnet msbuild <project> -restore -t:UpdateLocalizationCatalog` on a sample module
  that references `OrchardCore.Localization.Build`, and compare the file with the slide. The command needs an SDK
  recent enough for the worker, which the documentation describes; the video does not name a version.
- Step `how-2` says that, without the Localization feature, a localizer returns its text as written. The source
  registers `NullStringLocalizerFactory` and `NullHtmlLocalizerFactory` by default
  (`src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:142` and `:143`). Confirm it on a site where
  the feature is disabled.
- Step `loc-2` says an `IHtmlLocalizer` encodes its format arguments. The documentation says so (`README.md` of the
  Localize module, section "Deferring localization with `LocalizationSource`"), and the behavior belongs to
  ASP.NET Core's `LocalizedHtmlString`, which is not in the Orchard Core source. Confirm it with an argument that
  holds markup.
- Step `ext-4` says that Razor views and Liquid files are read. The extraction compiles Razor with the Razor compiler
  of the SDK that drives the build, which could not be run here. Confirm that a view of a sample module and a
  Liquid file with `{{ "Hello" | t }}` reach the template.
