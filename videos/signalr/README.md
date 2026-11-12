# SignalR: Real-Time Updates and Backplanes

**YouTube:** not published yet · **Length:** 4:44 · **Feature:** `OrchardCore.SignalR` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/SignalR/))

A video for module developers about the SignalR module. It shows the SignalR feature and the two backplane features
on the Features page, the Media Library updating in a second tab through the media hub, and then the code a module
needs: hosting a hub with its policy, requiring the SignalR feature, sending to a group through the hub context, and
loading the client in the browser. It ends with the Redis and Azure backplanes that carry hub messages between
instances. The title, description, tags and chapters YouTube shows are in `video.json`.

The code on the slides comes from the documentation and from the Media and Workflows modules. The video records two
chapters in the browser (`02-features` and `03-live`) and draws the rest as slides.

## Recording it again

The video is recorded on a **fresh site** with the files of the Blog recipe only, so it needs no mask script.
`recording-plan.md` has the exact actions of every step, the selectors and where each label comes from.

1. Start Redis in Docker (`docker run -d --name ocvideo-redis -p 6379:6379 redis:7`).
2. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). Set
   `OrchardCore__Redis__Configuration=localhost:6379` in the environment **before** the site starts: a backplane
   stays inactive when the site starts without it.
3. Sign the recorder in, and prepare the site (enable Media SignalR, outside the capture):

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/signalr/recorder
   export VIDEO_ROOT=<repo>/videos/signalr VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare     # enables Media SignalR; its Scenes.cs defines PrepareAsync
   ```

   `recorder/Scenes.cs` is written when the clips are recorded, from the steps of `recording-plan.md`.

   Stop the site and copy its `App_Data` aside as snapshot S1.
4. Narrate and record: `scripts/narrate.py videos/signalr`, then `Recorder record 02-features`. That clip enables SignalR
   Redis Backplane, so stop the site and copy `App_Data` aside again as snapshot S2. Then `Recorder record 03-live`,
   which renames a file of the Media Library. Restore S1 before another take of `02-features`, and S2 before another
   take of `03-live`.
5. The Azure SignalR Service is not recorded: its backplane appears on slides only, with a connection string on
   `example.com`.
