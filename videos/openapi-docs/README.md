# OpenAPI: Document Your APIs With Swagger, ReDoc and Scalar

**YouTube:** not published yet · **Length:** 4:29 · **Feature:** `OrchardCore.OpenApi` ([OpenAPI documentation](https://docs.orchardcore.net/en/latest/reference/modules/OpenApi/))

The OpenApi module builds an OpenAPI document from the endpoints of a site and shows it in three explorers: Swagger UI,
ReDoc and Scalar. The video is for developers. It enables the four features, explains who can open the explorers and how
Swagger UI and Scalar sign in to the API, browses and tries out an operation in Swagger UI, opens ReDoc and Scalar, shows the
schema access setting, and ends with the code of a documented controller and a named minimal API endpoint. The title, description,
tags and chapters YouTube shows are in `video.json`; the step-by-step actions for the person recording are in
`recording-plan.md`, and what still needs a check on a running site is in `OPEN-QUESTIONS.md`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of Orchard Core and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010` (plain HTTP), provisioned by Auto Setup: the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`. The Blog recipe enables
   Contents, Media and Queries, so the explorers have operations to list. None of the OpenApi or OpenID Connect
   features is enabled before the recording.
2. Sign the recorder in. This video has no `PrepareAsync`: the only seed is the id of the Blog content item, and the
   provisioning in the recording plan.

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/openapi-docs/recorder
   export VIDEO_ROOT=<repo>/videos/openapi-docs VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside as snapshot S0.
3. Narrate with `scripts/narrate.py videos/openapi-docs`, then record the clips in this order:
   - `02-enable` first, from snapshot S0. It enables the four OpenApi features, so it changes the site.
   - Then the provisioning of the recording plan (the OpenAPI Documentation recipe, and the redirect address of its
     OpenID application), outside the capture. Stop the site and copy its `App_Data` aside as snapshot S1.
   - Then `04-swagger` and `05-explorers`, from snapshot S1. They read only.
   - Last `06-schema`, from snapshot S1. It saves a setting and restarts the tenant.
   - To record a clip again, restore the snapshot named above: S0 for `02-enable`, S1 for the others.
