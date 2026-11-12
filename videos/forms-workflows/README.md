# Forms and Workflows: Process Each Submission

**YouTube:** not published yet · **Length:** 6:04 · **Feature:** `OrchardCore.Forms` ([Forms](https://docs.orchardcore.net/en/latest/reference/modules/Forms/))

A developer video about the path a form submission takes: the Form widget posts to the URL of a workflow, an HTTP
request event starts it, and the validation activities of the Forms module check the fields. The workflow then sends
an email and redirects to a thank-you page, or returns the visitor to the form with the errors and the typed values.
The slides show the code of the controller and of the filters behind it, and the
[Workflows documentation](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/) covers the designer.
The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Record the video on a **fresh site** with fake data only; it needs no mask script. `recording-plan.md` holds
every action, the exact seed data and the label sources, and `OPEN-QUESTIONS.md` lists what the source could not
settle.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Start a local SMTP catcher, such as smtp4dev in Docker, on port 2525. The workflow sends an email, and a site
   without a working email provider takes the Failed outcome of the Email Task instead of redirecting.
3. Sign the recorder in, and seed the site outside the capture: enable the Forms, Workflows, HTTP Workflows
   Activities, Email and SMTP Email Provider features, configure the SMTP provider, and create the `Contact` page with
   its Form widget (the Action stays empty) and the `Thank You` page:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/forms-workflows/recorder
   export VIDEO_ROOT=<repo>/videos/forms-workflows VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside as the first snapshot.
4. Narrate and record: `scripts/narrate.py videos/forms-workflows`, then `Recorder record 02-start 03-validate
   04-valid 05-connect 06-try` in that order. Each clip changes the site and the next one needs what it left: take a
   snapshot after each clip, and restore the one a clip needs before recording it again. The URL of the workflow is
   read in `02-start` and pasted in `05-connect`.
