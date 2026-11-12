# Recording plan: Media in Templates

A developer video. Most of it is slides (`slides.py`); two chapters are browser recordings: the Media Profiles screen
(`03-profile`) and a front-end page with five renditions of one image (`06-result`). Code, terminal commands and
configuration are slides, never recordings. In this plan a selector stands for `scene.Page.Locator(...)`.

Source references are to the Orchard Core checkout at commit `92db4a83` (`src/OrchardCore.Modules/...`), the one the
text was written from. Record against a build of the same commit, or a later one after reading the changes to the
files cited below.

## The site it needs

| What | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, run on `http://localhost:5010` (`VIDEO_BASE_URL`) |
| App data | `ORCHARD_APP_DATA` points at an empty folder outside both repositories |
| Setup | Auto Setup from environment variables (see the `orchardcore-tester` skill of that repository, `references/autosetup.md`), so no setup or sign-in form is recorded |
| Recipe | `Blog` (`RecipeName`). It enables Media, Templates (and so Liquid), Deployment, Autoroute, Contents and Flows (`TheBlogTheme/Recipes/blog.recipe.json`, step `feature`), so no feature has to be enabled |
| Tenant | `Default`; site name `Orchard Core`; time zone `America/Los_Angeles` |
| Admin | user `admin`, email `admin@example.com`, password `<password>`: the value of `VIDEO_PASSWORD`, chosen outside git, never written in a file of this repository |
| Theme | `TheBlogTheme`, the theme of the Blog recipe |
| Outbound calls | Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address for the site's process. The browser still loads the Google fonts that `Layout.liquid` of the theme links; without internet access the page uses fallback fonts, which is fine |

The environment variables of Auto Setup (names from the Auto Setup documentation):

```
OrchardCore__AutoSetup__Tenants__0__ShellName=Default
OrchardCore__AutoSetup__Tenants__0__SiteName=Orchard Core
OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
OrchardCore__AutoSetup__Tenants__0__AdminPassword=<password>
OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

No external service is involved: the Media module serves and resizes files from the site's own `Media` folder.

### What the Blog recipe already holds

- The media library holds `home-bg.jpg`, `about-bg.jpg` and `post-bg.jpg` in its root (step `media` of the recipe). The
  video uses `home-bg.jpg`, a 1900 by 1267 pixel picture of the theme (`TheBlogTheme/wwwroot/assets/img/home-bg.jpg`).
- Media Profiles holds `banner` (width 2048, height 600, Resize Mode Crop; step `MediaProfiles` of the recipe).
- The user's name appears only as `admin`. No real person, email address or token is on any screen of the video.

### The seed data (created outside the capture)

One JSON recipe creates everything else: the media profile `thumbnail`, the Liquid template
`Content__Slug__renditions`, and a page titled `Renditions` whose path is `renditions`. The template is picked for the
page by its path (`AutorouteAlternatesFactory.cs`, wired by `ContentAutorouteShapeTableProvider.cs`: a content item
with an Autoroute part gets the alternate `Content__Slug__<path>`).

1. Sign in once (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`).
2. Open **Tools > Deployments > JSON Import** (`/Admin/DeploymentPlan/Import/Json`,
   `OrchardCore.Deployment/AdminMenu.cs:18-30`). The page has one field, labeled `JSON`, and a button, `Import`
   (`Views/Import/Json.cshtml:15` and `:21`). The field is a CodeMirror editor: do not type the JSON, because the editor closes
   brackets and quotes. Set the value from the page, then click **Import**:

   ```js
   document.querySelector('.CodeMirror').CodeMirror.setValue(recipeText)
   ```

   where `recipeText` is the JSON below, as one string.
3. The page shows the notification `Recipe imported successfully!` (`ImportController.cs:185`). If it shows an error,
   the JSON is wrong or a feature is off; stop and read `OPEN-QUESTIONS.md`.

```json
{
  "steps": [
    {
      "name": "MediaProfiles",
      "MediaProfiles": {
        "thumbnail": {
          "Hint": "A square thumbnail",
          "Width": 160,
          "Height": 160,
          "Mode": 2,
          "Format": 0,
          "Quality": 100
        }
      }
    },
    {
      "name": "Templates",
      "Templates": {
        "Content__Slug__renditions": {
          "Description": "One image through resize_url with different arguments",
          "Content": "{% assign path = 'home-bg.jpg' %}\n<style>\n  .renditions { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 24px; padding: 110px 0 40px; }\n  .renditions figure { margin: 0; }\n  .renditions img { display: block; max-width: 100%; }\n  .renditions figcaption { margin-top: 8px; font-weight: 700; }\n  .renditions code { display: block; font-size: 12px; overflow-wrap: anywhere; }\n</style>\n<div class=\"renditions\">\n  <figure>\n    {% assign url = path | asset_url | resize_url: width: 200, height: 200 %}\n    <img src=\"{{ url }}\" alt=\"home-bg.jpg, max\" />\n    <figcaption>max</figcaption>\n    <code>{{ url | split: '&token=' | first }}</code>\n  </figure>\n  <figure>\n    {% assign url = path | asset_url | resize_url: width: 200, height: 200, mode: 'crop' %}\n    <img src=\"{{ url }}\" alt=\"home-bg.jpg, crop\" />\n    <figcaption>crop</figcaption>\n    <code>{{ url | split: '&token=' | first }}</code>\n  </figure>\n  <figure>\n    {% assign url = path | asset_url | resize_url: width: 200, height: 200, mode: 'pad', bgcolor: '404041' %}\n    <img src=\"{{ url }}\" alt=\"home-bg.jpg, pad\" />\n    <figcaption>pad</figcaption>\n    <code>{{ url | split: '&token=' | first }}</code>\n  </figure>\n  <figure>\n    {% assign url = path | asset_url | resize_url: width: 200, height: 200, mode: 'stretch' %}\n    <img src=\"{{ url }}\" alt=\"home-bg.jpg, stretch\" />\n    <figcaption>stretch</figcaption>\n    <code>{{ url | split: '&token=' | first }}</code>\n  </figure>\n  <figure>\n    {% assign url = path | asset_url | resize_url: profile: 'thumbnail' %}\n    <img src=\"{{ url }}\" alt=\"home-bg.jpg, profile: thumbnail\" />\n    <figcaption>profile: thumbnail</figcaption>\n    <code>{{ url | split: '&token=' | first }}</code>\n  </figure>\n</div>\n"
        }
      }
    },
    {
      "name": "content",
      "data": [
        {
          "ContentItemId": "4q8m2zk7tw5v9r3xhd6c1bnrdp",
          "ContentType": "Page",
          "DisplayText": "Renditions",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "TitlePart": {
            "Title": "Renditions"
          },
          "AutoroutePart": {
            "Path": "renditions",
            "SetHomepage": false
          }
        }
      ]
    }
  ]
}
```

The template, for reading (it is the `Content` value of the recipe, unescaped). It prints each URL without its
`token` parameter on purpose, so no token is on camera or in the check for private data:

```liquid
{% assign path = 'home-bg.jpg' %}
<style>
  .renditions { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 24px; padding: 110px 0 40px; }
  .renditions figure { margin: 0; }
  .renditions img { display: block; max-width: 100%; }
  .renditions figcaption { margin-top: 8px; font-weight: 700; }
  .renditions code { display: block; font-size: 12px; overflow-wrap: anywhere; }
</style>
<div class="renditions">
  <figure>
    {% assign url = path | asset_url | resize_url: width: 200, height: 200 %}
    <img src="{{ url }}" alt="home-bg.jpg, max" />
    <figcaption>max</figcaption>
    <code>{{ url | split: '&token=' | first }}</code>
  </figure>
  <figure>
    {% assign url = path | asset_url | resize_url: width: 200, height: 200, mode: 'crop' %}
    <img src="{{ url }}" alt="home-bg.jpg, crop" />
    <figcaption>crop</figcaption>
    <code>{{ url | split: '&token=' | first }}</code>
  </figure>
  <figure>
    {% assign url = path | asset_url | resize_url: width: 200, height: 200, mode: 'pad', bgcolor: '404041' %}
    <img src="{{ url }}" alt="home-bg.jpg, pad" />
    <figcaption>pad</figcaption>
    <code>{{ url | split: '&token=' | first }}</code>
  </figure>
  <figure>
    {% assign url = path | asset_url | resize_url: width: 200, height: 200, mode: 'stretch' %}
    <img src="{{ url }}" alt="home-bg.jpg, stretch" />
    <figcaption>stretch</figcaption>
    <code>{{ url | split: '&token=' | first }}</code>
  </figure>
  <figure>
    {% assign url = path | asset_url | resize_url: profile: 'thumbnail' %}
    <img src="{{ url }}" alt="home-bg.jpg, profile: thumbnail" />
    <figcaption>profile: thumbnail</figcaption>
    <code>{{ url | split: '&token=' | first }}</code>
  </figure>
</div>
```

Check the seed before any take:

- **Design > Templates** (`/Admin/Templates`) lists `Content__Slug__renditions`.
- **Media > Profiles** (`/Admin/MediaProfiles`) lists `banner` and `thumbnail`.
- `http://localhost:5010/renditions` shows a grid of five images. Reading left to right, then down:
  - `max`: 200 by 133 pixels, the whole picture;
  - `crop`: 200 by 200, the picture cut to a square;
  - `pad`: 200 by 200, the picture 200 by 133 between two dark gray bars (color `404041`) above and below;
  - `stretch`: 200 by 200, the whole picture squeezed into the square;
  - `profile: thumbnail`: 160 by 160, cropped.

  Under each image its URL is printed in small type, for example `/media/home-bg.jpg?width=200&height=200&rmode=crop`,
  with no `token`.

### Order of the clips, and what they change

Both clips only read: neither saves anything, so the site does not need a snapshot between takes, and the clips can be
recorded in any order. Both need the seed above. Take a snapshot of `App_Data` and `state.json` after the seed, and
restore it for a full take from scratch. The first request for a resized image writes a file to the resized image
cache (`<web root>/Default/media-cache`, `PhysicalFileSystemResizedImageCache.cs`), outside `App_Data`: this does not
change what the video shows.

Narrate first (`scripts/narrate.py videos/media-in-templates`), then record (`Recorder record 03-profile 06-result`).
The narration has spoken forms for identifiers (`spoken` in `storyboard.json`): a person confirms each one with
`scripts/pronounce.py` before the voice is generated (see `OPEN-QUESTIONS.md`).

## Clip `03-profile`: Media Profiles

Starts on `/Admin/MediaProfiles` (`MediaProfilesController.cs:58`, route `MediaProfiles`; a path relative to
`VIDEO_BASE_URL`), opened before the capture with `OpenAsync`. The list uses the List layout, which is the default
(`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs`, `DefaultLayout`) and which nothing in the Blog recipe
changes. Do not add a `layout` query: it is read only when the site setting that lets users choose a layout is on, and
it is off by default (`OrchardCore.Admin/Services/AdminListLayoutResolver.cs:31-37`,
`OrchardCore.Admin/Configuration/AdminListOptionsConfiguration.cs:40`). Both layouts show the hint under the name
(`Drivers/MediaProfileEntryDisplayDriver.cs:20-24` puts the hint in `Description` for the List layout and in the `Name`
column for the Grid layout), so the clip does not depend on the layout. The page title is `Media Profiles`
(`Views/MediaProfiles/Index.cshtml:6`) and the list holds two profiles, sorted by name (`MediaProfilesController.cs:80`):
`banner`, then `thumbnail`. Leave the admin menu as the recorder shows it.

- `profile-1`: Narration: "A media profile stores resize commands under a name...". Actions: `HighlightAsync` the link
  of `banner` (`a[href*='Edit?name=banner']:not(.btn)`) with the caption `banner`, pause 1,500 ms, then `HighlightAsync`
  the link of `thumbnail` (`a[href*='Edit?name=thumbnail']:not(.btn)`) with the caption `thumbnail`. On screen when the
  narration ends: the list with both names ringed, and under each name its hint: `A banner image (2048px x 600px,
  cropped)` and `A square thumbnail`. Labels: menu `Media` and `Profiles` (`AdminMenu.cs:18` and `:26`), the name link
  (`Views/MediaProfileEntry.Fields.SummaryAdmin.cshtml:7`), the hint
  (`Views/MediaProfileEntry.Description.SummaryAdmin.cshtml:8`).
- `profile-2`: Narration: "Let's open thumbnail...". Actions: `ClearHighlightsAsync`, then
  `ClickAndWaitForNavigationAsync` on the link of `thumbnail` (`a[href*='Edit?name=thumbnail']:not(.btn)`), then
  `HighlightAsync` `#SelectedWidth` with the caption `160`, `#SelectedHeight` with the caption `160`, and `#SelectedMode`
  with the caption `Crop`, about 700 ms apart. On screen when the narration ends: the page `Edit Media Profile`
  (`Views/MediaProfiles/Edit.cshtml:4`) with the fields `Name` (`thumbnail`), `Hint`, `Width` (`160`), `Height` (`160`),
  `Resize Mode` (`Crop`), and the three rings. The select options come from `MediaProfilesController.cs:352-370`
  (`Default`, the supported sizes, `Custom Size`; `Default (Max)`, `Max`, `Crop`, ...). Labels: `Edit.cshtml:13`
  (`Name`), `:22` (`Hint`), `:31` (`Width`), `:42` (`Height`), `:53` (`Resize Mode`), `:71` (`Format`), `:80`
  (`Quality`), `:92` (`Auto Orient`).
- `profile-3`: Narration: "A template refers to a profile by its name alone...". Actions: `ClearHighlightsAsync`, then
  `HighlightAsync` `#Name` with the caption `thumbnail`. Do not type in any field and do not click Save: the clip ends
  on the Edit page. On screen when the narration ends: the `Name` field with a ring and the caption.

## Slides

- `path-1`: slide "From a path to an image": four cards; the first two are bright (a path in the library, an asset URL).
- `path-2`: the same slide with the last two cards bright (a resized URL, a cached file).
- `path-3`: slide "Signed commands and supported sizes": the `Media` configuration section with `SupportedSizes` and
  `UseTokenizedQueryString`, and three cards (valid token, no token, invalid token).
- `liquid-1`: slide "asset_url and img_tag": a Liquid panel with the output of each filter as a comment.
- `liquid-2`: slide "resize_url": two `resize_url` calls, with a width and a height, and with a width only.
- `liquid-3`: slide "Resize modes": four cards (max, crop, pad, stretch); the last three are bright.
- `liquid-4`: slide "Background color, quality and format": a `pad` call with `bgcolor`, and a call with `format` and `quality`.
- `liquid-5`: slide "Media text and anchors": a loop over the `Paths` of a media field with `MediaTexts` and `Anchors`.
- `profile-4`: slide "The profile argument": `resize_url` with `profile: 'thumbnail'`, and with `mode: 'max'` added.
- `razor-1`: slide "The Orchard helper": the loop of the display template `MediaField.cshtml` of the Media module.
- `razor-2`: slide "AssetUrl and its options": four calls of `Orchard.AssetUrl` with named parameters.
- `razor-3`: slide "Media profiles in Razor": two calls of `Orchard.AssetProfileUrlAsync`.
- `tag-1`: slide "asset-src and asset-href": the `@addTagHelper` line, then an `img` and an `a` element.
- `tag-2`: slide "Resize attributes": three `img` elements with the `img-` attributes.
- `tag-3`: slide "Profiles, anchors and versions": four `img` elements (profile, profile with override, anchor, version).
- `end-1`: slide "Versioned URLs": the three ways to append a version (Liquid, Razor, tag helper).
- `end-2`: slide "A custom image engine": a class that implements `IImageProcessingEngine`, and its registration.

## Clip `06-result`: the result in the browser

Starts on `/renditions` (a path relative to `VIDEO_BASE_URL`, so `http://localhost:5010/renditions`), opened before the
capture with `OpenAsync`, not scrolled. Before
`StartCaptureAsync`, wait until the five images are loaded (the first request of each one resizes it, which takes a
moment):

```csharp
await scene.Page.WaitForFunctionAsync(
    "() => { const i = [...document.querySelectorAll('.renditions img')]; return i.length === 5 && i.every(x => x.complete && x.naturalWidth > 0); }");
```

In the selectors below, `N` is the position of the figure: 1 `max`, 2 `crop`, 3 `pad`, 4 `stretch`, 5 `profile: thumbnail`.

- `result-1`: Narration: "This page shows one image five times...". Actions: `HighlightAsync` `.renditions` with the
  caption `home-bg.jpg`, pause 2,500 ms, `ClearHighlightsAsync`, then `HighlightAsync`
  `.renditions figure:nth-child(1) code`. On screen when the narration ends: the five figures, and a ring around the
  small URL under the first image (`/media/home-bg.jpg?width=200&height=200`).
- `result-2`: Narration: "Max keeps the proportions...". Actions: `ClearHighlightsAsync`; then for N = 1 to 4,
  `HighlightAsync` `.renditions figure:nth-child(N) img` with the caption `max`, `crop`, `pad`, `stretch`, hold 1,800 ms,
  `ClearHighlightsAsync`; keep the ring of the fourth image at the end (skip the last `ClearHighlightsAsync`). On screen
  when the narration ends: the `stretch` image ringed with the caption `stretch`.
- `result-3`: Narration: "The last image uses the thumbnail profile...". Actions: `ClearHighlightsAsync`;
  `HighlightAsync` `.renditions figure:nth-child(5) img` with the caption `thumbnail`, hold 2,500 ms,
  `ClearHighlightsAsync`; then `HighlightAsync` `.renditions figure:nth-child(1) code` with the caption `200`. On screen
  when the narration ends: a ring around the URL of the first image with the caption `200`; the five figures visible.
  Labels: the captions come from the template above; no Orchard Core label is read in this clip.

## Checks for private data

- Every user, email and name on screen is fake: the user is `admin` (`admin@example.com` is never displayed), and the
  site name is `Orchard Core`. No password is typed in any clip: the session comes from `state.json`, which is git-ignored.
- No token is on any screen: the template prints each URL up to `&token=`. The slides write `token=...`.
- OCR the video at one frame per second and at every scene change, as the skill asks, and search for `token=`: it must
  not appear.
- The only picture on camera is `home-bg.jpg`, a stock picture shipped with the theme. Look at it once before the first take: no identifiable person may be in it.
- `state.json`, the `App_Data` copy and the environment values (`VIDEO_PASSWORD`) stay outside git.
