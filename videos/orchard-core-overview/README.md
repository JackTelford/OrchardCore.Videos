# Orchard Core: What It Is and What You Can Build

**YouTube:** not published yet · **Length:** 5:32 · **Feature:** [topic:README](https://docs.orchardcore.net/en/latest/reference/) (the Modules Reference, with the [Glossary](https://docs.orchardcore.net/en/latest/reference/glossary/) and the [Orchard Core home page](https://docs.orchardcore.net/en/latest/))

An overview for people who have never seen Orchard Core. It explains that Orchard Core is a framework and a CMS built on
it, shows a blog and its admin, and walks through the building blocks: content items and content types, themes,
widgets and layers, features, recipes and tenants. It ends with what you can build and where the documentation is. The
title, description, tags and chapters YouTube shows are in the metadata file of this folder.

## Recording it again

The video is recorded on a **fresh site** with the sample content of the Blog recipe and nothing else, so it needs no
seed data and no mask script. The multi-tenancy chapter is slides, so no second tenant is needed.

1. Run a build of Orchard Core CMS on a free port, with an empty data folder outside both repositories, and let Auto
   Setup install it with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the user
   admin (admin@example.com). The password is chosen outside git and passed to the recorder; it is never written here.
2. Enable no feature. Workflows and Tenants must stay disabled.
3. Sign the recorder in once, then narrate and record the clips. The five recordings only look at the site and do not
   depend on each other; none of them changes it. Record them in the order of the storyboard: the tour, the content
   items, the content types, the design pages, the features page.
4. Check every contact sheet, and OCR the result for private data before publishing.

Every command, every selector and every step of every clip is in recording-plan.md. Questions that only a running site
can answer are in OPEN-QUESTIONS.md: read them first.
