# Lists: Ordering, Paging and Contained Types

**YouTube:** not published yet · **Length:** 4:28 · **Feature:** OrchardCore.Lists ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Lists/))

This video shows the settings of the List part of a content type, which decide what a list accepts and how it presents its
items: the contained content types, the Page Size, manual ordering by dragging, a full pager with numbered pages, and a
header with the title of the list. It follows a blog with eight posts, in the admin and on the site. The title,
description, tags and chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script.

1. Run the web host of Orchard Core on port 5010, with its own App_Data folder outside both repositories, provisioned by
   Auto Setup: the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the user admin with the
   email admin@example.com. No feature needs to be enabled by hand. Leave the admin list layout, the layout selector and
   the pager options at their defaults.
2. Create the seed: import the recipe that recording-plan.md holds, through Tools, Deployments, JSON Import. It adds a
   blog named Garden Notes, at the address garden-notes, with eight posts created one week apart.
3. Sign the recorder in as admin (the password is chosen outside git and passed to the recorder), and copy the App_Data
   folder and the sign-in aside as the first snapshot.
4. Narrate the video, then record the clips in this order: 02-open, 03-types, 04-size, 05-ordering and 06-display. Every
   clip from 03-types on changes the settings of the List part of the Blog type, so each one starts from the snapshot
   its predecessor left: restore it before another take, and take a snapshot after each clip. The cards and the slides of
   the first chapter need no site.
5. OPEN-QUESTIONS.md lists what a person must confirm on the running site, first of all that dropping a row on the
   second page in 05-ordering saves its order.
