# Recording plan: Dynamic Cache

This is a developer video. Most of it is slides, drawn by `slides.py` (nothing to record). Two chapters are browser
recordings: `02-mode` (the feature and the Cache Mode setting) and `04-watch` (a cached block on a page). Everything a
person records is in this file; the sources of every label are `file:line` references into the Orchard Core checkout
that the site runs from.

In this file a selector such as `#search-box` stands for `scene.Page.Locator("#search-box")`.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, started in the **Development**
  environment: set `ASPNETCORE_ENVIRONMENT=Development` and `ASPNETCORE_URLS=http://localhost:5010` yourself and run
  `dotnet run --no-launch-profile`, so that the two variables decide and not the launch profile of the project
  (`src/OrchardCore.Cms.Web/Properties/launchSettings.json`, ports 5000 and 5001); a site started with neither variable
  runs in Production. The video depends on the environment: the default Cache Mode is "From environment", which caches in Production only
  (`OrchardCore.Modules/OrchardCore.DynamicCache/CacheOptionsConfiguration.cs:38-40`), so a Development site caches
  nothing until `02-mode` changes the setting. On a site started in Production the block would be cached before
  `02-mode`, and that clip would change nothing.
- **Port and data**: `http://localhost:5010` (`VIDEO_BASE_URL`), with its own `App_Data` folder outside both
  repositories (`ORCHARD_APP_DATA`).
- **Provisioning**: Auto Setup through environment variables, so no setup form is recorded. Recipe **Blog**, site name
  **Orchard Core**, time zone `America/Los_Angeles`, tenant `Default`, user `admin`, email `admin@example.com`, password
  chosen outside git and passed in `VIDEO_PASSWORD` (written `<password>` everywhere in the repository). This video is
  not about the Setup screen, so Auto Setup and a saved sign-in (`state.json`, git-ignored) apply.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address. The Blog theme links Google Fonts in its
  layout (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`); with the proxy those requests fail and the page uses
  fallback fonts, which is fine.
- **Features**: the Blog recipe already enables everything the video uses: Dynamic Cache (`OrchardCore.DynamicCache`),
  Templates, Deployment, Alias, Autoroute, Layers (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the
  `feature` step). Do not enable or disable anything else.
- **Settings**: none changed before recording. Cache Mode stays at its default, "From environment".
- **External services**: none.
- **Sign-in**: run `Recorder setup` once (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD`), which saves `state.json`.

### Seed data (create it outside the capture)

The About page of the Blog recipe is an Article at `/about`. The video needs one Liquid template that overrides how
Articles are displayed, so the page shows two lines: the time of the request, and a time inside a cache block.

1. Sign in as `admin`, open **Tools**, **Deployments**, **JSON Import** (`/Admin/DeploymentPlan/Import/Json`, `OrchardCore.Modules/OrchardCore.Deployment/Controllers/ImportController.cs:20`).
2. Paste the JSON below into the **JSON** box and click **Import** (`OrchardCore.Modules/OrchardCore.Deployment/Views/Import/Json.cshtml:15,21`;
   the box is a CodeMirror editor, so paste with the keyboard). It creates the template `Content__Article`, which is
   the Blog theme's `Content-Article.liquid` (`OrchardCore.Themes/TheBlogTheme/Views/Content-Article.liquid`) with an
   `assign` of `dependency`, the line **Rendered at** and a cache block that holds the line **Cached at** added after
   its `{% endzone %}`. The step name `Templates` and the property names `Content` and
   `Description` are those of `OrchardCore.Modules/OrchardCore.Templates/Recipes/TemplateStep.cs`. The name
   `Content__Article` is the alternate for the content type Article (`src/docs/reference/modules/Templates/README.md`,
   section `Content__[ContentType]`), and a template of the Templates module is looked up before the shape files of the
   theme for the same alternate (`OrchardCore/OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs`,
   `GetShapeBindingAsync`, which asks the shape binding resolvers first).
3. Open `http://localhost:5010/about`. It must show a heading "About" and, above the text, the lines **Rendered at**
   and **Cached at** with a time each. While Cache Mode is "From environment" both times are the same and change on every
   reload, because nothing is cached in Development.

```json
{
  "steps": [
    {
      "name": "Templates",
      "Templates": {
        "Content__Article": {
          "Content": "{% zone \"Header\" %}\n    <!-- Page Header -->\n    <!-- Set your background image for this header on the line below. -->\n    {% assign imagePath = Model.ContentItem.Content.Article.Image.Paths.first %}\n    {% if imagePath == nil %}\n        <header class=\"masthead\" style=\"background-image: url('{{ \"~/TheBlogTheme/assets/img/about-bg.jpg\" | href }}')\">\n    {% else %}\n        {% assign anchor = Model.ContentItem.Content.Article.Image.Anchors.first %}\n        <header class=\"masthead\" style=\"background-image: url('{{ imagePath | asset_url | resize_url: profile: 'banner', anchor: anchor }}')\">\n    {% endif %}\n        <div class=\"container position-relative px-4 px-lg-5\">\n            <div class=\"row gx-4 gx-lg-5 justify-content-center\">\n                <div class=\"col-md-10 col-lg-8 col-xl-7\">\n                    <div class=\"page-heading\">\n                        <h1>{{ Model.ContentItem.DisplayText }}</h1>\n                        <span class=\"subheading\">{{ Model.ContentItem.Content.Article.Subtitle.Text }}</span>\n                    </div>\n                </div>\n            </div>\n        </div>\n    </header>\n{% endzone %}\n\n{% assign dependency = \"contentitemid:\" | append: Model.ContentItem.ContentItemId %}\n<p class=\"text-center\"><strong>Rendered at</strong> {{ \"now\" | date: \"%T\" }}</p>\n{% cache \"about-clock\", dependencies: dependency, expires_after: \"00:05:00\" %}\n<p class=\"text-center\"><strong>Cached at</strong> {{ \"now\" | date: \"%T\" }}</p>\n{% endcache %}\n\n{{ Model.Content.ContentsMetadata.Value }}\n{{ Model.Content.ContentsMetadata | shape_render }}\n\n{{ Model.Content.HtmlBodyPart | shape_render }}\n",
          "Description": "Shows the time of the request and a cached time."
        }
      }
    }
  ]
}
```

The template's Liquid, for reference (the JSON above holds exactly this text):

```liquid
{% zone "Header" %}
    <!-- Page Header -->
    <!-- Set your background image for this header on the line below. -->
    {% assign imagePath = Model.ContentItem.Content.Article.Image.Paths.first %}
    {% if imagePath == nil %}
        <header class="masthead" style="background-image: url('{{ "~/TheBlogTheme/assets/img/about-bg.jpg" | href }}')">
    {% else %}
        {% assign anchor = Model.ContentItem.Content.Article.Image.Anchors.first %}
        <header class="masthead" style="background-image: url('{{ imagePath | asset_url | resize_url: profile: 'banner', anchor: anchor }}')">
    {% endif %}
        <div class="container position-relative px-4 px-lg-5">
            <div class="row gx-4 gx-lg-5 justify-content-center">
                <div class="col-md-10 col-lg-8 col-xl-7">
                    <div class="page-heading">
                        <h1>{{ Model.ContentItem.DisplayText }}</h1>
                        <span class="subheading">{{ Model.ContentItem.Content.Article.Subtitle.Text }}</span>
                    </div>
                </div>
            </div>
        </div>
    </header>
{% endzone %}

{% assign dependency = "contentitemid:" | append: Model.ContentItem.ContentItemId %}
<p class="text-center"><strong>Rendered at</strong> {{ "now" | date: "%T" }}</p>
{% cache "about-clock", dependencies: dependency, expires_after: "00:05:00" %}
<p class="text-center"><strong>Cached at</strong> {{ "now" | date: "%T" }}</p>
{% endcache %}

{{ Model.Content.ContentsMetadata.Value }}
{{ Model.Content.ContentsMetadata | shape_render }}

{{ Model.Content.HtmlBodyPart | shape_render }}
```

The cache block is the part the video is about: its id is `about-clock`, it depends on the content item
(`contentitemid:` followed by the id of the About page) and it expires five minutes after it was stored.

### Snapshots

Stop the site and copy its `App_Data` folder and `state.json` aside at these moments:

- **Snapshot A**: after provisioning, the template import and the sign-in. Cache Mode is "From environment".
- **Snapshot B**: after `02-mode` has been recorded. Cache Mode is "Enabled".

Restore the right snapshot, then start the site again, before every take.

## Order of the clips

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-mode` | Snapshot A | Yes: saves Cache Mode "Enabled" (Snapshot B is taken after it) |
| 2 | `04-watch` | Snapshot B | Yes: publishes the About page again (a new version), and refreshes the cache |

`04-watch` needs Cache Mode "Enabled", so it can only be recorded after `02-mode`, or from Snapshot B. To record
`02-mode` again, restore Snapshot A. To record `04-watch` again, restore Snapshot B. The slide clips (`01-why`,
`03-block`, `05-shape`, `06-invalidate`, `07-extend`) need no site.

## Clip `02-mode` (chapter 02, Turn caching on)

**Starts on** `/Admin/Features`, signed in, opened before the capture, nothing typed in the search box. Start the
capture after the page has finished loading (the list of features is shown, grouped by category).

- `mode-1`
  - Click the search box (`#search-box`, placeholder "Search") and type `Dynamic Cache` (`TypeAsync`). The filter runs in
    the browser (`features-index.js`): wait until the row **Dynamic Cache** is the only feature listed under its
    category heading.
  - Highlight the category heading **Performance** (the `h3` of the visible `.feature-group`) and the row **Dynamic
    Cache** with its **Enabled** badge. Do not click **Disable**.
  - **On screen when the narration ends**: the heading "Performance" and the row "Dynamic Cache" with the "Enabled"
    badge, and no other feature.
  - Labels: the search box `OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27` and the category
    heading `:88`; the badge `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`; the name and the category
    `OrchardCore.Modules/OrchardCore.DynamicCache/Manifest.cs:4,9`; the menu path **Tools**, **Features**
    `OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`.
- `mode-2`
  - `ClearHighlightsAsync()`, then `NavigateAsync("/Admin/Settings/general")` (the page title is **Settings**).
  - The cache setting is on a tab of this page. Click the tab named **Cache**
    (`GetByRole(AriaRole.Tab, new() { Name = "Cache", Exact = true })`: the tab is an `a` element with `role="tab"`, so
    it is a tab for the browser's accessibility tree and not a link; its text is the placement key `Cache`,
    `OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:56,59`, and the first tab is the active one on
    every load, `:47-50`); its heading is **Settings for dynamic caches** and its field is **Cache Mode**.
  - The drop-down list draws no open list in a headless browser, so show its four options by turning it into a list
    box (the options are in the page already; this changes only how they are drawn):
    `scene.Page.EvaluateAsync("() => { const label = [...document.querySelectorAll('label')].find(l => l.textContent.trim() === 'Cache Mode'); document.getElementById(label.htmlFor).size = 4; }")`.
  - Highlight the field (`GetByLabel("Cache Mode")`).
  - **On screen when the narration ends**: "Settings for dynamic caches", the label "Cache Mode" and the four options in
    this order: "From environment — enabled in Production, disabled otherwise", "Enabled", "Enabled with cache debug
    mode", "Disabled", with "From environment — enabled in Production, disabled otherwise" selected.
  - Labels: `OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Cache.Edit.cshtml:5,10,13-16`; the page title
    `Views/Admin/Index.cshtml:4`; the route `Controllers/AdminController.cs:50`; the tab comes from the placement
    `Content:1#Cache;30` in `Drivers/DefaultSiteSettingsDisplayDriver.cs:57`, and the tab strip from
    `OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:14-65` (see `OPEN-QUESTIONS.md`).
- `mode-3`
  - `ClearHighlightsAsync()`, then `SelectAsync(GetByLabel("Cache Mode"), "Enabled")` (the option value is `Enabled`).
  - Click **Save** (`button.save`, `ClickAndWaitForNavigationAsync`). Saving asks for a reload of the tenant
    (`Drivers/DefaultSiteSettingsDisplayDriver.cs:140`), so the response can take a few seconds: wait until the page title
    **Settings** is shown again.
  - The page is drawn again with its first tab active. Click the tab **Cache** again (the same `AriaRole.Tab` locator
    as in `mode-2`), run the same `EvaluateAsync` as in `mode-2`, and highlight the field.
  - **On screen when the narration ends**: "Cache Mode" with the option "Enabled" selected, after the save.
  - Labels: `Views/SiteSettings.SaveButton.cshtml:2` (**Save**), `Views/Settings-Cache.Edit.cshtml`.

## Clip `04-watch` (chapter 04, See it work)

**Needs** Snapshot B (Cache Mode "Enabled", template imported). **Starts on** `http://localhost:5010/about`, signed in,
opened **less than a minute before** the capture starts (this load stores the cache entry; it expires five minutes
after it was stored, so `watch-1` and `watch-2` must be recorded within five minutes of it). If the lines **Rendered at**
and **Cached at** are below the visible part of the page, scroll down (`ScrollAsync`) before the capture until both
are visible. Note the cached time: it must be the same on the next two loads.

- `watch-1`
  - `StartCaptureAsync()`. Highlight the paragraph **Rendered at** and the paragraph **Cached at**
    (`p:has-text("Rendered at")`, `p:has-text("Cached at")`).
  - **On screen when the narration ends**: the About page with the two highlighted lines, each with a time (the page is the
    one opened before the capture, not a new request, so the two times are the same, or one second apart when the
    request crossed a second boundary).
  - Labels: the two lines are text of the template in the seed data above.
- `watch-2`
  - `ClearHighlightsAsync()`, then `NavigateAsync("/about")`, scroll as needed so both lines are visible, highlight them
    again.
  - **On screen when the narration ends**: **Rendered at** shows a later time than in `watch-1`; **Cached at** shows
    exactly the same time as in `watch-1`.
- `watch-3`
  - `ClearHighlightsAsync()`, then `NavigateAsync("/Admin/Contents/ContentItems")` (the list is titled **Content Items**
    in the admin menu, under **Content**).
  - In the row titled **About** (type Article), click **Edit** (`ClickAndWaitForNavigationAsync`; the button is
    `a.btn.edit` in the row, shown as a button because the default actions layout of an admin list is `Buttons`,
    `OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:36`; if the row shows an ellipsis button instead,
    the site has the `Menu` layout and the button is in that menu). The editor of the About page opens with a
    `returnUrl` in its address, because the link of **Edit** carries one.
  - Click **Publish** (`button.publish`, the main button of the split button that the editor shows when it has a
    `returnUrl`; `button.dropdown-item` is **and continue** and is not clicked; `ClickAndWaitForNavigationAsync`). The list
    is shown again, since the `returnUrl` is the list.
  - **On screen when the narration ends**: the content list after the publish (the **About** row still listed), or the
    editor with the **Publish** button just clicked. Wait for the page to finish loading.
  - Labels: **Edit** `OrchardCore.Modules/OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:13`;
    **Publish** `Views/Content.PublishButton.cshtml:19` (`:7` is the same button when there is no `returnUrl`); the menu path `AdminMenu.cs:65,68`; the route
    `Controllers/AdminController.cs:67`.
- `watch-4`
  - `NavigateAsync("/about")`, scroll as needed so both lines are visible, highlight them.
  - **On screen when the narration ends**: **Rendered at** and **Cached at** show new times, both later than the
    **Cached at** time of `watch-1` and `watch-2`, and equal or a second apart (the block was rendered again).

Every action of a step is shorter than its narration except the navigation to the editor and the publish in `watch-3`:
if they run longer, the frame at the end of the narration shows the list or the editor, which is acceptable; the next
step starts when the page has loaded.

## Slide steps (no recording)

Drawn by `slides.py`; listed so a person can check each against its narration.

- `intro`, `outro`: the opening and closing cards (title "Dynamic Cache").
- `why-1`: three cards: "A page cache", "Dynamic Cache" and "A cache policy per section".
- `why-2`: the layout, with Section A (A1 varies by role) and Section B (B1 varies by query string).
- `why-3`: what happens on the first request, on later requests, and where entries are stored.
- `block-1`: the Liquid cache block around a query loop; the `cache` and `endcache` lines are bright.
- `block-2`: the four arguments of the block: `vary_by`, `dependencies`, `expires_after`, `expires_sliding`.
- `block-3`: a block that varies by a content item id, and two notes: any other value is used as it is, and the default
  expiration.
- `block-4`: the comments cache debug mode writes in the page source, for a Liquid block and for a shape.
- `shape-1`: the main menu of The Blog Theme in Liquid and in Razor, with their cache attributes.
- `shape-2`: the `dynamic-cache` tag helper in Razor.
- `shape-3`: the `Cache` call of `NotificationNavbarDisplayDriver`, with its callback: the user context, the three tags
  and the absolute expiration.
- `inval-1`: the handler of the Contents module that removes a tag, and the three tags Orchard Core removes.
- `inval-2`: the `ITagCache` interface and a call to `RemoveTagAsync`.
- `inval-3`: nested cache blocks, and the dependencies and expiration the outer block takes on when the inner blocks render.
- `inval-4`: `cache_dependency` inside a loop in Liquid, and the `cache-dependency` tag in Razor.
- `inval-5`: the separate resources shape of the Notifications module.
- `ext-1`: `RouteCacheContextProvider` and the line that registers it (in `OrchardCoreBuilderExtensions.cs`, where the
  built-in providers are registered).
- `ext-2`: the `FailoverRetryLatency` setting, and three notes about the storage.

## Checks for private data

- Only `admin`, `admin@example.com` and the fake site "Orchard Core" appear. The recording is the browser viewport: no
  address bar, no operating system notification, no real name.
- The password is never typed on camera (the sign-in comes from `state.json`).
- Look at the contact sheets (`review.py`) of both clips: no error message (for example a Liquid error in place of the
  two lines), no "Rendered at" line without its "Cached at" line, and no real name in the content list.
- OCR the finished video once per second and at every scene change, and search the text for names, emails other than
  `admin@example.com`, phone numbers and tokens.
