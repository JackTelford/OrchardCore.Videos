# Lucene and Elasticsearch Queries: Search From a Saved Query

**YouTube:** not published yet · **Length:** 4:48 · **Feature:** `OrchardCore.Lucene` ([Lucene](https://docs.orchardcore.net/en/latest/reference/modules/Lucene/)), with `OrchardCore.Elasticsearch` ([Elasticsearch](https://docs.orchardcore.net/en/latest/reference/modules/Elasticsearch/)) and `OrchardCore.Queries` ([Queries](https://docs.orchardcore.net/en/latest/reference/modules/Queries/))

The video is for module and site developers. It runs a query on the **Run Lucene Query** page, turns it into a Liquid
template with a parameter and a default, saves it as a named query, and explains what **Return Content Items** changes.
It then connects Elasticsearch, shows how the same query differs there, saves an Elasticsearch query, and ends with the
`Queries` recipe step. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with invented data only, so it needs no mask script. Everything the recorder
needs, clip by clip, is in `recording-plan.md`; `OPEN-QUESTIONS.md` lists what nobody could check without a running site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). Set `NO_PROXY=localhost,127.0.0.1` next to
   the dead `HTTP_PROXY` and `HTTPS_PROXY`, and `OrchardCore__Search__Elasticsearch__Url=http://localhost`.
2. Seed the site outside the capture: four more published blog posts, then the recipe **Blog - Lucene Search** from
   **Tools, Recipes** (not **Blog - Lucene Query**). Wait until **Run Lucene Query** finds every document of the index
   `Search`. Stop the site and copy its `App_Data` aside (snapshot S0).
3. Sign the recorder in and record:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/queries-lucene-elasticsearch/recorder
   export VIDEO_ROOT=<repo>/videos/queries-lucene-elasticsearch VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

4. Narrate (`scripts/narrate.py videos/queries-lucene-elasticsearch`), then record in this order: `02-run`, `03-params`,
   `04-save` (restore S0 before another take; keep the site after a good take as S1), then prepare Elasticsearch
   (Docker Compose file of the documentation, services `es01` and `es02`, and the `blogelastic` index imported from
   **Tools, Deployments, JSON Import**; keep the site as S2), then `06-elastic`. `02-run` and `03-params` change nothing;
   `04-save` and `06-elastic` each create a query, and `06-elastic` is the only clip that needs Docker.
