# Recording plan: Orchard Core media profiles

Everything a person needs to record this video without deciding anything. Labels are quoted as the Orchard Core
source spells them; every one has its file and line in the Orchard Core checkout the site runs from (paths start at
`src/OrchardCore.Modules/OrchardCore.Media` unless another module is named). A selector written below stands for
`scene.Page.Locator(...)`, since the recorder's methods take locators. Check each one on the running site with
`Recorder probe` before recording.

## The site it needs

- **Orchard Core**: the `src/OrchardCore.Cms.Web` project of an `OrchardCMS/OrchardCore` checkout, run on
  `http://localhost:5010` with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). The image
  resizing needs the native libvips library that comes with the NetVips packages of the Media module, so the first
  header image of the home page also tells you whether it loads.
- **Provisioning**: Auto Setup through environment variables, so no setup screen and no sign-in form is recorded. See
  the `orchardcore-tester` skill of the Orchard Core repository, `references/autosetup.md`. The values:

  | Variable (`OrchardCore__AutoSetup__Tenants__0__...`) | Value |
  |---|---|
  | `ShellName` | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>`, the value of `VIDEO_PASSWORD`, chosen outside git |
  | `DatabaseProvider` | `Sqlite` |
  | `RecipeName` | `Blog` |

- **Recipe**: Blog. It enables the Media feature, creates the `banner` media profile, copies the theme's sample photos
  `home-bg.jpg`, `post-bg.jpg` and `about-bg.jpg` to the Media Library, and creates the Blog (the home page), one blog
  post (`/blog/post-1`) and the About page, each with one of those photos as its Image
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1111` for the profile, `:983`, `:1024` and `:1081`
  for the images). No other feature needs to be enabled.
- **No outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, as `references/recording.md` says.
- **Sign-in**: run `Recorder setup` once, with `VIDEO_USER_NAME` set to `admin` and `VIDEO_PASSWORD` set to the
  password chosen outside git. It writes `state.json`, which is git-ignored.
- **Media settings**: leave the Media section of the configuration as it is. `UseTokenizedQueryString` is true by default
  (`Services/MediaOptionsConfiguration.cs:68` and `:113`). That is what adds the **Custom Size** option to the Width and
  Height lists (`Controllers/MediaProfilesController.cs:356-360`, used in `create-3`), and what lets a signed image
  address carry the Pad background color (`Processing/MediaCommandParser.cs:69-80`, used in `site-4` and `site-5`). Do not
  set it to false, and do not replace `SupportedSizes`: the list must keep 240, 600 and 2048 (`MediaOptionsConfiguration.cs:10`)
  and must not contain 320.
- **Admin list layout**: the defaults, List layout and Buttons for row actions (`OrchardCore.Admin.Abstractions`,
  `AdminListOptions.cs:30` and `:36`). The selector that lets a user pick another layout is off by default
  (`AdminListOptions.cs:42`), so on a fresh site nothing changes them. The Media Profiles page must show one row per
  profile, with **Edit** and **Delete** buttons at the right of the row. If a take shows a grid or a menu instead, a
  setting was changed: restore S0 and its `state.json`.
- **No `PrepareAsync`**: the Blog recipe is all the seed data there is. Nothing is created by hand before the first clip.

### The fake data of the video

The video creates one profile on camera and edits one that the recipe made.

| Profile | Name | Hint | Width | Height | Resize Mode | Format | Quality | Auto Orient | Background Color |
|---|---|---|---|---|---|---|---|---|---|
| Created in `04-create` | `thumbnail` | `A thumbnail (320px x 240px, cropped)` | Custom Size, 320 | 240 | Crop | Jpg | 80 | ticked | empty |
| Edited in `05-site` | `banner` (made by the recipe) | `A banner image (2048px x 600px, cropped)` | 2048 | 600 | Crop, then Pad | Default | 100 | ticked | empty, then `336699` |

After `05-site`, the hint of `banner` still reads "cropped" while its Resize Mode is Pad. The narration of `06-manage` does
not mention the hint, so leave it as it is.

The photos are the Blog theme's own sample photos (`src/OrchardCore.Themes/TheBlogTheme/wwwroot/assets/img`). What the
author saw in them:

- `home-bg.jpg` (1900 by 1267): a white desk seen from above, with a plant, a phone, a blank notebook, a pencil,
  glasses and a laptop. No person.
- `post-bg.jpg` (1900 by 1265): an astronaut in a spacesuit next to a solar panel, in black space. The face is hidden
  behind the visor and shows only a reflection; a mission patch is on the sleeve. This is a person, not identifiable by
  face or by a readable name. The maintainer must accept this photo, or replace it (see OPEN-QUESTIONS.md, section 6).
- `about-bg.jpg` (1900 by 1501): a map, a camera, a backpack and a field notebook. No person. It is not recorded.

Before recording, open `http://localhost:5010/media/home-bg.jpg` and `http://localhost:5010/media/post-bg.jpg` and look at
them again. If the maintainer asks for a replacement, upload a landscape photo (about 1900 by 1267 pixels) under the same
file name in Media, Library.

## Before the first take

1. Start the site and open `/` once, so Auto Setup runs.
2. Run `Recorder setup`.
3. Open Media, Profiles and check: exactly one row, `banner`, with the hint `A banner image (2048px x 600px, cropped)`;
   open its Edit page and check Width 2048, Height 600, Resize Mode Crop, Format Default, Quality 100, Auto Orient
   ticked.
4. Open `/` and `/blog/post-1` and check that both headers show a photo. Open `/about` too, as a last check that the
   About page has its photo (it is not recorded).
5. Stop the site and copy its `App_Data` folder and `state.json` aside. Call this copy **S0**.

## Order of the clips, and what each one changes

Record in this order. Each clip starts from the state the previous one left.

| Order | Clip | Starts from | Changes the site |
|---|---|---|---|
| 1 | `02-list` | S0 | Nothing. It must run while `banner` is the only profile. |
| 2 | `04-create` | S0 (after `02-list`) | Adds the profile `thumbnail`. |
| 3 | `05-site` | The state `04-create` left | Changes `banner` to Pad with the background color `336699`. |
| 4 | `06-manage` | The state `05-site` left | Deletes `thumbnail`. |

`06-manage` needs the `thumbnail` profile of `04-create`. The other clips do not depend on each other, apart from
the order above. To record a clip again, stop the site, restore S0 and its `state.json`, start the site, and bring it to
the state the table gives for that clip:

- `02-list`: S0 as it is.
- `04-create`: S0 as it is.
- `05-site`: S0 as it is. If `thumbnail` exists, the list simply shows a second row.
- `06-manage`: S0, then create `thumbnail` by hand, off camera, with the values of the table above.

The slides (`why-1`, `why-2`, `modes-1`, `modes-2`) are drawn by `slides.py`; nothing is recorded for them.

## Clip `02-list` (chapter 02, The Media Profiles page)

Starts on `/Admin` (the dashboard), opened with `OpenAsync` before the capture. Leave the admin menu as it is: opening
it is part of the story. The viewer is signed in as the administrator `admin`.

- `list-1`: Click the menu entry **Media** of the admin menu, then its child **Profiles** with
  `ClickAndWaitForNavigationAsync` (`scene.Page.GetByRole(AriaRole.Link, new() { Name = "Profiles", Exact = true })`).
  Move the cursor over the search box afterwards. The admin menu has two links named Media: use the top-level one (its node
  has the id `media`, `AdminMenu.cs:20`), not the one under Settings (`AdminMenu.cs:35`); confirm the selector with
  `Recorder probe`. On screen at the end of the narration: the **Media Profiles** page with
  the search box, the **Add Media Profile** button, and one row, `banner`, with its hint under the name. Labels:
  "Media" and "Profiles" `AdminMenu.cs:18` and `:26`; page title "Media Profiles" `Views/MediaProfiles/Index.cshtml:6`;
  the hint under the name `Views/MediaProfileEntry.Description.SummaryAdmin.cshtml:8`; the permission "Manage Media
  Profiles" `OrchardCore.Media.Core/MediaPermissions.cs:23` (in `src/OrchardCore`), granted by default to the
  Administrator role only, `PermissionProvider.cs:31`.
- `list-2`: Highlight the name `banner` and the hint under it
  (`HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "banner", Exact = true }))` and
  `HighlightAsync(scene.Page.Locator(".admin-list-secondary"))`), one after the other, about 3 seconds each. On screen at
  the end: the `banner` row with both rings, reading `A banner image (2048px x 600px, cropped)`. The profile comes from
  `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:1111`; the theme uses it in
  `Views/Content-Blog.liquid:9`, `Views/Content-BlogPost.liquid:9` and `Views/Content-Article.liquid:9`
  (the claim of the narration).
- `list-3`: `ClearHighlightsAsync()`. Move the cursor to `#search-box`, then to the row's **Edit** and **Delete** buttons
  (`scene.Page.Locator("a.btn-primary", new() { HasText = "Edit" })` and
  `scene.Page.Locator("a.btn-danger", new() { HasText = "Delete" })`), then to the **Add Media Profile** button (`a.create`),
  pausing about 1.5 seconds on each. Do not click anything. On screen at the end: the cursor on **Add Media Profile**.
  Labels: "Search" `Index.cshtml:20`, "Add Media Profile" `Index.cshtml:25`, "Edit" and "Delete"
  `Views/MediaProfileEntry.Buttons.SummaryAdmin.cshtml:6` and `:8`.

## Slides of chapter 01 and chapter 03

- `why-1`: four cards, "Banner", "Thumbnail", "Original" and "Sizes written by hand", all bright, under the heading
  "One picture, several sizes".
- `why-2`: four checked cards, "A name", "Image settings", "Asked for by name" and "The original stays", under the
  heading "A media profile".
- `modes-1`: four cards, "Max", "Crop", "Pad" and "Stretch" under the heading "Resize Mode"; Max and Crop are bright,
  Pad and Stretch dimmed. The statements come from `Processing/VipsImageProcessingEngine.cs:62-69` (the mode switch),
  `:73-75` (Max never upscales), `:111` (Crop) and `:163` (Pad). The narration says only that a large picture fills the
  size in Crop, because a picture smaller than the size is not enlarged on the path without a focal point (`:115-123`)
  and is enlarged on the path with one (`:141-147`); see OPEN-QUESTIONS.md, section 2.
- `modes-2`: the same four cards; Pad and Stretch are bright, Max and Crop dimmed. Stretch: `:86`.

## Clip `04-create` (chapter 04, Create a profile)

Starts on `/Admin/MediaProfiles`, opened with `OpenAsync` before the capture, with only `banner` listed (record it
after `02-list`). Every `SelectAsync` below passes the option's value, which the controller sets
(`Controllers/MediaProfilesController.cs:352-374`). A headless browser draws no open list, so the viewer sees the value
change, not the list.

- `create-1`: `ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"))`. On screen at the end: the **Create Media
  Profile** form, with the breadcrumb "Media Profiles" before the page title. Name and Hint are empty; Width and Height
  read "Default", Resize Mode reads "Default (Max)", Format reads "Default"; Quality holds 100 and Auto Orient is ticked
  (`Views/MediaProfiles/Create.cshtml:4`; the defaults are in `ViewModels/MediaProfileViewModel.cs`).
- `create-2`: `TypeAsync(scene.Page.Locator("#Name"), "thumbnail")`, then
  `TypeAsync(scene.Page.Locator("#Hint"), "A thumbnail (320px x 240px, cropped)")`. On screen at the end: both fields
  filled. Labels "Name" `Create.cshtml:12`, "Hint" `:23`. The name is saved in lowercase,
  `Services/MediaProfilesManager.cs:32`.
- `create-3`: `SelectAsync(scene.Page.Locator("#SelectedWidth"), "-1")` (the option "Custom Size"). The number box
  `#CustomWidth` appears under the list; it holds `0`, so empty it first with
  `await scene.Page.Locator("#CustomWidth").FillAsync("")`, then
  `TypeAsync(scene.Page.Locator("#CustomWidth"), "320")`. Then
  `SelectAsync(scene.Page.Locator("#SelectedHeight"), "240")`. On screen at the end: Width reads "Custom Size" with 320
  in the box below it, Height reads 240. The hint under each list ("Select the width for the processed image. A default
  value means a width command will not be applied." and the same for the height, `Create.cshtml:42` and `:55`) stays on
  screen: it is what the narration says about Default. The list holds 240 and does not hold 320
  (`Services/MediaOptionsConfiguration.cs:10`), which is why 320 needs Custom Size. Labels "Width" `Create.cshtml:34`,
  "Height" `:47`, "Default" and "Custom Size" `MediaProfilesController.cs:352` and `:358`; the box appears for "Custom
  Size" because of
  `wwwroot/Scripts/media-profile-editor/media-profile-editor.js` (built from the Bloom component named in
  `Assets/ts/media-profile-editor.ts`).
- `create-4`: `SelectAsync(scene.Page.Locator("#SelectedMode"), "2")` (the option "Crop"). Move the cursor over the place
  where the Background Color field would be. On screen at the end: Resize Mode reads "Crop", and there is **no**
  Background Color field between Resize Mode and Format. Labels "Resize Mode" `Create.cshtml:60`, "Crop"
  `MediaProfilesController.cs:364`; the Background Color row is hidden unless the mode is Pad or BoxPad,
  `Create.cshtml:70`.
- `create-5`: `SelectAsync(scene.Page.Locator("#SelectedFormat"), "3")` (the option "Jpg"; the WebP option has the value
  6, not 4). Then empty the quality box, which holds `100`:
  `await scene.Page.Locator("#Quality").FillAsync("")`, then `TypeAsync(scene.Page.Locator("#Quality"), "80")`. On screen at
  the end: Format reads "Jpg", Quality reads 80. Labels "Format" `Create.cshtml:82`, "Quality" `:93`, "Jpg"
  `MediaProfilesController.cs:372`, the hint "The quality percentage for the processed image. Only supported with the JPG
  and WebP format. A value of 100% means a quality command will not be applied." `Create.cshtml:99`. "At 100 the default
  quality applies": the profile sends a quality command only between 1 and 99 (`Services/MediaProfileService.cs:44`), and
  without one the engine encodes at its own quality (`Processing/VipsImageProcessingEngine.cs:262-266`).
- `create-6`: Scroll down if needed (`ClearHighlightsAsync()` first), then
  `HighlightAsync(scene.Page.Locator("#AutoOrient"))` for about 4 seconds. Then
  `ClearHighlightsAsync()`, `PauseAsync(1500)`, and click **Save** (plain **Save**, never "Save and continue") with
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit'][value='Save']"))`. Start the click no earlier
  than about 8 seconds into the step. On screen at the end: the Media Profiles list with two rows, `banner` and
  `thumbnail`, and `A thumbnail (320px x 240px, cropped)` under `thumbnail`. There is no success message after Save.
  Labels "Auto Orient" `Create.cshtml:107`, the hint "Automatically orient the image based on EXIF metadata. Enabled by
  default." `:109`, "Save" `:122`.

## Clip `05-site` (chapter 05, A profile on the site)

Starts on the home page, `/`, opened with `OpenAsync` before the capture. Wait for it to settle first
(`WaitForLoadStateAsync(LoadState.NetworkIdle)`).

- `site-1`: `HighlightAsync(scene.Page.Locator("header.masthead"))` and hold. On screen at the end: the home page of the
  Blog site, with the photo behind the title "Blog" and the text "This is the description of your blog", ringed in
  green. The header image is `/media/home-bg.jpg` with the `banner` profile applied
  (`Views/Content-Blog.liquid:9` in the Blog theme).
- `site-2`: `ClearHighlightsAsync()`, then `NavigateAsync("/blog/post-1")`; wait for the header photo to load
  (`NetworkIdle`). On screen at the end: the blog post "Man must explore, and this is exploration at its greatest" with
  its photo behind the title. (`Content-BlogPost.liquid:9`; the About page uses it too, `Content-Article.liquid:9`.)
- `site-3`: `NavigateAsync("/Admin/MediaProfiles")`, then open the Edit page of banner with
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "banner" }).GetByRole(AriaRole.Link, new() { Name = "Edit" }))`.
  On screen at the end: the **Edit Media Profile** form of `banner`: Name `banner`, Width 2048, Height 600, Resize Mode
  Crop. Label "Edit Media Profile" `Views/MediaProfiles/Edit.cshtml:4`.
- `site-4`: `SelectAsync(scene.Page.Locator("#SelectedMode"), "3")` (the option "Pad"). The **Background Color** field
  appears (`.media-profile-background-color-wrapper`). `TypeAsync(scene.Page.Locator("#BackgroundColor"), "336699")`.
  Pause 1.5 seconds so the viewer sees it, then click plain **Save** with
  `ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit'][value='Save']"))`. Never use "Save and
  continue": after a rename it returns to the form with the old name. On screen at the end: the Media Profiles list.
  Labels "Background Color" `Edit.cshtml:62`, "Pad" `MediaProfilesController.cs:365`.
  The color is read as a three- or six-digit hexadecimal code by `Processing/VipsImageProcessingEngine.cs:213`; use a
  value like `336699`.
- `site-5`: `NavigateAsync("/")`, then wait off camera until the header image has loaded in its changed form. The first
  request for a changed address makes the resized image, so wrap the wait in `OffCameraAsync`, and do it in two parts
  inside it. `WaitForFunctionAsync` polls a function that returns a plain value, so the first part must not be `async`:

  ```js
  // 1. WaitForFunctionAsync: true only once the page carries the changed header.
  () => {
    const header = document.querySelector('header.masthead');
    return !!header && getComputedStyle(header).backgroundImage.includes('rmode=pad');
  }
  ```

  ```js
  // 2. EvaluateAsync (it waits for the promise): requests the image once, so the picture is ready.
  async () => {
    const header = document.querySelector('header.masthead');
    const match = /url\("?([^")]+)"?\)/.exec(getComputedStyle(header).backgroundImage);
    return (await fetch(match[1])).ok;
  }
  ```

  Hold on the page for 3 seconds afterwards. On screen at the end: the home page header with the photo in the middle
  and a plain color band on each side, under the dark overlay of the theme. The header scales its image to cover the
  whole header (`background-size: cover`), so the top and bottom of the photo may be cut by a few percent at a wide
  window; the bands and the photo must both be visible at the window size of the recorder. If the bands cannot be told
  from the photo, see OPEN-QUESTIONS.md and do not publish this take.
- `site-6`: `NavigateAsync("/blog/post-1")` and the same two-part off-camera wait. On screen at the end: the header of the blog
  post with the photo in the middle and the color bands on both sides.

## Clip `06-manage` (chapter 06, Search and delete)

Starts on `/Admin/MediaProfiles`, opened with `OpenAsync` before the capture. It lists `banner` and `thumbnail`.

- `manage-1`: `TypeAsync(scene.Page.Locator("#search-box"), "thumb")`, then
  `WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"))`. On screen at the end: the search box holds
  `thumb` and only the `thumbnail` row is listed (`MediaProfilesController.cs:75`, the search by name;
  `:118`, the filter post).
- `manage-2`: Click the **Edit** button of the `thumbnail` row with `ClickAndWaitForNavigationAsync`. Highlight the
  Name field (`HighlightAsync(scene.Page.Locator("#Name"))`) for about 4 seconds. Do not change anything. Then
  `ClearHighlightsAsync()` and click **Cancel** (`a.cancel`) with `ClickAndWaitForNavigationAsync`, about 7 seconds into
  the step. On screen at the end: the Media Profiles list, back with `banner` and `thumbnail`. Labels "Edit" as above,
  "Cancel" `Edit.cshtml:116`.
- `manage-3`: Click the **Delete** button of the `thumbnail` row
  (`scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "thumbnail" }).GetByRole(AriaRole.Link, new() { Name = "Delete" })`).
  A dialog titled **Delete** asks "Are you sure you want to remove this element?" with **Ok** and **Cancel**
  (`src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`). `PauseAsync(2000)`, then click **Ok**
  (`#modalOkButton`) inside `WaitForNavigationAsync`. On screen at the end: the list with only `banner`, and the
  message "Media profile deleted successfully." (`MediaProfilesController.cs:313`) in the notification area. If the
  message has already faded by then, the step is still correct: the list is what the narration describes.

## After recording

Stop the site by its process ID. Keep S0 until the video is published, then delete it.

## Checks for private data

- The admin pages show the site name "Orchard Core" and the user name `admin`. The password and the email address are
  never on screen. If the user menu shows the email address, it is `admin@example.com`.
- The photos are the Blog theme's sample photos. None shows a face; the blog post photo shows an astronaut whose face is
  hidden by the visor, which the maintainer must accept (see the photos above and OPEN-QUESTIONS.md, section 6). Look at
  every contact sheet from `review.py`: the header photos on the home page and the blog post.
- The Blog recipe's sample text (the post "Man must explore, and this is exploration at its greatest" and the
  description of the blog) is the theme's own. No real names, emails, phone numbers or tokens are typed on camera: the
  only values typed are `thumbnail`, `A thumbnail (320px x 240px, cropped)`, `320`, `80`, `336699` and `thumb`.
- No browser address bar is captured. No `localhost` port, `state.json` or `App_Data` path appears in the video.
- OCR the video at one frame per second and at every scene change, as `SKILL.md` asks, and search the text for `@`,
  digits that look like phone numbers, and long random strings. The expected hits are only the labels above.
