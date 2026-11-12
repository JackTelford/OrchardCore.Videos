# Widgets: Add Content Blocks to Your Pages

**YouTube:** not published yet · **Length:** 4:49 · **Feature:** OrchardCore.Widgets ([Widgets](https://docs.orchardcore.net/en/latest/reference/modules/Widgets/))

An admin video about the Widgets module: a widget is a content item whose content type carries the Widget stereotype, and a
theme shows it in a named zone of a page. The video finds the Widgets feature, looks at the widget types of a Blog site,
attaches the WidgetsList part to the Article content type, adds a Paragraph and a Blockquote to the zones of the About
article, and then moves and removes widgets. Layers and the Flow part, other ways to place widgets, have their own videos. The title,
description, tags and chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script and no seed data of its own: the
Blog recipe creates everything the clips use (the Article content type, the About article, and the widget types).

1. Provision a local Orchard Core site with Auto Setup: recipe Blog, site name Orchard Core, time zone
   America/Los_Angeles, user admin with the email admin@example.com, and a password chosen outside git and passed to the
   recorder in VIDEO_PASSWORD. Outbound calls are blocked, as the recording guide of the skill describes.
2. Sign the recorder in once. No feature has to be enabled first: the Blog recipe turns on Widgets and Layers.
3. Copy the site's data aside before the first clip (copy S0). Two clips change the site, so make two more copies: S1 after
   the clip that allows widgets on a content type, and S2 after the clip that adds widgets to a page.
4. Narrate first, then record the clips in this order: the Widgets feature, the widget types, allow widgets on a content
   type, add widgets to a page, move and remove widgets. The first two start from S0, add widgets to a page needs S1 (the
   result of allowing widgets), and move and remove widgets needs S2 (the result of adding widgets). To take a clip again,
   restore the copy it starts from.

Everything a person needs to record without deciding anything (the page each clip starts on, the actions of every step,
the text to type, what the screen must show, and the source of each label) is in recording-plan.md. Questions that
only a running site can answer are in OPEN-QUESTIONS.md.
