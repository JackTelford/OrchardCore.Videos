# OpenID Connect Applications and Scopes

**YouTube:** not published yet · **Length:** 5:40 · **Feature:** [OrchardCore.OpenId.Management](https://docs.orchardcore.net/en/latest/reference/modules/OpenId/)

An administrator's tour of the Applications and Scopes pages that the OpenID Connect Management UI feature adds under
Access Control, OpenID Connect. The video registers a confidential application for a service and a public application
for a web app, searches, edits and deletes applications, and defines a scope of its own with API audiences. The title,
description, tags and chapters YouTube shows are in the YouTube metadata of this folder.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The step-by-step
instructions, with every click and every value, are in the recording plan of this folder; this is the short version.

1. Run a local Orchard Core site from a checkout of OrchardCore, with an empty data folder outside both repositories,
   on port 5010, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles,
   user admin with the email admin@example.com. The password is chosen outside git.
2. Sign the recorder in as admin.
3. Seed the site outside the capture, as the recording plan describes:
   - Enable the **OpenID Connect Authorization Server** feature (Tools, Features). It enables the **OpenID Connect
     Management UI** and creates the four default scopes.
   - Save the Authorization Server settings (Settings, OpenID Connect) with **Allow Client Credentials Flow**
     checked, so that the application form offers that flow.
   - Create one application, **Legacy Prototype**, so that the first list is not empty.

   Stop the site and copy its data folder and the saved sign-in aside: this is the starting snapshot.
4. Narrate the video, then record the clips in this order: 02-service, 03-web, 04-manage, 05-scopes. Each clip
   changes the site and starts from the state the previous one left, so copy the data aside after each clip. To
   record a clip again, restore the copy that was made before it, and record that clip and the ones after it.
