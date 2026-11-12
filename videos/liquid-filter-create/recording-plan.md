# Recording plan: Write a Liquid Filter

The video is a developer video. Thirteen of its 19 steps are slides, drawn by `slides.py`; two are cards; four are one
browser recording, the clip `05-template`. Nothing here is decided while recording: follow the sections in order.

Paths such as `src/OrchardCore.Modules/OrchardCore.Templates/AdminMenu.cs:19` are files of the Orchard Core checkout
(`OrchardCMS/OrchardCore`) that the site runs from. In this plan a selector stands for `scene.Page.Locator(...)`.

## The site it needs

- **Checkout:** a clean `OrchardCMS/OrchardCore` checkout of `main`, built with the SDK it asks for. The module below is
  added to it for the recording and removed afterwards (see "After the recording").
- **Provisioning (Auto Setup, no setup screen is recorded):** run `src/OrchardCore.Cms.Web` on `http://localhost:5010`,
  with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, and these environment variables
  (the keys are those of `src/docs/reference/modules/AutoSetup/README.md`):

  | Variable | Value |
  |---|---|
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

  The site name must be exactly `Orchard Core`: the narration of `class-4` and the page of `template-4` show it.
  The Blog recipe (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`) enables the Templates feature (which
  depends on Liquid), creates the article **About** at `/about`, and sets The Blog Theme.
- **Outbound calls:** set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address for the site process, as in
  `references/recording.md`. No external service is used.
- **Sign-in:** `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json`
  (git-ignored). The sign-in is never recorded.
- **Features to enable before recording:** the module below, called **Video Liquid Filters** on the Features page.
  Nothing else needs enabling.
- **Fake data:** the site holds only what the Blog recipe creates. No other seed data is needed.

### The module the video shows

Create the folder `src/OrchardCore.Modules/Videos.LiquidFilters/` in the checkout with these four files. The code
of `Startup.cs` and `WithSiteNameFilter.cs` is the code of the slides `delegate-1` to `class-3`, with the usings and
the namespace a file needs. `Directory.Build.props` and `Directory.Build.targets` of `src/OrchardCore.Modules` give the
project the shared build settings, including `ImplicitUsings` (`src/OrchardCore.Build/OrchardCore.Commons.props`).

`Videos.LiquidFilters.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Module.Targets\OrchardCore.Module.Targets.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Liquid.Abstractions\OrchardCore.Liquid.Abstractions.csproj" />
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Infrastructure.Abstractions\OrchardCore.Infrastructure.Abstractions.csproj" />
  </ItemGroup>

</Project>
```

`Manifest.cs`

```csharp
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Video Liquid Filters",
    Author = "The Orchard Core Team",
    Website = "https://orchardcore.net",
    Version = "0.0.1",
    Description = "Two Liquid filters for a video.",
    Category = "Development"
)]
```

`Startup.cs`

```csharp
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Modules;

namespace Videos.LiquidFilters;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptionsBuilder>(options =>
        {
            // A filter without dependencies.
            options.AddFilter("shout", (input, arguments, context) =>
                StringValue.Create(input.ToStringValue(context).ToUpperInvariant()));
        });

        // Added as a scoped service, and resolved each time the filter runs.
        services.AddLiquidFilter<WithSiteNameFilter>("with_site_name");
    }
}
```

`WithSiteNameFilter.cs`

```csharp
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;
using OrchardCore.Settings;

namespace Videos.LiquidFilters;

public class WithSiteNameFilter : ILiquidFilter
{
    private readonly ISiteService _siteService;

    public WithSiteNameFilter(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var site = await _siteService.GetSiteSettingsAsync();

        // The first argument is the separator, " | " when it is missing.
        var separator = arguments.At(0).IsNil() ? " | " : arguments.At(0).ToStringValue(context);

        return StringValue.Create(input.ToStringValue(context) + separator + site.SiteName);
    }
}
```

Then add this line to the `ItemGroup` of project references in `src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`:

```xml
<ProjectReference Include="..\OrchardCore.Modules\Videos.LiquidFilters\Videos.LiquidFilters.csproj" />
```

The feature id of the module is its assembly name, `Videos.LiquidFilters`.

### Seeding, outside the capture

1. Stop any running copy of the site (by its process ID). Build `src/OrchardCore.Cms.Web` and start it with the
   variables above. The first request provisions the site.
2. Run `Recorder setup`.
3. Enable the module. In the browser, open `/Admin/Features` (the menu is **Tools**, **Features**:
   `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`), type `Liquid Filters` in the search box
   (`#search-box`), and click the **Enable** link of **Video Liquid Filters**
   (`a[href*="Videos.LiquidFilters/Enable"]`, from
   `src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
   Wait until the row shows a **Disable** button.
4. Check, with the signed-in browser, that the filters are registered: opening
   `/OrchardCore.Liquid/Scripts/liquid-intellisense.js` shows a script that contains `'shout'` and `'with_site_name'`
   (`src/OrchardCore.Modules/OrchardCore.Liquid/Endpoints/Scripts/GetIntellisenseEndpoint.cs:75`).
5. Check that `/Admin/Templates` shows the message **Nothing here!** (no template exists), and that `/about` shows the
   article **About**, with its title in the header band (the page the template of the clip replaces).
6. Stop the site and copy its `App_Data` folder and `state.json` aside. This is the snapshot every take restores.

## The order of the clips and what they change

There is one recording, `05-template`. It needs the snapshot above and nothing else.

`05-template` **changes the site**: it saves the template `Content__Slug__about`. Before another take, stop the site,
restore the snapshot, and start it again (or delete the template with **Delete** in the list of `/Admin/Templates`).

## The clip `05-template`

Chapter 05, "Using the filters". It starts on `/Admin/Templates`, signed in as `admin` through `state.json`:
`await scene.OpenAsync("/Admin/Templates")`, then `await scene.StartCaptureAsync()`. The Templates page is empty and
shows its **Add Template** button.

The text typed in the editor, in two parts (the second part continues the line of the first):

- Part 1: `{{ Model.ContentItem.DisplayText | shout }}`
- Part 2: ` / {{ Model.ContentItem.DisplayText | with_site_name: " - " }}`

After step `template-3` the whole line is:

```
{{ Model.ContentItem.DisplayText | shout }} / {{ Model.ContentItem.DisplayText | with_site_name: " - " }}
```

Type it with the keyboard, not with `Fill`: the Content box is a Monaco editor (`monaco.editor.create`,
`src/OrchardCore.Modules/OrchardCore.Templates/Assets/ts/template-create.ts:14`) whose value is copied to the hidden
textarea `textarea.template-editor-value` on every change (`template-create.ts:19-21`). Typing `{` or `"` makes Monaco
insert the closing character, and typing the closing character types over it, so the typed text and the value stay
equal. Never press Enter or Tab in the editor: they accept an open suggestion.

### `template-1`

Page: `/Admin/Templates`, empty list.

Actions:

1. `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn.create"))`: the **Add Template** button
   (`src/OrchardCore.Modules/OrchardCore.Templates/Views/TemplateCreateButton.cshtml:6`). The page **Create Template**
   opens (`Views/Template/Create.cshtml:8`).
2. Wait until `#Content_editor .monaco-editor` is visible (the editor is created by a script after the page loads).
3. `await scene.TypeAsync(scene.Page.Locator("#Name"), "Content__Slug__about")`: the **Name** field
   (`Create.cshtml:18`, hint "The name of the template.").

Visible when the narration ends: the page **Create Template**, **Name** holding `Content__Slug__about`, and an empty
**Content** editor. The name selects the article whose path is `about`
(`src/OrchardCore.Modules/OrchardCore.Autoroute/Services/AutorouteAlternatesFactory.cs:30`; the page
`src/docs/reference/modules/Templates/README.md` lists `Content__Slug__[Slug]`).

### `template-2`

Page: **Create Template**, as left by `template-1`.

Actions:

1. `await scene.ClickAsync(scene.Page.Locator("#Content_editor"), pauseAfter: 300)`.
2. `await scene.Page.Keyboard.TypeAsync("{{ Model.ContentItem.DisplayText | ", new KeyboardTypeOptions { Delay = 45 })`.
   A space typed after a pipe opens the suggestion list
   (`src/OrchardCore.Modules/OrchardCore.Liquid/Assets/monaco/liquid-intellisense.ts:139-153`).
   Wait until `.suggest-widget.visible` exists.
3. `await scene.PauseAsync(2500)`: the list shows filter names.
4. `await scene.Page.Keyboard.TypeAsync("sh", new KeyboardTypeOptions { Delay = 120 })`, then `await scene.PauseAsync(1200)`:
   the list narrows to names that contain `sh`, and `shout` is one of them. The names come from the filters
   registered for the site (`GetIntellisenseEndpoint.cs:75`).
5. `await scene.Page.Keyboard.TypeAsync("out }}", new KeyboardTypeOptions { Delay = 90 })`.
6. If `.suggest-widget.visible` still exists, `await scene.Page.Keyboard.PressAsync("Escape")`.
7. Wait until the value of `textarea.template-editor-value` equals Part 1 exactly
   (`await scene.Page.WaitForFunctionAsync("() => document.querySelector('textarea.template-editor-value').value === '{{ Model.ContentItem.DisplayText | shout }}'")`).

Visible when the narration ends: the editor holds `{{ Model.ContentItem.DisplayText | shout }}`, with no suggestion
list. The actions take about 7 seconds; the narration is about 11.

### `template-3`

Page: **Create Template**, as left by `template-2`, the cursor at the end of the line.

Actions:

1. `await scene.Page.Keyboard.TypeAsync(" / {{ Model.ContentItem.DisplayText | with_site_name: \" - \" }}", new KeyboardTypeOptions { Delay = 45 })`.
2. If `.suggest-widget.visible` exists, press `Escape`.
3. Wait until the value of `textarea.template-editor-value` equals the whole line above.

Visible when the narration ends: the editor holds the whole line, with both expressions, and no suggestion list. If the
final value differs from the line (for example an extra `}` or `"`), do not record the take: see
`OPEN-QUESTIONS.md`.

### `template-4`

Page: **Create Template**, as left by `template-3`.

Actions:

1. `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save[value='Save']"))`: the **Save** button
   (`Create.cshtml:53`). The site returns to the list `/Admin/Templates` (the `returnUrl` of the **Add Template**
   button, `TemplateController.cs:367-378`), which shows the notification
   `The "Content__Slug__about" template has been created.` (`TemplateController.cs:204`) and the template in the list.
2. `await scene.PauseAsync(1500)`.
3. `await scene.NavigateAsync("/about")`. Wait until the main column of the page holds the text `ABOUT / About - Orchard Core`.

Visible when the narration ends: the page `/about` with the navigation bar of The Blog Theme (the brand **Orchard Core**
is the site name, `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27`), the main menu, and, in the main
column (`Layout.liquid:40`), the single line `ABOUT / About - Orchard Core`. There is no masthead image and no body
text: the template replaces the whole display of the article, including the `Header` zone of the theme's
`Content-Article.liquid`, which is expected. The first expression writes the title in capitals (`shout`), the second
writes the title, the separator `-` between spaces, and the site name (`with_site_name`).

Why this template is the one used for `/about`: the shape of the article has the alternate `Content__Slug__about`, added
after the alternates of its content type (`src/OrchardCore.Modules/OrchardCore.Autoroute/Services/ContentAutorouteShapeTableProvider.cs:17-27`);
the display looks at the last alternate first (`src/OrchardCore/OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:355`)
and asks the template resolver of the Templates module before the files of the theme (`DefaultHtmlDisplay.cs:359-373`,
`src/OrchardCore.Modules/OrchardCore.Templates/Services/TemplatesShapeBindingResolver.cs:41-58`).

## The slides and the cards

Each line says what the image shows. The images are drawn by `slides.py`; the code is the code of the module above.

- `intro`: the opening card, "Write a Liquid Filter".
- `how-1`: two panels, a template with `slugify` and `slugify` then `append`, and the text they write; three cards named Input, Arguments and Output.
- `how-2`: three cards: TemplateOptionsBuilder, TemplateOptions, Liquid templates.
- `delegate-1`: the `Startup` class with the `shout` delegate; the lines of `Configure<TemplateOptionsBuilder>` and `AddFilter` are bright.
- `delegate-2`: the same code; the line that returns the `StringValue` is bright.
- `delegate-3`: the template `{{ "hello" | shout }}` and its output `HELLO`.
- `class-1`: the class `WithSiteNameFilter`; its declaration, field, constructor and `ProcessAsync` are bright.
- `class-2`: the same class; the `await` line and the lines that read the argument are bright.
- `class-3`: the `ConfigureServices` method with `AddLiquidFilter`; the comment and the registration are bright.
- `class-4`: the template `with_site_name` with and without an argument, and the two lines it writes.
- `rules-1`: two small panels, `StringValue.Create(text)` and `StringValue.Create(html, encode: false)`, and two cards.
- `rules-2`: an excerpt of `LocalTimeZoneFilter.cs` that returns `NilValue.Instance`, and two cards.
- `rules-3`: three cards: configure the builder, not the options, and the message of the exception.
- `more-1`: four cards: global values, value converters, member access and tags.
- `outro`: the closing card.

## Spellings to confirm before narrating

The `spoken` map of `storyboard.json` makes the voice say the identifiers as words (`ILiquidFilter` as "I Liquid Filter",
`with_site_name` as "with site name"). A person confirms each spelling with `scripts/pronounce.py` before narrating, and
listens to "slugify" and "shout".

## Checks for private data

- The only names on screen are `admin`, the site name **Orchard Core** and the template name; the email
  `admin@example.com` is never shown.
- The password is never typed on camera or shown; `state.json` stays out of git.
- The Blog recipe content is fake. The fonts of The Blog Theme come from `fonts.googleapis.com`: they may or may not
  load; either way nothing private is sent.
- OCR the final video at one frame per second and at every scene change, and search for real names, email addresses
  and tokens, as in the skill.

## After the recording

Delete `src/OrchardCore.Modules/Videos.LiquidFilters/`, run `git checkout -- src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj`
in the checkout, and stop the site by its process ID. Keep the copy of `App_Data` outside git until the video is published.
