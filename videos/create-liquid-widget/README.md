# Create a Liquid Widget Without Writing a Module

**YouTube:** not published yet · **Length:** 4:52 · **Feature:** `guide:guides/create-liquid-widget` ([documentation](https://docs.orchardcore.net/en/latest/guides/create-liquid-widget/))

A Liquid widget is a content type that has the Liquid part and the Widget stereotype, so an editor writes its body in
Liquid and places it in a zone through a layer. The video builds one in the admin, with no module: the content type, the
stereotype, the placement in the Footer zone, and a `Widget__LiquidWidget` template that changes the markup the widget
renders. It ends with the same template in flows and in a theme. The title, description, tags and chapters YouTube
shows are in `video.json`; the steps to record are in `recording-plan.md`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`. The Blog recipe enables every feature the video uses.
2. Sign the recorder in. The video has no seed data and no `PrepareAsync`:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/create-liquid-widget/recorder
   export VIDEO_ROOT=<repo>/videos/create-liquid-widget VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside (the first snapshot).
3. Narrate with `scripts/narrate.py videos/create-liquid-widget`, then record the clips in the order they play:
   `02-type`, `03-widget`, `04-place`, `06-template`. Each one changes the site and starts from what the previous one left,
   so copy `App_Data` aside after each clip, and restore the copy of the clip before it to record a clip again.
   `recording-plan.md` has the exact actions, the text to enter and what must be on screen at the end of every step.
4. Look at every contact sheet, then check the video for private data as the skill says.
