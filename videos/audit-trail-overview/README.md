# Audit Trail: See Who Changed What and When

**YouTube:** not published yet · **Length:** 5:08 · **Feature:** [OrchardCore.AuditTrail](https://docs.orchardcore.net/en/latest/reference/modules/AuditTrail/)

An overview of the Audit Trail module for site owners and administrators. The video enables the module, chooses what
it records, makes a change to an Article, and reads the log: the rows, the filters and the search, the details of one
event, and the history of one content item. It ends with the permissions, the comment on a change, and the user
events. The title, description and tags YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact actions of every
step, the labels they come from, and the checks for private data are in recording-plan.md.

1. Start a fresh site from the Orchard Core source with Auto Setup: the Blog recipe, the site name Orchard Core, the
   time zone America/Los_Angeles, the user admin with the email admin@example.com, and a password chosen outside git.
   The Audit Trail feature stays disabled: the video enables it.
2. Sign the recorder in as admin, create the user jamie.chen (a second administrator), and copy the site's data folder
   and the sign-in aside as snapshot A.
3. Record the clips in this order: 02-enable, then 03-settings. They change the site, so restore snapshot A before another
   take of either, and record 02-enable again before a new take of 03-settings.
4. Create the earlier change as jamie.chen in a private window (the Article Pruning workshop, published), and copy
   the site aside as snapshot B. Restore snapshot B before every take of 04-change.
5. Record 04-change, copy the site aside as snapshot C, then record 05-list, 06-details and 07-item. They only read the
   log, and they depend on what 04-change leaves in it.
6. Narrate and assemble as the skill describes. Slide chapters 01 and 08 need no site.
