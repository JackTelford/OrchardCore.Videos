# Content API: Read and Write Content Items Over HTTP

**YouTube:** not published yet · **Length:** 5:08 · **Feature:** `OrchardCore.Contents` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Contents/))

The Contents module exposes three endpoints under `api/content` that read, create or update, and delete a single content
item as JSON. The video shows how a client authenticates with an access token from OpenID Connect, which permissions each
call needs and where they are set on a role, then makes each call on a slide and checks the result in the admin. The
title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Only two clips are recordings
of the browser, `03-role` and `07-admin`; every other step is a slide drawn by `slides.py`. The step-by-step plan, with
the exact actions, is in `recording-plan.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard
   Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Prepare the site as `recording-plan.md` describes: enable the features **OpenID Connect Authorization Server** and
   **OpenID Connect Token Validation**, allow the client credentials flow in the server settings, create the role `ApiClient` with the
   permissions *Access content via the api*, *Publish or unpublish content for others* and *Delete content for
   others*, and create the application `content-api-client` for that role. The client secret and the password are
   chosen outside git (`CLIENT_SECRET`, `VIDEO_PASSWORD`).
3. Make the API calls of the seed once from a shell (they create the articles "Planting notes", "Greenhouse log" and
   "Old notes", update the first one as published and as a draft, and delete the last one). The recorder drives a
   browser only, so these calls are not part of a clip.
4. Stop the site and copy its `App_Data` aside, with `state.json` from `Recorder setup`: restore both before a new take.
5. Narrate (`scripts/narrate.py videos/content-rest-api`), then record the two clips with the recorder:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/content-rest-api/recorder
   export VIDEO_ROOT=<repo>/videos/content-rest-api VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder record 03-role 07-admin
   ```

   Neither clip changes the site and they do not depend on each other. `07-admin` shows the state the seed leaves, so
   it is recorded before any other API call changes the items.
