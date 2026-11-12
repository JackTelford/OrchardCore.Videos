# Menu Items: Links, Content Items, HTML and Permissions

**YouTube:** not published yet · **Length:** 4:34 · **Feature:** [OrchardCore.Menu](https://docs.orchardcore.net/en/latest/reference/modules/Menu/)

The video shows the three types of menu item of the Menu module, a Link Menu Item, a Content Menu Item and an Html Menu
Item, and how each one is added from the menu editor. It then limits an item to the people who have a permission,
publishes the menu, and checks the result as the administrator and as a signed-out visitor. The title, description, tags
and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The full, step-by-step
instructions, with every label and its place in the source, are in recording-plan.md. In short:

1. Run the Cms.Web project of Orchard Core with its own App_Data folder outside both repositories, on
   http://localhost:5010, provisioned by Auto Setup with the **Blog** recipe (site name **Orchard Core**, time zone
   America/Los_Angeles, user admin, email admin@example.com). The password is chosen outside git and passed to the
   recorder in VIDEO_PASSWORD. The Blog recipe creates the Main Menu with the items Home and About, enables the Menu
   module and sets TheBlogTheme, so no feature has to be enabled.
2. Seed one fake Article, **Opening Hours**, published at the path opening-hours (recording-plan.md has the exact
   steps). Sign the recorder in, stop the site, and copy its App_Data folder aside as the first snapshot.
3. Narrate the video, then record the clips **in this order**, because each one starts from the site the previous one
   left: the dialog of the available menu items, the Link Menu Item (adds Editor Guide), the Content Menu Item (adds
   Hours), the Html Menu Item (adds Contact us), the permission on Editor Guide, the publication of the Main Menu, and
   finally the visit of a signed-out visitor. Every clip but the first and the last changes the site: take a snapshot
   after each one, and restore the one a clip starts from before recording it again.
4. Check every contact sheet, then scan the video for private data, as the skill describes.
