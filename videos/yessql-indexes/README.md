# YesSql Indexes: Query Your Own Documents

**YouTube:** not published yet · **Length:** 5:47 · **Feature:** `guide:topics/data/yessql` ([How YesSql works](https://docs.orchardcore.net/en/latest/topics/data/yessql/))

Orchard Core stores most of its data as JSON documents with YesSql, and an index is what makes a property of a
document queryable. The video is for module authors: it defines a map index for a product content part, registers its
provider, creates its table with a data migration, and queries it through `ISession`. It also shows a reduce index, the
index tables of a site in the Run SQL Query page, and the declarations that tell YesSql what an index holds. The title,
description, tags and chapters YouTube shows are in `video.json`.

Chapters 01 to 05 and 07 are slides with code from the documentation page and from the Orchard Core source. Chapter 06
is the only recording.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. Everything a person has
to do is in `recording-plan.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). The Blog recipe
   enables the SQL Queries and Roles features that the recording uses, and no data has to be seeded.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/yessql-indexes/recorder
   export VIDEO_ROOT=<repo>/videos/yessql-indexes VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Check the two queries listed in the plan return rows, then narrate and record: `scripts/narrate.py videos/yessql-indexes`,
   then `Recorder record 06-tables`. The clip only reads the database, so a take does not change the site and the clips
   need no particular order. The slides (chapters 01 to 05 and 07) are drawn by `slides.py` when the video is assembled.
