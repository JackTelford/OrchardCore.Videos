# Data in Orchard Core: Databases, YesSql and Documents

**YouTube:** not published yet · **Length:** 6:22 · **Feature:** `topic:Data` ([Data](https://docs.orchardcore.net/en/latest/reference/modules/Data/))

A developer video about how Orchard Core stores data. It explains YesSql documents, indexes and the session; the four
database providers and the Setup screen where a tenant chooses one; the SQLite file and its connection pooling; the
YesSql options and the table naming presets; and how to run your own SQL with `IDbConnectionAccessor` and `IStore`.
The title, description, tags and chapters YouTube shows are in `video.json`. Writing an index of your own is the
subject of a separate video.

## Recording it again

Only one clip is a recording: `03-setup`, the Setup screen of a site that is not set up. The rest of the video is
slides, drawn by `slides.py`. Everything the recorder needs to decide is in `recording-plan.md`; `OPEN-QUESTIONS.md`
lists what a person must confirm on a running site and by ear.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an **empty folder**
   outside both repositories, on `http://localhost:5010` (the command is in `recording-plan.md`: the launch profiles use
   other ports). This video is recorded **without Auto Setup**, without
   `state.json` and without signing in: set no `OrchardCore__AutoSetup__*` variable, and set no database provider or
   connection string in any configuration source, or the Setup screen hides the fields the clip shows.
2. Nothing is seeded, and `Recorder setup` and `Recorder prepare` are not run. The values typed in the form before the
   capture (site name, time zone `America/Los_Angeles`, a connection string with the host `db.example.com`) are fake,
   and the form is never submitted.
3. Write `recorder/Scenes.cs` with one method, for the clip `03-setup`, from `recording-plan.md`. Then narrate and record:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/data-and-yessql
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/data-and-yessql/recorder
   export VIDEO_ROOT=<repo>/videos/data-and-yessql VIDEO_BASE_URL=http://localhost:5010
   <bin>/Recorder record 03-setup
   ```

   The clip changes nothing on the site, so no snapshot of `App_Data` is needed between takes. It depends on no other
   clip. Check the pronunciations listed in `OPEN-QUESTIONS.md` with `scripts/pronounce.py` before narrating.
