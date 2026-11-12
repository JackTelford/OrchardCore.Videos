# Write a Workflow Activity

**YouTube:** not published yet · **Length:** 5:55 · **Feature:** `OrchardCore.Workflows` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/))

This video is for module authors. It reads the Notify Task activity of the Workflows module, part by part, in the order you write an activity: the class with its services, settings, outcomes and work, the display driver and its view model, the Thumbnail, Design and Edit views, settings with expressions, and the registration. It then finds each view in the workflow designer, and ends with event activities and the call that triggers them. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Only the chapter "In the designer" is a recording (clip `06-designer`); the other chapters are slides drawn from `slides.py`. The clip uses the Notify Task activity that the Workflows module provides, so the site needs no custom module. The exact steps, selectors and checks are in `recording-plan.md`; open questions for the person who records are in `OPEN-QUESTIONS.md`.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, a password you choose outside git).
2. In **Tools**, **Features**, enable **Workflows**. Leave the other Workflows features and Notifications disabled.
3. In **Design**, **Workflows**, create a workflow named **Notify editors** that starts with a Content Published Event, and leave it as a draft. Stop the site and copy its `App_Data` folder aside: restore that copy before every take.
4. Sign the recorder in and record the clip:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/workflow-activity-create/recorder
   export VIDEO_ROOT=<repo>/videos/workflow-activity-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

5. Narrate and record: `scripts/narrate.py videos/workflow-activity-create`, then `Recorder record 06-designer`. The clip changes the draft of the workflow, so it is the only clip to restore the snapshot for.
