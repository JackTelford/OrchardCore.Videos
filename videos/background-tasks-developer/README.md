# Write a Background Task: Schedules, Locks and Monitoring

**YouTube:** not published yet · **Length:** 5:58 · **Feature:** `OrchardCore.BackgroundTasks` ([Background Tasks](https://docs.orchardcore.net/en/latest/reference/modules/BackgroundTasks/))

A video for module authors. It shows how the background service runs the tasks of a tenant, how to write a task with
`IBackgroundTask` and the `BackgroundTask` attribute, how to register it, and where its title, description and schedule
show up in the admin. It then covers atomic tasks and distributed locks on a web farm, the `BackgroundService`
configuration, `IBackgroundTaskEventHandler`, and `IBackgroundTaskMonitor`. The code on the slides is taken from the
source of Orchard Core and the documentation. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video has one recording, `05-admin`, made on a **fresh site** with no seed data. The task it shows,
**Resized Media Cache Cleaner**, comes with the Media module of the Blog recipe. The full steps, with the exact actions
of every step, are in `recording-plan.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Sign the recorder in and prepare the site:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/background-tasks-developer/recorder
   export VIDEO_ROOT=<repo>/videos/background-tasks-developer VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare     # enables the Background Tasks feature, then waits until the background service lists the task
   ```

   Stop the site and copy its `App_Data` aside: restore that copy before every take.
3. Narrate and record: `scripts/narrate.py videos/background-tasks-developer`, then `Recorder record 05-admin`. The
   other chapters are slides drawn by `slides.py`. The recording changes nothing in the database; restart the site
   before each further take, so the execution state of the task starts empty. Do not record within five minutes of midnight in
   the time zone of the site, when the task is scheduled.
