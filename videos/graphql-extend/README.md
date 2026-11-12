# Extend GraphQL: Add Your Own Types and Queries

**YouTube:** not published yet · **Length:** 6:04 · **Feature:** `OrchardCore.Apis.GraphQL` ([GraphQL](https://docs.orchardcore.net/en/latest/reference/modules/Apis.GraphQL/), [GraphQL Abstractions](https://docs.orchardcore.net/en/latest/reference/modules/Apis.GraphQL.Abstractions/))

The video is for developers of modules. It shows how the GraphQL schema of Orchard Core is built from registered
services, and the four pieces a module registers to extend it: an object type for a content part, an input type with an
index to filter by that part, an `ISchemaBuilder` for a query of its own, and a `GraphQLFilter` for the content type
queries. The code on the slides is taken from Orchard Core's own Autoroute, Localization and content management GraphQL
libraries, and three short recordings in GraphiQL show the result. The title, description, tags and chapters YouTube
shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. It needs no custom module: the
recordings run on the Autoroute and Localization modules of Orchard Core, which register the same pieces as the slides.
`recording-plan.md` has every action, query and expected result; `OPEN-QUESTIONS.md` lists what to confirm first.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Enable the features **GraphQL** and **Localization**, save the cultures `en-US` (default) and `fr-FR`, and create the
   three blog posts of the plan. Then stop the site and copy its `App_Data` aside: restore that copy before a full take.
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/graphql-extend/recorder
   export VIDEO_ROOT=<repo>/videos/graphql-extend VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

4. Narrate and record: `scripts/narrate.py videos/graphql-extend`, then `Recorder record all`. The three clips
   (`03-explorer`, `05-where`, `07-run`) only run queries, so none depends on another and none changes the site; record
   them in any order from the one snapshot.
