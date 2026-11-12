# Taxonomies: Categorize Your Content

**YouTube:** not published yet · **Length:** 4:06 · **Feature:** OrchardCore.Taxonomies ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Taxonomies/))

An overview of the Taxonomies module for site owners and editors. It explains why a managed list of categories and
tags helps, shows the two features of the module, a taxonomy with its hierarchy of terms, the Taxonomy Field in a
Blog Post editor, and the category and tag badges and the term page on the site. The title, description, tags and
chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The exact steps, selectors
and the data to create are in recording-plan.md; this is the summary.

1. Build the web project of Orchard Core and run it on a local port (5010) with an empty data folder outside both
   repositories. Provision it with Auto Setup: the Blog recipe, site name Orchard Core, time zone
   America/Los_Angeles, user admin with the email admin@example.com. The password is chosen outside git and given to
   the recorder through the environment.
2. Sign the recorder in. Enable nothing: the Blog recipe enables Taxonomies, and Taxonomies Contents List Filters
   stays disabled.
3. Create the seed data outside the capture: the terms Food, Recipes and Restaurants in the Categories taxonomy
   (each term needs an icon, and the taxonomy is published afterwards), and a second Blog Post, "A weekend in
   Lisbon", with the category Travel and the tag Earth. Copy the site's data folder aside as the snapshot.
4. Narrate the video, then record the four recordings: the two features of the module, a taxonomy and its
   terms, categorizing a Blog Post, and the site. None of them changes the site, so they can be recorded in any order from
   the same snapshot. If a tag is created by mistake while recording the Blog Post editor, restore the snapshot
   before the next take.
