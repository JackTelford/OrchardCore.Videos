# Content Picker and User Picker Fields: Relate Content Items

**YouTube:** not published yet · **Length:** 5:37 · **Feature:** OrchardCore.ContentFields ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/ContentFields/))

This video shows how to relate a content item to other content items and to users. It adds a Content Picker Field and
a User Picker Field to a content type, sets which content types and which roles each one offers, picks items in the
editor, and shows the result on the page. Before a short recap, it shows the Lucene editor of the Content Picker Field and
names the Elasticsearch editor. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

Everything needed is in [recording-plan.md](recording-plan.md): the site, the seed data, the clips in the order to
record them, and what must be on screen at the end of every step. [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what
could not be checked without a running site.

In short:

- The video is recorded on a fresh site made by Auto Setup with the Blog recipe, with fake data only, so it needs no
  mask script. The signed-in user is admin; the password comes from outside git.
- Before recording, import the two content types and six content items, and create three users (morgan, jordan and
  taylor, with example.com addresses). The plan gives the exact data.
- The Lucene features and the Catalog index are added only after the user clips, so that the earlier clips show no
  Lucene editor.
- Clips 02-add-picker, 03-pick, 04-user-field, 04-user-pick, 05-lucene-field and 05-lucene-pick change the site and
  depend on each other. Record them in that order, and restore the saved copy of the site before another take. The slides
  and the two cards need no site.
- No Elasticsearch server is needed: the Elasticsearch editor appears on a slide only.
