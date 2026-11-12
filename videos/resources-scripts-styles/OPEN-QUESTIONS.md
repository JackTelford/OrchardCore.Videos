# Open questions: Resources: Declare and Include Scripts and Stylesheets

Everything in the narration was read from the source of Orchard Core at commit `92db4a83` and from the documentation
page of the Resources module. Nothing was run, so the points below are what a person must confirm on a running site
before recording, or when the voice is generated. None of them is stated in the narration as more than the source says.

## For the recording of `07-settings`

1. **The tab and its position.** The Resources options are placed in a tab named by the placement
   `Content:1#Resources;20` (`src/OrchardCore.Modules/OrchardCore.Settings/Drivers/DefaultSiteSettingsDisplayDriver.cs:54`)
   and `AdminTabs` prints the tab name as the link text
   (`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:59`). Confirm that **Settings, General** shows
   the tabs **Site**, **Resources** and **Cache**, and that the four options fit in the viewport (1536x864) without
   scrolling. `recording-plan.md` assumes they do.
2. **The admin menu locators.** `figcaption#settings .nav-group-label` (the **Settings** group, a toggle-only button) and
   `li#general a.item-label` (the **General** link) come from `src/OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`
   and `NavigationItemLink-admin.cshtml`. Check them with `Recorder probe /Admin "() => document.querySelector('#general a')?.href"`.
   Also check that **Settings** is collapsed when a fresh sign-in opens `/Admin`.
3. **The Save button.** `SiteSettings.SaveButton.cshtml` puts it in `div.action-bar-bottom`
   (`src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml`). Confirm whether it is visible
   without scrolling, or sticky; `settings-7` scrolls to it only if it is not.
4. **The reload warning.** `AddTenantReloadWarningWrapper` adds the wrapper `Settings_Wrapper__Reload`
   (`src/OrchardCore/OrchardCore.DisplayManagement/Extensions/BuildShapeContextExtensions.cs:6`), whose template
   shows "The website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness
   during the process." (`Views/Settings-Reload.Wrapper.cshtml:4`). The file name follows the same convention as
   `Settings-Resources.Edit.cshtml` (shape `Settings_Edit__Resources`), so it should be found, but confirm that the warning
   is on the page and how much height it takes, since the four options of the Resources tab must stay in view. The
   narration of `settings-7` does not depend on it: saving valid settings asks for the shell to be released when the
   request ends (`DefaultSiteSettingsDisplayDriver.cs:140`, and `Controllers/AdminController.cs:106-127` suspends the
   release when the model is invalid). Confirm on the site that a saved change is seen by the next page load.
5. **The default values.** The code defaults are cache busting on, framework CDN off, an empty CDN base url, and
   Resource Debug Mode "From environment". No recipe of the source sets them (a search for `UseCdn`, `AppendVersion`,
   `CdnBaseUrl` and `ResourceDebugMode` in the recipes finds nothing), but confirm them on the site that is recorded.

## For the voice (`narrate.py`)

6. **New spoken spellings.** `storyboard.json` "spoken" adds: `SetUrl` as "Set U.R.L.", `SetCdn` as "Set C.D.N.",
   `CDN` as "C.D.N.", `base url` as "base U.R.L.", `IResourceManager` as "I Resource Manager", `IConfigureOptions` as "I Configure Options",
   `OrchardCore.ResourceManagement.Abstractions` as "Orchard Core dot Resource Management dot Abstractions", and
   `scriptblock` and `styleblock` as "script block" and "style block". Check each with `scripts/pronounce.py` before the
   voice is generated.
7. **Identifiers left to the voice.** `DefineScript`, `DefineStyle`, `SetVersion`, `SetDependencies`,
   `ResourceManagementOptions`, `ResourceManifests`, `AddResourceConfiguration`, `HeadLink`, `HeadScript`, `FootScript`,
   `RegisterResource`, `UseVersion`, `AtFoot`, `RegisterHeadScript`, `RegisterFootScript`, `RegisterMeta` and
   `AppendMeta` are written as in the code. Listen to them; add a "spoken" entry for any the voice joins or splits wrongly.
   Also listen to "CDN", "jQuery", "Popper" and "the at option" (in `include-4`, `view-1` and `view-2`), which should
   not be heard as the preposition alone.

## Read from the source, not run (the slides depend on them)

8. **Which tags only declare.** A script with a name and a source is only declared unless `at` is also set; a style with a
   name and a source is declared and required at once, at the head by default
   (`src/OrchardCore.Modules/OrchardCore.Resources/Liquid/ScriptTag.cs`, `ProcessInlineDeclaration`, and `StyleTag.cs`,
   `ProcessInlineDeclaration`; the Razor tag helpers behave the same). The documentation examples for scripts omit `at`
   (see the documentation issues reported with this video). Confirm by rendering a view with each line of the `view-1`
   slide and reading the page source.
9. **Dependency order.** The `bootstrap` script without a version resolves to the highest declared release, which depends
   on `popperjs` (`ResourceManagementOptionsConfiguration.cs:398-412`); `depends_on` adds names after the declared ones
   (`ResourceManager.ExpandDependenciesImplementation`). The narration says only that Popper is rendered first. Confirm in
   the page source of a page that includes it.
10. **The markup on `links-1` and `links-2`.** The `link` output is the one in the documentation. The combined `meta`
    element (`<meta content="orchard, modules" name="keywords" />`) is derived from `MetaEntry.Combine` and the sorted
    attributes of `TagBuilder`. Confirm in the page source.
11. **A script without `at` that many views ask for.** `ResourceManager.RenderLocalScript` writes a requested script at
    every call, so a view rendered several times may write the same in-place script several times. The video does not claim
    "once" for in-place scripts; it says the layout renders the resources collected for the head and the foot once.
12. **An unknown name or version.** `ResourceManager.GetRequiredResources` throws when it renders; the Razor `resources` tag
    helper catches and logs it (`ResourcesTagHelper.cs`), as the documentation says. The Liquid `resources` tag has no
    such `try`, so confirm what a Liquid layout shows. A Razor or Liquid `script` tag without `at` renders in place and
    reaches the same code through `RenderLocalScript`, with no `try` either. The narration says only that an error is
    raised when the resources are rendered.
13. **A named block renders once.** `ResourceManager.RegisterResource` returns the same settings for the same type and
    name (`ResourceManager.cs:38-55`), so a named block is one required resource however many views ask for it. When
    several views give the same name different content, `FindResource` keeps the last definition of an unversioned
    resource (`ResourceManager.cs:203-207`). The narration of `view-3` says only that the block renders once. Confirm on
    a page that two views asking for the same name write one block.
