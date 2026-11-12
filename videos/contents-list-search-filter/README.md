# Content Items List: Search, Filter and Act on Many Items

**YouTube:** not published yet · **Length:** 5:06 · **Feature:** [OrchardCore.Contents](https://docs.orchardcore.net/en/latest/reference/modules/Contents/)

The content items list of the admin shows every content item of the types you may list. The video explains what a row
tells you, then searches the titles with AND, NOT and quotes, filters by content type and status, sorts the results,
and selects several items to publish, unpublish or delete them in one step. It is an admin video: it shows the admin
and no code. The title, description, tags and chapters YouTube shows are kept beside this file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. Everything a person needs, down
to the order of the clicks, is in recording-plan.md; this is the summary.

1. **The site.** Run Orchard Core from a checkout of OrchardCMS/OrchardCore with its own empty app data folder outside
   both repositories, provisioned by Auto Setup: the Blog recipe, site name Orchard Core, time zone
   America/Los_Angeles, user admin with the email admin@example.com. The password is chosen outside git and passed to the
   recorder in VIDEO_PASSWORD; it is never written in the repository. Block the outbound calls of the site.
2. **The features.** Only those the Blog recipe enables. The three Contents deployment features (Add Content To
   Deployment Plan, Export Content To Deployment Target, View Or Download Content As JSON) stay off, so the Actions
   menus hold only the commands the narration describes.
3. **The fake data.** Ten content items of an invented hiking club, created before the recording in a fixed order: two
   published pages, four published articles, four articles that are drafts, and one more edit that gives a published
   article a newer draft. With the five items of the Blog recipe the list holds fifteen items and two pages.
   recording-plan.md has the exact titles. Copy the app data folder and the sign-in aside as a snapshot.
4. **The order of the clips.** Narrate first. Then record the list, search and filter clips, which only read the site,
   in any order. Record the bulk actions clip last: it publishes two items and deletes two, so restore the snapshot
   before each take of it.
5. **The cards and the slides** (opening, closing, and chapter 01) need no recording.
