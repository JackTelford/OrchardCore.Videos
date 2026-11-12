# Data Pipelines: Import Rows as Content Items

**YouTube:** not published yet · **Length:** 5:12 · **Feature:** OrchardCore.DataPipelines ([Data Pipelines](https://docs.orchardcore.net/en/latest/reference/modules/DataPipelines/))

An administrator imports the rows of a CSV file from the media library as content items. The video explains what an import
needs, then designs the pipeline on screen (Read a media file, then Save content items, with the content type, the match on
the display text and the values), publishes it, runs it, checks the content items, and runs it a second time to show that the
items are updated rather than duplicated. A closing slide covers the Rejected output, which carries the rows that cannot be
saved. The title, description, tags and chapters YouTube shows are in the metadata file of the video.

## Recording it again

The video is recorded on a fresh site with fake data only (the user admin with the email admin@example.com, five invented
products), so it needs no mask script. Every action, selector and wait is in recording-plan.md, and what a person has to
confirm on a running site is in OPEN-QUESTIONS.md.

1. Build the Orchard Core web project and run it with its data folder in an empty folder outside both repositories, on port
   5010, provisioned by Auto Setup: the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, the user
   admin with the email admin@example.com, and a password chosen outside git.
2. Seed the site, outside any capture. The recording plan gives the exact recipes and the file: enable the Data Pipelines
   feature, create the Product content type (a title, a required Price and a Category) with a JSON Import, and put a CSV file
   of five products, the last one without a price, in the imports folder of the media library. Then sign the recorder in
   with the user admin and the password, and copy the data folder of the site aside as the snapshot named seeded.
3. Narrate the video, then record the four clips in this order, because each one starts from the site the previous one left:
   02-read, 03-save, 04-run, 05-check. Restore the snapshot that precedes a clip before recording it again (seeded for
   02-read; the plan lists the snapshot to keep after each clip). The chapters 01 and 06 are slides and need no site.
