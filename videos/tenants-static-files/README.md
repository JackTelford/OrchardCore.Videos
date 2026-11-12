# Static Files per Tenant: Serve Each Tenant's Own Files

**YouTube:** not published yet · **Length:** 3:45 · **Feature:** `OrchardCore.Tenants.FileProvider` ([Static File Provider](https://docs.orchardcore.net/en/latest/reference/modules/Tenants/#static-file-provider-feature))

The video is for developers. It explains why a file in the shared `wwwroot` folder of an application belongs to the application and not to a
tenant, and how the Static File Provider feature gives each tenant a `wwwroot` folder of its own. It enables
the feature on a tenant, serves a different `robots.txt` for two tenants, and reads the code that registers and serves the
files, with the order in which files are served and what removing a tenant does to them. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. The full plan, with every
action, selector and source reference, is in [`recording-plan.md`](recording-plan.md); questions that a person must
confirm on a running site are in [`OPEN-QUESTIONS.md`](OPEN-QUESTIONS.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup with three tenants: `Default`
   (the SaaS recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`), `Maple` (the Blog recipe,
   URL prefix `bakery`) and `Cedar` (the Blog recipe, URL prefix `garden`). The environment variables are in the plan.
2. Seed the site outside the capture: enable **Static File Provider** on Cedar by hand, and create a `robots.txt` in
   the `wwwroot` folder of the Maple and Cedar tenants (the exact three lines of each are in the plan). Maple does not
   have the feature yet: the video enables it on camera.
3. Write `recorder/Scenes.cs` from the recording plan (one method for each of the two recorded clips), sign the recorder in to
   the Default tenant, and copy `App_Data` and `state.json` aside:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/tenants-static-files/recorder
   export VIDEO_ROOT=<repo>/videos/tenants-static-files VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   The copy is the starting state: restore it before every take of `02-enable`.
4. Narrate and record: `scripts/narrate.py videos/tenants-static-files`, then `Recorder record 02-enable`, then
   `Recorder record 04-serve`. `04-serve` needs the feature enabled on Maple by `02-enable`; the other clips are slides
   drawn by `slides.py`.
