# Feeds: Publish an RSS Feed of Your Content

**YouTube:** not published yet · **Length:** 3:33 · **Feature:** [OrchardCore.Feeds](https://docs.orchardcore.net/en/latest/reference/modules/Feeds/), with [OrchardCore.Lists](https://docs.orchardcore.net/en/latest/reference/modules/Lists/)

This video shows how a list of content, such as the posts of a blog, publishes an RSS feed that people follow in a feed
reader. It explains what the Feeds and Lists features do together, enables Feeds and checks that Lists is enabled,
finds the feed settings of the Blog and opens its feed, then changes how many items the feed holds, where the link in
the page points, and whether the feed is on. The YouTube title, description, tags and chapters are kept with the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything a person needs to
record it, from the exact actions to the screens to check, is in the recording plan of this folder.

1. Set up a site with the Blog setup recipe, unattended (site name **Orchard Core**, time zone America/Los_Angeles,
   user **admin**), on a local address, with its data folder outside both repositories. The password is chosen outside
   git and never written in this repository.
2. Create the fake posts listed in the recording plan, in the order given: the feed lists the items by creation date.
   Then disable the Feeds feature, so that the first clip can enable it. Stop the site and copy its data folder aside.
3. Narrate, then record the clips in the order of the chapters: the enable clip, the find clip, then the control clip.
   The enable clip changes the site: restore the copy before another take of it, and save another copy once Feeds is
   enabled. The control clip changes the feed settings: restore that second copy before each take of it.
