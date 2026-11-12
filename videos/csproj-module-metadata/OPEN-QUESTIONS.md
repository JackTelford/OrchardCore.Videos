# Open questions: Module Metadata in Your Project File

Every statement of the narration was read from the Orchard Core source (commit 92db4a83) or from the documentation page
`src/docs/guides/leverage-csproj-meta-info/README.md`. Nothing was built or run, and nothing in the repository declares a
module with these item lists, so these points need a person with a build and a running site.

## 1. What the build passes for a metadata that an item leaves out

The documentation calls almost every metadata optional, and gives defaults ("0.0" for the version, false for the three
flags). In the targets, each attribute argument is `%(OrchardCoreModules.Name)` and so on, which MSBuild expands to an
empty string when the item has no such metadata. Read with an empty string, the attribute code gives:

- Name falls back to the Id, the category to "Uncategorized", and the priority to the module's (`FeatureAttribute.cs`,
  `FeaturesProvider.cs`): fine.
- The version stays empty: the `?? "0.0"` of `ModuleAttribute.Version` applies to null only.
- `Convert.ToBoolean("")` throws a `FormatException` for the three flags (`FeatureAttribute.cs`, constructor).

Whether MSBuild's attribute writer passes an empty string, passes null, or fails on a missing numbered parameter was not
visible from the source.

- **Checked:** the two targets files and the attribute classes.
- **To confirm:** build a module whose item leaves out Version and the three flags, read
  `obj/<configuration>/<framework>/<project name>.AssemblyInfo.cs`, and start the site.
- **Effect on the video:** the narration never calls a metadata optional and states no default for the version or the
  flags, and the slide examples give every metadata a value. `parts-2` depends on this answer: it says that a feature
  whose category, priority, or description is empty uses the module's, and that a module which declares a feature is no
  longer a feature itself unless one of the declared features has its identifier (`FeaturesProvider.cs`; for a theme,
  `ThemeFeaturesProvider.cs` adds the main feature, so the narration speaks of a module). The fallback itself is safe in
  the code (`Categorize`, `Prioritize` and `Describe` in `FeaturesProvider.cs` and `FeatureAttribute.cs` handle an empty
  and a null value), but it assumes the build accepts an item that leaves those metadata out, or sets them to an empty
  value.

## 2. Does the module of the slides build and load?

The slides `module-1` to `module-4` show one `OrchardCoreModules` item with every metadata, and `parts-1` shows one
`OrchardCoreFeatures` item. They were written from the targets and were not built.

- **To confirm:** in a scratch copy of `OrchardCMS/OrchardCore`, create `src/OrchardCore.Modules/Acme.Greetings/` with the
  project file of the slide (add `Sdk="Microsoft.NET.Sdk.Razor"`, the `FrameworkReference` to `Microsoft.AspNetCore.App`
  and the `ProjectReference` to `OrchardCore.Module.Targets`, as `OrchardCore.Cors.csproj` does), reference it from
  `OrchardCore.Cms.Web`, build, and open Tools, Features.
- **Expected:** one feature named "Acme Greetings", with the identifier `Acme.Greetings`, listed under "Content
  Management", with the description "Greets the visitors of a site." and a badge for the Contents feature. The attributes
  in the `AssemblyInfo.cs` file carry the values of the item.
- **Effect on the video:** none if it matches. The narration of `build-5` describes the pages from the views only.

## 3. The module attribute declared twice

`build-1` says "the Module attribute can be applied only once to an assembly", because `ModuleAttribute` and
`ThemeAttribute` are declared with `AllowMultiple = false`. (The targets also add a `ModuleMarkerAttribute` to every module
project, which `Module.cs` ignores when a real `ModuleAttribute` exists, so the narration does not say that an assembly
holds one attribute of that kind.) The documentation says the contents of `Manifest.cs` may be commented out "at your
discretion".

- **To confirm:** keep the `[assembly: Module(...)]` of `Manifest.cs` and add the item to the project file, and look at the
  build error (a duplicate attribute is expected). Also try one feature declared in both places: the feature attribute
  allows several, and the identifier would then appear twice.

## 4. The `OrchardCoreDebugTargets` messages

`build-3` says the build prints a message for each module and each feature. The messages have `Importance="high"` and the
condition `'$(OrchardCoreDebugTargets)' == 'true'` (`OrchardCore.Module.Targets.targets:149` and `:199`).

- **To confirm:** `dotnet build -p:OrchardCoreDebugTargets=true` prints them at the default verbosity.
- **Narrated:** `build-3` says the messages show "most of the values of the item": the module and feature messages do not
  print `EnabledByDependencyOnly`, and the feature message ends with a stray `)` (`OrchardCore.Module.Targets.targets:150`
  and `:200`).
- **Not narrated:** the message of the theme (`OrchardCore.Theme.Targets.targets:64`) ends its last two values with
  `'%(OrchardCoreThemes.AlwaysEnabled')` and `'%(OrchardCoreThemes.EnabledByDependencyOnly')`, a quote inside the
  reference, so the narration speaks of modules and features only. Try the property on a theme project too.

## 5. Appending to the embedding properties

`build-4` states only that the embedding runs after the targets in `OrchardCoreEmbeddingAfterTargets` (default
`AfterResolveReferences`) and before those in `OrchardCoreEmbeddingBeforeTargets` (default `GenerateAssemblyInfo`). The
documentation also shows appending a target of your own in a `PropertyGroup` of the project file. The video does not show
it, because `OrchardCore.Module.Targets.targets:48-49` assigns both properties without a condition and without reading the
previous value, in a file that is imported after the body of the project (`Directory.Build.targets` inside the
repository, the `build` folder of the package outside it). A value set in the project body would then be replaced.

- **To confirm:** set the property in the project body as the guide shows, and check with `dotnet build -v:diag` whether the
  custom target runs before the embedding. If it does not, the guide needs another place for the property.

## 6. Mixing the specific lists and `OrchardCoreAttributes`

The filters of `OrchardCoreAttributes` only run when the specific list of the kind is empty (the `Count() == 0` conditions at
`OrchardCore.Module.Targets.targets:109` and `:117`, and `OrchardCore.Theme.Targets.targets:12`). `parts-4` says to use one
list for every declaration and does not talk about mixing.

- **To confirm:** a project with an `OrchardCoreFeatures` item and an `OrchardCoreAttributes` item of type feature embeds
  the first only.

## 7. The Features page hides some features by default

The Features page has two filters selected by default, "Hide on demand" and "Hide always enabled"
(`Views/Admin/Features.cshtml`). A feature declared with `EnabledByDependencyOnly` or `AlwaysEnabled` set to true is hidden
until the filter is cleared. The narration of `build-5` says the page "lists a feature with its name ...", which stays true,
and does not say that every feature is visible.

## 8. Pronunciation

The `spoken` map of `storyboard.json` adds spellings that no other video has used: "Manifest dot C.S.", "M.S. Build", and
the spaced forms of `OrchardCoreModules`, `OrchardCoreFeatures`, `OrchardCoreThemes`, `OrchardCoreAttributes`,
`OrchardCoreDebugTargets`, `OrchardCoreEmbeddingAfterTargets`, `OrchardCoreEmbeddingBeforeTargets`, `AfterResolveReferences`,
`GenerateAssemblyInfo`, `InternalsVisibleTo`, `DefaultTenant`, `AlwaysEnabled`, `EnabledByDependencyOnly` and `ModuleType`.

- **To confirm:** listen to each with `scripts/pronounce.py` before narrating, in particular "M.S. Build" (the voice may read
  "MSBuild" correctly without it) and "Manifest dot C.S." at the end of a clause.
