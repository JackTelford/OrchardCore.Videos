# Redis: Distributed Cache, Locks, Bus and Data Protection

**YouTube:** not published yet · **Length:** 6:04 · **Feature:** `OrchardCore.Redis` ([Redis documentation](https://docs.orchardcore.net/en/latest/reference/modules/Redis/))

This video is for module and theme authors and for developers who run Orchard Core on several servers. It explains what
each server keeps for itself by default, how to connect a site to a Redis server, and what Redis Cache, Redis Bus,
Redis Lock and Distributed Data Protection (Redis) share between the servers. It ends with the code a module of your own
needs to use the connection. The title, description, tags and chapters YouTube shows are in `video.json`.

The chapters about the configuration, the four features and the code are slides. Two chapters are recorded in the browser:
enabling the features on the Features page, and checking the connection with the Redis health check.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The full steps, with the exact
actions of every step, are in [`recording-plan.md`](recording-plan.md).

1. Start a Redis server in Docker with persistence on, on port 6379 of the machine, and check that it answers a ping.
2. Build `src/OrchardCore.Cms.Web` of Orchard Core and run it on `http://localhost:5010` with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`). Set the Redis connection string, the instance prefix `MyApp:`, the
   admin commands setting and the Health Checks details setting in the environment of the site before it starts: the
   list is in `recording-plan.md`.
3. Sign the recorder in once on the fresh site, then stop the site and copy its `App_Data` and `state.json` aside as the
   first snapshot:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/redis/recorder
   export VIDEO_ROOT=<repo>/videos/redis VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   There is nothing to seed, so the scenes have no `PrepareAsync`.
4. Narrate and record: `scripts/narrate.py videos/redis`, then `Recorder record 03-enable`. The clip enables the Redis,
   Redis Cache, Redis Bus and Redis Lock features. Copy `App_Data` aside as the second snapshot, then record
   `Recorder record 04-health`, which enables Health Checks and opens the health check endpoint.
5. The clips depend on each other and are recorded in this order: `03-enable`, then `04-health`. Before every new take,
   stop the site, restore the snapshot the clip starts from, flush Redis (`redis-cli FLUSHALL` in the container), and
   start the site again.
