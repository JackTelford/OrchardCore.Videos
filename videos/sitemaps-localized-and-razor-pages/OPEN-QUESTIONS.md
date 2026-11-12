# Open questions: Sitemaps for Localized Content and Razor Pages

Things that were read in the source but not seen on a running site. The narration says nothing that depends on
question 3. Questions 1, 2, 4 and 5 must be answered before the clips are recorded. Paths are relative to the root
of `OrchardCMS/OrchardCore` at commit `92db4a83`.

## 1. The area of a Razor Page of the web project (steps `raz-2`, `raz-5`; slide `raz-2`)

**What the video says.** The route values "must include the area of the page, which for a page of your web project is
the name of the project". The slide uses `area = "OrchardSite"` with the comment "The name of the web project", and the
recording uses a host project named `OrchardSite`.

**What was checked.**
- `src/OrchardCore/OrchardCore.Sitemaps.Abstractions/SitemapsRazorPagesContentTypeOption.cs:24`: the route values "must
  include the area, or namespace of module, the razor page belongs to".
- `src/OrchardCore/OrchardCore.Mvc.Core/RazorPages/ModularPageRazorPagesOptionsSetup.cs:22-34`: only pages under an area
  are served, and the pages of the application are served as the area named `_applicationContext.Application.Name`.
- `src/OrchardCore/OrchardCore.Abstractions/Modules/Application.cs:25`: that name is `environment.ApplicationName`, the
  name of the web project.
- `src/OrchardCore/OrchardCore.Mvc.Core/ApplicationViewFileProvider.cs:50-75`: `Areas/{ApplicationName}/Pages/...` is read
  from the `Pages` folder of the web project.
- `src/docs/reference/modules/Sitemaps/README.md` ("Decoupled Razor Pages") uses `area = "OrchardCore.Sitemaps"`, which is
  the area of the Sitemaps module itself.

**What a person must confirm** on a running site (the checks in `recording-plan.md`): with the host project
`OrchardSite` and `area = "OrchardSite"`, the sitemap lists `https://www.example.com/blogpost/<content item id>` for
each post, and `/blogpost/<content item id>` opens the page. If the address is only `https://www.example.com`, the area is
not the name of the project: find the value that works, then change the narration of `raz-2`, the comment on the slide
(`slides.py`, `RAZOR_OPTIONS` and the `raz-2` branch), and the plan, and report the documentation issue with the right value.

## 2. Pronunciation of the `spoken` entries of `storyboard.json`

The voice must read these as the second column. A person confirms each one with
`.agents/skills/orchardcore-video-producer/scripts/pronounce.py` before narrating (new spellings must be confirmed):

| Written | Spoken |
|---|---|
| `OrchardCore.ContentLocalization.Sitemaps` | Orchard Core dot Content Localization dot Sitemaps |
| `IRouteableContentTypeProvider` | I Routeable Content Type Provider |
| `IContentItemsQueryProvider` | I Content Items Query Provider |
| `SitemapsRazorPagesOptions` | Sitemaps Razor Pages Options |
| `ConfigureContentType` | Configure Content Type |
| `RouteValues` | Route Values |
| `PageName` | Page Name |
| `Program.cs` | Program dot C.S. |
| `hreflang` | H ref lang |
| `xhtml` | X.H.T.M.L. |
| `href` | H ref |

Also listen to "routeable", "Autoroute", "decoupled" and "URL" in context. The entries are applied in the order of the
file, so `hreflang` is replaced before `href`.

## 3. Limit Items with a localized content type (not in the narration)

In `src/OrchardCore.Modules/OrchardCore.ContentLocalization/Sitemaps/LocalizedContentItemsQueryProvider.cs:52-80`, when
the Sitemap Content Types source uses "Limit Items" and the type has a `LocalizationPart`, the query keeps only the items
of the default culture (the comment says "Content item is valid if it is for the default culture"), and
`ReferenceContentItems` (line 116) is that same list. The metadata provider looks for the other localizations among
`ReferenceContentItems`, so the alternate links of such a sitemap seem to hold only the default culture. The comment on
line 68 says that all items are fetched "for reference", which the code does not do. Also, the source builder
(`src/OrchardCore.Modules/OrchardCore.Contents/Sitemaps/ContentTypesSitemapSourceBuilder.cs`, batch of 500) queries items in
batches, and the references are the items of one batch. A person with a site that has this data can confirm both
behaviors. The video says only that the provider looks "among the queried items".

## 4. The seed recipes and the browser tree were not run

The two recipes of `recording-plan.md` were written from `src/docs/reference/modules/Recipes/README.md`, the Blog recipe
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`), `Settings` step
(`src/OrchardCore.Modules/OrchardCore.Settings/Recipes/SettingsStep.cs`) and the `Content` step. These points were
checked in the source: the step names are matched without regard to case (`NamedRecipeStepHandler.cs`); the culture of
a `LocalizationPart` is kept as written when it is not empty (`OrchardCore.ContentLocalization/Handlers/LocalizationPartHandler.cs`,
`CreatingAsync`), so `hreflang` is `en` and `fr`; an Autoroute path that is not empty is kept
(`OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs`, `GenerateContainerPathFromPatternAsync`); an imported content
item keeps its `ContentItemId`, and `ModifiedUtc` is set, so each `url` has a `lastmod`. To confirm when seeding: the
import reports "Recipe imported successfully!" twice (a failed import shows no message on the JSON Import page, because
the controller redirects after adding the error); the sitemap lists the four items. The text locators of the clips
(`hreflang`, `xmlns:xhtml`, `/blogpost/`) depend on how Chromium draws an XML document; the plan says how to look at
the page with `Recorder probe` and adapt.

## 5. Building the throwaway host project

**What the video relies on.** `recording-plan.md` has a host project `src/OrchardSite` that is built next to
`src/OrchardCore.Cms.Web`, outside the solution.

**What was checked.** The project paths of its `.csproj` exist in the source (`OrchardCore.Application.Cms.Core.Targets`
props and targets, `OrchardCore.Application.Cms.Targets.csproj`, `OrchardCore.Application.Targets.targets`,
`OrchardCore.Build/OrchardCore.Commons.targets`), and `OrchardCore.Application.Cms.Core.Targets.csproj` references the
Sitemaps and Content Localization modules. `OrchardCore.Commons.props` sets `WarningsNotAsErrors` but no
`TreatWarningsAsErrors`; the repository-root `Directory.Build.props` was not in the copy that was read.

**What a person must confirm.** That the project builds and starts, and that `/blogpost/<content item id>` opens the
page (question 1). If a build warning is an error, fix it in the host project only.
