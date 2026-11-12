# Content Version Pruning: Remove Old Versions Automatically

**YouTube:** not published yet · **Length:** 3:34 · **Feature:** [OrchardCore.Contents.VersionPruning](https://docs.orchardcore.net/en/latest/reference/modules/Contents/#content-version-pruning)

Publishing a change to a versioned content item leaves the previous version in the database. The Content Version Pruning feature deletes the old ones on a schedule. The video explains which versions pruning keeps and which it deletes, enables the Content Version Pruning and Background Tasks features, chooses the retention period, the number of archived versions to keep and the content types to prune, and enables the background task and runs it on demand. The title, description, tags and chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script and no seed data: the content of the Blog recipe is enough, since the video never opens a content item.

1. Run a copy of Orchard Core on a local address with its own data folder outside both repositories, provisioned by Auto Setup with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the user admin (admin@example.com). The password is chosen outside git and passed to the recorder. The Blog recipe enables neither the Content Version Pruning feature nor the Background Tasks feature, and the video enables both, so the site must start with both disabled.
2. Sign the recorder in once and save the session, then stop the site and copy its data folder aside. Restore that copy before a full take.
3. Narrate the video, then record the clips in this order, because each one starts from the site the previous one left: 02-features, 03-settings, 04-task. The slides of chapter 1 need no recording. To record a single clip again, restore the copy of the site as the previous clip left it.

The recording plan has every step: the site, the order of the clips and the snapshots to restore between them, the actions of each step, what must be on screen when the narration of the step ends, and the checks for private data. The open questions list what a person must confirm on a running site.
