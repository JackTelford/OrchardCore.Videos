# Lucene Query Syntax: Write Precise Search Queries

**YouTube:** not published yet · **Length:** 4:58 · **Feature:** `guide:guides/lucene-query-syntax` ([Lucene Query Syntax](https://docs.orchardcore.net/en/latest/guides/lucene-query-syntax/))

A developer video about the text a visitor types into the search page of an Orchard Core site. It shows how the Lucene
search service parses that text with a `MultiFieldQueryParser`, using the default fields and the analyzer of the
index, and then writes each kind of query on the search page of a blog: Boolean operators, phrases and proximity,
wildcards, fuzzy terms and regular expressions, fields, groups and ranges. It ends with the same syntax in a
`query_string` query, and the mistakes that cause surprises. The title, description, tags and chapters YouTube shows
are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Every step, query and
expected result is in [recording-plan.md](recording-plan.md); questions that a person must confirm on a running site
are in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Build `src/OrchardCore.Cms.Web` of Orchard Core and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/lucene-query-syntax/recorder
   export VIDEO_ROOT=<repo>/videos/lucene-query-syntax VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Seed the site as the plan describes: import the six fake blog posts with **Tools** > **Deployments** > **JSON
   Import**, run the recipe **Blog - Lucene Search** from **Tools** > **Recipes**, and check the table of results.
   Stop the site and copy its `App_Data` folder aside: restore that copy before a full take.
4. Narrate and record: `scripts/narrate.py videos/lucene-query-syntax`, then `Recorder record all`. The four clips
   (`02-operators`, `03-phrases`, `04-wildcards`, `05-fields`) only search, so none depends on another and they can be
   recorded in any order from the same snapshot. The other chapters are slides and cards.
