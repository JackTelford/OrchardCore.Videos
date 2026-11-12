# Write a Content Field With Its Editor and Settings

**YouTube:** not published yet · **Length:** 7:18 · **Feature:** `OrchardCore.ContentFields` ([Content Fields](https://docs.orchardcore.net/en/latest/reference/modules/ContentFields/))

A video for developers who write their own content fields. It reads the pieces of a field in the Boolean field of the
Content Fields module: the class and its settings class, the display driver, the views, the settings driver with its
view, the registration in Startup, and the editors and display modes that a field can offer. It ends in the admin, where
the field is added to a content type and the pieces show as the settings page, the editor list, the content editor and
the front end. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Chapters 01 to 07 are slides
drawn by `slides.py`; only chapter 08 (clip `08-admin`) is recorded. `recording-plan.md` has every action of the clip.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). The Blog recipe enables every feature the
   clip needs.
2. Seed the site once, outside the capture: create the content type **Event** (the Title and Autoroute parts and its own
   part, no field) with Tools, Deployments, JSON Import, using the JSON of `recording-plan.md`. Sign the recorder in,
   stop the site, and copy its `App_Data` and `state.json` aside:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/content-field-create/recorder
   export VIDEO_ROOT=<repo>/videos/content-field-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Narrate with `scripts/narrate.py videos/content-field-create`, then record the clip with `Recorder record 08-admin`.
   The clip adds the field Featured to the Event type and publishes the Event "Community meetup", so restore the copy of
   `App_Data` and `state.json` before every take.
