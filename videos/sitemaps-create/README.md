# Sitemaps: Help Search Engines Find Your Pages

**YouTube:** not published yet · **Length:** 5:42 · **Feature:** `OrchardCore.Sitemaps` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Sitemaps/))

This video shows what a sitemap lists and how the Sitemaps module builds one. It enables the feature and sets the Base url,
creates a sitemap with a Sitemap Content Types source, views the result, chooses content types with their own Change
Frequency and Priority, adds a Sitemap Custom Path source, and controls a single page with the Sitemap part. The last slides
say how sitemaps reach the robots file. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` has every
step, selector and expected screen, and the commands that build the recorder and record the clips; `OPEN-QUESTIONS.md` has
the points to check on the running site first.

1. Run Orchard Core on `http://localhost:5010` with its own data folder outside both repositories, provisioned by Auto
   Setup: the Blog recipe, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email
   `admin@example.com`, and a password chosen outside git. Leave the Sitemaps feature off and the Base url empty: the first
   clip enables the feature and sets the Base url.
2. Create the seed data once, by hand, outside the capture: the three published articles **Spring Planting Calendar**,
   **Five Herbs for a Windowsill** and **Composting Basics** (the texts are in `recording-plan.md`). The Blog recipe adds the
   other content the sitemap lists. Stop the site and copy its data folder aside as snapshot S0.
3. Sign the recorder in, narrate, and record, with the commands of `recording-plan.md`. The clips change the site and depend
   on each other, so record them in this order, restoring the snapshot named in `recording-plan.md` before another take of
   any of them: `02-setup`, `03-create`, `04-source`, `05-view`, `06-types`, `07-path`, `08-part`. The slides (`01-what`,
   `09-reach`) and the cards need no site.
