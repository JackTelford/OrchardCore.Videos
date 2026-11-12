# Azure AI Search: Connect Your Site and Create Indexes

**YouTube:** not published yet · **Length:** 4:56 · **Feature:** OrchardCore.AzureAI ([Azure AI Search](https://docs.orchardcore.net/en/latest/reference/modules/AzureAISearch/))

The video shows how an Orchard Core site keeps its search indexes in an Azure AI Search service. It enables the module,
connects the site to the service with an endpoint and an API key, creates an index of the Blog Post content type,
explains what goes into an index, edits the index, and ends with the actions of the index list. The title, description,
tags and chapters YouTube shows are in the metadata file of this folder.

The recording does not use a real Azure service, because Azure AI Search has no local emulator. A small stand-in
program answers the few requests the module makes, and the address on screen is a local address.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact actions of every
step, the labels with the lines of the source they come from, and the code of the stand-in are in
[recording-plan.md](recording-plan.md); questions a person has to settle first are in
[OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Start the stand-in for the search service on a free port. The Azure SDK accepts only an https address, so the stand-in
   serves HTTPS with the ASP.NET Core development certificate, and the Endpoint on screen is a local https address. The plan
   gives the commands, a fallback, and a dry run that tells whether the route works.
2. Build the Orchard Core web project (src/OrchardCore.Cms.Web) and run it on port 5010 with an empty data folder outside
   both repositories, provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles, and the user admin with the email admin@example.com and a password chosen outside git. Enable no
   other feature: the video enables Azure AI Search on camera, and Secrets and Search stay disabled.
3. Sign the recorder in once, and save a copy of the data folder after each of the snapshots the plan lists.
4. Narrate the video, then record the five clips in this order: 02-enable, 03-connect, 04-create, 06-edit, 07-actions.
   Each clip starts from the state the previous one left. Restart the stand-in whenever the site is restored to a state
   from before the index was created.
5. Check every contact sheet, and the private data checks at the end of the plan, before assembling.
