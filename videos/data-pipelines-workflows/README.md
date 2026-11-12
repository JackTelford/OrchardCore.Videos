# Data Pipelines and Workflows: Run a Pipeline From a Workflow

**YouTube:** not published yet · **Length:** 4:36 · **Feature:** [OrchardCore.DataPipelines](https://docs.orchardcore.net/en/latest/reference/modules/DataPipelines/)

The Run Data Pipeline Task and the Data Pipeline Run Completed Event connect Data Pipelines to Workflows. The video
builds a workflow that runs a published pipeline when a blog post is published, reads the run in Run History, and
builds a second workflow that reacts when a run fails. The title, description, tags and chapters YouTube shows are in
video.json.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. recording-plan.md holds
everything a person needs, without deciding anything:

1. **Site.** A local Orchard Core site provisioned by Auto Setup with the Blog recipe, the site name Orchard Core, the
   time zone America/Los_Angeles, and the user admin with the address admin@example.com. The password is chosen outside
   git and passed to the recorder in VIDEO_PASSWORD.
2. **Features.** Workflows, Data Pipelines and Email are enabled; no email provider is, and no email settings are
   saved.
3. **Seed.** Two pipelines are imported with JSON Import and published: Blog posts export, which saves a file in the
   media library, and Blog posts by email, which fails because the site has no email settings. The plan gives the
   recipe to paste.
4. **Order.** Record 02-build, then 03-run, then 04-react, each from the site as the previous clip left it. Copy the
   site's data aside after the seed, after 02-build and after 03-run, and restore the right copy before each take.
5. **Narrate first**, so the recorder holds each step for its narration.
