# URL Rewriting: Redirect and Rewrite Requests

**YouTube:** not published yet · **Length:** 5:26 · **Feature:** [OrchardCore.UrlRewriting](https://docs.orchardcore.net/en/latest/reference/modules/UrlRewriting/)

This video shows how a site owner turns on the URL Rewriting feature and manages its rules in the admin. It explains the
difference between a Redirect rule, which sends the visitor to another address, and a Rewrite rule, which serves the
content of another address while the address in the browser stays the same. It creates one of each, tests both on the
front end, and then edits, reorders and deletes rules. The title, description, tags and chapters YouTube shows are in
the metadata file next to this one.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan
(recording-plan.md) gives every value, locator and wait; this is the outline.

1. Run the web project of Orchard Core with its own empty data folder outside both repositories, on port 5010, provisioned
   by Auto Setup with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the user admin
   (admin@example.com). The password is chosen outside git and passed to the recorder in the environment. Block outbound
   calls with a dead proxy address.
2. Sign the recorder in once, then stop the site and copy its data folder and the sign-in file aside. This is snapshot S0:
   the URL Rewriting feature is **disabled**, because the Blog recipe does not enable it. The video enables it on camera.
3. Nothing needs seeding. The Blog recipe provides the About page at /about and the media file about-bg.jpg, which
   the two rules and their tests use. The rules themselves are created on camera.
4. Narrate the video, then record the clips in the order of the plan: enable, redirect, redirect test, rewrite, rewrite
   test, manage. The enable, redirect, rewrite and manage clips change the site, so restore the snapshot that precedes a
   clip before taking it again. The plan lists the snapshots (S0 to S3) and which clip needs which.
5. The recorder shows no address bar, so the test clips draw a green label with the address the browser ended on. The plan
   gives the exact text of each label.
