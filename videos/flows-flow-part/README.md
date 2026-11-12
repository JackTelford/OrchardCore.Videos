# Flow Part: Build Pages From Widgets

**YouTube:** not published yet · **Length:** 5:14 · **Feature:** OrchardCore.Flows ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Flow/))

The video shows the Flow part of the Flows module, which lets editors build a page from widgets. It explains what the part
is and what a site needs to use it, shows where the part is added to a content type and what its settings do, builds a
page by adding widgets and setting their size and alignment, moves, inserts and removes them, and shows the page on the
site. It ends with the Blocks editor, which picks widgets from a dialog. The title, description, tags and chapters
YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only (the page "Hillside Bakery" is created on camera), so it
needs no mask script. The recording plan lists every action of every step, and the open questions list what the first
take must confirm.

1. Run Orchard Core (a checkout whose Flows module has the Blocks editor) on http://localhost:5010, with an empty data
   folder outside both repositories, provisioned by Auto Setup with the **Blog** recipe, site name **Orchard Core**,
   time zone America/Los_Angeles, user admin, email admin@example.com. The Blog recipe enables the Flows and Widgets
   features and creates the Page type, which has a Flow part, and the widget types Blockquote, Container, Image,
   Paragraph and Raw Html. Nothing else is enabled or seeded. The recording plan has the settings and the commands.
2. Sign the recorder in as admin, with the password chosen outside git. The video has no seeding step. Stop the site and
   copy its data folder aside (snapshot A): restore that copy before every take of the Build a page chapter, and only
   before that one.
3. Narrate, then record. The clips run in this order: the Add the Flow part chapter (it changes nothing), the Build a
   page chapter (it creates and publishes the page), the See the page chapter (it reads that page), and the Blocks
   editor chapter last, because it changes the Flow part of the Page type to the Blocks editor. Copy the data folder
   aside after the See the page chapter (snapshot B) and restore it before recording the Add the Flow part, the See the
   page or the Blocks editor chapter again.
