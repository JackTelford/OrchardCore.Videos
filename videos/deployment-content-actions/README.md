# Export Content From the List: Download, Add to Plan, Send

**YouTube:** not published yet · **Length:** 4:34 · **Feature:** [OrchardCore.Contents.Deployment.Download](https://docs.orchardcore.net/en/latest/reference/modules/Contents/) (with OrchardCore.Contents.Deployment.AddToDeploymentPlan and OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget)

Three features of the Contents module add actions to the content items list: view or download an item as JSON, add it to
a deployment plan, and export the selected items to a deployment target. The video enables the features, then uses each
action on one item and on several items, and shows which plan the export action uses and where to choose it. The title,
description, tags and chapters YouTube shows are in video.json.

The video is for site owners and administrators: it shows the admin screens only. It does not cover creating deployment
plans or setting up remote instances; separate videos of the Deployment series cover both.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. Every selector, label, wait and
command is in recording-plan.md; this is the outline.

1. **The site.** Build the Orchard Core web project, and run it on a free port with its data folder outside both
   repositories, provisioned by Auto Setup with the Blog recipe: site name Orchard Core, time zone
   America/Los_Angeles, user admin, email admin@example.com. The password is chosen outside git and passed to the
   recorder; it is never written in this folder.
2. **The seed, created outside the capture.** One empty deployment plan named Orchard Harvest Content, and three
   published articles created in this order: Cider Press Rota, Pruning Guide, Apple Harvest Schedule. None of them has a
   draft. Leave the three features of the video disabled: the first clip enables them on camera. Sign the recorder in,
   stop the site, and copy its data folder and sign-in aside as the first snapshot.
3. **The clips, in this order:** 02-features, 03-download, 04-add, 04-many, 05-settings, 06-export, 06-many. Four of
   them change the site (02-features, 04-add, 04-many and 05-settings), so restore the snapshot taken before a clip
   whenever it is recorded again. The recording plan names the snapshots.
4. **Downloads.** Four steps start a file download. The browser draws no download bar in the recording, so a green
   caption on the list shows that the file was saved. The files the downloads leave in the browser's folder are not part
   of the video and are never committed.
5. **Narrate, then record.** Generate the narration before recording, because each step is held for the length of its
   narration. Questions that a person must confirm on a running site are listed in OPEN-QUESTIONS.md.
