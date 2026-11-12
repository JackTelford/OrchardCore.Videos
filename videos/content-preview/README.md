# Content Preview: See Your Changes Before You Publish

**YouTube:** not published yet · **Length:** 3:32 · **Feature:** [OrchardCore.ContentPreview](https://docs.orchardcore.net/en/latest/reference/modules/ContentPreview/)

This video shows the two previews of a content item. Preview, in the editor, opens a separate window that follows the
form while you type, without saving anything. Preview Draft, in the content items list, opens the saved draft as a
page of the site. It also covers enabling the feature in Tools, Features, the permission that allows previewing, the
limits of live preview, and the Preview part, which sends the preview through a page of the site. The title, description,
tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. Chapters 02, 03 and 04 are browser
recordings; chapters 01 and 05 are slides, and chapters 00 and 99 are cards.

1. **The site.** Run Orchard Core from a checkout of the OrchardCore repository, with its own App_Data folder outside
   both repositories, on http://localhost:5010. Provision it with Auto Setup: the Blog recipe, site name Orchard Core,
   time zone America/Los_Angeles, user admin with the address admin@example.com, and a password chosen outside git and
   passed to the recorder in VIDEO_PASSWORD.
2. **The seed.** Create two fake Articles, "Autumn Open Day" and "Winter Workshop Series", publish both, and save a draft of
   the second with another Subtitle. Turn off the Content Preview feature in Tools, Features, stop the site, and keep a
   copy of its App_Data folder. The exact titles, subtitles and bodies are in recording-plan.md.
3. **The order.** Record the Enable Content Preview chapter first, since it turns the feature on. The live preview and the
   saved draft chapters follow in either order, and neither changes the site. To record the first chapter again, restore
   the copy of App_Data.
4. **The details.** recording-plan.md lists every step of every clip: the page it starts on, what to click and type, what
   must be on screen when the narration ends, and where each label comes from. OPEN-QUESTIONS.md lists what the author
   could not confirm without a running site.
