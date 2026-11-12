# Recording plan: docker-orchard-core

A developer video. Five of its six content chapters are slides, drawn by `slides.py`; the terminal commands, the
Dockerfile and the Compose file are slides, never recordings. Only chapter 04 (`04-first-request`) is recorded: the
Setup screen of a container, in a browser. The plan is written for a person who decides nothing: every value is below.

References to the Dockerfile, `.dockerignore`, `Dockerfile-CI` and the workflows are to files at the root of
OrchardCMS/OrchardCore at commit `92db4a837ddad07f4b35179df59ac78375017860` (written `Dockerfile:N`). The folder the
content was written from held only `src` and `mkdocs.yml`, so those files were read from the repository at that
commit. File references to `src/...` are to the same commit.

## The site it needs

The subject is a container that has never served a request, so this video is recorded **without Auto Setup, without
`state.json`, and without signing in**. There is no `Recorder setup`, no `PrepareAsync`, no mask script and no copy of
an `App_Data` folder: the site is installed on camera, in a container started from the image.

| | |
|---|---|
| Orchard Core | A checkout of OrchardCMS/OrchardCore at commit `92db4a837ddad07f4b35179df59ac78375017860` (the commit the content was written against). Do not use the image from Docker Hub for this recording: its Setup screen may differ from the source this plan cites. |
| Docker | Docker Desktop or Docker Engine, with Linux containers. Nothing else is installed. |
| Image | `oc`, built from the `Dockerfile` at the root of the checkout. |
| Container | `oc-video`, port 8080 of the machine mapped to port 80 of the container. |
| Auto Setup | None. No `OrchardCore__AutoSetup__*` variable is set in the shell, in Docker Desktop, or on the container, so the Setup screen is shown (`src/OrchardCore.Modules/OrchardCore.AutoSetup/Startup.cs:95-98` returns when the configuration section does not exist). |
| Tenant | `Default`, installed during the clip. |
| Time zone | Chosen on camera: `America/Los_Angeles`. |
| Features to enable | None. The Blog recipe enables what it needs. |
| Settings | None. |
| External services | None. No mail, SMS or payment provider is configured in a new container, so nothing needs a local stand-in. The Blog theme asks Google Fonts for fonts from the browser (`src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:12-13`); the page renders without them if the network is blocked. |
| Seed data | None, apart from the values typed on camera (below). |

Start the container outside the capture:

```bash
git clone https://github.com/OrchardCMS/OrchardCore.git orchardcore-docker
cd orchardcore-docker
git checkout 92db4a837ddad07f4b35179df59ac78375017860
docker build --platform=linux/amd64 -t oc .
docker run --name oc-video -p 8080:80 oc
```

`docker run` stays attached to its terminal: run the next command in a second terminal (or add `-d` to `docker run`).
Wait until `curl -s http://localhost:8080/` answers with the Setup page. Request the page once with `curl` before the
capture, so the first request on camera is not slow. That `GET` does not install the site: it only makes Orchard Core
create the `Default` tenant as uninitialized and save it to `tenants.json` in `App_Data`
(`src/OrchardCore/OrchardCore/Shell/ShellHost.cs:428-438`, `src/OrchardCore/OrchardCore/Shell/ShellSettingsManager.cs:132-173`), which is the state every take starts from anyway.

The values typed on camera, all fake:

| Field (label as on screen) | Value |
|---|---|
| What is the name of your site? | `Docker Demo` |
| Recipe | Blog |
| Default Time Zone | `America/Los_Angeles` |
| What type of database to use? | Sqlite (the default, left as it is) |
| User Name | `admin` |
| Email | `admin@example.com` |
| Password, Password Confirmation | the value of `VIDEO_PASSWORD`, chosen outside git; written `<password>` in this plan. It must satisfy the rules the Setup screen shows in the Password field's tooltip |

## Order of the clips and the state they leave

One clip needs recording, `04-first-request`. It **changes the site**: it installs it. The snapshot is the image itself.
Before every take, remove the container and start a new one from the same image, then request the page once with
`curl` again, as above:

```bash
docker rm -f oc-video
docker run --name oc-video -p 8080:80 oc
```

When the last take is accepted, remove the container and the image (`docker rm -f oc-video`, `docker image rm oc`). The
recording uses no volume; if the volume `orchard-data` was created to check `data-2` (`OPEN-QUESTIONS.md`), remove it
too (`docker volume rm orchard-data`).

The recorder is started with `VIDEO_ROOT=<repo>/videos/docker-orchard-core`, `VIDEO_BASE_URL=http://localhost:8080` and
`VIDEO_PASSWORD=<password>`; no `state.json` exists, so the scene opens an unsigned browser.

## Slide steps

Each slide is drawn by `slides.py`; a person checks them by rendering `slide()` in memory, never by saving an image.

- `need-1`: two cards, Windows (containers on the Windows Subsystem for Linux) and Ubuntu and other Linux systems (Docker Engine). From `src/docs/topics/docker/README.md`, "What you will need".
- `idea-1`: two cards, the published image `orchardproject/orchardcore-cms-linux` and an image built from the Dockerfile.
- `idea-2`: the `docker run` command of `src/docs/getting-started/test-drive-orchard-core.md` in a terminal panel, and a card for `--name` and one for `-p`.
- `dockerfile-1`: the build stage of `Dockerfile:1-15` (`Dockerfile:14` continued on two lines): the SDK image, `LABEL stage=build-env`, the three `COPY` lines, and `dotnet publish` of the CMS web project.
- `dockerfile-2`: the runtime stages of `Dockerfile:16-25`: the Windows and Linux runtime images chosen by `TARGETOS`, `EXPOSE 80`, `ENV ASPNETCORE_URLS=http://+:80`, `WORKDIR /app`, `COPY --from=build-env`, `ENTRYPOINT`.
- `dockerfile-3`: the whole `.dockerignore` (`.dockerignore:1-6`), with two cards, site data and build output.
- `dockerfile-4`: two cards, copying the Dockerfile and `.dockerignore` to the root of a solution, and adjusting the paths of the build stage (`src/docs/topics/docker/README.md`, the paragraph about a NuGet package solution).
- `build-1`: the terminal panel with three commands; the `docker build` lines are bright, with a card for `--platform` and one for `-t`. The `--platform` card cites the comment on `Dockerfile:1`.
- `build-2`: the same panel; the `docker run -p 8080:80 oc` lines are bright, with a card for `-p`.
- `build-3`: the same panel; the `docker image prune` lines are bright, with a card for the `label=stage=build-env` filter (`Dockerfile:4`; `src/docs/topics/docker/README.md`, "Prune intermediate images").
- `data-1`: three cards, `/app/App_Data/tenants.json`, `/app/App_Data/Sites/Default/OrchardCore.db` and `/app/App_Data/Sites/Default/DataProtection-Keys` (`src/OrchardCore/OrchardCore/Shell/ShellOptionsSetup.cs:20-28`; Setup stores the file name `OrchardCore.db` in the `DatabaseName` setting of a Sqlite tenant, `src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:178-181`, and the file is `App_Data/Sites/<tenant>/<DatabaseName>`, `src/OrchardCore/OrchardCore.Data.YesSql/SqliteHelper.cs:21` and `:51-52`; the name `yessql.db` is only the fallback for a tenant with no `DatabaseName`, `SqliteHelper.cs:23-27`; the keys are in `App_Data/Sites/<tenant>/DataProtection-Keys`, `src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:563-566`; the file name `tenants.json`, `src/OrchardCore/OrchardCore.Abstractions/OrchardCoreConstants.cs:9`).
- `data-2`: the `docker run -v orchard-data:/app/App_Data` command and a card for `-v`.
- `data-3`: five of the Auto Setup environment variables of `src/docs/reference/modules/AutoSetup/README.md` (`ShellName`, `SiteName`, `SiteTimeZone`, `AdminUsername`, `AdminEmail`; the section is `AutoSetup`, `src/OrchardCore.Modules/OrchardCore.AutoSetup/Startup.cs:27`), then a line `# ...`, and two cards (the double underscore, and Auto Setup installing the site on the first request).
- `compose-1`: the Compose file (19 lines) with every line bright and two cards, `web` and `postgresql`.
- `compose-2`: the same file with the `web` service lines (1 to 9) bright, and three cards (`build`, `ports`, `depends_on`).
- `compose-3`: the same file with the `postgresql` service and the volume lines (10 to 19) bright, and two cards (`environment`, `POSTGRES_PASSWORD`). The volume is mounted at `/var/lib/postgresql`, the data volume of the `postgres:latest` image from PostgreSQL 18 on (the documentation of the PostgreSQL image on Docker Hub); the Orchard Core documentation mounts `/var/lib/postgresql/data`, the path of PostgreSQL 17 and below (`OPEN-QUESTIONS.md`, item 8).
- `compose-4`: the shell commands (`export`, `docker-compose build`, `docker-compose up`) and a card telling to open the mapped port.
- `compose-5`: the connection string in a panel, and two cards naming the labels of the Setup screen: "What type of database to use?" and "Connection String" (`src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:176` and `:198`).

## Clip 04-first-request

Starts on: `/` of `http://localhost:8080`, the Setup screen of the new container, opened with `scene.OpenAsync("/")`
before `StartCaptureAsync()`. Nothing on the page is changed before the capture. Also before `StartCaptureAsync()`, call
`scene.Page.SetDefaultTimeout(180_000)`: the form's POST of `first-4` returns only when the setup has finished
(`SetupController.cs:187` awaits `SetupAsync`, `:209` redirects), and a Blog setup in a container that has just started
can take longer than the 20 seconds that the scene allows by default (`Scene.cs`, `StartAsync`).

Steps, in the storyboard's order. Selectors stand for `scene.Page.Locator(...)`; every label is the one on screen. Unless a path is given, `Index.cshtml` is `src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml` and `Layout.liquid` is `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`.

- `first-1`: the page is the Setup screen; no input yet. Actions: `HighlightAsync("h1", ...)` around the heading "Setup" (`Index.cshtml:102`), then clear the highlight. On screen when the narration ends: the heading "Setup" and the line "Please answer a few questions to configure your site." (`Index.cshtml:103`), with the form below, empty.
- `first-2`: Actions, in this order, so they follow the narration:
  1. `HighlightAsync("#DatabaseProvider")` around the select "What type of database to use?" (`Index.cshtml:176`), which shows "Sqlite"; then `ClearHighlightsAsync()`.
  2. `TypeAsync("#SiteName", "Docker Demo")` in "What is the name of your site?" (`Index.cshtml:118`).
  3. `ClickAsync("#recipeButton")`, then `ClickAsync("#recipes a.dropdown-item[data-recipe-name='Blog']")`. The button reads "Software as a Service" until then (`src/OrchardCore.Themes/TheTheme/Recipes/saas.recipe.json:3`, the recipe tagged "default"); the item reads "Blog" (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:3`). The label "Recipe" is at `Index.cshtml:130`.
  4. `SelectAsync("#SiteTimeZone", "America/Los_Angeles")` in "Default Time Zone" (`Index.cshtml:159`).

  On screen when the narration ends: site name "Docker Demo", the recipe button reading "Blog", the time zone set, the database type "Sqlite" and no Connection String field (it is hidden for Sqlite: `setup.ts`, `toggleConnectionStringAndPrefix`).
- `first-3`: Actions: `TypeAsync("#UserName", "admin")` in "User Name" (`Index.cshtml:222`), `TypeAsync("#Email", "admin@example.com")` in "Email" (`:227`), `TypeAsync("#Password", <password>)` in "Password" (`:234`), `TypeAsync("#PasswordConfirmation", <password>)` in "Password Confirmation" (`:244`), then `MoveToAsync("#SubmitButton")` to rest the pointer on the button "Finish Setup" (`#SubmitButton`, `:256`). The step does **not** click: the click is the first action of `first-4`, so that the frame at the end of this step shows what its narration describes. The two password fields are masked: never click `#togglePassword` or `#togglePasswordConfirmation`. On screen when the narration ends: the Super User fields filled (the password fields show dots) and the pointer on the button "Finish Setup".
- `first-4`: Actions: one call that cuts the whole wait off camera, with the click inside it, then the highlight:

  ```csharp
  await scene.OffCameraAsync(async () =>
  {
      await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#SubmitButton"));
      await scene.Page.Locator("#mainNav .navbar-brand").WaitForAsync();
  });
  await scene.HighlightAsync(scene.Page.Locator("#mainNav .navbar-brand"));
  ```

  The condition `#mainNav .navbar-brand` is visible is met only by the finished state: the Setup page has no such element, and the home page of the Blog has (`Layout.liquid:25` and `:27`). The click itself falls inside the cut: the viewer sees the filled form with the pointer on "Finish Setup" for a moment, then the home page. Then hold. On screen when the narration ends: the home page of the Blog, with the masthead and the navigation bar showing "Docker Demo" (`Layout.liquid:27`, `{{ Site.SiteName }}`), the name ringed. If an error message shows on the Setup page instead (for example a rejected password, or `An error occurred while running the setup`, `SetupController.cs:192`), the take is discarded.

## Checks for private data

- Only fake values are typed: `Docker Demo`, `admin`, `admin@example.com` and the password. The browser's address bar is not recorded.
- The two password fields show dots; the Connection String field is never shown (Sqlite).
- Search the OCR of the clip for the password and for any real name or address. The container's name, the machine's name and the path of the checkout never appear in a browser frame.
- The slides show only `example.com` addresses, the placeholder `<password>`, and names from the Orchard Core source and documentation; they hold no machine name and no real credential.
