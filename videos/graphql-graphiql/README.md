# GraphiQL: Explore and Test GraphQL Queries

**YouTube:** not published yet · **Length:** 4:36 · **Feature:** `OrchardCore.Apis.GraphQL` ([GraphQL documentation](https://docs.orchardcore.net/en/latest/reference/modules/Apis.GraphQL/))

The video is for developers. It shows GraphiQL, the page of the GraphQL module under Tools, GraphiQL: the layout of the
screen, a query built by clicking fields in the Explorer, the schema read in Docs, a query with variables, the address that
keeps a query, and an error. Slides show the request GraphiQL sends and the same request from an HTTP client. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Every step, selector and
expected screen is in `recording-plan.md`; `OPEN-QUESTIONS.md` lists what to confirm on the running site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/graphql-graphiql/recorder
   export VIDEO_ROOT=<repo>/videos/graphql-graphiql VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare     # enables the GraphQL feature and creates two blog posts
   ```

   The seed is the **GraphQL** feature (`OrchardCore.Apis.GraphQL`, which the Blog recipe does not enable) and two published
   blog posts, "Notes from the launch pad" and "A field guide to quiet places", next to the recipe's own post. Stop the site and
   copy its `App_Data` aside: restore that copy after a take that changed content.
3. Narrate and record: `scripts/narrate.py videos/graphql-graphiql`, then `Recorder record all`. The five recorded clips
   (`02-open`, `03-build`, `04-docs`, `05-variables`, `06-errors`) do not depend on each other, and none changes the site.
   The slide clips (`01-what`, `05-address`, `07-code`) are drawn by `slides.py`.
