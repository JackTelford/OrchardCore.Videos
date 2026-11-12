# Content Culture Picker: Let Visitors Switch Languages

**YouTube:** not published yet · **Length:** 4:00 · **Feature:** [OrchardCore.ContentLocalization.ContentCulturePicker](https://docs.orchardcore.net/en/latest/reference/modules/ContentLocalization/)

The video shows the language menu that the Content Culture Picker adds to the navigation bar of a site, and what it
needs: two or more supported cultures, translated pages with a path each, and a theme that shows the navigation bar.
It enables the feature, switches the language of a page, shows what happens when a page has no translation, and covers
the Homepage redirect and Set Cookie options and the permission that protects them. The title, description, tags and
chapters YouTube shows are kept with the video's YouTube details.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Every step, selector and
label is in the recording plan of this folder; the outline is:

1. Run the CMS web project of Orchard Core on a local address, with an empty data folder outside both repositories,
   provisioned by Auto Setup with the Blog recipe (site name Orchard Core, time zone America/Los_Angeles, user admin with
   the email admin@example.com). The password is chosen outside git and passed to the recorder.
2. Sign the recorder in once.
3. Seed the site with the two JSON Import recipes of the recording plan. They enable Content Localization and The
   Default Theme (the Blog theme does not show the language menu), set English as the default culture and French as the
   second, add the Localization part to the Article content type, and create five fake articles: Welcome and
   Bienvenue, Contact us and Nous contacter, and Opening hours, which has no French version. The Content Culture Picker
   feature stays disabled until the first clip enables it.
4. Restart the site, check the pages listed in the plan, and keep a copy of the data folder and the sign-in (snapshot A).
5. Narrate, then record the clips in this order: 02-enable, 03-switch, 04-missing, 05-homepage, 06-cookie. The first
   clip enables the feature (copy the site again afterwards: snapshot B, the start of clips 03, 04 and 05), the fifth
   saves the Homepage redirect option (snapshot C, the start of clip 06). To record a clip again, restore the snapshot it
   starts from first.
