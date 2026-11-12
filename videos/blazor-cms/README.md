# Blazor: A Blazor Server Front End for Orchard Core Content

**YouTube:** not published yet · **Length:** 6:50 · **Feature:** [`guide:guides/create-blazor-cms`](https://docs.orchardcore.net/en/latest/guides/create-blazor-cms/)

A video for developers. It shows how a Blazor Server front end lives in a Razor class library that the Orchard Core
web project references, how `Program.cs` maps it into the pipeline of every tenant, and how a page reads a content item
by its alias and renders its Markdown. It then adds a tenant under a URL prefix, sets the base tag from the tenant's
base URI, and makes a component interactive. The code is on slides; the browser is recorded for the admin screens
and for the Blazor site. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **throwaway Blazor site** built from the guide (`src/docs/guides/create-blazor-cms/README.md`
of `OrchardCMS/OrchardCore`), outside both repositories, with fake data only, so it needs no mask script. The
Orchard Core source has no Blazor site and the guide's own sample is not kept here, so
[recording-plan.md](recording-plan.md) says how to build it, with the deviations from the guide, and
[OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what to confirm on the running site.

1. Build the host and the Razor class library as `recording-plan.md` describes (Build A): Orchard Core packages of
   one version for both projects, `.AddSetupFeatures("OrchardCore.AutoSetup")` in `Program.cs`, and `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories. Provision it by Auto Setup on `http://localhost:5010`: the
   **Headless site** recipe (`Headless`), site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`,
   email `admin@example.com`, the password from `VIDEO_PASSWORD`.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/blazor-cms/recorder
   export VIDEO_ROOT=<repo>/videos/blazor-cms VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   There is no `prepare`: the content of the default tenant is imported on camera. Stop the site and copy its
   `App_Data` folder aside as snapshot R0 (with `state.json`); the tenant feature must be disabled in it.
3. Narrate and record: `scripts/narrate.py videos/blazor-cms`, then the clips, in this order:
   `02-content` (changes the site: snapshot R1), `05-run`, `06-tenant` (changes the site: snapshot R2), the off-camera
   setup of the tenant (snapshot R3), `07-site`, and, on Build B (Build A plus the Interactive page), `08-click`.
   Restore the snapshot before a clip that changes the site is recorded again.
