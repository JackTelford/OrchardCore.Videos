# Recording plan: display-shapes

"Shapes: How Orchard Core Renders a Page" has 27 narrated steps. Twenty are slides, drawn by `slides.py`, and two are
cards (`intro` and `outro`): none of them is recorded. Five steps are recorded, in two clips: `02-page` and `09-debug`. In this file a selector such as
`#navbarResponsive` stands for `scene.Page.Locator("#navbarResponsive")`, and every `file:line` is a place in the
Orchard Core source (`OrchardCMS/OrchardCore`, commit `92db4a83`) where the label or the markup comes from.

## The site it needs

A fresh site installed by Auto Setup, so no Setup screen and no login form are recorded. The Setup screen is not the
subject, so Auto Setup is used and a sign-in (`state.json`, never committed) is saved.

| What | Value |
|---|---|
| Orchard Core | A checkout of `OrchardCMS/OrchardCore` at commit `92db4a83` or a later `main`; run `src/OrchardCore.Cms.Web` on `http://localhost:5010` |
| App data | `ORCHARD_APP_DATA` points at an empty folder outside both repositories |
| Tenant | `Default` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Recipe | `Blog` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`); it enables `TheBlogTheme` as the site theme and `TheAdmin` as the admin theme |
| Admin | user `admin`, email `admin@example.com`, password `<password>` (the one passed in `VIDEO_PASSWORD`, chosen outside git) |
| Database | `Sqlite` |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address such as `http://127.0.0.1:9`, so the server never calls a mail, SMS or payment provider |
| Features to enable | None beyond the recipe. The Settings module, which holds the Debugging page, is always enabled (`OrchardCore.Settings/Manifest.cs:10`) |
| Settings to save | None. `Write shape debug information` must be **unchecked** before `09-debug` (it is off by default) |
| External services | None. The Blog theme asks the browser for Google fonts (`TheBlogTheme/Views/Layout.liquid:12-13`); the page is recorded as it loads, with those fonts or with the fallback fonts |

Auto Setup, as environment variables (`src/docs/reference/modules/AutoSetup/README.md`):

```bash
export OrchardCore__AutoSetup__Tenants__0__ShellName=Default
export OrchardCore__AutoSetup__Tenants__0__SiteName="Orchard Core"
export OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
export OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
export OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
export OrchardCore__AutoSetup__Tenants__0__AdminPassword="$VIDEO_PASSWORD"
export OrchardCore__AutoSetup__Tenants__0__DatabaseProvider=Sqlite
export OrchardCore__AutoSetup__Tenants__0__RecipeName=Blog
```

The recorder runs with `VIDEO_ROOT` (this folder), `VIDEO_BASE_URL=http://localhost:5010`, `VIDEO_USER_NAME=admin` and
`VIDEO_PASSWORD`. There is no `PrepareAsync`: the video needs no data other than the recipe's.

### The seed data

Nothing is created by hand. The Blog recipe creates all the content the clips show:

- A `Blog` content item titled `Blog`, with the text `This is the description of your blog`; it is the home page.
- One `BlogPost` titled `Man must explore, and this is exploration at its greatest`, in that blog.
- A `Main Menu` with the links `Home` and `About`.

All of it is sample content of the recipe, with no real person, company, email or domain. The author shown on the post is
the user `admin`.

## Snapshot, and the order of the clips

1. Start the site, open `/` once so that Auto Setup runs, then run `Recorder setup` to save `state.json`.
2. Stop the site (by its process ID, never by image name). Copy the `App_Data` folder, and `state.json`, to a folder
   outside both repositories: this is **the clean snapshot**. Start the site again.
3. Record `02-page`. It only reads: it does not change the site.
4. Record `09-debug`. **It changes the site**: it saves the Debugging settings with `Write shape debug information`
   checked. To record it again, or to make a full take, stop the site, restore the clean snapshot, and start the site.

The clips do not depend on each other. Record them in this order so that the first take of `02-page` is made with the
setting off.

## Clips

### `02-page`: A page made of shapes (chapter 02)

Starts on `/` (the home page, which shows the blog). Open it with `scene.OpenAsync("/")`, then wait until
`.post-preview` is attached (`await scene.Page.Locator(".post-preview").First.WaitForAsync()`), with the page scrolled to
the top, and only then `scene.StartCaptureAsync()`.

| Step | Actions | On screen when the narration ends | Source |
|---|---|---|---|
| `page-1` | `await scene.HighlightAsync(scene.Page.Locator("#navbarResponsive"), "Menu shape");` | The top of the page: the brand `Orchard Core` at the left of the navigation bar, and its links `Home` and `About` at the right, inside a green ring with the caption `Menu shape` | `TheBlogTheme/Views/Layout.liquid:32` creates the `menu` shape; `TheBlogTheme/Views/Menu.liquid:1` is its template (`id="navbarResponsive"`) |
| `page-2` | `await scene.ClearHighlightsAsync();` then `await scene.HighlightAsync(scene.Page.Locator("header.masthead"), "Header zone");` | The large header with the title `Blog` and the line `This is the description of your blog`, inside a green ring with the caption `Header zone`; the navigation bar above it | `TheBlogTheme/Views/Content-Blog.liquid:1` (`{% zone "Header" %}`) and `:6` (`header.masthead`); `TheBlogTheme/Views/Layout.liquid:35` renders the `Header` zone |
| `page-3` | `await scene.ClearHighlightsAsync();` then `await scene.ScrollAsync(0, 400);` then `await scene.PauseAsync(600);` then `await scene.HighlightAsync(scene.Page.Locator(".post-preview").First, "Summary display type");` | The summary of the post `Man must explore, and this is exploration at its greatest` (its title, the subtitle `Problems look mighty small from 150 miles up`, and `Posted by admin on` with a date), inside a green ring with the caption `Summary display type` | `TheBlogTheme/Views/Content-BlogPost.Summary.liquid:1` (`div.post-preview`); `TheBlogTheme/Views/Blog-ListPart.liquid:2` builds each item with the `Summary` display type |

The captions are drawn by the recorder; they are not labels of the product. Each action list is shorter than its
narration.

### `09-debug`: Find the template of a shape (chapter 09, first clip)

Starts on `/Admin` (open it with `scene.OpenAsync("/Admin")`, and wait for `#left-nav` to be visible; the `Settings`
group of the admin menu is collapsed). The site is the clean snapshot, with `Write shape debug information` unchecked.
Then `scene.StartCaptureAsync()`.

| Step | Actions | On screen when the narration ends | Source |
|---|---|---|---|
| `debug-1` | 1. `await scene.ClickAsync(scene.Page.Locator("#left-nav button.nav-group-label:has(.title:text-is(\"Settings\"))"), pauseAfter: 700);` expands the group. 2. `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a.item-label:has(.title:text-is(\"Debugging\"))"));` opens `/Admin/Settings/debugging`. 3. `await scene.ClickAsync(scene.Page.Locator("label:has-text(\"Write shape debug information\")"), pauseAfter: 700);` checks the box. 4. `await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.save"));` saves, and the page loads again. | The Debugging page with the title `Settings` in the breadcrumb, the box `Write shape debug information` **checked**, its hint `Wrap rendered shapes in HTML comments to help identify the template or binding that produced each fragment.`, the `Save` button, and, if it is still shown, the message `Site settings updated successfully.` | Menu: `OrchardCore.Settings/AdminMenu.cs:36` (`Settings`) and `:46` (`Debugging`); label and hint: `OrchardCore.Settings/Views/DebugSettings.Edit.cshtml:7` and `:9`; button: `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`; message: `OrchardCore.Settings/Controllers/AdminController.cs:119`; route: `AdminController.cs:50`; the toggle button of a first-level item with children and no page: `TheAdmin/Views/NavigationItemLink-admin.cshtml:29` (`button`), `:71-74` (class `nav-group-label`, which only a first-level item with children has, so the locator cannot match a nested item also titled `Settings`) |
| `debug-2` | `await scene.NavigateAsync("/");` then `await scene.Page.Locator("header.masthead").WaitForAsync();` | The home page of the blog, at the top: the navigation bar and the large header. The page looks the same as in `02-page` | `TheBlogTheme/Views/Content-Blog.liquid:6` |

The option takes effect without restarting the site: `DebugSettingsDisplayDriver` asks for an options refresh when the
value changes (`OrchardCore.Settings/Drivers/DebugSettingsDisplayDriver.cs:62-65`). The HTML comments the option
writes are not drawn in the page, so this clip does not show them: the next step shows their format on a slide. The
capture shows the page, not the tab strip of the browser; the title of the tab is a place where the comments may
show as text (`OPEN-QUESTIONS.md`, item 9).

## Slides

The steps below are drawn by `slides.py`; nothing is recorded for them.

| Step | What the slide shows |
|---|---|
| `what-1` | Four cards: Properties, Metadata, Items and Binding; the first three are bright |
| `what-2` | The same four cards; the Binding card is bright |
| `create-1` | The Razor code that creates a Car shape with `New` and renders it with `DisplayAsync`, and two cards: New, DisplayAsync |
| `create-2` | The template `Views/Car.cshtml`, and three cards: the shape type, Model, and the result `A Blue Acme car.` |
| `create-3` | The Liquid code with `shape_new`, `shape_add_properties` and `shape_render`, and three cards |
| `create-4` | The C# code that creates the shape with `IShapeFactory` and renders it with `IDisplayHelper`, and two cards |
| `contract-1` | The `Properties` dictionary code, and three cards: Metadata, Properties, Items |
| `contract-2` | Four cards: Type, DisplayType, Alternates, Wrappers |
| `contract-3` | The `Car` class and `CreateAsync<Car>`, and three cards: shape type, .NET type, requirements |
| `render-1` | Four cards: Displaying, Resolve a binding, Execute the binding, Displayed |
| `render-2` | Four cards: Razor template, Liquid template, Shape method, Binding resolver |
| `render-3` | Three cards for the override order (application, theme and base themes, modules), and the exception message when no binding is found |
| `name-1` | Four cards that map a file name to a binding name, from `Car.cshtml` to `Common.Text-FirstName.cshtml` |
| `code-1` | The `CarShapes` class with a `[Shape]` method and its registration, and three cards |
| `zone-1` | The tree of a Layout with its zones, and three cards: Layout, Zone, Theme |
| `zone-2` | The code that adds a shape to the `Header` zone, and two cards: on demand, by position |
| `driver-1` | Three cards: `IDisplayResult`, `ShapeResult`, and placement first |
| `driver-2` | The `ProductPartDisplayDriver` code with `Initialize` and `Location`, and three cards |
| `debug-3` | The format of the start and end comments, and three cards: type, bindings, shape-end |
| `debug-4` | Four cards, a checklist: read the comment, check the folder, check the name, check the order |

## Checks for private data

Before the clips go into the video, look at every contact sheet (`review.py`) and OCR the result:

- Only fake data is on screen: the user `admin`, the email `admin@example.com` (if the email shows at all), and the
  recipe's sample blog. No password: the `<password>` is never typed on camera, because the sign-in comes from
  `state.json`.
- No path of the machine, no user name of the machine, and no real host name: the address bar is not recorded, and the
  `debug-3` slide shows the fictional theme `Contoso`.
- No other tenant, no other site name than `Orchard Core`, and no leftover record of another site: the site is fresh,
  so no mask script is needed.
