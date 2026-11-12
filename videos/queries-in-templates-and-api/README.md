# Queries in Liquid, Razor and the API

**YouTube:** not published yet · **Length:** 4:50 · **Feature:** `OrchardCore.Queries` ([Queries](https://docs.orchardcore.net/en/latest/reference/modules/Queries/))

A saved query has a name, and that name is all a caller needs. The video starts from the `RecentBlogPosts` query that
the Blog recipe saves, then runs it from a Liquid template with the `Queries` property and the `query` filter, from a
Razor view with `ContentQueryAsync` and `QueryAsync`, and through the `api/queries/{name}` endpoint, with its
parameters, its permissions and its sign-in. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Most of it is slides
(`slides.py`); four clips are browser recordings. `recording-plan.md` lists every action, and `OPEN-QUESTIONS.md` lists
what a person must confirm on a running site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the **Blog** recipe, site name **Orchard
   Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, a password chosen outside git).
   Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
2. Enable no feature: the Blog recipe already enables Queries, SQL Queries, Roles, Lists and Templates. Create the
   three fake blog posts of the plan (`Notes From the Allotment`, `A Week of Slow Travel`, `Planning the Autumn Menu`,
   in that order) and check that `/api/queries/RecentBlogPosts` answers `404` before the Anonymous role is granted
   the permission (the seed checks of `recording-plan.md`, with `curl --noproxy '*'`).
3. Sign the recorder in, then stop the site and copy its `App_Data` folder and `state.json` aside:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/queries-in-templates-and-api/recorder
   export VIDEO_ROOT=<repo>/videos/queries-in-templates-and-api VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

4. Narrate, then record the clips in this order, which is also the order they play in: `01-queries`, `02-post`,
   `04-grant`, `04-call`. `04-grant` changes the site (it grants a permission to the Anonymous role): restore the
   copy of `App_Data` before another take of it. `04-call` needs `04-grant` and changes nothing.
