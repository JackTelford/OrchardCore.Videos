# Open questions: Create a Theme From an HTML Template

Everything in the narration was read from the documentation page of the video and from the source of Orchard Core
(commit `92db4a83`). The points below could not be verified without a running site, a built theme or the voice. A person
confirms each one before the clips are recorded or the video is assembled.

## On a running site

1. **The path through the admin menu.** `OrchardCore.Layers/AdminMenu.cs:26-34` puts "Widgets" under "Design" and "Zones"
   under the top-level group "Settings". `OrchardCore.Themes/AdminMenu.cs:18-21` puts "Themes" under "Design". The code
   settles how the groups open: "Settings" and "Design" have no page of their own (their `Href` is `#`,
   `NavigationManager.cs` `GetUrl`), so `NavigationItemLink-admin.cshtml:24-36` renders the label as a button that opens
   the group, and the separate "Toggle {name}" button (`NavigationItem-admin.cshtml:107-124`) is not drawn for them. The
   default menu behavior is "Focused" (`AdminMenuBehavior.cs`), so opening a group closes the others. Still to confirm on
   the Blog recipe site: that the selectors of `recording-plan.md` (`ul#adminMenu button.nav-group-label` for the groups,
   `ul#adminMenu a.item-label` for the children, both derived from the markup and not run) find exactly one element
   each, and that the admin menu of the recipe (the `AdminMenu` step of `blog.recipe.json`, with its "Blog", "Main Menu"
   and "Content" entries) does not hide or move "Settings" and "Design". Probe them with `Recorder probe`.
2. **The text of the Zones field.** The recipe's `LayerSettings` lists `Content` and `Footer`
   (`blog.recipe.json:121-124`) and the editor joins them with `", "` (`Drivers/LayerSiteSettingsDisplayDriver.cs:43`). Confirm the field
   shows `Content, Footer`.
3. **The cards of the Widgets page.** `Views/Admin/Index.cshtml:42-115` draws one card per zone with its widgets (the
   badge with the layer name is added at line 57). Confirm that the Footer card lists the widget "Footer" with the badge
   "Always" (the recipe puts it in the layer "Always", zone "Footer", `blog.recipe.json:1090-1101`), and that the right
   column lists the layers "Always" and "Homepage" (`blog.recipe.json:1141-1175`).
4. **The Themes page.** Confirm that the card "The Blog Theme" says "This is the current Site theme" and that at least one
   other card shows the button "Make Current" without scrolling (`ThemeEntry-ButtonsMakeCurrent.SummaryAdmin.cshtml:7`).
   The code settles the rest: `Themes/Controllers/AdminController.cs:Index` lists every theme feature that is available to
   the application (a referenced theme project is one), and `Drivers/ThemeEntryDisplayDriver.cs:19-29` draws "Make
   Current" on every card that is not current, enabled or not. Which other themes the application references (and so how
   many cards appear) was not checked on a running site.
5. **The front end without internet access.** `Layout.liquid` of TheBlogTheme asks the browser for two Google Fonts
   stylesheets (lines 12-13) and the Bootstrap script bundle comes from a CDN. Confirm that the page looks acceptable with
   the chosen setup (online, or offline with fallback fonts) and keep it the same for every take.
6. **The post list and the post.** Confirm that `/` shows the post "Man must explore, and this is exploration at its
   greatest" with "Posted by admin on <date>", and that `/blog/post-1` shows the masthead with the title, the subtitle,
   the author and the date above the body (`Content-BlogPost.liquid:14-30`).

## About the voice

7. **Spoken forms.** The `spoken` map of `storyboard.json` replaces written forms with the words the voice should say. None of
   them has been tried with `scripts/pronounce.py`. Confirm, or change, each one: `wwwroot` as "web root", `index.html` as
   "index dot HTML", `Layout.liquid` as "Layout dot liquid", `placement.json` as "placement dot json", `issetuprecipe` as
   "is setup recipe", `render_body`, `render_section` and `shape_render` without the underscore, the camel-case names of
   the Orchard Core API (`IConfigureOptions`, `ResourceManagementOptions`, `AddResourceConfiguration`, `DefineStyle`,
   `DefineScript`, `SetUrl`, `SetDependencies`, `DisplayText`, `RenderBodyAsync`, `RenderSectionAsync`, `MenuItem`, `MenuItemLink`, `Model.Content`) split into
   words (`MenuItemLink` comes before `MenuItem` in the map, because `narrate.py` replaces in order), `Content-BlogPost.Summary` as "Content dash Blog Post dot Summary", and the two paths `css/styles.css` and
   `/MyTheme/css/styles.css` spoken letter for letter with "slash" and "dot". `CSS` as "C.S.S" and `CDN` as "C.D.N" (also inside the two paths), to be checked the same way. Also confirm that "The Blog Theme" (the
   spoken form of `TheBlogTheme`) is what the viewer should hear, since the card of the Themes page shows "The Blog Theme".

## About the code on the slides

8. **The code was not compiled for this video.** The slides show the code of the documentation page (`MyTheme`) and of
   TheBlogTheme, trimmed. The classes of the documentation page are the same as those of TheBlogTheme
   (`ResourceManagementOptionsConfiguration.cs`, `Startup.cs`), which compile, but the `MyTheme` copies were not built.
9. **The starting layout of a new theme.** The theme project template (`dotnet new octheme`) adds a basic
   `Views/Layout.liquid` by default (`OrchardCore.Templates.Theme`, `AddLiquid`). The narration does not rely on it
   (it says to copy the template's main page to a file named `Layout.liquid`).
10. **Where the static files are served.** The prefix of the address is the id of the module that holds the `wwwroot`
    folder (`ModuleEmbeddedStaticFileProvider.cs:28-46`): `ModuleInfo.Id` when the manifest sets one, otherwise the assembly
    name (`Module.cs:85-90`), which is the name of the theme project. It is not the `Name` of the manifest: TheBlogTheme
    shows "The Blog Theme" on its card and serves its files under `/TheBlogTheme/`. The narration therefore says "the name
    of the theme project". A theme whose project is not named like its resources (`MyTheme.OrchardCore`, as
    `getting-started/theme.md` suggests) uses that other name in the addresses.

## Left out on purpose

11. **Shape Tracing.** The documentation page tells the reader to enable "the Shape Tracing tooling" to see which data and
    shapes a template receives. No such feature exists in the source at the commit above (no module, no menu entry, no
    string), and the page it links to (`shapes.md`) does not describe it. The narration points to the Templates reference
    and to the Views folder of TheBlogTheme instead. If the tooling exists elsewhere, a chapter step can describe it.
12. **Razor details.** The layout step names the Razor equivalents, as the documentation does. The project template
    (`dotnet new octheme`) already uses `Microsoft.NET.Sdk.Razor` (`OrchardCore.Templates.Theme.csproj:1`), while the
    project file of TheBlogTheme uses `Microsoft.NET.Sdk` and only Liquid; the video does not discuss project files,
    because another video covers creating a theme project.
13. **Overriding the menu link template.** The documentation says to override `Views/MenuItemLink.liquid` for the CSS classes
    of the navigation. The Menu module ships `MenuItemLink-LinkMenuItem.cshtml`, `-ContentMenuItem.cshtml` and
    `-HtmlMenuItem.cshtml`, and the shape gets those names as alternates (`MenuItemAlternatesFactory.cs:64-86`) that
    `DefaultHtmlDisplay.cs:355-369` tries before the base shape type. A plain `MenuItemLink` template of a theme therefore
    looks unreachable for those types, and TheBlogTheme overrides the three typed variants instead. The narration says
    "a MenuItemLink template for each type of menu item, as TheBlogTheme does". Confirm on a running site that a theme
    with only `Views/MenuItemLink.liquid` leaves the links of a Link Menu Item unchanged.
