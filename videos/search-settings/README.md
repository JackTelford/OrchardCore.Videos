# Site Search Settings: Index, Fields and Results Page

**YouTube:** not published yet · **Length:** 3:55 · **Feature:** OrchardCore.Search ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Search/))

The video covers the Site Search settings of the Search module: choosing the default index that the search page
queries, setting the placeholder text of the search box, trying the search page and its results, naming another index
in the address, and giving the Anonymous role permission to query an index. The title, description, tags and chapters
YouTube shows are kept in the video's metadata file.

## Recording it again

Record the video on a **fresh site** with fake data only; it needs no mask script.

1. Run the Blog recipe on a fresh Orchard Core site with Auto Setup: site name **Orchard Core**, time zone
   America/Los_Angeles, user **admin**, on port 5010, with outbound calls blocked and its data folder outside both
   repositories. The password is chosen outside git and passed to the recorder when it signs in.
2. Seed the site once, outside the capture, with the recipe printed in recording-plan.md. It enables the Search and
   Lucene features, adds three invented blog posts, and creates two Lucene indexes, **Posts** and **Pages**. Nothing
   else is set: the Site Search page has no default index and no placeholder, and the Anonymous role has no query
   permission. Copy the data folder aside as the seed snapshot.
3. Narrate first, then record the four clips in this order: **02-default** (from the seed snapshot), **03-search**,
   **05-placeholder** and **07-permissions**. Copy the data folder aside again after **02-default**, and restore that
   copy before **05-placeholder**, because its Placeholder must start empty. **02-default**, **05-placeholder** and
   **07-permissions** change the site; **03-search** does not.

recording-plan.md lists every step of every clip with the exact actions, what is on screen when its narration ends,
and where each label comes from in the Orchard Core source. The slides need no site.
