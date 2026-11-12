# Taxonomies in Code: Query Content Items by Term

**YouTube:** not published yet · **Length:** 4:40 · **Feature:** `OrchardCore.Taxonomies` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Taxonomies/))

A developer video about finding content items by the terms they are filed under. It shows what a `TaxonomyField` stores and the
`TaxonomyIndex` table, `QueryCategorizedContentItemsAsync` and `ISession` queries on that table, the term ids a Lucene index keeps and a
saved Lucene query with Liquid parameters, how Razor and Liquid call it, and the query behind a term page. The guide it follows is
[Creating the pieces needed to query content based on assigned taxonomies](https://docs.orchardcore.net/en/latest/guides/query-content-items-based-on-taxonomy/).
The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Two chapters are recordings (the Lucene index
settings and a rebuild, then saving and running a query); the others are slides drawn by `slides.py`. `recording-plan.md` has every
action, address and label, and `OPEN-QUESTIONS.md` lists what to confirm on the first take.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on
   `http://localhost:5010`, provisioned by Auto Setup (the `Blog` recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user
   `admin`, email `admin@example.com`, a password chosen outside git).
2. Run the recipe **Blog - Lucene Search** (Tools > Recipes), add the term **Politics** to the **Categories** taxonomy and publish it, and create
   three more published blog posts (the exact data is in `recording-plan.md`). Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/taxonomies-query-content/recorder
   export VIDEO_ROOT=<repo>/videos/taxonomies-query-content VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside (snapshot S1).
3. Narrate (`scripts/narrate.py videos/taxonomies-query-content`), then record `03-index` from S1. It changes the site (two fields are indexed
   and the `Search` index is rebuilt). When the rebuild is done, copy `App_Data` aside again (snapshot S2), and record `04-query`, which depends on
   `03-index` and saves the query `GetBlogsByFilter`. Restore S1 before another take of `03-index`, and S2 before another take of `04-query`.
