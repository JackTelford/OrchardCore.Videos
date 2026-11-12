# Taxonomies: Term Pages That List Their Content

**YouTube:** not published yet · **Length:** 4:01 · **Feature:** OrchardCore.Taxonomies ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Taxonomies/))

A taxonomy sorts content into terms, and a term page gives each term an address of its own and lists the content items
that have it. The video browses from a blog post to the page of its category and of its tag, shows what the taxonomy and
its terms need for the pages to exist (Route contained items, and the Permalink of the taxonomy and of a term), changes the
Page size to show how a term page is paged, and ends with the four things to check. The title, description, tags and
chapters YouTube shows are in video.json.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. The step-by-step plan, with
the exact seed data, the order of the clips and the actions of every step, is in [recording-plan.md](recording-plan.md);
this is the summary.

1. Run a local Orchard Core site on a data folder outside both repositories, provisioned by Auto Setup with the **Blog**
   recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, and the user admin (email
   admin@example.com, with a password chosen outside git). The Blog recipe enables every feature the video shows, so
   there is nothing to enable.
2. Create the five fake blog posts of the plan's seed table (all published, category Travel, tags from Earth, Exploration
   and Space), check the pages the plan lists, stop the site, and copy its data folder aside as the seeded snapshot.
3. Sign the recorder in, narrate the video, and record the clips in this order: the browse clip, the routing clip, then
   the paging clip. The first two only read the site. The paging clip changes the Page size from 10 to 3: restore the
   seeded snapshot, and the saved sign-in, before another take of it, and before any take of the other two clips that
   comes after it.
