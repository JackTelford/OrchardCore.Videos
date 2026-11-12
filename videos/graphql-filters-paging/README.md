# GraphQL: Filter, Sort and Page Content Items

**YouTube:** not published yet · **Length:** 4:53 · **Feature:** `OrchardCore.Apis.GraphQL` ([GraphQL documentation](https://docs.orchardcore.net/en/latest/reference/modules/Apis.GraphQL/))

A developer video about the five arguments of a content type query: `where` to filter, `orderBy` to sort, `first`
and `skip` to page, and `status` to choose the version of each item. It runs the queries in GraphiQL on a small blog,
lists the filter operators of each kind of field, shows how the arguments combine in one query, and ends with the
three settings that limit the number of results. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything the recorder
needs, down to the data and the text of every query, is in [recording-plan.md](recording-plan.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup with the **Headless** recipe (site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). Set
   `OrchardCore__Apis__GraphQL__MaxNumberOfResultsValidationMode=Enabled` so the last clip shows the validation error.
2. Seed the site outside the capture, with the two JSON imports of the plan (Tools > Deployments > JSON Import): the
   first enables Autoroute, the second creates the Blog Post type and nine posts, one of them a draft. Check the seed
   with the query of the plan before recording.
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/graphql-filters-paging/recorder
   export VIDEO_ROOT=<repo>/videos/graphql-filters-paging VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside after seeding: restore that copy before a full take.
4. Narrate and record: `scripts/narrate.py videos/graphql-filters-paging`, then `Recorder record all`. The clips are
   `02-filter`, `04-sort`, `05-page`, `06-status`, `07-combine` and `08-limit-error`; the other chapters are cards and
   slides. No clip changes the site, so the clips can be recorded in any order, one at a time.
