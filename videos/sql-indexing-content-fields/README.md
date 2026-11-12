# Content Fields Indexing: Query Fields With SQL

**YouTube:** not published yet · **Length:** 4:40 · **Feature:** `OrchardCore.ContentFields.Indexing.SQL` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/SQLIndexing/)), and `OrchardCore.ContentFields.Indexing.SQL.UserPicker` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/ContentFields/))

The video is for module and theme developers. It explains why the values of content fields have no columns to filter
on, enables the two Content Fields Indexing features, and walks through the tables they create: the columns every
field table shares, the value column of each field type, one row for each value, and which versions of a content
item have rows. It then reads a table with Run SQL Query, and shows the same tables from C# with `ISession`, from Razor
and Liquid, and through the GraphQL `where` filters. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Every action, selector and
value is in `recording-plan.md`; the unverified points are in `OPEN-QUESTIONS.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, SQLite, user `admin` with the email `admin@example.com`). Both Content Fields
   Indexing features must be disabled at that point.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/sql-indexing-content-fields/recorder
   export VIDEO_ROOT=<repo>/videos/sql-indexing-content-fields VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside (snapshot A): restore that copy before a full take.
3. Narrate with `scripts/narrate.py videos/sql-indexing-content-fields`. Two clips are recorded, in this order:
   `02-enable` (it enables the two features, so it changes the site), then, after the seed recipe of
   `recording-plan.md` is imported with Tools > Deployments > JSON Import (snapshot B), `04-sql` (it only runs `SELECT`
   statements, so it can be recorded again without restoring anything). The other chapters are slides drawn by `slides.py`.
