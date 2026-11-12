# Liquid Part and Liquid Field: Content That Runs Templates

**YouTube:** not published yet · **Length:** 4:20 · **Feature:** `OrchardCore.Liquid` ([Liquid](https://docs.orchardcore.net/en/latest/reference/modules/Liquid/))

The Liquid part and the Liquid field store a Liquid template inside a content item and run it when the content is
displayed. The video adds each one in the admin, writes a template that reads the site and the content item, and shows
the migration code that does the same. It then follows the template through the shapes that render it, and ends with the
Manage Liquid templates permission. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Two chapters are recordings
(`02-part` and `03-field`); the others are slides and cards. The exact actions of every step are in
`recording-plan.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin` with the email `admin@example.com`. The password is chosen outside git
   and passed as `VIDEO_PASSWORD`. No feature has to be enabled: the Liquid feature comes with Contents.
2. Create the seed outside the capture: one content type named **Notice** with the Title part (the steps are in
   `recording-plan.md`). The clips are methods of `recorder/Scenes.cs` in this folder, one per clip, written from
   `recording-plan.md`; the folder does not exist until then. Sign the recorder in, stop the site, and copy its `App_Data`
   and the `state.json` aside (snapshot S0):

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/liquid-part-and-field/recorder
   export VIDEO_ROOT=<repo>/videos/liquid-part-and-field VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Narrate with `scripts/narrate.py videos/liquid-part-and-field`, then record the clips in this order, restoring
   the snapshot first: `02-part` from S0, then `03-field` from the snapshot taken after `02-part` (S1). `03-field`
   needs the Liquid part and the item "Open House" that `02-part` creates.
4. Slides come from `slides.py`; confirm the spoken forms of the identifiers in the storyboard's `spoken` entries with
   `scripts/pronounce.py` before narrating (see `OPEN-QUESTIONS.md`).
