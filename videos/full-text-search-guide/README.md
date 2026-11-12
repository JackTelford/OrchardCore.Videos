# Implement Full-Text Search: From Index to Results Page

**YouTube:** not published yet · **Length:** 5:58 · **Feature:** `guide:guides/implement-fulltext-search` ([How to implement a website full-text search](https://docs.orchardcore.net/en/latest/guides/implement-fulltext-search/))

A developer video that follows the guide from the first feature to the results page. It enables the Lucene and Search
features, creates a Lucene index of Blog Post content, selects it in Site Search, grants the Anonymous role access to
it, and tries the search page. It then changes what is indexed with a custom full-text template, rebuilds the index,
shows the templates that draw the search page, and ends with the same setup as recipe steps. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The click-by-click plan,
with the seed data, is in `recording-plan.md`; questions that only a running site can answer are in `OPEN-QUESTIONS.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). The Lucene and Search features stay disabled:
   the first clip enables them on camera.
2. Create the three Blog Posts of `recording-plan.md` (invented titles, subtitles and bodies) in the sample blog, and
   publish them. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/full-text-search-guide/recorder
   export VIDEO_ROOT=<repo>/videos/full-text-search-guide VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` and `state.json` aside (snapshot S0): restore that copy before a full take.
3. Narrate and record: `scripts/narrate.py videos/full-text-search-guide`, then record the clips **in this order**, taking
   the snapshots named in `recording-plan.md`: `02-features`, `03-index`, `04-settings`, `05-permission`, `06-try`,
   `07-type`, `08-rebuild`. The clips depend on each other: each one starts from the site the previous one left, and
   `06-try` must be recorded before `07-type` because its last step needs a subtitle that is not in the index.
   The slides (chapters 01, 07, 09 and 10, and the end of chapter 03) are drawn by `slides.py` and need no recording.
