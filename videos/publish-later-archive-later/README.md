# Publish Later and Archive Later: Schedule Your Content

**YouTube:** not published yet · **Length:** 4:17 · **Feature:** [OrchardCore.PublishLater](https://docs.orchardcore.net/en/latest/reference/modules/PublishLater/) and [OrchardCore.ArchiveLater](https://docs.orchardcore.net/en/latest/reference/modules/ArchiveLater/)

The video shows how to schedule a change of state for a content item. Publish Later holds a draft until a chosen date and
time and then publishes it; Archive Later unpublishes a published item at a chosen date and time. It enables the two
features, adds their parts to the Article content type, schedules an article to go live and to come down, and then changes
and cancels a schedule. It ends with the time zone that applies, the permission the controls need, and when the change
happens. The title, description, tags and chapters YouTube shows are kept with the video's YouTube metadata.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan in this folder
holds every action, selector and source reference of each step; this section only gives the outline.

1. Run the Cms.Web project of an Orchard Core checkout with its own App_Data folder outside both repositories, on
   port 5010, provisioned by Auto Setup: recipe Blog, site name Orchard Core, time zone America/Los_Angeles, user admin,
   email admin@example.com. The password is chosen outside git and passed to the recorder as VIDEO_PASSWORD.
2. Do not enable the Publish Later and Archive Later features beforehand: the first clip enables them on camera. Sign the
   recorder in once, and take the first snapshot of the site (stop it, and copy App_Data and the sign-in aside).
3. Narrate, then record the clips in this order: the features, the parts, Publish Later, Archive Later, and the seed
   items, then the cancel clip. Each clip starts from the state the previous one left, so take a snapshot after each clip
   and restore it before a new take. The Publish Later and Archive Later clips wait a few minutes for the background task
   (cut from the video), so a full take needs about ten minutes of real time.
4. The seed data is two Article items, created outside the capture after the Archive Later clip: Holiday Hours (a draft with a
   Publish Later time) and Summer Campaign (published, with an Archive Later time). The recording plan gives their values.
