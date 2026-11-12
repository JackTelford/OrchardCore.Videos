# Drafts and Versions: Change Content Without Losing the Live Page

**YouTube:** not published yet · **Length:** 4:46 · **Feature:** [OrchardCore.Contents](https://docs.orchardcore.net/en/latest/reference/modules/Contents/)

The video explains how a content item keeps a published version and a draft, and what the Content Items list offers for
each. It saves a draft of a published page and shows that the live page does not change, previews the draft, publishes
it, discards another draft, unpublishes the page and publishes it again, and finds the items that have a draft with the
Only draft items filter. Two slides at the end cover the permissions behind each action and where older versions go.
The title, description, tags and chapters YouTube shows are in the video's metadata file in this folder.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The recording plan in this
folder (recording-plan.md) holds every action, selector, label source and shell command; this is the summary.

1. Build the CMS web project of OrchardCore and run it with its data folder pointing at an empty folder outside both
   repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, site name Orchard Core, time zone
   America/Los_Angeles, user admin with the email admin@example.com. The password is chosen outside git and passed to
   the recorder in the environment. No feature has to be enabled: the Blog recipe already enables everything the clips
   use.
2. Seed the site by hand in the admin, outside the capture. The recipe's About article stays as it is. Create the
   Article Parking Information, publish it, and save a draft of it with a longer Subtitle. Create the Article Spring
   Event and save it as a draft without publishing. The plan lists the exact titles and subtitles. Never pick the
   "and continue" choice of the Save Draft or Publish buttons.
3. Sign the recorder in, stop the site, and copy its data folder aside as the first snapshot.
4. Narrate, then record the clips in this order: the draft, publish or discard, unpublish, and find clips. Every one of
   the first three changes the About article, so restore the snapshot taken before that clip to record it again. The
   two slide clips and the two cards need no site.
