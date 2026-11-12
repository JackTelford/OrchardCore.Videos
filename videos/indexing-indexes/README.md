# Indexes: Create, Rebuild and Reset Search Indexes

**YouTube:** not published yet · **Length:** 5:13 · **Feature:** [OrchardCore.Indexing](https://docs.orchardcore.net/en/latest/reference/modules/Indexing/) and OrchardCore.Indexing.Worker

The video shows what a search index is and what the Indexing module manages, then adds a Lucene index of the Article
content type from Search > Indexes. It explains Synchronize, Reset and Rebuild, runs them on an index, edits and
deletes an index, and acts on several indexes at once. It ends with the Indexing Worker, the background task that keeps
indexes in sync, and the Manage Indexes permission. The title, description, tags and chapters YouTube shows are kept in
the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only (the user admin, admin@example.com, and the index names
articles and pages), so it needs no mask script. The recording plan holds every action, selector and source reference,
and the commands; this is the short version.

1. Build the Orchard Core web site, run it with an empty application data folder outside both repositories on port
   5010, and provision it with Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles,
   user admin, email admin@example.com. The password is chosen outside git and passed in the VIDEO_PASSWORD variable.
2. Sign the recorder in, as the recording plan describes.
3. In Tools > Features, enable **Lucene** and **Background Tasks**, and leave **Indexing Worker** disabled. Create the
   three seed Article content items listed in the recording plan. Stop the site and copy its application data aside as
   the starting snapshot.
4. Narrate the video, then record the clips in this order: the clip that adds the index; then, off camera, the second
   index named pages, and another copy of the site; then the clips that run the actions, edit the indexes and enable the
   Indexing Worker. The clips that add the index, edit the indexes and enable the worker change the site, so restore the
   matching snapshot before another take.
