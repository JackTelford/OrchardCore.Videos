# Recording plan: Module Metadata in Your Project File

The video has no recording. It is a developer video about a project file, and the two admin pages it names (the Features
page and the Themes page) are described on a slide, not shown. Every chapter between the opening and the closing card is
a slide clip, drawn by `slides.py`, so the recorder is not needed and nothing has to be captured. The opening card
(`00-intro`) and the closing card (`99-outro`) are drawn by `assemble.py` from the storyboard.

## The site it needs

None. No Orchard Core site, no Auto Setup, no tenant, no time zone, no feature to enable, no setting, no external service,
no seed data, no `state.json` and no sign-in. There is nothing to restore before a new take.

The names on the slides are invented: the module `Acme.Greetings`, the author `Acme Software`, the website
`https://example.com`. The project file snippets are shown as they are in `slides.py`; they are not built for the video.

## The order of the clips, and what changes the site

The video has six clips, and none of them is a recording:

| Clip | Kind | Steps |
|---|---|---|
| `00-intro` | card | `intro` |
| `01-why` | slide | `why-1`, `why-2` |
| `02-module` | slide | `module-1`, `module-2`, `module-3`, `module-4` |
| `03-parts` | slide | `parts-1`, `parts-2`, `parts-3`, `parts-4` |
| `04-build` | slide | `build-1`, `build-2`, `build-3`, `build-4`, `build-5` |
| `99-outro` | card | `outro` |

Narrate (`narrate.py videos/csproj-module-metadata`), then assemble (`assemble.py videos/csproj-module-metadata`). No clip
depends on another, and no clip changes anything.

## Slide clips

Each line says what the slide shows when its narration ends. The heading is the slide's own heading, below the chapter
title. A "bright" line or card is one that is not dimmed.

### `01-why`

- `why-1`: the heading "The manifest of a module", the code panel `Manifest.cs` (a `[assembly: Module(...)]` attribute with
  Name "Acme Greetings", Author, Website, Version, Description and Category), and two cards: "Typed by hand" (Name,
  author, website, version) and "Read by the site" (from the assembly attributes).
- `why-2`: the heading "An item list becomes an attribute", the code panel "Project file" with the `AssemblyAttribute`
  item for `InternalsVisibleTo` (from the documentation page), and two cards: "Declare an item" and "Properties".

### `02-module`

The four slides show the same code panel, `Acme.Greetings.csproj`, a `PropertyGroup` with `AssemblyVersion` and an
`ItemGroup` holding one `OrchardCoreModules` item with all its metadata. The lines that are not bright are dimmed. The
heading is "The OrchardCoreModules item" on all four.

- `module-1`: the `ItemGroup` lines and the `OrchardCoreModules Include="$(MSBuildProjectName)"` line are bright. Cards:
  "Include" (the module identifier) and "Usually" (the name of the project).
- `module-2`: the ten lines from `Name` to `Tags` are bright: Name, ModuleType, Category, Priority, Description, Author,
  Version, Website, Dependencies, Tags. Cards: "Name, Category, Description" (show on the Features page) and
  "Dependencies, Tags" (lists, split by a semicolon, a comma, or a space).
- `module-3`: the `AssemblyVersion` line, the `Include` line and the `Version="$(AssemblyVersion)"` line are bright.
  Cards: "Properties", "Include" with `$(MSBuildProjectName)` and "Version" with `$(AssemblyVersion)`.
- `module-4`: the three lines `DefaultTenant`, `AlwaysEnabled` and `EnabledByDependencyOnly`, each `"false"`, are bright.
  Cards: "DefaultTenant" (default tenant only), "AlwaysEnabled" (cannot be disabled) and "EnabledByDependencyOnly" (only as
  a dependency).

### `03-parts`

- `parts-1`: the heading "Declare features", the code panel with one `OrchardCoreFeatures` item (`Acme.Greetings.Banner`,
  with a comment saying that `Acme.Greetings` is declared the same way), and two cards: "Per feature" (name, category,
  description) and "Not on a feature" (author, website, version, tags).
- `parts-2`: the heading "When a module declares features" and three cards: "No feature declared" (the module itself
  is the only feature), "A feature declared" (only the declared features exist) and "Empty category, priority, or
  description" (the module's values are used).
- `parts-3`: the heading "Declare a theme", the code panel with the `OrchardCoreThemes` item as the documentation page
  lists it (every value is `"..."`, with `Base` after `Name`), and three cards: "Base" (names the base theme), "Theme
  project" (`OrchardCore.Theme.Targets`) and "Module project" (`OrchardCore.Module.Targets`).
- `parts-4`: the heading "One list for everything", the code panel with the `OrchardCoreAttributes` item (`Include`,
  `Type`, and `...`), and three cards: "Type feature" (declares a feature), "Type theme" (declares a theme) and "Any other
  Type" (declares a module).

### `04-build`

- `build-1`: the heading "Comment out or delete Manifest.cs", the code panel `Manifest.cs` with every line commented out
  (shown green), and two cards: "Module attribute" (applied once to an assembly) and "The project file" (declares the module).
- `build-2`: the heading "Three rules, checked at build" and four cards: "At most one module" (OC2002), "At most one
  theme" (OC3001), "Never both" (a module and a theme together fail with OC3002) and "Any number of features".
- `build-3`: the heading "See what the build embeds", a terminal panel with `dotnet build -p:OrchardCoreDebugTargets=true`,
  and two cards: "Each module and each feature" (gets a message with its values) and "The values" (most of those of
  the item).
- `build-4`: the heading "Where the embedding runs" and two cards: `OrchardCoreEmbeddingAfterTargets` (default
  `AfterResolveReferences`) and `OrchardCoreEmbeddingBeforeTargets` (default `GenerateAssemblyInfo`).
- `build-5`: the heading "Where the values show in the admin" and two cards: "Tools, Features" (name, identifier,
  category, description, and dependencies of a feature) and "Design, Themes" (name, description, author, website, tags,
  and version of a theme).

## Where the labels come from

- The menu path "Tools", "Features": `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`. What the page
  shows for a feature: `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml`
  (name and identifier), `FeatureEntry.Description.SummaryAdmin.cshtml` (description), `FeatureEntry.DefaultTags.SummaryAdmin.cshtml`
  (dependencies), and the category as the heading of each group in `Controllers/AdminController.cs:91-94` and
  `Views/Admin/Features.cshtml`.
- The menu path "Design", "Themes": `src/OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs`. What the page shows for a
  theme: `src/OrchardCore.Modules/OrchardCore.Themes/Views/Items/ThemeEntry-Title.SummaryAdmin.cshtml`,
  `ThemeEntry-Descriptions.SummaryAdmin.cshtml` and `ThemeEntry-Attributes.SummaryAdmin.cshtml` (author, website, tags,
  version).
- The item names and the metadata: `src/OrchardCore/OrchardCore.Module.Targets/OrchardCore.Module.Targets.targets`
  (`OrchardCoreEmbeddingAfterTargets` and `OrchardCoreEmbeddingBeforeTargets` at lines 48-49, `OrchardCoreAttributes`
  filtered into features at line 107 and into modules at line 115, the `OrchardCoreFeatures` attribute at line 152, the
  `OrchardCoreDebugTargets` messages at lines 149 and 199, OC2002 at line 171, the `OrchardCoreModules` attribute at
  line 202)
  and `src/OrchardCore/OrchardCore.Theme.Targets/OrchardCore.Theme.Targets.targets` (`OrchardCoreThemes`, OC3001 at line 23,
  OC3002 at line 33).
- The attributes: `src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/ModuleAttribute.cs` and `FeatureAttribute.cs`,
  and `src/OrchardCore/OrchardCore.DisplayManagement/Manifest/ThemeAttribute.cs`.

## Checks for private data

- Read every slide once: the only names are `Acme.Greetings`, `Acme Greetings`, `Acme Software`, `https://example.com`,
  `OrchardCore.Contents`, and the Orchard Core names. There is no real person, email address, token, password or path of
  a machine.
- The opening and closing cards show the video's title and `docs.orchardcore.net`, nothing else.
- Run the OCR scan of the skill on the final video as for any other, even though no browser is recorded.
