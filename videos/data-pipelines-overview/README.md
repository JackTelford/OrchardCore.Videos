# Data Pipelines: Move and Transform Your Site's Data

**YouTube:** not published yet · **Length:** 4:54 · **Feature:** [OrchardCore.DataPipelines](https://docs.orchardcore.net/en/latest/reference/modules/DataPipelines/)

An overview of the Data Pipelines module for site owners and administrators. It explains what a pipeline is (sources that
read rows, transforms that change them, files that hold them, destinations that deliver them), enables the feature, and
builds a first pipeline in the designer: it reads the Blog Post content items, writes them to a CSV file and saves the file
to the media library. It then publishes the pipeline, runs it, and reads the run in the History tab, on the run page, in Run
History and in the list of pipelines. The video ends with the other steps pipelines offer and the three permissions that
control them. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The recording plan holds every action,
selector and label of every step; this section gives the outline.

1. Run the Orchard Core web project on port 5010, with its own App_Data folder outside both repositories, set up by Auto Setup
   with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the user admin (email
   admin@example.com). The password is chosen outside git and passed to the recorder. Block outbound calls with a dead proxy
   address. The Blog recipe already enables Contents, Media and Queries; leave Data Pipelines disabled, since the first
   recording enables it on camera.
2. Sign the recorder in outside the capture, and create the four fake Blog Posts listed in the recording plan (the Blog recipe
   adds the fifth).
3. Narrate the video, then record the four clips in this order, taking a copy of App_Data before the first clip and after each
   one: enabling the feature, creating the pipeline, building its three steps, then publishing and running it. Each clip starts
   from the copy the previous clip left, so restore that copy before taking a clip again; the last clip also leaves a CSV file in
   the media library.
4. Review the contact sheet of every clip, and check the video for private data before assembling it, as the skill describes.
