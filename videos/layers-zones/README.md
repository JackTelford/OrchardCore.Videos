# Layers: Show Widgets on the Right Pages

**YouTube:** not published yet · **Length:** 5:39 · **Feature:** [OrchardCore.Layers](https://docs.orchardcore.net/en/latest/reference/modules/Layers/)

The video explains how zones, widgets and layers decide what a page shows. It walks through the Widgets and Layers page,
adds a zone in the settings, creates a layer with a Url condition, places a Paragraph widget in a zone on that layer, and
checks on the site which pages show it. It ends with moving a widget and with the safeguard on deleting a layer. The
title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with the content of the Blog recipe and no other data, so it needs no mask
script and no seed data.

1. Build the Orchard Core web project and run it with its data folder in an empty place outside both repositories, on
   port 5010, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user
   admin with the email admin@example.com. The Blog recipe enables Layers, Widgets and Rules, so no feature has to be
   enabled.
2. Build the recorder with the scenes of this video, and sign it in as admin with the password chosen outside git
   (recording-plan.md and the skill give the steps). Stop the site and copy its data folder aside: this is the first
   snapshot, restored before a full take.
3. Narrate, then record the clips in the order of the video: the Widgets and Layers page, the zones, the layer, the
   widget, the site, the move, and the edit and delete. The zones, the layer and the widget clips add what the later
   clips need, so a single clip is recorded again from the snapshot the plan names for it. recording-plan.md gives every
   action of every step.
