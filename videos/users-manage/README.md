# Users: Create, Edit, Disable and Delete Accounts

**YouTube:** not published yet · **Length:** 5:05 · **Feature:** [OrchardCore.Users](https://docs.orchardcore.net/en/latest/reference/modules/Users/)

This video shows how an administrator manages user accounts on the Users page, under Access Control. It explains
accounts, roles and permissions, then creates a user, edits it, changes its password, disables, enables and deletes
accounts, and finds users with search, filters and bulk actions. The title, description, tags and chapters YouTube
shows are in video.json.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The recording plan holds every
selector, value and wait; this section is the overview.

1. **The site.** An Orchard Core site set up by Auto Setup with the Blog recipe, the site name Orchard Core, the time
   zone America/Los_Angeles, and the user admin (admin@example.com), with its own data folder outside both
   repositories, on port 5010. The password is chosen outside this repository and is never written in it.
2. **The features.** The Blog recipe enables the Users and Roles features, and nothing else is needed. Leave every other
   Users feature (registration, reset password, change email, two-factor, audit trail, time zone, localization,
   custom user settings) and Workflows off, since each one adds content the narration does not mention.
3. **The settings.** Keep Settings > Security > User Login and Settings > Admin at their defaults.
4. **The seed data.** Before the first take, create six invented users with the roles Editor, Author, Contributor,
   Moderator, Contributor and Author, all with example.com addresses, and disable the last one. The plan gives the
   names and the way to create them. The user created on camera, morgan.ellis, is not part of the seed.
5. **The clips and their order.** Record the list, the creation, the edit, the Actions menu and the search clips in that
   order; each one after the list depends on the ones before it. Save a copy of the site's data after the seed, and
   after each of the creation, edit and Actions clips, and restore the copy that precedes a clip before recording it
   again. The slides need no site.

Then narrate, record, assemble and check the result for private data, as the video producer skill describes.
