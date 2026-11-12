# Write a Content Part With a Display Driver

**YouTube:** not published yet · **Length:** 6:28 · **Feature:** `topic:ContentParts` ([Content Parts](https://docs.orchardcore.net/en/latest/reference/modules/ContentParts/))

The video goes through the pieces of a content part in the code of the Title part of Orchard Core: the class, its view model
and its definition, the display driver with its `Display`, `Edit` and `UpdateAsync` methods, the templates of each shape, and
the registration. It then adds the part to a content type on a site, shows the validation error that `UpdateAsync` adds,
and shows the part on the front end. The title, description, tags and chapters YouTube shows are in `video.json`, and the
documentation pages it backs are Content Parts and Display Management.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The slides need no site: they
draw the code of `OrchardCore.Title` from `slides.py`. Only the two clips `06-attach` and `07-use` are recorded, and no
module is built, because the example is the Title part that ships with Orchard Core. [recording-plan.md](recording-plan.md)
holds every action of every step.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup with the **Blank** recipe (not Blog), site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin` with the email `admin@example.com`.
2. No feature needs enabling: the Blank recipe enables Features, Contents, Content Types and Title. In the admin, enable
   the theme **The Default Theme** and make it current (Design, Themes): the Blank recipe sets no site theme, and The
   Default Theme leaves the `Content` template of `OrchardCore.Contents` alone, which renders the `Header` zone where the
   Title part places its shape.
3. Write `recorder/Scenes.cs` from the clips of the plan (one method per clip), then sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/content-part-create/recorder
   export VIDEO_ROOT=<repo>/videos/content-part-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside: this is snapshot S0.
4. Check the spoken forms of the storyboard (`spoken`: for example "Title Part underscore Edit") with
   `scripts/pronounce.py`, then narrate: `scripts/narrate.py videos/content-part-create`.
5. Record in this order: `06-attach` from S0, which creates the content type Workshop (copy `App_Data` aside again
   as S1), then `07-use` from S1, which creates the content item Pruning Apple Trees. `07-use` depends on `06-attach`.
   Restore S0 before another take of `06-attach`, and S1 before another take of `07-use`.
