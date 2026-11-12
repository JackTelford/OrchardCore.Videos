# Create a Theme: Your First Orchard Core Theme

**YouTube:** not published yet · **Length:** 5:41 · **Feature:** `guide:getting-started/theme` ([Getting started with an Orchard Core Theme](https://docs.orchardcore.net/en/latest/getting-started/theme/))

An audience of developers. The video generates a theme project with the `octheme` template, adds it to a solution, and
walks through the three files the template creates: the project file, the manifest and the layout. It then describes
the theme in its manifest, gives it a thumbnail, and applies it from Design, Themes with Make Current. The title,
description, tags and chapters YouTube shows are in `video.json`.

Only the last task is a recording (`05-apply`, the Themes page and the front end). Every other chapter is a slide
(`slides.py`). `recording-plan.md` holds everything needed to record it; `OPEN-QUESTIONS.md` lists what a person must
confirm.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The theme that the video
builds is written by hand in a throwaway Orchard Core checkout, because the recording only needs the browser.

1. Make a throwaway checkout of `OrchardCMS/OrchardCore` at the commit named in `recording-plan.md`, and add the theme
   project and its reference to `OrchardCore.Cms.Web` exactly as `recording-plan.md` lists them (project file,
   manifest, layout, and a `Theme.png` made with Pillow). None of these files belongs in this repository.
2. Run `src/OrchardCore.Cms.Web` with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on
   `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone
   `America/Los_Angeles`, user `admin`, email `admin@example.com`).
3. Sign the recorder in, then stop the site and copy its `App_Data` and `state.json` aside:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/theme-create/recorder
   export VIDEO_ROOT=<repo>/videos/theme-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   No `prepare` step: the video has no seed data beyond the Blog recipe.
4. Narrate and record: `scripts/narrate.py videos/theme-create`, then `Recorder record 05-apply`. It is the only clip.
   It changes the site (it enables the theme and makes it the site theme), so restore the copy of `App_Data` before
   every take.
