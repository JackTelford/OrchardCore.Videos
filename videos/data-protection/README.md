# Data Protection Keys: Share Them Across Servers

**YouTube:** not published yet · **Length:** 5:23 · **Feature:** `OrchardCore.DataProtection.Azure` ([Data Protection (Azure Storage)](https://docs.orchardcore.net/en/latest/reference/modules/DataProtection.Azure/)), with `OrchardCore.Redis.DataProtection` ([Redis](https://docs.orchardcore.net/en/latest/reference/modules/Redis/)) and the [Data Protection guide](https://docs.orchardcore.net/en/latest/guides/data-protection/)

A video for module and theme authors and for the developers who deploy Orchard Core. It explains what Data Protection
keys protect, why the default key ring in the local folder of each tenant fails with several servers, and how the
Distributed Data Protection (Azure Storage) and Distributed Data Protection (Redis) features keep the key ring in a
shared store. One chapter is recorded in a browser (enabling the feature under Tools, Features); the settings, the
layout of the store and the code of the features are slides. The title, description, tags and chapters YouTube shows
are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The complete steps, selectors
and checks are in `recording-plan.md`; the facts a person must confirm on a running site are in `OPEN-QUESTIONS.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). **Give the site no
   Azure Blob Storage connection string and no Redis configuration**: enabling the feature with a connection string
   switches the tenant to an empty key ring, and the saved sign-in is expected to stop working.
2. Sign the recorder in, once, outside the capture:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/data-protection/recorder
   export VIDEO_ROOT=<repo>/videos/data-protection VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` and `state.json` aside. Restore that copy before every take: the clip enables a
   feature.
3. Narrate and record: `scripts/narrate.py videos/data-protection`, then `Recorder record 03-enable`. The video has one
   recording, `03-enable`; the other chapters are slides drawn from `slides.py` by `assemble.py`. No seed data is
   needed, and no external service runs during the capture (Azurite in Docker is only used for the optional check
   described in the plan).
