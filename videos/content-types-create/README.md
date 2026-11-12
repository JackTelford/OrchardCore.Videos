# Content Types: Create a Type and Add Parts and Fields

**YouTube:** not published yet · **Length:** 5:23 · **Feature:** [OrchardCore.ContentTypes](https://docs.orchardcore.net/en/latest/reference/modules/ContentTypes/)

An admin video for site owners and editors. It builds an Event type in the admin from start to finish: it explains how a
content type is built from parts and fields, creates the type with a Display Name and a Technical Name, attaches the Title
part, adds a Text, a Date Time and a Numeric field (configuring one of them), sets the order that editors see, and creates
an item with the type. The title, description, tags and chapters YouTube shows are in the metadata file of this folder.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script and no seed data: the Event
type, its part, its fields and its item are all made on camera. The step-by-step actions, the labels and their sources,
the commands that run the recorder, and what must be on screen at the end of each step are in recording-plan.md; the
points that a person must confirm on a running site are in OPEN-QUESTIONS.md.

1. Run a build of Orchard Core on http://localhost:5010 with its own empty data folder outside both repositories,
   provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user admin
   with the email admin@example.com. The Blog recipe enables every feature the video uses, so nothing is enabled by hand.
2. Sign the recorder in once, outside the capture, as recording-plan.md describes. Stop the site and copy its data folder
   aside as the first snapshot before the first clip.
3. Narrate, then record the clips in this order: 02-create, 03-parts, 04-fields, 05-order, 06-item. Every one of
   them changes the site, so copy the data folder aside after each clip and restore the snapshot before the clip to
   record it again. The slide chapters (01-model, 07-recap) are drawn from slides.py and need no site.
