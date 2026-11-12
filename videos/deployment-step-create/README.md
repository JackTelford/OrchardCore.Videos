# Write a Deployment Step

**YouTube:** not published yet · **Length:** 5:43 · **Feature:** `OrchardCore.Deployment` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Deployment/))

A video for module authors. It follows the All Features deployment step of the Features module through its parts: the
step class a plan stores (`Name`, `Category`, `Title`), the source that adds recipe steps and files to the package, the
display driver with its views, and the registration in a startup class. It ends with the step in a plan in the admin,
and the `Recipe.json` of the package it builds. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Only one clip, `06-plan`, is a
recording; every other chapter is a slide, drawn by `slides.py` from code of Orchard Core. The steps, the clip and the
checks are in `recording-plan.md`, and what a person must confirm on a running site is in `OPEN-QUESTIONS.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). No feature needs to be enabled: the Blog
   recipe enables Deployment, and the Features module is always enabled.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/deployment-step-create/recorder
   export VIDEO_ROOT=<repo>/videos/deployment-step-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` and `state.json` aside: restore them before every take, since the clip creates a
   deployment plan.
3. Narrate and record: `scripts/narrate.py videos/deployment-step-create`, then `Recorder record 06-plan`.
