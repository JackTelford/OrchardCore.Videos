# Recording plan: Media Uploads

Everything a person needs to record the clips of `storyboard.json` without deciding anything. Source references
(`file:line`) are relative to the `OrchardCMS/OrchardCore` checkout the site is built from. The labels were read at
commit `92db4a83`; read them again if the checkout is newer. A path that starts with `Startup.cs`, `Services/`, `Views/` or
`Assets/` is in `src/OrchardCore.Modules/OrchardCore.Media/`. In this file a selector such as `#search-box` stands for
`scene.Page.Locator("#search-box")`. The password is always `<password>`: it is chosen outside git and passed in
`VIDEO_PASSWORD`.

## The clips

| Chapter | Clip | Kind | Starts from | Changes the site |
|---|---|---|---|---|
| 01 | `01-why` | slide | no recording | no |
| 02 | `02-enable` | clip | snapshot S0 | yes: enables four features (Media Slugify, Media TUS Uploads, Media SignalR, SignalR) |
| 03 | `03-names` | slide | no recording | no |
| 03 | `03-create` | clip | snapshot S1 | yes: adds the folder `images-docs` and two files |
| 04 | `04-resume` | clip | snapshot S2 | yes: adds two large files |
| 05 | `05-live` | clip | snapshot S2 | no: a file is uploaded and deleted again |
| 06 | `06-servers` | slide | no recording | no |

Record in the order `02-enable`, `03-create`, `04-resume`, `05-live`. `04-resume` and `05-live` both need the folder
`images-docs` that `03-create` makes, and do not depend on each other: restore snapshot S2 before each of them.

Snapshots: stop the site, copy its `App_Data` folder and `state.json` aside, start the site again.

- **S0**: the fresh site with the seed below (the folder `Team Photos` and its file), the three features disabled.
- **S1**: S0 after `02-enable`.
- **S2**: S1 after `03-create`.

To record a clip again, stop the site, restore the snapshot it starts from, and start the site again. Rehearse
`02-enable` once before the real take (enabling a feature restarts the tenant, and the first request after that is
slow), then restore S0.

## The site it needs

- A local build of `src/OrchardCore.Cms.Web` from a checkout that holds the features Media Slugify, Media TUS Uploads
  and Media SignalR, running on `http://localhost:5010` with its own `ORCHARD_APP_DATA` folder outside both
  repositories, provisioned by Auto Setup (see `references/recording.md` of the skill and the `orchardcore-tester` skill
  of the OrchardCore repository).
- Auto Setup values: the **Blog** recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`,
  email `admin@example.com`, password `<password>` (`VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`).
  This video is not about the Setup screen, so the recording uses Auto Setup and `state.json` as usual.
- Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address. No external service is needed.
- Features to enable before recording: none. The Blog recipe enables **Media**
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `feature` step). Before `02-enable`, check on
  Tools, Features that **Media Slugify**, **Media TUS Uploads**, **Media SignalR** and **SignalR** are all disabled.
- Settings: leave the Media options as they are. The defaults the video relies on: a maximum file size of 30,000,000
  bytes (`src/OrchardCore.Modules/OrchardCore.Media/Services/MediaOptionsConfiguration.cs:62`), `.jpg` and `.mp4` among
  the allowed extensions (`MediaOptionsConfiguration.cs:12-54`), and a temporary file lifetime of one hour (`:76`).
- The Media Library lists 10 items per page by default (`Assets/media-gallery/src/services/LocalStorage.ts:10`): no
  folder of this video ever holds more than 10 files.

## The fake files

All files are made by the person recording, in a folder outside the repository (for example `ocvideo-media-uploads`
in the temporary folder). Nothing here is committed. The names are exact, including capital letters and spaces.

```bash
mkdir -p "$TEMP/ocvideo-media-uploads" && cd "$TEMP/ocvideo-media-uploads"
python - <<'EOF'
import os
from PIL import Image, ImageDraw

def picture(name, color, text):
    image = Image.new("RGB", (1200, 800), color)
    ImageDraw.Draw(image).text((60, 60), text, fill=(255, 255, 255))
    image.save(name, "JPEG", quality=85)

picture("Summer Party 2019.jpg", (46, 125, 90), "Summer party")
picture("The team (2020).jpg", (30, 90, 140), "The team")
picture("The Team 2020.jpg", (140, 70, 40), "Another team picture")
picture("summer-picnic.jpg", (170, 120, 40), "Summer picnic")
for name in ("retreat-video.mp4", "office-tour.mp4"):
    with open(name, "wb") as handle:
        handle.write(os.urandom(25_000_000))
EOF
```

| File | Used by | Notes |
|---|---|---|
| `Summer Party 2019.jpg` | seed | A picture. Uploaded before Media Slugify is enabled. |
| `The team (2020).jpg` | `create-4` | A picture. Its stored name becomes `the-team-2020.jpg`. |
| `The Team 2020.jpg` | `create-5` | A different picture with the same slug. |
| `retreat-video.mp4` | `resume-1` to `resume-4` | Exactly 25,000,000 bytes of random data, under the 30,000,000 byte limit. |
| `office-tour.mp4` | `resume-5`, `resume-6` | The same size. It must be a different file from the first, because the Media Library refuses a name that is already in the folder. |
| `summer-picnic.jpg` | `live-2` | A picture. |

The two `.mp4` files are not real videos. The Media Library does not look at the content of a file. Never open them
with View.

## The seed (outside the capture)

Do it once on the fresh site, before snapshot S0, in the Media Library page itself (Media, Library at `/Admin/Media`),
not with `fetch` calls: in cookie mode the Media API refuses a request without the antiforgery token
(`src/OrchardCore.Modules/OrchardCore.Media/Endpoints/Api/MediaApiAntiforgeryEndpointFilter.cs`).

1. Open `/Admin/Media`. Open the menu button of the **Files** folder (the three vertical dots, tooltip **Folder Action**),
   choose **Create a subfolder**, type `Team Photos`, select **Ok**.
2. Select the folder `Team Photos` in the folder tree, select **Upload files** and choose `Summer Party 2019.jpg`.
3. Check that the tree shows `Team Photos` and the list shows `Summer Party 2019.jpg`, with capital letters and
   spaces. Media Slugify is disabled, so the names are kept as typed.
4. Stop the site and take snapshot S0.

## Throttling the large uploads (clip `04-resume`)

On localhost a 25,000,000 byte upload ends in about a second, too fast to pause. In `04-resume` the upload speed of the
page is limited with the Chrome DevTools Protocol, before `StartCaptureAsync`, and changed during the clip:

```csharp
var cdp = await scene.Context.NewCDPSessionAsync(scene.Page);
await cdp.SendAsync("Network.enable");

Task Throttle(int bytesPerSecond) => cdp.SendAsync("Network.emulateNetworkConditions", new Dictionary<string, object>
{
    ["offline"] = false,
    ["latency"] = 0,
    ["downloadThroughput"] = -1,
    ["uploadThroughput"] = bytesPerSecond,   // 524288 = 0.5 MB/s; -1 lifts the limit
});

await Throttle(524288);   // 25,000,000 bytes take about 48 seconds
```

If the progress bar is already above 60 % when the narration of `resume-2` ends, lower the value (for example to
400000) and record again. Run `Throttle(-1)` before the clip ends, and never leave a limit on a page used for another
clip: each clip opens its own browser context.

## Uploading a file in a clip

The **Upload files** button is a label for the file input `#fileupload`
(`Assets/media-gallery/src/App.vue:93-96`; text from `Services/MediaJSLocalizer.cs:27`). To show the click and still
choose a file, click the label while waiting for the file chooser:

```csharp
var chooser = await scene.Page.RunAndWaitForFileChooserAsync(async () =>
    await scene.ClickAsync(scene.Page.Locator("label[for=fileupload]")));
await chooser.SetFilesAsync(Path.Combine(files, "The team (2020).jpg"));
```

`files` is the folder of the fake files. A file row in the list is `.file-item`
(`Assets/media-gallery/src/components/FileItemsComponent.vue:31`); the menu button of a row is its `a.action-button`.
The folder tree is `#folder-tree`; a folder row is `li.folder`, selected by clicking its `a.folder-menu-item`, and its
menu button is `.folder-actions a.action-button` (`Assets/media-gallery/src/components/FolderRow.vue`).

## Clip `02-enable`: enable the three features

Start page: `/Admin/Features` (opened with `OpenAsync`, not recorded). The search box is empty. Start from snapshot S0.

Every Enable button reloads the page and restarts the tenant. Use `ClickAndWaitForNavigationAsync`. After each reload
the search box is empty again: type the search text again before the next step needs the filtered list.

- `enable-1`: type `media` into `#search-box` (placeholder **Search**, `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:27`) with `TypeAsync(..., delay: 90)`.
  - On screen when the narration ends: the list filtered to the media features. The search matches the name, the id and the description
    of a feature (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), and the page keeps one list per category, so the
    list is longer than three rows. Read from the manifests, the category **Content Management** lists, in this order, **Media**,
    **Media Cache**, **Media SignalR**, **Media Slugify**, **Media TUS Uploads** and **Secure Media** (names from
    `OrchardCore.Media/Manifest.cs:12`, `:46`, `:57`, `:68`, `:87`, `:98`). Below it come other categories (for example Data, Hosting and Search)
    with the rows whose description mentions media. **Media** has a **Disable** button, the others an **Enable** button.
    Highlight the three rows **Media SignalR**, **Media Slugify** and **Media TUS Uploads**; if they do not fit, scroll a little
    (clear any highlight first).
- `enable-2`: click **Enable** on **Media Slugify** (`#btn-enable-OrchardCore_Media_Slugify`; label from
  `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`). Wait for the reloaded page. A message
  **The feature Media Slugify was enabled.** shows at the top of the page (`OrchardCore.Features/Controllers/AdminController.cs:317`).
  Type `media` into `#search-box` again, click **Enable** on **Media TUS Uploads** (`#btn-enable-OrchardCore_Media_Tus`),
  wait for the reload, and type `media` again.
  - On screen when the narration ends: the message **The feature Media TUS Uploads was enabled.** at the top, and the
    filtered list with a **Disable** button on **Media Slugify** and **Media TUS Uploads**, and an **Enable** button on **Media SignalR**.
- `enable-3`: type `signalr` into `#search-box` (clear it first). The list shows four rows, all with an **Enable** button:
  **Media SignalR** (category Content Management), and in the category Infrastructure **SignalR**, **SignalR Azure Backplane** and
  **SignalR Redis Backplane** (`OrchardCore.SignalR*/Manifest.cs`; the backplane modules are part of
  `OrchardCore.Application.Cms.Core.Targets`). Click **Enable** on **Media SignalR**
  (`#btn-enable-OrchardCore_Media_SignalR`), wait for the reload, and type `signalr` again.
  - On screen when the narration ends: the message **The feature Media SignalR was enabled.** at the top, and the rows
    **SignalR** (`#btn-disable-OrchardCore_SignalR`) and **Media SignalR** both with a **Disable** button, while the two backplane rows
    keep their **Enable** button. Highlight **SignalR** and **Media SignalR**. The message names
    only the feature that was asked for (`src/OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:74-90`, `EnableOrDisableFeaturesAsync`), so it does not mention SignalR.
- `enable-4`: open **Settings**, then **Media**, then **Options** in the admin menu (`OrchardCore.Media/AdminMenu.cs:34-36`)
  with `ClickAndWaitForNavigationAsync` on the **Options** link. If the menu is collapsed or animates badly, use
  `NavigateAsync("/Admin/Media/Options")` instead. The page title is **Media options** (`Views/Admin/Options.cshtml:13`).
  In the first half of the narration, show **Temporary file lifetime** (`Options.cshtml:85`; scroll down a little if it is
  not on screen) and highlight its row. Clear the highlight, then `ScrollAsync(0, 2000)` to the bottom.
  - On screen when the narration ends: the bottom of the page with the rows **SignalR** (`Options.cshtml:155`) and
    **TUS uploads** (`Options.cshtml:163`), each with a green check mark.

After the clip, stop the site and take snapshot S1.

## Clip `03-names`: slides

- `names-1`: the slide "From the name you type to the stored name": the cards "What you type" and "What Media Slugify stores" are bright.
- `names-2`: the same slide: the cards "The public address" and "Left as they are" are bright.

## Clip `03-create`: slugified names

Start page: `/Admin/Media`, from snapshot S1. Before `StartCaptureAsync`: select the folder `Team Photos` in the tree and wait
until the list shows the row `Summer Party 2019.jpg`.

- `create-1`: highlight the folder `Team Photos` in the tree, then the row `Summer Party 2019.jpg`.
  - On screen when the narration ends: the tree with **Files** and `Team Photos`, the folder `Team Photos` selected, and the list
    with `Summer Party 2019.jpg` (capital letters and spaces).
- `create-2`: click the menu button of the root folder row **Files** (`#folder-tree .treeroot .folder-actions a.action-button`;
  tooltip **Folder Action**, `Services/MediaJSLocalizer.cs:62`, `FolderMenu.vue:4`). The menu opens with one item, **Create a
  subfolder** (`FolderMenu.vue:68`; text from `MediaJSLocalizer.cs:64`). The root folder has no Delete item.
  - On screen when the narration ends: the open menu with **Create a subfolder**.
- `create-3`: click **Create a subfolder**. A dialog titled **Create folder** (`MediaJSLocalizer.cs:65`) opens with the path `/`
  in bold and a text box with the placeholder **Enter a folder name** (`ModalFolderAction.vue:14`). Type `Images & docs`
  with `TypeAsync(scene.Page.Locator("input[name=create-folder]"), "Images & docs")`, then click `#btn-submit` (label **Ok**,
  `FolderActionModalService.ts:44`).
  - On screen when the narration ends: the dialog is closed, and the tree lists the folder `images-docs` (lowercase, a dash)
    between **Files** and `Team Photos`. Highlight the `images-docs` row. The tree shows the name the server returned
    (`Assets/media-gallery/src/services/FileLibraryManager.ts:151`).
- `create-4`: click the folder `images-docs` in the tree. The list shows **This folder is empty**
  (`Services/MediaJSLocalizer.cs:32`). Click **Upload files**, choose `The team (2020).jpg`, and wait until the list has a row
  named `the-team-2020.jpg`.
  - On screen when the narration ends: the row `the-team-2020.jpg` in the list of `images-docs`. The Uploads panel at the
    bottom right shows the original name `The team (2020).jpg` while the file is sent and disappears three seconds after
    the upload ends (`UploadToast.vue:135`), so the list row, not the panel, is what the frame must show. Highlight the row.
- `create-5`: click **Upload files**, choose `The Team 2020.jpg`, and wait until the list has a row named `the-team-2020-1.jpg`.
  - On screen when the narration ends: two rows in the list, `the-team-2020.jpg` and `the-team-2020-1.jpg`. Highlight the second.
  - The second name comes from `Startup.cs:731` (`GetAvailableMediaFilePathAsync`), which adds `-1` when Media TUS Uploads is
    enabled. If an error such as **A file with this name already exists in the current folder.** shows instead, the upload
    did not go through the TUS endpoint: see `OPEN-QUESTIONS.md`.

After the clip, stop the site and take snapshot S2.

## Clip `04-resume`: pause, resume and recover

Start page: `/Admin/Media`, from snapshot S2. Before `StartCaptureAsync`: select `images-docs` in the tree, wait until the list shows
its two rows, create the CDP session and call `Throttle(524288)` (see above).

- `resume-1`: click **Upload files**, choose `retreat-video.mp4`.
  - On screen when the narration ends: the Uploads panel at the bottom right (title **Uploads**, `UploadToast.vue:9`) with the row
    `retreat-video.mp4` and a progress bar that is moving, with the speed next to it (about `512 KB/s`); the list still shows two rows.
- `resume-2`: highlight the Uploads panel (`.upload-toast-container`) with no caption. Do nothing else.
  - On screen when the narration ends: the progress bar of `retreat-video.mp4` between 20 % and 60 %, and the speed text. Clear the highlight.
- `resume-3`: click the pause button of the row, the first button inside `.upload-toast-item` (icon: pause bars; its tooltip **Pause upload**
  is not drawn in the capture, `UploadToast.vue:32-34`, `MediaJSLocalizer.cs:77`). Wait until the icon is a play triangle.
  - On screen when the narration ends: the progress bar stopped at the same width, drawn in the paused style (`is-paused`), the speed text gone,
    and the play triangle where the pause bars were.
- `resume-4`: call `Throttle(-1)`, then click the same button (tooltip **Resume upload**, `MediaJSLocalizer.cs:78`). The bar moves on from
  where it stopped. Wait, off camera if it takes longer than the narration (`OffCameraAsync`, condition: a `.file-item` row with the text
  `retreat-video.mp4` is visible), until the file is in the list.
  - On screen when the narration ends: the list of `images-docs` with a third row `retreat-video.mp4` (its size is in the Size column). The Uploads panel may already be gone.
- `resume-5`: call `Throttle(524288)`, click **Upload files**, choose `office-tour.mp4`. Wait (condition: the style width of
  `.upload-toast-progress-bar` is at least 20 %, about 10 seconds at 524288 bytes per second, which is close to the length of the narration; a
  higher value makes the step much longer than its narration). Then `await scene.NavigateAsync("/Admin/Media")` to reload the page. The Media Library opens
  on the folder it showed before, because it keeps the selected folder in the browser's local storage
  (`Assets/media-gallery/src/services/LocalStorage.ts:42-52`); if it opens on the root, click `images-docs`. Call `Throttle(524288)` again if
  the limit was lost with the reload.
  - On screen when the narration ends: the reloaded page on `images-docs`, no Uploads panel, and no row `office-tour.mp4` in the list.
  - Reload and select the file again within an hour of the last part the server received: the expiration of an upload is sliding and lasts the
    temporary file lifetime (`Startup.cs:589-591`, `Services/DistributedMediaTusStore.cs:251-257`). Nothing in the source removes the expired
    data, so do not narrate what happens to the file after that hour.
- `resume-6`: click **Upload files**, choose `office-tour.mp4` again. The Uploads panel shows the row with a bar that starts above 0 %, where
  the first attempt stopped. Hold three seconds so that the start is visible, call `Throttle(-1)`, and wait (off camera if needed,
  condition: a `.file-item` row with the text `office-tour.mp4` is visible).
  - On screen when the narration ends: the list of `images-docs` with the rows `retreat-video.mp4` and `office-tour.mp4` among the others.
  - If the bar starts at 0 %, the upload did not resume: record the clip again, from snapshot S2. Do not edit around it.

## Clip `05-live`: live updates

Start page: `/Admin/Media`, from snapshot S2. Two pages of one browser context are used; only the first is recorded until a step says otherwise.
Before `StartCaptureAsync`:

1. Window A is `scene.Page`. Keep it in a variable `pageA`. Open `/Admin/Media`, select `images-docs`, wait for its two rows.
2. Window B: `var pageB = await scene.Context.NewPageAsync();` Open `BaseUrl + "/Admin/Media"` in it, wait for the network to be idle,
   select `images-docs` in its tree, and wait for the two rows. Both windows now hold an open connection to `/hubs/media`
   (`Startup.cs:776`, `Assets/media-gallery/src/services/SignalR.ts`). Wait two seconds more so that both have subscribed to the folder.
3. `await pageA.BringToFrontAsync();` then `StartCaptureAsync()`.

- `live-1`: highlight the breadcrumb of window A (`.breadcrumb-path`, it shows **Files** and `images-docs`) with no caption.
  - On screen when the narration ends: window A, the folder `images-docs` selected, two file rows.
- `live-2`: `await scene.SwitchToAsync(pageB);` click **Upload files** in window B, choose `summer-picnic.jpg`, and wait until its row
  `summer-picnic.jpg` is in the list of window B.
  - On screen when the narration ends: window B with the row `summer-picnic.jpg` among the files of `images-docs`.
- `live-3`: `await scene.SwitchToAsync(pageA);`. Do not reload. Wait until the row `summer-picnic.jpg` is visible in window A (condition, up to
  10 seconds). Highlight the row.
  - On screen when the narration ends: window A with three rows, including `summer-picnic.jpg`, which it received without a reload.
  - If the row does not show up, the connection to the hub is not open: check the console for `SignalR connected to MediaHub`, and check that
    Media SignalR is enabled.
- `live-4`: with the locators of `pageB` (it is not recorded now), open the menu button of the row `summer-picnic.jpg`, choose **Delete**
  (`FileMenu.vue:98`), and select **Ok** in the dialog **Delete file** that asks **Are you sure you want to delete this file?**
  (`MediaJSLocalizer.cs:50-51`). Window A is the recorded page. Click with Playwright directly on window B, for example
  `await pageB.Locator(...).ClickAsync()`: `scene.ClickAsync` moves the cursor and clicks on `scene.Page`, which is window A. The row of
  window A disappears by itself once the hub announces the deletion (`Hubs/MediaSignalREventHandler.cs:15-20`).
  - On screen when the narration ends: window A with the two original rows and no `summer-picnic.jpg`.
- `live-5`: highlight the breadcrumb of window A again, with no caption.
  - On screen when the narration ends: window A on `images-docs`, with the two original rows.

Close window B (`pageB.CloseAsync()`) after the clip.

## Clip `06-servers`: slides

- `servers-1`: the slide "Running on several servers": the card "Resumable uploads" is bright.
- `servers-2`: the same slide: the card "Live updates" is bright.

## Clip `01-why`: slides

- `why-1`: the slide "What each feature is for": the card "Media Slugify" is bright.
- `why-2`: the same slide: the card "Media TUS Uploads" is bright.
- `why-3`: the same slide: the card "Media SignalR" is bright.

## Checks for private data

- Every name on screen is one of the fake names above, or `admin`; every email is `admin@example.com`.
- The admin pages show no path of this machine: check the Media options page (`Assets path` is the relative `Media`), the
  notifications, and the Uploads panel.
- Look at every contact sheet (`review.py`): there must be no error toast, no empty list where the narration talks about rows, and no
  `Unauthorized` notification.
- OCR the finished video once per second and at every scene change, and search for `@` addresses other than `@example.com`, for real
  names, and for the password.
- The `state.json`, the fake files and the snapshots stay outside the repository.
