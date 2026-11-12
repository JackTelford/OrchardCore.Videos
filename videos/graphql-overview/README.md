# GraphQL: Query Your Content With One Request

**YouTube:** not published yet · **Length:** 5:12 · **Feature:** `OrchardCore.Apis.GraphQL` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Apis.GraphQL/))

An overview of the GraphQL module for developers. It shows why a client asks for exactly the fields it needs, how to
enable the module and run a first query in GraphiQL, what the schema contains, how a client calls the endpoint and who
may run a query, which options limit queries and results, and how to extend the schema with your own parts and
queries. The title, description, tags and chapters YouTube shows are in `video.json`; the exact steps to record are in
`recording-plan.md`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the **Blog** recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). Do not enable GraphQL:
   the first recording does it on camera, so do not use the Headless site recipe either.
2. Write the scenes of the two recordings in `recorder/Scenes.cs` from `recording-plan.md`, then sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/graphql-overview/recorder
   export VIDEO_ROOT=<repo>/videos/graphql-overview VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Seed the site by hand, outside the capture, as `recording-plan.md` describes: import the JSON with three blog
   posts into the Blog, delete the sample post, and check the list. Stop the site and copy its `App_Data` aside as
   snapshot A. The seed is small and is created from the admin, so the scenes need no `PrepareAsync`.
4. Narrate with `scripts/narrate.py videos/graphql-overview`, then record. The order matters:
   - `02-enable` enables GraphQL, so restore snapshot A before every take of it.
   - `03-query` needs GraphQL enabled and changes nothing, so it can be recorded again as often as needed.
   - The slide clips (`01-why`, `04-schema`, `05-http`, `06-config`, `07-extend`) need no site.
5. Before recording `03-query`, run the checks of `recording-plan.md` (section 3) in GraphiQL, and read
   `OPEN-QUESTIONS.md`: it lists the names and selectors that were read from the source and not from a running site,
   and the spoken forms to confirm with `scripts/pronounce.py`.
