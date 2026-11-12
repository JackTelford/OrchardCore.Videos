# Recording plan: File Upload Security

This plan is for the person who records the clips. It names every choice, so nothing is decided while recording.
In this plan a selector stands for `scene.Page.Locator(...)`. File references are to the Orchard Core source this
video was written from (`src/...`). Anything marked "to confirm" is also listed in `OPEN-QUESTIONS.md`.

The video has four recorded clips, all in chapter 3: `03-enable`, `03-clean`, `03-test` and `03-down`. Every other
chapter is slides (`slides.py`). The site is provisioned by Auto Setup, so the Setup screen and the sign-in page never appear in a clip.

## The site it needs

- **Orchard Core**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout. It includes the ClamAV module
  because `OrchardCore.Application.Cms.Core.Targets.csproj:44` references `OrchardCore.Antivirus`.
- **Address**: `http://localhost:5010` (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an empty folder outside
  both repositories.
- **Auto Setup** (see the `orchardcore-tester` skill of the Orchard Core checkout), tenant `Default`:

  | Setting | Value |
  | --- | --- |
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

- **ClamAV connection**, set in the environment before the site starts (the feature reads it from the configuration;
  `src/OrchardCore.Modules/OrchardCore.Antivirus/Startup.cs:30`):

  ```
  OrchardCore__Antivirus__ClamAV__Host=localhost
  OrchardCore__Antivirus__ClamAV__Port=3310
  ```

  The two timeouts keep their defaults (5 and 30 seconds). Without `Host`, every scan fails
  (`ClamAvFileEventHandler.cs:164`), which is not what the clips show.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, as `references/recording.md` asks. The
  scanner uses its own TCP connection to `localhost:3310` (`ClamAvConnection.cs`), so these variables do not change
  how it connects.
- **Features**: the Blog recipe enables Media, Deployment and Remote Deployment
  (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:57-58` and `:65`). Do **not** enable the ClamAV Antivirus
  Scanner before the video: `03-enable` enables it on camera. Keep **Media TUS Uploads** disabled (its default): the
  resumable upload path stores a finished file with the plain media store call, without the scan
  (`src/OrchardCore.Modules/OrchardCore.Media/Startup.cs:712`), so with it enabled the test file would be stored
  instead of rejected.
- **Library content**: the Blog recipe puts three images in the media library root (`about-bg.jpg`, `home-bg.jpg`,
  `post-bg.jpg`, `blog.recipe.json:1124-1138`). Leave them; they are fake theme images.

### The scanner: ClamAV in Docker

The scanner is a `clamd` service in a container named `clamav`, published on port 3310 of the machine:

```
docker run -d --name clamav -p 3310:3310 clamav/clamav:stable
```

The image downloads its signatures when it starts. Wait until the container is ready before recording anything
(`docker ps` reports it as healthy, and `docker logs clamav` ends with the line that says clamd has started). The image
name, the tag and the readiness signal are to confirm. Remove the container (`docker rm -f clamav`) after the last take.

The site keeps one TCP connection to the scanner and reuses it for the next scan (`ClamAvConnection.cs:78-83`,
`ClamAvConnectionFactory.cs:31-37`). The handler sends a plain `INSTREAM` command, not inside a session
(`ClamAvConnection.cs:10`), and the clamd protocol describes clamd closing the connection after such a command. A scan
that reuses a connection that clamd has closed can fail instead of reaching a verdict (`OPEN-QUESTIONS.md`, item 7). So the plan below
starts the site afresh before every clip that scans: the first scan of a process always opens a new connection.

### The seed files (outside the capture, outside git)

Create a folder outside both repositories, for example `<temp>/ocvideo-uploads/`, with three files:

- `office-supplies.csv`:

  ```
  Item,Quantity
  Notebook,12
  Pencil,40
  Stapler,3
  ```

- `price-list.csv`:

  ```
  Item,Price
  Notebook,4.50
  Pencil,0.60
  Stapler,9.00
  ```

- `eicar-test.csv`: the standard EICAR antivirus test file, which is 68 characters of harmless text published by the
  EICAR organization. Copy the text exactly as published, with no extra lines, and save it with the name
  `eicar-test.csv`. `.csv` is in the default list of allowed extensions
  (`src/OrchardCore.Modules/OrchardCore.Media/Services/MediaOptionsConfiguration.cs:12-51`), and `.txt` and `.com` are
  not, so a file with those extensions would be refused by the extension check before any scan.
  **Never put this text in a file of either repository**: it must not be committed, and the machine's antivirus
  program may quarantine it. Keep the seed folder in a location that the antivirus program excludes, or turn real-time
  protection off for that folder only while recording, and put the protection back afterwards. Whether ClamAV detects
  the file under the name `eicar-test.csv` is to confirm.

## Order of the clips and what each one changes

Record in the order they play: `03-enable`, `03-clean`, `03-test`, `03-down`. Stop the site and start it again before
each of `03-clean`, `03-test` and `03-down`, so that the first scan of the clip is the first scan of the process (see
"The scanner" above).

| Clip | Needs | Changes the site | Before a new take |
| --- | --- | --- | --- |
| `03-enable` | The fresh site (snapshot S0), the container running | Enables the ClamAV Antivirus Scanner | Restore S0 |
| `03-clean` | The feature enabled (snapshot S1), the container running, the site just started | Adds `office-supplies.csv` to the media library root | Restore S1, start the site |
| `03-test` | `office-supplies.csv` in the library (snapshot S2), the container running, the site just started | Nothing: the upload is rejected | Stop the site, start it again |
| `03-down` | Snapshot S2, **the container stopped**, the site just started | Nothing: the upload is rejected | Stop the site, start it again |

Snapshots: copy the `App_Data` folder (and stop the site first) after each of these states, and restore the copy
before a new take, with the site stopped:

- **S0**: Auto Setup done, nothing else touched. Sign in once with `Recorder setup`.
- **S1**: after `03-enable`.
- **S2**: after `03-clean`.

The sign-in in `state.json` survives a restart, because the keys that protect the cookie are in `App_Data`.

Start the container before `03-enable`. Run `docker stop clamav` after `03-test` and before `03-down`, outside the
capture, and `docker start clamav` (and wait for it to be ready) before taking `03-clean` or `03-test` again. A snapshot
holds no scanner state; the container and the connection of the running process are the only other things that change
between clips.

### Check the scanner before the first take

Do this once, outside the capture, after snapshot S0 is saved, and restore S0 afterwards. It decides nothing in the
plan, and it answers `OPEN-QUESTIONS.md` items 2 and 7.

1. Enable the ClamAV Antivirus Scanner, start the site afresh and, in Media, Library, upload `office-supplies.csv`.
   It must be listed with no error in the "Uploads" panel.
2. Without restarting the site, upload `price-list.csv`. Write down whether it is listed or rejected, and the message.
   If it is rejected with "failed while scanning" or "returned an unexpected response", a second scan on the same
   connection fails, as item 7 describes. The clips above are already arranged for that case, and the video shows
   only the first scan of a process. Report the failure to the Orchard Core project before publishing.
3. Start the site afresh and upload `eicar-test.csv`. Write down the full message of the rejection.
4. Stop the site, restore S0 and start it again.

## Slides (chapters 1, 2, 4 and 5)

Drawn by `slides.py`; nothing to record. What each one shows:

- `pipeline-1`: "Before the file is stored", four cards (CreateAsync, CreatingAsync, Store, CreatedAsync); the first two are bright.
- `pipeline-2`: the same four cards; the last two are bright.
- `pipeline-3`: "Where the pipeline runs", four cards: Media upload API, Media recipe step, Package Import, and Skipped (Media TUS Uploads, direct media store calls).
- `clamav-1`: "What the ClamAV handler decides", three cards: Clean, Detected, Scanner problem.
- `clamav-2`: "Configure the connection": the JSON configuration, the same keys as environment variables, and three notes (Host, Port, Timeouts).
- `clamav-3`: "Rewind the stream, copy it if needed": the rewind code of the ClamAV handler, and the code that copies a stream that cannot seek to a temporary file.
- `own-1`: "1. Run the pipeline before the write": the upload service code, with the `CreateAsync` call bright.
- `own-2`: "2. Check the result, then store": the same code, with the result check, the store and `CreatedAsync` bright.
- `own-3`: "3. Register the service, use the right overload": the `TryAddTransient` registration, and the two media store calls (with and without the service) in a panel titled "IMediaFileStore calls".
- `own-4`: "4. Implement IFileEventHandler": the handler that rejects executable files.
- `own-5`: "5. Register the handler": the `TryAddEnumerable` registration, and three notes (Failed, Success, Exception).
- `temp-1`: "Who asks for a temporary path", four cards: chunked uploads, resumable uploads (with the default disk store), deployment imports, Tenants API.
- `temp-2`: "Where the files go": the `TempDirectory` configuration and three notes (Default, Per tenant, Unix-like systems).
- `temp-3`: "Use a mounted file share", three notes: Works, Does not work, Why.
- `temp-4`: "Inject ITempDirectoryProvider": an import service that uses the provider, and the three members as notes.

## Clip `03-enable`

**Starts on**: `/Admin/Features` (open it before `StartCaptureAsync`, signed in as `admin`). The page is titled
"Features", the search box is empty, and the features are listed under their category headings.

- `enable-1`: type `ClamAV` into the search box, then move the cursor to the Enable button of the matching row.
  - Actions: `scene.TypeAsync(scene.Page.Locator("#search-box"), "ClamAV")`; wait until only the "ClamAV Antivirus
    Scanner" row remains, under the heading "Security", and the summary line reads "Showing 1 of ... features"; then
    `scene.MoveToAsync(enableButton)` with `enableButton` as given below. Do not click yet.
  - On screen when the narration ends: the "Security" heading, the "ClamAV Antivirus Scanner" row with its description
    "Scans files with ClamAV before Orchard Core stores them." and the Enable button, the cursor on that button.
  - Labels: `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features),
    `Views/Admin/Features.cshtml:27` (the placeholder "Search"), `:88` (the category heading),
    `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable); the name, description and category are in
    `src/OrchardCore.Modules/OrchardCore.Antivirus/Manifest.cs`.
- `enable-2`: click Enable.
  - Actions: `scene.ClickAndWaitForNavigationAsync(enableButton)`; wait until the success notification is visible.
    The button is a link that enables the feature and redirects back to the features page.
  - On screen when the narration ends: the notification "The feature ClamAV Antivirus Scanner was enabled." at the top
    of the page. Whether the notification stays on screen for the whole step is to confirm.
  - Label: `AdminController.cs:317` of the Features module.

`enableButton` is the link named exactly "Enable" inside the row that contains the text "ClamAV Antivirus Scanner",
for example `scene.Page.Locator(".list-group-item, tr").Filter(new() { HasText = "ClamAV Antivirus Scanner" }).GetByRole(AriaRole.Link, new() { Name = "Enable", Exact = true })`.
The link also has an id that starts with `btn-enable-`; read the real value with `Recorder probe`, do not derive it.

## Choosing a file (clips `03-clean`, `03-test` and `03-down`)

The Scene API has no method for a file input, and a headless browser draws no file dialog. Click the
label with the file chooser handled by Playwright, then give it the file:

```csharp
var uploadLabel = scene.Page.Locator("label.upload-button");   // the label "Upload files"
var chooser = await scene.Page.RunAndWaitForFileChooserAsync(() => scene.ClickAsync(uploadLabel));
await chooser.SetFilesAsync(Path.Combine(seedFolder, name));   // name: "office-supplies.csv", for example
```

The input is `#fileupload` (`Assets/media-gallery/src/App.vue:93-96`); the file starts uploading as soon as it is chosen,
to `~/api/media/Upload` (`Views/Admin/Index.cshtml:20`). Name this block `ChooseFileAsync(name)` in the scenes.

## Clip `03-clean`

**Starts on**: `/Admin/Media` (Media, Library), opened before `StartCaptureAsync`, on a site that was started afresh from
snapshot S1. The root folder is selected, the three blog images are listed, and no panel is shown at the bottom right.
The scanner container is running. The clip ends after `upload-2`; then stop the site and save snapshot S2.

- `upload-1`: move to the Upload files button and choose the first file at the end of the step.
  - Actions: `scene.MoveToAsync(uploadLabel)`; `scene.HighlightAsync(uploadLabel)`; hold until about two seconds
    before the narration ends; `scene.ClearHighlightsAsync()`; `ChooseFileAsync("office-supplies.csv")`.
  - On screen when the narration ends: the library with the Upload files button (with a ring around it, or just
    cleared), and the upload starting.
  - Labels: `AdminMenu.cs:18-21` (Media, Library), `Services/MediaJSLocalizer.cs:27` (Upload files), in
    `src/OrchardCore.Modules/OrchardCore.Media`.
- `upload-2`: wait until the file is listed.
  - Actions: wait until the text `office-supplies.csv` is visible in the file list; hold. The panel "Uploads" at the
    bottom right may show a check mark for about three seconds, and disappears by itself
    (`components/UploadToast.vue`, 3000 ms); do not wait for it.
  - On screen when the narration ends: `office-supplies.csv` among the files of the library root, with no error.

## Clip `03-test`

**Starts on**: `/Admin/Media`, opened before `StartCaptureAsync`, on a site that was started afresh from snapshot S2.
The root folder lists `office-supplies.csv` and the three blog images, and no panel is shown. The scanner container is
running. This is the first scan of the process, so the connection to the scanner is new.

- `upload-3`: move to the Upload files button and choose the test file at the end of the step.
  - Actions: as `upload-1` of `03-clean`, with `ChooseFileAsync("eicar-test.csv")` at the end.
  - On screen when the narration ends: the library with `office-supplies.csv`, and the upload of the second file
    starting.
- `upload-4`: wait for the rejection, then expand the message.
  - Actions: wait until the panel header reads "Uploads" followed by "Errors: 1" in red; click the red message line
    under the file name `eicar-test.csv` so that it expands to its full text (the line is cut to one row until it is
    clicked, `Assets/media-gallery/src/assets/css/file.css:1579-1594`); hold.
  - On screen when the narration ends: the panel "Uploads" with "Errors: 1", the file name `eicar-test.csv`, and the
    full message "The uploaded file 'eicar-test.csv' was rejected because ClamAV detected '...'." with the name of the
    signature in place of the dots (to confirm). A "Validation error" warning may also appear
    (`Assets/media-gallery/src/services/UppyFileUpload.ts:530-540`; the panel error is emitted at `:551`); if it
    covers the panel, wait for it to go before the end of the step. Whether it covers the panel is to confirm.
  - Labels: `Services/MediaJSLocalizer.cs:73-74` (Uploads, Errors); the message is
    `src/OrchardCore.Modules/OrchardCore.Antivirus/ClamAV/ClamAvFileEventHandler.cs:152`; it reaches the panel through
    `Endpoints/Api/UploadMediaEndpoint.cs:162-173` (the exception message becomes the `Error` of the file).
- `upload-5`: show that the file is not in the library, then clear the error.
  - Actions: move the cursor along the file list; click the button "Clear Errors" of the panel header
    (`scene.Page.GetByRole(AriaRole.Button, new() { Name = "Clear Errors" })`) about two seconds before the narration
    ends; the panel disappears.
  - On screen when the narration ends: the library with `office-supplies.csv` and the blog images, no `eicar-test.csv`,
    and no panel.
  - Label: `Services/MediaJSLocalizer.cs:75` (Clear Errors).

## Clip `03-down`

**Starts on**: `/Admin/Media`, opened before `StartCaptureAsync`, with the container **stopped** (`docker stop clamav`,
run before the clip and outside the capture), on a site that was started afresh from snapshot S2, so that
`office-supplies.csv` is in the library and no connection to the scanner exists yet.

- `down-1`: move to the Upload files button and choose the second clean file at the end of the step.
  - Actions: as `upload-1` of `03-clean`, with `ChooseFileAsync("price-list.csv")` at the end.
  - On screen when the narration ends: the library with `office-supplies.csv`, and the upload starting.
- `down-2`: wait for the rejection, then expand the message.
  - Actions: wait until the panel header reads "Uploads" followed by "Errors: 1"; click the red message line under
    `price-list.csv` to expand it; hold. If the wait takes longer than about five seconds, put only that wait in
    `scene.OffCameraAsync(...)` with the condition "Errors: 1 is visible", so the video jumps to the result.
  - On screen when the narration ends: the panel with "Errors: 1", the file name `price-list.csv`, and a message that
    starts "The ClamAV antivirus scanner". On a new connection that is refused, it reads "The ClamAV antivirus scanner
    could not be reached while scanning 'price-list.csv'." Other failures give "timed out" or "failed" in place of
    "could not be reached", or "returned an unexpected response while scanning 'price-list.csv': " followed by what the
    scanner answered. Which one appears with a stopped container is to confirm; the narration is true for all four,
    because each names the scanner and the file.
  - Labels: `ClamAvFileEventHandler.cs:66`, `:77`, `:88` and `:156-157` (the four messages).

## Checks for private data

- Only fake data is on screen: the user `admin`, the address `admin@example.com`, the site name "Orchard Core", the file
  names of the seed folder, and the three images of the Blog recipe. No other person, company or address.
- The password never appears: the sign-in is saved in `state.json` (git-ignored) by `Recorder setup`, and the video
  never shows the sign-in page.
- No path of the recording machine is visible: the panel shows file names only. Look at every frame of `upload-4` and
  `down-2` for a folder name or a user name.
- The EICAR text and the seed files are not in either repository: `git status` shows nothing outside
  `videos/file-upload-security/`.
- Run the OCR scan of the skill (one frame per second, every scene change, then five frames per second) on the
  finished video, and search the text for names, email addresses other than `admin@example.com`, paths and tokens.
