# Workflows and the Database Session: Commit, Fork and Join

**YouTube:** not published yet · **Length:** 4:35 · **Feature:** `OrchardCore.Workflows.Session` ([Tenants documentation](https://docs.orchardcore.net/en/latest/reference/modules/Tenants/), [Workflows documentation](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/))

A video for module authors. While a workflow runs, everything its activities save goes through one database session.
It shows when that session is committed, what the Commit Transaction task of the Session Workflows Activities feature
does and how its code is registered, how Fork and Join run their branches on one session, and where to place the
commit in a workflow. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact steps of every
clip, the seed workflow to import and the labels with their sources are in [recording-plan.md](recording-plan.md);
what a person must confirm on a running site is in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/workflows-session-activities/recorder
   export VIDEO_ROOT=<repo>/videos/workflows-session-activities VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Enable **Workflows** at Tools, Features, and leave **Session Workflows Activities** disabled. Copy `App_Data` and
   `state.json` aside (snapshot A).
4. Narrate and record: `scripts/narrate.py videos/workflows-session-activities`, then the clips in this order:
   `02-enable` (it enables the Session feature), then import the "Order handoff" workflow of the recording plan and
   copy `App_Data` aside again (snapshot B), then `02-activity`, `03-workflow` and `03-run`. Restore snapshot A before
   another take of `02-enable`, and snapshot B before another take of `03-run`, which creates a workflow instance.
