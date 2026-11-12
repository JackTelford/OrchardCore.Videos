# Recording plan: source-generators

This video has **no recording**. The opening and closing cards are drawn by the producer, and every chapter between them is made of
slides drawn by `slides.py` (`kind` is `slide` for each clip). A developer video shows code on slides, and the
generators have no screen in a browser.

## The site it needs

None. There is no site to run, no Auto Setup recipe, no tenant, no time zone, no feature to enable, no setting, no
external service, no seed data, no `state.json`, no `recorder/` folder and no recorder run (`dotnet run` is never needed).

## Order of the clips, and the state they change

No clip is recorded, so no clip depends on another and none changes a site: there is no snapshot to restore before a new
take. The slides can be drawn and the narration generated in any order.

## What to do

1. From the repository root, with the skill's `scripts` folder on `PYTHONPATH`, render every slide once in memory to
   check that nothing overflows (no file is written):

   ```bash
   cd videos/source-generators
   PYTHONPATH=../../.agents/skills/orchardcore-video-producer/scripts python -c "
   import json, slides
   for ch in json.load(open('storyboard.json', encoding='utf-8'))['chapters']:
       for clip in ch['clips']:
           if clip['kind'] == 'slide':
               for step in clip['steps']:
                   print(step['id'], slides.slide(step['id'], ch['title'], 'Chapter ' + ch['id']).size)
   "
   ```

2. Narrate: `python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/source-generators`. The
   `"spoken"` map of `storyboard.json` holds the spellings of the identifiers (see "Pronunciation" below).
3. Assemble: `python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/source-generators`.

Clips run in any order: none depends on another, and none changes anything.

## Slides

Every slide shows code taken from the Orchard Core source (the file name is in the title bar of the code panel) or
the output of the generators, shortened. The generated code was written from the generators' source
(`src/OrchardCore/OrchardCore.SourceGenerators/*.cs`), not copied from a build: see `OPEN-QUESTIONS.md`.

### Chapter 01, `01-why` (bullet cards)

- `why-1`: heading "What happens at runtime"; four cards, the first two bright: "Arguments by reflection" and
  "Typed shapes by proxy". Source: `src/OrchardCore/OrchardCore.DisplayManagement/Arguments.cs:63-73` (the reflection fallback of `Arguments.From`),
  `ShapeFactoryExtensions.cs:191-210` of the same folder (the proxy of a strongly typed shape).
- `why-2`: heading "OrchardCore.SourceGenerators"; the same four cards, the last two bright: "Generators write the code" and
  "Analyzers report mistakes". Source: the four classes of `OrchardCore.SourceGenerators`.

### Chapter 02, `02-arguments` (code)

- `args-1`: heading "1. Mark a partial class"; `AdminMenuFilter.cs` with the `NavigationArguments` class
  (`src/OrchardCore.Modules/OrchardCore.Admin/AdminMenuFilter.cs:78-83`), next to three cards: Attribute
  "GenerateArguments", Modifier "partial", Properties "public, with a getter".
- `args-2`: heading "2. The generator writes the other half"; `OrchardCore_Admin_NavigationArguments.g.cs` (the generator names the file after the type, with underscores), the generated half, written
  from `src/OrchardCore/OrchardCore.SourceGenerators/ArgumentsProviderGenerator.cs:112-150`, next to three cards: Names, Values, Reflection.
- `args-3`: heading "3. Pass it to Arguments.From"; the call in `AdminMenuFilter.cs` (lines 58-63), and `Arguments.cs` with the
  fast path of `Arguments.From<T>` (`Arguments.cs:63-73`).
- `args-4`: heading "What the generator needs"; four cards: "The class is partial", "Public instance properties",
  "Declared on the class itself", "No other base class". Source: `ArgumentsProviderGenerator.cs:58-70` and `:116` (the base class of the generated half).

### Chapter 03, `03-anonymous` (code)

- `anon-1`: heading "An anonymous object passed to Arguments.From"; `PagerSlimAsync`
  (`src/OrchardCore/OrchardCore.Navigation.Core/ShapeFactoryExtensions.cs:31-38`), and two points under the panel.
- `anon-2`: heading "The generated interceptor"; `ArgumentsFromInterceptors.g.cs`, written from
  `ArgumentsFromInterceptor.cs:114-207`.
- `anon-3`: heading "Interceptor or reflection"; three cards: "Interceptor", "Reflection", "Skipped". Source:
  `ArgumentsFromInterceptor.cs:75-98`, `src/OrchardCore/OrchardCore.DisplayManagement/ArgumentsReflectionHelper.cs`.

### Chapter 04, `04-shapes` (code)

- `shapes-1`: heading "Initialize in a display driver"; `DebugSettingsDisplayDriver.cs`
  (`src/OrchardCore.Modules/OrchardCore.Settings/Drivers/DebugSettingsDisplayDriver.cs:33-47`), and two points under the panel.
- `shapes-2`: heading "A generated class replaces the proxy"; `ShapeFactoryGenerator.g.cs` with the generated shape class, written
  from `ShapeFactoryGenerator.cs:479-552`, next to three cards: Derives from, Implements, Intercepts.
- `shapes-3`: heading "GenerateShape on a model you own"; `NavigationItemViewModel.cs`
  (`src/OrchardCore/OrchardCore.Navigation.Core/NavigationItemViewModel.cs`), next to three cards: Attribute, Generated, Needs.
  Source of the card texts: `src/OrchardCore/OrchardCore.DisplayManagement/GenerateShapeAttribute.cs`.

### Chapter 05, `05-analyzers` (code)

- `analyzers-1`: heading "OCSG001: a sealed model"; the panel titled "Example from the documentation" holds a sealed `DebugSettingsViewModel` and
  the `Initialize` call (from the documentation page of source generators; the class in the source,
  `src/OrchardCore.Modules/OrchardCore.Settings/ViewModels/DebugSettingsViewModel.cs`, is not sealed), and the text of the warning
  (`SealedShapeTypeAnalyzer.cs:20`).
- `analyzers-2`: heading "OCSG002: a feature of another module"; left, the manifest and the startup class of the ClamAV
  Antivirus feature (`src/OrchardCore.Modules/OrchardCore.Antivirus/Manifest.cs`, `Startup.cs`); right, an invented module
  `MyCompany.Reports` whose startup class names `OrchardCore.Contents`, and the text of the warning
  (`FeatureAttributeTargetAnalyzer.cs:23`).

### Chapter 06, `06-build` (code)

- `build-1`: heading "OrchardCore.Commons.props"; the `InterceptorsNamespaces` property and the `ProjectReference` of the
  generator project (`src/OrchardCore.Build/OrchardCore.Commons.props:17-18` and `:56-58`), and two points under the panel (the
  `Directory.Build.props` files of `OrchardCore`, `OrchardCore.Modules` and `OrchardCore.Themes` import the file).

- `build-2`: heading "What the reference decides"; three cards: "With the analyzer reference", "Without it", "At runtime".
  Source: `Arguments.cs:63-73` (a type that is not `INamedEnumerable<object>` takes the reflection path) and
  `ArgumentsProviderGenerator.cs` (the generated half exists only when the generator runs).

### Chapter 07, `07-choose` (bullet cards)

- `choose-1`: heading "Choosing an approach"; four cards, all bright: "Arguments you reuse", "A value you pass once",
  "A view model you own", "Every other typed shape".

## Pronunciation

`storyboard.json` writes the identifiers as they are in code (the captions show them) and maps them to what the voice says
in `"spoken"`. Listen to each with `scripts/pronounce.py` before narrating, and change the spelling if the voice reads one
wrongly: `Arguments.From` ("Arguments dot From"), `OCSG001` and `OCSG002` ("O.C.S.G. zero zero one", "O.C.S.G. zero zero two"),
`IShape` ("I Shape"), `IPositioned` ("I Positioned"), `PropertyBasedNamedEnumerable`, `InterceptorsNamespaces`,
`GenerateArguments`, `GenerateShape`, `CreateAsync`, `OrchardCore.SourceGenerators` ("Orchard Core dot Source Generators"),
and the three class names. The spellings are untested.

## Checks for private data

- The slides hold only code of the Orchard Core source, the invented module name `MyCompany.Reports`, and the invented
  class and file names the generators produce (`Interceptor_0F3A9C`, `GeneratedShape_A1B2C3`). No person, email address,
  domain, credential or path of a machine appears.
- OCR a frame of every slide after assembling and search it for `C:\`, `Users`, an email address and a token. The rendering
  step above writes no file, and no PNG of a slide is kept.
