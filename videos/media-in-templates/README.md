# Media in Templates: Image Resizing, Asset URLs and Tag Helpers

**YouTube:** not published yet · **Length:** 5:25 · **Feature:** `OrchardCore.Media` ([Media documentation](https://docs.orchardcore.net/en/latest/reference/modules/Media/))

A video for module and theme authors. It follows a media path from a media field to the browser: the asset URL, the
resize commands and their signed token, the cache, and the same options in Liquid filters, Razor helpers and tag
helpers, with media profiles by name. Most of it is slides; two clips are recorded: the Media Profiles screen, and a
front-end page that shows one image resized five ways. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` has the
exact seed (a JSON recipe), the steps of each clip, and the checks for private data.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the **Blog** recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). The Blog recipe already enables Media, Templates and
   Deployment, and holds the picture `home-bg.jpg` that the video resizes.
2. Sign the recorder in, then seed the site once from the recipe of `recording-plan.md` (**Tools > Deployments > JSON
   Import**):

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/media-in-templates/recorder
   export VIDEO_ROOT=<repo>/videos/media-in-templates VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside once the seed is imported: restore that copy before a full take.
3. Narrate and record: `scripts/narrate.py videos/media-in-templates` (after a person has confirmed the spoken forms in
   `storyboard.json`, see `OPEN-QUESTIONS.md`), then `Recorder record 03-profile 06-result`. The two clips only read
   the site, so they can be recorded in any order; both need the seed. The other chapters are slides, drawn by
   `slides.py`.
