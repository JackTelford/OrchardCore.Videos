# Admin Settings: Tailor the Admin for Your Editors

**YouTube:** not published yet · **Length:** 3:31 · **Feature:** OrchardCore.Admin ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Admin/))

One settings page, Settings, Admin, changes how the admin menu, the top bar and the lists behave for everyone who uses
the admin of a site. The video goes through each option: the Filter box and the "New" entry of the menu and how its
sections open, the quick navigation button, the theme toggler, page titles and the breadcrumb trail, and the layout
of lists and of their row actions. The title, description, tags and chapters YouTube shows are in the video's
metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan in this
folder holds every action of every clip, the exact seed data, and what must be on screen when each step ends. The
commands of the recorder are in the skill's own documentation.

1. Run the CMS host project of Orchard Core with an empty data folder outside both repositories, on port 5010, set
   up by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, the user
   admin and the email admin@example.com. The password is chosen outside git and passed to the recorder in an
   environment variable.
2. Enable no feature: the Blog recipe has what the video needs. Leave Content Quick Navigation and Admin Dashboard
   disabled. Create the six seed content items of the recording plan (two pages and four articles, one of them left as
   a draft). Then stop the site and copy its data folder aside: that copy is the state every full take starts from.
3. Sign the recorder in, narrate the video, and record the clips in the order they play: 02-open, 03-menu,
   04-topbar, 05-lists. Each clip starts from the settings the one before it saved, so after the first one every clip
   changes the site. To record one clip again, restore the copy of the data folder, set the options to the clip's
   start state (the table in the recording plan), and record it.
4. The one slide of the video is drawn from the slides file of this folder when the video is assembled.
