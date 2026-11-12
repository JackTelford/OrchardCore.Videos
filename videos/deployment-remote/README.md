# Remote Deployment: Send a Plan Straight to Another Site

**YouTube:** not published yet · **Length:** 5:37 · **Feature:** [OrchardCore.Deployment.Remote](https://docs.orchardcore.net/en/latest/reference/modules/Deployment.Remote/)

The video shows how the Remote Deployment module sends a deployment plan straight to another Orchard Core site. It creates a
remote client on the destination site, creates a remote instance on the source site, executes a plan on that instance, and
checks the result on the destination. Slides explain how the two sites work together, how the Url of the destination is built,
and which security and troubleshooting points matter. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh installation** with fake data only, so it needs no mask script. It shows two sites: the
source is the Default tenant, called **Orchard Core**, and the destination is a second tenant called **Staging Site**, reached
under the URL prefix staging. Both run in one local host.

1. Build and run the Orchard Core CMS web project with its own data folder outside both repositories, on port 5010. Auto Setup
   installs both tenants with the Blog recipe, the time zone America/Los_Angeles and the user admin (admin@example.com, with a
   password chosen outside git). The environment variables, the proxy settings and the first requests are in recording-plan.md.
2. Sign the recorder in to **both** tenants: the Default tenant with the recorder's setup command, and the staging tenant in
   the video's prepare step, saved to the same sign-in state file. The Blog recipe enables the Deployment and Remote Deployment
   features on both tenants.
3. Seed the source site outside the capture: two published Page content items (Spring Planting Workshop and Seed Swap Weekend)
   and a deployment plan called Publish Pages with one Content step for the type Page. The destination needs nothing. The exact
   steps are in recording-plan.md. Copy the data folder and the sign-in state aside as the first snapshot.
4. Narrate, then record the clips in this order, restoring the snapshots of recording-plan.md before a new take: 02-client
   (destination), 03-instance (source), 04-send (source), 05-check (destination). The clips depend on each other: 04-send needs
   the remote client and the remote instance, and 05-check needs the pages that 04-send imports. The key shared by the client
   and the instance is a throwaway value chosen outside git.

OPEN-QUESTIONS.md lists what was read in the source but not seen on a running site.
