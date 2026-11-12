# Build a Product Catalog: Types, Categories and Search

**YouTube:** not published yet · **Length:** 7:07 · **Feature:** `OrchardCore.ContentTypes` ([Content Types documentation](https://docs.orchardcore.net/en/latest/reference/modules/ContentTypes/))

This video builds a product catalog in the admin of an Orchard Core site. It creates a Product content type with a price,
an image and a category, groups the products with a taxonomy, uploads the pictures to the Media Library, publishes a
product, and adds search with a Lucene index, the Site Search settings and a permission for visitors. The title,
description, tags and chapters YouTube shows are in `video.json`; the exact clicks of every recording are in
`recording-plan.md`.

The other features it shows are documented on the pages of
[Taxonomies](https://docs.orchardcore.net/en/latest/reference/modules/Taxonomies/),
[Media](https://docs.orchardcore.net/en/latest/reference/modules/Media/),
[Media Slugify](https://docs.orchardcore.net/en/latest/reference/modules/Media.Slugify/) and
[Search](https://docs.orchardcore.net/en/latest/reference/modules/Search/).

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The full plan, with every
click, the selectors, the settings of the site and the source of every label, is in [recording-plan.md](recording-plan.md).

1. Run the Orchard Core web application from a checkout of OrchardCore, with its own empty data folder outside both
   repositories, on port 5010. Provision it with Auto Setup: the **Blog** recipe, the site name **Orchard Core**, the time
   zone Los Angeles, and the user **admin** (`admin@example.com`) with a password chosen outside git. The Blog recipe
   enables Content Types, Taxonomies, Media and Indexing. Block outbound calls, so the site never reaches a mail, SMS or
   payment provider.
2. Make three plain pictures outside the repository: Trail Backpack, Two-Person Tent and Down Sleeping Bag, each a colored
   background with the product name drawn on it. Never commit them.
3. Sign the recorder in. The video has no preparation step: every setting and every record is created on camera, except
   the second and third product, which the product clip creates off camera.
4. Narrate the video, then record the clips in the order they play, because each one starts from the state the previous
   one left: the Product type, the Product Category type, the taxonomy, the Category field, Media Slugify, the Media
   Library, the first product, the Lucene and Search features, the index, the Site Search settings, the Anonymous
   permission, and the search itself. Every clip changes the site except the last one. Copy the data folder aside after
   the taxonomy, the Media Library, the first product and the permission, and restore the copy before another take of the
   clip that follows it.
5. The slides are drawn from the video's slides file and need no recording. Before the search clips, wait until the index has
   finished building, and check once in a private window that a visitor who is not signed in can search.
