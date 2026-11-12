# Data Pipelines: Schedule Runs and Read the History

**YouTube:** not published yet · **Length:** 4:24 · **Feature:** [OrchardCore.DataPipelines](https://docs.orchardcore.net/en/latest/reference/modules/DataPipelines/)

This video shows how a data pipeline runs on a schedule and how to read what each run did. It explains that a run
executes the published version of a pipeline in the background, starts one from the list of pipelines, and schedules a
pipeline with a workflow made of a Timer Event and a Run Data Pipeline Task. It then reads the Run History: the status,
version and starter of each run, the filter by status, the steps, the log, the error of a failed run, and what a run
delivered. It ends with how runs are cancelled and how long the history is kept. The title, description, tags and
chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything a person needs
to record it without deciding anything is in recording-plan.md; OPEN-QUESTIONS.md lists what to confirm on screen.

1. Run the Orchard Core CMS web project on http://localhost:5010 with its own empty App_Data folder outside both
   repositories, provisioned by Auto Setup: the Blog recipe, the site name Orchard Core, the time zone
   America/Los_Angeles, the user admin with the email admin@example.com. The password is chosen outside git and passed
   to the recorder in VIDEO_PASSWORD.
2. Enable the features Data Pipelines, Workflows and Timer Workflows Activities. The Blog recipe already enables
   Contents, Media and Deployment.
3. Seed the site as recording-plan.md says, outside the capture: a folder of the media library with a small file of
   three fake subscribers, three pipelines imported with JSON Import (two published by admin, one left as a draft), and a
   run history that holds manual runs, a failed run, and three runs started by a workflow.
4. Stop the site and copy its App_Data folder aside; restore that copy before every full take.
5. Narrate, then record the clips in this order: 02-run, 03-schedule, 04-list, 04-details. The clips 04-list and
   04-details read the history that 02-run leaves, and 03-schedule needs the temporary workflow of the seed deleted.
   The slides and the cards need no site. The recorder commands are in the skill (SKILL.md and recording.md).
