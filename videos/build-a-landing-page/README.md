# Build a Landing Page With Flows, Widgets and Layers

**YouTube:** not published yet · **Length:** 5:48 · **Feature:** `OrchardCore.Flows` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Flow/))

A walkthrough for site owners and editors. It builds a landing page from blocks with a Flow (the Flows module), creates a
layer with a Url condition (the Layers module), adds a widget to the Footer zone for that layer (the Widgets module), and
checks that the widget shows on the landing page and not on the homepage. The title, description, tags and chapters
YouTube shows are in `video.json`; every step to record is in `recording-plan.md`.

The documentation pages it backs: [Flows](https://docs.orchardcore.net/en/latest/reference/modules/Flow/),
[Bag part](https://docs.orchardcore.net/en/latest/reference/modules/Flow/BagPart/),
[Widgets](https://docs.orchardcore.net/en/latest/reference/modules/Widgets/) and
[Layers](https://docs.orchardcore.net/en/latest/reference/modules/Layers/).

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script and no seed script. The commands,
the settings of the site and the exact actions of every step are in `recording-plan.md`.

1. Start a throwaway Orchard Core site with its own empty data folder, provisioned by Auto Setup: the **Blog** recipe, the
   site name **Orchard Core**, the time zone `America/Los_Angeles`, and the user `admin` with the email
   `admin@example.com`. The password is one you choose; it is passed to the recorder when it signs in and is never written
   in this repository. Block the site's outbound calls.
2. Nothing to enable and nothing to seed. The Blog recipe turns on Flows, Widgets, Layers and Rules, and creates the Page
   content type with a Flow, the widget types, the zones Content and Footer, and the layers Always and Homepage.
3. Sign the recorder in, then copy the site's data folder and the saved sign-in aside as the first snapshot.
4. Narrate, then record the clips in this order: `02-page`, `03-layer`, `04-zones`, `05-widget`, `06-result`. The clips
   change the site, so restore the snapshot named in `recording-plan.md` before each take. `05-widget` needs the layer
   from `03-layer`, and `06-result` needs the page from `02-page` and the widget from `05-widget`. `04-zones` only reads a
   settings page.
5. The slides (`01-parts`, `07-choose`) are drawn by `slides.py` and need no site.
