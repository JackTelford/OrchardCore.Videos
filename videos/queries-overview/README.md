# Queries: Save and Reuse Search and Database Queries

**YouTube:** not published yet · **Length:** 5:04 · **Feature:** `OrchardCore.Queries` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Queries/))

A module overview for developers. It explains what a saved query is (a name, a source, an optional schema and a flag
for content items), the Queries screen, creating a query, running one by name and the permissions the API checks, the
recipe and deployment steps, and the pieces of a query source. The title, description, tags and chapters YouTube shows
are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The two recordings (`02-screen`
and `03-create`) use the Blog recipe as it is: the recipe enables **Queries** and **SQL Queries** and saves one query,
`RecentBlogPosts`, so there is nothing to seed. The other chapters are slides. Every detail is in
[recording-plan.md](recording-plan.md), and what a person must confirm on a running site is in
[OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, a password chosen
   outside git).
2. Sign the recorder in, once:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/queries-overview/recorder
   export VIDEO_ROOT=<repo>/videos/queries-overview VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside: restore that copy before every take of `02-screen` and `03-create`.
3. Narrate and record: `scripts/narrate.py videos/queries-overview`, then record `02-screen`, then `03-create`.
   `03-create` adds a query, so the site must be restored before `02-screen` is recorded again.
