# Health Checks: Let Your Host Know the Site Is Up

**YouTube:** not published yet · **Length:** 3:51 · **Feature:** `OrchardCore.HealthChecks` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/HealthChecks/))

A video for module and theme authors about the Health Checks module. It explains why a load balancer, a container
platform or an uptime monitor probes a site, enables the feature and opens the endpoint, adds checks from Orchard Core
features and from custom code, reads the compact and the detailed response, and shows a check failing with the Redis
feature and no server. It ends with how to keep the endpoint private. The title, description, tags and chapters
YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with no seed data, so it needs no mask script. Only the admin Features page
and the text of the endpoint appear on screen. The full steps, with every action, are in `recording-plan.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore (a checkout that has the Health Checks module) and run it with
   `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by
   Auto Setup (the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). Health Checks
   and Redis start disabled, and no `HealthChecks` or `Redis` configuration is set.
2. Sign the recorder in. The video has no `PrepareAsync`, because there is nothing to seed:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/health-checks/recorder
   export VIDEO_ROOT=<repo>/videos/health-checks VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside (snapshot A).
3. Narrate with `scripts/narrate.py videos/health-checks`, then record in this order, taking a snapshot after each
   clip that changes the site:
   - `02-enable` enables Health Checks; `05-compact` enables Redis. Record both in one run
     (`Recorder record 02-enable 05-compact`), then stop the site and copy `App_Data` aside (snapshot C). To record
     `05-compact` alone again, restore snapshot A and record both clips again.
   - `05-detailed` needs the site restarted from snapshot C with `OrchardCore__HealthChecks__ShowDetails=true`.
     Then `Recorder record 05-detailed`.
