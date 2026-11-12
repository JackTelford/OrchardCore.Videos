# Taxonomies: Create a Taxonomy and Organize Its Terms

**YouTube:** not published yet · **Length:** 5:03 · **Feature:** [OrchardCore.Taxonomies](https://docs.orchardcore.net/en/latest/reference/modules/Taxonomies/)

This video is for site owners and editors. It shows how to create a taxonomy in the admin, choose the content type of its
terms, add terms at the top level and inside other terms, edit and delete terms, arrange them in a hierarchy by dragging,
and publish the taxonomy. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact steps, the selectors,
the seed data and the checks for private data are in recording-plan.md; in short:

1. Run a site of Orchard Core that has the Taxonomies module, with its own data folder outside both repositories,
   provisioned by Auto Setup with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the
   user admin (admin@example.com) with a password chosen outside git.
2. Sign the recorder in, then create the Topic content type, a content type that has only a title, with JSON Import.
   The text to import and the steps to do it outside the capture are in recording-plan.md. Take a copy of the site's
   data folder, outside the repository, as the snapshot of the starting state.
3. Narrate the video, then record the clips in this order: create, add, edit, organize, publish. Each clip starts from
   the state that the one before it left, and every clip changes the site, so take a snapshot after each clip. To record
   a clip again, restore the snapshot taken before it.
4. Look at every contact sheet, and scan the result for private data before publishing.
