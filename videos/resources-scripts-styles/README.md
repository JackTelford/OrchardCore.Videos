# Resources: Declare and Include Scripts and Stylesheets

**YouTube:** not published yet · **Length:** 5:16 · **Feature:** `OrchardCore.Resources` ([Resources module](https://docs.orchardcore.net/en/latest/reference/modules/Resources/))

A video for module and theme authors. It shows how the Resources module declares scripts and stylesheets in a resource
manifest, how Razor and Liquid views include them by name, with a version, dependencies and a location, and how the
layout renders them. It also covers script and style blocks, link and meta tags, and the `IResourceManager` API. The one
recording is the Resources tab of **Settings, General**: cache busting, the framework CDN, the CDN base url and the
Resource Debug Mode. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. Only chapter 07, the clip
`07-settings`, is a recording; the other chapters are slides drawn by `slides.py`. [`recording-plan.md`](recording-plan.md)
holds the exact actions of every step, and [`OPEN-QUESTIONS.md`](OPEN-QUESTIONS.md) what to confirm on the running site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`). No feature has to be enabled: the Resources and Settings modules are
   always enabled, and the settings stay at their defaults.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/resources-scripts-styles/recorder
   export VIDEO_ROOT=<repo>/videos/resources-scripts-styles VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   There is no `prepare` step: the video seeds no data. Collapse the **Settings** group of the admin menu before saving
   the sign-in.
3. Narrate and record: `scripts/narrate.py videos/resources-scripts-styles`, then `Recorder record 07-settings`. The clip
   never saves the settings, so it does not change the site and no copy of `App_Data` has to be restored between takes.
