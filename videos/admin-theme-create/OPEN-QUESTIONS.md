# Open questions: Create an Admin Theme: Restyle the Admin

Everything in the narration was read from the Orchard Core source at commit `92db4a83` and from the documentation
pages of the tracker entry. The points below could not be run, so a person confirms each one on a running site
before recording. Nothing below is stated in the narration beyond what the source shows.

## 1. The sample theme builds and appears on the Themes page

**Read:** the `Manifest.cs`, `_ViewImports.cshtml`, `AdminBranding.cshtml` and `theme.css` of `recording-plan.md` follow the
documentation (`guides/create-admin-theme`, `reference/themes/admin-theme.md`) and the source of `TheAdmin`
(`src/OrchardCore.Themes/TheAdmin/Views/_ViewImports.cshtml`, `Manifest.cs`).

**Not run:** the `MyAdminTheme.csproj` of the plan (project references instead of the template's package references) and
the `ProjectReference` added to `OrchardCore.Cms.Web.csproj` were not built.

**Confirm:** the solution builds; **Design**, **Themes** lists a card `MyAdminTheme` with the tags line `admin`, the
author `My name` and the version `0.0.1`; its footer has `Make Current` and `Enable`.

## 2. Deleting `Views/Layout.liquid` of the template (step `create-3`)

**Read:** the `octheme` template adds `Views/Layout.liquid` unless `AddLiquid` is false
(`src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Theme/.template.config.src/template.json:29-49`).
A template of a derived theme replaces the template of its base theme for the same shape: the documentation says so
("Views in `MyAdminTheme` will override views in `TheAdmin`"), and the shape table keeps the last binding for a shape
(`OrchardCore/OrchardCore.DisplayManagement/Descriptors/FeatureShapeDescriptor.cs:105-112`, features ordered base theme first in
`DefaultShapeTableManager.cs`). The narration therefore says that a Layout in the theme replaces the layout of
TheAdmin.

**Not run:** what the admin looks like when the Liquid layout is kept.

**Confirm (optional, one take outside the capture):** keep `Views/Layout.liquid`, make the theme current, and check that
the admin loses its menu and top bar. Then delete the file, which is what the video shows. If the admin keeps its
layout, change `create-3`.

## 3. The stylesheet required from `AdminBranding`, and its order (steps `template-3`, `template-4`, `forms-3`)

**Read:** `Layout.cshtml` of TheAdmin renders the branding before the page head, with the comment "Branding and Navbar
are pre-rendered to allow resource injection" (`src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:14-16`, `:44`). A
`style` tag with `asp-src` and `depends-on` declares the file with its dependencies
(`src/OrchardCore/OrchardCore.ResourceManagement/TagHelpers/StyleTagHelper.cs:72-92`), and the resource manager expands
the dependencies of a resource before the resource (`ResourceManager.cs`, `ExpandDependenciesImplementation`).

**Why the sample lists three styles, not only `the-admin`:** the `the-admin` style is defined without dependencies
(`src/OrchardCore.Themes/TheAdmin/ResourceManagementOptionsConfiguration.cs:31-33`). `Layout.cshtml:34` and `:39` add
`bootstrap` and `TheAdminLayout` as dependencies only on the require settings of their own tag
(`StyleTagHelper.cs:127-130`), and they run after the branding (`:15`), so `theme.css` is the first style required.
Expanding `depends-on="the-admin"` alone would therefore put `the-admin` and `theme.css` into the ordered dictionary
before `TheAdminLayout` and `bootstrap`, and the dictionary keeps the position of the first insertion
(`src/OrchardCore/OrchardCore.ResourceManagement/ResourceDictionary.cs:6` and `:45`, a `System.Collections.Specialized.OrderedDictionary`). The page would load `TheAdmin.min.css` before
`bootstrap.min.css`, and Bootstrap would override rules of TheAdmin. The sample names `TheAdminLayout,bootstrap,the-admin`,
the order in which `Layout.cshtml` requires them (`:29`, `:38`, `:39`). `bootstrap` without a version resolves to the
highest version, 5.3.8, since 4.6.1 is also defined (`ResourceManager.cs`, `SelectMatchingResource`;
`src/OrchardCore.Modules/OrchardCore.Resources/ResourceManagementOptionsConfiguration.cs:366` and `:432`).

**Not run:** the page source of an admin page. The sample is written for a left-to-right culture: `Layout.cshtml:31-35`
requires `bootstrap-rtl` instead of `bootstrap` for a right-to-left culture, so the sample is not right for such a site.

**Confirm:** in the page head of an admin page, the `link` tags come in this order: `/TheAdmin/css/admin-layout.min.css`,
`bootstrap.min.css` (from `/OrchardCore.Resources/Styles/`), `/TheAdmin/css/TheAdmin.min.css` (or the files without
`.min` in a debug run), then `/MyAdminTheme/Styles/theme.css`. The admin must look as it does with TheAdmin (colors,
buttons, cards). The rules of `theme.css` must win over those of TheAdmin at the recording width (1536 px, where
TheAdmin's own `ocat-limited` rule for wide screens, `Assets/scss/components/_ocat.scss`, also applies). If the order
differs, or the General settings page stays vertical, change `depends-on` in the sample, and `template-3` and `template-4`.

## 4. The result on screen (chapter 06)

**Read:** the labels `Site name`, `Page title format` and `Base url` use `ocat-wrapper`, `ocat-label` and `ocat-end`
(`src/OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:15-40`); `Default Time Zone` and `Page size` use
`ocat-limited-wrapper` and `ocat-limited` (`:42-60`).

**Confirm:** with the stylesheet loaded, the labels sit at the left of their fields, right-aligned, as the
documentation's animation shows; the brand in the top bar is the plain text `Orchard Core`, linked to `/Admin`.
The original `AdminBranding` of the Admin module also adds a favicon link in the `HeadMeta` zone
(`src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminBranding.cshtml:2-4`); the sample leaves it out, and the video does not
show the browser tab.

## 5. The menu path to the General settings (step `result-2`)

**Read:** `Settings` and `General` are the labels of the menu builder
(`src/OrchardCore.Modules/OrchardCore.Settings/AdminMenu.cs:36` and `:39`), and no menu builder of the source adds a
parent named "Configuration" (`src/OrchardCore.Modules/OrchardCore.Admin/AdminMenu.cs:26` also puts `Admin` under a
top-level `Settings`). The Admin module documentation calls the path "Configuration → Settings → Admin"
(`src/docs/reference/modules/Admin/README.md`, lines 95, 236 and 544); this is reported as a documentation issue.

**Confirm:** the sidebar shows `Settings` at the top level, with `General` under it. If a parent such as
`Configuration` exists on the running site, change the narration of `result-2` ("Open Settings, General") and the plan.
Until then, the plan allows `NavigateAsync("/Admin/Settings/general")` (the route is
`[Admin("Settings/{groupId}", "AdminSettings")]` with the group `general`).

## 6. Timing of the confirmation messages (step `enable-3`)

**Read:** a success message stays 5 seconds
(`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`), and the page after `Make Current` is
rendered after the tenant restarts, which can take a few seconds.

**Confirm:** both messages (`MyAdminTheme was enabled` and `MyAdminTheme was set as the default Admin theme`) are on
screen while the narration of `enable-3` says "reports that". Adjust the pause before the click.

## 7. Pronunciations to confirm with `scripts/pronounce.py`

The `spoken` map of `storyboard.json` adds spellings that a person confirms before the narration is generated:

| Written | Spoken as | Why |
|---|---|---|
| `TheAdmin` | The Admin | one word in the code, two in speech |
| `MyAdminTheme` | My Admin Theme | same |
| `AdminBranding` | Admin Branding | same |
| `ocat ` and `ocat-` | O.C.A.T. | the abbreviation of Orchard Core Admin Theme; confirm that the letters sound better than a word |
| `the-admin` | the admin | the name of the resource, with a hyphen |
| `Hint.Admin` | Hint dot Admin | the name of the shape |
| `Model.Hint` | Model dot Hint | the property read by the template |
| `TheAdminLayout` | The Admin Layout | the name of a style; its entry comes first, since `TheAdmin` is part of it |
| `wwwroot` | W.W.W. root | the name of the static files folder |
| `depends-on` | depends on | the attribute name, with a hyphen |
| `Base url` | Base U.R.L. | the label of a field |

The keys `ocat ` (with a trailing space) and `ocat-` avoid changing words such as "location".

## 8. The commands of the slide `create-1`

**Read:** the commands follow `docs/getting-started/theme.md` (the folder is named after the theme, then `dotnet new octheme`,
`dotnet sln add` and `dotnet add ... reference`), with the theme name `MyAdminTheme` and the layout of the guide
`guides/create-admin-theme` (`MySite` and the theme side by side). The documentation page runs `dotnet sln add` inside the
theme folder and references `../OrchardCore.Themes/MyTheme.OrchardCore.csproj`, a path without the folder of the theme (`dotnet new octheme` creates the files in the current folder, which the
documentation names after the theme); the slide runs both commands from the folder that holds the solution and gives the path
with the folder. This is reported as a documentation issue.

**Not run:** the commands. The Orchard Core source has no solution file at its root in the copy that was read, so a solution
name could not be checked.

**Confirm:** the commands work in the order shown for a new site and theme side by side (one solution file in the folder
that holds `MySite` and `MyAdminTheme`). The video does not run them.
