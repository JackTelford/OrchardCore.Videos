# Debugging Settings: Show Errors While You Build a Site

**YouTube:** not published yet · **Length:** 3:40 · **Feature:** [`OrchardCore.Settings`](https://docs.orchardcore.net/en/latest/reference/modules/Settings/)

The video is for site owners and editors. It explains that Orchard Core builds a page from shapes, turns on Write shape
debug information under Settings, Debugging, and reads the comments that the option writes around each shape in the
source of a page. It then turns the option off, and covers the permissions that open the page and the related Resource
Debug Mode option of the General settings. The title, description, tags and chapters YouTube shows are in video.json;
recording-plan.md tells the person who records it what to do, step by step, and OPEN-QUESTIONS.md lists what to confirm
on a running site.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Run a local Orchard Core site with its own empty data folder outside both repositories, provisioned by Auto Setup
   with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, the user admin, the email
   admin@example.com and a password chosen outside git. recording-plan.md lists the exact values.
2. No feature needs to be enabled and no data needs to be seeded: the Settings module is always enabled, and the Write
   shape debug information option is off on a fresh site. Sign the recorder in once, stop the site, and copy its data
   folder aside as snapshot A (option off).
3. Narrate, then record the three recordings in this order: the one that turns the option on (stop the site afterwards
   and copy the data folder aside as snapshot B, option on), the one that reads the comments in the page source (it needs
   the option on), and the one that turns the option off. The two sets of slides need no site. To take the first
   recording again, restore snapshot A; to take either of the other two again, restore snapshot B. recording-plan.md has
   the details of every step, including how the page source is shown in a headless browser.
