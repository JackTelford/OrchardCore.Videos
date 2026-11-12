# Admin Dashboard: Build a Start Page for Your Editors

**YouTube:** not published yet · **Length:** 4:38 · **Feature:** [OrchardCore.AdminDashboard](https://docs.orchardcore.net/en/latest/reference/modules/AdminDashboard/)

The video replaces the home page of the Orchard Core admin with a dashboard of widgets. It enables the Admin Dashboard
feature, adds an Html Dashboard Widget, arranges the widgets by dragging and resizing them, and shows the Actions menu
of a published widget and of a draft. It then explains the two permissions and the right to view a widget that decide
who sees the dashboard, and how any content type can become a widget type. The title, description, tags and chapters
YouTube shows are in the metadata file of this folder.

## Recording it again

The video is recorded on a **fresh site** with invented data only, so it needs no mask script. The file
recording-plan.md in this folder holds every action, selector, source reference and command; this is the summary.

1. Provision a site with Auto Setup from a checkout of OrchardCore that has the Admin Dashboard module: the Blog
   recipe, site name **Orchard Core**, time zone America/Los_Angeles, user **admin** with the email
   admin@example.com and a password chosen outside git, on a free local port, with an empty application data folder
   outside both repositories. Admin Dashboard stays disabled: the first recording enables it. No other feature is
   enabled before recording, and the video has no preparation step.
2. Sign the recorder in once, outside the capture, as described in recording-plan.md.
3. Narrate the video, then record in this order, because each recording starts from the state the previous one left:
   the enable recording, the add recording, then the three seed widgets (Style reminders, Office hours, and Holiday
   schedule, which is saved as a draft), then the arrange recording. The slides and the two cards need no recording.
4. Copy the application data folder and the sign-in aside before the enable recording, after it, and after the seed
   widgets. Restore the matching copy before recording one of these clips again, since each of them changes the site.
