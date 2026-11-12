# Azure Media Storage: Keep Media in Azure Blob Storage

**YouTube:** not published yet · **Length:** 5:38 · **Feature:** `OrchardCore.Media.Azure.Storage` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Media.Azure/))

A video for developers and hosting engineers on the Azure Media module. It shows why media can live in an Azure Blob
Storage container, how to configure the connection and run against the Azurite emulator, how to enable Azure Media
Storage and follow an uploaded file to the browser, what the feature registers, how Azure Media Image Cache keeps resized
images in a container, and how containers, tenant removal, and Gen2 storage accounts are configured. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The full plan, with the exact
actions of every step, is in `recording-plan.md`; what a person must still confirm on a running site is in
`OPEN-QUESTIONS.md`.

1. Start the Azurite emulator in Docker with its data in a folder outside git (the command is in `recording-plan.md`).
2. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). Set the four
   `OrchardCore__Media__Azure__...` environment variables of the plan, which point both features at the emulator. Neither
   feature is enabled before recording.
3. Copy `home-bg.jpg` from the Blog theme of OrchardCore into a folder outside git. It is the only file uploaded.
4. Sign the recorder in (the password comes from `VIDEO_PASSWORD`, chosen outside git), then stop the site and copy
   `App_Data` and the emulator's data folder aside as snapshot `S0`. The plan names the snapshots `S1` and `S2`; restore the
   one before a clip, and delete the `ms-cache` and `media-cache` folders of the web root, before each take.
5. Narrate and record: `scripts/narrate.py videos/media-azure-blob-storage`, then `Recorder record 03-enable 03-check
   04-serve 06-cache`, in that order. `03-check` only reads; `03-enable`, `04-serve` and `06-cache` change the site, and
   `04-serve` and `06-cache` depend on the clips before them.
