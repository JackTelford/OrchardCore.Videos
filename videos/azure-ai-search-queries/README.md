# Azure AI Search Queries: Search From a Saved Query

**YouTube:** not published yet · **Length:** 4:28 · **Feature:** `OrchardCore.AzureAI` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/AzureAISearch/))

A developer video. The Queries module saves a search under a name, so the API, Liquid, Razor and scripts can run it.
Its sources are Lucene, Elasticsearch and SQL; the Azure AI Search module registers a search service but no query
source. The video writes a small module that adds one: the query source, its settings and handler, its editor and
its startup. It then creates a saved query on the Queries page, runs it through the API, and points to the Azure
Search SDK for vector and hybrid searches. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Everything the recorder needs, including the code of the module and of the stand-in for the Azure AI Search service,
is in [recording-plan.md](recording-plan.md). In short:

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. The Azure AI Search
service has no local emulator, so the site talks to a small stand-in over `https://localhost:7443`; no Azure
account, endpoint or key is involved.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore with the throwaway module `Acme.AzureAIQueries` (listed in the plan,
   never committed) and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on
   `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone
   `America/Los_Angeles`, user `admin`, email `admin@example.com`), with the stand-in running.
2. Sign the recorder in and seed the site outside the capture: enable the features, import three fake blog posts,
   create the index `BlogPostsAI`, and wait until the stand-in holds the posts.

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/azure-ai-search-queries/recorder
   export VIDEO_ROOT=<repo>/videos/azure-ai-search-queries VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside (snapshot A): restore that copy before every take of `04-create`.
3. Narrate (`scripts/narrate.py videos/azure-ai-search-queries`), then record `04-create`. It creates the query
   BlogSearch. Give the Anonymous role the permission "Execute Api - BlogSearch", copy `App_Data` aside again
   (snapshot B), and record `05-run` from snapshot B. The other chapters are slides and cards.
