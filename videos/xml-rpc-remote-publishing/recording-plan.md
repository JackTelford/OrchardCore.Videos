# Recording plan: XML-RPC and Remote Publishing

How to record this video without deciding anything. Every file:line is in the Orchard Core source at the commit the
catalog was read from (`src/...` of `OrchardCMS/OrchardCore`); a path that starts with a module name, such as
`OrchardCore.Lists/...`, is under `src/OrchardCore.Modules/`. A selector in this plan stands for
`scene.Page.Locator(...)`. The password is written `<password>`: the recorder takes it from `VIDEO_PASSWORD`, chosen
outside git.

The video has three recordings (`02-enable`, `03-discover`, `05-post`) and four slide chapters. A desktop blogging
client cannot be recorded by the browser recorder, so the video never shows one: the narration says what a client
sends, the slides show the call, and a script stands in for the client (see "The stand-in for the client").

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`,
  with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
- **Setup**: Auto Setup through environment variables, so no setup form or sign-in form is recorded
  (`references/recording.md`). One tenant, `Default`:
  - recipe `Blog`, site name `Orchard Core`, time zone `America/Los_Angeles`, database provider `Sqlite`;
  - user `admin`, email `admin@example.com`, password `<password>` (the value of `VIDEO_PASSWORD`).
- **Outbound calls**: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, so the site never calls a service. The video
  needs no external service.
- **Features enabled by the Blog recipe** (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:50-73`):
  Autoroute, Html, Lists, Markdown, Media and Title, among others. Nothing else is enabled before recording.
- **Features that must be disabled when `02-enable` starts**: `XML-RPC` (`OrchardCore.XmlRpc`) and `Remote Publishing`
  (`OrchardCore.RemotePublishing`). The Blog recipe does not enable them. The clip enables them on camera.
- **Settings**: none are changed. The admin list layout stays at its default (a list of rows with buttons:
  `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`).
- **Seed data**: the content of the Blog recipe, as it is (a `Blog` item with the path `blog`, and one `BlogPost`,
  "Man must explore, and this is exploration at its greatest", at `blog/post-1`:
  `blog.recipe.json:965-1018`). After `03-discover`, the stand-in adds the two posts below.
- **Sign-in**: `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` writes `state.json`
  (git-ignored). Sign in once, outside the capture.

### The stand-in for the client

A script plays the part of the desktop blogging client: it sends the same XML-RPC calls to `/xmlrpc`, as `admin`. Run
it once, after `03-discover`, from a shell where `VIDEO_BASE_URL`, `VIDEO_USER_NAME` and `VIDEO_PASSWORD` are set. It
reads the password from the environment; do not save the script in the repository.

```python
import os, re, urllib.request
from xml.sax.saxutils import escape

base = os.environ.get("VIDEO_BASE_URL", "http://localhost:5010")
user = os.environ.get("VIDEO_USER_NAME", "admin")
password = os.environ["VIDEO_PASSWORD"]


def call(method, params):
    body = f'<?xml version="1.0"?><methodCall><methodName>{method}</methodName><params>{params}</params></methodCall>'
    request = urllib.request.Request(base + "/xmlrpc", data=body.encode("utf-8"), headers={"Content-Type": "text/xml"})
    # No proxy: the site's dead HTTP_PROXY must not apply to a call to localhost.
    opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))
    with opener.open(request) as response:
        return response.read().decode("utf-8")


def text(value):
    return f"<param><value><string>{escape(value)}</string></value></param>"


def new_post(blog_id, title, description, publish):
    members = (f"<member><name>title</name><value><string>{escape(title)}</string></value></member>"
               f"<member><name>description</name><value><string>{escape(description)}</string></value></member>")
    struct = f"<param><value><struct>{members}</struct></value></param>"
    flag = f"<param><value><boolean>{1 if publish else 0}</boolean></value></param>"
    return call("metaWeblog.newPost", text(blog_id) + text(user) + text(password) + struct + flag)


blogs = call("blogger.getUsersBlogs", text("appkey") + text(user) + text(password))
print(blogs)
blog_id = re.search(r"<name>blogid</name>\s*<value>\s*<string>([^<]+)</string>", blogs).group(1)
print(new_post(blog_id, "Draft written offline", "This draft waits for review.", False))
print(new_post(blog_id, "Hello from a desktop client",
               "This post was written in a desktop blogging client and sent to the site.", True))
```

What the three responses must be (the handler is `MetaWeblogHandler.cs`, in `src/OrchardCore.Modules/OrchardCore.Lists/RemotePublishing/`):

- `blogger.getUsersBlogs` (lines 218-249): an array with one struct, `blogName` `Blog`, a `blogid`, and a `url`.
- Each `metaWeblog.newPost` (lines 291-357): a `methodResponse` with one `string`, the id of the new item.
- A `fault` (`faultString`) instead means a call failed: the text is the exception message, for example "The username
  or e-mail or password provided is incorrect." (line 477). A status 500 means no handler answered, which happens when
  `Remote Publishing` or `Lists` is disabled (`HomeController.cs:38-41`). With `Media` disabled the handler cannot be
  created (its constructor needs `IMediaFileStore`, registered by `OrchardCore.Media/Startup.cs:119`), and the call
  fails the same way. Fix the cause and restore the snapshot
  before running it again; never run it twice on the same site, since the list would show duplicates.

The draft is sent first and the published post second, so the published one is the most recently modified item, and
the Content Items list (sorted by last modification, newest first: the default branch
`src/OrchardCore.Modules/OrchardCore.Contents/Services/DefaultContentsAdminListFilterProvider.cs:97-98` and the helper at 306-307) lists it first.

## Order of the clips, and what each one changes

1. Start from a fresh site with the Blog recipe. Stop it and copy its `App_Data` and `state.json` aside as
   **snapshot A** (features disabled).
2. Record `02-enable`. **It changes the site**: it enables `XML-RPC` and `Remote Publishing`. To record it again,
   restore snapshot A. When it is done, stop the site and copy `App_Data` aside as **snapshot B**.
3. Record `03-discover`. It only reads; it needs the two features enabled (snapshot B).
4. Run the stand-in script. **It changes the site**: it adds a draft and a published `BlogPost`. Copy `App_Data`
   aside as **snapshot C**. To run it again, restore snapshot B.
5. Record `05-post`. It only reads; it needs snapshot C.

The clips depend on each other in this order only: `02-enable` before `03-discover`, and the stand-in before
`05-post`. `03-discover` and `05-post` can be recorded again from snapshot C as often as needed. The slide chapters
(`features-1`, `features-2`, `calls-1` to `calls-4`, `extend-1` to `extend-3`) need no recording.

Narrate first (`narrate.py`), then record: each step is held for the length of its narration.

## Clip `02-enable`

Starts on `/Admin/Features`, signed in as `admin`, with the two features disabled (snapshot A). Everything before
`StartCaptureAsync` is not recorded: open the page, and leave the search box empty.

A helper for the row of a feature, as in the skill's example scene: the list item (`li.list-group-item`) that holds
`#btn-enable-<id>` or `#btn-disable-<id>`, where `<id>` is the feature id with its dot replaced by an underscore:
`OrchardCore_RemotePublishing` and `OrchardCore_XmlRpc`.

`enable-1`

- Actions: type `Open Live Writer` into `#search-box` (delay 60). Then highlight the category heading, the `h3` with the
  text `Infrastructure` (caption `Infrastructure`). Then highlight the dependency badge in the row of Remote
  Publishing, `.direct-dependencies` inside the row (caption `Dependency`).
- On screen when the narration ends: the search box holds `Open Live Writer`; one group, `Infrastructure`, with two rows,
  `XML-RPC` `OrchardCore.XmlRpc` and `Remote Publishing` `OrchardCore.RemotePublishing`, each with the badge
  `Disabled` and the button `Enable`; the row of Remote Publishing shows the badge `XML-RPC`, and the highlight ring is
  around it.
- Labels: the search box `OrchardCore.Features/Views/Admin/Features.cshtml:27`; the category heading `Features.cshtml:88` with the category of the
  features `OrchardCore.XmlRpc/Manifest.cs:14,22`; the name and the id in a row
  `OrchardCore.Features/Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`; the search matches the name, the id and the
  description `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20` (both descriptions hold "Open Live
  Writer": `Manifest.cs:13,20`); the dependency badges `FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13-19`; `Disabled`
  `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`; `Enable` `FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; the menu
  path Tools, Features `OrchardCore.Features/AdminMenu.cs:26-27` (paths without `src/` are under `src/OrchardCore.Modules/`).

`enable-2`

- Actions: clear the highlights. Move to `#btn-enable-OrchardCore_RemotePublishing` and click it with
  `ClickAndWaitForNavigationAsync` (the page posts, enables both features, and loads the list again; the tenant
  restarts, so wait up to 30 seconds for the page). Wait until the text `The feature Remote Publishing was enabled.`
  is visible. Type `Open Live Writer` into `#search-box` again.
- On screen when the narration ends: the notification `The feature Remote Publishing was enabled.` (it names only the
  feature that was clicked; XML-RPC is enabled with it without being named), and the group `Infrastructure` with both
  rows showing the badge `Enabled` and the button `Disable`. If the notification has already faded, the two
  `Enabled` badges are enough.
- Labels: the notification `OrchardCore.Features/Controllers/AdminController.cs:317` (the action is lines 226-256 and
  passes `force` as true, so the dependency is enabled too); `Enabled` `FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`;
  `Disable` `FeatureEntry.Buttons.SummaryAdmin.cshtml:53`; the dependency is enabled with it
  `src/OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:74-78,163-168`.
- If the click and the reload take longer than the narration, the step only runs longer; no new take is needed.

## Clip `03-discover`

Needs snapshot B or C: the two features enabled. Starts on `/blog` (the Blog item, which is also the home page).

Before `StartCaptureAsync`, outside the capture:

1. Open `/blog` and evaluate `document.querySelector('link[rel="wlwmanifest"]')?.href` and
   `document.querySelector('link[rel="EditURI"]')?.href`. Both must be non-empty. The first ends with
   `/xmlrpc/metaweblog`. The second ends with `/xmlrpc/metaweblog/<blog id>/rsd`. Keep the path of the second as
   `<rsd path>` (`new URL(href).pathname`). If either is empty, the features are not enabled or the page is not the
   blog: stop and fix the site.
2. Return to `/blog` and start the capture.

The links come from `src/OrchardCore.Modules/OrchardCore.Lists/Views/ListPart.RemotePublishing.cshtml:8-9`, which the
Blog theme renders in the content of the Blog item (`OrchardCore.Themes/TheBlogTheme/Views/Content-Blog.liquid:27`) and
writes in the head (`Layout.liquid:20`).

`discover-1`

- Actions: pause 2500 ms on the blog page, then `NavigateAsync("/xmlrpc/metaweblog")`.
- On screen when the narration ends: the XML of the manifest, shown as a tree by the browser: a `manifest` element
  with an `options` element, whose children are, in this order, `supportsAutoUpdate` Yes, `clientType` MetaWeblog,
  `supportsKeywords` No, `supportsCategories` No, `supportsFileUpload` Yes, `supportsCustomDate` No, `supportsSlug` Yes.
- Source: the manifest `OrchardCore.XmlRpc/Controllers/MetaWeblogController.cs:25-53`; the route `xmlrpc/metaweblog`
  `OrchardCore.XmlRpc/Startup.cs:28-40`; the link type of the Windows Live Writer manifest
  `src/OrchardCore/OrchardCore.Infrastructure.Abstractions/MediaTypeNamesExtended.cs:17`.

`discover-2`

- Actions: highlight `supportsFileUpload` (`GetByText("supportsFileUpload").First`), then, when the narration reaches
  it, `supportsSlug` (`GetByText("supportsSlug").First`). The browser's XML view shows the element names as page text,
  so the text locators find them; if a highlight cannot be placed, leave the page as it is.
- On screen when the narration ends: the manifest with the rings around `supportsFileUpload` and `supportsSlug`, both
  `Yes`.
- Source: `supportsFileUpload` is `No` by default (`MetaWeblogController.cs:39`) and the Media driver sets `Yes`
  (`OrchardCore.Media/RemotePublishing/MediaMetaWeblogDriver.cs:7-10`); `supportsSlug` is added by the Autoroute driver
  (`OrchardCore.Autoroute/RemotePublishing/AutorouteMetaWeblogDriver.cs:11-14`); the handler collects them
  (`OrchardCore.Lists/RemotePublishing/MetaWeblogHandler.cs:64-72`).

`discover-3`

- Actions: clear the highlights, `NavigateAsync(<rsd path>)`, then highlight `apiLink` (`GetByText("apiLink").First`).
- On screen when the narration ends: the RSD document: `rsd` (version 1.0), `service` with `engineName` Orchard CMS,
  `engineLink` and `homePageLink` `https://orchardcore.net`, and `apis` with one `api` whose attributes are
  `name="MetaWeblog"`, `preferred="true"`, `apiLink="http://localhost:5010/xmlrpc"` (ringed) and `blogID` (the id of
  the Blog item).
- Source: `OrchardCore.Lists/Controllers/RemotePublishingController.cs:25-66` (lines 50, 56-58 for the values); the
  route `xmlrpc/metaweblog/{contentItemId}/rsd` `OrchardCore.Lists/RemotePublishing/RemotePublishingStartup.cs:24-29`.

## Clip `05-post`

Needs snapshot C. Starts on `/Admin/Contents/ContentItems/BlogPost` (the list of Content Items filtered to the type
Blog Post; the menu path is Content, Content Items). Before `StartCaptureAsync`, check the order of the rows: the
post `Hello from a desktop client` first, `Draft written offline` second, and the post of the recipe third. If the
order differs, restore snapshot B and run the stand-in again.

A helper for a row: `li.list-group-item` with the text of the title.

`post-1`

- Actions: move to the row `Hello from a desktop client` and highlight it (no caption).
- On screen when the narration ends: the row with the title `Hello from a desktop client` (a link), the type badge
  `BlogPost`, and the badge `Published`; no `Draft` badge.
- Labels and source: `Content Items` `OrchardCore.Contents/AdminMenu.cs:68`; the route
  `OrchardCore.Contents/Controllers/AdminController.cs:67`; the type badge shows the type name
  `OrchardCore.Contents/Views/ContentsType_SummaryAdmin.cshtml:6`; `Published` and `Draft`
  `OrchardCore.Contents/Views/ContentsTags_SummaryAdmin.cshtml:12,19`. The item is created as the first contained type
  of the list (`MetaWeblogHandler.cs:307-308`, with `BlogPost` as the contained type of the Blog:
  `blog.recipe.json:311`) and published when the call asks for it (lines 322, 337-339).

`post-2`

- Actions: clear the highlights, move to the row `Draft written offline` and highlight it.
- On screen when the narration ends: the row with the title `Draft written offline`, the type badge `BlogPost`, and
  the badge `Draft`; no `Published` badge.
- Source: `MetaWeblogHandler.cs:337-344` (a draft when the publish flag is off or the date is in the future) and
  `388-446` (an edit with the publish flag publishes it).

## Slide steps

One line each: what the slide shows. Each slide is drawn by `slides.py`.

- `features-1`: two cards, XML-RPC (the endpoint `/xmlrpc`) and Remote Publishing (the MetaWeblog API).
- `features-2`: three cards: the Lists module answers the calls, a blog is a content item with a List part, a post is an
  item the list contains.
- `calls-1`: four cards with the seven method names, grouped as find, read, write and upload.
- `calls-2`: the XML of a `metaWeblog.newPost` call (placeholders only), with three cards beside it: blog identifier, post
  members, publish flag.
- `calls-3`: three cards: the members `title` (a posted title becomes the display text: `TitleMetaWeblogDriver.cs:45`),
  `description` and `wp_slug`, and the module that maps each.
  The stand-in must not call `metaWeblog.getPost` or `metaWeblog.getRecentPosts` (see `OPEN-QUESTIONS.md`, section 2).
- `calls-4`: four cards: credentials, Edit content for others, Publish or unpublish content for others, and Delete content
  for others, with the media permissions in its text (labels: `OrchardCore.Contents.Core/CommonPermissions.cs:17,21,25`,
  shown by `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:14`).
- `extend-1`: the Html Body driver, `HtmlBodyMetaWeblogDriver`, with `BuildPost` and `EditPost`.
- `extend-2`: the Autoroute driver's `SetCapabilities`, and the startup that registers it as an `IMetaWeblogDriver`.
- `extend-3`: the `MetaWeblogHandler` with `ProcessAsync` (`SetCapabilities`, the other member of `IXmlRpcHandler`, is the
  trimmed `// ...`), and the registration line of an `IXmlRpcHandler`.

## Checks for private data

Before publishing, OCR the video at one frame per second, at every scene change and at five frames per second, and
search the text.

- The site is new and holds the Blog recipe only: no real names, emails or phone numbers. The recipe's author is `admin`.
- The only email is `admin@example.com`; it is not shown in this video, which does not open a user page.
- The manifest and RSD pages show `localhost:5010` and a random content item id: neither is private.
- The slides show `PASSWORD` as a placeholder. The stand-in reads the real password from `VIDEO_PASSWORD`, so it never
  appears on screen. Search the OCR text for the password anyway.
- No browser address bar, cookie or `state.json` is part of the capture.
