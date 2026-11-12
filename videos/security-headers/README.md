# Security Headers: Content Security Policy and More

**YouTube:** not published yet · **Length:** 5:07 · **Feature:** [OrchardCore.Security](https://docs.orchardcore.net/en/latest/reference/modules/Security/)

The video explains what security headers are and which four the Security module sends. It then opens Settings, Security,
Security Headers in the admin and sets the three policies that have a tab there: Content Security Policy, Permissions
Policy and Referrer Policy. It closes with the two permissions that give access to the page and the settings a developer
can make in the configuration of the site. The title, description, tags and chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The recording plan next to this
file lists every clip, every action and the checks to run; this is a summary.

1. Run the web project of Orchard Core (the commit the catalog was read from, or a later main) on port 5010, with an
   empty App_Data folder outside both repositories. Provision it with Auto Setup: the Blog recipe, site name Orchard
   Core, time zone America/Los_Angeles, user admin, email admin@example.com. The password is chosen outside git and
   passed to the recorder; it is never written in a file. The Blog recipe turns the Security feature on, so no feature
   is enabled by hand and no content is created by hand.
2. Sign the recorder in, stop the site, and copy its App_Data folder and the sign-in aside. This is the snapshot of the
   default settings.
3. Narrate, then record the clips in this order: the one that opens the settings, the Content Security Policy clip, the
   Permissions Policy clip and the Referrer Policy clip. The last three each save settings, so restore the snapshot
   before each of them, the first take included.
4. After each saving clip, check the headers the site sends with the probe described in the recording plan, outside the
   capture. The open questions file lists what a person must confirm on the running site.
