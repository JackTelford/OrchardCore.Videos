# Recording plan: Lists in Templates

This is a developer video. Everything is drawn from the storyboard and `slides.py` (two cards and thirteen slides),
except one browser recording: the clip `05-site`, which shows a template in the admin and a blog post page that uses
the three filters of the Lists module. The recording reads the site and never changes it.

Selectors below stand for `scene.Page.Locator(...)`. Labels come from the Orchard Core source at commit `92db4a83`
(paths under `src/OrchardCore.Modules`, unless noted).

## 1. The site it needs

- Orchard Core: a checkout of `OrchardCMS/OrchardCore` at that commit or later. Run `src/OrchardCore.Cms.Web` on
  `http://localhost:5010` (`VIDEO_BASE_URL`) with `ORCHARD_APP_DATA` pointing at an empty folder outside both
  repositories. Start it with
  `dotnet run --project src/OrchardCore.Cms.Web --no-launch-profile --urls http://localhost:5010` (the Auto Setup
  variables below set in the same shell). Follow the `orchardcore-tester` skill of that repository for Auto Setup.
- Auto Setup, by environment variables (documented in `src/docs/reference/modules/AutoSetup/README.md`), tenant `0`:

  | Variable (after `OrchardCore__AutoSetup__Tenants__0__`) | Value |
  |---|---|
  | `ShellName` | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git) |
  | `DatabaseProvider` | `Sqlite` |
  | `RecipeName` | `Blog` |

- Block outbound calls: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address. The video needs no external service and no
  local stand-in.
- Features: the Blog recipe enables everything this video uses (Lists, Templates and Liquid, Contents, Title, Autoroute,
  Menu, Queries) and sets TheBlogTheme as the site theme. Enable nothing else and save no settings.
- Sign in once outside the capture: `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`, `Recorder setup` writes
  `state.json` (git-ignored).

## 2. Seed the site (outside the capture)

The Blog recipe creates a blog named **Blog** (path `blog`) and one sample post. All the data below is invented.

### 2.1 Remove the sample post

Its title quotes a real person, and the count the video shows must be three.

1. Admin menu, **Content**, **Content Items** (`OrchardCore.Contents/AdminMenu.cs:65-68`).
2. Click the title **Blog**. The page of the blog opens (`OrchardCore.Lists/Handlers/ListPartHandler.cs:12-19` makes the admin link of a list
   the `Display` action) and shows the list of its posts, with the buttons **Edit Blog** and **Create Blog Post**
   (`OrchardCore.Lists/Views/ListPartNavigationAdmin.cshtml:43,52`). The button **List Blog Post Items** is not shown
   on this page (`ListPartNavigationAdmin.cshtml:28`). The recipe also adds the admin menu item **Blog**, which opens
   the same page (`TheBlogTheme/Recipes/blog.recipe.json`, step `AdminMenu`).
3. On the row *Man must explore, and this is exploration at its greatest*, open the actions menu, choose **Delete**,
   and confirm with **Ok** (`OrchardCore.Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:40`).

### 2.2 Create four posts

For each row of the table, open the page of the blog (**Content**, **Content Items**, **Blog**), click
**Create Blog Post**, fill **Title** (`OrchardCore.Title/Views/TitlePart.Edit.cshtml:12`) and **Subtitle**, leave every other field empty, and
click the button of the last column (**Publish** or **Save Draft**:
`OrchardCore.Contents/Views/Content.PublishButton.cshtml:19` and `Content.SaveDraftButton.cshtml:19`: the editor opened
by **Create Blog Post** carries a `returnUrl`, so each button is the default entry of a button group; without a
`returnUrl` the plain buttons at line 7 apply). Create them in this order:

| Title | Subtitle | Button |
|---|---|---|
| Notes from the harbour | Boats, tides and early mornings | **Publish** |
| A week in the highlands | Trails, weather and good boots | **Publish** |
| Market day in the old town | Stalls, spices and a slow lunch | **Publish** |
| Packing list for the coast | Still being written | **Save Draft** |

Expected: the page of the blog in the admin shows four posts, newest first, so *Packing list for the coast* is the
first row and is marked as a draft (the admin list sorts by creation date, newest first, when ordering is not enabled:
`OrchardCore.Lists/Services/ContainerService.cs`, `QueryContainedItemsAsync`). The Autoroute pattern of the
Blog Post type (`TheBlogTheme/Recipes/blog.recipe.json:216`) gives the published posts the paths
`blog/notes-from-the-harbour`, `blog/a-week-in-the-highlands` and `blog/market-day-in-the-old-town`.

### 2.3 Create the template

The template is `Content__BlogPost`, the text of `TheBlogTheme/Views/Content-BlogPost.liquid` with its last four lines
(the `Queries.RecentBlogPosts` loop) replaced by the block of the slides. Its complete text:

```liquid
{% zone "Header" %}
    <!-- Page Header -->
    <!-- Set your background image for this header on the line below. -->
    {% assign imagePath = Model.ContentItem.Content.BlogPost.Image.Paths.first %}
    {% if imagePath == nil %}
        <header class="masthead" style="background-image: url('{{ "~/TheBlogTheme/assets/img/post-bg.jpg" | href }}')">
    {% else %}
        {% assign anchor = Model.ContentItem.Content.BlogPost.Image.Anchors.first %}
        <header class="masthead" style="background-image: url('{{ imagePath | asset_url | resize_url: profile:"banner", anchor: anchor }}')">
    {% endif %}
        <div class="container position-relative px-4 px-lg-5">
            <div class="row gx-4 gx-lg-5 justify-content-center">
                <div class="col-md-10 col-lg-8 col-xl-7">
                    <div class="post-heading">
                        <h1>{{ Model.ContentItem.DisplayText }}</h1>
                        <h2 class="subheading">{{ Model.ContentItem.Content.BlogPost.Subtitle.Text }}</h2>
                        <span class="meta">
                            {% assign format = "MMMM dd, yyyy" | t %}
                            {% assign dateTime = "DateTime" | shape_new: utc: Model.ContentItem.CreatedUtc, format: format | shape_stringify %}
                            {{ "Posted by" | t }} <a href="#">{{ Model.ContentItem.Author }}</a> {{ "on {0}" | t: dateTime | raw }}
                        </span>
                    </div>
                </div>
            </div>
        </div>
    </header>
{% endzone %}

{{ Model.Content.ContentsMetadata | shape_render }}
{{ Model.Content.MarkdownBodyPart | shape_render }}
{{ Model.Content.BlogPost-Category | shape_render }}
{{ Model.Content.BlogPost-Tags | shape_render }}

{% assign blog = Model.ContentItem | container %}

<h3>More in
    <a href="{{ blog | display_url }}">{{ blog | display_text }}</a>
</h3>
<p>{{ blog | list_count }} published posts</p>

{% assign posts = blog | list_items %}
<ul>
{% for post in posts %}
    <li>
        <a href="{{ post | display_url }}">{{ post | display_text }}</a>
    </li>
{% endfor %}
</ul>
```

Create it with a `fetch` call in a page of the admin (preferred: the code editor of the form auto-closes `{{` and
`{%`, so typing the text into it corrupts it). Let `TEMPLATE` be the text above:

```js
const page = await (await fetch('/Admin/Templates/Create')).text();
const token = new DOMParser().parseFromString(page, 'text/html')
    .querySelector('input[name="__RequestVerificationToken"]').value;
const response = await fetch('/Admin/Templates/Create', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
        __RequestVerificationToken: token,
        AdminTemplates: 'false',
        Name: 'Content__BlogPost',
        Description: 'Blog post with the list filters',
        Content: TEMPLATE,
        submit: 'Save',
    }),
});
```

(`TemplateController.CreatePost`, `Views/Template/Create.cshtml`.) The response is the list of templates, and it
contains `Content__BlogPost`; if it contains the validation text *The name is mandatory.*, *A template with the same
name already exists.* or *The content is mandatory.* instead (`TemplateController.cs:398,409,420`), the template was not
created. If you create it through the screen instead: **Design**,
**Templates** (`OrchardCore.Templates/AdminMenu.cs:19-20`), **Add Template**
(`Views/TemplateCreateButton.cshtml:6`), fill **Name**, **Description** and **Content**
(`Views/Template/Create.cshtml:18,27,35`) by pasting, and click **Save**. Then check that **Content** holds exactly
the text above.

### 2.4 Check the seed

1. Open `http://localhost:5010/blog/a-week-in-the-highlands` while signed out or signed in. After the content of the
   post, the page shows the heading **More in Blog** (a link), the line **3 published posts**, and a list of three
   links: Notes from the harbour, A week in the highlands, Market day in the old town (in the order the page gives
   them). It does not show Packing list for the coast.
2. If the page shows an error instead, or a different count, stop and read `OPEN-QUESTIONS.md`.
3. Stop the site and copy its `App_Data` folder and `state.json` aside as the "seeded" snapshot.

## 3. Order of the clips

- Only `05-site` is recorded in the browser. It needs the complete seed of section 2 and nothing else.
- It does not change the site, so another take needs no restore. Restore the "seeded" snapshot only if you changed the
  site.
- The cards and the slides need no site.

## 4. The clips

### Cards

- `00-intro` (step `intro`): the opening card, "Lists in Templates", subtitle "Liquid filters for containers and their items".
- `99-outro` (step `outro`): the closing card, "Lists in Templates", subtitle "docs.orchardcore.net".

### Slides (drawn by `slides.py`)

- `01-stored`, step `store-1`: the one-line `ListPart` class and the `ContainedPart` class with `ListContentItemId`,
  `ListContentType` and `Order`.
- `01-stored`, step `store-2`: three cards, `container`, `list_items` and `list_count`, each with what it takes and returns.
- `02-part`, step `part-1`: the `ListPartViewModel` class (the other members stand for `// ...`), with `ContentItems`
  and `Pager` bright, and two notes about them.
- `02-part`, step `part-2`: the template `Blog-ListPart.liquid` as the documentation and `TheBlogTheme/Views` have it,
  with the four lines that assign the texts of the pager links shown as `<!-- ... -->`, and cards for the Blog content
  type, the loop over `Model.ContentItems` and the pager `Model.Pager`.
- `03-container`, step `container-1`: `Model.ContentItem | container | display_text` with its output, and the
  Autoroute pattern of the Blog Post type from the Blog recipe.
- `03-container`, step `container-2`: three cards, the cases in which `container` returns the item itself or raises an
  exception (the message `"A Content Item was expected"`, as `ContainerFilter.cs:20` has it).
- `04-items`, step `items-1`: `list_items` and `list_count` applied to a content item and to a content item id string.
- `04-items`, step `items-2`: four cards, published items only, the whole list, `list_items` sorts by Order, a content
  item or its id.
- `05-template`, step `template-1`: the block of the post template, after a first line `<!-- ... the header and the
  content of the post ... -->` that stands for the rest of the template, with the `container` lines and the heading and
  the count bright.
- `05-template`, step `template-2`: the same block, with the `list_items` lines and the loop bright.
- `05-template`, step `template-3`: two cards, the file `Content-BlogPost.liquid` in a theme and the admin template
  `Content__BlogPost`, which takes priority over the file and so holds the complete template.
- `06-razor`, step `razor-1`: a Razor view calling `QueryListItemsAsync` and `QueryListItemsCountAsync`, with those
  lines bright, and two notes.
- `06-razor`, step `razor-2`: the same view, with the call that passes the predicate `x => x.Latest` bright, and a note.

### `05-site` (kind `clip`)

Starts on `/Admin/Templates`, opened with `scene.OpenAsync` before `StartCaptureAsync`, signed in with `state.json`.
The list shows the template `Content__BlogPost` with the description *Blog post with the list filters*. Keep the
admin menu as the site shows it.

- `site-1`: wait 600 ms, then
  `scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Content__BlogPost", Exact = true }), "Content__BlogPost")`
  (the name of the template is a link, `Views/TemplateEntry.Fields.SummaryAdmin.cshtml`).
  When the narration ends, the list of templates is on screen with the row `Content__BlogPost` ringed. Labels:
  **Design**, **Templates** (`OrchardCore.Templates/AdminMenu.cs:19-20`); the template name is the one created in 2.3.
- `site-2`: `scene.ClearHighlightsAsync()`, then `scene.NavigateAsync("/blog/a-week-in-the-highlands")`. Wait until
  the heading `h3` with the text *More in* is attached, then `scene.ScrollAsync(0, 700)` (adjust so the heading and the
  count are both in view), then ring the heading (`scene.Page.Locator("h3", new() { HasText = "More in" })`) and the
  paragraph with the text *published posts*. When the narration ends, the heading **More in Blog** and the line
  **3 published posts** are ringed. The text comes from the template of 2.3.
- `site-3`: `scene.ClearHighlightsAsync()`, then ring the list that follows the count
  (`scene.Page.Locator("p:has-text('published posts') + ul")`). When the narration ends, the three links are ringed,
  and *Packing list for the coast* is not on the page.

If the path of the post differs (the Autoroute path is generated from the title), open the post from the list of the
blog, or use the URL its **View** button gives.

## 5. Checks for private data

- Every name on screen is invented: the blog is called Blog, the posts are the four of 2.2, the user is `admin`.
  The email address of the account is `admin@example.com` and is not on any screen of this video.
- The sample post *Man must explore, and this is exploration at its greatest* is deleted before recording; the video
  must not show it.
- OCR the video at one frame per second and at every scene change, and search for any name, email address other than
  `@example.com`, token or password. The password is never typed on camera (`state.json` carries the session).
- The post page shows, besides the invented posts, the text *Posted by admin* with the creation date, the menu entries
  *Home* and *About*, and the footer of the recipe, *Copyright © Your Website 2021*, with three social icons that link to
  `#!` (`TheBlogTheme/Recipes/Snippets/footer.html`). None is private.
- The site is new: no mask script is needed. Do not commit `state.json`, the snapshot, the audio, the clips or the MP4.
