# Open questions: display-shapes

What was read in the Orchard Core source (commit `92db4a83`) and could not be confirmed without a running site. The
narration says only what the source shows; each item names what a person must check on the recording site.

## 1. The text of the shape debug comments (slide `debug-3`)

**Checked.** `OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:258-325` writes
`<!--shape-start type:{type} bindings:{binding} -->` before the shape and `<!--shape-end type:{type} -->` after it, where
`{binding}` is `{binding name} => {binding source} ({engine})`; a shape with wrappers lists the binding of each wrapper
after the first one, separated by ` | `. The engine is `razor` for a `.cshtml` source, `liquid` for `.liquid`, and
`code` otherwise (a `[Shape]` method has the source `{class full name}::{method name}`,
`ShapeAttributeBindingStrategy.cs:49`). For a template file, the binding name is the lower-case name made by
`Descriptors/ShapeTemplateStrategy/BasicShapeTemplateHarvester.cs:32-58` (`Menu-Main.cshtml` gives `menu__main`), and the
source is the relative path of the file (`ShapeTemplateBindingStrategy.cs:127-142`). That path starts with
`Areas/{extension id}` (`OrchardCore.Abstractions/Modules/Module.cs:91`, `OrchardCore/Extensions/ExtensionManager.cs:184`,
`OrchardCore.Mvc.Core/FileProviders/FileProviderExtensions.cs:49`), so a theme with the id `Contoso` gives
`Areas/Contoso/Views/Menu-Main.cshtml`. The documentation shows `Themes/Contoso/Views/Menu-Main.cshtml`, the binding
name `Menu__Main` and an end comment without a space before `-->`; the code differs on all three.

**To confirm.** With the option on, open the home page of the recording site, view its source, and copy the first
`shape-start` lines. The `debug-3` slide shows the example of the documentation with the values the code writes
(`Areas/Contoso/Views/Menu-Main.cshtml`, `menu__main`, a space before `-->`). Check the path prefix a real site writes,
for a theme and for a module, and the case of the binding name; if they differ, change `DEBUG_COMMENTS` in `slides.py`.
The narration speaks of "its source, such as the path of a template", so it stays true.

## 2. Which shapes show comments on the blog home page

**Checked.** When the content of a shape comes from the dynamic cache, no binding runs, and `DefaultHtmlDisplay.cs:244-265`
writes no comment. The theme layout renders the main menu with a `cache_id` (`TheBlogTheme/Views/Layout.liquid:32`).

**To confirm.** Which shape types have comments on `/` after the setting is saved (the content shapes are expected to).
The narration says "each shape rendered by a binding is wrapped", which holds either way.

## 3. A theme template over a module template (step `page-1`)

**Checked.** `OrchardCore.Menu/Views/Menu.cshtml` and `TheBlogTheme/Views/Menu.liquid` both bind the `Menu` shape.
A theme is ordered after every module (`OrchardCore.DisplayManagement/Extensions/ThemeExtensionDependencyStrategy.cs`),
and later features overwrite earlier bindings (`Descriptors/DefaultShapeTableManager.cs:122-143`,
`Descriptors/FeatureShapeDescriptor.cs:105-113`), so the theme template is the one used. The documentation says the same
(`topics/display/shapes.md`, "Bindings, descriptors, and the shape table").

**To confirm.** On the recording site, the markup of the main menu is the one of `Menu.liquid` (`div#navbarResponsive`
with the class `navbar-collapse`), not the one of the module.

## 4. The admin menu locators (step `debug-1`)

**Checked.** In `TheAdmin/Views/NavigationItem-admin.cshtml` and `NavigationItemLink-admin.cshtml`, a first-level item
without a page of its own (`Settings`, which has no action in `OrchardCore.Settings/AdminMenu.cs`) is a
`button.item-label` that expands its children; an item with a page (`Debugging`) is an `a.item-label`. Both hold a
`span.title` with the text (`TheAdmin/Views/NavigationItemText.cshtml`, and `OrchardCore.Settings/Views/NavigationItemText-settings.Id.cshtml`
for `Settings`). Only a first-level item with children has the class `nav-group-label`
(`NavigationItemLink-admin.cshtml:71-74`), so the plan uses `button.nav-group-label` for `Settings`: another item with
children and no page of its own, titled `Settings`, would also be a `button.item-label` at a deeper level.
Nothing is expanded when the page loads.

**To confirm.** With `Recorder probe`, that the two locators of the plan find one element each, that the click on
`Settings` expands the group, and that the menu is not in its collapsed (icons only) state at 1536 px.

## 5. What stays on screen at the end of `debug-1`

**Checked.** After the save, the controller redirects to the same page and shows `Site settings updated successfully.`
(`OrchardCore.Settings/Controllers/AdminController.cs:119`).

**To confirm.** That the message is still visible when the narration ends, about 13 seconds after the step starts. If it
fades, the checked box and the hint are enough; nothing else changes.

## 6. The front end of the recording site (clip `02-page`)

**Checked.** The selectors come from the theme files: `#navbarResponsive` (`TheBlogTheme/Views/Menu.liquid:1`),
`header.masthead` (`Content-Blog.liquid:6`), `.post-preview` (`Content-BlogPost.Summary.liquid:1`).

**To confirm.** That the three elements are visible at 1536 by 864 pixels (the links of the navigation bar are shown at
that width), that the header image loads (the recipe's media), that `.post-preview` is in view after a 400 pixel
scroll, and that the signed-in `admin` sees no administration bar on the front end.

## 7. Pronunciation

The `spoken` entries of `storyboard.json` are plain words, except one dotted initialism: `I H.T.M.L. Display` for
`IHtmlDisplay`, so that the voice does not read `Html` as a word. The plain words are `I Shape`, `I Shape Factory`,
`I Display Helper`, `I Display Result`, `I Shape Binding Resolver`, `I Shape Attribute Provider`, `I Layout Accessor`,
`Shape Execute Async`, `Create Async`, `Display Async`, `Add Shape Attributes`, `Shape Result`, `Display Type`,
`Date Time`, `Time Span`, `shape new`, `shape add properties`, `shape render`, and
`Content hyphen Blog Post dot Summary`. Check them, and the voice's reading of `HTML`, `.NET`, `Razor` and `Liquid`,
with `scripts/pronounce.py` before narrating.

## 8. The code on the slides

The code is copied from `topics/display/shapes.md`, `reference/modules/DisplayManagement/README.md` and the source.
Four things are not a verbatim copy. The sample value `Toyota` of the documentation is `Acme` on every slide, because a
real make is not fake data. Slide `contract-3` omits the last lines of the documentation example and ends with `// ...`.
Slide `code-1` puts two consecutive blocks of the documentation in one panel. Slide `debug-3` writes the values the code
produces (item 1) instead of those of the documentation. None of the code was compiled.

The `CarShapes` method of slide `code-1` casts the shape to `Car`, as the documentation does: it works for a shape
created with `CreateAsync<Car>` (slide `contract-3`), not for one created with `New.Car(...)` in Razor, which is a
dynamic `Shape`.

## 9. The title of the browser tab (step `debug-2`)

**Checked.** `TheBlogTheme/Views/Layout.liquid:6` writes `<title>{{ "PageTitle" | shape_new | shape_stringify }}</title>`.
`shape_stringify` renders the shape through `DefaultHtmlDisplay` (`ShapeStringifyFilter.cs`), and `PageTitle` is a
`[Shape]` method (`PageTitleShapes.cs:20`), so with the option on its comments are written inside `<title>`. The content
of a `title` element is text in HTML, not markup, so the tab is expected to show the comment text. The narration says only
that the content of the page looks the same.

**To confirm.** With the option on, read `document.title` on `/` (for example with `Recorder probe`), and that the
comments appear in the page source and not in the body text. The capture does not show the tab strip, so the clip is not
affected; if the title is clean, nothing changes.
