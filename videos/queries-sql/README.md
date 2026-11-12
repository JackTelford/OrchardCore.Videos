# SQL Queries: Query Content Items With SQL

**YouTube:** not published yet · **Length:** 5:44 · **Feature:** `OrchardCore.Queries.Sql` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Queries/))

The video shows the SQL Queries feature of the Queries module for developers. It starts with the `ContentItemIndex` table, then runs statements on the Run SQL Query page, uses parameters and a Liquid condition, saves a statement as a named query with Return Documents, creates one with a recipe step, and ends with the Manage SQL Queries permission. The title, description, tags and chapters YouTube shows are in `video.json`, and the page-by-page recording instructions are in `recording-plan.md`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the **Blog** recipe (it enables the Queries and SQL Queries features), site name **Orchard Core**, time zone `America/Los_Angeles`, the default SQLite database with no table prefix, user `admin` with the email `admin@example.com`.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/queries-sql/recorder
   export VIDEO_ROOT=<repo>/videos/queries-sql VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Seed the site outside the capture: import the five blog posts of `recording-plan.md` at **Tools**, **Deployments**, **JSON Import**. Stop the site and copy its `App_Data` folder and `state.json` aside; restore that copy before every full take.
4. Narrate and record: `scripts/narrate.py videos/queries-sql`, then `Recorder record 02-run 03-params 05-save`. The three clips do not depend on each other; `05-save` creates the query `PublishedBlogPosts`, so restore the copy of `App_Data` before another take of any clip. The other chapters are slides drawn by `slides.py`.
5. Check every contact sheet of `review.py` for an empty result list or an error message, then scan the video for private data as `recording-plan.md` describes.

Questions a person has to confirm on the running site are in `OPEN-QUESTIONS.md`.
