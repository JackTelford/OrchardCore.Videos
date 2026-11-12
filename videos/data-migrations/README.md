# Data Migrations: Create and Update Your Module's Data

**YouTube:** not published yet · **Length:** 5:24 · **Feature:** `topic:Migrations` ([Data Migrations](https://docs.orchardcore.net/en/latest/reference/modules/Migrations/))

A developer video about data migrations: the `DataMigration` class with its `CreateAsync` and `UpdateFrom` steps, how a
Startup class registers it, and how Orchard Core runs it as a tenant starts. Slides with code cover content
definitions, index tables, recipes and changes to existing content. One browser recording enables the Forms feature to
show its migration run. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Eight of the nine chapters are slides, drawn by `slides.py`. Only the chapter "See a migration run" (the clip `04-run`) is
recorded. `recording-plan.md` lists every action of that clip, what must be on screen at the end of each step, and the
checks to make before the first take.

The video was recorded on a **fresh site** with fake data only, so it needs no mask script and no seed data.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard
   Core**, time zone `America/Los_Angeles`, user `admin`). Enable no feature: the Forms feature must be disabled when
   the clip starts.
2. Sign the recorder in. The clip needs no seed data, so there is nothing to prepare:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/data-migrations/recorder
   export VIDEO_ROOT=<repo>/videos/data-migrations VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside: restore that copy before every take, because the clip enables the Forms
   feature, and disabling it does not remove the content types its migration created.
3. Narrate and record: `scripts/narrate.py videos/data-migrations`, then `Recorder record 04-run`. There is one clip, so
   there is no order to keep.
