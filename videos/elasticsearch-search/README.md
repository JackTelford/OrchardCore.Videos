# Elasticsearch: Search Indexes on an Elasticsearch Server

**YouTube:** not published yet · **Length:** 4:54 · **Feature:** OrchardCore.Elasticsearch ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Elasticsearch/))

An admin video about the Elasticsearch module. It explains how the site, an Elasticsearch server and an index fit
together, enables the Elasticsearch and Search features, creates an index of the Article content type with its
Elasticsearch settings, and chooses that index for the search page of the site. It ends with the other tools of the
module: Query Index, Index Info, Run Elasticsearch Query, and the index settings of parts and fields. The title,
description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything a person needs is
in recording-plan.md; OPEN-QUESTIONS.md lists what must be confirmed on a running site first.

1. **The site.** A build of src/OrchardCore.Cms.Web of Orchard Core, on http://localhost:5010, with its own empty
   application data folder outside both repositories, provisioned by Auto Setup: tenant Default, the Blog recipe, site
   name Orchard Core, time zone America/Los_Angeles, user admin with the email admin@example.com. The password is chosen
   outside git and passed to the recorder; it is never written in this repository.
2. **The Elasticsearch server.** A real single-node server in a throwaway Docker container, with security turned off,
   on port 9200. The site reaches it through its configuration, set before the first start. The recording plan gives
   the container, the connection and the proxy settings. Remove the container when the video is recorded.
3. **The seed.** Four fake published articles about a made-up orchard, imported once with Tools, Deployments, JSON
   Import before the first recording. The search word of the video is harvest.
4. **The features.** The Elasticsearch and Search features start disabled: the first recording enables them on camera.
   Indexing, which the Blog recipe enables, must be on before the articles are imported.
5. **Narrate, then record.** Follow SKILL.md of the video producer skill: sign the recorder in, narrate, record.
   Take a snapshot of the site data and of the sign-in after the seed (before the first recording) and after each
   recording.
6. **The order.** Three recordings, each starting from the state the previous one left: 02-enable, 03-create,
   04-search. The chapters 01-fit and 05-tools are slides. To record one of them again, restore the
   snapshot it starts from. For 03-create, also delete the index on the Elasticsearch server, so that the server
   matches the snapshot. For 04-search, keep the index: its snapshot holds it.
