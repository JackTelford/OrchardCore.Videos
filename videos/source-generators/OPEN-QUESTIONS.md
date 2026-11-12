# Open questions: source-generators

Everything the narration says was read from the source of Orchard Core (`src/OrchardCore/OrchardCore.SourceGenerators`,
`OrchardCore.DisplayManagement`, `OrchardCore.Build/OrchardCore.Commons.props`) or from its documentation. The points
below were not verified. The narration does not state them as facts; a person must confirm them by building a project
before the video says more.

## 1. How a module outside the Orchard Core solution gets the generators

- Checked: `OrchardCore.Commons.props` adds the `ProjectReference` to `OrchardCore.SourceGenerators` (as an analyzer,
  `ReferenceOutputAssembly="false"`) and the `InterceptorsNamespaces` entry for `OrchardCore.DisplayManagement.Generated`.
  The `Directory.Build.props` files of `src/OrchardCore`, `src/OrchardCore.Modules` and `src/OrchardCore.Themes` import it.
  No project packs that file into a NuGet package (searched the `.csproj`, `.props` and `.targets` files of `src`).
- Checked: the module template (`src/Templates/.../OrchardCore.Templates.Cms.Module.csproj`) references the packages
  `OrchardCore.Module.Targets`, `OrchardCore.ContentManagement`, `OrchardCore.ContentTypes.Abstractions` and
  `OrchardCore.DisplayManagement`, and nothing about the generators. `OrchardCore.SourceGenerators.csproj` packs its assembly
  under `analyzers/dotnet/cs`, and `OrchardCore.Abstractions.csproj` packs the localization analyzers there, but no project
  packs the source generators into another package.
- Not checked: whether a module built from the NuGet packages gets the generators, which package or reference it needs,
  and whether it must add `InterceptorsNamespaces` itself.
- To confirm: create a module from the template, add a `GenerateArguments` class, call `Arguments.From` with it and with an
  anonymous object, build it, and look at what is generated (`EmitCompilerGeneratedFiles` set to `true` writes the files
  under `obj`). Then record the steps that make it work in the documentation page, and add a chapter to the video.
  The video says only what the build of the Orchard Core solution does ("Turning them on").
  `build-2` states only what the code implies: without the analyzer reference a `GenerateArguments` class has no generated
  half, so `Arguments.From` takes its reflection path (`Arguments.cs:63-73`). Confirm it with the same build.

## 2. The generated code on the slides comes from the generators' source, not from a build

`args-2`, `anon-2` and `shapes-2` show the code the generators write, reconstructed from the `StringBuilder` calls of
`ArgumentsProviderGenerator.cs`, `ArgumentsFromInterceptor.cs` and `ShapeFactoryGenerator.cs`, with `global::` prefixes and
blank lines removed, and the names that depend on a hash or an id (`Interceptor_0F3A9C`, `GeneratedShape_A1B2C3`) invented.

- To confirm: build `OrchardCore.Admin`, `OrchardCore.Navigation.Core` and `OrchardCore.Settings` with
  `EmitCompilerGeneratedFiles` set to `true`, and compare `NavigationArguments`, the interceptor for `PagerSlimAsync`
  and the generated shape for `DebugSettingsViewModel` with the slides.

## 3. A sealed model at a call the shape generator intercepts

`ShapeFactoryGenerator.GetInvocationInfo` excludes abstract models, models that implement `IShape`, models with
`GenerateShape` and models without an accessible parameterless constructor, but not sealed models, while a generated shape
derives from the model. `SealedShapeTypeAnalyzer` reports `OCSG001` for the same call.

- Not checked: what the build reports for a sealed view model passed to `Initialize` (only `OCSG001`, or also an error from
  the generated file).
- The narration (`analyzers-1`) states only what `OCSG001` reports and its three fixes, as the documentation page does.
- To confirm: build the example of the documentation page (a sealed `DebugSettingsViewModel`) in a module of the solution.

## 4. A class marked with GenerateArguments cannot name another base class

The generated half declares `PropertyBasedNamedEnumerable` as the base class (`ArgumentsProviderGenerator.cs:116`). The C#
compiler rejects partial declarations with different base classes (error CS0263), and rejects a missing `partial` modifier
(error CS0260); both were checked by compiling a small equivalent of the two halves outside the repository. The generator
itself reports nothing for these cases.

- To confirm: mark a class that has a base class with `GenerateArguments` in the solution and read the build output.

## 5. Pronunciation

The `"spoken"` map of `storyboard.json` is untested. Listen to each entry with `scripts/pronounce.py` (it writes audio, so
it was not run here): "Arguments dot From", "O.C.S.G. zero zero one", "O.C.S.G. zero zero two", "I Shape", "I Positioned",
"Create Async", "Interceptors Namespaces", "Property Based Named Enumerable", "Orchard Core dot Source Generators".
Check also that the voice reads "Initialize", "partial", "Metadata" and "NavigationItemViewModel" ("Navigation Item View
Model") correctly in the middle of a sentence.

## 6. A Feature attribute that names a feature of another module

`analyzers-2` says the name is not found at runtime and the class joins every feature of its own module. Checked:
`ExtensionManager.cs:204-224` (a type whose feature name is not a feature of its extension is added to all features of
that extension) and `ShellContainerFactory.cs:178-189`. Not run.

- To confirm: in a module with two features, add a startup class with `[Feature("OrchardCore.Contents")]`, enable only one
  of the module's features, and check that the class is active.
