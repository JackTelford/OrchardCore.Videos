# Sitemap Indexes and the Sitemaps Cache

**YouTube:** not published yet · **Length:** 4:00 · **Feature:** `OrchardCore.Sitemaps` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Sitemaps/))

A sitemap index groups several sitemaps under one address. The video creates an index from two sitemaps, opens it in the
browser, and shows how the robots file names it, once the Robots settings are saved with the sitemaps included. It then
explains how Orchard Core stores each sitemap as a file, walks through the Sitemaps Cache page with its Purge and Purge
All buttons, and enables the Sitemaps Cleanup feature, which removes the files that no sitemap uses. The audience is
administrators, so the video shows the admin screens and the pages a search engine reads. The title, description, tags
and chapters YouTube shows are in video.json.

## Recording it again

The recording plan in this folder says everything a person needs: the site, the seed data, the order of the clips and
what each step shows. In short:

1. Run the Orchard Core web application on a fresh site with its own data folder outside both repositories, on port 5010,
   provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user **admin**
   with the email admin@example.com.
2. Enable the **Sitemaps**, **SEO** and **Background Tasks** features, and leave **Sitemaps Cleanup** disabled: the video
   enables it on camera. Set **Base url** under **Settings**, **General** to https://www.example.com, and save the Robots
   settings once under **Settings**, **Search**, **Search Engine Optimization**, **Robots**.
3. Seed the site outside the capture: the sitemaps **Posts** (content type Blog Post) and **Articles** (content type
   Article), then the cache files described in the plan, including one file that no sitemap uses.
4. Sign the recorder in with the user **admin** and the password chosen outside git, narrate, and record the clips in the
   order 02-create, 03-view, 05-cache, 06-cleanup. The clips change the site, so restore the snapshots listed in the plan
   before another take. The slide chapters (01-why and 04-how) need no site.

OPEN-QUESTIONS.md lists what the author could not check without a running site.
