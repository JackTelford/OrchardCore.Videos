# HTTP Workflows: Handle Requests and Call Web APIs

**YouTube:** not published yet · **Length:** 5:35 · **Feature:** `OrchardCore.Workflows.Http` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Scripting/))

The video shows how the HTTP Workflows Activities feature connects a workflow to the web. A workflow starts from a POST
request, reads its JSON body in a script, calls a stock API with an HTTP Request task, and replies to the caller with an
HTTP Response. It then covers signals, the Filter Incoming HTTP Request event, HTTP Redirect, and moving a workflow to
another site. The activities are also described in the [Workflows documentation](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/).
The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only (one workflow and two product codes), so it needs no mask
script. [recording-plan.md](recording-plan.md) holds every action of every clip and the seed workflow as JSON.
[OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what a person must confirm on a running site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`). Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, and
   `NO_PROXY=localhost,127.0.0.1` so the site can reach the local stock API.
2. Start the stand-in stock API (`python -m http.server 5080 --bind 127.0.0.1` in a throwaway folder holding
   `A-100.json`), enable **Workflows** and **HTTP Workflows Activities** on Tools > Features, and import the **Stock
   check** workflow with Tools > Deployments > **JSON Import**, as `recording-plan.md` describes.
3. Sign the recorder in, and save the site's `App_Data` aside:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/workflows-http/recorder
   export VIDEO_ROOT=<repo>/videos/workflows-http VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

4. Narrate and record: `scripts/narrate.py videos/workflows-http`, then `Recorder record all`. The four recordings
   (`02-activities`, `03-receive`, `05-call`, `06-run`) do not depend on each other. Record them in that order, and restore
   the saved `App_Data` before a full take: `06-run` adds workflow instances and needs the stock API running.
