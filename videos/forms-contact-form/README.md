# Forms: Build a Contact Form From Widgets

**YouTube:** not published yet · **Length:** 5:46 · **Feature:** [OrchardCore.Forms](https://docs.orchardcore.net/en/latest/reference/modules/Forms/)

The Forms module provides a widget for each part of a form. The video enables the feature, creates a page, adds a Form
widget to it, and fills the form with Input, Select, Text Area, and Button widgets, each with the settings that decide
its name, its label, and its placeholder. It ends with the published form on the site. The title, description, tags and
chapters YouTube shows are in video.json.

Validation messages and the processing of a submission are not part of this video; the Forms series has a video for each.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The exact actions, selectors and
values of every step are in recording-plan.md; this is the outline.

1. Build the web project of Orchard Core at the commit named in the catalog, and run it with an empty App_Data folder
   outside both repositories, on port 5010. Provision it with Auto Setup: the Blog recipe, the site name Orchard Core,
   the time zone America/Los_Angeles, and the user admin with the email admin@example.com. The password is chosen
   outside git and never written down in the repository. Point the outbound proxy at a dead address.
2. The Forms feature stays disabled: the first clip enables it on camera. No feature has to be enabled and no data has to
   be seeded before recording.
3. Sign the recorder in once, then copy the App_Data folder and the sign-in aside as the starting snapshot.
4. Narrate, then record the clips in the order they play, because each one starts from the page the previous one left:
   Create the form, Text fields, Choice, message, and button, The form on the site. Take a snapshot after each of the
   first three, and restore the one that precedes a clip before recording it again.
5. Chapter 01 is made of slides and needs no recording.
