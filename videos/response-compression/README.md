# Response Compression: Smaller Responses With Gzip and Brotli

**YouTube:** not published yet · **Length:** 3:39 · **Feature:** [`OrchardCore.ResponseCompression`](https://docs.orchardcore.net/en/latest/reference/modules/ResponseCompression/)

The Response Compression module adds the ASP.NET Core response compression middleware to a tenant. The video explains
how a client and a server agree on a compressed response, enables the feature from Tools, Features and from a recipe,
reads the module's one `Startup` class (the services, the middleware and its order), shows how to check the result with
a request that accepts one format, and ends with when to use the module: a proxy or web server first, and the HTTPS
risk. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Only one clip, `02-enable`, is
recorded; the other chapters are cards and slides that `slides.py` draws. `recording-plan.md` has every action, locator
and label.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, in Kestrel with no proxy in front. Provision it with Auto Setup (the
   Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Sign the recorder in. The video has no seed data and no `PrepareAsync`:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/response-compression/recorder
   export VIDEO_ROOT=<repo>/videos/response-compression VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside. The feature must be disabled in that copy: restore it before every take,
   because the clip enables Response Compression.
3. Narrate and record: `scripts/narrate.py videos/response-compression`, then `Recorder record 02-enable`. The clip
   depends on nothing else, and the other chapters need no recording.
