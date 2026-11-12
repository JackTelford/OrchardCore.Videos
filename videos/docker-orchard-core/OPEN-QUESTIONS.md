# Open questions: docker-orchard-core

What was checked in the source and the documentation, and what a person must confirm on a running machine before the
video is published. The narration states only what was checked; each item below says what remains.

## Where the facts come from

The folder the content was written from held `src` and `mkdocs.yml` only. The files that this video is about sit at the
root of the repository, so they were read from OrchardCMS/OrchardCore at commit
`92db4a837ddad07f4b35179df59ac78375017860`: `Dockerfile`, `.dockerignore`, `Dockerfile-CI`, `Dockerfile-CI.dockerignore`,
`global.json`, and `.github/workflows/preview_ci.yml` and `release_ci.yml` (the steps that push the published images).
The slides show the `Dockerfile` and `.dockerignore` as they are at that commit. A reviewer without network access
cannot compare them to the local folder.

The documentation page (`src/docs/topics/docker/README.md`) shows an older Dockerfile and an older `.dockerignore`.
The video follows the files in the repository; the differences are listed as documentation issues in the result of the
content session.

## To confirm on a running machine

1. **Image tags on the slides.** `dockerfile-1` and `dockerfile-2` show the real `FROM` lines and `--framework`, which
   carry the version of .NET the repository targets. The narration never names a version, but the slides will show an
   outdated number once the repository moves on. A maintainer decides whether the slides keep the real tags or elide
   them (see the specification questions in the result).
2. **`data-1`: the folder in the container.** The files are at `/app/App_Data` because the working directory is `/app`
   (`Dockerfile:23`) and the content root is the working directory (`ShellOptionsSetup.cs:20-28`). After the clip
   `04-first-request`, run `docker exec oc-video ls -R /app/App_Data` and confirm `tenants.json`,
   `Sites/Default/OrchardCore.db` and `Sites/Default/DataProtection-Keys`. The code names the database file
   `OrchardCore.db` when Setup installs a Sqlite tenant (`SetupService.cs:178-181`); `yessql.db` is only the fallback for
   a tenant with no `DatabaseName` setting (`SqliteHelper.cs:23-27`), and the documentation does not name the file.
3. **`data-2`: the volume.** Confirm that a container started with `-v orchard-data:/app/App_Data` keeps the site after
   `docker rm -f`, and that a second container started with the same volume shows the home page of the site instead of
   the Setup screen. Docker's own behavior was not read from the documentation of this repository.
4. **`build-1`: the platform option.** The slide shows `docker build --platform=linux/amd64 -t oc .`, because the
   comment on `Dockerfile:1` says `TARGETOS` is set when `--platform` is provided and `Dockerfile:19` uses it. The
   documentation shows `docker build -t oc .`. Confirm whether the plain command builds with the builder that is the
   default of Docker today, and what a machine with an Arm processor should pass. The Compose file of `compose-1` builds
   the same Dockerfile with no platform setting: confirm that `docker-compose build` reaches the `build_linux` stage
   (`Dockerfile:18-19`).
5. **`build-3`: pruning.** The documentation prunes with `docker image prune -f --filter label=stage=build-env`, and the
   label is on `Dockerfile:4`. Confirm that the command removes something after a build with the default builder. The
   narration only says the documentation prunes with a filter on the label; if the command removes nothing, the step
   can be cut.
6. **`idea-2`: the published image.** The command is the one of `src/docs/getting-started/test-drive-orchard-core.md`.
   The workflows push `orchardproject/orchardcore-cms-linux` (`release_ci.yml:126`, `preview_ci.yml:149`). Confirm that the
   image exists on Docker Hub, starts with the command on the slide, and answers with the Setup screen.
7. **`data-3`: Auto Setup in a container.** The variable names are from `src/docs/reference/modules/AutoSetup/README.md`,
   and the slide shows five of them. Confirm that a container started with the full set (including the password, the
   database provider and the recipe) skips the Setup screen. A separate video covers Auto Setup.
8. **`compose-1` to `compose-5`: the Compose file.** The file is the `web` and `postgresql` services of the
   documentation, with the password read from `${POSTGRES_PASSWORD}` so that no password is written in the file. Run it
   once, outside any capture, and confirm: Compose fills the variable from the shell; `docker-compose build` and
   `docker-compose up` are the commands of the documentation (confirm whether the machine needs `docker compose`
   instead, and change the slide and the spoken entry together); the web container reaches the database at the server
   name `postgresql`; `docker-compose build` builds the Linux runtime stage without a platform setting; and the Setup screen accepts the connection string of the slide with "Postgres" as the type of
   database. Also check the `postgres:latest` image on the day of the recording: the documentation of the PostgreSQL
   image (docker-library/docs, `postgres`, not part of this repository) says that from PostgreSQL 18 on, its data volume
   is `/var/lib/postgresql`, and that PostgreSQL 17 and below use `/var/lib/postgresql/data`, the path in the Compose
   file of the Orchard Core documentation. The slides mount `/var/lib/postgresql`. Confirm that `docker-compose up`
   starts the database with that mount and that the data survives `docker-compose down` and `up`; if the machine pulls
   an image older than 18, the mount path of `compose-1` to `compose-3` and of `slides.py` changes together. The
   narration never says that the data persists. The documentation's file starts with a `version` line; the slide omits it, and whether Compose still needs it
   was not checked.
9. **`first-1`: the language of the Setup screen.** The Setup screen takes its default culture from the machine
   (`OrchardCore.Setup/Startup.cs`, `InstalledUICulture`). Confirm that the container shows English labels, so that the
   labels of the plan match the screen.
10. **Pronunciation.** The storyboard's `spoken` map has three entries (`.dockerignore`, `App_Data`, `docker-compose`)
    that a person must confirm with `scripts/pronounce.py`. Also check how the voice reads "Dockerfile", "Sqlite",
    "PostgreSQL", "NuGet", "Docker Hub" and "app slash App_Data".
11. **The recorder.** `ClickAndWaitForNavigationAsync` and `OffCameraAsync` are used together in `first-4` of the plan
    (`first-3` only rests the pointer on "Finish Setup"); confirm on the first take that the setup wait is cut, that the
    page timeout of 180 seconds is enough, and that the clip is not frozen after the form is submitted. Also confirm in
    `first-3` that the "Password strength" popover that opens while the Password field has focus
    (`Index.cshtml:236`, `data-trigger="focus"`) does not hide the Email field or the pointer on camera, and does not
    show the password.
12. **`first-2`: the default recipe.** The plan says the recipe button reads "Software as a Service" before the click,
    because `saas.recipe.json` is the only setup recipe tagged `default` (`SetupController.cs:61`). Confirm on the Setup
    screen of the container that the button shows that name, and that the list holds "Blog".
