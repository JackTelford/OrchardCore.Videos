# Open questions: Create a Module

What was read from the Orchard Core source (commit `92db4a83`) and the documentation could not be run, so a person
must confirm the following on a running site before the video is recorded. The narration states only what the source
shows; each item says what was checked and what to confirm.

## 1. Does the generated module build as it comes?

Checked: `src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Module/Startup.cs` imports
`OrchardCore.Templates.Cms.Module.Drivers`, `.Handlers`, `.Models` and `.Settings` without a condition, while
`.template.config.src/template.json` excludes the files that declare those namespaces when `AddPart` is false (the
default). The first three lines of the file hold an empty `#if (AddPart)` block, as if the imports had once been inside
it.

The same file also imports `OrchardCore.Liquid` and `OrchardCore.Data.Migration`, which the four package references
of the generated project file may not provide when no part is added (not checked).

Confirm: run `dotnet new ocmodulecms -n Greetings.OrchardCore` and build it. If it does not build, report it to the
Orchard Core team. `recording-plan.md` does not depend on the answer: it replaces `Startup.cs` with a file that has only
the usings the module needs, and the narration and slides show the module without a part either way.

## 2. Versions: templates, host and module

Checked: the narration and slides name no version. The slides install the templates with
`dotnet new install OrchardCore.ProjectTemplates`, without the version the documentation pins
(`getting-started/templates/README.md`), so the video stays accurate. The host of the recording is a checkout of one
commit that targets net11.0 (`src/OrchardCore.Build/TargetFrameworks.props:7,11`), so the released packages that a
generated project refers to cannot match it. `recording-plan.md` therefore builds the module inside the checkout, under
`src/OrchardCore.Modules`, whose `Directory.Build.props` and `Directory.Build.targets` import the Module Targets (the
way `OrchardCore.Features` becomes a module), with project references written after
`src/OrchardCore.Modules/OrchardCore.Features/OrchardCore.Features.csproj` and the template's `Views/_ViewImports.cshtml`
(which needs DisplayManagement and ResourceManagement).

Not checked: the copy of the source that was read has no root `Directory.Build.props` or `Directory.Packages.props`, and
no build was run, so nothing here was built. In particular, whether warnings are treated as errors for the
generated files under `src/OrchardCore.Modules`, and whether the host's `dotnet add ... reference` restores, are unknown.

Confirm: the module builds there and its feature appears on the Features page. Confirm that the released templates
generate the files shown on the slides (`Manifest.cs`, `Startup.cs`, `Controllers/HomeController.cs`,
`Views/Home/Index.cshtml`, the project file), and that the Features page of the checkout matches the labels in the plan.

## 3. Is `Manifest.cs` required?

Checked: the guide says "The module must contain a manifest file (`Manifest.cs`)". In the source,
`OrchardCore.Module.Targets` adds a `ModuleMarkerAttribute` to every module assembly
(`src/OrchardCore/OrchardCore.Module.Targets/OrchardCore.Module.Targets.targets:80`), and `Module.cs:40-44` falls back to that marker, so a module without a
manifest should still load and be listed under its assembly name, in the category "Uncategorized"
(`src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/FeatureAttribute.cs:290-306`). The narration therefore lists the Module Targets package and the project reference
as the two checks, and does not name the manifest.

Confirm: remove `Manifest.cs` from a test module, build, and check the Features page.

## 4. Manifest changes: build and run, or disable and enable?

Checked: the guide says that if a manifest change is not reflected, "disable and re-enable the module". The source
reads the manifest from the assembly attributes once, when it first loads the extensions (`ExtensionManager`), and the Features
page does not read it again when a feature is enabled or disabled. The narration says to run the site again after a
change to the manifest.

Confirm: change `Description` and `Category`, build and run, and check that the Features page shows them without
disabling and enabling the feature.

## 5. Text of the message after Enable

Checked: `AdminController.cs:317` builds the message with `H.Plural(features.Length, "The feature {1} was enabled.", ...)`
and the feature name as the first argument. The narration does not quote it, and `recording-plan.md` expects
"The feature Greetings was enabled."

Confirm: the text on screen, and that the message shows for about 5 seconds after the page reloads.

## 6. The page at `/Home/Index`

Checked: the route and controller come from the template, the layout from `ThemeViewStart.cs`, which assigns the
theme layout whenever the request has an active theme.

Confirm: on the Blog recipe site, `/Home/Index` shows the heading inside the theme's layout once the feature is
enabled, and nothing else answers that address. The narration does not say what happens after the feature is
disabled.

## 7. Selectors of the admin menu

Checked: `Navigation-admin.cshtml` and `NavigationItem-admin.cshtml` of `TheAdmin` render "Tools" as a button inside
`#left-nav`, with its children in a `ul` whose `data-title` is "Tools". The menu restores the groups left open.

Confirm: the selectors in `recording-plan.md` against the running admin, and that Tools starts collapsed on the
dashboard.

## 8. Visual Studio steps

Checked: the narration of the chapter "Install the module" follows the guide ("Dependencies", "Add Project
Reference"). Those labels belong to Visual Studio, not to Orchard Core, so they could not be read in the source. No
Visual Studio screen is shown.

Confirm: the labels in a current Visual Studio.

## 9. Pronunciation

Spoken spellings added to the storyboard's `spoken` map, each to be checked with `scripts/pronounce.py` before the
voice is generated: "dot net", "Manifest dot C.S.", "Startup dot C.S.", "Program dot C.S.", "Startup Base",
"Home slash Index", "Add Orchard C.M.S.", "Use Orchard Core", "Project Reference", "Orchard Core dot Project
Templates", "Greetings dot OrchardCore".

## 10. The default filters of the Features page

Checked: `Assets/ts/features-index.ts` applies the filters when the page loads, and the Visibility filter starts with
"Hide on demand" and "Hide always enabled" selected (`Views/Admin/Features.cshtml:65-66`), so the summary reads "Showing V
of T features" with V lower than T. A feature that is neither on demand nor always enabled, such as the module of this
video, stays in the list.

Confirm: the row of the module is listed under "Samples" without changing a filter, and `recording-plan.md` waits for the
summary as described there.
