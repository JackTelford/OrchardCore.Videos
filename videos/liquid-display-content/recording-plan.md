# Recording plan: Liquid: Display Content Items, Shapes and Zones

How to record this video without deciding anything. In this file a selector such as `header.masthead` stands for
`scene.Page.Locator("header.masthead")`, since the recorder's methods take locators. Source references are
`path:line` in `OrchardCMS/OrchardCore` at the commit of `catalog/features.json`, from the `src` folder.

The video has 24 narrated steps in 8 chapters. **Only one clip is a recording**: `02-page`, a tour of the front end of
a fresh Blog site. Every other chapter is a slide chapter (drawn by `slides.py`) or a card. The recording only reads
the site: it creates, saves and deletes nothing.

## 1. The site it needs

Record the front end of a fresh site made with the Blog recipe. The site needs no seed data of its own: the recipe
creates every record the clip shows.

| Setting | Value |
|---|---|
| Orchard Core | `src/OrchardCore.Cms.Web`, run on `http://localhost:5010` (`VIDEO_BASE_URL`), `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` (variable names: `docs/reference/modules/AutoSetup/README.md`) |
| Tenant | `Default`, no URL prefix |
| Password | `<password>` is chosen outside git and passed in `VIDEO_PASSWORD`. Never write it in a file of the repository |
| Theme | The Blog Theme (`TheBlogTheme`), set as the site theme by the Blog recipe (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:83`) |
| Features to enable | None. The Blog recipe enables Templates, which enables Liquid (`OrchardCore.Templates/Manifest.cs`: `Dependencies = ["OrchardCore.Liquid"]`), and also Alias, Autoroute, Contents, Layers, Widgets and Dynamic Cache |
| Settings to save | None |
| External services | None. Start the site with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls out. The layout of the theme links Google Fonts (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`) from the browser: record with network access, so the fonts load the same way in every take |
| Time zone | `America/Los_Angeles`. It decides the date shown in "Posted by admin on ..." |

The records that the clip shows, all created by the Blog recipe (`blog.recipe.json`, `content` step):

| Record | Type | Path | Shown in the clip |
|---|---|---|---|
| Blog | Blog | `/blog`, also the home page (`blog.recipe.json:114`) | Title "Blog", description "This is the description of your blog" (`:976`) |
| Man must explore, and this is exploration at its greatest | Blog Post | `/blog/post-1` (`:1018`) | Title, subtitle "Problems look mighty small from 150 miles up" (`:1022`), Markdown body |
| Footer | Raw Html widget | layer `Always`, zone `Footer` (`:1097-1098`) | The social icons and the copyright line |

Add no other record. The list of the Blog then holds one post, which is what the narration of `page-3` describes.

Sign in once, outside the capture:

```bash
export VIDEO_ROOT=<repo>/videos/liquid-display-content VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
MSYS_NO_PATHCONV=1 <bin>/Recorder setup
```

`state.json` stays in the folder of the video and is never committed. No mask script is needed: the site is fresh, and
the only person's name on the pages is the user `admin`.

## 2. Order of the clips, and what changes the site

- Narrate first (`scripts/narrate.py`), so `durations.json` exists and each step is held for the length of its narration.
- There is one recording, `02-page`. It depends on no other clip.
- No clip changes the site, so no snapshot has to be restored between takes. To record again, reload the home page and
  start over.
- The slide chapters (`01`, `03`, `04`, `05`, `06`) and the two cards (`00`, `99`) are not recorded: `assemble.py` draws
  them.

## 3. Clip `02-page`: the Blog page, zone by zone

Starts on `/` (the home page, which is the Blog), with the signed-in session of `state.json`.

Before the capture, with `scene.OpenAsync("/")`:

1. Wait until `header.masthead h1` is visible and has the text "Blog".
2. Wait until `div.post-preview` exists in the page (it can be below the visible area).
3. Make sure the page is scrolled to the top.

Then call `scene.StartCaptureAsync()`. Keep every step's actions shorter than its narration (`durations.json`); the pauses below
are upper bounds.

### `page-1`

- Actions: `scene.MoveToAsync(header.masthead h1)`. Nothing else: the step shows the page as it loads.
- Visible when the narration ends: the top of the home page. The navigation bar `#mainNav` shows the site name
  "Orchard Core" and the menu button or links. Under it the masthead shows the heading "Blog" and the line "This is the
  description of your blog". No ring and no caption.
- Labels from: `OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:25-27` (navigation bar, `Site.SiteName`),
  `Views/Content-Blog.liquid:15-16` (heading and description).

### `page-2`

- Actions: `scene.HighlightAsync(header.masthead, "Header zone")`.
- Visible when the narration ends: a green ring around the whole masthead, with the caption "Header zone".
- Labels from: `Views/Content-Blog.liquid:1,22` (the zone tag that fills Header), `Views/Layout.liquid:35` (the layout
  draws it above the page).

### `page-3`

- Actions: `scene.ClearHighlightsAsync()`, then scroll down with `scene.ScrollAsync(0, 400)`, repeated in steps of 400
  until `div.post-preview` is entirely inside the viewport (one or two scrolls), then
  `scene.HighlightAsync(div.post-preview, "Summary display")`.
- Visible when the narration ends: the post preview inside a green ring with the caption "Summary display": the title
  "Man must explore, and this is exploration at its greatest", the subtitle "Problems look mighty small from 150 miles
  up", and the line "Posted by admin on" followed by the date of the setup, with the category and tag badges after it.
- Labels from: `Views/Content-BlogPost.Summary.liquid:1-13` (title, subtitle, "Posted by", author, date),
  `Views/Blog-ListPart.liquid:1-3` (the list draws each post in the Summary display).

### `page-4`

- Actions: `scene.ClearHighlightsAsync()`, then scroll down with `scene.ScrollAsync(0, 400)`, repeated until
  `footer.border-top` is entirely inside the viewport (the end of the page), then
  `scene.HighlightAsync(footer.border-top, "Footer zone")`.
- Visible when the narration ends: a green ring with the caption "Footer zone" around the footer, which shows three round
  icons and the line "Copyright" followed by "Your Website".
- Labels from: `Views/Layout.liquid:44-46` (the footer element and `render_section "Footer"`),
  `Recipes/blog.recipe.json:1097-1098` (the widget is in layer `Always`, zone `Footer`), `Recipes/Snippets/footer.html`.

### `page-5`

- This step has the shortest narration of the clip (30 words, about 12 seconds) and the most actions. The actions must
  finish before the narration does: read `durations.json["page-5"]`, and if the total below is longer than that value
  minus 1 second, reduce the pause of action 4 (never below 1000 ms). The frame at the end of the narration must show the
  body, not the middle of the actions.
- Actions, in this order (about 7 seconds in all):
  1. `scene.ClearHighlightsAsync()`.
  2. Scroll up with `scene.ScrollAsync(0, -2000)` (one scroll, 1.5 seconds at most), then, if `h2.post-title` is not
     inside the viewport, scroll down with `scene.ScrollAsync(0, 200)`, repeated until it is.
  3. `scene.ClickAndWaitForNavigationAsync(h2.post-title)` (2 seconds at most). Wait until the URL path is `/blog/post-1`
     and `header.masthead .post-heading h1` is visible with the text "Man must explore, and this is exploration at its
     greatest".
  4. `scene.HighlightAsync(header.masthead, "Header zone")`, then `scene.PauseAsync(2000)`.
  5. `scene.ClearHighlightsAsync()`, then scroll down with `scene.ScrollAsync(0, 300)`, repeated until the first
     paragraph of the body is inside the viewport (1.5 seconds at most).
- Visible when the narration ends: the post page scrolled so that the first lines of the body are in view: "Never in
  all their history have men been able truly to conceive of the world as one". No ring.
- Labels from: `Views/Content-BlogPost.liquid:1-27` (the Header zone: title, subtitle, "Posted by"),
  `Views/Content-BlogPost.liquid:30` (the Markdown body part),
  `Recipes/blog.recipe.json:1018` and the `MarkdownBodyPart` of that item (the text of the body).

The bottom of the post page prints the title of the recent posts as plain text (`Views/Content-BlogPost.liquid:34-37`).
Do not scroll that far.

## 4. Slides and cards

Drawn by `slides.py` (check it with Pillow and Pygments in memory; nothing is recorded):

- `intro`: the opening card, with the title and the subtitle "Draw pages from shapes, zones and content items".
- `shapes-1`: three cards: Shape, Template, Model.
- `shapes-2`: four cards, one per zone of the Content shape: Header, Meta, Content, Footer.
- `shapes-3`: a three-line Liquid panel (title, `Model.Content`, `Model.Content.MarkdownBodyPart`) and three cards that name them.
- `content-1`: `Content-BlogPost.liquid`, trimmed, with the zone and the Markdown body lines bright, and three cards.
- `content-2`: `Content-Blog.liquid`, trimmed, with the body part, the two `shape_remove_item` lines and the final render bright.
- `content-3`: four cards: the theme file name, the admin template name, the Summary template name, and "The admin comes first".
- `zones-1`: the zone tag around the page header, and three cards.
- `zones-2`: the trimmed `Layout.liquid` with its `render_section` and `render_body` tags, and three cards.
- `zones-3`: three numbered cards: the Content zone, the other zones, the Layout shape.
- `zones-4`: the layout tag, and two cards.
- `items-1`: the loop of `Blog-ListPart.liquid` with `shape_build_display`, and three cards.
- `items-2`: loading a post with the `Content` accessor and a slug handle, and three cards: `alias:`, `slug:`, `contentitemid:`.
- `items-3`: the `contentitem` tag and the `content_item_id` filter, and three cards.
- `make-1`: the DateTime shape of the Blog Post template and the PageTitle shape of the layout, and three cards.
- `make-2`: the `shape` tag of the main menu in the layout, on six lines, and three cards.
- `make-3`: four cards that list the tags that change a shape.
- `make-4`: a post built in the Summary display type, given one more alternate, and three cards.
- `outro`: the closing card, with the subtitle `docs.orchardcore.net` and "Thanks for watching."

## 5. Checks for private data

- The pages show only the records of the Blog recipe and the user name `admin` ("Posted by admin"). No email address, real
  name, phone number or token appears in any frame.
- OCR the video at one frame per second and at every scene change (ffmpeg `select='gt(scene,0.02)'`), then at 5 frames
  per second, and search the text for real names, email addresses and tokens.
- Look at the contact sheet of `02-page` (`review.py`): the frame at the end of each step must match the "Visible when the
  narration ends" line above, with no error page and no empty list.
- The address bar is not part of the capture. Check that no frame shows a machine name or a path of this computer.
