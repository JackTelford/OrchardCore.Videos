# Recording plan: liquid-basics

A developer video. Twenty-one of its steps are slides drawn by `slides.py`, two are cards, and four steps (`page-1` to
`page-4`, one clip) are a browser recording of a public page of the Blog theme. Only that clip needs a site.

In this plan a selector such as `a.navbar-brand` stands for `scene.Page.Locator("a.navbar-brand")`. File references are
to the Orchard Core source at commit `92db4a83`, the one this plan was written from.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, built and run on
  `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Auto Setup** (the host calls `AddSetupFeatures("OrchardCore.AutoSetup")`, `src/OrchardCore.Cms.Web/Program.cs:9`),
  configured with environment variables, so that no Setup screen is recorded:

  | Variable | Value |
  | --- | --- |
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>`, chosen outside git, the same value as `VIDEO_PASSWORD` |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **Recipe**: Blog (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`). It sets `TheBlogTheme` as the
  site theme (line 85), so the page is drawn by the Liquid templates of that theme that the slides show.
- **Features to enable before recording**: none. The recipe enables `OrchardCore.Templates` (line 74), whose manifest
  depends on `OrchardCore.Liquid` (`src/OrchardCore.Modules/OrchardCore.Templates/Manifest.cs:14`), which depends on
  Liquid Core Services. Before the take, open `/Admin/Features`, search for `Liquid`, and check that the Liquid feature
  is listed as enabled. This check needs a session, so do it in an ordinary browser, signed in as `admin` with
  `<password>`, outside the take. Do not change anything there.
- **Settings**: none to change.
- **External services**: none. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address in the shell that starts the
  site, so that the site never reaches out. The proxy variables apply to the site, not to the recorder's browser: the
  theme links two Google Fonts stylesheets in its `<head>` (`Views/Layout.liquid:12-13`), and the browser loads them
  when the machine is online and falls back to its own fonts when it is offline. Both are fine, and no site data is
  sent. Record every take of the clip in the same condition.
- **Sign-in**: the page is public, so the clip is recorded signed out. Do not run `Recorder setup` for this video, or
  move `state.json` aside for the take: the recorder then opens the page without a session
  (`Scene.StartAsync` uses the state file only when it exists).
- **Mask script**: none. The site is new and holds fake data only.

## The exact seed data

Nothing is created outside the capture: the Blog recipe creates everything the clip shows. Check these records once
on the site before recording (`/Admin/Contents/ContentItems`, or just open the pages in the clip):

| Record | Value | Where the recipe sets it |
| --- | --- | --- |
| Blog post title | `Man must explore, and this is exploration at its greatest` | `blog.recipe.json:1002` |
| Blog post subtitle | `Problems look mighty small from 150 miles up` | `blog.recipe.json:1022` |
| Blog post path | `blog/post-1` | `blog.recipe.json:1018` |
| Blog post author | `admin`, the user name given to Auto Setup | `blog.recipe.json:1006` |
| Site name | `Orchard Core` | the `SiteName` variable above |

The date after "on" in the Posted by line is the day the site was set up. It is not private data.

## Order of the clips, and what they change

There is one clip, `03-page`. It only reads public pages and changes nothing, so no snapshot is needed to record
it again. Record it after the site has started and answered its first request (the first request runs Auto Setup,
which takes some seconds): open `/` once in an ordinary browser and wait for the Blog home page before the take.

## Clip `03-page` (chapter 03, "On the page")

Starts on `/` (the Blog home page, `HomeRoute` in `blog.recipe.json:114`), opened with `scene.OpenAsync("/")` before
`StartCaptureAsync()`. The page shows the large header "Blog" and the summary of the blog post, a link whose text is
the title of the post.

Do not use the admin menu or the admin theme in this clip. The browser stays on the front end.

### `page-1`

- Actions: `ClickAndWaitForNavigationAsync(a:has(h2.post-title:has-text("Man must explore")))`, with the title of the
  post as text, so that the click never lands on another post (the Blog recipe creates one blog post). The page that
  opens is `/blog/post-1`. Wait until `.post-heading h1` is visible before the step ends (`WaitForAsync`, 5 seconds). If the
  link is missing, call `NavigateAsync("/blog/post-1")` instead.
- Visible when the narration ends: the blog post page from its top. The navigation bar shows the site name; the
  large header shows the title `Man must explore, and this is exploration at its greatest`, the subtitle and the
  Posted by line. No highlight yet.
- Source of the labels: `Content-BlogPost.Summary.liquid:2-3` (the link and `h2.post-title`),
  `Content-BlogPost.liquid:14-20` (the header), all in `src/OrchardCore.Themes/TheBlogTheme/Views`.

### `page-2`

- Actions: `HighlightAsync(a.navbar-brand, "Site.SiteName")`. Hold the highlight until the end of the step.
- Visible when the narration ends: a green ring around the site name `Orchard Core` at the left of the navigation
  bar, with the caption `Site.SiteName`.
- Source: `Views/Layout.liquid:27`, which writes `{{ Site.SiteName }}` inside a link whose address is `{{ '~/' | href }}`.

### `page-3`

- Actions: `ClearHighlightsAsync()`; `HighlightAsync(.post-heading h1)`; `PauseAsync(4000)`;
  `HighlightAsync(.post-heading h2.subheading)`. Pass no caption to either call: `HighlightAsync` draws a caption
  under its ring, and the theme leaves 0.75rem between the title and the subtitle and 2rem between the subtitle and
  the Posted by line (`src/OrchardCore.Themes/TheBlogTheme/wwwroot/css/styles.css:11171-11176`), so a caption would
  cover the next line of text. The narration names the two expressions.
- Visible when the narration ends: green rings, without captions, around the title `Man must explore, and this is
  exploration at its greatest` and around the subtitle `Problems look mighty small from 150 miles up`.
- Source: `Views/Content-BlogPost.liquid:15` (`Model.ContentItem.DisplayText`) and `:16`
  (`Model.ContentItem.Content.BlogPost.Subtitle.Text`).

### `page-4`

- Actions: `ClearHighlightsAsync()`. Then, only when the Posted by line is partly below the visible area, scroll:
  read `var box = await scene.Page.Locator(".post-heading span.meta").BoundingBoxAsync();` and, when
  `box.Y + box.Height > 780`, call `ScrollAsync(0, box.Y + box.Height - 700)`. Then
  `HighlightAsync(.post-heading span.meta, "t filter, Author")`.
- Visible when the narration ends: a green ring around the line `Posted by admin on <date>`, with the caption.
- Source: `Views/Content-BlogPost.liquid:17-20`: `{{ "Posted by" | t }}` and `{{ Model.ContentItem.Author }}`.

After `page-4`, call `ClearHighlightsAsync()` and let the recorder hold the last frame.

## The slides, one line each

All slides are drawn with the HTML syntax colors, since `brand.py` has no Liquid lexer. The Liquid code is taken from
the Blog theme (`src/OrchardCore.Themes/TheBlogTheme/Views`), the Blog recipe and the documentation of the Liquid module.

- `where-1`: four cards, "Where Liquid runs"; the first one (files in a theme or a module) is bright.
- `where-2`: the same cards; "Templates written in the admin" and "Content and settings" are bright.
- `where-3`: the same cards; "Two permissions" is bright (Manage Liquid templates, and Manage templates for the
  Templates module).
- `syntax-1`: an excerpt of `Content-BlogPost.liquid`, and three cards: the braces that print a value, the pipe that
  applies a filter, the tag delimiters that run a tag.
- `syntax-2`: the same excerpt with its first two lines bright (the title and the subtitle), and three cards about reading properties.
- `syntax-3`: the `raw` line of `Content-BlogPost.liquid`, and the `sanitize_html` example of the documentation.
- `objects-1`: four property reads of `Model.ContentItem` (`DisplayText`, `ContentType`, `CreatedUtc`, `Author`, in the
  order of the narration), and two cards: `Model` (the shape being displayed, the view model) and `ContentItem`.
- `objects-2`: the expression for the subtitle, and three cards that walk it: Content, BlogPost (the part named after
  the content type) and Subtitle with Text.
- `objects-3`: four cards, "Objects of Liquid Core Services": Culture, Request, Environment, HttpContext and TrackingConsent.
- `objects-4`: three cards, "Objects that features add": Site, Content and User (with a note that it needs the Liquid
  feature too), in the order of the narration.
- `filters-1`: the Autoroute pattern of the Blog recipe (`display_text`, `slugify`), and three cards that explain the chain.
- `filters-2`: four cards, "Where filters come from"; the standard filters and the Liquid module are bright.
- `filters-3`: three examples (`t`, `href`, `absolute_url`), and three cards about them.
- `filters-4`: three examples of `local` and `utc`, and two cards about them.
- `filters-5`: the "Where filters come from" cards; "Contents and Users" is bright.
- `filters-6`: the same cards; "Other features" is bright.
- `tags-1`: the image header of `Content-BlogPost.liquid` (`assign`, `if`, `else`), and three cards: assign, if and else, for.
- `tags-2`: three cards, "Tags that features add", each naming the feature that registers its tags (Liquid Core
  Services for `form`, `antiforgerytoken`, `helper` and `block`, and for `cache`; the Resources module for `script`,
  `style`, `link`, `meta` and `resources`); the first card is bright.
- `tags-3`: the same cards; the second (`cache`) and the third (the Resources tags) are bright.
- `parser-1`: three cards, "Liquid Core Services": always enabled, the parser and the core objects, the parser options.
- `parser-2`: the `appsettings.json` excerpt with the three parser options set to true, and three cards, one per
  option, saying what it does.

The cards `intro` and `outro` need no recording.

## Checks for private data

- Every frame of `03-page` shows only the Blog theme, the title, the subtitle and `admin`. No email address, no
  password and no sign-in form should appear, and no admin page either.
- Open the contact sheet of `03-page` and read the end of each step: the page must not show an error page, a missing
  image icon in place of the header picture, or a cookie banner.
- OCR the assembled video (one frame per second, and at every scene change): the only names are `admin`, `Orchard
  Core` and the Blog recipe's sample text. The slides hold no real name or address; their only host is `example.com`.
