# Open questions: Response Compression

No Orchard Core site was run for this script. It was written from the Orchard Core source at commit `92db4a83`, and the
defaults of the ASP.NET Core response compression were read by building the services that `AddResponseCompression`
registers in a throw-away console project outside both repositories, against the .NET 11.0.0 shared framework (the
target framework of the source: `src/OrchardCore.Build/TargetFrameworks.props`). A person confirms each point below on a
running site before the video is recorded, and changes the narration if a check fails. File references are relative to
`src/OrchardCore.Modules/` unless they start with `src/`.

## 1. Which formats and content types the module handles

- **Checked:** `OrchardCore.ResponseCompression/Startup.cs:19` calls `AddResponseCompression` and lists no providers, so the
  defaults of ASP.NET Core apply. On the .NET 11.0.0 framework the default providers, in order of priority, are
  Zstandard (`zstd`), Brotli (`br`) and gzip (`gzip`). With the options of `Startup.cs`: `Accept-Encoding: gzip` selects
  gzip, `br` selects Brotli, `zstd` selects Zstandard; `gzip, deflate, br, zstd` selects Zstandard and `gzip, br` selects
  Brotli (the order of the providers decides when the weights are equal; `gzip;q=0.5, br` selects Brotli); a header with
  only `identity` or `deflate` selects nothing; and a request without the header is not compressed.
- The default content types are `text/plain`, `text/css`, `application/javascript`, `text/javascript`, `text/html`,
  `application/xml`, `text/xml`, `application/json`, `text/json` and `application/wasm`. `image/png`, `image/svg+xml` and
  `font/woff2` are not compressed.
- **The narration** says the defaults "include Zstandard, Brotli and gzip" (`code-1`), "include" the list above (`code-3`;
  it names WebAssembly for `application/wasm`), and that a client that lists several formats without weights gets the
  first of Zstandard, Brotli, gzip (`check-2`). It never says "only".
- **To confirm on the recording site:** with the feature enabled, the two commands of the `check-1` slide return
  `Content-Encoding: gzip` and `Content-Encoding: br`, and a request with `Accept-Encoding: gzip, deflate, br, zstd`
  returns `Content-Encoding: zstd`.

## 2. The `check-1` slide

- The slide shows two `curl` commands and two cards that name the headers each should return. The commands were not run.
- **To confirm:** the address `https://localhost:5001/` (the Kestrel profile of `src/OrchardCore.Cms.Web/Properties/launchSettings.json`),
  that `--insecure` is needed for the development certificate, and that the response headers contain the two lines named
  on the cards. Over HTTP/2, `curl` prints header names in lowercase; the cards use the spelling of the documentation.
- `/dev/null` is for a POSIX shell. The narration does not mention the commands' options.

## 3. What the feature compresses and what it does not

- **Checked, files of the host:** `src/OrchardCore.Cms.Web/Program.cs:26-28` and `src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Web/Program.cs`
  call `app.UseStaticFiles()` before `app.UseOrchardCore()`, so a file of the host's `wwwroot` folder is answered before
  the tenant pipeline runs. The narration (`code-4`) says so.
- **Checked, files of modules and themes:** the tenant adds its own static file middleware (`src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs`,
  `AddStaticFiles`, the `builder.Configure(...)` call without an order). That order is 0 (`src/OrchardCore/OrchardCore.Abstractions/Modules/Builder/OrchardCoreBuilder.cs:63`,
  carried by `StartupActionsStartup.Order`), and the tenant pipeline runs the startups in ascending `ConfigureOrder`
  (`src/OrchardCore/OrchardCore/Modules/Extensions/ShellPipelineExtensions.cs:80`), so this module, at -5, is ahead of that
  middleware. The options keep `HttpsCompression` of the host (`ServiceCollectionExtensions.cs`, in `AddStaticFiles`),
  and the framework's default is `Compress`. The narration (`code-4`) says the files of modules and themes are served
  inside the pipeline, so the middleware can compress them (when their content type is on the list).
- **To confirm:** a file of the host's `wwwroot` has no `Content-Encoding` header when the feature is enabled, and a
  stylesheet or a script of a module (for example one linked from an admin page) has one when the request accepts gzip.
- Not narrated: the tenant's static file provider also serves the web-root files of the application through the
  application's module prefix (`AddStaticFiles`, `ApplicationStaticFileProvider`), which are inside the pipeline.

## 4. The starting state, the locators and the toast of `02-enable`

- **Checked:** no recipe in the source mentions `OrchardCore.ResponseCompression`, and the manifest has no always-enabled
  flag, so the feature is off after Auto Setup with the Blog recipe. **To confirm** on the first take.
- The admin menu was read from `OrchardCore.Features/AdminMenu.cs:26-27` (`Tools`, `Features`), not from the rendered
  menu of the admin theme. The recorder probes `/Admin` and follows the real structure.
- The admin lists keep their defaults (the `List` layout, the actions as `Buttons`: `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`);
  the Blog recipe sets neither. With the `Menu` layout the Enable and Disable links sit inside an Actions drop-down.
- The Enable link is selected by the row (`data-filter-value`, set at `OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`)
  and the `btn-primary` class. Its id comes from `Html.GenerateIdFromName`, which was not run.
- The confirmation text `The feature Response Compression was enabled.` comes from
  `OrchardCore.Features/Controllers/AdminController.cs:317`. It is a toast that the admin theme hides itself 5 seconds
  after the page loads (`src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml`, `src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`).
  **To confirm:** it is on screen when `enable-3` starts and for at least the first 3 seconds of that step; the plan clears
  its highlight after 2.5 seconds.
- The sentence "The tenant is rebuilt on its next request" (`enable-3`) comes from the comment at
  `src/OrchardCore/OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs:133` and from `ShellHost.ChangedAsync`
  (`src/OrchardCore/OrchardCore/Shell/ShellHost.cs:174-175`), which releases the shell so that a new one is built for
  the next request. **To confirm:** a request sent right after enabling the feature already returns a compressed response.

## 5. Pronunciation

The voice's reading of these was not tested; check them with `scripts/pronounce.py` before narrating:

- `gzip` (the `spoken` entry says "gee zip"), `Nginx` ("Engine X"), `BREACH` ("Breach"), `wwwroot` ("web root"),
  `Zstandard` ("Zee standard"), and the identifiers `ConfigureServices`, `AddResponseCompression`, `UseResponseCompression`,
  `EnableForHttps`, `UseStaticFiles` and `UseOrchardCore`.
- `Brotli` has no `spoken` entry. `br` (in `check-1`) is spelled out through the `spoken` entries "Accept-Encoding br" and "Content-Encoding br" (both read "B.R."); confirm the voice says "B R" and that the entries match every occurrence. `WebAssembly` (in `code-3`) has none either.

## 6. The description on the Features page

The row of `enable-1` shows the manifest's description, "The Response Compression module allows to enable the gzip
compression." (`OrchardCore.ResponseCompression/Manifest.cs:8`). The narration does not read it. The module adds no
provider, so the defaults of the framework apply, and the sentence names only gzip. The documentation page of the module
does the same (see the documentation issues reported with this video).
