# Build a Headless Site: Content in Orchard Core, Front End Anywhere

**YouTube:** not published yet · **Length:** 7:30 · **Feature:** `OrchardCore.Apis.GraphQL` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Apis.GraphQL/))

A headless site keeps its content in Orchard Core and leaves the pages to another application. The video builds that on a
Blog site: it enables the GraphQL and CORS Configuration features, explores the API in GraphiQL, shows how the saved query
`RecentBlogPosts` becomes a GraphQL field, grants the Anonymous role the permissions to call it, and adds a CORS policy so a
web page on its own address can read the posts. It also covers `OrchardCore.Queries` and `OrchardCore.Cors`, and ends
with recipes, limits and sign-in for a production site. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The audience is developers:
the browser clips show the admin and a small web page, and the code is on slides. `recording-plan.md` has every action,
selector and source reference; this is the outline.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup with the **Blog** recipe (site name **Orchard
   Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). The Blog recipe is required: the video
   uses its content type `BlogPost`, its saved query `RecentBlogPosts` and the Queries and SQL Queries features it
   enables. GraphQL and CORS Configuration stay disabled until the first clip enables them.
2. Sign the recorder in, then seed the site outside the capture:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/build-a-headless-site/recorder
   export VIDEO_ROOT=<repo>/videos/build-a-headless-site VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Import the three fake blog posts of the plan with **Tools > Deployments > JSON Import**, and serve the two files of the
   front end (a page and a script, given in the plan) from a folder outside both repositories on
   `http://localhost:5173`.
3. Stop the site and copy its `App_Data` aside as snapshot S0: restore it before every full take.
4. Narrate and record: `scripts/narrate.py videos/build-a-headless-site`, then `Recorder record all`. The clips change the
   site and depend on each other, so they are recorded in the order they play: `02-features` (enables GraphQL and CORS
   Configuration), `03-graphiql`, `04-query`, `05-permissions` (saves the Anonymous role), `06-before` (the page fails:
   no CORS policy), `07-policy` (saves the policy), `07-after` (the page shows the posts). Take a snapshot after
   `02-features` and after `05-permissions`; the plan says which one to restore for each clip. The two clips that show the
   front-end page open it with the browser page directly, since the scene methods add the site address to their argument.
