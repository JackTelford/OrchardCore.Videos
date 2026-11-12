# Taxonomy Field: Categories and Tags on Your Content

**YouTube:** not published yet · **Length:** 5:00 · **Feature:** [OrchardCore.Taxonomies](https://docs.orchardcore.net/en/latest/reference/modules/Taxonomies/)

This video is for site owners and editors. It shows how a content type uses a taxonomy: it adds a Category field with the
Standard editor (the Unique and Leaves only settings) and a Tags field with the Tags editor (the Open setting and the Tags
display mode) to Blog Post, then chooses a category, picks a tag and creates a tag while editing a post, and shows the
result on the site. The title, description, tags and chapters YouTube shows are in video.json.

The Blog recipe gives Blog Post its own Tags and Category fields; the seed of the recording removes them, so that the video
can add them.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact steps, the selectors,
the seed data and the checks for private data are in recording-plan.md; in short:

1. Run a site of Orchard Core on a local port (5010), with its own data folder outside both repositories, provisioned by
   Auto Setup with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the user admin
   (admin@example.com) with a password chosen outside git and given to the recorder through the environment.
2. Sign the recorder in. Then seed the site by hand, as recording-plan.md describes: remove the Tags and Category fields
   from Blog Post, add the terms Trains (under Travel) and Food to the Categories taxonomy, each with an icon, publish the
   taxonomy, and create the post "A Night Train Across the Alps". Copy the site's data folder aside, outside the
   repositories, as snapshot A.
3. Narrate the video, then record the clips in this order: the Category field, the Tags field, editing the post, and the
   site. Before editing the post, copy the data folder aside again as snapshot B.
4. Three clips change the site: the first two each add a field, and editing the post creates a tag and publishes the post.
   To record one of them again, restore the snapshot taken before it: A for the Category field; for the Tags field, a copy
   taken after the Category field, or A with the Category field recorded again first; B for editing the post. The last
   clip only reads.
5. Look at every contact sheet, and scan the result for private data before publishing.
