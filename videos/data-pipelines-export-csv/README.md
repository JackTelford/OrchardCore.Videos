# Data Pipelines: Export Content Items to a CSV File

**YouTube:** not published yet · **Length:** 6:08 · **Feature:** OrchardCore.DataPipelines ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/DataPipelines/))

The video designs a data pipeline in the browser that reads the blog posts of a site, keeps the published ones,
writes them to a CSV file with labeled columns, and saves the file in a folder of the media library. It then
publishes the pipeline, runs it, follows the run in the designer, and finds the file in the media library. It also
says who may publish and run pipelines.

The video is for administrators: it shows the admin screens only. The narration is the storyboard of the folder, the
slides are drawn by the slides script, and everything a person needs to record the video is in recording-plan.md.
What could not be confirmed from the source is listed in OPEN-QUESTIONS.md. The title, description, tags and
chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build the web project of Orchard Core (a checkout that has the Data Pipelines module) and run it on port 5010,
   with its own empty data folder outside both repositories. Provision it with Auto Setup: the Blog recipe, site name
   **Orchard Core**, time zone America/Los_Angeles, user **admin**, email admin@example.com, and a password chosen
   outside git. This video is not about the Setup screen, so neither the setup form nor the sign-in form is recorded.
2. Enable the **Data Pipelines** feature (Tools, Features). Leave Email, Workflows and the FTP and SFTP features
   disabled, and keep the default media options.
3. Seed the site: delete the sample blog post of the Blog recipe, then import the eight invented blog posts listed in
   the recording plan (Tools, Deployments, JSON Import): six published, two drafts. Copy the site's data folder and
   the saved sign-in aside as the first snapshot.
4. Sign the recorder in, narrate the video, and build the recorder with the video's scenes, as the recording plan
   describes.
5. Record the clips in the order they play in: 02-create, 03-source, 04-file, 05-deliver, 06-publish,
   08-run and 09-media. Each clip starts from the site the previous one left, so take a snapshot after each clip,
   and restore the snapshot that precedes a clip to record it again. The clips 01-idea and 07-access are slides
   and need no recording.
