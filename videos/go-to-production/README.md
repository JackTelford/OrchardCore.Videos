# Go to Production: A Checklist for a Live Orchard Core Site

**YouTube:** not published yet · **Length:** 6:51 · **Feature:** `guide:topics/configuration` ([Configuration](https://docs.orchardcore.net/en/latest/topics/configuration/))

A checklist for developers who take an Orchard Core site live. It goes through the environment and the configuration,
the Reverse Proxy Configuration and HTTPS features, and the data protection keys. Two chapters are recordings of
admin pages, and the others are slides with the code and configuration behind them. The title, description, tags and
chapters YouTube shows are in `video.json`.

Documentation pages the video backs: [Configuration](https://docs.orchardcore.net/en/latest/topics/configuration/),
[HTTPS](https://docs.orchardcore.net/en/latest/reference/modules/Https/),
[Reverse Proxy](https://docs.orchardcore.net/en/latest/reference/modules/ReverseProxy/) and
[Data Protection](https://docs.orchardcore.net/en/latest/guides/data-protection/).

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Only two chapters need a
browser: `03-proxy` (Settings, Reverse Proxy) and `04-https` (Settings, Security, HTTPS). Cards and slides are drawn
from `storyboard.json` and `slides.py`. [recording-plan.md](recording-plan.md) has every action, selector and source
reference, and [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what a person must still confirm on a running site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010` over plain HTTP, provisioned by Auto Setup (the Blog recipe,
   site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, and a password
   passed as `VIDEO_PASSWORD`, never written in a file). The exact variables are in the plan.
2. In **Tools, Features**, enable **HTTPS** and **Reverse Proxy Configuration**. No setting is saved and no content
   is created before recording: both settings pages start in their default state.
3. Stop the site and copy its `App_Data` folder and `state.json` aside. Restore that copy before every full take.
4. Sign the recorder in and record:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/go-to-production/recorder
   export VIDEO_ROOT=<repo>/videos/go-to-production VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

5. Narrate with `scripts/narrate.py videos/go-to-production`, then `Recorder record 03-proxy 04-https`.

`03-proxy` saves the Reverse Proxy settings, which restarts the tenant, so restore the copy of `App_Data` before another
take of it. `04-https` changes nothing: it opens the HTTPS page over HTTP and saves nothing. The two clips do not
depend on each other.
