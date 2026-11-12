# Search: Add Site Search to Your Site

**YouTube:** not published yet · **Length:** 4:42 · **Feature:** OrchardCore.Search ([Search](https://docs.orchardcore.net/en/latest/reference/modules/Search/))

Site search in Orchard Core rests on four pieces: a search provider, an index of the content, the Search feature and a
permission that lets visitors query the index. The video explains these four pieces with slides, then enables the Lucene and
Search features, creates an index of Blog Post and Article content, chooses it as the default index in the Site Search
settings, grants the query permission to the Anonymous role and adds a search box with the Search Form widget. It ends by
searching as a visitor who is not signed in, and names the actions that keep an index in step with the content. The title,
description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan holds every
step, with the labels, the waits and the source of each label. The open questions file lists what to confirm on the running
site before a take.

1. Run Orchard Core on its own empty data folder, outside both repositories, on port 5010, provisioned by Auto Setup: the
   Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, the user admin with the e-mail
   admin@example.com, and a password chosen outside git. The Search and Lucene features stay disabled, because the first
   clip enables them.
2. Sign the recorder in once. There is no prepare command: the three fake blog posts of the plan are imported once with
   **Tools**, **Deployments**, **JSON Import**, and then the site is stopped and its data folder is copied aside.
3. Narrate, then record the clips in the order of the chapters: Enable the features, Create an index, Choose the default
   index, Let visitors search, Add a search box, and Search as a visitor. They depend on each other. Every clip except the
   last changes the site, so copy the data folder aside after each one and restore the previous copy before recording a clip
   again. The last clip is recorded signed out, so it does not use the saved sign-in.
