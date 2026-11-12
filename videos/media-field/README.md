# Media Field: Attach Images and Files to Content

**YouTube:** not published yet · **Length:** 5:38 · **Feature:** `OrchardCore.Media` ([Media](https://docs.orchardcore.net/en/latest/reference/modules/Media/))

The video shows the Media Field of the Media module for site owners and editors. It adds a field to the Page content
type, explains its settings (Required, Multiple, media text, anchors and allowed file types), and attaches files
with the Select Media dialog, with a description and a focal point for each. It then switches the field to the
Gallery editor and adds a second field that uses the Attached editor. The title, description, tags and chapters
that YouTube shows are kept in the video's metadata file, which also lists the pages that embed it (the
[Media Picker](https://docs.orchardcore.net/en/latest/topics/media/media-picker/) page first).

## Recording it again

The video is recorded on a **fresh site** with invented data only, so it needs no mask script. The plan for every
clip, with the exact actions and the source of each label, is in [recording-plan.md](recording-plan.md), and the
commands are in its last section. What the source could not settle is in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Start a throwaway Orchard Core site on the Blog recipe with Auto Setup: site name Orchard Core, time zone
   America/Los_Angeles, user admin with the email admin@example.com. The Blog recipe enables the Media feature, so no
   feature is enabled by hand. The first section of the plan lists the exact settings.
2. Seed the media library outside the capture: make three pictures and one PDF with the script of the plan, create a
   library folder named open-house, and upload the four files into it. The files are never committed.
3. Sign the recorder in with a fresh browser context after the seeding, so that no library preferences are saved. Then
   stop the site and keep a copy of its data folder as the starting snapshot.
4. Narrate, then record the clips in the order they play: 02-add, 03-settings, 04-attach, 05-gallery and 06-attached.
   Each clip changes the site and starts from the state the previous one left, so keep a copy of the data folder after
   every clip, and restore the copy taken before a clip to record it again. The clips 05-gallery and 06-attached open
   the Page that 04-attach creates, by its id (see the plan). The slides and the two cards need no recording.
