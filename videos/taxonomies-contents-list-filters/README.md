# Taxonomies: Filter the Content Items List by Term

**YouTube:** not published yet · **Length:** 3:26 · **Feature:** [OrchardCore.Taxonomies.ContentsAdminList](https://docs.orchardcore.net/en/latest/reference/modules/Taxonomies/#taxonomies-contents-list-filters)

The video shows how the Taxonomies Contents List Filters feature turns the terms of a taxonomy into filters for the
Content Items list. It enables the feature in Tools, Features, chooses the taxonomies to filter by in Settings,
Taxonomy Filters, and then filters the list by a term, with Show all and Clear filter. It ends with how the filters
follow the selected content type. The YouTube title, description, tags and links are kept with the video, in its
metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact actions, the
seed values and the labels of every step are in recording-plan.md; this is the outline.

1. Run a fresh local Orchard Core site provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the
   time zone America/Los_Angeles, the user **admin**, on port 5010. Its data folder lives outside both repositories.
   The feature stays disabled, and no taxonomy filter is saved, until the clips enable and save them.
2. Make the seed data once, outside the capture: the Blog Post content type is made creatable and listable, the
   Categories taxonomy gets the terms Food, Technology, Gadgets and Software, each with an icon because the term type
   requires one, and is published again, and five blog posts are created from the Blog item next to the sample post of
   the recipe. The plan lists every title, category and tag.
3. Sign the recorder in (the password is chosen outside git and passed in the environment), narrate the video, and
   record the clips in this order: the feature, the settings, the filter, the content types. Copy the site's data
   folder aside before the first clip, after the first clip and after the second clip. The first two clips change the
   site: restore the matching copy before taking either of them again. The last two only read it.
