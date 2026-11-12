# Lucene: Full-Text Search With No External Service

**YouTube:** not published yet · **Length:** 5:20 · **Feature:** [OrchardCore.Lucene](https://docs.orchardcore.net/en/latest/reference/modules/Lucene/)

The video explains that the Lucene module keeps a search index in files inside the site, so full-text search needs no
other service. It enables the Lucene feature, creates an index with the Content in Lucene source, shows what the search
looks in (the Full-text section of a content type and the Lucene Index settings of its parts), and searches the site
with the Search feature. A last chapter explains how each server keeps its own copy of the index when a site runs on
several servers. The title, description, tags and chapters YouTube shows are kept in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. It is an administrator video:
every clip is a browser recording of the admin and of the site's search page.

1. Provision a site with Auto Setup, with the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles and the user admin (email admin@example.com, a password chosen outside git). The recording
   plan lists the exact settings and the commands.
2. Before the first take, enable the Search feature and import the four fake articles, both outside the
   capture. Three of them are about gardening, and the fourth, a winter reading list, is the one a search for the
   word garden must not find. The Lucene feature stays disabled: the first clip enables it, and the second clip
   creates the index.
3. Narrate, then record the clips in this order: the enable clip, the create clip, the fields clip, the search clip.
   The first two change the site, so restore the saved copy of the site before a full take. The last two change
   nothing, but they need the index that the second clip creates, filled with the four articles.
4. The two slide chapters need no recording.

The recording plan, [recording-plan.md](recording-plan.md), is the reference: it gives the starting page of every
clip, the actions of every step, what must be on screen when its narration ends, where each label comes from in the
Orchard Core source, and the checks for private data. [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what a person must
confirm on a running site.
