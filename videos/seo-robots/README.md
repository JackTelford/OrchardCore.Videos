# Robots: Tell Search Engines What to Crawl

**YouTube:** not published yet · **Length:** 3:28 · **Feature:** OrchardCore.Seo ([SEO documentation](https://docs.orchardcore.net/en/latest/reference/modules/Seo/))

The video shows how the Robots settings of the SEO feature build the robots.txt file of a site. It enables the
feature, opens the file in the browser to see the default rules, then changes the Robots settings and adds a rule of
its own. Slides explain what else changes the file: the Sitemaps feature, and a robots.txt file that already exists
among the static files of the site. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan in this
folder holds every action of every clip, with the selectors and the commands, and the open-questions file lists what a
person must confirm on a running site.

1. Build the web project of Orchard Core (OrchardCMS/OrchardCore) and run it with an empty data folder outside both
   repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone
   America/Los_Angeles, user admin with the email admin@example.com. The password is chosen outside git. Leave the
   **SEO** feature disabled, because the video enables it on camera, and do not enable Sitemaps or Static File Provider.
2. Sign the recorder in once, outside the capture, as the recording plan describes. Stop the site and copy its data
   folder aside as snapshot A.
3. Narrate the video, then record in this order: the enable clip, a second copy of the data folder as snapshot B, the
   default clip, the settings clip. The enable and settings clips change the site: restore snapshot A before another
   take of the first, and snapshot B before another take of the second. Chapters 01 and 05 are slides and need no
   recording.
