# Run in a Web Farm: Distributed Tenants, Workers and Shared Storage

**YouTube:** not published yet · **Length:** 6:36 · **Feature:** `OrchardCore.Tenants.Distributed` ([Tenants](https://docs.orchardcore.net/en/latest/reference/modules/Tenants/))

The video shows what Orchard Core keeps on each server of a web farm, and what keeps the servers in step: the tenant
settings shared through the Azure Shells Configuration provider, the Distributed Tenants feature with a Redis cache, and
the Indexing Worker for local Lucene indexes. It is a developer video: the idea and the code are on slides, and the
browser shows the Features page, the Tenants list of two instances, and the Background Tasks page. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` has every
command, selector and step; this is the short version.

1. **Services.** PostgreSQL, Azurite and Redis run in Docker. Create the blob container `farm-shells` on Azurite.
2. **Host.** Build a throwaway copy of `src/OrchardCore.Cms.Web` of `OrchardCMS/OrchardCore` that also calls
   `AddAzureShellsConfiguration()` and references `OrchardCore.Shells.Azure`. It stays outside git.
3. **Instances.** Instance A listens on `http://localhost:5010`, instance B on `http://localhost:5011`. They share the
   database, the container and the Redis server, and each has its own `ORCHARD_APP_DATA` folder, outside both
   repositories. Auto Setup provisions the Default tenant once, from A (recipe `SaaS`, site name **Orchard Core**, time
   zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, the password of `VIDEO_PASSWORD`).
4. **Seed.** On A, enable **Background Tasks** and **Distributed Data Protection (Redis)**, so the sign-in is valid on both
   instances. Do not enable Redis Cache, Distributed Tenants or Indexing Worker: the clips do. Sign in again, run
   `Recorder setup`, and take a snapshot of the three Docker volumes and the `App_Data` folder of A.
5. **Record, in this order:** `03-enable` on A alone, then start B and take a second snapshot, then `04-sync` (A and B in
   two tabs), `05-features` (A), and `05-task` (B, after the task is loaded). Restore the second snapshot before another
   take of any of the last three clips.

   ```bash
   export VIDEO_ROOT=<repo>/videos/web-farm VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   VIDEO_BASE_URL=http://localhost:5010 <bin>/Recorder record 03-enable
   ```

6. Narrate with `scripts/narrate.py videos/web-farm` before recording, and assemble afterwards, as `SKILL.md` describes.

Questions that need a running farm to answer are in `OPEN-QUESTIONS.md`.
