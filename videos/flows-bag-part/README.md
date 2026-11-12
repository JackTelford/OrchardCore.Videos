# Bag Part: Embed Content Items Inside a Content Item

**YouTube:** not published yet · **Length:** 4:32 · **Feature:** [OrchardCore.Flows](https://docs.orchardcore.net/en/latest/reference/modules/Flow/)

This video shows the Bag part of the Flows module, a content part that holds content items of the content types you
choose inside another content item. It adds a Bag to a content type, fills the bag with items in the content editor,
reorders and removes items, shows the bag on the site, and switches the bag to the Blocks editor. It is an admin video:
it shows the admin screens and the site, and no code. The YouTube title, description, tags and chapters are kept with
the video.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The exact actions of every step,
the text to type and the labels with their places in the Orchard Core source are in the recording plan of this folder.
Points that only a running site can confirm are in the open questions of this folder.

1. Build the Orchard Core web project of a checkout with the Flows module, and run it with an empty data folder outside
   both repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, site name Orchard Core, time zone
   America/Los_Angeles, user admin with the email admin@example.com. The Blog recipe turns the Flows feature on, so no
   feature is enabled before recording.
2. Sign the recorder in as admin with the password chosen outside git, and seed the site: two content types, Question
   (a Title and a text field named Answer) and Tip (a Title and a text field named Details), imported through the JSON
   Import page. Each display name stays equal to its technical name, which the drag in 04-manage needs. The recording
   plan holds the import text and a manual alternative. Stop the site and copy its data folder aside as the first
   snapshot.
3. Narrate the video, then record the five recordings in this order: 02-add, 03-edit, 04-manage, 05-view, 06-blocks.
   Each one starts from the snapshot taken after the previous one, because 02-add creates the Help Page content type,
   03-edit creates the Account Help page, 04-manage publishes it again with the items reordered, and 06-blocks changes
   the editor of the bag. 05-view only reads the site. The four slide steps need no site.
