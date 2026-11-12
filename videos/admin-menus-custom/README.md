# Admin Menus: Add Your Own Links to the Admin Navigation

**YouTube:** not published yet · **Length:** 5:16 · **Feature:** `OrchardCore.AdminMenu` ([Admin Menu](https://docs.orchardcore.net/en/latest/reference/modules/AdminMenu/))

The Admin Menu module lets site owners and editors build admin menus of their own. The video explains the two ideas, an
admin menu and an admin node, and the four kinds of node. It then creates a menu, adds a link, groups links with a
placeholder, adds nodes that list content types and blogs, and shows how to control who sees a menu. A closing slide
covers deployment plans and recipes. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The audience is administrators:
nothing on screen is code, and the narration names every label as the admin spells it.
`recording-plan.md` has every action, label and source reference; `OPEN-QUESTIONS.md` lists what to check on the running site first.

1. Start a local Orchard Core site with Auto Setup: the Blog recipe, site name **Orchard Core**, time zone
   `America/Los_Angeles`, user `admin` with the email `admin@example.com`. The Blog recipe turns the Admin Menu feature
   on and creates the menu **Admin menus** that the first clip shows, so no feature is enabled by hand. Keep the site's
   data folder outside both repositories.
2. Sign the recorder in as `admin` with a password chosen outside git, and seed the site: publish one more blog named
   **Travel Notes**. Stop the site and copy its data folder aside as the starting snapshot.
3. Narrate, then record the clips in this order, restoring the snapshot each one starts from (the plan names them):
   `02-list` and `03-create` both start from the starting snapshot; then `04-link`, `05-group`, `06-dynamic` and
   `07-access`, each from the state the previous clip left. The three slide steps (`ideas-1`, `ideas-2`, `reuse-1`) need no site.
4. Check every contact sheet and scan the result for private data before assembling.
